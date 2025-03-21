using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;


/*
    - Pseudo 3d 'tooth like' pyramids
*/


namespace my
{
    public class myObj_1200 : myObject
    {
        // Priority
        public static int Priority => 10;
		public static System.Type Type => typeof(myObj_1200);

        private bool dir;
        private float x, y, dx, dy, w1, w2;
        private float a, A, R1, G1, B1, R2, G2, B2;

        private static int N = 0, mode = 0, colorMode = 0, opacityMode = 0, widthMode = 0, addMode = 0, mode2offset = 0;
        private static float dimAlpha = 0.05f, darkFactor = 1, xSpdFactor = 1;
        private static float sR = 0.1f, sG = 0.1f, sB = 0.1f;

        private static myScreenGradient grad = null;
        private static Polygon4 p4 = null;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_1200()
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

                mode = rand.Next(4);
                widthMode = rand.Next(2);
                addMode = rand.Next(2);

                if (mode == 2 || mode == 3)
                    N *= 2;
            }

            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time local initialization
        private void initLocal()
        {
            doClearBuffer = true;

            colorMode = rand.Next(2);
            opacityMode = rand.Next(4);
            mode2offset = 111 + rand.Next(666);

            darkFactor = 0.5f + 0.001f * rand.Next(250);
            xSpdFactor = 0.1f + myUtils.randFloat(rand) * 0.9f;

            sR = 0.1f;
            sG = 0.1f;
            sB = 0.1f;

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            height = 600;

            string str = $"Obj = {Type}\n\n"                             +
                            myUtils.strCountOf(list.Count, N)            +
                            $"mode = {mode}\n"                           +
                            $"addMode = {addMode}\n"                     +
                            $"widthMode = {widthMode}\n"                 +
                            $"colorMode = {colorMode}\n"                 +
                            $"opacityMode = {opacityMode}\n"             +
                            $"mode2offset = {mode2offset}\n"             +
                            $"xSpdFactor = {myUtils.fStr(xSpdFactor)}\n" +
                            $"darkFactor = {myUtils.fStr(darkFactor)}\n" +
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
            switch (mode)
            {
                case 0:
                    dir = true;
                    break;

                case 1:
                    dir = false;
                    break;

                case 2:
                case 3:
                    dir = myUtils.randomBool(rand);
                    break;
            }

            x = rand.Next(gl_Width);
            y = rand.Next(gl_Height);

            dx = myUtils.randFloat(rand, 0.25f) * myUtils.randomSign(rand) * xSpdFactor;
            dy = myUtils.randFloat(rand, 0.25f) * myUtils.randomSign(rand);

            w1 = 200 + rand.Next(666);
            w2 = 200 + rand.Next(666);

            if (widthMode == 0)
                w2 = w1;

            a = 0;

            switch (colorMode)
            {
                case 0:
                    colorPicker.getColor(x, y, ref R1, ref G1, ref B1);
                    break;

                case 1:
                    R1 = sR;
                    G1 = sG;
                    B1 = sB;

                    sR += myUtils.randomChance(rand, 1, 2) ? 0.9f / N : 0;
                    sG += myUtils.randomChance(rand, 1, 2) ? 0.9f / N : 0;
                    sB += myUtils.randomChance(rand, 1, 2) ? 0.9f / N : 0;
                    break;
            }

            R2 = R1 * darkFactor;
            G2 = G1 * darkFactor;
            B2 = B1 * darkFactor;

            switch (opacityMode)
            {
                case 0: A = 0.9f; break;
                case 1: A = 1.0f; break;
                case 2: A = myUtils.randomBool(rand) ? 0.9f : 1.0f; break;
                case 3: A = myUtils.randFloatClamped(rand, 0.25f); break;
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            x += dx;
            y += dy;
            a += a < A ? 0.001f : 0;

            int top = 0, bottom = 0;

            switch (mode)
            {
                case 0:
                case 1:
                    top = 333;
                    bottom = gl_Height - 333;
                    break;

                case 2:
                    {
                        if (dir)
                        {
                            top = gl_y0 - 111;
                            bottom = gl_Height - 111;
                        }
                        else
                        {
                            top = 111;
                            bottom = gl_y0 + 111;
                        }
                    }
                    break;

                case 3:
                    if (dir)
                    {
                        top = gl_y0 - gl_y0/2;
                        bottom = gl_y0;

                        top = gl_y0 - gl_y0/2;
                        bottom = gl_y0 + gl_y0/2;
                    }
                    else
                    {
                        top = gl_y0;
                        bottom = gl_y0 + gl_y0/2;

                        top = gl_y0 - gl_y0/2;
                        bottom = gl_y0 + gl_y0/2;
                    }
                    break;
            }

            if (x < 0 && dx < 0)
                dx *= -1;

            if (x > gl_Width && dx > 0)
                dx *= -1;

            if (dir)
            {
                if (y < top && dy < 0)
                    dy *= -1;

                if (y > bottom && dy > 0)
                    dy *= -1;
            }
            else
            {
                if (y < top && dy < 0)
                    dy *= -1;

                if (y > bottom && dy > 0)
                    dy *= -1;
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void drawTriangle(int bottom, bool doFill)
        {
            myPrimitive._Triangle.SetColor(R1, G1, B1, a);
            myPrimitive._Triangle.Draw(x, y, x - w1, bottom, x, bottom, doFill);

            myPrimitive._Triangle.SetColor(R2, G2, B2, a);
            myPrimitive._Triangle.Draw(x, y, x + w2, bottom, x, bottom, doFill);

            // Draw outline
            myPrimitive._Triangle.SetColor(0, 0, 0, a);
            myPrimitive._Triangle.Draw(x, y, x - w1, bottom, x, bottom, false);
            myPrimitive._Triangle.Draw(x, y, x + w2, bottom, x, bottom, false);
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void drawTriangleUpsideDown(int top, bool doFill)
        {
            myPrimitive._Triangle.SetColor(R1, G1, B1, a);
            myPrimitive._Triangle.Draw(x, y, x - w1, top, x, top, doFill);

            myPrimitive._Triangle.SetColor(R2, G2, B2, a);
            myPrimitive._Triangle.Draw(x, y, x + w2, top, x, top, doFill);

            // Draw outline
            myPrimitive._Triangle.SetColor(0, 0, 0, a);
            myPrimitive._Triangle.Draw(x, y, x - w1, top, x, top, false);
            myPrimitive._Triangle.Draw(x, y, x + w2, top, x, top, false);
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Show()
        {
            bool doFill = true;

            if (true)
            {
                switch (mode)
                {
                    case 0:
                        drawTriangle(gl_Height, doFill);
                        break;

                    case 1:
                        drawTriangleUpsideDown(0, doFill);
                        break;

                    case 2:
                        if (dir)
                        {
                            drawTriangle(gl_Height, doFill);
                        }
                        else
                        {
                            drawTriangleUpsideDown(0, doFill);
                        }
                        break;

                    case 3:
                        if (dir)
                        {
                            drawTriangle(gl_y0, doFill);
                        }
                        else
                        {
                            drawTriangleUpsideDown(gl_y0, doFill);
                        }
                        break;
                }

                return;
            }

            myPrimitive._Triangle.SetColor(R1, G1, B1, a);
            myPrimitive._Triangle.Draw(0, 0, x, y, gl_Width, 0, doFill);

            float x1 = 0;
            float y1 = 0;
            float x2 = x;
            float y2 = y;
            float x3 = 0;
            float y3 = gl_Height;
            float x4 = x;
            float y4 = gl_Height;

            p4.SetColor(R1 * 0.75f, G1 * 0.75f, B1 * 0.75f, A);
            //p4.Draw(x1, y1, x2, y2, x3, y3, x4, y4, doFill);

            x1 = x;
            y1 = y;
            x2 = gl_Width;
            y2 = 0;
            x3 = x;
            y3 = gl_Height;
            x4 = gl_Width;
            y4 = gl_Height;

            p4.SetColor(R1 * 1.2f, G1 * 1.2f, B1 * 1.2f, A);
            //p4.Draw(x1, y1, x2, y2, x3, y3, x4, y4, doFill);

/*
            x1 = x;
            y1 = y;
            x2 = 0;
            y2 = gl_Width;
            x3 = x;
            y3 = gl_Height;
            x4 = gl_Width;
            y4 = gl_Height;

            p4.SetColor(R * 1.2f, G * 1.2f, B * 1.2f, A);
            p4.Draw(x1, y1, x2, y2, x3, y3, x4, y4, doFill);
*/
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
                    glClear(GL_COLOR_BUFFER_BIT);
                    grad.Draw();
                }

                // Render Frame
                {
                    for (int i = 0; i != Count; i++)
                    {
                        var obj = list[i] as myObj_1200;

                        obj.Show();
                        obj.Move();
                    }
                }

                if (Count < N)
                {
                    bool doAdd = false;

                    switch (addMode)
                    {
                        case 0:
                            doAdd = true;
                            break;

                        case 1:
                            doAdd = myUtils.randomChance(rand, 1, 111);
                            break;
                    }

                    if (doAdd)
                    {
                        list.Add(new myObj_1200());
                    }
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

            myPrimitive.init_Triangle();
            p4 = new Polygon4();

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------
    }
};
