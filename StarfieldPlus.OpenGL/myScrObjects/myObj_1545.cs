using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;


/*
    - Invisible static dots + one moving point. The moving point builds connections to invisible dots while it travels around
*/


namespace my
{
    public class myCellManager<Type>
    {
        public class myCellManager_Cell<Type>
        {
            public int x, y;
            public Dictionary<uint, Type> items = null;

            public myCellManager_Cell()
            {
                items = new Dictionary<uint, Type>();
            }
        }

        public Dictionary<int, myCellManager_Cell<Type>> _dic = null;
        private int _offset;

        public int _cellSize;
        public int _cellRowSize;
        public int _minId, _maxId;

        public myCellManager(int cellSize, int offset, int width, int height)
        {
            _cellSize = cellSize;
            _offset = offset;

            _dic = new Dictionary<int, myCellManager_Cell<Type>>();

            int minx = -offset;
            int maxx = width + offset;
            int miny = -offset;
            int maxy = height + offset;

            _cellRowSize = 1 + (maxx - minx) / cellSize;

            _minId = 99 * 99 * 99;
            _maxId = -1;

            for (int j = miny; j < maxy; j += cellSize)
            {
                for (int i = minx; i < maxx; i += cellSize)
                {
                    int id = i / cellSize + (j / cellSize) * _cellRowSize;

                    Debug.Assert(!_dic.ContainsKey(id));

                    if (id > _maxId)
                        _maxId = id;

                    if (id < _minId)
                        _minId = id;

                    _dic[id] = new myCellManager_Cell<Type>();

                    _dic[id].x = i / cellSize;
                    _dic[id].y = j / cellSize;
                }
            }
        }

        public int GetCellId(float x, float y)
        {
            return (int)x / _cellSize + ((int)y / _cellSize) * _cellRowSize;
        }

        public int Add(float x, float y, uint id, Type item)
        {
            int cellId = GetCellId(x, y);

            _dic[cellId].items.Add(id, item);

            return cellId;
        }

        public int Move(float x, float y, uint id, int oldCellId, Type item)
        {
            int newCellId = GetCellId(x, y);

            if (oldCellId != newCellId)
            {
                if (oldCellId >= _minId && oldCellId <= _maxId)
                {
                    _dic[oldCellId].items.Remove(id);
                }

                if (newCellId >= _minId && newCellId <= _maxId)
                {
                    _dic[newCellId].items.Add(id, item);
                }

                return newCellId;
            }

            return oldCellId;
        }
    }







#pragma warning disable 0162
    public class myObj_1545 : myObject
    {
        // Priority
        public static int Priority => 10;
        public static System.Type Type => typeof(myObj_1545);

        private int cnt, lifeCnt, cellId = -1000;
        private float x, y, dx, dy;
        private float A, R, G, B;

        private static int N = 0, n = 0, colorMode = 0;
        private static int cellSize = 100;
        private static bool doDestroy = false;
        private static float dimAlpha = 0.05f;

        private static myScreenGradient grad = null;
        private static myCellManager<myObj_1545> cellManager = null;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_1545()
        {
            if (id != uint.MaxValue)
                generateNew();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time global initialization
        protected override void initGlobal()
        {
            colorPicker = new myColorPicker(gl_Width, gl_Height);
            list = new List<myObject>();

            cellManager = new myCellManager<myObj_1545>(100, 0, gl_Width, gl_Height);

            // Global unmutable constants
            {
                n = 11;
                N = 5000;
                N = 300000;
                N = 10000;
            }

            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time local initialization
        private void initLocal()
        {
            doClearBuffer = false;
            doDestroy = myUtils.randomBool(rand);

            colorMode = rand.Next(2);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            height = 600;

            string str = $"Obj = {Type}\n\n"                       +
                            myUtils.strCountOf(list.Count, N)      +
                            $"doClearBuffer = {doClearBuffer}\n"   +
                            $"colorMode = {colorMode}\n"           +
                            $"doDestroy = {doDestroy}\n"           +
                            $"file: {colorPicker.GetFileName()}"
                ;
            return str;
        }

        // ---------------------------------------------------------------------------------------------------------------

        // 
        protected override void setNextMode()
        {
            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void generateNew()
        {
            if (id < n)
            {
                x = gl_x0;
                y = gl_y0;

                float spd = 1.5f;

                dx = (0.5f + myUtils.randFloat(rand) * spd) * myUtils.randomSign(rand);
                dy = (0.5f + myUtils.randFloat(rand) * spd) * myUtils.randomSign(rand);

                A = 1;
                R = G = B = 1;
                cnt = 50 + rand.Next(111); // used a a dist
            }
            else
            {
                x = rand.Next(gl_Width);
                y = rand.Next(gl_Height);

                dx = dy = 0;

                dx = (0.1f + myUtils.randFloat(rand)) * myUtils.randomSign(rand);
                dy = (0.1f + myUtils.randFloat(rand)) * myUtils.randomSign(rand);

                A = 0.15f + myUtils.randFloat(rand) * 0.1f;
                colorPicker.getColor(x, y, ref R, ref G, ref B);
                cnt = 333 + rand.Next(999);
                lifeCnt = 33;
            }

            cellId = cellManager.Move(x, y, id, cellId, this);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            x += dx;
            y += dy;

            if (id < n)
            {
                if (x < 0)
                    dx += 0.01f;

                if (y < 0)
                    dy += 0.01f;

                if (x > gl_Width)
                    dx -= 0.01f;

                if (y > gl_Height)
                    dy -= 0.01f;
            }
            else
            {
                if (--cnt == 0)
                {
                    generateNew();
                }

                if (doDestroy && lifeCnt == 0)
                {
                    generateNew();
                }
            }

            cellId = cellManager.Move(x, y, id, cellId, this);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void Show_2(myObj_1545 item)
        {
            if (item.id >= n)
            {
                int maxDist = 25;

                float dx, dy, dist2;

                int rayDist = maxDist / cellManager._cellSize;

                int min = -rayDist - 1;
                int max = +rayDist + 2;

                int cnt2 = maxDist * maxDist;

                for (int i = min; i < max; i++)
                {
                    int cell_i = item.cellId + cellManager._cellRowSize * i;

                    for (int j = min; j < max; j++)
                    {
                        int cell = cell_i + j;

                        if (cell >= cellManager._minId && cell <= cellManager._maxId)
                        {
                            foreach (var other in cellManager._dic[cell].items)
                            {
                                // Prevent drawing the same line more than once
                                //if (id < other.Value.id)
                                {
                                    dx = item.x - other.Value.x;
                                    dy = item.y - other.Value.y;

                                    dist2 = dx * dx + dy * dy;

                                    // Other particle is within reach of the first one
                                    if (dist2 < cnt2)
                                    {
                                        myPrimitive._LineInst.setInstanceCoords(other.Value.x, other.Value.y, item.x, item.y);
                                        myPrimitive._LineInst.setInstanceColor(1, 1, 1, 0.05f);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return;
        }

        protected override void Show()
        {
#if true

            if (id < n)
            {
                float dx, dy, dist2;

                int rayDist = cnt / cellManager._cellSize;

                int min = -rayDist - 1;
                int max = +rayDist + 2;

                int cnt2 = cnt * cnt;

                for (int i = min; i < max; i++)
                {
                    int cell_i = cellId + cellManager._cellRowSize * i;

                    for (int j = min; j < max; j++)
                    {
                        int cell = cell_i + j;

                        if (cell >= cellManager._minId && cell <= cellManager._maxId)
                        {
                            foreach (var other in cellManager._dic[cell].items)
                            {
                                // Prevent drawing the same line more than once
                                if (id < other.Value.id)
                                {
                                    dx = x - other.Value.x;
                                    dy = y - other.Value.y;

                                    dist2 = dx * dx + dy * dy;

                                    // Other particle is within reach of the first one
                                    if (dist2 < cnt2)
                                    {
                                        myPrimitive._LineInst.setInstanceCoords(x, y, other.Value.x, other.Value.y);

                                        myPrimitive._LineInst.setInstanceColor(1, 0, 0, 0.1f);

/*
                                        switch (colorMode)
                                        {
                                            case 0:
                                                myPrimitive._LineInst.setInstanceColor(1, 1, 1, 0.05f);
                                                break;

                                            case 1:
                                                myPrimitive._LineInst.setInstanceColor(other.Value.R, other.Value.G, other.Value.B, 0.05f);
                                                break;
                                        }
*/
                                        Show_2(other.Value);
                                    }
                                }
                            }
                        }
                    }
                }
            }

#else
            if (id < n)
            {
                int Count = list.Count;

                for (int i = n; i < Count; i++)
                {
                    var other = list[i] as myObj_1545;

                    float dx = x - other.x;
                    float dy = y - other.y;

                    float dist = (float)Math.Sqrt(dx * dx + dy * dy);

                    if (dist < cnt)
                    {
                        myPrimitive._LineInst.setInstanceCoords(x, y, other.x, other.y);

                        switch (colorMode)
                        {
                            case 0:
                                myPrimitive._LineInst.setInstanceColor(1, 1, 1, 0.05f);
                                break;

                            case 1:
                                myPrimitive._LineInst.setInstanceColor(other.R, other.G, other.B, 0.05f);
                                break;
                        }

                        other.lifeCnt--;

                        for (int j = n; j < Count; j++)
                        {
                            var oth = list[j] as myObj_1545;

                            dx = other.x - oth.x;
                            dy = other.y - oth.y;

                            dist = (float)Math.Sqrt(dx * dx + dy * dy);

                            if (dist < 50)
                            {
                                myPrimitive._LineInst.setInstanceCoords(other.x, other.y, oth.x, oth.y);
                                myPrimitive._LineInst.setInstanceColor(1, 1, 1, 0.05f);
                            }
                        }
                    }
                }
            }
#endif
            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Process(Window window)
        {
            uint cnt = 0;
            initShapes();

            clearScreenSetup(doClearBuffer, 0.1f);

            if (!false)
                while (list.Count < N)
                    list.Add(new myObj_1545());

            stopwatch = new StarfieldPlus.OpenGL.myUtils.myStopwatch(true);

            while (!Glfw.WindowShouldClose(window))
            {
                int Count = list.Count;

                processInput(window);

                // Swap fore/back framebuffers, and poll for operating system events.
                Glfw.SwapBuffers(window);
                Glfw.PollEvents();

                // Dim screen
                {
                    if (doClearBuffer)
                    {
                        glClear(GL_COLOR_BUFFER_BIT);
                        grad.Draw();
                    }
                    else
                    {
                        dimScreen(dimAlpha);
                    }
                }

                // Render Frame
                {
                    myPrimitive._LineInst.ResetBuffer();

                    for (int i = 0; i != Count; i++)
                    {
                        var obj = list[i] as myObj_1545;

                        obj.Show();
                        obj.Move();
                    }

                    myPrimitive._LineInst.Draw();
                }

                if (Count < N)
                {
                    list.Add(new myObj_1545());
                }

                stopwatch.WaitAndRestart();
                cnt++;
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void initShapes()
        {
            myPrimitive.init_ScrDimmer();

            grad = new myScreenGradient();
            grad.SetRandomColors(rand, 0.2f);

            myPrimitive.init_LineInst(n * 10000);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------
    }
};
