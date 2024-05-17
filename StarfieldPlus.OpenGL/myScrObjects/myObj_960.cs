using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;


/*
    - Pulsing grids
*/


namespace my
{
    public class myObj_960 : myObject
    {
        // Priority
        public static int Priority => 10;
		public static System.Type Type => typeof(myObj_960);

        private float cellSize = 1.0f, dSize = 0;
        private float A, dA, R, G, B;

        private static int N = 0, mode = 0;
        private static myScreenGradient grad = null;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_960()
        {
            cellSize = -1;

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
                N = 1 + rand.Next(13);
            }

            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time local initialization
        private void initLocal()
        {
            doClearBuffer = myUtils.randomBool(rand);

            renderDelay = rand.Next(11) + 3;

            mode = rand.Next(2);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            height = 600;

            string nStr(int   n) { return n.ToString("N0");    }
            string fStr(float f) { return f.ToString("0.000"); }

            string str = $"Obj = {Type}\n\n"                         +
                            $"N = {nStr(list.Count)} of {nStr(N)}\n" +
                            $"mode = {mode}\n"                       +
                            $"cellSize = {fStr(cellSize)}\n"         +
                            $"dSize = {fStr(dSize)}\n"               +
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
            if (cellSize < 0)
            {
                cellSize = 1 + rand.Next(333);
                dSize = 0.01f + myUtils.randFloat(rand) * 0.5f;
                dA = 0.001f + myUtils.randFloat(rand) * 0.01f;
            }
            else
            {
                switch (mode)
                {
                    case 0:
                        dSize *= -1;
                        break;

                    case 1:
                        cellSize = 1;
                        dSize = 0.01f + myUtils.randFloat(rand) * 0.5f;
                        break;
                }    
            }

            A = 0.05f + myUtils.randFloat(rand) * 0.2f;
            colorPicker.getColorRand(ref R, ref B, ref G);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            cellSize += dSize;

            if (cellSize < 2 || cellSize > 500)
            {
                switch (mode)
                {
                    case 0:
                        generateNew();
                        break;

                    case 1:
                        if ((A -= dA) < 0)
                            generateNew();
                        break;
                }
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Show()
        {
            float a = A;

            for (float i = gl_x0 - cellSize / 2; i > 0; i -= cellSize)
            {
                myPrimitive._LineInst.setInstanceCoords(i, 0, i, gl_Height);
                myPrimitive._LineInst.setInstanceColor(R, G, B, a);
                a *= 0.9f;
            }

            a = A;

            for (float i = gl_x0 + cellSize/2; i < gl_Width; i += cellSize)
            {
                myPrimitive._LineInst.setInstanceCoords(i, 0, i, gl_Height);
                myPrimitive._LineInst.setInstanceColor(R, G, B, a);
                a *= 0.9f;
            }

            a = A;

            for (float i = gl_y0 - cellSize / 2; i > 0; i -= cellSize)
            {
                myPrimitive._LineInst.setInstanceCoords(0, i, gl_Width, i);
                myPrimitive._LineInst.setInstanceColor(R, G, B, a);
                a *= 0.9f;
            }

            a = A;

            for (float i = gl_y0 + cellSize / 2; i < gl_Height; i += cellSize)
            {
                myPrimitive._LineInst.setInstanceCoords(0, i, gl_Width, i);
                myPrimitive._LineInst.setInstanceColor(R, G, B, a);
                a *= 0.9f;
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Process(Window window)
        {
            uint cnt = 0;
            initShapes();

            // Disable VSYNC if needed
            // Glfw.SwapInterval(0);

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
                        var obj = list[i] as myObj_960;

                        myPrimitive._LineInst.ResetBuffer();

                        obj.Show();
                        obj.Move();

                        myPrimitive._LineInst.Draw();
                    }
                }

                if (Count < N)
                {
                    list.Add(new myObj_960());
                }

                cnt++;
                System.Threading.Thread.Sleep(renderDelay);
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void initShapes()
        {
            myPrimitive.init_LineInst(gl_Width + gl_Height + 1);

            grad = new myScreenGradient();
            grad.SetRandomColors(rand, 0.2f);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------
    }
};
