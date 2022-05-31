using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;


/*
    - Randomly Roaming Squares (Snow Like)
*/


namespace my
{
    public class myObj_010 : myObject
    {
        private int dx, dy, Size, X, Y;
        float A = 0, R = 0, G = 0, B = 0;

        static bool isDimmable = false, doFillShapes = false;
        static int minX = 0, minY = 0, maxX = 0, maxY = 0, showMode = 0, maxSize = 0;

        private static int x0, y0, N = 1;

        private static myInstancedPrimitive inst = null;

        public myObj_010()
        {
            if (colorPicker == null)
            {
                colorPicker = new myColorPicker(gl_Width, gl_Height);
                list = new List<myObject>();

                // In case the colorPicker points to an image, try something different
/*
                if (colorPicker.getMode() < 2)
                {
                    showMode = rand.Next(2);
                }
*/
                isDimmable = rand.Next(2) == 0;

                var alpha = isDimmable ? 33 + rand.Next(130) : 255;

                // Sometimes set dimBrush to very low alpha -- to have longer tails
                if (isDimmable && rand.Next(11) == 0)
                {
                    alpha = 7 + rand.Next(11);
                }

                // In case the border is wider than the screen's bounds, the movement looks a bit different (no bouncing)
                int offset = rand.Next(2) == 0 ? 0 : 100 + rand.Next(500);

                minX = 0 - offset;
                minY = 0 - offset;
                maxX = gl_Width  + offset;
                maxY = gl_Height + offset;

                maxSize = showMode == 0 ? 11 : 50;
            }

            X = rand.Next(gl_Width);
            Y = rand.Next(gl_Height);

            int maxSpeed = 20;

            dx = (rand.Next(maxSpeed) + 1) * (rand.Next(2) == 0 ? 1 : -1);
            dy = (rand.Next(maxSpeed) + 1) * (rand.Next(2) == 0 ? 1 : -1);

            Size = rand.Next(maxSize) + 1;

            //A = rand.Next(256 - 75) + 75;
            //colorPicker.getColor(X, Y, ref R, ref G, ref B);

            A = (float)rand.NextDouble();
            R = (float)rand.NextDouble();
            G = (float)rand.NextDouble();
            B = (float)rand.NextDouble();

            A = 0.50f;
            R = 0.75f;
            G = 0.50f;
            B = 0.50f;
        }

        // -------------------------------------------------------------------------

        protected override void Move()
        {
            X += dx;
            Y += dy;

            if (X < minX || X > maxX)
            {
                dx *= -1;
            }

            if (Y < minY || Y > maxY)
            {
                dy *= -1;
            }

            return;
        }

        // -------------------------------------------------------------------------

        protected override void Show()
        {
            switch (showMode)
            {
                case 0:
                    var rectInst = inst as myRectangleInst;

                    rectInst.setInstanceCoords(X, Y, 2 * Size, 2 * Size);
                    rectInst.setInstanceColor(R, G, B, A);
                    rectInst.setInstanceAngle(0);
                    break;



                // Solid color
/*
                case 0:
                    br.Color = Color.FromArgb(A, R, G, B);
                    g.FillRectangle(br, X, Y, Size, Size);

                    if (Size > 3)
                    {
                        g.FillRectangle(Brushes.Black, X, Y, 1, 1);
                        g.FillRectangle(Brushes.Black, X + Size - 1, Y, 1, 1);
                        g.FillRectangle(Brushes.Black, X, Y + Size - 1, 1, 1);
                        g.FillRectangle(Brushes.Black, X + Size - 1, Y + Size - 1, 1, 1);
                    }
                    break;
*/
            }

            return;
        }

        // -------------------------------------------------------------------------

        protected override void Process(Window window)
        {
            N = 10;
            renderDelay = 1;

            myPrimitive.init_Rectangle();

            myPrimitive.init_RectangleInst(N);
            myPrimitive._RectangleInst.setRotationMode(0);
            inst = myPrimitive._RectangleInst;


            while (list.Count < N)
            {
                list.Add(new myObj_010());
            }


            glDrawBuffer(GL_FRONT_AND_BACK);


            while (!Glfw.WindowShouldClose(window))
            {
                processInput(window);

                // Swap fore/back framebuffers, and poll for operating system events.
                Glfw.SwapBuffers(window);
                Glfw.PollEvents();

                glClearColor(0, 0, 0, 1);
                glClear(GL_COLOR_BUFFER_BIT);

                inst.ResetBuffer();

                for (int i = 0; i < list.Count; i++)
                {
                    var obj = list[i] as myObj_010;
                    obj.Show();
                    //obj.Move();
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

                System.Threading.Thread.Sleep(renderDelay);
            }


/*
            g.FillRectangle(Brushes.Black, 0, 0, Width, Height);

            while (isAlive && list.Count < Count)
            {
                g.FillRectangle(dimBrush, 0, 0, Width, Height);

                foreach (myObj_010 s in list)
                {
                    s.Show();
                    s.Move();
                }

                list.Add(new myObj_010());

                form.Invalidate();
                System.Threading.Thread.Sleep(t);
            }

            while (isAlive)
            {
                g.FillRectangle(dimBrush, 0, 0, Width, Height);

                foreach (myObj_010 s in list)
                {
                    s.Show();
                    s.Move();
                }

                form.Invalidate();
                System.Threading.Thread.Sleep(t);
            }
*/
            return;
        }
    };
};
