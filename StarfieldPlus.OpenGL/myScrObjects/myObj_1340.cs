using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;


/*
    - 
*/


namespace my
{
    public class myObj_1340 : myObject
    {
        // Priority
        public static int Priority => 10;
		public static System.Type Type => typeof(myObj_1340);

        private int cnt, parent_id, dir;
        private float x, y, dx, dy, rad;
        private float size, A, R, G, B, angle = 0, dAngle;

        private static int N = 0, n = 0, shape = 0, dAngleMode = 0, radMode = 0;
        private static bool doFillShapes = false;

        private List<myObj_1340> children = null;

        static myTexRectangle tex = null;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_1340()
        {
            parent_id = -1;

            if (id != uint.MaxValue)
                generateNew();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time global initialization
        protected override void initGlobal()
        {
            colorPicker = new myColorPicker(gl_Width, gl_Height, mode: myColorPicker.colorMode.SNAPSHOT_OR_IMAGE);
            list = new List<myObject>();

            // Global unmutable constants
            {
                N = 100000;
                n = rand.Next(7) + 1;

                shape = rand.Next(5);
            }

            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time local initialization
        private void initLocal()
        {
            doClearBuffer = myUtils.randomChance(rand, 99, 100);
            doFillShapes = true;

            dAngleMode = rand.Next(3);
            radMode = rand.Next(2);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            height = 600;

            string str = $"Obj = {Type}\n\n"                  +
                            myUtils.strCountOf(list.Count, N) +
                            $"n = {n}\n"                      +
                            $"radMode = {radMode}\n"          +
                            $"dAngleMode = {dAngleMode}\n"    +
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
                cnt = 333 + rand.Next(999);

                x = rand.Next(gl_Width);
                y = rand.Next(gl_Height);

                rad = 123 + rand.Next(666);
                size = rand.Next(5) + 3;
                dir = rand.Next(3);

                dAngle = myUtils.randFloatClamped(rand, 0.1f) * 0.005f;

                if (children == null)
                {
                    children = new List<myObj_1340>();
                }
            }
            else
            {
                if (parent_id < 0)
                {
                    parent_id = rand.Next(n);
                    (list[parent_id] as myObj_1340).children.Add(this);
                }

                var parent = list[parent_id] as myObj_1340;

                size = parent.size;
                angle = myUtils.randFloat(rand) * 321;

                switch (radMode)
                {
                    case 0:
                        rad = rand.Next((int)parent.rad);
                        break;

                    case 1:
                        rad = 100 + rand.Next(123) + rand.Next((int)parent.rad) / 2;
                        break;
                }

                switch (dAngleMode)
                {
                    case 0:
                        dAngle = parent.dAngle;
                        break;

                    case 1:
                        dAngle = (rad / parent.rad) * 0.01f;
                        break;

                    case 2:
                        dAngle = myUtils.randFloatClamped(rand, 0.1f) * 0.005f;
                        break;
                }

                switch (parent.dir)
                {
                    case 0:
                        dAngle *= +1;
                        break;

                    case 1:
                        dAngle *= +1;
                        break;

                    case 2:
                        dAngle *= myUtils.randomSign(rand);
                        break;
                }

                x = parent.x + rad * (float)Math.Sin(angle);
                y = parent.y + rad * (float)Math.Cos(angle);

                A = myUtils.randFloat(rand);
                colorPicker.getColorAverage(x, y, 3, 3, ref R, ref G, ref B);
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            if (id < n)
            {
                int Count = children.Count;
                int negativeOpacityCnt = 0;
                cnt--;

                for (int i = 0; i < Count; i++)
                {
                    var child = children[i];

                    child.angle += child.dAngle;

                    child.x = x + child.rad * (float)Math.Sin(child.angle);
                    child.y = y + child.rad * (float)Math.Cos(child.angle);

                    if (cnt < 0)
                    {
                        child.A -= 0.001f;

                        if (child.A < 0)
                            negativeOpacityCnt++;
                    }
                }

                if (negativeOpacityCnt == Count)
                {
                    generateNew();

                    for (int i = 0; i < Count; i++)
                    {
                        var child = children[i];
                        child.generateNew();
                    }
                }
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Show()
        {
            if (id < n)
            {
                for (int i = 0; i < children.Count; i++)
                {
                    var child = children[i];

                    float size2x = child.size * 2;

                    switch (shape)
                    {
                        // Instanced squares
                        case 0:
                            myPrimitive._RectangleInst.setInstanceCoords(child.x - child.size, child.y - child.size, size2x, size2x);
                            myPrimitive._RectangleInst.setInstanceColor(child.R, child.G, child.B, child.A);
                            myPrimitive._RectangleInst.setInstanceAngle(child.angle);
                            break;

                        // Instanced triangles
                        case 1:
                            myPrimitive._TriangleInst.setInstanceCoords(child.x, child.y, child.size, child.angle);
                            myPrimitive._TriangleInst.setInstanceColor(child.R, child.G, child.B, child.A);
                            break;

                        // Instanced circles
                        case 2:
                            myPrimitive._EllipseInst.setInstanceCoords(child.x, child.y, size2x, child.angle);
                            myPrimitive._EllipseInst.setInstanceColor(child.R, child.G, child.B, child.A);
                            break;

                        // Instanced pentagons
                        case 3:
                            myPrimitive._PentagonInst.setInstanceCoords(child.x, child.y, size2x, child.angle);
                            myPrimitive._PentagonInst.setInstanceColor(child.R, child.G, child.B, child.A);
                            break;

                        // Instanced hexagons
                        case 4:
                            myPrimitive._HexagonInst.setInstanceCoords(child.x, child.y, size2x, child.angle);
                            myPrimitive._HexagonInst.setInstanceColor(child.R, child.G, child.B, child.A);
                            break;
                    }
                }
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Process(Window window)
        {
            uint cnt = 0;
            initShapes();

            clearScreenSetup(doClearBuffer, 0.1f);

            while (list.Count < N)
            {
                list.Add(new myObj_1340());
            }

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
                    tex.setOpacity(1.0f);
                    tex.Draw(0, 0, gl_Width, gl_Height);
                }

                // Render Frame
                {
                    inst.ResetBuffer();

                    for (int i = 0; i != n; i++)
                    {
                        var obj = list[i] as myObj_1340;

                        obj.Show();
                        obj.Move();
                    }

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
                    list.Add(new myObj_1340());
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
            base.initShapes(shape, N, 0);

            tex = new myTexRectangle(colorPicker.getImg());

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------
    }
};
