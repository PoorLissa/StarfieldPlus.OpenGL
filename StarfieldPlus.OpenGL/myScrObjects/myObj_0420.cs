using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;


/*
    -     // - system, where the center attracts and repels all the particles at the same time. vary both forces
*/


namespace my
{
    public class myObj_0420 : myObject
    {
        // Priority
        public static int Priority => 10;
		public static System.Type Type => typeof(myObj_0420);

        private float x, y, dx, dy, oldx, oldy;
        private float size, mass, A, R, G, B, angle = 0, dAngle = 0;

        private static int N = 0, shape = 0, dxyMode = 0;
        private static bool doFillShapes = false, doCreateAtOnce = true;
        private static float dimAlpha = 0.05f, gravFactor = 0.0005f;

        private static int xCenter = 0, yCenter = 0;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_0420()
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
                xCenter = gl_x0;
                yCenter = gl_y0;

                shape = rand.Next(5);

                N = 1000 + rand.Next(11111);
            }

            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time local initialization
        private void initLocal()
        {
            doCreateAtOnce = myUtils.randomChance(rand, 1, 2);
            doClearBuffer  = myUtils.randomChance(rand, 1, 7);

            dxyMode = rand.Next(4);                             // initial dx/dy value

            renderDelay = rand.Next(11) + 3;

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            height = 600;

            string nStr(int   n) { return n.ToString("N0");    }
            string fStr(float f) { return f.ToString("0.000"); }

            string str = $"Obj = {Type}\n\n"                         	+
                            $"N = {nStr(list.Count)} of {nStr(N)}\n"    +
                            $"doClearBuffer = {doClearBuffer}\n"        +
                            $"dxyMode = {dxyMode}\n"                    +
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
            x = oldx = rand.Next(gl_Width);
            y = oldy = rand.Next(gl_Width) - (gl_Width - gl_Height)/2;

            switch (dxyMode)
            {
                case 0:
                    dx = dy = 0;
                    break;

                case 1:
                    dx = myUtils.randFloat(rand) * 11;
                    dy = 0;
                    break;

                case 2:
                    dx = 0;
                    dy = myUtils.randFloat(rand) * 11;
                    break;

                case 3:
                    dx = myUtils.randFloat(rand) * 11;
                    dy = myUtils.randFloat(rand) * 11;
                    break;
            }

            size = rand.Next(11) + 3;
            size = 3;
            mass = size * (rand.Next(50) + 100);

            colorPicker.getColorRand(ref R, ref G, ref B);
            A = 0.1f + mass / 375;

            size *= A * 1.2f;

            dAngle = myUtils.randFloat(rand, 0.05f) * 0.01f;

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            float DX = 0, DY = 0, dist = 0, F = 0, d2 = 0;

            DX = x - xCenter;
            DY = y - yCenter;
            d2 = DX * DX + DY * DY;

            if (d2 > 0)
            {
                dist = (float)Math.Sqrt(d2);

                F = gravFactor * mass / dist;

                dx -= F * DX;
                dy -= F * DY;
            }

            // Apply resisting force:
            dx *= 0.99f;
            dy *= 0.99f;

            A *= 0.995f;

            oldx = x;
            oldy = y;

            angle += dAngle;

            if (id == 0)
            {
                //xCenter += 1;
            }

            if (A < 0.0001f)
            {
                generateNew();
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Show()
        {
            float size2x = size * 2;

            x += dx;
            y += dy;

            myPrimitive._LineInst.setInstanceCoords(x, y, oldx, oldy);
            myPrimitive._LineInst.setInstanceColor(1, 1, 1, A * 0.5f);

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

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Process(Window window)
        {
            uint cnt = 0;
            initShapes();

            clearScreenSetup(doClearBuffer, 0.1f);

            if (doCreateAtOnce)
            {
                while (list.Count < N)
                    list.Add(new myObj_0420());
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
                    myPrimitive._LineInst.ResetBuffer();

                    int Count = list.Count;

                    for (int i = 0; i != Count; i++)
                    {
                        (list[i] as myObj_0420).Move();
                    }

                    for (int i = 0; i != Count; i++)
                    {
                        (list[i] as myObj_0420).Show();
                    }

                    myPrimitive._LineInst.Draw();

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
                    list.Add(new myObj_0420());
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
            myPrimitive.init_LineInst(N);
            base.initShapes(shape, N, 0);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------
    }
};
