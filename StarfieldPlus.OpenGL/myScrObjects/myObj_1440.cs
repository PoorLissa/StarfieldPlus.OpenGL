using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;


/*
    - Depth focus test
    - Reference: https://www.youtube.com/watch?v=R5jIoLnL_nE&ab_channel=JosuRelax
*/


namespace my
{
    // ===================================================================================================================

    // Custom free shader class with additional uniform variable
    class myFreeShader_1440 : myFreeShader
    {
        private int myDepth = -123;

        public myFreeShader_1440(string fHeader = "", string fMain = "") : base(fHeader, fMain)
        {
            myDepth = myDepth < 0 ? glGetUniformLocation(shaderProgram, "myDepth") : myDepth;
        }

        public void Draw(float x, float y, float w, float h, float depth, int extraOffset = 0)
        {
            glUseProgram(shaderProgram);
            glUniform1f(myDepth, depth);
            base.Draw(x, y, w, h, extraOffset);
        }
    }

    // ===================================================================================================================

    public class myObj_1440 : myObject
    {
        // Priority
        public static int Priority => 10;
		public static System.Type Type => typeof(myObj_1440);

        private float x, y, dx, dy;
        private float size, A, R, G, B;
        private float focus;

        private static int N = 0, mode = 0;
        private static int linearSpeed = 0;
        private static float dimAlpha = 0.05f, t = 0, dt = 0;

        private static myScreenGradient grad = null;
        private static myFreeShader_1440 shader = null;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_1440()
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
                mode = rand.Next(2);

                N = mode == 0 ? 123 : 33;
            }

            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time local initialization
        private void initLocal()
        {
            doClearBuffer = true;

            linearSpeed = myUtils.randomChance(rand, 2, 3)
                ? rand.Next(3) + 2
                : rand.Next(5) + 2;

            t = 0;
            dt = 0.0001f;

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            height = 600;

            string str = $"Obj = {Type}\n\n"                    +
                            myUtils.strCountOf(list.Count, N)   +
                            $"mode = {mode}\n"                  +
                            $"linearSpeed = {linearSpeed}\n"    +
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
            // Pick size
            {
                if (myUtils.randomChance(rand, 10, 11))
                {
                    // Normal
                    size = rand.Next(333) + 111;
                }
                else
                {
                    if (myUtils.randomChance(rand, 1, 3))
                    {
                        // Small
                        size = rand.Next(11) + 3;
                    }
                    else
                    {
                        // Tiny
                        size = rand.Next(5) + 3;
                    }
                }
            }

            dx = myUtils.randFloatSigned(rand, 0.1f) * linearSpeed;
            dy = 0;
            x = dx > 0 ? -size : gl_Width + size;

            switch (mode)
            {
                case 0:
                    y = rand.Next(gl_Height);
                    break;

                case 1:
                    y = gl_y0;
                    break;
            }

            colorPicker.getColorRand(ref R, ref G, ref B);

            // Brighten up small/tiny particles
            if (size < 10)
            {
                do {

                    R += 0.0001f;
                    G += 0.0001f;
                    B += 0.0001f;

                } while (R + G + B < 2.33f);
            }

            A = 0.1f + myUtils.randFloat(rand) * 0.5f;
            focus = 0.01f + myUtils.randFloat(rand) * 0.01f;

            if (myUtils.randomChance(rand, 1, 111))
            {
                focus = 0.01f + myUtils.randFloat(rand) * 0.3f;
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            x += dx;
            y += dy;

            if (x < -size && dx < 0)
                generateNew();

            if (x > gl_Width + size && dx > 0)
                generateNew();

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Show()
        {
            int off = (int)size;
            off = off < 50 ? 50 : off;

            shader.SetColor(R, G, B, A);
            shader.Draw(x, y, size, size, focus, off);
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Process(Window window)
        {
            uint cnt = 0;
            initShapes();

            clearScreenSetup(doClearBuffer, 0.1f);

            stopwatch = new StarfieldPlus.OpenGL.myUtils.myStopwatch(true);

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
                    for (int i = 0; i != Count; i++)
                    {
                        var obj = list[i] as myObj_1440;

                        obj.Show();
                        obj.Move();
                    }
                }

                if (Count < N && myUtils.randomChance(rand, 1, 50))
                {
                    list.Add(new myObj_1440());
                }

                stopwatch.WaitAndRestart();
                t += dt;
                cnt++;
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void initShapes()
        {
            myPrimitive.init_ScrDimmer();

            getShader();

            grad = new myScreenGradient();
            grad.SetRandomColors(rand, 0.2f);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        // General shader selector
        private void getShader()
        {
            string fHeader = "", fMain = "";

            getShader_000(ref fHeader, ref fMain);
            shader = new myFreeShader_1440(fHeader, fMain);
        }

        // ---------------------------------------------------------------------------------------------------------------

        // Circular smooth spot
        private void getShader_000(ref string h, ref string m)
        {
            // Smooth circle
            var circle1 = $@"
                uniform float myDepth;
                float circle(vec2 uv, float rad)
                {{
                    float len = rad - length(uv);
                    if (len > 0)
                        return smoothstep(0.0, myDepth, len);
                    return 0;
                }}
            ";

            // Smooth circle with added dynamic noise
            var circle2 = $@"
                uniform float myDepth;
                {myShaderHelpers.Generic.noiseFunc12_v1}
                float circle(vec2 uv, float rad)
                {{
                    float len = rad - length(uv);
                    if (len > 0)
                        return smoothstep(0.0, myDepth, len) * (0.8 + noise12_v1(gl_FragCoord.xy * sin(uTime * 0.0001)) * 0.2);
                    return 0;
                }}
            ";

            // Smooth circle with added static noise
            var circle3 = $@"
                uniform float myDepth;
                {myShaderHelpers.Generic.noiseFunc12_v1}
                float circle(vec2 uv, float rad)
                {{
                    float len = rad - length(uv);
                    if (len > 0)
                        return smoothstep(0.0, myDepth, len) * (0.8 + noise12_v1(gl_FragCoord.xy) * 0.2);
                    return 0;
                }}
            ";

            // Ring
            var circle4 = $@"
                uniform float myDepth;
                float circle(vec2 uv, float rad)
                {{
                    float len = rad - length(uv);
                    return 1.0 - smoothstep(0.0, myDepth, abs(len));
                }}
            ";

            switch (rand.Next(9))
            {
                case 0:
                case 1:
                case 2:
                case 3:
                    h = circle1;
                    break;

                case 4:
                case 5:
                    h = circle2;
                    break;

                case 6:
                case 7:
                    h = circle3;
                    break;

                case 8:
                    h = circle4;
                    break;
            }

            m = $@"
                vec2 uv = (gl_FragCoord.xy / iResolution.xy * 2.0 - 1.0);

                uv -= Pos.xy;
                uv *= aspect;

                float r = circle(uv, Pos.z);
                result = vec4(myColor.xyz, r * myColor.w);
            ";
        }

        // ---------------------------------------------------------------------------------------------------------------

        // Ink drop amoebas
        private void getShader_001(ref string h, ref string m)
        {
            h = $@"
                uniform float myDepth;

                float hash(vec2 p) {{
                    return fract(sin(dot(p ,vec2(127.1, 311.7))) * 43758.5453);
                }}

                float noise(vec2 p) {{
                    vec2 i = floor(p);
                    vec2 f = fract(p);
    
                    // 4 corners interpolation
                    float a = hash(i);
                    float b = hash(i + vec2(1.0, 0.0));
                    float c = hash(i + vec2(0.0, 1.0));
                    float d = hash(i + vec2(1.0, 1.0));

                    vec2 u = f*f*(3.0-2.0*f); // smoothstep

                    return mix(mix(a, b, u.x), mix(c, d, u.x), u.y);
                }}

                float circle(vec2 uv, float rad)
                {{
                    float edgeSoftness = 0.02;    // Smoothing factor for the edge
                    float noiseAmount = 0.03;     // How much the edge is distorted

                    float len = length(uv);
    
                    // Sample noise based on direction, time, etc.
                    float n = noise(uv * 5.0 + uTime/2 + myDepth); // scale = detail, time = animation
    
                    float disturbedRadius = rad + (n - 0.5) * noiseAmount;

                    // Apply depth focus effect
                    edgeSoftness = myDepth;

                    // Smooth edge using smoothstep
                    return smoothstep(disturbedRadius, disturbedRadius - edgeSoftness, len);
                }}
            ";

            m = $@"
                vec2 uv = (gl_FragCoord.xy / iResolution.xy * 2.0 - 1.0);

                uv -= Pos.xy;
                uv *= aspect;

                float r = circle(uv, Pos.z);
                result = vec4(myColor.xyz, r * myColor.w);
            ";
        }

        // ---------------------------------------------------------------------------------------------------------------
    }
};
