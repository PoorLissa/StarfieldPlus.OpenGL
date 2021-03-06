using GLFW;
using static OpenGL.GL;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections.Generic;


/*
    - Test
*/


#pragma warning disable IDE0051
#pragma warning disable CS0414
#pragma warning disable CS0169
#pragma warning disable CS0162


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
                colorPicker = new myColorPicker(gl_Width, gl_Height, myColorPicker.colorMode.SNAPSHOT_OR_IMAGE);
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
            myPrimitive._Rectangle.SetColor(_r, _g, _b, 1);
            myPrimitive._Rectangle.Draw((int)x, (int)y, 25, 25, true);
        }

        // -------------------------------------------------------------------------

        // If you want to read a rectangular area form the framebuffer, then you can use GL.ReadPixels
        // For instance: https://stackoverflow.com/questions/64573427/save-drawn-texture-with-opengl-in-to-a-file

        protected override void Process(Window window)
        {
            myPrimitive.init_Line();
            myPrimitive.init_Triangle();
            myPrimitive.init_Rectangle();
            myPrimitive.init_Hexagon();
            myPrimitive.init_Ellipse();
            myPrimitive.init_Rectangle();

            while (list.Count < 333)
            {
                list.Add(new myObj_999a());
            }

            // it's static and not loading the second time
            myTex tex1 = new myTex(colorPicker.getImg());
            //myTex tex2 = new myTex(@"C:\_maxx\tex_star.png");

            int x1 = 666;
            int y1 = 666;
            int z1 = 200;

            int x0 = 500;
            int y0 = 500;
            int w0 = 500;
            int h0 = 500;

            int xx1 = 0;
            int yy1 = 0;

            uint cnt = 0;

            bool doClearBuffer = false;

            if (doClearBuffer == false)
            {
                glDrawBuffer(GL_FRONT_AND_BACK);
            }

            float t = 0, dt = 0.1f;

            while (!Glfw.WindowShouldClose(window))
            {
                cnt++;

                processInput(window);

                // Swap fore/back framebuffers, and poll for operating system events.
                Glfw.SwapBuffers(window);
                Glfw.PollEvents();

                // Clear the framebuffer to defined background color
                if (doClearBuffer)
                {
                    glClearColor(0.2f, 0.3f, 0.3f, 1.0f);
                    glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT);
                }

                // Render frame:
                // Copy rectangles from the texture
                // drops lots of colored rectangles and sometimes draws a piece of real picture -- need this
                if (false)
                {
                    foreach (myObj_999a obj in list)
                    {
                        obj.Show();
                        obj.Move();
                    }

/*
                    tex1.Draw(0, 0, gl_Width, gl_Height);
                    myPrimitive._Rectangle.SetColor(0.5f, 0.5f, 0.5f, 0.66f);
                    myPrimitive._Rectangle.Draw(0, 0, gl_Width, gl_Height, true);
*/

                    tex1.Draw(x0, y0, w0, h0, x0, y0, w0, h0);
                    //tex2.Draw(x1, y1, z1, z1);

                    if (cnt % 33 == 0)
                    {
                        x1 = rand.Next(gl_Width);
                        y1 = rand.Next(gl_Height);
                        z1 = rand.Next(300) + 100;
                    }

                    if (cnt % 50 == 0)
                    {
                        x0 = rand.Next(gl_Width);
                        y0 = rand.Next(gl_Height);
                        w0 = rand.Next(500) + 50;
                        h0 = rand.Next(500) + 50;
                    }
                }

                // need this option -- if not already
                if (!true)
                {
                    for (int i = 0; i < 100; i++)
                    {
                        //myPrimitive._Rectangle.SetColor((float)rand.NextDouble(), (float)rand.NextDouble(), (float)rand.NextDouble(), (float)rand.NextDouble());
                        myPrimitive._Hexagon.SetColor((float)rand.NextDouble(), (float)rand.NextDouble(), (float)rand.NextDouble(), (float)rand.NextDouble());

                        int x = rand.Next(gl_Width);
                        int y = rand.Next(gl_Height);
                        int r = rand.Next(33) + 1;

                        //myPrimitive._Rectangle.Draw(x, y, r, r, true);
                        //myPrimitive._Hexagon.SetAngle((float)rand.NextDouble() * 11);
                        myPrimitive._Hexagon.SetAngle(cnt * 0.1f);
                        myPrimitive._Hexagon.Draw(x, y, r, false);
                    }
                }

                // already have something like this, but need this as well
                if (false)
                {
                    myPrimitive._Hexagon.SetColor(1, 0, 0, 1);

                    int x = gl_Width/2;
                    int y = gl_Height /2;
                    int r = 333;

                    myPrimitive._Hexagon.SetAngle(t);
                    myPrimitive._Hexagon.Draw(x, y, r, false);

                    //myPrimitive._Hexagon.SetColor(1, 0.3f, 0, 0.25f);
                    myPrimitive._Hexagon.SetColor(1, 0.3f, 0, 0.9f);
                    myPrimitive._Hexagon.Draw(x, y, (int)(3 * r * Math.Sin(t/100)), false);

                    myPrimitive._Hexagon.SetColor(1, 0.3f, 0, 0.25f);
                    myPrimitive._Hexagon.Draw(x, y, 2 * r, false);

                    myPrimitive._Hexagon.SetColor(1, 0.3f, 0.5f, 0.99f);
                    myPrimitive._Hexagon.Draw(x, y, (int)(5 * r * Math.Sin(t / 333)), false);


                    if (cnt % 50 == 0)
                    {
                        dt -= 0.01f;
                    }
                }

                if (false)
                {
                    int x = gl_Width / 2;
                    int y = gl_Height / 2;

                    myPrimitive._Ellipse.SetColor(1, 0, 0, 1);
                    myPrimitive._Ellipse.Draw(x-x1+50, y - x1+50, 2*x1-100, 2*x1-100, false);

                    myPrimitive._Hexagon.SetColor(1, 0, 0, 1);
                    myPrimitive._Hexagon.SetAngle(t);
                    myPrimitive._Hexagon.Draw(x, y, x1, false);
                    x1 -= 2;

                    myPrimitive._Hexagon.SetColor(1, 0, 1, 1);
                    myPrimitive._Hexagon.SetAngle(t);
                    myPrimitive._Hexagon.Draw(x, y, y1, false);
                    y1 -= 1;

                    if (x1 < 0)
                    {
                        x1 = gl_Height;
                    }

                    if (y1 < 0)
                    {
                        y1 = gl_Height;
                    }

                    if (cnt % 50 == 0)
                    {
                        dt -= 0.01f;
                    }
                }

                if (false)
                {
                    int x = gl_Width  / 2;
                    int y = gl_Height / 2;

                    myPrimitive._Rectangle.SetColor(1, 0, 0, 1);
                    myPrimitive._Rectangle.SetAngle(t/10);
                    myPrimitive._Rectangle.Draw(x - x1, y - x1, 2 * x1, 2 * x1, false);
                    x1 -= 2;

                    if (x1 < 0)
                    {
                        x1 = gl_Height;
                    }

                    if (cnt % 50 == 0)
                    {
                        //dt -= 0.01f;
                    }
                }

                if (true)
                {
                    myPrimitive._Line.SetColor(1, 1, 0, 1);

                    myPrimitive._Line.SetAngle(t/2);
                    myPrimitive._Line.SetColor(1, 1, 0, 0.1f);
                    myPrimitive._Line.Draw(xx1 + 100, yy1 + 100, xx1 + 100, yy1 + 200, 3);

                    myPrimitive._Line.SetColor(1, 1, 0, 1);
                    myPrimitive._Line.Draw(xx1 + 100, yy1 + 100, xx1 + 100, yy1 + 200, 1);
                }

                System.Threading.Thread.Sleep(25);
                t += dt;

                if (doClearBuffer == false)
                {
                    //myPrimitive._Rectangle.SetColor(1, 1, 1, 0.005f);
                    myPrimitive._Rectangle.SetColor(0, 0, 0, 0.025f);
                    myPrimitive._Rectangle.Draw(0, 0, gl_Width, gl_Height, true);
                }
            }

            return;
        }
    }
};

#pragma warning restore IDE0051
#pragma warning restore CS0414
#pragma warning restore CS0169
#pragma warning restore CS0162
