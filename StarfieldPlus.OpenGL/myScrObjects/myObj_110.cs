using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;


/*
    - Puts random colored shapes all over the screen
*/


namespace my
{
    public class myObj_110 : myObject
    {
        // Priority
        public static int Priority => 10;
		public static System.Type Type => typeof(myObj_110);

        private int x, y, w, h, size, cnt;
        private float A, R, G, B, angle = 0;

        private static int N = 0, shape = 0, borderMode = 0, borderOpacityMode = 0, shapeOpacityMode = 0, angleMode = 0,
                           maxSize = 0, maxCnt = 0;
        private static bool doUseRandomSize = false, doUseWasH = false;
        private static float lineWidth = 0, dimAlpha = 0.05f, brdrR = 0, brdrG = 0, brdrB = 0;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_110()
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
                N = myUtils.randomChance(rand, 4, 5) ? 1 : rand.Next(5) + 1;
            }

            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time local initialization
        private void initLocal()
        {
            shape = rand.Next(6);                                       // Extra one for a random shape selection

            doClearBuffer = myUtils.randomChance(rand, 1, 5);
            dimAlpha = myUtils.randFloat(rand) * 0.01f * N;

            doUseRandomSize = myUtils.randomChance(rand, 1, 2);
            doUseWasH       = myUtils.randomChance(rand, 1, 2);         // Whether h == w

            borderMode = rand.Next(8);
            borderOpacityMode = rand.Next(6);
            shapeOpacityMode = rand.Next(10);
            angleMode = rand.Next(7);

            // Custom border color
            brdrR = myUtils.randFloat(rand);
            brdrG = myUtils.randFloat(rand);
            brdrB = myUtils.randFloat(rand);

            // Border line width
            lineWidth = 0.25f + myUtils.randFloat(rand) + rand.Next(3);

            switch (rand.Next(21))
            {
                case 0:
                    maxSize = rand.Next(666) + 1;
                    break;

                case 1: case 2:
                    maxSize = rand.Next(444) + 25;
                    break;

                case 3: case 4: case 5:
                    maxSize = rand.Next(333) + 50;
                    break;

                case 6: case 7: case 8: case 9: case 10:
                    maxSize = rand.Next(222) + 75;
                    break;

                case 11: case 12: case 13: case 14: case 15:
                    maxSize = rand.Next(111) + 100;
                    break;

                case 16: case 17: case 18: case 19: case 20:
                    maxSize = rand.Next(50) + 50;
                    break;
            }

            maxCnt = rand.Next(N * 3) + N;
            renderDelay = rand.Next(20);

#if false
            shape = 0;
            maxSize = 127;
            angleMode = 0;
            doUseRandomSize = false;

            borderMode = 0;
            borderOpacityMode = 1;
            shapeOpacityMode = 6;
            angleMode = 6;
            dimAlpha = 0.006f;
            lineWidth = 2.224f;
            renderDelay = 5;
            maxCnt = 3;
#endif

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            height = 600;

            string nStr(int   n) { return n.ToString("N0");    }
            string fStr(float f) { return f.ToString("0.000"); }

            string str = $"Obj = {Type}\n\n"                          	 +
                            $"N = {nStr(list.Count)} of {nStr(N)}\n"     +
                            $"doClearBuffer = {doClearBuffer}\n"         +
                            $"shape = {shape}\n"                         +
                            $"maxSize = {maxSize}\n"                     +
                            $"doUseRandomSize = {doUseRandomSize}\n"     +
                            $"borderMode = {borderMode}\n"               +
                            $"borderOpacityMode = {borderOpacityMode}\n" +
                            $"shapeOpacityMode = {shapeOpacityMode}\n"   +
                            $"angleMode = {angleMode}\n"                 +
                            $"dimAlpha = {fStr(dimAlpha)}\n"             +
                            $"lineWidth = {fStr(lineWidth)}\n"           +
                            $"renderDelay = {renderDelay}\n"             +
                            $"maxCnt = {maxCnt}\n"                       +
                            $"file: {colorPicker.GetFileName()}\n"       +
                            $"colorMode = {colorPicker.getModeStr()}"
                ;
            return str;
        }

        // ---------------------------------------------------------------------------------------------------------------

        // 
        protected override void setNextMode()
        {
            dimScreen(0.5f);

            initLocal();

            dimScreen(0.5f);
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void generateNew()
        {
            // Overall speed factor
            cnt = rand.Next(maxCnt);

            if (doUseRandomSize)
            {
                size = rand.Next(maxSize) + 3;
            }
            else
            {
                size = maxSize > 333 ? maxSize/3 : maxSize;
            }

            x = rand.Next(gl_Width);
            y = rand.Next(gl_Height);

            colorPicker.getColor(x, y, ref R, ref G, ref B);

            x -= size;
            y -= size;

            if (doUseWasH)
            {
                w = size * 2;
                h = size * 2;
            }
            else
            {
                w = size * 2;
                h = size * 1;
            }

            // Adjust the color in case colorPicker returns const color
            if (colorPicker.getMode() == (int)myColorPicker.colorMode.SINGLE_RANDOM)
            {
                R += myUtils.randomSign(rand) * myUtils.randFloat(rand) * 0.1f;
                G += myUtils.randomSign(rand) * myUtils.randFloat(rand) * 0.1f;
                B += myUtils.randomSign(rand) * myUtils.randFloat(rand) * 0.1f;
            }

            // Adjust the color in case colorPicker returns const color
            if (colorPicker.getMode() == (int)myColorPicker.colorMode.GRAY)
            {
                R += myUtils.randomSign(rand) * myUtils.randFloat(rand) * 0.1f;
                G += myUtils.randomSign(rand) * myUtils.randFloat(rand) * 0.1f;
                B += myUtils.randomSign(rand) * myUtils.randFloat(rand) * 0.1f;
            }

            switch (shapeOpacityMode)
            {
                case 0:
                    A = 0.1f;
                    break;

                case 1:
                    A = 0.25f;
                    break;

                case 2:
                    A = 0.5f;
                    break;

                case 3:
                    A = 0.75f;
                    break;

                case 4:
                    A = myUtils.randFloat(rand) * 0.1f;
                    break;

                case 5:
                    A = myUtils.randFloat(rand) * 0.25f;
                    break;

                case 6:
                    A = myUtils.randFloat(rand) * 0.5f;
                    break;

                case 7:
                    A = myUtils.randFloat(rand) * 0.75f;
                    break;

                case 8:
                    A = 0.1f * (rand.Next(10) + 1);
                    break;

                case 9:
                    A = myUtils.randFloat(rand);
                    break;
            }

            switch (angleMode)
            {
                case 0: case 1: case 2:
                    angle = 0;
                    break;

                case 3:
                    angle = myUtils.randomSign(rand) * myUtils.randFloat(rand) * 0.10f;
                    break;

                case 4:
                    angle = myUtils.randomSign(rand) * myUtils.randFloat(rand) * 0.25f;
                    break;

                case 5:
                    angle = myUtils.randomSign(rand) * myUtils.randFloat(rand) * 0.50f;
                    break;

                case 6:
                    angle = myUtils.randomSign(rand) * myUtils.randFloat(rand);
                    break;
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            if (cnt > 0)
            {
                cnt--;
            }
            else
            {
                generateNew();
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Show()
        {
            if (cnt == 0)
            {
                float bR = 0, bG = 0, bB = 0, bA = 0;

                switch (borderMode)
                {
                    // No border
                    case 0:
                        break;

                    // The same color
                    case 1:
                        bR = R;
                        bG = G;
                        bB = B;
                        break;

                    // Black
                    case 2:
                        bR = 0;
                        bG = 0;
                        bB = 0;
                        break;

                    // Dark random
                    case 3:
                        bR = myUtils.randFloat(rand) * 0.1f;
                        bG = myUtils.randFloat(rand) * 0.1f;
                        bB = myUtils.randFloat(rand) * 0.1f;
                        break;

                    // White
                    case 4:
                        bR = 1;
                        bG = 1;
                        bB = 1;
                        break;

                    // Light random
                    case 5:
                        bR = 1.0f - myUtils.randFloat(rand) * 0.1f;
                        bG = 1.0f - myUtils.randFloat(rand) * 0.1f;
                        bB = 1.0f - myUtils.randFloat(rand) * 0.1f;
                        break;

                    // Custom color
                    case 6:
                        bR = brdrR;
                        bG = brdrG;
                        bB = brdrB;
                        break;

                    // Random color
                    case 7:
                        bR = myUtils.randFloat(rand);
                        bG = myUtils.randFloat(rand);
                        bB = myUtils.randFloat(rand);
                        break;
                }

                switch (borderOpacityMode)
                {
                    case 0:
                        bA = A;
                        break;

                    case 1:
                        bA = A * 2;
                        break;

                    case 2:
                        bA = A * 3;
                        break;

                    case 3:
                        bA = 0.5f;
                        break;

                    case 4:
                        bA = 1.0f;
                        break;

                    case 5:
                        bA = myUtils.randFloat(rand, 0.1f);
                        break;
                }

                int Shape = (shape < 5) ? shape : rand.Next(5);

                switch (Shape)
                {
                    // Rectangle
                    case 0:
                        myPrimitive._Rectangle.SetAngle(angle);
                        myPrimitive._Rectangle.SetColor(R, G, B, A);
                        myPrimitive._Rectangle.Draw(x, y, w, h, true);

                        myPrimitive._Rectangle.SetColor(bR, bG, bB, bA);
                        myPrimitive._Rectangle.Draw(x, y, w, h, false);
                        break;

                    // Triangle
                    case 1:
                        myPrimitive._Triangle.SetAngle(angle);

                        if (w == h)
                        {
                            myPrimitive._Triangle.SetColor(R, G, B, A);
                            myPrimitive._Triangle.Draw(x, y - w, x - 5 * w / 6, y + w / 2, x + 5 * w / 6, y + w / 2, true);

                            myPrimitive._Triangle.SetColor(bR, bG, bB, bA);
                            myPrimitive._Triangle.Draw(x, y - w, x - 5 * w / 6, y + w / 2, x + 5 * w / 6, y + w / 2, false);
                        }
                        else
                        {
                            int x1 = x + size + myUtils.randomSign(rand) * rand.Next(size / 2);
                            int y1 = y + rand.Next(size / 2);

                            int x2 = x + myUtils.randomSign(rand) * rand.Next(size / 2);
                            int y2 = y + w + myUtils.randomSign(rand) * rand.Next(size / 2);

                            int x3 = x + w + myUtils.randomSign(rand) * rand.Next(size / 2);
                            int y3 = y + w + myUtils.randomSign(rand) * rand.Next(size / 2);

                            myPrimitive._Triangle.SetColor(R, G, B, A);
                            myPrimitive._Triangle.Draw(x1, y1, x2, y2, x3, y3, true);

                            myPrimitive._Triangle.SetColor(bR, bG, bB, bA);
                            myPrimitive._Triangle.Draw(x1, y1, x2, y2, x3, y3, false);
                        }
                        break;

                    // Circle
                    case 2:
                        myPrimitive._Ellipse.SetColor(R, G, B, A);
                        myPrimitive._Ellipse.Draw(x, y, w, h, true);

                        myPrimitive._Ellipse.SetColor(bR, bG, bB, bA);
                        myPrimitive._Ellipse.Draw(x, y, w, h, false);
                        break;

                    // Pentagon
                    case 3:
                        myPrimitive._Pentagon.SetAngle(angle);
                        myPrimitive._Pentagon.SetColor(R, G, B, A);
                        myPrimitive._Pentagon.Draw(x, y, w, true);

                        myPrimitive._Pentagon.SetColor(bR, bG, bB, bA);
                        myPrimitive._Pentagon.Draw(x, y, w, false);
                        break;

                    // Hexagon
                    case 4:
                        myPrimitive._Hexagon.SetAngle(angle);
                        myPrimitive._Hexagon.SetColor(R, G, B, A);
                        myPrimitive._Hexagon.Draw(x, y, w, true);

                        myPrimitive._Hexagon.SetColor(bR, bG, bB, bA);
                        myPrimitive._Hexagon.Draw(x, y, w, false);
                        break;
                }
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

            dimScreenRGB_SetRandom(0.1f);

            if (doClearBuffer)
            {
                glDrawBuffer(GL_FRONT_AND_BACK);
                glClearColor(myObject.bgrR, myObject.bgrG, myObject.bgrB, 1);
            }
            else
            {
                glDrawBuffer(GL_BACK);
                glDrawBuffer(GL_FRONT_AND_BACK);
            }

            while (!Glfw.WindowShouldClose(window))
            {
                processInput(window);

                // Swap fore/back framebuffers, and poll for operating system events.
                Glfw.SwapBuffers(window);
                Glfw.PollEvents();

                glLineWidth(lineWidth);

                // Dim screen
                {
                    if (doClearBuffer)
                    {
                        dimScreen(dimAlpha);
                    }
                }

                // Render Frame
                {
                    for (int i = 0; i != list.Count; i++)
                    {
                        var obj = list[i] as myObj_110;

                        obj.Show();
                        obj.Move();
                    }
                }

                if (list.Count < N)
                {
                    list.Add(new myObj_110());
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

            myPrimitive.init_Rectangle();
            myPrimitive.init_Triangle();
            myPrimitive.init_Ellipse();
            myPrimitive.init_Pentagon();
            myPrimitive.init_Hexagon();

            myUtils.SetAntializingMode(true);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------
    }
};
