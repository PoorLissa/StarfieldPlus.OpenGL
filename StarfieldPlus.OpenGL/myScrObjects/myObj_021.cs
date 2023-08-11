using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;


/*
    - Ever Growing Shapes located at the center of the screen + small offset
*/


namespace my
{
    public class myObj_021 : myObject
    {
        // Priority
        public static int Priority => 999910;

        private float x, y, size, dSize, angle, A = 0, R = 0, G = 0, B = 0;
        int lifeCounter = 0;

        private static int shape = 0, N = 0, angleMode = 0, opacityMode = 0, colorMode = 0, xyGenerateMode = 0, offset = 0, offsetMode = 0;
        private static bool doFillShapes = false;
        private static float dimAlpha = 0.1f, dSizeBase = 0, dA = 0, Angle = 0;
        private static uint cnt = 0;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_021()
        {
            generateNew();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time global initialization
        protected override void initGlobal()
        {
            colorPicker = new myColorPicker(gl_Width, gl_Height);
            list = new List<myObject>();

            switch (rand.Next(3))
            {
                case 0: N = 333 + rand.Next(0111); break;
                case 1: N = 333 + rand.Next(1111); break;
                case 2: N = 333 + rand.Next(3333); break;
            }

            shape = rand.Next(5);

            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // Local initialization
        private void initLocal()
        {
            renderDelay = rand.Next(11) + 3;

            doFillShapes = myUtils.randomBool(rand);
            doClearBuffer = myUtils.randomChance(rand, 1, 2);
            dimAlpha = myUtils.randFloat(rand) * 0.7f + 0.05f;      // 0.75f at max, otherwise with doClearBuffer == true we won't see anything

            colorMode = rand.Next(2);
            opacityMode = rand.Next(2);
            offsetMode = rand.Next(2);
            angleMode = rand.Next(3);
            xyGenerateMode = rand.Next(6);

            switch (offsetMode)
            {
                case 0: offset = 333; break;
                case 1: offset = 111 + rand.Next(234); break;
            }

            switch (rand.Next(3))
            {
                case 0: dSizeBase = 0.0005f; break;
                case 1: dSizeBase = 0.0010f; break;
                case 2: dSizeBase = 0.0015f; break;
            }

            switch (rand.Next(3))
            {
                case 0: dA = 0.001f; break;
                case 1: dA = 0.002f; break;
                case 2: dA = 0.003f; break;
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            string fStr(float f) { return f.ToString("0.000"); }

            string str = $"Obj = myObj_021\n\n"                         +
                            $"N = {list.Count} of {N}\n"                +
                            $"doClearBuffer = {doClearBuffer}\n"        +
                            $"dimAlpha = {dimAlpha.ToString("0.00")}\n" +
                            $"shape = {shape}\n"                        +
                            $"dSizeBase = {fStr(dSizeBase)}\n"          +
                            $"dA = {fStr(dA)}\n"                        +
                            $"offset = {offset}\n"                      +
                            $"xyGenerateMode = {xyGenerateMode}\n"      +
                            $"colorMode = {colorMode}\n"                +
                            $"opacityMode = {opacityMode}\n"            +
                            $"angleMode = {angleMode}\n"                +
                            $"renderDelay = {renderDelay}\n"            +
                            $"cnt = {cnt}\n"                            +
                            $"file: {colorPicker.GetFileName()}"
                ;
            return str;
        }

        // ---------------------------------------------------------------------------------------------------------------

        // Press 'Space' to change mode
        protected override void setNextMode()
        {
            getNewCounter(5000, 30000);

            initLocal();

            System.Threading.Thread.Sleep(123);

            clearScreenSetup(doClearBuffer, 0.2f);
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void generateNew()
        {
            lifeCounter = rand.Next(100) + 100;

            getXY();

            switch (angleMode)
            {
                case 0:
                    angle = 0;
                    break;

                case 1:
                    angle = myUtils.randFloat(rand);
                    break;

                case 2:
                    angle = Angle;
                    Angle += 0.005f;
                    break;
            }

            size = 1.0f;
            dSize = dSizeBase * (rand.Next(1000) + 1);

            A = myUtils.randFloat(rand);

            switch (colorMode)
            {
                case 0:
                    colorPicker.getColor(x, y, ref R, ref G, ref B);
                    break;

                case 1:
                    colorPicker.getColorRand(ref R, ref G, ref B);
                    break;
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            if (size != 0)
            {
                size += dSize;
                A -= dA;

                if (A < 0)
                {
                    x = -1111;
                    y = -1111;
                    size = 0;
                }
            }
            else
            {
                if (lifeCounter-- == 0)
                {
                    generateNew();
                }
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Show()
        {
            if (size > 0)
            {
                float sizeX2 = size * 2;

                switch (shape)
                {
                    case 0:
                        var rectInst = inst as myRectangleInst;

                        rectInst.setInstanceCoords(x - size, y - size, sizeX2, sizeX2);
                        rectInst.setInstanceColor(R, G, B, A);
                        rectInst.setInstanceAngle(angle);
                        break;

                    case 1:
                        var triangleInst = inst as myTriangleInst;

                        triangleInst.setInstanceCoords(x, y, size, angle);
                        triangleInst.setInstanceColor(R, G, B, A);
                        break;

                    case 2:
                        var ellipseInst = inst as myEllipseInst;

                        ellipseInst.setInstanceCoords(x, y, sizeX2, angle);
                        ellipseInst.setInstanceColor(R, G, B, A);
                        break;

                    case 3:
                        var pentagonInst = inst as myPentagonInst;

                        pentagonInst.setInstanceCoords(x, y, sizeX2, angle);
                        pentagonInst.setInstanceColor(R, G, B, A);
                        break;

                    case 4:
                        var hexagonInst = inst as myHexagonInst;

                        hexagonInst.setInstanceCoords(x, y, sizeX2, angle);
                        hexagonInst.setInstanceColor(R, G, B, A);
                        break;
                }
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Process(Window window)
        {
            int Count = 0;

            getNewCounter(10000, 5000);
            initShapes();

            clearScreenSetup(doClearBuffer, 0.2f);

            while (!Glfw.WindowShouldClose(window))
            {
                processInput(window);
                Glfw.PollEvents();

                if (doClearBuffer)
                {
                    Glfw.SwapBuffers(window);
                    glClear(GL_COLOR_BUFFER_BIT);
                }
                else
                {
                    dimScreen(dimAlpha, false);
                    Glfw.SwapBuffers(window);
                }

                Count = list.Count;

                // Render Frame
                {
                    inst.ResetBuffer();

                    for (int i = 0; i != Count; i++)
                    {
                        var obj = list[i] as myObj_021;

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
                    list.Add(new myObj_021());
                }

                System.Threading.Thread.Sleep(renderDelay);

                // Switch the mode occasionally
                if (--cnt == 0)
                {
                    setNextMode();
                }
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void initShapes()
        {
			myPrimitive.init_ScrDimmer();

            base.initShapes(shape, N, 0);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        void getNewCounter(uint min, int rnd)
        {
            cnt = min + (uint)rand.Next(rnd);
        }

        // ---------------------------------------------------------------------------------------------------------------

        // Generate initial coordinates of a particle
        void getXY()
        {
            switch (xyGenerateMode)
            {
                // Central spot
                case 0:
                    x = gl_x0 + rand.Next(offset * 2) - offset;
                    y = gl_y0 + rand.Next(offset * 2) - offset;
                    break;

                // Horizontal line
                case 1:
                    y = gl_y0;
                    x = gl_x0 + rand.Next(offset * 6) - offset*3;
                    break;

                // Vertical line
                case 2:
                    x = gl_x0;
                    y = gl_y0 + rand.Next(offset * 4) - offset * 2;
                    break;

                // Rectangle
                case 3:
                    switch (rand.Next(4))
                    {
                        case 0:
                            x = offset;
                            y = rand.Next(gl_Height);
                            break;

                        case 1:
                            x = gl_Width - offset;
                            y = rand.Next(gl_Height);
                            break;

                        case 2:
                            x = rand.Next(gl_Width);
                            y = offset;
                            break;

                        case 3:
                            x = rand.Next(gl_Width);
                            y = gl_Height - offset;
                            break;
                    }
                    break;

                // Generate outside of the screen
                case 4:
                    switch (rand.Next(4))
                    {
                        case 0:
                            x = -100;
                            y = rand.Next(gl_Height);
                            break;

                        case 1:
                            x = gl_Width + 100;
                            y = rand.Next(gl_Height);
                            break;

                        case 2:
                            x = rand.Next(gl_Width);
                            y = -100;
                            break;

                        case 3:
                            x = rand.Next(gl_Width);
                            y = gl_Height + 100;
                            break;
                    }
                    break;

                // 4 corners
                case 5:
                    switch (rand.Next(4))
                    {
                        case 0:
                            x = offset;
                            y = offset;
                            break;

                        case 1:
                            x = gl_Width - offset;
                            y = offset;
                            break;

                        case 2:
                            x = offset;
                            y = gl_Height - offset;
                            break;

                        case 3:
                            x = gl_Width - offset;
                            y = gl_Height - offset;
                            break;
                    }
                    break;
            }
        }

        // ---------------------------------------------------------------------------------------------------------------

    };
};
