using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;


/*
    - Particles move radially from the off-center position, creating a vortex-like structure
*/


namespace my
{
    public class myObj_390 : myObject
    {
        private float x, y, dx, dy;
        private float size, A, R, G, B, angle = 0, da;

        private static int N = 0, shape = 0;
        private static bool doFillShapes = false, doCreateAtOnce = false;
        private static float dimAlpha = 0.05f, dAlphaStatic = 0;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_390()
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
                doClearBuffer  = myUtils.randomBool(rand);
                doFillShapes   = myUtils.randomBool(rand);
                doCreateAtOnce = myUtils.randomBool(rand);

                N = 33333;
                shape = rand.Next(5);

                renderDelay = 10;
                dimAlpha = 0.075f;
            }

            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time local initialization
        private void initLocal()
        {
            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            height = 600;

            string nStr(int   n) { return n.ToString("N0");    }
            string fStr(float f) { return f.ToString("0.000"); }

            string str = $"Obj = myObj_390\n\n"                         +
                            $"N = {nStr(list.Count)} of {nStr(N)}\n"    +
                            $"shape = {shape}\n"                        +
                            $"doClearBuffer = {doClearBuffer}\n"        +
                            $"doFillShapes = {doFillShapes}\n"          +
                            $"doCreateAtOnce = {doCreateAtOnce}\n"      +
                            $"dimAlpha = {fStr(dimAlpha)}\n"            +
                            $"renderDelay = {renderDelay}\n"            +
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
            double dist = 0;

            // try different variations of this loop thing

            do
            {
                x = rand.Next(gl_Width);
                y = rand.Next(gl_Width);

                dist = Math.Sqrt((x - gl_x0) * (x - gl_x0) + (y - gl_x0) * (y - gl_x0));
            }
            while (dist < 200 || dist > 500);

            double sp_dist = 5.0 / dist;

            // Get current angle alpha:
            double alpha = Math.Acos((y - gl_x0) / dist);
            double dAlpha = dAlphaStatic;

            if (x > gl_x0)
            {
                alpha += dAlpha;

/*
                float x2 = gl_x0 + (float)(dist * Math.Sin(alpha));
                float y2 = gl_x0 + (float)(dist * Math.Cos(alpha));

                dx = (float)((x2 - gl_x0) * sp_dist);
                dy = (float)((y2 - gl_x0) * sp_dist);*/

                // Optimized:
                dx = (float)(Math.Sin(alpha) * 5);
                dy = (float)(Math.Cos(alpha) * 5);
            }
            else
            {
                alpha -= dAlpha;
/*
                float x2 = gl_x0 - (float)(dist * Math.Sin(alpha));
                float y2 = gl_x0 + (float)(dist * Math.Cos(alpha));

                dx = (float)((x2 - gl_x0) * sp_dist);
                dy = (float)((y2 - gl_x0) * sp_dist);*/

                // Optimized:
                dx = (float)(-Math.Sin(alpha) * 5);
                dy = (float)(+Math.Cos(alpha) * 5);
            }

            y -= (gl_Width - gl_Height) / 2;

            size = 3;

            A = 0;

            // nice effect with flashing
            //da = 0.005f + myUtils.randomSign(rand) * myUtils.randFloat(rand) * 0.0001f;

            //da = myUtils.randomChance(rand, 1, 3) ? 0.005f : 0.01f;

            da = myUtils.randomChance(rand, 1, 2) ? 0.015f : 0.010f;

            da = myUtils.randFloat(rand, 0.01f) * 0.01f;

            colorPicker.getColor(x, y, ref R, ref G, ref B);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            x += dx;
            y += dy;

            A += da;

            if (A >= 1)
            {
                da *= -10;
            }

            //if (x < 0 || x > gl_Width || y < 0 || y > gl_Height)
            if (A < 0)
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

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Process(Window window)
        {
            uint cnt = 0;
            int listCnt = 0;

            initShapes();

            if (doCreateAtOnce)
            {
                while (list.Count < N)
                    list.Add(new myObj_390());
            }

            // Disable VSYNC if needed
            // Glfw.SwapInterval(0);

            dimScreenRGB_SetRandom(0.1f);

            if (doClearBuffer)
            {
                glDrawBuffer(GL_FRONT_AND_BACK | GL_DEPTH_BUFFER_BIT);
                glClearColor(myObject.bgrR, myObject.bgrG, myObject.bgrB, 1);
            }
            else
            {
                glDrawBuffer(GL_BACK);
            }

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

                    listCnt = list.Count;

                    for (int i = 0; i != listCnt; i++)
                    {
                        var obj = list[i] as myObj_390;

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
                    list.Add(new myObj_390());
                }

                cnt++;
                System.Threading.Thread.Sleep(renderDelay);

                if (cnt % 10 == 0)
                    dAlphaStatic += 0.05f;
            }

            while (!Glfw.WindowShouldClose(window))
            {
                processInput(window);

                // Swap fore/back framebuffers, and poll for operating system events.
                Glfw.SwapBuffers(window);
                Glfw.PollEvents();
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void initShapes()
        {
            myPrimitive.init_ScrDimmer();
            base.initShapes(shape, N, 0);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------
    }
};
