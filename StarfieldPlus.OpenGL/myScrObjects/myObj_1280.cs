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

        private int yMax, yMin, cnt;
        private float x, y, dx, dy;
        private float sizeX, sizeY, A, R, G, B;

        private static int N = 0, spdMax = 1, waitTimeMax = 1;
        private static int opacityMode = 0, sizeMode = 0;
        private static float dimAlpha = 0.05f, sizeYfactor = 1;

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
                N = 100 + rand.Next(100);
            }

            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time local initialization
        private void initLocal()
        {
            doClearBuffer = myUtils.randomChance(rand, 1, 2);
            doClearBuffer = true;

            sizeMode = rand.Next(2);
            opacityMode = rand.Next(4);

            spdMax = 1 + rand.Next(5);
            waitTimeMax = rand.Next(150);
            sizeYfactor = 1.0f + myUtils.randFloat(rand) * (rand.Next(3) + 1);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            height = 600;

            string str = $"Obj = {Type}\n\n"                               +
                            myUtils.strCountOf(list.Count, N)              +
                            $"opacityMode = {opacityMode}\n"               +
                            $"sizeMode = {sizeMode}\n"                     +
                            $"spdMax = {spdMax}\n"                         +
                            $"waitTimeMax = {waitTimeMax}\n"               +
                            $"sizeYfactor = {myUtils.fStr(sizeYfactor)}\n" +
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
            cnt = 10 + rand.Next(waitTimeMax);

            x = rand.Next(gl_Width);
            y = gl_Height + 100 + rand.Next(123);

            dx = 0;
            dy = myUtils.randFloatClamped(rand, 0.1f) * (rand.Next(spdMax) + 1) * -1.0f;

            switch (sizeMode)
            {
                case 0:
                    sizeX = 100;
                    break;

                case 1:
                    sizeX = 50 + rand.Next(100);
                    break;
            }

            sizeY = sizeX / sizeYfactor;

            yMax = gl_Height - rand.Next(333);
            yMin = rand.Next(333);

            A = 1;
            R = (float)rand.NextDouble();
            G = (float)rand.NextDouble();
            B = (float)rand.NextDouble();

            switch (opacityMode)
            {
                case 0:
                case 1:
                    A = 1;
                    break;

                case 2:
                    if (id < N / 2)
                        A = myUtils.randFloat(rand);
                    break;

                case 3:
                    A = myUtils.randFloatClamped(rand, 0.1f);
                    break;
            }

            colorPicker.getColor(rand.Next(gl_Width), rand.Next(gl_Height), ref R, ref G, ref B);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            bool doFlip = false;

            if (y < yMin && dy < 0)
            {
                doFlip = true;
            }

            if (y > yMax && dy > 0)
            {
                doFlip = true;
            }

            if (doFlip)
            {
                if (--cnt == 0)
                {
                    cnt = 10 + rand.Next(waitTimeMax);
                    dy *= -1;
                }
                else
                {
                    return;
                }
            }

            x += dx;
            y += dy;

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
            p4.Draw(x1, y1, x2, y2, x3, y3, x4, y4, true);
            p4.SetColor(0, 0, 0, 1);
            p4.Draw(x1, y1, x2, y2, x3, y3, x4, y4, false);

            p4.SetColor(R * 1.1f, G * 1.1f, B * 1.1f, A);
            p4.Draw(x1, y1, x3, y3, x1, gl_Height, x3, gl_Height, true);
            p4.SetColor(0, 0, 0, 1);
            p4.Draw(x1, y1, x3, y3, x1, gl_Height, x3, gl_Height, false);

            p4.SetColor(R * 0.9f, G * 0.9f, B * 0.9f, A);
            p4.Draw(x4, y4, x3, y3, x4, gl_Height, x3, gl_Height, true);
            p4.SetColor(0, 0, 0, 1);
            p4.Draw(x4, y4, x3, y3, x4, gl_Height, x3, gl_Height, false);

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
