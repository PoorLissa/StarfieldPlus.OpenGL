using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;


/*
    - Doom style prysms
*/


namespace my
{
    public class myObj_1280 : myObject
    {
        // Priority
        public static int Priority => 10;
		public static System.Type Type => typeof(myObj_1280);

        private int yMax, yMin;
        private float x, y, dx, dy;
        private float sizeX, sizeY, A, R, G, B, angle = 0;

        private static int N = 0, shape = 0;
        private static bool doFillShapes = false;
        private static float dimAlpha = 0.05f;

        private static myScreenGradient grad = null;
        private static Polygon4 p4 = null;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_1280()
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
                N = 100;
            }

            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time local initialization
        private void initLocal()
        {
            doClearBuffer = myUtils.randomChance(rand, 1, 2);
            doClearBuffer = true;

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
            x = rand.Next(gl_Width);
            y = gl_Height + 100 + rand.Next(123);

            dx = 0;
            dy = myUtils.randFloatClamped(rand, 0.25f) * (rand.Next(10) + 1) * -1.0f;

            sizeX = 100;
            sizeY = sizeX/2;

            yMax = gl_Height - rand.Next(333);
            yMin = rand.Next(333);

            A = 1;
            R = (float)rand.NextDouble();
            G = (float)rand.NextDouble();
            B = (float)rand.NextDouble();

            R = G = B = 0.3f;

            //colorPicker.getColor(x, y, ref R, ref G, ref B);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            x += dx;
            y += dy;

            if (y < yMin && dy < 0)
            {
                dy *= -1;
            }

            if (y > yMax && dy > 0)
            {
                dy *= -1;
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Show()
        {
            float x1 = x - sizeX;
            float y1 = y;
            float x2 = x;
            float y2 = y - sizeY;
            float x3 = x;
            float y3 = y + sizeY;
            float x4 = x + sizeX;
            float y4 = y;

            p4.SetColor(R, G, B, A);
            p4.Draw(x1, y1, x2, y2, x3, y3, x4, y4, !false);

            p4.SetColor(R * 1.1f, G * 1.1f, B * 1.1f, A);
            p4.Draw(x1, y1, x3, y3, x1, gl_Height, x3, gl_Height, !false);

            p4.SetColor(R * 0.9f, G * 0.9f, B * 0.9f, A);
            p4.Draw(x4, y4, x3, y3, x4, gl_Height, x3, gl_Height, !false);

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
                    for (int i = 0; i != Count; i++)
                    {
                        var obj = list[i] as myObj_1280;

                        obj.Show();
                        obj.Move();
                    }
                }

                if (Count < N)
                {
                    list.Add(new myObj_1280());
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

            grad = new myScreenGradient();
            grad.SetRandomColors(rand, 0.2f);

            p4 = new Polygon4();

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------
    }
};
