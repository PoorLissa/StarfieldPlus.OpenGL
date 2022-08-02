using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;


/*
    - todo -- provide description
*/


namespace my
{
    public class myObj_040 : myObject
    {
        private float x, y, dx, dy, size, R, G, B, A, dA, angle, dAngle;
        int oldX = 0, oldY = 0, initX = 0, initY = 0, repeaterCnt = 0, rnd1 = 0, rnd2 = 0;

        static float dimAlpha = 0;
        static int N = 0, shape = 0, moveType = 0, dimRate = 0, t = 0, maxSize = 0;

        private static myInstancedPrimitive inst = null;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_040()
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
            gl_x0 = gl_Width / 2;
            gl_y0 = gl_Height / 2;

            N = 1111;
            renderDelay = 10;

            shape = rand.Next(5);
            moveType = rand.Next(5);
            t = 15 + rand.Next(11);

            moveType = 0;
            dimAlpha = 0.02f;

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            return $"Obj = myObj_040\n\n" +
                            $"N = {N}\n"
                            ;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void generateNew()
        {
            dx = 0;
            dy = 0;

            rnd1 = rand.Next(gl_Width);
            rnd2 = rand.Next(gl_Width);

            x = rand.Next(gl_Width);
            y = rand.Next(gl_Height);

            oldX = initX = (int)x;
            oldY = initY = (int)y;

            repeaterCnt = 0;

            float speed = 1.5f + 0.1f * rand.Next(30);
            float dist = (float)(Math.Sqrt((x - gl_x0) * (x - gl_x0) + (y - gl_y0) * (y - gl_y0)));

            if (dimRate == 1)
                speed *= 2.0f;

            if (dimRate == 2)
                speed *= 1.33f;

            dx = (x - gl_x0) * speed / dist;
            dy = (y - gl_y0) * speed / dist;

            size = rand.Next(maxSize) + 1;
            angle = 0;
            dAngle = 0.001f * rand.Next(111) * myUtils.randomSign(rand);

            R = 1;
            G = 1;
            B = 1;
            A = 0;

/*
            switch (moveType)
            {
                case 0:
                    da = 0.8f + 0.05f * rand.Next(8);
                    break;

                case 1:
                    da = 0.2f;
                    break;

                default:
                    da = 0.3f + 0.01f * rand.Next(33);
                    break;
            }
*/

            dA = 0.01f;

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            switch (moveType)
            {
                case 0: move0(); break;
                //case 1: move1(); break;
                //case 2: move2(); break;
                //case 3: move3(); break;
                //case 4: move4(); break;
            }

            if (A < 1.0f)
                A += dA;

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Show()
        {
            switch (shape)
            {
                case 0:
                    var rectInst = inst as myRectangleInst;

                    rectInst.setInstanceCoords(x - size, y - size, 2 * size, 2 * size);
                    rectInst.setInstanceColor(R, G, B, A);
                    rectInst.setInstanceAngle(angle);
                    break;

                case 1:
                    var triangleInst = inst as myTriangleInst;

                    triangleInst.setInstanceCoords(x, y, size, angle);
                    triangleInst.setInstanceColor(R, G, B, A);
                    break;

                case 2:
                    var ellipseInst = inst as myEllipseInst;

                    ellipseInst.setInstanceCoords(x, y, 2 * size, angle);
                    ellipseInst.setInstanceColor(R, G, B, A);
                    break;

                case 3:
                    var pentagonInst = inst as myPentagonInst;

                    pentagonInst.setInstanceCoords(x, y, 2 * size, angle);
                    pentagonInst.setInstanceColor(R, G, B, A);
                    break;

                case 4:
                    var hexagonInst = inst as myHexagonInst;

                    hexagonInst.setInstanceCoords(x, y, 2 * size, angle);
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

            glDrawBuffer(GL_FRONT_AND_BACK);

            while (!Glfw.WindowShouldClose(window))
            {
                processInput(window);

                // Swap fore/back framebuffers, and poll for operating system events.
                Glfw.SwapBuffers(window);
                Glfw.PollEvents();

                // Dim the screen constantly
                {
                    myPrimitive._Rectangle.SetAngle(0);

                    // Shift background color just a bit, to hide long lasting traces of shapes
                    myPrimitive._Rectangle.SetColor(rand.Next(5) * 0.01f, rand.Next(5) * 0.01f, rand.Next(5) * 0.01f, dimAlpha);

                    if (rand.Next(11) == -1)
                        myPrimitive._Rectangle.SetColor(rand.Next(5) * 0.01f, rand.Next(5) * 0.01f, rand.Next(5) * 0.01f, dimAlpha * 2);

                    if (rand.Next(33) == -1)
                        myPrimitive._Rectangle.SetColor(rand.Next(5) * 0.01f, rand.Next(5) * 0.01f, rand.Next(5) * 0.01f, dimAlpha * 5);

                    myPrimitive._Rectangle.Draw(0, 0, gl_Width, gl_Height, true);
                }

                inst.ResetBuffer();

                for (int i = 0; i < list.Count; i++)
                {
                    var obj = list[i] as myObj_040;

                    obj.Show();
                    obj.Move();
                }

                inst.SetColorA(0);
                inst.Draw(false);

                if (list.Count < N)
                {
                    list.Add(new myObj_040());
                }

                cnt++;

                System.Threading.Thread.Sleep(renderDelay);
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void initShapes()
        {
            int lineN = N * 3, shapeN = N;

            myPrimitive.init_Rectangle();
            myPrimitive.init_LineInst(lineN);

            int rotationSubMode = 0;

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

        private void move0()
        {
            int rndMax = 500;

            oldX = (int)x;
            oldY = (int)y;

            x += dx;
            y += dy;

            x += 0.01f * rand.Next(rndMax) * myUtils.randomSign(rand);
            y += 0.01f * rand.Next(rndMax) * myUtils.randomSign(rand);

            size += 0.05f;
            angle += dAngle;

            angle = (float)rand.NextDouble();

            //colorPicker.getColor(x, y, ref R, ref G, ref B);

            if (x < 0 || x > gl_Width || y < 0 || y > gl_Height)
            {
                generateNew();
            }
        }

        // ---------------------------------------------------------------------------------------------------------------
    };
};
