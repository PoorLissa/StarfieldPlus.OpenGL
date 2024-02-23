using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;


/*
    - Stack moving with a delay
*/


namespace my
{
    public class myObj_930 : myObject
    {
        // Priority
        public static int Priority => 999910;
		public static System.Type Type => typeof(myObj_930);

        private int cnt;
        private float x1, y1, x2, y2, dx, dy;
        private float size, A, R, G, B, angle = 0;

        private static int N = 0, shape = 0, maxSize = 1, dSize = 1, delayCnt = 0, newDistMode = 0;
        private static bool doFillShapes = false;
        private static float dimAlpha = 0.05f, spdFactor = 1;

        private static myScreenGradient grad = null;
        private static myFreeShader shader = null;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_930()
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

                shape = rand.Next(5);
                shape = 2;
            }

            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time local initialization
        private void initLocal()
        {
            doClearBuffer = myUtils.randomBool(rand);
            doFillShapes = true;

            maxSize = 250 + rand.Next(150);
            maxSize = 50;
            dSize = 20 + rand.Next(33);

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
                            $"maxSize = {maxSize}\n"                 +
                            $"dSize = {dSize}\n"                     +
                            $"spdFactor = {fStr(spdFactor)}\n"       +
                            $"delayCnt = {delayCnt}\n"               +
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

                cnt = 100;
            }
            else
            {
                var parent = list[(int)id - 1] as myObj_930;

                size = parent.size + dSize;
                x1 = parent.x1;
                y1 = parent.y1;

                cnt = parent.cnt + delayCnt;

                R = parent.R + 0.035f;
                G = parent.G + 0.035f;
                B = parent.B + 0.035f;
            }

            A = 1.0f;

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            if (dx == 0 && dy == 0)
            {
                if (--cnt == 0)
                {
                    cnt = 100;

                    if (id == 0)
                    {
                        getNewDist();

                        dx = x2 - x1;
                        dy = y2 - y1;

                        double dist = Math.Sqrt(dx * dx + dy * dy);

                        dx = (float)(spdFactor * dx / dist);
                        dy = (float)(spdFactor * dy / dist);
                    }
                    else
                    {
                        var parent = list[(int)id - 1] as myObj_930;

                        x2 = parent.x2;
                        y2 = parent.y2;
                        dx = parent.dx;
                        dy = parent.dy;
                    }
                }
            }
            else
            {
                if (dx != 0)
                {
                    x1 += dx;

                    if (Math.Abs(x1 - x2) <= Math.Abs(dx))
                    {
                        x1 = x2;
                        dx = 0;
                    }
                }

                if (dy != 0)
                {
                    y1 += dy;

                    if (Math.Abs(y1 - y2) <= Math.Abs(dy))
                    {
                        y1 = y2;
                        dy = 0;
                    }
                }
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Show()
        {
            shader.SetColor(R, G, B, A);
            shader.Draw(x1, y1, size, size, 10);

            return;

            float size2x = size * 2;

            switch (shape)
            {
                // Instanced squares
                case 0:
                    myPrimitive._RectangleInst.setInstanceCoords(x1 - size, y1 - size, size2x, size2x);
                    myPrimitive._RectangleInst.setInstanceColor(R, G, B, A);
                    myPrimitive._RectangleInst.setInstanceAngle(angle);
                    break;

                // Instanced triangles
                case 1:
                    myPrimitive._TriangleInst.setInstanceCoords(x1, y1, size, angle);
                    myPrimitive._TriangleInst.setInstanceColor(R, G, B, A);
                    break;

                // Instanced circles
                case 2:
                    myPrimitive._EllipseInst.setInstanceCoords(x1, y1, size2x, angle);
                    myPrimitive._EllipseInst.setInstanceColor(R, G, B, A);
                    break;

                // Instanced pentagons
                case 3:
                    myPrimitive._PentagonInst.setInstanceCoords(x1, y1, size2x, angle);
                    myPrimitive._PentagonInst.setInstanceColor(R, G, B, A);
                    break;

                // Instanced hexagons
                case 4:
                    myPrimitive._HexagonInst.setInstanceCoords(x1, y1, size2x, angle);
                    myPrimitive._HexagonInst.setInstanceColor(R, G, B, A);
                    break;
            }

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
                    grad.Draw();
                }

                // Render Frame
                {
                    inst.ResetBuffer();

                    for (int i = Count-1; i >= 0; i--)
                    {
                        var obj = list[i] as myObj_930;

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
                    list.Add(new myObj_930());
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

            string header = "";
            string main = "";

            //my.myShaderHelpers.Shapes.getShader_000(ref rand, ref header, ref main);
            getShader_000(ref rand, ref header, ref main);
            shader = new myFreeShader(header, main);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void getNewDist()
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

        private static void getShader_000(ref Random rand, ref string h, ref string m)
        {
            string myCircleFunc = "";

            switch (rand.Next(1111111))
            {
                case 0:
                    myCircleFunc = "return 1.0 - smoothstep(0.0, 0.005, abs(rad-len));";
                    break;

                case 1:
                    myCircleFunc = "if (rad > len) return 1.0 - smoothstep(0.0, 0.01, rad-len); else return 1.0 - smoothstep(0.0, 0.005, len-rad);";
                    break;

                default:
                    myCircleFunc = "return smoothstep(rad, rad - 0.005, len);";
                    break;
            }

            h = $@"float circle(float len, float rad) {{ {myCircleFunc} }};";

            m = @"vec2 uv = (gl_FragCoord.xy / iResolution.xy * 2.0 - 1.0);

                    uv -= Pos.xy; uv *= aspect;

                    float rad = Pos.z;
                    float len = length(uv);
                    vec3 color = myColor.xyz;

                    if (len < rad)
                    {{
                        float circ = smoothstep(rad+0.0075, rad-0.01, len);
                        //result = vec4(color * circ, myColor.w);
                        result = vec4(color * circ, circ);
                    }}
                    else
                    {{
                        float circ = 1 - smoothstep(rad-0.003, rad + 0.005, len);
                        result = vec4(0, 0, 0, circ/1.1);
                    }}
                ";

/*
                    float c1 = 0 + smoothstep(rad + 0.0075, rad - 0.009, len);
                    float c2 = 1 - smoothstep(rad - 0.0001, rad + 0.005, len);
                    result = vec4(color * c1, c1 + c2);
                    return;
*/
        }

        // ---------------------------------------------------------------------------------------------------------------
    }
};
