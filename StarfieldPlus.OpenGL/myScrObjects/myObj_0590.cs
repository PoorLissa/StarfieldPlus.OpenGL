using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;


/*
    - Particle moves as a result of an average of n other particles movement
*/


namespace my
{
    public class myObj_0590 : myObject
    {
        // Data item class
        private class data
        {
            public float x, y, z, dx, dy, dz;
            public float x0, y0, rad, angle, dAngle;
        };

        // ---------------------------------------------------------------------------------------------------------------

        // Priority
        public static int Priority => 10;
        public static System.Type Type => typeof(myObj_0590);

        private float X, Y, Z;
        private float A, R, G, B;
        private float size;
        private myParticleTrail trail = null;

        private static int N = 0, n = 0, moveMode = 0, dAngleMode = 0, nTrail = 0, lineWidth = 1, connectionMode = 0, connectionDist = 0, maxRad = 0;
        private static bool doFillShapes = false, doUseTrails = false, doUseZsize = false, doUseOnlyStraightMoves = false;
        private static float dimAlpha = 0.05f, s_dAngle;

        private static myFreeShader shader = null;

        private List<data> dataList = null;

        private static myScreenGradient grad = null;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_0590()
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
                doClearBuffer = true;
                doUseTrails = myUtils.randomChance(rand, 11, 12);

                N = rand.Next(10) + 2;
                n = rand.Next(10) + 2;

                if (doUseTrails)
                {
                    switch (rand.Next(3))
                    {
                        case 0: nTrail = 100 + rand.Next(100); break;
                        case 1: nTrail = 100 + rand.Next(333); break;
                        case 2: nTrail = 100 + rand.Next(666); break;
                    }
                }
            }

            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time local initialization
        private void initLocal()
        {
            doUseZsize = myUtils.randomBool(rand);
            doUseOnlyStraightMoves = myUtils.randomChance(rand, 1, 3);

            moveMode = rand.Next(3);
            connectionMode = rand.Next(3);
            dAngleMode = rand.Next(4);

            lineWidth = 1 + rand.Next(6);
            connectionDist = 333 + rand.Next(666);

            maxRad = 333 + rand.Next(999);
            s_dAngle = myUtils.randFloat(rand, 0.2f) * 0.05f;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            height = 600;

            string nStr(int   n) { return n.ToString("N0");    }

            string str = $"Obj = {Type}\n\n"                      	               +
                            $"N = {nStr(list.Count)} of {nStr(N)}\n"               +
                            $"n = {n}\n"                                           +
                            $"doUseTrails = {doUseTrails}\n"                       +
                            $"doUseZsize = {doUseZsize}\n"                         +
                            $"doUseOnlyStraightMoves = {doUseOnlyStraightMoves}\n" +
                            $"nTrail = {nTrail}\n"                                 +
                            $"moveMode = {moveMode}\n"                             +
                            $"dAngleMode = {dAngleMode}\n"                         +
                            $"s_dAngle = {myUtils.fStr(s_dAngle)}\n"               +
                            $"connectionMode = {connectionMode}\n"                 +
                            $"connectionDist = {connectionDist}\n"                 +
                            $"lineWidth = {lineWidth}\n"                           +
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
            size = 20 + rand.Next(15);

            X = Y = Z = 0;

            if (dataList == null)
            {
                dataList = new List<data>();
            }

            for (int i = 0; i < n; i++)
            {
                var d = new data();

                d.x = rand.Next(gl_Width);
                d.y = rand.Next(gl_Height);
                d.z = rand.Next(1000);

                d.dx = myUtils.randFloat(rand) * myUtils.randomSign(rand) * (rand.Next(5) + 11);
                d.dy = myUtils.randFloat(rand) * myUtils.randomSign(rand) * (rand.Next(5) + 11);
                d.dz = myUtils.randFloat(rand) * myUtils.randomSign(rand) * (rand.Next(5) + 11);

                if (doUseOnlyStraightMoves)
                {
                    if (myUtils.randomChance(rand, 1, 2))
                        d.dx = 0;
                    else
                        d.dy = 0;
                }

                // for the radial motion
                if (moveMode == 2)
                {
                    d.x0 = d.x;
                    d.y0 = d.y;
                    d.z = 1;
                    d.dx = 0;
                    d.dy = 0;
                    d.dz = 0;
                    d.rad = 33 + rand.Next(maxRad);
                    d.angle = myUtils.randFloat(rand) * 321;

                    d.x = d.x0 + d.rad * (float)Math.Sin(d.angle);
                    d.y = d.y0 + d.rad * (float)Math.Cos(d.angle);

                    switch (dAngleMode)
                    {
                        case 0:
                            d.dAngle = myUtils.randFloat(rand, 0.1f) * 0.05f;
                            break;

                        case 1:
                            d.dAngle = myUtils.randFloatSigned(rand, 0.1f) * 0.05f;
                            break;

                        case 2:
                            d.dAngle = s_dAngle;
                            break;

                        case 3:
                            d.dAngle = s_dAngle * myUtils.randomSign(rand);
                            break;
                    }
                }

                dataList.Add(d);

                X += d.x;
                Y += d.y;
                Z += d.z;
            }

            // Initial position
            X /= n;
            Y /= n;
            Z /= n;

            A = 0.8f + myUtils.randFloat(rand) * 0.2f;
            colorPicker.getColorRand(ref R, ref G, ref B);

            // Initialize Trail
            if (doUseTrails && trail == null)
            {
                trail = new myParticleTrail(nTrail, X, Y);
            }

            trail?.updateDa(A);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            // Update trail info
            if (doUseTrails)
            {
                trail?.update(X, Y);
            }

            for (int i = 0; i < n; i++)
            {
                var d = dataList[i];

                d.x += d.dx;
                d.y += d.dy;
                d.z += d.dz;

                switch (moveMode)
                {
                    case 0:
                        {
                            if (d.x < 0 && d.dx < 0)
                                d.dx *= -1;

                            if (d.y < 0 && d.dy < 0)
                                d.dy *= -1;

                            if (d.x > gl_Width && d.dx > 0)
                                d.dx *= -1;

                            if (d.y > gl_Height && d.dy > 0)
                                d.dy *= -1;

                            if (d.z > 1000 && d.dz > 0)
                                d.dz *= -1;

                            if (d.z < 0 && d.dz < 0)
                                d.dz *= -1;
                        }
                        break;

                    case 1:
                        {
                            float spd = 0.5f;

                            if (d.x < 0)
                                d.dx += spd;

                            if (d.y < 0)
                                d.dy += spd;

                            if (d.z < 0)
                                d.dz += spd;

                            if (d.x > gl_Width)
                                d.dx -= spd;

                            if (d.y > gl_Height)
                                d.dy -= spd;

                            if (d.z > 1000)
                                d.dz -= spd;
                        }
                        break;

                    case 2:
                        {
                            d.x = d.x0 + d.rad * (float)Math.Sin(d.angle);
                            d.y = d.y0 + d.rad * (float)Math.Cos(d.angle);
                            d.angle += d.dAngle;
                            //d.angle += myUtils.randFloat(rand) * 0.01f;
                        }
                        break;
                }
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Show()
        {
            X = Y = Z = 0;

            for (int i = 0; i < n; i++)
            {
                var particle = dataList[i];

                myPrimitive._EllipseInst.setInstanceCoords(particle.x, particle.y, 10, 0);
                myPrimitive._EllipseInst.setInstanceColor(R, G, B, 0.5f);

                X += particle.x;
                Y += particle.y;
                Z += particle.z;
            }

            X /= n;
            Y /= n;
            Z /= n;

            float mySize = doUseZsize
                ? size * (Z + 1) / 1000
                : size;

            shader.SetColor(R, G, B, A);
            shader.Draw(X, Y, 5, 5, 10);
            shader.Draw(X, Y, mySize, mySize, 10);

            // Display connections between primary particles
            switch (connectionMode)
            {
                // No connections
                case 0:
                    break;

                // Simple stupid connections prev to next
                case 1:
                    {
                        if (id > 0)
                        {
                            var prev = list[(int)id - 1] as myObj_0590;

                            myPrimitive._LineInst.setInstanceCoords(X, Y, prev.X, prev.Y);
                            myPrimitive._LineInst.setInstanceColor(1, 1, 1, 0.1f);

                            if (id == N - 1)
                            {
                                var first = list[0] as myObj_0590;

                                myPrimitive._LineInst.setInstanceCoords(X, Y, first.X, first.Y);
                                myPrimitive._LineInst.setInstanceColor(1, 1, 1, 0.1f);
                            }
                        }
                    }
                    break;

                // Smart proximity connections
                case 2:
                    {
                        for (int i = 0; i < list.Count; i++)
                        {
                            if (i != id)
                            {
                                var other = list[i] as myObj_0590;

                                float dx = X - other.X;
                                float dy = Y - other.Y;

                                float dist = (float)Math.Sqrt(dx * dx + dy * dy);

                                if (dist < connectionDist)
                                {
                                    float opacity = 0.5f * (1.0f * connectionDist - dist) / connectionDist;

                                    myPrimitive._LineInst.setInstanceCoords(X, Y, other.X, other.Y);
                                    myPrimitive._LineInst.setInstanceColor(1, 1, 1, opacity);
                                }
                            }
                        }
                    }
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
                //glDrawBuffer(GL_DEPTH_BUFFER_BIT);
            }

            stopwatch = new StarfieldPlus.OpenGL.myUtils.myStopwatch(true);

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
                    }
                    else
                    {
                        dimScreen(dimAlpha);
                    }

                    grad.Draw();
                }

                // Render Frame
                {
                    inst.ResetBuffer();
                    myPrimitive._LineInst.ResetBuffer();

                    for (int i = 0; i != Count; i++)
                    {
                        var obj = list[i] as myObj_0590;

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

                    myPrimitive._LineInst.Draw();

                    // Draw the trail -- use a separate call to maintain tail line width
                    if (doUseTrails)
                    {
                        myPrimitive._LineInst.ResetBuffer();
                        myPrimitive._LineInst.setLineWidth(lineWidth);

                        for (int i = 0; i != Count; i++)
                        {
                            var obj = list[i] as myObj_0590;
                            obj.trail?.Show(obj.R, obj.G, obj.B, obj.A);
                        }

                        myPrimitive._LineInst.Draw();
                        myPrimitive._LineInst.setLineWidth(1);
                    }
                }

                if (Count < N)
                {
                    list.Add(new myObj_0590());
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
            myPrimitive.init_LineInst(doUseTrails ? (N * N + N * nTrail) : N * N);

            base.initShapes(2, N * n, 0);

            string h = "", m = "";
            my.myShaderHelpers.Shapes.getShader_000(ref rand, ref h, ref m);
            shader = new myFreeShader(h, m);

            grad = new myScreenGradient();
            grad.SetRandomColors(rand, 0.2f);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------
    }
};
