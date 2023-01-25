using GLFW;
using static OpenGL.GL;
using System.Collections.Generic;


/*
    - The image is split into big number of particles that fall down
*/


namespace my
{
    public class myObj_370 : myObject
    {
        private int cnt;
        private float x, y, dx, dy;
        private float size, A, R, G, B, angle = 0, dAngle = 0;

        private static int N = 0, shape = 0, mode = 0, angleMode = 0, accelerationMode = 0, maxSize = 1,
                           dxFactor = 1, dyFactor = 1;
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

                shape = rand.Next(5);
            }

            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time local initialization
        private void initLocal()
        {
            doClearBuffer = myUtils.randomBool(rand);
            doFillShapes  = myUtils.randomBool(rand);

            mode             = rand.Next(4);                    // How the dy is calculated
            angleMode        = rand.Next(7);                    // How the particles are rotated
            maxSize          = rand.Next(11) + 1;
            accelerationMode = rand.Next(3);
            renderDelay      = rand.Next(11);
            dxFactor         = rand.Next(10) + 1;
            dyFactor         = rand.Next(33) + 1;

            accRate = myUtils.randFloat(rand) * 0.01f;
            dA = myUtils.randFloat(rand) * 0.5f;

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            height = 800;

            string str = $"Obj = myObj_370\n\n"                         +
                            $"N = {list.Count} of {N}\n"                +
                            $"doClearBuffer = {doClearBuffer}\n"        +
                            $"mode = {mode}\n"                          +
                            $"angleMode = {angleMode}\n"                +
                            $"accelerationMode = {accelerationMode}\n"  +
                            $"dxFactor = {dxFactor}\n"                  +
                            $"dyFactor = {dyFactor}\n"                  +
                            $"dA = {dA.ToString("0.000")}\n"            +
                            $"renderDelay = {renderDelay}\n"            +
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
            cnt = (cnt == 0) ? 123 : rand.Next(33) + 1;                         // Time to wait until the particle starts falling

            x = rand.Next(gl_Width);
            y = rand.Next(gl_Height + 123) - 123;

            size = myUtils.randFloat(rand, 0.02f) * (rand.Next(maxSize) + 1);

            colorPicker.getColor(x, y, ref R, ref G, ref B);

            switch (mode)
            {
                // dy is random
                case 0:
                case 1:
                    {
                        dx = myUtils.randFloat(rand, 0.05f) * myUtils.randomSign(rand) / dxFactor;
                        dy = myUtils.randFloat(rand, 0.05f) * (rand.Next(dyFactor) + 1);

                        A = 0.1f + 2 * dy / dyFactor;
                    }
                    break;

                // dy is a function of particle's color (opacity ver.1)
                case 2:
                    {
                        dx = myUtils.randFloat(rand, 0.05f) * myUtils.randomSign(rand) / dxFactor;
                        dy = myUtils.randFloat(rand, 0.05f) * (rand.Next(dyFactor) + 1);

                        A = 0.1f + 2 * dy / dyFactor;
                        dy = (R + G + B) * dyFactor * 0.5f;
                    }
                    break;

                // dy is a function of particle's color (opacity ver.2)
                case 3:
                    {
                        dx = myUtils.randFloat(rand, 0.05f) * myUtils.randomSign(rand) / dxFactor;
                        dy = (R + G + B) * dyFactor * 0.5f;

                        A = 0.1f + 2 * dy / dyFactor;
                    }
                    break;
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
            }
            else
            {
                x += dx;
                y += dy;
                A -= dA;
                angle += dAngle;

                switch (accelerationMode)
                {
                    case 0:
                        break;

                    case 1:
                        y += accRate;
                        break;

                    case 2:
                        y += size * accRate;
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
