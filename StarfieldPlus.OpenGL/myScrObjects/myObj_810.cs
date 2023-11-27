using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;


/*
    - Raster scan of an image
*/


namespace my
{
    public class myObj_810 : myObject
    {
        // Priority
        public static int Priority => 9999910;
		public static System.Type Type => typeof(myObj_810);

        private float x, y;
        private float size, A, R, G, B, dR, dG, dB;

        private static uint cnt = 0;
        private static int N = 0, Y = 0, step = 10;
        private static int yDir = 1;
        private static float maxOpacity = 0, r = 0, g = 0, b = 0;

        private static myScreenGradient grad = null;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_810()
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
                N = gl_Width;
            }

            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time local initialization
        private void initLocal()
        {
            doClearBuffer = myUtils.randomBool(rand);
            doClearBuffer = false;

            maxOpacity = 0.05f + myUtils.randFloat(rand) * 0.25f;

            step = 5 + rand.Next(16);
            renderDelay = 20 - step;

            renderDelay += 1;

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
                            $"step = {step }\n"                      +
                            $"maxOpacity = {fStr(maxOpacity)}\n"     +
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
            x = id;
            y = gl_y0;
            size = 1;

            A = maxOpacity;
            R = G = B = 0;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            if (cnt == 0)
            {
                colorPicker.getColor(x, Y, ref r, ref g, ref b);

                dR = (r - R) / step;
                dG = (g - G) / step;
                dB = (b - B) / step;
            }

            R += dR;
            G += dG;
            B += dB;

            size = 5 + (R + G + B) * 333;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Show()
        {
            myPrimitive._LineInst.setInstanceCoords(x, y + size, x, y - size);
            myPrimitive._LineInst.setInstanceColor(R, G, B, A);
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Process(Window window)
        {
            initShapes();


            clearScreenSetup(doClearBuffer, 0.1f);

            while (list.Count < N)
            {
                list.Add(new myObj_810());
            }

            if (doClearBuffer == false)
            {
                grad.SetOpacity(0.1f);
            }


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
                    }

                    grad.Draw();
                }

                // Render Frame
                {
                    myPrimitive._LineInst.ResetBuffer();

                    for (int i = 0; i != Count; i++)
                    {
                        var obj = list[i] as myObj_810;

                        obj.Show();
                        obj.Move();
                    }

                    myPrimitive._LineInst.Draw();
                }

                System.Threading.Thread.Sleep(renderDelay);

                if (++cnt == step)
                {
                    cnt = 0;
                    Y += yDir;

                    if (Y == gl_Height && yDir > 0)
                    {
                        yDir *= -1;
                    }

                    if (Y == 0 && yDir < 0)
                    {
                        yDir *= -1;
                    }
                }
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void initShapes()
        {
            myPrimitive.init_LineInst(N);

            grad = new myScreenGradient();
            grad.SetRandomColors(rand, 0.2f, 0);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------
    }
};
