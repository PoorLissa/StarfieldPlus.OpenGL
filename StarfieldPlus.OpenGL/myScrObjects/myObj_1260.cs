using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;


/*
    - 
*/


namespace my
{
    public class myObj_1260 : myObject
    {
        // Priority
        public static int Priority => 10;
		public static System.Type Type => typeof(myObj_1260);

        private float x, y, dx, dy;
        private float size, a, A, R, G, B, angle, dAngle;

        private static int N = 0, n = 0, shape = 0;
        private static bool doFillShapes = false;
        private static float dimAlpha = 0.05f;
        private static float X, Y, X0, Y0, Rad, maxOpacity = 1;

        private static myScreenGradient grad = null;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_1260()
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
                N = 1000;
                n = 1;

                shape = rand.Next(5);
            }

            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time local initialization
        private void initLocal()
        {
            doClearBuffer = myUtils.randomChance(rand, 1, 2);

            X = gl_x0;
            Y = gl_y0;
            Rad = 333 + rand.Next(333);
            maxOpacity = 0.5f;

            X0 = X + rand.Next((int)Rad * 2) - Rad;
            Y0 = Y + rand.Next((int)Rad * 2) - Rad;

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            height = 600;

            string str = $"Obj = {Type}\n\n"                  +
                            myUtils.strCountOf(list.Count, N) +
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
            if (id < n)
            {
                x = 0;
                y = 0;

                x = rand.Next(gl_Width);
                y = rand.Next(gl_Height);

                dx = dy = 0;
            }
            else
            {
                var parent = list[rand.Next(n)] as myObj_1260;

                x = parent.x;
                y = parent.y;

                var dX = x - X0;
                var dY = y - Y0;

                var dist = (float)Math.Sqrt(dX * dX + dY * dY);

                float spd = -5.0f - myUtils.randFloat(rand) * 2;

                dx = spd * dX / dist;
                dy = spd * dY / dist;

                angle = 0;
                dAngle = myUtils.randFloat(rand) * 0.0001f * myUtils.randomSign(rand);

                //B = ((float)Math.Abs(spd) - 5) / 2;
            }

            size = 2;

            a = 0;
            A = myUtils.randFloat(rand) * 0.75f;
            R = (float)rand.NextDouble();
            G = (float)rand.NextDouble();
            B = (float)rand.NextDouble();

            //R = G = 0.33f;

            //colorPicker.getColor(x, y, ref R, ref G, ref B);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            if (id >= n)
            {
                x += dx;
                y += dy;
                angle += dAngle;

                if (a < A)
                {
                    a += 0.001f;
                }

                if (x > X - Rad && x < X + Rad)
                {
                    if (y > Y - Rad && y < Y + Rad)
                    {
                        float dX = X - x;
                        float dY = Y - y;

                        float dist = (float)Math.Sqrt(dX * dX + dY * dY);

                        if (dist <= Rad)
                        {
                            float sp_dist = 0.05f / dist;

                            dx -= dX * sp_dist;
                            dy -= dY * sp_dist;
                        }
                    }
                }

                if (x < 0 || x > gl_Width || y < 0 || y > gl_Height)
                {
                    generateNew();
                }
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Show()
        {
            if (id >= n)
            {
                float size2x = size * 2;

                switch (shape)
                {
                    // Instanced squares
                    case 0:
                        myPrimitive._RectangleInst.setInstanceCoords(x - size, y - size, size2x, size2x);
                        myPrimitive._RectangleInst.setInstanceColor(R, G, B, a);
                        myPrimitive._RectangleInst.setInstanceAngle(angle);
                        break;

                    // Instanced triangles
                    case 1:
                        myPrimitive._TriangleInst.setInstanceCoords(x, y, size2x, angle);
                        myPrimitive._TriangleInst.setInstanceColor(R, G, B, a);
                        break;

                    // Instanced circles
                    case 2:
                        myPrimitive._EllipseInst.setInstanceCoords(x, y, size2x, angle);
                        myPrimitive._EllipseInst.setInstanceColor(R, G, B, a);
                        break;

                    // Instanced pentagons
                    case 3:
                        myPrimitive._PentagonInst.setInstanceCoords(x, y, size2x, angle);
                        myPrimitive._PentagonInst.setInstanceColor(R, G, B, a);
                        break;

                    // Instanced hexagons
                    case 4:
                        myPrimitive._HexagonInst.setInstanceCoords(x, y, size2x, angle);
                        myPrimitive._HexagonInst.setInstanceColor(R, G, B, a);
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

            float lineThickness = 1.0f;
            float dThickneess = 0.025f;

            clearScreenSetup(doClearBuffer, 0.1f);

            stopwatch = new StarfieldPlus.OpenGL.myUtils.myStopwatch(true);

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

                    // Draw event horizon
                    {
                        myPrimitive._Ellipse.setLineThickness(lineThickness);
                        myPrimitive._Ellipse.SetColor(1, 1, 1, 0.05f);
                        myPrimitive._Ellipse.Draw(X - Rad, Y - Rad, 2 * Rad, 2 * Rad, false);

                        lineThickness += dThickneess;

                        if (lineThickness > 13.0f || lineThickness < 1.0f)
                            dThickneess *= -1;
                    }

                    for (int i = 0; i != Count; i++)
                    {
                        var obj = list[i] as myObj_1260;

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
                    list.Add(new myObj_1260());
                }

                stopwatch.WaitAndRestart();
                cnt++;
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

            myPrimitive.init_Ellipse();

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------
    }
};
