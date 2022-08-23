using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;


/*
    - Growing shapes -- Rain drops alike
*/


namespace my
{
    public class myObj_131 : myObject
    {
        private int maxSize;
        private float x, y, dx, dy, size, dSize, A, R, G, B, angle, dA, dAngle;

        private static float dX = 0, dY = 0;
        private static int N = 0, shape = 0, rotationMode = 0, moveMode = 0, dxdyMode = 0, dxdyFactor = 1, daMode = 0;
        private static bool doClearBuffer = true, doFillShapes = false;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_131()
        {
            if (colorPicker == null)
            {
                colorPicker = new myColorPicker(gl_Width, gl_Height);
                list = new List<myObject>();

                init();
            }

            generateNew();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time initialization
        private void init()
        {
            gl_x0 = gl_Width  / 2;
            gl_y0 = gl_Height / 2;

            dX = (float)rand.NextDouble();
            dY = (float)rand.NextDouble();

            if (N == 0)
            {
                switch (rand.Next(7))
                {
                    case 0:
                        N = 1111 + rand.Next(3333);
                        break;

                    case 1:
                    case 2:
                        N = 777 + rand.Next(1111);
                        break;

                    default:
                        N = 333 + rand.Next(111);
                        break;
                }
            }

            doFillShapes = myUtils.randomChance(rand, 1, 3);
            moveMode = rand.Next(7);
            dxdyMode = rand.Next(3);

            dxdyFactor = rand.Next(7) + 1;

            shape = rand.Next(6) - 1;
            shape = (shape < 0) ? 2 : shape;

            rotationMode = rand.Next(3);

            daMode = rand.Next(3);

            //renderDelay = 10;

            if (false)
            {
                shape = 2;
                rotationMode = 0;
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            string str = $"Obj = myObj_131\n\n" +
                            $"N = {N} ({list.Count})\n" +
                            $"shape = {shape}\n" +
                            $"moveMode = {moveMode}\n" +
                            $"daMode = {daMode}\n" +
                            $"dxdyMode = {dxdyMode}\n" +
                            $"dxdyFactor = {dxdyFactor}\n" +
                            $""
                ;
            return str;
        }

        // ---------------------------------------------------------------------------------------------------------------

        // 
        protected override void setNextMode()
        {
            init();
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void generateNew()
        {
            x = rand.Next(gl_Width);
            y = rand.Next(gl_Height);

            size = 0;
            maxSize = rand.Next(333) + 111;
            dSize = 0.01f * (rand.Next(111) + 1);

            angle = 0;

            if (rotationMode > 0)
                angle = (float)rand.NextDouble();

            if (rotationMode > 1)
                dAngle = (float)rand.NextDouble() / 11 * myUtils.randomSign(rand);

            colorPicker.getColor(x, y, ref R, ref B, ref G);
            A = 0.85f + (float)rand.NextDouble() / 4;

            // dA affects the life expectancy of the particle (and its final size as well)
            switch (daMode)
            {
                case 0:
                    dA = 0.01f * (rand.Next(10) + 1);
                    break;

                case 1:
                    dA = 0.005f * (rand.Next(50) + 1);
                    break;

                case 2:
                    dA = 0.001f * (rand.Next(100) + 1);
                    break;
            }

            switch (dxdyMode)
            {
                case 0:
                    dx = dy = (dX + dY) / 2;
                    break;

                case 1:
                    dx = dX;
                    dy = dY;
                    break;

                case 2:
                    dx = (float)rand.NextDouble();
                    dy = (float)rand.NextDouble();
                    break;
            }

            dx *= dxdyFactor;
            dy *= dxdyFactor;

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            switch (moveMode)
            {
                case 2:
                    x += dx * myUtils.randomSign(rand);
                    y += dy * myUtils.randomSign(rand);
                    break;

                case 3:
                case 4:
                    x += (moveMode == 2) ? dx : -dx;
                    break;

                case 5:
                case 6:
                    y += (moveMode == 4) ? dy : -dy;
                    break;
            }

            size += dSize;

            // Increase disappearing speed when max size is reached
            if (size > maxSize)
            {
                dA *= 1.1f;
            }

            // Decrease opacity until fully invisible
            A -= dA;

            if (A < 0)
            {
                generateNew();
            }

            angle += dAngle;

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Show()
        {
            switch (shape)
            {
                // Instanced squares
                case 0:
                    var rectInst = inst as myRectangleInst;

                    rectInst.setInstanceCoords(x - size, y - size, 2 * size, 2 * size);
                    rectInst.setInstanceColor(R, G, B, A);
                    rectInst.setInstanceAngle(angle);
                    break;

                // Instanced triangles
                case 1:
                    var triangleInst = inst as myTriangleInst;

                    triangleInst.setInstanceCoords(x, y, 2 * size, angle);
                    triangleInst.setInstanceColor(R, G, B, A);
                    break;

                // Instanced circles
                case 2:
                    var ellipseInst = inst as myEllipseInst;

                    ellipseInst.setInstanceCoords(x, y, 2 * size, angle);
                    ellipseInst.setInstanceColor(R, G, B, A);
                    break;

                // Instanced pentagons
                case 3:
                    var pentagonInst = inst as myPentagonInst;

                    pentagonInst.setInstanceCoords(x, y, 2 * size, angle);
                    pentagonInst.setInstanceColor(R, G, B, A);
                    break;

                // Instanced hexagons
                case 4:
                    var hexagonInst = inst as myHexagonInst;

                    hexagonInst.setInstanceCoords(x, y, 2 * size, angle);
                    hexagonInst.setInstanceColor(R, G, B, A);
                    break;
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Process(Window window)
        {
            uint cnt = 0;
            initShapes();

            //Glfw.SwapInterval(0);

            if (doClearBuffer)
            {
                glDrawBuffer(GL_FRONT_AND_BACK | GL_DEPTH_BUFFER_BIT);
                glClearColor(0, 0, 0, 1);
            }

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
                    inst.ResetBuffer();

                    for (int i = 0; i < list.Count; i++)
                    {
                        var obj = list[i] as myObj_131;

                        obj.Show();
                        obj.Move();
                    }

                    if (doFillShapes)
                    {
                        // Tell the fragment shader to multiply existing instance opacity by 0.5:
                        inst.SetColorA(-0.5f);
                        inst.Draw(true);
                    }

                    // Tell the fragment shader to do nothing with the existing instance opacity:
                    inst.SetColorA(0);
                    inst.Draw(false);
                }

                if (list.Count < N)
                {
                    list.Add(new myObj_131());
                }

                System.Threading.Thread.Sleep(renderDelay);
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void initShapes()
        {
            myPrimitive.init_Rectangle();
            base.initShapes(shape, N, 0);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------
    }
};
