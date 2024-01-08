using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;


/*
    - 
*/


namespace my
{
    public class myObj_860 : myObject
    {
        // Priority
        public static int Priority => 10;
		public static System.Type Type => typeof(myObj_860);

        private float x, y, dx, dy;
        private float size, A, R, G, B, angle = 0;

        private static int N = 0, shape = 0;
        private static bool doFillShapes = false;
        private static float dimAlpha = 0.05f;

        private static myScreenGradient grad = null;

        private myFreeShader_FullScreen shaderFull = null;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_860()
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

                shape = rand.Next(5);
            }

            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time local initialization
        private void initLocal()
        {
            doClearBuffer = myUtils.randomBool(rand);

            renderDelay = rand.Next(11) + 3;

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            height = 600;

            string nStr(int   n) { return n.ToString("N0");    }
            //string fStr(float f) { return f.ToString("0.000"); }

            string str = $"Obj = {Type}\n\n"                         +
                            $"N = {nStr(list.Count)} of {nStr(N)}\n" +
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
            x = rand.Next(gl_Width);
            y = rand.Next(gl_Height);

            dx = myUtils.randFloat(rand);
            dy = myUtils.randFloat(rand);

            size = rand.Next(11) + 3;

            A = 1;
            R = (float)rand.NextDouble();
            G = (float)rand.NextDouble();
            B = (float)rand.NextDouble();

            colorPicker.getColor(x, y, ref R, ref G, ref B);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            x += dx;
            y += dy;

            if (x < 0 || x > gl_Width || y < 0 || y > gl_Height)
            {
                generateNew();
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Show()
        {
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
                if (false)
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
                    shaderFull.Draw();

                    for (int i = 0; i != Count; i++)
                    {
                        var obj = list[i] as myObj_860;

                        obj.Show();
                        obj.Move();
                    }
                }

                if (Count < N)
                {
                    list.Add(new myObj_860());
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
            base.initShapes(shape, N, 0);

            grad = new myScreenGradient();
            grad.SetRandomColors(rand, 0.2f);

            string fHeader = null, fMain = null;

            getShader(ref fHeader, ref fMain);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        // Select random mode and get shader code: header + main func
        private void getShader(ref string header, ref string main)
        {
            do {

                R = myUtils.randFloat(rand);
                G = myUtils.randFloat(rand);
                B = myUtils.randFloat(rand);
            }
            while (R + G + B < 0.5f);

            string stdHeader = $@"
                vec4 myColor = vec4({R}, {G}, {B}, 1.0);
                {myShaderHelpers.Generic.rotationMatrix}
                float t = uTime;
                vec2 uv = (gl_FragCoord.xy - 0.5 * iResolution.xy) / iResolution.y;";

            header = stdHeader;

            string getVal = "x * sin(y) + len";
            string mode = $@"{rand.Next(10)}";

            main = $@"
                uv *= 5;

                switch ({mode})
                {{
                    case 6:
                        uv *= 2;
                        break;

                    case 7:
                        uv *= 1.2;
                        break;

                    case 9:
                        uv *= 0.75;
                        break;

                    default:
                        uv *= rot(t * 0.01);
                }}

                float x = uv.x;
                float y = uv.y;

                float len = length(uv);
                float val = {getVal};
                float sint = sin(t);
                float cost = cos(t);

                bool isSmoothstep = true;

                switch ({mode})
                {{
                    case 0:
                        val = cos(5 * x  + y * t * sin(t));
                        break;

                    case 1:
                        val = cos(5 * x  * x + 10 * y + sin(x * y + t));
                        break;

                    case 2:
                        val = cos(5 * x + 10 * y + sin(x * y + t));
                        break;

                    case 3:
                        val  = cos(5 * x + 10 * y + sin(x * y + t));
                        val += sin(5 * y + 10 * x + cos(x * y + t));
                        break;

                    case 4:
                        val  = cos(5 * x + 10 * y + sin(x * y + t));
                        val += sin(5 * y - 10 * x - cos(x + y - t));
                        break;

                    case 5:
                        val  = cos(+5 * x + 10 * y + sin(x * y + t));
                        val += cos(-5 * y + 10 * x + sin(x * y + t));
                        val += smoothstep(0.0, 0.1, sin(t / 3)) * 0.1;
                        break;

                    case 6:
                        val = cos(3 * x + 11 * y + sin(x * y + t));
                        val *= 10.0 * sin(t * 0.1);
                        val += 3 * sin(x / 33 * sint);
                        val += 5 * sin(x * x + y * y);
                        break;

                    case 7:
                        val = cos(3 * y * y * y + sin(x * y + t));
                        val = smoothstep(-0.9, 0.9, val);
                        isSmoothstep = false;
                        break;

                    case 8:
                        // vary this: (x * y + t * 1) VS (x * y * t * 1)
                        val = cos(3 * x * x * x + 11 * y * y * y + sin(x * y + t * 1));

                        //val = cos(0 * x * x * x + 3 * y * y * y + sin(x * y + t * 1));

                        val = smoothstep(-0.9, 0.9, val);

                        // vary this: use OR not
                        //val *= cos(3 * x + 11 * y + sin(x * y + t));

                        isSmoothstep = false;
                        break;

                    case 9:
                        val = cos(3 * x * x * x + 11 * y * y * y + sin(x * y + t * 1));
                        val = sin(val * val * val) / cos(val * val);

                        isSmoothstep = false;
                        break;

                    case 111:
                        val = cos(3 * x + 11 * y + sin(x * y + t));
                        val += 3 * sin(x / 33 * sint);
                        break;
                }}

                // Optional, looks good with and without it
                if (isSmoothstep)
                    val = smoothstep(0.0, 0.2, val);

                //if (val > 0.9) val -= 0.75;

                result = vec4(myColor.xyz * val, val);
            ";

            shaderFull = new myFreeShader_FullScreen(fHeader: header, fMain: main);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------
    }
};
