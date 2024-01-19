using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;


/*
    - Desktop pieces falling down in a matrix-style
*/


namespace my
{
    public class myObj_890 : myObject
    {
        // Priority
        public static int Priority => 9999910;
		public static System.Type Type => typeof(myObj_890);

        private float x, y, dy, size, A;

        private static int N = 0, mode = 0, spdFactor;
        private static float dimAlpha = 0.05f;
        private static bool doAllocateAtOnce = false;

        private static myScreenGradient grad = null;
        private static myTexRectangleInst _texInst = null;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_890()
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
                N = rand.Next(10) + 10;
                N = 6666 + rand.Next(33333);

                doAllocateAtOnce = myUtils.randomChance(rand, 1, 2);
            }

            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time local initialization
        private void initLocal()
        {
            doClearBuffer = myUtils.randomChance(rand, 1, 2);

            mode = 0;

            spdFactor = rand.Next(13) + 1;
            dimAlpha = 0.05f;

            renderDelay = rand.Next(5);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            height = 600;

            string nStr(int   n) { return n.ToString("N0");    }
            //string fStr(float f) { return f.ToString("0.000"); }

            string str = $"Obj = {Type}\n\n"                           +
                            $"N = {nStr(list.Count)} of {nStr(N)}\n"   +
                            $"doClearBuffer = {doClearBuffer}\n"       +
                            $"doAllocateAtOnce = {doAllocateAtOnce}\n" +
                            $"mode = {mode}\n"                         +
                            $"spdFactor = {spdFactor}\n"               +
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
            size = rand.Next(33) + 3;

            x = rand.Next(gl_Width);
            y = -size;
            dy = myUtils.randFloat(rand, 0.1f) * spdFactor;

            A = myUtils.randFloat(rand) * 0.05f;

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            y += dy;

            if (y > gl_Height)
            {
                generateNew();
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Show()
        {
            switch (mode)
            {
                case 0:
                    {
                        int X = (int)x;
                        int Y = (int)y;
                        int Size = (int)size;

                        _texInst.setInstanceCoords(x, y, Size, Size, x, y, Size, Size);
                        _texInst.setInstanceColor(1, 1, 1, A);
                        _texInst.setInstanceAngle(0);
                    }
                    break;

                case 1:
                    {
                    }
                    break;

                case 2:
                    {
                    }
                    break;
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

            if (doAllocateAtOnce)
            {
                while (list.Count < N)
                    list.Add(new myObj_890());
            }

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
                        grad.Draw();
                    }
                    else
                    {
                        grad.Draw();
                    }
                }

                // Render Frame
                {
                    _texInst.ResetBuffer();

                    for (int i = 0; i != Count; i++)
                    {
                        var obj = list[i] as myObj_890;

                        obj.Show();
                        obj.Move();
                    }

                    _texInst.Draw();
                }

                if (Count < N)
                {
                    list.Add(new myObj_890());
                }

                cnt++;
                System.Threading.Thread.Sleep(renderDelay);
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void initShapes()
        {
            grad = new myScreenGradient();
            grad.SetRandomColors(rand, 0.2f);

            if (doClearBuffer == false)
            {
                grad.SetOpacity(dimAlpha);
            }

            _texInst = new myTexRectangleInst(colorPicker.getImg(), N);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------
    }
};
