using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;


/*
    - A ring of moving particles
*/


namespace my
{
    public class myObj_530 : myObject
    {
        // Priority
        public static int Priority { get { return getPriority(); } }

        private int cnt;
        private float x, y;
        private float size, A, dA, R, G, B, angle = 0, dAngle, alpha, dAlpha, r, spd;

        private static int N = 0, n = 0, shape = 0, dAlphaMode = 0, Thickness = 0, maxCnt = 0, rotationMode = 0, colorMode = 0;
        private static bool doFillShapes = false, doTraceColor = false, doMoveWhileWaiting = false;
        private static float dimAlpha = 0.05f, maxDa = 0;

        private static int[] Rads;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_530()
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
                N = rand.Next(10000) + 3000;
                N = 33333;

                n = rand.Next(2) + 1;
                n = 1;

                Rads = new int[n];

                shape = rand.Next(5);
            }

            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time local initialization
        private void initLocal()
        {
            doClearBuffer = myUtils.randomChance(rand, 12, 13);
            doTraceColor = myUtils.randomChance(rand, 1, 11);
            doFillShapes = myUtils.randomChance(rand, 1, 3);
            doMoveWhileWaiting = myUtils.randomChance(rand, 1, 2);

            dAlphaMode = rand.Next(5);
            rotationMode = rand.Next(6);
            colorMode = rand.Next(2);

            for (int i = 0; i < n; i++)
            {
                Rads[i] = 333 + rand.Next(333);
            }

            Thickness = rand.Next(111) + 1;
            maxCnt = rand.Next(50) + 10;

            maxDa = 0.001f + myUtils.randFloat(rand) * 0.01f;

            renderDelay = rand.Next(11) + 1;

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            height = 600;

            string nStr(int n) { return n.ToString("N0"); }
            string fStr(float f) { return f.ToString("0.000"); }

            string str = $"Obj = myObj_530\n\n"                            +
                            $"N = {nStr(list.Count)} of {nStr(N)}\n"       +
                            $"doClearBuffer = {doClearBuffer}\n"           +
                            $"dAlphaMode = {dAlphaMode}\n"                 +
                            $"Thickness = {Thickness}\n"                   +
                            $"maxDa = {fStr(maxDa)}\n"                     +
                            $"maxCnt = {maxCnt}\n"                         +
                            $"rotationMode = {rotationMode}\n"             +
                            $"doMoveWhileWaiting = {doMoveWhileWaiting}\n" +
                            $"renderDelay = {renderDelay}\n"               +
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
            cnt = maxCnt;

            r = Rads[rand.Next(n)] + rand.Next(Thickness);

            alpha = (float)Math.PI * myUtils.randFloat(rand) * rand.Next(101);

            switch (dAlphaMode)
            {
                case 0:
                    dAlpha = 0;
                    break;

                case 1:
                    dAlpha = myUtils.randFloat(rand) * 0.2f;
                    break;

                case 2:
                    dAlpha = myUtils.randFloat(rand) * myUtils.randomSign(rand) * 0.2f;
                    break;

                case 3:
                    dAlpha = myUtils.randomChance(rand, 1, 2) ? 0.01f : -0.01f;
                    break;

                case 4:
                    dAlpha = myUtils.randFloat(rand) * myUtils.randomSign(rand) * 0.01f;
                    break;

            }

            x = gl_x0 + r * (float)Math.Sin(alpha);
            y = gl_y0 + r * (float)Math.Cos(alpha);

            spd = 0.5f;
            size = rand.Next(5) + 2;

            if (myUtils.randomChance(rand, 1, 2))
            {
                spd *= -1;
                size -= 1;
            }

            if (spd > 0)
            {
                dAlpha = dAlpha < 0 ? -dAlpha : dAlpha;
            }

            A = myUtils.randFloat(rand) * 0.5f;
            dA = myUtils.randomChance(rand, 1, 1111) ? maxDa / 2 : maxDa;


            // Particle color
            switch (colorMode)
            {
                case 0:
                    colorPicker.getColor(x, y, ref R, ref G, ref B);
                    break;

                case 1:
                    R = 1; G = B = 0; A = 0.66f;
                    break;
            }


            // Particle rotation
            switch (rotationMode)
            {
                case 0:
                    angle = 0;
                    dAngle = 0;
                    break;

                case 1:
                case 2:
                    angle = myUtils.randFloat(rand) * rand.Next(123);
                    dAngle = 0;
                    break;

                case 3:
                case 4:
                case 5:
                    angle = myUtils.randFloat(rand) * rand.Next(123);
                    dAngle = myUtils.randFloat(rand) * myUtils.randomSign(rand) * 0.01f;
                    break;
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        // todo:
        // 1. random linear movement starting from the circle curface
        // 2. rotation on different orbits

        protected override void Move()
        {
            angle += dAngle;
            alpha += dAlpha;

            if (cnt > 0)
            {
                cnt--;

                if (doMoveWhileWaiting)
                {
                    //x += dx * 0.1f;
                    //y += dy * 0.1f;
                }

                //x = gl_x0 + r * (float)Math.Sin(alpha);
                //y = gl_y0 + r * (float)Math.Cos(alpha);

                if (cnt == 1)
                {
                    dAlpha *= 0.2f;
                    //dAlpha = 0;
                }
            }
            else
            {
                r += spd;

                x = gl_x0 + r * (float)Math.Sin(alpha);
                y = gl_y0 + r * (float)Math.Cos(alpha);

                A -= dA;

                if (A < 0 || r < 0)
                {
                    generateNew();
                }
                else
                {
                    if (doTraceColor)
                    {
                        colorPicker.getColor(x, y, ref R, ref G, ref B);
                    }
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

            // Disable VSYNC if needed
            // Glfw.SwapInterval(0);

            clearScreenSetup(doClearBuffer, 0.11f, true);


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
                    else
                    {
                        dimScreen(dimAlpha);
                    }
                }

                // Render Frame
                {
                    inst.ResetBuffer();

                    for (int i = 0; i != Count; i++)
                    {
                        var obj = list[i] as myObj_530;

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
                    list.Add(new myObj_530());
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

        private static int getPriority()
        {
#if DEBUG
            return 10;
#endif
            return 10;
        }
    }
};
