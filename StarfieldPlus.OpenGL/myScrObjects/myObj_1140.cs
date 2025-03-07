using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;
using StarfieldPlus.OpenGL.myUtils;


/*
    - Spiraling particles with long tails
*/


namespace my
{
    public class myObj_1140 : myObject
    {
        // Priority
        public static int Priority => 9999910;
		public static System.Type Type => typeof(myObj_1140);

        private float x, y, t, dt, tRad, dtRad;
        private float size, A, R, G, B, rad, Rad, angle = 0, dAngle;

        private myParticleTrail trail = null;

        private static int N = 0, shape = 0, nTrail = 100, dirMode = 0, moveMode = 0;
        private static bool doFillShapes = false;
        private static float dimAlpha = 0.05f;

        private static myScreenGradient grad = null;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_1140()
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
                N = 105;

                shape = rand.Next(5);
            }

            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time local initialization
        private void initLocal()
        {
            doClearBuffer = myUtils.randomChance(rand, 10, 11);
            doFillShapes = myUtils.randomChance(rand, 4, 5);

            dirMode = rand.Next(3);
            moveMode = rand.Next(2);

moveMode = 1;

            renderDelay = rand.Next(3) + 1;

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            height = 600;

            string str = $"Obj = {Type}\n\n"                  +
                            myUtils.strCountOf(list.Count, N) +
                            $"renderDelay = {renderDelay}\n"  +
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
            rad = Rad = 100 + rand.Next(gl_x0);
            t = 0;
            tRad = 0;
            dt = 0.025f + myUtils.randFloat(rand) * 0.01f;

            dtRad = myUtils.randFloat(rand) * 0.001f;

            x = gl_x0 + (float)Math.Sin(t) * rad;
            y = gl_y0 + (float)Math.Cos(t) * rad;

            size = rand.Next(9) + 3;

            dAngle = myUtils.randFloatSigned(rand, 0.1f) * 0.01f;

            A = 0.35f + myUtils.randFloat(rand) * 0.35f;

            do
            {
                R = (float)rand.NextDouble();
                G = (float)rand.NextDouble();
                B = (float)rand.NextDouble();
            }
            while (R + G + B < 0.25f);

            switch (dirMode)
            {
                case 0:
                case 1:
                    dt *= (dirMode == 0 ? +1 : -1);
                    break;

                case 2:
                    dt *= myUtils.randomSign(rand);
                    break;
            }

            // Initialize trail
            {
                if (trail == null)
                {
                    trail = new myParticleTrail(nTrail, x, y);
                }
                else
                {
                    trail.reset(x, y);
                }

                trail?.updateDa(A);
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            x = gl_x0 + (float)Math.Sin(t) * rad;
            y = gl_y0 + (float)Math.Cos(t) * rad;

            t += dt;
            tRad += dtRad;
            angle += dAngle;

            switch (moveMode)
            {
                case 0:
                    rad = Rad;
                    break;

                case 1:
                    rad = Rad + (float)Math.Sin(tRad) * 10;
                    break;
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Show()
        {
            float size2x = size * 2;

            trail.update(x, y);
            trail.Show(R, G, B, A);

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

            stopwatch = new myStopwatch();
            stopwatch.Start();

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
                        var obj = list[i] as myObj_1140;

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

                if (Count < N)
                {
                    list.Add(new myObj_1140());
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

            myPrimitive.init_LineInst(N * nTrail);
            myPrimitive._LineInst.setLineWidth(2);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------
    }
};
