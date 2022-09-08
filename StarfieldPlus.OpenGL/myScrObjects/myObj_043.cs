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

            N = 9999;

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

            //mass = rand.Next(100) == 0 ? 10000 : 500;
            mass = rand.Next(3333) + 10;
            mass = 500;

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
                R = 0.50f + (float)(rand.NextDouble() * 0.5);
                G = 0.25f;
                B = 0.15f;
            }

            if (type == ParticleType.Two)
            {
                R = 0.1f;
                G = 0.50f + (float)(rand.NextDouble() * 0.25);
                B = 0.1f;
            }

            if (type == ParticleType.Three)
            {
                R = 0.15f;
                G = 0.45f;
                B = 0.50f + (float)(rand.NextDouble() * 0.33);
            }

            if (type == ParticleType.Four)
            {
                R = 1.0f / 250 * 245;
                G = 1.0f / 250 * 180;
                B = 1.0f / 250 *  40;
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        // https://www.youtube.com/watch?v=0Kx4Y9TVMGg&ab_channel=Brainxyz

        protected override void Move()
        {
            double distSquared;
            float DX = 0, DY = 0, dist = 0, F = 0, factor = 0;

            for (int i = 0; i < list.Count; i++)
            {
                var obj = list[i] as myObj_043;

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
                            switch (type)
                            {
                                case ParticleType.One:

                                    if (dist >= 1000)
                                        factor *= 0.1f;

                                    factor *= 5;
                                    break;

                                case ParticleType.Two:
                                    if (dist >= 1000)
                                        factor *= 0.1f;

                                    factor *= 3;
                                    break;

                                case ParticleType.Three:
                                    if (dist >= 1000)
                                        factor *= 0.1f;

                                    factor *= 2;
                                    break;

                                case ParticleType.Four:
                                    if (dist >= 1000)
                                        factor *= 0.1f;

                                    factor *= 1;
                                    break;
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
                                factor *= -1000;

                                switch (type)
                                {
                                    case ParticleType.One:
                                        break;

                                    case ParticleType.Two:
                                        break;

                                    case ParticleType.Three:
                                        break;

                                    case ParticleType.Four:
                                        break;
                                }
                            }
                        }

                        if (factor != 0)
                        {
                            F = -obj.mass / dist;

                            dx += factor * F * DX;
                            dy += factor * F * DY;
                        }

                        // Optional resisting force
                        if (true)
                        {
                            float resistFactor = 0.99999f;

                            dx *= resistFactor;
                            dy *= resistFactor;
                        }
                    }
#if true
                    int border = 11;
                    float reverseFactor = 0.99999f;

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

#if false
                    for (int k = 0; k < nTaskCount; k++)
                    {
                        int K = k;

                        var task = new System.Threading.Tasks.Task(() => {

                            int beg = (K + 0) * list.Count / nTaskCount;
                            int end = (K + 1) * list.Count / nTaskCount;

                            for (int i = beg; i < end; i++)
                                (list[i] as myObj_043).Move();
                        });

                        task.Start();
                        taskList[k] = task;
                    }
#else
                    for (int k = 0; k < nTaskCount; k++)
                    {
                        var task = System.Threading.Tasks.Task<int>.Factory.StartNew(action, k);
                        taskList[k] = task;
                    }
#endif
                    System.Threading.Tasks.Task.WaitAll(taskList);

                    for (int i = 0; i < list.Count; i++)
                    {
                        (list[i] as myObj_043).Show();
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
