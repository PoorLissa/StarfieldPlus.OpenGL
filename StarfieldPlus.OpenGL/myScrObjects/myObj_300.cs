using GLFW;
using static OpenGL.GL;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections.Generic;


/*
    - 
*/


namespace my
{
    public class Obj
    {
        public float x, y, r, dx, dy, a;
    };

    public class myObj_300 : myObject
    {
        private static bool doClearBuffer = false, doChangeBgrColor = false, randomDrad = false, isCenter = true;
        private static int x0, y0, shapeType = 0, moveType = 0, dimMode = 0, t = 25, N = 1, daBase = 0;
        private static float baseDt = 1.0f, dimAlpha = 0.025f;

        private float x, y, Rad, rad, drad, time1 = 0, dt1 = 0, time2 = 0, dt2 = 0, R, G, B, A, dA, lineTh, shape = 0;

        private int lifeCounter = 0, lifeMax = 0;
        private int dt1Counter = 0, dt1CounterMax = 0;
        private float ddt1 = 0;

        private List<Obj> lst = null;

        // -------------------------------------------------------------------------

        public myObj_300()
        {
            if (colorPicker == null)
            {
                x0 = gl_Width  / 2;
                y0 = gl_Height / 2;

                colorPicker = new myColorPicker(gl_Width, gl_Height, myColorPicker.colorMode.RANDOM);
                list = new List<myObject>();

                doChangeBgrColor = myUtils.randomBool(rand);
                randomDrad = myUtils.randomBool(rand);
                shapeType = rand.Next(4);
                moveType = rand.Next(2);
                dimMode = rand.Next(3);                         // 0 = const base value, 1 = const random value, 2 = oscillating value

                // Set number of objects N:
                N = rand.Next(11) + 3;

doChangeBgrColor = false;
dimMode = 0;
dimAlpha = 0.5f;
shapeType = 4;
moveType = 0;
N = 100;

                if (dimMode == 1)
                {
                    dimAlpha = 0.001f + 0.001f * rand.Next(100);
                }

                t = 1;
            }

            lst = new List<Obj>();

            for (int i = 0; i < 25; i++)
            {
                lst.Add(new Obj());
            }

            generateNew();
        }

        // -------------------------------------------------------------------------

        protected override void generateNew()
        {
            x = rand.Next(gl_Width);
            y = rand.Next(gl_Height);

            A = (float)rand.NextDouble() + 0.1f;

            colorPicker.getColorRand(ref R, ref G, ref B);

            shape = shapeType;

            lifeCounter = 0;
            lifeMax = rand.Next(333) + 333;

            foreach (var obj in lst)
            {
                obj.x = x;
                obj.y = y;
                obj.r = 5;
                obj.dx = 0.001f * (rand.Next(1000) - 500);
                obj.dy = 0.001f * (rand.Next(1000) - 500);
                obj.a = (float)rand.NextDouble() + 0.33f;
            }

            return;
        }

        // -------------------------------------------------------------------------

        protected override void Move()
        {
            lifeCounter++;

            switch (moveType)
            {
                case 0:

                    foreach (var obj in lst)
                    {
                        obj.x += obj.dx;
                        obj.y += obj.dy;
                        obj.a -= 0.005f;
                    }

                    if (lifeCounter > lifeMax)
                    {
                        generateNew();
                    }
                    break;
            }

            return;
        }

        // -------------------------------------------------------------------------

        protected override void Show()
        {
            switch (shape)
            {
                case 0:
                    myPrimitive._Hexagon.SetAngle(time1);
                    myPrimitive._Hexagon.SetColor(R, G, B, A);
                    myPrimitive._Hexagon.Draw(x, y, rad, false);
                    break;

                case 1:
                    glLineWidth(lineTh);
                    myPrimitive._Rectangle.SetAngle(time1);
                    myPrimitive._Rectangle.SetColor(R, G, B, A);
                    myPrimitive._Rectangle.Draw(x - rad, y - rad, 2 * rad, 2 * rad, false);
                    break;

                case 2:
                    glLineWidth(lineTh);
                    myPrimitive._Ellipse.SetColor(R, G, B, A/2);
                    myPrimitive._Ellipse.Draw(x-rad, y-rad, 2*rad, 2*rad, false);

                    myPrimitive._Triangle.SetAngle(time1);
                    myPrimitive._Triangle.SetColor(R, G, B, A);
                    myPrimitive._Triangle.Draw(x, y - rad, x - 5 * rad / 6, y + rad / 2, x + 5 * rad / 6, y + rad / 2, false);
                    break;

                case 3:
                    myPrimitive._Pentagon.SetAngle(time1);
                    myPrimitive._Pentagon.SetColor(R, G, B, A);
                    myPrimitive._Pentagon.Draw(x, y, rad, false);
                    break;

                case 4:
                    glLineWidth(1);

                    foreach (var obj in lst)
                    {
                        myPrimitive._Ellipse.SetColor(R, G, B, obj.a);
                        myPrimitive._Ellipse.Draw(obj.x - obj.r, obj.y - obj.r, 2 * obj.r, 2 * obj.r, false);
                    }
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
                list.Add(new myObj_300());
            }

            while (!Glfw.WindowShouldClose(window))
            {
                cnt++;

                processInput(window);

                // Swap fore/back framebuffers, and poll for operating system events.
                Glfw.SwapBuffers(window);
                Glfw.PollEvents();

                foreach (myObj_300 obj in list)
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
