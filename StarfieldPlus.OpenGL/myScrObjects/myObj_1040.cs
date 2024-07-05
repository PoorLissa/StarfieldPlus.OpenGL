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
        public static int Priority => 10;
		public static System.Type Type => typeof(myObj_1040);

        private int x, y, cnt;

        private static float maxA = 1;
        private static int N = 0, cellSize = 1, cellMargin = 0, xOffset = 0, yOffset = 0, mode = 0, cntThreshold = 1, gl_cnt = 0;
        private static bool doChangeCellSize = false;

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

                setUpCellSize();
            }

            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time local initialization
        private void initLocal()
        {
            doClearBuffer = false;
            doChangeCellSize = myUtils.randomBool(rand);

            renderDelay = rand.Next(3) + 1;

            maxA = 0.1f + myUtils.randFloat(rand) * 0.9f;

            mode = rand.Next(5) == 0 ? 1 : 0;

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            height = 600;

            string str = $"Obj = {Type}\n\n"                           +
                            myUtils.strCountOf(list.Count, N)          +
                            $"mode = {mode}\n"                         +
                            $"cellSize = {cellSize}\n"                 +
                            $"cellMargin = {cellMargin}\n"             +
                            $"maxA = {myUtils.fStr(maxA)}\n"           +
                            $"doChangeCellSize = {doChangeCellSize}\n" +
                            $"gl_cnt = {gl_cnt}\n"                     +
                            $"cntThreshold = {cntThreshold}\n"         +
                            $"renderDelay = {renderDelay}\n"           +
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
            cnt = 1;

            x = rand.Next(gl_Width  + 100) - 50;
            y = rand.Next(gl_Height + 100) - 50;

            switch (mode)
            {
                case 0:
                    x -= x % (cellSize + cellMargin);
                    y -= y % (cellSize + cellMargin);

                    x -= xOffset;
                    y -= yOffset;
                    break;

                case 1:
                    break;
            }
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

                System.Threading.Thread.Sleep(renderDelay);

                if (++gl_cnt == cntThreshold)
                {
                    gl_cnt = 0;

                    colorPicker.reloadImage();
                    tex.reloadImg(colorPicker.getImg());

                    if (doChangeCellSize)
                    {
                        setUpCellSize();
                    }
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

        private void setUpCellSize()
        {
            int sizeMin = 10;
            int sizeMax = 61;
            int nMin = 6000;
            int nMax = 3000;

            cellSize = sizeMin + rand.Next(sizeMax - sizeMin);
            cellMargin = 1 + rand.Next(3);

            xOffset = gl_Width % (cellSize + cellMargin);
            xOffset = (cellSize + cellMargin - xOffset) / 2;

            yOffset = gl_Height % (cellSize + cellMargin);
            yOffset = (cellSize + cellMargin - yOffset) / 2;

            // Linear interpolation
            int a = (nMax - nMin) / (sizeMax - sizeMin);
            int b = nMin - a * sizeMin;

            cntThreshold = a * cellSize + b;

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------
    }
};
