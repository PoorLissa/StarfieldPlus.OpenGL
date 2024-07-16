using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;


/*
    - Roaming lines, no buffer clearing
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
        private float A, r, g, b, dR, dG, dB, R, G, B;

        private static uint gl_cnt = 0;
        private static int N = 0, maxCnt = 1, mode, colorMode = 0, dimMode = 0, modeOld = 0, spd = 1;
        private static float dimAlpha = 0.005f;
        private static bool doUseGravity = false;

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

                spd = 5;
                maxCnt = 2000;
                mode = rand.Next(4);
                modeOld = rand.Next(2);
                colorMode = rand.Next(3);

                doUseGravity = false;

                switch (rand.Next(3))
                {
                    case 0:
                        dimMode = 0;
                        dimAlpha = 0.25f;
                        N = 100 + rand.Next(250);
                        spd = 2 + rand.Next(3);
                        break;

                    case 1:
                        dimMode = 1;
                        dimAlpha = 0.05f;
                        break;

                    case 2:
                        dimMode = 2;
                        dimAlpha = 0.001f;
                        break;
                }
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

            string str = $"Obj = {Type}\n\n"                         +
                            myUtils.strCountOf(list.Count, N)        +
                            $"mode = {mode}\n"                       +
                            $"dimMode = {dimMode}\n"                 +
                            $"colorMode = {colorMode}\n"             +
                            $"spd = {spd}\n"                         +
                            $"modeOld = {modeOld}\n"                 +
                            $"dimAlpha = {myUtils.fStr(dimAlpha)}\n" +
                            $"renderDelay = {renderDelay}\n"         +
                            $"gl_cnt = {gl_cnt}\n"                   +
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

            dx = (0.2f + myUtils.randFloat(rand)) * spd * myUtils.randomSign(rand);
            dy = (0.2f + myUtils.randFloat(rand)) * spd * myUtils.randomSign(rand);

            A = 0;

            switch (colorMode)
            {
                case 0:
                case 1:
                    {
                        r = R = (float)rand.NextDouble();
                        g = G = (float)rand.NextDouble();
                        b = B = (float)rand.NextDouble();
                    }
                    break;

                case 2:
                    {
                        if (id == 0)
                        {
                            do
                            {
                                r = R = (float)rand.NextDouble();
                                g = G = (float)rand.NextDouble();
                                b = B = (float)rand.NextDouble();
                            }
                            while (r + g + b < 0.5f);
                        }
                        else
                        {
                            r = R = (list[0] as myObj_1050).R + myUtils.randFloatSigned(rand) * 0.1f;
                            g = G = (list[0] as myObj_1050).G + myUtils.randFloatSigned(rand) * 0.1f;
                            b = B = (list[0] as myObj_1050).B + myUtils.randFloatSigned(rand) * 0.1f;
                        }
                    }
                    break;
            }

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

            if (A < 0.3)
            {
                A += 0.01f;
            }

            switch(modeOld)
            {
                case 0:
                    xOld = x;
                    yOld = y;
                    break;

                case 1:
                    xOld = x + rand.Next(7) - 3;
                    yOld = y + rand.Next(7) - 3;
                    break;
            }

            if (doUseGravity)
            {
                x += dx;
                y += dy;

                for (int i = 0; i < list.Count; i++)
                {
                    var obj = list[i] as myObj_1050;

                    if (obj.id != id)
                    {
                        float dX = obj.x - x;
                        float dY = obj.y - y;

                        double distSq = dX * dX + dY * dY + 0.01;

                        double factor = 33;

                        x += (float)(factor * dX / distSq);
                        y += (float)(factor * dY / distSq);
                    }
                }
            }
            else
            {
                x += dx;
                y += dy;

                dx += (float)Math.Sin(x + y);
                dy += (float)Math.Cos(y - x);
            }

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
                        float back = 0.04f * spd;

                        if (x < 0)
                            dx += back;

                        if (y < 0)
                            dy += back;

                        if (x > gl_Width)
                            dx -= back;

                        if (y > gl_Height)
                            dy -= back;
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

                case 3:
                    {
                        if (x < 0)
                            x = xOld = gl_Width;

                        if (y < 0)
                            y = yOld = gl_Height;

                        if (x > gl_Width)
                            x = xOld = 0;

                        if (y > gl_Height)
                            y = yOld = 0;
                    }
                    break;
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Show()
        {
#if !false
            myPrimitive._Line.SetColor(r, g, b, A);
            myPrimitive._Line.Draw(x, y, xOld, yOld, A);

            myPrimitive._Line.SetColor(r, g, b, A * 0.5f);
            myPrimitive._Line.Draw(x, y, xOld, yOld, 5);

            myPrimitive._Line.SetColor(r, g, b, A * 0.15f);
            myPrimitive._Line.Draw(x, y, xOld, yOld, 7);
#else
            myPrimitive._Line.SetColor(r, g, b, A);
            myPrimitive._Line.Draw(x, y, xOld, yOld, A);

            myPrimitive._Line.SetColor(r, g, b, A * 0.5f);
            myPrimitive._Line.Draw(x, y, xOld, yOld, 7);
#endif
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Process(Window window)
        {
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
                    dimScreen(dimAlpha);
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

                gl_cnt++;
                System.Threading.Thread.Sleep(renderDelay);

                // Dim faster to clear the screen
                if (dimMode == 2)
                {
                    if (gl_cnt > 5555)
                    {
                        dimAlpha += 0.0001f;

                        if (dimAlpha > 0.12f)
                        {
                            gl_cnt = 0;
                            dimAlpha = 0.001f;
                        }
                    }
                }
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void initShapes()
        {
            myPrimitive.init_Line();
            myPrimitive.init_ScrDimmer();

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
