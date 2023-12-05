using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;


/*
    - Drop a random point, get its underlying color, then draw a horizontal or vertical line through this point
*/


namespace my
{
    public class myObj_720 : myObject
    {
        // Priority
        public static int Priority => 10;
		public static System.Type Type => typeof(myObj_720);

        private int cnt, dir;
        private float x, y;
        private float size, A, R, G, B;

        private static int N = 0, sizeMode = 0, maxSize = 0;
        private static bool doFillShapes = false;
        private static float dimAlpha = 0.05f;

        private static myScreenGradient grad = null;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_720()
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
                N = rand.Next(25) + 1;

                if (myUtils.randomChance(rand, 1, 13))
                {
                    N = 1111 + rand.Next(666);
                }
            }

            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time local initialization
        private void initLocal()
        {
            doClearBuffer = false;
            doFillShapes = myUtils.randomChance(rand, 1, 2);

            sizeMode = rand.Next(2);
            maxSize = 1 + rand.Next(3);

            renderDelay = rand.Next(11) + 3;

            dimAlpha = 0.05f + myUtils.randFloat(rand) * 0.50f;
            dimAlpha = 0.10f + myUtils.randFloat(rand) * 0.15f;

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
                            $"doClearBuffer = {doClearBuffer}\n"     +
                            $"dimAlpha = {fStr(dimAlpha)}\n"         +
                            $"maxSize = {maxSize}\n"                 +
                            $"sizeMode = {sizeMode}\n"               +
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

            switch (sizeMode)
            {
                case 0:
                    size = maxSize;
                    break;

                case 1:
                    size = 1 + rand.Next(maxSize);
                    break;
            }

            A = myUtils.randFloat(rand) * 0.33f;
            colorPicker.getColor(x, y, ref R, ref G, ref B);

            cnt = 333 + rand.Next(666);
            dir = rand.Next(2);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            if (--cnt == 0)
                generateNew();
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Show()
        {
            myPrimitive._Rectangle.SetColor(R, G, B, A);
            myPrimitive._Rectangle.SetAngle(0);

            if (dir == 0)
            {
                myPrimitive._Rectangle.Draw(x, 0, size, gl_Height, doFillShapes);
            }
            else
            {
                myPrimitive._Rectangle.Draw(0, y, gl_Width, size, doFillShapes);
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
                    if (doClearBuffer)
                    {
                        glClear(GL_COLOR_BUFFER_BIT);
                    }

                    grad.Draw();
                }

                // Render Frame
                {
                    for (int i = 0; i != Count; i++)
                    {
                        var obj = list[i] as myObj_720;

                        obj.Show();
                        obj.Move();
                    }
                }

                if (Count < N)
                {
                    list.Add(new myObj_720());
                }

                cnt++;
                System.Threading.Thread.Sleep(renderDelay);
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void initShapes()
        {
            myPrimitive.init_Rectangle();

            grad = new myScreenGradient();
            grad.SetRandomColors(rand, 0.2f);
            grad.SetOpacity(dimAlpha);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------
    }
};
