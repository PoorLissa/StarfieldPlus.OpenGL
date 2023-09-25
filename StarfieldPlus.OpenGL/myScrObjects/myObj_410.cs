using GLFW;
using static OpenGL.GL;
using System.Collections.Generic;


/*
    - Concentric vibrating circles around randomly moving center point
*/


namespace my
{
    public class myObj_410 : myObject
    {
        // Priority
        public static int Priority => 10;
		public static System.Type Type => typeof(myObj_410);

        int circCount;
        private float size, dSize, A, R, G, B;

        private static int N = 0, shape = 0, lineTh = 9;
        private static bool doFillShapes = false;
        private static float dimAlpha = 0.05f;

        // The Center
        static float centerX = 0, centerY = 0, centerDx = 0, centerDy = 0;
        static int centerOffX = 0, centerOffY = 0;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_410()
        {
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
                N = rand.Next(100) + 75;
            }

            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time local initialization
        private void initLocal()
        {
            doClearBuffer = myUtils.randomChance(rand, 1, 13);
            dimAlpha = 0.005f + myUtils.randFloat(rand) * 0.01f;

            centerX = gl_x0;
            centerY = gl_y0;

            centerDx = myUtils.randFloat(rand, 0.2f) * 3 * myUtils.randomSign(rand);
            centerDy = myUtils.randFloat(rand, 0.2f) * 3 * myUtils.randomSign(rand);

            centerOffX = rand.Next(1111) + 111;
            centerOffY = rand.Next(1111) + 111;

            renderDelay = rand.Next(11) + 1;

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            height = 600;

            string nStr(int   n) { return n.ToString("N0");    }
            string fStr(float f) { return f.ToString("0.000"); }

            string str = $"Obj = myObj_410\n\n"                         +
                            $"N = {nStr(list.Count)} of {nStr(N)}\n"    +
                            $"doClearBuffer = {doClearBuffer}\n"        +
                            $"dimAlpha = {fStr(dimAlpha)}\n"            +
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
            size = rand.Next(1234) + 1234;
            dSize = rand.Next(33) + 5;
            circCount = rand.Next(250) + 250;

            colorPicker.getColorRand(ref R, ref G, ref B);
            A = myUtils.randFloat(rand) * 0.123f;

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            // Move the Center
            if (id == 0)
            {
                centerX += centerDx;
                centerY += centerDy;

                if (centerX > gl_x0 + centerOffX)
                    centerDx -= 0.25f;

                if (centerX < gl_x0 - centerOffX)
                    centerDx += 0.25f;

                if (centerY > gl_y0 + centerOffY)
                    centerDy -= 0.25f;

                if (centerY < gl_y0 - centerOffY)
                    centerDy += 0.25f;
            }

            if (size <= 0 || circCount <= 0)
            {
                generateNew();
            }
            else
            {
                size -= dSize;
                circCount--;

                if (myUtils.randomChance(rand, 1, 3))
                {
                    dSize *= 0.95f;
                    A *= 1.005f;
                }
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Show()
        {
            int x = (int)centerX + rand.Next(11) - 5;
            int y = (int)centerY + rand.Next(11) - 5;

            myPrimitive._Ellipse.SetColor(R, G, B, A);
            myPrimitive._Ellipse.Draw(x - size, y - size, 2 * size, 2 * size);


/*
            float size2x = size * 2;

            switch (shape)
            {
                // Instanced squares
                case 0:
                    var rectInst = inst as myRectangleInst;

                    rectInst.setInstanceCoords(x - size, y - size, size2x, size2x);
                    rectInst.setInstanceColor(R, G, B, A);
                    rectInst.setInstanceAngle(angle);
                    break;

                // Instanced triangles
                case 1:
                    var triangleInst = inst as myTriangleInst;

                    triangleInst.setInstanceCoords(x, y, size2x, angle);
                    triangleInst.setInstanceColor(R, G, B, A);
                    break;

                // Instanced circles
                case 2:
                    var ellipseInst = inst as myEllipseInst;

                    ellipseInst.setInstanceCoords(x, y, size2x, angle);
                    ellipseInst.setInstanceColor(R, G, B, A);
                    break;

                // Instanced pentagons
                case 3:
                    var pentagonInst = inst as myPentagonInst;

                    pentagonInst.setInstanceCoords(x, y, size2x, angle);
                    pentagonInst.setInstanceColor(R, G, B, A);
                    break;

                // Instanced hexagons
                case 4:
                    var hexagonInst = inst as myHexagonInst;

                    hexagonInst.setInstanceCoords(x, y, size2x, angle);
                    hexagonInst.setInstanceColor(R, G, B, A);
                    break;
            }
*/
            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Process(Window window)
        {
            uint cnt = 0;
            initShapes();

            // Disable VSYNC if needed
            // Glfw.SwapInterval(0);

            if (doClearBuffer)
            {
                glDrawBuffer(GL_FRONT_AND_BACK | GL_DEPTH_BUFFER_BIT);
                glClearColor(0, 0, 0, 1);
            }
            else
            {
                dimScreenRGB_SetRandom(0.1f);
                glDrawBuffer(GL_BACK);
            }

            myPrimitive._Ellipse.setLineThickness(lineTh);

            while (!Glfw.WindowShouldClose(window))
            {
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
                    else
                    {
                        dimScreen(dimAlpha);
                    }
                }

                // Render Frame
                {
                    inst.ResetBuffer();

                    for (int i = 0; i != list.Count; i++)
                    {
                        var obj = list[i] as myObj_410;

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

                if (list.Count < N)
                {
                    list.Add(new myObj_410());
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

            base.initShapes(shape, N, 0);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------
    }
};
