using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;


/*
    - Free shader experiments

    https://www.shadertoy.com/view/3tXXRn

    Read this later https://miketuritzin.com/post/rendering-particles-with-compute-shaders/
*/


namespace my
{
    public class myObj_500 : myObject
    {
        private float R, G, B;

        private string fHeader = "", fMain = "";

        private myFreeShader shader = null;

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
            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            height = 600;

            string nStr(int   n) { return n.ToString("N0");    }
            string fStr(float f) { return f.ToString("0.000"); }

            string str = $"Obj = myObj_500 -- Free Shader Experiments\n\n"  +
                            $"N = {0}\n"                                    +
                            $"renderDelay = {renderDelay}\n"                +
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

        protected override void Process(Window window)
        {
            uint cnt = 0;

            glDrawBuffer(GL_FRONT_AND_BACK | GL_DEPTH_BUFFER_BIT);

            getMainShader(ref fHeader, ref fMain);

            shader = new myFreeShader(fHeader: fHeader, fMain: fMain);

            while (!Glfw.WindowShouldClose(window))
            {
                processInput(window);

                // Swap fore/back framebuffers, and poll for operating system events.
                Glfw.SwapBuffers(window);
                Glfw.PollEvents();

                // Render Frame
                {
                    shader.Draw();
                }

                cnt++;
                System.Threading.Thread.Sleep(renderDelay);
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void getMainShader(ref string header, ref string main)
        {
            int max = 3;
            int mode = rand.Next(max);

            //mode = 3;

            // Default header
            header = " ";

            switch (mode)
            {
                case 0: getShader_000(ref header, ref main); break;
                case 1: getShader_001(ref header, ref main); break;
                case 2: getShader_002(ref header, ref main); break;
                case 3: getShader_003(ref header, ref main); break;
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        // https://www.shadertoy.com/view/tdlBR8
        private void getShader_000(ref string header, ref string main)
        {
            header = @"

                float concentric(vec2 m, float repeat, float t) {
                    float r = length(m);
                    float v = sin((1.0 - r) * (1.0 - r) * repeat + t) * 0.5 + 0.5;
                    return v;
                }

                float spiral(vec2 m, float repeat, float dir, float t) {
	                float r = length(m);
	                float a = atan(m.y, m.x);
	                float v = sin(repeat * (sqrt(r) + (1.0 / repeat) * dir * a - t)) * 0.5 + 0.5;
	                return v;
                }
            ";

            main = $@"

                vec2 iResolution = vec2({gl_Width}, {gl_Height});
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
            main = $@"

                vec2 iResolution = vec2({gl_Width}, {gl_Height});

                // Normalized pixel coordinates (from 0 to 1)
                vec2 uv = (gl_FragCoord.xy - iResolution.xy * 0.5) / iResolution.y;

                float mask = smoothstep(0.5, 0.0, length(uv));
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

                // Output to screen
                result = vec4(vec3(final * {R}, final * {G}, final * {B}), 1.0);
            ";
        }

        // ---------------------------------------------------------------------------------------------------------------

        // https://www.shadertoy.com/view/MdKBzt
        private void getShader_002(ref string header, ref string main)
        {
            header = @"

                float distLine(vec3 ro, vec3 rd, vec3 p) {
                    float d = length(cross(p-ro, rd))/length(rd);
                    d = smoothstep(.06, .05, d);
	                return d;
                }

                float box(vec3 ro, vec3 rd) {
                    float d = 0.;
                    d += distLine(ro, rd, vec3(0., 0., 0.));
                    d += distLine(ro, rd, vec3(0., 1., 0.));
                    d += distLine(ro, rd, vec3(1., 1., 0.));
                    d += distLine(ro, rd, vec3(1., 0., 0.));
                    d += distLine(ro, rd, vec3(0., 0., 1.0));
                    d += distLine(ro, rd, vec3(0., 1., 1.0));
                    d += distLine(ro, rd, vec3(1., 1., 1.0));
                    d += distLine(ro, rd, vec3(1., 0., 1.0));

	                return d;
                }
            ";

            main = $@"

                vec2 iResolution = vec2({gl_Width}, {gl_Height});

                vec2 uv = gl_FragCoord.xy/iResolution.xy;
                uv -= 0.5;
                uv.x *= iResolution.x/iResolution.y;

                vec3 lookAt = vec3(0.5);
                vec3 ro = vec3(3.* sin(uTime * 0.1), 2. * cos(uTime * 0.1), -3.);
                float zoom = 0.5 + sin(uTime * 0.1) * 0.2;
    
                vec3 f = normalize(lookAt - ro);
                vec3 r = cross(vec3(0., -1., 0.), f);
                vec3 u = cross(r, f);
                vec3 c = ro + f * zoom;
                vec3 i = c + uv.x * r + uv.y * u;
                vec3 rd = i - ro;
    
                float d = box(ro, rd);
                result = vec4(d * {R}, d * {G}, d * {B}, 0.25);
            ";
        }

        // ---------------------------------------------------------------------------------------------------------------

        // Test
        private void getShader_003(ref string header, ref string main)
        {
            main = $@"

                vec2 iResolution = vec2({gl_Width}, {gl_Height});
                float aspect = iResolution.x / iResolution.y;
    
                vec2 uv = (gl_FragCoord.xy / iResolution.xy * 2.0 - 1.0) * vec2(1.0, 1.0 / aspect);
                float r = length(uv);
                r = r * r + sin(r * uTime);

                if (r > 0.1)
                    result = vec4(vec3(1-r), 1);
            ";
        }

        // ---------------------------------------------------------------------------------------------------------------

        // Next one
        private void getShader_004(ref string header, ref string main)
        {
        }

        // ---------------------------------------------------------------------------------------------------------------
    }
};
