using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;
using System.Collections;


/*
    - Depth focus test
*/


namespace my
{
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


    public class myObj_1420 : myObject
    {
        // Priority
        public static int Priority => 10;
		public static System.Type Type => typeof(myObj_1420);

        private float x, y, dx, dy;
        private float size, A, R, G, B, depth = 0;

        private static int N = 0;
        private static float dimAlpha = 0.05f, focusDist = 0, t = 0, dt = 0;

        private static List<myObj_1420> sortedList = null;
        private static myScreenGradient grad = null;
        private static myFreeShader_1420 shader = null;

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

            t = 0;
            dt = 0.0001f;

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            height = 600;

            string str = $"Obj = {Type}\n\n"                           +
                            myUtils.strCountOf(sortedList.Count, N)    +
                            $"focusDist = {myUtils.fStr(focusDist)}\n" +
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
            dx = myUtils.randFloatSigned(rand) * 0.33f;
            dy = myUtils.randFloat(rand, 0.1f) * 2;

            depth = myUtils.randFloat(rand) * 0.03f;

            size = myUtils.randomChance(rand, 1, 3)
                ? rand.Next(66) + 5
                : rand.Next(11) + 3;

            x = rand.Next(gl_Width);
            y = -(33 + size);

            A = 0.25f + myUtils.randFloat(rand) * 0.175f;
            colorPicker.getColor(x, y, ref R, ref G, ref B);

/*
            R = (float)rand.NextDouble();
            G = 0.1f;
            B = 0.1f;
*/
            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            x += dx;
            y += dy;

            if (x < 0 || x > gl_Width || y > gl_Height + size)
            {
                generateNew();
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Show()
        {
            float focusDepth = (float)Math.Abs(depth - focusDist);

            if (focusDepth < 0.001f)
                focusDepth = 0.001f;

            shader.SetColor(R, G, B, A);
            shader.Draw(x, y, size, size, focusDepth, 5);
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

                if (Count < N)
                {
                    sortedList.Add(new myObj_1420());
                }

                stopwatch.WaitAndRestart();
                t += dt;
                cnt++;

                // dist = [0 .. 0.03]
                // focusDist = [-0.01 .. 0.04]
                focusDist = (float)(Math.Abs(Math.Sin(t*10) * 0.05f)) - 0.01f;
                //focusDist = 0.03f;
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
        }

        // ---------------------------------------------------------------------------------------------------------------

        // Circular smooth spot
        private void getShader_000(ref string h, ref string m)
        {
            h = $@"
                uniform float myDepth;
                float circle(vec2 uv, float rad)
                {{
                    float len = rad - length(uv);
                    if (len > 0)
                        return smoothstep(0.0, myDepth, len);
                    return 0;
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
    }
};
