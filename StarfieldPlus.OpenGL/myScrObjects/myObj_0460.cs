using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;


/*
    - Generator cyclically moves along a spiral, constantly leaving a trail;
    - Trail is made of particles that move outwards from the center OR in a point's opposite direction
*/


namespace my
{
    public class myObj_0460 : myObject
    {
        // Priority
        public static int Priority => 10;
		public static System.Type Type => typeof(myObj_0460);

        private float x, y, dx, dy, oldx, oldy, rad;
        private float size, A, R, G, B, angle = 0, dAngle = 0;

        private myParticleTrail trail = null;
        private myParticleTrail Trail = null;

        private static int N = 0, n = 1, shape = 0, generatorMoveMode = 0, pointDirMode = 0, pointMoveMode = 0, nTrail = 20, nMainTrail = 100;
        private static bool doFillShapes = false, doRandomizeSpeedVector = true, doUseTrails = false;
        private static float dimAlpha = 0.05f, t = 0, dt = 0, tRad = 0, dtRad = 0, timeFactor = 0, gravityFactor = 1;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_0460()
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

                nMainTrail = 10 + rand.Next(333);

                shape = rand.Next(5);

                doUseTrails = myUtils.randomBool(rand);
            }

            initLocal();

            t = 0;
            tRad = 0;
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time local initialization
        private void initLocal()
        {
            doClearBuffer = myUtils.randomBool(rand);
            doRandomizeSpeedVector = myUtils.randomBool(rand);

            generatorMoveMode = rand.Next(6);
            pointMoveMode     = rand.Next(5);
            pointDirMode      = rand.Next(5);

            dimAlpha = 0.15f;

            // Set up dt (The less is dt, the larger becomes final radius)
            {
                dt = myUtils.randomSign(rand) * 0.033f;
                dtRad = 0.033f;

                //dt *= 0.1f;
                //timeFactor *= 2;
                //rad /= 3; -- change it in generateNew
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

                    case 2:
                        dtRad = 0.000001f;
                        break;

                    case 3:
                        dtRad = 0.001f * 0.333f;
                        break;

                    case 4:
                        break;

                    case 5:
                        timeFactor = 10 + rand.Next(30);
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

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            height = 600;

            string str = $"Obj = {Type}\n\n"                                     	+
                            myUtils.strCountOf(list.Count, N)                       +
                            $"doClearBuffer = {doClearBuffer}\n"                    +
                            $"doUseTrails = {doUseTrails}\n"                        +
                            $"nTrail = {nTrail}\n"                                  +
                            $"nMainTrail = {nMainTrail}\n"                          +
                            $"doRandomizeSpeedVector = {doRandomizeSpeedVector}\n"  +
                            $"generatorMoveMode = {generatorMoveMode}\n"            +
                            $"pointDirMode = {pointDirMode}\n"                      +
                            $"pointMoveMode = {pointMoveMode}\n"                    +
                            $"timeFactor = {myUtils.fStr(timeFactor)}\n"            +
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
                //rad /= 5;

                x = gl_x0 + (float)Math.Sin(t) * rad;
                y = gl_y0 + (float)Math.Cos(t) * rad;

                oldx = oldy = 0;
                size = 5;
                A = 1;

                if (Trail == null)
                {
                    Trail = new myParticleTrail(nMainTrail, x, y);
                }
                else
                {
                    Trail.reset(x, y);
                }

                Trail?.updateDa(A * 0.75f);
            }
            else
            {
                var pt = list[0] as myObj_0460;

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

                    // Slow down the particles using the same rate
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

                    // Radial out mode
                    case 4:
                        {
                            double dist = Math.Sqrt((x - gl_x0) * (x - gl_x0) + (y - gl_y0) * (y - gl_y0));
                            double sp_dist = 10 / dist;

                            dx = (float)((x - gl_x0) * sp_dist);
                            dy = (float)((y - gl_y0) * sp_dist);
                        }
                        break;
                }

                size = rand.Next(1) + 3;
                A = myUtils.randFloat(rand, 0.1f) * 0.5f;
                angle = myUtils.randFloat(rand);
                dAngle = myUtils.randFloat(rand) * myUtils.randomSign(rand) * 0.01f;

                // Initialize trail
                if (doUseTrails)
                {
                    if (trail == null)
                    {
                        trail = new myParticleTrail(nTrail, x, y);
                    }
                    else
                    {
                        trail.reset(x, y);
                    }

                    trail?.updateDa(0.25f);
                }
            }

            colorPicker.getColor(x, y, ref R, ref G, ref B);

#if false
            if (id == 0)
            {
                R = G = B = 0.66f;
            }
            else
            {
                R = G = B = 0.1f;
            }
#endif
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
                tRad += dtRad;

                switch (generatorMoveMode)
                {
                    case 0:
                    case 1:
                        rad -= (float)Math.Sin(tRad * timeFactor) * 10;
                        break;

                    case 2:
                        rad -= (float)Math.Sin(tRad * 3) * 10;
                        break;

                    // Simple spiral -- expandng and contracting
                    case 3:
                        rad = 666 - (float)Math.Sin(tRad) * 500;
                        break;

                    // Elliptic trajectory which randomly changes its radius
                    case 4:
                        rad = myUtils.randomChance(rand, 1, 1000)
                            ? rad = rand.Next(999) + 100
                            : rad;
                        break;

                    // Elliptic/sine trajectory which randomly changes its radius
                    case 5:
                        rad = myUtils.randomChance(rand, 1, 1000)
                            ? rad = rand.Next(999) + 100
                            : rad;

                        rad += (float)Math.Sin(tRad * timeFactor) * 7;
                        break;
                }
            }
            else
            {
                if (false)
                {
                    var pt = list[0] as myObj_0460;
                    //pt.oldx = pt.x;
                    //pt.oldy = pt.y;
                    pt.x = gl_x0 + (float)Math.Sin(t) * pt.rad;
                    pt.y = gl_y0 + (float)Math.Cos(t) * pt.rad;
                    t += dt/list.Count;
                }

                x += dx;
                y += dy;

                // Keep track of the trail -- not used anymore
                oldx += dx * 0.75f;
                oldy += dy * 0.75f;

                angle += dAngle;

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

            trail?.update(x, y);
            trail?.Show(R, G, B, A);
            Trail?.update(x, y);
            Trail?.Show(R, G, B, A);

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

            stopwatch = new StarfieldPlus.OpenGL.myUtils.myStopwatch();
            stopwatch.Start();

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
                        var obj = list[i] as myObj_0460;

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
                    list.Add(new myObj_0460());
                }

                stopwatch.WaitAndRestart();
                cnt++;
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void initShapes()
        {
            myPrimitive.init_ScrDimmer();
            base.initShapes(shape, N, 0);

            myPrimitive.init_LineInst(nMainTrail + (doUseTrails ? N * nTrail : 1));

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------
    }
};
