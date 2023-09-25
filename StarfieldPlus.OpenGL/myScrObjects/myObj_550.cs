using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;


/*
    - Orbits of different size + a small planet is moving along each orbit
*/


namespace my
{
    public class myObj_550 : myObject
    {
        // Priority
        public static int Priority => 10;
        public static System.Type Type => typeof(myObj_550);

        private float x, y, X, Y, rad1, rad2, angle, dAngle, lineThickness;
        private float A, R, G, B, a, r, g, b;

        private static int N = 0, mode = 0, dirMode = 0, spdMode = 0, radMode = 0, colorMode = 0, maxRad = 1, maxDist = 0;
        private static float dimAlpha = 0.1f, radFactor = 0;
        private static bool doShowLines = true, doUseGradient = true;

        private static myFreeShader shader1 = null;
        private static myFreeShader shader2 = null;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_550()
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
                switch (rand.Next(3))
                {
                    case 0: N = rand.Next(33) + 11; break;
                    case 1: N = rand.Next(66) + 22; break;
                    case 2: N = rand.Next(99) + 33; break;
                }
            }

            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time local initialization
        private void initLocal()
        {
            doClearBuffer = myUtils.randomChance(rand, 1, 2);
            doShowLines   = myUtils.randomChance(rand, 1, 2);
            doUseGradient = myUtils.randomChance(rand, 1, 2);

            maxDist = 10000 + rand.Next(80001);

            mode = rand.Next(10);

            spdMode = rand.Next(4);

            colorMode = rand.Next(2);

            maxRad = 111 + rand.Next(gl_Width);

            renderDelay = rand.Next(11) + 3;

            dirMode = rand.Next(4);

            radMode = rand.Next(5);

            radFactor = myUtils.randFloat(rand) * 0.5f;

            dimAlpha = 0.1f + myUtils.randFloat(rand) * 0.2f;

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            height = 600;

            string nStr(int   n) { return n.ToString("N0");    }
            string fStr(float f) { return f.ToString("0.000"); }

            string str = $"Obj = myObj_550\n\n"                      +
                            $"N = {nStr(list.Count)} of {nStr(N)}\n" +
                            $"doClearBuffer = {doClearBuffer}\n"     +
                            $"doShowLines = {doShowLines}\n"         +
                            $"doUseGradient = {doUseGradient}\n"     +
                            $"maxRad = {maxRad}\n"                   +
                            $"maxDist = {maxDist}\n"                 +
                            $"mode = {mode}\n"                       +
                            $"spdMode = {spdMode}\n"                 +
                            $"dirMode = {dirMode}\n"                 +
                            $"colorMode = {colorMode}\n"             +
                            $"radMode = {radMode}\n"                 +
                            $"radFactor = {fStr(radFactor)}\n"       +
                            $"renderDelay = {renderDelay}\n"         +
                            $"dimAlpha = {fStr(dimAlpha)}\n"         +
                            $"file: {colorPicker.GetFileName()}"
                ;
            return str;
        }

        // ---------------------------------------------------------------------------------------------------------------

        // Press 'Space' to change mode
        protected override void setNextMode()
        {
            initLocal();

            System.Threading.Thread.Sleep(123);

            clearScreenSetup(doClearBuffer, 0.15f);

            for (int i = 0; i < list.Count; i++)
            {
                (list[i] as myObj_550).generateNew();
            }
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void generateNew()
        {
            angle = myUtils.randFloat(rand);
            dAngle = 0.0005f + myUtils.randFloat(rand) * 0.025f;

            switch (spdMode)
            {
                case 0: dAngle *= 1.00f; break;
                case 1: dAngle *= 0.75f; break;
                case 2: dAngle *= 0.50f; break;
                case 3: dAngle *= 0.25f; break;
            }

            switch (dirMode)
            {
                case 0:
                    dAngle *= +1;
                    break;

                case 1:
                    dAngle *= -1;
                    break;

                case 2:
                case 3:
                    dAngle *= myUtils.randomSign(rand);
                    break;
            }

            lineThickness = 1.0f + myUtils.randFloat(rand) * rand.Next(5);

            switch (mode)
            {
                // Center
                case 0:
                case 1:
                case 2:
                    X = gl_x0;
                    Y = gl_y0;
                    break;

                // Center + x offset
                case 3:
                    X = gl_x0 + rand.Next(222) - 111;
                    Y = gl_y0;
                    break;

                // Center + y offset
                case 4:
                    X = gl_x0;
                    Y = gl_y0 + rand.Next(222) - 111;
                    break;

                // Center + x offset + y offset
                case 5:
                    X = gl_x0 + rand.Next(222) - 111;
                    Y = gl_y0 + rand.Next(222) - 111;
                    break;

                case 6:
                case 7:
                case 8:
                case 9:
                    X = rand.Next(gl_Width);
                    Y = rand.Next(gl_Height);
                    break;
            }

            rad1 = 333 + rand.Next(maxRad);
            rad2 =  10 + rand.Next(10);

            A = 0.1f + myUtils.randFloat(rand) * 0.85f;

            switch (colorMode)
            {
                case 0:
                    colorPicker.getColor(X, Y, ref R, ref G, ref B);
                    break;

                case 1:
                    colorPicker.getColorRand(ref R, ref G, ref B);
                    break;
            }

            a = A;
            r = R;
            g = G;
            b = B;

            A = A / N * 10;

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            angle += dAngle;

            x = X + rad1 * (float)Math.Sin(angle);
            y = Y + rad1 * (float)Math.Cos(angle);

            switch (radMode)
            {
                case 0:
                case 1:
                    rad1 += (float)Math.Sin(angle * radFactor);
                    break;

                default:
                    // rad1 is const
                    break;
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Show()
        {
            if (rad1 >= 0)
            {
                shader1.SetColor(R, G, B, A);
                shader1.Draw(X, Y, rad1, rad1, 10);

                shader2.SetColor(r, g, b, a);
                shader2.Draw(x, y, rad2, rad2, 10);

                if (doShowLines)
                {
                    int Count = list.Count;

                    for (int i = 0; i != Count; i++)
                    {
                        var obj = list[i] as myObj_550;

                        if (obj != this)
                        {
                            float lineOpacity = 0.1f;
                            float xx = obj.x - x;
                            float yy = obj.y - y;
                            float dist2 = xx * xx + yy * yy;

                            if (dist2 < maxDist && obj.rad1 > 0)
                            {
                                myPrimitive._LineInst.setInstanceCoords(obj.x, obj.y, x, y);
                                myPrimitive._LineInst.setInstanceColor(1, 1, 1, lineOpacity);
                            }
                        }
                    }
                }
            }
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Process(Window window)
        {
            var tex = new myTexRectangle(myUtils.getGradientBgr(ref rand, gl_Width, gl_Height));

            uint cnt = 0;
            initShapes();

            clearScreenSetup(doClearBuffer, 0.15f);

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

                    if (doUseGradient)
                    {
                        tex.Draw(0, 0, gl_Width, gl_Height);
                    }
                }

                // Render Frame
                {
                    myPrimitive._LineInst.ResetBuffer();

                    for (int i = 0; i != list.Count; i++)
                    {
                        var obj = list[i] as myObj_550;

                        obj.Show();
                        obj.Move();
                    }

                    myPrimitive._LineInst.Draw();
                }

                if (list.Count < N)
                //if (list.Count < N && cnt == 100)
                {
                    list.Add(new myObj_550());
                    cnt = 0;
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
            myPrimitive.init_LineInst(N * N);

            getShader();

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void getShader()
        {
            shader1 = new myFreeShader($@"
                        float circle(vec2 uv, float rad) {{ return 1.0 - smoothstep(0.0, 0.00175, abs(rad-length(uv))); }}
                    ",

                    $@"
                        vec2 uv = (gl_FragCoord.xy / iResolution.xy * 2.0 - 1.0);

                        uv -= Pos.xy;
                        uv *= aspect;

                        result = vec4(myColor * circle(uv, Pos.z));
                    "
            );

            shader2 = new myFreeShader($@"
                        float circle(vec2 uv, float rad) {{ return smoothstep(rad, rad - 0.005, length(uv)); }}
                    ",

                    $@"
                        vec2 uv = (gl_FragCoord.xy / iResolution.xy * 2.0 - 1.0);

                        uv -= Pos.xy;
                        uv *= aspect;

                        result = vec4(myColor * (circle(uv, Pos.z) + 0.75 * circle(uv, Pos.z * 0.75)));
                    "
            );
        }

        // ---------------------------------------------------------------------------------------------------------------

    }
};
