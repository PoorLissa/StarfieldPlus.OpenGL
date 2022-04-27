using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;


/*
    - Star Field
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
            if (list == null)
            {
                list = new List<myObject>();
            }

            generateNew();
        }

        // -------------------------------------------------------------------------

        protected override void generateNew()
        {
            _a = (float)rand.NextDouble() + 0.02f;
            _r = (float)rand.NextDouble();
            _g = (float)rand.NextDouble();
            _b = (float)rand.NextDouble();

            x = rand.Next(gl_Width);
            y = rand.Next(gl_Height);

            y1 = rand.Next(66) + 5;
            x2 = y1;
            y2 = y1;
            x3 = y1;
            y3 = y1;

            time = 0.0f;
            dt = 0.001f * (rand.Next(33)+1);
        }

        // -------------------------------------------------------------------------

        protected override void Move()
        {
            time += dt;
        }

        // -------------------------------------------------------------------------

        protected override void Show()
        {
            myPrimitive._T.SetAngle(time);
            myPrimitive._T.SetColor(_r, _g, _b, _a);
            myPrimitive._T.Draw(x, y-y1, x-x2, y+y2, x+x3, y+y3, false);
        }

        // -------------------------------------------------------------------------

        protected override void Process(Window window)
        {
            if (myPrimitive._T == null)
            {
                myPrimitive._T = new Triangle();
            }

            while (list.Count < 333)
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
            }

            return;
        }
    }
};
