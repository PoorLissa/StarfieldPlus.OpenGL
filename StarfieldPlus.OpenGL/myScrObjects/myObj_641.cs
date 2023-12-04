using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;


/*
    - 
*/


namespace my
{
    public class myObj_641 : myObject
    {
        // Priority
        public static int Priority => 9999910;
		public static System.Type Type => typeof(myObj_641);

        private bool isFilled;
        private int cnt;
        private float x, y;
        private float size, A, R, G, B;

        private static int N = 0, maxSize = 1, fillMode = 0;

        private static myScreenGradient grad = null;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_641()
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
                N = 3333;
            }

            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time local initialization
        private void initLocal()
        {
            doClearBuffer = true;

            fillMode = rand.Next(3);

            renderDelay = rand.Next(3);

            maxSize = 222;

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
                            $"fillMode = {fillMode}\n"               +
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
            cnt = 666 + rand.Next(333);

            switch (fillMode)
            {
                case 0:
                    isFilled = false;
                    break;

                case 1:
                    isFilled = true;
                    break;

                case 2:
                    isFilled = myUtils.randomChance(rand, 1, 2);
                    break;
            }

            // Must be true: dist < (r1 + r2)
            {
                int Count = list.Count;
                bool isOk = false;

                while (isOk == false)
                {
                    isOk = true;

                    x = rand.Next(gl_Width);
                    y = rand.Next(gl_Height);

                    size = maxSize;

                    for (int i = 0; isOk && i < Count; i++)
                    {
                        var obj = list[i] as myObj_641;

                        if (id != obj.id)
                        {
                            float dx = x - obj.x;
                            float dy = y - obj.y;
                            float dist = (float)Math.Sqrt(dx * dx + dy * dy);
                            float R = size + obj.size;

                            if (dist < R)
                            {
                                if (dist < obj.size)
                                {
                                    isOk = false;
                                    break;
                                }
                                else
                                {
                                    size = dist - obj.size;

                                    size += isFilled ? 1 : -2;

                                    if (size < 3)
                                    {
                                        isOk = false;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            A = 0.75f;
            colorPicker.getColor(x, y, ref R, ref G, ref B);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            if (--cnt < 0)
            {
                A -= 0.005f;

                if (A < 0)
                {
                    generateNew();
                }
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Show()
        {
            myPrimitive._Ellipse.SetColor(R, G, B, A);
            myPrimitive._Ellipse.Draw(x - size, y - size, size * 2, size * 2, isFilled);
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Process(Window window)
        {
            uint cnt = 0;
            initShapes();


            clearScreenSetup(doClearBuffer, 0.1f);


            while (!Glfw.WindowShouldClose(window))
            {
                int Count = list.Count;

                processInput(window);

                // Swap fore/back framebuffers, and poll for operating system events.
                Glfw.SwapBuffers(window);
                Glfw.PollEvents();

                // Dim screen
                {
                    if (doClearBuffer)
                    {
                        glClear(GL_COLOR_BUFFER_BIT);
                    }

                    grad.Draw();
                }

                // Render Frame
                {
                    for (int i = 0; i != Count; i++)
                    {
                        var obj = list[i] as myObj_641;

                        obj.Show();
                        obj.Move();
                    }
                }

                if (cnt == 1 && Count < N)
                {
                    cnt = 0;
                    list.Add(new myObj_641());
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
            myPrimitive.init_Ellipse();

            myPrimitive._Ellipse.setLineThickness(3);

            grad = new myScreenGradient();
            grad.SetRandomColors(rand, 0.2f, 0);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------
    }
};
