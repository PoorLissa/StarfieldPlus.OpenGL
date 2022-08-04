using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;


/*
    - todo -- provide description
*/


namespace my
{
    public class myObj_040 : myObject
    {
        private float x, y, dx, dy, size, dSize, R, G, B, A, dA, angle, dAngle;
        int angleMode = 0, signX, signY, oldX = 0, oldY = 0;

        static float dimAlpha = 0;
        static int N = 0, rndMax = 0, shape = 0, moveType = 0, dimRate = 0, maxSize = 0, lineMode = 0, fillMode = 0;

        private static myInstancedPrimitive inst = null;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_040()
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
            gl_x0 = gl_Width / 2;
            gl_y0 = gl_Height / 2;

            N = (N == 0) ? 1111 + rand.Next(333) : N;
            renderDelay = 10;

            shape = rand.Next(5);
            moveType = rand.Next(12);
            lineMode = rand.Next(5);
            fillMode = rand.Next(3);

            moveType = 13;

            rndMax = rand.Next(800) + 100;

            dimAlpha = 0.001f * (rand.Next(100) + 1);
            dimAlpha *= myUtils.randomChance(rand, 1, 2) ? 1.0f : 0.5f;

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void setNextMode()
        {
            // Keep shape type and connection mode, as changing those requires also reinitializing other primitives;
            // todo: fix this sometimes later
            var oldShapeType = shape;

            list.Clear();

            init();
            shape = oldShapeType;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            return $"Obj = myObj_040\n\n" +
                            $"N = {N}\n" + 
                            $"moveType = {moveType}\n" +
                            $"dimAlpha = {dimAlpha}\n" + 
                            $"rndMax = {rndMax}\n" +
                            $"fillMode = {fillMode}\n" + 
                            $"lineMode = {lineMode}"
                            ;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void generateNew()
        {
            dx = 0;
            dy = 0;

            x = rand.Next(gl_Width);
            y = rand.Next(gl_Height);

            oldX = (int)x;
            oldY = (int)y;

            float speed = 1.5f + 0.1f * rand.Next(30);
            float dist = (float)(Math.Sqrt((x - gl_x0) * (x - gl_x0) + (y - gl_y0) * (y - gl_y0)));

            if (dimRate == 1)
                speed *= 2.0f;

            if (dimRate == 2)
                speed *= 1.33f;

            dx = (x - gl_x0) * speed / dist;
            dy = (y - gl_y0) * speed / dist;

            signX = dx > 0 ? 1 : -1;
            signY = dy > 0 ? 1 : -1;

            size = rand.Next(maxSize) + 1;
            dSize = 0.001f * rand.Next(50);
            angle = 0;
            dAngle = 0.001f * rand.Next(111) * myUtils.randomSign(rand);
            angleMode = rand.Next(3);

            R = 1;
            G = 1;
            B = 1;
            A = 0;

            dA = 0.0001f * (rand.Next(100) + 1);

            colorPicker.getColorRand(ref R, ref G, ref B);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            oldX = (int)x;
            oldY = (int)y;

            switch (moveType)
            {
                case 0 :  move0(); break;
                case 1 :  move1(); break;
                case 2 :  move2(); break;
                case 3 :  move3(); break;
                case 4 :  move4(); break;
                case 5 :  move5(); break;
                case 6 :  move6(); break;
                case 7 :  move7(); break;
                case 8 :  move8(); break;
                case 9 :  move9(); break;
                case 10: move10(); break;
                case 11: move11(); break;
                case 12: move12(); break;
                case 13: move13(); break;
            }

            if (y < 0 || y > gl_Height || x < 0 || x > gl_Width || A < 0)
            {
                generateNew();
            }

            switch (angleMode)
            {
                case 1: angle += dAngle; break;
                case 2: angle = (float)rand.NextDouble(); break;
            }

            A += dA;

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
                    case 1: case 2:
                        myPrimitive._LineInst.setInstanceColor(1, 1, 1, A);
                        break;

                    case 3: case 4:
                        myPrimitive._LineInst.setInstanceColor(1, 1, 1, 1);
                        break;
                }
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Process(Window window)
        {
            uint cnt = 0;
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
                    var obj = list[i] as myObj_040;

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
                    list.Add(new myObj_040());
                }

                cnt++;

                System.Threading.Thread.Sleep(renderDelay);
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void initShapes()
        {
            int lineN = N, shapeN = N;

            myPrimitive.init_Rectangle();
            myPrimitive.init_LineInst(lineN);

            int rotationSubMode = 0;

            switch (shape)
            {
                case 0:
                    myPrimitive.init_RectangleInst(shapeN);
                    myPrimitive._RectangleInst.setRotationMode(rotationSubMode);
                    inst = myPrimitive._RectangleInst;
                    break;

                case 1:
                    myPrimitive.init_TriangleInst(shapeN);
                    myPrimitive._TriangleInst.setRotationMode(rotationSubMode);
                    inst = myPrimitive._TriangleInst;
                    break;

                case 2:
                    myPrimitive.init_EllipseInst(shapeN);
                    myPrimitive._EllipseInst.setRotationMode(rotationSubMode);
                    inst = myPrimitive._EllipseInst;
                    break;

                case 3:
                    myPrimitive.init_PentagonInst(shapeN);
                    myPrimitive._PentagonInst.setRotationMode(rotationSubMode);
                    inst = myPrimitive._PentagonInst;
                    break;

                case 4:
                    myPrimitive.init_HexagonInst(shapeN);
                    myPrimitive._HexagonInst.setRotationMode(rotationSubMode);
                    inst = myPrimitive._HexagonInst;
                    break;
            }

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

        private void move0()
        {
            float baseStep = rndMax > 750 ? 0.005f : 0.01f;

            x += dx;
            y += dy;

            x += baseStep * rand.Next(rndMax) * myUtils.randomSign(rand);
            y += baseStep * rand.Next(rndMax) * myUtils.randomSign(rand);

            size += dSize;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void move1()
        {
            x += dx;
            y += dy;

            if (rand.Next(11) == 0)
                dx += (float)rand.NextDouble() / 3 * signX;

            if (rand.Next(11) == 0)
                dy += (float)rand.NextDouble() / 3 * signY;

            size += dSize;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void move2()
        {
            x += dx;
            y += dy;

            if (rand.Next(33) == 0)
            {
                if (Math.Abs(dx) > Math.Abs(dy))
                {
                    dy = dx * myUtils.randomSign(rand);
                    dx = 0;
                }
                else
                {
                    dx = dy * myUtils.randomSign(rand);
                    dy = 0;
                }

                size += dSize;

                // Make sure the particle does not live forever: start reducing opacity at some point
                if (size > 3 && dA > 0)
                {
                    dA *= -1;
                }
            }
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void move3()
        {
            x += dx;
            y += dy;

            if (rand.Next(33) == 0)
            {
                if (Math.Abs(dx) > Math.Abs(dy))
                {
                    dy = Math.Abs(dx) * signY;
                    dx = 0;
                }
                else
                {
                    dx = Math.Abs(dy) * signX;
                    dy = 0;
                }

                size += dSize;
            }
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void move4()
        {
            x += dx;
            y += dy;

            x += dx + (float)(Math.Sin(y) * 5);
            y += dy + (float)(Math.Sin(x) * 5);

            size += dSize;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void move5()
        {
            x += dx;
            y += dy;

            x += dx + (float)(Math.Sin(y) * rand.Next(7));
            y += dy + (float)(Math.Sin(x) * rand.Next(7));

            size += dSize;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void move6()
        {
            x += dx;
            y += dy;

            x += dx + (float)(Math.Sin(y) * rand.Next(7) * myUtils.randomSign(rand));
            y += dy + (float)(Math.Sin(x) * rand.Next(7) * myUtils.randomSign(rand));

            size += dSize;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void move7()
        {
            x += dx;
            y += dy;

            x += dx + (float)(Math.Sin(1 / y)) * myUtils.randomSign(rand);
            y += dy + (float)(Math.Cos(1 / x)) * myUtils.randomSign(rand);

            size += dSize;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void move8()
        {
            x += dx;
            y += dy;

            x += dx + (float)(Math.Sin(y) + Math.Cos(x) * rand.Next(7));
            y += dy + (float)(Math.Sin(x) + Math.Cos(y) * rand.Next(7));

            size += dSize;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void move9()
        {
            x += dx;
            y += dy;

            x += (float)(Math.Sin(size * dx));
            y += (float)(Math.Sin(size * dy));

            size += dSize;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void move10()
        {
            x += dx;
            y += dy;

            x += (float)(Math.Sin(size * dx));
            y += (float)(Math.Cos(size * dy));

            size += dSize;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void move11()
        {
            x += dx;
            y += dy;

            if (rand.Next(10) == 0)
                dx *= (float)rand.NextDouble() * 2.1f;

            if (rand.Next(10) == 0)
                dy *= (float)rand.NextDouble() * 2.1f;

            size += dSize;

            if (size > 10 && dA > 0)
            {
                dA *= -1;
            }
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void move12()
        {
            if (dx != 0 && dy != 0)
            {
                if (myUtils.randomChance(rand, 1, 2))
                {
                    dx = 0;
                }
                else
                {
                    dy = 0;
                }
            }

            float baseStep = 0.001f;

            x += dx;
            y += dy;

            if (dy == 0)
                y += baseStep * rand.Next(rndMax) * myUtils.randomSign(rand);
            else
                x += baseStep * rand.Next(rndMax) * myUtils.randomSign(rand);

            size += dSize;

            if (size > 10 && dA > 0)
            {
                dA *= -1;
            }
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void move13()
        {
            if (dx != 0 && dy != 0)
            {
                if (myUtils.randomChance(rand, 1, 2))
                {
                    dx = 0;
                }
                else
                {
                    dy = 0;
                }
            }

            float baseStep = 0.005f;

            x += dx;
            y += dy;

            if (dx == 0)
                y += baseStep * rand.Next(rndMax) * myUtils.randomSign(rand);
            else
                x += baseStep * rand.Next(rndMax) * myUtils.randomSign(rand);

            size += dSize;

            if (size > 10 && dA > 0)
            {
                dA *= -1;
            }
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void move14()
        {
            x += dx + dy;
            y += dy + dx;

            size += dSize;

            if (size > 10 && dA > 0)
            {
                dA *= -1;
            }
        }

    };
};
