using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;


/*
    - Snake-like patterns, stupid implementation
*/


namespace my
{
    public class myObj_0610 : myObject
    {
        // Priority
        public static int Priority => 10;
        public static System.Type Type => typeof(myObj_0610);

        private float x, y, dx, dy;
        private float size, A, R, G, B;

        private static int N = 0;
        private static float dimAlpha = 0.05f;

        private static myFreeShader shader = null;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_0610()
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
                N = rand.Next(1111) + rand.Next(1111) + rand.Next(1111) + 111;
            }

            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time local initialization
        private void initLocal()
        {
            doClearBuffer = false;
            dimAlpha = 0.01f;

            renderDelay = rand.Next(11) + 3;

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            height = 600;

            string nStr(int   n) { return n.ToString("N0");    }
            //string fStr(float f) { return f.ToString("0.000"); }

            string str = $"Obj = {Type}\n\n"                      	 +
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

            dx = myUtils.randFloat(rand) * (rand.Next(5) + 3) * myUtils.randomSign(rand);
            dy = myUtils.randFloat(rand) * (rand.Next(5) + 3) * myUtils.randomSign(rand);

            size = rand.Next(13) + 3;

            A = 0.85f;
            A = myUtils.randFloat(rand, 0.1f);
            colorPicker.getColor(x, y, ref R, ref G, ref B);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            x += dx;
            y += dy;

            // Adjust R / G / B
            {
                if (myUtils.randomChance(rand, 1, 5))
                    updColor(ref R);

                if (myUtils.randomChance(rand, 1, 5))
                    updColor(ref G);

                if (myUtils.randomChance(rand, 1, 5))
                    updColor(ref B);
            }

            // Adjust dx / dy
            {
                if (myUtils.randomChance(rand, 1, 3))
                    dx += myUtils.randFloat(rand) * 0.5f * myUtils.randomSign(rand);

                if (myUtils.randomChance(rand, 1, 3))
                    dy += myUtils.randFloat(rand) * 0.5f * myUtils.randomSign(rand);
            }

            float bounceFactor = 0.05f;

            if (x < 0)
                dx += bounceFactor;

            if (x > gl_Width)
                dx -= bounceFactor;

            if (y < 0)
                dy += bounceFactor;

            if (y > gl_Height)
                dy -= bounceFactor;

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Show()
        {
            shader.SetColor(R, G, B, A);
            shader.Draw(x, y, size, size, 13);
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Process(Window window)
        {
            uint cnt = 0;
            initShapes();

            clearScreenSetup(doClearBuffer, 0.1f, front_and_back: true);

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
                        var obj = list[i] as myObj_0610;

                        obj.Show();
                        obj.Move();
                    }
                }

                if (list.Count < N)
                {
                    list.Add(new myObj_0610());
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
            //base.initShapes(shape, N, 0);

            getShader();

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void getShader()
        {
            string header = "";
            string main = "";

            my.myShaderHelpers.Shapes.getShader_000(ref rand, ref header, ref main);
            shader = new myFreeShader(header, main);
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void updColor(ref float Color)
        {
            Color += myUtils.randFloat(rand) * 0.1f * myUtils.randomSign(rand);

            if (Color < 0)
                Color = 0;

            if (Color > 1)
                Color = 1;
        }
    }
};
