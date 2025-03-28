using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;


/*
    - 
*/


namespace my
{
    public class myObj_1250 : myObject
    {
        // Priority
        public static int Priority => 10;
		public static System.Type Type => typeof(myObj_1250);

        private float x, y, dy, size, dSize;
        private float A, R, G, B;

        private static int N = 0;
        private static bool doFillShapes = false;
        private static float dimAlpha = 0.05f;

        private static myScreenGradient grad = null;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_1250()
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
                N = 11;
            }

            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time local initialization
        private void initLocal()
        {
            doClearBuffer = false;
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
            size = rand.Next(50) + 100;

            switch (rand.Next(2))
            {
                case 0:
                    y = gl_Height + rand.Next(200);
                    dy = 20 + rand.Next(10);
                    break;

                case 1:
                    y = 0 - rand.Next(200);
                    dy = -20 - rand.Next(10);
                    break;
            }

            x = rand.Next(gl_Width);
            size = rand.Next(100) + 100;
            dSize = 2.5f + myUtils.randFloatClamped(rand, 0.25f) * 1.5f;

/*
            x = rand.Next(gl_Width);
            y = gl_Height + rand.Next(200);

            dy = 20 + rand.Next(30);
            dSize = 1.0f + myUtils.randFloatClamped(rand, 0.25f) * rand.Next(7);

            size = rand.Next(100) + 100;
            dSize = 2.5f + myUtils.randFloatClamped(rand, 0.25f) * 1.5f;
            dy = 20 + rand.Next(10);
*/
            A = 1;
            R = (float)rand.NextDouble();
            G = (float)rand.NextDouble();
            B = (float)rand.NextDouble();

            //colorPicker.getColor(x, y, ref R, ref G, ref B);
            //R = G = B = 0.33f;

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            y -= dy;
            dy -= dy > 0 ? 0.4f : -0.4f;
            size -= dSize;

            R += myUtils.randFloat(rand) * 0.025f;
            G += myUtils.randFloat(rand) * 0.025f;
            B += myUtils.randFloat(rand) * 0.025f;

            if (size < 1)
            {
                generateNew();
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Show()
        {
            if (dy > 0)
            {
                myPrimitive._TriangleInst.setInstanceCoords(x, y, size, 0);
                myPrimitive._TriangleInst.setInstanceColor(R, G, B, A);
            }
            else
            {
                myPrimitive._TriangleInst.setInstanceCoords(x, y, size, (float)Math.PI);
                myPrimitive._TriangleInst.setInstanceColor(R, G, B, A);
            }
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Process(Window window)
        {
            uint cnt = 0;
            initShapes();

            clearScreenSetup(doClearBuffer, 0.1f, true);

            stopwatch = new StarfieldPlus.OpenGL.myUtils.myStopwatch(true);

            grad.SetOpacity(1);
            grad.Draw();
            grad.SetOpacity(0.005f);

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
                        glClear(GL_COLOR_BUFFER_BIT);

                    grad.Draw();
                }

                // Render Frame
                {
                    inst.ResetBuffer();

                    for (int i = 0; i != Count; i++)
                    {
                        var obj = list[i] as myObj_1250;

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

                if (Count < N)
                {
                    list.Add(new myObj_1250());
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
            base.initShapes(1, N, 0);

            grad = new myScreenGradient();
            grad.SetRandomColors(rand, 0.2f);
            grad.SetOpacity(0.005f);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------
    }
};
