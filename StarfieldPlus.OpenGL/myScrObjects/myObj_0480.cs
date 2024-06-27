using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;


/*
    - Oscilloscope (running harmonics)
*/


namespace my
{
    public class myObj_0480 : myObject
    {
        // Priority
        public static int Priority => 10;
        public static System.Type Type => typeof(myObj_0480);

        private float x, y, oldx, oldy, dx;
        private float size, A, R, G, B;

        private static int N = 0, shape = 0, dtCount = 0, colorMode = 0, moveMode = 0, DX = 1;
        private static bool doUseDdt = true, doUseNoise = true, doShowLine = true, doUseShader = false;
        private static float dimAlpha = 0.05f, t = 0, dt = 0, ddt = 0, lineTh = 1;

        private static int[] prm_i = new int[5];

        private static myFreeShader_FullScreen shader = null;
        private static myScreenGradient grad = null;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_0480()
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
                N = rand.Next(3) + 1;

                shape = rand.Next(6);

                doUseShader = false;
            }

            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time local initialization
        private void initLocal()
        {
            doClearBuffer = myUtils.randomBool(rand);
            doUseNoise = myUtils.randomChance(rand, 1, 3);      // Add some random noise to the final signal
            doUseDdt = myUtils.randomBool(rand);                // true: dt is going to change over time
            doShowLine = (shape == 5)
                ? false
                : myUtils.randomChance(rand, 1, 2);

            moveMode = rand.Next(13);
            colorMode = rand.Next(2);

            DX = myUtils.randomChance(rand, 4, 5) ? rand.Next(5) + 1 : rand.Next(11) + 1;
            dt = myUtils.randomSign(rand) * myUtils.randFloat(rand, 0.1f) * 0.1f;

            dimAlpha = 0.1f + myUtils.randFloat(rand) * 0.2f;

            lineTh = 0.25f + myUtils.randFloat(rand) * rand.Next(10);

            switch (moveMode)
            {
                case 004:
                    prm_i[0] = rand.Next(100) + 3;
                    break;

                case 005:
                    break;

                case 006:
                    prm_i[0] = rand.Next(13) + 2;
                    prm_i[1] = rand.Next(33) + 1;
                    prm_i[2] = rand.Next(10) + 1;
                    break;
            }

            renderDelay = rand.Next(11) + 3;

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            height = 600;

            string str = $"Obj = {Type}\n\n"                       	 +
                            myUtils.strCountOf(list.Count, N)        +
                            $"shape = {shape}\n"                     +
                            $"moveMode = {moveMode}\n"               +
                            $"colorMode = {colorMode}\n"             +
                            $"doUseShader = {doUseShader}\n"         +
                            $"doClearBuffer = {doClearBuffer}\n"     +
                            $"doShowLine = {doShowLine}\n"           +
                            $"doUseDdt = {doUseDdt}\n"               +
                            $"doUseNoise = {doUseNoise}\n"           +
                            $"lineTh = {myUtils.fStr(lineTh)}\n"     +
                            $"dimAlpha = {myUtils.fStr(dimAlpha)}\n" +
                            $"dx = {DX}\n"                           +
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
            x = 0;
            y = gl_y0;

            dx = DX;

            size = rand.Next(333) + 111;

            switch (colorMode)
            {
                case 0:
                    colorPicker.getColorRand(ref R, ref G, ref B);
                    break;

                case 1:
                    do
                    {
                        R = myUtils.randFloat(rand);
                        G = myUtils.randFloat(rand);
                        B = myUtils.randFloat(rand);
                    }
                    while (R + G + B < 0.33f);
                    break;
            }

            A = 1;

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            t += dt;

            if (doUseDdt)
            {
                if (dtCount == 0)
                {
                    ddt = 0;

                    if (myUtils.randomChance(rand, 1, 33))
                    {
                        if (myUtils.randomChance(rand, 1, 3))
                        {
                            ddt = myUtils.randomSign(rand) * myUtils.randFloat(rand) * 0.001f;
                            dtCount = rand.Next(100);
                        }
                    }
                }
                else
                {
                    dtCount--;
                    dt += ddt;
                }
            }
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Show()
        {
            float xFactor = 0.01f;

            x = 0;
            y = gl_y0 + (float)Math.Sin(x * xFactor + t) * size;

            if (shape < 5)
            {
                inst.ResetBuffer();
            }

            myPrimitive._LineInst.ResetBuffer();

            while (x <= gl_Width)
            {
                oldx = x;
                oldy = y;

                x += dx;
                y = (float)Math.Sin(x * xFactor + t);

                float Y = y;

                switch (moveMode)
                {
                    case 000:
                        y += myUtils.randFloat(rand) * 0.5f;
                        break;

                    case 001:
                        y += (float)Math.Cos(y * t);
                        break;

                    case 002:
                        y += (float)Math.Cos(y * y);
                        y += (float)Math.Cos(y * t);
                        break;

                    case 003:
                        y += (float)Math.Cos(x / 2 * xFactor + t);
                        break;

                    case 004:
                        y += (float)Math.Cos(Math.Cos(x/prm_i[0]));
                        break;

                    case 005:
                        y += (int)(Math.Cos(x * xFactor + t/2) * 3);
                        break;

                    case 006:
                        y += (int)(Math.Cos(x * xFactor / prm_i[2] + t / prm_i[1]) * prm_i[0]) / (prm_i[0] / 2);
                        break;

                    case 007:
                        y += (float)(Math.Sin(Y * Y * Y * t/10) * 50) / 49;
                        break;

                    case 008:
                        y += (int)(Math.Sin(Y * Y * Y * t / 10) * 50) / 49;
                        break;

                    case 009:
                        y += (float)Math.Cos(x / 2 * xFactor + t);
                        y += (float)Math.Sin(x + y);
                        break;

                    case 010:
                        y += (float)Math.Cos(x / 2 * xFactor + t);
                        y += (float)Math.Sin(x + y);
                        y += (float)Math.Cos(y);
                        break;

                    case 011:
                        {
                            y += (float)Math.Cos(y);
                            y += (float)Math.Cos(x);
                        }
                        break;

                    case 012:
                        {
                            //y += (float)(x * Math.Sin(x) * Math.Cos(Y)) / 3000;
                            //y = (float)(x * x * Math.Sin(x) * Math.Cos(Y)) / 3000000;
                            //y = (float)(x * y * Math.Sin(x) * Math.Cos(Y)) / 1000;
                            //y = (float)(x * Math.Sin(x * y * 0.001)) / 1000;
                            y += (float)(Y * Math.Sin(x * Y * 0.001)) * 2;
                        }
                        break;
                }

                if (doUseNoise)
                {
                    y += myUtils.randFloat(rand) * 0.1f;
                }

                y = gl_y0 + y * size;

                switch (shape)
                {
                    // Instanced squares
                    case 0:
                        myPrimitive._RectangleInst.setInstanceCoords(x - 3, y - 3, 6, 6);
                        myPrimitive._RectangleInst.setInstanceColor(R, G, B, A);
                        myPrimitive._RectangleInst.setInstanceAngle(0);
                        break;

                    // Instanced triangles
                    case 1:
                        myPrimitive._TriangleInst.setInstanceCoords(x, y, 6, 0);
                        myPrimitive._TriangleInst.setInstanceColor(R, G, B, A);
                        break;

                    // Instanced circles
                    case 2:
                        myPrimitive._EllipseInst.setInstanceCoords(x, y, 6, 0);
                        myPrimitive._EllipseInst.setInstanceColor(R, G, B, A);
                        break;

                    // Instanced pentagons
                    case 3:
                        myPrimitive._PentagonInst.setInstanceCoords(x, y, 6, 0);
                        myPrimitive._PentagonInst.setInstanceColor(R, G, B, A);
                        break;

                    // Instanced hexagons
                    case 4:
                        myPrimitive._HexagonInst.setInstanceCoords(x, y, 6, 0);
                        myPrimitive._HexagonInst.setInstanceColor(R, G, B, A);
                        break;

                    // Instanced lines
                    case 5:
                        myPrimitive._LineInst.setInstanceCoords(x, y, oldx, oldy);
                        myPrimitive._LineInst.setInstanceColor(R, G, B, A);
                        break;
                }

                if (doShowLine)
                {
                    myPrimitive._LineInst.setInstanceCoords(x, y, oldx, oldy);
                    myPrimitive._LineInst.setInstanceColor(R, G, B, A/2);
                }
            }

            myPrimitive._LineInst.Draw();

            if (shape < 5)
            {
                inst.Draw(false);
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Process(Window window)
        {
            uint cnt = 0;
            initShapes();

            if (doClearBuffer)
            {
                glDrawBuffer(GL_FRONT_AND_BACK | GL_DEPTH_BUFFER_BIT);
                glClearColor(0, 0, 0, 1);
            }
            else
            {
                dimScreenRGB_SetRandom(0.1f);
                glDrawBuffer(GL_FRONT_AND_BACK);
                //glDrawBuffer(GL_DEPTH_BUFFER_BIT);
            }

            while (!Glfw.WindowShouldClose(window))
            {
                int Count = list.Count;

                processInput(window);

                // Swap fore/back framebuffers, and poll for operating system events.
                Glfw.SwapBuffers(window);
                Glfw.PollEvents();

                if (doUseShader)
                {
                    glDrawBuffer(GL_BACK);
                    shader.Draw();
                }
                else
                {
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
                            grad.Draw();
                        }
                    }

                    // Render Frame
                    {
                        glLineWidth(lineTh);

                        for (int i = 0; i != Count; i++)
                        {
                            var obj = list[i] as myObj_0480;

                            obj.Show();
                            obj.Move();
                        }
                    }

                    if (Count < N)
                    {
                        list.Add(new myObj_0480());
                    }
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

            myPrimitive.init_LineInst(gl_Width + 1);

            if (shape < 5)
            {
                base.initShapes(shape, gl_Width + 1, 0);
            }

            grad = new myScreenGradient();
            grad.SetRandomColors(rand, 0.2f);

            if (doClearBuffer == false)
            {
                grad.SetOpacity(0.05f);
            }

            if (doUseShader)
            {
                string fHeader = "", fMain = "";

                getShader(ref fHeader, ref fMain);

                shader = new myFreeShader_FullScreen(fHeader: fHeader, fMain: fMain);
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void getShader(ref string header, ref string main)
        {
            header = $@"
                vec4 myColor = vec4({R}, {G}, {B}, 1.0);
                float t = uTime;
                vec2 uv = (gl_FragCoord.xy - 0.5 * iResolution.xy) / iResolution.y;
            ";

            main = $@"
                    float len = length(uv);

                    float s1 = sin(uv.x + t * 1.0) * 0.25;
                    float s2 = cos(uv.x + t * 0.5) * 0.25;

                    float color = 0;
                    color = sin(uv.x * 25);
                    color = 1 - smoothstep(0, 0.1, abs(uv.y * 20 - color));
                    //color = 1 - smoothstep(0, 0.5, abs(color));

                    result = vec4(myColor.xyz * color, 1);
                ";
        }

        // ---------------------------------------------------------------------------------------------------------------
    }
};
