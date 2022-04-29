using GLFW;
using static OpenGL.GL;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections.Generic;


/*
    - Spiraling in shapes
*/


namespace my
{
    public class myObj_200 : myObject
    {
        private static bool doClearBuffer = false, randomDrad = false;
        private static int x0, y0, shape = 0, moveType = 0, rotationType = 0, t = 25, N = 1;
        private static float baseDt = 1.0f;

        private float x, y, Rad, rad, drad, time = 0, dt = 0, R, G, B, A, lineTh;

        // -------------------------------------------------------------------------

        public myObj_200()
        {
            if (colorPicker == null)
            {
                x0 = gl_Width  / 2;
                y0 = gl_Height / 2;

                colorPicker = new myColorPicker(gl_Width, gl_Height);
                list = new List<myObject>();

                randomDrad = rand.Next(2) == 0;
                shape = rand.Next(3);
                moveType = rand.Next(2);
                baseDt = 0.001f + 0.001f * rand.Next(1000);

                // Set number of objects N:
                switch (rand.Next(3))
                {
                    case 0: N = rand.Next(03) + 1; break;
                    case 1: N = rand.Next(11) + 1; break;
                    case 2: N = rand.Next(66) + 1; break;
                }

                // Set rotation type for all the shapes: [no rotation, -1, 0, +1]
                if (rand.Next(5) == 0)
                {
                    rotationType = -100 - (rand.Next(2) == 0 ? 0 : 1);
                }
                else
                {
                    rotationType = rand.Next(3) - 1;
                }

                t = 1;
            }

            Rad = rand.Next(gl_Height-333) + 333;
            dt = baseDt * (rand.Next(100) + 1);
            drad = 0.5f * (rand.Next(5) + 1);

            // Set rotation type for the shape
            if (rotationType <= -100)
            {
                dt = 0;
            }
            else
            {
                // -1, +1 or random[-1 / +1]
                dt *= (rotationType != 0) ? rotationType : (rand.Next(2) == 0 ? 1 : -1);
            }

            generateNew();
        }

        // -------------------------------------------------------------------------

        protected override void generateNew()
        {
            x = x0;
            y = y0;
            rad = Rad;

            lineTh = rand.Next(5) + 1;
            A = (float)rand.NextDouble() + 0.1f;

            colorPicker.getColorRand(ref R, ref G, ref B);

            if (randomDrad)
            {
                drad = 0.5f * (rand.Next(33) + 1);
            }

            if (rotationType == -101)
            {
                // No rotation, but the angle is different for every newly generated shape
                time = (float)rand.NextDouble() * 1234;
            }

            return;
        }

        // -------------------------------------------------------------------------

        protected override void Move()
        {
            int zzz = 66;

            switch (moveType)
            {
                // Spiraling to the center
                case 0:
                    rad -= drad;
                    time += dt;
                    break;

                // Spiraling to the center, but the center coordinates are randomized a bit
                case 1:

                    if (shape == 1 || shape == 2)
                        zzz = 33;

                    zzz = rad > zzz ? zzz : (int)rad;

                    x = x0 + (zzz - rand.Next(2*zzz));
                    y = y0 + (zzz - rand.Next(2*zzz));

                    rad -= drad;
                    time += dt;
                    break;

                // Spiraling to the center, but the center coordinates are moving ellptically
                case 2:
                    x = x0 + (float)Math.Sin(time) * 111;
                    y = y0 + (float)Math.Cos(time) * 111;

                    rad -= drad;
                    time += dt;
                    break;
            }

            if (rad <= 0)
            {
                generateNew();
            }
        }

        // -------------------------------------------------------------------------

        protected override void Show()
        {
            switch (shape)
            {
                case 0:
                    myPrimitive._Rectangle.SetColor(R, G, B, A);
                    myPrimitive._Rectangle.SetAngle(time / 10);
                    myPrimitive._Rectangle.Draw(x - rad, y - rad, 2 * rad, 2 * rad, false);
                    break;

                case 1:
                    myPrimitive._Ellipse.SetColor(R, G, B, A);
                    myPrimitive._Ellipse.setLineThickness(lineTh);
                    myPrimitive._Ellipse.Draw(x - rad, y - rad, 2 * rad, 2 * rad, false);
                    break;

                case 2:
                    myPrimitive._Triangle.SetColor(R, G, B, A);
                    myPrimitive._Triangle.SetAngle(time / 10);
                    myPrimitive._Triangle.Draw(x, y - rad, x - 5*rad/6, y + rad/2, x + 5*rad/6, y + rad/2, false);
                    break;

                case 3:
                    myPrimitive._Hexagon.SetColor(R, G, B, A);
                    myPrimitive._Hexagon.SetAngle(time / 10);
                    myPrimitive._Hexagon.Draw(x, y, rad, false);
                    break;
            }

            return;
        }

        // -------------------------------------------------------------------------

        protected override void Process(Window window)
        {
            uint cnt = 0;

            myPrimitive.init_Triangle();
            myPrimitive.init_Rectangle();
            myPrimitive.init_Hexagon();
            myPrimitive.init_Ellipse();

            if (doClearBuffer == false)
            {
                glDrawBuffer(GL_FRONT_AND_BACK);
            }
    
            while (list.Count < N)
            {
                list.Add(new myObj_200());
            }

            while (!Glfw.WindowShouldClose(window))
            {
                cnt++;

                processInput(window);

                // Swap fore/back framebuffers, and poll for operating system events.
                Glfw.SwapBuffers(window);
                Glfw.PollEvents();

                foreach (myObj_200 obj in list)
                {
                    obj.Show();
                    obj.Move();
                }

                System.Threading.Thread.Sleep(t);

                if (doClearBuffer == false)
                {
                    myPrimitive._Rectangle.SetColor(0, 0, 0, 0.025f);
                    myPrimitive._Rectangle.Draw(0, 0, gl_Width, gl_Height, true);
                }
            }

            return;
        }
    }
};
