﻿using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;


/*
    - Various shapes growing out from a single starting point
    - Based initially on the starfield class -- where all the stars are generated at a center point

    -- case 100 x drawMode 5
*/


namespace my
{
    public class myObj_043 : myObject
    {
        private float x, y, dx, dy;
        private float mass, size, A, R, G, B, angle;
        private bool doCalc;

        private static int N = 0, shape = 0;
        private static bool doClearBuffer = false, doFillShapes = true;
        private static float dimAlpha = 0.5f;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_043()
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

            N = (N == 0) ? 100 + rand.Next(100) : N;

            N = 100;

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            string str = $"Obj = myObj_043\n\n" +
                            $"N = {N} ({list.Count})\n" +
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

            dx = dy = 0;

            mass = rand.Next(3333) + 10;
            size = mass / 333;

            if (size == 0)
                size = 1;

#if false
            if (list.Count == 11)
            {
                mass = 5000000;
                size = 23;
            }
#endif

            A = 1;
            R = (float)rand.NextDouble();
            G = (float)rand.NextDouble();
            B = (float)rand.NextDouble();

            doCalc = true;

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        // https://www.youtube.com/watch?v=0Kx4Y9TVMGg&ab_channel=Brainxyz

        protected override void Move()
        {
            if (doCalc)
            {
                float factor = 0.0015f;

                factor = 0.000001f;

                for (int i = 0; i < list.Count; i++)
                {
                    var obj = list[i] as myObj_043;

                    if (obj != this)
                    {
                        factor = 0.000001f;

                        double distSquared = (x - obj.x) * (x - obj.x) + (y - obj.y) * (y - obj.y);

                        float ddx = 0, ddy = 0;
                        float dist = (float)Math.Sqrt(distSquared);

                        if (dist > 0)
                        {
                            ddx = obj.mass * (obj.x - x) / dist;
                            ddy = obj.mass * (obj.y - y) / dist;
/*
                            // Some distance factor here:
                            // The farther away are the 2 objects, the lesser is the force between them
                            factor /= dist;
                            factor *= 100;

                            if (dist > 123)
                            {
                                factor = 0;
                            }
*/

                            if (dist >= 200)
                            {
                                factor /= (dist/10);
                                factor  = 0;
                            }
                        }

                        dx += factor * ddx;
                        dy += factor * ddy;
                    }
                }

                doCalc = false;
            }
            else
            {
                doCalc = true;
                x += dx;
                y += dy;
            }
/*
            For 2 points, the center of masses MC lies somewhere on the line between them.
            The distance from pt1 to MC, d = DIST / (m1/m2 + 1);
 */
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

            // Disable VSYNC if needed
            Glfw.SwapInterval(0);

            if (doClearBuffer)
            {
                glDrawBuffer(GL_FRONT_AND_BACK | GL_DEPTH_BUFFER_BIT);
                glClearColor(0, 0, 0, 1);
            }

            while (list.Count < N)
            {
                list.Add(new myObj_043());
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
                else
                {
                    dimScreen(false);
                }

                // Render Frame
                {
                    inst.ResetBuffer();

                    for (int i = 0; i < list.Count; i++)
                    {
                        var obj = list[i] as myObj_043;

                        obj.Show();
                        obj.Move();
                    }

                    for (int i = 0; i < list.Count; i++)
                    {
                        var obj = list[i] as myObj_043;
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

        // Dim the screen constantly
        private void dimScreen(bool useStrongerDimFactor = false)
        {
            int rnd = rand.Next(101), dimFactor = 1;

            if (useStrongerDimFactor && rnd < 11)
            {
                dimFactor = (rnd == 0) ? 5 : 2;
            }

            myPrimitive._Rectangle.SetAngle(0);

            // Shift background color just a bit, to hide long lasting traces of shapes
            myPrimitive._Rectangle.SetColor(rand.Next(5) * 0.01f, rand.Next(5) * 0.01f, rand.Next(5) * 0.01f, dimAlpha * dimFactor);
            myPrimitive._Rectangle.Draw(0, 0, gl_Width, gl_Height, true);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------
    }
};
