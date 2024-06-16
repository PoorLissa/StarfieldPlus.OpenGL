using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;


/*
    - Thin texture lines moving top to bottom of the screen
*/


namespace my
{
    public class myObj_0830 : myObject
    {
        // Priority
        public static int Priority => 10;
		public static System.Type Type => typeof(myObj_0830);

        private float x, y, dx, dy, A;
        private float size1, size2;

        private static int N = 0, mode = 0, maxSpeed = 0, maxSize = 0;
        private static float dimAlpha = 0.05f;

        private static myScreenGradient grad = null;
        private static myTexRectangle tex = null;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_0830()
        {
            if (id != uint.MaxValue)
                generateNew();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time global initialization
        protected override void initGlobal()
        {
            colorPicker = new myColorPicker(gl_Width, gl_Height, myColorPicker.colorMode.SNAPSHOT_OR_IMAGE);
            list = new List<myObject>();

            // Global unmutable constants
            {
                dimAlpha = 0.025f + myUtils.randFloat(rand) * 0.075f;
            }

            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time local initialization
        private void initLocal()
        {
            doClearBuffer = false;

            mode = rand.Next(6);

            maxSize  = 2 + rand.Next(9);
            maxSpeed = 2 + rand.Next(9);

            renderDelay = rand.Next(11) + 3;

            switch (mode)
            {
                case 0:
                case 1:
                case 2:
                case 3:
                    {
                        switch (rand.Next(5))
                        {
                            case 0: N = 1; break;
                            case 1: N = 1 + rand.Next(3); break;
                            case 2: N = 1 + rand.Next(5); break;
                            case 3: N = 1 + rand.Next(7); break;
                            case 4: N = 1 + rand.Next(9); break;
                        }
                    }
                    break;

                case 4:
                case 5:
                    {
                        N = 123;
                    }
                    break;
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            height = 600;

            string nStr(int   n) { return n.ToString("N0");    }
            string fStr(float f) { return f.ToString("0.000"); }

            string str = $"Obj = {Type}\n\n"                         +
                            $"N = {nStr(N)} of {nStr(list.Count)}\n" +
                            $"doClearBuffer = {doClearBuffer}\n"     +
                            $"mode = {mode}\n"                       +
                            $"maxSpeed = {maxSpeed}\n"               +
                            $"dimAlpha = {fStr(dimAlpha)}\n"         +
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
            float getSpeed() {
                return 0.25f + myUtils.randFloat(rand) * maxSpeed;
            }

            // -----------------------------------------------------------

            A = 0.75f + myUtils.randFloat(rand) * 0.15f;

            switch (mode)
            {
                case 0:
                    {
                        size2 = gl_Width;
                        x = 0;
                        dx = 0;

                        y = 0;
                        dy = getSpeed();
                    }
                    break;

                case 1:
                    {
                        size2 = gl_Width;
                        x = 0;
                        dx = 0;

                        if (myUtils.randomChance(rand, 1, 2))
                        {
                            y = 0;
                            dy = getSpeed();
                        }
                        else
                        {
                            y = gl_Height;
                            dy = -getSpeed();
                        }
                    }
                    break;

                case 2:
                    {
                        size2 = gl_Height;
                        y = 0;
                        dy = 0;

                        x = 0;
                        dx = getSpeed();
                    }
                    break;

                case 3:
                    {
                        size2 = gl_Height;
                        y = 0;
                        dy = 0;

                        if (myUtils.randomChance(rand, 1, 2))
                        {
                            x = 0;
                            dx = getSpeed();
                        }
                        else
                        {
                            x = gl_Width;
                            dx = -getSpeed();
                        }
                    }
                    break;

                case 4:
                    {
                        A *= 0.5f;

                        size2 = 50 + rand.Next(333);
                        x = rand.Next(gl_Width);
                        dx = 0;

                        y = 0;
                        dy = getSpeed();
                    }
                    break;

                case 5:
                    {
                        A *= 0.5f;

                        size2 = 50 + rand.Next(333);
                        x = rand.Next(gl_Width);
                        dx = 0;

                        if (myUtils.randomChance(rand, 1, 2))
                        {
                            y = 0;
                            dy = getSpeed();
                        }
                        else
                        {
                            y = gl_Height;
                            dy = -getSpeed();
                        }
                    }
                    break;
            }

            size1 = maxSize;

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            x += dx;
            y += dy;

            if ((x < -10 && dx < 0) || (x > gl_Width && dx > 0) || (y < -10 && dy < 0) || (y > gl_Height && dy > 0))
            {
                generateNew();
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Show()
        {
            tex.setOpacity(A);

            switch (mode)
            {
                case 0:
                case 1:
                case 4:
                case 5:
                    tex.Draw((int)x, (int)y, (int)size2, (int)size1, (int)x, (int)y, (int)size2, (int)size1);
                    break;

                case 2:
                case 3:
                    tex.Draw((int)x, (int)y, (int)size1, (int)size2, (int)x, (int)y, (int)size1, (int)size2);
                    break;
            }
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Process(Window window)
        {
            uint cnt = 0;
            initShapes();


            clearScreenSetup(doClearBuffer, 0.1f, true);


            while (list.Count < 123)
            {
                list.Add(new myObj_0830());
            }


            while (!Glfw.WindowShouldClose(window))
            {
                int Count = N;

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
                        //dimScreen(dimAlpha);
                        grad.Draw();
                    }
                }

                // Render Frame
                {
                    for (int i = 0; i != Count; i++)
                    {
                        var obj = list[i] as myObj_0830;

                        obj.Show();
                        obj.Move();
                    }
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

            grad = new myScreenGradient();
            grad.SetRandomColors(rand, 0.2f);
            grad.SetOpacity(dimAlpha);

            tex = new myTexRectangle(colorPicker.getImg());

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------
    }
};
