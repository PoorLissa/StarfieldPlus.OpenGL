using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;


/*
    - ...
*/


namespace my
{
    public class myObj_480 : myObject
    {
        private float x, y, oldx, oldy, dx, dy;
        private float size, A, R, G, B;

        private static int N = 0, colorMode = 0;
        private static bool doFillShapes = false;
        private static float dimAlpha = 0.05f, t = 0, dt = 0;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_480()
        {
            generateNew();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time global initialization
        protected override void initGlobal()
        {
            colorPicker = new myColorPicker(gl_Width, gl_Height);
            list = new List<myObject>();

            // Global unmutable constants
            {
                N = 1;
            }

            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time local initialization
        private void initLocal()
        {
            doClearBuffer = myUtils.randomBool(rand);

            colorMode = rand.Next(2);

            dt = myUtils.randomSign(rand) * myUtils.randFloat(rand, 0.1f) * 0.1f;

            dimAlpha = 0.1f + myUtils.randFloat(rand) * 0.2f;

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            height = 600;

            string nStr(int   n) { return n.ToString("N0");    }
            string fStr(float f) { return f.ToString("0.000"); }

            string str = $"Obj = myObj_480\n\n"                       +
                            $"N = {nStr(list.Count)} of {nStr(N)}\n"    +
                            $"renderDelay = {renderDelay}\n"            +
                            $"file: {colorPicker.GetFileName()}"
                ;
            return str;
        }

        // ---------------------------------------------------------------------------------------------------------------

        // 
        protected override void setNextMode()
        {
            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void generateNew()
        {
            x = 0;
            y = gl_y0;

            dx = 5;
            dy = 0;

            size = rand.Next(333) + 111;

            colorPicker.getColorRand(ref R, ref G, ref B);
            A = 1;

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            t += dt;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Show()
        {
            float xFactor = 0.01f;

            x = 0;
            y = gl_y0 + (float)Math.Sin(x * xFactor + t) * size;

            myPrimitive._LineInst.ResetBuffer();

            while (x <= gl_Width)
            {
                oldx = x;
                oldy = y;

                x += dx;
                y = (float)Math.Sin(x * xFactor + t);
                y += (float)Math.Cos(y*t);

                y = gl_y0 + y * size;

                myPrimitive._LineInst.setInstanceCoords(x, y, oldx, oldy);

                if (colorMode == 1)
                {
                    colorPicker.getColor(x, y, ref R, ref G, ref B);
                }

                myPrimitive._LineInst.setInstanceColor(R, G, B, A);
            }

            myPrimitive._LineInst.Draw();

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Process(Window window)
        {
            uint cnt = 0;
            initShapes();

            // Disable VSYNC if needed
            // Glfw.SwapInterval(0);

            if (doClearBuffer)
            {
                glDrawBuffer(GL_FRONT_AND_BACK | GL_DEPTH_BUFFER_BIT);
                glClearColor(0, 0, 0, 1);
            }
            else
            {
                dimScreenRGB_SetRandom(0.1f);
                glDrawBuffer(GL_FRONT_AND_BACK);
                //glDrawBuffer(GL_DEPTH_BUFFER_BIT);
            }

            while (!Glfw.WindowShouldClose(window))
            {
                processInput(window);

                // Swap fore/back framebuffers, and poll for operating system events.
                Glfw.SwapBuffers(window);
                Glfw.PollEvents();

                // Dim screen
                {
                    if (doClearBuffer)
                    {
                        glClear(GL_COLOR_BUFFER_BIT);
                    }
                    else
                    {
                        dimScreen(dimAlpha);
                    }
                }

                // Render Frame
                {
                    for (int i = 0; i != list.Count; i++)
                    {
                        var obj = list[i] as myObj_480;

                        obj.Show();
                        obj.Move();
                    }
                }

                if (list.Count < N)
                {
                    list.Add(new myObj_480());
                }

                cnt++;
                System.Threading.Thread.Sleep(renderDelay);
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void initShapes()
        {
            myPrimitive.init_ScrDimmer();

            myPrimitive.init_LineInst(gl_Width);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------
    }
};
