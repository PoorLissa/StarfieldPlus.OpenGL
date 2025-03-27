using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;


/*
    - Several generators that produce concentric circles of small opacity
*/


namespace my
{
    public class myObj_1240 : myObject
    {
        // Priority
        public static int Priority => 10;
		public static System.Type Type => typeof(myObj_1240);

        private int cnt;
        private float x, y, rad, dRad;
        private float A, R, G, B;

        private static int N = 0, n = 0, shape = 0;
        private static int dRadMode = 0, posMode = 0, posOffset = 0;
        private static bool doFillShapes = false;
        private static float dimAlpha = 0.05f;

        private static myScreenGradient grad = null;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_1240()
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
                n = 7;
                N = 3000;
                shape = 2;
            }

            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time local initialization
        private void initLocal()
        {
            doClearBuffer = myUtils.randomChance(rand, 19, 20);

            dRadMode = rand.Next(2);
            posMode = rand.Next(2);

            posOffset = 2 * rand.Next(5) + 3;       // 3, 5, 7, 9, 11

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            height = 600;

            string str = $"Obj = {Type}\n\n"                  +
                            myUtils.strCountOf(list.Count, N) +
                            $"n = {n}\n"                      +
                            $"dRadMode = {dRadMode}\n"        +
                            $"posMode = {posMode}\n"          +
                            $"posOffset = {posOffset}\n"      +
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
                cnt = 666 + rand.Next(1234);

                x = rand.Next(gl_Width);
                y = rand.Next(gl_Height);

                dRad = myUtils.randFloatClamped(rand, 0.25f) * (2 + rand.Next(4));

                colorPicker.getColor(x, y, ref R, ref G, ref B);
            }
            else
            {
                var parent = list[rand.Next(n)] as myObj_1240;

                x = parent.x;
                y = parent.y;
                rad = 0;

                switch (posMode)
                {
                    case 1:
                        x += rand.Next(posOffset * 2) - posOffset;
                        y += rand.Next(posOffset * 2) - posOffset;
                        break;
                }

                switch (dRadMode)
                {
                    case 0:
                        dRad = parent.dRad;
                        break;

                    case 1:
                        dRad = myUtils.randFloatClamped(rand, 0.25f) * (2 + rand.Next(4));
                        break;
                }

                A = 0.05f + myUtils.randFloat(rand) * 0.075f;

                R = parent.R;
                G = parent.G;
                B = parent.B;

                //R = G = B = 0.3f;
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            if (id < n)
            {
                if (--cnt == 0)
                {
                    generateNew();
                }
            }
            else
            {
                rad += dRad;
                dRad *= 1.0002f;

                if (rad > gl_x0)
                {
                    A -= 0.003f;

                    if (A < 0)
                    {
                        generateNew();
                    }
                }
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Show()
        {
            myPrimitive._EllipseInst.setInstanceCoords(x, y, rad, 0);
            myPrimitive._EllipseInst.setInstanceColor(R, G, B, A);

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
                        var obj = list[i] as myObj_1240;

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
                    list.Add(new myObj_1240());
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

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------
    }
};
