using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;


/*
    - Scrolling texture images
*/


namespace my
{
    public class myObj_1170 : myObject
    {
        // Priority
        public static int Priority => 10;
		public static System.Type Type => typeof(myObj_1170);

        private float x, y, spd, A;

        private static int N = 0, n = 0, mode = 0;

        private static myScreenGradient grad = null;
        private static myTexRectangle tex = null;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_1170()
        {
            if (id != uint.MaxValue)
                generateNew();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time global initialization
        protected override void initGlobal()
        {
            colorPicker = new myColorPicker(gl_Width, gl_Height,  mode: myColorPicker.colorMode.SNAPSHOT_OR_IMAGE);
            list = new List<myObject>();

            // Global unmutable constants
            {
                N = 4;
                n = rand.Next(N-1) + 2;
            }

            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time local initialization
        private void initLocal()
        {
            doClearBuffer = myUtils.randomBool(rand);
            mode = rand.Next(3);
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            height = 600;

            string str = $"Obj = {Type}\n\n"                  +
                            myUtils.strCountOf(list.Count, N) +
                            $"n = {n}\n"                      +
                            $"mode = {mode}\n"                +
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
            x = 0;
            y = 0;

            spd = myUtils.randFloatClamped(rand, 0.5f) * 3 * myUtils.randomSign(rand);
            A = 0.25f;

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Show()
        {
            int X = (int)x;
            int Y = (int)y;

            tex.setOpacity(A);

            switch (mode)
            {
                case 0:
                    {
                        if (id < n)
                        {
                            tex.Draw(X, Y, gl_Width - X, gl_Height - Y, 0, 0, gl_Width - X, gl_Height - Y);
                            tex.Draw(0, 0, X, gl_Height, gl_Width - X, gl_Height - Y, X, gl_Height);
                            x += spd;
                        }
                    }
                    break;

                case 1:
                    {
                        if (id < n)
                        {
                            tex.Draw(X, Y, gl_Width - X, gl_Height - Y, 0, 0, gl_Width - X, gl_Height - Y);
                            tex.Draw(0, 0, gl_Width, Y, gl_Width - X, gl_Height - Y, gl_Width, Y);
                            y += spd;
                        }
                    }
                    break;

                case 2:
                    {
                        switch (id % 2)
                        {
                            case 0:
                                tex.Draw(X, Y, gl_Width - X, gl_Height - Y, 0, 0, gl_Width - X, gl_Height - Y);
                                tex.Draw(0, 0, X, gl_Height, gl_Width - X, gl_Height - Y, X, gl_Height);
                                x += spd;
                                break;

                            case 1:
                                tex.Draw(X, Y, gl_Width - X, gl_Height - Y, 0, 0, gl_Width - X, gl_Height - Y);
                                tex.Draw(0, 0, gl_Width, Y, gl_Width - X, gl_Height - Y, gl_Width, Y);
                                y += spd;
                                break;
                        }
                    }
                    break;
            }
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
                        glClear(GL_COLOR_BUFFER_BIT);

                    grad.Draw();
                }

                // Render Frame
                {
                    for (int i = 0; i != Count; i++)
                    {
                        var obj = list[i] as myObj_1170;

                        obj.Show();
                        obj.Move();
                    }
                }

                if (Count < N)
                {
                    list.Add(new myObj_1170());
                }

                stopwatch.WaitAndRestart();
                cnt++;
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void initShapes()
        {
            grad = new myScreenGradient();
            grad.SetRandomColors(rand, 0.2f);

            tex = new myTexRectangle(colorPicker.getImg());

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------
    }
};
