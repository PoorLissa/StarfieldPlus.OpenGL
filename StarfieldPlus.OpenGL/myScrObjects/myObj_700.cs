using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;


/*
    - Straight lines that reflect backwards
*/


namespace my
{
    public class myObj_700 : myObject
    {
        // Priority
        public static int Priority => 10;
		public static System.Type Type => typeof(myObj_700);

        private int cnt;
        private float x, y, dx, dy;
        private float size, A, R, G, B, angle = 0;

        private static int N = 0, shape = 0, nTrail = 0;
        private static bool doFillShapes = false;
        private static float dimAlpha = 0.05f;

        private myParticleTrail trail = null;

        private static myScreenGradient grad = null;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_700()
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
                N = 1111;

                shape = rand.Next(5);

                nTrail = 300 + rand.Next(111);
            }

            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time local initialization
        private void initLocal()
        {
            doClearBuffer = myUtils.randomBool(rand);
            doClearBuffer = true;

            renderDelay = 1;

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
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void generateNew()
        {
            cnt = 0;

            x = rand.Next(gl_Width);
            y = rand.Next(gl_Height);

            dx = myUtils.randFloatSigned(rand) * (rand.Next(23) + 3);
            dy = myUtils.randFloatSigned(rand) * (rand.Next(23) + 3);

            size = 3;

            A = myUtils.randFloat(rand, 0.1f) * 0.85f;
            colorPicker.getColor(x, y, ref R, ref G, ref B);

            // Initialize Trail
            {
                if (trail == null)
                {
                    trail = new myParticleTrail(nTrail, x, y);
                }
                else
                {
                    trail.reset(x, y);
                }

                trail.updateDa(A * 1);
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            if (trail != null)
            {
                trail.update(x, y);
            }

            x += dx;
            y += dy;

            if (x < 0 && dx < 0)
            {
                dx *= -1 + myUtils.randFloatSigned(rand) * 0.0001f;
                dy *= -1 + myUtils.randFloatSigned(rand) * 0.0001f;
            }

            if (y < 0 && dy < 0)
            {
                dx *= -1 + myUtils.randFloatSigned(rand) * 0.0001f;
                dy *= -1 + myUtils.randFloatSigned(rand) * 0.0001f;
            }

            if (x > gl_Width && dx > 0)
            {
                dx *= -1 + myUtils.randFloatSigned(rand) * 0.0001f;
                dy *= -1 + myUtils.randFloatSigned(rand) * 0.0001f;
            }

            if (y > gl_Height && dy > 0)
            {
                dx *= -1 + myUtils.randFloatSigned(rand) * 0.0001f;
                dy *= -1 + myUtils.randFloatSigned(rand) * 0.0001f;
            }

            if (cnt == 0 && myUtils.randomChance(rand, 1, 999))
            {
                cnt = 100 + rand.Next(123);
            }

            if (cnt > 0)
            {
                cnt--;

                if (myUtils.randomChance(rand, 1, 23))
                    dx *= -1;

                if (myUtils.randomChance(rand, 1, 23))
                    dy *= -1;
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Show()
        {
            if (trail != null)
            {
                trail.Show(R, G, B, A);
            }

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

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Process(Window window)
        {
            uint cnt = 0;
            initShapes();

            // Disable VSYNC if needed
            // Glfw.SwapInterval(0);

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
                    myPrimitive._LineInst.ResetBuffer();

                    for (int i = 0; i != Count; i++)
                    {
                        var obj = list[i] as myObj_700;

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

                if (Count < N && myUtils.randomChance(rand, 1, 23))
                {
                    list.Add(new myObj_700());
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
            grad.SetRandomColors(rand, 0.2f, 0);

            myPrimitive.init_LineInst(N * nTrail);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------
    }
};
