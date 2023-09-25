using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;


/*
    - Pseudo-3d-rain
*/


namespace my
{
    public class myObj_031 : myObject
    {
        // Priority
        public static int Priority => 10;
		public static System.Type Type => typeof(myObj_031);

        private int stage, depth, bottom, count;
        private float x, y, dx, dy;
        private float size, A, R, G, B;

        private static int N = 0, shape = 0, ellipticMode = 0, xFlowMode = 0;
        private static float dimAlpha = 0.05f, xWindValue = 0;

        private static myFreeShader shader = null;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_031()
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
                N = rand.Next(10) + 10;
                N = 3000;
            }

            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time local initialization
        private void initLocal()
        {
            doClearBuffer = myUtils.randomChance(rand, 10, 11);

            ellipticMode = rand.Next(4);
            xFlowMode = rand.Next(2);
            dimAlpha = 0.35f;
            xWindValue = myUtils.randFloat(rand) * (rand.Next(4) + 0.001f);

            renderDelay = rand.Next(11);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            height = 600;

            string nStr(int   n) { return n.ToString("N0");    }
            string fStr(float f) { return f.ToString("0.000"); }

            string str = $"Obj = myObj_031 -- TBD: implement ellipse at last!\n\n" +
                            $"N = {nStr(list.Count)} of {nStr(N)}\n"               +
                            $"doClearBuffer = {doClearBuffer}\n"                   +
                            $"ellipticMode = {ellipticMode}\n"                     +
                            $"xFlowMode = {xFlowMode}\n"                           +
                            $"xWindValue = {fStr(xWindValue)}\n"                   +
                            $"renderDelay = {renderDelay}\n"                       +
                            $"file: {colorPicker.GetFileName()}"
                ;
            return str;
        }

        // ---------------------------------------------------------------------------------------------------------------

        // 
        protected override void setNextMode()
        {
            initLocal();

            clearScreenSetup(doClearBuffer, 0.1f);
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void generateNew()
        {
            stage = 0;

            // Get depth
            {
                int maxDepth = 100;

                depth = rand.Next(maxDepth) + 1;

                if (depth < maxDepth)
                {
                    depth += rand.Next(maxDepth);
                }

                if (depth > maxDepth)
                    depth = maxDepth - rand.Next(maxDepth / 2);
            }

            bottom = gl_Height - depth * 15;
            bottom += myUtils.randomSign(rand) * rand.Next(bottom/5);

            x = rand.Next(gl_Width + 400) - 200;
            y = 0;

            dx = rand.Next(3) - 1;

            dx = myUtils.randFloat(rand) * myUtils.randomSign(rand);

            if (myUtils.randomBool(rand))
                dy = (40.0f + rand.Next(20)) / (float)Math.Log10(depth);
            else
                dy = 500.0f / depth;

            // Set size
            {
                size = 3 / (float)Math.Log10(depth);

                if (size > 5)
                    size = 5;

                if (size < 1.0)
                    size = 1;
            }

            // Set opacity
            {
                A = 2.5f * 10.0f / depth;

                if (A > 1)
                    A = 1;

                if (A < 0.1f)
                    A = 0.1f;
            }

            colorPicker.getColor(x, bottom, ref R, ref G, ref B);

            count = 3 + rand.Next(23);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            switch (stage)
            {
                case 0:
                    {
                        x += dx;
                        x += xWindValue;
                        y += dy;

                        if (y >= bottom)
                        {
                            stage = 1;
                        }
                    }
                    break;

                case 1:
                    {
                        // Add water flowing effect
                        {
                            switch (xFlowMode)
                            {
                                case 0:
                                    x += myUtils.randFloat(rand, 0.1f) * (float)(gl_Height) / y;
                                    break;

                                case 1:
                                    x += myUtils.randFloat(rand, 0.1f) * (3.0f * y) / gl_Height;
                                    break;
                            }

                            y += myUtils.randFloat(rand, 0.1f) * 0.25f;
                        }

                        size += (0.1f * (101 - depth)) / 1;

                        A -= (0.05f * (1.0f / (depth * 0.1f))) / 2;

                        if (A < 0 && --count == 0)
                        {
                            generateNew();
                        }
                    }
                    break;
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Show()
        {
            if (A > 0)
            {
                switch (stage)
                {
                    case 0:
                        shader.SetColor(R + 0.1f, G + 0.1f, B + 0.1f, 0.23f + myUtils.randFloat(rand) * 0.07f);
                        shader.Draw((int)x, (int)y, 1, 1, 300/depth);
                        break;

                    case 1:
                        {
                            float ellipticFactor = 1;

                            switch (ellipticMode)
                            {
                                case 0:
                                    ellipticFactor = 2;
                                    break;

                                case 1:
                                    ellipticFactor += 1.0f * gl_Height / y;
                                    break;

                                case 2:
                                    ellipticFactor += 2.0f * gl_Height / y;
                                    break;

                                case 3:
                                    ellipticFactor += 3.0f * gl_Height / y;
                                    break;
                            }

                            shader.SetColor(R, G, B, A);

                            if (y < gl_y0)
                            {
                                float f = depth * 0.01f;
                                shader.SetColor(R * f, G * f, B * f, A);
                            }

                            int sz1 = (int)(size * 0.5f);
                            int sz2 = (int)(sz1 / ellipticFactor);

                            shader.Draw((int)x, (int)y, sz1, sz2, 10);
                        }
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

            clearScreenSetup(doClearBuffer, 0.1f);

            getShader();

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

                    int Count = list.Count;

                    for (int i = 0; i != Count; i++)
                    {
                        var obj = list[i] as myObj_031;

                        obj.Show();
                        obj.Move();
                    }
                }

                if (list.Count < N)
                {
                    list.Add(new myObj_031());
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

            myPrimitive.init_Ellipse();

            base.initShapes(shape, N, 0);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void getShader()
        {
            shader = new myFreeShader($@"
                        float circle(vec2 uv, float rad) {{ return smoothstep(rad, rad - 0.005, length(uv)); }}
                        float Circl1(vec2 uv, float rad) {{ return 1.0 - smoothstep(0.0, 0.0075, abs(rad-length(uv))); }}

                        float Circl2(vec2 uv, float rad)
                        {{
                            float len = length(uv * vec2(1, Pos.z / Pos.w));    {"" /* Make an ellipse by changing aspect */ }

                            return (len < rad + 0.0075)
                                ? 1.0 - smoothstep(0.0, {0.2 + myUtils.randFloat(rand)*0.3}, abs(sin((rad-len)*50)))
                                : 0;
                        }}

                        float Circl3(vec2 uv, float r1, float r2)
                        {{
                            float X = uv.x * uv.x;
                            float Y = uv.y * uv.y;

                            float a = X / (r1 * r1);
                            float b = Y / (r2 * r2);

                            if (a + b < 1.01)
                            {{
                                float val = abs(sin((a + b) * 15));

                                val *= abs(sin(a + b))*1.3;

                                return 1.0 - smoothstep(0.0, 0.6, val);
                            }}

                            return 0;
                        }}"
                    ,

                    $@"
                            vec2 uv = (gl_FragCoord.xy / iResolution.xy * 2.0 - 1.0);

                            uv -= Pos.xy;
                            uv *= aspect;

                            float circ = 0;

                            if (true)
                            {{
                                circ = Circl2(uv, Pos.z);
                            }}
                            else
                            {{
                                circ = Circl3(uv, Pos.z, Pos.w);
                            }}

                            result = vec4(myColor * circ);
                        "
            );
        }

        // ---------------------------------------------------------------------------------------------------------------

    }
};
