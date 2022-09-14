using GLFW;
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
        private enum ParticleType { One, Two, Three, Four };

        private float x, y, dx, dy;
        private float mass, size, A, R, G, B, angle;
        private ParticleType type;

        private static int N = 0, shape = 0;
        private static bool doClearBuffer = false, doFillShapes = true, doUseRandomMass = false;
        private static float dimAlpha = 0.05f;

        private static int border = 3;
        private static float reverseFactor = 0.99999f;
        private static float resistFactor  = 0.99999f;

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

            N = 3333;

            doUseRandomMass = myUtils.randomBool(rand);

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

            dx = myUtils.randomSign(rand) * (float)rand.NextDouble() * 3;
            dy = myUtils.randomSign(rand) * (float)rand.NextDouble() * 3;

            if (doUseRandomMass)
            {
                mass = rand.Next(3333) + 10;
                //mass *= rand.Next(100) == 0 ? 10 : 1;
            }
            else
            {
                mass = 500;
            }

            size = mass < 500 ? 1 : mass / 500;

            A = 0.5f;
            R = (float)rand.NextDouble();
            G = (float)rand.NextDouble();
            B = (float)rand.NextDouble();

            type = ParticleType.Two;
/*
            if (rand.Next(666) == 0)
                type = ParticleType.One;
*/
            type = ParticleType.Three;
            type = (ParticleType)rand.Next(4);

#if false
            if (list.Count == 11 /*|| list.Count == 12 || list.Count == 13*/)
            {
                type = ParticleType.Four;
                mass = 5000000;
                size = 7;
                dx = 0;
                dy = 0;
            }
#endif

            if (type == ParticleType.One)
            {
                R = 0.50f + (float)(rand.NextDouble() * 0.50);
                G = 0.20f + (float)(rand.NextDouble() * 0.10);
                B = 0.10f + (float)(rand.NextDouble() * 0.10);
            }

            if (type == ParticleType.Two)
            {
                R = 0.10f + (float)(rand.NextDouble() * 0.10);
                G = 0.50f + (float)(rand.NextDouble() * 0.25);
                B = 0.10f + (float)(rand.NextDouble() * 0.10);
            }

            if (type == ParticleType.Three)
            {
                R = 0.10f + (float)(rand.NextDouble() * 0.10);
                G = 0.45f + (float)(rand.NextDouble() * 0.10);
                B = 0.50f + (float)(rand.NextDouble() * 0.33);
            }

            if (type == ParticleType.Four)
            {
                R = 1.0f / 250 * 245 + (float)(rand.NextDouble() * 0.10);
                G = 1.0f / 250 * 180 + (float)(rand.NextDouble() * 0.10);
                B = 1.0f / 250 *  40 + (float)(rand.NextDouble() * 0.10);
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        // https://www.youtube.com/watch?v=0Kx4Y9TVMGg&ab_channel=Brainxyz

        protected override void Move()
        {
            float DX = 0, DY = 0, dist = 0, F = 0, factor = 0;

            for (int i = 0; i < list.Count; i++)
            {
                var obj = (myObj_043)(list[i]);

                if (obj != this)
                {
                    factor = 0.000001f;

                    DX = x - obj.x;
                    DY = y - obj.y;

                    dist = (float)Math.Sqrt(DX * DX + DY * DY);

                    if (dist > 0)
                    {
                        if (type == obj.type)
                        {
                            if (dist < 20)
                            {
                                factor *= dist < 10 ? -1000 : -500;

                                if (!doUseRandomMass)
                                {
                                    factor *= 10;
                                }
                            }
                            else
                            {
                                switch (type)
                                {
                                    case ParticleType.One:

                                        if (dist >= 222)
                                            factor *= 0.1f;

                                        if (dist > 333)
                                            factor *= 0.05f;

                                        factor *= 5;
                                        break;

                                    case ParticleType.Two:

                                        if (dist >= 222)
                                            factor *= 0.1f;

                                        if (dist > 333)
                                            factor *= 0.03f;

                                        factor *= 3;
                                        break;

                                    case ParticleType.Three:

                                        if (dist >= 222)
                                            factor *= 0.1f;

                                        if (dist > 333)
                                            factor *= 0.02f;

                                        factor *= 2;
                                        break;

                                    case ParticleType.Four:

                                        if (dist >= 222)
                                            factor *= 0.1f;

                                        if (dist > 333)
                                            factor *= 0.01f;

                                        factor *= 1;
                                        break;
                                }
                            }
                        }
                        else
                        {
                            if (dist >= 100)
                            {
                                factor = 0;
                            }
                            else
                            {
                                //factor *= -1000;
                                //factor *= aaa;

                                switch (type)
                                {
                                    case ParticleType.One:
                                        factor *= 3;
                                        break;

                                    case ParticleType.Two:
                                        factor *= 3;
                                        break;

                                    case ParticleType.Three:
                                        factor *= 3;
                                        break;

                                    case ParticleType.Four:
                                        factor *= 3;
                                        break;
                                }
                            }
                        }

                        if (factor != 0)
                        {
                            F = factor * obj.mass / dist;

                            dx -= F * DX;
                            dy -= F * DY;
                        }

                        // Another resisting force
                        dx *= (1.0f - 0.00001f);
                        dy *= (1.0f - 0.00001f);

#if false
                        // Optional resisting force
                        if (dist < 10)
                        {
                            dx *= resistFactor;
                            dy *= resistFactor;
                        }
                        else
                        {
                            dx *= resistFactor * 1.00000001f;
                            dy *= resistFactor * 1.00000001f;
                        }
#endif
                    }

#if false
                    if (x < border && dx < 0)
                    {
                        //dx *= -1;
                        dx *= reverseFactor;
                    }

                    if (x > gl_Width - border && dx > 0)
                    {
                        //dx *= -1;
                        dx *= reverseFactor;
                    }

                    if (y < border && dy < 0)
                    {
                        //dy *= -1;
                        dy *= reverseFactor;
                    }

                    if (y > gl_Height - border && dy > 0)
                    {
                        //dy *= -1;
                        dy *= reverseFactor;
                    }
#endif
                }
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
            x += dx;
            y += dy;

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

        static int aaa = 1;

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

            int nTaskCount = Environment.ProcessorCount;
            nTaskCount = 100;
            var taskList = new System.Threading.Tasks.Task[nTaskCount];

            // Define a delegate that prints and returns the system tick count
            Func<object, int> action = (object obj) =>
            {
                int k = (int)obj;

                int beg = (k + 0) * list.Count / nTaskCount;
                int end = (k + 1) * list.Count / nTaskCount;

                for (int i = beg; i < end; i++)
                    (list[i] as myObj_043).Move();

                return 0;
            };

            for (int k = 0; k < nTaskCount; k++)
            {
                var task = System.Threading.Tasks.Task<int>.Factory.StartNew(action, k);
                taskList[k] = task;
            }

            var t1 = System.DateTime.Now.Ticks;

            while (!Glfw.WindowShouldClose(window))
            {
                cnt++;

                if (cnt == 3000)
                {
                    aaa = -10000;
                }

                if (cnt == 3100)
                {
                    aaa = 1;
                    cnt = 0;
                }

/*
                if (cnt == 1000)
                {
                    var tDiff = (System.DateTime.Now.Ticks - t1);
                    TimeSpan elapsedSpan = new TimeSpan(tDiff);
                    System.Windows.Forms.MessageBox.Show($"{elapsedSpan.TotalMilliseconds}", $"fps = {1000 * cnt/ elapsedSpan.TotalMilliseconds}", System.Windows.Forms.MessageBoxButtons.OK);
                    break;
                }
*/
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

                    // Wait untill all the tasks have finished
                    System.Threading.Tasks.Task.WaitAll(taskList);

                    // Draw everything to the Inst
                    for (int i = 0; i < list.Count; i++)
                    {
                        (list[i] as myObj_043).Show();
                    }

                    // Restart all the tasks
                    for (int k = 0; k < nTaskCount; k++)
                    {
                        var task = System.Threading.Tasks.Task<int>.Factory.StartNew(action, k);
                        taskList[k] = task;
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

                //System.Threading.Thread.Sleep(renderDelay);
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
