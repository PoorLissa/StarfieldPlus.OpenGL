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
        // Priority
        public static int Priority => 10;
		public static System.Type Type => typeof(myObj_041);

        private int dxi, dyi, oldX, oldY;
        private float dxf = 0, dyf = 0, x = 0, y = 0, time = 0, size = 0, A = 0, dA = 0, angle, dAngle;

        private static float dimAlpha = 0.0f, R = 1, G = 1, B = 1;
        private static int N = 0, x0 = 0, y0 = 0;
        private static int moveMode = 0, shape = 0, speedMode = 0, fillMode = 0, lineMode = 0, colorMode = 0, maxRnd = 0;
        private static bool doUseStrongerDim = true;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_041()
        {
            generateNew();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time global initialization
        protected override void initGlobal()
        {
            colorPicker = new myColorPicker(gl_Width, gl_Height);
            list = new List<myObject>();

            // Global immutable consts
            {
                doClearBuffer = false;

                N = 1111 + rand.Next(333);
                shape = rand.Next(5);
            }

            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time local initialization
        private void initLocal()
        {
            renderDelay = 10;

            maxRnd = rand.Next(20) + 1;

            lineMode  = rand.Next(5);
            moveMode  = rand.Next(19);
            speedMode = rand.Next(2);
            fillMode  = rand.Next(3);
            colorMode = rand.Next(2);

            x0 = gl_x0;
            y0 = gl_y0;

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

            doUseStrongerDim = dimAlpha < 0.05f;

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void setNextMode()
        {
            list.Clear();

            initLocal();

            dimScreen(0.5f);
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            return $"Obj = {Type}\n\n"                               	+
                            $"N = {list.Count} of {N}\n"                +
                            $"doClearBuffer = {doClearBuffer}\n"        +
                            $"dimAlpha = {dimAlpha.ToString("0.00")}\n" +
                            $"moveMode = {moveMode}\n"                  +
                            $"colorMode = {colorMode}\n"                +
                            $"fillMode = {fillMode}\n"                  +
                            $"lineMode = {lineMode}\n"                  +
                            $"maxRnd = {maxRnd}\n"                      +
                            $"renderDelay = {renderDelay}\n"
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

            switch (colorMode)
            {
                case 0:
                    R = 1.0f - myUtils.randFloat(rand) * 0.1f;
                    G = 1.0f - myUtils.randFloat(rand) * 0.1f;
                    B = 1.0f - myUtils.randFloat(rand) * 0.1f;
                    break;

                case 1:
                    colorPicker.getColorRand(ref R, ref G, ref B);
                    break;
            }

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

            if (A > 3.0f && dA > 0)
            {
                dA *= -1.25f;
            }

            if (dimAlpha > 0.3f && rand.Next(33) == 0)
            {
                x0 += rand.Next(11) - 5;
                y0 += rand.Next(11) - 5;
            }

            if (x < 0 || x > gl_Width || y < 0 || y > gl_Height || A < 0)
            {
                generateNew();
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Show()
        {
            float size2x = size * 2;

            switch (shape)
            {
                case 0:
                    var rectInst = inst as myRectangleInst;

                    rectInst.setInstanceCoords(x - size, y - size, size2x, size2x);
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

                    ellipseInst.setInstanceCoords(x, y, size2x, angle);
                    ellipseInst.setInstanceColor(R, G, B, A);
                    break;

                case 3:
                    var pentagonInst = inst as myPentagonInst;

                    pentagonInst.setInstanceCoords(x, y, size2x, angle);
                    pentagonInst.setInstanceColor(R, G, B, A);
                    break;

                case 4:
                    var hexagonInst = inst as myHexagonInst;

                    hexagonInst.setInstanceCoords(x, y, size2x, angle);
                    hexagonInst.setInstanceColor(R, G, B, A);
                    break;
            }

            if (lineMode != 0)
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
            int i;
            initShapes();

            dimScreenRGB_SetRandom(0.1f);
            glDrawBuffer(GL_FRONT_AND_BACK);

            while (!Glfw.WindowShouldClose(window))
            {
                processInput(window);

                // Swap fore/back framebuffers, and poll for operating system events.
                Glfw.SwapBuffers(window);
                Glfw.PollEvents();

                // Dim screen
                {
                    // Modify background color occasionally
                    if (myUtils.randomChance(rand, 1, 10001))
                    {
                        dimScreenRGB_Adjust(0.1f);
                    }

                    dimScreen(dimAlpha, false, doUseStrongerDim);
                }

                // Render frame
                {
                    inst.ResetBuffer();

                    if (lineMode != 0)
                    {
                        myPrimitive._LineInst.ResetBuffer();
                    }

                    for (i = 0; i < list.Count; i++)
                    {
                        var obj = list[i] as myObj_041;

                        obj.Show();
                        obj.Move();
                    }

                    if (lineMode != 0)
                    {
                        myPrimitive._LineInst.Draw();
                    }

                    if (fillMode > 0)
                    {
                        inst.SetColorA(-0.25f);
                        inst.Draw(true);
                    }

                    inst.SetColorA(0);
                    inst.Draw(false);
                }

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

            myPrimitive.init_ScrDimmer();
            myPrimitive.init_LineInst(lineN);

            base.initShapes(shape, N, 0);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------
    };
};
