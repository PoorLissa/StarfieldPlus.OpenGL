using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;


/*
    - Growing shapes -- Rain circles alike -- no buffer clearing
*/


namespace my
{
    public class myObj_130 : myObject
    {
        private int x, y, dx, dy;
        private int maxSize = 0, mult = 0, counter = 0;
        private float size, dSize, A, R, G, B, dA, angle, dAngle;

        private static int N = 0, shape = 0, moveMode = 0, growMode = 0, rotationMode = 0;
        private static bool doClearBuffer = false, doFillShapes = false;
        private static float dimAlpha = 0.0f, aFill = 0, bgrR = -1, bgrG = -1, bgrB = -1;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_130()
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

            N = (N == 0) ? 10 + rand.Next(33) : N;

            dimAlpha = 0.001f * (rand.Next(100) + 1);
            aFill = (float)rand.NextDouble() / 3;

            shape = rand.Next(5);
            rotationMode = rand.Next(3);
            moveMode = rand.Next(5);
            growMode = rand.Next(2);
            doFillShapes = myUtils.randomChance(rand, 1, 3);

            if (bgrR < 0 && bgrG < 0 && bgrB < 0)
            {
                if (myUtils.randomChance(rand, 1, 7))
                {
                    bgrR = (float)rand.NextDouble();
                    bgrG = (float)rand.NextDouble();
                    bgrB = (float)rand.NextDouble();
                }
                else
                {
                    if (myUtils.randomChance(rand, 1, 2))
                    {
                        bgrR = 0;
                        bgrG = 0;
                        bgrB = 0;
                    }
                    else
                    {
                        bgrR = 1;
                        bgrG = 1;
                        bgrB = 1;
                    }
                }

            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            string str = $"Obj = myObj_130\n\n" +
                            $"N = {N} ({list.Count})\n" +
                            $"shape = {shape}\n" +
                            $"rotationMode = {rotationMode}\n" +
                            $"moveMode = {moveMode}\n" +
                            $"growMode = {growMode}\n" +
                            $"bgr = [{bgrR}, {bgrG}, {bgrB}]\n" +
                            $"dimAlpha = {dimAlpha}\n" +
                            $"aFill = {aFill}\n" +
                            $"doFillShapes = {doFillShapes}\n" +
                            $""
                ;
            return str;
        }

        // ---------------------------------------------------------------------------------------------------------------

        // 
        protected override void setNextMode()
        {
            var oldShape = shape;

            init();

            shape = oldShape;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void generateNew()
        {
            x = rand.Next(gl_Width);
            y = rand.Next(gl_Height);

            maxSize = rand.Next(333) + 33;
            dSize = 0.1f * (rand.Next(33) + 1);
            dA = 0.0001f * (rand.Next(1000) + 1);
            mult = rand.Next(3) + 1;
            counter = 0;

            size = (growMode == 0) ? 0 : maxSize;
            angle = 0;
            dAngle = 0;

            if (rotationMode > 0)
                angle = (float)rand.NextDouble();

            if (rotationMode > 1)
                dAngle = (float)rand.NextDouble() / 11 * myUtils.randomSign(rand);

            A = (growMode == 0) ? 1 : (float)rand.NextDouble() / 100;
            colorPicker.getColor(x, y, ref R, ref G, ref B);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            switch (moveMode)
            {
                case 0: move0(); break;
                case 1: move1(); break;
                case 2: move2(); break;
                case 3: move3(); break;
                case 4: move4(); break;
/*
                case 4: move4(); break;
                case 5: move5(); break;
                case 6: move6(); break;
                case 7: move7(); break;
                case 8: move8(); break;
*/
                default:
                    //move_test();
                    break;
            }

            counter++;
            angle += dAngle;

            if (growMode == 0)
            {
                size += dSize;

                // Increase disappearing speed when max size is reached
                if (size > maxSize)
                    dA *= 1.05f;

                // Decrease opacity until fully invisible
                A -= dA;

                if (A < 0)
                {
                    generateNew();
                }
            }
            else
            {
                size -= dSize;
                A += dA;

                if (size < 0)
                {
                    generateNew();
                }
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void move0()
        {
            ;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void move1()
        {
            x += (int)(Math.Sin(counter) * counter / mult);
            y += (int)(Math.Cos(counter) * counter / mult);
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void move2()
        {
            x += myUtils.random101(rand) * mult;
            y += myUtils.random101(rand) * mult;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void move3()
        {
            x += myUtils.random101(rand) * mult * 5;
            y += myUtils.random101(rand) * mult * 5;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void move4()
        {
            x += (int)(Math.Sin(x) * Math.Cos(x) * 10);
            y += (int)(Math.Sin(y) * Math.Cos(y) * 10);
            x += (int)(Math.Tan(x) * 10);
            y += (int)(Math.Tan(y) * 10);
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

            // Disable VSYNC if needed
            // Glfw.SwapInterval(0);

            glDrawBuffer(GL_FRONT_AND_BACK);

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
                else
                {
                    dimScreen(useStrongerDimFactor: dimAlpha < 0.05f);
                }

                // Render Frame
                {
                    inst.ResetBuffer();

                    for (int i = 0; i < list.Count; i++)
                    {
                        var obj = list[i] as myObj_130;

                        obj.Show();
                        obj.Move();
                    }

                    if (doFillShapes)
                    {
                        // Tell the fragment shader to multiply existing instance opacity by aFill:
                        inst.SetColorA(-aFill);
                        inst.Draw(true);
                    }

                    // Tell the fragment shader to do nothing with the existing instance opacity:
                    inst.SetColorA(0);
                    inst.Draw(false);
                }

                if (list.Count < N)
                {
                    list.Add(new myObj_130());
                }

                System.Threading.Thread.Sleep(renderDelay);
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void initShapes()
        {
            myPrimitive.init_Rectangle();
            base.initShapes(shape, N, rotationSubMode: 0);

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
            myPrimitive._Rectangle.SetColor(bgrR + rand.Next(5) * 0.01f, bgrG + rand.Next(5) * 0.01f, bgrB + rand.Next(5) * 0.01f, dimAlpha * dimFactor);
            myPrimitive._Rectangle.Draw(0, 0, gl_Width, gl_Height, true);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

    }
};
