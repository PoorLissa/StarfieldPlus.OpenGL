using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;


/*
    - Cellular Automaton: Conway's Life
*/


namespace my
{
    public class myObj_150 : myObject
    {
        private int x, y;
        private float R, G, B;

        private bool alive = false;
        private int liveCnt = 0, lifeSpanCnt = 0;

        private static int N = 0, step = 0, startX = 0, startY = 0, drawMode = 0, lightMode = 0, clearMode = 0, cellOffset = 0, a = 0, b = 0, c = 0, d = 0, drawW = 0;
        private static float bgrR = 0, bgrG = 0, bgrB = 0, borderR = 0, borderG = 0, borderB = 0, cellR = 0, cellG = 0, cellB = 0, colorStepR = 0, colorStepG = 0, colorStepB = 0;

        static myTexRectangle tex = null;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_150()
        {
            alive = false;

            liveCnt = -1;
            lifeSpanCnt = 0;

            R = G = B = -1;
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
            N = 0;
            step = rand.Next(33) + 25;
            cellOffset = rand.Next(4);

            // In case the colorPicker targets an image, drawMode could be 3
            drawMode = colorPicker.getMode() < 2 ? rand.Next(4) : rand.Next(3);     // Draw cells mode
            lightMode = rand.Next(2);                                               // Light (0) vs Dark (1) theme
            clearMode = rand.Next(2);                                               // The way dead cells behave

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

            string str = $"Obj = myObj_150 -- Conway's Life\n\n" +
                            $"N = {list.Count}\n" +
                            $"step = {step}\n" +
                            $"cellOffset = {cellOffset}\n" +
                            $"drawMode = {drawMode}\n" +
                            $"clearMode = {clearMode}\n" +
                            $"lightMode = {lightMode}\n" +
                            $"colorSteps: {colorSteps}\n" +
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
            x = rand.Next(gl_Width);
            y = rand.Next(gl_Height);

            x += startX - (x % step);
            y += startY - (y % step);

            R = -1;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            if (liveCnt == -1)
            {
                // 1st iteration: observe neighbours
                liveCnt = alive ? -1 : 0;
                int x = -1, y = -1;

                getCoords(ref x, ref y);

                for (int i = x - 1; i < x + 2; i++)
                {
                    for (int j = y - 1; j < y + 2; j++)
                    {
                        var obj = getObj(i, j) as myObj_150;

                        if (obj != null && obj.alive)
                        {
                            liveCnt++;
                        }
                    }
                }
            }
            else
            {
                // 2nd iteration: make life decision
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
                    myPrimitive._Rectangle.SetColor(bgrR, bgrG, bgrB, 1.0f);
                    myPrimitive._Rectangle.Draw(drawX, drawY, drawW + 1, drawW + 1, true);
                }

                tex.setOpacity(opacity);
                tex.Draw(drawX, drawY, drawW, drawW, drawX, drawY, drawW, drawW);
            }

            if (alive)
            {
                switch (drawMode)
                {
                    // Single solid color (predefined) -- border
                    case 0:
                        R = cellR;
                        G = cellG;
                        B = cellB;
                        drawBorder(true);
                        break;

                    // Single solid color (predefined) -- full cell
                    case 1:
                        R = cellR;
                        G = cellG;
                        B = cellB;
                        drawCell();
                        break;

                    // Solid color from the colorPicker
                    case 2:
                        colorPicker.getColor(x, y, ref R, ref G, ref B);
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
                            myPrimitive._Rectangle.SetColor(bgrR, bgrG, bgrB, 1.0f);
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
                                    myPrimitive._Rectangle.SetColor(bgrR, bgrG, bgrB, 1.0f);
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
                                    myPrimitive._Rectangle.SetColor(bgrR, bgrG, bgrB, 1.0f);
                                }
                            }

                            if (drawMode == 0)
                            {
                                drawBorder(false);
                            }
                            else
                            {
                                myPrimitive._Rectangle.Draw(x + a, y + b, step - c + 1, step - d + 1, true);
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
            int t = 500, cnt = 0;
            int w = 1 + gl_Width  / step;
            int h = 1 + gl_Height / step;

            initShapes();

            glDrawBuffer(GL_FRONT_AND_BACK);

            drawGrid();

            // Create an object for every cell out there
            for (int j = startY - step; j < step * h; j += step)
            {
                for (int i = startX - step; i < step * w; i += step)
                {
                    var obj = new myObj_150();

                    obj.x = i;
                    obj.y = j;

                    list.Add(obj);
                }
            }

            // Set some of the objects to be alive
            {
                int Cnt = list.Count / 7;
                Cnt = rand.Next(Cnt / 2) + Cnt;

                for (int i = 0; i < Cnt; i++)
                {
                    if (Glfw.WindowShouldClose(window))
                        break;

                    var obj = list[rand.Next(list.Count)] as myObj_150;

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
                if (cnt % 5 == 0)
                {
                    for (int i = 0; i < list.Count; i++)
                    {
                        var obj = list[i] as myObj_150;
                        obj.Move();
                    }

                    for (int i = 0; i < list.Count; i++)
                    {
                        var obj = list[i] as myObj_150;

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
                        var obj = list[rand.Next(list.Count)] as myObj_150;

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
            startX = (gl_Width  % step) / 2;
            startY = (gl_Height % step) / 2;

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

            myPrimitive._Rectangle.SetColor(bgrR, bgrG, bgrB, 1.0f);
            myPrimitive._Rectangle.Draw(0, 0, gl_Width, gl_Height, true);

            for (int i = startX; i < gl_Width; i += step)
                myPrimitive._Line.Draw(i, 0, i, gl_Height);

            for (int i = startY; i < gl_Height; i += step)
                myPrimitive._Line.Draw(0, i, gl_Width, i);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private myObject getObj(int x, int y)
        {
            int index = y * (gl_Width / step + 2) + x;

            if (index >= 0 && index < list.Count)
            {
                return list[index];
            }

            return null;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void getCoords(ref int X, ref int Y)
        {
            X = x / step + 1;
            Y = y / step + 1;

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------
    }
};
