using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;
using static System.Net.Mime.MediaTypeNames;


/*
    - 
*/


namespace my
{
    public class myObj_1380 : myObject
    {
        // Priority
        public static int Priority => 10;
		public static System.Type Type => typeof(myObj_1380);

        private float x, y, dx, dy;
        private float size, A, R, G, B;
        private float lR, lG, lB;

        private static int N = 0, moveMode = 0, gridSize = 0, gridOffsetX = 0, gridOffsetY = 0;

        private static myScreenGradient grad = null;
        private static myTexRectangle tex = null;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_1380()
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
                N = rand.Next(23) + 3;
            }

            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time local initialization
        private void initLocal()
        {
            doClearBuffer = myUtils.randomChance(rand, 1, 2);
            doClearBuffer = true;

            myUtils.getRandomColor(rand, ref lR, ref lG, ref lB, 0.33f);

            moveMode = rand.Next(3);
            gridSize = 10 + rand.Next(41);

            gridOffsetX = myUtils.randomChance(rand, 1, 2)
                ? 0
                : rand.Next(333);

            gridOffsetY = myUtils.randomChance(rand, 1, 2)
                ? 0
                : rand.Next(333);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            height = 600;

            string str = $"Obj = {Type}\n\n"                  +
                            myUtils.strCountOf(list.Count, N) +
                            $"moveMode = {moveMode}\n"        +
                            $"gridOffsetX = {gridOffsetX}\n"  +
                            $"gridOffsetY = {gridOffsetY}\n" +
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
            x = rand.Next(gl_Width);
            y = rand.Next(gl_Height);

            dx = myUtils.randFloatClamped(rand, 0.5f) * myUtils.randomSign(rand) * 0.5f;
            dy = myUtils.randFloatClamped(rand, 0.5f) * myUtils.randomSign(rand) * 0.5f;

            size = 21;

            A = 1;
            myUtils.getRandomColor(rand, ref R, ref G, ref B, 0.33f);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            x += dx;
            y += dy;

            switch (moveMode)
            {
                case 0:
                    {
                        if (x < 0 && dx < 0)
                            dx *= -1;

                        if (y < 0 && dy < 0)
                            dy *= -1;

                        if (x > gl_Width && dx > 0)
                            dx *= -1;

                        if (y > gl_Height && dy > 0)
                            dy *= -1;
                    }
                    break;

                case 1:
                    {
                        float dd = 0.1f;
                        int offset = 100;

                        if (x < offset)
                            dx += dd;

                        if (y < offset)
                            dy += dd;

                        if (x > gl_Width - offset)
                            dx -= dd;

                        if (y > gl_Height - offset)
                            dy -= dd;
                    }
                    break;

                case 2:
                    {
                        if (x < 0)
                        {
                            x = gl_Width;
                            y += y % 100;
                        }

                        if (y < 0)
                        {
                            y = gl_Height;
                            x += x % 100;
                        }

                        if (x > gl_Width)
                        {
                            x = 0;
                            y -= y % 100;
                        }

                        if (y > gl_Height)
                        {
                            y = 0;
                            x -= x % 100;
                        }
                    }
                    break;
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Show()
        {
            int lineOffset = (int)size/2 - 1;

            myPrimitive._Line.SetColor(R, G, B, 0.5f);

            myPrimitive._Line.Draw(x + lineOffset, 0, x + lineOffset, gl_Height, 2);
            myPrimitive._Line.Draw(0, y + lineOffset, gl_Width, y + lineOffset, 2);

            myPrimitive._Line.Draw(x + lineOffset, 0, x + lineOffset, gl_Height, 2);
            myPrimitive._Line.Draw(0, y + lineOffset, gl_Width, y + lineOffset, 2);

            myPrimitive._Line.SetColor(R, G, B, 0.05f);

            myPrimitive._Line.Draw(x + lineOffset, 0, x + lineOffset, gl_Height, 10);
            myPrimitive._Line.Draw(0, y + lineOffset, gl_Width, y + lineOffset, 10);
            myPrimitive._Line.Draw(x + lineOffset, 0, x + lineOffset, gl_Height, 20);
            myPrimitive._Line.Draw(0, y + lineOffset, gl_Width, y + lineOffset, 20);

            myPrimitive._Rectangle.setPixelDensityOffset(1);

            glLineWidth(2);
            myPrimitive._Rectangle.SetColor(R, G, B, 0.15f);
            myPrimitive._Rectangle.Draw(x - 3, y - 3, size + 6, size + 6, true);

            glLineWidth(3);
            myPrimitive._Rectangle.SetColor(R, G, B, 0.25f);
            myPrimitive._Rectangle.Draw(x, y, size, size, false);

            glLineWidth(1);
            myPrimitive._Rectangle.SetColor(R, G, B, A);
            myPrimitive._Rectangle.Draw(x, y, size, size, false);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Process(Window window)
        {
            uint cnt = 0;
            initShapes();

            list.Add(new myObj_1380());

            clearScreenSetup(doClearBuffer, 0.1f);

            stopwatch = new StarfieldPlus.OpenGL.myUtils.myStopwatch(true);

            while (!Glfw.WindowShouldClose(window))
            {
                int Count = list.Count;

                processInput(window);

                // Swap fore/back framebuffers, and poll for operating system events.
                Glfw.SwapBuffers(window);
                Glfw.PollEvents();

                // Dim screen
                {
                    tex.setOpacity(1);
                    tex.Draw(0, 0, gl_Width, gl_Height);
                }

                // Render Frame
                {
                    myPrimitive._LineInst.ResetBuffer();

                    int offsetX = (gl_Width  % gridSize) / 2;
                    int offsetY = (gl_Height % gridSize) / 2;

                    for (int i = offsetX; i < gl_Width; i += gridSize)
                    {
                        if (i < gridOffsetX || i > gl_Width - gridOffsetX)
                            continue;

                        myPrimitive._LineInst.setInstanceCoords(i, 0, i, gl_Height);
                        myPrimitive._LineInst.setInstanceColor(lR, lG, lB, 0.1f);
                    }

                    for (int i = offsetY; i < gl_Height; i += gridSize)
                    {
                        if (i < gridOffsetY || i > gl_Height - gridOffsetY)
                            continue;

                        myPrimitive._LineInst.setInstanceCoords(0, i, gl_Width, i);
                        myPrimitive._LineInst.setInstanceColor(lR, lG, lB, 0.1f);
                    }

                    myPrimitive._LineInst.setLineWidth(1);
                    myPrimitive._LineInst.Draw();

                    for (int i = 0; i != Count; i++)
                    {
                        var obj = list[i] as myObj_1380;

                        obj.Show();
                        obj.Move();
                    }
                }

                if (Count < N && cnt == 500)
                {
                    list.Add(new myObj_1380());
                    cnt = 0;
                }

                stopwatch.WaitAndRestart();
                cnt++;
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void initShapes()
        {
            myPrimitive.init_ScrDimmer();
            myPrimitive.init_Rectangle();
            myPrimitive.init_Line();

            myPrimitive.init_LineInst(gl_Width/gridSize + gl_Height/gridSize + 10);

            grad = new myScreenGradient();
            grad.SetRandomColors(rand, 0.2f);

            tex = new myTexRectangle(colorPicker.getImg());

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------
    }
};
