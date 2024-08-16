using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;
using static System.Net.Mime.MediaTypeNames;


/*
    - Draw texture's pixels only if their color is close to a target color
*/


namespace my
{
    public class myObj_1060 : myObject
    {
        // Priority
        public static int Priority => 999910;
		public static System.Type Type => typeof(myObj_1060);

        private float x, y;
        private float A, R, G, B;

        private static int N = 0, shape = 0;
        private static bool doFillShapes = false;
        private static float dimAlpha = 0.05f, t = 0, dt = 0.001f, r, g, b;

        private static myTexRectangle tex = null;
        private static myScreenGradient grad = null;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_1060()
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
                N = 1;

                shape = rand.Next(5);

                r = (myUtils.randFloat(rand) + 0.1f) + rand.Next(5);
                g = (myUtils.randFloat(rand) + 0.1f) + rand.Next(5);
                b = (myUtils.randFloat(rand) + 0.1f) + rand.Next(5);
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

            string str = $"Obj = {Type}\n\n"                     +
                            myUtils.strCountOf(list.Count, N)    +
                            $"r = {r}\n"                         +
                            $"g = {g}\n"                         +
                            $"b = {b}\n"                         +
                            $"renderDelay = {renderDelay}\n"     +
                            $"doClearBuffer = {doClearBuffer}\n" +
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

            A = 1;
            R = G = B = 0;

            colorPicker.getColor(x, y, ref R, ref G, ref B);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            t += dt;

            R = (float)Math.Abs(Math.Sin(r * t));
            G = (float)Math.Abs(Math.Sin(g * t));
            B = (float)Math.Abs(Math.Sin(b * t));

            tex.setColor(R, G, B);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Show()
        {
            tex.Draw(0, 0, gl_Width, gl_Height);
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
                    inst.ResetBuffer();

                    for (int i = 0; i != Count; i++)
                    {
                        var obj = list[i] as myObj_1060;

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
                    list.Add(new myObj_1060());
                }

                cnt++;
                System.Threading.Thread.Sleep(renderDelay);
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void initShapes()
        {
            string fragMain = $@"

                float target = {0.25f + myUtils.randFloat(rand) * 0.3f};
                result = texture(myTexture, fragTxCoord);

                if (distance(result, fragColor) < target)
                {{
                    result = texture(myTexture, fragTxCoord);
                }}
                else
                {{
                    result = texture(myTexture, fragTxCoord) * vec4(0.25, 0.25, 0.25, 0.1);
                }}
            ";

            myPrimitive.init_ScrDimmer();
            base.initShapes(shape, N, 0);

            grad = new myScreenGradient();
            grad.SetRandomColors(rand, 0.2f);

            tex = new myTexRectangle(colorPicker.getImg(), fragMain: fragMain);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------
    }
};
