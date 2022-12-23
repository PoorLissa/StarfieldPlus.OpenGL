using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;


/*
    - Desktop: Diminishing pieces

    Variations:
        - grid-based
        - leaving traces
*/


namespace my
{
    public class myObj_170 : myObject
    {
        private int x, y, lifeCnt;
        private float size, dSize, A, R, G, B;
        bool doDraw = false, isSizeChanged = false;

        private static int N = 0, shape = 0, angle = 0, drawMode = 0, eraseMode = 0, t = 0, maxSize = 66;
        private static int cellSize = 0, startX = 0, startY = 0;
        private static bool doLeaveTrace = false, doUseCells = false;
        private static float largeDSize = 0.0f, eraseOpacity = 0;

        private static myTexRectangle tex = null;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_170()
        {
            generateNew();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time global initialization
        protected override void initGlobal()
        {
            colorPicker = new myColorPicker(gl_Width, gl_Height);//, mode: myColorPicker.colorMode.SNAPSHOT_OR_IMAGE);
            list = new List<myObject>();

            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------


        // One-time local initialization
        private void initLocal()
        {
            doClearBuffer = false;
            N = 666;

            // Grid-based set-up
            {
                doUseCells = myUtils.randomChance(rand, 1, 2);
                cellSize = 50 + rand.Next(151);
                startX = (gl_Width  % cellSize) / 2;
                startY = (gl_Height % cellSize) / 2;
            }

            t = rand.Next(50) + 1;

            // Solid color is the default mode ('0')
            drawMode = 0;

            // But when colorPicker has an image, the mode is set to '1' with the probability of 2/3
            if (colorPicker.getMode() < 2)
                if (rand.Next(3) > 0)
                    drawMode = 2;

            doLeaveTrace = rand.Next(2) == 0;

            // With the probability of 1/5 we'll have static dSize > 1.0 (which will cause it to leave concentric traces)
            {
                largeDSize = rand.Next(5);

                if (largeDSize == 0.0f)
                {
                    largeDSize = 1.0f + myUtils.randFloat(rand) + myUtils.randFloat(rand) * 0.25f;
                    t += 13;
                }
                else
                {
                    largeDSize = 0.0f;
                }
            }

            switch (rand.Next(3))
            {
                case 0: maxSize = 066; break;
                case 1: maxSize = 111; break;
                case 2: maxSize = 166; break;
            }

            eraseMode = rand.Next(3);
            eraseOpacity = 0.5f + myUtils.randFloat(rand)/2;

#if false
            drawMode = 0;
            doLeaveTrace = false;
            largeDSize = 0.0f;
            doUseCells = true;
#endif

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            height = 800;

            string str = $"Obj = myObj_170\n\n" +
                            $"N = {list.Count} of {N}\n" +
                            $"drawMode = {drawMode}\n" +
                            $"eraseMode = {eraseMode}\n" +
                            $"eraseOpacity = {eraseOpacity}\n" +
                            $"file: {colorPicker.GetFileName()}" +
                            $""
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
            lifeCnt = rand.Next(123) + 13;
            doDraw = true;
            isSizeChanged = false;

            int iSize = rand.Next(maxSize) + 5;

            dSize = (largeDSize == 0.0f) ? myUtils.randFloat(rand) : largeDSize;

            if (doUseCells)
            {
                iSize = cellSize / 2;

                x = rand.Next(gl_Width);
                y = rand.Next(gl_Height);

                if (x >= startX)
                {
                    x -= startX;
                    x -= x % cellSize;
                    x += startX;
                }
                else
                {
                    x = startX - cellSize;
                }

                if (y >= startY)
                {
                    y -= startY;
                    y -= y % cellSize;
                    y += startY;
                }
                else
                {
                    y = startY - cellSize;
                }
            }
            else
            {
                x = iSize + rand.Next(gl_Width  - 2 * iSize);
                y = iSize + rand.Next(gl_Height - 2 * iSize);
            }

            x -= iSize;
            y -= iSize;

            size = iSize;

            colorPicker.getColor(x, y, ref R, ref G, ref B);
            A = myUtils.randFloat(rand);
            A += A < 0.25f ? 0.2f : 0.0f;

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            if (size > 0)
            {
                int iSize = (int)size;

                size -= dSize;

                // To draw the black rectangle only once
                isSizeChanged = (int)(size) != iSize;
            }
            else
            {
                if (--lifeCnt == 0)
                    generateNew();
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Show()
        {
            int iSize = (int)size;
            int X = x - iSize;
            int Y = y - iSize;
            int W = 2 * iSize;

            // Draw the whole shape only once per cell's life time
            if (doDraw)
            {
                doDraw = false;

                switch (drawMode)
                {
                    // Solid color from color picker
                    case 0:
                        myPrimitive._Rectangle.SetColor(R, G, B, A);
                        myPrimitive._Rectangle.Draw(X, Y, W, W, true);

                        myPrimitive._Rectangle.SetColor(R, G, B, 1);
                        myPrimitive._Rectangle.Draw(X, Y, W, W, false);
                        break;

                    // Solid color from color picker, draw only outline
                    case 1:
                        myPrimitive._Rectangle.SetColor(R, G, B, A);
                        myPrimitive._Rectangle.Draw(X, Y, W, W, false);
                        break;

                    // Piece of an image
                    case 2:
                        tex.Draw(X, Y, W, W, X, Y, W, W);
                        break;
                }

                if (!doLeaveTrace)
                    size++;
            }
            else
            {
                if (size >= 0 && isSizeChanged)
                {
                    if (size >= 1)
                    {
                        switch (drawMode)
                        {
                            case 0:
                                EraseInst(X-1, Y-1, W+2);
                                break;

                            case 1:
                                myPrimitive._Rectangle.SetColor(0, 0, 0, 1);
                                myPrimitive._Rectangle.Draw(X, Y, W, W, false);

                                myPrimitive._Rectangle.SetColor(R, G, B, A);
                                myPrimitive._Rectangle.Draw(X + 1, Y + 1, W - 2, W - 2, false);
                                break;

                            case 2:
                                EraseInst(X-1, Y-1, W+2);
                                break;
                        }
                    }
                    else
                    {
                        EraseInst(X, Y, W, false);
                    }
                }
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void EraseInst(int X, int Y, int W, bool lines = true)
        {
            if (lines)
            {
                float erOp = 1;

                switch (eraseMode)
                {
                    case 0:
                        erOp = 1.0f;
                        break;

                    case 1:
                        erOp = myUtils.randFloat(rand);
                        break;

                    case 2:
                        erOp  = eraseOpacity;
                        break;
                }

                // hor top
                myPrimitive._LineInst.setInstanceCoords(X, Y, X + W, Y);
                myPrimitive._LineInst.setInstanceColor(0, 0, 0, erOp);

                // vert right
                myPrimitive._LineInst.setInstanceCoords(X + W, Y, X + W, Y + W);
                myPrimitive._LineInst.setInstanceColor(0, 0, 0, erOp);

                // hor bottom
                myPrimitive._LineInst.setInstanceCoords(X, Y + W, X + W, Y + W);
                myPrimitive._LineInst.setInstanceColor(0, 0, 0, erOp);

                // vert left
                myPrimitive._LineInst.setInstanceCoords(X, Y + W + 1, X, Y);
                myPrimitive._LineInst.setInstanceColor(0, 0, 0, erOp);
            }
            else
            {
                myPrimitive._Rectangle.SetColor(0, 0, 0, 1);
                myPrimitive._Rectangle.Draw(X - 2, Y - 1, 3, 3, true);
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void Erase(int X, int Y, int W)
        {
            switch (eraseMode)
            {
                case 0:
                    myPrimitive._Rectangle.SetColor(0, 0, 0, 1.0f);
                    break;

                case 1:
                    myPrimitive._Rectangle.SetColor(0, 0, 0, myUtils.randFloat(rand));
                    break;

                case 2:
                    myPrimitive._Rectangle.SetColor(0, 0, 0, eraseOpacity);
                    break;
            }

            myPrimitive._Rectangle.Draw(X, Y, W, W, false);

            // Additional fix for a missing corner
            myPrimitive._Rectangle.Draw(X, Y + W, 1, 1, false);

            if (true)
            {
                myPrimitive._Rectangle.Draw(X - 1, Y - 1, W + 2, W + 2, false);

                myPrimitive._Rectangle.SetColor(R, G, B, A / 2);
                myPrimitive._Rectangle.Draw(X, Y, W, W, false);
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Process(Window window)
        {
            uint cnt = 0;
            initShapes();

            // Disable VSYNC if needed
            Glfw.SwapInterval(0);

            glDrawBuffer(GL_FRONT_AND_BACK);

            while (!Glfw.WindowShouldClose(window))
            {
                cnt++;

                processInput(window);

                // Swap fore/back framebuffers, and poll for operating system events.
                Glfw.SwapBuffers(window);
                Glfw.PollEvents();

                // Render Frame
                {
                    myPrimitive._LineInst.ResetBuffer();
                    myPrimitive._RectangleInst.ResetBuffer();

                    for (int i = 0; i < list.Count; i++)
                    {
                        var obj = list[i] as myObj_170;

                        obj.Show();
                        obj.Move();
                    }

                    //Erase(1333, 1000, 333);

                    myPrimitive._LineInst.Draw();
                    myPrimitive._RectangleInst.Draw(true);
                }

                if (list.Count < N)
                {
                    list.Add(new myObj_170());
                }

                System.Threading.Thread.Sleep(renderDelay);
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void initShapes()
        {
            myPrimitive.init_Rectangle();
            base.initShapes(shape, N, 0);

            myPrimitive.init_LineInst(N*8);
            myPrimitive.init_RectangleInst(N);

            if (drawMode == 2)
            {
                tex = new myTexRectangle(colorPicker.getImg());
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------
    }
};
