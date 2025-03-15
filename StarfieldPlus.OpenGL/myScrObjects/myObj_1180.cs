using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;


/*
    - 
*/


namespace my
{
    public class myObj_1180 : myObject
    {
        // Priority
        public static int Priority => 10;
		public static System.Type Type => typeof(myObj_1180);

        private float x, y, dx, dy;
        private float size, rad, A, R, G, B, angle = 0, dAngle, alpha = 0;

        private static int N = 0, shape = 0, mode = 0;
        private static bool doFillShapes = false;
        private static float dimAlpha = 0.05f, dAlpha = 0, Rad = 0, t = 0, dt = 0;

        private static myScreenGradient grad = null;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_1180()
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
                N = 1000;

                shape = rand.Next(5);
            }

            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time local initialization
        private void initLocal()
        {
            doClearBuffer = myUtils.randomChance(rand, 9, 10);

            mode = rand.Next(2);

            Rad = 500;
            dAlpha = (float)(2 * Math.PI / N);
            dt = 0.001f;

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            height = 600;

            string str = $"Obj = {Type}\n\n"                     +
                            myUtils.strCountOf(list.Count, N)    +
                            $"mode = {mode}\n"                   +
                            $"doClearBuffer = {doClearBuffer}\n" +
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
            x = gl_x0;
            y = gl_y0;

            rad = Rad;

            alpha = (id == 0)
                ? 0
                : (list[(int)id-1] as myObj_1180).alpha + dAlpha;

            angle = myUtils.randFloat(rand);
            dAngle = myUtils.randFloatClamped(rand, 0.1f) * 0.01f;

            x += (float)Math.Sin(alpha) * rad;
            y += (float)Math.Cos(alpha) * rad;

            size = 2;

            A = 1;
            R = (float)rand.NextDouble();
            G = (float)rand.NextDouble();
            B = (float)rand.NextDouble();

            colorPicker.getColor(x, y, ref R, ref G, ref B);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            angle += dAngle;

            switch (mode)
            {
                case 0:
                    {
                        if (myUtils.randomChance(rand, 1, 111))
                        {
                            rad += myUtils.randFloatSigned(rand) * rand.Next(11);

                            x = gl_x0 + (float)Math.Sin(alpha) * rad;
                            y = gl_y0 + (float)Math.Cos(alpha) * rad;
                        }
                    }
                    break;

                case 1:
                    {
                        //rad += (float)Math.Sin(Math.Cos(id * t)) * 3.1f;
                        //rad += (float)Math.Sin(Math.Cos(id * t)) * 3;

                        rad += (float)Math.Sin(Math.Cos(id * id * 0.01 + 3 * t * alpha)) * 0.2f;

                        x = gl_x0 + (float)Math.Sin(alpha) * rad;
                        y = gl_y0 + (float)Math.Cos(alpha) * rad;
                    }
                    break;
            }
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

            if (list.Count == N)
            {
                myObj_1180 parent = null;

                if (id == 0)
                {
                    parent = list[N - 1] as myObj_1180;
                }
                else
                {
                    parent = list[(int)id - 1] as myObj_1180;
                }

                myPrimitive._LineInst.setInstanceCoords(x, y, parent.x, parent.y);
                myPrimitive._LineInst.setInstanceColor(1, 1, 1, 0.25f);
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
                list.Add(new myObj_1180());
            }

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
                    myPrimitive._LineInst.ResetBuffer();

                    for (int i = 0; i != Count; i++)
                    {
                        var obj = list[i] as myObj_1180;

                        obj.Show();
                        obj.Move();
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

                t += dt;
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

            myPrimitive.init_LineInst(N);
            myPrimitive._LineInst.setLineWidth(2);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------
    }
};
