using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;


/*
    - 
*/


namespace my
{
    public class myObj_1520 : myObject
    {
        // Priority
        public static int Priority => 999910;
		public static System.Type Type => typeof(myObj_1520);

        private float x, y, dx, dy;
        private float size, A, R, G, B, angle = 0, phase = 0;

        private static int N = 0, shape = 0, nTrail = 300, mode = 0, dir = 0;
        private static bool doFillShapes = false;
        private static float dimAlpha = 0.05f, t = 0, dt = 0;
        private myObj_1520 _parent = null;

        private myParticleTrail trail = null;

        private static myScreenGradient grad = null;
        private static myFreeShader_001 shader = null;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_1520()
        {
            if (id != uint.MaxValue)
            {
                if (id % 2 != 0)
                {
                    _parent = list[(int)(id - 1)] as myObj_1520;
                }

                generateNew();
            }
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time global initialization
        protected override void initGlobal()
        {
            colorPicker = new myColorPicker(gl_Width, gl_Height);
            list = new List<myObject>();

            // Global unmutable constants
            {
                switch (rand.Next(3))
                {
                    case 0:
                        N = 200 + rand.Next(111);
                        break;

                    case 1:
                        N = 200 + rand.Next(222);
                        break;

                    case 2:
                        N = 200 + rand.Next(333);
                        break;
                }

                N += N % 2;

                shape = rand.Next(5);

                t = 0;
                dt = 0.001f;
            }

            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time local initialization
        private void initLocal()
        {
            doClearBuffer = true;

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
            if (_parent == null)
            {
                x = rand.Next(gl_Width);
                y = rand.Next(gl_Height);

                A = 0.25f + myUtils.randFloat(rand) * 0.75f;
                size = 9 * A;
                phase = 1.0f + myUtils.randFloat(rand) * 0.1f;
                //phase = 1.0f;

                colorPicker.getColor(x, y, ref R, ref G, ref B);

                switch (dir)
                {
                    case 0:
                        y = 10;
                        dx = 0;
                        dy = myUtils.randFloat(rand, 0.5f) + (rand.Next(3) + 1);
                        break;
                }

                if (shape == 1)
                {
                    angle = (float)(Math.PI * 45);
                }
            }
            else
            {
                x = _parent.x;
                y = _parent.y;
                dx = _parent.dx;
                dy = _parent.dy;
                size = _parent.size;
                A = _parent.A;
                R = _parent.R;
                G = _parent.G;
                B = _parent.B;
                angle = _parent.angle;
                phase = _parent.phase;
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
            x += dx;
            y += dy;

            if (_parent != null)
            {
                dx = +(float)Math.Sin(t * 100 * phase) * 1;
            }
            else
            {
                dx = -(float)Math.Sin(t * 100 * phase) * 1;
            }

            switch (dir)
            {
                case 0:
                    if (y > gl_Height)
                    {
                        A -= 0.002f;

                        if (A < 0)
                            generateNew();
                    }
                    break;
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Show()
        {
            float size2x = size * 2;
            float size3x = size * 3;

            trail?.update(x, y);
            trail?.Show(R, G, B, A);

            int off = 150 + (int)(size * 0.25f);
            shader.SetColor(R, G, B, A);
            shader.Draw(x, y, size3x, size3x, 0.02f, off);

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
                        var obj = list[i] as myObj_1520;

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
                    list.Add(new myObj_1520());
                    list.Add(new myObj_1520());
                }

                stopwatch.WaitAndRestart();
                cnt++;
                t += dt;
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
            myPrimitive._LineInst.setLineWidth(3);
            myPrimitive._LineInst.setAntialized(true);

            getShader();

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        // General shader selector
        private void getShader()
        {
            string fHeader = "", fMain = "";

            myFreeShader_001.getShader_000(ref fHeader, ref fMain, 0);

            shader = new myFreeShader_001(fHeader, fMain);
        }

        // ---------------------------------------------------------------------------------------------------------------
    }
};
