using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;


/*
    - 
*/


namespace my
{
    public class myObj_1310 : myObject
    {
        // Priority
        public static int Priority => 10;
		public static System.Type Type => typeof(myObj_1310);

        private int cnt;
        private float x, y, dx, dy;
        private float a, b, c;
        private float size, A, R, G, B, angle = 0;

        private static int N = 0, n = 0, shape = 0, moveMode = 0, opacityMode = 0;
        private static bool doFillShapes = false;
        private static float dimAlpha = 0.05f;

        private static myScreenGradient grad = null;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_1310()
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
                N = rand.Next(10000) + 12345;
                n = 1;

                shape = rand.Next(5);
            }

            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time local initialization
        private void initLocal()
        {
            doClearBuffer = true;

            moveMode = rand.Next(2);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            height = 600;

            string str = $"Obj = {Type}\n\n"                  +
                            myUtils.strCountOf(list.Count, N) +
                            $"moveMode = {moveMode}\n"        +
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

            if (id < n)
            {
                dx = myUtils.randFloatSigned(rand, 0.2f) * 3;
                dy = myUtils.randFloatSigned(rand, 0.2f) * 3;

                size = 1;

                R = G = B = A = 0;

                a = 7;
                b = -3;
                c = 5000;
            }
            else
            {
                cnt = 333 + rand.Next(333);
                size = rand.Next(3) + 3;
                size = 2;

                R = (float)rand.NextDouble();
                G = (float)rand.NextDouble();
                B = (float)rand.NextDouble();

                R = G = B = 0.5f;

                A = 0.1f;
                //colorPicker.getColor(x, y, ref R, ref G, ref B);
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            if (id < n)
            {
                x += dx;
                y += dy;

                a += 0.02f;
                //b += 0.01f;

                c -= 33;

                int maxDist = 250;

                for (int i = n; i < list.Count; i++)
                {
                    var other = list[i] as myObj_1310;

                    float d = (float)(Math.Abs(a * other.x + b * other.y + c) / (Math.Sqrt(a * a + b * b)));

                    var dX = x - other.x;
                    var dY = y - other.y;

                    //var dist = Math.Sqrt(dX * dX + dY * dY);

                    var dist = d;
                    maxDist = 50;

opacityMode = 0;

                    switch (opacityMode)
                    {
                        case 0:
                            {
                                if (dist > maxDist)
                                {
                                    other.A = other.A > 0.1f ? other.A : 0.1f;
                                }
                                else
                                {
                                    other.A = (float)((1.0 * maxDist - dist) / maxDist);
                                }
                            }
                            break;

                        case 1:
                            {
                                if (dist > maxDist)
                                {
                                    other.A -= (other.A > 0.1f) ? 0.003f : 0;
                                }
                                else
                                {
                                    //other.A = (float)((1.0 * maxDist - dist) / maxDist);

                                    other.A += 0.03f * (float)((1.0 * maxDist - dist) / maxDist);
                                }
                            }
                            break;
                    }
                }

                switch (moveMode)
                {
                    case 0:
                        {
                            if (x < 0 && dx < 0)
                                dx *= -1;

                            if (y < 0 && dy < 0)
                                dy *= -1;

                            if (x > gl_Width && dx > 0)
                                dx *= -1;

                            if (y > gl_Height && dy > 0)
                                dy *= -1;
                        }
                        break;

                    case 1:
                        {
                            if (x < 0)
                                dx += 0.01f;

                            if (y < 0)
                                dy += 0.01f;

                            if (x > gl_Width)
                                dx -= 0.01f;

                            if (y > gl_Height)
                                dy -= 0.01f;
                        }
                        break;
                }
            }
            else
            {
                if (--cnt < 0)
                {
                    A -= 0.0001f;

                    if (A < 0)
                        generateNew();
                }
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Show()
        {
            if (id >= n)
            {
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
                        myPrimitive._TriangleInst.setInstanceCoords(x, y, size, angle);
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

            while (list.Count < N / 2)
            {
                list.Add(new myObj_1310());
            }

            stopwatch = new StarfieldPlus.OpenGL.myUtils.myStopwatch(true);

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
                        var obj = list[i] as myObj_1310;

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
                    list.Add(new myObj_1310());
                }

                stopwatch.WaitAndRestart();
                cnt++;
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void initShapes()
        {
            myPrimitive.init_ScrDimmer();
            base.initShapes(shape, N, 0);

            grad = new myScreenGradient();
            grad.SetRandomColors(rand, 0.2f);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------
    }
};
