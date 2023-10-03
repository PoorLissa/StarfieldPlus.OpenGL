using GLFW;
using static OpenGL.GL;
using System.Collections.Generic;


/*
    - Growing shapes -- Rain circles alike
*/


namespace my
{
    public class myObj_131 : myObject
    {
        // Priority
        public static int Priority => 10;
		public static System.Type Type => typeof(myObj_131);

        private int maxSize, sizeStep;
        private float x, y, dx, dy, size, dSize, A, R, G, B, angle, dA, dAngle;

        private static float dX = 0, dY = 0;
        private static int N = 0, shape = 0, shapeN = 1, rotationMode = 0, moveMode = 0, dxdyMode = 0, dxdyFactor = 1, daMode = 0;
        private static bool doFillShapes = false, doShake = false;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_131()
        {
            generateNew();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time global initialization
        protected override void initGlobal()
        {
            colorPicker = new myColorPicker(gl_Width, gl_Height);
            list = new List<myObject>();

            {
                // Number of shapes drawn per object;
                // Min is 2, because we start drawing at '1'
                shapeN = rand.Next(5) + 2;

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

                shape = rand.Next(6) - 1;
                shape = (shape < 0) ? 2 : shape;
            }

            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time local initialization
        private void initLocal()
        {
            dX = (float)rand.NextDouble();
            dY = (float)rand.NextDouble();

            doClearBuffer = myUtils.randomChance(rand, 2, 3);
            doFillShapes  = myUtils.randomChance(rand, 1, 3);
            moveMode = rand.Next(8);
            dxdyMode = rand.Next(3);

            dxdyFactor = rand.Next(7) + 1;

            rotationMode = rand.Next(4);
            daMode = rand.Next(3);
            renderDelay = rand.Next(23);

            doShake = (shape == 2) && (shapeN > 4) && myUtils.randomChance(rand, 1, 3);

            if (false)
            {
                doClearBuffer = false;
                doFillShapes = true;
                doShake = false;
                moveMode = 0;
                rotationMode = 3;
                daMode = 0;
                dxdyMode = 2;
                dxdyFactor = 3;
                renderDelay = 16;
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            string str = $"Obj = {Type}\n\n" 							+
                            $"N = {list.Count} of {N} x {shapeN - 1}\n" +
                            $"doClearBuffer = {doClearBuffer}\n" 		+
                            $"doFillShapes = {doFillShapes}\n" 			+
                            $"shape = {shape}\n" 						+
                            $"moveMode = {moveMode}\n" 					+
                            $"rotationMode = {rotationMode}\n" 			+
                            $"daMode = {daMode}\n" 						+
                            $"dxdyMode = {dxdyMode}\n" 					+
                            $"dxdyFactor = {dxdyFactor}\n" 				+
                            $"renderDelay = {renderDelay}\n" 			+
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
            x = rand.Next(gl_Width);
            y = rand.Next(gl_Height);

            size = 0;
            maxSize = rand.Next(333) + 111;
            sizeStep = rand.Next(25) + 3;
            dSize = 0.01f * (rand.Next(111) + 1);

            angle = 0;

            // Starting angle is not zero
            if (rotationMode > 0)
                angle = (float)rand.NextDouble();

            // Slow rotation
            if (rotationMode > 1)
                dAngle = (float)rand.NextDouble() / 11 * myUtils.randomSign(rand);

            // Fast rotation
            if (rotationMode > 2)
                dAngle = 0.5f * (myUtils.randFloat(rand, 1.0f)) * myUtils.randomSign(rand);

            colorPicker.getColor(x, y, ref R, ref B, ref G);
            A = 0.85f + myUtils.randFloat(rand) * 0.25f;

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

            if (moveMode > 0)
            {
                dx *= dxdyFactor;
                dy *= dxdyFactor;
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            switch (moveMode)
            {
                case 0:
                    x += dx * myUtils.randomSign(rand);
                    y += dy * myUtils.randomSign(rand);
                    break;

                case 1:
                case 2:
                    x += (moveMode == 2) ? dx : -dx;
                    break;

                case 3:
                case 4:
                    y += (moveMode == 4) ? dy : -dy;
                    break;

                case 5:
                    if (x < gl_Width / 4)
                        y += dy;

                    if (x > 3 * gl_Width / 4)
                        y -= dy;

                    if (y < gl_Height / 4)
                        x -= dx;

                    if (y > 3* gl_Height / 4)
                        x += dx;
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

            // Rotate
            angle += dAngle;

            if (A < 0)
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
                // Instanced squares
                case 0:
                    {
                        var rectInst = inst as myRectangleInst;

                        for (int i = 1; i != shapeN; i++)
                        {
                            rectInst.setInstanceCoords(x - size, y - size, size2x, size2x);
                            rectInst.setInstanceColor(R, G, B, A);
                            rectInst.setInstanceAngle(angle/i);
                        }
                    }
                    break;

                // Instanced triangles
                case 1:
                    {
                        var triangleInst = inst as myTriangleInst;

                        for (int i = 1; i != shapeN; i++)
                        {
                            triangleInst.setInstanceCoords(x, y, size2x, angle/i);
                            triangleInst.setInstanceColor(R, G, B, A);
                        }
                    }
                    break;

                // Instanced circles
                case 2:
                    {
                        var ellipseInst = inst as myEllipseInst;

                        for (int i = 1; i != shapeN; i++)
                        {
                            if (doShake)
                            {
                                float xx = x + rand.Next(3) - 1;
                                float yy = y + rand.Next(3) - 1;

                                ellipseInst.setInstanceCoords(xx, yy, size2x - i * sizeStep, 0);
                                ellipseInst.setInstanceColor(R, G, B, A / i);
                            }
                            else
                            {
                                ellipseInst.setInstanceCoords(x, y, size2x - i * sizeStep, 0);
                                ellipseInst.setInstanceColor(R, G, B, A / i);
                            }
                        }
                    }
                    break;

                // Instanced pentagons
                case 3:
                    {
                        var pentagonInst = inst as myPentagonInst;

                        for (int i = 1; i != shapeN; i++)
                        {
                            pentagonInst.setInstanceCoords(x, y, size2x, angle/i);
                            pentagonInst.setInstanceColor(R, G, B, A);
                        }
                    }
                    break;

                // Instanced hexagons
                case 4:
                    {
                        var hexagonInst = inst as myHexagonInst;

                        for (int i = 1; i != shapeN; i++)
                        {
                            hexagonInst.setInstanceCoords(x, y, size2x, angle/i);
                            hexagonInst.setInstanceColor(R, G, B, A);
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

            // Glfw.SwapInterval(0);

            if (doClearBuffer)
            {
                glDrawBuffer(GL_FRONT_AND_BACK | GL_DEPTH_BUFFER_BIT);

                float lightnessFactor = 11;

                // Make bgr color lighter sometimes
                if (myUtils.randomChance(rand, 1, 5))
                    lightnessFactor = 2 + rand.Next(7);

                float r = (float)rand.NextDouble() / lightnessFactor;
                float g = (float)rand.NextDouble() / lightnessFactor;
                float b = (float)rand.NextDouble() / lightnessFactor;

                glClearColor(r, g, b, 1.0f);
            }
            else
            {
                //glDrawBuffer(GL_FRONT_AND_BACK);
                glDrawBuffer(GL_BACK);
                dimScreenRGB_SetRandom(0.1f);
            }

            while (!Glfw.WindowShouldClose(window))
            {
                cnt++;

                processInput(window);

                // Swap fore/back framebuffers, and poll for operating system events
                Glfw.SwapBuffers(window);
                Glfw.PollEvents();

                if (doClearBuffer)
                {
                    glClear(GL_COLOR_BUFFER_BIT);
                }
                else
                {
                    dimScreen(0.25f, false);
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

                if (renderDelay > 0)
                {
                    System.Threading.Thread.Sleep(renderDelay);
                }
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void initShapes()
        {
            myPrimitive.init_ScrDimmer();
            base.initShapes(shape, N * (shapeN - 1), 0);
        }

        // ---------------------------------------------------------------------------------------------------------------
    }
};
