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
        public static int Priority => 10;
		public static System.Type Type => typeof(myObj_640);

        private int x, y, width, height, cnt;
        private float A, dA, R, G, B;
        private bool isOk;

        private static int N = 0, min = 5, minSize = 50, maxSize = 100, maxCnt = 1000, rectMode = 0, drawMode = 0;
        private static bool doFillShapes = false, doUseSquares = true, doUseSize = true, doUseDa = true;

        private static float lineWidth = 2;

        private static myTexRectangle tex = null;
        private static myScreenGradient grad = null;

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
                N = 1000 + rand.Next(6666);

                drawMode = 0;

                if (colorPicker.getMode() == (int)myColorPicker.colorMode.SNAPSHOT || colorPicker.getMode() == (int)myColorPicker.colorMode.IMAGE)
                    if (myUtils.randomChance(rand, 1, 3))
                        drawMode = 2 + rand.Next(2);        // 2 .. 3
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
            doUseSize    = myUtils.randomChance(rand, 1, 2);
            doUseDa      = myUtils.randomChance(rand, 1, 2);

            min = 5 + rand.Next(10);            // min size while reducing shapes
            minSize = 50;                       // min size while generating shapes
            maxSize = 100 + rand.Next(500);     // max size while generating shapes

            rectMode = rand.Next(3);
            maxCnt = 1000 + rand.Next(12345);

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
                            $"minSize = {minSize}\n"                 +
                            $"maxSize = {maxSize}\n"                 +
                            $"maxCnt = {maxCnt}\n"                   +
                            $"rectMode = {rectMode}\n"               +
                            $"drawMode = {drawMode}\n"               +
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

            x = rand.Next(gl_Width);
            y = rand.Next(gl_Height);

            if (doUseSquares)
            {
                width = height = minSize + rand.Next(maxSize);
            }
            else
            {
                width  = minSize + rand.Next(maxSize);
                height = minSize + rand.Next(maxSize);
            }

            // Check if this rectangle intersects any other existing rectangles:
            {
                int Count = list.Count, minOffset = 3;

                for (int i = 0; isOk && i < Count; i++)
                {
                    if (i != this.id)
                    {
                        var other = list[i] as myObj_640;

                        if (other.isOk)
                        {
                            int dx = Math.Abs(x - other.x);
                            int dy = Math.Abs(y - other.y);

                            // Center of the current shape is lying within the other shape (with some tolerance)
                            if ((dx < other.width + minOffset + 1) && (dy < other.height + minOffset + 1))
                            {
                                isOk = false;
                                break;
                            }

                            do
                            {
                                int w = minOffset + width  + other.width;
                                int h = minOffset + height + other.height;

                                isOk = dx > w || dy > h;

                                if (!isOk)
                                {
                                    if (doUseSquares)
                                    {
                                        width = height = dx > dy
                                            ? dx - other.width  - minOffset - 1
                                            : dy - other.height - minOffset - 1;
                                    }
                                    else
                                    {
                                        switch (rectMode)
                                        {
                                            case 0:
                                                width  -= 2;
                                                height -= 2;
                                                break;

                                            case 1:
                                                if (dx < dy)
                                                {
                                                    height = dy - other.height - minOffset - 1;
                                                }
                                                else
                                                {
                                                    width = dx - other.width - minOffset - 1;
                                                }
                                                break;

                                            case 2:
                                                if (width < height)
                                                {
                                                    height = dy - other.height - minOffset - 1;
                                                }
                                                else
                                                {
                                                    width = dx - other.width - minOffset - 1;
                                                }
                                                break;
                                        }
                                    }
                                }
                            }
                            while (!isOk && width >= min && height >= min);
                        }
                    }
                }
            }

            if (isOk)
            {
                cnt = 300 + rand.Next(maxCnt);
                colorPicker.getColor(x, y, ref R, ref G, ref B);

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

                if (doUseSize)
                {
                    cnt += (width + height) / 2;
                }

                if (drawMode == 3)
                {
                    A = 1;
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
                switch (drawMode)
                {
                    // Draw plain rectangles / squares
                    case 0:
                    case 1:
                        {
                            myPrimitive._Rectangle.SetColor(R, G, B, A);
                            myPrimitive._Rectangle.SetAngle(0);
                            myPrimitive._Rectangle.Draw(x - width + lineWidth, y - height + lineWidth, 2 * width - lineWidth, 2 * height - lineWidth, false);

                            if (doFillShapes)
                            {
                                myPrimitive._Rectangle.SetColor(R, G, B, A * 0.2f);
                                myPrimitive._Rectangle.SetAngle(0);
                                myPrimitive._Rectangle.Draw(x - width, y - height, 2 * width, 2 * height, true);
                            }
                        }
                        break;

                    // Draw from image
                    case 2:
                        {
                            tex.setOpacity(A);
                            tex.Draw(x - width, y - height, 2 * width, 2 * height, x - width, y - height, 2 * width, 2 * height);

                            myPrimitive._Rectangle.SetColor(R, G, B, A);
                            myPrimitive._Rectangle.SetAngle(0);
                            myPrimitive._Rectangle.Draw(x - width + lineWidth, y - height + lineWidth, 2 * width - lineWidth, 2 * height - lineWidth, false);
                        }
                        break;

                    // Draw from image, opacity is inversely proportional to size
                    case 3:
                        {
                            float factor = 25.0f * (min * min) / (minSize * maxSize);

                            tex.setOpacity(A * factor);
                            tex.Draw(x - width, y - height, 2 * width, 2 * height, x - width, y - height, 2 * width, 2 * height);

                            myPrimitive._Rectangle.SetColor(R, G, B, A);
                            myPrimitive._Rectangle.SetAngle(0);
                            myPrimitive._Rectangle.Draw(x - width + lineWidth, y - height + lineWidth, 2 * width - lineWidth, 2 * height - lineWidth, false);
                        }
                        break;
                }

/*
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
*/
            }
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Process(Window window)
        {
            uint cnt = 0;
            initShapes();


            glDrawBuffer(GL_FRONT_AND_BACK | GL_DEPTH_BUFFER_BIT);
            glClearColor(0, 0, 0, 1);


            while (!Glfw.WindowShouldClose(window))
            {
                int Count = list.Count;

                processInput(window);

                // Swap fore/back framebuffers, and poll for operating system events.
                Glfw.SwapBuffers(window);
                Glfw.PollEvents();

                // Clear screen
                {
                    glClear(GL_COLOR_BUFFER_BIT);
                    grad.Draw();
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
            grad = new myScreenGradient();
            grad.SetRandomColors(rand, 0.23f, mode: 0);

            myPrimitive.init_Rectangle();
            myPrimitive.init_Line();

            glLineWidth(lineWidth);

            if (drawMode == 2 || drawMode == 3)
            {
                tex = new myTexRectangle(colorPicker.getImg());
            }
        }

        // ---------------------------------------------------------------------------------------------------------------
    }
};
