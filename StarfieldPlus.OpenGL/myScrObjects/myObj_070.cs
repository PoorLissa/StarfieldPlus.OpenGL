using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;


/*
    - Pieces drop off the desktop and fall down

    todo:
        - look for target color across all the cell, not only in a single pixel;
*/


namespace my
{
    public class myObj_070 : myObject
    {
        private float x, y, dx, dy, ddy;
        private float size, A, R, G, B, angle = 0, dAngle = 0;
        private bool alive, isFirstStep;

        private static int N = 0, shape = 0, rotationMode = 0, slowMode = 0,
                           minSize = 5, maxSize = 25, maxHeight = gl_Height + 100, gridStep = 1, gridOffset = 1;
        private static bool doFillShapes = false, doUseGrid = true, doUseConstSize = true;
        private static float dimAlpha = 0.5f;

        static myTexRectangle tex = null;
        static myTexRectangle_Renderer offScrRenderer = null;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_070()
        {
            alive = true;
            generateNew();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time global initialization
        protected override void initGlobal()
        {
            //myColorPicker.setFileName("C:\\_maxx\\pix\\_renamer.old.visuals.png");

            colorPicker = new myColorPicker(gl_Width, gl_Height, mode: myColorPicker.colorMode.SNAPSHOT_OR_IMAGE);
            list = new List<myObject>();

            N = rand.Next(999) + 100;
            shape = rand.Next(2);

            rotationMode = rand.Next(4);

            doFillShapes = true;
            doClearBuffer = true;

            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time local initialization
        private void initLocal()
        {
            doUseGrid      = myUtils.randomBool(rand);          // Align particles to grid
            doUseConstSize = myUtils.randomBool(rand);          // Particle size: const vs random

            gridStep   = rand.Next(25) + minSize;
            gridOffset = rand.Next(05) + 1;

            slowMode = rand.Next(10);                           // ddy reduce factor
            slowMode = slowMode > 5 ? 0 : slowMode;

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            height = 600;

            string str = $"Obj = myObj_070 -- Falling Pieces\n\n"   +
                            $"N = {list.Count} of {N}\n"            +
                            $"shape = {shape}\n"                    +
                            $"doUseGrid = {doUseGrid}\n"            +
                            $"doUseConstSize = {doUseConstSize}\n"  +
                            $"gridStep = {gridStep}\n"              +
                            $"gridOffset = {gridOffset}\n"          +
                            $"rotationMode = {rotationMode}\n"      +
                            $"slowMode = {slowMode}\n"              +
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
            size = doUseConstSize
                    ? gridStep
                    : rand.Next(maxSize) + minSize;

            dx = myUtils.randomSign(rand) * myUtils.randFloat(rand);
            dy = 0;
            ddy = 0.005f + size / 20.0f;

            switch (slowMode)
            {
                case 1: ddy /= 10; break;
                case 2: ddy /= 20; break;
                case 3: ddy /= 30; break;
                case 4: ddy /= 40; break;
                case 5: ddy /= 50; break;
            }

            isFirstStep = true;

            int failCnt = 0;

            //do
            {
                x = rand.Next(gl_Width);
                y = rand.Next(gl_Height);

                if (++failCnt > 50)
                {
                    alive = false;
                    maxSize -= (maxSize == 5) ? 0 : 1;
                    return;
                }
            }
            //while (R == 0 && G == 0 && B == 0);

            angle = 0;
            dAngle = myUtils.randomSign(rand) * myUtils.randFloat(rand, 0.25f) * 0.005f;

            // For rotation modes which are > 0, dAngle should be somewhat greater
            if (rotationMode > 0)
            {
                dAngle *= rand.Next(7) + 5;
            }

            if (doUseGrid)
            {
                size = doUseConstSize
                        ? gridStep
                        : rand.Next(gridStep - 3) + 3;

                x -= x % (gridStep * 2 + gridOffset);
                y -= y % (gridStep * 2 + gridOffset);
            }

            A = myUtils.randFloat(rand, 0.1f) * 0.5f;
            colorPicker.getColor(x, y, ref R, ref G, ref B);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            x += dx;
            y += dy;
            dy += ddy;
            angle += dAngle;

            // Reduce rotating speed gradually
            if (myUtils.randomChance(rand, 1, 33))
            {
                dAngle *= 0.9f;
            }

            if (y > maxHeight)
            {
                generateNew();
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Show()
        {
            float size2x = size * 2;
            float X = x - size;
            float Y = y - size;

            if (isFirstStep)
            {
                isFirstStep = false;

                // Dim the tile on the src image
                offScrRenderer.startRendering();
                {
                    float r = myUtils.randFloat(rand) * 0.1f;
                    float g = myUtils.randFloat(rand) * 0.1f;
                    float b = myUtils.randFloat(rand) * 0.1f;

                    switch (shape)
                    {
                        case 0:
                            myPrimitive._Rectangle.SetColor(r, g, b, A * 0.25f);
                            myPrimitive._Rectangle.Draw(X + 1, gl_Height - Y - size2x + 2, size2x - 3, size2x - 3, true);
                            myPrimitive._Rectangle.Draw(X + 1, gl_Height - Y - size2x + 2, size2x - 3, size2x - 3, false);
                            break;

                        case 1:
                            myPrimitive._Triangle.SetColor(r, g, b, A * 0.25f);
                            myPrimitive._Triangle.SetAngle(myUtils.randFloat(rand));
                            myPrimitive._Triangle.Draw(x, y - size2x, x - 5 * size2x / 6, y + size2x / 2, x + 5 * size2x / 6, y + size2x / 2, true);
                            break;
                    }
                }
                offScrRenderer.stopRendering();
            }

            switch (shape)
            {
                // Instanced squares
                case 0:
                    var rectInst = inst as myRectangleInst;

                    rectInst.setInstanceCoords(X, Y, size2x, size2x);
                    rectInst.setInstanceColor(R, G, B, A);
                    rectInst.setInstanceAngle(angle);
                    break;

                // Instanced triangles
                case 1:
                    var triangleInst = inst as myTriangleInst;

                    triangleInst.setInstanceCoords(x, y, size2x, angle);
                    triangleInst.setInstanceColor(R, G, B, A);
                    break;

                // Instanced circles
                case 2:
                    var ellipseInst = inst as myEllipseInst;

                    ellipseInst.setInstanceCoords(x, y, size2x, angle);
                    ellipseInst.setInstanceColor(R, G, B, A);
                    break;

                // Instanced pentagons
                case 3:
                    var pentagonInst = inst as myPentagonInst;

                    pentagonInst.setInstanceCoords(x, y, size2x, angle);
                    pentagonInst.setInstanceColor(R, G, B, A);
                    break;

                // Instanced hexagons
                case 4:
                    var hexagonInst = inst as myHexagonInst;

                    hexagonInst.setInstanceCoords(x, y, size2x, angle);
                    hexagonInst.setInstanceColor(R, G, B, A);
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

            if (doClearBuffer)
            {
                glDrawBuffer(GL_FRONT_AND_BACK | GL_DEPTH_BUFFER_BIT);
                glClearColor(0, 0, 0, 1);
            }
            else
            {
                glDrawBuffer(GL_FRONT_AND_BACK);
            }

            glDrawBuffer(GL_FRONT_AND_BACK);

            // Render our main texture to the off-screen texture and show it for the first time
            {
                offScrRenderer.startRendering();
                {
                    glClearColor(0, 0, 0, 0);
                    glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT);
                    //glClear(GL_DEPTH_BUFFER_BIT);

                    tex.setOpacity(1);
                    tex.Draw(0, 0, gl_Width, gl_Height, 0, 0, gl_Width, -gl_Height);

                    //myPrimitive._Rectangle.SetColor(0, 0, 0, 0.95f);
                    //myPrimitive._Rectangle.Draw(0, 0, gl_Width, gl_Height, true);
                }
                offScrRenderer.stopRendering();

                offScrRenderer.Draw(0, 0, gl_Width, gl_Height);
                Glfw.SwapBuffers(window);
                System.Threading.Thread.Sleep(111);
            }

            inst.setDrawingMode(myInstancedPrimitive.drawMode.OWN_COLOR_CUSTOM_OPACITY);

            while (!Glfw.WindowShouldClose(window))
            {
                cnt++;

                processInput(window);

                // Swap fore/back framebuffers, and poll for operating system events.
                Glfw.SwapBuffers(window);
                Glfw.PollEvents();

                glClearColor(0, 0, 0, 1);

                // Render Frame
                {
                    inst.ResetBuffer();

                    // Draw our off-screen texture
                    tex.UpdateVertices__WorkaroundTmp();
                    offScrRenderer.Draw(0, 0, gl_Width, gl_Height);

                    for (int i = 0; i != list.Count; i++)
                    {
                        var obj = list[i] as myObj_070;

                        obj.Show();
                        obj.Move();
                    }

                    if (doFillShapes)
                    {
                        // Set fill opacity to A x 0.5
                        inst.SetColorA(-0.5f);
                        inst.Draw(true);
                    }

                    // Set border opacity to A x 2
                    inst.SetColorA(-2.0f);
                    inst.Draw(false);
                }

                if (list.Count < N)
                {
                    list.Add(new myObj_070());
                }

                System.Threading.Thread.Sleep(renderDelay);
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void initShapes()
        {
            myPrimitive.init_Rectangle();
            myPrimitive.init_Triangle();

            base.initShapes(shape, N, 0);

            offScrRenderer = new myTexRectangle_Renderer();

            tex = new myTexRectangle(colorPicker.getImg());

            // Set rotation mode for instanced particles
            switch (shape)
            {
                case 0:
                    (inst as myRectangleInst).setRotationMode(rotationMode);
                    break;

                case 1:
                    (inst as myTriangleInst).setRotationMode(rotationMode);
                    break;
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------
    }
};
