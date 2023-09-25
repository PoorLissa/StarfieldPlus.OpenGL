using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;


/*
    - Big Bang
*/


namespace my
{
    public class myObj_100 : myObject
    {
        // Priority
        public static int Priority => 20;
		public static System.Type Type => typeof(myObj_100);

        private float x, y, dx, dy, angle, dAngle, size, A, R, G, B;
        private int cnt, lifeCounter, max, color;

        private static int N = 0, shape = 0, colorMode = 0, maxLife = 500, maxSpeed = 0, explosionSpeed = 0, dustNum = 666, offCenterRad = 1, dyFactor = 1;
        private static int centerGenMode = 0, x0off = -1, y0off = -1;
        private static bool doFillShapes = false, isExplosionMode = false, doUseAcceleration = false, doShowZeroSize = true;
        private static bool doUseDiscreetSpeed = true, doUseFastDeath = false, doUseBackSuction = false, doUseDyFactor = false;
        private static float accelerationFactor = 1.0f, t = 0, aMin = 0.65f;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_100()
        {
            generateNew();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time global initialization
        protected override void initGlobal()
        {
            colorPicker = new myColorPicker(gl_Width, gl_Height);
            list = new List<myObject>();

            shape = rand.Next(5);
            dustNum = 666;
            N = 1234;

            switch (rand.Next(7))
            {
                case 0:
                    dustNum += rand.Next(666);
                    N += rand.Next(1234);
                    break;

                case 1:
                    renderDelay = 20;
                    dustNum += rand.Next(3210);
                    N += rand.Next(1234);
                    break;

                case 2:
                    renderDelay = 10;
                    dustNum += rand.Next(43210);
                    N += rand.Next(12345);
                    break;

                case 3:
                    renderDelay = 3;
                    dustNum += rand.Next(543210);
                    N += rand.Next(123456);
                    break;

                case 4:
                    renderDelay = 20;
                    dustNum += rand.Next(1234);
                    N += rand.Next(3210);
                    break;

                case 5:
                    renderDelay = 10;
                    dustNum += rand.Next(12345);
                    N += rand.Next(43210);
                    break;

                case 6:
                    renderDelay = 3;
                    dustNum += rand.Next(123456);
                    N += rand.Next(543210);
                    break;
            }

            N += dustNum;

            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time local initialization
        private void initLocal()
        {
            doClearBuffer      = myUtils.randomBool(rand);
            doFillShapes       = myUtils.randomBool(rand);
            doUseBackSuction   = myUtils.randomBool(rand);          // Particles are born not in the center, but rathe on the opposite side of it
            doUseDyFactor      = myUtils.randomChance(rand, 1, 3);  // dy gets divided by factor
            doShowZeroSize     = myUtils.randomBool(rand);          // Particles of zero size are still drawn
            doUseFastDeath     = myUtils.randomBool(rand);          // The particle dies when the counter reaches 0, OR also when the particle reaches the border
            doUseAcceleration  = myUtils.randomChance(rand, 1, 3);  // The particles receive additional acceleration
            doUseDiscreetSpeed = myUtils.randomBool(rand);          // The initial speed is generated using int or float variable

            centerGenMode = rand.Next(4);
            maxSpeed = rand.Next(11);
            explosionSpeed = rand.Next(333) + 33;
            colorMode = rand.Next(2);
            accelerationFactor = 1.0f + myUtils.randFloat(rand) * 0.02f;
            offCenterRad = rand.Next(121) - 60;
            dyFactor = rand.Next(11) + 1;

            switch (rand.Next(7))
            {
                // Original mode with very slow expansion -- no explosion
                case 0:
                    maxSpeed = 0;
                    explosionSpeed = 0;
                    break;

                // Original mode with very slow expansion -- plus explosion
                case 1:
                    maxSpeed = 0;
                    break;

                // No Explosion
                case 2:
                    maxSpeed = 0;
                    break;
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            height = 800;

            string str = $"Obj = myObj_100\n\n" +
                            $"N = {list.Count} of {N}\n" +
                            $"starNum = {N - dustNum}\n" +
                            $"dustNum = {dustNum}\n" +
                            $"doClearBuffer = {doClearBuffer}\n" +
                            $"doUseDiscreetSpeed = {doUseDiscreetSpeed}\n" +
                            $"doUseFastDeath = {doUseFastDeath}\n" +
                            $"doUseBackSuction = {doUseBackSuction}\n" +
                            $"doUseAcceleration = {doUseAcceleration}\n" +
                            $"accelerationFactor = {accelerationFactor.ToString("0.000")}\n" +
                            $"doShowZeroSize = {doShowZeroSize}\n" +
                            $"maxSpeed = {maxSpeed}\n" +
                            $"explosionSpeed = {explosionSpeed}\n" +
                            $"offCenterRad = {offCenterRad}\n" +
                            $"colorMode = {colorMode}\n" +
                            $"renderDelay = {renderDelay}\n" +
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
            size = 0;
            cnt = 0;
            angle = 0;
            dAngle = myUtils.randFloat(rand) * 0.01f * myUtils.randomSign(rand);
            lifeCounter = rand.Next(maxLife) + maxLife;
            max = rand.Next(200) + 100;

            if (N > 10000)
            {
                aMin = 0.50f;
            }
            else if (N > 50000)
            {
                aMin = 0.35f;
            }
            else if (N > 100000)
            {
                aMin = 0.20f;
            }
            else if (N > 200000)
            {
                aMin = 0.11f;
            }

            A = aMin + myUtils.randFloat(rand) * 0.2f;
            color = rand.Next(50);

            if (color < 6)
            {
                switch (colorMode)
                {
                    // Colors from the colorPicker
                    case 0:
                        colorPicker.getColorRand(ref R, ref G, ref B);
                        break;

                    // Colors from the original implementation
                    case 1:
                        switch (color)
                        {
                            case 0: R = 1.00f; G = 0.00f; B = 0.00f; break;             // Red
                            case 1: R = 1.00f; G = 1.00f; B = 0.00f; break;             // Yellow
                            case 2: R = 0.00f; G = 0.00f; B = 1.00f; break;             // Blue
                            case 3: R = 1.00f; G = 0.65f; B = 0.00f; break;             // Orange
                            case 4: R = 0.00f; G = 1.00f; B = 1.00f; break;             // Aqua
                            case 5: R = 0.93f; G = 0.51f; B = 0.93f; break;             // Violet
                        }
                        break;
                }
            }
            else
            {
                R = 1.0f - myUtils.randFloat(rand) * 0.1f;
                G = 1.0f - myUtils.randFloat(rand) * 0.1f;
                B = 1.0f - myUtils.randFloat(rand) * 0.1f;
            }

            int iSpeed = 1;
            float fSpeed = myUtils.randFloat(rand, 0.1f) + myUtils.randFloat(rand, 0.1f);

            if (id < dustNum)
            {
                // Dust particles (no acceleration, size == 1, low alpha)
                size = 1;

                if (doUseDiscreetSpeed)
                {
                    iSpeed += rand.Next(maxSpeed);
                }
                else
                {
                    fSpeed += rand.Next(maxSpeed);
                }

                lifeCounter += lifeCounter / 3;
                A = 0.01f;
            }
            else
            {
                // Normal particles
                if (isExplosionMode)
                {
                    size = 1;

                    if (doUseDiscreetSpeed)
                    {
                        iSpeed += rand.Next(explosionSpeed) + explosionSpeed / 100;
                    }
                    else
                    {
                        fSpeed += rand.Next(explosionSpeed) + explosionSpeed / 100;
                    }
                }
                else
                {
                    if (doUseDiscreetSpeed)
                    {
                        iSpeed += rand.Next(maxSpeed);
                    }
                    else
                    {
                        fSpeed += rand.Next(maxSpeed);
                    }
                }
            }

            if (isExplosionMode)
            {
                size = rand.Next(3) + 1;
                lifeCounter = rand.Next(100) + 33;
            }

            // and also add a mode where [x0, y0] is not in the center, but is offset somewhere
            // and also a mode where [x0, y0] is rotating around the center

            int W = 0, x0 = 0, y0 = 0;

            switch (centerGenMode)
            {
                case 0:
                    W = gl_Width;
                    x0 = gl_x0;
                    y0 = gl_x0;
                    break;

                case 1:
                    W = gl_Width * 4;
                    x0 = rand.Next(W);
                    y0 = rand.Next(W);
                    break;

                // Central point is not in the center
                case 2:
                    {
                        W = gl_Width * 4;

                        if (x0off == -1 && y0off == -1)
                        {
                            x0off = rand.Next(W/3);
                            y0off = rand.Next(W/3);
                        }

                        x0 = rand.Next(W) - x0off;
                        y0 = rand.Next(W) - y0off;
                    }
                    break;

                case 3:
                    W = gl_Width;
                    x0 = gl_x0 + (int)(Math.Sin(t) * 333);
                    y0 = gl_x0 + (int)(Math.Cos(t) * 333);
                    t += 0.01f;
                    break;
            }

            // As X and Y are generated within a square [Width x Width],
            // both dx and dy will be calculated using point [x0, x0]
            x = rand.Next(W);
            y = rand.Next(W);

            double sp_dist, dist = Math.Sqrt((x - x0) * (x - x0) + (y - y0) * (y - y0));

            if (doUseDiscreetSpeed)
            {
                sp_dist = iSpeed / dist;
            }
            else
            {
                sp_dist = fSpeed / dist;
            }

            dx = (float)((x - x0) * sp_dist);
            dy = (float)((y - y0) * sp_dist);

            if (doUseDyFactor)
            {
                dy /= dyFactor;
            }

            // Move each object to the starting point:
            if (doUseBackSuction)
            {
                if (offCenterRad >= 0)
                {
                    x = gl_x0 - offCenterRad * dx;
                    y = gl_y0 - offCenterRad * dy;
                }
                else
                {
                    x = gl_x0 - rand.Next(-offCenterRad) * dx;
                    y = gl_y0 - rand.Next(-offCenterRad) * dy;
                }
            }
            else
            {
                x = gl_x0;
                y = gl_y0;
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            lifeCounter--;

            x += dx;
            y += dy;

            angle += dAngle;

            // Accelerate
            if (doUseAcceleration && id >= dustNum)
            {
                dx *= accelerationFactor * (size + 1);
                dy *= accelerationFactor * (size + 1);
            }

            bool isDead = lifeCounter == 0;

            if (!isDead && doUseFastDeath)
            {
                if (x < 0 || y < 0 || x > gl_Width || y > gl_Height)
                    isDead = true;
            }

            if (isDead)
            {
                generateNew();
            }
            else
            {
                if (id >= dustNum)
                {
                    if (cnt++ > max)
                    {
                        cnt = 0;
                        size = rand.Next(5) + 1;

                        if (doUseDyFactor)
                        {
                            if (dy < 0)
                            {
                                size = rand.Next(3) + 1;
                                dy *= 0.999999f;
                            }
                            else
                            {
                                dy *= 1.000001f;
                                size += 1;
                            }
                        }
                    }

                    // Change opacity sometimes
                    if (cnt % 100 == 0)
                    {
                        A = 0.65f + myUtils.randFloat(rand) * 0.2f;

                        if (doUseDyFactor)
                        {
                            if (dy < 0)
                                A *= 0.5f;
                        }

                        // Dim the star sometimes
                        if (rand.Next(11) == 0)
                        {
                            A /= (rand.Next(5) + 1);
                        }
                    }
                }
                else
                {
                    if (doClearBuffer)
                    {
                        if (A < 0.23f)
                        {
                            A += 0.01f;
                        }
                    }
                    else
                    {
                        if (A < 0.17f)
                        {
                            A += 0.01f;
                        }
                    }
                }
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Show()
        {
            int Size = (int)size;
            float a = A;

            if (size == 0 && doShowZeroSize)
            {
                Size = 1;
                a = 0.23f;
            }

            switch (shape)
            {
                // Instanced squares
                case 0:
                    var rectInst = inst as myRectangleInst;

                    rectInst.setInstanceCoords(x, y, Size, Size);
                    rectInst.setInstanceColor(R, G, B, a);
                    rectInst.setInstanceAngle(angle);
                    break;

                // Instanced triangles
                case 1:
                    var triangleInst = inst as myTriangleInst;

                    triangleInst.setInstanceCoords(x, y, Size, angle);
                    triangleInst.setInstanceColor(R, G, B, a);
                    break;

                // Instanced circles
                case 2:
                    var ellipseInst = inst as myEllipseInst;

                    ellipseInst.setInstanceCoords(x, y, Size, 0);
                    ellipseInst.setInstanceColor(R, G, B, a);
                    break;

                // Instanced pentagons
                case 3:
                    var pentagonInst = inst as myPentagonInst;

                    pentagonInst.setInstanceCoords(x, y, Size, angle);
                    pentagonInst.setInstanceColor(R, G, B, a);
                    break;

                // Instanced hexagons
                case 4:
                    var hexagonInst = inst as myHexagonInst;

                    hexagonInst.setInstanceCoords(x, y, Size, angle);
                    hexagonInst.setInstanceColor(R, G, B, a);
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
            //Glfw.SwapInterval(0);

            if (doClearBuffer)
            {
                glDrawBuffer(GL_FRONT_AND_BACK | GL_DEPTH_BUFFER_BIT);

                float r = (float)rand.NextDouble() / (rand.Next(11) + 11);
                float g = (float)rand.NextDouble() / (rand.Next(11) + 11);
                float b = (float)rand.NextDouble() / (rand.Next(11) + 11);

                glClearColor(r, g, b, 1.0f);
            }
            else
            {
                dimScreenRGB_SetRandom(0.1f);

                // This combination does not result in blinking on the higher number of particles
                // That is, in Win7
                glDrawBuffer(GL_DEPTH_BUFFER_BIT);
            }

            isExplosionMode = true;

            while (list.Count < N)
            {
                list.Add(new myObj_100());
            }

            isExplosionMode = false;

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
                    dimScreen(0.1f, false);
                }

                // Render Frame
                {
                    inst.ResetBuffer();

                    for (int i = 0; i < list.Count; i++)
                    {
                        var obj = list[i] as myObj_100;

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
                    list.Add(new myObj_100());
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
            base.initShapes(shape, N, 0);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------
    }
};
