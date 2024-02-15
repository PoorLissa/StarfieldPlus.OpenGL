using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;


/*
    - 
*/


namespace my
{
    public class myObj_920 : myObject
    {
        // Priority
        public static int Priority => 999910;
		public static System.Type Type => typeof(myObj_920);

        private float x, y, dx, dy;
        private float size, A, R, G, B, angle = 0;

        private static int N = 0, shape = 0;
        private static bool doFillShapes = false, doAllocateAll = false;
        private static float dimAlpha = 0.05f, rFactor = 0, gFactor = 0, bFactor = 0;

        private static myScreenGradient grad = null;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_920()
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
                N = rand.Next(3333) + 33333;

                shape = rand.Next(5);
            }

            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time local initialization
        private void initLocal()
        {
            doClearBuffer = myUtils.randomChance(rand, 10, 11);
            doFillShapes  = myUtils.randomBool(rand);
            doAllocateAll = myUtils.randomBool(rand);

            renderDelay = rand.Next(11) + 3;

            rFactor = 100 + rand.Next(gl_y0);
            gFactor = 100 + rand.Next(gl_y0);
            bFactor = 100 + rand.Next(gl_y0);

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

            grad.SetOpacity(doClearBuffer ? 1.0f : 0.1f);
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void generateNew()
        {
            x = rand.Next(gl_Width);
            y = rand.Next(gl_Height);

            dx = x - gl_x0;
            dy = y - gl_y0;

            float dist = (float)Math.Sqrt(dx * dx + dy * dy) + 0.01f;
            float distInv = 0.5f / dist;

            dx *= distInv;
            dy *= distInv;

            size = rand.Next(7) + 3;

            A = 0.25f + myUtils.randFloat(rand) * 0.33f;
            A = 0.5f;

            R = rFactor * distInv;
            G = gFactor * distInv;
            B = bFactor * distInv;

/*
            dx = dy = 0;
            x = gl_x0;
            y = gl_y0;
            size = dist;
            angle = rand.Next(666);
*/
            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            x += dx;
            y += dy;
            A -= 0.01f;

            if (A < 0 || x < 0 || x > gl_Width || y < 0 || y > gl_Height)
            {
                generateNew();
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Show()
        {
            float size2x = size * 2;

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
                    myPrimitive._TriangleInst.setInstanceCoords(x, y, size, angle);
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

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Process(Window window)
        {
            uint cnt = 0;
            initShapes();

            clearScreenSetup(doClearBuffer, 0.1f);

            if (doAllocateAll)
                while (list.Count < N)
                    list.Add(new myObj_920());

            while (!Glfw.WindowShouldClose(window))
            {
                int Count = list.Count;

                processInput(window);

                // Swap fore/back framebuffers, and poll for operating system events.
                Glfw.SwapBuffers(window);
                Glfw.PollEvents();

                // Dim screen
                {
                    //glClear(GL_COLOR_BUFFER_BIT);
                    grad.Draw();
                }

                // Render Frame
                {
                    inst.ResetBuffer();

                    for (int i = 0; i != Count; i++)
                    {
                        var obj = list[i] as myObj_920;

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
                    list.Add(new myObj_920());
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
            grad.SetOpacity(doClearBuffer ? 1.0f : 0.1f);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------
    }
};
