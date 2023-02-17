using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;


/*
    - Bouncing ball and lots of triangles rotating to point to it
*/


namespace my
{
    public class myObj_440 : myObject
    {
        private int cnt;
        protected float x, y, dx, dy;
        protected float size, a, da, A, R, G, B, angle = 0;

        private static int N = 0, shape = 0;
        private static bool doFillShapes = false;
        private static float dimAlpha = 0.05f, lineTh = 1.0f;

        protected static float centerX = 0, centerY = 0;

        protected static double _1pi2 = 1.0 * Math.PI/2;
        protected static double _3pi2 = 3.0 * Math.PI/2;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_440()
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
                N = 1 + rand.Next(6666) + rand.Next(3333) + 1234;

                N = 33333;
                N = 3333;

                switch (rand.Next(13))
                {
                    case 0:  shape = 0; break;      // Square
                    case 1:  shape = 3; break;      // Pentagon
                    case 2:  shape = 4; break;      // Hexagon
                    default: shape = 1; break;      // Triangle (default)
                }
            }

            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time local initialization
        private void initLocal()
        {
            doClearBuffer = myUtils.randomChance(rand, 1, 2);
            doFillShapes  = myUtils.randomChance(rand, 1, 2);

            renderDelay = rand.Next(11) + 5;

            dimAlpha = 0.25f;

            lineTh = myUtils.randFloat(rand, 0.1f) * (rand.Next(5) + 1);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            height = 600;

            string nStr(int   n) { return n.ToString("N0");    }
            string fStr(float f) { return f.ToString("0.000"); }

            string str = $"Obj = myObj_440\n\n"                      +
                            $"N = {nStr(list.Count)} of {nStr(N)}\n" +
                            $"shape = {shape}\n"                     +
                            $"lineTh = {fStr(lineTh)}\n"             +
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
            cnt = 0;

            x = rand.Next(gl_Width);
            y = rand.Next(gl_Height);

            size = rand.Next(33) + 3;

            A = myUtils.randFloat(rand, 0.33f);                     // Target opacity
            a = 0;                                                  // Current opacity
            da = myUtils.randFloat(rand, 0.01f) * 0.01f;            // Opacity increment

            colorPicker.getColor(x, y, ref R, ref G, ref B);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            if (cnt == 0)
            {
                if (a < A)
                    a += da;

                dx = x - centerX;
                dy = y - centerY;

                if (dx > 0)
                {
                    angle = (float)(_1pi2 - Math.Atan(dy / dx));
                }
                else
                {
                    angle = (float)(_3pi2 - Math.Atan(dy / dx));
                }

                float distSquared = dx * dx + dy * dy;

                if (distSquared < 110000)
                {
                    cnt = 0;
                }
                else if (distSquared < 202500)
                {
                    cnt = rand.Next(2);
                }
                else
                {
                    cnt = rand.Next(3);
                }
            }
            else
            {
                cnt--;
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
                    rectInst.setInstanceColor(R, G, B, a);
                    rectInst.setInstanceAngle(angle);
                    break;

                // Instanced triangles
                case 1:
                    var triangleInst = inst as myTriangleInst;

                    triangleInst.setInstanceCoords(x, y, size2x, angle);
                    triangleInst.setInstanceColor(R, G, B, a);
                    break;

                // Instanced circles
                case 2:
                    var ellipseInst = inst as myEllipseInst;

                    ellipseInst.setInstanceCoords(x, y, size2x, angle);
                    ellipseInst.setInstanceColor(R, G, B, a);
                    break;

                // Instanced pentagons
                case 3:
                    var pentagonInst = inst as myPentagonInst;

                    pentagonInst.setInstanceCoords(x, y, size2x, angle);
                    pentagonInst.setInstanceColor(R, G, B, a);
                    break;

                // Instanced hexagons
                case 4:
                    var hexagonInst = inst as myHexagonInst;

                    hexagonInst.setInstanceCoords(x, y, size2x, angle);
                    hexagonInst.setInstanceColor(R, G, B, a);
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

            // Add moving ball
            list.Add(new myObj_440_Ball());

#if true
            while (list.Count < N)
                list.Add(new myObj_440());
#endif

            glLineWidth(lineTh);

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

                    int Count = list.Count;

                    for (int i = 0; i != Count; i++)
                    {
                        var obj = list[i] as myObj_440;

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
                    list.Add(new myObj_440());
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

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------
    }


    // =============================================================================================================================
    // =============================================================================================================================


    public class myObj_440_Ball : myObj_440
    {
        protected override void generateNew()
        {
            myPrimitive.init_Ellipse();

            x = rand.Next(gl_Width);
            y = rand.Next(gl_Height);

            dx = myUtils.randomSign(rand) * myUtils.randFloat(rand, 0.25f) * (rand.Next(15) + 3);
            dy = myUtils.randomSign(rand) * myUtils.randFloat(rand, 0.25f) * (rand.Next(15) + 3);

            switch (rand.Next(3))
            {
                case 0:
                    size = rand.Next(3) + 3;
                    break;

                case 1:
                    size = rand.Next(33) + 3;
                    break;

                case 2:
                    size = rand.Next(333) + 3;
                    break;
            }

            a = 0.5f;
            colorPicker.getColor(x, y, ref R, ref G, ref B);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            x += dx;
            y += dy;

            centerX = x;
            centerY = y;

            if (x < 0)
                dx += 0.1f;

            if (y < 0)
                dy += 0.1f;

            if (x > gl_Width)
                dx -= 0.1f;

            if (y > gl_Height)
                dy -= 0.1f;

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Show()
        {
            myPrimitive._Ellipse.SetColor(R, G, B, a);
            myPrimitive._Ellipse.Draw(x - size, y - size, 2 * size, 2 * size, true);

            //base.Show();
        }

        // ---------------------------------------------------------------------------------------------------------------
    };
};
