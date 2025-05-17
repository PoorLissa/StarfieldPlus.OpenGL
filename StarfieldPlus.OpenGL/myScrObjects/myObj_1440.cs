using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;


/*
    - Depth focus test
    - Reference: https://www.youtube.com/watch?v=R5jIoLnL_nE&ab_channel=JosuRelax
*/


namespace my
{
    public class myObj_1440 : myObject
    {
        // Priority
        public static int Priority => 10;
		public static System.Type Type => typeof(myObj_1440);

        private float x, y, dx, dy;
        private float size, A, R, G, B;
        private float focus;

        private static int N = 0, mode = 0, shaderNo = 0;
        private static int linearSpeed = 0;
        private static float dimAlpha = 0.05f, t = 0, dt = 0;

        private static myScreenGradient grad = null;
        private static myFreeShader_001 shader = null;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_1440()
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
                mode = rand.Next(3);
                shaderNo = rand.Next(2);

                switch (mode)
                {
                    case 0:
                        N = 123;
                        break;

                    case 1:
                        N = 33;
                        break;

                    case 2:
                        N = 1234;
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

            linearSpeed = myUtils.randomChance(rand, 2, 3)
                ? rand.Next(3) + 2
                : rand.Next(5) + 2;

            t = 0;
            dt = 0.0001f;

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            height = 600;

            string str = $"Obj = {Type}\n\n"                    +
                            myUtils.strCountOf(list.Count, N)   +
                            $"mode = {mode}\n"                  +
                            $"shaderNo = {shaderNo}\n"          +
                            $"linearSpeed = {linearSpeed}\n"    +
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
            // Pick size
            {
                if (myUtils.randomChance(rand, 2, 3))
                {
                    // Normal
                    size = rand.Next(333) + 111;
                }
                else
                {
                    if (myUtils.randomChance(rand, 1, 3))
                    {
                        // Small
                        size = rand.Next(11) + 3;
                    }
                    else
                    {
                        // Tiny
                        size = rand.Next(5) + 3;
                    }
                }
            }

            dx = myUtils.randFloatSigned(rand, 0.1f) * linearSpeed;
            dy = 0;
            x = dx > 0 ? -size : gl_Width + size;

            A = 0.1f + myUtils.randFloat(rand) * 0.5f;
            focus = 0.01f + myUtils.randFloat(rand) * 0.01f;

            if (myUtils.randomChance(rand, 1, 111))
            {
                focus = 0.01f + myUtils.randFloat(rand) * 0.3f;
            }

            switch (mode)
            {
                case 0:
                    y = rand.Next(gl_Height);
                    break;

                case 1:
                    y = gl_y0;
                    break;

                case 2:
                    size = rand.Next(66) + 3;
                    y = rand.Next(gl_Height);
                    focus = (float)(Math.Abs(y - gl_y0)) / gl_y0;
                    focus *= 0.05f;
                    focus += 0.001f;
                    break;
            }

            colorPicker.getColorRand(ref R, ref G, ref B);

            // Brighten up small/tiny particles
            if (size < 10)
            {
                do {

                    R += 0.0001f;
                    G += 0.0001f;
                    B += 0.0001f;

                } while (R + G + B < 2.33f);
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            x += dx;
            y += dy;

            if (x < -size && dx < 0)
                generateNew();

            if (x > gl_Width + size && dx > 0)
                generateNew();

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Show()
        {
            int off = 150 + (int)(size * 0.25f);

            shader.SetColor(R, G, B, A);
            shader.Draw(x, y, size, size, focus, off);
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
                    for (int i = 0; i != Count; i++)
                    {
                        var obj = list[i] as myObj_1440;

                        obj.Show();
                        obj.Move();
                    }
                }

                //if (Count < N && myUtils.randomChance(rand, 1, 50))
                if (Count < N)
                {
                    list.Add(new myObj_1440());
                }

                stopwatch.WaitAndRestart();
                t += dt;
                cnt++;
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void initShapes()
        {
            myPrimitive.init_ScrDimmer();

            getShader();

            grad = new myScreenGradient();
            grad.SetRandomColors(rand, 0.2f);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        // General shader selector
        private void getShader()
        {
            string fHeader = "", fMain = "";

            switch (shaderNo)
            {
                case 0:
                    myFreeShader_001.getShader_000(ref fHeader, ref fMain);
                    break;

                case 1:
                    myFreeShader_001.getShader_001(ref fHeader, ref fMain);
                    break;
            }

            shader = new myFreeShader_001(fHeader, fMain);
        }

        // ---------------------------------------------------------------------------------------------------------------
    }
};
