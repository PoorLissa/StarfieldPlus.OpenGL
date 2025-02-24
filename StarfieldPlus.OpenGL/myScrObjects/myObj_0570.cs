using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;


/*
    - Some spots using the color of an image. Linear connections between these shapes
*/


namespace my
{
    public class myObj_0570 : myObject
    {
        // Priority
        public static int Priority => 10;
        public static System.Type Type => typeof(myObj_0570);

        private int cnt;
        private float x, y;
        private float size, A, R, G, B;

        private List<myObj_0570> connections = null;

        private static int N = 0;
        private static float dimAlpha = 0.05f;

        private static myFreeShader shader = null;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_0570()
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
                N = rand.Next(111) + 111;
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

            string str = $"Obj = {Type}\n\n"                      	 +
                            $"N = {nStr(list.Count)} of {nStr(N)}\n" +
                            $"doClearBuffer = {doClearBuffer}\n"     +
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
            cnt = rand.Next(666) + 111;
            size = rand.Next(11) + 3;

            x = rand.Next(gl_Width  + 400) - 200;
            y = rand.Next(gl_Height + 400) - 200;

            A = myUtils.randFloat(rand, 0.1f);

            colorPicker.getColor(x, y, ref R, ref G, ref B);

            if (connections == null && list.Count == N)
            {
                connections = new List<myObj_0570>();

                int n = rand.Next(3) + 1;

                for (int i = 0; i < n; i++)
                {
                    int pos = rand.Next(list.Count);

                    connections.Add(list[pos] as myObj_0570);
                }
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            if (--cnt <= 0)
            {
                if (A <= 0)
                {
                    generateNew();
                }

                A -= 0.0025f;
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Show()
        {
            if (connections != null)
            {
                for (int i = 0; i < connections.Count; i++)
                {
                    float r = (R + connections[i].R) / 2;
                    float g = (G + connections[i].G) / 2;
                    float b = (B + connections[i].B) / 2;

                    myPrimitive._Line.SetColor(r, g, b, doClearBuffer ? 0.05f : 0.01f);
                    myPrimitive._Line.Draw(x, y, connections[i].x, connections[i].y);
                }
            }

            shader.SetColor(R, G, B, A);

#if false
            int SIZE = 50;

            int xx = rand.Next(SIZE) - SIZE/2;
            int yy = rand.Next(SIZE) - SIZE/2;

            shader.Draw(x + xx, y + yy, size, size, 10);
#else
            shader.Draw(x, y, size, size, 10);
#endif
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Process(Window window)
        {
            uint cnt = 0;
            initShapes();

            clearScreenSetup(doClearBuffer, 0.15f);

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
                    for (int i = 0; i != list.Count; i++)
                    {
                        var obj = list[i] as myObj_0570;

                        obj.Show();
                        obj.Move();
                    }
                }

                if (list.Count < N)
                {
                    list.Add(new myObj_0570());
                }

                cnt++;
                System.Threading.Thread.Sleep(renderDelay);
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void initShapes()
        {
            myPrimitive.init_Line();
            myPrimitive.init_ScrDimmer();

            getShader();

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void getShader()
        {
            shader = new myFreeShader($@"
                        float circle(vec2 uv, float rad) {{ return smoothstep(rad, rad - 0.005, length(uv)); }}
                    ",

                    $@"
                        vec2 uv = (gl_FragCoord.xy / iResolution.xy * 2.0 - 1.0);

                        uv -= Pos.xy;
                        uv *= aspect;

                        result = vec4(myColor * (circle(uv, Pos.z) + 0.75 * circle(uv, Pos.z * 0.75)));
                    "
            );
        }

        // ---------------------------------------------------------------------------------------------------------------

    }
};
