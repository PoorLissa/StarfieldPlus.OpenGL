using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;


/*
    - Create random rectangles, but put them on the screen only when they don't intersect any existing rectangles
*/


namespace my
{
    public class myObj_640 : myObject
    {
        // Priority
        public static int Priority => 99910;
		public static System.Type Type => typeof(myObj_640);

        private int x, y, width, height, cnt;
        private float A, R, G, B;
        private bool isOk;

        private static int N = 0;
        private static bool doFillShapes = false, doUseSquares = true;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_640()
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
                N = 1000;
            }

            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time local initialization
        private void initLocal()
        {
            doClearBuffer = true;
            doFillShapes = myUtils.randomChance(rand, 1, 2);
            doUseSquares = myUtils.randomChance(rand, 1, 2);

            renderDelay = 10;

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            height = 600;

            string nStr(int   n) { return n.ToString("N0");    }
            //string fStr(float f) { return f.ToString("0.000"); }

            string str = $"Obj = myObj_640\n\n"                      +
                            $"N = {nStr(list.Count)} of {nStr(N)}\n" +
                            $"doFillShapes = {doFillShapes}\n"       +
                            $"doUseSquares = {doUseSquares}\n"       +
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
            isOk = true;
            cnt = 300 + rand.Next(1000);

            x = rand.Next(gl_Width);
            y = rand.Next(gl_Height);

            if (doUseSquares)
            {
                width = height = 50 + rand.Next(100);
            }
            else
            {
                width  = 50 + rand.Next(100);
                height = 50 + rand.Next(100);
            }

            // Check if this shape intersects any other existing shapes:
            {
                for (int i = 0; i < list.Count; i++)
                {
                    if (isOk && i != this.id)
                    {
                        var other = list[i] as myObj_640;

                        do
                        {
                            int dx = Math.Abs(x - other.x);
                            int dy = Math.Abs(y - other.y);

                            int w = 3 + width  + other.width;
                            int h = 3 + height + other.height;

                            if (dx < w && dy < h)
                            {
                                isOk = false;
                                width  -= 2;
                                height -= 2;
                            }
                            else
                            {
                                isOk = true;
                            }
                        }
                        while (!isOk && width >= 50);
                    }

                    if (!isOk)
                        break;
                }
            }

            if (isOk)
            {
                A = 0.5f;
                colorPicker.getColor(x, y, ref R, ref G, ref B);
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            if (--cnt < 0)
            {
                cnt = 0;
                generateNew();
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Show()
        {
            if (isOk)
            {
                myPrimitive._Rectangle.SetColor(R, G, B, A);
                myPrimitive._Rectangle.SetAngle(0);
                myPrimitive._Rectangle.Draw(x - width, y - height, 2 * width, 2 * height, false);

                if (doFillShapes)
                {
                    myPrimitive._Rectangle.SetColor(R, G, B, A / 5);
                    myPrimitive._Rectangle.SetAngle(0);
                    myPrimitive._Rectangle.Draw(x - width, y - height, 2 * width, 2 * height, true);
                }
            }
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
                    glClear(GL_COLOR_BUFFER_BIT);
                }

                // Render Frame
                {
                    for (int i = 0; i != Count; i++)
                    {
                        var obj = list[i] as myObj_640;

                        obj.Show();
                        obj.Move();
                    }
                }

                if (Count < N)
                {
                    list.Add(new myObj_640());
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
            myPrimitive.init_Rectangle();
        }

        // ---------------------------------------------------------------------------------------------------------------
    }
};
