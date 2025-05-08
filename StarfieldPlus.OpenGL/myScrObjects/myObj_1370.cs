using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;


/*
    - Unfinished
*/


namespace my
{
    public class myObj_1370 : myObject
    {
        // Priority
        public static int Priority => 1;
		public static System.Type Type => typeof(myObj_1370);

        private int cnt;
        private float x, y, dx, dy;
        private float size, a, dA, A, R, G, B, angle = 0;

        private static int N = 0, n1 = 0, n2 = 0;
        private static bool doFillShapes = false;
        private static float dimAlpha = 0.05f;

        private static myScreenGradient grad = null;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_1370()
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
                N = rand.Next(1000) + 1000;

                n1 = 111;
                n2 = 13;
            }

            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time local initialization
        private void initLocal()
        {
            doClearBuffer = true;
            doFillShapes = true;

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            height = 600;

            string str = $"Obj = {Type}\n\n"                  +
                            myUtils.strCountOf(list.Count, N) +
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
            if (id < n1)
            {
                x = rand.Next(gl_Width);
                y = gl_Height;

                dx = dy = 0;

                cnt = 333 + rand.Next(666);
                size = rand.Next(234) + 50;

                colorPicker.getColor(x, y, ref R, ref G, ref B);

                a = 0;
                A = 0.45f + myUtils.randFloat(rand) * 0.5f;
                dA = 0.001f + myUtils.randFloat(rand) * 0.005f;

                R = G = B = 0.33f + myUtils.randFloat(rand) * 0.66f;

                A = 1;

                y -= (1.0f - R) * 300;
            }
            else if (id < n1 + n2)
            {
                x = rand.Next(gl_Width);
                y = gl_Height + 100;

                dx = 0;
                dy = -1 * myUtils.randFloat(rand, 0.2f) * 3;

                size = size = rand.Next(111) + 25;

                a = 1.0f;
                colorPicker.getColorRand(ref R, ref G, ref B);
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            x += dx;
            y += dy;

            if (id < n1)
            {
                if (dA > 0)
                {
                    a += dA;
                }
                else
                {
                    if (--cnt < 0)
                        a += dA;
                }

                if (a >= A && dA > 0)
                {
                    dA *= -1;
                }

                if (a < 0)
                {
                    generateNew();
                }

                return;
            }

            if (id < n1 + n2)
            {
                if (y < -size)
                    generateNew();
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Show()
        {
            float size2x = size * 2;

            if (id < n1)
            {
                myPrimitive._EllipseInst.setInstanceCoords(x, y, size2x, angle);
                myPrimitive._EllipseInst.setInstanceColor(R, G, B, a);
            }
            else if (id < n1 + n2)
            {
                myPrimitive._EllipseInst.setInstanceCoords(x, y, size2x, angle);
                myPrimitive._EllipseInst.setInstanceColor(R, G, B, a);
            }

/*
            myPrimitive._RectangleInst.setInstanceCoords(x - size, y - size, size2x, size2x);
            myPrimitive._RectangleInst.setInstanceColor(R, G, B, A);
            myPrimitive._RectangleInst.setInstanceAngle(angle);
*/
            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Process(Window window)
        {
            uint cnt = 0;
            initShapes();

            clearScreenSetup(doClearBuffer, 0.1f);

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
                    inst.ResetBuffer();

                    for (int i = 0; i != Count; i++)
                    {
                        if (i >= n1)
                            break;

                        var obj = list[i] as myObj_1370;

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

                    if (Count > n1)
                    {
                        inst.ResetBuffer();

                        for (int i = n1; i != Count; i++)
                        {
                            if (i >= n1 + n2)
                                break;

                            var obj = list[i] as myObj_1370;

                            obj.Show();
                            obj.Move();
                        }

                        if (doFillShapes)
                        {
                            // Tell the fragment shader to multiply existing instance opacity by 0.5:
                            inst.SetColorA(-0.99f);
                            inst.Draw(true);
                        }

                        // Tell the fragment shader to do nothing with the existing instance opacity:
                        inst.SetColorA(0);
                        inst.Draw(false);
                    }
                }

                if (Count < N)
                {
                    list.Add(new myObj_1370());
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
            base.initShapes(2, N, 0);

            grad = new myScreenGradient();
            grad.SetRandomColors(rand, 0.2f);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------
    }
};
