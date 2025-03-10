﻿using GLFW;
using static OpenGL.GL;
using System.Collections.Generic;


/*
    - Cellular Automaton: Conway's Life
*/


namespace my
{
    public class myObj_0150 : myObject
    {
        // Priority
        public static int Priority => 10;
		public static System.Type Type => typeof(myObj_0150);

        private int x, y, ix, jy;
        private float R, G, B;

        private bool alive = false;
        private int liveCnt = 0, lifeSpanCnt = 0;

        private static bool doUseRandBgr = false, doUseRandCellColor = false;
        private static int W = 0, H = 0, step = 0, startX = 0, startY = 0, drawMode = 0, lightMode = 0, clearMode = 0, populateMode = 0;
        private static int cellOffset = 0, a = 0, b = 0, c = 0, d = 0, drawW = 0, frameRate = 5;
        private static float borderR = 0, borderG = 0, borderB = 0, cellR = 0, cellG = 0, cellB = 0, colorStepR = 0, colorStepG = 0, colorStepB = 0;
        private static string extraInfo = "";

        static myTexRectangle tex = null;

        // ---------------------------------------------------------------------------------------------------------------

        // Default constructor; we need it because myObj_0Prioritizer won't work with the parametrized one
        public myObj_0150()
        {
        }

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_0150(int i = 0, int j = 0)
        {
            alive = false;

            liveCnt = -1;
            lifeSpanCnt = 0;

            x = i;
            y = j;

            generateNew();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time global initialization
        protected override void initGlobal()
        {
            colorPicker = new myColorPicker(gl_Width, gl_Height);
            list = new List<myObject>();

            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time local initialization
        private void initLocal()
        {
            if (myUtils.randomChance(rand, 4, 5))
            {
                step = rand.Next(33) + 25;
            }
            else
            {
                step = rand.Next(23) + 10;
            }

            cellOffset = rand.Next(4);
            frameRate = 1 + (myUtils.randomChance(rand, 2, 3) ? rand.Next(11) : rand.Next(33));

            // In case the colorPicker targets an image, drawMode could be 3
            drawMode = colorPicker.isImage() ? rand.Next(4) : rand.Next(3);     // Draw cells mode
            lightMode = rand.Next(2);                                           // Light (0) vs Dark (1) theme
            clearMode = rand.Next(2);                                           // The way dead cells behave

            doUseRandBgr = myUtils.randomBool(rand);                            // If true, the cells will be cleared with bgr color that is slightly randomly offset (where applicable)
            doUseRandCellColor = myUtils.randomBool(rand);                      // The same, but for the main cell color

            if (drawMode == 3)
                clearMode = rand.Next(5);

            // Drawing offsets
            {
                // These offsets give us a gap of 2 pixels on each side of the square;
                // This works fine in case the rect is not adjusted for a missing BL angle (float fx = 2.0f * x / (Width) - 1.0f;)
                a = 2 + cellOffset;
                b = 3 + cellOffset;
                c = 5 + cellOffset * 2;
                d = 5 + cellOffset * 2;
                drawW = step - c;
            }

            // Cell border color
            switch (lightMode)
            {
                case 0:
                    do
                    {
                        borderR = myUtils.randFloat(rand, 0.05f);
                        borderG = myUtils.randFloat(rand, 0.05f);
                        borderB = myUtils.randFloat(rand, 0.05f);
                    }
                    while (borderR + borderG + borderB > 0.5f);
                    break;

                case 1:
                    do
                    {
                        borderR = myUtils.randFloat(rand, 0.05f);
                        borderG = myUtils.randFloat(rand, 0.05f);
                        borderB = myUtils.randFloat(rand, 0.05f);
                    }
                    while (borderR + borderG + borderB < 0.5f);
                    break;
            }

            // Color changing steps
            if (myUtils.randomChance(rand, 1, 2))
            {
                colorStepR = myUtils.randFloat(rand, 0.1f) * 0.1f;
                colorStepG = myUtils.randFloat(rand, 0.1f) * 0.1f;
                colorStepB = myUtils.randFloat(rand, 0.1f) * 0.1f;
            }
            else
            {
                colorStepR = colorStepG = colorStepB = myUtils.randFloat(rand, 0.1f) * 0.1f;
            }

#if DEBUG
    #if false
            step = 60;
            drawMode = 1;
            clearMode = 1;
    #endif
#endif

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            height = 800;

            string colorSteps = (colorStepR == colorStepG && colorStepR == colorStepB) ? "The same" : "Different";

            string str = $"Obj = {Type} -- Conway's Life\n\n" 	+
                            $"N = {list.Count}\n" 				+
                            $"step = {step}\n" 					+
                            $"cellOffset = {cellOffset}\n" 		+
                            $"drawMode = {drawMode}\n" 			+
                            $"clearMode = {clearMode}\n" 		+
                            $"lightMode = {lightMode}\n" 		+
                            $"populateMode = {populateMode}\n" 	+
                            $"doUseRandBgr = {doUseRandBgr}\n" 	+
                            $"doUseRandCellColor = {doUseRandCellColor}\n" +
                            $"colorSteps: {colorSteps}\n" 		+
                            $"frameRate = {frameRate}\n" 		+
                            $"extraInfo: {extraInfo}\n" 		+
                            $"file: {colorPicker.GetFileName()}"
                ;
            return str;
        }

        // ---------------------------------------------------------------------------------------------------------------

        // 
        protected override void setNextMode()
        {
            var oldStep = step;

            initLocal();

            step = oldStep;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void generateNew()
        {
            ix = (x - startX) / step;
            jy = (y - startY) / step;

            R = G = B = -1;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            if (liveCnt == -1)
            {
                // 1st iteration: count neighbours
                liveCnt = alive ? -1 : 0;

                for (int i = ix - 1; i < ix + 2; i++)
                {
                    for (int j = jy - 1; j < jy + 2; j++)
                    {
                        var obj = getObj(i, j) as myObj_0150;

                        if (obj != null && obj.alive)
                        {
                            liveCnt++;
                        }
                    }
                }
            }
            else
            {
                // 2nd iteration: see if the cell stays alive
                if (alive)
                {
                    alive = false;

                    if (liveCnt == 2 || liveCnt == 3)
                    {
                        alive = true;
                        lifeSpanCnt++;
                    }
                }
                else
                {
                    if (liveCnt == 3)
                    {
                        alive = true;
                        lifeSpanCnt = 0;
                    }
                }

                liveCnt = -1;
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Show()
        {
            void drawBorder(bool setColor)
            {
                int drawX = x + a;
                int drawY = y + b;

                // Draw cell's border
                if (setColor)
                    myPrimitive._Rectangle.SetColor(R, G, B, 1);

                myPrimitive._Rectangle.Draw(drawX + 1, drawY, drawW - 1, drawW - 1, false);
            }

            void drawCell()
            {
                int drawX = x + a;
                int drawY = y + b;

                // Draw the cell
                myPrimitive._Rectangle.SetColor(R, G, B, 1);
                myPrimitive._Rectangle.Draw(drawX, drawY, drawW, drawW, true);

                // Draw cell's border
                myPrimitive._Rectangle.SetColor(borderR, borderG, borderB, 1);
                myPrimitive._Rectangle.Draw(drawX + 1, drawY, drawW - 1, drawW - 1, false);
            }

            void drawTex(float opacity, bool doErase)
            {
                int drawX = x + a;
                int drawY = y + b;

                if (doErase)
                {
                    setClearingColor();
                    myPrimitive._Rectangle.Draw(drawX, drawY, drawW + 1, drawW + 1, true);
                }

                tex.setOpacity(opacity);
                tex.Draw(drawX, drawY, drawW, drawW, drawX, drawY, drawW, drawW);
            }

            void setClearingColor()
            {
                if (doUseRandBgr)
                {
                    // Slightly offset clearing color
                    float r = myUtils.randFloat(rand) * 0.025f * myUtils.randomSign(rand);
                    float g = myUtils.randFloat(rand) * 0.025f * myUtils.randomSign(rand);
                    float b = myUtils.randFloat(rand) * 0.025f * myUtils.randomSign(rand);

                    myPrimitive._Rectangle.SetColor(bgrR + r, bgrG + g, bgrB + b, 1.0f);
                }
                else
                {
                    myPrimitive._Rectangle.SetColor(bgrR, bgrG, bgrB, 1.0f);
                }
            }

            void setCellColor()
            {
                R = cellR;
                G = cellG;
                B = cellB;

                // Slightly offset clearing color
                if (doUseRandCellColor)
                {
                    R += myUtils.randFloat(rand) * 0.025f * myUtils.randomSign(rand);
                    G += myUtils.randFloat(rand) * 0.025f * myUtils.randomSign(rand);
                    B += myUtils.randFloat(rand) * 0.025f * myUtils.randomSign(rand);
                }
            }

            if (alive)
            {
                switch (drawMode)
                {
                    // Single solid color (predefined) -- border
                    case 0:
                        setCellColor();
                        drawBorder(true);
                        break;

                    // Single solid color (predefined) -- full cell
                    case 1:
                        setCellColor();
                        drawCell();
                        break;

                    // Solid color from the colorPicker
                    case 2:
                        colorPicker.getColor(x, y, ref R, ref G, ref B);
                        setCellColor();
                        drawCell();
                        break;

                    // Image texture
                    case 3:
                        R = cellR;
                        G = cellG;
                        B = cellB;
                        drawTex(1.0f, false);
                        break;
                }
            }
            else
            {
                switch (clearMode)
                {
                    // Just clear the cell (once) with a background color
                    case 0:
                        if (R > 0)
                        {
                            R = -1;
                            setClearingColor();
                            myPrimitive._Rectangle.Draw(x, y + 1, step - 1, step - 1, true);
                        }
                        break;

                    // Gradually change cell's color until it blends with background
                    case 1:
                        if (R >= 0)
                        {
                            if (lightMode == 0)
                            {
                                if (R < bgrR)
                                    R += colorStepR;

                                if (G < bgrG)
                                    G += colorStepG;

                                if (B < bgrB)
                                    B += colorStepB;

                                // While any RGB component is darker than the bgr, draw the cell
                                if (R < bgrR || G < bgrG || B < bgrB)
                                {
                                    myPrimitive._Rectangle.SetColor(R, G, B, 1);
                                }
                                else
                                {
                                    R = -1;
                                    setClearingColor();
                                }
                            }
                            else
                            {
                                if (R > bgrR)
                                    R -= colorStepR;

                                if (G > bgrG)
                                    G -= colorStepG;

                                if (B > bgrB)
                                    B -= colorStepB;

                                // While any RGB component is lighter than the bgr, draw the cell
                                if (R > bgrR || G > bgrG || B > bgrB)
                                {
                                    myPrimitive._Rectangle.SetColor(R, G, B, 1);
                                }
                                else
                                {
                                    R = -1;
                                    setClearingColor();
                                }
                            }

                            if (drawMode == 0)
                            {
                                drawBorder(false);
                            }
                            else
                            {
                                myPrimitive._Rectangle.Draw(x + a, y + b, drawW, drawW, true);
                            }
                        }
                        break;

                    // Draw underlying image with low opacity (once) -- only for the tex modes
                    case 2:
                        if (R >= 0)
                        {
                            R = -1;
                            drawTex(0.25f, true);
                        }
                        break;

                    // Draw underlying image with gradually decreasing opacity -- only for the tex modes
                    case 3:
                    case 4:
                        if (R >= 0)
                        {
                            if (R < 10)
                            {
                                // First erase iteration only
                                R = 11;                         // Flag to enter this block only once
                                G = 0.05f;                      // Opacity step
                                B = 1.0f;                       // Opacity
                            }

                            B -= G;

                            if (B > 0)
                            {
                                drawTex(B, true);
                            }
                            else
                            {
                                // The only difference between modes 3 and 4: the texture will be removed completely
                                if (clearMode == 4)
                                    drawTex(0, true);
                                R = -1;
                            }
                        }
                        break;
                }
            }
            
            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Process(Window window)
        {
            int cnt = 0;

            initShapes();

            glDrawBuffer(GL_FRONT_AND_BACK);

            // Draw grid and create an object for every cell out there
            drawGrid();

            // Set some of the objects to be alive
            populate(window, ref populateMode);

            cnt = 123;

            while (!Glfw.WindowShouldClose(window) && cnt > 0)
            {
                processInput(window);
                Glfw.PollEvents();
                System.Threading.Thread.Sleep(10);
                cnt--;
            }

            while (!Glfw.WindowShouldClose(window))
            {
                cnt++;

                processInput(window);

                // Swap fore/back framebuffers, and poll for operating system events.
                Glfw.SwapBuffers(window);
                Glfw.PollEvents();

                // Render Frame
                if (cnt % frameRate == 0)
                {
                    for (int i = 0; i < list.Count; i++)
                    {
                        var obj = list[i] as myObj_0150;
                        obj.Move();
                    }

                    for (int i = 0; i < list.Count; i++)
                    {
                        var obj = list[i] as myObj_0150;

                        obj.Move();
                        obj.Show();
                    }
                }

                // Add some new random cells
                if (cnt > 500)
                {
                    cnt = 0;

                    for (int i = 0; i < rand.Next(100); i++)
                    {
                        var obj = list[rand.Next(list.Count)] as myObj_0150;

                        if (!obj.alive)
                            obj.alive = true;
                    }
                }

                System.Threading.Thread.Sleep(renderDelay);
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void initShapes()
        {
            myPrimitive.init_Line();
            myPrimitive.init_Rectangle();

            if (drawMode == 3)
            {
                tex = new myTexRectangle(colorPicker.getImg());
                tex.setAngle(0);
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        // Draw the grid and initilize all the static colors;
        // Normally, should be called only once
        private void drawGrid()
        {
            W = gl_Width  / step;
            H = gl_Height / step;

            startX = (gl_Width  % step) / 2;
            startY = (gl_Height % step) / 2;

            if (startX > 0)
            {
                startX -= step;
                W += 2;
            }

            if (startY > 0)
            {
                startY -= step;
                H += 2;
            }

            // Create an object for every cell out there
            for (int j = 0; j < step * H; j += step)
            {
                for (int i = 0; i < step * W; i += step)
                {
                    var obj = new myObj_0150(startX + i, startY + j);
                    list.Add(obj);
                }
            }

            if (lightMode == 0)
            {
                float lightFactor = myUtils.randFloat(rand) * 0.33f;

                bgrR = 1 - myUtils.randFloat(rand) * lightFactor;
                bgrG = 1 - myUtils.randFloat(rand) * lightFactor;
                bgrB = 1 - myUtils.randFloat(rand) * lightFactor;

                do
                {
                    cellR = myUtils.randFloat(rand);
                    cellG = myUtils.randFloat(rand);
                    cellB = myUtils.randFloat(rand);
                }
                while (cellR + cellG + cellB > bgrR + bgrG + bgrB);

                myPrimitive._Line.SetColor(0.75f, 0.75f, 0.75f, 1.0f);
            }
            else
            {
                float darkFactor = myUtils.randFloat(rand) * 0.33f;

                bgrR = myUtils.randFloat(rand) * darkFactor;
                bgrG = myUtils.randFloat(rand) * darkFactor;
                bgrB = myUtils.randFloat(rand) * darkFactor;

                do
                {
                    cellR = myUtils.randFloat(rand);
                    cellG = myUtils.randFloat(rand);
                    cellB = myUtils.randFloat(rand);
                }
                while (cellR + cellG + cellB < bgrR + bgrG + bgrB);

                myPrimitive._Line.SetColor(0.25f, 0.25f, 0.25f, 1.0f);
            }

            // Sometimes use random color for the grid
            if (myUtils.randomChance(rand, 1, 10))
            {
                float r = myUtils.randFloat(rand);
                float g = myUtils.randFloat(rand);
                float b = myUtils.randFloat(rand);

                myPrimitive._Line.SetColor(r, g, b, 1.0f);
            }

            myPrimitive._Rectangle.SetColor(bgrR, bgrG, bgrB, 1.0f);
            myPrimitive._Rectangle.Draw(0, 0, gl_Width, gl_Height, true);

            for (int i = startX; i <= gl_Width; i += step)
                myPrimitive._Line.Draw(i, 0, i, gl_Height);

            for (int i = startY; i <= gl_Height; i += step)
                myPrimitive._Line.Draw(0, i, gl_Width, i);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        // Set a number of cells to be alive
        private void populate(Window window, ref int populateMode)
        {
            myObj_0150 obj = null;

            populateMode = rand.Next(17);

            switch (populateMode)
            {
                // Fill the field with random cells
                case 0:
                    {
                        int Cnt = list.Count / 7;
                        Cnt = rand.Next(Cnt / 2) + Cnt;

                        for (int i = 0; i < Cnt; i++)
                        {
                            if (Glfw.WindowShouldClose(window))
                                break;

                            obj = list[rand.Next(list.Count)] as myObj_0150;

                            if (!obj.alive)
                            {
                                obj.alive = true;
                                obj.Show();
                            }

                            if (i % 3 == 0)
                            {
                                Glfw.SwapBuffers(window);
                                System.Threading.Thread.Sleep(3);
                            }

                            processInput(window);
                            Glfw.PollEvents();
                        }
                    }
                    break;

                // Fill a random rectangle with random cells
                case 1:
                    {
                        int xx = 3 + rand.Next(W / 2);
                        int yy = 3 + rand.Next(H / 2);
                        int ww = 3 + rand.Next(W / 2);
                        int hh = 3 + rand.Next(H / 2);

                        int Cnt = ww * hh / 3;

                        for (int i = 0; i < Cnt; i++)
                        {
                            if (Glfw.WindowShouldClose(window))
                                break;

                            obj = getObj(xx + rand.Next(ww), yy + rand.Next(hh)) as myObj_0150;

                            if (obj != null)
                            {
                                obj.alive = true;
                                obj.Show();

                                Glfw.SwapBuffers(window);
                                System.Threading.Thread.Sleep(3);
                            }

                            processInput(window);
                            Glfw.PollEvents();
                        }
                    }
                    break;

                // Fill the field with squares
                case 2:
                    {
                        int stepx = 3 + rand.Next(7);
                        int stepy = 3 + rand.Next(5);
                        int offsetMode = rand.Next(3);

                        for (int j = 1; j < H; j += stepy)
                        {
                            int i = 0;

                            switch (offsetMode)
                            {
                                case 0: i = 1; break;
                                case 1: i = j % 2 == 0 ? 1 : 2; break;
                                case 2: i = rand.Next(5); break;
                            }

                            for (; i < W; i += stepx)
                            {
                                if (Glfw.WindowShouldClose(window))
                                    return;

                                // Make a square
                                for (int k = i; k < i + 2; k++)
                                {
                                    for (int n = j; n < j + 2; n++)
                                    {
                                        obj = getObj(k, n) as myObj_0150;

                                        if (obj != null)
                                        {
                                            obj.alive = true;
                                            obj.Show();
                                        }
                                    }
                                }

                                Glfw.SwapBuffers(window);
                                Glfw.PollEvents();
                                processInput(window);
                            }
                        }
                    }
                    break;

                // Fill the field with solid horizontal lines
                case 3:
                    {
                        int stepy = rand.Next(6);

                        for (int j = 1; j < H; j += stepy > 0 ? stepy : rand.Next(5) + 1)
                        {
                            for (int i = 0; i < W; i++)
                            {
                                obj = getObj(i, j) as myObj_0150;

                                if (obj != null)
                                {
                                    obj.alive = true;
                                    obj.Show();
                                }
                            }

                            Glfw.SwapBuffers(window);
                        }
                    }
                    break;

                // Fill the field with breaking horizontal lines
                case 4:
                    {
                        int stepy = rand.Next(6);

                        for (int j = 1; j < H; j += stepy > 0 ? stepy : rand.Next(5) + 1)
                        {
                            for (int i = 1; i < W; i++)
                            {
                                for (int k = i; k < rand.Next(13) + i; i++, k++)
                                {
                                    if (k == W)
                                        break;

                                    obj = getObj(k, j) as myObj_0150;

                                    if (obj != null)
                                    {
                                        obj.alive = true;
                                        obj.Show();
                                    }
                                }

                                Glfw.SwapBuffers(window);
                            }
                        }
                    }
                    break;

                // Draw random rectangle
                case 5:
                    {
                        int xx = 3 + rand.Next(W / 2);
                        int yy = 3 + rand.Next(H / 2);
                        int ww = 3 + rand.Next(W / 2);
                        int hh = 3 + rand.Next(H / 2);

                        for (int i = 0; i < ww; i++)
                        {
                            obj = getObj(xx + i, yy) as myObj_0150;

                            if (obj != null)
                            {
                                obj.alive = true;
                                obj.Show();
                            }

                            obj = getObj(xx + i, yy + hh) as myObj_0150;

                            if (obj != null)
                            {
                                obj.alive = true;
                                obj.Show();
                            }

                            Glfw.SwapBuffers(window);
                        }

                        System.Threading.Thread.Sleep(333);

                        for (int j = 0; j < hh; j++)
                        {
                            obj = getObj(xx, yy + j) as myObj_0150;

                            if (obj != null)
                            {
                                obj.alive = true;
                                obj.Show();
                            }

                            obj = getObj(xx + ww - 1, yy + j) as myObj_0150;

                            if (obj != null)
                            {
                                obj.alive = true;
                                obj.Show();
                            }

                            Glfw.SwapBuffers(window);
                        }
                    }
                    break;

                // Draw 3-dot lines (to become crosses)
                case 6:
                    {
                        bool isRnd = myUtils.randomBool(rand);

                        int stepx = 4 + rand.Next(3);
                        int stepy = 4 + rand.Next(3);

                        void drawH(int i, int j)
                        {
                            for (int k = 0; k < 3; k++)
                            {
                                obj = getObj(k + i, j) as myObj_0150;

                                if (obj != null)
                                {
                                    obj.alive = true;
                                    obj.Show();
                                }
                            }
                        }

                        void drawV(int i, int j)
                        {
                            for (int k = 0; k < 3; k++)
                            {
                                obj = getObj(i + 1, k + j - 1) as myObj_0150;

                                if (obj != null)
                                {
                                    obj.alive = true;
                                    obj.Show();
                                }
                            }
                        }

                        for (int j = stepy/2; j < H; j += stepy)
                        {
                            for (int i = stepx/2; i < W; i += stepx)
                            {
                                // Draw cross
                                if (isRnd)
                                {
                                    if (myUtils.randomBool(rand))
                                        drawH(i, j);
                                    else
                                        drawV(i, j);
                                }
                                else
                                {
                                    drawH(i, j);
                                }

                                Glfw.SwapBuffers(window);
                            }
                        }
                    }
                    break;

                // Draw single horizontal line
                case 7:
                    {
                        int off = 3 + rand.Next(W/3);

                        for (int i = off; i < W - off; i++)
                        {
                            obj = getObj(i, H/2) as myObj_0150;
                            obj.alive = true;
                            obj.Show();
                            Glfw.SwapBuffers(window);
                        }
                    }
                    break;

                // Draw criss-cross lines
                case 8:
                    {
                        bool bothWays = myUtils.randomBool(rand);
                        int i = 0;

                        for (int j = 0; j < H; j++)
                        {
                            obj = getObj(i, j) as myObj_0150;
                            obj.alive = true;
                            obj.Show();

                            obj = getObj(W - i - 1, j) as myObj_0150;
                            obj.alive = true;
                            obj.Show();

                            if (bothWays)
                            {
                                obj = getObj(i, H - j - 1) as myObj_0150;
                                obj.alive = true;
                                obj.Show();

                                obj = getObj(W - i - 1, H - j - 1) as myObj_0150;
                                obj.alive = true;
                                obj.Show();
                            }

                            Glfw.SwapBuffers(window);
                            i++;
                        }
                    }
                    break;

                // Draw pulsars at random positions
                case 9:
                    {
                        void drawPulsar(int x, int y)
                        {
                            int[] arr = { 4, 7, 5, 7, 6, 7, 7, 6, 7, 5, 7, 4, 4, 2, 5, 2, 6, 2, 2, 4, 2, 5, 2, 6 };

                            for (int i = 0; i < arr.Length; i += 2)
                            {
                                Put(x + arr[i], y + arr[i + 1]);
                                Put(x + 8 + (8 - arr[i]), y + arr[i + 1]);
                                Put(x + arr[i], y + 7 + (9 - arr[i + 1]));
                                Put(x + 8 + (8 - arr[i]), y + 7 + (9 - arr[i + 1]));
                            }

                            Glfw.SwapBuffers(window);
                        }

                        int n = 1 + rand.Next(10);
                        extraInfo = $"pulsars qty = {n}";

                        for (int i = 0; i < n; i++)
                        {
                            int x = rand.Next(W);
                            int y = rand.Next(H);

                            drawPulsar(x, y);
                        }
                    }
                    break;

                // Draw random short lines
                case 10:
                    {
                        void drawLine(int x, int y, int size)
                        {
                            for (int i = 0; i < size; i++)
                                Put(x + i, y);
                        }

                        for (int i = 0; i < rand.Next(50) + 3; i++)
                        {
                            drawLine(rand.Next(W), rand.Next(H), rand.Next(9) + 2);
                            Glfw.SwapBuffers(window);
                        }

                        System.Threading.Thread.Sleep(500);
                    }
                    break;

                // Draw random doughnuts of random size
                case 11:
                    {
                        void drawDoughnut(int x, int y, int size)
                        {
                            for (int i = 0; i < size; i++)
                            {
                                Put(x + i, y);
                                Put(x + i, y + size + 1);
                            }

                            for (int i = 0; i < size; i++)
                            {
                                Put(x + size, y + i + 1);
                                Put(x - 1, y + i + 1);
                            }

                            Glfw.SwapBuffers(window);
                        }

                        for (int i = 0; i < rand.Next(50) + 5; i++)
                            drawDoughnut(rand.Next(W), rand.Next(H), rand.Next(9) + 3);

                        System.Threading.Thread.Sleep(500);
                    }
                    break;

                // Draw random doughnuts of the same size
                case 12:
                    {
                        void drawDoughnut(int x, int y, int size)
                        {
                            for (int i = 0; i < size; i++)
                            {
                                Put(x + i, y);
                                Put(x + i, y + size + 1);
                            }

                            for (int i = 0; i < size; i++)
                            {
                                Put(x + size, y + i + 1);
                                Put(x - 1, y + i + 1);
                            }

                            Glfw.SwapBuffers(window);
                        }

                        int Size = rand.Next(5) + 1;

                        for (int i = 0; i < rand.Next(35) + 15; i++)
                            drawDoughnut(rand.Next(W), rand.Next(H), Size);

                        System.Threading.Thread.Sleep(500);
                    }
                    break;

                // Draw R-Pentomino
                case 13:
                    {
                        void drawRPentomino(int x, int y)
                        {
                            int[] arr = { 0, 1, 1, 0, 1, 1, 1, 2, 2, 0 };

                            for (int i = 0; i < arr.Length; i += 2)
                            {
                                Put(x + arr[i], y + arr[i+1]);
                            }

                            Glfw.SwapBuffers(window);
                        }

                        drawRPentomino(2*W/3, H/2);
                        System.Threading.Thread.Sleep(500);
                    }
                    break;

                // Draw Diehard
                case 14:
                    {
                        void drawDieHard(int x, int y)
                        {
                            int[] arr = { 0, 3, 1, 3, 1, 4, 5, 4, 6, 4, 7, 4, 6, 2 };

                            for (int i = 0; i < arr.Length; i += 2)
                                Put(x + arr[i], y + arr[i + 1]);

                            Glfw.SwapBuffers(window);
                        }

                        for (int i = 0; i < rand.Next(11) + 1; i++)
                            drawDieHard(rand.Next(W), rand.Next(H));

                        System.Threading.Thread.Sleep(500);
                    }
                    break;

                // Draw Acorn
                case 15:
                    {
                        void drawAcorn(int x, int y)
                        {
                            int[] arr = { 0, 2, 1, 0, 1, 2, 3, 1, 4, 2, 5, 2, 6, 2 };

                            for (int i = 0; i < arr.Length; i += 2)
                                Put(x + arr[i], y + arr[i + 1]);

                            Glfw.SwapBuffers(window);
                        }

                        drawAcorn(W/2, H/2);
                        System.Threading.Thread.Sleep(500);
                    }
                    break;

                // Draw Gosper Glider Gun
                case 16:
                    {
                        void drawGosperGliderGun(int x, int y)
                        {
                            int[] arr = { 1, 4, 2, 4, 1, 5, 2, 5, 11, 4, 11, 5, 11, 6, 12, 3, 12, 7, 13, 2, 13, 8, 14, 2,
                                          14, 8, 15, 5, 16, 3, 16, 7, 17, 4, 17, 5, 17, 6, 18, 5,
                                          21, 2, 21, 3, 21, 4, 22, 2, 22, 3, 22, 4, 23, 1, 23, 5, 25, 1, 25, 0, 25, 5, 25, 6,
                                          35, 2, 35, 3, 36, 2, 36, 3
                            };

                            for (int i = 0; i < arr.Length; i += 2)
                                Put(x + arr[i], y + arr[i + 1]);

                            Glfw.SwapBuffers(window);
                        }

                        drawGosperGliderGun(W/5, H/4);
                        System.Threading.Thread.Sleep(500);
                    }
                    break;
            }
        }

        // ---------------------------------------------------------------------------------------------------------------

        // Get linear index of an object in the list and return this object
        private myObject getObj(int i, int j)
        {
            if (i < 0 || j < 0 || i >= W || j >= H)
                return null;

            int index = j * W + i;

            if (index >= 0 && index < list.Count)
            {
                return list[index];
            }

            return null;
        }

        // ---------------------------------------------------------------------------------------------------------------

        // Make the cell alive and draw it
        private void Put(int x, int y)
        {
            var obj = getObj(x, y) as myObj_0150;

            if (obj != null)
            {
                obj.alive = true;
                obj.Show();
            }
        }

        // ---------------------------------------------------------------------------------------------------------------
    }
};
