using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;


/*
    - pseudo-3d-rain
*/


namespace my
{
    public class myObj_031 : myObject
    {
        private int stage, depth, bottom, count;
        private float x, y, dx, dy;
        private float size, A, R, G, B;

        private static int N = 0, shape = 0;
        private static float dimAlpha = 0.05f;

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
            doClearBuffer = myUtils.randomChance(rand, 2, 3);
            doClearBuffer = false;

            dimAlpha = 0.35f;

            renderDelay = rand.Next(11);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            height = 600;

            string nStr(int   n) { return n.ToString("N0");    }
            //string fStr(float f) { return f.ToString("0.000"); }

            string str = $"Obj = myObj_031 -- TBD: implement ellipse at last!\n\n" +
                            $"N = {nStr(list.Count)} of {nStr(N)}\n"               +
                            $"doClearBuffer = {doClearBuffer}\n"                   +
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

            x = rand.Next(gl_Width + 200) - 100;
            y = 0;

            dx = rand.Next(3) - 1;
            dy = 500.0f / depth;

            dy = 200 / (float)Math.Log10(depth);

            size = 3 / (float)Math.Log10(depth);

            if (size > 5)
            {
                size = 3;
            }

            if (size < 1.0)
                size = 1;

            A = 2.5f * 10.0f / depth;

            if (A > 1)
                A = 1;

            if (A < 0.1f)
                A = 0.1f;

            colorPicker.getColor(x, bottom, ref R, ref G, ref B);

R = G = B = 1;

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
                        y += dy;

                        if (y >= bottom)
                        {
                            stage = 1;
                        }
                    }
                    break;

                case 1:
                    {
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
                        shader.SetColor(R, G, B, 0.15f);
                        shader.Draw((int)x, (int)y, 1, 1, 3);
                        break;

                    case 1:
                        {
                            int ellipticFactor = 5;

                            if (depth > 33)
                                ellipticFactor++;

                            if (depth > 66)
                                ellipticFactor++;

                            shader.SetColor(R, G, B, A);

                            int sz1 = (int)(size / 2);
                            int sz2 = (int)(sz1 / ellipticFactor);

                            sz1 = sz1 < 1 ? 1 : sz1;
                            sz2 = sz2 < 1 ? 1 : sz2;

                            shader.Draw((int)x, (int)y, sz1, sz2, 10);
                            //shader.Draw((int)x, (int)y, sz1, sz1, 10);
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
                            {"" /* Make an ellipse by changing aspect -- tried to move it into circle function, but it worked slower (needs more proof) */ }
                            //float len = length(uv);

                            float len = length(uv * vec2(1, Pos.z / Pos.w));

                            return (len <= rad)
                                ? 1.0 - smoothstep(0.0, 0.6, abs(sin((rad-len)*50)))
                                : 0;
                        }}

                        float Circl3(vec2 uv, float r1, float r2)
                        {{
                            float X = uv.x * uv.x;
                            float Y = uv.y * uv.y;

                            float a = X / (r1 * r1);
                            float b = Y / (r2 * r2);

                            if (a + b < 1.0)
                            {{
                                //float val = abs(sin((a + b) * {5 + rand.Next(11)}));

//float val = (sin((a + b) * {5 + rand.Next(11)}));

float val = abs(sin((X + Y) * 10001 + uTime/10));

                                return 1.0 - smoothstep(0.0, 0.3, val);
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
