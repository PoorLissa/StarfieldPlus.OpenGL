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
        private float A, R, G, B;

        private bool alive = false, clear = false;
        private int liveCnt = 0, lifeSpanCnt = 0;

        private static int N = 0, shape = 0;
        private static int step = 0, startX = 0, startY = 0, drawMode = 0, lightMode = 0;
        private static float bgrR = 0, bgrG = 0, bgrB = 0;
        // ---------------------------------------------------------------------------------------------------------------

        public myObj_150()
        {
            alive = false;
            clear = false;

            liveCnt = -1;
            lifeSpanCnt = 0;
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
            N = (N == 0) ? 10 + rand.Next(10) : N;

            lightMode = rand.Next(2);

            step = rand.Next(30) + 25;

            // In case the colorPicker does not taget any image, exclude unsupported drawing mode (mode #3)
            drawMode = colorPicker.getMode() < 2 ? rand.Next(3) : rand.Next(2);

//step = 67;
drawMode = 0;

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            height = 800;

            string str = $"Obj = myObj_150\n\n" +
                            $"N = {list.Count} of {N}\n" +
                            "" + 
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
            x = rand.Next(gl_Width);
            y = rand.Next(gl_Height);

            x += startX - (x % step);
            y += startY - (y % step);
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
            // These offsets give us a gap of 2 pixels on each side of the square;
            // This works fine in case the rect is not adjusted for a missing BL angle (float fx = 2.0f * x / (Width) - 1.0f;)
            int a = 2;
            int b = 3;
            int c = 5;
            int d = 5;

            if (alive)
            {
                switch (drawMode)
                {
                    // Single solid color
                    case 0:
                        myPrimitive._Rectangle.SetColor(0.35f, 0.35f, 0.35f, 1.0f);
                        myPrimitive._Rectangle.Draw(x + a, y + b, step - c, step - d, true);

                        myPrimitive._Rectangle.SetColor(0.5f, 0.25f, 0.25f, 1.0f);
                        myPrimitive._Rectangle.Draw(x + a + 1, y + b, step - c - 1, step - d - 1, false);
                        break;
                }

                clear = false;
            }
            else
            {
                if (!clear)
                {
                    myPrimitive._Rectangle.SetColor(bgrR, bgrG, bgrB, 1.0f);
                    myPrimitive._Rectangle.Draw(x + a, y + b, step - c + 1, step - d + 1, true);
                }

                clear = true;
            }
            
            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Process(Window window)
        {
            int t = 500, Cnt = 0, cnt = 0;
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
            Cnt = list.Count / 7;
            Cnt = rand.Next(Cnt / 2) + Cnt;

            for (int i = 0; i < Cnt; i++)
            {
                int index = rand.Next(list.Count);

                var obj = list[index] as myObj_150;

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
            }

            while (!Glfw.WindowShouldClose(window))
            {
                cnt++;

                processInput(window);

                // Swap fore/back framebuffers, and poll for operating system events.
                Glfw.SwapBuffers(window);
                Glfw.PollEvents();

                // Render Frame
                if (cnt % 20 == 0)
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
                if (false && cnt > 101)
                {
                    cnt = 0;

                    for (int i = 0; i < 100; i++)
                    {
                        int index = rand.Next(list.Count);

                        var obj = list[index] as myObj_150;

                        if (!obj.alive)
                        {
                            obj.alive = true;
                        }
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
            base.initShapes(shape, N, 0);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void drawGrid()
        {
            startX = (gl_Width  % step) / 2;
            startY = (gl_Height % step) / 2;

            if (lightMode == 0)
            {
                bgrR = 1 - myUtils.randFloat(rand) * 0.1f;
                bgrG = 1 - myUtils.randFloat(rand) * 0.1f;
                bgrB = 1 - myUtils.randFloat(rand) * 0.1f;

                myPrimitive._Line.SetColor(0.25f, 0.25f, 0.25f, 1.0f);
            }
            else
            {
                bgrR = myUtils.randFloat(rand) * 0.1f;
                bgrG = myUtils.randFloat(rand) * 0.1f;
                bgrB = myUtils.randFloat(rand) * 0.1f;

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
