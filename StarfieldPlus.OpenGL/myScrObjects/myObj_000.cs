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
        private int x, y, dx, dy;
        private float size, A, R, G, B, angle;

        private static int N = 0, shape = 0;
        private static bool doClearBuffer = false, doFillShapes = false;

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
            gl_x0 = gl_Width  / 2;
            gl_y0 = gl_Height / 2;

            N = (N == 0) ? 10 + rand.Next(10) : N;

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            height = 800;

            string str = $"Obj = myObj_000\n\n" +
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

            size = rand.Next(11) + 3;

            A = 1;
            R = (float)rand.NextDouble();
            G = (float)rand.NextDouble();
            B = (float)rand.NextDouble();

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

            //myTex bgrTex = new myTex(buildGalaxy());
            var bgrTex = new myTex2("C:\\_maxx\\pix\\__asd2.bmp");

            var rect = new myRectangle();
            rect.SetColor(1, 1, 1, 1);

            // Disable VSYNC if needed
            //Glfw.SwapInterval(0);

            if (doClearBuffer)
            {
                glDrawBuffer(GL_FRONT_AND_BACK | GL_DEPTH_BUFFER_BIT);
                glClearColor(0, 0, 0, 1);
            }

            var tBefore = System.DateTime.Now.Ticks;

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
                    // The texture doesn't wok properly: it disappears when we draw anything else -- fix it

                    int x = rand.Next(gl_Width);
                    int y = rand.Next(gl_Height);

                    bgrTex.Draw(x, y, 222, 222, x, y, 222, 222);

                    rect.Draw(x - 11, y - 11, 222 + 22, 222 + 22);

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
                    list.Add(new myObj_000());
                }

                //System.Threading.Thread.Sleep(renderDelay);

                if (cnt == 333)
                    break;
            }

            var tDiff = (System.DateTime.Now.Ticks - tBefore);
            System.TimeSpan elapsedSpan = new System.TimeSpan(tDiff);

            // 5574
            System.Windows.Forms.MessageBox.Show(elapsedSpan.TotalMilliseconds.ToString(), "time", System.Windows.Forms.MessageBoxButtons.OK);

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
};
