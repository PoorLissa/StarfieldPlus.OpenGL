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

        private static int N = 0, shape = 0, colorMode = 0;
        private static bool doFillShapes = false;
        private static float dimAlpha = 0.05f, t = 0, dt = 0.001f;
        private static float rFactor, gFactor, bFactor, R, G, B, rCustom, gCustom, bCustom;

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

                colorMode = rand.Next(2);
                shape = rand.Next(5);

                do {

                    rCustom = myUtils.randFloat(rand);
                    gCustom = myUtils.randFloat(rand);
                    bCustom = myUtils.randFloat(rand);

                } while (rCustom + gCustom + bCustom < 1);

                rFactor = (myUtils.randFloat(rand) + 0.1f) + rand.Next(5);
                gFactor = (myUtils.randFloat(rand) + 0.1f) + rand.Next(5);
                bFactor = (myUtils.randFloat(rand) + 0.1f) + rand.Next(5);
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
                            $"colorMode = {colorMode}\n"         +
                            $"rFactor = {rFactor}\n"             +
                            $"gFactor = {gFactor}\n"             +
                            $"bFactor = {bFactor}\n"             +
                            $"rCustom = {rCustom}\n"             +
                            $"gCustom = {gCustom}\n"             +
                            $"bCustom = {bCustom}\n"             +
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
            R = G = B = 0;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            t += dt;

            // Recalc target color
            R = (float)Math.Abs(Math.Sin(rFactor * t));
            G = (float)Math.Abs(Math.Sin(gFactor * t));
            B = (float)Math.Abs(Math.Sin(bFactor * t));

            // Send target color into shader
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
            string result = "";

            double A = doClearBuffer ? 0.05 : 0.01;

            switch (colorMode)
            {
                // Draw everything that we see in a normal way
                case 0:
                    result = "";
                    break;

                // Draw everything that we see in a special color
                case 1:
                    result = $"result = vec4({rCustom}, {gCustom}, {bCustom}, 1);";
                    A = 0.001;
                    break;
            }

            // Compare target color with current pixel color;
            // Draw the pixel only if it is close to the target color
            string fragMain = $@"

                float target = {0.25f + myUtils.randFloat(rand) * 0.3f};
                result = texture(myTexture, fragTxCoord);

                if (distance(result, fragColor) < target)
                {{
                    {result}
                }}
                else
                {{
                    result *= vec4(1, 1, 1, {A});
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
