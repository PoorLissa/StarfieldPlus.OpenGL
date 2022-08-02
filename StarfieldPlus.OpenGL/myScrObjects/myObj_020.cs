using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;


/*
    - Linearly Moving Circles (Soap Bubbles)
*/


namespace my
{
    public class myObj_020 : myObject
    {
        private float x, y, dx, dy, Size, dSize, angle, dAngle, A = 0, R = 0, G = 0, B = 0;
        int lifeCounter = 0;

        private static int shape = 0, N = 0, shapeCnt = 1;
        private static bool doFillShapes = false;
        private static float spdConst = 0;

        private static myInstancedPrimitive inst = null;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_020()
        {
            if (colorPicker == null)
            {
                colorPicker = new myColorPicker(gl_Width, gl_Height);
                list = new List<myObject>();

                init();
            }

            generateNew();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time initialization
        private void init()
        {
            N = 3333 + rand.Next(6666);
            renderDelay = 10;
            spdConst = (float)(rand.Next(50) + 1) / 1000;

            doFillShapes = myUtils.randomBool(rand);

            shape = rand.Next(5);
            shapeCnt = rand.Next(5) + 1;

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            return $"Obj = myObj_020\n\n" +
                            $"N = {N}\n" +
                            $"spdConst = {spdConst}\n" +
                            $"shapeCnt = {shapeCnt}\n"
                            ;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void generateNew()
        {
            lifeCounter = rand.Next(100) + 100;

            int x0 = rand.Next(gl_Width);
            int y0 = rand.Next(gl_Height);
            int speed = rand.Next(2000) + 10;

            do
            {
                x = rand.Next(gl_Width);
                y = rand.Next(gl_Height);
            }
            while (x == x0 && y == y0);

            double dist = Math.Sqrt((x - x0) * (x - x0) + (y - y0) * (y - y0));
            double sp_dist = spdConst * speed / dist;

            dx = (float)((x - x0) * sp_dist);
            dy = (float)((y - y0) * sp_dist);

            Size = 1;
            dSize = 0.0005f * (rand.Next(1000) + 1);

            A = (float)rand.NextDouble();

            angle = 0;
            dAngle = myUtils.randomSign(rand) * (float)rand.NextDouble() * 0.1f;

            colorPicker.getColor(x, y, ref R, ref G, ref B);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            if (Size != 0)
            {
                x += dx;
                y += dy;
                angle += dAngle;
                Size += dSize;
                A -= 0.001f;

                if (A < 0 || x < -Size || x > gl_Width + Size || y < -Size || y > gl_Height + Size)
                {
                    x = -1111;
                    y = -1111;
                    Size = 0;
                }
            }
            else
            {
                if (lifeCounter-- == 0)
                {
                    generateNew();
                }
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Show()
        {
            if (Size > 0)
            {
                float oldSize = Size;

                for (int i = 1; i < shapeCnt + 1; i++)
                {
                    Size = oldSize / i;

                    switch (shape)
                    {
                        case 0:
                            var rectInst = inst as myRectangleInst;

                            rectInst.setInstanceCoords(x - Size, y - Size, 2 * Size, 2 * Size);
                            rectInst.setInstanceColor(R, G, B, A/i);
                            rectInst.setInstanceAngle(angle/i);
                            break;

                        case 1:
                            var triangleInst = inst as myTriangleInst;

                            triangleInst.setInstanceCoords(x, y, Size, angle/i);
                            triangleInst.setInstanceColor(R, G, B, A/i);
                            break;

                        case 2:
                            var ellipseInst = inst as myEllipseInst;

                            ellipseInst.setInstanceCoords(x, y, 2 * Size, angle/i);
                            ellipseInst.setInstanceColor(R, G, B, A/i);
                            break;

                        case 3:
                            var pentagonInst = inst as myPentagonInst;

                            pentagonInst.setInstanceCoords(x, y, 2 * Size, angle/i);
                            pentagonInst.setInstanceColor(R, G, B, A/i);
                            break;

                        case 4:
                            var hexagonInst = inst as myHexagonInst;

                            hexagonInst.setInstanceCoords(x, y, 2 * Size, angle/i);
                            hexagonInst.setInstanceColor(R, G, B, A/i);
                            break;
                    }
                }

                Size = oldSize;
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Process(Window window)
        {
            uint cnt = 0;
            initShapes();

            while (!Glfw.WindowShouldClose(window))
            {
                processInput(window);

                // Swap fore/back framebuffers, and poll for operating system events.
                Glfw.SwapBuffers(window);
                Glfw.PollEvents();

                glClearColor(0, 0, 0, 1);
                glClear(GL_COLOR_BUFFER_BIT);

                inst.ResetBuffer();

                for (int i = 0; i < list.Count; i++)
                {
                    var obj = list[i] as myObj_020;

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

                if (list.Count < N)
                {
                    list.Add(new myObj_020());
                }

                cnt++;

                System.Threading.Thread.Sleep(renderDelay);
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void initShapes()
        {
            int lineN = N * 3, shapeN = N * shapeCnt;

            myPrimitive.init_Rectangle();

            //myPrimitive.init_LineInst(lineN);

            int rotationSubMode = rand.Next(5) == 0 ? rand.Next(4) : 0;

            switch (shape)
            {
                case 0:
                    myPrimitive.init_RectangleInst(shapeN);
                    myPrimitive._RectangleInst.setRotationMode(rotationSubMode);
                    inst = myPrimitive._RectangleInst;
                    break;

                case 1:
                    myPrimitive.init_TriangleInst(shapeN);
                    myPrimitive._TriangleInst.setRotationMode(rotationSubMode);
                    inst = myPrimitive._TriangleInst;
                    break;

                case 2:
                    myPrimitive.init_EllipseInst(shapeN);
                    myPrimitive._EllipseInst.setRotationMode(rotationSubMode);
                    inst = myPrimitive._EllipseInst;
                    break;

                case 3:
                    myPrimitive.init_PentagonInst(shapeN);
                    myPrimitive._PentagonInst.setRotationMode(rotationSubMode);
                    inst = myPrimitive._PentagonInst;
                    break;

                case 4:
                    myPrimitive.init_HexagonInst(shapeN);
                    myPrimitive._HexagonInst.setRotationMode(rotationSubMode);
                    inst = myPrimitive._HexagonInst;
                    break;
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------
    };
};
