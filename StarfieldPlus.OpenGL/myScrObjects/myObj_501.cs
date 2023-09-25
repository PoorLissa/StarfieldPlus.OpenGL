using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;


/*
    - Free full screen shader experiments - 2
*/


namespace my
{
    public class myObj_501 : myObject
    {
        // Priority
        public static int Priority => 13;
        public static System.Type Type => typeof(myObj_501);

        private float R, G, B;
        private int mode = 0;
        private string fHeader = "", fMain = "", stdHeader = "", shaderInfo = "";

        private myFreeShader_FullScreen shaderFull = null;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_501()
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

            string str = $"Obj = myObj_501 -- Free Shader Experiments\n\n" +
                            $"N = {nStr(1)}\n"                             +
                            $"R = {fStr(R)};\n"                            +
                            $"G = {fStr(G)};\n"                            +
                            $"B = {fStr(B)}\n"                             +
                            $"mode = {mode}\n"                             +
                            $"renderDelay = {renderDelay}\n"               +
                            $"shaderInfo: {shaderInfo}\n"                  +
                            $"file: {colorPicker.GetFileName()}"
                ;
            return str;
        }

        // ---------------------------------------------------------------------------------------------------------------

        // 
        protected override void setNextMode()
        {
            getShader(ref fHeader, ref fMain);

            System.Threading.Thread.Sleep(100);

            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Process(Window window)
        {
            uint cnt = 0;

            glDrawBuffer(GL_FRONT_AND_BACK | GL_DEPTH_BUFFER_BIT);

            getShader(ref fHeader, ref fMain);

            while (!Glfw.WindowShouldClose(window))
            {
                processInput(window);

                // Swap fore/back framebuffers, and poll for operating system events.
                Glfw.SwapBuffers(window);
                Glfw.PollEvents();

                glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT);

                // Render Frame
                {
                    shaderFull.Draw();
                }

                cnt++;
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        // Select random mode and get shader code: header + main func
        private void getShader(ref string header, ref string main)
        {
            shaderInfo = string.Empty;

            stdHeader = $@"
                vec4 myColor = vec4({R}, {G}, {B}, 1.0);
                {myShaderHelpers.Generic.rotationMatrix}
                float t = uTime;
                vec2 uv = (gl_FragCoord.xy - 0.5 * iResolution.xy) / iResolution.y;";

            mode = rand.Next(13);

#if DEBUG
            //mode = 13;
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
            }

            shaderFull = new myFreeShader_FullScreen(fHeader: fHeader, fMain: fMain);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void getShader_000(ref string header, ref string main)
        {
            shaderInfo += $"shader 000\n";

            string getVal = "";
            header = stdHeader;

            switch (rand.Next(2))
            {
                case 0: getVal = "x * sin(y) + len"; break;
                case 1: getVal = "x + sin(y) + cos(len) + y * cos(y) + len"; break;
            }

            main = $@"
                    uv *= 12.5;

                    float x = uv.x + 0;
                    float y = uv.y + 0;

                    float len = length(uv);
                    float val = {getVal};

                    for (int i = 0; i < 3; i++)
                    {{
                        val += sin(x + y + t) * 0.5;
                        val += cos(x - y + t) * 0.5;

                        val = min(cos(val) * len * t, val);

                        myColor[i] *= smoothstep(-10, 10, val);

                        val = cos(val) * len * t;
                        val = smoothstep(0, 1, val) * 3;
                    }}

                    val *= sin(val);
                    val = smoothstep(0, 0.5, val);
                    result = vec4(myColor.xyz, val);
                ";
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void getShader_001(ref string header, ref string main)
        {
            shaderInfo += $"shader 001\n";

            header = stdHeader;

            main = $@"
                    uv *= 12.5;

                    myColor.x += abs(uv.x + t);
                    myColor.y *= abs(uv.y + t);
                    myColor.z *= abs(uv.x + t);

                    uv *= rot(t * 0.01);

                    float len = length(uv);

                    float a = abs(uv.x + sin(1*t) * 3);
                    float b = abs(uv.y + cos(1*t) * 3);

                    float val = min(a, b) * sin(len*t) * 4;

                    val *= max(a, b) * sin(t*0.1*uv.x) * 2;

                    float a2 = abs(uv.y + sin(1*t) * 11);
                    float b2 = abs(uv.x + cos(1*t) * 11);

                    val *= min(a2, b2) * sin(len) * 11;
                    val *= val;
                    val = smoothstep(0, 0.3, val);

                    result = vec4(myColor.xyz, val);
                ";
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void getShader_002(ref string header, ref string main)
        {
            shaderInfo += $"shader 002\n";

            header = stdHeader;

            main = $@"
                    uv *= rot(t * 0.01);
                    uv *= 12.5;

                    float len = length(uv);

                    float a = abs(uv.x + sin(1*t) * 3);
                    float b = abs(uv.y + cos(1*t) * 3);

                    float val = min(a, b) * sin(len*t) * 4;

                    val *= min(a, b) * sin(len*t*0.1) * 2;

                    float a2 = abs(uv.y + sin(1*t) * 3);
                    float b2 = abs(uv.x + cos(1*t) * 3);

                    val *= min(a2, b2) * sin(len) * 11;
                    val *= val;
                    val = smoothstep(0.0, 0.3, val);

                    result = vec4(vec3(myColor.x + abs(uv.x), myColor.y * abs(uv.y), myColor.z * abs(uv.x)), val);
                ";
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void getShader_003(ref string header, ref string main)
        {
            shaderInfo += $"shader 003\n";

            header = stdHeader;

            main = $@"
                    //uv = (gl_FragCoord.xy) / iResolution.y / 16.0 + vec2(uTime / 2.0, t / 3.0) / 64.0;

                    uv = (gl_FragCoord.xy - 0.5 * iResolution.xy) / iResolution.y + vec2(t / 2.0, 1) * 0.1;
                    uv *= 1 + sin(t) * 0.05;
                    uv *= 0.25;
                    uv *= rot(t * 0.01);

                    float col = 0;

                    for (int k = 0; k < 9; k++)
                    {{
                        uv = abs(fract(uv.yx - vec2(uv.x, -uv.y) * 2.0) - 0.5);

                        col = max(length(uv / 2.0), col);
                        col = max(abs(col * 2.0 - 1.0), col / 4.0);
                    }}

                    result = vec4(min(vec3(col * 2.0), vec3(1.0)), 1.0);
                    result.xyz *= myColor.xyz;
                ";
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void getShader_004(ref string header, ref string main)
        {
            shaderInfo += $"shader 004\n";

            float thickness = 0.2f;
            float mult = 10.0f;

            header = stdHeader;
            header +=
                $@"float shape2(vec2 uv, float rad)
                    {{
                        float res = 0, len = length(uv);

                        float val = (rad - len) * {thickness};

                        float val2 = abs(sin(uv.x)) * abs(sin(uv.y)) * {mult};

                        res = smoothstep(0.0, val2, val) * 0.5;

                        if (abs(val - val2) < 0.001)
                            res += (1 - smoothstep(-0.01, 0.01, (val - val2)));

                        return res;
                    }}
                ";

            main = $@"
                    uv *= 1 + sin(t * 1.01) * 0.5;
                    uv *= rot(t * 0.1);
                    float r = shape2(uv, 0.23);

                    uv *= 1 + sin(t * 1.01) * 0.5;
                    r *= shape2(uv, 0.23);

                    result = vec4(myColor.xyz, r);
                ";
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void getShader_005(ref string header, ref string main)
        {
            shaderInfo += $"shader 005\n";

            header = stdHeader;
            header +=
                $@"
                    float nRays = 17;
                    #define pi1x {Math.PI * 1}
                    #define pi2x {Math.PI * 2}";

            main = $@"
                    uv *= rot(uTime * 0.05);

                    float at = atan(uv.y, uv.x);
                    float len = length(uv);
                    float rad = 0.35;

                    float color = smoothstep(-0.001, 0.025, rad - len);

                    float val = len - rad + sin(t) * 0.1;

                    float f = sin(at * nRays) * 0.25;

                    color = smoothstep(0, 0.1, (val) * f * 123.1);

                    if (len < 0.1 + sin(t * 0.793) * 0.09)
                        color = smoothstep(0.01, 0.3, len);

                    if (len < 0.1 + sin(t * 0.937) * 0.08)
                        color += smoothstep(0.01, 0.3, len);

                    if (len < 0.2 + cos(t * 0.317) * 0.13)
                        color += smoothstep(0.01, 0.3, len) * 0.3;

                    result = vec4(vec3(color), 1.0);
                ";
        }

        // ---------------------------------------------------------------------------------------------------------------

        // Circle with solid border
        private void getShader_006(ref string header, ref string main)
        {
            shaderInfo += $"shader 006\n";

            header = stdHeader;
            header += $@"
                    #define pi1x {Math.PI * 1}
                    #define pi2x {Math.PI * 2}

                    float F(vec2 uv, float rad)
                    {{
                        float at = atan(uv.y, uv.x);
                        float len = length(uv);

                        float val = len - rad;
                        float absVal = abs(val);

                        if (len < rad)
                        {{
                            float res = 0, off = abs(len - rad + 0.05);

                            if (off < 0.0025)
                                res = (1 - smoothstep(0, 0.001, off)) * 2;

                            return res + smoothstep(0, 0.05, absVal) * 0.5;
                        }}

                        return 0;
                    }}

                    float F1(vec2 uv, float rad)
                    {{
                        float at = atan(uv.y, uv.x);
                        float len = length(uv);

                        float val = len - rad;
                        float absVal = abs(val);

                        if (absVal < 0.001)
                            return (1 - smoothstep(0, 0.001, absVal)) * 2;

                        if (len < rad)
                            return smoothstep(0, 0.05, absVal);

                        return 0;
                    }}
                ";

            main = $@"
                    float rad = 0.35 + sin(t * 0.1) * 0.25;
                    float color = F(uv, rad);
                    result = vec4(myColor.xyz, color);
                ";
        }

        // ---------------------------------------------------------------------------------------------------------------

        // Circle made of sin curve
        private void getShader_007(ref string header, ref string main)
        {
            shaderInfo += $"shader 007\n";

            header = stdHeader;
            header += $@"
                float nRays = {rand.Next(23) + 3};
                #define pi1x {Math.PI}
                #define pi2x {Math.PI * 2}
            ";

            main = $@"
                uv *= rot(uTime * 0.05);

                float at = atan(uv.y, uv.x);
                float len = length(uv);
                float rad = 0.35;

                float val = len - rad + sin(at * nRays) * rad/25 + cos(at * 13) * rad/33;

                float color = 1 - smoothstep(0, 0.025, abs(val));   // remove abs for a solid shape

                result = vec4(vec3(color), 1.0);
            ";
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void getShader_008(ref string header, ref string main)
        {
            shaderInfo += $"shader 008\n";

            header = stdHeader;
            header += $@"
                float f1(vec2 p) {{ return sin(p.x) * cos(p.y); }}
                float f2(vec2 p) {{ return sin(p.y) * cos(p.x); }}
            ";

            main = $@"
                uv *= t;

                float len = length(uv);

                float val1 = f1(uv) * {(rand.Next(2) == 0 ? "sin(t)" : "sin(t + len)")};
                float val2 = f2(uv) * cos(t);

                float a = 0.3;
                float r = smoothstep(-a, +a, val1 - val2);

                result = vec4(myColor.xyz, r);
            ";
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void getShader_009(ref string header, ref string main)
        {
            string v1 = "", v2 = "";

            switch (rand.Next(4))
            {
                case 0: v1 = "val1 + val1";     break;
                case 1: v1 = "val1 + val1 + t"; break;
                case 2: v1 = "val1 + val2";     break;
                case 3: v1 = "val1 + val2 + t"; break;
            }

            switch (rand.Next(4))
            {
                case 0: v2 = "val2 * val2";     break;
                case 1: v2 = "val2 * val2 + t"; break;
                case 2: v2 = "val1 * val2";     break;
                case 3: v2 = "val1 * val2 + t"; break;
            }

            shaderInfo += $"shader 009:\nv1 = {v1}\nv2 = {v2}";

            header = stdHeader;
            header += $@"

                // x * sin(x) * cos(y)
                float f1(vec2 p)
                {{
                    float x = p.x;
                    float y = p.y;

                    return abs(sin(x) * cos(y));
                }}

                float f2(vec2 p)
                {{
                    float x = p.x;
                    float y = p.y;

                    return (x + y) * (x + y);
                }}
            ";

            main = $@"

                uv *= rot(-3.14/4);
                uv *= 2;

                float at = atan(uv.y, uv.x);
                float len = length(uv);

                float val1 = f1(uv * t * 0.5);
                float val2 = f2(uv);

                val1 = sin({v1});
                val2 = cos({v2});

                float a = 0.3;
                float r = smoothstep(-a, +a, val1 - val2);

                r *= fract(r);

                result = vec4(myColor.xyz, r);
            ";
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void getShader_010(ref string header, ref string main)
        {
            shaderInfo += $"shader 010\n";

            header = stdHeader;

            string R = "", rotate = "", changeColor = "";

            switch (rand.Next(4))
            {
                case 0: rotate = "uv *= rot(+t * 0.01);"; break;
                case 1: rotate = "uv *= rot(-t * 0.02);"; break;
            }

            float smooth = 0.3f + myUtils.randFloat(rand) * 0.7f;

            switch(rand.Next(11))
            {
                case 00: R = "r";                           break;
                case 01: R = "r * r";                       break;
                case 02: R = "r + S";                       break;
                case 03: R = "r * S";                       break;
                case 04: R = "r / S";                       break;
                case 05: R = "S * S + r";                   break;
                case 06: R = "S * S * r";                   break;
                case 07: R = "S * S + r * r";               break;
                case 08: R = "(r + S) * (r + S) + r + S";   break;
                case 09: R = "(r + S) * (r - S) + r + S";   break;
                case 10: R = "(r + S) * (r + S) + r * S";   break;
            }

            switch (rand.Next(3))
            {
                case 0: changeColor = "myColor.x *= R"; break;
                case 1: changeColor = "myColor.y *= R"; break;
                case 2: changeColor = "myColor.z *= R"; break;
            }

            shaderInfo += $"rotate: {rotate}\nR = {R}\n";

            main = $@"

                float len = length(uv);

                uv *= 33 + {rand.Next(133)} + 33 * sin(t * 0.2);

                {rotate}

                float R = 0, S = 0, r = 0;
                
                r = cos(uv.x + sin(t));
                S = sin(uv.y + cos(t));

                r = sin(r - S + t);

                R = smoothstep(0, {smooth}, {R});
                R *= 1 - len;

                {changeColor};

                result = vec4(myColor.xyz, R);
            ";
        }

        // ---------------------------------------------------------------------------------------------------------------

        // Based on getShader_010
        private void getShader_011(ref string header, ref string main)
        {
            shaderInfo += $"shader 011\n";

            header = stdHeader;

            string R = "", rotate = "", changeMyColor = "";

            switch (rand.Next(2))
            {
                case 0: rotate = "uv *= rot(+t * 0.01);"; break;
                case 1: rotate = "uv *= rot(-t * 0.02);"; break;
            }

            float smooth = 1f;

            switch (rand.Next(7))
            {
                case 0: smooth = 0.05f + myUtils.randFloat(rand) * 0.25f; break;
                case 1: smooth = 0.10f + myUtils.randFloat(rand) * 0.50f; break;
                case 2: smooth = 0.10f + myUtils.randFloat(rand) * 0.90f; break;
                case 3: smooth = 0.10f + myUtils.randFloat(rand) * rand.Next(3); break;
                case 4: smooth = 0.10f + myUtils.randFloat(rand) * rand.Next(9); break;
                case 5: smooth = 0.10f + myUtils.randFloat(rand) * rand.Next(15); break;
                case 6: smooth = 0.10f + myUtils.randFloat(rand) * rand.Next(50); break;
            }

            switch (rand.Next(2))
            {
                case 0: R = $"smoothstep(0, {smooth}, abs(r*r*r + uv.y * uv.x));"; break;
                case 1: R = $"smoothstep(0, {smooth}, abs(r*r*r + sin(uv.y * uv.x + t)))"; break;
            }

            switch (rand.Next(3))
            {
                case 0: changeMyColor = "myColor.x *= R"; break;
                case 1: changeMyColor = "myColor.y *= R"; break;
                case 2: changeMyColor = "myColor.z *= R"; break;
            }

            shaderInfo += $"rotate: {rotate}\nsmooth = {smooth}\nR = {R}";

            main = $@"

                float len = length(uv);

                uv *= 20;

                {rotate}

                float R = 0, S = 0, r = 0;
                
                r = cos(uv.x + sin(t));
                S = sin(uv.y + cos(t));
                r = sin(r - S + t);

                R = {R};
                R *= 1 - len;

                {changeMyColor};

                result = vec4(myColor.xyz, R);
            ";
        }

        // ---------------------------------------------------------------------------------------------------------------

        // The same as 011, but uv = sin(uv) + variants added

        private void getShader_012(ref string header, ref string main)
        {
            shaderInfo += $"shader 012\n";

            header = stdHeader;

            string R = "", rotate = "", changeMyColor = "", uvModif = "";

            switch (rand.Next(2))
            {
                case 0: rotate = "uv *= rot(+t * 0.01);"; break;
                case 1: rotate = "uv *= rot(-t * 0.02);"; break;
            }

            float smooth = 1f;

            switch (rand.Next(7))
            {
                case 0: smooth = 0.05f + myUtils.randFloat(rand) * 0.25f; break;
                case 1: smooth = 0.10f + myUtils.randFloat(rand) * 0.50f; break;
                case 2: smooth = 0.10f + myUtils.randFloat(rand) * 0.90f; break;
                case 3: smooth = 0.10f + myUtils.randFloat(rand) * rand.Next(3); break;
                case 4: smooth = 0.10f + myUtils.randFloat(rand) * rand.Next(9); break;
                case 5: smooth = 0.10f + myUtils.randFloat(rand) * rand.Next(15); break;
                case 6: smooth = 0.10f + myUtils.randFloat(rand) * rand.Next(50); break;
            }

            switch (rand.Next(2))
            {
                case 0: R = $"abs(r*r*r + uv.y * uv.x)";         break;
                case 1: R = $"abs(r*r*r + sin(uv.y * uv.x + t))"; break;
            }

            switch (rand.Next(3))
            {
                case 0: changeMyColor = "myColor.x *= R"; break;
                case 1: changeMyColor = "myColor.y *= R"; break;
                case 2: changeMyColor = "myColor.z *= R"; break;
            }

            switch (rand.Next(3))
            {
                case 0: uvModif = "sin(uv)"; break;
                case 1: uvModif = "sin(uv) + cos(uv)"; break;
                case 2: uvModif = "sin(uv.yx) + cos(uv.xy)"; break;
            }

            shaderInfo += $"rotate: {rotate}\nsmooth = {smooth}\nR = {R}\nuv = {uvModif}";

            main = $@"

                float len = length(uv);

                uv *= 20;
                uv = {uvModif};

                {rotate}

                float R = 0, S = 0, r = 0;
                
                r = cos(uv.x + sin(t));
                S = sin(uv.y + cos(t));
                r = sin(r - S + t);

                R = smoothstep(0, {smooth}, {R});
                R *= 1 - len;

                {changeMyColor};

                result = vec4(myColor.xyz, R);
            ";
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void getShader_013(ref string header, ref string main)
        {
            shaderInfo += $"shader 012\n";

            header = stdHeader;
        }

        // ---------------------------------------------------------------------------------------------------------------

    }
};
