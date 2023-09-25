using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;


/*
    - Linearly Moving Shapes (Soap Bubbles alike)
*/


namespace my
{
    public class myObj_020 : myObject
    {
        // Priority
        public static int Priority => 10;
		public static System.Type Type => typeof(myObj_020);

        private float x, y, dx, dy, Size, dSize, angle, dAngle, angleRate, A = 0, R = 0, G = 0, B = 0;
        int lifeCounter = 0;

        private static int shape = 0, N = 0, shapeCnt = 1, angleMode = 0, opacityMode = 0;
        private static bool doFillShapes = false;
        private static float spdConst = 0, dimAlpha = 0.1f, colorShiftRateR = 0, colorShiftRateG = 0, colorShiftRateB = 0;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_020()
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

        // One-time local initialization
        private void initLocal()
        {
            N = 3333 + rand.Next(6666);
            renderDelay = 10;
            spdConst = (rand.Next(50) + 1) / 1000.0f;

            doFillShapes = myUtils.randomBool(rand);
            doClearBuffer = myUtils.randomChance(rand, 1, 3);
            dimAlpha = myUtils.randFloat(rand, 0.1f);

            shape = rand.Next(5);
            shapeCnt = (myUtils.randomChance(rand, 1, 3) ? rand.Next(15) : rand.Next(5)) + 1;
            angleMode = rand.Next(7);
            opacityMode = rand.Next(2);

            colorShiftRateR = myUtils.randFloat(rand) * 0.1f;
            colorShiftRateG = myUtils.randFloat(rand) * 0.1f;
            colorShiftRateB = myUtils.randFloat(rand) * 0.1f;

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            string str = $"Obj = myObj_020\n\n"                         +
                            $"N = {list.Count} of {N}\n"                +
                            $"doClearBuffer = {doClearBuffer}\n"        +
                            $"dimAlpha = {dimAlpha.ToString("0.00")}\n" +
                            $"shape = {shape}\n"                        +
                            $"spdConst = {spdConst}\n"                  +
                            $"shapeCnt = {shapeCnt}\n"                  +
                            $"angleMode = {angleMode}\n"                +
                            $"opacityMode = {opacityMode}\n"            +
                            $"renderDelay = {renderDelay}\n"            +
                            $"file: {colorPicker.GetFileName()}"
                ;
            return str;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void generateNew()
        {
            lifeCounter = rand.Next(100) + 100;

            int x0 = rand.Next(gl_Width);
            int y0 = rand.Next(gl_Width);
            int speed = rand.Next(2000) + 10;

            do
            {
                x = rand.Next(gl_Width);
                y = rand.Next(gl_Width);
            }
            while (x == x0 && y == y0);

            double dist = Math.Sqrt((x - x0) * (x - x0) + (y - y0) * (y - y0));
            double sp_dist = spdConst * speed / dist;

            dx = (float)((x - x0) * sp_dist);
            dy = (float)((y - y0) * sp_dist);

            Size = 1;
            dSize = 0.0005f * (rand.Next(1000) + 1);

            A = myUtils.randFloat(rand);

            angle = 0;
            dAngle = myUtils.randomSign(rand) * (float)rand.NextDouble() * 0.1f;
            angleRate = rand.Next(5) + 1;

            colorPicker.getColor(x, y, ref R, ref G, ref B);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            if (Size != 0)
            {
                x += dx;
                y += dy;
                angle += dAngle;
                Size += dSize;
                A -= 0.001f;

                if (A < 0 || x < -Size || x > gl_Width + Size || y < -Size || y > gl_Height + Size)
                {
                    x = -1111;
                    y = -1111;
                    Size = 0;
                }
            }
            else
            {
                if (lifeCounter-- == 0)
                {
                    generateNew();
                }
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Show()
        {
            if (Size > 0)
            {
                float oldSize = Size, newAngle = 0, sizeX2, a = 1;
                float sizeTmp = oldSize / shapeCnt;

                for (int i = 1; i != (shapeCnt + 1); i++)
                {
                    Size = oldSize - (sizeTmp * (i-1));
                    sizeX2 = Size * 2;

                    switch (opacityMode)
                    {
                        // Outer shape is the most visible
                        case 0:
                            a = A / i;
                            break;

                        // Inner shape is the most visible
                        case 1:
                            a = A / (shapeCnt - i + 1);
                            break;
                    }

                    switch (angleMode)
                    {
                        case 0: newAngle = angle; break;
                        case 1: newAngle = angle / i / angleRate; break;
                        case 2: newAngle = angle - i * dAngle * angleRate; break;
                        case 3: newAngle = angle - i * i * dAngle * angleRate; break;
                        case 4: newAngle = i == 1 ? angle : myUtils.randFloat(rand); break;
                        case 5: newAngle = i == 1 ? angle : angle * Size / 5 / angleRate; break;
                        case 6: newAngle = i == 1 ? angle : angle * Size / 25 / angleRate; break;
                    }

                    switch (shape)
                    {
                        case 0:
                            var rectInst = inst as myRectangleInst;

                            rectInst.setInstanceCoords(x - Size, y - Size, sizeX2, sizeX2);
                            rectInst.setInstanceColor(R + i * colorShiftRateR, G + i * colorShiftRateG, B + i * colorShiftRateB, a);
                            rectInst.setInstanceAngle(newAngle);
                            break;

                        case 1:
                            var triangleInst = inst as myTriangleInst;

                            triangleInst.setInstanceCoords(x, y, Size, newAngle);
                            triangleInst.setInstanceColor(R + i * colorShiftRateR, G + i * colorShiftRateG, B + i * colorShiftRateB, a);
                            break;

                        case 2:
                            var ellipseInst = inst as myEllipseInst;

                            ellipseInst.setInstanceCoords(x, y, sizeX2, newAngle);
                            ellipseInst.setInstanceColor(R + i * colorShiftRateR, G + i * colorShiftRateG, B + i * colorShiftRateB, a);
                            break;

                        case 3:
                            var pentagonInst = inst as myPentagonInst;

                            pentagonInst.setInstanceCoords(x, y, sizeX2, newAngle);
                            pentagonInst.setInstanceColor(R + i * colorShiftRateR, G + i * colorShiftRateG, B + i * colorShiftRateB, a);
                            break;

                        case 4:
                            var hexagonInst = inst as myHexagonInst;

                            hexagonInst.setInstanceCoords(x, y, sizeX2, newAngle);
                            hexagonInst.setInstanceColor(R + i * colorShiftRateR, G + i * colorShiftRateG, B + i * colorShiftRateB, a);
                            break;
                    }
                }

                Size = oldSize;
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Process(Window window)
        {
            uint cnt = 0;
            initShapes();

            if (doClearBuffer)
            {
                glDrawBuffer(GL_FRONT_AND_BACK | GL_DEPTH_BUFFER_BIT);

                // Bgr color is close to black
                float lightnessFactor = 11;

                // Make bgr color lighter sometimes
                if (myUtils.randomChance(rand, 1, 11))
                {
                    lightnessFactor = 2 + rand.Next(7);
                }

                float r = (float)rand.NextDouble() / lightnessFactor;
                float g = (float)rand.NextDouble() / lightnessFactor;
                float b = (float)rand.NextDouble() / lightnessFactor;

                glClearColor(r, g, b, 1.0f);
            }
            else
            {
                dimScreenRGB_SetRandom(0.1f);
                //glDrawBuffer(GL_FRONT_AND_BACK);
                glDrawBuffer(GL_DEPTH_BUFFER_BIT);
            }



            while (!Glfw.WindowShouldClose(window))
            {
                processInput(window);
                Glfw.PollEvents();

                if (doClearBuffer)
                {
                    Glfw.SwapBuffers(window);
                    glClear(GL_COLOR_BUFFER_BIT);
                }
                else
                {
                    dimScreen(dimAlpha, false);
                    Glfw.SwapBuffers(window);
                }

                // Render Frame
                {
                    inst.ResetBuffer();

                    for (int i = 0; i != list.Count; i++)
                    {
                        var obj = list[i] as myObj_020;

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
                    list.Add(new myObj_020());
                }

                System.Threading.Thread.Sleep(renderDelay);
                cnt++;
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void initShapes()
        {
            int lineN = N * 3, shapeN = N * shapeCnt;

			myPrimitive.init_ScrDimmer();
            myPrimitive.init_Rectangle();
            //myPrimitive.init_LineInst(lineN);

            int rotationSubMode = rand.Next(5) == 0 ? rand.Next(4) : 0;

            base.initShapes(shape, shapeN, rotationSubMode);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------
    };
};
