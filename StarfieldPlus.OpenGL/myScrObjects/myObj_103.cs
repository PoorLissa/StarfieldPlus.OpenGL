﻿using GLFW;
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
        private float x, y, sizex, sizey, A, R, G, B;

        private static int N = 0, shaderNo = 0;
        private static int opacityMode = 0;
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
            }

            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time local initialization
        private void initLocal()
        {
            renderDelay = rand.Next(11);

            opacityMode = rand.Next(2);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            height = 600;

            string nStr(int   n) { return n.ToString("N0");    }
            //string fStr(float f) { return f.ToString("0.000"); }

            string str = $"Obj = myObj_103\n\n"                         +
                            $"N = {nStr(list.Count)} of {nStr(N)}\n"    +
                            $"shaderNo = {shaderNo}\n"                  +
                            $"opacityMode = {opacityMode}\n"            +
                            $"renderDelay = {renderDelay}\n"            +
                            $"file: {colorPicker.GetFileName()}"
                ;
            return str;
        }

        // ---------------------------------------------------------------------------------------------------------------

        // 
        protected override void setNextMode()
        {
            getShader();

            System.Threading.Thread.Sleep(123);

            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void generateNew()
        {
            if (myUtils.randomChance(rand, 1, 5))
            {
                A = myUtils.randFloat(rand, 0.1f) * 0.25f;
            }
            else
            {
                A = 0.25f;
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            x = rand.Next(gl_Width);
            y = rand.Next(gl_Height);

            sizex = rand.Next(100) + 5;
            sizey = sizex;

            colorPicker.getColor(x, y, ref R, ref G, ref B);

            switch (opacityMode)
            {
                case 1:
                    A = myUtils.randFloat(rand, 0.1f) * 0.5f;
                    break;
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Show()
        {
            shader.SetColor(R, G, B, A);

            shader.Draw(x, y, sizex, sizey, 5);

            return;
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
            shaderNo = 6;
#endif
            switch (shaderNo)
            {
                case 0: getShader_000(ref fHeader, ref fMain); break;
                case 1: getShader_001(ref fHeader, ref fMain); break;
                case 2: getShader_002(ref fHeader, ref fMain); break;
                case 3: getShader_003(ref fHeader, ref fMain); break;
                case 4: getShader_004(ref fHeader, ref fMain); break;
                case 5: getShader_005(ref fHeader, ref fMain); break;
                case 6: getShader_006(ref fHeader, ref fMain); break;   // test
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

            h = $@"

                {myShaderHelpers.Generic.rotationMatrix}

                float circle(vec2 uv, float rad)
                {{
                    float len = length(uv);
                    if (len < rad)
                        return smoothstep(0.0, abs(sin(uv.x)) * abs(sin(uv.y)) * {mult}, (rad - len) * {thickness});
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

        // ...
        private void getShader_006(ref string h, ref string m)
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







    }
};
