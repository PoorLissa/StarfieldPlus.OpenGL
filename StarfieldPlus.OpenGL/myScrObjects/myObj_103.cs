using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;


/*
    - Random shapes with a color from the underlying image (point-based or average) -- Uses custom shader

        Shadertoy: https://www.shadertoy.com/
*/


namespace my
{
    public class myObj_103 : myObject
    {
        private float x, y, size, A, R, G, B;

        private static int N = 0, shaderNo = 0;
        private static int opacityMode = 0, sizeMode = 0;
        private static float dimAlpha = 0.005f;

        private static myFreeShader shader = null;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_103()
        {
            if (id != uint.MaxValue)
                generateNew();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time global initialization
        protected override void initGlobal()
        {
            colorPicker = new myColorPicker(gl_Width, gl_Height, mode: myColorPicker.colorMode.SNAPSHOT_OR_IMAGE);
            list = new List<myObject>();

            // Global unmutable constants
            {
                doClearBuffer = false;

                N = rand.Next(10) + 10;

#if DEBUG
                N = 1;
#endif
            }

            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time local initialization
        private void initLocal()
        {
            renderDelay = rand.Next(11);

            opacityMode = rand.Next(3);
            sizeMode    = rand.Next(5);

            dimAlpha = myUtils.randFloat(rand, 0.001f) * 0.005f;    // [0.001 .. 0.005]

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            height = 600;

            string nStr(int   n) { return n.ToString("N0");    }
            string fStr(float f) { return f.ToString("0.000"); }

            string str = $"Obj = myObj_103\n\n"                         +
                            $"N = {nStr(list.Count)} of {nStr(N)}\n"    +
                            $"shaderNo = {shaderNo}\n"                  +
                            $"sizeMode = {sizeMode}\n"                  +
                            $"opacityMode = {opacityMode}\n"            +
                            $"renderDelay = {renderDelay}\n"            +
                            $"dimAlpha = {fStr(dimAlpha)}\n"            +
                            $"file: {colorPicker.GetFileName()}"
                ;
            return str;
        }

        // ---------------------------------------------------------------------------------------------------------------

        // 
        protected override void setNextMode()
        {
            initShapes();

            System.Threading.Thread.Sleep(123);

            dimScreen(0.5f);

            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void generateNew()
        {
            A = 0.25f;

            if (myUtils.randomChance(rand, 1, 5))
            {
                A = myUtils.randFloat(rand, 0.1f) * 0.25f;
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            x = rand.Next(gl_Width);
            y = rand.Next(gl_Height);

            switch (sizeMode)
            {
                case 0: size = rand.Next(033) + 11; break;
                case 1: size = rand.Next(066) + 11; break;
                case 2: size = rand.Next(066) + 33; break;
                case 3: size = rand.Next(099) + 33; break;
                case 4: size = rand.Next(133) + 11; break;
            }

            colorPicker.getColor(x, y, ref R, ref G, ref B);

            switch (opacityMode)
            {
                // Const opacity
                case 0:
                    break;

                // Random opacity every iteration
                case 1:
                    A = myUtils.randFloat(rand, 0.1f) * 0.5f;
                    break;

                // Opacity changes depending on the size
                case 2:
                    A = (1.0f + sizeMode) / size;
                    A = A < 0.05f ? 0.05f : A;
                    break;
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Show()
        {
            shader.SetColor(R, G, B, A);

            shader.Draw(x, y, size, size, 5);       // todo: change 5 to like 50 and check is FPS drops; if not, set it to higher value

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Process(Window window)
        {
            uint cnt = 0;

            initShapes();

            clearScreenSetup(doClearBuffer, 0.1f, front_and_back: true);


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
                    int Count = list.Count;

                    for (int i = 0; i != Count; i++)
                    {
                        var obj = list[i] as myObj_103;

                        obj.Move();
                        obj.Show();
                    }
                }

                if (list.Count < N)
                {
                    list.Add(new myObj_103());
                }

                cnt++;
                System.Threading.Thread.Sleep(renderDelay);
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void initShapes()
        {
            getShader();

            myPrimitive.init_ScrDimmer();

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        // General shader selector
        private void getShader()
        {
            string fHeader = "", fMain = "";

            shaderNo = rand.Next(6);

#if DEBUG
            //shaderNo = 4;
#endif
            switch (shaderNo)
            {
                case 0: getShader_000(ref fHeader, ref fMain); break;
                case 1: getShader_001(ref fHeader, ref fMain); break;
                case 2: getShader_002(ref fHeader, ref fMain); break;
                case 3: getShader_003(ref fHeader, ref fMain); break;
                case 4: getShader_004(ref fHeader, ref fMain); break;
                case 5: getShader_005(ref fHeader, ref fMain); break;
                case 6: getShader_006(ref fHeader, ref fMain); break;

                case 7: getShader_007(ref fHeader, ref fMain); break;   // test
            }

            shader = new myFreeShader(fHeader, fMain);
        }

        // ---------------------------------------------------------------------------------------------------------------

        // Circle
        private void getShader_000(ref string h, ref string m)
        {
            string myCircleFunc = "";

            switch (rand.Next(3))
            {
                case 0: myCircleFunc = "return smoothstep(rad, rad - 0.005, length(uv));"; break;
                case 1: myCircleFunc = "return 1.0 - smoothstep(0.0, 0.005, abs(rad-length(uv)));"; break;
                case 2: myCircleFunc = "float len = length(uv); if (rad > len) return 1.0 - smoothstep(0.0, 0.01, rad-len); else return 1.0 - smoothstep(0.0, 0.005, len-rad);"; break;
            }

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

        // Rectangle (Square, actually)
        private void getShader_001(ref string h, ref string m)
        {
            h = $@"
                float rect(vec2 uv, float size)
                {{
                    float f = 0.01;

                    if (abs(uv.x) > size)
                        return smoothstep(size - f, size + f, abs(uv.x)) * 0.5;

                    if (abs(uv.y) > size)
                        return smoothstep(size - f, size + f, abs(uv.y)) * 0.5;

                    return { 0.125f + myUtils.randFloat(rand) * 0.1 };
                }}
            ";

            m = $@"
                vec2 uv = (gl_FragCoord.xy / iResolution.xy * 2.0 - 1.0);

                uv -= Pos.xy;
                uv *= aspect;

                float r = rect(uv, Pos.z);
                result = vec4(myColor.xyz, r);
            ";
        }

        // ---------------------------------------------------------------------------------------------------------------

        // Circular smooth spot
        private void getShader_002(ref string h, ref string m)
        {
            h = $@"
                float circle(vec2 uv, float rad)
                {{
                    float th = {0.02 + myUtils.randFloat(rand) * 0.3};
                    float len = rad - length(uv);
                    if (len > 0)
                        return smoothstep(0.0, th, len);
                    return 0;
                }}
            ";

            m = $@"
                vec2 uv = (gl_FragCoord.xy / iResolution.xy * 2.0 - 1.0);

                uv -= Pos.xy;
                uv *= aspect;

                float r = circle(uv, Pos.z);
                result = vec4(myColor.xyz, r);
            ";
        }

        // ---------------------------------------------------------------------------------------------------------------

        // 4-ray stars
        private void getShader_003(ref string h, ref string m)
        {
            float thickness = (rand.Next(2) == 0)
                ? myUtils.randFloat(rand) * (rand.Next(13) + 1)
                : myUtils.randFloat(rand);

            float mult = rand.Next(20) + 1;
            string func = (rand.Next(2) == 0) ? "min" : "max";

            h = $@"

                {myShaderHelpers.Generic.rotationMatrix}

                float circle(vec2 uv, float rad)
                {{
                    float len = length(uv);
                    if (len < rad)
                        return smoothstep(0.0, {func}(abs(sin(uv.x)), abs(sin(uv.y))) * {mult}, (rad - len) * {thickness});
                    return 0;
                }}
            ";

            m = $@"
                vec2 uv = (gl_FragCoord.xy / iResolution.xy * 2.0 - 1.0);

                uv -= Pos.xy;
                uv *= aspect;

                if ({rand.Next(2)} == 0)
                    uv *= rot(uTime);

                float r = circle(uv, Pos.z);
                result = vec4(myColor.xyz, myColor.w * r);
            ";
        }

        // ---------------------------------------------------------------------------------------------------------------

        // 4-ray star, concave
        private void getShader_004(ref string h, ref string m)
        {
            float thickness = 0.002f + myUtils.randFloat(rand) * 0.1f;
            float mult = rand.Next(20) + 1;
            int doRotate = rand.Next(2);

            string shapeFunc = myUtils.randomChance(rand, 2, 3) ? "shape_border" : "shape";

            h = $@"

                {myShaderHelpers.Generic.rotationMatrix}

                float shape(vec2 uv, float rad)
                {{
                    float len = length(uv);

                    if (len < rad)
                        return smoothstep(0.0, abs(sin(uv.x)) * abs(sin(uv.y)) * {mult}, (rad - len) * {thickness});

                    return 0;
                }}

                float shape_border(vec2 uv, float rad)
                {{
                    float res = 0, len = length(uv);

                    float val1 = (rad - len) * {thickness};
                    float val2 = abs(sin(uv.x)) * abs(sin(uv.y)) * {mult};

                    res = smoothstep(0.0, val2, val1);

                    if (abs(val1 - val2) < 0.0001)
                        res += (1 - smoothstep(-0.01, 0.01, (val1 - val2))) * 0.5;

                    return res;
                }}
            ";

            m = $@"
                vec2 uv = (gl_FragCoord.xy / iResolution.xy * 2.0 - 1.0);

                uv -= Pos.xy;
                uv *= aspect;

                if ({doRotate} == 1)
                    uv *= rot(uTime);

                float r = {shapeFunc}(uv, Pos.z);
                result = vec4(myColor.xyz, myColor.w * r);
            ";
        }

        // ---------------------------------------------------------------------------------------------------------------

        // Another 4-ray star
        private void getShader_005(ref string h, ref string m)
        {
            float targetVal = 0.000001f + myUtils.randFloat(rand) * 0.00001f;

            h = $@"

                float circle(vec2 uv, float rad)
                {{
                    return 1 - smoothstep(0, {targetVal}, abs(uv.x * uv.y));
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

        // Circle-bended sin-cos-wave
        private void getShader_006(ref string h, ref string m)
        {
/*
            float val = len - rad + sin(at * nRays) * rad/25 + cos(at * 13) * rad/33;
            float color = smoothstep(0, 0.025, abs(val));   // remove abs for a solid shape
            result = vec4(vec3(color), 1.0);
*/
            float nRays = 2;

            switch (rand.Next(3))
            {
                case 0: nRays += rand.Next(11); break;
                case 1: nRays += rand.Next(33); break;
                case 2: nRays += rand.Next(99); break;
            }

            float thickness = 1.0f / (rand.Next(30) + 5);
            float borderOpacityFactor = myUtils.randFloat(rand) + rand.Next(3) + 1;

            float smoothVal = -0.01f * (rand.Next(10) + 1);      // [-0.01 .. -0.10] -- this affects the depth of non-black pixels inside the shape

            bool useBorder = myUtils.randomChance(rand, 2, 3);
            int borderMode = rand.Next(3);

            string F = "len - rad;";
            string myFunc = useBorder ? "func_border" : "func";

            switch (rand.Next(3))
            {
                // No rotation
                case 0: F = $@"len - rad + sin(at * {nRays}) * rad * {thickness};"; break;

                // Random rotation
                case 1: F = $@"len - rad + sin(at * {nRays} + rand(uTime) * 13) * rad * {thickness};"; break;

                // Random cos added to make the shape irregular
                case 2: F = F = $@"len - rad + sin(at * {nRays}) * cos(at * uTime) * rad * {thickness};"; break;
            }

            h = $@"

                {myShaderHelpers.Generic.randFunc}

                float func(vec2 uv, float rad)
                {{
                    float at = atan(uv.y, uv.x);    // angle
                    float len = length(uv);

                    float val = {F};

                    if (val < 0)
                        return smoothstep({smoothVal}, 0, val);

                    return 0;
                }}

                float func_border(vec2 uv, float rad)
                {{
                    float res = 0;
                    float at = atan(uv.y, uv.x);
                    float len = length(uv);

                    float val = {F};

                    if (val < 0)
                    {{
                        float off = abs(val + 0.0075);

                        if (off < 0.0025)
                            res = (1 - smoothstep(0, 0.001, off)) * {borderOpacityFactor};

                        switch({borderMode})
                        {{
                            case 0: res += smoothstep(0, {-smoothVal}, -val) * 0.75; break;
                            case 1: res += smoothstep({+smoothVal}, 0, +val) * 0.75; break;
                            case 2:
                                res += smoothstep(0, {-smoothVal}, -val) * 0.5;
                                res += smoothstep({smoothVal}, 0, val) * 0.5;
                                break;
                        }}
                    }}

                    return res;
                }}
            ";

            m = $@"
                vec2 uv = (gl_FragCoord.xy / iResolution.xy * 2.0 - 1.0);

                uv -= Pos.xy;
                uv *= aspect;

                float r = {myFunc}(uv, Pos.z);

                result = vec4(myColor.xyz, r * myColor.w);
            ";
        }

        // ---------------------------------------------------------------------------------------------------------------

        // ...
        private void getShader_007(ref string h, ref string m)
        {
            float nRays = 2 + (rand.Next(2) == 0 ? rand.Next(33) : rand.Next(100));
            float thickness = 1.0f / (rand.Next(30) + 5);

            float smoothVal = -0.01f * (rand.Next(10) + 1);

            h = $@"
                float Func(vec2 uv, float rad)
                {{
                    float at = atan(uv.y, uv.x);
                    float len = length(uv);

                    float val = len - rad + sin(at * {nRays}) * rad * {thickness};
                    float absVal = abs(val);

                    if (val < 0)
                    {{
                        float res = 0, off = abs(val + 0.0075);

                        if (off < 0.0025)
                            res = (1 - smoothstep(0, 0.001, off)) * 1;

                        return res + smoothstep(0, 0.01, absVal) * 0.5;
                    }}

                    return 0;
                }}
            ";

            m = $@"
                vec2 uv = (gl_FragCoord.xy / iResolution.xy * 2.0 - 1.0);

                uv -= Pos.xy;
                uv *= aspect;

                float r = Func(uv, Pos.z);

                result = vec4(myColor.xyz, r * myColor.w);
            ";
        }

        // ---------------------------------------------------------------------------------------------------------------




    }
};
