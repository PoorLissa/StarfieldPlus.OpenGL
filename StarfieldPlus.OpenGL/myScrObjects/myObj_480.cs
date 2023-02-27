using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;


/*
    - ...
*/


namespace my
{
    public class myObj_480 : myObject
    {
        private float x, y, oldx, oldy, dx, dy;
        private float size, A, R, G, B;

        private static int N = 0, dtCount = 0, colorMode = 0, moveMode = 0;
        private static bool doUseDdt = true, doUseNoise = true;
        private static float dimAlpha = 0.05f, t = 0, dt = 0, ddt = 0, lineTh = 1;

        private static int[] prm_i = new int[5];

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_480()
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
                N = 1;
            }

            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time local initialization
        private void initLocal()
        {
            doClearBuffer = myUtils.randomBool(rand);
            doUseNoise = myUtils.randomChance(rand, 1, 3);      // Add some random noise to the final signal
            doUseDdt = myUtils.randomBool(rand);                // true: dt is going to change over time

            moveMode = rand.Next(9);
            colorMode = rand.Next(2);

            dt = myUtils.randomSign(rand) * myUtils.randFloat(rand, 0.1f) * 0.1f;

            dimAlpha = 0.1f + myUtils.randFloat(rand) * 0.2f;

            lineTh = 0.25f + myUtils.randFloat(rand) * rand.Next(10);

            switch (moveMode)
            {
                case 004:
                    prm_i[0] = rand.Next(100) + 3;
                    break;

                case 005:
                    break;

                case 006:
                    prm_i[0] = rand.Next(13) + 2;
                    prm_i[1] = rand.Next(33) + 1;
                    prm_i[2] = rand.Next(10) + 1;
                    break;
            }

            renderDelay = rand.Next(11) + 3;

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            height = 600;

            string nStr(int   n) { return n.ToString("N0");    }
            string fStr(float f) { return f.ToString("0.000"); }

            string str = $"Obj = myObj_480\n\n"                       +
                            $"N = {nStr(list.Count)} of {nStr(N)}\n"  +
                            $"moveMode = {moveMode}\n"                +
                            $"colorMode = {colorMode}\n"              +
                            $"doUseDdt = {doUseDdt}\n"                +
                            $"doUseNoise = {doUseNoise}\n"            +
                            $"lineTh = {fStr(lineTh)}\n"              +
                            $"dimAlpha = {fStr(dimAlpha)}\n"          +
                            $"renderDelay = {renderDelay}\n"          +
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
            x = 0;
            y = gl_y0;

            dx = 5;
            dy = 0;

            size = rand.Next(333) + 111;

            switch (colorMode)
            {
                case 0:
                    colorPicker.getColorRand(ref R, ref G, ref B);
                    break;

                case 1:
                    do
                    {
                        R = myUtils.randFloat(rand);
                        G = myUtils.randFloat(rand);
                        B = myUtils.randFloat(rand);
                    }
                    while (R + G + B < 0.33f);
                    break;
            }

            A = 1;

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            t += dt;

            if (doUseDdt)
            {
                if (dtCount == 0)
                {
                    ddt = 0;

                    if (myUtils.randomChance(rand, 1, 33))
                    {
                        if (myUtils.randomChance(rand, 1, 3))
                        {
                            ddt = myUtils.randomSign(rand) * myUtils.randFloat(rand) * 0.001f;
                            dtCount = rand.Next(100);
                        }
                    }
                }
                else
                {
                    dtCount--;
                    dt += ddt;
                }
            }
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Show()
        {
            float xFactor = 0.01f;

            x = 0;
            y = gl_y0 + (float)Math.Sin(x * xFactor + t) * size;

            myPrimitive._LineInst.ResetBuffer();
            myPrimitive._RectangleInst.ResetBuffer();

            while (x <= gl_Width)
            {
                oldx = x;
                oldy = y;

                x += dx;
                y = (float)Math.Sin(x * xFactor + t);

                float Y = y;

                switch (moveMode)
                {
                    case 000:
                        y += myUtils.randFloat(rand) * 0.5f;
                        break;

                    case 001:
                        y += (float)Math.Cos(y * t);
                        break;

                    case 002:
                        y += (float)Math.Cos(y * y);
                        y += (float)Math.Cos(y * t);
                        break;

                    case 003:
                        y += (float)Math.Cos(x / 2 * xFactor + t);
                        break;

                    case 004:
                        y += (float)Math.Cos(Math.Cos(x/prm_i[0]));
                        break;

                    case 005:
                        y += (int)(Math.Cos(x * xFactor + t/2) * 3);
                        break;

                    case 006:
                        y += (int)(Math.Cos(x * xFactor / prm_i[2] + t / prm_i[1]) * prm_i[0]) / (prm_i[0] / 2);
                        break;

                    case 007:
                        y += (float)(Math.Sin(Y * Y * Y * t/10) * 50) / 49;
                        break;

                    case 008:
                        y += (int)(Math.Sin(Y * Y * Y * t / 10) * 50) / 49;
                        break;
                }

                if (doUseNoise)
                {
                    y += myUtils.randFloat(rand) * 0.1f;
                }

                y = gl_y0 + y * size;

                myPrimitive._LineInst.setInstanceCoords(x, y, oldx, oldy);
                myPrimitive._LineInst.setInstanceColor(R, G, B, A/2);

                myPrimitive._RectangleInst.setInstanceCoords(x - 3, y - 3, 6, 6);
                myPrimitive._RectangleInst.setInstanceColor(R, G, B, A);
                myPrimitive._RectangleInst.setInstanceAngle(0);
            }

            myPrimitive._LineInst.Draw();
            myPrimitive._RectangleInst.Draw();

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
                glDrawBuffer(GL_FRONT_AND_BACK);
                //glDrawBuffer(GL_DEPTH_BUFFER_BIT);
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
                    glLineWidth(lineTh);

                    for (int i = 0; i != list.Count; i++)
                    {
                        var obj = list[i] as myObj_480;

                        obj.Show();
                        obj.Move();
                    }
                }

                if (list.Count < N)
                {
                    list.Add(new myObj_480());
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

            myPrimitive.init_LineInst(gl_Width);

            base.initShapes(0, gl_Width, 0);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------
    }
};
