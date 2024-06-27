using GLFW;
using static OpenGL.GL;
using System.Collections.Generic;


/*
    - Circular texture stripes moving
*/


namespace my
{
    public class myObj_0400 : myObject
    {
        // Priority
        public static int Priority => 11;
		public static System.Type Type => typeof(myObj_0400);

        private int dir, size, cnt;
        private float x, y, dx, dy;
        private float A;

        private static int N = 0, maxCnt = 0, maxSize = 0, mode = 0;
        private static float speedFactor = 1.0f;

        private static myTexRectangle tex = null;
        private static myScreenGradient grad = null;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_0400()
        {
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
                N = 25 + rand.Next(666);
            }

            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time local initialization
        private void initLocal()
        {
            doClearBuffer = myUtils.randomChance(rand, 2, 7);

            mode = rand.Next(11);
            maxCnt = rand.Next(333) + 100;
            maxSize = myUtils.randomChance(rand, 1, 2)
                ? rand.Next(50) + 1
                : rand.Next(10) + 1;

            speedFactor = 0.25f + myUtils.randFloat(rand) * 0.75f;

            renderDelay = 1;

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            height = 600;

            string str = $"Obj = {Type}\n\n"                               +
                            myUtils.strCountOf(list.Count, N)              +
                            $"mode = {mode}\n"                             +
                            $"doClearBuffer = {doClearBuffer}\n"           +
                            $"maxCnt = {maxCnt}\n"                         +
                            $"maxSize = {maxSize}\n"                       +
                            $"speedFactor = {myUtils.fStr(speedFactor)}\n" +
                            $"renderDelay = {renderDelay}\n"               +
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
            switch (mode)
            {
                case 0:
                case 1:
                case 2:
                case 3:
                    dir = mode;
                    break;

                case 4:
                case 5:
                    dir = rand.Next(2);
                    break;

                case 6:
                case 7:
                    dir = rand.Next(2) + 2;
                    break;

                case 8:
                case 9:
                case 10:
                    dir = rand.Next(4);
                    break;
            }

            cnt = rand.Next(maxCnt) + 1;

            x = rand.Next(gl_Width);
            y = rand.Next(gl_Height);

            size = rand.Next(maxSize) + 1;

            dx = myUtils.randFloat(rand, 0.1f) * (rand.Next(5) + 1) * speedFactor;
            dy = myUtils.randFloat(rand, 0.1f) * (rand.Next(5) + 1) * speedFactor;

            A = myUtils.randFloat(rand, 0.1f);

            switch (dir)
            {
                // Top -> Bottom
                case 0:
                    dx = 0;
                    y = 0;
                    break;

                // Bottom -> Top
                case 1:
                    dx = 0;
                    y = gl_Height;
                    dy *= -1;
                    break;

                // Left -> Right
                case 2:
                    dy = 0;
                    x = 0;
                    break;

                // Right -> Left
                case 3:
                    dy = 0;
                    x = gl_Width;
                    dx *= -1;
                    break;
            }

            A = 1;

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            if (cnt == 0)
            {
                x += dx;
                y += dy;
            }
            else
            {
                cnt--;
            }

            switch (dir)
            {
                case 0:
                    if (y >= gl_Height)
                    {
                        y = 0;
                        cnt = rand.Next(maxCnt) + 1;
                    }
                    break;

                case 1:
                    if (y <= 0)
                    {
                        y = gl_Height;
                        cnt = rand.Next(maxCnt) + 1;
                    }
                    break;

                case 2:
                    if (x >= gl_Width)
                    {
                        x = 0;
                        cnt = rand.Next(maxCnt) + 1;
                    }
                    break;

                case 3:
                    if (x <= 0)
                    {
                        x = gl_Width;
                        cnt = rand.Next(maxCnt) + 1;
                    }
                    break;
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Show()
        {
            int X = (int)x;
            int Y = (int)y;

            tex.setOpacity(A);

            switch (dir)
            {
                // Vertical line
                case 0:
                case 1:
                    tex.Draw(X, Y, size, gl_Height, X, 0, size, gl_Height);
                    tex.Draw(X, 0, size, Y, X, gl_Height - Y, size, Y);
                    break;

                // Horizontal line
                case 2:
                case 3:
                    tex.Draw(X, Y, gl_Width, size, 0, Y, gl_Width, size);
                    tex.Draw(0, Y, X, size, gl_Width - X, Y, X, size);
                    break;
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
                        tex.setOpacity(1);
                        tex.Draw(0, 0, gl_Width, gl_Height);
                    }
                }

                // Render Frame
                {
                    for (int i = 0; i != list.Count; i++)
                    {
                        var obj = list[i] as myObj_0400;

                        obj.Show();
                        obj.Move();
                    }
                }

                if (list.Count < N)
                {
                    list.Add(new myObj_0400());
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

            tex = new myTexRectangle(colorPicker.getImg());

            grad = new myScreenGradient();
            grad.SetRandomColors(rand, 0.2f);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------
    }
};
