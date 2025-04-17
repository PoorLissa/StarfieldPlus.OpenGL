using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;


/*
    - 
*/


namespace my
{
    public class myObj_1330 : myObject
    {
        // Priority
        public static int Priority => 10;
		public static System.Type Type => typeof(myObj_1330);

        private int cnt, dir;
        private float x0, y0, x, y, dx, dy;
        private float A, R, G, B;
        private myObj_1330 parent;

        private static int N = 0, n = 0, dirMode = 0, spdMode = 0;
        private static float dimAlpha = 0.05f;

        private static myScreenGradient grad = null;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_1330()
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
                N = rand.Next(1000) + 1000;
                n = rand.Next(3) + 1;
            }

            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time local initialization
        private void initLocal()
        {
            doClearBuffer = myUtils.randomChance(rand, 1, 2);
            doClearBuffer = true;

            dirMode = rand.Next(3);
            spdMode = rand.Next(2);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            height = 600;

            string str = $"Obj = {Type}\n\n"                  +
                            myUtils.strCountOf(list.Count, N) +
                            $"n = {n}\n"                      +
                            $"dirMode = {dirMode}\n"          +
                            $"spdMode = {spdMode}\n"          +
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
                cnt = 333 + rand.Next(666);

                x = rand.Next(gl_Width);
                y = rand.Next(gl_Height);

                dx = myUtils.randFloatClamped(rand, 0.05f);
                dy = myUtils.randFloatClamped(rand, 0.05f);

                A = 0.3f;
                R = (float)rand.NextDouble();
                G = (float)rand.NextDouble();
                B = (float)rand.NextDouble();

                //colorPicker.getColor(x, y, ref R, ref G, ref B);
            }
            else
            {
                parent = list[rand.Next(n)] as myObj_1330;

                dx = dy = 0;

                switch (dirMode)
                {
                    case 0:
                        dir = 0;
                        dx = parent.dx;
                        break;

                    case 1:
                        dir = 1;
                        dy = parent.dy;
                        break;

                    case 2:
                        dir = rand.Next(2);

                        if (dir == 0)
                        {
                            dx = parent.dx;
                        }
                        else
                        {
                            dy = parent.dy;
                        }
                        break;
                }

                x = 0;
                y = 0;
                x0 = parent.x;
                y0 = parent.y;

                R = parent.R;
                G = parent.G;
                B = parent.B;
                A = parent.A;
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            if (id < n)
            {
                if (--cnt == 0)
                    generateNew();
            }
            else
            {
                x += dx;
                y += dy;

                A -= 0.00025f;

                if (x < 0 || x > gl_Width || y < 0 || y > gl_Height)
                {
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
                if (dir == 0)
                {
                    myPrimitive._LineInst.setInstanceCoords(x0 + x, 0, x0 + x, gl_Height);
                    myPrimitive._LineInst.setInstanceColor(R, G, B, A);

                    myPrimitive._LineInst.setInstanceCoords(x0 - x, 0, x0 - x, gl_Height);
                    myPrimitive._LineInst.setInstanceColor(R, G, B, A);
                }
                else
                {
                    myPrimitive._LineInst.setInstanceCoords(0, y0 + y, gl_Width, y0 + y);
                    myPrimitive._LineInst.setInstanceColor(R, G, B, A);

                    myPrimitive._LineInst.setInstanceCoords(0, y0 - y, gl_Width, y0 - y);
                    myPrimitive._LineInst.setInstanceColor(R, G, B, A);
                }

                switch (spdMode)
                {
                    case 0:
                        {
                            float spd = 1.0075f;
                            dx *= spd;
                            dy *= spd;
                        }
                        break;

                    case 1:
                        {
                            float spd = 0.0075f;
                            dx += spd;
                            dy += spd;
                        }
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
                    myPrimitive._LineInst.ResetBuffer();

                    for (int i = 0; i != Count; i++)
                    {
                        var obj = list[i] as myObj_1330;

                        obj.Show();
                        obj.Move();
                    }

                    myPrimitive._LineInst.Draw();
                }

                if (Count < N)
                {
                    list.Add(new myObj_1330());
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
            myPrimitive.init_LineInst(N * 2);

            grad = new myScreenGradient();
            grad.SetRandomColors(rand, 0.2f);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------
    }
};
