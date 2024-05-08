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

        private static int N = 0, mode = 0, dirMode = 0;
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

                mode = rand.Next(2);        // Drawing : Stretch vs Repeat
                dirMode = rand.Next(3);     // Movement: Vertical, Horizontal or Both

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
                            $"dirMode = {dirMode}\n"                 +
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

            switch (dirMode)
            {
                case 0:
                    dx = myUtils.randFloat(rand);
                    dx = 0.25f;
                    dy = 0;
                    break;

                case 1:
                    dx = 0;
                    dy = 0.25f;
                    break;

                case 2:
                    dx = 0.25f;
                    dy = 0.25f;
                    break;
            }

            size = rand.Next(10) + 1;

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            x += dx;
            y += dy;

            if (x < 0 || x > gl_Width || y < 0 || y > gl_Height)
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

            switch (dirMode)
            {
                // Horizontal movement
                case 0:
                    {
                        // Slightly shit Y coordinate up and down
                        if (doShiftImg)
                        {
                            Y = (int)(Math.Sin(x * 0.1) * 10);
                        }

                        switch (mode)
                        {
                            // Stretch
                            case 0:
                                {
                                    tex.Draw(0, Y, gl_Width, gl_Height, X, 0, size, gl_Height);
                                }
                                break;

                            // Repeat
                            case 1:
                                {
                                    for (int i = 0; i < gl_Width; i += size)
                                    {
                                        tex.Draw(i, Y, size, gl_Height, X, 0, size, gl_Height);
                                    }
                                }
                                break;
                        }
                    }
                    break;

                // Vertical movement
                case 1:
                    {
                        // Slightly shit X coordinate up and down
                        if (doShiftImg)
                        {
                            X = (int)(Math.Sin(y * 0.1) * 10);
                        }

                        switch (mode)
                        {
                            // Stretch
                            case 0:
                                {
                                    tex.Draw(X, 0, gl_Width, gl_Height, 0, Y, gl_Width, size);
                                }
                                break;

                            // Repeat
                            case 1:
                                {
                                    for (int i = 0; i < gl_Height; i += size)
                                    {
                                        tex.Draw(X, i, gl_Width, size, 0, X, gl_Width, size);
                                    }
                                }
                                break;
                        }
                    }
                    break;

                // Vertical + Horizontal movements
                case 2:
                    {
                        switch (mode)
                        {
                            // Stretch
                            case 0:
                                {
                                    tex.Draw(0, 0, gl_Width, gl_Height, X, 0, size, gl_Height);
                                    tex.Draw(0, 0, gl_Width, gl_Height, 0, Y, gl_Width, size);
                                }
                                break;

                            // Repeat
                            case 1:
                                {
                                    for (int i = 0; i < gl_Width; i += size)
                                    {
                                        tex.Draw(i, 0, size, gl_Height, X, 0, size, gl_Height);
                                    }

                                    for (int i = 0; i < gl_Height; i += size)
                                    {
                                        tex.Draw(0, i, gl_Width, size, 0, Y, gl_Width, size);
                                    }
                                }
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
