using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;


/*
    - ...
*/


namespace my
{
    public class myObj_510 : myObject
    {
        // Priority
        public static int Priority { get { return getPriority(); } }

        private float x, y, X, Y, dx, dy;
        private float size, A, R, G, B, angle = 0;

        private static int N = 0, shape = 0, nSrc = 0, nRcv = 0;
        private static bool doFillShapes = false;
        private static float dimAlpha = 0.05f;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_510()
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
                nSrc = rand.Next(25) + 3;
                nRcv = rand.Next(25) + 3;

                N = nSrc + nRcv + 1000;

                shape = rand.Next(5);
            }

            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time local initialization
        private void initLocal()
        {
            doClearBuffer = myUtils.randomBool(rand);
            doFillShapes  = myUtils.randomBool(rand);

            float _R = 0, _G = 0, _B = 0;

            // Source nodes
            {
                myUtils.getRandomColor(rand, ref _R, ref _G, ref _B, min: 0.5f);

                for (int i = 0; i < nSrc; i++)
                {
                    if (list.Count < i + 1)
                    {
                        list.Add(new myObj_510());
                    }

                    myObj_510 obj = list[i] as myObj_510;

                    obj.x = rand.Next(gl_Width);
                    obj.y = rand.Next(gl_Height);

                    obj.dx = 0;
                    obj.dy = 0;

                    obj.dx = myUtils.randFloat(rand, 0.1f) * 1.5f;
                    obj.dy = myUtils.randFloat(rand, 0.1f) * 1.5f;

                    obj.size = 10;
                    obj.R = _R;
                    obj.G = _G;
                    obj.B = _B;
                    obj.A = 0.5f;
                }
            }

            // Receiving nodes
            {
                myUtils.getRandomColor(rand, ref _R, ref _G, ref _B, min: 0.5f);

                for (int i = nSrc; i < nSrc + nRcv; i++)
                {
                    if (list.Count < i + 1)
                    {
                        list.Add(new myObj_510());
                    }

                    myObj_510 obj = list[i] as myObj_510;

                    obj.x = rand.Next(gl_Width);
                    obj.y = rand.Next(gl_Height);
                    obj.dx = obj.dy = 0;

                    obj.size = 10;
                    obj.R = _R;
                    obj.G = _G;
                    obj.B = _B;
                    obj.A = 0.5f;
                }
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            height = 600;

            string nStr(int   n) { return n.ToString("N0");    }
            //string fStr(float f) { return f.ToString("0.000"); }

            string str = $"Obj = myObj_510\n\n"                      +
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
            if (id > nSrc + nRcv)
            {
                int i = rand.Next(nSrc);
                int j = rand.Next(nRcv) + nSrc;

                x = (list[i] as myObj_510).x;
                y = (list[i] as myObj_510).y;
                X = (list[j] as myObj_510).x;
                Y = (list[j] as myObj_510).y;

                dx = X - x;
                dy = Y - y;

                float dist = (float)(Math.Sqrt(dx * dx + dy * dy));

                float spd = 1.0f;

                spd = myUtils.randFloat(rand, 0.1f) * 0.5f * (rand.Next(5) + 1);

                dx = (dx / dist) * spd;
                dy = (dy / dist) * spd;

                size = 3;

                A = 0.5f;
                colorPicker.getColor(x, y, ref R, ref G, ref B);
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            bool checkCollision(float val, float dVal, float target)
            {
                bool sign1 = (target - val) >= 0;
                bool sign2 = (target - val - dVal) >= 0;

                return (sign1 != sign2);
            }

            x += dx;
            y += dy;

            if (id < nSrc + nRcv)
            {
                if (x < 0)
                    dx += myUtils.randFloat(rand) * 0.05f;

                if (x > gl_Width)
                    dx -= myUtils.randFloat(rand) * 0.05f;

                if (y < 0)
                    dy += myUtils.randFloat(rand) * 0.05f;

                if (y > gl_Height)
                    dy -= myUtils.randFloat(rand) * 0.05f;
            }
            else
            {
                if (checkCollision(x, dx, X) || checkCollision(y, dy, Y) || x < 0 || y < 0 || x > gl_Width || y > gl_Height)
                {
                    generateNew();
                }
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
            initShapes();

            clearScreenSetup(doClearBuffer, 0.1f);

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
                        var obj = list[i] as myObj_510;

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
                    list.Add(new myObj_510());
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

        // ---------------------------------------------------------------------------------------------------------------

        private static int getPriority()
        {
#if DEBUG
            return 9999910;
#endif
            return 10;
        }
    }
};
