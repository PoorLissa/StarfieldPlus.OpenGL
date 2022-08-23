using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;


/*
    - Growing shapes -- Rain drops alike
*/


namespace my
{
    public class myObj_131 : myObject
    {
        private int x, y, dx, dy, maxSize;
        private float size, dSize, A, R, G, B, angle, dA, dAngle;

        private static int N = 0, shape = 0, rotationMode = 0;
        private static bool doClearBuffer = true, doFillShapes = false;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_131()
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
            gl_x0 = gl_Width  / 2;
            gl_y0 = gl_Height / 2;

            N = (N == 0) ? 333 + rand.Next(111) : N;

            doFillShapes = myUtils.randomChance(rand, 1, 3);

            shape = rand.Next(5);
            rotationMode = rand.Next(3);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            height = 800;

            string str = $"Obj = myObj_131\n\n" +
                            $"N = {N}\n" +
                            $""
                ;
            return str;
        }

        // ---------------------------------------------------------------------------------------------------------------

        // 
        protected override void setNextMode()
        {
            init();
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void generateNew()
        {
            x = rand.Next(gl_Width);
            y = rand.Next(gl_Height);

            size = 0;
            maxSize = rand.Next(333) + 33;
            dSize = 0.1f * (rand.Next(11) + 1);

            angle = 0;

            if (rotationMode > 0)
                angle = (float)rand.NextDouble();

            if (rotationMode > 1)
                dAngle = (float)rand.NextDouble() / 11 * myUtils.randomSign(rand);

            colorPicker.getColor(x, y, ref R, ref B, ref G);
            A = 0.85f + (float)rand.NextDouble() / 4;
            dA = 0.01f * (rand.Next(11) + 1);
            //dA = 0.001f * (rand.Next(111) + 1);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
/*
            x += rand.Next(3) - 1;
            y += rand.Next(3) - 1;
*/

            size += dSize;

            // Increase disappearing speed when max size is reached
            if (size > maxSize)
            {
                dA *= 1.1f;
            }

            // Decrease opacity until fully invisible
            A -= dA;

            if (A < 0)
            {
                generateNew();
            }

            angle += dAngle;

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Show()
        {
            switch (shape)
            {
                // Instanced squares
                case 0:
                    var rectInst = inst as myRectangleInst;

                    rectInst.setInstanceCoords(x - size, y - size, 2 * size, 2 * size);
                    rectInst.setInstanceColor(R, G, B, A);
                    rectInst.setInstanceAngle(angle);
                    break;

                // Instanced triangles
                case 1:
                    var triangleInst = inst as myTriangleInst;

                    triangleInst.setInstanceCoords(x, y, 2 * size, angle);
                    triangleInst.setInstanceColor(R, G, B, A);
                    break;

                // Instanced circles
                case 2:
                    var ellipseInst = inst as myEllipseInst;

                    ellipseInst.setInstanceCoords(x, y, 2 * size, angle);
                    ellipseInst.setInstanceColor(R, G, B, A);
                    break;

                // Instanced pentagons
                case 3:
                    var pentagonInst = inst as myPentagonInst;

                    pentagonInst.setInstanceCoords(x, y, 2 * size, angle);
                    pentagonInst.setInstanceColor(R, G, B, A);
                    break;

                // Instanced hexagons
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

            if (doClearBuffer)
            {
                glDrawBuffer(GL_FRONT_AND_BACK | GL_DEPTH_BUFFER_BIT);
                glClearColor(0, 0, 0, 1);
            }

            while (!Glfw.WindowShouldClose(window))
            {
                cnt++;

                processInput(window);

                // Swap fore/back framebuffers, and poll for operating system events.
                Glfw.SwapBuffers(window);
                Glfw.PollEvents();

                if (doClearBuffer)
                {
                    glClear(GL_COLOR_BUFFER_BIT);
                }

                // Render Frame
                {
                    inst.ResetBuffer();

                    for (int i = 0; i < list.Count; i++)
                    {
                        var obj = list[i] as myObj_131;

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
                    list.Add(new myObj_131());
                }

                System.Threading.Thread.Sleep(renderDelay);
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void initShapes()
        {
            myPrimitive.init_Rectangle();
            base.initShapes(shape, N, 0);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------
    }
};
