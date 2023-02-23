using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;


/*
    - ...
*/


namespace my
{
    public class myObj_460 : myObject
    {
        private float x, y, dx, dy, oldx, oldy, rad;
        private float size, A, R, G, B, angle = 0;

        private static int N = 0, n = 1, shape = 0, generatorMoveMode = 0, pointDirMode = 0, pointMoveMode = 0;
        private static bool doFillShapes = false, doShowTrails = true, doRandomizeSpeedVector = true;
        private static float dimAlpha = 0.05f, t = 0, dt = 0, timeFactor = 0, gravityFactor = 1;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_460()
        {
            if (id != uint.MaxValue)
            {
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
                N = rand.Next(10000) + 10000;
                n = 1;
                N += n;

                shape = rand.Next(5);
            }

            initLocal();

            t = 0;
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time local initialization
        private void initLocal()
        {
            doClearBuffer = myUtils.randomBool(rand);
            doShowTrails  = myUtils.randomBool(rand);
            doRandomizeSpeedVector = myUtils.randomBool(rand);

            generatorMoveMode = rand.Next(2);
            pointMoveMode     = rand.Next(4);
            pointDirMode      = rand.Next(5);

            dimAlpha = 0.15f;

            dt = 0.033f;

            // Set up dt (The less is dt, the larger becomes final radius)
            {
                dt *= 0.1f;
                //rad /= 3; -- change it in generateNew
            }

            switch (rand.Next(2))
            {
                case 0: dt *= +1; break;
                case 1: dt *= -1; break;
            }

            // Set Time Factor
            {
                int maxTimeFactor = myUtils.randomChance(rand, 1, 7) ? 25 : 11;

                switch (generatorMoveMode)
                {
                    case 0:
                        timeFactor = rand.Next(maxTimeFactor) + 1;
                        break;

                    case 1:
                        timeFactor = rand.Next(maxTimeFactor) + 1 + myUtils.randFloat(rand);
                        break;
                }
            }

            // Set Gravity Factor
            {
                switch (rand.Next(3))
                {
                    case 0:
                        gravityFactor = myUtils.randFloat(rand, 0.1f) * 0.01f;
                        break;

                    case 1:
                        gravityFactor = myUtils.randFloat(rand, 0.1f) * 0.1f;
                        break;

                    case 2:
                        gravityFactor = myUtils.randFloat(rand, 0.1f) * 0.5f;
                        break;
                }
            }

            renderDelay = rand.Next(11) + 3;

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            height = 600;

            string nStr(int   n) { return n.ToString("N0");    }
            string fStr(float f) { return f.ToString("0.000"); }

            string str = $"Obj = myObj_460\n\n"                                     +
                            $"N = {nStr(list.Count)} of {nStr(N)}\n"                +
                            $"doClearBuffer = {doClearBuffer}\n"                    +
                            $"doShowTrails = {doShowTrails}\n"                      +
                            $"doRandomizeSpeedVector = {doRandomizeSpeedVector}\n"  +
                            $"generatorMoveMode = {generatorMoveMode}\n"            +
                            $"pointDirMode = {pointDirMode}\n"                      +
                            $"pointMoveMode = {pointMoveMode}\n"                    +
                            $"timeFactor = {fStr(timeFactor)}\n"                    +
                            $"renderDelay = {renderDelay}\n"                        +
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
            if (id < n)
            {
                rad = 666;
                rad /= 5;

                x = rand.Next(gl_Width);
                y = rand.Next(gl_Height);

                oldx = 0;
                oldy = 0;

                size = 5;
                A = 1;
            }
            else
            {
                var pt = list[0] as myObj_460;

                x = pt.x;
                y = pt.y;

                oldx = pt.oldx;
                oldy = pt.oldy;

                switch (pointDirMode)
                {
                    case 0:
                        dx = oldx - x;
                        dy = oldy - y;
                        break;

                    case 1:
                        dx = x - oldx;
                        dy = y - oldy;
                        break;

                    case 2:
                        dx = oldx - x;
                        dy = y - oldy;
                        break;

                    case 3:
                        dx = x - oldx;
                        dy = oldy - y;
                        break;

                    // mode 0 OR mode 1, randomly
                    case 4:
                        {
                            if (myUtils.randomBool(rand))
                            {
                                dx = oldx - x;
                                dy = oldy - y;
                            }
                            else
                            {
                                dx = x - oldx;
                                dy = y - oldy;
                            }
                        }
                        break;
                }

                // Randomize speed vector just a bit
                if (doRandomizeSpeedVector)
                {
                    float factor = 0.9f + myUtils.randFloat(rand) * 0.2f;

                    dx *= factor;
                    dy *= factor;
                }

                float rate = 1;

                switch (pointMoveMode)
                {
                    // Do nothing
                    case 0:
                        break;

                    // Slow the particles down using the same rate
                    case 1:
                        rate = 0.25f;
                        dx *= rate;
                        dy *= rate;
                        break;

                    // Slow the particles down using random rate
                    case 2:
                        {
                            rate = myUtils.randFloat(rand, 0.05f);
                            dx *= rate;
                            dy *= rate;
                        }
                        break;

                    // Gravity mode
                    case 3:
                        {
                            dx = 0;
                            dy = 0;
                        }
                        break;
                }

                size = rand.Next(1) + 3;
                A = myUtils.randFloat(rand, 0.1f) * 0.5f;
                angle = myUtils.randFloat(rand);
            }

            colorPicker.getColor(x, y, ref R, ref G, ref B);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            if (id < n)
            {
                oldx = x;
                oldy = y;

                x = gl_x0 + (float)Math.Sin(t) * rad;
                y = gl_y0 + (float)Math.Cos(t) * rad;

                t += dt;

                switch (generatorMoveMode)
                {
                    case 0:
                    case 1:
                        rad -= (float)Math.Sin(t * timeFactor) * 10;
                        break;
                }
            }
            else
            {
                if (false)
                {
                    var pt = list[0] as myObj_460;
                    //pt.oldx = pt.x;
                    //pt.oldy = pt.y;
                    pt.x = gl_x0 + (float)Math.Sin(t) * pt.rad;
                    pt.y = gl_y0 + (float)Math.Cos(t) * pt.rad;
                    t += dt/list.Count;
                }

                x += dx;
                y += dy;

                // Keep track of the trail
                oldx += dx * 0.75f;
                oldy += dy * 0.75f;

                switch (pointMoveMode)
                {
                    case 0:
                    case 1:
                    case 2:
                        if (x < 0 || x > gl_Width || y < 0 || y > gl_Height)
                        {
                            A -= 0.005f;
                            if (A < 0)
                                generateNew();
                        }
                        break;

                    case 3:
                        dy += gravityFactor + myUtils.randFloat(rand) * gravityFactor;
                        if (y > gl_Height)
                        {
                            A -= 0.005f;
                            if (A < 0)
                                generateNew();
                        }
                        break;
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

            if (doShowTrails && id > n)
            {
                myPrimitive._LineInst.setInstanceCoords(x, y, oldx, oldy);
                myPrimitive._LineInst.setInstanceColor(1, 1, 1, doClearBuffer ? 0.05f : 0.02f);
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
                    myPrimitive._LineInst.ResetBuffer();

                    for (int i = 0; i != list.Count; i++)
                    {
                        var obj = list[i] as myObj_460;

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

                if (list.Count < N)
                {
                    list.Add(new myObj_460());
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

            myPrimitive.init_LineInst(N);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------
    }
};
