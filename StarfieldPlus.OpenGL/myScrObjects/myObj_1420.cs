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
    class myFreeShader_1420 : myFreeShader
    {
        private int depth = -123;

        public myFreeShader_1420(string fHeader = "", string fMain = "") : base(fHeader, fMain)
        {
            depth = depth < 0 ? glGetUniformLocation(shaderProgram, "myDepth") : depth;
        }

        public void Draw(float x, float y, float w, float h, float Depth, int extraOffset = 0)
        {
            glUniform1f(depth, Depth);
            base.Draw(x, y, w, h, extraOffset);
        }
    }

    // ===================================================================================================================

    public class myObj_1420 : myObject
    {
        // Priority
        public static int Priority => 10;
		public static System.Type Type => typeof(myObj_1420);

        private float x, y, dx, dy;
        private float size, A, R, G, B, depth = 0;

        private static int N = 0;
        private static int move2Mode = 0, dirMode = 0, sizeMode = 0, focusMode = 0, focusCnt = 0, focusCntMax = 0, opacityMode = 0, colorMode = 0;
        private static int hugeChance = 0, colorCnt = 0, linearSpeed = 0;
        private static bool doUseAmoebas = false, doUseMediums = false;
        private static float dimAlpha = 0.05f, minDepth = 0, maxDepth = 0.03f, currentFocus = 0, targetFocus = 0, dFocus = 0, t = 0, dt = 0;
        private static float gl_R = -1, gl_G = -1, gl_B = -1;

        private static List<myObj_1420> sortedList = null;
        private static myScreenGradient grad = null;
        private static myFreeShader_1420 shader = null;
        private static myFreeShader_1420 shaderBig = null;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_1420()
        {
            if (id != uint.MaxValue)
                generateNew();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time global initialization
        protected override void initGlobal()
        {
            var m = myColorPicker.colorMode.COLORMAP;

            colorPicker = new myColorPicker(gl_Width, gl_Height, mode: m);
            sortedList = new List<myObj_1420>();

            // Global unmutable constants
            {
                N = 1234;
            }

            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time local initialization
        private void initLocal()
        {
            doClearBuffer = true;
            doUseAmoebas = myUtils.randomChance(rand, 1, 2);
            doUseMediums = myUtils.randomChance(rand, 1, 2);

            move2Mode = rand.Next(2);               // If particles move straight or diagonally
            dirMode = rand.Next(3);                 // Top-down or left-right motion or free motion
            opacityMode = rand.Next(3);
            focusCntMax = 100 + rand.Next(777);     // In focusMode 1, time between focus switches

            linearSpeed = myUtils.randomChance(rand, 2, 3)
                ? rand.Next(3) + 2
                : rand.Next(12) + 2;
            colorMode = myUtils.randomChance(rand, 2, 3)
                ? 0
                : 1;
            sizeMode = myUtils.randomChance(rand, 6, 7)
                ? 0
                : 1;
            focusMode = myUtils.randomChance(rand, 1, 2)
                ? 1
                : rand.Next(3);
            hugeChance = myUtils.randomChance(rand, 1, 2)
                ? 66
                : 22 + rand.Next(44);

            t = 0;
            dt = 0.0001f;

            focusCnt = 10;
            targetFocus = minDepth + myUtils.randFloat(rand) * maxDepth;
            dFocus = 0.0001f + myUtils.randFloat(rand) * 0.001f;

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            height = 600;

            string _dFocus = focusMode > 0 ?
                myUtils.fStr(Math.Abs(dFocus), 5)
                : "n/a";

            string str = $"Obj = {Type}\n\n"                                    +
                            myUtils.strCountOf(sortedList.Count, N)             +
                            $"doUseAmoebas = {doUseAmoebas}\n"                  +
                            $"doUseMediums = {doUseMediums}\n"                  +
                            $"dirMode = {dirMode}\n"                            +
                            $"linearSpeed = {linearSpeed}\n"                    +
                            $"sizeMode = {sizeMode}\n"                          +
                            $"move2Mode = {move2Mode}\n"                        +
                            $"opacityMode = {opacityMode}\n"                    +
                            $"colorMode = {colorMode}\n"                        +
                            $"focusMode = {focusMode}\n"                        +
                            $"focusCntMax = {focusCntMax}\n"                    +
                            $"currentFocus = {myUtils.fStr(currentFocus, 5)}\n" +
                            $"dFocus = {_dFocus}\n"                             +
                            $"hugeChance = {hugeChance}\n"                      +
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
            depth = myUtils.randFloat(rand) * 0.03f;

            // Pick size
            {
                if (myUtils.randomChance(rand, 1, hugeChance))
                {
                    // Huge
                    size = rand.Next(266) + 133;
                }
                else if (doUseMediums && myUtils.randomChance(rand, 1, 3))
                {
                    // Medium
                    size = rand.Next(99) + 30;
                }
                else if (myUtils.randomChance(rand, 1, 3))
                {
                    // Normal
                    size = rand.Next(66) + 11;
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

            switch (dirMode)
            {
                case 0:
                    dx = move2Mode == 0 ? 0 : myUtils.randFloatSigned(rand) * 0.33f;
                    dy = myUtils.randFloat(rand, 0.1f) * linearSpeed;

                    x = rand.Next(gl_Width);
                    y = -(33 + size);
                    break;

                case 1:
                    dx = myUtils.randFloat(rand, 0.1f) * linearSpeed;
                    dy = move2Mode == 0 ? 0 : myUtils.randFloatSigned(rand) * 0.33f;

                    x = -(33 + size);
                    y = rand.Next(gl_Height);
                    break;

                case 2:
                    dx = myUtils.randFloatSigned(rand, 0.1f) * 1;
                    dy = myUtils.randFloatSigned(rand, 0.1f) * 1;

                    x = rand.Next(gl_Width);
                    y = rand.Next(gl_Height);
                    break;
            }

            switch (opacityMode)
            {
                case 0:
                    A = 0.25f + myUtils.randFloat(rand) * 0.175f;
                    break;

                case 1:
                    A = 0.5f + myUtils.randFloat(rand) * 0.5f;
                    break;

                case 2:
                    A = 0.85f + myUtils.randFloat(rand) * 0.15f;
                    break;
            }

            // In Size mode 1, show only huge particles
            if (sizeMode == 1)
            {
                if (size < 130)
                    A = 0.01f;
            }

            switch (colorMode)
            {
                // Generate color every time
                case 0:
                    colorPicker.getColorRand(ref R, ref G, ref B);
                    break;

                // Reuse the color, generate only sometimes
                case 1:
                    {
                        if (gl_R < 0 || --colorCnt == 0)
                        {
                            colorCnt = rand.Next(100) + 123;
                            colorPicker.getColorRand(ref gl_R, ref gl_G, ref gl_B);
                        }

                        R = gl_R + myUtils.randFloatSigned(rand) * 0.1f;
                        G = gl_G + myUtils.randFloatSigned(rand) * 0.1f;
                        B = gl_B + myUtils.randFloatSigned(rand) * 0.1f;
                    }
                    break;
            }

            // Huge particles additional setup
            if (size > 130)
            {
                A = 0.1f + myUtils.randFloat(rand) * 0.175f;
                depth = -0.02f - myUtils.randFloat(rand) * 0.03f;

                if (dirMode == 0)
                    y = -(33 + size);

                if (dirMode == 1)
                    x = -(33 + size);
            }

            // Brighten up small/tiny particles
            if (size < 10)
            {
                do {

                    R += 0.0001f;
                    G += 0.0001f;
                    B += 0.0001f;

                } while (R + G + B < 2.33f);
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            x += dx;
            y += dy;

            switch (dirMode)
            {
                case 0:
                    {
                        if (y > gl_Height + size)
                            generateNew();
                    }
                    break;

                case 1:
                    {
                        if (x > gl_Width + size)
                            generateNew();
                    }
                    break;

                case 2:
                    {
                        if (y < - size || y > gl_Height + size)
                            generateNew();

                        if (x < -size || x > gl_Width + size)
                            generateNew();
                    }
                    break;
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Show()
        {
            float focus = (float)(Math.Abs(depth - currentFocus));

            if (focus < 0.001f)
                focus = 0.001f;

            int off = (int)size;
            off = off < 50 ? 50 : off;

            if (doUseAmoebas == false)
            {
                shader.SetColor(R, G, B, A);
                shader.Draw(x, y, size, size, focus, off);
            }
            else
            {
                if (size < 130)
                {
                    // Tiny - Small - Normal - Medium
                    shader.SetColor(R, G, B, A);
                    shader.Draw(x, y, size, size, focus, off);
                }
                else 
                {
                    // Huge
                    shaderBig.SetColor(R, G, B, A);
                    shaderBig.Draw(x, y, size, size, focus, off);
                }
            }
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
                int Count = sortedList.Count;

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

                SortParticles();

                // Render Frame
                {
                    for (int i = 0; i != Count; i++)
                    {
                        var obj = sortedList[i] as myObj_1420;

                        obj.Show();
                        obj.Move();
                    }
                }

                if (Count < N && myUtils.randomChance(rand, 1, 5))
                {
                    sortedList.Add(new myObj_1420());
                }

                stopwatch.WaitAndRestart();
                t += dt;
                cnt++;

                setFocus();
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
            shader = new myFreeShader_1420(fHeader, fMain);

            getShader_001(ref fHeader, ref fMain);
            shaderBig = new myFreeShader_1420(fHeader, fMain);
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

        // Sort the list, so the particles are drawn in correct z-order
        private void SortParticles()
        {
            sortedList.Sort(delegate (myObj_1420 obj1, myObj_1420 obj2)
            {
                return obj1.depth < obj2.depth
                    ? -1
                    : obj1.depth > obj2.depth
                        ? 1
                        : 0;
            });
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void setFocus()
        {
            // dist = [0 .. 0.03]
            // focusDist = [-0.01 .. 0.04]

            switch (focusMode)
            {
                case 0:
                    {
                        currentFocus = (float)(Math.Abs(Math.Sin(t * 10) * 0.05f)) - 0.01f;
                    }
                    break;

                case 1:
                    {
                        if (--focusCnt == 0)
                        {
                            focusCnt = 100 + rand.Next(focusCntMax);

                            targetFocus = minDepth + myUtils.randFloat(rand) * maxDepth;

                            targetFocus = -0.03f + myUtils.randFloat(rand) * 0.07f;

                            if (currentFocus > targetFocus)
                            {
                                dFocus = -1 * (float)Math.Abs(dFocus);
                            }
                            else
                            {
                                dFocus = +1 * (float)Math.Abs(dFocus);
                            }
                        }

                        if (dFocus > 0 && currentFocus < targetFocus)
                        {
                            currentFocus += dFocus;
                        }

                        if (dFocus < 0 && currentFocus > targetFocus)
                        {
                            currentFocus += dFocus;
                        }
                    }
                    break;

                case 2:
                    {
                        if (focusCnt == 0)
                        {
                            focusCnt = 100 + rand.Next(123);

                            if (currentFocus <= minDepth)
                            {
                                currentFocus = minDepth + 0.000001f;
                                targetFocus = maxDepth;
                                dFocus = +0.00025f;
                            }
                            else
                            {
                                currentFocus = maxDepth - 0.000001f;
                                targetFocus = minDepth;
                                dFocus = -0.00025f;
                            }
                        }

                        if (currentFocus < maxDepth && currentFocus > minDepth)
                        {
                            currentFocus += dFocus;
                        }
                        else
                        {
                            focusCnt--;
                        }
                    }
                    break;
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------
    }
};
