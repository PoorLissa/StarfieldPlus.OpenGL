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
        private float A, dA, R, G, B;
        private bool isOk;

        private static int N = 0, min = 5, max = 100;
        private static bool doFillShapes = false, doUseSquares = true, doUseSize = true, doUseDa = true;

        private static float lineWidth = 2;

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
            doUseSquares = myUtils.randomChance(rand, 2, 3);
            doUseSize    = myUtils.randomChance(rand, 1, 2);
            doUseDa      = myUtils.randomChance(rand, 1, 2);

            min = 5 + rand.Next(10);
            max = 100 + rand.Next(500);

            renderDelay = 10;

            lineWidth = 1.0f + myUtils.randFloat(rand) * 1.5f;

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
                            $"doFillShapes = {doFillShapes}\n"       +
                            $"doUseSquares = {doUseSquares}\n"       +
                            $"doUseSize = {doUseSize}\n"             +
                            $"doUseDa = {doUseDa}\n"                 +
                            $"min = {min}\n"                         +
                            $"max = {max}\n"                         +
                            $"lineWidth = {fStr(lineWidth)}\n"       +
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

            glLineWidth(lineWidth);
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
                width = height = 50 + rand.Next(max);
            }
            else
            {
                width  = 50 + rand.Next(max);
                height = 50 + rand.Next(max);
            }

            // Check if this shape intersects any other existing shapes:
            {
                for (int i = 0; i < list.Count; i++)
                {
                    if (isOk && i != this.id)
                    {
                        var other = list[i] as myObj_640;

                        if (other.isOk)
                        do
                        {
                            int dx = Math.Abs(x - other.x);
                            int dy = Math.Abs(y - other.y);

                            int w = 3 + width  + other.width;
                            int h = 3 + height + other.height;

                            if (dx < w && dy < h)
                            {
                                isOk = false;

                                if (doUseSquares)
                                {
                                    width  -= 2;
                                    height -= 2;
                                }
                                else
                                {
                                    width  -= 2;
                                    height -= 2;
                                }
                            }
                            else
                            {
                                isOk = true;
                            }
                        }
                        while (!isOk && width >= min && height >= min);
                    }

                    if (!isOk)
                        break;
                }
            }

            if (isOk)
            {
                if (doUseDa)
                {
                    A = 0.85f + myUtils.randFloat(rand) * 0.15f;
                    dA = A / cnt;
                }
                else
                {
                    A = 0.5f + myUtils.randFloat(rand) * 0.5f;
                    dA = 0;
                }

                colorPicker.getColor(x, y, ref R, ref G, ref B);

                if (doUseSize)
                {
                    cnt += (width + height) / 2;
                }
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            if (--cnt < 0)
            {
                isOk = false;
                cnt = 0;

                generateNew();
            }
            else
            {
                A -= dA;
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
                myPrimitive._Rectangle.Draw(x - width + lineWidth, y - height + lineWidth, 2 * width - lineWidth, 2 * height - lineWidth, false);

                if (doFillShapes)
                {
                    myPrimitive._Rectangle.SetColor(R, G, B, A / 5);
                    myPrimitive._Rectangle.SetAngle(0);
                    myPrimitive._Rectangle.Draw(x - width, y - height, 2 * width, 2 * height, true);
                }

                if (false)
                {
                    int zzz = 10;

                    myPrimitive._Rectangle.SetColor(R, G, B, A);
                    myPrimitive._Rectangle.SetAngle(0);
                    myPrimitive._Rectangle.Draw(x - width + zzz, y - height + zzz, 2 * (width - zzz), 2 * (height - zzz), false);

                    myPrimitive._Line.SetColor(R, G, B, A);
                    myPrimitive._Line.Draw(x - width, y - height, x - width + zzz, y - height + zzz);
                    myPrimitive._Line.Draw(x - width, y + height, x - width + zzz, y + height - zzz);
                    myPrimitive._Line.Draw(x + width, y - height, x + width - zzz, y - height + zzz);
                    myPrimitive._Line.Draw(x + width, y + height, x + width - zzz, y + height - zzz);

                    myPrimitive._Rectangle.SetColor(R, G, B, A/3);
                    myPrimitive._Rectangle.SetAngle(0);
                    myPrimitive._Rectangle.Draw(x - width + zzz, y - height + zzz, 2 * (width - zzz), 2 * (height - zzz), true);
                }

                if (false)
                {
                    void rect(int factor)
                    {
                        int w = width  / factor;
                        int h = height / factor;

                        float ox = -1 * w * (gl_x0 - x) / gl_x0;
                        float oy = -1 * h * (gl_y0 - y) / gl_y0;

                        myPrimitive._Rectangle.SetColor(R, G, B, A/2);
                        myPrimitive._Rectangle.SetAngle(0);
                        myPrimitive._Rectangle.Draw(x - w - ox, y - h - oy, 2 * w, 2 * h, false);

                        myPrimitive._Line.SetColor(R, G, B, A/2);
                        myPrimitive._Line.Draw(x - width, y - height, x - w - ox, y - h - oy);
                        myPrimitive._Line.Draw(x - width, y + height, x - w - ox, y + h - oy);
                        myPrimitive._Line.Draw(x + width, y - height, x + w - ox, y - h - oy);
                        myPrimitive._Line.Draw(x + width, y + height, x + w - ox, y + h - oy);
                    }

                    rect(3);
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

            myPrimitive.init_Line();

            glLineWidth(lineWidth);
        }

        // ---------------------------------------------------------------------------------------------------------------
    }
};
