using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;


/*
    - Orbits of different size, a small planet is moving along each orbit
*/


namespace my
{
    public class myObj_550 : myObject
    {
        // Priority
        public static int Priority => 10;

        private float x, y, rad1, rad2, angle, dAngle, lineThickness;
        private float A, R, G, B;

        private static int N = 0, mode = 0, dirMode = 0, maxRad = 1;
        private static float dimAlpha = 0.1f;

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
                N = rand.Next(33) + 11;
            }

            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time local initialization
        private void initLocal()
        {
            doClearBuffer = myUtils.randomChance(rand, 1, 2);

            mode = rand.Next(2);

            maxRad = 111 + rand.Next(gl_Width);

            renderDelay = rand.Next(11) + 3;

            dirMode = rand.Next(4);

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
                            $"maxRad = {maxRad}\n"                   +
                            $"mode = {mode}\n"                       +
                            $"dirMode = {dirMode}\n"                 +
                            $"renderDelay = {renderDelay}\n"         +
                            $"dimAlpha = {fStr(dimAlpha)}\n"         +
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
            angle = myUtils.randFloat(rand);
            dAngle = 0.0005f + myUtils.randFloat(rand) * 0.025f;

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
                case 0:
                    x = gl_x0;
                    y = gl_y0;
                    break;

                case 1:
                    x = rand.Next(gl_Width);
                    y = rand.Next(gl_Height);
                    break;
            }

            rad1 = 333 + rand.Next(maxRad);
            rad2 =  10 + rand.Next(10);

            A = 0.1f + myUtils.randFloat(rand) * 0.85f;
            colorPicker.getColor(x, y, ref R, ref G, ref B);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            angle += dAngle;

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Show()
        {
            float orbitOpacity = A / N * 10;

            shader1.SetColor(R, G, B, orbitOpacity);
            shader1.Draw(x, y, rad1, rad1, 10);

            float xx = x + rad1 * (float)Math.Sin(angle);
            float yy = y + rad1 * (float)Math.Cos(angle);

            shader2.SetColor(R, G, B, A);
            shader2.Draw(xx, yy, rad2, rad2, 10);
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Process(Window window)
        {
            uint cnt = 0;
            initShapes();

            getShader();

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
                //glDrawBuffer(GL_DEPTH_BUFFER_BIT);
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
                    for (int i = 0; i != list.Count; i++)
                    {
                        var obj = list[i] as myObj_550;

                        obj.Show();
                        obj.Move();
                    }
                }

                if (list.Count < N)
                {
                    list.Add(new myObj_550());
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

                        result = vec4(myColor * circle(uv, Pos.z));
                    "
            );
        }

        // ---------------------------------------------------------------------------------------------------------------

    }
};
