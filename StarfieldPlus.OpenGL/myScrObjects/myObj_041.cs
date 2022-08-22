using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;


/*
    - Branches/snakes moving inwards/outwards with different set of rules
*/


namespace my
{
    public class myObj_041 : myObject
    {
        private int dxi, dyi, oldX, oldY;
        private float dxf = 0, dyf = 0, x = 0, y = 0, time = 0, size = 0, A = 0, dA = 0, angle, dAngle;

        static float dimAlpha = 0.0f, R = 1, G = 1, B = 1;
        static int N = 0, x0 = 0, y0 = 0, moveMode = -1, shape = -1, speedMode = -1, t = -1, fillMode = 0, lineMode = 0, maxRnd = 0;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_041()
        {
            if (colorPicker == null)
            {
                colorPicker = new myColorPicker(gl_Width, gl_Height);
                list = new List<myObject>();

                init();
            }

            generateNew();

            time = 0;
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time initialization
        private void init()
        {
            gl_x0 = gl_Width / 2;
            gl_y0 = gl_Height / 2;

            N = (N == 0) ? 1111 + rand.Next(333) : N;
            renderDelay = 10;

            maxRnd = rand.Next(20) + 1;

            shape = rand.Next(5);
            lineMode = rand.Next(5);
            moveMode = rand.Next(19);
            speedMode = rand.Next(2);
            fillMode = rand.Next(3);
            t = rand.Next(15) + 1;

            x0 = gl_Width / 2;
            y0 = gl_Height / 2;

            x0 += rand.Next(gl_Width) - x0;

            switch (rand.Next(3))
            {
                case 0:
                    dimAlpha = 0.001f * (rand.Next(100) + 1);
                    break;

                case 1:
                    dimAlpha = 0.001f * (rand.Next(66) + 1);
                    break;

                case 2:
                    dimAlpha = 0.001f * (rand.Next(33) + 1);
                    break;
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void setNextMode()
        {
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            return $"Obj = myObj_041\n\n" +
                            $"N = {N}\n" +
                            $"moveMode = {moveMode}\n" +
                            $"dimAlpha = {dimAlpha}\n" +
                            $"fillMode = {fillMode}\n" +
                            $"lineMode = {lineMode}\n" +
                            $"maxRnd = {maxRnd}\n"
                            ;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void generateNew()
        {
            int speed = 5;

            if (speedMode == 0)
            {
                speed = 3 + rand.Next(5);
            }

            dxi = 0;
            dyi = 0;
            dxf = 0;
            dyf = 0;

            do
            {
                x = rand.Next(gl_Width);
                y = rand.Next(gl_Height);

                double dist = Math.Sqrt((x - x0) * (x - x0) + (y - y0) * (y - y0));

                dxi = (int)((x - x0) * speed / dist);
                dyi = (int)((y - y0) * speed / dist);

                // floats
                {
                    dxf = (float)((x - x0) * speed / dist);
                    dyf = (float)((y - y0) * speed / dist);
                }
            }
            while (dxi == 0 && dyi == 0);

            oldX = (int)x;
            oldY = (int)y;

            A = 0;
            dA = 0.0001f * (rand.Next(100) + 1);

            size = rand.Next(5) + 1;
            time = 0.001f * rand.Next(1111);

            angle = (float)rand.NextDouble();
            dAngle = 0.0001f * rand.Next(333);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            oldX = (int)x;
            oldY = (int)y;

            switch (moveMode)
            {
                case 0:
                    x += dxf * 2 + (float)(Math.Sin(y) * 5);
                    y += dyf * 2 + (float)(Math.Sin(x) * 5);
                    break;

                case 1:
                    x += dxf;
                    y += dyf;
                    break;

                case 2:
                    x += dxf;
                    y += dyf;
                    x = (int)x;
                    y = (int)y;
                    break;

                case 3:
                    time += (float)(rand.Next(999) / 1000.0f);

                    x = (int)(x + dxf + (float)(Math.Sin(time) * 1));
                    y = (int)(y + dyf + (float)(Math.Cos(time) * 1));
                    break;

                case 4:
                    time += 0.1f;

                    x = (int)(x + dxf + (float)(Math.Sin(time) * 1));
                    y = (int)(y + dyf + (float)(Math.Cos(time) * 1));
                    break;

                case 5:
                    time += 0.01f;

                    x = (int)(x + dxf + (float)(Math.Sin(time) * 1));
                    y = (int)(y + dyf + (float)(Math.Cos(time) * 1));
                    break;

                case 6:
                    time += 0.1f;

                    x = (int)(x + dxf + (float)(Math.Sin(time * dxf) * 2));
                    y = (int)(y + dyf + (float)(Math.Cos(time * dyf) * 2));
                    break;

                case 7:
                    time += 0.01f;

                    x = (int)(x + dxf + (float)(Math.Sin(time * dxf) * 2));
                    y = (int)(y + dyf + (float)(Math.Sin(time * dyf) * 2));
                    break;

                case 8:
                    time += 0.01f;

                    x = (int)(x + dxf + (float)(Math.Sin(time * dxf) * 3));
                    y = (int)(y + dyf + (float)(Math.Sin(time * dyf) * 3));
                    break;

                case 9:
                    time += 0.01f;

                    x = (int)(x + dxf + (float)(Math.Sin(time * dyf) * 2));
                    y = (int)(y + dyf + (float)(Math.Sin(time * dxf) * 2));
                    break;

                case 10:
                    // need low alpha
                    time += 0.01f;

                    x = (int)(x - dxf + (float)(Math.Sin(time * 1) * 1));
                    y = (int)(y - dyf + (float)(Math.Sin(time * 2) * 1));
                    break;

                case 11:
                    // need low alpha
                    time += 0.1f;

                    x = (int)(x - dxf + (float)(Math.Sin(time * 1) * 1));
                    y = (int)(y - dyf + (float)(Math.Sin(time * 2) * 1));
                    break;

                case 12:
                    time += 0.01f;

                    x += dxf + (float)(Math.Sin(x) * time);
                    y += dyf + (float)(Math.Cos(y) * time);
                    break;

                case 13:
                    time += 0.01f;

                    x += dxf + (float)(Math.Sin(time * x));
                    y += dyf + (float)(Math.Cos(time * y));
                    break;

                case 14:
                    time += 0.01f;

                    x += dxf + (float)(Math.Sin(time));
                    y += dyf + (float)(Math.Cos(time));
                    break;

                case 15:
                    time += (float)(rand.NextDouble() / (rand.Next(100) + 1));

                    x += dxf + (float)(Math.Sin(time));
                    y += dyf + (float)(Math.Cos(time));
                    break;

                case 16:
                    time += (float)(rand.NextDouble() / (rand.Next(100) + 1));

                    x += dxf + (float)(Math.Sin(time));
                    y -= dyf + (float)(Math.Cos(time));
                    break;

                case 17:
                    time += 0.01f;

                    x += dxf + (float)(Math.Sin(time) * Math.Cos(x));
                    y += dyf + (float)(Math.Cos(time) * Math.Sin(y));
                    break;

                case 18:
                    time += 0.01f;

                    x += dxf + (float)Math.Sin(Math.Cos(x) + Math.Cos(y) * 13) * rand.Next(maxRnd);
                    y += dyf + (float)Math.Cos(Math.Sin(y) + Math.Sin(x) * 13) * rand.Next(maxRnd);
                    break;

                // --- Research mode, don't use ---
                case 99:
                    time += 0.01f;

                    x += dxf + (float)Math.Sin(Math.Cos(x) + Math.Cos(y) * 13) * rand.Next(maxRnd);
                    y += dyf + (float)Math.Cos(Math.Sin(y) + Math.Sin(x) * 13) * rand.Next(maxRnd);
                    break;
            }

            A += dA;
            angle += dAngle;

            if (x < 0 || x > gl_Width || y < 0 || y > gl_Height || A < 0)
            {
                generateNew();
            }

            if (A > 3.0f && dA > 0)
            {
                dA *= -1.25f;
            }

            if (dimAlpha > 0.3f && rand.Next(33) == 0)
            {
                x0 += rand.Next(11) - 5;
                y0 += rand.Next(11) - 5;
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Show()
        {
            switch (shape)
            {
                case 0:
                    var rectInst = inst as myRectangleInst;

                    rectInst.setInstanceCoords(x - size, y - size, 2 * size, 2 * size);
                    rectInst.setInstanceColor(R, G, B, A);
                    rectInst.setInstanceAngle(angle);
                    break;

                case 1:
                    var triangleInst = inst as myTriangleInst;

                    triangleInst.setInstanceCoords(x, y, size, angle);
                    triangleInst.setInstanceColor(R, G, B, A);
                    break;

                case 2:
                    var ellipseInst = inst as myEllipseInst;

                    ellipseInst.setInstanceCoords(x, y, 2 * size, angle);
                    ellipseInst.setInstanceColor(R, G, B, A);
                    break;

                case 3:
                    var pentagonInst = inst as myPentagonInst;

                    pentagonInst.setInstanceCoords(x, y, 2 * size, angle);
                    pentagonInst.setInstanceColor(R, G, B, A);
                    break;

                case 4:
                    var hexagonInst = inst as myHexagonInst;

                    hexagonInst.setInstanceCoords(x, y, 2 * size, angle);
                    hexagonInst.setInstanceColor(R, G, B, A);
                    break;
            }

            if (lineMode > 0)
            {
                myPrimitive._LineInst.setInstanceCoords(x, y, oldX, oldY);

                switch (lineMode)
                {
                    case 1:
                    case 2:
                        myPrimitive._LineInst.setInstanceColor(1, 1, 1, A);
                        break;

                    case 3:
                    case 4:
                        myPrimitive._LineInst.setInstanceColor(1, 1, 1, 1);
                        break;
                }
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Process(Window window)
        {
            initShapes();

            glDrawBuffer(GL_FRONT_AND_BACK);

            while (!Glfw.WindowShouldClose(window))
            {
                processInput(window);

                // Swap fore/back framebuffers, and poll for operating system events.
                Glfw.SwapBuffers(window);
                Glfw.PollEvents();

                dimScreen(useStrongerDimFactor: dimAlpha < 0.05f);

                inst.ResetBuffer();
                myPrimitive._LineInst.ResetBuffer();

                for (int i = 0; i < list.Count; i++)
                {
                    var obj = list[i] as myObj_041;

                    obj.Show();
                    obj.Move();
                }

                myPrimitive._LineInst.Draw();

                if (fillMode > 0)
                {
                    inst.SetColorA(-0.25f);
                    inst.Draw(true);
                }

                inst.SetColorA(0);
                inst.Draw(false);

                if (list.Count < N)
                {
                    list.Add(new myObj_041());
                }

                System.Threading.Thread.Sleep(renderDelay);
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void initShapes()
        {
            int lineN = N;

            myPrimitive.init_Rectangle();
            myPrimitive.init_LineInst(lineN);

            base.initShapes(shape, N, 0);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        // Dim the screen constantly
        private void dimScreen(bool useStrongerDimFactor = false)
        {
            int rnd = rand.Next(101), dimFactor = 1;

            if (useStrongerDimFactor && rnd < 11)
            {
                dimFactor = (rnd == 0) ? 5 : 2;
            }

            myPrimitive._Rectangle.SetAngle(0);

            // Shift background color just a bit, to hide long lasting traces of shapes
            myPrimitive._Rectangle.SetColor(rand.Next(5) * 0.01f, rand.Next(5) * 0.01f, rand.Next(5) * 0.01f, dimAlpha * dimFactor);
            myPrimitive._Rectangle.Draw(0, 0, gl_Width, gl_Height, true);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------
    };
};
