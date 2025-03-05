using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;
using System.Net;


/*
    - ...
*/


namespace my
{
    public class myObj_1130 : myObject
    {
        // Priority
        public static int Priority => 99910;
		public static System.Type Type => typeof(myObj_1130);

        private bool firstIteration, firstLine;
        private float x, y;
        private float sizex, sizey, offsetx, offsety, A, R, G, B, angle = 0;

        private static int N = 0, shape = 0, gap = 3, mode = 0;
        private static bool doFillShapes = false;
        private static float dimAlpha = 0.15f, sizeFactorX = 1, sizeFactorY = 1;

        private static myScreenGradient grad = null;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_1130()
        {
            if (id != uint.MaxValue)
                generateNew();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time global initialization
        protected override void initGlobal()
        {
            var colorMode = myUtils.randomChance(rand, 2, 3)
                ? myColorPicker.colorMode.SNAPSHOT_OR_IMAGE
                : myColorPicker.colorMode.RANDOM_MODE;

            colorPicker = new myColorPicker(gl_Width, gl_Height, colorMode);
            list = new List<myObject>();

            // Global unmutable constants
            {
                N = 1;
                shape = 0;
            }

            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time local initialization
        private void initLocal()
        {
            doClearBuffer = false;
            doFillShapes = true;

            mode = rand.Next(5);

            dimAlpha = myUtils.randomChance(rand, 1, 2)
                ? myUtils.randFloat(rand) * 0.002f
                : 0;

            renderDelay = rand.Next(3) + 3;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            height = 600;

            string str = $"Obj = {Type}\n\n"                                  +
                            myUtils.strCountOf(list.Count, N)                 +
                            $"mode = {mode}\n"                                +
                            $"renderDelay = {renderDelay}\n"                  +
                            $"dimAlpha: {myUtils.fStr(dimAlpha)}\n"           +
                            $"colorPicker mode: {colorPicker.getModeStr()}\n" +
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
            firstIteration = true;
            firstLine = true;

            x = gl_x0;
            y = gl_y0;

            switch (mode)
            {
                case 0:
                    sizex = 60;
                    sizey = 30;

                    sizeFactorX = 1.20f;
                    sizeFactorY = 1.05f;
                    break;

                // Square
                case 1:
                    sizex = 60;
                    sizey = 60;

                    sizeFactorX = 1.1f;
                    sizeFactorY = 1.1f;
                    break;

                // Random horizontal rectangle
                case 2:
                    sizey = 30 + rand.Next(21);
                    sizex = (int)(sizey * (1.0f + myUtils.randFloat(rand, 0.2f)));

                    sizeFactorX = 1.20f;
                    sizeFactorY = 1.05f;
                    break;

                // Random square
                case 3:
                    sizex = 30 + rand.Next(41);
                    sizey = sizex;

                    sizeFactorX = 1.1f;
                    sizeFactorY = 1.1f;
                    break;

                // Random rectangle
                case 4:
                    sizex = 30 + rand.Next(33);
                    sizey = 30 + rand.Next(33);

                    sizeFactorX = 1.1f;
                    sizeFactorY = 1.1f;
                    break;
            }

            offsetx = 0;
            offsety = 0;

            A = 1;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Show()
        {
            void doDraw(float x, float y)
            {
                x -= sizex;
                y -= sizey;
                float size2x = sizex * 2;
                float size2y = sizey * 2;

                if (colorPicker.isImage())
                    colorPicker.getColorAverage(x, y, (int)size2x, (int)size2y, ref R, ref G, ref B);
                else
                    colorPicker.getColor(x, y, ref R, ref G, ref B);

                myPrimitive._RectangleInst.setInstanceCoords(x, y, size2x, size2y);
                myPrimitive._RectangleInst.setInstanceColor(R, G, B, A);
                myPrimitive._RectangleInst.setInstanceAngle(angle);
            }

            if (firstIteration)
            {
                // Draw central figure
                offsetx = 0;
                firstIteration = false;

                if (firstLine)
                {
                    doDraw(gl_x0, gl_y0);
                }
                else
                {
                    doDraw(gl_x0, y - offsety);
                    doDraw(gl_x0, y + offsety);
                }
            }
            else
            {
                float X = 0, Y = 0;
                offsetx += 2 * sizex + gap;

                if (firstLine)
                {
                    // Draw central line (2 figures at a time)
                    X = x - offsetx;
                    Y = y;
                    doDraw(X, Y);

                    X = x + offsetx;
                    Y = y;
                    doDraw(X, Y);

                    if (X > gl_Width)
                    {
                        offsetx = 0;
                        offsety += sizey;

                        sizex /= sizeFactorX;
                        sizey /= sizeFactorY;

                        offsety += sizey + gap;

                        firstLine = false;
                        firstIteration = true;
                    }
                }
                else
                {
                    // Draw 2 lines (4 figures) at a time
                    X = (int)(x - offsetx);
                    Y = (int)(y - offsety);
                    doDraw(X, Y);

                    X = x + offsetx;
                    Y = y - offsety;
                    doDraw(X, Y);

                    X = x - offsetx;
                    Y = y + offsety;
                    doDraw(X, Y);

                    X = x + offsetx;
                    Y = y + offsety;
                    doDraw(X, Y);

                    if (X > gl_Width)
                    {
                        offsetx = 0;
                        offsety += sizey;

                        sizex /= sizeFactorX;
                        sizey /= sizeFactorY;

                        offsety += sizey + gap;

                        firstIteration = true;

                        if ((X > gl_Width && Y > gl_Height) || sizex <= 0 || sizey <= 0)
                        {
                            glClear(GL_COLOR_BUFFER_BIT);
                            generateNew();
                        }
                    }
                }
            }
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Process(Window window)
        {
            uint cnt = 0;
            initShapes();

            clearScreenSetup(doClearBuffer, 0.1f, true);

            while (!Glfw.WindowShouldClose(window))
            {
                int Count = list.Count;

                processInput(window);

                // Swap fore/back framebuffers, and poll for operating system events.
                Glfw.SwapBuffers(window);
                Glfw.PollEvents();

                // Dim screen
                {
                    if (doClearBuffer)
                    {
                        glClear(GL_COLOR_BUFFER_BIT);
                        grad.Draw();
                    }
                    else
                    {
                        dimScreen(dimAlpha);
                    }
                }

                // Render Frame
                if (cnt == 5)
                {
                    cnt = 0;
                    inst.ResetBuffer();

                    for (int i = 0; i != Count; i++)
                    {
                        var obj = list[i] as myObj_1130;

                        obj.Show();
                        obj.Move();
                    }

                    if (doFillShapes)
                    {
                        // Tell the fragment shader to multiply existing instance opacity by 0.5:
                        inst.SetColorA(-0.5f);
                        inst.Draw(true);
                    }

                    // Tell the fragment shader to do nothing with the existing instance opacity:
                    inst.SetColorA(0);
                    inst.Draw(false);
                }

                if (Count < N)
                {
                    list.Add(new myObj_1130());
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
            base.initShapes(shape, N * 4, 0);

            grad = new myScreenGradient();
            grad.SetRandomColors(rand, 0.2f);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------
    }
};
