using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;


/*
    - Perspective made of sine-cosine graphs
*/


namespace my
{
    public class myObj_880 : myObject
    {
        // Priority
        public static int Priority => 999910;
		public static System.Type Type => typeof(myObj_880);

        private float x, xFactor, y, dx;
        private float size, A, R, G, B, angle = 0;
        private float t = 0, dt = 0;

        private static int N = 0, n = 100, shape = 0;
        private static float dimAlpha = 0.05f;

        private static myScreenGradient grad = null;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_880()
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
                N = rand.Next(10) + 10;
                N = 33;

                shape = rand.Next(5);
            }

            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time local initialization
        private void initLocal()
        {
            doClearBuffer = myUtils.randomBool(rand);
            //doClearBuffer = true;

            renderDelay = 0;

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
            y = gl_y0 - id * 20;

            dx = gl_Width / n;
            t = 0;
            dt = 0.01f + 0.001f * id;
            dt = 0.1f;
            dt = 0.01f;
            //dt = 0.01f + 0.0001f * id;

            xFactor = 1.0f + 0.001f * id;
            xFactor = 1.5f + id * 0.00025f;

            size = 100 - id * 3;

            A = 0.66f - id * 0.01f;
            R = 0.33f;
            G = 0.33f;
            B = 0.33f;

            //colorPicker.getColor(x, y, ref R, ref G, ref B);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            t += dt;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Show()
        {
            int func = 0;

            float x1 = 0, y1 = size * (float)Math.Sin(t), x2 = 0, y2 = 0; 

            for (int i = 0; i < n; i++)
            {
                x2 = x1 + dx;

                switch (func)
                {
                    case 0:
                        y2 = size * (float)Math.Sin(xFactor * (x2 + t));
                        break;

                    case 1:
                        y2 = size * (float)Math.Sin(xFactor * x2 + t) + 0.1f * size * (float)Math.Cos(10 * xFactor * x2 + 11 * t);
                        break;
                }

                myPrimitive._LineInst.setInstanceCoords(x1, y + y1, x2, y + y2);
                myPrimitive._LineInst.setInstanceColor(R, G, B, A);

                x1 = x2;
                y1 = y2;
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Process(Window window)
        {
            uint cnt = 0;
            initShapes();

            clearScreenSetup(doClearBuffer, 0.1f);

            while (list.Count < N)
            {
                list.Add(new myObj_880());
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
                        dimScreen(dimAlpha);
                    }
                }

                // Render Frame
                {
                    myPrimitive._LineInst.ResetBuffer();

                    for (int i = 0; i != Count; i++)
                    {
                        var obj = list[i] as myObj_880;

                        obj.Show();
                        obj.Move();
                    }

                    myPrimitive._LineInst.Draw();
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
            myPrimitive.init_LineInst(N * n);

            grad = new myScreenGradient();
            grad.SetRandomColors(rand, 0.2f);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------
    }
};
