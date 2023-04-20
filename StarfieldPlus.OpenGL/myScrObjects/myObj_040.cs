using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;


/*
    - Snake-like branches moving outwards from the center
*/


namespace my
{
    public class myObj_040 : myObject
    {
        // Priority
        public static int Priority => 10;

        private float x, y, dx, dy, size, dSize, R, G, B, A, dA, angle, dAngle;
        private int angleMode = 0, signX, signY, oldX = 0, oldY = 0;

        private static float dimAlpha = 0, ddSize = 0, offX = 0, offY = 0, dOffX = 0, dOffY = 0;
        private static int N = 0, rndMax = 0, shape = 0, moveType = 0, dimRate = 0, maxSize = 0,
                           lineMode = 0, fillMode = 0, centerGenMode = 0, tailLength = 0;
        static bool doUseStrongerDimFactor = false, doDrawTwice = true, doAdjustOpacity = true, doAdjustSpeed = true;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_040()
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
                doDrawTwice   = myUtils.randomBool(rand);

                shape = rand.Next(5);
                N = 1111 + rand.Next(333);
                stepsPerFrame = rand.Next(5) + 1;
            }

            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time local initialization
        private void initLocal()
        {
            renderDelay = 10;

            moveType = rand.Next(14);
            lineMode = rand.Next(13);                               // Draw straight line from (x, y) to (xOld, yOld)
            fillMode = rand.Next(3);
            centerGenMode = rand.Next(4);                           // How dx/dy are generated, also, position of center
            doAdjustOpacity = myUtils.randomChance(rand, 2, 3);     // If true, reduce opacity for multi-step modes
            doAdjustSpeed   = myUtils.randomChance(rand, 1, 2);     // If true, reduce dx and dy for multi-step modes

            tailLength = myUtils.randomChance(rand, 1, 2) ? rand.Next(11) : 0;
            rndMax = rand.Next(800) + 100;

            ddSize = myUtils.randomChance(rand, 1, 3)
                ? 0 
                : myUtils.randFloat(rand, 0.1f) * 0.0005f;

            // Coordinates to offset the center
            if (centerGenMode >= 2)
            {
                offX = myUtils.randomSign(rand) * rand.Next(gl_x0);
                offY = myUtils.randomSign(rand) * rand.Next(gl_y0);

                dOffX = myUtils.randomSign(rand) * myUtils.randFloat(rand, 0.25f) * 5;
                dOffY = myUtils.randomSign(rand) * myUtils.randFloat(rand, 0.25f) * 5;
            }

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

            doUseStrongerDimFactor = dimAlpha < 0.05f;

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
            return $"Obj = myObj_040\n\n"                                           +
                            $"N = {list.Count} of {N}\n"                            +
                            $"doClearBuffer = {doClearBuffer}\n"                    +
                            $"doDrawTwice = {doDrawTwice}\n"                        +
                            $"doUseStrongerDimFactor = {doUseStrongerDimFactor}\n"  +
                            $"doAdjustOpacity = {doAdjustOpacity}\n"                +
                            $"doAdjustSpeed = {doAdjustSpeed}\n"                    +
                            $"moveType = {moveType}\n"                              +
                            $"dimAlpha = {dimAlpha}\n"                              +
                            $"ddSize = {ddSize}\n"                                  +
                            $"tailLength = {tailLength}\n"                          +
                            $"rndMax = {rndMax}\n"                                  +
                            $"fillMode = {fillMode}\n"                              +
                            $"lineMode = {lineMode}\n"                              +
                            $"centerGenMode = {centerGenMode}\n"                    +
                            $"stepsPerFrame = {stepsPerFrame}\n"
                ;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void generateNew()
        {
            // Calculate x, y, dx, dy
            {
                int x0 = 0, y0 = 0;

                switch (centerGenMode)
                {
                    case 0:
                        x = rand.Next(gl_Width);
                        y = rand.Next(gl_Height);
                        x0 = gl_x0;
                        y0 = gl_y0;
                        break;

                    case 1:
                    case 2:
                    case 3:
                        x = rand.Next(gl_Width);
                        y = rand.Next(gl_Width);
                        x0 = gl_x0;
                        y0 = gl_x0;
                        break;
                }

                float speed = 1.5f + 0.1f * rand.Next(30);
                float dist = (float)(Math.Sqrt((x - x0) * (x - x0) + (y - y0) * (y - y0)));

                // todo: Not used?..
                switch (dimRate)
                {
                    case 0:                 break;
                    case 1: speed *= 2.0f;  break;
                    case 2: speed *= 1.33f; break;
                }

                float factor = speed / dist;

                dx = (x - x0) * factor;
                dy = (y - y0) * factor;

                // Adjust overall speed for multi-step modes
                if (doAdjustSpeed)
                {
                    dx /= stepsPerFrame;
                    dy /= stepsPerFrame;
                }

                switch (centerGenMode)
                {
                    case 0:
                        break;

                    case 1:
                        y -= (gl_Width - gl_Height) / 2;
                        break;

                    case 2:
                        y -= (gl_Width - gl_Height) / 2;

                        x += offX;
                        y += offY;
                        break;

                    // Move central point around the field
                    case 3:
                        y -= (gl_Width - gl_Height) / 2;

                        x += (int)offX;
                        y += (int)offY;

                        if (id == 0)
                        {
                            offX += dOffX;
                            offY += dOffY;

                            if (x > gl_Width)
                                dOffX -= 0.5f;

                            if (x < 0)
                                dOffX += 0.5f;

                            if (y > gl_Height)
                                dOffY -= 0.5f;

                            if (y < 0)
                                dOffY += 0.5f;
                        }
                        break;
                }
            }

            signX = dx > 0 ? 1 : -1;
            signY = dy > 0 ? 1 : -1;

            size = rand.Next(maxSize) + 1;
            dSize = 0.001f * rand.Next(50);
            angle = 0;
            dAngle = 0.001f * rand.Next(111) * myUtils.randomSign(rand);
            angleMode = rand.Next(3);

            R = G = B = A = 0;

            dA = 0.0001f * (rand.Next(100) + 1);

            // Adjust overall opacity for multi-step modes
            if (doAdjustOpacity)
            {
                dA /= stepsPerFrame;
            }

            colorPicker.getColorRand(ref R, ref G, ref B);

            oldX = (int)x;
            oldY = (int)y;

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

            switch (angleMode)
            {
                case 1: angle += dAngle;                 break;
                case 2: angle = myUtils.randFloat(rand); break;
            }

            A += dA;

            if (y < 0 || y > gl_Height || x < 0 || x > gl_Width || A < 0)
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
                    {
                        var rectInst = inst as myRectangleInst;

                        rectInst.setInstanceCoords(x - size, y - size, size2x, size2x);
                        rectInst.setInstanceColor(R, G, B, A);
                        rectInst.setInstanceAngle(angle);

                        if (doDrawTwice)
                        {
                            size2x += 2;
                            rectInst.setInstanceCoords(x - size - 1, y - size - 1, size2x, size2x);
                            rectInst.setInstanceColor(R, G, B, A * 0.33f);
                            rectInst.setInstanceAngle(angle);
                        }
                    }
                    break;

                case 1:
                    {
                        var triangleInst = inst as myTriangleInst;

                        triangleInst.setInstanceCoords(x, y, size, angle);
                        triangleInst.setInstanceColor(R, G, B, A);

                        if (doDrawTwice)
                        {
                            triangleInst.setInstanceCoords(x, y, size + 2, angle);
                            triangleInst.setInstanceColor(R, G, B, A * 0.33f);
                        }
                    }
                    break;

                case 2:
                    {
                        var ellipseInst = inst as myEllipseInst;

                        ellipseInst.setInstanceCoords(x, y, size2x, angle);
                        ellipseInst.setInstanceColor(R, G, B, A);

                        if (doDrawTwice)
                        {
                            ellipseInst.setInstanceCoords(x, y, size2x + 2, angle);
                            ellipseInst.setInstanceColor(R, G, B, A * 0.33f);
                        }
                    }
                    break;

                case 3:
                    {
                        var pentagonInst = inst as myPentagonInst;

                        pentagonInst.setInstanceCoords(x, y, size2x, angle);
                        pentagonInst.setInstanceColor(R, G, B, A * 0.33f);

                        if (doDrawTwice)
                        {
                            pentagonInst.setInstanceCoords(x, y, size2x + 2, angle);
                            pentagonInst.setInstanceColor(R, G, B, A * 0.33f);
                        }
                    }
                    break;

                case 4:
                    {
                        var hexagonInst = inst as myHexagonInst;

                        hexagonInst.setInstanceCoords(x, y, size2x, angle);
                        hexagonInst.setInstanceColor(R, G, B, A);

                        if (doDrawTwice)
                        {
                            hexagonInst.setInstanceCoords(x, y, size2x + 2, angle);
                            hexagonInst.setInstanceColor(R, G, B, A * 0.33f);
                        }
                    }
                    break;
            }

            if (lineMode != 0)
            {
                myPrimitive._LineInst.setInstanceCoords(x, y, oldX - dx * tailLength, oldY - dy * tailLength);

                switch (lineMode)
                {
                    // White, opacity = A
                    case 1: case 2:
                        myPrimitive._LineInst.setInstanceColor(1.0f, 1.0f, 1.0f, A);
                        break;

                    // White, opacity = 1
                    case 3: case 4:
                        myPrimitive._LineInst.setInstanceColor(1.0f, 1.0f, 1.0f, 1.0f);
                        break;

                    // Black , opacity = A
                    case 5: case 6:
                        myPrimitive._LineInst.setInstanceColor(0, 0, 0, A);
                        break;

                    // Black , opacity = 1
                    case 7: case 8:
                        myPrimitive._LineInst.setInstanceColor(0, 0, 0, 1.0f);
                        break;

                    // RGB, opacity = A
                    case 9: case 10:
                        myPrimitive._LineInst.setInstanceColor(R, G, B, A);
                        break;

                    // RGB, opacity = 1
                    case 11: case 12:
                        myPrimitive._LineInst.setInstanceColor(R, G, B, 1.0f);
                        break;
                }
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Process(Window window)
        {
            uint cnt = 0;
            int step, i;
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

                    dimScreen(dimAlpha, false, doUseStrongerDimFactor);
                }

                // Render frame
                {
                    inst.ResetBuffer();

                    if (lineMode != 0)
                    {
                        myPrimitive._LineInst.ResetBuffer();
                    }

                    for (step = 0; step < stepsPerFrame; step++)
                    {
                        for (i = 0; i != list.Count; i++)
                        {
                            var obj = list[i] as myObj_040;

                            obj.Show();
                            obj.Move();
                        }
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
                    list.Add(new myObj_040());
                }

                cnt++;
                System.Threading.Thread.Sleep(renderDelay);
            }

            list.Clear();

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void initShapes()
        {
            int lineN = N;

            myPrimitive.init_Rectangle();

            myPrimitive.init_ScrDimmer();
            myPrimitive.init_LineInst(lineN * stepsPerFrame);

            base.initShapes(shape, N * stepsPerFrame * (doDrawTwice ? 2 : 1), 0);

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

            size += dSize > 0 ? dSize : 0;
            dSize -= ddSize;
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

            size += dSize > 0 ? dSize : 0;
            dSize -= ddSize;
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

                size += dSize > 0 ? dSize : 0;
                dSize -= ddSize;
            }
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void move4()
        {
            x += dx;
            y += dy;

            x += dx + (float)(Math.Sin(y) * 5);
            y += dy + (float)(Math.Sin(x) * 5);

            size += dSize > 0 ? dSize : 0;
            dSize -= ddSize;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void move5()
        {
            x += dx;
            y += dy;

            x += dx + (float)(Math.Sin(y) * rand.Next(7));
            y += dy + (float)(Math.Sin(x) * rand.Next(7));

            size += dSize > 0 ? dSize : 0;
            dSize -= ddSize;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void move6()
        {
            x += dx;
            y += dy;

            x += dx + (float)(Math.Sin(y) * rand.Next(7) * myUtils.randomSign(rand));
            y += dy + (float)(Math.Sin(x) * rand.Next(7) * myUtils.randomSign(rand));

            size += dSize > 0 ? dSize : 0;
            dSize -= ddSize;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void move7()
        {
            x += dx;
            y += dy;

            x += dx + (float)(Math.Sin(1 / y)) * myUtils.randomSign(rand);
            y += dy + (float)(Math.Cos(1 / x)) * myUtils.randomSign(rand);

            size += dSize > 0 ? dSize : 0;
            dSize -= ddSize;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void move8()
        {
            x += dx;
            y += dy;

            x += dx + (float)(Math.Sin(y) + Math.Cos(x) * rand.Next(7));
            y += dy + (float)(Math.Sin(x) + Math.Cos(y) * rand.Next(7));

            size += dSize > 0 ? dSize : 0;
            dSize -= ddSize;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void move9()
        {
            x += dx;
            y += dy;

            x += (float)(Math.Sin(size * dx));
            y += (float)(Math.Sin(size * dy));

            size += dSize > 0 ? dSize : 0;
            dSize -= ddSize;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void move10()
        {
            x += dx;
            y += dy;

            x += (float)(Math.Sin(size * dx));
            y += (float)(Math.Cos(size * dy));

            size += dSize > 0 ? dSize : 0;
            dSize -= ddSize;
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

            size += dSize > 0 ? dSize : 0;
            dSize -= ddSize/2;

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

            size += dSize > 0 ? dSize : 0;
            dSize -= ddSize/2;

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

            size += dSize > 0 ? dSize : 0;
            dSize -= ddSize/2;

            if (size > 10 && dA > 0)
            {
                dA *= -1;
            }
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void move14()
        {
            // not used yet

            x += dx + dy;
            y += dy + dx;

            size += dSize > 0 ? dSize : 0;
            dSize -= ddSize/2;

            if (size > 10 && dA > 0)
            {
                dA *= -1;
            }
        }

    };
};
