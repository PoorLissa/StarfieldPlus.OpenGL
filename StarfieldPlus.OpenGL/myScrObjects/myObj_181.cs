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
        private bool isLive;
        private float x, y, dx, dy;
        private float size, A, R, G, B, angle, dAngle;

        private static int N = 0, n = 0, shape = 0, rate = 1, rateBase = 50, generatorLocationMode = 0,
                           sizeBase = 3, sizeMode = 0, waveSize = 1, waveSizeBase = 3000;
        private static bool doFillShapes = true, doUseRandomSpeed = true, doUseGravity = true, doMoveGenerators = true;
        private static float dimAlpha = 0.05f, Speed = 1.0f, speedBase = 1.0f, gravityValue = 0.0f;

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
            doMoveGenerators = myUtils.randomChance(rand, 1, 3);

            generatorLocationMode = rand.Next(3);
            sizeMode = rand.Next(4);

            renderDelay = rand.Next(11) + 3;

            rate = rateBase + myUtils.randomSign(rand) * rand.Next(33);

            waveSize = myUtils.randomChance(rand, 1, 5) ? rand.Next(waveSizeBase) + 123 : waveSizeBase;

            gravityValue = myUtils.randFloat(rand) * rand.Next(3);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            height = 600;

            string nStr(int   n) { return n.ToString("N0");    }
            string fStr(float f) { return f.ToString("0.000"); }

            string str = $"Obj = myObj_181 -- Particle Wave Generators\n\n"         +
                            $"N = {nStr(N - n)} (+ {nStr(n)} generator(s))\n"       +
                            $"liveCnt / deadCnt = {N-n-deadCnt} / {deadCnt}\n"      +
                            $"sizeMode= {sizeMode}\n"                               +
                            $"rate = {rate}\n"                                      +
                            $"waveSize = {waveSize}\n"                              +
                            $"doUseRandomSpeed = {doUseRandomSpeed}\n"              +
                            $"doMoveGenerators= {doMoveGenerators}\n"               +
                            $"renderDelay = {renderDelay}\n"                        +
                            $"dimAlpha = {fStr(dimAlpha)}\n"                        +
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
                switch (generatorLocationMode)
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

                switch (doMoveGenerators)
                {
                    case true:
                        dx = myUtils.randomSign(rand) * myUtils.randFloat(rand, 0.1f);
                        dy = myUtils.randomSign(rand) * myUtils.randFloat(rand, 0.1f);
                        break;

                    case false:
                        dx = dy = 0;
                        break;
                }

                size = 7;
                R = G = B = 1;
                A = 0.33f;
            }
            else
            {
                isLive = false;

                // Generator's id
                int i = rand.Next(n);

                x = (list[i] as myObj_181).x;
                y = (list[i] as myObj_181).y;

                float dX = gl_x0 - rand.Next(gl_Width);
                float dY = gl_x0 - rand.Next(gl_Width);

                double dist = Math.Sqrt(dX * dX + dY * dY);

                dx = (float)(dX * Speed / dist);
                dy = (float)(dY * Speed / dist);

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

                colorPicker.getColor(x, y, ref R, ref G, ref B);

                //R = G = B = 0.1f;
                A = 0.75f;
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            // Generator
            if (doMoveGenerators && id < n)
            {
                x += dx;
                y += dy;

                if (x < 0)
                    dx += 0.1f;

                if (x > gl_Width)
                    dx -= 0.1f;

                if (y < 0)
                    dy += 0.1f;

                if (y > gl_Height)
                    dy -= 0.1f;
            }

            // Particle
            if (isLive)
            {
                x += dx;
                y += dy;

                angle += dAngle;

                if (doUseGravity)
                {
                    y += gravityValue;
                }

                switch (sizeMode)
                {
                    case 0:
                        size += 0.025f;
                        break;
                }

                if (x < 0 || x > gl_Width || y < 0 || y > gl_Height)
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

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Process(Window window)
        {
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

                        if (obj.isLive)
                        {
                            obj.Show();
                            obj.Move();
                        }
                        else
                        {
                            if (newWaveCnt > 0)
                            {
                                obj.generateNew();

                                obj.isLive = true;
                                deadCnt--;
                                newWaveCnt--;
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
