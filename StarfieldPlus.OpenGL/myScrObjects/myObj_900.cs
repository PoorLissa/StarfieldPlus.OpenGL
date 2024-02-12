using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;
using static System.Net.Mime.MediaTypeNames;


/*
    - ...
*/


namespace my
{
    public class myObj_900 : myObject
    {
        // Priority
        public static int Priority => 999910;
		public static System.Type Type => typeof(myObj_900);

        private int cnt, lastId;
        private float x, y, dx, dy;
        private float size, A, R, G, B, angle = 0, dAngle = 0;

        private static int N = 0, n = 0, shape = 0, dyMode = 0;
        private static bool doFillShapes = false;
        private static float dimAlpha = 0.05f, lineA = 1, lineWidth = 1, speedFactor = 1;

        private static int cellSize = 1, cellX = 0, cellY = 0;

        private static myScreenGradient grad = null;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_900()
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

            // Global unmutable constants
            {
                n = 3 + rand.Next(33);
                N = rand.Next(11111) + 12345;

                shape = rand.Next(5);

                lineA = 0.5f;
                lineWidth = 3.0f + rand.Next(7);

                speedFactor = 1.0f + myUtils.randFloat(rand) * rand.Next(5);

                dyMode = rand.Next(2);

                // Grid setup
                cellSize = 50 + rand.Next(150);
                cellX = (gl_Width  % cellSize) / 2;
                cellY = (gl_Height % cellSize) / 2;
            }

            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time local initialization
        private void initLocal()
        {
            doClearBuffer = myUtils.randomChance(rand, 20, 21);
            renderDelay = rand.Next(2);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            height = 600;

            string nStr(int   n) { return n.ToString("N0");    }
            string fStr(float f) { return f.ToString("0.000"); }

            string str = $"Obj = {Type}\n\n"                         +
                            $"N = {nStr(list.Count)} of {nStr(N)}\n" +
                            $"n = {nStr(n)}\n"                       +
                            $"shape = {shape}\n"                     +
                            $"dyMode = {dyMode}\n"                   +
                            $"lineA = {fStr(lineA)}\n"               +
                            $"lineWidth = {fStr(lineWidth)}\n"       +
                            $"speedFactor = {fStr(speedFactor)}\n"   +
                            $"renderDelay = {renderDelay}\n"         +
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
                cnt = rand.Next(100);
                lastId = (int)id;

                x = 0;
                y = rand.Next(gl_Height);

                dx = 0.5f + myUtils.randFloat(rand) * speedFactor;
                dy = myUtils.randFloatSigned(rand);

                //colorPicker.getColor(x, y, ref R, ref G, ref B);

                R = (float)rand.NextDouble();
                G = (float)rand.NextDouble();
                B = (float)rand.NextDouble();
            }
            else
            {
                var parent = list[rand.Next(n)] as myObj_900;

                x = parent.x;
                y = parent.y;

                dx = parent.dx;

                switch (dyMode)
                {
                    case 0: dy = 0; break;
                    case 1: dy = parent.dy * 0.1f; break;
                }

                angle = 0;
                dAngle = myUtils.randFloatSigned(rand) * 0.05f;

                lastId = parent.lastId;
                parent.lastId = (int)id;

                size = rand.Next(3) + 3;

                A = myUtils.randFloat(rand);

                R = parent.R;
                G = parent.G;
                B = parent.R;
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            y += dy;

            if (id < n)
            {
                // Parent objects moving along y-axis
                if (--cnt == 0)
                {
                    cnt = rand.Next(100);
                    dy = myUtils.randFloatSigned(rand);
                }

                if (y < 0)
                    dy += 0.01f;

                if (y > gl_Height)
                    dy -= 0.01f;
            }
            else
            {
                // Child objects moving along x-axis
                x += dx;
                angle += dAngle;

                if (x < 0 || x > gl_Width)
                {
                    generateNew();
                }
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Show()
        {
            float size2x = size * 2;

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
                    myPrimitive._TriangleInst.setInstanceCoords(x, y, size, angle);
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

            // Connection lines
            //if (id > n && lastId > n)
            {
                var next = list[lastId] as myObj_900;

                if (x < next.x)
                {
                    myPrimitive._LineInst.setInstance(x, y, next.x, next.y, R, G, B, lineA);
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

            while (!Glfw.WindowShouldClose(window))
            {
                int Count = list.Count;

                processInput(window);

                // Swap fore/back framebuffers, and poll for operating system events.
                Glfw.SwapBuffers(window);
                Glfw.PollEvents();

                // Dim screen
                {
                    grad.Draw();
                }

                // Render Frame
                {
                    showGrid();

                    inst.ResetBuffer();
                    myPrimitive._LineInst.ResetBuffer();

                    for (int i = 0; i != Count; i++)
                    {
                        var obj = list[i] as myObj_900;

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

                    myPrimitive._LineInst.Draw();
                }

                if (Count < N)
                {
                    list.Add(new myObj_900());
                }

                cnt++;
                System.Threading.Thread.Sleep(renderDelay);
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void initShapes()
        {
            myPrimitive.init_ScrDimmer();
            base.initShapes(shape, N, 0);

            grad = new myScreenGradient();
            grad.SetRandomColors(rand, 0.23f);

            if (doClearBuffer == false)
            {
                grad.SetOpacity(0.01f);
            }

            myPrimitive.init_LineInst(N);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void showGrid()
        {
            myPrimitive._LineInst.ResetBuffer();
            myPrimitive._LineInst.setLineWidth(1);

            for (int i = cellX; i < gl_Width; i += cellSize)
            {
                myPrimitive._LineInst.setInstance(i, 0, i, gl_Height, 1, 1, 1, 0.25f);
            }

            for (int i = cellY; i < gl_Height; i += cellSize)
            {
                myPrimitive._LineInst.setInstance(0, i, gl_Width, i, 1, 1, 1, 0.25f);
            }

            myPrimitive._LineInst.Draw();
            myPrimitive._LineInst.setLineWidth(lineWidth);
        }
    }
};
