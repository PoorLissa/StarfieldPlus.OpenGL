using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;


/*
    - Stack of circle shapes moving with a delay
*/


namespace my
{
    public class myObj_0930 : myObject
    {
        // Priority
        public static int Priority => 10;
		public static System.Type Type => typeof(myObj_0930);

        private int cnt, shadowFactor;
        private float x1, y1, x2, y2, dx, dy;
        private bool xReady, yReady;
        private float size, A, R, G, B, angle = 0;
        private myObj_0930 parent = null;

        private static int N = 0, shape = 0, maxSize = 1, dSize = 1, delayCnt = 0, newDistMode = 0;
        private static int parentCntMax = 100, shaderFunc = 0, moveMode = 0;
        private static int shadowStayFactor = 0, shadowMoveFactor = 0, shadowPosX = 0, shadowPosY = 0;
        private static bool doFillShapes = false;
        private static float dimAlpha = 0.05f, spdFactor = 1;

        private static myScreenGradient grad = null;
        private static myFreeShader shader = null;
        private static myFreeShader shaderShadow = null;
        private static myTexRectangle tex = null;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_0930()
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
                N = rand.Next(10) + 10;
                N = 5 + rand.Next(50);
            }

            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time local initialization
        private void initLocal()
        {
            doClearBuffer = myUtils.randomBool(rand);
            doFillShapes = true;

            moveMode = rand.Next(2);

            maxSize = 250 + rand.Next(150);
            maxSize = 50;

            dSize = myUtils.randomChance(rand, 1, 3)
                ? rand.Next(20) + 1
                : rand.Next(33) + 20;

            shadowPosX = rand.Next(15) - 7;
            shadowPosY = rand.Next(15) - 7;
            shadowStayFactor = 2;
            shadowMoveFactor  = 7;

            if (N > 50)
                maxSize = 50;
            else if (N > 40)
                maxSize = 60;
            else if (N > 30)
                maxSize = 70;
            else if (N > 20)
                maxSize = 80;
            else if (N > 10)
                maxSize = 90;
            else
                maxSize = 150 + rand.Next(250);

            spdFactor = (rand.Next(5) + 10) + myUtils.randFloat(rand);
            delayCnt = rand.Next(11) + 3;
            newDistMode = rand.Next(4);

            renderDelay = rand.Next(3);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            height = 600;

            string nStr(int   n) { return n.ToString("N0");    }
            string fStr(float f) { return f.ToString("0.000"); }

            string str = $"Obj = {Type}\n\n"                         +
                            $"N = {nStr(list.Count)} of {nStr(N)}\n" +
                            $"moveMode = {moveMode}\n"               +
                            $"maxSize = {maxSize}\n"                 +
                            $"dSize = {dSize}\n"                     +
                            $"spdFactor = {fStr(spdFactor)}\n"       +
                            $"delayCnt = {delayCnt}\n"               +
                            $"shaderFunc = {shaderFunc}\n"           +
                            $"newDistMode = {newDistMode}\n"         +
                            $"renderDelay = {renderDelay}\n"         +
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
            x2 = y2 = dx = dy = 0;
            xReady = yReady = true;

            if (id == 0)
            {
                if (myUtils.randomChance(rand, 1, 3))
                {
                    R = 0.33f;
                    G = 0.33f;
                    B = 0.33f;
                }
                else do
                {
                    R = myUtils.randFloat(rand);
                    G = myUtils.randFloat(rand);
                    B = myUtils.randFloat(rand);
                }
                while (R + G + B < 1.0f || R + G + B > 2.0f);

                x1 = rand.Next(gl_Width);
                y1 = rand.Next(gl_Height);

                size = maxSize;

                cnt = parentCntMax;
            }
            else
            {
                parent = list[(int)id - 1] as myObj_0930;

                size = parent.size + dSize;
                x1 = parent.x1;
                y1 = parent.y1;

                cnt = parent.cnt + delayCnt;

                R = parent.R + 0.035f;
                G = parent.G + 0.035f;
                B = parent.B + 0.035f;
            }

            // Staying in place
            shadowFactor = shadowStayFactor;

            A = 1.0f;

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            if (xReady && yReady)
            {
                // Lowered
                shadowFactor = shadowStayFactor;

                if (--cnt == 0)
                {
                    cnt = parentCntMax;
                    xReady = yReady = false;

                    // Raised
                    shadowFactor = shadowMoveFactor;

                    if (id == 0)
                    {
                        getNewDestination();       // Get x2 and y2
                        getNewDxDy();
                    }
                    else
                    {
                        switch (moveMode)
                        {
                            case 0:
                                x2 = parent.x2;
                                y2 = parent.y2;
                                dx = parent.dx;
                                dy = parent.dy;
                                break;

                            case 1:
                                x2 = parent.x2;
                                y2 = parent.y2;
                                getNewDxDy();
                                break;
                        }
                    }
                }
            }
            else
            {
                if (!xReady)
                {
                    x1 += dx;

                    if (Math.Abs(x1 - x2) <= Math.Abs(dx))
                    {
                        x1 = x2;
                        xReady = true;
                    }
                }

                if (!yReady)
                {
                    y1 += dy;

                    if (Math.Abs(y1 - y2) <= Math.Abs(dy))
                    {
                        y1 = y2;
                        yReady = true;
                    }
                }
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Show()
        {
            float shadowSize = size + shadowFactor;
            shaderShadow.SetColorA(shadowFactor == shadowStayFactor ? 0.75f : 0.15f);
            shaderShadow.Draw(x1 + shadowPosX, y1 + shadowPosY, shadowSize, shadowSize, 20);

            shader.SetColor(R, G, B, A);
            shader.Draw(x1, y1, size, size, 10);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Process(Window window)
        {
            uint cnt = 0;
            initShapes();

            clearScreenSetup(doClearBuffer, 0.1f);

            while (!Glfw.WindowShouldClose(window))
            {
                int Count = list.Count;

                processInput(window);

                // Swap fore/back framebuffers, and poll for operating system events.
                Glfw.SwapBuffers(window);
                Glfw.PollEvents();

                // Dim screen
                {
                    if (tex == null)
                    {
                        grad.Draw();
                    }
                    else
                    {
                        tex.Draw(0, 0, gl_Width, gl_Height);
                    }
                }

                // Render Frame
                {
                    inst.ResetBuffer();

                    for (int i = Count-1; i >= 0; i--)
                    {
                        var obj = list[i] as myObj_0930;

                        obj.Show();
                        obj.Move();
                    }

                    if (doFillShapes)
                    {
                        // Tell the fragment shader to multiply existing instance opacity by 0.5:
                        inst.SetColorA(-0.5f);
                        inst.Draw(true);
                    }

                    // Tell the fragment shader to do nothing with the existing instance opacity:
                    inst.SetColorA(0);
                    inst.Draw(false);
                }

                if (Count < N)
                {
                    list.Add(new myObj_0930());
                }

                cnt++;
                System.Threading.Thread.Sleep(renderDelay);
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void initShapes()
        {
            base.initShapes(shape, N, 0);

            grad = new myScreenGradient();
            grad.SetRandomColors(rand, 0.2f);

            string main = "", header = "";

            getShader_000(ref rand, ref header, ref main);
            shader = new myFreeShader(header, main);

            getShader_001(ref rand, ref header, ref main);
            shaderShadow = new myFreeShader(header, main);

            if (colorPicker.getMode() == (int)myColorPicker.colorMode.IMAGE || colorPicker.getMode() == (int)myColorPicker.colorMode.SNAPSHOT)
            {
                tex = new myTexRectangle(colorPicker.getImg());
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void getNewDxDy()
        {
            dx = x2 - x1;
            dy = y2 - y1;

            double distInv = 1.0 / Math.Sqrt(dx * dx + dy * dy);

            dx = (float)(spdFactor * dx * distInv);
            dy = (float)(spdFactor * dy * distInv);
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void getNewDestination()
        {
            int dist = 0, d = 0;

            switch (newDistMode)
            {
                case 0:
                    dist = gl_Width * 2;
                    break;

                case 1:
                    dist = 333;
                    break;

                case 2:
                    dist = 666;
                    break;

                case 3:
                    dist = 999;
                    break;
            }

            do
            {
                x2 = rand.Next(gl_Width);
                y2 = rand.Next(gl_Height);

                float dx = (x2 - x1);
                float dy = (y2 - y1);

                d = (int)Math.Sqrt(dx*dx + dy*dy);
            }
            while (d > dist);
        }

        // ---------------------------------------------------------------------------------------------------------------

        // Main circle
        private static void getShader_000(ref Random rand, ref string h, ref string m)
        {
            string myCircleFunc = "";

            shaderFunc = rand.Next(4);

            switch (shaderFunc)
            {
                case 0:
                    {
                        myCircleFunc = $@"
                            float circ = 1.0 - smoothstep(0.0, 0.005, abs(rad-len));
                            result = vec4(color * circ, myColor.w * circ);
                        ";
                    }
                    break;

                case 1:
                    {
                        float softness = myUtils.randomChance(rand, 1, 3)
                            ? 0.001f
                            : 0.001f + myUtils.randFloat(rand) * 0.002f;

                        myCircleFunc = $@"
                            float circ = smoothstep(rad+{softness}, rad-{softness}, len);
                            result = vec4(color * circ, myColor.w * circ);
                        ";
                    }
                    break;

                case 2:
                    {
                        myCircleFunc = @"
                            if (len < rad)
                            {
                                float circ = smoothstep(rad+0.0075, rad-0.01, len);
                                result = vec4(color * circ, myColor.w);
                                //result = vec4(color * circ, circ);
                            }
                            else
                            {
                                //float circ = 1 - smoothstep(rad-0.003, rad + 0.005, len);
                                //result = vec4(0, 0, 0, circ/1.1);
                            }
                        ";
                    }
                    break;

                case 3:
                    {
                        myCircleFunc = $@"
                            float c1 = 0 + smoothstep(rad + 0.0075, rad - 0.009, len);
                            float c2 = 1 - smoothstep(rad - 0.0001, rad + 0.005, len);
                            result = vec4(color * c1, c1 + c2);
                        ";
                    }
                    break;
            }

            h = $@"";

            m = $@"vec2 uv = (gl_FragCoord.xy / iResolution.xy * 2.0 - 1.0);

                    uv -= Pos.xy; uv *= aspect;

                    float rad = Pos.z;
                    float len = length(uv);
                    vec3 color = myColor.xyz;

                    {myCircleFunc}
                ";
        }

        // ---------------------------------------------------------------------------------------------------------------

        // Shadow
        private static void getShader_001(ref Random rand, ref string h, ref string m)
        {
            m = @"vec2 uv = (gl_FragCoord.xy / iResolution.xy * 2.0 - 1.0);

                    uv -= Pos.xy; uv *= aspect;

                    float rad = Pos.z;
                    float len = length(uv);
                    vec3 color = myColor.xyz;

                    float r = 0.001;
                    float a = smoothstep(rad + 0.001, rad - 0.005, len);

                    result = vec4(0.1, 0.1, 0.1, a * myColor.w);
                ";
        }

        // ---------------------------------------------------------------------------------------------------------------
    }
};
