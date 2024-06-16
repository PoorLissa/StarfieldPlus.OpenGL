using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;


/*
    - 
*/


namespace my
{
    public class myObj_0670 : myObject
    {
        // Priority
        public static int Priority => 10;
		public static System.Type Type => typeof(myObj_0670);

        private int i1, i2, cnt, lifeCnt;
        private float x, y, dx, dy, X, Y;
        private float size, width, height, A, R, G, B, angle = 0;

        private static int N = 0, n = 0, shape = 0, x0 = 0, y0 = 0;
        private static bool doFillShapes = false;
        private static float dimAlpha = 0.05f, DX = 0, DY = 0;

        private static myScreenGradient grad = null;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_0670()
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
                N = rand.Next(1111) + 3333;
                n = 3 + rand.Next(20);

                shape = rand.Next(5);

                x0 = rand.Next(gl_Width);
                y0 = rand.Next(gl_Height);

                DX = myUtils.randFloatSigned(rand, 0.1f) * (rand.Next(5) + 1);
                DY = myUtils.randFloatSigned(rand, 0.1f) * (rand.Next(5) + 1);
            }

            initLocal();

            System.Diagnostics.Debug.Assert(n < N);
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time local initialization
        private void initLocal()
        {
            doClearBuffer = true;

            renderDelay = 0;

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            height = 600;

            string nStr(int   n) { return n.ToString("N0");    }
            //string fStr(float f) { return f.ToString("0.000"); }

            string str = $"Obj = {Type}\n\n"                         +
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
            if (id < n)
            {
                int xOff = gl_x0 / (n + 1);
                int yOff = gl_y0 / (n + 1);

                x = id * xOff + 1;
                y = id * yOff + 1;

                width  = gl_Width  - 2 * x;
                height = gl_Height - 2 * y;
            }
            else
            {
                if (myUtils.randomChance(rand, 1, 1111))
                {
                    x0 = rand.Next(gl_Width);
                    y0 = rand.Next(gl_Height);

                    DX = myUtils.randFloatSigned(rand, 0.1f) * (rand.Next(5) + 1);
                    DY = myUtils.randFloatSigned(rand, 0.1f) * (rand.Next(5) + 1);
                }

                cnt = 0;
                lifeCnt = 333 + rand.Next(3333);

                x = rand.Next(gl_Width);
                y = rand.Next(gl_Height);

                x = x0;
                y = y0;

                dx = myUtils.randFloatSigned(rand, 0.1f) * (rand.Next(5) + 1);
                dy = myUtils.randFloatSigned(rand, 0.1f) * (rand.Next(5) + 1);

                if (myUtils.randomChance(rand, 1, 2))
                {
                    dx = DX + myUtils.randFloatSigned(rand) * 0.075f;
                    dy = DY + myUtils.randFloatSigned(rand) * 0.075f;
                }
                else
                {
                    dx = -DX + myUtils.randFloatSigned(rand) * 0.075f;
                    dy = -DY + myUtils.randFloatSigned(rand) * 0.075f;
                }

                findBorders();

                size = 3;

                A = myUtils.randFloat(rand);
                colorPicker.getColor(x0, y0, ref R, ref G, ref B);
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            if (id >= n)
            {
                if (--lifeCnt == 0)
                {
                    generateNew();
                }

                x += dx;
                y += dy;

                if (i2 < 0)
                {
                    var rect = list[i1] as myObj_0670;

                    int rx = (int)rect.x;
                    int ry = (int)rect.y;
                    int rw = (int)rect.width;
                    int rh = (int)rect.height;

                    if (x < rx && dx < 0)
                    {
                        dx *= -1;
                        X = x;
                        Y = y;
                        cnt = 50;
                    }

                    if (x > rx + rw && dx > 0)
                    {
                        dx *= -1;
                        X = x;
                        Y = y;
                        cnt = 50;
                    }

                    if (y < ry && dy < 0)
                    {
                        dy *= -1;
                        X = x;
                        Y = y;
                        cnt = 50;
                    }

                    if (y > ry + rh && dy > 0)
                    {
                        dy *= -1;
                        X = x;
                        Y = y;
                        cnt = 50;
                    }
                }
                else
                {
                    if (outBounce())
                    {
                        cnt = 50;
                    }
                    else
                    {
                        var rect2 = list[i2] as myObj_0670;

                        int rx2 = (int)rect2.x;
                        int ry2 = (int)rect2.y;
                        int rw2 = (int)rect2.width;
                        int rh2 = (int)rect2.height;

                        bool xIsInside = x > rx2 && x < rx2 + rw2;
                        bool yIsInside = y > ry2 && y < ry2 + rh2;

                        if (xIsInside && yIsInside)
                        {
                            if (dx > 0 && x < rx2 + 10)
                            {
                                dx *= -1;
                                X = x;
                                Y = y;
                                cnt = 50;
                            }
                            else if (dx < 0 && x > rx2 + rw2 - 10)
                            {
                                dx *= -1;
                                X = x;
                                Y = y;
                                cnt = 50;
                            }

                            if (dy > 0 && y < ry2 + 10)
                            {
                                dy *= -1;
                                X = x;
                                Y = y;
                                cnt = 50;
                            }
                            else if (dy < 0 && y > ry2 + rh2 - 10)
                            {
                                dy *= -1;
                                X = x;
                                Y = y;
                                cnt = 50;
                            }
                        }
                    }
                }
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Show()
        {
            if (id < n)
            {
                myPrimitive._Rectangle.SetColor(1, 1, 1, 0.07f);
                myPrimitive._Rectangle.Draw(x, y, width, height, false);
            }
            else
            {
                if (--cnt > 0)
                {
                    //myPrimitive._Rectangle.SetColor(0, 1, 0, 0.33f);
                    //myPrimitive._Rectangle.Draw(X, Y, 2, 2, false);
                }

                float size2x = size * 2;

                switch (shape)
                {
                    // Instanced squares
                    case 0:
                        myPrimitive._RectangleInst.setInstanceCoords(x - size, y - size, size2x, size2x);
                        myPrimitive._RectangleInst.setInstanceColor(R, G, B, A);
                        myPrimitive._RectangleInst.setInstanceAngle(angle);
                        break;

                    // Instanced triangles
                    case 1:
                        myPrimitive._TriangleInst.setInstanceCoords(x, y, size2x, angle);
                        myPrimitive._TriangleInst.setInstanceColor(R, G, B, A);
                        break;

                    // Instanced circles
                    case 2:
                        myPrimitive._EllipseInst.setInstanceCoords(x, y, size2x, angle);
                        myPrimitive._EllipseInst.setInstanceColor(R, G, B, A);
                        break;

                    // Instanced pentagons
                    case 3:
                        myPrimitive._PentagonInst.setInstanceCoords(x, y, size2x, angle);
                        myPrimitive._PentagonInst.setInstanceColor(R, G, B, A);
                        break;

                    // Instanced hexagons
                    case 4:
                        myPrimitive._HexagonInst.setInstanceCoords(x, y, size2x, angle);
                        myPrimitive._HexagonInst.setInstanceColor(R, G, B, A);
                        break;
                }
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
                        var obj = list[i] as myObj_0670;

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
                    list.Add(new myObj_0670());
                }

                cnt++;
                System.Threading.Thread.Sleep(renderDelay);
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        // Find the largest parent rectangle for which the current particle is outside of it
        private void findBorders()
        {
            i1 = n - 1;     // The most inner rectangle
            i2 = -1;        // Invalid id

            for (int i = 1; i < n; i++)
            {
                var rect = list[i] as myObj_0670;

                int rx = (int)rect.x;
                int ry = (int)rect.y;
                int rw = (int)rect.width;
                int rh = (int)rect.height;

                if (x < rx || x > rx + rw || y < ry || y > ry + rh)
                {
                    i1 = i - 1;
                    i2 = i;
                    break;
                }
            }
        }

        // ---------------------------------------------------------------------------------------------------------------

        private bool outBounce()
        {
            bool res = false;

            var rect = list[i1] as myObj_0670;

            int rx = (int)rect.x;
            int ry = (int)rect.y;
            int rw = (int)rect.width;
            int rh = (int)rect.height;

            if (x < rx && dx < 0)
            {
                dx *= -1;
                res = true;
                X = x;
                Y = y;
            }
            else if (x > rx + rw && dx > 0)
            {
                dx *= -1;
                res = true;
                X = x;
                Y = y;
            }

            if (y < ry && dy < 0)
            {
                dy *= -1;
                res = true;
                X = x;
                Y = y;
            }
            else if (y > ry + rh && dy > 0)
            {
                dy *= -1;
                res = true;
                X = x;
                Y = y;
            }

            return res;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void initShapes()
        {
            myPrimitive.init_ScrDimmer();
            myPrimitive.init_Rectangle();
            base.initShapes(shape, N, 0);

            grad = new myScreenGradient();
            grad.SetRandomColors(rand, 0.2f);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------
    }
};
