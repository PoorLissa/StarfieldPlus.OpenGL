using GLFW;
using System;
using static OpenGL.GL;
using System.Collections.Generic;


/*
    - Grid covered with hexagons -- unfinished yet
*/


namespace my
{
    public class myObj_340 : myObject
    {
        private int x, y, lifeCounter;
        private float size, A, R, G, B;

        private static int N = 0, nActive = 0, shape = 0, baseSize = 0, sizeOff = 0, dSize = 0, mode = 0, lifeCntMode = 0, lifeCntBase = 0;
        private static bool doFillShapes = true, doUseRotation = true, doReduceSize = true;
        private static float dimAlpha = 0.05f, t = 0, dt = 0, sinPi3 = 0, lineWidth = 1;

        private static myHexagonInst hexInst = null;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_340()
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
                N = 100;
                shape = 4;
                doClearBuffer = false;

                sinPi3 = (float)Math.Sin(Math.PI / 3);
            }

            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time local initialization
        private void initLocal()
        {
            // Number of active object for this session
            nActive = rand.Next(N-3) + 3;

            // Grid size: Only even numbers work somehow
            baseSize = (rand.Next(77) + 3) * 2;

            mode = rand.Next(2);
            lifeCntMode = rand.Next(3);
            lifeCntBase = rand.Next(10) + 1;

            dt = myUtils.randFloat(rand, 0.1f) * 0.1f;

            doFillShapes     = myUtils.randomBool(rand);
            doUseRotation    = myUtils.randomBool(rand);
            doReduceSize     = myUtils.randomBool(rand);

            lineWidth = 0.01f * (rand.Next(666) + 1);           // Width of line
            sizeOff = 3 + rand.Next(baseSize/3);                // Size offset
            dSize = rand.Next(7) + 1;                           // Size changing speed
            dimAlpha = 0.005f * (rand.Next(11) + 1);            //

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            height = 800;

            string str = $"Obj = myObj_340\n\n"                             +
                            $"N = {list.Count} of {N}; active: {nActive}\n" +
                            $"mode = {mode}\n"                              +
                            $"lifeCntMode = {lifeCntMode}\n"                +
                            $"baseSize = {baseSize}\n"                      +
                            $"doUseRotation = {doUseRotation}\n"            +
                            $"doReduceSize = {doReduceSize}\n"              +
                            $"dimAlpha = {dimAlpha.ToString("0.000")}\n"    +
                            $"lineWidth = {lineWidth.ToString("0.000")}\n"  +
                            $"file: {colorPicker.GetFileName()}"
                ;
            return str;
        }

        // ---------------------------------------------------------------------------------------------------------------

        // 
        protected override void setNextMode()
        {
            initLocal();

            dimScreen(0.1f);
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void generateNew()
        {
            // Aligh to hex grid
            {
                x = rand.Next(gl_Width  + 100);
                y = rand.Next(gl_Height + 200);

                x -= x % (3 * baseSize / 2);
                y -= y % (int)(baseSize * sinPi3 * 2);

                if (x % baseSize / 2 != 0)
                {
                    y -= (int)(baseSize * sinPi3);
                }
            }

            switch (mode)
            {
                case 0:
                    size = baseSize - sizeOff;
                    break;

                case 1:
                    size = rand.Next(baseSize - 3) + 3;
                    break;
            }

            colorPicker.getColor(x, y, ref R, ref G, ref B);
            A = myUtils.randFloat(rand, 0.25f);

            switch (lifeCntMode)
            {
                case 0:
                    lifeCounter = 1;
                    break;

                case 1:
                    lifeCounter = rand.Next(33) + 1;
                    break;

                case 2:
                    lifeCounter = lifeCntBase;
                    break;
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            t += dt;
            lifeCounter--;

            if (doReduceSize && size > dSize)
            {
                size -= dSize;
            }

            if (lifeCounter == 0)
            {
                generateNew();
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Show()
        {
            if (doUseRotation)
            {
                hexInst.setInstanceCoords(x, y, size * 2, t);
            }
            else
            {
                hexInst.setInstanceCoords(x, y, size * 2, 0);
            }

            hexInst.setInstanceColor(R, G, B, A);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Process(Window window)
        {
            uint cnt = 0;
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
                glDrawBuffer(GL_FRONT_AND_BACK);
            }

            while (!Glfw.WindowShouldClose(window))
            {
                processInput(window);

                // Swap fore/back framebuffers, and poll for operating system events.
                Glfw.SwapBuffers(window);
                Glfw.PollEvents();

                if (cnt == 1001)
                {
                    cnt = 0;
                    setNextMode();
                }

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
                        if (i == nActive)
                        {
                            break;
                        }
                        else
                        {
                            var obj = list[i] as myObj_340;

                            obj.Show();
                            obj.Move();
                        }
                    }

                    if (doFillShapes)
                    {
                        // Tell the fragment shader to multiply existing instance opacity by 0.5:
                        inst.SetColorA(-0.1f);
                        inst.Draw(true);
                    }

                    glLineWidth(lineWidth);

                    // Tell the fragment shader to do nothing with the existing instance opacity:
                    inst.SetColorA(0);
                    inst.Draw(false);
                }

                if (list.Count < N)
                {
                    list.Add(new myObj_340());
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

            // Only single shape is used
            hexInst = inst as myHexagonInst;

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------
    }
};
