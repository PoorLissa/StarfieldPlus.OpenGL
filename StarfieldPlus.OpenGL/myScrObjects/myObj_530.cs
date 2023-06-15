using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;


/*
    - 
*/


namespace my
{
    public class myObj_530 : myObject
    {
        // Priority
        public static int Priority => 9999910;

        private int cnt;
        private float x, y, dx, dy;
        private float size, A, dA, R, G, B, angle = 0, dAngle, r;

        private static int N = 0, shape = 0, Rad = 0, Thickness = 0, maxCnt = 0, rotationMode = 0;
        private static bool doFillShapes = false, doTraceColor = false, doMoveWhileWaiting = false;
        private static float dimAlpha = 0.05f, maxDa = 0;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_530()
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
                N = rand.Next(10000) + 3000;

                N = 33333;

                shape = rand.Next(5);
            }

            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time local initialization
        private void initLocal()
        {
            doClearBuffer = myUtils.randomChance(rand, 12, 13);
            doTraceColor = myUtils.randomChance(rand, 1, 11);
            doFillShapes = myUtils.randomChance(rand, 1, 3);
            doMoveWhileWaiting = myUtils.randomChance(rand, 1, 2);

            rotationMode = rand.Next(6);

            Rad = 333 + rand.Next(333);
            Thickness = rand.Next(111) + 1;
            maxCnt = rand.Next(50) + 10;

            maxDa = 0.001f + myUtils.randFloat(rand) * 0.01f;

            renderDelay = rand.Next(11) + 1;

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            height = 600;

            string nStr(int n) { return n.ToString("N0"); }
            string fStr(float f) { return f.ToString("0.000"); }

            string str = $"Obj = myObj_530\n\n" +
                            $"N = {nStr(list.Count)} of {nStr(N)}\n" +
                            $"Rad = {Rad}\n" +
                            $"Thickness = {Thickness}\n" +
                            $"maxDa = {fStr(maxDa)}\n" +
                            $"maxCnt = {maxCnt}\n" +
                            $"rotationMode = {rotationMode}\n" +
                            $"doMoveWhileWaiting = {doMoveWhileWaiting}\n" +
                            $"renderDelay = {renderDelay}\n" +
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
            cnt = maxCnt;

            r = Rad + rand.Next(Thickness);

            float alpha = myUtils.randFloat(rand) * rand.Next(123);

            x = r * (float)Math.Sin(alpha);
            y = r * (float)Math.Cos(alpha);

            float dist = (float)Math.Sqrt(x * x + y * y);
            float spd = 2.5f;

            dx = x * spd / dist;
            dy = y * spd / dist;

            x += gl_x0;
            y += gl_y0;

            size = rand.Next(5) + 2;

            if (myUtils.randomChance(rand, 1, 2))
            {
                dx *= -1;
                dy *= -1;

                size -= 1;
            }

            colorPicker.getColor(x, y, ref R, ref G, ref B);

            A = myUtils.randFloat(rand) * 0.5f;
            dA = myUtils.randomChance(rand, 1, 1111) ? maxDa / 2 : maxDa;

            switch (rotationMode)
            {
                case 0:
                    angle = 0;
                    dAngle = 0;
                    break;

                case 1:
                case 2:
                    angle = myUtils.randFloat(rand) * rand.Next(123);
                    dAngle = 0;
                    break;

                case 3:
                case 4:
                case 5:
                    angle = myUtils.randFloat(rand) * rand.Next(123);
                    dAngle = myUtils.randFloat(rand) * myUtils.randomSign(rand) * 0.01f;
                    break;
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        // todo:
        // 1. random linear movement starting from the circle curface
        // 2. rotation on different orbits

        protected override void Move()
        {
            angle += dAngle;

            if (cnt > 0)
            {
                cnt--;
/*
                if (doMoveWhileWaiting)
                {
                    x += dx * 0.1f;
                    y += dy * 0.1f;
                }
*/
                x = gl_x0 + r * (float)Math.Sin(angle);
                y = gl_y0 + r * (float)Math.Cos(angle);

                if (cnt == 1)
                {
                    float X = x - gl_x0;
                    float Y = y - gl_y0;

                    float dist = (float)Math.Sqrt(X * X + Y * Y);
                    float spd = 2.5f;

                    dx = X * spd / dist;
                    dy = Y * spd / dist;

                    if (myUtils.randomChance(rand, 1, 2))
                    {
                        dx *= -1;
                        dy *= -1;
                    }
                }
            }
            else
            {
                x += dx;
                y += dy;

                A -= dA;

                if (A <= 0)
                {
                    generateNew();
                }
                else
                {
                    if (doTraceColor)
                    {
                        colorPicker.getColor(x, y, ref R, ref G, ref B);
                    }
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

            // Disable VSYNC if needed
            // Glfw.SwapInterval(0);

            if (doClearBuffer)
            {
                glDrawBuffer(GL_FRONT_AND_BACK | GL_DEPTH_BUFFER_BIT);
                glClearColor(0, 0, 0, 1);
            }
            else
            {
                dimScreenRGB_SetRandom(0.1f);
                glDrawBuffer(GL_FRONT_AND_BACK);
                //glDrawBuffer(GL_DEPTH_BUFFER_BIT);
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

                    for (int i = 0; i != list.Count; i++)
                    {
                        var obj = list[i] as myObj_530;

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
                    list.Add(new myObj_530());
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
};
