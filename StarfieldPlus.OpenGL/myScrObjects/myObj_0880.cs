using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;


/*
    - Perspective made of sine-cosine graphs
*/


namespace my
{
    public class myObj_0880 : myObject
    {
        // Priority
        public static int Priority => 10;
		public static System.Type Type => typeof(myObj_0880);

        private float xFactor, y, dx;
        private float size, A, R, G, B;
        private float t = 0, dt = 0;

        private static int N = 0, n = 0, colorMode = 0, sizeMode = 0, funcMode = 0;
        private static float dimAlpha = 0.05f;
        private static float dR = 0, dG = 0, dB = 0;

        private static int  [] i_param = new   int[5];
        private static float[] f_param = new float[5];

        private static myScreenGradient grad = null;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_0880()
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
                N = 50;
                n = 100;
            }

            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time local initialization
        private void initLocal()
        {
            doClearBuffer = myUtils.randomChance(rand, 99, 100);

            colorMode = rand.Next(4);
            sizeMode = rand.Next(2);
            funcMode = rand.Next(11);
            renderDelay = 0;

            dR = myUtils.randFloatSigned(rand) * 0.05f;
            dG = myUtils.randFloatSigned(rand) * 0.05f;
            dB = myUtils.randFloatSigned(rand) * 0.05f;

            //funcMode = 0;

            switch (funcMode)
            {
                case 0:
                    i_param[1] = rand.Next(2);
                    i_param[2] = 10 + rand.Next(200);
                    f_param[1] = myUtils.randFloat(rand, 0.01f);
                    break;

                case 1:
                    i_param[0] = 5 + rand.Next(50);
                    i_param[1] = rand.Next(2);
                    i_param[2] = 10 + rand.Next(200);
                    f_param[1] = myUtils.randFloat(rand, 0.01f);
                    break;

                case 2:
                    i_param[1] = rand.Next(2);
                    i_param[2] = 10 + rand.Next(200);
                    f_param[0] = 0.01f + rand.Next(100) * 0.01f;
                    f_param[1] = myUtils.randFloat(rand, 0.01f);
                    break;

                case 3:
                    i_param[0] = 5 + rand.Next(50);
                    i_param[1] = rand.Next(2);
                    i_param[2] = 10 + rand.Next(200);
                    f_param[0] = 0.01f + rand.Next(100) * 0.01f;
                    f_param[1] = myUtils.randFloat(rand, 0.01f);
                    break;

                case 4:
                    i_param[0] = 5 + rand.Next(50);
                    i_param[1] = rand.Next(2);
                    i_param[2] = 10 + rand.Next(200);
                    f_param[0] = 0.001f + myUtils.randFloat(rand) * 0.035f;
                    f_param[1] = myUtils.randFloat(rand, 0.01f);
                    break;

                case 5:
                    i_param[0] = 50 + rand.Next(600);
                    f_param[0] = myUtils.randomChance(rand, 2, 3)
                        ? 0.0005f + myUtils.randFloat(rand) * 0.01f
                        : myUtils.randFloat(rand, 0.001f);
                    break;

                case 6:
                    i_param[0] = 50 + rand.Next(20);
                    f_param[0] = myUtils.randFloat(rand) * 0.001f;
                    break;

                case 7:
                    f_param[0] = myUtils.randomChance(rand, 2, 3)
                        ? myUtils.randFloat(rand) * 0.00003f
                        : myUtils.randFloat(rand) * 0.0003f;
                    break;

                case 8:
                    f_param[0] = myUtils.randFloatClamped(rand, 0.1f);
                    break;

                case 9:
                    i_param[0] = 25 + rand.Next(60);
                    f_param[0] = 0.01f + myUtils.randFloat(rand) * 0.08f;
                    break;
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            height = 600;

            string nStr(int   n) { return n.ToString("N0");    }

            string str = $"Obj = {Type}\n\n"                         +
                            $"N = {nStr(list.Count)} of {nStr(N)}\n" +
                            $"doClearBuffer = {doClearBuffer}\n"     +
                            $"colorMode = {colorMode}\n"             +
                            $"sizeMode = {sizeMode}\n"               +
                            $"funcMode = {funcMode}\n"               +
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
            y = gl_y0 + gl_y0 / 2 - id * 20;

            dx = gl_Width / n;
            t = 0;
            dt = 0.01f + 0.001f * id;
            dt = 0.1f;
            dt = 0.01f;
            //dt = 0.01f + 0.0001f * id;

            xFactor = 1.0f + 0.001f * id;
            xFactor = 1.5f + id * 0.00025f;

            size = 150 - id * 3;

            A = 0.66f - id * 0.01f;

            getColor();

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            t += dt;

            switch (sizeMode)
            {
                case 0:
                    break;

                case 1:
                    size += (float)Math.Sin(t) * 0.25f;
                    break;
            }
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Show()
        {
            float x1 = -100;
            float y1 = 0;
            float x2 = 0;
            float y2 = 0;

            //funcMode = 10;

            for (int i = -1; i < n + 1; i++)
            {
                switch (funcMode)
                {
                    // Flat -- Simple
                    case 0:
                        {
                            y2 = size * (float)Math.Sin(xFactor * t + x2);

                            if (i_param[1] == 1)
                                y2 += (float)Math.Sin(i * f_param[1]) * i_param[2];
                        }
                        break;

                    // Flat -- Complex 1
                    case 1:
                        {
                            y2 = size * (float)Math.Sin(xFactor * t + x2);
                            y2 += (float)Math.Sin(y2 * 0.25f) * i_param[0];

                            if (i_param[1] == 1)
                                y2 += (float)Math.Sin(i * f_param[1]) * i_param[2];
                        }
                        break;

                    // Flat -- Complex 2
                    case 2:
                        {
                            y2 = size * (float)Math.Sin(xFactor * t + x2);
                            y2 += (float)Math.Sin(y2 * f_param[0]) * 33;

                            if (i_param[1] == 1)
                                y2 += (float)Math.Sin(i * f_param[1]) * i_param[2];
                        }
                        break;

                    // Flat -- Complex 3
                    case 3:
                        {
                            y2 = size * (float)Math.Sin(xFactor * t + x2);
                            y2 += (float)Math.Sin(y2 * f_param[0]) * i_param[0];

                            if (i_param[1] == 1)
                                y2 += (float)Math.Sin(i * f_param[1]) * i_param[2];
                        }
                        break;

                    // Flat -- Complex 4
                    case 4:
                        {
                            y2 = size * (float)Math.Sin(xFactor * t + x2);
                            y2 += (float)Math.Sin(y2 * id * f_param[0]) * i_param[0];

                            if (i_param[1] == 1)
                                y2 += (float)Math.Sin(i * f_param[1]) * i_param[2];
                        }
                        break;

                    // Flat -- Complex 5
                    case 5:
                        {
                            y2 = size * (float)Math.Sin(xFactor * t + x2);
                            y2 += (float)Math.Cos(i * id * f_param[0]) * i_param[0];
                        }
                        break;

                    // Flat -- Complex 6
                    case 6:
                        {
                            y2 = size * (float)Math.Sin(xFactor * t + x2);
                            y2 += (float)Math.Cos(i * i * id * f_param[0]) * i_param[0];
                        }
                        break;

                    // Flat -- Complex 7
                    case 7:
                        {
                            y2 = size * (float)Math.Sin(xFactor * t + x2);
                            var ppp = Math.Sin(i * i * id * f_param[0]) * y2 * 0.1;
                            y2 += (float)Math.Cos(ppp) * 66;
                        }
                        break;

                    // Flat -- Complex 8
                    case 8:
                        {
                            y2 = size * (float)Math.Sin(xFactor * t + x2);
                            var ppp = Math.Sin(i * i * 0.0001) * y2 * f_param[0];
                            y2 += (float)Math.Cos(ppp) * 66;
                        }
                        break;

                    // Flat -- Complex 9
                    case 9:
                        {
                            y2 = size * (float)Math.Sin(xFactor * t + y2 * 0.001);
                            y2 += (float)Math.Cos(x1 + t * id * f_param[0]) * i_param[0];
                        }
                        break;

                    case 10:
                        y2 = size * (float)Math.Sin(xFactor * (x2 + t));
                        break;
                }

                myPrimitive._LineInst.setInstanceCoords(x1, y + y1, x2, y + y2);
                myPrimitive._LineInst.setInstanceColor(R, G, B, A);

                x1 = x2;
                x2 = x1 + dx;
                y1 = y2;
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Process(Window window)
        {
            uint cnt = 0;
            initShapes();

            clearScreenSetup(doClearBuffer, 0.1f);

            stopwatch = new StarfieldPlus.OpenGL.myUtils.myStopwatch(true);

            while (list.Count < N)
            {
                list.Add(new myObj_0880());
            }

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
                        grad.Draw();
                    }
                    else
                    {
                        dimScreen(dimAlpha);
                    }
                }

                // Render Frame
                {
                    myPrimitive._LineInst.ResetBuffer();

                    for (int i = 0; i != Count; i++)
                    {
                        var obj = list[i] as myObj_0880;

                        obj.Show();
                        obj.Move();
                    }

                    myPrimitive._LineInst.Draw();
                }

                stopwatch.WaitAndRestart();
                cnt++;
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void initShapes()
        {
            myPrimitive.init_ScrDimmer();
            myPrimitive.init_LineInst((N + 5) * n);

            myPrimitive._LineInst.setLineWidth(3);

            grad = new myScreenGradient();
            grad.SetRandomColors(rand, 0.2f);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void getColor()
        {
            switch (colorMode)
            {
                // Gray
                case 0:
                    R = G = B = 0.33f;
                    break;

                // Color picker
                case 1:
                    colorPicker.getColor(1, y, ref R, ref G, ref B);
                    break;

                // Gradual change starting from gray color
                case 2:
                    {
                        if (id == 0)
                        {
                            R = G = B = 0.5f;
                        }
                        else
                        {
                            R = (list[(int)(id - 1)] as myObj_0880).R + dR;
                            G = (list[(int)(id - 1)] as myObj_0880).G + dG;
                            B = (list[(int)(id - 1)] as myObj_0880).B + dB;
                        }
                    }
                    break;

                // Gradual change starting from random color
                case 3:
                    {
                        if (id == 0)
                        {
                            do
                            {
                                R = myUtils.randFloat(rand);
                                G = myUtils.randFloat(rand);
                                B = myUtils.randFloat(rand);
                            }
                            while ((R + G + B < 0.25f) || (R + G + B > 0.75f));
                        }
                        else
                        {
                            R = (list[(int)(id - 1)] as myObj_0880).R + dR;
                            G = (list[(int)(id - 1)] as myObj_0880).G + dG;
                            B = (list[(int)(id - 1)] as myObj_0880).B + dB;
                        }
                    }
                    break;
            }

            return;
        }
    }
};
