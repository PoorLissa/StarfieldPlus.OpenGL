using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;


/*
    - Semispheres growing into the screen space from the screen borders
*/


namespace my
{
    public class myObj_1360 : myObject
    {
        // Priority
        public static int Priority => 10;
		public static System.Type Type => typeof(myObj_1360);

        private float x, y, rad, dRad;
        private float A, R, G, B;

        private static int N = 0, mode = 0, maxRad = 0;
        private static bool doFillShapes = false;
        private static float dimAlpha = 0.05f, dA = 0;

        private static myScreenGradient grad = null;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_1360()
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
                N = rand.Next(10) + 10;
                dA = 0.001f;

                mode = rand.Next(4);

                switch (mode)
                {
                    case 0:
                        maxRad = gl_Width;
                        break;

                    case 1:
                        maxRad = gl_Height;
                        break;

                    case 2:
                        maxRad = rand.Next(333) + 222;
                        N = 50;
                        break;

                    case 3:
                        maxRad = rand.Next(50) + 11;
                        N = 50 + rand.Next(25);
                        dA = 0.003f + myUtils.randFloat(rand) * 0.003f;
                        break;
                }
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
                            $"mode = {mode}\n"                +
                            $"maxRad = {maxRad}\n"            +
                            $"dA = {myUtils.fStr(dA)}\n"      +
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
            switch (rand.Next(4))
            {
                case 0:
                    x = 0;
                    y = rand.Next(gl_Height);
                    break;

                case 1:
                    x = gl_Width;
                    y = rand.Next(gl_Height);
                    break;

                case 2:
                    x = rand.Next(gl_Width);
                    y = 0;
                    break;

                case 3:
                    x = rand.Next(gl_Width);
                    y = gl_Height;
                    break;
            }

            rad = 0;
            dRad = myUtils.randFloatClamped(rand, 0.25f) * (rand.Next(4) + 2);

            A = 0.25f + myUtils.randFloat(rand) * 0.5f;
            colorPicker.getColorRand(ref R, ref G, ref B);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            rad += dRad;

            if (rad > maxRad)
            {
                A -= dA;

                if (A < 0)
                    generateNew();
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Show()
        {
            myPrimitive._EllipseInst.setInstanceCoords(x, y, rad, 0);
            myPrimitive._EllipseInst.setInstanceColor(R, G, B, A);
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
                        var obj = list[i] as myObj_1360;

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
                    list.Add(new myObj_1360());
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
