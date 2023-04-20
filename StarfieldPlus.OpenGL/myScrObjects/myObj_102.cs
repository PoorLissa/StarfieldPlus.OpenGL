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
        // Priority
        public static int Priority => 10;

        private int x, y, size;
        private float R, G, B, angle;

        private static bool doClearOnce = false, doUseGrid = false, doUseRandSize = false;
        private static int N = 0, nObj = 0, angleMode = 0, gridSize = 0, baseSize = 0, shapeMode = 0, colorMode = 0,
                           borderMode = 0, borderOffset = 0, opacityMode = 0, randSizeFactor = 1, colorStep = 1;
        private static float A = 1, lineWidth = 1;

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

                // Reserve larger N, because in borderMode 7-8 we'll need more particles
                N = 100 + rand.Next(100);
            }

            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time initialization
        private void initLocal()
        {
            if (myUtils.randomChance(rand, 1, 11))
            {
                nObj = rand.Next(35) + 10;
            }
            else
            {
                nObj = rand.Next(5) + 1;
            }

            doUseGrid = myUtils.randomBool(rand);
            doUseRandSize = myUtils.randomBool(rand);

            shapeMode      = rand.Next(6);
            colorMode      = rand.Next(2);                      // Color at a point vs area average color
            borderMode     = rand.Next(9);                      // Color of the border. Also, in modes 7-8 only the border is drawn
            borderOffset   = rand.Next(13) - 6;                 // Offset to the size of the border (-6 .. +6)
            angleMode      = rand.Next(13);
            randSizeFactor = rand.Next(3) + 1;
            colorStep      = rand.Next(5) + 1;
            opacityMode    = myUtils.randomChance(rand, 1, 7)   // If > 0, opacity/size will depend on current color
                                ? rand.Next(8) : 0;

            lineWidth = myUtils.randFloat(rand) * 2;

            renderDelay = (nObj - 1) * 2;

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

            // Shape opacity
            if (myUtils.randomChance(rand, 1, 11))
            {
                A = myUtils.randFloat(rand, 0.1f) * 0.25f;
            }
            else
            {
                A = 0.25f;
            }

            // Adjust settings for borderModes 7-8
            if (borderMode > 6)
            {
                nObj = N;

                // We only draw the outline. Want to be fast here
                renderDelay = rand.Next(10) + 1;
                lineWidth = lineWidth < 0.5 ? 1 : lineWidth;

                // Getting average color value can be expensive on larger N;
                // Select easier mode:
                colorMode = 0;
            }

#if false
            angleMode = 10;
            shapeMode = 0;
            doUseGrid = false;
            doUseRandSize = false;
#endif

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            width = 500;
            height = 600;

            string str = $"Obj = myObj_102\n\n"                         +
                            $"N = {list.Count} of {N}; nObj = {nObj}\n" +
                            $"doClearBuffer = {doClearBuffer}\n"        +
                            $"doUseGrid = {doUseGrid}\n"                +
                            $"doUseRandSize = {doUseRandSize}\n"        +
                            $"shapeMode = {shapeMode}\n"                +
                            $"opacityMode = {opacityMode}\n"            +
                            $"colorMode = {colorMode}\n"                +
                            $"colorStep = {colorStep}\n"                +
                            $"angleMode = {angleMode}\n"                +
                            $"borderMode = {borderMode}\n"              +
                            $"borderOffset = {borderOffset}\n"          +
                            $"baseSize = {baseSize}\n"                  +
                            $"gridSize = {gridSize}\n"                  +
                            $"opacity = {A.ToString("0.000")}\n"        +
                            $"renderDelay = {renderDelay}\n"            +
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

            dimScreen(0.1f);

            doClearOnce = true;

            System.Threading.Thread.Sleep(123);
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            if (doUseGrid)
            {
                x = rand.Next(gl_Width  + 100);
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
                case 0:
                    angle += 0.001f;
                    break;

                case 1:
                    angle = (float)rand.NextDouble();
                    break;

                case 2:
                    if(myUtils.randomChance(rand, 1, 333))
                        angle = (float)rand.NextDouble();
                    break;
            }

            // In this mode, opacity and size depend on the color of current particle
            if (opacityMode > 0)
            {
                float avg = (R + G + B) * 0.333f + 0.0001f;

                switch (opacityMode)
                {
                    case 1:
                        {
                            size = (int)(100.0f * avg);             // size = [0 .. 100], the lighter, the larger
                            /* A = A;*/;                            // A is standard
                        }
                        break;

                    case 2:
                        {
                            size = (int)(100.0f - 100.0f * avg);    // size = [0 .. 100], the darker, the larger
                            /* A = A;*/;                            // A is standard
                        }
                        break;

                    case 3:
                        {
                            size = (int)(100.0f * avg);             // size = [0 .. 100], the lighter, the larger
                            A = avg;                                //   A  = [0 .. 1.0], the lighter, the higher
                        }
                        break;

                    case 4:
                        {
                            size = (int)(100.0f * avg);             // size = [0 .. 100], the lighter, the larger
                            A = 1.0f - avg;                         //   A  = [0 .. 1.0], the darker, the higher
                        }
                        break;

                    case 5:
                        {
                            size = (int)(100.0f - 100.0f * avg);    // size = [0 .. 100], the darker, the larger
                            A = avg;                                //   A  = [0 .. 1.0], the lighter, the higher
                        }
                        break;

                    case 6:
                        {
                            size = (int)(100.0f - 100.0f * avg);    // size = [0 .. 100], the darker, the larger
                            A = 1.0f - avg;                         //   A  = [0 .. 1.0], the darker, the higher
                        }
                        break;
                }
            }
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Show()
        {
            int oldShapeMode = -1;

randShape:

            if (borderMode < 7)
            {
                switch (shapeMode)
                {
                    case 0:
                        myPrimitive._Rectangle.SetColor(R, G, B, A);
                        myPrimitive._Rectangle.SetAngle(angle);
                        myPrimitive._Rectangle.Draw(x - size, y - size, 2 * size, 2 * size, true);
                        break;

                    case 1:
                        myPrimitive._Ellipse.SetColor(R, G, B, A);
                        myPrimitive._Ellipse.Draw(x - size, y - size, 2 * size, 2 * size, true);
                        break;

                    case 2:
                        myPrimitive._Triangle.SetColor(R, G, B, A);
                        myPrimitive._Triangle.SetAngle(angle);
                        myPrimitive._Triangle.Draw(x, y - size, x - 5 * size / 6, y + size / 2, x + 5 * size / 6, y + size / 2, true);
                        break;

                    case 3:
                        myPrimitive._Hexagon.SetColor(R, G, B, A);
                        myPrimitive._Hexagon.SetAngle(angle);
                        myPrimitive._Hexagon.Draw(x, y, size, true);
                        break;

                    case 4:
                        myPrimitive._Pentagon.SetColor(R, G, B, A);
                        myPrimitive._Pentagon.SetAngle(angle);
                        myPrimitive._Pentagon.Draw(x, y, size, true);
                        break;

                    case 5:
                        oldShapeMode = shapeMode;
                        shapeMode = rand.Next(5);
                        goto randShape;
                        break;
                }
            }

            drawBorder();

            if (oldShapeMode != -1)
            {
                shapeMode = oldShapeMode;
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Process(Window window)
        {
            uint cnt = 0;

            initShapes();

            // Set background to random color
            {
                switch (rand.Next(3))
                {
                    // Random color
                    case 0:
                        myObject.bgrR = myUtils.randFloat(rand);
                        myObject.bgrG = myUtils.randFloat(rand);
                        myObject.bgrB = myUtils.randFloat(rand);
                        break;

                    // Random Light
                    case 1:
                        myObject.bgrR = 1.0f - myUtils.randFloat(rand) * 0.1f;
                        myObject.bgrG = 1.0f - myUtils.randFloat(rand) * 0.1f;
                        myObject.bgrB = 1.0f - myUtils.randFloat(rand) * 0.1f;
                        break;

                    // Random Dark
                    case 2:
                        myObject.bgrR = myUtils.randFloat(rand) * 0.1f;
                        myObject.bgrG = myUtils.randFloat(rand) * 0.1f;
                        myObject.bgrB = myUtils.randFloat(rand) * 0.1f;
                        break;
                }

                {
                    dimScreenRGB_Set(myObject.bgrR, myObject.bgrG, myObject.bgrB);
                    glClearColor(myObject.bgrR, myObject.bgrG, myObject.bgrB, 1);

                    glDrawBuffer(GL_FRONT_AND_BACK);
                    glClear(GL_COLOR_BUFFER_BIT);
                }
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

                    int Count = list.Count < nObj ? list.Count : nObj;

                    for (int i = 0; i != Count; i++)
                    {
                        var obj = list[i] as myObj_102;

                        obj.Move();
                        obj.Show();
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

        // Draw a border around the shape
        private void drawBorder()
        {
            if (borderMode != 0)
            {
                float r = 0, g = 0, b = 0, a = 0.5f;

                switch (borderMode)
                {
                    // The same color as the shape
                    case 1:
                        r = R; g = G; b = B;
                        break;

                    // The same color as the shape, but slightly offset randomly
                    case 2:
                        r = R + myUtils.randomSign(rand) * myUtils.randFloat(rand) * 0.1f;
                        g = G + myUtils.randomSign(rand) * myUtils.randFloat(rand) * 0.1f;
                        b = B + myUtils.randomSign(rand) * myUtils.randFloat(rand) * 0.1f;
                        break;

                    // Black
                    case 3:
                        r = 0; g = 0; b = 0;
                        break;

                    // Dark Random
                    case 4:
                        r = myUtils.randFloat(rand) * 0.1f;
                        g = myUtils.randFloat(rand) * 0.1f;
                        b = myUtils.randFloat(rand) * 0.1f;
                        break;

                    // White
                    case 5:
                        r = 1; g = 1; b = 1;
                        break;

                    // Light Random
                    case 6:
                        r = 1.0f - myUtils.randFloat(rand) * 0.1f;
                        g = 1.0f - myUtils.randFloat(rand) * 0.1f;
                        b = 1.0f - myUtils.randFloat(rand) * 0.1f;
                        break;

                    // The same color as the shape --- Only the shape's outline is drawn
                    case 7:
                        r = R; g = G; b = B;
                        break;

                    // The same color as the shape, but slightly offset randomly --- Only the shape's outline is drawn
                    case 8:
                        r = R + myUtils.randomSign(rand) * myUtils.randFloat(rand) * 0.1f;
                        g = G + myUtils.randomSign(rand) * myUtils.randFloat(rand) * 0.1f;
                        b = B + myUtils.randomSign(rand) * myUtils.randFloat(rand) * 0.1f;
                        break;
                }

                int bSize = size - borderOffset;
                int bSize2x = bSize * 2;

                // In borderMode 7-8, draw the outline using shape's original opacity
                if (borderMode > 6)
                    a = myUtils.randFloat(rand, 0.1f);

                switch (shapeMode)
                {
                    case 0:
                        myPrimitive._Rectangle.SetColor(r, g, b, a);
                        myPrimitive._Rectangle.Draw(x - bSize, y - bSize, bSize2x, bSize2x, false);
                        break;

                    case 1:
                        myPrimitive._Ellipse.SetColor(r, g, b, a);
                        myPrimitive._Ellipse.setLineThickness(1);
                        myPrimitive._Ellipse.Draw(x - bSize, y - bSize, bSize2x, bSize2x, false);
                        break;

                    case 2:
                        myPrimitive._Triangle.SetColor(r, g, b, a);
                        myPrimitive._Triangle.Draw(x, y - bSize, x - 5 * bSize / 6, y + bSize / 2, x + 5 * bSize / 6, y + bSize / 2, false);
                        break;

                    case 3:
                        myPrimitive._Hexagon.SetColor(r, g, b, a);
                        myPrimitive._Hexagon.Draw(x, y, bSize, false);
                        break;

                    case 4:
                        myPrimitive._Pentagon.SetColor(r, g, b, a);
                        myPrimitive._Pentagon.Draw(x, y, bSize, false);
                        break;
                }
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------
    }
};
