﻿using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;


/*
    - Free shader experiments

    https://www.shadertoy.com/

    GLSL Manual: https://registry.khronos.org/OpenGL-Refpages/es3.1/

    Online live shader tool: https://glslsandbox.com/e

    Read this later https://miketuritzin.com/post/rendering-particles-with-compute-shaders/

    // To investigate later:
    https://www.shadertoy.com/view/3tXXRn   -- tentacles (super cool but super complex)
    https://www.shadertoy.com/view/Xsl3z2
    https://www.shadertoy.com/view/Ddy3zD
    https://www.shadertoy.com/view/DdyGzy
    https://www.shadertoy.com/view/dsy3Dw

    // Math links:
    dot     : https://www.cuemath.com/algebra/dot-product/
    cross   : https://www.cuemath.com/geometry/cross-product/
*/


namespace my
{
    public class myObj_500 : myObject
    {
        private float R, G, B;

        private string fHeader = "", fMain = "";

        private myFreeShader shader = null;
        private myFreeShader_FullScreen shaderFull = null;

        private int mode = 0;

        private static bool doUseFullScreenShader = true;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_500()
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
                do
                {
                    R = myUtils.randFloat(rand);
                    G = myUtils.randFloat(rand);
                    B = myUtils.randFloat(rand);
                }
                while (R + G + B < 0.33f);
            }

            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time local initialization
        private void initLocal()
        {
            doUseFullScreenShader = true;

            renderDelay = rand.Next(11) + 3;

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            height = 600;

            string nStr(int n) { return n.ToString("N0"); }
            string fStr(float f) { return f.ToString("0.000"); }

            string str = $"Obj = myObj_500 -- Free Shader Experiments\n\n" +
                            $"N = {nStr(0)}\n" +
                            $"R = {fStr(R)}; G = {fStr(G)}; B = {fStr(B)}\n" +
                            $"mode = {mode}\n" +
                            $"renderDelay = {renderDelay}\n" +
                            $"file: {colorPicker.GetFileName()}"
                ;
            return str;
        }

        // ---------------------------------------------------------------------------------------------------------------

        // 
        protected override void setNextMode()
        {
            getShader(ref fHeader, ref fMain, doUseFullScreenShader);

            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        class zzz
        {
            public int x, y, w, h;
            public float t = 0, dt = myUtils.randFloat(rand) * 0.01f;
        };

        protected override void Process(Window window)
        {
            // Set culture to avoid incorrect float conversion in shader strings
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");

            uint cnt = 0;

            glDrawBuffer(GL_FRONT_AND_BACK | GL_DEPTH_BUFFER_BIT);

            getShader(ref fHeader, ref fMain, doUseFullScreenShader);

            if (!doUseFullScreenShader)
            {
                int n = 111;
                List<zzz> lst = new List<zzz>();

                shader.SetColor(myUtils.randFloat(rand), myUtils.randFloat(rand), myUtils.randFloat(rand), 0.85f);

                for (int i = 0; i < n; i++)
                {
                    var z = new zzz();

                    z.x = rand.Next(gl_Width);
                    z.y = rand.Next(gl_Height);
                    z.w = rand.Next(333) + 33;
                    z.h = z.w / (rand.Next(5) + 1);

                    z.dt = 0.01f * (rand.Next(10) + 1);

                    lst.Add(z);
                }
            }

/*
            // How to enable depth testing:
            glEnable(GL_DEPTH_TEST);
            glDepthFunc(GL_LESS);
            glDepthRange(0, 1);
*/
            while (!Glfw.WindowShouldClose(window))
            {
                processInput(window);

                // Swap fore/back framebuffers, and poll for operating system events.
                Glfw.SwapBuffers(window);
                Glfw.PollEvents();

                glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT);

                if (false)
                {
                    myPrimitive.init_Rectangle();

                    myPrimitive._Rectangle.SetAngle(0);
                    myPrimitive._Rectangle.SetColor(0.25f, 0.5f, 0.15f, 0.9f);
                    myPrimitive._Rectangle.Draw(123, 123, 222, 222, true);

                    myPrimitive._Rectangle.SetAngle((float)(Math.PI / 3));
                    myPrimitive._Rectangle.SetColor(0.5f, 0.25f, 0.33f, 0.9f);
                    myPrimitive._Rectangle.Draw(111, 111, 333, 333, true);

                    continue;
                }

                // Render Frame
                if (doUseFullScreenShader)
                {
                    shaderFull.Draw();
                }
                else
                {
                    /*
                                        for (int i = 0; i < lst.Count; i++)
                                        {
                                            var z = lst[i];

                                            shader.Draw(z.x, z.y, z.w + (int)(Math.Cos(z.t) * 66), z.h + (int)(Math.Sin(z.t) * 33), 33);
                                            z.t += z.dt;
                                        }*/
                    /*
                                        shader.SetColor(0.25f, 0.66f, 0.33f, 1);
                                        int rad = 100 + (int)(Math.Sin(0.025 * cnt) * 50);

                                        shader.Draw(333, 333, 222, 222, 3);
                                        shader.Draw(333, 333, 333 + (int)(Math.Cos(cnt * 0.1) * 66), 222 + (int)(Math.Sin(cnt * 0.1) * 33), 3);

                                        if (false)
                                        {
                                            shader.SetColor(0.66f, 0.33f, 0.22f, 0.33f);

                                            for (int i = 0; i < gl_Width; i += 200)
                                                for (int j = 0; j < gl_Height; j += 200)
                                                    shader.Draw(i, j, 66, 55, 3);
                                        }
                    */
                }

                cnt++;
                System.Threading.Thread.Sleep(renderDelay);
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        // Select random mode and get shader code: header + main func
        private void getShader(ref string header, ref string main, bool fullScreen)
        {
            if (fullScreen)
            {
                int max = 8;
                mode = rand.Next(max);
#if DEBUG
                mode = 10;
#endif
                switch (mode)
                {
                    case 00: getShader_000(ref header, ref main); break;
                    case 01: getShader_001(ref header, ref main); break;
                    case 02: getShader_002(ref header, ref main); break;
                    case 03: getShader_003(ref header, ref main); break;
                    case 04: getShader_004(ref header, ref main); break;
                    case 05: getShader_005(ref header, ref main); break;
                    case 06: getShader_006(ref header, ref main); break;
                    case 07: getShader_007(ref header, ref main); break;
                    case 08: getShader_008(ref header, ref main); break;
                    case 09: getShader_009(ref header, ref main); break;
                    case 10: getShader_010(ref header, ref main); break;
                }

                shaderFull = new myFreeShader_FullScreen(fHeader: fHeader, fMain: fMain);
            }
            else
            {
                shader = new myFreeShader($@"
                        float circle(vec2 uv, float rad) {{ return smoothstep(rad, rad - 0.005, length(uv)); }}
                        float Circle(vec2 uv, float rad) {{ return 1.0 - smoothstep(0.0, 0.005, abs(rad-length(uv))); }}
                    ",

                        $@"
                            vec2 uv = (gl_FragCoord.xy / iResolution.xy * 2.0 - 1.0);

                            uv -= Pos.xy;
                            uv *= aspect;

                            // Make an ellipse by changins aspect -- tried to move it into circle function, but it worked slower (needs more proof)
                            if (Pos.w != Pos.z)
                                uv *= vec2(1.0, Pos.z / Pos.w);

                            float rad = Pos.z;
                            float circ = Circle(uv, rad);

                            result = vec4(myColor.xyz * circ, myColor.w * circ);
                        "
                );
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        // https://www.shadertoy.com/view/tdlBR8
        private void getShader_000(ref string header, ref string main)
        {
            header = $@"

                float concentric(vec2 m, float repeat, float t) {{
                    float r = length(m);
                    float v = sin((1.0 - r) * (1.0 - r) * repeat + t) * 0.5 + 0.5;
                    return v;
                }}

                float spiral(vec2 m, float repeat, float dir, float t) {{
	                float r = length(m);
	                float a = atan(m.y, m.x);
	                float v = sin(repeat * (sqrt(r) + (1.0 / repeat) * dir * a - t)) * 0.5 + 0.5;
	                return v;
                }}
            ";

            main = $@"

                float aspect = iResolution.x / iResolution.y;
    
                vec2 uv = (gl_FragCoord.xy / iResolution.xy * 2.0 - 1.0) * vec2(1.0, 1.0 / aspect);
                float r = length(uv);

                float c0 = 1.0 - sin(r * r) * 0.85;

                float c1 = concentric(uv, 50.0, uTime * 0.1) * 0.5 + 0.5;
                float c2 = spiral(uv, 90.0, +1.0, uTime * 0.11) * 0.9 + 0.1;
                float c3 = spiral(uv, 30.0, -1.0, uTime * 0.09) * 0.8 + 0.2;

                vec3 col = vec3(c0 * c1 * c2 * c3) * vec3({R}, {G}, {B});

                result = vec4(col, 1.0);
            ";
        }

        // ---------------------------------------------------------------------------------------------------------------

        // https://www.shadertoy.com/view/3dBXDy
        private void getShader_001(ref string header, ref string main)
        {
            header = $@"
                {myShaderHelpers.Generic.noiseFunc}
                {myShaderHelpers.Generic.randFunc}
            ";

            main = $@"

                // Normalized pixel coordinates (from 0 to 1)
                vec2 uv = (gl_FragCoord.xy - iResolution.xy * 0.5) / iResolution.y;

                float mask = smoothstep(0.5, 0.0, length(uv) * 1.0);      // 1.0 changes size ?..
                mask *= 1.0 - (uv.y + 0.5);

                float f = 10.0;
                float newTime = uTime * 3.0;
                float d = (uv.y + 0.5) + 1.0; // denser higher
                float final = 0.05 * sin(dot(uv, vec2(sin(newTime * 0.2), cos(newTime * 0.15))) * 10.5 * d + newTime);
                final += 0.15 * sin(dot(uv, vec2(sin(newTime * +0.20 + 1.42), cos(newTime * +0.15 + 1.46))) * 06.5 * d * d + newTime);
                final += 0.15 * sin(dot(uv, vec2(sin(newTime * -0.20 + 2.42), cos(newTime * -0.20 + 2.42))) * 20.5 + newTime);
                final += 0.09 * sin(dot(uv, vec2(sin(newTime * +0.26 + 2.42), cos(newTime * +0.26 + 2.42))) * 16.5 + newTime);

                final = final * 0.5 + 0.5;
                final *= mask;
                final += mask * 0.7;

                final += smoothstep(0.5, 0.6, final);

                //final = mask*10.0;

                // Randomly add noise
                switch({rand.Next(3)})
                {{
                    case 0:
                        final *= 0.975 + rand((uv.x + uv.y) * sin(uTime)) * 0.025;
                        final *= 0.975 + rand((uv.x - uv.y) * sin(uTime)) * 0.025;
                        break;

                    case 1:
                        final *= 0.85 + noise(gl_FragCoord.xy * uv) * 0.15;
                        break;
                }}

                // Output to screen
                result = vec4(vec3(final * {R}, final * {G}, final * {B}), 1.0);
            ";
        }

        // ---------------------------------------------------------------------------------------------------------------

        // https://www.shadertoy.com/view/MdKBzt
        // Rotating cube made of 8 points
        private void getShader_002(ref string header, ref string main)
        {
            header = $@"

                float distLine(vec3 ro, vec3 rd, vec3 p) {{
                    float d = length(cross(p-ro, rd))/length(rd);
                    return smoothstep(.06, .05, d);
                }}

                float box(vec3 ro, vec3 rd) {{
                    float d = 0.;
                    d += distLine(ro, rd, vec3(0.0, 0.0, 0.0));
                    d += distLine(ro, rd, vec3(0.0, 1.0, 0.0));
                    d += distLine(ro, rd, vec3(1.0, 1.0, 0.0));
                    d += distLine(ro, rd, vec3(1.0, 0.0, 0.0));
                    d += distLine(ro, rd, vec3(0.0, 0.0, 1.0));
                    d += distLine(ro, rd, vec3(0.0, 1.0, 1.0));
                    d += distLine(ro, rd, vec3(1.0, 1.0, 1.0));
                    d += distLine(ro, rd, vec3(1.0, 0.0, 1.0));

	                return d;
                }}
            ";

            main = $@"

                vec2 uv = (gl_FragCoord.xy/iResolution.xy - 0.5);
                uv.x *= iResolution.x/iResolution.y;

                vec3 lookAt = vec3(0.5);
                vec3 ro = vec3(3.* sin(uTime * 0.1), 2. * cos(uTime * 0.1), -3.);       {"" /* Rotation vector */}

                float zoom = 0.5 + sin(uTime * 0.1) * 0.2;
    
                vec3 f = normalize(lookAt - ro);
                vec3 r = cross(vec3(0.0, -1.0, 0.0), f);
                vec3 u = cross(r, f);
                vec3 c = ro + f * zoom;
                vec3 i = c + uv.x * r + uv.y * u;
                vec3 rd = i - ro;
    
                float d = box(ro, rd);
                result = vec4(d * {R}, d * {G}, d * {B}, 0.25);
            ";
        }

        // ---------------------------------------------------------------------------------------------------------------

        // https://www.shadertoy.com/view/ddy3RW
        private void getShader_003(ref string header, ref string main)
        {
            header = $@"
                {myShaderHelpers.Generic.rotationMatrix}
                float PI  = {Math.PI};
                float TAU = {Math.PI} * 2.0;

                float circleSDF(vec2 coords, float rad, vec2 offset)
                {{
                    return abs(length(coords-offset) - rad*rad) - 0.000002;
                }}
            ";

            // uv is Texture Coordinates: 0-1

            main = $@"
   
                vec2 uv = ( gl_FragCoord.xy - .5 * iResolution.xy ) / iResolution.y;
                float a = (PI/2.0) * pow((1.5-pow(length(uv),.5)), 3.5);

                float rad = {myUtils.randFloat(rand) * 0.5};

                uv *= rot(a*sin(uTime+a));     // add rotation
                uv = mod(uv, rad);
                vec3 col;
                float cSDF = circleSDF(uv, rad + 0.05, vec2(rad/2, rad/2));
                col += smoothstep(0.006, -0.006, cSDF);

                result = vec4(col * vec3({R}, {G}, {B}), 1);
            ";
        }

        // ---------------------------------------------------------------------------------------------------------------

        // https://www.shadertoy.com/view/ll2GD3
        private void getShader_004(ref string header, ref string main)
        {
            header = $@"

                vec3 pal(float t, vec3 a, vec3 b, vec3 c, vec3 d)
                {{
                    return a + b * cos(6.28318*(c * t + d));
                }}

                float noise(vec2 p)
                {{
                    return fract(sin(dot(p, vec2(12.9898, 78.233))) * 43758.5453123);
                }}

            ";

            main = $@"

	            vec2 p = gl_FragCoord.xy / iResolution.xy;
    
                // animate
                p.x += {myUtils.randFloat(rand) * 0.23} * uTime;

                int n = {rand.Next(5) + 3};

                // compute colors
                            vec3 col = pal(p.x, vec3(0.5,0.5,0.5), vec3(0.5,0.5,0.5), vec3(1.0,1.0,1.0), vec3(0.0,0.33,0.67));
                if (p.y > 1.0/n) col = pal(p.x, vec3(0.5,0.5,0.5), vec3(0.5,0.5,0.5), vec3(1.0,1.0,1.0), vec3(0.0,0.10,0.20));
                if (p.y > 2.0/n) col = pal(p.x, vec3(0.5,0.5,0.5), vec3(0.5,0.5,0.5), vec3(1.0,1.0,1.0), vec3(0.3,0.20,0.20));
                if (p.y > 3.0/n) col = pal(p.x, vec3(0.5,0.5,0.5), vec3(0.5,0.5,0.5), vec3(1.0,1.0,0.5), vec3(0.8,0.90,0.30));
                if (p.y > 4.0/n) col = pal(p.x, vec3(0.5,0.5,0.5), vec3(0.5,0.5,0.5), vec3(1.0,0.7,0.4), vec3(0.0,0.15,0.20));
                if (p.y > 5.0/n) col = pal(p.x, vec3(0.5,0.5,0.5), vec3(0.5,0.5,0.5), vec3(2.0,1.0,0.0), vec3(0.5,0.20,0.25));
                if (p.y > 6.0/n) col = pal(p.x, vec3(0.8,0.5,0.4), vec3(0.2,0.4,0.2), vec3(2.0,1.0,1.0), vec3(0.0,0.25,0.25));

                // band
                float f = fract(p.y * n);

                // borders
                col *= smoothstep(0.49, 0.47, abs(f-0.5));

                // shadowing
                col *= 0.5 + 0.5 * sqrt(4.0 * f * (1.0-f));

                // Noise
                if ({rand.Next(2)} == 0)
                    col *= 0.85 + noise(gl_FragCoord.xy * uTime * 0.00025) * {myUtils.randFloat(rand, 0.2f) * 0.5};

	            result = vec4(col, 1.0);
            ";
        }

        // ---------------------------------------------------------------------------------------------------------------

        // https://www.shadertoy.com/view/4dsXzM
        private void getShader_005(ref string header, ref string main)
        {
            header = $@"

                {myShaderHelpers.Generic.rotationMatrix /* add rotation matrix function */}

                float layers = 11;
                float scale = 300;
                float lengt = 0.13;
                float thickness = 0.0;

                vec2 hash12(float p)
                {{
	                return fract(vec2(sin(p * 591.32), cos(p * 391.32)));
                }}

                float hash21(in vec2 n) 
                {{
	                return fract(sin(dot(n, vec2(12.9898, 4.1414))) * 43758.5453);
                }}

                vec2 hash22(in vec2 p)
                {{
                    p = vec2(dot(p, vec2(127.1, 311.7)), dot(p, vec2(269.5, 183.3)));
	                return fract(sin(p)*43758.5453);
                }}

                float field1(in vec2 p)
                {{
                    vec2 n = floor(p)-0.5;
                    vec2 f = fract(p)-0.5;

                    // vec o will move the shapes slightly off the grid
                    vec2 o = hash22(n)*.35;
	                vec2 r = - f - o;

                    // add rotation
	                r *= rot(uTime + hash21(n)*3.14);
	
                    // 1st half of a shape
	                float d1 = 1.0 - smoothstep(thickness,thickness+0.09,abs(r.x));
	                d1 *= 1.0 - smoothstep(lengt,lengt+0.02,abs(r.y));
	
                    // 2nd half of a shape
	                float d2 =  1.0-smoothstep(thickness,thickness+0.09,abs(r.y));
	                d2 *= 1.-smoothstep(lengt,lengt+0.02,abs(r.x));
	
                    return max(d1, d2);
                }}
            ";

            main = $@"

	            vec2 p = gl_FragCoord.xy / iResolution.xy-0.5;

	            p.x *= iResolution.x/iResolution.y;
	
	            float mul = (iResolution.x + iResolution.y) / scale;

	            vec3 col = vec3(0);

	            for (float i = 0.0; i < layers; i++)
	            {{
                    // just get random vec2
                    vec2 ds = hash12(i * 2.5) * 0.20;

                    // 1st part gets us our shapes
                    // 2nd part is just a color
		            col = max(col, field1((p+ds) * mul) * (sin(ds.x * 5100. + vec3(1.0, 2.0, 3.5)) * 0.4 + 0.6));
	            }}

	            result = vec4(col, 1.0);
            ";
        }

        // ---------------------------------------------------------------------------------------------------------------

        // my test
        private void getShader_006(ref string header, ref string main)
        {
            header = $@"
                {myShaderHelpers.Generic.noiseFunc}
                {myShaderHelpers.Generic.rotationMatrix}
            ";

            main = $@"

                float aspect = iResolution.x / iResolution.y;
    
                vec2 uv = (gl_FragCoord.xy / iResolution.xy * 2.0 - 1.0) * vec2(1.0, 1.0 / aspect);

                // rotation
                //float rotSpd = 0.1;
                //uv *= rot(uTime*rotSpd);

                float r = length(uv);

                float scale = {myUtils.randFloat(rand, 0.1f) + rand.Next(66) + 1};

                //scale = 1.23;

                float x = (uv.x) * scale * uTime;
                float y = (uv.y) * scale * uTime;

                //x += uTime * 15;

                float F = 0;

                // ok functions:
                F = x * sin(x) * cos(y);
                //F = x * sin(x) + y * cos(y);
                //F = (x + y) * x * y;

                float res = y;

                //F = x * y * cos(x * y * uTime) * sin(x + y);
                //F = sin(cos(x * y) * uTime) * 1;

                vec3 col = vec3({myUtils.randFloat(rand)}, {myUtils.randFloat(rand)}, {myUtils.randFloat(rand)});

                float d = 0.01;

                if ((F - res) < 0.1)
                {{
                    d = smoothstep(0.05, 0.45, abs(F - res));
                }}

                col *= 0.85 + noise(gl_FragCoord.xy * uTime * 0.00025) * {myUtils.randFloat(rand, 0.2f) * 0.5};

                result = vec4(col, d);
            ";
        }

        // ---------------------------------------------------------------------------------------------------------------

        // my test
        private void getShader_007(ref string header, ref string main)
        {
            header = $@"
                {myShaderHelpers.Generic.noiseFunc}
                {myShaderHelpers.Generic.randFunc}
                {myShaderHelpers.Generic.rotationMatrix}

                vec3 col = vec3({R}, {G}, {B});
            ";

            main = $@"

                float aspect = iResolution.x / iResolution.y;
    
                vec2 uv = (gl_FragCoord.xy / iResolution.xy * 2.0 - 1.0) * vec2(1.0, 1.0 / aspect);

                // rotation
                float rotSpd = 0.1 * {myUtils.randomSign(rand)};
                uv *= rot(uTime * rotSpd);

                float r = length(uv);

                float scale = {myUtils.randFloat(rand, 0.1f) + rand.Next(66) + 1};

                scale *= sin(uTime*0.1) * 0.25;

                float x = (uv.x) * scale;
                float y = (uv.y) * scale;

                float F = 0;

                // ok functions:
                F = x * sin(x) * cos(y) * uTime;

                float res = sin(x / y) * sin(uTime);

                float d = 0.01;

                float dF = F - res;

                //if (F - res < 0.5)
                if (dF > 0 && dF < 10)
                {{
                    //d = smoothstep(0.05, 0.01, abs(F - res));

                    d = smoothstep(0.0, 10.0, dF);
                }}

                col *= 0.85 + noise(gl_FragCoord.xy * uTime * 0.00025) * {myUtils.randFloat(rand, 0.2f) * 0.5};

                result = vec4(col, d);
            ";
        }

        // ---------------------------------------------------------------------------------------------------------------

        // https://www.shadertoy.com/view/csVGR3
        private void getShader_008(ref string header, ref string main)
        {
            header = $@"
                {myShaderHelpers.Generic.noiseFunc}
                {myShaderHelpers.Generic.randFunc}
                {myShaderHelpers.Generic.rotationMatrix}

                mat3 rotateX(in float a)
                {{
                    return mat3(
                        1.0, 0.0, 0.0,
                        0.0, cos(a), -sin(a),
                        0.0, sin(a), cos(a));
                }}

                mat3 rotateY(in float a)
                {{
                    return mat3(
                        cos(a), 0.0, sin(a),
                        0.0, 1.0, 0.0,
                        -sin(a), 0.0, cos(a));
                }}

                float sdfBox(in vec3 p, in vec3 size)
                {{
                    vec3 d = abs(p) - size;
                    return min(max(d.x, max(d.y, d.z)), 0.0) + length(max(d, 0.0));
                }}

                float map(in vec3 p)
                {{
                    vec3 boxSize = vec3(1.0);
                    vec3 q = rotateY(uTime) * rotateX(0.5 * uTime) * p;
                    return sdfBox(q, boxSize);
                }}

                vec3 raymarch(in vec3 ro, in vec3 rd)
                {{
                    float t = 0.0;
                    for (int i = 0; i < 64; i++)
                    {{
                        vec3 p = ro + t * rd;
                        float d = map(p);
                        if (d < 0.001) return p;
                        t += d;
                    }}
                    return vec3(0.0);
                }}
            ";

            main = $@"

                vec2 uv = (gl_FragCoord.xy - 0.5 * iResolution.xy) / iResolution.y;
                float time = uTime;

                vec3 cameraPos = vec3(0.0, 0.0, 14.0);
                vec3 rd = normalize(vec3(uv, -1.0));        {"" /* calculate the unit vector in the same direction as the original vector */}
                vec3 ro = cameraPos;

                vec3 p = raymarch(ro, rd);

                vec3 normal = normalize(vec3(
                    map(p + vec3(0.001, 0.0, 0.0)) - map(p - vec3(0.001, 0.0, 0.0)),
                    map(p + vec3(0.0, 0.001, 0.0)) - map(p - vec3(0.0, 0.001, 0.0)),
                    map(p + vec3(0.0, 0.0, 0.001)) - map(p - vec3(0.0, 0.0, 0.001))
                ));

                vec3 light = vec3(0.0, 5.0, 5.0);
                vec3 lightDir = normalize(light - p);

                float diff = clamp(dot(lightDir, normal), 0.1, 1.0);

                vec3 col = vec3(0.5, 0.7, 1.0) * diff;

                result = vec4(col, 1.0);
            ";
        }

        // ---------------------------------------------------------------------------------------------------------------

        // https://www.shadertoy.com/view/mdjXRd
        private void getShader_009(ref string header, ref string main)
        {
            header = $@"
                vec3 col = vec3(0);
                float i, dist, anim;
            ";

            main = $@"

                vec2 uv = (gl_FragCoord.xy - iResolution/2) / iResolution.y;

                // zoom out a bit
                uv *= 2;

                result.xyz *= 0;

                for (i = 0; i < 22.0; i++)
                {{
                    // shape + distance
                    dist = 0.004 / (abs(length(uv * uv) - i * 0.04) + 0.005);

                    // color
                    col = cos(vec3(0, 1, 2) + i) + 1.0;

                    // animation
                    anim = smoothstep(0.35, 0.4, abs(abs(mod(uTime, 2.0) - i * 0.1) - 1.0));

                    result.xyz += dist * col * anim;

                    // rotation
                    uv *= mat2(cos((uTime + i) * 0.03 + vec4(0, 33, 11, 0)));
                }}

                result.w = 1;
            ";
        }

        // ---------------------------------------------------------------------------------------------------------------

        // RayMarching starting point:
        // https://www.shadertoy.com/view/WtGXDD                                -- live example
        // https://michaelwalczyk.com/blog-ray-marching.html                    -- good explanation
        // https://timcoster.com/2020/02/17/raymarching-shader-pt4-primitives/  -- some explanation + src code for SDF primitives
        private void getShader_010(ref string header, ref string main)
        {
            header = $@"

                #define MAX_STEPS   100             // change this to adjust artefacts
                #define MAX_DIST    100.0
                #define SURF_DIST   0.001           // change this to adjust artefacts

                {myShaderHelpers.Generic.rotationMatrix}
                {myShaderHelpers.Generic.noiseFunc}

                {myShaderHelpers.SDF.boxSDF}
                {myShaderHelpers.SDF.sphereSDF}
                {myShaderHelpers.SDF.roundBoxSDF}
                {myShaderHelpers.SDF.hexPrismSDF}

                float GetDist(vec3 p)
                {{
                    float d = 0;

                    switch ({rand.Next(4)})
                    {{
                        case 0: d = sphereSDF(p, 1); break;
                        case 1: d = boxSDF(p, vec3(1)); break;
                        case 2: d = roundBoxSDF(p, vec3(1), 0.33); break;
                        case 3: d = hexPrismSDF(p, vec2(2, 0.66)); break;
                    }}

                    // Displacement
                    float d1 = sin(5.0 * p.x + uTime) * sin(5.0 * p.y + uTime) * sin(5.0 * p.z + uTime) * 0.25;
                    float d2 = sin(5.0 * p.x * uTime) * sin(5.0 * p.y * uTime) * sin(5.0 * p.z * uTime) * 0.25;

                    float d3 = sin(5.0 * p.x + uTime * {R}) * sin(5.0 * p.y + uTime * {G}) * sin(5.0 * p.z + uTime * {B}) * 0.25;
                    float d4 = sin(5.0 * p.x * uTime * {R}) * sin(5.0 * p.y * uTime * {G}) * sin(5.0 * p.z * uTime * {B}) * 0.25;

                    switch ({rand.Next(4)})
                    {{
                        case 0:
                            return d + d1 * 0.33 + d2 * 0.02;

                        case 1:
                            return d + d1 + d2 * sin(uTime);

                        case 2:
                            return d + d3 * 0.33 + d4 * 0.02;

                        case 3:
                            return d + d3 + d4 * sin(uTime) * 0.1;
                    }}
                }}

                float RayMarch(vec3 ro, vec3 rd)
                {{
                    float dTotal = 0.0;                     {"" /* Total distance the ray has traveled */ }

                    for (int i = 0; i < MAX_STEPS; i++)
                    {{
    	                vec3 curPos = ro + rd * dTotal;     {"" /* Move along the ray */ }
                        float dS = GetDist(curPos);         {"" /* Distance-aided ray marching: treat this as a rad of a Sphere centered around our curr position*/ }
                        dTotal += dS;

                        {"" /* Here, dS is a distance to a closest object in a scene;
                               It is positive, when we're outside of an object, and is negative, when we're inside of it;
                               When dS is within the tolerance interval, we assume we've hit the surface */ }
                        if (dTotal > MAX_DIST || abs(dS) < SURF_DIST)
                            break;
                    }}
    
                    return dTotal;
                }}

                {"" /* The idea is, we can 'nudge' our point p slightly in the positive and negative direction along each of the X/Y/Z axes,
                       recalculate our SDF, and see how the values change.
                       If you are familiar with vector calculus, we are essentially calculating the gradient of the distance field at p. */
                }
                vec3 GetNormal(vec3 p)
                {{
                    //vec2 e = vec2(.001, 0);
                    //vec3 n = GetDist(p) - vec3(GetDist(p-e.xyy), GetDist(p-e.yxy), GetDist(p-e.yyx));

                    const vec3 small_step = vec3(0.001, 0.0, 0.0);

                    float gradient_x = GetDist(p + small_step.xyy) - GetDist(p - small_step.xyy);
                    float gradient_y = GetDist(p + small_step.yxy) - GetDist(p - small_step.yxy);
                    float gradient_z = GetDist(p + small_step.yyx) - GetDist(p - small_step.yyx);

                    vec3 n = vec3(gradient_x, gradient_y, gradient_z);
    
                    return normalize(n);
                }}

                // Get unit vector of the ray's direction
                vec3 GetRayDir(vec2 uv, vec3 ro, vec3 l, float z)
                {{
                    {"" /* This also works, but draws things differently, depending on the camera position */}
                    //return normalize(vec3(uv, 1));

                    {"" /* This code will orient the camera towards the object */}
                    vec3
                        f = normalize(l-ro),
                        r = normalize(cross(vec3(0, 1, 0), f)),
                        u = cross(f, r),
                        c = f * z,
                        i = c + uv.x*r + uv.y*u;

                    return normalize(i);
                }}
            ";

            main = $@"

                vec2 uv = (gl_FragCoord.xy - 0.5 * iResolution.xy) / iResolution.y;

                // Camera position (Ray Origin)
                vec3 rayOrigin = vec3(-2.0, 3.0, -5.0);

                // Rotation matrix applied
                //rayOrigin.yz *= rot(uTime/5);
                //rayOrigin.yx *= rot(uTime/5);
    
                // Ray direction as a unit vector
                vec3 rayDir = GetRayDir(uv, rayOrigin, vec3(0, 0, 0), 1);

                vec3 col = vec3(0);
                float opacity = 0.0;

                // Cast a ray and get a distance to the point it hits (if any)
                float d = RayMarch(rayOrigin, rayDir);

                if (d < MAX_DIST)
                {{
                    vec3 p = rayOrigin + rayDir * d;        // The point where the ray touches the surface
                    vec3 n = GetNormal(p);                  // The normal unit vector for this point (need this for shading)
                    vec3 r = reflect(rayDir, n);            // reflection direction for the ray -- not used

                    // diffuse lighting
                    //float diffuse_intensity = dot(n, normalize(vec3(1, 2, 3))) * 0.5 + 0.5;

                    vec3 light_position = vec3(2.0, -5.0, 3.0);
                    vec3 direction_to_light = normalize(p - light_position);
                    float diffuse_intensity = max(0.0, dot(n, direction_to_light));

                    // 2nd light source
                    light_position = vec3(0.0, 5.0, 0.0);
                    direction_to_light = normalize(p - light_position);
                    float dif2 = max(0.0, dot(n, direction_to_light));

                    diffuse_intensity += dif2 * 0.25;

                    col = vec3({R}, {G}, {B}) * diffuse_intensity;
                    opacity = smoothstep(0.01, 1.0, diffuse_intensity);

                    // gamma correction
                    col = pow(col, vec3(.4545));
                }}
   
                result = vec4(col, opacity);
            ";
        }

        // ---------------------------------------------------------------------------------------------------------------



    }
};
