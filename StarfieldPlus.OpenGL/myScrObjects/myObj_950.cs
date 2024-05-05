using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;


/*
    - Very narrow window from a texture stretched to full-screen
*/


namespace my
{
    public class myObj_950 : myObject
    {
        // Priority
        public static int Priority => 11;
		public static System.Type Type => typeof(myObj_950);

        private static int size;
        private static float x, y, dx, dy;

        private static int N = 0, mode = 0;
        private static bool doShiftImg = false;
        private static float dimAlpha = 0.05f;

        private static myScreenGradient grad = null;

        private static myTexRectangle tex = null;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_950()
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
                N = 1;

                mode = rand.Next(2);

                doShiftImg = myUtils.randomBool(rand);
            }

            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time local initialization
        private void initLocal()
        {
            doClearBuffer = false;

            renderDelay = rand.Next(3) + 3;

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
                            $"mode = {mode}\n"                       +
                            $"size = {size}\n"                       +
                            $"dx = {fStr(dx)}\n"                     +
                            $"doShiftImg = {doShiftImg}\n"           +
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
            x = 0;
            y = 0;

            dx = myUtils.randFloat(rand);
            dx = 0.25f;
            dy = 0;

            size = rand.Next(10) + 1;

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            x += dx;
            y += dy;

            if (x < 0 || x > gl_Width)
            {
                generateNew();
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Show()
        {
            int X = (int)x;
            int Y = (int)y;

            // Slightly shit Y coordinate up and down
            if (doShiftImg)
            {
                Y = (int)(Math.Sin(x * 0.1) * 10);
            }

            switch (mode)
            {
                case 0:
                    {
                        tex.Draw(0, Y, gl_Width, gl_Height, (int)x, 0, size, gl_Height);
                    }
                    break;

                case 1:
                    {
                        for (int i = 0; i < gl_Width; i += size)
                        {
                            tex.Draw(i, Y, size, gl_Height, (int)x, 0, size, gl_Height);
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

            while (!Glfw.WindowShouldClose(window))
            {
                int Count = list.Count;

                processInput(window);

                // Swap fore/back framebuffers, and poll for operating system events.
                Glfw.SwapBuffers(window);
                Glfw.PollEvents();

                // Dim screen
                if (false)
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
                    for (int i = 0; i != Count; i++)
                    {
                        var obj = list[i] as myObj_950;

                        obj.Show();
                        obj.Move();
                    }
                }

                if (Count < N)
                {
                    list.Add(new myObj_950());
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

            tex = new myTexRectangle(colorPicker.getImg());

            tex.setOpacity(0.1f);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------
    }
};
