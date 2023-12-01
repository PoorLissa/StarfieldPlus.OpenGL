using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;


/*
    - Multiple generators of particle waves
*/


namespace my
{
    public class myObj_181 : myObject
    {
        // Priority
        public static int Priority => 10;
		public static System.Type Type => typeof(myObj_181);

        private int parent_id;
        private bool isLive;
        private float x, y, dx, dy;
        private float size, A, R, G, B, angle, dAngle;

        private static int N = 0, n = 0, shape = 0, rate = 1, rateBase = 50, genLocationMode = 0, genOrderMode = 0,
                           sizeBase = 3, sizeMode = 0, waveSize = 1, waveSizeBase = 3000, moveMode = 0;
        private static bool doFillShapes = true, doUseRandomSpeed = true, doUseGravity = true;
        private static float dimAlpha = 0.05f, Speed = 1.0f, speedBase = 1.0f, gravityValue = 0.0f, randSpeedFactor = 0;

        private static int deadCnt = 0;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_181()
        {
            if (id != uint.MaxValue)
                generateNew();

            isLive = false;

            angle = 0;
            dAngle = myUtils.randomSign(rand) * myUtils.randFloat(rand, 0.1f) * 0.01f;
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time global initialization
        protected override void initGlobal()
        {
            colorPicker = new myColorPicker(gl_Width, gl_Height);
            list = new List<myObject>();

            // Global unmutable constants
            {
                N = 100000;                     // Number of particles
                n = rand.Next(9) + 2;           // Number of generators
                N += n;

                shape = rand.Next(5);
            }

            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time local initialization
        private void initLocal()
        {
            doClearBuffer    = myUtils.randomChance(rand, 9, 10);
            doFillShapes     = myUtils.randomBool(rand);
            doUseRandomSpeed = myUtils.randomBool(rand);
            doUseGravity     = myUtils.randomChance(rand, 1, 7);

            genLocationMode = rand.Next(3);
            sizeMode = rand.Next(4);
            moveMode = myUtils.randomChance(rand, 1, 3) ? rand.Next(4) : 0;
            genOrderMode = rand.Next(2);

            renderDelay = rand.Next(11) + 3;

            rate = rateBase + myUtils.randomSign(rand) * rand.Next(33);

            waveSize = myUtils.randomChance(rand, 1, 5) ? rand.Next(waveSizeBase) + 123 : waveSizeBase;

            gravityValue = myUtils.randFloat(rand) * 0.01f;

            randSpeedFactor = 0.01f * rand.Next(6);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            height = 600;

            string nStr(int   n) { return n.ToString("N0");    }
            string fStr(float f) { return f.ToString("0.000"); }

            string str = $"Obj = {Type} -- Particle Wave Generators\n\n"         	+
                            $"N = {nStr(N - n)} (+ {nStr(n)} generator(s))\n"       +
                            $"liveCnt / deadCnt = {N-n-deadCnt} / {deadCnt}\n"      +
                            $"sizeMode = {sizeMode}\n"                              +
                            $"moveMode = {moveMode}\n"                              +
                            $"genOrderMode = {genOrderMode}\n"                      +
                            $"rate = {rate}\n"                                      +
                            $"waveSize = {waveSize}\n"                              +
                            $"doUseRandomSpeed = {doUseRandomSpeed}\n"              +
                            $"renderDelay = {renderDelay}\n"                        +
                            $"dimAlpha = {fStr(dimAlpha)}\n"                        +
                            $"randSpeedFactor = {fStr(randSpeedFactor)}\n"          +
                            $"gravity = {fStr(doUseGravity ? gravityValue : 0)}\n"  +
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
            if (id < n)
            {
                // Place Generators:
                switch (genLocationMode)
                {
                    // Randomly
                    case 0:
                        x = rand.Next(gl_Width);
                        y = rand.Next(gl_Height);
                        break;

                    // On a central line, evenly distributed
                    case 1:
                        x = (id + 1) * gl_Width / (n + 1);
                        y = gl_y0;
                        break;

                    // On a central line, randomly distributed
                    case 2:
                        x = rand.Next(gl_Width);
                        y = gl_y0;
                        break;
                }

                // Set generator's move pattern
                switch (moveMode)
                {
                    case 0:
                        dx = dy = 0;
                        break;

                    case 1:
                        dx = myUtils.randomSign(rand) * myUtils.randFloat(rand, 0.1f);
                        dy = 0;
                        break;

                    case 2:
                        dy = myUtils.randomSign(rand) * myUtils.randFloat(rand, 0.1f);
                        dx = 0;
                        break;

                    case 3:
                        dx = myUtils.randomSign(rand) * myUtils.randFloat(rand, 0.1f);
                        dy = myUtils.randomSign(rand) * myUtils.randFloat(rand, 0.1f);
                        break;
                }

                size = 7;
                A = 0.33f;
            }
            else
            {
                isLive = false;

                // Attach this particle to a generator:
                if (parent_id < 0)
                {
                    parent_id = rand.Next(n);
                }

                x = (list[parent_id] as myObj_181).x;
                y = (list[parent_id] as myObj_181).y;

                float dX = gl_x0 - rand.Next(gl_Width);
                float dY = gl_x0 - rand.Next(gl_Width);

                double dist = Math.Sqrt(dX * dX + dY * dY);

                // Vary speed just a bit
                dx = (float)(dX * (Speed + myUtils.randFloat(rand, 0.1f) * randSpeedFactor) / dist);
                dy = (float)(dY * (Speed + myUtils.randFloat(rand, 0.1f) * randSpeedFactor) / dist);

                switch (sizeMode)
                {
                    case 0:
                        size = 0;
                        break;

                    case 1:
                        size = sizeBase;
                        break;

                    case 2:
                        size = rand.Next(sizeBase) + 1;
                        break;

                    case 3:
                        size = rand.Next(sizeBase * 3) + 1;
                        break;
                }

                A = myUtils.randFloat(rand, 0.01f);
            }

            colorPicker.getColor(x, y, ref R, ref G, ref B);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            // Generator
            if (id < n && moveMode > 0)
            {
                x += dx;
                y += dy;

                if (x < 0)
                    dx += 0.01f;

                if (x > gl_Width)
                    dx -= 0.01f;

                if (y < 0)
                    dy += 0.01f;

                if (y > gl_Height)
                    dy -= 0.01f;

                return;
            }

            // Particle
            if (isLive)
            {
                x += dx;
                y += dy;

                //x += dx * myUtils.randFloat(rand);
                //y += dy * myUtils.randFloat(rand);

                angle += dAngle;

                if (doUseGravity)
                {
                    dy += gravityValue;
                }

                switch (sizeMode)
                {
                    case 0:
                        size += 0.025f;
                        break;
                }

                if (x < -123 || x > gl_Width + 123 || y < -123 || y > gl_Height + 123)
                {
                    isLive = false;
                    deadCnt++;
                    x = -1234;
                    y = -1234;
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
                    myPrimitive._RectangleInst.setInstanceCoords(x - size, y - size, size2x, size2x);
                    myPrimitive._RectangleInst.setInstanceColor(R, G, B, A);
                    myPrimitive._RectangleInst.setInstanceAngle(angle);
                    break;

                // Instanced triangles
                case 1:
                    myPrimitive._TriangleInst.setInstanceCoords(x, y, size2x, angle);
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
            int currentGenerator = 0;
            uint cnt = 0;
            initShapes();

            clearScreenSetup(doClearBuffer, 0.1f);

            while (list.Count < N)
            {
                list.Add(new myObj_181());
                deadCnt++;
            }

            deadCnt -= n;

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
                //if (false)
                {
                    inst.ResetBuffer();

                    int newWaveCnt = (cnt % rate == 0) && deadCnt >= waveSize ? waveSize : 0;

                    if (newWaveCnt > 0)
                    {
                        Speed = doUseRandomSpeed ? 0.75f + 0.01f * rand.Next(500) : speedBase;
                    }

                    int Count = list.Count;

                    for (int i = 0; i != Count; i++)
                    {
                        var obj = list[i] as myObj_181;

                        if (obj.id < n)
                        {
                            obj.Show();
                            obj.Move();
                        }
                        else
                        {
                            if (obj.isLive)
                            {
                                obj.Show();
                                obj.Move();
                            }
                            else
                            {
                                if (newWaveCnt > 0)
                                {
                                    switch (genOrderMode)
                                    {
                                        case 0:
                                            obj.parent_id = -1;
                                            break;

                                        case 1:
                                            obj.parent_id = currentGenerator++;

                                            if (currentGenerator == n)
                                            {
                                                currentGenerator = 0;
                                            }
                                            break;
                                    }

                                    obj.generateNew();

                                    obj.isLive = true;
                                    deadCnt--;
                                    newWaveCnt--;
                                }
                            }
                        }
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

                cnt++;
                System.Threading.Thread.Sleep(renderDelay);
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
