using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;
using static System.Net.Mime.MediaTypeNames;


/*
    - Pieces falling off the desktop
*/


namespace my
{
    public class myObj_0070 : myObject
    {
        // Priority
        public static int Priority => 10;
		public static System.Type Type => typeof(myObj_0070);

        private int cnt;
        private float x, y, dx, dy, ddy, da;
        private float size, A, R, G, B, angle = 0, dAngle = 0;

        private static int N = 0, shape = 0, rotationMode = 0, slowMode = 0, opacityMode = 0, colorMode = 0,
                           minSize = 5, maxSize = 25, maxHeight = gl_Height + 100, gridStep = 1, gridOffset = 1;
        private static bool doFillShapes = false, doUseGrid = true, doUseConstSize = true;

        private static myTexRectangle_Renderer offScrRenderer = null;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_0070()
        {
            generateNew();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time global initialization
        protected override void initGlobal()
        {
            colorPicker = new myColorPicker(gl_Width, gl_Height, mode: myColorPicker.colorMode.SNAPSHOT_OR_IMAGE);
            list = new List<myObject>();

            N = rand.Next(999) + 100;
            shape = rand.Next(5);

            rotationMode = rand.Next(4);

            doFillShapes = true;
            doClearBuffer = true;

#if false
            N = 222;
            shape = 0;
            rotationMode = 0;
#endif

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

            colorMode = rand.Next(2);                           // The way the color of a particle is calculated
            opacityMode = rand.Next(3);                         // The way the particles lose their opacity (none/some/every obj)

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            height = 600;

            string str = $"Obj = {Type} -- Falling Pieces\n\n"   	+
                            $"N = {list.Count} of {N}\n"            +
                            $"shape = {shape}\n"                    +
                            $"doUseGrid = {doUseGrid}\n"            +
                            $"doUseConstSize = {doUseConstSize}\n"  +
                            $"gridStep = {gridStep}\n"              +
                            $"gridOffset = {gridOffset}\n"          +
                            $"rotationMode = {rotationMode}\n"      +
                            $"slowMode = {slowMode}\n"              +
                            $"colorMode = {colorMode}\n"            +
                            $"opacityMode = {opacityMode}\n"        +
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
            cnt = 25;

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

            int failCnt = 0;

            //do
            {
                x = rand.Next(gl_Width);
                y = rand.Next(gl_Height);

                if (++failCnt > 50)
                {
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

            switch (colorMode)
            {
                case 0:
                    colorPicker.getColor(x, y, ref R, ref G, ref B);
                    break;

                case 1:
                    colorPicker.getColorAverage(x - 3, y - 3, 6, 6, ref R, ref G, ref B);
                    break;
            }

            // Set the opacity decrease
            switch (opacityMode)
            {
                case 0:
                    da = 0;
                    break;

                case 1:
                    if (myUtils.randomChance(rand, 1, 3))
                    {
                        da = -myUtils.randFloat(rand) * 0.01f;
                    }
                    break;

                case 2:
                    da = -myUtils.randFloat(rand) * 0.01f;
                    break;
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            // Stay in place, while cnt > 0
            if (cnt > 0)
            {
                cnt--;
            }
            else
            {
                x += dx;
                y += dy;
                dy += ddy;
                angle += dAngle;
                A += da;

                // Reduce rotation speed gradually
                if (myUtils.randomChance(rand, 1, 33))
                {
                    dAngle *= 0.9f;
                }

                if (y > maxHeight || A < 0)
                {
                    generateNew();
                }
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Show()
        {
            float size2x = size * 2;
            float X = x - size;
            float Y = y - size;
            float a = A;

            if (cnt > 0)
            {
                a = (A + 0.1f) * (25 - cnt) / 25;
            }

            // Render objects to the off-screen texture, when they appear for the first time:
            if (cnt == 1)
            {
                // Dim the tile on the src image -- leave permament mark on the bgr texture
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

                        case 2:
                            myPrimitive._Ellipse.SetColor(r, g, b, A * 0.25f);
                            myPrimitive._Ellipse.Draw(x - size, gl_Height - y - size, size2x, size2x, true);
                            break;

                        case 3:
                            myPrimitive._Pentagon.SetColor(r, g, b, A * 0.25f);
                            myPrimitive._Pentagon.SetAngle(myUtils.randFloat(rand));
                            myPrimitive._Pentagon.Draw(x, gl_Height - y, size, true);
                            break;

                        case 4:
                            myPrimitive._Hexagon.SetColor(r, g, b, A * 0.25f);
                            myPrimitive._Hexagon.SetAngle(myUtils.randFloat(rand));
                            myPrimitive._Hexagon.Draw(x, gl_Height - y, size, true);
                            break;
                    }
                }
                offScrRenderer.stopRendering();
            }

            switch (shape)
            {
                // Instanced squares
                case 0:
                    myPrimitive._RectangleInst.setInstanceCoords(X, Y, size2x, size2x);
                    myPrimitive._RectangleInst.setInstanceColor(R, G, B, a);
                    myPrimitive._RectangleInst.setInstanceAngle(angle);
                    break;

                // Instanced triangles
                case 1:
                    myPrimitive._TriangleInst.setInstanceCoords(x, y, size2x, angle);
                    myPrimitive._TriangleInst.setInstanceColor(R, G, B, a);
                    break;

                // Instanced circles
                case 2:
                    myPrimitive._EllipseInst.setInstanceCoords(x, y, size2x, angle);
                    myPrimitive._EllipseInst.setInstanceColor(R, G, B, a);
                    break;

                // Instanced pentagons
                case 3:
                    myPrimitive._PentagonInst.setInstanceCoords(x, y, size2x, angle);
                    myPrimitive._PentagonInst.setInstanceColor(R, G, B, a);
                    break;

                // Instanced hexagons
                case 4:
                    myPrimitive._HexagonInst.setInstanceCoords(x, y, size2x, angle);
                    myPrimitive._HexagonInst.setInstanceColor(R, G, B, a);
                    break;
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Process(Window window)
        {
            uint cnt = 0;
            initShapes();


            // Render our main texture to the off-screen texture and show it for the first time
            offScrRenderer.Draw(0, 0, gl_Width, gl_Height);
            Glfw.SwapBuffers(window);
            System.Threading.Thread.Sleep(123);


            // This is the default setting for double-buffered setups
            glDrawBuffer(GL_BACK);
            glClearColor(0, 0, 0, 1);
            inst.setDrawingMode(myInstancedPrimitive.drawMode.OWN_COLOR_CUSTOM_OPACITY);


            while (!Glfw.WindowShouldClose(window))
            {
                cnt++;

                processInput(window);

                // Swap fore/back framebuffers, and poll for operating system events.
                Glfw.SwapBuffers(window);
                Glfw.PollEvents();

                if (doClearBuffer)
                {
                    glClear(GL_COLOR_BUFFER_BIT);
                }

                // Render Frame
                {
                    inst.ResetBuffer();

                    // Render our off-screen texture first
                    offScrRenderer.Draw(0, 0, gl_Width, gl_Height);

                    // Render particles;
                    // Render permanent marks on the bgr texture
                    for (int i = 0; i != list.Count; i++)
                    {
                        var obj = list[i] as myObj_0070;

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
                    list.Add(new myObj_0070());
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
            myPrimitive.init_Ellipse();
            myPrimitive.init_Pentagon();
            myPrimitive.init_Hexagon();

            base.initShapes(shape, N, 0);

            offScrRenderer = new myTexRectangle_Renderer(colorPicker.getImg());

            // Set rotation mode for instanced particles
            switch (shape)
            {
                case 0:
                    myPrimitive._RectangleInst.setRotationMode(rotationMode);
                    break;

                case 1:
                    myPrimitive._TriangleInst.setRotationMode(rotationMode);
                    break;
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------
    }
};
