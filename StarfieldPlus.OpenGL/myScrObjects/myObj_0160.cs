using GLFW;
using static OpenGL.GL;
using System.Collections.Generic;


/*
    - Desktop: Ever fading away pieces

    todo:
        - compare it to the original
        - see if using dimrate with higher dimAlpha is any different from not using it, but the dimAlpha is lower
*/


namespace my
{
    public class myObj_0160 : myObject
    {
        // Priority
        public static int Priority => 10;
		public static System.Type Type => typeof(myObj_0160);

        private int x, y;
        private float A, R, G, B;

        private static int N = 0;

        private int lifeCnt = 0;
        private bool doDraw = false;

        private static int drawMode = 0, moveMode = 0, size = 0, dimRate = 0;
        private static int step = 0, startX = 0, startY = 0, cellMargin = 0;
        private static bool doUseCells = false, doDrawCellBorder = false, doUseExtraDim = true;
        private static float dimAlpha = 0.025f;

        static myTexRectangle tex = null;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_0160()
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
                N = 666;
            }

            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time local initialization
        private void initLocal()
        {
            doClearBuffer = myUtils.randomBool(rand);
            doUseExtraDim = myUtils.randomBool(rand);

            // Solid color ('0') or Solid color Border ('1') is the default mode
            drawMode = rand.Next(2);

            // But when colorPicker has an image, the mode is set to '1' with the probability of 2/3
            if (colorPicker.getMode() < 2)
                if (rand.Next(3) > 0)
                    drawMode = 2;

            renderDelay = rand.Next(20) + 1;
            moveMode = rand.Next(2);

            if (myUtils.randomChance(rand, 1, 3))
            {
                size = rand.Next(66) + 5;
            }
            else
            {
                size = rand.Next(66) + 25;
            }

            dimRate = rand.Next(11) + 2;
            doDrawCellBorder = myUtils.randomBool(rand);

            // Grid-based set-up
            {
                doUseCells = myUtils.randomChance(rand, 1, 3);
                step = 50 + rand.Next(151);
                startX = (gl_Width  % step) / 2;
                startY = (gl_Height % step) / 2;

                // Distance between cells in a grid-based mode
                cellMargin = doUseCells ? rand.Next(25) + 1 : 1;
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            height = 600;

            string str = $"Obj = {Type}\n\n"                         	+
                            $"N = {list.Count} of {N}\n"                +
                            $"size = {size}\n"                          +
                            $"moveMode = {moveMode}\n"                  +
                            $"drawMode = {drawMode}\n"                  +
                            $"doClearBuffer = {doClearBuffer}\n"        +
                            $"doUseCells = {doUseCells}\n"              +
                            $"doUseExtraDim = {doUseExtraDim}\n"        +
                            $"doDrawCellBorder = {doDrawCellBorder}\n"  +
                            $"renderDelay = {renderDelay}\n"            +
                            $"dimRate = {dimRate}\n"                    +
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
            if (moveMode == 1)
            {
                size = rand.Next(66) + 5;
            }

            lifeCnt = rand.Next(100) + 100;
            doDraw = true;

            x = rand.Next(gl_Width);
            y = rand.Next(gl_Height);

            if (doUseCells)
            {
                size = step;

                if (x >= startX)
                {
                    x -= startX;
                    x -= x % step;
                    x += startX;
                }
                else
                {
                    x = startX - step;
                }

                if (y >= startY)
                {
                    y -= startY;
                    y -= y % step;
                    y += startY;
                }
                else
                {
                    y = startY - step;
                }
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            if (--lifeCnt == 0)
            {
                generateNew();
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Show()
        {
            int Z = size / 2;
            int X = x - Z;
            int Y = y - Z;
            Z = size - cellMargin;

            // Draw only once per cell's life time
            if (doDraw)
            {
                doDraw = false;

                switch (drawMode)
                {
                    // Solid color from color picker
                    case 0:
                        colorPicker.getColor(x, y, ref R, ref G, ref B);
                        A = myUtils.randFloat(rand, 0.3f) * 0.5f;

                        myPrimitive._Rectangle.SetColor(R, G, B, A);
                        myPrimitive._Rectangle.Draw(X, Y, Z, Z, true);

                        if (doDrawCellBorder)
                        {
                            myPrimitive._Rectangle.SetColor(R, G, B, A*2);
                            myPrimitive._Rectangle.Draw(X, Y, Z+1, Z, false);
                        }
                        break;

                    case 1:
                        colorPicker.getColor(x, y, ref R, ref G, ref B);
                        A = myUtils.randFloat(rand, 0.3f);

                        myPrimitive._Rectangle.SetColor(R, G, B, A);
                        myPrimitive._Rectangle.Draw(X, Y, Z, Z, false);
                        break;

                    // Piece of an image
                    case 2:
                        A = myUtils.randFloat(rand, 0.3f);

                        tex.setOpacity(A);
                        tex.Draw(X, Y, Z, Z, X, Y, Z, Z);

                        if (doDrawCellBorder)
                        {
                            colorPicker.getColor(x, y, ref R, ref G, ref B);

                            myPrimitive._Rectangle.SetColor(R, G, B, A*2);
                            myPrimitive._Rectangle.Draw(X, Y, Z, Z, false);
                        }
                        break;
                }
            }
            else
            {
                if (doUseExtraDim)
                {
                    X--;
                    Y--;
                    Z += 2;
                    myPrimitive._Rectangle.SetColor(0, 0, 0, 0.05f);
                    myPrimitive._Rectangle.Draw(X, Y, Z, Z, true);
                }
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Process(Window window)
        {
            uint cnt = 0;
            initShapes();

            // Disable VSYNC if needed
            // Glfw.SwapInterval(0);

            dimScreenRGB_SetRandom(0.1f);
            glDrawBuffer(GL_BACK);
            glDrawBuffer(GL_FRONT_AND_BACK);

            //clearScreenSetup(doClearBuffer, 0.1f);

            while (!Glfw.WindowShouldClose(window))
            {
                processInput(window);

                // Swap fore/back framebuffers, and poll for operating system events.
                Glfw.SwapBuffers(window);
                Glfw.PollEvents();

                // Dim screen
                //if (cnt % dimRate == 0)
                {
                    dimScreen(dimAlpha);
                }

                // Render Frame
                {
                    for (int i = 0; i != list.Count; i++)
                    {
                        var obj = list[i] as myObj_0160;

                        obj.Show();
                        obj.Move();
                    }
                }

                if (list.Count < N)
                {
                    list.Add(new myObj_0160());
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
            myPrimitive.init_Rectangle();

            if (colorPicker.getMode() < 2)
            {
                tex = new myTexRectangle(colorPicker.getImg());
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------
    }
};
