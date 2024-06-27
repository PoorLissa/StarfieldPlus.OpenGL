using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;


/*
    - Lines 3: Patchwork / Micro Schematics
*/


namespace my
{
    public class myObj_0042 : myObject
    {
        // Priority
        public static int Priority => 10;
		public static System.Type Type => typeof(myObj_0042);

        private int x, y, dx, dy, oldx, oldy, iterCounter, staticCounter, colorCounter;
        private float size, size2x, a, A, R, G, B, dR, dG, dB, angle, dAngle;
        private bool isStatic = false;

        private static int N = 0, moveMode = 0, colorMode = 0, sizeMode = 0;
        private static int shape = 0, baseSize = 0, spd = 0, divider = 0, divX = 1, divY = 1, divMax = 1;
        private static int sinRepeater = 1, sinConst1_i = 1, sinConst2 = 0, sinConstCnt = 0;
        private static float moveConst = 0.0f, dimAlpha = 0.0f, maxOpacity = 0.33f, sinConst1_f = 0, dRstatic, dGstatic, dBstatic, secondOpacityFactor = 1;
        private static bool doShowStatics = false, doReuseStatics = false, doIncrementSinConst = false, doVaryOpacity = true, doUseStrongDim = false;
        private static bool doRotate = false, doDrawTwice = true, doUseShader = false;

        private static myFreeShader shader = null;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_0042()
        {
            generateNew();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time global initialization
        protected override void initGlobal()
        {
            colorPicker = new myColorPicker(gl_Width, gl_Height);
            list = new List<myObject>();

            // Once-per-run settings
            {
                N = 333 + rand.Next(111);
                doClearBuffer = false;
                doDrawTwice = myUtils.randomChance(rand, 1, 2);             // Draw any particle twice to smooth its appearance
                stepsPerFrame = rand.Next(33) + 1;
                shape = rand.Next(5);
                doUseShader = myUtils.randomChance(rand, 1, 2);             // Use custom shader instead of standard shapes

                if (rand.Next(2) == 0)
                {
                    renderDelay = 1 + stepsPerFrame > 25 ? 25 : 1 + stepsPerFrame;
                }
                else
                {
                    renderDelay = rand.Next(7);
                }
            }

            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time local initialization
        private void initLocal()
        {
            maxOpacity = 0.075f + myUtils.randFloat(rand) * 0.33f;
            secondOpacityFactor = 0.25f + myUtils.randFloat(rand) * 0.09f;
            dimAlpha = 0.001f * (rand.Next(10) + 1);

            spd = (rand.Next(2) == 0) ? -1 : rand.Next(20) + 1;         // Only used for dx / dy generation
            baseSize = (rand.Next(7))/3 + 1;                            // The size of the particles (1-2-3)
            sizeMode = rand.Next(4);                                    // Size of particles: Const (0-1); Random (2); Random float < 1 (3)
            divMax = 111 + rand.Next(3333);                             // 
            moveMode = rand.Next(34);                                   // 
            colorMode = rand.Next(7);                                   // Color changing over time mode
            sinRepeater = rand.Next(10) + 1;                            // 

            doShowStatics       = myUtils.randomChance(rand, 1, 2);
            doReuseStatics      = doShowStatics && myUtils.randomChance(rand, 1, 2);
            doIncrementSinConst = myUtils.randomChance(rand, 1, 5);
            doVaryOpacity       = myUtils.randomChance(rand, 3, 5);
            doUseStrongDim      = dimAlpha >= 0.05f ? false : myUtils.randomChance(rand, 1, 2);
            doRotate            = myUtils.randomChance(rand, 1, 2);

            if (doIncrementSinConst)
            {
                sinConst1_i = 0;
                sinConst1_f = 0;
            }
            else
            {
                sinConst1_i = rand.Next(33333) + 1;
                sinConst1_f = myUtils.randFloat(rand);

                // Pick sinConst out of some known 'good' values sometimes:
                if (myUtils.randomChance(rand, 1, 2))
                {
                    int[] arr = { 111, 222, 33, 333 };
                    sinConst1_i = arr[rand.Next(arr.Length)];
                }

                if (myUtils.randomChance(rand, 1, 2))
                {
                    sinConst1_f += sinConst1_i;
                }
            }

            // Get moveConst as a Gaussian distribution [1 .. 10] skewed to the left
            {
                int n = 3, moveConst_i = 0;

                for (int i = 0; i < n; i++)
                {
                    // Get symmetrical distribution...
                    moveConst_i += rand.Next(999 / n);

                    if (rand.Next(2) == 0)
                    {
                        // ... and skew it to the left
                        moveConst_i -= 2 * rand.Next(moveConst_i) / 3;
                    }
                }

                moveConst = 1.0f + moveConst_i * 0.01f;
            }

            // Get divider:
            // More often this divider will be 1, but sometimes it can be [1..4]
            {
                divider = rand.Next(10) + 1;
                divider = divider > 4 ? 1 : divider;

                // In case the divider is less than moveConst, all the particles become static;
                // Avoid that:
                while (divider > moveConst)
                    divider--;
            }

#if false
            moveMode = 2;
            divMax = 123;
            moveConst = 4.01f;
            divider = 2;
            shape = 0;
#endif
            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void setNextMode()
        {
            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            height = 700;

            string getShape(int shape)
            {
                switch (shape)
                {
                    case 0: return "Rectangle"; break;
                    case 1: return "Triangle";  break;
                    case 2: return "Circle";    break;
                    case 3: return "Pentagon";  break;
                    case 4: return "Hexagon";   break;
                }

                return "";
            }

            return $"Obj = {Type}\n\n" 																+
                            myUtils.strCountOf(list.Count, N)                                       +
                            $"doUseShader = {doUseShader}\n"                                        +
                            $"shape = {shape} ({getShape(shape)})\n"                                +
                            $"baseSize = {baseSize}\n"                                              +
                            $"maxOpacity= {maxOpacity.ToString("0.000")}\n"                         +
                            $"secondOpacityFactor = {secondOpacityFactor.ToString("0.000")}\n"      +
                            $"dimAlpha = {dimAlpha.ToString("0.000")}\n"                            +
                            $"moveMode = {moveMode}\n"                                              +
                            $"colorMode = {colorMode}\n"                                            +
                            $"sizeMode = {sizeMode}\n"                                              +
                            $"moveConst = {moveConst}\n"                                            +
                            $"divider = {divider}; "                                                +
                            $"spd = {spd}\n"                                                        +
                            $"divMax = {divMax}\n"                                                  +
                            $"sinRepeater = {sinRepeater}\n"                                        +
                            $"sinConst1_i = {sinConst1_i} (doIncrement = {doIncrementSinConst})\n"  +
                            $"sinConst1_f = {sinConst1_f} (doIncrement = {doIncrementSinConst})\n"  +
                            $"doShowStatics = {doShowStatics}\n"                                    +
                            $"doReuseStatics = {doReuseStatics}\n"                                  +
                            $"doVaryOpacity = {doVaryOpacity}\n"                                    +
                            $"doRotate = {doRotate}\n"                                              +
                            $"doDrawTwice = {doDrawTwice}\n"                                        +
                            $"stepsPerFrame = {stepsPerFrame}\n"                                    +
                            $"renderDelay = {renderDelay}";
            }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void generateNew()
        {
            // Start with random close-to-white color
            R = 1.0f - myUtils.randFloat(rand) / 11;
            G = 1.0f - myUtils.randFloat(rand) / 11;
            B = 1.0f - myUtils.randFloat(rand) / 11;

            a = A = maxOpacity + myUtils.randomSign(rand) * myUtils.randFloat(rand) * 0.05f;

            switch (sizeMode)
            {
                case 0: case 1:
                    size = baseSize;
                    break;

                case 2:
                    size = rand.Next(3) + 1;
                    break;

                // Size is float, less than 1
                // size = 0.3675f; -- min for shape 0-1-3-4
                // size = 0.7250f;  -- min for shape 2
                case 3:
                    switch (shape)
                    {
                        case 2:
                            size = 0.7250f + myUtils.randFloat(rand) * 0.2f;
                            break;

                        default:
                            size = 0.3675f + myUtils.randFloat(rand) * 0.5f;
                            break;
                    }
                    break;
            }

            isStatic = false;
            iterCounter = 0;
            staticCounter = 0;
            colorCounter = rand.Next(777) + 333;
            size2x = size * 2;
            angle = dAngle = 0;

            if (doRotate)
            {
                dAngle = myUtils.randFloat(rand) * myUtils.randomSign(rand) * 0.01f;
            }

#if true
            {

                x = rand.Next(gl_Width);
                y = rand.Next(gl_Height);

                int speed = (spd > 0) ? spd : rand.Next(20) + 1;

                int dist = (int)Math.Sqrt((x - gl_x0) * (x - gl_x0) + (y - gl_y0) * (y - gl_y0));

                // dx and dy are not used in every moveMode;
                // also, their impact is questionable
                dx = (x - gl_x0) * speed / dist;
                dy = (y - gl_y0) * speed / dist;

                oldx = x;
                oldy = y;

            }
#else
            dx = dy = 0;
            do
            {
                x = rand.Next(gl_Width);
                y = rand.Next(gl_Height);

                int speed = 5;

                double dist = Math.Sqrt((x - gl_x0) * (x - gl_x0) + (y - gl_y0) * (y - gl_y0));

                dx = (int)((x - gl_x0) * speed / dist);
                dy = (int)((y - gl_y0) * speed / dist);
            }
            while (dx == 0 && dy == 0);
#endif
            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            angle += dAngle;

            switch (colorMode)
            {
                // Color stays the same
                case 0:
                    break;

                // Totally random color on a rare occasion
                case 1:
                    if (myUtils.randomChance(rand, 1, 10001))
                    {
                        R = myUtils.randFloat(rand);
                        G = myUtils.randFloat(rand);
                        B = myUtils.randFloat(rand);
                    }
                    break;

                // Totally random color each iteration
                case 2:
                    R = myUtils.randFloat(rand);
                    G = myUtils.randFloat(rand);
                    B = myUtils.randFloat(rand);
                    break;

                // Random color from colorPicker, on a rare occasion
                case 3:
                    if (myUtils.randomChance(rand, 1, 10001))
                    {
                        colorPicker.getColorRand(ref R, ref G, ref B);
                    }
                    break;

                // Gradual change of color for each particle;
                // In mode 5, the only difference is, all the particles use the same static dR, dG, dB
                case 4:
                case 5:

                    if (colorCounter == 0)
                    {
                        // Current color life ends
                        float targetR = (float)rand.NextDouble();
                        float targetG = (float)rand.NextDouble();
                        float targetB = (float)rand.NextDouble();

                        colorCounter = rand.Next(333) + 111 - 1;
                        colorCounter *= -1;

                        dR = (R - targetR) / colorCounter;
                        dG = (G - targetG) / colorCounter;
                        dB = (B - targetB) / colorCounter;

                        dRstatic = dR;
                        dGstatic = dG;
                        dBstatic = dB;
                        break;
                    }

                    if (colorCounter == -1)
                    {
                        // Target color transition ends, current color life starts
                        colorCounter = rand.Next(777) + 333;
                        break;
                    }

                    if (colorCounter > 0)
                    {
                        // Current color life goes on
                        colorCounter--;
                        break;
                    }

                    if (colorCounter < 0)
                    {
                        // Transition to the target color in progress
                        R += colorMode == 4 ? dR : dRstatic;
                        G += colorMode == 4 ? dG : dGstatic;
                        B += colorMode == 4 ? dB : dBstatic;
                        colorCounter++;
                        break;
                    }
                    break;

                // Randomly change only one of the R-G-B components
                case 6:
                    {
                        void changeRGB(ref float rgb)
                        {
                            rgb += myUtils.randomSign(rand) * myUtils.randFloat(rand) * 0.1f;
                            rgb = rgb < 0 ? 0 : rgb;
                        }

                        if (myUtils.randomChance(rand, 1, 101))
                        {
                            switch (rand.Next(3))
                            {
                                case 0: changeRGB(ref R); break;
                                case 1: changeRGB(ref G); break;
                                case 2: changeRGB(ref B); break;
                            }
                        }
                    }
                    break;
            }

            switch (moveMode)
            {
                case 0:
                    x += (int)(Math.Sin(y) * moveConst) / divider;
                    y += (int)(Math.Sin(x) * moveConst) / divider;
                    break;

                case 1:
                    x += (int)(Math.Sin(y) * moveConst) / divider;
                    y += (int)(Math.Cos(x) * moveConst) / divider;
                    break;

                case 2:
                    x += (int)(Math.Sin(y + dy) * moveConst) / divider;
                    y += (int)(Math.Sin(x + dx) * moveConst) / divider;
                    break;

                case 3:
                    x += (int)(Math.Sin(y + dy) * moveConst) / divider;
                    y += (int)(Math.Cos(x + dx) * moveConst) / divider;
                    break;

                case 4:
                    x += (int)(Math.Sin(y + dx) * moveConst) / divider;
                    y += (int)(Math.Sin(x + dy) * moveConst) / divider;
                    break;

                case 5:
                    x += (int)(Math.Sin(y + dx) * moveConst) / divider;
                    y += (int)(Math.Cos(x + dy) * moveConst) / divider;
                    break;

                case 6:
                    x += (int)(Math.Sin(y + dx) * moveConst) / divider;
                    y += (int)(Math.Sin(x + dx) * moveConst) / divider;
                    break;

                case 7:
                    x += (int)(Math.Sin(y + dx) * moveConst) / divider;
                    y += (int)(Math.Cos(x + dx) * moveConst) / divider;
                    break;

                case 8:
                    x += (int)(Math.Sin(y + dy) * moveConst) / divider;
                    y += (int)(Math.Sin(x + dy) * moveConst) / divider;
                    break;

                case 9:
                    x += (int)(Math.Sin(y + dy) * moveConst) / divider;
                    y += (int)(Math.Cos(x + dy) * moveConst) / divider;
                    break;

                case 10:
                    x += (int)(Math.Sin(y * dy) * moveConst) / divider;
                    y += (int)(Math.Sin(x * dx) * moveConst) / divider;
                    break;

                case 11:
                    x += (int)(Math.Sin(y * dy) * moveConst) / divider;
                    y += (int)(Math.Cos(x * dx) * moveConst) / divider;
                    break;

                case 12:
                    x += (int)(Math.Sin(y * dx) * moveConst) / divider;
                    y += (int)(Math.Sin(x * dy) * moveConst) / divider;
                    break;

                case 13:
                    x += (int)(Math.Sin(y * dx) * moveConst) / divider;
                    y += (int)(Math.Cos(x * dy) * moveConst) / divider;
                    break;

                case 14:
                    x += (int)(Math.Sin(y * dx) * moveConst) / divider;
                    y += (int)(Math.Sin(x * dx) * moveConst) / divider;
                    break;

                case 15:
                    x += (int)(Math.Sin(y * dx) * moveConst) / divider;
                    y += (int)(Math.Cos(x * dx) * moveConst) / divider;
                    break;

                case 16:
                    x += (int)(Math.Sin(y * dy) * moveConst) / divider;
                    y += (int)(Math.Sin(x * dy) * moveConst) / divider;
                    break;

                case 17:
                    x += (int)(Math.Sin(y * dy) * moveConst) / divider;
                    y += (int)(Math.Cos(x * dy) * moveConst) / divider;
                    break;

                // --- % variations ---

                case 18:
                    divX = divY = rand.Next(divMax) + 1;
                    x += (int)(Math.Sin(y % divY) * moveConst) / divider;
                    y += (int)(Math.Sin(x % divX) * moveConst) / divider;
                    break;

                case 19:
                    divX = 1 + rand.Next(divMax);
                    divY = 1 + rand.Next(divMax);
                    x += (int)(Math.Sin(y % divY) * moveConst) / divider;
                    y += (int)(Math.Sin(x % divX) * moveConst) / divider;
                    break;

                case 20:
                    divX = divY = rand.Next(divMax) + 1;
                    x += (int)(Math.Sin(y % divY) * moveConst) / divider;
                    y += (int)(Math.Cos(x % divX) * moveConst) / divider;
                    break;

                case 21:
                    divX = 1 + rand.Next(divMax);
                    divY = 1 + rand.Next(divMax);
                    x += (int)(Math.Sin(y % divY) * moveConst) / divider;
                    y += (int)(Math.Cos(x % divX) * moveConst) / divider;
                    break;

                case 22:
                    x += (int)(Math.Sin(y % divMax) * moveConst) / divider;
                    y += (int)(Math.Sin(x % divMax) * moveConst) / divider;
                    break;

                case 23:
                    x += (int)(Math.Sin(y % divMax) * moveConst) / divider;
                    y += (int)(Math.Cos(x % divMax) * moveConst) / divider;
                    break;

                case 24:
                    x += (int)(Math.Sin(y % divMax + y) * moveConst) / divider;
                    y += (int)(Math.Sin(x % divMax + x) * moveConst) / divider;
                    break;

                case 25:
                    x += (int)(Math.Sin(y % divMax + y) * moveConst) / divider;
                    y += (int)(Math.Cos(x % divMax + x) * moveConst) / divider;
                    break;

                // --- Sin Repeater variations ---

                case 26:
                    x += (int)(SinRepeat(y, sinRepeater) * moveConst) / divider;
                    y += (int)(SinRepeat(x, sinRepeater) * moveConst) / divider;
                    break;

                case 27:
                    sinRepeater = rand.Next(3) + 1;
                    x += (int)(SinRepeat(y, sinRepeater) * moveConst) / divider;
                    y += (int)(SinRepeat(x, sinRepeater) * moveConst) / divider;
                    break;

                case 28:
                    x += (int)(SinRepeat(y * y, sinRepeater) * moveConst) / divider;
                    y += (int)(SinRepeat(x * x, sinRepeater) * moveConst) / divider;
                    break;

                case 29:
                    sinRepeater = rand.Next(3) + 1;
                    x += (int)(SinRepeat(y * y, sinRepeater) * moveConst) / divider;
                    y += (int)(SinRepeat(x * x, sinRepeater) * moveConst) / divider;
                    break;

                case 30:
                    x += (int)(SinRepeat(y * sinConst1_i, sinRepeater) * moveConst) / divider;
                    y += (int)(SinRepeat(x * sinConst1_i, sinRepeater) * moveConst) / divider;
                    break;

                case 31:
                    sinRepeater = rand.Next(3) + 1;
                    x += (int)(SinRepeat(y * sinConst1_i, sinRepeater) * moveConst) / divider;
                    y += (int)(SinRepeat(x * sinConst1_i, sinRepeater) * moveConst) / divider;
                    break;

                case 32:
                    sinConst2 = sinConst1_i >= 10 ? sinConst1_i / 10 : 3;

                    x += (int)(SinRepeat(y * sinConst1_i, sinRepeater) * moveConst) / divider;
                    y += (int)(SinRepeat(x * sinConst1_i, sinRepeater) * moveConst) / divider;

                    x += (int)(SinRepeat(y * sinConst2 + y % sinConst2, sinRepeater) * moveConst) / divider;
                    y += (int)(SinRepeat(x * sinConst2 + x % sinConst2, sinRepeater) * moveConst) / divider;
                    break;

                case 33:
                    x += (int)(SinRepeat(y * sinConst1_f, sinRepeater) * moveConst) / divider;
                    y += (int)(SinRepeat(x * sinConst1_f, sinRepeater) * moveConst) / divider;
                    break;

                case 199:

                    if (false)
                    {
                        x += (int)(Math.Sin(y + (int)(10 * Math.Sin(y))) * moveConst) / divider;
                        y += (int)(Math.Sin(x + (int)(10 * Math.Sin(x))) * moveConst) / divider;
                    }

                    if (false)
                    {
                        int n = 10;

                        int arg = (int)(n * Math.Sin(y) + n);
                        arg = arg == 0 ? 900000 : arg;
                        x += (int)(Math.Sin(y % arg) * moveConst) / divider;

                        arg = (int)(n * Math.Sin(x) + n);
                        arg = arg == 0 ? 900000 : arg;
                        y += (int)(Math.Sin(x % arg) * moveConst) / divider;
                    }

                    break;

#if false
                case 995:
                    X += (int)(Math.Sin(Y * Math.Tan(time)) * 3);
                    Y += (int)(Math.Sin(X * Math.Tan(time)) * 3);
                    break;

                case 994:
                    X += (int)(Math.Sin(Y * Math.Sin(time)) * 3);
                    Y += (int)(Math.Sin(X * Math.Cos(time)) * 3);
                    break;

                case 993:
                    X += (int)(Math.Sin(Y * Math.Sin(time)) * 3);
                    Y += (int)(Math.Sin(X * Math.Sin(time)) * 3);
                    break;

                case 992:
                    X += (int)(Math.Sin(Y * time) * 3);
                    Y += (int)(Math.Sin(X * time) * 3);
                    break;

                case 991:
                    X += (int)(Math.Sin(Y * time) * 3);
                    Y += (int)(Math.Cos(X * time) * 3);
                    break;
#endif
            }

            // Find the shapes that are relatively small and static
            if (!isStatic)
            {
                if (iterCounter < 999)
                {
                    if (x == oldx && y == oldy)
                        staticCounter++;

                    iterCounter++;
                }
                else
                {
                    // Static counter > 100 in 1000 iterations: this shape is probably static
                    if (staticCounter > 100)
                    {
                        // Conceal the shape
                        dimScreenRGB_Get(ref R, ref G, ref B);
                        a = A = 0.05f + myUtils.randFloat(rand) * 0.01f;

                        if (++iterCounter > 1111)
                        {
                            if (doReuseStatics)
                            {
                                generateNew();
                            }
                            else
                            {
                                isStatic = true;
                            }
                        }
                    }
                }
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Show()
        {
            if (!isStatic || doShowStatics)
            {
                if (doVaryOpacity)
                {
                    a = A + myUtils.randomSign(rand) * myUtils.randFloat(rand) * 0.33f;
                }

                if (doUseShader)
                {
                    shader.SetColor(R, G, B, a);
                    shader.Draw(x, y, size, size, 13);
                }
                else
                {
                    switch (shape)
                    {
                        case 0:
                            {
                                myPrimitive._RectangleInst.setInstanceCoords(x - size, y - size, size2x, size2x);
                                myPrimitive._RectangleInst.setInstanceColor(R, G, B, a);
                                myPrimitive._RectangleInst.setInstanceAngle(angle);

                                if (doDrawTwice)
                                {
                                    myPrimitive._RectangleInst.setInstanceCoords(x - size - 1, y - size - 1, size2x + 2, size2x + 2);
                                    myPrimitive._RectangleInst.setInstanceColor(R, G, B, a * secondOpacityFactor);
                                    myPrimitive._RectangleInst.setInstanceAngle(angle);
                                }
                            }
                            break;

                        case 1:
                            {
                                myPrimitive._TriangleInst.setInstanceCoords(x, y, size, angle);
                                myPrimitive._TriangleInst.setInstanceColor(R, G, B, a);

                                if (doDrawTwice)
                                {
                                    myPrimitive._TriangleInst.setInstanceCoords(x, y - 1, size + 1, angle);
                                    myPrimitive._TriangleInst.setInstanceColor(R, G, B, a * secondOpacityFactor);
                                }
                            }
                            break;

                        case 2:
                            {
                                myPrimitive._EllipseInst.setInstanceCoords(x, y, size2x, angle);
                                myPrimitive._EllipseInst.setInstanceColor(R, G, B, a);

                                if (doDrawTwice)
                                {
                                    myPrimitive._EllipseInst.setInstanceCoords(x, y, size2x + 3, angle);
                                    myPrimitive._EllipseInst.setInstanceColor(R, G, B, a * secondOpacityFactor);
                                }
                            }
                            break;

                        case 3:
                            {
                                myPrimitive._PentagonInst.setInstanceCoords(x, y, size2x, angle);
                                myPrimitive._PentagonInst.setInstanceColor(R, G, B, a);

                                if (doDrawTwice)
                                {
                                    myPrimitive._PentagonInst.setInstanceCoords(x, y, size2x + 2, angle);
                                    myPrimitive._PentagonInst.setInstanceColor(R, G, B, a * secondOpacityFactor);
                                }
                            }
                            break;

                        case 4:
                            {
                                myPrimitive._HexagonInst.setInstanceCoords(x, y, size2x, angle);
                                myPrimitive._HexagonInst.setInstanceColor(R, G, B, a);

                                if (doDrawTwice)
                                {
                                    myPrimitive._HexagonInst.setInstanceCoords(x, y, size2x + 2, angle);
                                    myPrimitive._HexagonInst.setInstanceColor(R, G, B, a * secondOpacityFactor);
                                }
                            }
                            break;
                    }
                }
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Process(Window window)
        {
            int cnt = 0, i = 0, step = 0, maxIter = 3333 + rand.Next(3333);
            initShapes();

            dimScreenRGB_SetRandom(0.1f);
            glDrawBuffer(GL_FRONT_AND_BACK);

            // Disable VSYNC, as we need to draw fast in this mode
            Glfw.SwapInterval(0);

            for (i = 0; i < N; i++)
            {
                list.Add(new myObj_0042());
            }

            while (!Glfw.WindowShouldClose(window))
            {
                int staticsCnt = 0;

                processInput(window);

                // Swap fore/back framebuffers, and poll for operating system events.
                Glfw.SwapBuffers(window);
                Glfw.PollEvents();

                dimScreen(dimAlpha, doUseStrongDim);

                int Count = list.Count;

                // Render frame
                if (doUseShader)
                {
                    for (step = 0; step != stepsPerFrame; step++)
                    {
                        for (i = 0; i != Count; i++)
                        {
                            var obj = list[i] as myObj_0042;

                            if (obj.isStatic)
                                staticsCnt++;

                            obj.Show();
                            obj.Move();
                        }
                    }
                }
                else
                {
                    inst.ResetBuffer();

                    for (step = 0; step != stepsPerFrame; step++)
                    {
                        for (i = 0; i != Count; i++)
                        {
                            var obj = list[i] as myObj_0042;

                            if (obj.isStatic)
                                staticsCnt++;

                            obj.Show();
                            obj.Move();
                        }
                    }

                    inst.SetColorA(0);
                    inst.Draw(false);
                }

                cnt++;
                System.Threading.Thread.Sleep(renderDelay);

                if (doIncrementSinConst)
                {
                    if (++sinConstCnt > 666)
                    {
                        sinConstCnt = 0;
                        sinConst1_i++;
                        sinConst1_f += (float)(rand.NextDouble() * 0.01);
                    }
                }
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void initShapes()
        {
            myPrimitive.init_ScrDimmer();
            // myPrimitive.init_LineInst(N * stepsPerFrame);

            if (doUseShader)
            {
                getShader();
            }
            else
            {
                base.initShapes(shape, N * stepsPerFrame * (doDrawTwice ? 2 : 1), 0);
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        float SinRepeat(float val, int cnt)
        {
            double d = Math.Sin(val);

            while (--cnt != 0)
                d = Math.Sin(d);

            return (float)d;
        }

        // ---------------------------------------------------------------------------------------------------------------

        float CosRepeat(float val, int cnt)
        {
            double d = Math.Cos(val);

            while (--cnt != 0)
                d = Math.Cos(d);

            return (float)d;
        }

        // ---------------------------------------------------------------------------------------------------------------

        float SinCosRepeat(float val, int cnt)
        {
            int i = 0;
            double d = Math.Sin(val);

            while (--cnt != 0)
                d = (++i % 2 == 0) ? Math.Sin(d) : Math.Cos(d);

            return (float)d;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void getShader()
        {
            string fHeader = "", fMain = "";

            getShader_000(ref fHeader, ref fMain);

            shader = new myFreeShader(fHeader, fMain);
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void getShader_000(ref string h, ref string m)
        {
            string myCircleFunc = "return 1.0 - smoothstep(0.0, 0.005, abs(rad-length(uv)));";

            h = $@"float circle(vec2 uv, float rad) {{ {myCircleFunc} }};";

            m = @"vec2 uv = (gl_FragCoord.xy / iResolution.xy * 2.0 - 1.0);

                  uv -= Pos.xy;
                  uv *= aspect;

                  float rad = Pos.z;
                  float circ = circle(uv, rad);

                  result = vec4(myColor.xyz * circ, myColor.w * circ);
            ";
        }

        // ---------------------------------------------------------------------------------------------------------------

    };
};
