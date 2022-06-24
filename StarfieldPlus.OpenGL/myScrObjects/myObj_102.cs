using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;


/*
    - Get a rectangle at the original screen
    - Calculate average color in this rectangle
    - Put a shape filled with this average color at the same place
*/


namespace my
{
    public class myObj_102 : myObject
    {
        // ---------------------------------------------------------------------------------------------------------------

        private static bool doClearBuffer = false, doCleanOnce = false, doUseGrid = false, doUseRandSize = false;
        private static int angleMode = 0, gridSize = 0, baseSize = 0, shapeMode = 0, colorMode = 0;

        private int x, y, size, Cnt;
        private float R, G, B, angle;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_102()
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
            doUseGrid     = myUtils.randomBool(rand);
            doUseRandSize = myUtils.randomBool(rand);
            doClearBuffer = false;

            shapeMode = rand.Next(1);
            colorMode = rand.Next(2);
            baseSize  = rand.Next(50) + 10;

            angleMode = rand.Next(13);

            renderDelay = 0;
            gridSize = baseSize * 2 + 2 * rand.Next(5) + 1;

#if true
            angleMode = 2;
#endif
            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            string str = $"Obj = myObj_102\n\n";

            return str;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void generateNew()
        {
            size = baseSize;
            angle = 0;

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void setNextMode()
        {
            init();
            doCleanOnce = true;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            x = rand.Next(gl_Width);
            y = rand.Next(gl_Height);

            getColor();

            if (doUseGrid)
            {
                x -= x % gridSize;
                y -= y % gridSize;
            }

            if (doUseRandSize)
            {
                size = rand.Next(baseSize) + 3;
            }

            switch (angleMode)
            {
                case 0: angle += 0.001f; break;
                case 1: angle = (float)rand.NextDouble(); break;
            }
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Show()
        {
            switch (shapeMode)
            {
                case 0:
                    myPrimitive._Rectangle.SetColor(R, G, B, 0.25f);
                    myPrimitive._Rectangle.SetAngle(angle);
                    myPrimitive._Rectangle.Draw(x - size, y - size, 2 * size, 2 * size, true);
                    break;
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Process(Window window)
        {
            uint cnt = 0;

            initShapes();

            // Set background to random color
            glDrawBuffer(GL_FRONT_AND_BACK);
            glClearColor((float)rand.NextDouble(), (float)rand.NextDouble(), (float)rand.NextDouble(), 1);
            glClear(GL_COLOR_BUFFER_BIT);

            while (!Glfw.WindowShouldClose(window))
            {
                processInput(window);

                // Swap fore/back framebuffers, and poll for operating system events.
                Glfw.SwapBuffers(window);
                Glfw.PollEvents();

                if (doClearBuffer || doCleanOnce)
                {
                    glClear(GL_COLOR_BUFFER_BIT);
                    doCleanOnce = false;
                }

                // Render Frame
                {
                    Show();
                    Move();
                }

                //System.Threading.Thread.Sleep(renderDelay);
                cnt++;
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void initShapes()
        {
            myPrimitive.init_Rectangle();

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void getColor()
        {
            int cnt = 0;
            float r = 0, g = 0, b = 0;

            switch (colorMode)
            {
                case 0:
                    colorPicker.getColor(x, y, ref R, ref G, ref B);
                    break;

                case 1:

                    R = 0; G = 0; B = 0;

                    for (int i = x - size; i < x + 2 * size; i++)
                    {
                        for (int j = y - size; j < y + 2 * size; j++)
                        {
                            if (i > -1 && j > -1 && i < gl_Width && j < gl_Height)
                            {
                                colorPicker.getColor(i, j, ref r, ref g, ref b);

                                R += r;
                                G += g;
                                B += b;

                                cnt++;
                            }
                        }
                    }

                    R /= cnt;
                    G /= cnt;
                    B /= cnt;
                    break;
            }
        }

        // ---------------------------------------------------------------------------------------------------------------
    }
};
