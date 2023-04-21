using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;


/*
    - Desktop pieces move from the offscreen postion into their original positions
*/


namespace my
{
    public class myObj_072 : myObject
    {
        // Priority
        public static int Priority => 10;

        private int cnt;
        private float x, y, X, Y, dx, dy;
        private float size, A, R, G, B;

        private static int N = 0, mode = 0, sizeMode = 0;
        private static int maxSize = 0;
        private static float dimAlpha = 0.05f;

        static myTexRectangle tex = null;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_072()
        {
            if (id != uint.MaxValue)
                generateNew();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time global initialization
        protected override void initGlobal()
        {
            colorPicker = new myColorPicker(gl_Width, gl_Height, mode: myColorPicker.colorMode.SNAPSHOT_OR_IMAGE);
            list = new List<myObject>();

            // Global unmutable constants
            {
                switch (rand.Next(3))
                {
                    case 0:
                        N = rand.Next(300) + 300;
                        break;

                    case 1:
                        N = rand.Next(450) + 450;
                        break;

                    case 2:
                        N = rand.Next(600) + 600;
                        break;
                }
            }

            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time local initialization
        private void initLocal()
        {
            doClearBuffer = myUtils.randomBool(rand);

            mode = rand.Next(2);
            sizeMode = rand.Next(2);

            maxSize = 50 + rand.Next(100);

            renderDelay = rand.Next(11) + 1;

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            height = 600;

            string nStr(int   n) { return n.ToString("N0");    }
            string fStr(float f) { return f.ToString("0.000"); }

            string str = $"Obj = myObj_072\n\n"                      +
                            $"N = {nStr(list.Count)} of {nStr(N)}\n" +
                            $"mode = {mode}\n"                       +
                            $"sizeMode = {sizeMode}\n"               +
                            $"maxSize = {maxSize}\n"                 +
                            $"dimAlpha = {fStr(dimAlpha)}\n"         +
                            $"renderDelay = {renderDelay}\n"         +
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
            cnt = 0;

            switch (sizeMode)
            {
                case 0:
                    size = maxSize;
                    break;

                case 1:
                    size = rand.Next(maxSize) + 5;
                    break;
            }

            X = rand.Next(gl_Width);
            Y = rand.Next(gl_Height);

            colorPicker.getColor(X, Y, ref R, ref G, ref B);

            A = myUtils.randFloat(rand) * 0.5f;

            X -= size / 2;
            Y -= size / 2;

            dx = dy = 0;

            float dxy = myUtils.randFloat(rand, 0.1f) * 10.0f;

            switch (rand.Next(4))
            {
                case 0:
                    x = X;
                    y = -200;
                    dy = +dxy;
                    break;

                case 1:
                    x = X;
                    y = gl_Height + 200;
                    dy = -dxy;
                    break;

                case 2:
                    y = Y;
                    x = -200;
                    dx = +dxy;
                    break;

                case 3:
                    y = Y;
                    x = gl_Width + 200;
                    dx = -dxy;
                    break;
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            if (cnt == 0)
            {
                x += dx;
                y += dy;

                if (dx == 0)
                {
                    if (y >= Y == dy > 0)
                    {
                        cnt = -10;
                    }
                }
                else
                {
                    if (x >= X == dx > 0)
                        cnt = -10;
                }
            }
            else
            {
                if (cnt == -10)
                {
                    x = X;
                    y = Y;

                    dx = dy = 0;

                    A = 0.85f + myUtils.randFloat(rand) * 0.15f;

                    cnt = rand.Next(100) + 100;
                }
                else
                {
                    if (cnt > 1)
                    {
                        cnt--;
                    }
                    else
                    {
                        if (A > 0)
                        {
                            A -= myUtils.randFloat(rand) * 0.05f;
                        }
                        else
                        {
                            generateNew();
                        }
                    }
                }
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Show()
        {
            float size2x = size * 2;

            mode = 0;

            switch (mode)
            {
                case 0:
                    {
                        if (cnt == 0)
                        {
                            myPrimitive._Rectangle.SetColor(R, G, B, A * 2);
                            myPrimitive._Rectangle.Draw(x, y, size, size, false);
                        }
                        else
                        {
                            tex.setOpacity(A);
                            tex.Draw((int)X, (int)Y, (int)size, (int)size, (int)x, (int)y, (int)size, (int)size);
                        }
                    }
                    break;

                case 1:
                    {
                        tex.setOpacity(A);
                        tex.Draw((int)x, (int)y, (int)size, (int)size, (int)X, (int)Y, (int)size, (int)size);
                    }
                    break;
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Process(Window window)
        {
            uint cnt = 0;
            initShapes();


            clearScreenSetup(doClearBuffer, 0.1f);


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
                        var obj = list[i] as myObj_072;

                        obj.Show();
                        obj.Move();
                    }
                }

                if (list.Count < N)
                {
                    list.Add(new myObj_072());
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
            myPrimitive.init_Rectangle();

            tex = new myTexRectangle(colorPicker.getImg());

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------
    }
};
