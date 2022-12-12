using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;


/*
    - Splines
*/


namespace my
{
    public class myObj_132 : myObject
    {
        private static int N = 1;

        static int max_dSize = 0, t = 0, tDefault = 0, shape = 0, x0 = 0, y0 = 0, si1 = 0, si2 = 0, invalidateRate = 1;
        static bool isDimmableGlobal = true, isDimmableLocal = false, needNewScreen = false, doFillShapes = false;
        static float sf1 = 0, sf2 = 0, sf3 = 0, sf4 = 0, sf5 = 0, sf6 = 0, sf7 = 0, sf8 = 0, fLifeCnt = 0, fdLifeCnt = 0;

        private int maxSize = 0, R = 0, G = 0, B = 0, dSize = 0, dA = 0, dA_Filling = 0;
        private float x, y, dx, dy, size, a, r, g, b, angle = 0, time, time2, dt2, float_B, x1, y1, x2, y2, x3, y3, x4, y4;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_132()
        {
            generateNew();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time global initialization
        protected override void initGlobal()
        {
            colorPicker = new myColorPicker(gl_Width, gl_Height);
            list = new List<myObject>();

            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time initialization
        private void initLocal()
        {
            gl_x0 = gl_Width  / 2;
            gl_y0 = gl_Height / 2;

            doClearBuffer = false;

            N = (N == 0) ? 10 + rand.Next(10) : N;

            max_dSize = rand.Next(15) + 3;
            isDimmableGlobal = rand.Next(2) == 0;
            tDefault = 33;

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            height = 800;

            string str = $"Obj = myObj_132\n\n" +
                            $"N = {N} ({list.Count})\n" +
                            $""
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
            fLifeCnt = 255.0f;
            fdLifeCnt = 0.25f;

            x = rand.Next(gl_Width);
            y = rand.Next(gl_Height);

            float_B = 1.0f;

            colorPicker.getColor(x, y, ref r, ref g, ref b);

            maxSize = rand.Next(333) + 33;
            shape = rand.Next(91);
            isDimmableGlobal = rand.Next(2) == 0;
            isDimmableLocal = false;

            t = tDefault;
            t -= isDimmableGlobal ? 13 : 0;

#if false
            fdLifeCnt = 0.01f;
            shape = 1300;
            shape = 91;
            t = 3;
#endif

shape = 0;

            size = 1;
            dSize = rand.Next(max_dSize) + 1;
            dA = rand.Next(5) + 1;
            dA = 1;
            dA_Filling = rand.Next(5) + 2;

            time = 0.0f;
            time2 = 0.0f;
            dt2 = 0.01f;

            invalidateRate = 1;
            needNewScreen = true;

            setUpConstants();

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void setUpConstants()
        {
            switch (shape)
            {
                case 0:
                case 1:
                    fdLifeCnt = 0.5f;
                    break;
            }
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            int tNow = System.DateTime.Now.Millisecond;

            size += dSize;
            time += 0.1f;
            time2 += dt2;

            dx = (float)(Math.Sin(time)) * 5 * size / 10;
            dy = (float)(Math.Cos(time)) * 5 * size / 10;

            x += (int)dx;
            y += (int)dy;

            move_0();

            if ((fLifeCnt -= fdLifeCnt) < 0)
            {
                generateNew();
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void move_0()
        {
            float dx2 = (float)(Math.Cos(time)) * 10;
            float dy2 = (float)(Math.Sin(time)) * 10;

            //g.DrawLine(p, X, Y, Width/2, 10);
            //g.DrawLine(p, Width / 2, 10, Y, X);
            //g.DrawLine(p, X/2, Y/2, Size + dx, Size + dy);
            //g.DrawLine(p, X * dx2, Y * dy2, Size * dx, Size * dy);
            //g.DrawLine(p, X - Size, Y - Size, 2 * Size + dx, 2 * Size + dy);
            //g.DrawLine(p, X, Y, 2 * Size + dx, 2 * Size + dy);

            switch (shape)
            {
                case 0:
                    x1 = 2 * size;
                    y1 = 2 * size - dx;
                    x2 = size * dx;
                    y2 = size * dy;
                    break;

                case 1:
                    x1 = size + dx2;
                    y1 = size / 2 + dy2;
                    x2 = x / 2;
                    y2 = y / 2;
                    break;

                case 2:
                    x1 = size + dx2;
                    y1 = size + dy2;

                    x1 = size;
                    y1 = 1 * gl_Height / 5 + dy2 * dx / 100;

                    x2 = x / 2;
                    y2 = y / 2;
                    break;

                case 3:
                    x1 = size + dx2;
                    y1 = size + dy2;
                    x2 = gl_Width - size + dx2;
                    y2 = gl_Height - size + dy2;

                    x3 = gl_Width - size + dx2;
                    y3 = size + dy2;
                    x4 = size + dx2;
                    y4 = gl_Height - size + dy2;
                    break;

                case 4:
                    x1 = size + dx2 * dx / 10;
                    y1 = size + dy2;
                    x2 = gl_Width - size + dx2 * dx / 10;
                    y2 = gl_Height - size + dy2;

                    x3 = gl_Width - size + dx2 * dx / 10;
                    y3 = size + dy2;                        // * dy/10;
                    x4 = size + dx2 * dx / 10;
                    y4 = gl_Height - size + dy2;
                    break;

                case 5:
                    x1 = size + dx2;
                    y1 = size + dy2 * dy / 20;

                    x1 += float_B;

                    x2 = gl_Width - x1;
                    y2 = gl_Height - y1;

                    float_B += 1.123f;  // try changing this value
                    break;
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Show()
        {
            //p.Color = Color.FromArgb(100, R, G, B);

            switch (shape)
            {
                case 0:
                    myPrimitive._Line.SetColor(1, 1, 1, 0.1f);
                    myPrimitive._Line.Draw(x, y, x1, y1);
                    myPrimitive._Line.Draw(x, y, x2, y2);
                    break;

                case 1:
                case 2:
                    myPrimitive._Rectangle.SetColor(1.0f, 0.5f, 0.5f, 1.0f);
                    myPrimitive._Rectangle.Draw(x1, y1, 3, 3, false);
                    myPrimitive._Line.SetColor(1, 1, 1, 0.1f);
                    myPrimitive._Line.Draw(x2, y2, x1, y1);
                    break;

                case 3:
                case 4:
/*
                    g.DrawLine(p, x1, y1, x2, y2);
                    g.DrawLine(p, x3, y3, x4, y4);

                    g.DrawRectangle(Pens.DarkOrange, x1, y1, 3, 3);
                    g.DrawRectangle(Pens.DarkOrange, x2, y2, 3, 3);
                    g.DrawRectangle(Pens.DarkOrange, x3, y3, 3, 3);
                    g.DrawRectangle(Pens.DarkOrange, x4, y4, 3, 3);*/
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
                glDrawBuffer(GL_FRONT_AND_BACK);
            }

            list.Add(new myObj_132());

            while (!Glfw.WindowShouldClose(window))
            {
                cnt++;

                processInput(window);

                // Swap fore/back framebuffers, and poll for operating system events.
                Glfw.SwapBuffers(window);
                Glfw.PollEvents();

                if (doClearBuffer)
                {
                    glClear(GL_COLOR_BUFFER_BIT);
                }

                // Render Frame
                {
                    for (int i = 0; i < list.Count; i++)
                    {
                        var obj = list[i] as myObj_132;

                        obj.Show();
                        obj.Move();
                    }
                }

                System.Threading.Thread.Sleep(renderDelay);
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void initShapes()
        {
            myPrimitive.init_Line();
            myPrimitive.init_Rectangle();

            //base.initShapes(shape, N, 0);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------
    }
};
