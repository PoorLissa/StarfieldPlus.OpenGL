using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;


/*
    - Tiled image transitioning to another image over time
*/


namespace my
{
    public class myObj_1040 : myObject
    {
        // Priority
        public static int Priority => 9999910;
		public static System.Type Type => typeof(myObj_1040);

        private int x, y, cnt;

        private static float maxA = 1;
        private static int N = 0, cellSize = 1, cellMargin = 0, xOffset = 0, yOffset = 0;

        private static myScreenGradient grad = null;
        private static myTexRectangle tex = null;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_1040()
        {
            if (id != uint.MaxValue)
                generateNew();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time global initialization
        protected override void initGlobal()
        {
            colorPicker = new myColorPicker(gl_Width, gl_Height, mode: myColorPicker.colorMode.IMAGE);
            list = new List<myObject>();

            // Global unmutable constants
            {
                N = rand.Next(10) + 10;

                cellSize = 10 + rand.Next(50);
                cellMargin = 1 + rand.Next(3);

                xOffset = gl_Width % (cellSize + cellMargin);
                xOffset = (cellSize + cellMargin - xOffset) / 2;

                yOffset = gl_Height % (cellSize + cellMargin);
                yOffset = (cellSize + cellMargin - yOffset) / 2;
            }

            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time local initialization
        private void initLocal()
        {
            doClearBuffer = false;

            renderDelay = rand.Next(3) + 1;

            maxA = 0.1f + myUtils.randFloat(rand) * 0.9f;

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            height = 600;

            string str = $"Obj = {Type}\n\n"                  +
                            myUtils.strCountOf(list.Count, N) +
                            $"cellSize = {cellSize}\n"        +
                            $"cellMargin = {cellMargin}\n"    +
                            $"maxA = {myUtils.fStr(maxA)}\n"  +
                            $"renderDelay = {renderDelay}\n"  +
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
            x = rand.Next(gl_Width  + 100) - 50;
            y = rand.Next(gl_Height + 100) - 50;

            x -= x % (cellSize + cellMargin);
            y -= y % (cellSize + cellMargin);

            x -= xOffset;
            y -= yOffset;

            cnt = 1;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            if (--cnt == 0)
            {
                Show();
                generateNew();
            }
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Show()
        {
            tex.setOpacity(myUtils.randFloat(rand) * maxA);
            tex.Draw(x, y, cellSize, cellSize, x, y, cellSize, cellSize);
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Process(Window window)
        {
            uint cnt = 0;
            initShapes();

            clearScreenSetup(doClearBuffer, 0.1f, true);

            grad.Draw();

            while (!Glfw.WindowShouldClose(window))
            {
                int Count = list.Count;

                processInput(window);

                // Swap fore/back framebuffers, and poll for operating system events.
                Glfw.SwapBuffers(window);
                Glfw.PollEvents();

                // Render Frame
                {
                    for (int i = 0; i != Count; i++)
                    {
                        (list[i] as myObj_1040).Move();
                    }
                }

                if (Count < N)
                {
                    list.Add(new myObj_1040());
                }

                cnt++;
                System.Threading.Thread.Sleep(renderDelay);

                if (cnt == 3000)
                {
                    cnt = 0;

                    colorPicker.reloadImage();
                    tex.reloadImg(colorPicker.getImg());
                }
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

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------
    }
};
