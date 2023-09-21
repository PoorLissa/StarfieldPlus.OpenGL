using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;


/*
    - Trails test
*/


namespace my
{
    public class myObj_999_test_001 : myObject
    {
        // Priority
        public static int Priority => 999910;

        private float x, y, dx, dy;
        private float size, A, R, G, B, angle = 0;

        private myParticleTrail trail = null;

        private static int N = 0, shape = 0, nTrailMax = 500;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_999_test_001()
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
                N = rand.Next(100) + 100;
                shape = rand.Next(5);
            }

            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time local initialization
        private void initLocal()
        {
            doClearBuffer = true;
            renderDelay = 0;

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            height = 600;

            string nStr(int   n) { return n.ToString("N0");    }
            //string fStr(float f) { return f.ToString("0.000"); }

            string str = $"Obj = myObj_999_test_001\n\n"             +
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
            x = rand.Next(gl_Width);
            y = rand.Next(gl_Height);

            dx = myUtils.randFloatSigned(rand, 0.01f) * 2;
            dy = myUtils.randFloatSigned(rand, 0.01f) * 2;

            size = rand.Next(3) + 3;

            A = 1.0f;
            colorPicker.getColor(x, y, ref R, ref G, ref B);

            A = 0.33f;

            int nTrail = 10 + rand.Next(nTrailMax);

            // Initialize Trail
            if (trail == null)
            {
                trail = new myParticleTrail(nTrail, x, y);
            }

            if (trail != null)
            {
                trail.updateDa(A);
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            if (false)
            {
                trail.update(x, y);
            }
            else
            {
                // dy / dist = sin(a)
                // dx / dist = cos(a)

                float oldx = x;
                float oldy = y;

                x += dx;
                y += dy;

                float dist = (float)Math.Sqrt(dx * dx + dy * dy);

                float sin = (float)Math.Sin(x) * 15;
                float cos = (float)Math.Cos(x) * 15;

                //trail.update(x * sin, y * sin);
                trail.update(x + sin * dx/dist, y + cos*dy/dist);
            }

            // ---------------------------------------------------------

            if (x < 0 && dx < 0)
                dx *= -1;

            if (y < 0 && dy < 0)
                dy *= -1;

            if (x > gl_Width && dx > 0)
                dx *= -1;

            if (y > gl_Height && dy > 0)
                dy *= -1;

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Show()
        {
            trail.Show(R, G, B, A);

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

                glClear(GL_COLOR_BUFFER_BIT);

                // Render Frame
                {
                    myPrimitive._LineInst.ResetBuffer();
                    inst.ResetBuffer();

                    for (int i = 0; i != Count; i++)
                    {
                        var obj = list[i] as myObj_999_test_001;

                        obj.Show();
                        obj.Move();
                    }

                    myPrimitive._LineInst.Draw();

                    // Tell the fragment shader to do nothing with the existing instance opacity:
                    inst.SetColorA(0);
                    inst.Draw(false);
                }

                if (Count < N)
                {
                    list.Add(new myObj_999_test_001());
                }

                cnt++;
                System.Threading.Thread.Sleep(renderDelay);
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void initShapes()
        {
            base.initShapes(shape, N, 0);
            myPrimitive.init_LineInst(N * nTrailMax);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------
    }
};
