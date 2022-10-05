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
        struct Obj
        {
            public float x, y, dx, dy;
            public int w;
        };

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

            myTexRectangle tex1 = new myTexRectangle(colorPicker.getImg());

            const int n = 1000;

            Obj[] arr = new Obj[n];
            tex1.setOpacity(0.23f);
            for (int i = 0; i < n; i++)
            {
                arr[i].w = rand.Next(33) + 5;
                arr[i].x = rand.Next(gl_Width);
                arr[i].y = rand.Next(gl_Height);
                arr[i].dx = (float)rand.NextDouble() * myUtils.randomSign(rand) * 5;
                arr[i].dy = (float)rand.NextDouble() * myUtils.randomSign(rand) * 5;

                continue;

                if (rand.Next(2) == 0)
                {
                    arr[i].dx = (float)rand.NextDouble() * myUtils.randomSign(rand) * 5;
                    arr[i].dy = 0;
                }
                else
                {
                    arr[i].dx = 0;
                    arr[i].dy = (float)rand.NextDouble() * myUtils.randomSign(rand) * 5;
                }
            }


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

                {
                    glClear(GL_COLOR_BUFFER_BIT);

                    for (int i = 0; i < n; i++)
                    {
                        tex1.Draw((int)arr[i].x - arr[i].w, (int)arr[i].y - arr[i].w, 2*arr[i].w, 2*arr[i].w, (int)arr[i].x - arr[i].w, (int)arr[i].y - arr[i].w, 2*arr[i].w, 2*arr[i].w);

                        arr[i].x += arr[i].dx;
                        arr[i].y += arr[i].dy;

                        if (arr[i].x < 0 || arr[i].x > gl_Width)
                            arr[i].dx *= -1;

                        if (arr[i].y < 0 || arr[i].y > gl_Height)
                            arr[i].dy *= -1;
                    }

                    continue;
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

                        tex1.setOpacity(133.0f / w / h);
                    }

                    if (mode == 1)
                    {
                        int W = (rand.Next(10) == 0) ? 2 : 1;

                        int max = 666;

                        if (rand.Next(2) == 0)
                        {
                            w = rand.Next(max) + 1;
                            h = W;

                            tex1.setOpacity(0.1f * max / w);
                        }
                        else
                        {
                            h = rand.Next(max) + 1;
                            w = W;

                            tex1.setOpacity(0.1f * max / h);
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
