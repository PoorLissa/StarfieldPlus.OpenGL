using GLFW;
using static OpenGL.GL;
using System.Collections.Generic;


/*
    - TexText text to estimate drawing performance
*/


namespace my
{
    public class myObj_999_test_003 : myObject
    {
        // Priority
        public static int Priority => 99910;
        public static System.Type Type => typeof(myObj_999_test_003);

        private int index, cnt;
        private float x, y, dy, angle, dAngle, sizeFactor;
        private float A, R, G, B;

        private static int N = 0, size = 20;

        private static TexText tTex = null;
        private static myScreenGradient grad = null;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_999_test_003()
        {
            if (id != uint.MaxValue)
                generateNew();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time global initialization
        protected override void initGlobal()
        {
            colorPicker = new myColorPicker(gl_Width, gl_Height, mode: myColorPicker.colorMode.SNAPSHOT);
            list = new List<myObject>();

            // Global unmutable constants
            {
                N = 10000;

                size = 33;

                renderDelay = 0;
            }

            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time local initialization
        private void initLocal()
        {
            doClearBuffer = true;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            height = 600;

            string nStr(int   n) { return n.ToString("N0");    }
            //string fStr(float f) { return f.ToString("0.000"); }

            string str = $"Obj = {Type}\n\n"                           	  +
                            $"N = {nStr(list.Count)} of {nStr(N)}\n"      +
                            $"doClearBuffer = {doClearBuffer}\n"          +
                            $"renderDelay = {renderDelay}\n"              +
                            $"font = '{tTex.FontFamily()}'\n"             +
                            $"size = {size}\n"                            +
                            $"file: {colorPicker.GetFileName()}"
                ;
            return str;
        }

        // ---------------------------------------------------------------------------------------------------------------

        // 
        protected override void setNextMode()
        {
            return;
            initLocal();
            clearScreenSetup(doClearBuffer, 0.2f);
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void generateNew()
        {
            index = rand.Next(tTex.Lengh());

            x = gl_x0 + rand.Next(gl_Width ) - gl_x0;
            y = gl_y0 + rand.Next(gl_Height) - gl_y0;

            A = 0.25f;

            // Size factor (should only reduce the symbols, as enlarging makes them pixelated)
            {
                int maxSize = 20;

                sizeFactor = tTex.getFieldHeight() > maxSize
                    ? 1.0f * maxSize / tTex.getFieldHeight()
                    : 1.0f;
            }

            colorPicker.getColor(rand.Next(gl_Width), rand.Next(gl_Height), ref R, ref G, ref B);

            angle = 0;
            cnt = 100 + rand.Next(100);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            x += myUtils.randFloatSigned(rand) * 0.25f;
            y += myUtils.randFloatSigned(rand) * 0.25f;

            if (--cnt == 0)
            {
                generateNew();
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Show()
        {
            tTex.Draw(x, y, index, sizeFactor);
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Process(Window window)
        {
            var bgrTex = new myTexRectangle(colorPicker.getImg());

            initShapes();

            clearScreenSetup(doClearBuffer, 0.2f);

            while (!Glfw.WindowShouldClose(window))
            {
                int Count = list.Count;

                processInput(window);

                // Swap fore/back framebuffers, and poll for operating system events.
                Glfw.SwapBuffers(window);
                Glfw.PollEvents();

                // Dim screen
                {
                    glClear(GL_COLOR_BUFFER_BIT);
                    grad.Draw();
                    //bgrTex.Draw(0, 0, gl_Width, gl_Height);
                }

                // Render Frame
                {
                    tTex.getTexInst().ResetBuffer();

                    for (int i = 0; i != Count; i++)
                    {
                        var obj = list[i] as myObj_999_test_003;

                        obj.Show();
                        obj.Move();
                    }

                    tTex.getTexInst().Draw();
                }

                if (Count < N)
                {
                    list.Add(new myObj_999_test_003());
                }
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void initShapes()
        {
            TexText.setScrDimensions(gl_Width, gl_Height);
            //tTex = new TexText(size, false, alphabetId: 5);

            tTex = new TexText(size, false, N, alphabetId: 5);

            grad = new myScreenGradient();
            grad.SetRandomColors(rand, 0.2f, 0);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

    }
};
