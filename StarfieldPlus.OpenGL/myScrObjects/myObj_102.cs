using GLFW;
using static OpenGL.GL;
using System.Collections.Generic;


/*
    - Get a rectangle at the original screen
    - Get color at a point / Calculate average color in this rectangle
    - Put a shape filled with this average color at the same place
*/


namespace my
{
    public class myObj_102 : myObject
    {
        // ---------------------------------------------------------------------------------------------------------------

        private static bool doClearOnce = false, doUseGrid = false, doUseRandSize = false;
        private static int N = 0, angleMode = 0, gridSize = 0, baseSize = 0, shapeMode = 0, colorMode = 0, borderMode = 0, randSizeFactor = 1, colorStep = 1;
        private static float lineWidth = 1;

        private int x, y, size;
        private float R, G, B, angle;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_102()
        {
            generateNew();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time global initialization
        protected override void initGlobal()
        {
            colorPicker = new myColorPicker(gl_Width, gl_Height, mode: myColorPicker.colorMode.SNAPSHOT_OR_IMAGE);
            list = new List<myObject>();

            {
                doClearBuffer = myUtils.randomChance(rand, 1, 7);

                if (myUtils.randomChance(rand, 1, 11))
                {
                    N = rand.Next(35) + 10;
                }
                else
                {
                    N = rand.Next(5) + 1;
                }
            }

            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------


        // One-time initialization
        private void initLocal()
        {
            doUseGrid     = myUtils.randomBool(rand);
            doUseRandSize = myUtils.randomBool(rand);

            shapeMode  = rand.Next(6);
            colorMode  = rand.Next(2);
            borderMode = rand.Next(6);
            angleMode  = rand.Next(13);
            randSizeFactor = rand.Next(3) + 1;
            colorStep = rand.Next(5) + 1;

            lineWidth = myUtils.randFloat(rand) * 2;

            renderDelay = (N - 1) * 2;

            switch (rand.Next(6))
            {
                case 0:
                    baseSize = rand.Next(50) + 10;
                    break;

                case 1: case 2: case 3:
                    baseSize = rand.Next(60) + 20;
                    break;

                case 4: case 5:
                    baseSize = rand.Next(70) + 30;
                    break;
            }

            if (baseSize > 20 && myUtils.randomChance(rand, 1, 3))
            {
                // Sometimes make grid step less than baseSize
                gridSize = baseSize - rand.Next(baseSize/2);
            }
            else
            {
                gridSize = baseSize * 2 + 2 * rand.Next(5) + 1;
            }

#if false
            angleMode = 2;
            shapeMode = 0;
            doUseGrid = true;
            doUseRandSize = true;
#endif

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            width = 500;
            height = 400;

            string str = $"Obj = myObj_102\n\n"                  +
                            $"N = {list.Count} of {N}\n"         +
                            $"doClearBuffer = {doClearBuffer}\n" +
                            $"doUseGrid = {doUseGrid}\n"         +
                            $"doUseRandSize = {doUseRandSize}\n" +
                            $"shapeMode = {shapeMode}\n"         +
                            $"colorMode = {colorMode}\n"         +
                            $"angleMode = {angleMode}\n"         +
                            $"borderMode = {borderMode}\n"       +
                            $"baseSize = {baseSize}\n"           +
                            $"gridSize = {gridSize}\n"           +
                            $"renderDelay = {renderDelay}\n"     +
                            $"file: {colorPicker.GetFileName()}"
                ;
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
            initLocal();

            //dimScreen(0.1f);

            doClearOnce = true;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            if (doUseGrid)
            {
                x = rand.Next(gl_Width + 100);
                y = rand.Next(gl_Height + 100);
            }
            else
            {
                x = rand.Next(gl_Width);
                y = rand.Next(gl_Height);
            }

            if (doUseGrid)
            {
                x -= x % gridSize;
                y -= y % gridSize;
            }

            if (doUseRandSize)
            {
                size = rand.Next(baseSize * randSizeFactor) + 3;
            }

            switch (colorMode)
            {
                case 0:
                    colorPicker.getColor(x, y, ref R, ref G, ref B);
                    break;

                case 1:
                    colorPicker.getColorAverage(x - size, y - size, 2 * size, 2 * size, ref R, ref G, ref B, colorStep);
                    break;
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
            int oldShapeMode = -1;

randShape:

            switch (shapeMode)
            {
                case 0:
                    myPrimitive._Rectangle.SetColor(R, G, B, 0.25f);
                    myPrimitive._Rectangle.SetAngle(angle);
                    myPrimitive._Rectangle.Draw(x - size, y - size, 2 * size, 2 * size, true);
                    break;

                case 1:
                    myPrimitive._Ellipse.SetColor(R, G, B, 0.25f);
                    myPrimitive._Ellipse.Draw(x - size, y - size, 2 * size, 2 * size, true);
                    break;

                case 2:
                    myPrimitive._Triangle.SetColor(R, G, B, 0.25f);
                    myPrimitive._Triangle.SetAngle(angle);
                    myPrimitive._Triangle.Draw(x, y - size, x - 5 * size / 6, y + size / 2, x + 5 * size / 6, y + size / 2, true);
                    break;

                case 3:
                    myPrimitive._Hexagon.SetColor(R, G, B, 0.25f);
                    myPrimitive._Hexagon.SetAngle(angle);
                    myPrimitive._Hexagon.Draw(x, y, size, true);
                    break;

                case 4:
                    myPrimitive._Pentagon.SetColor(R, G, B, 0.25f);
                    myPrimitive._Pentagon.SetAngle(angle);
                    myPrimitive._Pentagon.Draw(x, y, size, true);
                    break;

                case 5:
                    oldShapeMode = shapeMode;
                    shapeMode = rand.Next(5);
                    goto randShape;
                    break;
            }

            drawBorder();

            if (oldShapeMode != -1)
            {
                shapeMode = oldShapeMode;
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void drawBorder()
        {
            if (borderMode != 0)
            {
                float r = 0, g = 0, b = 0;

                switch (borderMode)
                {
                    case 1:
                        r = R;
                        g = G;
                        b = B;
                        break;

                    case 2:
                        r = 0;
                        g = 0;
                        b = 0;
                        break;

                    case 3:
                        r = myUtils.randFloat(rand) * 0.1f;
                        g = myUtils.randFloat(rand) * 0.1f;
                        b = myUtils.randFloat(rand) * 0.1f;
                        break;

                    case 4:
                        r = 1;
                        g = 1;
                        b = 1;
                        break;

                    case 5:
                        r = 1.0f - myUtils.randFloat(rand) * 0.1f;
                        g = 1.0f - myUtils.randFloat(rand) * 0.1f;
                        b = 1.0f - myUtils.randFloat(rand) * 0.1f;
                        break;
                }

                switch (shapeMode)
                {
                    case 0:
                        myPrimitive._Rectangle.SetColor(r, g, b, 0.5f);
                        myPrimitive._Rectangle.Draw(x - size, y - size, 2 * size, 2 * size, false);
                        break;

                    case 1:
                        myPrimitive._Ellipse.SetColor(r, g, b, 0.5f);
                        myPrimitive._Ellipse.setLineThickness(1);
                        myPrimitive._Ellipse.Draw(x - size, y - size, 2 * size, 2 * size, false);
                        break;

                    case 2:
                        myPrimitive._Triangle.SetColor(r, g, b, 0.5f);
                        myPrimitive._Triangle.Draw(x, y - size, x - 5 * size / 6, y + size / 2, x + 5 * size / 6, y + size / 2, false);
                        break;

                    case 3:
                        myPrimitive._Hexagon.SetColor(r, g, b, 0.5f);
                        myPrimitive._Hexagon.Draw(x, y, size, false);
                        break;

                    case 4:
                        myPrimitive._Pentagon.SetColor(r, g, b, 0.5f);
                        myPrimitive._Pentagon.Draw(x, y, size, false);
                        break;
                }
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Process(Window window)
        {
            uint cnt = 0;

            initShapes();

            myObject.bgrR = myUtils.randFloat(rand);
            myObject.bgrG = myUtils.randFloat(rand);
            myObject.bgrB = myUtils.randFloat(rand);

            // Set background to random color
            {
                dimScreenRGB_Set(myObject.bgrR, myObject.bgrG, myObject.bgrB);
                glClearColor(myObject.bgrR, myObject.bgrG, myObject.bgrB, 1);

                glDrawBuffer(GL_FRONT_AND_BACK);
                glClear(GL_COLOR_BUFFER_BIT);
            }

            while (!Glfw.WindowShouldClose(window))
            {
                processInput(window);

                // Swap fore/back framebuffers, and poll for operating system events.
                Glfw.SwapBuffers(window);
                Glfw.PollEvents();

                if (doClearOnce)
                {
                    glClear(GL_COLOR_BUFFER_BIT);
                    doClearOnce = false;
                }

                if (doClearBuffer)
                {
                    dimScreen(0.005f);
                }

                // Render Frame
                {
                    glLineWidth(lineWidth);

                    for (int i = 0; i < list.Count; i++)
                    {
                        var obj = list[i] as myObj_102;

                        obj.Show();
                        obj.Move();
                    }
                }

                if (list.Count < N)
                {
                    list.Add(new myObj_102());
                }

                System.Threading.Thread.Sleep(renderDelay);
                cnt++;
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void initShapes()
        {
            myPrimitive.init_ScrDimmer();

            myPrimitive.init_Rectangle();
            myPrimitive.init_Ellipse();
            myPrimitive.init_Hexagon();
            myPrimitive.init_Triangle();
            myPrimitive.init_Pentagon();

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------
    }
};
