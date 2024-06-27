using GLFW;
using static OpenGL.GL;
using System.Collections.Generic;


/*
    - Concentric vibrating circles around randomly moving central point
*/


namespace my
{
    public class myObj_0410 : myObject
    {
        // Priority
        public static int Priority => 10;
		public static System.Type Type => typeof(myObj_0410);

        int circCount;
        private float size, dSize, A, R, G, B;

        private static int N = 0, lineTh = 9;
        private static float dimAlpha = 0.05f;

        // The Center
        static float centerX = 0, centerY = 0, centerDx = 0, centerDy = 0;
        static int centerOffX = 0, centerOffY = 0;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_0410()
        {
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
                N = rand.Next(100) + 75;
            }

            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time local initialization
        private void initLocal()
        {
            doClearBuffer = myUtils.randomChance(rand, 1, 13);
            dimAlpha = 0.005f + myUtils.randFloat(rand) * 0.01f;

            centerX = gl_x0;
            centerY = gl_y0;

            centerDx = myUtils.randFloat(rand, 0.2f) * 3 * myUtils.randomSign(rand);
            centerDy = myUtils.randFloat(rand, 0.2f) * 3 * myUtils.randomSign(rand);

            centerOffX = rand.Next(1111) + 111;
            centerOffY = rand.Next(1111) + 111;

            renderDelay = rand.Next(11) + 1;

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            height = 600;

            string str = $"Obj = {Type}\n\n"                         +
                            myUtils.strCountOf(list.Count, N)        +
                            $"doClearBuffer = {doClearBuffer}\n"     +
                            $"lineTh = {lineTh}\n"                   +
                            $"dimAlpha = {myUtils.fStr(dimAlpha)}\n" +
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
            size = rand.Next(1234) + 1234;
            dSize = rand.Next(33) + 5;
            circCount = rand.Next(250) + 250;

            colorPicker.getColorRand(ref R, ref G, ref B);
            A = myUtils.randFloat(rand) * 0.123f;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            // Move the Center
            if (id == 0)
            {
                centerX += centerDx;
                centerY += centerDy;

                if (centerX > gl_x0 + centerOffX)
                    centerDx -= 0.25f;

                if (centerX < gl_x0 - centerOffX)
                    centerDx += 0.25f;

                if (centerY > gl_y0 + centerOffY)
                    centerDy -= 0.25f;

                if (centerY < gl_y0 - centerOffY)
                    centerDy += 0.25f;
            }

            if (size <= 0 || circCount <= 0)
            {
                generateNew();
            }
            else
            {
                size -= dSize;
                circCount--;

                if (myUtils.randomChance(rand, 1, 3))
                {
                    dSize *= 0.95f;
                    A *= 1.005f;
                }
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Show()
        {
            float size2x = size * 2;

            int x = (int)centerX + rand.Next(11) - 5;
            int y = (int)centerY + rand.Next(11) - 5;

            myPrimitive._Ellipse.SetColor(R, G, B, A);
            myPrimitive._Ellipse.Draw(x - size, y - size, size2x, size2x);
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Process(Window window)
        {
            uint cnt = 0;
            initShapes();


            if (doClearBuffer)
            {
                glDrawBuffer(GL_FRONT_AND_BACK | GL_DEPTH_BUFFER_BIT);
                glClearColor(0, 0, 0, 1);
            }
            else
            {
                dimScreenRGB_SetRandom(0.1f);
                glDrawBuffer(GL_BACK);
            }


            myPrimitive._Ellipse.setLineThickness(lineTh);


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
                    else
                    {
                        dimScreen(dimAlpha);
                    }
                }

                // Render Frame
                {
                    for (int i = 0; i != Count; i++)
                    {
                        var obj = list[i] as myObj_0410;

                        obj.Show();
                        obj.Move();
                    }
                }

                if (Count < N)
                {
                    list.Add(new myObj_0410());
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
            myPrimitive.init_Ellipse();

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------
    }
};
