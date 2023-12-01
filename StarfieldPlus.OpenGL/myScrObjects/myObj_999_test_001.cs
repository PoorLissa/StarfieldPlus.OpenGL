using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;


/*
    - Trails test
*/


namespace my
{
    public class myObj_999_test_001 : myObject
    {
        // Priority
        public static int Priority => 10;
        public static System.Type Type => typeof(myObj_999_test_001);

        private float x, y, dx, dy;
        private float size, A, R, G, B, angle = 0;

        private myParticleTrail trail = null;

        private static int N = 0, shape = 0, nTrailMax = 111, mode = 0;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_999_test_001()
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
                N = rand.Next(100) + 100;
                shape = rand.Next(5);

                N = 1000;

                mode = 2;
            }

            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time local initialization
        private void initLocal()
        {
            doClearBuffer = true;
            renderDelay = 0;

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            height = 600;

            string nStr(int   n) { return n.ToString("N0");    }
            //string fStr(float f) { return f.ToString("0.000"); }

            string str = $"Obj = {Type}\n\n"             			 +
                            $"N = {nStr(list.Count)} of {nStr(N)}\n" +
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
            A = 0.5f;

            switch (mode)
            {
                case 0:
                    x = gl_x0 + rand.Next(31) - 15;
                    y = gl_y0 + rand.Next(31) - 15;

                    dx = myUtils.randFloatSigned(rand, 0.01f) * 2;
                    dy = myUtils.randFloatSigned(rand, 0.01f) * 2;

                    colorPicker.getColor(x, y, ref R, ref G, ref B);
                    break;

                case 1:
                    x = rand.Next(gl_Width);
                    y = rand.Next(gl_Height);

                    dx = 0;
                    dy = myUtils.randFloat(rand, 0.01f) * 2;

                    colorPicker.getColor(x, y, ref R, ref G, ref B);
                    break;

                case 2:
                    A = myUtils.randFloat(rand, 0.1f);

                    x = rand.Next(gl_Width);
                    y = rand.Next(gl_Height);

                    colorPicker.getColor(x, y, ref R, ref G, ref B);

                    y = gl_Height;

                    dx = 0;
                    dy = -myUtils.randFloat(rand, 0.01f) * 3;
                    break;
            }

            size = rand.Next(3) + 3;

            int nTrail = 50 + rand.Next(nTrailMax);

            // Initialize Trail
            if (trail == null)
            {
                trail = new myParticleTrail(nTrail, x, y);
            }
            else
            {
                trail.reset(x, y);
            }

            if (trail != null)
            {
                trail.updateDa(A);
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            if (false)
            {
                trail.update(x, y);

                x += dx;
                y += dy;
            }
            else
            {
                x += dx;
                y += dy;

                float factor = 11;

                float extraX = 0;
                float extraY = 0;

                if (dy < 0)
                    factor = 3;

                switch (mode)
                {
                    case 2:
                        factor = 3 + (float)Math.Sin(y / 3) * 5;
                            extraX = myUtils.randFloat(rand) * 5;
                            extraY = myUtils.randFloat(rand) * 5;

                        if (dx == 0 && myUtils.randomChance(rand, 1, 33))
                        {
                            dx = dy * myUtils.randomSign(rand);
                            dy = 0;
                        }

                        if (dy == 0 && myUtils.randomChance(rand, 1, 15))
                        {
                            dy = dx < 0 ? dx : -dx;
                            dx = 0;
                        }

                        break;
                }

                float addX = dx + (float)Math.Sin(y / 4) * factor + extraX;
                float addY = dy + (float)Math.Sin(x / 4) * factor + extraY;

                trail.update(x + addX, y + addY);
            }

            // ---------------------------------------------------------

            switch (mode)
            {
                case 0:
                case 1:
                    {
                        if (x < 0 && dx < 0)
                            dx *= -1;

                        if (y < 0 && dy < 0)
                            dy *= -1;

                        if (x > gl_Width && dx > 0)
                            dx *= -1;

                        if (y > gl_Height && dy > 0)
                            dy *= -1;
                    }
                    break;

                case 2:
                    {
                        if (y < 0)
                        {
                            A -= 0.001f;

                            if (A < 0)
                                generateNew();
                        }
                    }
                    break;
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Show()
        {
            trail.Show (R, G, B, A);

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
            uint cnt = 0;
            initShapes();

            // Disable VSYNC if needed
            // Glfw.SwapInterval(0);

            clearScreenSetup(doClearBuffer, 0.1f);

            while (!Glfw.WindowShouldClose(window))
            {
                int Count = list.Count;

                processInput(window);

                // Swap fore/back framebuffers, and poll for operating system events.
                Glfw.SwapBuffers(window);
                Glfw.PollEvents();

                glClear(GL_COLOR_BUFFER_BIT);

                // Render Frame
                {
                    myPrimitive._LineInst.ResetBuffer();
                    inst.ResetBuffer();

                    for (int i = 0; i != Count; i++)
                    {
                        var obj = list[i] as myObj_999_test_001;

                        obj.Show();
                        obj.Move();
                    }

                    myPrimitive._LineInst.Draw();

                    // Tell the fragment shader to do nothing with the existing instance opacity:
                    inst.SetColorA(0);
                    inst.Draw(false);
                }

                if (Count < N)
                {
                    list.Add(new myObj_999_test_001());
                }

                cnt++;
                System.Threading.Thread.Sleep(renderDelay);
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void initShapes()
        {
            base.initShapes(shape, N, 0);
            myPrimitive.init_LineInst(N * nTrailMax);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------
    }
};
