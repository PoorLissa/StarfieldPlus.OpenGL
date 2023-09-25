using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;


/*
    - Pieces falling off the desktop, ver2
    - Mostly the same as myObj_070, but uses original texture for pieces that fall off
*/


namespace my
{
    public class myObj_071 : myObject
    {
        // Priority
        public static int Priority => 10;
		public static System.Type Type => typeof(myObj_071);

        private int cnt, x0, y0;
        private float x, y, dx, dy, ddy, da;
        private float size, A, R, G, B, angle = 0, dAngle = 0;

        private static int N = 0, slowMode = 0, opacityMode = 0, colorMode = 0,
                           minSize = 5, maxSize = 25, maxHeight = gl_Height + 100, gridStep = 1, gridOffset = 1;
        private static bool doUseGrid = true, doUseConstSize = true;

        static myTexRectangle tex = null;
        static myTexRectangle_Renderer offScrRenderer = null;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_071()
        {
            generateNew();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time global initialization
        protected override void initGlobal()
        {
#if DEBUG
            //myColorPicker.setFileName("C:\\_maxx\\pix\\_renamer.old.visuals.png");
#endif

            colorPicker = new myColorPicker(gl_Width, gl_Height, mode: myColorPicker.colorMode.SNAPSHOT_OR_IMAGE);
            list = new List<myObject>();

            N = rand.Next(999) + 100;

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

            colorMode = rand.Next(2);                           // The way the color of a particle is calculated
            opacityMode = rand.Next(3);                         // The way the particles lose their opacity (none/some/every obj)

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            height = 600;

            string str = $"Obj = myObj_071 -- Falling Pieces, ver2\n\n" +
                            $"N = {list.Count} of {N}\n"                +
                            $"doUseGrid = {doUseGrid}\n"                +
                            $"doUseConstSize = {doUseConstSize}\n"      +
                            $"gridStep = {gridStep}\n"                  +
                            $"gridOffset = {gridOffset}\n"              +
                            $"slowMode = {slowMode}\n"                  +
                            $"colorMode = {colorMode}\n"                +
                            $"opacityMode = {opacityMode}\n"            +
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

            if (doUseGrid)
            {
                size = doUseConstSize
                        ? gridStep
                        : rand.Next(gridStep - 3) + 3;

                x -= x % (gridStep * 2 + gridOffset);
                y -= y % (gridStep * 2 + gridOffset);
            }

            // Store initial coordinates of the particle
            x0 = (int)(x - size);
            y0 = (int)(y - size);

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
                // Dim the tile on the src image
                offScrRenderer.startRendering();
                {
                    if (true)
                    {
                        float r = myUtils.randFloat(rand) * 0.1f;
                        float g = myUtils.randFloat(rand) * 0.1f;
                        float b = myUtils.randFloat(rand) * 0.1f;

                        myPrimitive._Rectangle.SetColor(r, g, b, A * 0.25f);
                        myPrimitive._Rectangle.Draw(X + 1, gl_Height - Y - size2x + 2, size2x - 3, size2x - 3, true);
                        myPrimitive._Rectangle.Draw(X + 1, gl_Height - Y - size2x + 2, size2x - 3, size2x - 3, false);
                    }
                    else
                    {
                        float r = myUtils.randFloat(rand) * 0.1f;
                        float g = myUtils.randFloat(rand) * 0.1f;
                        float b = myUtils.randFloat(rand) * 0.1f;

                        for (int i = 0; i < (int)size2x;)
                        {
                            int offX = rand.Next(5);
                            int size = rand.Next(5) + 1;
                            i += size;

                            myPrimitive._Rectangle.SetColor(r, g, b, A * 0.15f + myUtils.randFloat(rand) * 0.1f);
                            myPrimitive._Rectangle.Draw(X + offX, gl_Height - Y - i, size2x + rand.Next(5), size, true);
                        }

                        //myPrimitive._Rectangle.Draw(X + 1, gl_Height - Y - size2x + 2, size2x - 3, size2x - 3, true);
                    }
                }
                offScrRenderer.stopRendering();
            }

            // Draw instanced squares (borders only)
            if (true)
            {
                var rectInst = inst as myRectangleInst;

                rectInst.setInstanceCoords(X, Y, size2x, size2x);
                rectInst.setInstanceColor(R, G, B, a);
                rectInst.setInstanceAngle(angle);
            }

            // Draw texture pieces
            if (true)
            {
                int wh = (int)size2x;

                tex.setOpacity(a);
                tex.setAngle(-angle);

                tex.Draw((int)X, (int)Y, wh, wh, (int)x0, (int)y0, wh, wh);
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Process(Window window)
        {
            uint cnt = 0;
            initShapes();

            // Render our main texture to the off-screen texture and show it for the first time
            if (true)
            {
                offScrRenderer.startRendering();
                {
                    glDrawBuffer(GL_BACK);
                    glClearColor(1, 1, 1, 1);
                    glClear(GL_COLOR_BUFFER_BIT);
                    //glClear(GL_DEPTH_BUFFER_BIT);

                    tex.setOpacity(1);
                    tex.Draw(0, 0, gl_Width, gl_Height, 0, 0, gl_Width, -gl_Height);
                }
                offScrRenderer.stopRendering();

                offScrRenderer.Draw(0, 0, gl_Width, gl_Height);
                Glfw.SwapBuffers(window);
                System.Threading.Thread.Sleep(111);
            }

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
                    tex.UpdateVertices__WorkaroundTmp();
                    offScrRenderer.Draw(0, 0, gl_Width, gl_Height);

                    int Count = list.Count;

                    for (int i = 0; i != Count; i++)
                    {
                        var obj = list[i] as myObj_071;

                        obj.Show();
                        obj.Move();
                    }

                    // Set border opacity to A x 2
                    inst.SetColorA(-2.0f);
                    inst.Draw(false);
                }

                if (list.Count < N)
                {
                    list.Add(new myObj_071());
                }

                System.Threading.Thread.Sleep(renderDelay);
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void initShapes()
        {
            myPrimitive.init_Rectangle();

            base.initShapes(0, N, 0);

            offScrRenderer = new myTexRectangle_Renderer();

            tex = new myTexRectangle(colorPicker.getImg());

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------
    }
};
