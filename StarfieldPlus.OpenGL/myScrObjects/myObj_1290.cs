using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;


/*
    - Prysms
*/


namespace my
{
    public class myObj_1290 : myObject
    {
        // Priority
        public static int Priority => 10;
        public static System.Type Type => typeof(myObj_1290);

        private int yMax, yMin, cnt;
        private float x, y, dx, dy, height;
        private float sizeX, sizeY, A, R, G, B;

        private static bool doDrawBottom = false;
        private static int N = 0, spdMax = 1, waitTimeMax = 1;
        private static int mode = 0, opacityMode = 0, sizeMode = 0;
        private static float dimAlpha = 0.05f, sizeYfactor = 1;

        private static myScreenGradient grad = null;
        private static Polygon4 p4 = null;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_1290()
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
            mode = rand.Next(2);

            switch (mode)
            {
                case 0:
                    doClearBuffer = false;
                    break;

                case 1:
                    doClearBuffer = true;
                    break;
            }

            sizeMode = rand.Next(4);
            opacityMode = rand.Next(3);

            spdMax = 1 + rand.Next(5);
            waitTimeMax = rand.Next(150);
            sizeYfactor = 1.0f + myUtils.randFloat(rand) * (rand.Next(3) + 1);

            doDrawBottom = myUtils.randomBool(rand);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            height = 600;

            string str = $"Obj = {Type}\n\n"                                +
                            myUtils.strCountOf(list.Count, N)               +
                            $"mode = {mode}\n"                              +
                            $"opacityMode = {opacityMode}\n"                +
                            $"doDrawBottom = {doDrawBottom}\n"              +
                            $"sizeMode = {sizeMode}\n"                      +
                            $"spdMax = {spdMax}\n"                          +
                            $"waitTimeMax = {waitTimeMax}\n"                +
                            $"sizeYfactor = {myUtils.fStr(sizeYfactor)}\n"  +
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
            switch (sizeMode)
            {
                case 0:
                    sizeX = 33;
                    break;

                case 1:
                    sizeX = 50;
                    break;

                case 2:
                    sizeX = 33 + rand.Next(66);
                    break;

                // Linear speed in this mode will be proportionate to the size
                case 3:
                    sizeX = 11 + rand.Next(66);
                    break;
            }

            switch (mode)
            {
                // Static prysms
                case 0:
                    x = rand.Next(gl_Width);
                    y = rand.Next(gl_Height);

                    height = 33 + rand.Next(150);
                    cnt = 100 + rand.Next(100);
                    dx = dy = 0;

                    colorPicker.getColor(x, y, ref R, ref G, ref B);
                    break;

                case 1:
                    x = rand.Next(gl_Width);
                    y = gl_Height + 50 + rand.Next(100);
                    dx = 0;
                    dy = -1.0f * myUtils.randFloatClamped(rand, 0.1f) * (1 + rand.Next(spdMax));
                    height = sizeX + rand.Next(250);

                    if (sizeMode == 3)
                    {
                        dy = -0.5f * myUtils.randFloatClamped(rand, 0.1f) * sizeX;
                    }

                    colorPicker.getColor(x, rand.Next(gl_Height), ref R, ref G, ref B);
                    break;
            }

            sizeY = sizeX / sizeYfactor;

            yMax = gl_Height - rand.Next(333);
            yMin = rand.Next(333);

            switch (opacityMode)
            {
                case 0:
                    A = 1.0f;
                    break;

                case 1:
                    A = 0.5f;
                    break;

                case 2:
                    A = myUtils.randFloatClamped(rand, 0.05f);
                    break;
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            x += dx;
            y += dy;

            switch (mode)
            {
                case 0:
                    {
                        if (--cnt == 0)
                            generateNew();
                    }
                    break;

                case 1:
                    {
                        if (y + height < 0)
                            generateNew();
                    }
                    break;
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

            int bottomOffset = 0;

            p4.SetColor(R, G, B, A);
            p4.Draw(x1, y1, x2, y2, x3, y3, x4, y4, true);
            p4.SetColor(0, 0, 0, 1);
            p4.Draw(x1, y1, x2, y2, x3, y3, x4, y4, false);

            if (doDrawBottom)
            {
                p4.SetColor(0, 0, 0, 0.1f);
                p4.Draw(x1, y1 + height, x2, y2 + height, x3, y3 + height, x4, y4 + height, !false);
            }

            p4.SetColor(R * 1.1f, G * 1.1f, B * 1.1f, A);
            p4.Draw(x1, y1, x3, y3, x1 + bottomOffset, y1 + height, x3, y3 + height, true);
            p4.SetColor(0, 0, 0, 1);
            p4.Draw(x1, y1, x3, y3, x1 + bottomOffset, y1 + height, x3, y3 + height, false);

            p4.SetColor(R * 0.9f, G * 0.9f, B * 0.9f, A);
            p4.Draw(x4, y4, x3, y3, x4 - bottomOffset, y4 + height, x3, y3 + height, true);
            p4.SetColor(0, 0, 0, 1);
            p4.Draw(x4, y4, x3, y3, x4 - bottomOffset, y4 + height, x3, y3 + height, false);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Process(Window window)
        {
            uint cnt = 0;
            initShapes();

            clearScreenSetup(doClearBuffer, 0.1f);

            // Gradient
            {
                grad.SetOpacity(1);
                grad.Draw();
                Glfw.SwapBuffers(window);
                grad.Draw();
                grad.SetOpacity(doClearBuffer ? 1 : 0.1f);
            }

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
                        glClear(GL_COLOR_BUFFER_BIT);

                    grad.Draw();
                }

                // Render Frame
                {
                    for (int i = 0; i != Count; i++)
                    {
                        var obj = list[i] as myObj_1290;

                        obj.Show();
                        obj.Move();
                    }
                }

                if (Count < N)
                {
                    list.Add(new myObj_1290());
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
