using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;
using StarfieldPlus.OpenGL.myUtils;


/*
    - Black hole
*/


namespace my
{
    public class myObj_1120 : myObject
    {
        // Priority
        public static int Priority => 999910;
		public static System.Type Type => typeof(myObj_1120);

        private int cnt;
        private float x, y, dx, dy;
        private float size, A, R, G, B, angle = 0, dAngle;

        private static int N = 0, shape = 0;
        private static bool doFillShapes = false, doAccelerate = false;
        private static float dimAlpha = 0.05f, X, Y, Rad, maxOpacity = 1;

        private static myScreenGradient grad = null;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_1120()
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
                N = rand.Next(33333) + 3333;

                shape = rand.Next(5);
            }

            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time local initialization
        private void initLocal()
        {
            doClearBuffer = myUtils.randomChance(rand, 999, 1000);
            doFillShapes = myUtils.randomChance(rand, 2, 3);
            doAccelerate = myUtils.randomBool(rand);

            renderDelay = rand.Next(11) + 3;

            X = gl_x0;
            Y = gl_y0;
            Rad = 333 + rand.Next(333);

            maxOpacity = 0.5f;

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            height = 600;

            string str = $"Obj = {Type}\n\n"                   +
                            myUtils.strCountOf(list.Count, N)  +
                            $"doAccelerate = {doAccelerate}\n" +
                            $"renderDelay = {renderDelay}\n"   +
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
            float dX = 0, dY = 0, dist = 0;

            cnt = 100 + rand.Next(123);

            int off = 600;
            int W = gl_Width + off;

            // Make sure the particle generates outside of the event horizon
            do
            {
                x = rand.Next(W) - off/2;
                y = rand.Next(W) - (gl_Width - gl_Height) / 2 - off/2;

                dX = x - X;
                dY = y - Y;
                dist = (float)Math.Sqrt(dX * dX + dY * dY);
            }
            while (dist < Rad * 1.25f);

            if (doAccelerate)
            {
                dx = dy = 0;
            }
            else
            {
                float spd = -0.75f;

                dx = spd * dX / dist;
                dy = spd * dY / dist;
            }

            size = rand.Next(3) + 3;
            dAngle = myUtils.randFloatSigned(rand) * 0.05f;

            A = myUtils.randFloat(rand, 0.25f) * maxOpacity;
            colorPicker.getColor(x, y, ref R, ref G, ref B);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            angle += dAngle;

            float dX = x - X;
            float dY = y - Y;
            float dist = (float)Math.Sqrt(dX * dX + dY * dY);

            if (doAccelerate)
            {
                float spd = 0.003f;

                dx -= spd * dX / dist;
                dy -= spd * dY / dist;
            }

            x += dx;
            y += dy;

            if (dist < Rad)
            {
                A -= 0.02f * maxOpacity;
                size -= 0.02f;

                dx *= 1.01f;
                dy *= 1.01f;

                if (doAccelerate == false)
                {
                    dx *= 1.025f;
                    dy *= 1.025f;
                }

                R += 0.02f;
                G += 0.02f;
                B += 0.02f;

                if (A < 0)
                    generateNew();
            }
            else
            {
                if (--cnt == 0)
                {
                    A = myUtils.randFloat(rand, 0.25f) * maxOpacity;
                    cnt = 100 + rand.Next(123);
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

            clearScreenSetup(doClearBuffer, 0.1f);

            stopwatch = new myStopwatch();
            stopwatch.Start();

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
                    inst.ResetBuffer();

                    myPrimitive._Ellipse.SetColor(1, 1, 1, 0.05f);
                    myPrimitive._Ellipse.Draw(X - Rad, Y - Rad, 2 * Rad, 2 * Rad, false);

                    for (int i = 0; i != Count; i++)
                    {
                        var obj = list[i] as myObj_1120;

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
                    list.Add(new myObj_1120());
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

            grad = new myScreenGradient();
            grad.SetRandomColors(rand, 0.2f);

            myPrimitive.init_Ellipse();

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------
    }
};
