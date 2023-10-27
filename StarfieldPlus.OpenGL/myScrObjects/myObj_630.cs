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
        public static int Priority => 10;
        public static System.Type Type => typeof(myObj_630);

        private int index, cnt;
        private float x, y;
        private float A, dA, R, G, B, radx, rady, sizeFactor, angle = 0, dAngle;

        private static int N = 0, size = 20, minRad = 0, maxRad = 0;
        private static int dAngleMode = 0, dAMode, radMode = 0, sizeMode = 0;
        private static float dimAlpha = 0.05f, dAngleCommon = 0, ellipticFactor = 1.0f;
        private static bool doUseRGB = true;

        private static TexText tTex = null;
        private static int[] Rads = null;
        private static float[] dAngles = null;

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

                    ellipticFactor = myUtils.randomChance(rand, 1, 2)
                        ? 1.0f
                        : 0.8f + myUtils.randFloat(rand) * 0.2f;

                    if (radMode == 1)
                    {
                        int nCircles = 1;

                        switch (rand.Next(3))
                        {
                            case 0: nCircles = 2 + rand.Next(11); break;    // 2 .. 12
                            case 1: nCircles = 4 + rand.Next(09); break;    // 4 .. 12 
                            case 2: nCircles = 6 + rand.Next(07); break;    // 6 .. 12
                        }

                        Rads = new int[nCircles];           // Radius of a circle
                        dAngles = new float[nCircles];      // dAngle for this raduis

                        N = rand.Next(123 * nCircles) + 123;

                        for (int i = 0; i < nCircles; i++)
                        {
                            Rads[i] = minRad + rand.Next(maxRad - minRad);
                            dAngles[i] = myUtils.randFloatSigned(rand, 0.05f) * 0.01f;
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

            // Special mode, where each circle's particles rotate at the same speed
            if (radMode == 1 && myUtils.randomChance(rand, 1, 2))
            {
                dAngleMode = 4;
            }

            size = 50 + rand.Next(66);
            renderDelay = 0;

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            int nCircles = radMode == 0 ? 0 : Rads.Length;

            height = 600;

            string nStr(int   n) { return n.ToString("N0");    }
            string fStr(float f) { return f.ToString("0.000"); }

            string str = $"Obj = {Type}\n\n"                          	 +
                            $"N = {nStr(list.Count)} of {nStr(N)}\n"     +
                            $"doUseRGB = {doUseRGB}\n"                   +
                            $"radMode = {radMode}\n"                     +
                            $"ellipticFactor = {fStr(ellipticFactor)}\n" +
                            $"num of Circles = {nCircles}\n"             +
                            $"sizeMode = {sizeMode}\n"                   +
                            $"dAMode = {dAMode}\n"                       +
                            $"dAngleMode = {dAngleMode}\n"               +
                            $"font = '{tTex.FontFamily()}'\n"            +
                            $"minRad = {minRad}\n"                       +
                            $"maxRad = {maxRad}\n"                       +
                            $"renderDelay = {renderDelay}\n"             +
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
            int circleId = -1;

            cnt = 50 + rand.Next(111);

            x = rand.Next(gl_Width);
            y = rand.Next(gl_Height);

            switch (radMode)
            {
                case 0:
                    radx = minRad + rand.Next(maxRad - minRad);
                    break;

                case 1:
                    circleId = rand.Next(Rads.Length);
                    radx = Rads[circleId];
                    break;
            }

            rady = radx * ellipticFactor;

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

                case 4:
                    dAngle = dAngles[circleId];                         // Value and sign are the same for this particular circle
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

                x = gl_x0 + radx * (float)Math.Sin(angle);
                y = gl_y0 + rady * (float)Math.Cos(angle);
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
            tTex = new TexText(size, doUseRGB, fontStyle: 0);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------
    }
};
