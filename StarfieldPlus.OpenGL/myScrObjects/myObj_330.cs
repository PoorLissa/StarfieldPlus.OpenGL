using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;


/*
    -- Textures, take 1
*/


namespace my
{
    public class myObj_330 : myObject
    {
        private const int N = 50;

        static bool doClearBuffer = false;
        static float dimAlpha = 0.05f, dimAlphaOld = 0;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_330()
        {
            if (colorPicker == null)
            {
                colorPicker = new myColorPicker(gl_Width, gl_Height, mode: myColorPicker.colorMode.SNAPSHOT_OR_IMAGE);
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

            //doClearBuffer = myUtils.randomBool(rand);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            string str = $"Obj = myObj_330\n\n" +
                            $"N = {N} ({list.Count})\n"
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
            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Process(Window window)
        {
            int maxCnt = 333;
            int cnt = rand.Next(maxCnt) + 11;
            int mode = rand.Next(2);

            initShapes();

            if (doClearBuffer)
            {
                glDrawBuffer(GL_FRONT_AND_BACK | GL_DEPTH_BUFFER_BIT);
                glClearColor(0, 0, 0, 1);
            }
            else
            {
                glDrawBuffer(GL_FRONT_AND_BACK);
            }

            myTex tex1 = new myTex(colorPicker.getImg());

            while (!Glfw.WindowShouldClose(window))
            {
                processInput(window);

                Glfw.SwapBuffers(window);
                Glfw.PollEvents();

                if (doClearBuffer)
                {
                    if (cnt > 0)
                        cnt--;
                    else
                    {
                        cnt = rand.Next(maxCnt) + 11;
                        glClear(GL_COLOR_BUFFER_BIT);
                    }
                }
                else
                {
                    dimScreen(dimAlpha / 10, false);
                }

                for (int i = 0; i < 10; i++)
                {
                    int x = rand.Next(gl_Width);
                    int y = rand.Next(gl_Height);
                    int w = rand.Next(133) + 1;
                    int h = rand.Next(133) + 1;

                    if (mode == 0)
                    {
                        int rndx = rand.Next(7) - 3;
                        int rndy = rand.Next(7) - 3;
                        x += rndx;
                        y += rndy;
                    }

                    if (mode == 1)
                    {
                        int W = (rand.Next(10) == 0) ? 2 : 1;

                        if (rand.Next(2) == 0)
                        {
                            w = rand.Next(333) + 1;
                            h = W;
                        }
                        else
                        {
                            h = rand.Next(333) + 1;
                            w = W;
                        }
                    }

                    tex1.Draw(x, y, w, h, x, y, w, h);
                }
            }
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void initShapes()
        {
            myPrimitive.init_Rectangle();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // Dim the screen constantly
        private void dimScreen(float dimAlpha, bool useStrongerDimFactor = false)
        {
            int rnd = rand.Next(101), dimFactor = 1;

            if (useStrongerDimFactor && rnd < 11)
            {
                dimFactor = (rnd == 0) ? 5 : 2;
            }

            myPrimitive._Rectangle.SetAngle(0);

            // Shift background color just a bit, to hide long lasting traces of shapes
            myPrimitive._Rectangle.SetColor(rand.Next(5) * 0.01f, rand.Next(5) * 0.01f, rand.Next(5) * 0.01f, dimAlpha * dimFactor);
            myPrimitive._Rectangle.Draw(0, 0, gl_Width, gl_Height, true);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------
    }
};
