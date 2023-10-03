using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;


/*
    - Particles move radially from the off-center position, creating a vortex-like structure
*/


namespace my
{
    public class myObj_390 : myObject
    {
        // Priority
        public static int Priority => 10;
		public static System.Type Type => typeof(myObj_390);

        private int x0, y0;
        private float x, y, dx, dy;
        private float size, A, R, G, B, angle = 0, da, spdFactor;

        private static int N = 0, shape = 0, angleMode = 0, colorMode = 0, genMode = 0, connectMode = 0, sizeMode = 0, daMode = 0;
        private static int spdX = 0, spdY = 0, wh2 = 0;
        private static bool doFillShapes = false, doCreateAtOnce = false, doUseRandSpdFactor = true, doConnect = true;
        private static float dimAlpha = 0.05f, alphaStatic = 0, dAlphaStatic = 0;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_390()
        {
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
                doCreateAtOnce = myUtils.randomBool(rand);
                doConnect      = myUtils.randomBool(rand);

                N = 33333;
                shape = rand.Next(5);

                wh2 = (gl_Width - gl_Height) / 2;
            }

            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time local initialization
        private void initLocal()
        {
            doClearBuffer      = myUtils.randomBool(rand);
            doFillShapes       = myUtils.randomBool(rand);
            doUseRandSpdFactor = myUtils.randomBool(rand);

            renderDelay = 10;
            dimAlpha = 0.075f;

            dAlphaStatic = 0.01f * (rand.Next(5) + 1);                              // Static angle changing rate

            genMode = rand.Next(14);                                                // Particle position generation mode
            colorMode = rand.Next(2);
            connectMode = rand.Next(2);
            sizeMode = rand.Next(2);
            daMode = rand.Next(4);

            // Speed factor
            {
                int maxSpd = 15;

                spdX = rand.Next(maxSpd) + 1;
                spdY = rand.Next(maxSpd) + 1;

                if (myUtils.randomChance(rand, 4, 5))
                    spdY = spdX;
            }

            // Angle mode
            {
                // Base mode is 3...
                angleMode = 3;

                // ... but sometimes we allow the mode to be different
                if (myUtils.randomChance(rand, 1, 5))
                {
                    angleMode = rand.Next(4);
                }

                switch (angleMode)
                {
                    case 0:
                    case 1:
                        alphaStatic = myUtils.randFloat(rand) * (rand.Next(123) + 1);
                        break;

                    case 2:
                        break;

                    case 3:
                        alphaStatic = 0;
                        break;
                }
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            height = 600;

            string nStr(int   n) { return n.ToString("N0");    }
            string fStr(float f) { return f.ToString("0.000"); }

            string str = $"Obj = {Type}\n\n"                             	+
                            $"N = {nStr(list.Count)} of {nStr(N)}\n"        +
                            $"shape = {shape}\n"                            +
                            $"genMode = {genMode}\n"                        +
                            $"angleMode = {angleMode}\n"                    +
                            $"colorMode = {colorMode}\n"                    +
                            $"connectMode = {connectMode}\n"                +
                            $"doClearBuffer = {doClearBuffer}\n"            +
                            $"doFillShapes = {doFillShapes}\n"              +
                            $"doCreateAtOnce = {doCreateAtOnce}\n"          +
                            $"doUseRandSpdFactor = {doUseRandSpdFactor}\n"  +
                            $"doConnect = {doConnect}\n"                    +
                            $"spdX = {spdX}; spdY = {spdY}\n"               +
                            $"dAlphaStatic = {fStr(dAlphaStatic)}\n"        +
                            $"dimAlpha = {fStr(dimAlpha)}\n"                +
                            $"renderDelay = {renderDelay}\n"                +
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
            double dist = 0;

            spdFactor = doUseRandSpdFactor
                ? 0.25f * (rand.Next(30) + 1)
                : 1.0f;

            switch (genMode)
            {
                case 0:
                    x = rand.Next(gl_Width);
                    y = rand.Next(gl_Width);

                    dist = Math.Sqrt((x - gl_x0) * (x - gl_x0) + (y - gl_x0) * (y - gl_x0));
                    break;

                // Circle
                case 1: case 2: case 3:
                    do
                    {
                        x = rand.Next(gl_Width);
                        y = rand.Next(gl_Width);

                        dist = Math.Sqrt((x - gl_x0) * (x - gl_x0) + (y - gl_x0) * (y - gl_x0));
                    }
                    while (dist < 300);
                    break;

                // Ring
                case 4: case 5: case 6:
                    do
                    {
                        x = rand.Next(gl_Width);
                        y = rand.Next(gl_Width);

                        dist = Math.Sqrt((x - gl_x0) * (x - gl_x0) + (y - gl_x0) * (y - gl_x0));
                    }
                    while (dist < 200 || dist > 500);
                    break;

                // Line
                case 7:
                    x = gl_x0 + rand.Next(1001) - 500;
                    y = gl_x0;

                    dist = Math.Sqrt((x - gl_x0) * (x - gl_x0) + (y - gl_x0) * (y - gl_x0));
                    break;

                // Cross
                case 8: case 9:
                    switch (rand.Next(2))
                    {
                        case 0:
                            x = gl_x0 + rand.Next(1001) - 500;
                            y = gl_x0;
                            break;

                        case 1:
                            x = gl_x0;
                            y = gl_x0 + rand.Next(1001) - 500;
                            break;
                    }

                    dist = Math.Sqrt((x - gl_x0) * (x - gl_x0) + (y - gl_x0) * (y - gl_x0));
                    break;

                // Square
                case 10: case 11:
                    x = gl_x0 + rand.Next(1001) - 500;
                    y = gl_x0 + rand.Next(1001) - 500;

                    dist = Math.Sqrt((x - gl_x0) * (x - gl_x0) + (y - gl_x0) * (y - gl_x0));
                    break;

                // 4 lines making a rectangle
                case 12: case 13:
                    switch (rand.Next(4))
                    {
                        case 0:
                            x = 500;
                            y = rand.Next(gl_Width);
                            break;

                        case 1:
                            x = gl_Width - 500;
                            y = rand.Next(gl_Width);
                            break;

                        case 2:
                            x = rand.Next(gl_Width);
                            y = 500 + wh2;
                            break;

                        case 3:
                            x = rand.Next(gl_Width);
                            y = gl_Width - wh2 - 500;
                            break;
                    }

                    dist = Math.Sqrt((x - gl_x0) * (x - gl_x0) + (y - gl_x0) * (y - gl_x0));
                    break;
            }

            // Get current angle alpha:
            double currentAngle = Math.Acos((y - gl_x0) / dist);

            switch (angleMode)
            {
                // Const static angle for every particle
                case 0:
                    {
                        if (x > gl_x0)
                        {
                            dx = (float)(+Math.Sin(alphaStatic) * spdX);
                            dy = (float)(+Math.Cos(alphaStatic) * spdY);
                        }
                        else
                        {
                            dx = (float)(-Math.Sin(alphaStatic) * spdX);
                            dy = (float)(+Math.Cos(alphaStatic) * spdY);
                        }
                    }
                    break;

                // Const static angle for every particle
                case 1:
                    {
                        if (x > gl_x0)
                        {
                            currentAngle += alphaStatic;

                            dx = (float)(Math.Sin(currentAngle) * spdX);
                            dy = (float)(Math.Cos(currentAngle) * spdY);
                        }
                        else
                        {
                            currentAngle -= alphaStatic;

                            // Optimized:
                            dx = (float)(-Math.Sin(currentAngle) * spdX);
                            dy = (float)(+Math.Cos(currentAngle) * spdY);
                        }
                    }
                    break;

                // Random angle for each particle
                case 2:
                    {
                        float angle = myUtils.randFloat(rand);

                        dx = (float)(Math.Sin(angle) * spdX) * myUtils.randomSign(rand);
                        dy = (float)(Math.Cos(angle) * spdY) * myUtils.randomSign(rand);
                    }
                    break;
                    
                // Every particle's gets rotated by the same angle
                case 3:
                    {
                        if (x > gl_x0)
                        {
                            currentAngle += alphaStatic;

                            dx = (float)(Math.Sin(currentAngle) * spdX * spdFactor);
                            dy = (float)(Math.Cos(currentAngle) * spdY * spdFactor);
                        }
                        else
                        {
                            currentAngle -= alphaStatic;

                            dx = (float)(-Math.Sin(currentAngle) * spdX * spdFactor);
                            dy = (float)(+Math.Cos(currentAngle) * spdY * spdFactor);
                        }
                    }
                    break;
            }

#if false
            double dAlpha = alphaStatic;

            if (x > gl_x0)
            {
                alpha += dAlpha;

/*
                float x2 = gl_x0 + (float)(dist * Math.Sin(alpha));
                float y2 = gl_x0 + (float)(dist * Math.Cos(alpha));

                dx = (float)((x2 - gl_x0) * sp_dist);
                dy = (float)((y2 - gl_x0) * sp_dist);*/

                // Optimized:
                dx = (float)(Math.Sin(alpha) * 5);
                dy = (float)(Math.Cos(alpha) * 5);
            }
            else
            {
                alpha -= dAlpha;
/*
                float x2 = gl_x0 - (float)(dist * Math.Sin(alpha));
                float y2 = gl_x0 + (float)(dist * Math.Cos(alpha));

                dx = (float)((x2 - gl_x0) * sp_dist);
                dy = (float)((y2 - gl_x0) * sp_dist);*/

                // Optimized:
                dx = (float)(-Math.Sin(alpha) * 5);
                dy = (float)(+Math.Cos(alpha) * 5);
            }
#endif

            // Normalize y from gl_Width to gl_Height
            y -= wh2;

            switch (sizeMode)
            {
                case 0:
                    size = 3;
                    break;

                case 1:
                    size = rand.Next(5) + 1;
                    break;
            }

            switch (daMode)
            {
                // Standard mode
                case 0:
                    da = myUtils.randFloat(rand, 0.01f) * 0.01f;
                    break;

                // Nice flashing effect (in case all the particles are generated at the same time)
                case 1:
                    da = 0.005f + myUtils.randomSign(rand) * myUtils.randFloat(rand) * 0.0001f;
                    break;

                case 2:
                    da = myUtils.randomChance(rand, 1, 2) ? 0.015f : 0.010f;
                    break;

                case 3:
                    da = myUtils.randomChance(rand, 1, 3) ? 0.005f : 0.01f;
                    break;
            }

            switch (colorMode)
            {
                case 0:
                    colorPicker.getColor(x, y, ref R, ref G, ref B);
                    break;

                case 1:
                    colorPicker.getColorRand(ref R, ref G, ref B);
                    break;
            }

            A = 0;

            x0 = (int)x;
            y0 = (int)y;

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            x += dx;
            y += dy;

            A += da;

            if (A >= 1)
            {
                da *= -10;
            }

            //if (x < 0 || x > gl_Width || y < 0 || y > gl_Height)
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
            }

            if (doConnect)
            {
                switch (connectMode)
                {
                    case 0:
                        myPrimitive._LineInst.setInstanceCoords(x, y, x0, y0);
                        myPrimitive._LineInst.setInstanceColor(1, 1, 1, 0.01f);
                        break;

                    case 1:
                        myPrimitive._LineInst.setInstanceCoords(x, y, x0, y0);
                        myPrimitive._LineInst.setInstanceColor(1, 1, 1, 0.5f);

                        x0 = (int)x;
                        y0 = (int)y;
                        break;
                }
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Process(Window window)
        {
            uint cnt = 0;
            int listCnt = 0;

            initShapes();

            if (doCreateAtOnce)
            {
                while (list.Count < N)
                    list.Add(new myObj_390());
            }

            // Disable VSYNC if needed
            // Glfw.SwapInterval(0);

            clearScreenSetup(doClearBuffer, 0.1f);

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

                    if (doConnect)
                    {
                        myPrimitive._LineInst.ResetBuffer();
                    }

                    listCnt = list.Count;

                    for (int i = 0; i != listCnt; i++)
                    {
                        var obj = list[i] as myObj_390;

                        obj.Show();
                        obj.Move();
                    }

                    if (doConnect)
                    {
                        myPrimitive._LineInst.Draw();
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

                if (doCreateAtOnce == false && list.Count < N)
                {
                    list.Add(new myObj_390());
                }

                cnt++;
                System.Threading.Thread.Sleep(renderDelay);

                if (cnt % 10 == 0)
                {
                    switch (angleMode)
                    {
                        case 3:
                            alphaStatic += dAlphaStatic;
                            break;
                    }
                }
            }
/*
            while (!Glfw.WindowShouldClose(window))
            {
                processInput(window);

                // Swap fore/back framebuffers, and poll for operating system events.
                Glfw.SwapBuffers(window);
                Glfw.PollEvents();
            }
*/
            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void initShapes()
        {
            myPrimitive.init_ScrDimmer();
            base.initShapes(shape, N, 0);

            if (doConnect)
            {
                myPrimitive.init_LineInst(N);
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------
    }
};
