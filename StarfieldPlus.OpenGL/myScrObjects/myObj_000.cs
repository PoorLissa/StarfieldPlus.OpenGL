using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;
using System.Drawing;


/*
    - Starfield
*/


namespace my
{
    public class myObj_000 : myObject
    {
        protected float size, A, R, G, B, angle;
        protected float x, y, dx, dy, acceleration = 1.0f;
        protected int cnt = 0, max = 0, color = 0;

        protected static int drawMode = 0;
        private static int N = 0, staticStarsN = 0, shape = 0;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_000()
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
            N = (N == 0) ? 100 + rand.Next(100) : N;
            staticStarsN = rand.Next(333) + 333;
            N += staticStarsN;

            shape = rand.Next(5);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            height = 800;

            string str = $"Obj = myObj_000 (Starfield)\n\n" +
                            $"N = {N} ({list.Count})\n" +
                            $""
                ;
            return str;
        }

        // ---------------------------------------------------------------------------------------------------------------

        // 
        protected override void setNextMode()
        {
            //init();
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void generateNew()
        {
            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
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

                    rectInst.setInstanceCoords(x - size/2, y - size/2, size, size);
                    rectInst.setInstanceColor(R, G, B, A);
                    rectInst.setInstanceAngle(angle);
                    break;

                // Instanced triangles
                case 1:
                    var triangleInst = inst as myTriangleInst;

                    triangleInst.setInstanceCoords(x, y, size, angle);
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

            var bgrTex = new myTexRectangle(buildGalaxy());
            //var bgrTex = new myTexRectangle("C:\\_maxx\\pix\\__asd2.bmp");

            // Disable VSYNC if needed
            //Glfw.SwapInterval(0);

            if (doClearBuffer)
            {
                glDrawBuffer(GL_FRONT_AND_BACK | GL_DEPTH_BUFFER_BIT);
                glClearColor(0, 0, 0, 1);
            }

            for (int i = 0; i < staticStarsN; i++)
            {
                list.Add(new myObj_000_StaticStar());
            }

            while (!Glfw.WindowShouldClose(window))
            {
                cnt++;

                processInput(window);

                // Swap fore/back framebuffers, and poll for operating system events.
                Glfw.SwapBuffers(window);
                Glfw.PollEvents();

                glClear(GL_COLOR_BUFFER_BIT);

                // Render Frame
                {
#if false
                    for (int i = 0; i < 333; i++)
                    {
                        int x = rand.Next(gl_Width);
                        int y = rand.Next(gl_Width);
                        int w = rand.Next(11) + 3;

                        bgrTex.Draw(x, y, w, w, rand.Next(gl_Width), rand.Next(gl_Width), w, w);
                    }
                    continue;
#endif
                    bgrTex.Draw(0, 0, gl_Width, gl_Height);

                    inst.ResetBuffer();

                    for (int i = 0; i < list.Count; i++)
                    {
                        var obj = list[i] as myObj_000;

                        obj.Show();
                        obj.Move();
                    }

                    // Tell the fragment shader to do nothing with the existing instance opacity:
                    inst.SetColorA(0);
                    inst.Draw(false);
                }

                if (list.Count < N)
                {
                    list.Add(new myObj_000_Star());
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

        // Build random Galaxy background
        private Bitmap buildGalaxy()
        {
            Bitmap bmp = new Bitmap(gl_Width, gl_Height);
            int x1 = 0, y1 = 0, x2 = 0, y2 = 0;

            using (var gr = Graphics.FromImage(bmp))
            using (var br = new SolidBrush(Color.Red))
            {
                // Black background
                gr.FillRectangle(Brushes.Black, 0, 0, gl_Width, gl_Height);

                // Add low opacity colored spots
                for (int i = 0; i < 10; i++)
                {
                    int opacity = rand.Next(7) + 1;

                    x1 = rand.Next(gl_Width);
                    y1 = rand.Next(gl_Height);

                    x2 = rand.Next(333) + 100 * opacity;
                    y2 = rand.Next(333) + 100 * opacity;

                    br.Color = Color.FromArgb(1, rand.Next(256), rand.Next(256), rand.Next(256));

                    while (opacity != 0)
                    {
                        gr.FillRectangle(br, x1, y1, x2, y2);

                        x1 += rand.Next(25) + 25;
                        y1 += rand.Next(25) + 25;
                        x2 -= rand.Next(50) + 50;
                        y2 -= rand.Next(50) + 50;
                        opacity--;
                    }
                }

                // Add nebulae
                for (int k = 0; k < 111; k++)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        x1 = rand.Next(gl_Width);
                        y1 = rand.Next(gl_Height);

                        int masterOpacity = rand.Next(100) + 100;
                        int radius1 = rand.Next(250) + 100;
                        int radius2 = rand.Next(250) + 100;
                        colorPicker.getColor(br, x1, y1, masterOpacity);

                        gr.FillRectangle(br, x1, y1, 1, 1);

                        for (int j = 0; j < 100; j++)
                        {
                            x2 = x1 + rand.Next(radius1) - radius1 / 2;
                            y2 = y1 + rand.Next(radius2) - radius2 / 2;

                            int maxSlaveOpacity = 50 + rand.Next(50);
                            int slaveOpacity = rand.Next(maxSlaveOpacity);

                            br.Color = Color.FromArgb(slaveOpacity, br.Color.R, br.Color.G, br.Color.B);
                            gr.FillRectangle(br, x2, y2, 1, 1);

                            int r = rand.Next(2) + 3;

                            br.Color = Color.FromArgb(3, br.Color.R, br.Color.G, br.Color.B);
                            gr.FillRectangle(br, x2 - r, y2 - r, 2 * r, 2 * r);
                        }
                    }
                }
            }

            return bmp;
        }

        // ---------------------------------------------------------------------------------------------------------------
    }


    // ===================================================================================================================
    // ===================================================================================================================


    // Moving stars
    class myObj_000_Star : myObj_000
    {
        protected override void generateNew()
        {
            A = (float)rand.NextDouble();
            R = (float)rand.NextDouble();
            G = (float)rand.NextDouble();
            B = (float)rand.NextDouble();

            int speed = rand.Next(10) + 1;

            max = rand.Next(75) + 20;
            cnt = 0;
            color = rand.Next(50);
            acceleration = 1.005f + (rand.Next(100) * 0.0005f);

            x = rand.Next(gl_Width);
            y = rand.Next(gl_Width);

            double dist = Math.Sqrt((x - gl_x0) * (x- gl_x0) + (y - gl_x0) * (y - gl_x0));
            double sp_dist = speed / dist;

            dx = (float)((x - gl_x0) * sp_dist);
            dy = (float)((y - gl_x0) * sp_dist);

            y = (y - (gl_Width - gl_Height) / 2);

            size = 0;
        }

        protected override void Show()
        {
            base.Show();
/*
            switch (drawMode)
            {
                case 0:
                    g.FillRectangle(br, X, Y, Size, Size);
                    break;

                case 1:
                    if (Size == 1)
                    {
                        g.FillRectangle(br, X, Y, 1, 1);
                    }
                    else
                    {
                        p.Color = br.Color;
                        g.DrawRectangle(p, X, Y, Size - 1, Size - 1);
                    }
                    break;
            }
*/
            return;
        }

        protected override void Move()
        {
            x += dx;
            y += dy;

            // todo: 
            size += 0.01f;

            acceleration *= (1.0f + (size * 0.0001f));

            if (cnt++ > max)
            {
                cnt = 0;
                //size++;

                // Accelerate acceleration rate
                //acceleration *= (1.0f + (size * 0.001f));
            }

            // Accelerate our moving stars
            dx *= acceleration;
            dy *= acceleration;

            if (x < 0 || x > gl_Width || y < 0 || y > gl_Height)
            {
                generateNew();
            }

            return;
        }
    };


    // ===================================================================================================================
    // ===================================================================================================================


    class myObj_000_StaticStar : myObj_000
    {
        private int lifeCounter = 0;
        protected int alpha = 0, bgrAlpha = 0;
        private static int factor = 1;
        private static bool doMove = true;

        protected override void generateNew()
        {
            lifeCounter = (rand.Next(500) + 500) * factor;

            x = rand.Next(gl_Width);
            y = rand.Next(gl_Height);
            color = rand.Next(50);
            alpha = rand.Next(50) + 175;

            bgrAlpha = 1;

            if (rand.Next(11) == 0)
                bgrAlpha = 2;

            max = (rand.Next(200) + 100) * factor;
            cnt = 0;
            size = 0;

            A = (float)rand.NextDouble() / 2;
            R = (float)rand.NextDouble();
            G = (float)rand.NextDouble();
            B = (float)rand.NextDouble();

            // Make our static stars not so static
            if (doMove)
            {
                // Linear speed outwards:
                double dist = Math.Sqrt((x - gl_x0) * (x - gl_x0) + (y - gl_y0) * (y - gl_y0));
                double sp_dist = 0.1f / dist;

                dx = (float)((x - gl_x0) * sp_dist);
                dy = (float)((y - gl_y0) * sp_dist);
            }
        }

        protected override void Show()
        {
            base.Show();
        }

        protected override void Move()
        {
            //angle += 0.01f;

            if (doMove)
            {
                x += dx;
                y += dy;
            }

            if (lifeCounter-- == 0)
            {
                factor = 3;
                generateNew();
            }
            else
            {
                if (cnt++ > max)
                {
                    cnt = 0;
                    size = rand.Next(5) + 1;
                }
            }
        }
    };


    // ===================================================================================================================
    // ===================================================================================================================


    class myObj_000_Comet : myObj_000
    {
        protected override void generateNew()
        {
        }

        protected override void Show()
        {
            base.Show();
        }

        protected override void Move()
        {
        }
    };


    // ===================================================================================================================
    // ===================================================================================================================
};
