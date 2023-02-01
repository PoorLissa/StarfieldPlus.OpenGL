using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;


/*
    - ...
*/


namespace my
{
    public class myObj_110 : myObject
    {
        private int x, y, w, h, size;
        private float A, R, G, B, angle = 0;

        private static int N = 0, shape = 0, borderMode = 0, maxSize = 0;
        private static bool doFillShapes = false, doUseRandomSize = false;
        private static float dimAlpha = 0.05f;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_110()
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
                doClearBuffer = false;

                doUseRandomSize = true;

                N = 1;

                shape = rand.Next(3);
                shape = 0;
            }

            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time local initialization
        private void initLocal()
        {
            //doUseRandomSize = myUtils.randomChance(rand, 1, 2);

            switch (rand.Next(5))
            {
                case 0:
                    maxSize = rand.Next(666) + 1;
                    break;

                case 1:
                    maxSize = rand.Next(444) + 25;
                    break;

                case 2:
                    maxSize = rand.Next(333) + 50;
                    break;

                case 3:
                    maxSize = rand.Next(222) + 75;
                    break;

                case 4:
                    maxSize = rand.Next(111) + 100;
                    break;
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            height = 600;

            string nStr(int   n) { return n.ToString("N0");    }
            string fStr(float f) { return f.ToString("0.000"); }

            string str = $"Obj = myObj_110\n\n"                       +
                            $"N = {nStr(list.Count)} of {nStr(N)}\n"    +
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
            if (doUseRandomSize)
            {
                size = rand.Next(maxSize) + 3;
            }

            x = rand.Next(gl_Width)  - size;
            y = rand.Next(gl_Height) - size;

            w = size * 2;
            h = size * 2;

            A = myUtils.randFloat(rand);
            R = myUtils.randFloat(rand);
            G = myUtils.randFloat(rand);
            B = myUtils.randFloat(rand);

            //colorPicker.getColor(x, y, ref R, ref G, ref B);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            generateNew();

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Show()
        {
            float size2x = size * 2;

            switch (shape)
            {
                case 0:
                    myPrimitive._Rectangle.SetColor(R, G, B, A);
                    myPrimitive._Rectangle.Draw(x, y, w, h, true);
                    break;
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Process(Window window)
        {
            uint cnt = 0;
            initShapes();

            // Disable VSYNC if needed
            // Glfw.SwapInterval(0);

            dimScreenRGB_SetRandom(0.1f);

            if (doClearBuffer)
            {
                glDrawBuffer(GL_FRONT_AND_BACK | GL_DEPTH_BUFFER_BIT);
                glClearColor(myObject.bgrR, myObject.bgrG, myObject.bgrB, 1);
            }
            else
            {
                glDrawBuffer(GL_BACK);
                glDrawBuffer(GL_FRONT_AND_BACK);
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
                        //dimScreen(dimAlpha);
                    }
                }

                // Render Frame
                {
                    for (int i = 0; i != list.Count; i++)
                    {
                        var obj = list[i] as myObj_110;

                        obj.Show();
                        obj.Move();
                    }
                }

                if (list.Count < N)
                {
                    list.Add(new myObj_110());
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

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------
    }
};
