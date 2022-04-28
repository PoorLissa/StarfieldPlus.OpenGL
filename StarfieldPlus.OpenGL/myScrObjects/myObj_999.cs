using GLFW;
using static OpenGL.GL;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections.Generic;


/*
    - Test
*/


namespace my
{
    public class myObj_999 : myObject
    {
        private float x, y, time = 0, dt = 0.01f;
        private float y1, x2, y2, x3, y3;

        // -------------------------------------------------------------------------

        public myObj_999()
        {
            if (colorPicker == null)
            {
                colorPicker = new myColorPicker(gl_Width, gl_Height, myColorPicker.colorMode.SNAPSHOT);
                list = new List<myObject>();
            }

            generateNew();
        }

        // -------------------------------------------------------------------------

        protected override void generateNew()
        {
            x = rand.Next(gl_Width);
            y = rand.Next(gl_Height);

            _a = (float)rand.NextDouble() + 0.02f;
            colorPicker.getColor(x, y, ref _r, ref _g, ref _b);

            y1 = rand.Next(66) + 5;
            x2 = 5 * y1 / 6;
            y2 = y1/2;
            x3 = 5 * y1 / 6;
            y3 = y1/2;

            time = 0.0f;
            dt = 0.002f * (rand.Next(33)+1);
        }

        // -------------------------------------------------------------------------

        protected override void Move()
        {
            time += dt;
        }

        // -------------------------------------------------------------------------

        protected override void Show()
        {
            int mode = 0;

            switch (mode)
            {
                case 0:
                    myPrimitive._T.SetAngle(time);
                    myPrimitive._T.SetColor(_r, _g, _b, _a);
                    myPrimitive._T.Draw(x, y - y1, x - x2, y + y2, x + x3, y + y3, false);
                    break;

                case 1:
                    myPrimitive._R.SetColor(_r, _g, _b, _a);
                    myPrimitive._R.Draw((int)x, (int)y, 50, 50, true);
                    break;
            }
        }

        // -------------------------------------------------------------------------

        protected override void Process(Window window)
        {
            if (myPrimitive._T == null)
                myPrimitive._T = new Triangle();

            if (myPrimitive._R == null)
                myPrimitive._R = new myRectangle();

            while (list.Count < 3333)
            {
                list.Add(new myObj_999());
            }

            while (!Glfw.WindowShouldClose(window))
            {
                processInput(window);

                // Swap fore/back framebuffers, and poll for operating system events.
                Glfw.SwapBuffers(window);
                Glfw.PollEvents();

                // Clear the framebuffer to defined background color
                glClearColor(0.2f, 0.3f, 0.3f, 1.0f);
                glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT);

                // Render frame:
                {
                    foreach (myObj_999 obj in list)
                    {
                        obj.Show();
                        obj.Move();
                    }
                }

                System.Threading.Thread.Sleep(20);
            }

            return;
        }
    }
};



namespace my
{
    public class myObj_999a : myObject
    {
        private float x, y, time = 0, dt = 0.01f;
        private float y1, x2, y2, x3, y3;

        // -------------------------------------------------------------------------

        public myObj_999a()
        {
            if (colorPicker == null)
            {
                colorPicker = new myColorPicker(gl_Width, gl_Height, myColorPicker.colorMode.SNAPSHOT);
                list = new List<myObject>();
            }

            generateNew();
        }

        // -------------------------------------------------------------------------

        protected override void generateNew()
        {
        }

        // -------------------------------------------------------------------------

        protected override void Move()
        {
            x = rand.Next(gl_Width);
            y = rand.Next(gl_Height);

        }

        // -------------------------------------------------------------------------

        protected override void Show()
        {
            colorPicker.getColor(x, y, ref _r, ref _g, ref _b);
            myPrimitive._R.SetColor(_r, _g, _b, 1);
            myPrimitive._R.Draw((int)x, (int)y, 25, 25, true);
        }

        // -------------------------------------------------------------------------

        // If you want to read a rectangular area form the framebuffer, then you can use GL.ReadPixels.For instance: https://stackoverflow.com/questions/64573427/save-drawn-texture-with-opengl-in-to-a-file

        protected override void Process(Window window)
        {
            if (myPrimitive._T == null)
                myPrimitive._T = new Triangle();

            if (myPrimitive._R == null)
                myPrimitive._R = new myRectangle();

            while (list.Count < 33)
            {
                list.Add(new myObj_999a());
            }

            //myTex tex = new myTex("d:\\tex.png");

            myTex tex = new myTex(colorPicker.getImg());

            while (!Glfw.WindowShouldClose(window))
            {
                processInput(window);

                // Swap fore/back framebuffers, and poll for operating system events.
                Glfw.SwapBuffers(window);
                Glfw.PollEvents();

                // Clear the framebuffer to defined background color
                glClearColor(0.2f, 0.3f, 0.3f, 1.0f);
                glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT);

                // Render frame:
                if (true)
                {
                    tex.Draw(0, 0, colorPicker.getImg().Width, colorPicker.getImg().Height);

                    foreach (myObj_999a obj in list)
                    {
                        obj.Show();
                        obj.Move();
                    }
                }

                System.Threading.Thread.Sleep(50);
            }

            return;
        }
    }
};
