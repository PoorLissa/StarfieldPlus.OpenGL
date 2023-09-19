using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;


/*
    - Rotating circles made of letters and symbols
*/


namespace my
{
    public class myObj_630 : myObject
    {
        // Priority
        public static int Priority => 999910;

        private int index, cnt;
        private float x, y;
        private float A, dA, R, G, B, rad, sizeFactor, angle = 0, dAngle;

        private static int N = 0, size = 20, minRad = 0, maxRad = 0;
        private static int dAngleMode = 0, dAMode, radMode = 0, sizeMode = 0;
        private static float dimAlpha = 0.05f, dAngleCommon = 0;
        private static bool doUseRGB = true;

        private static TexText tTex = null;
        private static int[] Rad = null;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_630()
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
                N = rand.Next(123) + 123;

                // Set up Radius:
                {
                    radMode = rand.Next(2);
                    minRad = rand.Next(333) + 33;
                    maxRad = gl_x0 - rand.Next(333);

                    if (radMode == 1)
                    {
                        Rad = new int[rand.Next(11) + 2];

                        for (int i = 0; i < Rad.Length; i++)
                        {
                            Rad[i] = minRad + rand.Next(maxRad - minRad);
                        }
                    }
                }

                // If true, paint alphabet in white and then set custom color for each particle
                doUseRGB = myUtils.randomChance(rand, 1, 2);
            }

            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time local initialization
        private void initLocal()
        {
            doClearBuffer = true;

            dAMode = rand.Next(2);
            sizeMode = rand.Next(2);

            dAngleMode = rand.Next(4);
            dAngleCommon = myUtils.randFloatSigned(rand, 0.05f) * 0.01f;

            size = 50 + rand.Next(50);
            renderDelay = rand.Next(11) + 3;

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            height = 600;

            string nStr(int   n) { return n.ToString("N0");    }
            //string fStr(float f) { return f.ToString("0.000"); }

            string str = $"Obj = myObj_630\n\n"                      +
                            $"N = {nStr(list.Count)} of {nStr(N)}\n" +
                            $"doUseRGB = {doUseRGB}\n"               +
                            $"radMode = {radMode}\n"                 +
                            $"sizeMode = {sizeMode}\n"               +
                            $"dAMode = {dAMode}\n"                   +
                            $"dAngleMode = {dAngleMode}\n"           +
                            $"font = '{tTex.FontFamily()}'\n"        +
                            $"minRad = {minRad}\n"                   +
                            $"maxRad = {maxRad}\n"                   +
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
            cnt = 50 + rand.Next(111);

            x = rand.Next(gl_Width);
            y = rand.Next(gl_Height);

            switch (radMode)
            {
                case 0:
                    rad = minRad + rand.Next(maxRad - minRad);
                    break;

                case 1:
                    rad = Rad[rand.Next(Rad.Length)];
                    break;
            }

            angle = myUtils.randFloatSigned(rand) * rand.Next(123);
            dAngle = myUtils.randFloat(rand, 0.05f) * 0.01f;

            switch (dAngleMode)
            {
                case 0:
                    dAngle *= myUtils.signOf(dAngleCommon);             // Value is random, sign is the same
                    break;

                case 1:
                    dAngle *= myUtils.randomSign(rand);                 // Value is random, sign is random
                    break;

                case 2:
                    dAngle = dAngleCommon;                              // Value is the same, sign is the same
                    break;

                case 3:
                    dAngle = dAngleCommon * myUtils.randomSign(rand);   // Value is the same, sign is random
                    break;
            }

            A = myUtils.randFloat(rand);
            dA = myUtils.randFloat(rand, 0.1f) * 0.0025f;

            colorPicker.getColor(x, y, ref R, ref G, ref B);

            // Pair the particle to a symbol
            index = rand.Next(tTex.Lengh());

            sizeFactor = (sizeMode == 1 && size > 21)
                ? 0.5f + myUtils.randFloat(rand) * (20 / (float)size)
                : 1.0f;

            if (sizeFactor < 1)
            {
                ;
            }

            // Move once to start at the right location
            Move();

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            switch (dAMode)
            {
                case 1:
                    if (--cnt < 0)
                        A -= dA;
                    break;
            }

            if (A < 0)
            {
                generateNew();
            }
            else
            {
                angle += dAngle;

                x = gl_x0 + rad * (float)Math.Sin(angle);
                y = gl_y0 + rad * (float)Math.Cos(angle);
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Show()
        {
            if (doUseRGB)
            {
                tTex.Draw(x, y, index, A, ((float)(Math.PI) - angle), sizeFactor, R, G, B);
            }
            else
            {
                tTex.Draw(x, y, index, A, ((float)(Math.PI) - angle), sizeFactor);
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
                    else
                    {
                        dimScreen(dimAlpha);
                    }
                }

                // Render Frame
                {
                    for (int i = 0; i != Count; i++)
                    {
                        var obj = list[i] as myObj_630;

                        obj.Show();
                        obj.Move();
                    }
                }

                if (Count < N)
                {
                    list.Add(new myObj_630());
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

            TexText.setScrDimensions(gl_Width, gl_Height);
            tTex = new TexText(size, doUseRGB);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------
    }
};
