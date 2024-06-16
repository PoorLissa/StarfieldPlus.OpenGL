using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;


/*
    - test for sorted O(N^2) interactions with limited distance

    1. Remove sortedList, work off (and sort) the main list: this way, we don't need to use binarySearch: we'll already know the position
    2. Use cells. Each cell contains a dictionary, each dictionary contains all the particles belonging to this cell
        For the current particle, get its x % cellSize and y % cellSize => cellId
        For that cellId, find all neighbouring cells
        For every such neighbour, check its child particles vs the current one
*/


namespace my
{
    public class cell
    {
        public int x, y, w, h;

        public Dictionary<uint, myObj_9999_test_002b> items = null;

        public cell()
        {
            items = new Dictionary<uint, myObj_9999_test_002b>();
        }
    };

    public class myObj_9999_test_002b : myObject
    {
        // Priority
        public static int Priority => 10;
		public static System.Type Type => typeof(myObj_9999_test_002b);

        private int cellId;
        private float x, y, dx, dy;
        private float size, A, R, G, B, angle = 0;

        private static int N = 0, shape = 0, maxConnectionDist = 100, nTaskCount = 1, lenMode = 0;
        private static bool doFillShapes = false, doGenerateAll = false, doShowCellBounds = true;
        private static float dSpeed = 0.01f, opacityFactor = 0.025f;

        private static int   cellSize = 100, cellRow = 0;
        private static int   maxDistSquared = 0;
        private static float maxDistSquared_Inverted = 1.0f;

        private static myScreenGradient grad = null;

        private static object _lock = new object();

        // Override base list, to sort it without type casting
        private static new List<myObj_9999_test_002b> list = null;

        private static Dictionary<int, cell> dic = null;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_9999_test_002b()
        {
            if (id != uint.MaxValue)
                generateNew();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time global initialization
        protected override void initGlobal()
        {
            colorPicker = new myColorPicker(gl_Width, gl_Height);
            list = new List<myObj_9999_test_002b>();

            {
                dic = new Dictionary<int, cell>();
                cellSize = 250;

                int minx = 0;
                int maxx = gl_Width;

                cellRow = 1 + (maxx - minx) / cellSize;

                for (int j = 0; j < gl_Height; j += cellSize)
                {
                    for (int i = minx; i < maxx; i += cellSize)
                    {
                        int id = i / cellSize + (j / cellSize) * cellRow;

                        dic[id] = new cell();

                        dic[id].x = i / cellSize;
                        dic[id].y = j / cellSize;
                        dic[id].w = cellSize;
                        dic[id].h = cellSize;
                    }
                }
            }

            // Global unmutable constants
            {
                N = rand.Next(100) + 100;
                N = 3500;
                N = 2500 + rand.Next(1001);
                N = 2345;

                shape = rand.Next(5);

                nTaskCount = Environment.ProcessorCount - 1;
                nTaskCount = 1;

                doGenerateAll = false;
                doGenerateAll = true;
            }

            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time local initialization
        private void initLocal()
        {
            doClearBuffer = true;

            lenMode = rand.Next(3);

lenMode = 0;

            switch (lenMode)
            {
                case 0: maxConnectionDist = 250; break;
                case 1: maxConnectionDist = 100 + rand.Next(150); break;
                case 2: maxConnectionDist = 200; break;
            }

            maxDistSquared = maxConnectionDist * maxConnectionDist;
            maxDistSquared_Inverted = 1.0f / maxDistSquared;

            renderDelay = 0;

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            height = 600;

            string nStr(int   n) { return n.ToString("N0");    }
            //string fStr(float f) { return f.ToString("0.000"); }

            string str = $"Obj = {Type}\n\n"                             +
                            $"N = {nStr(list.Count)} of {nStr(N)}\n"     +
                            $"nTaskCount = {nTaskCount}\n"               +
                            $"lenMode = {lenMode}\n"                     +
                            $"maxConnectionDist = {maxConnectionDist}\n" +
                            $"renderDelay = {renderDelay}\n"             +
                            $"cell info: {getCellsInfo()}\n"             +
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
            x = rand.Next(gl_Width);
            y = rand.Next(gl_Height);

            cellId = (int)x / cellSize + ((int)y / cellSize) * cellRow;

            dx = myUtils.randFloatSigned(rand) * (rand.Next(5) + 1);
            dy = myUtils.randFloatSigned(rand) * (rand.Next(5) + 1);

            angle = 0;
            size = shape == 1 ? 2 : 3;
            angle = myUtils.randFloat(rand) * rand.Next(123);

            A = 0.5f + myUtils.randFloat(rand) * 0.5f;
            colorPicker.getColor(x, y, ref R, ref G, ref B);

            dic[cellId].items.Add(id, this);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            //x += dx;
            //y += dy;

            x += dx * 0.1f;
            y += dy * 0.1f;

#if true
            if (x < 0) dx += dSpeed; else if (x > gl_Width ) dx -= dSpeed;
            if (y < 0) dy += dSpeed; else if (y > gl_Height) dy -= dSpeed;
#else
            if(x < 0 )
#endif

            // Check if the particle has moved out of its current cell
            {
                int newCellId = (int)x / cellSize + ((int)y / cellSize) * cellRow;

                if (cellId != newCellId && dic.ContainsKey(newCellId))
                {
                    var oldDic = dic[cellId];
                    var newDic = dic[newCellId];

                    oldDic.items.Remove(id);
                    newDic.items.Add(id, this);

                    cellId = newCellId;
                }
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Show()
        {
            float size2x = size * 2;

            if (doShowCellBounds)
            {
                var cell = dic[cellId];
                myPrimitive._Rectangle.SetColor(1, 1, 1, 0.05f);
                myPrimitive._Rectangle.Draw(cell.x * cellSize, cell.y * cellSize, cell.w, cell.h);
            }

            switch (shape)
            {
                // Instanced squares
                case 0:
                    myPrimitive._RectangleInst.setInstanceCoords(x - size, y - size, size2x, size2x);
                    myPrimitive._RectangleInst.setInstanceColor(R, G, B, A);
                    myPrimitive._RectangleInst.setInstanceAngle(angle);
                    break;

                // Instanced triangles
                case 1:
                    myPrimitive._TriangleInst.setInstanceCoords(x, y, size2x, angle);
                    myPrimitive._TriangleInst.setInstanceColor(R, G, B, A);
                    break;

                // Instanced circles
                case 2:
                    myPrimitive._EllipseInst.setInstanceCoords(x, y, size2x, angle);
                    myPrimitive._EllipseInst.setInstanceColor(R, G, B, A);
                    break;

                // Instanced pentagons
                case 3:
                    myPrimitive._PentagonInst.setInstanceCoords(x, y, size2x, angle);
                    myPrimitive._PentagonInst.setInstanceColor(R, G, B, A);
                    break;

                // Instanced hexagons
                case 4:
                    myPrimitive._HexagonInst.setInstanceCoords(x, y, size2x, angle);
                    myPrimitive._HexagonInst.setInstanceColor(R, G, B, A);
                    break;
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Process(Window window)
        {
            initShapes();


            clearScreenSetup(doClearBuffer, 0.1f);


            if (doGenerateAll)
                while (list.Count < N)
                    list.Add(new myObj_9999_test_002b());


            switch (nTaskCount)
            {
                // Single thread
                case 1:
                    process1(window);
                    break;

                // Multiple threads
                default:
                    process2(window);
                    break;
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        // Single thread process
        private void process1(Window window)
        {
            uint cnt = 0;

            while (!Glfw.WindowShouldClose(window))
            {
                int Count = list.Count;

                processInput(window);

                // Swap fore/back framebuffers, and poll for operating system events.
                Glfw.SwapBuffers(window);
                Glfw.PollEvents();

                // Dim screen
                {
                    glClear(GL_COLOR_BUFFER_BIT);
                    grad.Draw();
                }

                // Render Frame
                {
                    inst.ResetBuffer();
                    myPrimitive._LineInst.ResetBuffer();

                    // Draw all the connecting lines between particles;
                    // This is the most time consuming part here, and is optimized using multimap approach
                    for (int i = 0; i != Count; i++)
                    {
                        list[i].showConnections();
                    }

                    // As we're working off a sortedList, Show and Move methods should be called from within the separate loops
                    for (int i = 0; i != Count; i++)
                    {
                        var obj = list[i];
                        obj.Show();
                        obj.Move();
                    }

                    myPrimitive._LineInst.Draw();

                    if (doFillShapes)
                    {
                        // Tell the fragment shader to multiply existing instance opacity by 0.5:
                        inst.SetColorA(-0.5f);
                        inst.Draw(true);
                    }

                    // Tell the fragment shader to do nothing with the existing instance opacity:
                    inst.SetColorA(0);
                    inst.Draw(false);
                }

                if (Count < N)
                {
                    list.Add(new myObj_9999_test_002b());
                }

                cnt++;
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        // Multiple threads process
        private void process2(Window window)
        {
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void initShapes()
        {
            base.initShapes(shape, N, 0);

            int n = N < 10 ? 100 : N * (N / 10);

            myPrimitive.init_LineInst(n);

            if (doShowCellBounds)
                myPrimitive.init_Rectangle();

            grad = new myScreenGradient();
            grad.SetRandomColors(rand, 0.2f);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void showConnections()
        {
            float dx, dy, dist2, a;

            for (int i = -1; i < 2; i++)
            {
                for (int j = -1; j < 2; j++)
                {
                    int cell = cellId + cellRow * i + j;

                    if (dic.ContainsKey(cell))
                    {
                        foreach (var other in dic[cell].items)
                        {
                            if (id != other.Value.id)
                            {
                                dx = x - other.Value.x;
                                dy = y - other.Value.y;

                                dist2 = dx * dx + dy * dy;

                                if (dist2 < maxDistSquared)
                                {
                                    a = (1.0f - dist2 * maxDistSquared_Inverted) * opacityFactor;

                                    myPrimitive._LineInst.setInstanceCoords(x, y, other.Value.x, other.Value.y);
                                    myPrimitive._LineInst.setInstanceColor(1, 1, 1, a);
                                }
                            }
                        }
                    }
                }
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private string getCellsInfo()
        {
            int nCells = dic.Count;
            int min = N;
            int max = 0;
            int avg = 0;
            int particles = 0;

            foreach (var Cell in dic)
            {
                int cnt = Cell.Value.items.Count;

                if (cnt < min)
                    min = cnt;

                if (cnt > max)
                    max = cnt;

                avg += cnt;
            }

            particles = avg;
            avg /= nCells;

            return $"\ncells = {nCells}\nparticles = {particles}\nmin = {min}\nmax = {max}\navg = {avg}";
        }

        // ---------------------------------------------------------------------------------------------------------------
    }
};
