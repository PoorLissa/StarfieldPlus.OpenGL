using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;


/*
    - Thin and long vertical or horizontal rectangles with low opacity and random color
*/


namespace my
{
    public class myObj_870 : myObject
    {
        // Priority
        public static int Priority => 10;
		public static System.Type Type => typeof(myObj_870);

        private int cnt;
        private float x, y;
        private float size, Size, A, R, G, B;

        private static int N = 0, mode = 0;
        private static bool doFillShapes = false;
        private static float dimAlpha = 0.01f;

        private static myScreenGradient grad = null;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_870()
        {
            if (id != uint.MaxValue)
                generateNew();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time global initialization
        protected override void initGlobal()
        {
            colorPicker = new myColorPicker(gl_Width, gl_Height);
            list = new List<myObject>();

            // Global unmutable constants
            {
                switch (rand.Next(4))
                {
                    case 0: N = rand.Next( 3) + 1; break;
                    case 1: N = rand.Next(13) + 3; break;
                    case 2: N = rand.Next(33) + 5; break;
                    case 3: N = rand.Next(99) + 9; break;
                }
            }

            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time local initialization
        private void initLocal()
        {
            doClearBuffer = myUtils.randomChance(rand, 2, 3);

            mode = rand.Next(2);

            renderDelay = rand.Next(11) + 3;

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            height = 600;

            string nStr(int   n) { return n.ToString("N0");    }
            //string fStr(float f) { return f.ToString("0.000"); }

            string str = $"Obj = {Type}\n\n"                         +
                            $"N = {nStr(list.Count)} of {nStr(N)}\n" +
                            $"doClearBuffer = {doClearBuffer}\n"     +
                            $"mode = {mode}\n"                       +
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
            x = rand.Next(gl_Width);
            y = rand.Next(gl_Height);

            A = 0.5f + myUtils.randFloat(rand) * 0.1f;
            colorPicker.getColor(x, y, ref R, ref G, ref B);

            cnt = 10 + rand.Next(123);
            Size = rand.Next(100) + 11;
            size = 1;

            switch (rand.Next(2))
            {
                case 0:
                    x = rand.Next(gl_Width);
                    y = 0;
                    break;

                case 1:
                    x = 0;
                    y = rand.Next(gl_Height);
                    break;
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            if (cnt < 0)
            {
                if (++cnt == 0)
                {
                    generateNew();
                }
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Show()
        {
            if (cnt > 0)
            {
                myPrimitive._Rectangle.SetColor(R, G, B, A);

                switch (mode)
                {
                    case 0:
                        {
                            if (cnt > 0)
                                cnt *= -1;

                            if (x == 0)
                            {
                                myPrimitive._Rectangle.Draw(0, y, gl_Width, Size+0, true);
                                myPrimitive._Rectangle.Draw(0, y, gl_Width, Size+0, false);
                            }
                            else
                            {
                                myPrimitive._Rectangle.Draw(x, 0, Size, gl_Height, true);
                                myPrimitive._Rectangle.Draw(x, 0, Size, gl_Height, false);
                            }
                        }
                        break;

                    case 1:
                        {
                            if (x == 0)
                            {
                                myPrimitive._Rectangle.Draw(0, y - size, gl_Width, 2 * size, true);
                            }
                            else
                            {
                                myPrimitive._Rectangle.Draw(x - size, 0, 2 * size, gl_Height, true);
                            }

                            if (size < Size)
                                size++;
                            else
                                cnt *= -1;
                        }
                        break;
                }
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Process(Window window)
        {
            uint cnt = 0;
            initShapes();

            clearScreenSetup(doClearBuffer, 0.1f);
            glDrawBuffer(GL_FRONT_AND_BACK);

            grad.SetOpacity(1);
            grad.Draw();
            grad.SetOpacity(dimAlpha);

            while (!Glfw.WindowShouldClose(window))
            {
                int Count = list.Count;

                processInput(window);

                // Swap fore/back framebuffers, and poll for operating system events.
                Glfw.SwapBuffers(window);
                Glfw.PollEvents();

                // Dim screen
                {
                    if (doClearBuffer == false)
                    {
                        grad.Draw();
                    }
                }

                // Render Frame
                {
                    for (int i = 0; i != Count; i++)
                    {
                        var obj = list[i] as myObj_870;

                        obj.Show();
                        obj.Move();
                    }
                }

                if (Count < N)
                {
                    list.Add(new myObj_870());
                }

                cnt++;
                System.Threading.Thread.Sleep(renderDelay);
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void initShapes()
        {
            myPrimitive.init_Rectangle();

            grad = new myScreenGradient();
            grad.SetRandomColors(rand, 0.2f);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------
    }
};
