using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;


/*
    - 
*/


namespace my
{
    public class myObj_1410 : myObject
    {
        // Priority
        public static int Priority => 10;
		public static System.Type Type => typeof(myObj_1410);

        private bool isMoving;
        private int dir;
        private float x, y, x0, y0, xOld, yOld, tFactor, Rad;

        private static int N = 0;
        private static float dimAlpha = 0.05f, t = 0, dt = 0.01f, rad;

        private static myScreenGradient grad = null;
        private static myTexRectangle tex = null;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_1410()
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
                N = 2 + rand.Next(17);
            }

            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time local initialization
        private void initLocal()
        {
            doClearBuffer = myUtils.randomChance(rand, 1, 2);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            height = 600;

            string str = $"Obj = {Type}\n\n"                  +
                            myUtils.strCountOf(list.Count, N) +
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
            if (id == 0)
            {
                x = y = 0;
            }
            else
            {
                x = rand.Next(7) - 3;
                y = rand.Next(7) - 3;

                tFactor = 0.5f + myUtils.randFloat(rand) * 0.33f;

                Rad = 7 + rand.Next(51);

                dir = myUtils.randomSign(rand);

                isMoving = false;
            }

            x0 = xOld = x;
            y0 = yOld = y;

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            if (false)
            {
                if (myUtils.randomChance(rand, 1, 111))
                {
                    xOld = x;
                    yOld = y;

                    x = rand.Next(27) - 13;
                    y = rand.Next(27) - 13;
                }
            }

            rad = 3 + (float)Math.Abs(Math.Sin(t * tFactor) * Rad);

            x = x0 + (float)Math.Sin(dir * t * tFactor) * rad;
            y = y0 + (float)Math.Cos(dir * t * tFactor) * rad;

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Show()
        {
            if (id == 0)
            {
                tex.setOpacity(1);
                tex.Draw(0, 0, gl_Width, gl_Height);
            }
            else
            {
                tex.setOpacity(1.0 / N);
                tex.Draw((int)x, (int)y, gl_Width, gl_Height);
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
                        //glClear(GL_COLOR_BUFFER_BIT);
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
                        var obj = list[i] as myObj_1410;

                        obj.Show();
                        obj.Move();
                    }
                }

                if (Count < N)
                {
                    list.Add(new myObj_1410());
                }

                stopwatch.WaitAndRestart();
                cnt++;
                t += dt;
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void initShapes()
        {
            myPrimitive.init_ScrDimmer();
            tex = new myTexRectangle(colorPicker.getImg());

            grad = new myScreenGradient();
            grad.SetRandomColors(rand, 0.2f);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------
    }
};
