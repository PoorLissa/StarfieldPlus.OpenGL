using GLFW;
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
    https://www.shadertoy.com/view/ms3GzH
    https://www.shadertoy.com/view/cdGGDK

    // Math links:
    dot     : https://www.cuemath.com/algebra/dot-product/
    cross   : https://www.cuemath.com/geometry/cross-product/

    // TODO:
    - Clock where we have 2 rotating bezels, one for hours and one for minutes. 
      Something similar: http://mkweb.bcgsc.ca/fun/clock/
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
            }

            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time local initialization
        private void initLocal()
        {
            doUseFullScreenShader = true;

            renderDelay = 0;

            do
            {
                R = myUtils.randFloat(rand);
                G = myUtils.randFloat(rand);
                B = myUtils.randFloat(rand);
            }
            while (R + G + B < 0.33f);

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

            System.Threading.Thread.Sleep(100);

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
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        // Select random mode and get shader code: header + main func
        private void getShader(ref string header, ref string main, bool fullScreen)
        {
            if (fullScreen)
            {
                mode = rand.Next(18);
#if DEBUG
                //mode = 17;
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
                    case 11: getShader_011(ref header, ref main); break;
                    case 12: getShader_012(ref header, ref main); break;
                    case 13: getShader_013(ref header, ref main); break;
                    case 14: getShader_014(ref header, ref main); break;
                    case 15: getShader_015(ref header, ref main); break;
                    case 16: getShader_016(ref header, ref main); break;
                    case 17: getShader_017(ref header, ref main); break;
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
                {myShaderHelpers.Generic.noiseFunc12_v1}
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
                        final *= 0.85 + noise12_v1(gl_FragCoord.xy * uv) * 0.15;
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
                {myShaderHelpers.Generic.noiseFunc12_v1}
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

                col *= 0.85 + noise12_v1(gl_FragCoord.xy * uTime * 0.00025) * {myUtils.randFloat(rand, 0.2f) * 0.5};

                result = vec4(col, d);
            ";
        }

        // ---------------------------------------------------------------------------------------------------------------

        // my test
        private void getShader_007(ref string header, ref string main)
        {
            header = $@"
                {myShaderHelpers.Generic.noiseFunc12_v1}
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

                col *= 0.85 + noise12_v1(gl_FragCoord.xy * uTime * 0.00025) * {myUtils.randFloat(rand, 0.2f) * 0.5};

                result = vec4(col, d);
            ";
        }

        // ---------------------------------------------------------------------------------------------------------------

        // https://www.shadertoy.com/view/csVGR3
        private void getShader_008(ref string header, ref string main)
        {
            header = $@"
                {myShaderHelpers.Generic.noiseFunc12_v1}
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

                #define MAX_STEPS   150             // change this to adjust artefacts
                #define MAX_DIST    100.0
                #define SURF_DIST   0.001           // change this to adjust artefacts

                {myShaderHelpers.Generic.rotationMatrix}
                {myShaderHelpers.Generic.noiseFunc12_v1}

                {myShaderHelpers.SDF.boxSDF}
                {myShaderHelpers.SDF.sphereSDF}
                {myShaderHelpers.SDF.roundBoxSDF}
                {myShaderHelpers.SDF.hexPrismSDF}
                {myShaderHelpers.SDF.triPrismSDF}
                {myShaderHelpers.SDF.capsuleSDF}
                {myShaderHelpers.SDF.verticalCapsuleSDF}
                {myShaderHelpers.SDF.cylinderSDF}
                {myShaderHelpers.SDF.cappedCylinderSDF}
                {myShaderHelpers.SDF.coneSDF}
                {myShaderHelpers.SDF.torusSDF}
                {myShaderHelpers.SDF.octahedronSDF}

                {"" /* To display more than 1 shape, calculate all the shapes and then return the shortest distance */ }
                float GetDist(vec3 p)
                {{
                    float d = 0;

                    switch ({rand.Next(12)})
                    {{
                        case  0: d = sphereSDF(p, 1); break;
                        case  1: d = boxSDF(p, vec3(1)); break;
                        case  2: d = roundBoxSDF(p, vec3(1), 0.33); break;
                        case  3: d = hexPrismSDF(p, vec2(2, 0.66)); break;
                        case  4: d = triPrismSDF(p, vec2(2, 0.66)); break;
                        case  5: d = capsuleSDF(p, vec3(1, 2, 3)/2, vec3(3, 2, 1)/2, 0.5); break;
                        case  6: d = verticalCapsuleSDF(p, 1.5, 0.75); break;
                        case  7: d = cylinderSDF(p, vec3(1, 2, 3)); break;
                        case  8: d = cappedCylinderSDF(p, 1.5, 1); break;
                        case  9: d = coneSDF(p, vec2(1, 1)/{rand.Next(10)+1}); break;
                        case 10: d = torusSDF(p, vec2(1, 0.5)); break;
                        case 11: d = octahedronSDF(p, 1); break;
                    }}

                    // Displacement
                    float d1 = sin(5.0 * p.x + uTime) * sin(5.0 * p.y + uTime) * sin(5.0 * p.z + uTime) * 0.25;
                    float d2 = sin(5.0 * p.x * uTime) * sin(5.0 * p.y * uTime) * sin(5.0 * p.z * uTime) * 0.25;

                    float d3 = sin(5.0 * p.x + uTime * {R}) * sin(5.0 * p.y + uTime * {G}) * sin(5.0 * p.z + uTime * {B}) * 0.25;
                    float d4 = sin(5.0 * p.x * uTime * {R}) * sin(5.0 * p.y * uTime * {G}) * sin(5.0 * p.z * uTime * {B}) * 0.25;

                    switch ({rand.Next(5)})
                    {{
                        case 0:
                            return d + d1 * 0.33 + d2 * 0.02;

                        case 1:
                            return d + d1 + d2 * sin(uTime);

                        case 2:
                            return d + d3 * 0.33 + d4 * 0.02;

                        case 3:
                            return d + d3 + d4 * sin(uTime) * 0.1;

                        case 4:
                        {{
                            float aa = 5 * sin(uTime / 2 + p.y),
                                  bb = 5 * sin(uTime / 3 + p.z),
                                  cc = 5 * sin(uTime / 4 + p.x),
                                  zz = sin(aa * p.x + uTime * {R}) * sin(bb * p.y + uTime * {G}) * sin(cc * p.z + uTime * {B}) * 0.25;

                            return d + zz * 0.25;
                        }}
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
                rayOrigin.yz *= rot(uTime/{rand.Next(33)+1});
                rayOrigin.yx *= rot(uTime/{rand.Next(33)+1});
    
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

                    diffuse_intensity += 0.25;

                    col = vec3({R}, {G}, {B}) * diffuse_intensity;
                    opacity = smoothstep(0.01, 1.0, diffuse_intensity);

                    // gamma correction
                    col = pow(col, vec3(.4545));
                }}
   
                result = vec4(col, opacity);
            ";
        }

        // ---------------------------------------------------------------------------------------------------------------

        // My Mandelbrot Set
        private void getShader_011(ref string header, ref string main)
        {
            header = $@"
                {myShaderHelpers.Generic.noiseFunc12_v1}

                #define MOD3 vec3(.1031,.11369,.13787)

                vec3 hash31(float p) {{
                    vec3 p3 = fract(vec3(p * {myUtils.randFloat(rand)}) * MOD3);
                    p3 += dot(p3, p3.yzx + 19.19);
                    return fract(vec3((p3.x + p3.y)*p3.z, (p3.x+p3.z)*p3.y, (p3.y+p3.z)*p3.x));
                }}

                bool useHash = 0 == {rand.Next(2)};
            ";

            main = $@"

                vec2 c = (gl_FragCoord.xy - 0.5 * iResolution.xy) / iResolution.y;

                float zoom = pow(1.01, uTime);

                //c.x -= 0.50 + zoom * 0.5;
                //c.y += 0.25 + zoom * 0.0;
    
                c /= zoom;

                c.x -= 1.4 + zoom * 0.0001;
    
                vec2 z = vec2(0.0);
                float clr = 0, i = 0.0, maxStep = 500.0, X, Y;
    
                for (; i < maxStep; i++)
                {{
                    X = z.x * z.x;
                    Y = z.y * z.y;

                    if (X + Y > 4.0)
                        break;

                    z = vec2(X - Y, 2.0 * z.x * z.y) + c;
                }}

                if (i >= maxStep)
                {{
                    clr = (0.1 + noise12_v1(gl_FragCoord.xy * c) * 0.23);
                    result = vec4(clr);
                }}
                else
                {{
                    clr = (maxStep/10) * log(i * maxStep) / maxStep;
                    float nz = 0.9 + noise12_v1(gl_FragCoord.xy * c) * 0.1;

                    result = useHash ? vec4(hash31(i)*nz, clr) : vec4(vec3(clr*nz), clr);
                }}
            ";
        }

        // ---------------------------------------------------------------------------------------------------------------

        // https://www.shadertoy.com/view/cdG3z3
        private void getShader_012(ref string header, ref string main)
        {
            header = $@"

                float noise(vec2 p) {{

                    return sin(p.x) * cos(p.y);

                    float nz = fract(sin(dot(p, vec2(12.9898, 78.233))) * 43758.5453123);
                    return nz * 0.25;
                    return 0.25;
                    //return texture(iChannel0, p * 0.05 ).x;
                }}

                float fbm(vec2 p) {{
                    float a = 1.0;
                    float f = 1.0;

                    return a * 1.00 * noise(p)
                         + a * 0.50 * noise(p*f*2.0)
                         + a * 0.25 * noise(p*f*4.0)
                         + a * 0.10 * noise(p*f*8.0);
                }}

                float circle(vec2 p) {{
                    float r = length(p);
                    float radius = 0.4;
                    float height = 1.0;
                    float width = 150.0;
    
	                return height - pow(r - radius, 2.0) * width;
                }}
            ";

            main = $@"

                vec2 uv = (gl_FragCoord.xy - 0.5 * iResolution.xy) / iResolution.y;

                vec2 st = vec2(atan(uv.y, uv.x), length(uv) * 1.0 + uTime * 0.1);
    
                st.x += st.y * 1.1;                 // - iTime * 0.3;
                st.x = mod(st.x, {Math.PI*2});
    
                float n = fbm(st) * 1.5 - 1.0;

                n = noise(uv * uTime * 3) + noise(uv * uTime * 7) + noise(uv * uTime * 11);

n = noise(uv * uTime * 3) + noise(uv * uTime * 7) + noise(uv * uTime * 11) + noise(uv * uTime * 17);
//result = vec4(vec3(n), 1.0); return;


// good
//n = noise(uv * uTime * 3) + noise(uv * uTime * 7) + noise(uv * uTime * 11);
//result = vec4(vec3(n), n); return;

//result = vec4(vec3(n / uv.x), 1.0); return;

                n = max(n, 0.1);

                //n = 0.05;

                float Circle = 1 - circle(uv);
                Circle = max(Circle, 0.0);
    
                float color = n/Circle;

                float mask = smoothstep(0.48, 0.4, length(uv));
    
                color *= mask;
                vec3 rez = vec3(1.0, 0.5, 0.25) * color;

                result = vec4(rez, 1.0);
            ";
        }

        // ---------------------------------------------------------------------------------------------------------------

        // https://www.shadertoy.com/view/mdy3R3
        private void getShader_013(ref string header, ref string main)
        {
            header = $@"
            ";

            main = $@"

                vec2 uv = (gl_FragCoord.xy - 0.5 * iResolution.xy) / iResolution.y;

                uv *= 2;

                float time = uTime * 0.5;

                float gearRadius = 0.4;
                float toothSize = 0.1;
                float toothWidth = 0.2;
                float toothCount = 12;
                float rotationSpeed = 1.0;
    
                vec2 gearCenter = vec2(0.0, 0.0);

                //gearCenter = vec2(sin(time/3), cos(time/7)) / 10;

                float angle = atan(uv.y, uv.x);                 // angle from the center to the current point (x, y)
                float radius = length(uv - gearCenter);

                //angle += sin(time * cos(angle));
    
                float toothAngle = {2.0 * Math.PI} / toothCount;
                float toothDistance = mod(angle + time * rotationSpeed, toothAngle) - toothAngle * 0.5;
                float toothRadius = gearRadius + toothSize * smoothstep(0.0, toothWidth, abs(toothDistance));

                float circleDist = abs(radius - toothRadius) - toothSize * 0.5;

                vec3 color = vec3(1.0 - smoothstep(0.0, 0.005, circleDist));

                result = vec4(color, 1.0);
            ";
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void getShader_014(ref string header, ref string main)
        {
            header = $@"

                {myShaderHelpers.Generic.rotationMatrix}
                {myShaderHelpers.Generic.noiseFunc12_v1}

            ";

            main = $@"

                vec2 uv = (gl_FragCoord.xy - 0.5 * iResolution.xy) / iResolution.y;

                uv *= rot(uTime * 0.5);

                float nz1 = abs(noise12_v1(uv) * sin(uv.x * uTime * 5 + uv.y));
                float nz2 = abs(noise12_v1(uv) * sin(uv.y * uTime * 5 + uv.x));

                vec3 color1 = vec3(1.0 - smoothstep(0.0, 1.0, nz1 * 2));
                vec3 color2 = vec3(1.0 - smoothstep(0.0, 1.0, nz2 * 2));

                float opacity = 1.0;

                opacity = smoothstep(0.0, 1.0, (nz1 + nz2)/2);

                opacity *= (1 - length(uv));

                result = vec4(color1 + color2, opacity);
            ";

            main = $@"

                vec2 uv = (gl_FragCoord.xy - 0.5 * iResolution.xy) / iResolution.y;
                uv *= 3;

                float t = uTime/2, d = 1;
    
                for (float i = 0; i < {2.0 * Math.PI}; i += {Math.PI/50})
                {{
                    d = min(d, length(uv-vec2(sin(5.*i-t), cos(7.*i-t))-.75*sin(i)));
                }}
    
                result = vec4(1.0 - d * 3.0) * vec4(0.8, 0.95, 0.2, 1.0);
            ";
        }

        // ---------------------------------------------------------------------------------------------------------------

        // https://www.shadertoy.com/view/mdG3Dw
        private void getShader_015(ref string header, ref string main)
        {
            header = $@"

                {myShaderHelpers.Generic.rotationMatrix}
                {myShaderHelpers.Generic.noiseFunc12_v1}

                vec4 myColor = vec4(0.8, 0.95, 0.2, 1.0);
                float res, Dist = 100, step = 1;
                int cnt = 0, n = 0;
            ";

            main = $@"

                vec2 uv = (gl_FragCoord.xy - 0.5 * iResolution.xy) / iResolution.y;
                uv *= 5;";

            //switch (rand.Next(3))
            switch (1)
            {
                // Worm
                case 0:
                    main += $@"

                        int drawMode = {rand.Next(5)}, colMode = {rand.Next(2)}, interval = 6;

                        vec3 col = vec3(1);

                        switch (drawMode)
                        {{
                            case 0: step = {0.05 + myUtils.randFloat(rand) * 0.20}; break;
                            case 1: step = {0.01 + myUtils.randFloat(rand) * 0.04}; break;
                            case 2: step = {0.05 + myUtils.randFloat(rand) * 0.03}; break;
                            case 3: step = {0.01 + myUtils.randFloat(rand) * 0.24}; break;
                            case 4: step = {0.05 + myUtils.randFloat(rand) * 0.20}; break;
                        }}

                        float nSteps = interval/step;

                        float yTimeFactor = {0.1 + myUtils.randFloat(rand) + rand.Next(6)};
                        float xTimeFactor = {0.1 + myUtils.randFloat(rand) * 1.0};

                        for (float i = interval/2; i > -interval/2; i -= step)
                        {{
                            float y = sin(i + uTime * yTimeFactor),
                                  x = i + cos(uTime * xTimeFactor),
                               dist = length(uv - vec2(x, y));

                            if (dist < Dist) {{
                                Dist = dist;
                                n = cnt;
                            }}

                            cnt++;
                        }}

                        switch (drawMode)
                        {{
                            case 0: res = 1 - Dist * 3.0; break;
                            case 1: res = 1 - Dist * 3.0 * (n + 1 + 5); break;
                            case 2: res = 1 - Dist * 3.0 * (n + 1); break;
                            case 3: res = 1 - Dist * 3.0 * (0.05 * (n+5)); break;
                            case 4: res = 1 - Dist * 0.1 * (0.95 * (interval/step - n)); break;
                        }}

                        switch (colMode)
                        {{
                            case 0: col = vec3(1); break;
                            case 1: col = vec3(1-n/nSteps, n/nSteps, n/nSteps/2); break;
                        }}

                        result = vec4(vec3(res) * col, smoothstep(0.1, 0.2, res));
                    ";
                    break;

                case 1:
                    main += $@"

                        //uv *= rot(uTime * 0.01);

                        float interval = 6;
                        float step = 0.05;
                        float t = uTime + {myUtils.randFloat(rand) + rand.Next(1234)};

                        float ytf = {1.0 / (myUtils.randFloat(rand) + rand.Next(11) + 0.0001)};
                        float xtf = {1.0 / (myUtils.randFloat(rand) + rand.Next(11) + 0.0001)};

                        for (float i = 0; i < interval; i+=step)
                        {{
                            float y = 2 * sin(i * t * ytf);
                            float x = (i - interval * 0.5) * (sin(i + t * xtf));

                            float dist = length(uv - vec2(x, y));

                            if (dist < Dist) {{
                                Dist = dist;
                                n = cnt;
                            }}

                            cnt++;
                        }}

                        res = 1 - Dist * 7;

                        result = vec4(vec3(res), smoothstep(0.1, 0.2, res));
                    ";
                    break;

                case 2:
                    main += $@"
                        //vec4 myColor = vec4({R}, {G}, {B}, 1.0);

                        uv *= rot(uTime/5);

                        float t = uTime/2, d = 1;

                        int cnt = 0;
    
                        for (float i = 0; i < {2.0 * Math.PI}; i += {Math.PI / 50})
                        {{
                            // Trajectory as a function of time (and i)
                            vec2 traj = vec2(sin(5.0 * i - t), cos(7.0 * i - t));

                            // For each i, get the distances between current uv point and the trajectory point
                            float len = length(uv - traj - 0.75 * sin(i));

                            // Select trajectory point which is the closest to out uv point
                            d = min(d, len);

                            //if(cnt++ == 3) break;
                        }}
    
                        result = vec4(1.0 - d * 3.0) * myColor;

                        float nz = 0.95 + noise12_v1(gl_FragCoord.xy) * 0.05;
                        result.xyz *= nz;
                    ";
                    break;
            }
        }

        // ---------------------------------------------------------------------------------------------------------------

        // My circle fractal
        private void getShader_016(ref string header, ref string main)
        {
            header = $@"

                #define t uTime
                vec4 myColor = vec4({R}, {G}, {B}, 1.0);

                float Circl3(vec2 uv, int mode)
                {{
                    float X = uv.x * uv.x;
                    float Y = uv.y * uv.y;

                    float a = X * 0.0125;
                    float b = Y * 0.0125;

                    float val = 0;

                    switch (mode)
                    {{
                        case 0:
                            val = abs(sin((a + b) * 10001 + t * {rand.Next(5) + 1}));
                            return 1.0 - smoothstep(0.01, 0.99, val);

                        case 1:
                            val = sin((a + b) * 10001 + t * {rand.Next(5) + 1});
                            return 1.0 - smoothstep(0.01, 0.99, val);

                        case 2:
                            val = abs(sin((X + Y) * 10001 + t*0.1));
                            return 1.0 - smoothstep(0.0, 0.03, val);

                        case 3:
                            val = abs(sin((X + Y) * 10001 + t*0.1));
                            return 1.0 - smoothstep(0.0, abs(sin(X + Y + t)), val);

                        case 4:
                            val = sin((X + Y) * (1000 + {rand.Next(100000)}) + t * 2);
                            return smoothstep(0.00001, 0.9, val);
                    }}

                    return 0;
                }}
            ";

            main = $@"

                int mode = {rand.Next(5)};

                vec2 uv = (gl_FragCoord.xy - 0.5 * iResolution.xy) / iResolution.y;

                // Dive deeper
                switch (mode)
                {{
                    case 0: uv *= {rand.Next(05) + 1}; break;
                    case 1: uv *= {rand.Next(05) + 1}; break;
                    case 2: uv *= {rand.Next(50) + 1}; break;
                    case 3: uv *= {rand.Next(50) + 1}; break;
                    case 4: uv *= {rand.Next(50) + 1}; break;
                }}

                float circ = Circl3(uv, mode);

                result = vec4(circ) * myColor;
            ";
        }

        // ---------------------------------------------------------------------------------------------------------------

        // My rotating arcs
        private void getShader_017(ref string header, ref string main)
        {
            header = $@"

                #define t uTime
                #define pi1x {Math.PI}
                #define pi2x {Math.PI * 2}
                vec4 myColor = vec4({R}, {G}, {B}, 1.0);

                float arc(vec2 uv, float rad, float th, float a, float t, float arc)
                {{
                    float at = (atan(uv.y, uv.x));
                    float len = length(uv);

                    a = mod(a, pi2x);

                    // This happens when uv.y < 0 -- Now [0 <= at <= 2P]
                    if (at < 0)
                      at = pi2x + at;

                    if (at < a && (a + arc < pi2x || at > a + arc - pi2x))
                            return 0;
                    
                    if (at > a + arc)
                        return 0;

                    return len < rad ? smoothstep(rad - th, rad, len) : 1 - smoothstep(rad, rad + th, len);
                }}
            ";

            main = $@"

                vec2 uv = (gl_FragCoord.xy - 0.5 * iResolution.xy) / iResolution.y;

                float f = 0;

                float th = {0.005 + myUtils.randFloat(rand) * 0.015};

                for (int i = 0; i < 25; i++)
                {{
                    float rad = 0.75 - i * 0.03;
                    float angle = uTime * (i + 1) * {0.05 + myUtils.randFloat(rand) * 0.15};

                    f += arc(uv, rad, th, angle, 0, 1.0 + i * {0.05 + myUtils.randFloat(rand) * 0.15});
                }}

                result = vec4(f) * myColor;
            ";
        }

        // ---------------------------------------------------------------------------------------------------------------







    }
};
