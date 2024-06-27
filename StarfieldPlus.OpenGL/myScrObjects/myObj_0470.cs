using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;


/*
    - angled rays
*/


namespace my
{
    public class myObj_0470 : myObject
    {
        // Priority
        public static int Priority => 10;
        public static System.Type Type => typeof(myObj_0470);

        private float x1, y1, dx1, dy1;
        private float x2, y2, dx2, dy2;
        private float A, R, G, B;
        private float lineTh;
        private int cnt, dir;

        private static int N = 0, dirMode = 0, drawMode = 0, di = 0;
        private static float dimAlpha = 0.05f, angleFactor = 1, dA;
        private static bool doUseRandSpeed = true;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_0470()
        {
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
                switch (rand.Next(5))
                {
                    case 0:
                        N = rand.Next(100) + 10000;
                        dA = 0;
                        break;

                    default:
                        N = rand.Next(100) + 100;
                        dA = -0.0075f;
                        break;
                }
            }

            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time local initialization
        private void initLocal()
        {
            doClearBuffer  = myUtils.randomChance(rand, 2, 3);
            doUseRandSpeed = myUtils.randomChance(rand, 1, 3);

            dirMode  = rand.Next(13);
            drawMode = N < 1000 ? rand.Next(2) : 0;

            // AngleFactor of 1 will result in parallel lines
            switch (rand.Next(7))
            {
                case 0:
                    angleFactor = 1;
                    break;

                default:
                    angleFactor = rand.Next(10) + 1.0f + myUtils.randFloat(rand);
                    break;
            }

            di = rand.Next(10) + 1;                 // In drawMode1, height step
            renderDelay = rand.Next(10);
            dimAlpha = 0.2f + myUtils.randFloat(rand) * 0.25f;

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            height = 600;

            string str = $"Obj = {Type}\n\n"                      	 +
                            myUtils.strCountOf(list.Count, N)        +
                            $"doClearBuffer = {doClearBuffer}\n"     +
                            $"doUseRandSpeed = {doUseRandSpeed}\n"   +
                            $"dirMode = {dirMode}\n"                 +
                            $"drawMode = {drawMode}\n"               +
                            $"di = {di}\n"                           +
                            $"renderDelay = {renderDelay}\n"         +
                            $"dimAlpha = {myUtils.fStr(dimAlpha)}\n" +
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
            cnt = rand.Next(100) + 13;

            switch (dirMode)
            {
                case 0:
                case 1:
                    dir = rand.Next(2);
                    break;

                case 2:
                case 3:
                    dir = rand.Next(2) + 2;
                    break;

                case 4:
                    dir = myUtils.randomBool(rand) ? 0 : 2;
                    break;

                case 5:
                    dir = myUtils.randomBool(rand) ? 1 : 3;
                    break;

                default:
                    dir = rand.Next(4);
                    break;
            }

            // dy
            float DY = myUtils.randFloat(rand, 0.1f) * 5;

            if (doUseRandSpeed == false)
            {
                DY = 3;
            }

            switch (dir)
            {
                case 0:
                    {
                        x1 = -10;
                        x2 = gl_Width + 10;
                        y1 = y2 = gl_Height + 3;

                        dx1 = dx2 = 0;
                        dy1 = -DY;
                        dy2 = dy1 * angleFactor;
                    }
                    break;

                case 1:
                    {
                        x1 = -10;
                        x2 = gl_Width + 10;
                        y1 = y2 = gl_Height + 3;

                        dx1 = dx2 = 0;
                        dy2 = -DY;
                        dy1 = dy2 * angleFactor;
                    }
                    break;

                case 2:
                    {
                        x1 = -10;
                        x2 = gl_Width + 10;
                        y1 = y2 = -3;

                        dx1 = dx2 = 0;
                        dy2 = DY;
                        dy1 = dy2 * angleFactor;
                    }
                    break;

                case 3:
                    {
                        x1 = -10;
                        x2 = gl_Width + 10;
                        y1 = y2 = -3;

                        dx1 = dx2 = 0;
                        dy1 = DY;
                        dy2 = dy1 * angleFactor;
                    }
                    break;
            }

            lineTh = rand.Next(3) + myUtils.randFloat(rand);

            colorPicker.getColorRand(ref R, ref G, ref B);

            A = N < 1000 ? myUtils.randFloat(rand, 0.1f) : 0.1f;

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            if (--cnt < 0)
            {
                x1 += dx1;
                y1 += dy1;
                x2 += dx2;
                y2 += dy2;

                A += dA;

                if (true)
                {
                    dy1 *= 1.001f;
                    dy2 *= 1.001f;
                }

                if (A < 0)
                {
                    generateNew();
                }
                else
                {
                    switch (dir)
                    {
                        case 0:
                        case 1:
                            if (y1 < 0 && y2 < 0)
                                generateNew();
                            break;

                        case 2:
                        case 3:
                            if (y1 > gl_Height && y2 > gl_Height)
                                generateNew();
                            break;
                    }
                }
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Show()
        {
            switch (drawMode)
            {
                case 0:
                    {
                        myPrimitive._Line.SetColor(R, G, B, A / 4);
                        myPrimitive._Line.Draw(x1, y1, x2, y2, lineTh * 5);

                        myPrimitive._Line.SetColor(R, G, B, A / 2);
                        myPrimitive._Line.Draw(x1, y1, x2, y2, lineTh * 3);

                        myPrimitive._Line.SetColor(R, G, B, A);
                        myPrimitive._Line.Draw(x1, y1, x2, y2, lineTh);
                    }
                    break;

                case 1:
                    {
                        myPrimitive._Line.SetColor(R, G, B, A);

                        for (int i = 0; i < 30; i += di)
                        {
                            myPrimitive._Line.Draw(x1, y1 + i, x2, y2 + i, lineTh);
                        }
                    }
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

            if (doClearBuffer)
            {
                glDrawBuffer(GL_FRONT_AND_BACK | GL_DEPTH_BUFFER_BIT);
                glClearColor(0, 0, 0, 1);
            }
            else
            {
                dimScreenRGB_SetRandom(0.1f);
                glDrawBuffer(GL_BACK);
            }

            while (!Glfw.WindowShouldClose(window))
            {
                processInput(window);

                // Swap fore/back framebuffers, and poll for operating system events.
                Glfw.SwapBuffers(window);
                Glfw.PollEvents();

                // Dim screen
                {
                    if (doClearBuffer)
                    {
                        glClear(GL_COLOR_BUFFER_BIT);
                    }
                    else
                    {
                        dimScreen(dimAlpha);
                    }
                }

                // Render Frame
                {
                    for (int i = 0; i != list.Count; i++)
                    {
                        var obj = list[i] as myObj_0470;

                        obj.Show();
                        obj.Move();
                    }
                }

                if (list.Count < N)
                {
                    list.Add(new myObj_0470());
                }

                cnt++;
                System.Threading.Thread.Sleep(renderDelay);
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void initShapes()
        {
            myPrimitive.init_ScrDimmer();
            myPrimitive.init_Line();

            glEnable(GL_LINE_SMOOTH);
            glEnable(GL_POLYGON_SMOOTH);
            glHint(GL_LINE_SMOOTH_HINT, GL_NICEST);
            glHint(GL_POLYGON_SMOOTH_HINT, GL_NICEST);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------
    }
};
