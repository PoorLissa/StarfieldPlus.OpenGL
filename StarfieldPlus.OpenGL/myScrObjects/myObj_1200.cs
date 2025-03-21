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
        private float a, A, R, G, B;

        private static int N = 0, mode = 0, opacityMode = 0, widthMode = 0, mode2offset = 0;
        private static float dimAlpha = 0.05f, darkFactor = 1, xSpdFactor = 1;

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

                mode = rand.Next(3);
                widthMode = rand.Next(2);

                if (mode == 2)
                    N *= 2;
            }

            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time local initialization
        private void initLocal()
        {
            doClearBuffer = myUtils.randomBool(rand);
            doClearBuffer = true;

            opacityMode = rand.Next(3);
            mode2offset = 111 + rand.Next(666);

            darkFactor = 0.5f + 0.001f * rand.Next(250);
            xSpdFactor = 0.1f + myUtils.randFloat(rand) * 0.9f;

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            height = 600;

            string str = $"Obj = {Type}\n\n"                             +
                            myUtils.strCountOf(list.Count, N)            +
                            $"mode = {mode}\n"                           +
                            $"widthMode = {widthMode}\n"                 +
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
            colorPicker.getColor(x, y, ref R, ref G, ref B);

            switch (opacityMode)
            {
                case 0: A = 0.9f; break;
                case 1: A = 1.0f; break;
                case 2: A = myUtils.randomBool(rand) ? 0.9f : 1.0f; break;
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
            }

            if (x < 0 && dx < 0)
                dx *= -1;

            if (x > gl_Width && dx > 0)
                dx *= -1;

            if (dir && y < top && dy < 0)
                dy *= -1;

            if (dir && y > bottom && dy > 0)
                dy *= -1;

            if (!dir && y < top && dy < 0)
                dy *= -1;

            if (!dir && y > bottom && dy > 0)
                dy *= -1;

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Show()
        {
            bool doFill = !false;

            if (true)
            {
                switch (mode)
                {
                    case 0:
                        {
                            myPrimitive._Triangle.SetColor(R, G, B, a);
                            myPrimitive._Triangle.Draw(x, y, x - w1, gl_Height, x, gl_Height, doFill);

                            myPrimitive._Triangle.SetColor(R * darkFactor, G * darkFactor, B * darkFactor, a);
                            myPrimitive._Triangle.Draw(x, y, x + w2, gl_Height, x, gl_Height, doFill);

                            myPrimitive._Triangle.SetColor(0, 0, 0, a);
                            myPrimitive._Triangle.Draw(x, y, x - w1, gl_Height, x, gl_Height, false);
                            myPrimitive._Triangle.Draw(x, y, x + w2, gl_Height, x, gl_Height, false);
                        }
                        break;

                    case 1:
                        {
                            myPrimitive._Triangle.SetColor(R, G, B, a);
                            myPrimitive._Triangle.Draw(x, y, x - w1, 0, x, 0, doFill);

                            myPrimitive._Triangle.SetColor(R * darkFactor, G * darkFactor, B * darkFactor, a);
                            myPrimitive._Triangle.Draw(x, y, x + w2, 0, x, 0, doFill);

                            myPrimitive._Triangle.SetColor(0, 0, 0, a);
                            myPrimitive._Triangle.Draw(x, y, x - w1, 0, x, 0, false);
                            myPrimitive._Triangle.Draw(x, y, x + w2, 0, x, 0, false);
                        }
                        break;

                    case 2:
                        {
                            if (dir)
                            {
                                myPrimitive._Triangle.SetColor(R, G, B, a);
                                myPrimitive._Triangle.Draw(x, y, x - w1, gl_Height, x, gl_Height, doFill);

                                myPrimitive._Triangle.SetColor(R * darkFactor, G * darkFactor, B * darkFactor, a);
                                myPrimitive._Triangle.Draw(x, y, x + w2, gl_Height, x, gl_Height, doFill);

                                myPrimitive._Triangle.SetColor(0, 0, 0, a);
                                myPrimitive._Triangle.Draw(x, y, x - w1, gl_Height, x, gl_Height, false);
                                myPrimitive._Triangle.Draw(x, y, x + w2, gl_Height, x, gl_Height, false);
                            }
                            else
                            {
                                myPrimitive._Triangle.SetColor(R, G, B, a);
                                myPrimitive._Triangle.Draw(x, y, x - w1, 0, x, 0, doFill);

                                myPrimitive._Triangle.SetColor(R * darkFactor, G * darkFactor, B * darkFactor, a);
                                myPrimitive._Triangle.Draw(x, y, x + w2, 0, x, 0, doFill);

                                myPrimitive._Triangle.SetColor(0, 0, 0, a);
                                myPrimitive._Triangle.Draw(x, y, x - w1, 0, x, 0, false);
                                myPrimitive._Triangle.Draw(x, y, x + w2, 0, x, 0, false);
                            }
                        }
                        break;
                }

                return;
            }

            myPrimitive._Triangle.SetColor(R, G, B, A);
            myPrimitive._Triangle.Draw(0, 0, x, y, gl_Width, 0, doFill);

            float x1 = 0;
            float y1 = 0;
            float x2 = x;
            float y2 = y;
            float x3 = 0;
            float y3 = gl_Height;
            float x4 = x;
            float y4 = gl_Height;

            p4.SetColor(R * 0.75f, G * 0.75f, B * 0.75f, A);
            //p4.Draw(x1, y1, x2, y2, x3, y3, x4, y4, doFill);

            x1 = x;
            y1 = y;
            x2 = gl_Width;
            y2 = 0;
            x3 = x;
            y3 = gl_Height;
            x4 = gl_Width;
            y4 = gl_Height;

            p4.SetColor(R * 1.2f, G * 1.2f, B * 1.2f, A);
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
                        var obj = list[i] as myObj_1200;

                        obj.Show();
                        obj.Move();
                    }
                }

                if (Count < N)
                {
                    list.Add(new myObj_1200());
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
