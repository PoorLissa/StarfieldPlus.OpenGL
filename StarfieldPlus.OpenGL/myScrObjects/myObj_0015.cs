﻿using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;


/*
    - Snow-like pattern made of different layers moving in different directions
*/


namespace my
{
    public class myObj_0015 : myObject
    {
        // Priority
        public static int Priority => 10;
		public static System.Type Type => typeof(myObj_0015);

        private float x, y, dx, dy, rad, t, dt;
        private float size, A, R, G, B, angle = 0, dAngle = 0;

        private static int N = 0, shape = 0, mode = 0, rotateMode = 0, step = 0;
        private static int minX = 0, minY = 0, maxX = 0, maxY = 0;
        private static bool doFillShapes = false, doTraceColor = false, doUseBaseMove = true;
        private static float sAngle = 0, dimRate = 0;

        private static myScreenGradient grad = null;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_0015()
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
                N = rand.Next(10000) + 1234;

                //N = 33333;

                shape = rand.Next(5);
            }

            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time local initialization
        private void initLocal()
        {
            doUseBaseMove = true;
            doClearBuffer = myUtils.randomChance(rand, 10, 11);
            doFillShapes  = myUtils.randomChance(rand, 1, 3);
            doTraceColor  = myUtils.randomChance(rand, 1, 2) &&
                                        (colorPicker.getMode() == myColorPicker.colorMode.SNAPSHOT ||
                                         colorPicker.getMode() == myColorPicker.colorMode.IMAGE
            );

            rotateMode = rand.Next(3);
            step = rand.Next(333) + 1;

            dimRate = myUtils.randomChance(rand, 1, 3)
                ? myUtils.randFloat(rand) * 0.005f
                : 0.0f;

            {
                int offset = rand.Next(234);

                minX = offset;
                minY = offset;
                maxX = gl_Width  - offset;
                maxY = gl_Height - offset;
            }

            mode = rand.Next(24);

#if DEBUG
            mode = 22;
#endif

            renderDelay = rand.Next(11) + 1;

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            height = 600;

            string str = $"Obj = {Type}\n\n"                        +
                            myUtils.strCountOf(list.Count, N)       +
                            $"doClearBuffer = {doClearBuffer}\n"    +
                            $"doFillShapes = {doFillShapes}\n"      +
                            $"doTraceColor = {doTraceColor}\n"      +
                            $"rotateMode = {rotateMode}\n"          +
                            $"mode = {mode}\n"                      +
                            $"offset = {minX}\n"                    +
                            $"dimRate = {myUtils.fStr(dimRate)}\n"  +
                            $"renderDelay = {renderDelay}\n"        +
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
            size = rand.Next(5) + 3;

            // Set angle / rotation
            switch (rotateMode)
            {
                case 0:
                    angle = 0;
                    dAngle = 0;
                    break;

                case 1:
                    angle = rand.Next(111) * myUtils.randFloat(rand);
                    dAngle = 0;
                    break;

                case 2:
                    angle = 0;
                    dAngle = myUtils.randFloat(rand) * myUtils.randomSign(rand) * 0.01f;
                    break;
            }

            switch (mode)
            {
                // Random direction, start at random position
                case 000:
                    {
                        x = rand.Next(gl_Width);
                        y = rand.Next(gl_Height);

                        dx = myUtils.randFloat(rand, 0.05f) * myUtils.randomSign(rand);
                        dy = myUtils.randFloat(rand, 0.05f) * myUtils.randomSign(rand);

                        A = 0.2f + myUtils.randFloat(rand) * 0.1f;
                    }
                    break;

                // Random direction, start at a central line
                case 001:
                    {
                        x = rand.Next(gl_Width);
                        y = gl_y0 + rand.Next(50) * myUtils.randomSign(rand);

                        dx = myUtils.randFloat(rand, 0.05f) * myUtils.randomSign(rand);
                        dy = myUtils.randFloat(rand, 0.05f) * myUtils.randomSign(rand);

                        A = 0.2f + myUtils.randFloat(rand) * 0.1f;
                    }
                    break;

                // Random direction, start at a top or bottom line
                case 002:
                    {
                        x = rand.Next(gl_Width);
                        y = rand.Next(2) == 0
                            ? minY + rand.Next(50) * myUtils.randomSign(rand)
                            : maxY + rand.Next(50) * myUtils.randomSign(rand);

                        dx = myUtils.randFloat(rand, 0.05f) * myUtils.randomSign(rand);
                        dy = myUtils.randFloat(rand, 0.05f) * myUtils.randomSign(rand);

                        A = 0.2f + myUtils.randFloat(rand) * 0.1f;
                    }
                    break;

                // Random direction, start at center spot
                case 003:
                    {
                        x = gl_x0 + rand.Next(50) * myUtils.randomSign(rand);
                        y = gl_y0 + rand.Next(50) * myUtils.randomSign(rand);

                        dx = myUtils.randFloat(rand, 0.05f) * myUtils.randomSign(rand);
                        dy = myUtils.randFloat(rand, 0.05f) * myUtils.randomSign(rand);

                        A = 0.2f + myUtils.randFloat(rand) * 0.1f;
                    }
                    break;

                // 45 degrees direction, start at a random position
                case 004:
                    {
                        x = rand.Next(gl_Width);
                        y = rand.Next(gl_Height);

                        switch (rand.Next(2))
                        {
                            case 0:
                                dx = myUtils.randFloat(rand, 0.05f);
                                dy = dx;
                                A = 0.25f + myUtils.randFloat(rand) * 0.1f;
                                break;

                            case 1:
                                dx = -myUtils.randFloat(rand, 0.05f);
                                dy = -dx;
                                A = 0.1f + myUtils.randFloat(rand) * 0.05f;
                                break;
                        }
                    }
                    break;

                // 45 degrees direction, start at a top line
                case 005:
                    {
                        x = rand.Next(gl_Width);
                        y = minY + rand.Next(50) * myUtils.randomSign(rand);

                        switch (rand.Next(2))
                        {
                            case 0:
                                dx = myUtils.randFloat(rand, 0.05f);
                                dy = dx;
                                A = 0.25f + myUtils.randFloat(rand) * 0.1f;
                                break;

                            case 1:
                                dx = -myUtils.randFloat(rand, 0.05f);
                                dy = -dx;
                                A = 0.1f + myUtils.randFloat(rand) * 0.05f;
                                break;
                        }
                    }
                    break;

                // Vertical and horizontal direction, start at a random position
                case 006:
                    {
                        x = rand.Next(gl_Width);
                        y = rand.Next(gl_Height);

                        switch (rand.Next(2))
                        {
                            case 0:
                                dx = myUtils.randFloat(rand, 0.05f);
                                dy = 0;
                                A = 0.25f + myUtils.randFloat(rand) * 0.1f;
                                break;

                            case 1:
                                dy = myUtils.randFloat(rand, 0.05f);
                                dx = 0;
                                A = 0.1f + myUtils.randFloat(rand) * 0.05f;
                                break;
                        }
                    }
                    break;

                // Vertical + horizontal + diagonal direction, start at a random position
                case 007:
                    {
                        x = rand.Next(gl_Width);
                        y = rand.Next(gl_Height);

                        switch (rand.Next(3))
                        {
                            case 0:
                                dx = myUtils.randFloat(rand, 0.05f);
                                dy = 0;
                                A = 0.25f + myUtils.randFloat(rand) * 0.1f;
                                break;

                            case 1:
                                dy = myUtils.randFloat(rand, 0.05f);
                                dx = 0;
                                A = 0.1f + myUtils.randFloat(rand) * 0.05f;
                                break;

                            case 2:
                                dx = -myUtils.randFloat(rand, 0.05f);
                                dy = dx;
                                A = 0.2f + myUtils.randFloat(rand) * 0.05f;
                                break;
                        }
                    }
                    break;

                // All the particles move in the same direction, but the angle constantly changes; start at a random position
                case 008:
                    {
                        x = rand.Next(gl_Width);
                        y = rand.Next(gl_Height);

                        float spd = 5;

                        dx = spd * (float)Math.Sin(sAngle);
                        dy = spd * (float)Math.Cos(sAngle);

                        sAngle += 0.001f;

                        A = 0.2f + myUtils.randFloat(rand) * 0.5f;
                    }
                    break;

                // All the particles move in the same direction + opposite direction, but the angle constantly changes; start at a random position
                case 009:
                    {
                        x = rand.Next(gl_Width);
                        y = rand.Next(gl_Height);

                        float spd = 5;

                        dx = spd * (float)Math.Sin(sAngle);
                        dy = spd * (float)Math.Cos(sAngle);

                        if (myUtils.randomChance(rand, 1, 2))
                        {
                            dx *= -1;
                            dy *= -1;
                        }

                        sAngle += 0.001f;

                        A = 0.2f + myUtils.randFloat(rand) * 0.5f;
                    }
                    break;

                // All the particles move in the same direction + opposite direction, but the angle constantly changes; start at a central spot
                case 010:
                    {
                        x = gl_x0 + rand.Next(50) * myUtils.randomSign(rand);
                        y = gl_y0 + rand.Next(50) * myUtils.randomSign(rand);

                        float spd = 5;

                        dx = spd * (float)Math.Sin(sAngle);
                        dy = spd * (float)Math.Cos(sAngle);

                        if (myUtils.randomChance(rand, 1, 2))
                        {
                            dx *= -1;
                            dy *= -1;
                        }

                        sAngle += 0.001f;

                        A = 0.2f + myUtils.randFloat(rand) * 0.5f;
                    }
                    break;

                // All the particles move in the same direction + opposite direction, but the angle constantly changes; start at a central spot
                // Added 2 more streams with slower changing angle
                case 011:
                    {
                        x = gl_x0 + rand.Next(50) * myUtils.randomSign(rand);
                        y = gl_y0 + rand.Next(50) * myUtils.randomSign(rand);

                        float spd = 5;

                        dx = spd * (float)Math.Sin(sAngle);
                        dy = spd * (float)Math.Cos(sAngle);

                        if (myUtils.randomChance(rand, 1, 2))
                        {
                            dx *= 0.5f;
                            dy *= 0.5f;
                        }

                        if (myUtils.randomChance(rand, 1, 2))
                        {
                            dx *= -1;
                            dy *= -1;
                        }

                        sAngle += 0.001f;

                        A = 0.2f + myUtils.randFloat(rand) * 0.5f;
                    }
                    break;

                // All the particles move in the same direction + opposite direction, but the angle constantly changes; start at a central spot
                // Added 2 more streams with half-angle
                case 012:
                    {
                        x = gl_x0 + rand.Next(50) * myUtils.randomSign(rand);
                        y = gl_y0 + rand.Next(50) * myUtils.randomSign(rand);

                        float spd = 5;

                        dx = spd * (float)Math.Sin(sAngle);
                        dy = spd * (float)Math.Cos(sAngle);

                        if (myUtils.randomChance(rand, 1, 2))
                        {
                            dx = spd * (float)Math.Sin(sAngle * 0.5f);
                            dy = spd * (float)Math.Cos(sAngle * 0.5f);
                        }

                        if (myUtils.randomChance(rand, 1, 2))
                        {
                            dx *= -1;
                            dy *= -1;
                        }

                        sAngle += 0.001f;

                        A = 0.2f + myUtils.randFloat(rand) * 0.5f;
                    }
                    break;

                // All the particles move in the same direction + opposite direction, but the angle constantly changes; start at a central line
                case 013:
                    {
                        x = rand.Next(gl_Width);
                        y = gl_y0 + rand.Next(50) * myUtils.randomSign(rand);

                        float spd = 5;

                        dx = spd * (float)Math.Sin(sAngle);
                        dy = spd * (float)Math.Cos(sAngle);

                        if (myUtils.randomChance(rand, 1, 2))
                        {
                            dx = spd * (float)Math.Sin(sAngle / 2);
                            dy = spd * (float)Math.Cos(sAngle / 2);
                        }

                        if (myUtils.randomChance(rand, 1, 2))
                        {
                            dx *= -1;
                            dy *= -1;
                        }

                        sAngle += 0.001f;

                        A = 0.2f + myUtils.randFloat(rand) * 0.5f;
                    }
                    break;

                // All the particles move in the same direction + opposite direction, but the angle constantly changes;
                // Start at a central spot
                // Increased angle change speed
                // Angle randomized to get triangular distribution
                case 014:
                    {
                        x = gl_x0 + rand.Next(11) * myUtils.randomSign(rand);
                        y = gl_y0 + rand.Next(11) * myUtils.randomSign(rand);

                        float spd = 5;

                        dx = spd * (float)Math.Sin(sAngle + myUtils.randFloat(rand) * 0.25f);
                        dy = spd * (float)Math.Cos(sAngle + myUtils.randFloat(rand) * 0.25f);

                        if (myUtils.randomChance(rand, 1, 2))
                        {
                            dx *= -1;
                            dy *= -1;
                        }

                        sAngle += 0.003f;

                        A = 0.2f + myUtils.randFloat(rand) * 0.5f;
                    }
                    break;

                // All the particles move in the same direction + opposite direction, but the angle constantly changes;
                // Start at a central spot
                // Increased angle change speed
                // Angle randomized to get triangular distribution
                // Added 1/2 chance to use a negative angle
                case 015:
                    {
                        x = gl_x0 + rand.Next(11) * myUtils.randomSign(rand);
                        y = gl_y0 + rand.Next(11) * myUtils.randomSign(rand);

                        float spd = 5;

                        if (myUtils.randomChance(rand, 1, 2))
                        {
                            dx = spd * (float)Math.Sin(sAngle + myUtils.randFloat(rand) * 0.25f);
                            dy = spd * (float)Math.Cos(sAngle + myUtils.randFloat(rand) * 0.25f);
                        }
                        else
                        {
                            dx = spd * (float)Math.Sin(-sAngle - myUtils.randFloat(rand) * 0.25f);
                            dy = spd * (float)Math.Cos(-sAngle - myUtils.randFloat(rand) * 0.25f);
                        }

                        if (myUtils.randomChance(rand, 1, 2))
                        {
                            dx *= -1;
                            dy *= -1;
                        }

                        sAngle += 0.003f;

                        A = 0.2f + myUtils.randFloat(rand) * 0.5f;
                    }
                    break;

                // 45 degrees criss-cross movement; start position is random
                case 016:
                    {
                        x = rand.Next(gl_Width);
                        y = rand.Next(gl_Height);

                        dx = myUtils.randFloat(rand) + 0.1f;
                        dy = dx;

                        A = 0.2f + myUtils.randFloat(rand) * 0.5f;

                        if (myUtils.randomChance(rand, 1, 2))
                        {
                            dy *= -1;
                            A /= 2;
                        }

                        if (myUtils.randomChance(rand, 1, 2))
                        {
                            dx *= -1;
                            dy *= -1;
                        }

                        if (myUtils.randomChance(rand, 1, 10000))
                        {
                            step = rand.Next(333) + 1;
                        }
                    }
                    break;

                // 45 degrees criss-cross movement; start position is random, but aligned to a grid
                case 017:
                    {
                        x = rand.Next(gl_Width);
                        y = rand.Next(gl_Height);

                        x -= x % step;
                        y -= y % step;

                        dx = myUtils.randFloat(rand) + 0.1f;
                        dy = dx;

                        A = 0.2f + myUtils.randFloat(rand) * 0.5f;

                        if (myUtils.randomChance(rand, 1, 2))
                        {
                            dy *= -1;
                            A /= 2;
                        }

                        if (myUtils.randomChance(rand, 1, 2))
                        {
                            dx *= -1;
                            dy *= -1;
                        }

                        if (myUtils.randomChance(rand, 1, 10000))
                        {
                            step = rand.Next(333) + 1;
                        }
                    }
                    break;

                // 45 degrees criss-cross movement; start position is random;
                // The points that get into the grid receive higher opacity
                case 018:
                    {
                        x = rand.Next(gl_Width);
                        y = rand.Next(gl_Height);

                        dx = myUtils.randFloat(rand) + 0.1f;
                        dy = dx;

                        A = 0.1f + myUtils.randFloat(rand) * 0.25f;

                        if (x == x % step)
                        {
                            A += 0.2f;
                        }

                        if (y == y % step)
                        {
                            A += 0.2f;
                        }

                        if (myUtils.randomChance(rand, 1, 2))
                        {
                            dy *= -1;
                            A /= 2;
                        }

                        if (myUtils.randomChance(rand, 1, 2))
                        {
                            dx *= -1;
                            dy *= -1;
                        }

                        if (myUtils.randomChance(rand, 1, 10000))
                        {
                            step = rand.Next(333) + 1;
                        }
                    }
                    break;

                // Circular (spiral) motion
                case 019:
                    {
                        x = rand.Next(gl_Width);
                        y = rand.Next(gl_Height);

                        rad = rand.Next(333) + 50;

                        t = myUtils.randFloat(rand) * rand.Next(111);
                        dt = myUtils.randFloat(rand) * 0.05f;

                        dx = dy = 0;

                        A = 0.1f + myUtils.randFloat(rand) * 0.25f;
                    }
                    break;

                // Horizontal, vertical and diagonal movement at a fixed speed, ver1
                case 020:
                    {
                        x = rand.Next(gl_Width);
                        y = rand.Next(gl_Height);

                        A = 0.2f + myUtils.randFloat(rand) * 0.5f;

                        switch (rand.Next(3))
                        {
                            case 0:
                                dx = 0;
                                dy = 0.5f;
                                break;

                            case 1:
                                dy = 0;
                                dx = 0.5f;
                                break;

                            case 2:
                                dx = -0.33f;
                                dy = -0.33f;
                                break;
                        }
                    }
                    break;

                // Horizontal, vertical and diagonal movement at a fixed speed, ver2
                case 021:
                    {
                        x = rand.Next(gl_Width);
                        y = rand.Next(gl_Height);

                        x -= x % step;
                        y -= y % step;

                        A = 0.2f + myUtils.randFloat(rand) * 0.5f;

                        switch (rand.Next(3))
                        {
                            case 0:
                                dx = 0;
                                dy = 0.5f;
                                break;

                            case 1:
                                dy = 0;
                                dx = 0.5f;
                                break;

                            case 2:
                                dx = -0.33f;
                                dy = -0.33f;
                                break;
                        }
                    }
                    break;

                // Oscillating movement along random line
                case 022:
                    {
                        doUseBaseMove = false;

                        x = rand.Next(gl_Width);
                        y = rand.Next(gl_Height);

                        A = 0.2f + myUtils.randFloat(rand) * 0.5f;

                        dx = (myUtils.randFloat(rand) + 0.1f) * myUtils.randomSign(rand);
                        dy = (myUtils.randFloat(rand) + 0.1f) * myUtils.randomSign(rand);

                        dt = myUtils.randFloat(rand) * 0.05f;
                    }
                    break;

                // Oscillating movement along random line;
                // Attached to grid
                case 023:
                    {
                        doUseBaseMove = false;

                        x = rand.Next(gl_Width);
                        y = rand.Next(gl_Height);

                        x -= x % step;
                        y -= y % step;

                        A = 0.2f + myUtils.randFloat(rand) * 0.5f;

                        dx = (myUtils.randFloat(rand) + 0.1f) * myUtils.randomSign(rand);
                        dy = (myUtils.randFloat(rand) + 0.1f) * myUtils.randomSign(rand);

                        dt = myUtils.randFloat(rand) * 0.05f;
                    }
                    break;

                // ======================================

                case 099:
                    {
                    }
                    break;
            }

            colorPicker.getColor(x, y, ref R, ref G, ref B);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            // Special cases
            {
                if (mode == 019)
                    move_019();

                if (mode == 022 || mode == 023)
                    move_022();
            }

            if (doUseBaseMove)
            {
                x += dx;
                y += dy;
            }

            angle += dAngle;

            A -= dimRate;

            if (doTraceColor && rand.Next(3) == 0)
            {
                colorPicker.getColor(x, y, ref R, ref G, ref B);
            }

            if (x < minX || x > maxX || y < minY || y > maxY)
            {
                A -= 0.005f;

                if (A < 0)
                {
                    generateNew();
                }
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void move_019()
        {
            x += (float)Math.Sin(t) * rad * 0.01f;
            y += (float)Math.Cos(t) * rad * 0.01f;

            t += dt;

            rad -= 1;

            if (rad <= 0)
                generateNew();
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void move_022()
        {
            x += dx * (float)Math.Sin(t);
            y += dy * (float)Math.Sin(t);

            t += dt;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Show()
        {
            float size2x = size * 2;

            switch (shape)
            {
                // Instanced squares
                case 0:
                    myPrimitive._RectangleInst.setInstanceCoords(x - size, y - size, size2x, size2x);
                    myPrimitive._RectangleInst.setInstanceColor(R, G, B, A);
                    myPrimitive._RectangleInst.setInstanceAngle(angle);
                    break;

                // Instanced triangles
                case 1:
                    myPrimitive._TriangleInst.setInstanceCoords(x, y, size, angle);
                    myPrimitive._TriangleInst.setInstanceColor(R, G, B, A);
                    break;

                // Instanced circles
                case 2:
                    myPrimitive._EllipseInst.setInstanceCoords(x, y, size2x, angle);
                    myPrimitive._EllipseInst.setInstanceColor(R, G, B, A);
                    break;

                // Instanced pentagons
                case 3:
                    myPrimitive._PentagonInst.setInstanceCoords(x, y, size2x, angle);
                    myPrimitive._PentagonInst.setInstanceColor(R, G, B, A);
                    break;

                // Instanced hexagons
                case 4:
                    myPrimitive._HexagonInst.setInstanceCoords(x, y, size2x, angle);
                    myPrimitive._HexagonInst.setInstanceColor(R, G, B, A);
                    break;
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Process(Window window)
        {
            uint cnt = 0;
            initShapes();

            clearScreenSetup(doClearBuffer, 0.1f);

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
                    }

                    grad.Draw();
                }

                // Render Frame
                {
                    inst.ResetBuffer();

                    for (int i = 0; i != Count; i++)
                    {
                        var obj = list[i] as myObj_0015;

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

                if (Count < N)
                {
                    list.Add(new myObj_0015());
                }

                cnt++;
                System.Threading.Thread.Sleep(renderDelay);
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void initShapes()
        {
            base.initShapes(shape, N, 0);

            grad = new myScreenGradient();
            grad.SetRandomColors(rand, 0.23f);

            if (doClearBuffer == false)
            {
                grad.SetOpacity(0.05f);
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

    }
};
