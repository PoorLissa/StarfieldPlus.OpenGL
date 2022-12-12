using GLFW;
using static OpenGL.GL;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections.Generic;


/*
    - Spiraling in shapes

    todo:
        -- add more variance. slowly rotating out shapes, changin the radius change speed, changing time change speed; saw-looking dRad or dT functions;
*/


namespace my
{
    public class myObj_200 : myObject
    {
        private static bool doChangeBgrColor = false, randomDrad = false;
        private static int x0, y0, shapeType = 0, moveType = 0, rotationType = 0, dimMode = 0, t = 25, N = 1;
        private static float baseDt = 1.0f, dimAlpha = 0.025f;

        private float x, y, Rad, rad, drad, time = 0, dt = 0, R, G, B, A, lineTh, shape = 0;

        // -------------------------------------------------------------------------

        public myObj_200()
        {
            if (colorPicker == null)
            {
                x0 = gl_Width  / 2;
                y0 = gl_Height / 2;

                colorPicker = new myColorPicker(gl_Width, gl_Height);
                list = new List<myObject>();

                doChangeBgrColor = myUtils.randomBool(rand);
                randomDrad = myUtils.randomBool(rand);
                shapeType = rand.Next(6);

                moveType = rand.Next(2);
                dimMode = rand.Next(3);                         // 0 = const base value, 1 = const random value, 2 = oscillating value
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
                    rotationType = -100 - (myUtils.randomBool(rand) ? 0 : 1);
                }
                else
                {
                    rotationType = myUtils.randomSign(rand);
                }

                t = 1;

                if (baseDt > 0.5f)
                {
                    t += rand.Next(33);
                }

                if (dimMode == 1)
                {
                    dimAlpha = 0.001f + 0.001f * rand.Next(100);
                }
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
                dt *= (rotationType != 0) ? rotationType : myUtils.randomSign(rand);
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

            shape = shapeType != 5 ? shapeType : rand.Next(5);

            return;
        }

        // -------------------------------------------------------------------------

        protected override void Move()
        {
            int zzz = 66;

            // Switfly decrease too large radiuses
            rad -= rad > 666 ? (rad - 666) / 50 : 0;

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
                    myPrimitive._Rectangle.SetAngle(time / 10);

                    glLineWidth(3);
                    myPrimitive._Rectangle.SetColor(R, G, B, 0.15f);
                    myPrimitive._Rectangle.Draw(x - rad - 1, y - rad - 1, 2 * rad + 2, 2 * rad + 2, false);

                    glLineWidth(1);
                    myPrimitive._Rectangle.SetColor(R, G, B, A);
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
                    myPrimitive._Triangle.Draw(x, y - rad, x - 5 * rad / 6, y + rad / 2, x + 5 * rad / 6, y + rad / 2, false);
                    break;

                case 3:
                    myPrimitive._Hexagon.SetColor(R, G, B, A);
                    myPrimitive._Hexagon.SetAngle(time / 10);
                    myPrimitive._Hexagon.Draw(x, y, rad, false);
                    break;

                case 4:
                    myPrimitive._Pentagon.SetColor(R, G, B, A);
                    myPrimitive._Pentagon.SetAngle(time / 10);
                    myPrimitive._Pentagon.Draw(x, y, rad, false);
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
            myPrimitive.init_Pentagon();
            myPrimitive.init_Hexagon();
            myPrimitive.init_Ellipse();

            float dimR = 0, dimG = 0, dimB = 0;

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

                // Dim the screen constantly
                if (doClearBuffer == false)
                {
                    myPrimitive._Rectangle.SetAngle(0);
                    myPrimitive._Rectangle.SetColor(dimR, dimG, dimB, dimAlpha);
                    myPrimitive._Rectangle.Draw(0, 0, gl_Width, gl_Height, true);

                    if (doChangeBgrColor && cnt % 100 == 0)
                    {
                        dimR += (rand.Next(2) == 0) ? 0.01f : -0.01f;
                        dimG += (rand.Next(2) == 0) ? 0.01f : -0.01f;
                        dimB += (rand.Next(2) == 0) ? 0.01f : -0.01f;

                        if (dimR < 0) dimR = 0;
                        if (dimG < 0) dimG = 0;
                        if (dimB < 0) dimB = 0;

                        if (dimR > 0.25f) dimR = 0.25f;
                        if (dimG > 0.25f) dimG = 0.25f;
                        if (dimB > 0.25f) dimB = 0.25f;
                    }
                }

                // Oscillate dim speed
                if (dimMode == 2)
                {
                    dimAlpha += (float)(Math.Sin(cnt/100)) / 1000;
                }
            }

            return;
        }
    }
};
