using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;


/*
    - Edge finding random algorythm
*/


namespace my
{
    public class myObj_980 : myObject
    {
        // Priority
        public static int Priority => 10;
		public static System.Type Type => typeof(myObj_980);

        private int cnt;
        private float x, y;
        private float size, A, R, G, B, angle = 0;

        private static int N = 0, shape = 0, mode = 0;
        private static bool doFillShapes = false;
        private static float dimAlpha = 0.05f, targetR = 0, targetG = 0, targetB = 0;

        private static myScreenGradient grad = null;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_980()
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
                N = rand.Next(10) + 11111;

                shape = rand.Next(5);
            }

            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time local initialization
        private void initLocal()
        {
            doClearBuffer = myUtils.randomBool(rand);

            mode = rand.Next(4);
            renderDelay = rand.Next(3) + 1;

            do {

                targetR = myUtils.randFloat(rand);
                targetG = myUtils.randFloat(rand);
                targetB = myUtils.randFloat(rand);

            } while (targetR + targetG + targetG < 0.1f);

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
                            $"mode = {mode}\n"                       +
                            $"doClearBuffer = {doClearBuffer}\n"     +
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
            cnt = 1;

            x = rand.Next(gl_Width);
            y = rand.Next(gl_Height);

            A = 0.25f;
            colorPicker.getColor(x, y, ref R, ref G, ref B);

            float r1 = 0, r2 = 0, g1 = 0, g2 = 0, b1 = 0, b2 = 0;

            if (x < gl_Width)
            {
                colorPicker.getColor(x + 1, y, ref r1, ref g1, ref b1);
            }

            if (y < gl_Height)
            {
                colorPicker.getColor(x, y + 1, ref r2, ref g2, ref b2);
            }

            float targetDiff = 0.25f;

            float diff1 = Math.Abs((R + G + B) - (r1 + g1 + b1));
            float diff2 = Math.Abs((R + G + B) - (r2 + g2 + b2));

            if (diff1 > targetDiff || diff2 > targetDiff)
            {
                size = 1;

                switch (mode)
                {
                    // Leave original color
                    case 0:
                    case 1:
                        break;

                    // Change to target color
                    case 2:
                    case 3:
                        R = targetR;
                        G = targetG;
                        B = targetB;
                        break;
                }

                cnt = doClearBuffer ? 111 : 11;
            }
            else
            {
                // Allow one-time draw of non-edge particle
                switch (mode)
                {
                    case 1:
                    case 3:
                        A = 0.1f;
                        cnt = 2;
                        break;
                }
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            if (--cnt == 0)
            {
                generateNew();
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Show()
        {
            if (cnt > 1)
            {
                float size2x = size * 2;
                angle = myUtils.randFloat(rand) * 123;

                switch (shape)
                {
                    // Instanced squares
                    case 0:
                        myPrimitive._RectangleInst.setInstanceCoords(x - size, y - size, size2x, size2x);
                        myPrimitive._RectangleInst.setInstanceColor(R, G, B, A);
                        myPrimitive._RectangleInst.setInstanceAngle(angle);
                        break;

                    // Instanced triangles
                    case 1:
                        myPrimitive._TriangleInst.setInstanceCoords(x, y, size2x, angle);
                        myPrimitive._TriangleInst.setInstanceColor(R, G, B, A);
                        break;

                    // Instanced circles
                    case 2:
                        myPrimitive._EllipseInst.setInstanceCoords(x, y, size2x, angle);
                        myPrimitive._EllipseInst.setInstanceColor(R, G, B, A);
                        break;

                    // Instanced pentagons
                    case 3:
                        myPrimitive._PentagonInst.setInstanceCoords(x, y, size2x, angle);
                        myPrimitive._PentagonInst.setInstanceColor(R, G, B, A);
                        break;

                    // Instanced hexagons
                    case 4:
                        myPrimitive._HexagonInst.setInstanceCoords(x, y, size2x, angle);
                        myPrimitive._HexagonInst.setInstanceColor(R, G, B, A);
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
                    inst.ResetBuffer();

                    for (int i = 0; i != Count; i++)
                    {
                        var obj = list[i] as myObj_980;

                        obj.Show();
                        obj.Move();
                    }

                    if (doFillShapes)
                    {
                        // Tell the fragment shader to multiply existing instance opacity by 0.5:
                        inst.SetColorA(-0.5f);
                        inst.Draw(true);
                    }

                    // Tell the fragment shader to do nothing with the existing instance opacity:
                    inst.SetColorA(0);
                    inst.Draw(false);
                }

                if (Count < N)
                {
                    list.Add(new myObj_980());
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
            base.initShapes(shape, N, 0);

            grad = new myScreenGradient();
            grad.SetRandomColors(rand, 0.2f);
            grad.SetOpacity(0.05f);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------
    }
};
