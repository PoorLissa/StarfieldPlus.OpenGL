using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;


/*
    - The image is split into big number of particles that fall down (or move somehow)
*/


namespace my
{
    public class myObj_370 : myObject
    {
        private int cnt;
        private float x, y, x0, y0, dx, dy;
        private float size, A, R, G, B, angle = 0, dAngle = 0, t, dt;

        private static int N = 0, shape = 0, mode = 0, sizeMode = 0, angleMode = 0, accMode = 0, cntMode = 0, idleMode = 0, localMode = 0,
                           maxCnt = 0, maxSize = 1, dxFactor = 1, dyFactor = 1, xWidth = 1, yWidth = 1;
        private static bool doFillShapes = false;
        private static float dimAlpha = 0.05f, accRate = 0, dA = 0;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_370()
        {
            cnt = 0;
            generateNew();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time global initialization
        protected override void initGlobal()
        {
            int clrMode = myUtils.randomChance(rand, 1, 11) ? -1 : (int)myColorPicker.colorMode.SNAPSHOT_OR_IMAGE;

            colorPicker = new myColorPicker(gl_Width, gl_Height, mode: clrMode);
            list = new List<myObject>();

            // Global unmutable constants
            {
                N = 300000 + rand.Next(100000);
                shape = rand.Next(6);
            }

            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time local initialization
        private void initLocal()
        {
            doClearBuffer = myUtils.randomBool(rand);
            doFillShapes  = myUtils.randomBool(rand);

            mode        = rand.Next(13);                        // How the dx / dy are calculated
            sizeMode    = rand.Next(7);                         // How the size of particles is generated
            angleMode   = rand.Next(7);                         // How the particles are rotated
            maxSize     = myUtils.randomChance(rand, 1, 2)
                                ? rand.Next(07) + 1
                                : rand.Next(11) + 1;
            accMode     = rand.Next(3);
            idleMode    = rand.Next(3);                         // What particles do while waiting
            cntMode     = rand.Next(3);                         // The wait counter behaviour
            maxCnt      = rand.Next(222) + 111;
            renderDelay = rand.Next(11);
            dxFactor    = rand.Next(10) + 1;
            dyFactor    = myUtils.randomChance(rand, 1, 3)
                                ? rand.Next(11) + 1
                                : rand.Next(33) + 1;

            accRate = myUtils.randFloat(rand) * 0.01f;
            dA = myUtils.randFloat(rand) * 0.5f;


            // Adjust static parameters depending on the current mode:
            switch (mode)
            {
                case 004:
                    dyFactor = rand.Next(5) + 1;
                    break;

                case 005:
                    dyFactor = -1;
                    break;

                case 007:
                    localMode = rand.Next(10);                  // Color dependency
                    break;

                case 008:
                    localMode = rand.Next(18);                  // Color dependency
                    break;

                case 009:
                    localMode = rand.Next(6);
                    xWidth = 33 + rand.Next(222);
                    yWidth = 33 + rand.Next(222);
                    break;

                case 010:
                    localMode = rand.Next(2);
                    break;

                case 011:
                    localMode = rand.Next(4);                   // Rotation speed
                    dxFactor = rand.Next(2);                    // Const vs Increasing radius
                    dyFactor = rand.Next(7) + 1;                // Const radius factor
                    dA = myUtils.randFloat(rand) * 0.025f;
                    break;

                case 012:
                    dxFactor = rand.Next(5) + 1;                // rand move factor for x
                    dyFactor = rand.Next(5) + 1;                // rand move factor for y
                    break;
            }

            switch (shape)
            {
                case 5:
                    idleMode = rand.Next(2) + 1;
                    break;
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            height = 800;

            string nStr(int   n) { return n.ToString("N0");    }
            string fStr(float f) { return f.ToString("0.000"); }

            string str = $"Obj = myObj_370\n\n"                      +
                            $"N = {nStr(list.Count)} of {nStr(N)}\n" +
                            $"doClearBuffer = {doClearBuffer}\n"     +
                            $"mode = {mode}\n"                       +
                            $"sizeMode = {sizeMode}\n"               +
                            $"shape = {shape}\n"                     +
                            $"angleMode = {angleMode}\n"             +
                            $"accelerationMode = {accMode}\n"        +
                            $"accelerationRate = {fStr(accRate)}\n"  +
                            $"cntMode = {cntMode}\n"                 +
                            $"maxSize = {maxSize}\n"                 +
                            $"maxCnt = {maxCnt}\n"                   +
                            $"dxFactor = {dxFactor}\n"               +
                            $"dyFactor = {dyFactor}\n"               +
                            $"dA = {fStr(dA)}\n"                     +
                            $"localMode = {localMode}\n"             +
                            $"renderDelay = {renderDelay}\n"         +
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
            // Time to wait until the particle starts falling
            cnt = (cnt == 0) ? 123 : rand.Next(33) + 1;

            x = x0 = rand.Next(gl_Width);
            y = y0 = rand.Next(gl_Height + 123) - 123;

            colorPicker.getColor(x, y, ref R, ref G, ref B);

            switch (sizeMode)
            {
                case 0: case 1: case 2:
                    size = myUtils.randFloat(rand, 0.02f) * (rand.Next(maxSize) + 1);
                    break;

                case 3:
                    size = (R + G + B) * 1;
                    break;

                case 4:
                    size = (R + G + B) * 2;
                    break;

                case 5:
                    size = 12.0f / (R + G + B + 3.0f);
                    break;

                case 6:
                    size = 6.0f / (R + G + B + 1.0f);
                    break;
            }

            switch (mode)
            {
                // Falling down: dy is random
                case 000:
                case 001:
                    {
                        dx = myUtils.randFloat(rand, 0.05f) * myUtils.randomSign(rand) / dxFactor;
                        dy = myUtils.randFloat(rand, 0.05f) * (rand.Next(dyFactor) + 1);

                        A = 0.1f + 2 * dy / dyFactor;
                    }
                    break;

                // Falling down: dy is a function of particle's color (opacity ver.1)
                case 002:
                    {
                        dx = myUtils.randFloat(rand, 0.05f) * myUtils.randomSign(rand) / dxFactor;
                        dy = myUtils.randFloat(rand, 0.05f) * (rand.Next(dyFactor) + 1);

                        A = 0.1f + 2 * dy / dyFactor;
                        dy = (R + G + B) * dyFactor * 0.5f;
                    }
                    break;

                // Falling down: dy is a function of particle's color (opacity ver.2)
                case 003:
                    {
                        dx = myUtils.randFloat(rand, 0.05f) * myUtils.randomSign(rand) / dxFactor;
                        dy = (R + G + B) * dyFactor * 0.5f;

                        A = 0.1f + 2 * dy / dyFactor;
                    }
                    break;

                // Moving away: dx / dy are random
                case 004:
                case 005:
                    {
                        float factor = dyFactor > 0
                            ? dyFactor
                            : rand.Next(3) + 1;

                        dx = myUtils.randFloat(rand, 0.05f) * myUtils.randomSign(rand) * factor;
                        dy = myUtils.randFloat(rand, 0.05f) * myUtils.randomSign(rand) * factor;

                        A = 0.1f + (float)(Math.Abs(dx) + Math.Abs(dy)) / factor;
                    }
                    break;

                // Only vertical or horizontal movement
                case 006:
                    {
                        if (myUtils.randomBool(rand))
                        {
                            dx = myUtils.randFloat(rand, 0.05f) * myUtils.randomSign(rand) * dyFactor;
                            dy = 0;
                            A = 0.1f + (float)Math.Abs(2 * dx) / dyFactor;
                        }
                        else
                        {
                            dx = 0;
                            dy = myUtils.randFloat(rand, 0.05f) * myUtils.randomSign(rand) * dyFactor;
                            A = 0.1f + (float)Math.Abs(2 * dy) / dyFactor;
                        }
                    }
                    break;

                // Vertical movement: the particles move up or down, depending on the color/opacity
                case 007:
                    {
                        dx = myUtils.randFloat(rand, 0.05f) * myUtils.randomSign(rand) / dxFactor;
                        dy = myUtils.randFloat(rand, 0.05f) * (rand.Next(dyFactor) + 1);

                        A = 0.1f + 2 * dy / dyFactor;

                        switch (localMode)
                        {
                            case 0: case 1:
                                if (R + G + B < 1.0f)
                                    dy *= -1;
                                break;

                            case 2: case 3:
                                if (A < 0.3f)
                                    dy *= -1;
                                break;

                            case 4:
                                if (R < 0.5f)
                                    dy *= -1;
                                break;

                            case 5:
                                if (G < 0.5f)
                                    dy *= -1;
                                break;

                            case 6:
                                if (B < 0.5f)
                                    dy *= -1;
                                break;

                            case 7:
                                if (R + G < 1.0f)
                                    dy *= -1;
                                break;

                            case 8:
                                if (R + B < 1.0f)
                                    dy *= -1;
                                break;

                            case 9:
                                if (G + B < 1.0f)
                                    dy *= -1;
                                break;
                        }
                    }
                    break;

                // Color depenent movement
                case 008:
                    {
                        switch (localMode)
                        {
                            case 0: case 1:
                                dx = (0.5f - R) * dyFactor * 2;
                                dy = (0.5f - G) * dyFactor * 2;

                                if (localMode == 1)
                                    myUtils.swap<float>(ref dx, ref dy);
                                break;

                            case 2: case 3:
                                dx = (0.5f - R) * dyFactor * 2;
                                dy = (0.5f - B) * dyFactor * 2;

                                if (localMode == 3)
                                    myUtils.swap<float>(ref dx, ref dy);
                                break;

                            case 4: case 5:
                                dx = (0.5f - G) * dyFactor * 2;
                                dy = (0.5f - B) * dyFactor * 2;

                                if (localMode == 5)
                                    myUtils.swap<float>(ref dx, ref dy);
                                break;

                            case 6: case 7:
                                dx = (R - G) * dyFactor * 2;
                                dy = (R - B) * dyFactor * 2;

                                if (localMode == 7)
                                    myUtils.swap<float>(ref dx, ref dy);
                                break;

                            case 8: case 9:
                                dx = (G - R) * dyFactor * 2;
                                dy = (G - B) * dyFactor * 2;

                                if (localMode == 9)
                                    myUtils.swap<float>(ref dx, ref dy);
                                break;

                            case 10: case 11:
                                dx = (B - R) * dyFactor * 2;
                                dy = (B - G) * dyFactor * 2;

                                if (localMode == 11)
                                    myUtils.swap<float>(ref dx, ref dy);
                                break;

                            case 12: case 13:
                                dx = (R*G - B) * dyFactor * 2;
                                dy = (R*B - G) * dyFactor * 2;

                                if (localMode == 13)
                                    myUtils.swap<float>(ref dx, ref dy);
                                break;

                            case 14: case 15:
                                dx = (G*B - R) * dyFactor * 2;
                                dy = (G*R - B) * dyFactor * 2;

                                if (localMode == 15)
                                    myUtils.swap<float>(ref dx, ref dy);
                                break;

                            case 16: case 17:
                                dx = (B*R - G) * dyFactor * 2;
                                dy = (B*G - R) * dyFactor * 2;

                                if (localMode == 17)
                                    myUtils.swap<float>(ref dx, ref dy);
                                break;
                        }

                        A = 0.1f + (float)(Math.Abs(dx) + Math.Abs(dy)) / dyFactor;
                    }
                    break;

                // dx/dy depend on sin x/y
                case 009:
                    {
                        switch (localMode)
                        {
                            case 0:
                                dx = myUtils.randFloat(rand, 0.05f) * myUtils.randomSign(rand) / dxFactor;
                                dy = (float)Math.Sin(x / xWidth) * dyFactor;
                                A = 0.1f + (float)(2 * Math.Abs(dy)) / dyFactor;
                                break;

                            case 1:
                                dy = myUtils.randFloat(rand, 0.05f) * myUtils.randomSign(rand) / dxFactor;
                                dx = (float)Math.Sin(y / yWidth) * dyFactor;
                                A = 0.1f + (float)(2 * Math.Abs(dx)) / dyFactor;
                                break;

                            case 2:
                                if (myUtils.randomChance(rand, 1, 2))
                                    goto case 0;
                                else
                                    goto case 1;
                                break;

                            case 3:
                                dx = myUtils.randFloat(rand, 0.05f) * myUtils.randomSign(rand) / dxFactor;
                                dy = (float)Math.Sin(y / yWidth) * dyFactor;
                                A = 0.1f + (float)(2 * Math.Abs(dy)) / dyFactor;
                                break;

                            case 4:
                                dy = myUtils.randFloat(rand, 0.05f) * myUtils.randomSign(rand) / dxFactor;
                                dx = (float)Math.Sin(x / xWidth) * dyFactor;
                                A = 0.1f + (float)(2 * Math.Abs(dx)) / dyFactor;
                                break;

                            case 5:
                                if (myUtils.randomChance(rand, 1, 2))
                                    goto case 3;
                                else
                                    goto case 4;
                                break;
                        }
                    }
                    break;

                // Particles from the upper and lower halfs are moving in opposite directions
                case 010:
                    {
                        dx = myUtils.randFloat(rand, 0.05f) * myUtils.randomSign(rand) / dxFactor;
                        dy = (float)Math.Sin((y - gl_y0)/gl_y0) * (dyFactor / 3 + 1);

                        dy *= localMode == 0 ? 1 : -1;

                        A = 0.2f + (float)(2 * Math.Abs(dy)) / (dyFactor / 3 + 1);
                    }
                    break;

                // Rotation around the initial position
                case 011:
                    {
                        t = rand.Next(1234);

                        switch (localMode)
                        {
                            case 0:
                                dt = 0.1f;
                                break;

                            case 1:
                                dt = dA * 10;
                                break;

                            case 2:
                                dt = myUtils.randFloat(rand) * 0.5f;
                                break;

                            case 3:
                                dt = myUtils.randFloat(rand) * 0.05f;
                                break;
                        }

                        // In half of the cases, direction of rotaion is random
                        if (maxCnt % 2 == 0)
                        {
                            dt *= myUtils.randomSign(rand);
                        }

                        // Set up the radius of rotation (dx) and it's changing speed (dy)
                        if (dxFactor == 0)
                        {
                            dx = size * dyFactor;
                            dy = 0;
                        }
                        else
                        {
                            dx = myUtils.randFloat(rand) * rand.Next(5);
                            dy = myUtils.randFloat(rand, 0.25f) * 0.45f;
                        }

                        A = 0.2f + myUtils.randFloat(rand, 0.2f);
                    }
                    break;

                // Random movement around the initial position
                case 012:
                    {
                        dx = dy = 0;
                        A = 0.2f + myUtils.randFloat(rand, 0.2f);
                    }
                    break;
            }

            // Adjust the wait time
            {
                switch (cntMode)
                {
                    // Random value
                    case 1:
                        cnt += rand.Next(maxCnt);
                        break;

                    // Depending on the color
                    case 2:
                        cnt += rand.Next((maxCnt + 333) / (int)((R + G + B + 0.01f) * 100));
                        break;
                }
            }

            // Rotation mode
            {
                dAngle = 0;

                switch (angleMode)
                {
                    case 1:
                        angle = myUtils.randFloat(rand) * rand.Next(1234);
                        break;

                    case 2:
                        dAngle = myUtils.randFloat(rand) * 0.1f * myUtils.randomSign(rand);
                        break;

                    case 3:
                        angle = myUtils.randFloat(rand) * rand.Next(1234);
                        dAngle = myUtils.randFloat(rand) * 0.1f * myUtils.randomSign(rand);
                        break;
                }
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            if (cnt > 1)
            {
                cnt--;

                switch (idleMode)
                {
                    case 0:
                        break;

                    case 1:
                        if (myUtils.randomChance(rand, 1, 7))
                        {
                            x += myUtils.randomSign(rand) * myUtils.randFloat(rand) * 0.66f;
                            y += myUtils.randomSign(rand) * myUtils.randFloat(rand) * 0.66f;
                        }
                        break;

                    case 2:
                        if (myUtils.randomChance(rand, 1, 7))
                        {
                            x += myUtils.randomSign(rand) * dx * 0.2f;
                            y += myUtils.randomSign(rand) * dy * 0.2f;
                        }
                        break;
                }
            }
            else
            {
                x += dx;
                y += dy;
                A -= dA;
                angle += dAngle;

                // Acceleration
                switch (accMode)
                {
                    case 1: y += accRate;        break;
                    case 2: y += accRate * size; break;
                }

                switch (mode)
                {
                    case 011:
                        x = x0 + (float)Math.Sin(t) * dx;       // dx here is a rotation raduis
                        y = y0 + (float)Math.Cos(t) * dx;
                        t += dt;
                        dx += dy;                               // increase the radius gradually
                        break;

                    case 012:
                        dx += myUtils.randomSign(rand) * myUtils.randFloat(rand) * dxFactor;
                        dy += myUtils.randomSign(rand) * myUtils.randFloat(rand) * dyFactor;
                        break;
                }

                if (y > gl_Height || A <= 0)
                {
                    generateNew();
                }
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
                    var rectInst = inst as myRectangleInst;

                    rectInst.setInstanceCoords(x - size, y - size, size2x, size2x);
                    rectInst.setInstanceColor(R, G, B, A);
                    rectInst.setInstanceAngle(angle);
                    break;

                // Instanced triangles
                case 1:
                    var triangleInst = inst as myTriangleInst;

                    triangleInst.setInstanceCoords(x, y, size2x, angle);
                    triangleInst.setInstanceColor(R, G, B, A);
                    break;

                // Instanced circles
                case 2:
                    var ellipseInst = inst as myEllipseInst;

                    ellipseInst.setInstanceCoords(x, y, size2x, angle);
                    ellipseInst.setInstanceColor(R, G, B, A);
                    break;

                // Instanced pentagons
                case 3:
                    var pentagonInst = inst as myPentagonInst;

                    pentagonInst.setInstanceCoords(x, y, size2x, angle);
                    pentagonInst.setInstanceColor(R, G, B, A);
                    break;

                // Instanced hexagons
                case 4:
                    var hexagonInst = inst as myHexagonInst;

                    hexagonInst.setInstanceCoords(x, y, size2x, angle);
                    hexagonInst.setInstanceColor(R, G, B, A);
                    break;

                // Instanced lines
                case 5:
                    myPrimitive._LineInst.setInstanceCoords(x, y, x0, y0);
                    myPrimitive._LineInst.setInstanceColor(R, G, B, A);
                    break;
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Process(Window window)
        {
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
                dimScreenRGB_SetRandom(0.1f);
                //glDrawBuffer(GL_FRONT_AND_BACK);
                glDrawBuffer(GL_DEPTH_BUFFER_BIT);
            }

            while (list.Count < N)
            {
                list.Add(new myObj_370());
            }

            if (shape != 5)
            {
                while (!Glfw.WindowShouldClose(window))
                {
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
                        else
                        {
                            dimScreen(dimAlpha);
                        }
                    }

                    // Render Frame
                    {
                        inst.ResetBuffer();

                        for (int i = 0; i != list.Count; i++)
                        {
                            var obj = list[i] as myObj_370;

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

                    System.Threading.Thread.Sleep(renderDelay);
                }
            }
            else
            {
                while (!Glfw.WindowShouldClose(window))
                {
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
                        else
                        {
                            dimScreen(dimAlpha);
                        }
                    }

                    // Render Frame
                    {
                        myPrimitive._LineInst.ResetBuffer();

                        for (int i = 0; i != list.Count; i++)
                        {
                            var obj = list[i] as myObj_370;

                            obj.Show();
                            obj.Move();
                        }

                        myPrimitive._LineInst.Draw();
                    }

                    System.Threading.Thread.Sleep(renderDelay);
                }
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void initShapes()
        {
            myPrimitive.init_ScrDimmer();

            if (shape != 5)
            {
                base.initShapes(shape, N, 0);
            }
            else
            {
                myPrimitive.init_LineInst(N);
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------
    }
};
