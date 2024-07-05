﻿using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;


/*
    - 
*/


namespace my
{
    public class myObj_1050 : myObject
    {
        // Priority
        public static int Priority => 9999910;
		public static System.Type Type => typeof(myObj_1050);

        private int cnt;
        private float x, y, dx, dy, xOld, yOld;
        private float size, A, r, g, b, dR, dG, dB, R, G, B, angle = 0;

        private static int N = 0, shape = 0, maxCnt = 1, mode;
        private static bool doFillShapes = false;
        private static float dimAlpha = 0.005f;

        private static myScreenGradient grad = null;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_1050()
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
                N = 33;

                shape = rand.Next(5);

                maxCnt = 2000;

                mode = rand.Next(3);

                dimAlpha = 0.25f;
                dimAlpha = 0.05f;
            }

            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time local initialization
        private void initLocal()
        {
            doClearBuffer = false;

            renderDelay = rand.Next(3) + 1;

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            height = 600;

            string str = $"Obj = {Type}\n\n"                  +
                            myUtils.strCountOf(list.Count, N) +
                            $"renderDelay = {renderDelay}\n"  +
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
            cnt = 333 + rand.Next(maxCnt);

            x = rand.Next(gl_Width);
            y = rand.Next(gl_Height);

            int spd = 5;

            dx = (0.2f + myUtils.randFloat(rand)) * spd * myUtils.randomSign(rand);
            dy = (0.2f + myUtils.randFloat(rand)) * spd * myUtils.randomSign(rand);

            size = rand.Next(3) + 2;

            A = 0;
            r = R = (float)rand.NextDouble();
            g = G = (float)rand.NextDouble();
            b = B = (float)rand.NextDouble();

            //colorPicker.getColor(x, y, ref R, ref G, ref B);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            if (--cnt == 0)
            {
                cnt = -1;

                R = (float)rand.NextDouble();
                G = (float)rand.NextDouble();
                B = (float)rand.NextDouble();

                dR = (R - r) / 100;
                dG = (G - g) / 100;
                dB = (B - b) / 100;
            }

            if (cnt < 0)
            {
                r += dR;
                g += dG;
                b += dB;

                if (dR > 0 && r >= R)
                    dR = 0;

                if (dR < 0 && r <= R)
                    dR = 0;

                if (dG > 0 && g >= G)
                    dG = 0;

                if (dG < 0 && g <= G)
                    dG = 0;

                if (dB > 0 && b >= B)
                    dB = 0;

                if (dB < 0 && b <= B)
                    dB = 0;

                if (dR == 0 && dG == 0 && dB == 0)
                    cnt = 333 + rand.Next(maxCnt);
            }

            if (A < 1)
            {
                A += 0.01f;
            }

            angle += 0.001f;

            xOld = x;
            yOld = y;

            x += dx;
            y += dy;

            dx += (float)Math.Sin(x + y);
            dy += (float)Math.Cos(y - x);

            switch (mode)
            {
                case 0:
                    {
                        if (x < 0 && dx < 0)
                            dx *= -1;

                        if (y < 0 && dy < 0)
                            dy *= -1;

                        if (x > gl_Width && dx > 0)
                            dx *= -1;

                        if (y > gl_Height && dy > 0)
                            dy *= -1;
                    }
                    break;

                case 1:
                    {
                        float back = 0.095f;

                        if (x < 0)
                            dx += back;

                        if (y < 0)
                            dy += back;

                        if (x > gl_Width)
                            dx -= back;

                        if (y > gl_Height)
                            dx -= back;
                    }
                    break;

                case 2:
                    {
                        if (x < 0 || x > gl_Width || y < 0 || y > gl_Height)
                        {
                            generateNew();
                        }
                    }
                    break;
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Show()
        {
            float size2x = size * 2;

            myPrimitive._Line.SetColor(r, g, b, A);
            myPrimitive._Line.Draw(x, y, xOld, yOld, A);

            myPrimitive._Line.SetColor(r, g, b, A * 0.5f);
            myPrimitive._Line.Draw(x, y, xOld, yOld, 7);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Process(Window window)
        {
            uint cnt = 0;
            initShapes();

            clearScreenSetup(doClearBuffer, 0.1f, true);

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
                        var obj = list[i] as myObj_1050;

                        obj.Move();
                        obj.Show();
                    }
                }

                if (Count < N)
                {
                    list.Add(new myObj_1050());
                }

                cnt++;
                System.Threading.Thread.Sleep(renderDelay);
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void initShapes()
        {
            myPrimitive.init_Line();

            myPrimitive.init_ScrDimmer();
            base.initShapes(shape, N, 0);

            grad = new myScreenGradient();
            grad.SetRandomColors(rand, 0.2f);

            glEnable(GL_LINE_SMOOTH);
            glEnable(GL_POLYGON_SMOOTH);
            glHint(GL_LINE_SMOOTH_HINT, GL_NICEST);
            glHint(GL_POLYGON_SMOOTH_HINT, GL_NICEST);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------
    }
};