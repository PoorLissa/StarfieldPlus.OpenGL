using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;


/*
    - Centers of rotation attached to a grid
*/


namespace my
{
    public class myObj_1220 : myObject
    {
        // Priority
        public static int Priority => 10;
		public static System.Type Type => typeof(myObj_1220);

        private float x, y, x0, y0, dx, dy, rad, mass, alpha, dAlpha;
        private float size, A, R, G, B, angle = 0;

        private static int N = 0, shape = 0, gridStepX = 1, gridStepY = 1, radConst = 1;
        private static int radMode = 0, radMode2 = 0, dAlphaMode = 0;
        private static bool doFillShapes = false, doUseImgColor = false;
        private static float dimAlpha = 0.05f;

        private static myScreenGradient grad = null;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_1220()
        {
            if (id != uint.MaxValue)
                generateNew();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time global initialization
        protected override void initGlobal()
        {
            var colorMode = myUtils.randomChance(rand, 8, 9)
                ? myColorPicker.colorMode.SNAPSHOT_OR_IMAGE
                : myColorPicker.colorMode.RANDOM_MODE;

            colorPicker = new myColorPicker(gl_Width, gl_Height, colorMode);
            list = new List<myObject>();

            // Global unmutable constants
            {
                N = rand.Next(1000) + 50000;

                shape = rand.Next(5);
            }

            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time local initialization
        private void initLocal()
        {
            doClearBuffer = myUtils.randomChance(rand, 2, 3);
            doFillShapes = myUtils.randomChance(rand, 1, 2);
            doUseImgColor = myUtils.randomChance(rand, 1, 5);

            radMode = rand.Next(7);
            radMode2 = myUtils.randomChance(rand, 4, 5) ? 0 : 1;
            dAlphaMode = rand.Next(6);

            int minStep = 33 + rand.Next(17);

            gridStepX = rand.Next(150) + minStep;
            gridStepY = rand.Next(150) + minStep;

            if (rand.Next(2) == 0)
            {
                gridStepY = gridStepX;
            }

            radConst = 33 + rand.Next(133);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            height = 600;

            string str = $"Obj = {Type}\n\n"                     +
                            myUtils.strCountOf(list.Count, N)    +
                            $"doClearBuffer = {doClearBuffer}\n" +
                            $"doFillShapes = {doFillShapes}\n"   +
                            $"doUseImgColor = {doUseImgColor}\n" +
                            $"dAlphaMode = {dAlphaMode}\n"       +
                            $"radMode = {radMode}\n"             +
                            $"radMode2 = {radMode2}\n"           +
                            $"gridStepX = {gridStepX}\n"         +
                            $"gridStepY = {gridStepY}\n"         +
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
            int offsetX = (gl_Width  % gridStepX) / 2;
            int offsetY = (gl_Height % gridStepX) / 2;

            x = rand.Next(gl_Width  + 100);
            y = rand.Next(gl_Height + 100);

            x0 = x - x % gridStepX + offsetX;
            y0 = y - y % gridStepY + offsetY;

            dx = x - x0;
            dy = y - y0;

            size = rand.Next(3) + 3;
            size = 4;
            mass = 1.0f + myUtils.randFloat(rand);
            alpha = myUtils.randFloat(rand) * 321;

            // radMode
            {
                switch (radMode)
                {
                    // Const radius
                    case 0:
                        rad = radConst;
                        break;

                    // Radius is a half grid step
                    case 1:
                        rad = gridStepX / 2;
                        break;

                    // Radius is actual distance from this point to the center of rotation
                    case 2:
                        rad = (float)Math.Sqrt(dx * dx + dy * dy);
                        break;

                    // Radius is a half of actual distance from this point to the center of rotation
                    case 3:
                        rad = (float)Math.Sqrt(dx * dx + dy * dy) / 2;
                        break;

                    // Half of a grid step OR random dist within this number
                    case 4:
                        rad = gridStepX / 2;
                        if (myUtils.randomChance(rand, 1, 2))
                            rad = rand.Next((int)rad);
                        break;

                    // Sin?..
                    case 5:
                        rad = (float)Math.Sin(x) * 100;
                        break;

                    // Sin?..
                    case 6:
                        rad = (float)Math.Sin(id) * 100;
                        break;
                }
            }

            // dAlphaMode
            {
                switch (dAlphaMode)
                {
                    case 0:
                    case 1:
                        dAlpha = 0.01f;
                        break;

                    case 2:
                    case 3:
                        dAlpha = myUtils.randFloatClamped(rand, 0.1f) * 0.02f;
                        break;

                    case 4:
                    case 5:
                        dAlpha = (float)Math.Sin(rad) * 0.03f;
                        break;
                }

                // In odd modes rotation goes 2 ways
                if (dAlphaMode % 2 == 1)
                {
                    dAlpha *= myUtils.randomBool(rand) ? +1 : -1;
                }
            }

            A = myUtils.randFloatClamped(rand, 0.1f);
            colorPicker.getColor(x, y, ref R, ref G, ref B);

            dx = dy = 0;

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            x = x0 + (float)Math.Sin(alpha) * rad;
            y = y0 + (float)Math.Cos(alpha) * rad;

            alpha += dAlpha;

            rad += radMode2 == 1
                ? (float)Math.Sin(alpha * 0.1f) * 0.1f
                : 0;

            if (doUseImgColor && myUtils.randomChance(rand, 1, 33))
                colorPicker.getColor(x, y, ref R, ref G, ref B);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Show()
        {
            float size2x = size * 2;

            switch (shape)
            {
                // Instanced squares
                case 0:
                    myPrimitive._RectangleInst.setInstanceCoords(x - size, y - size, size2x, size2x);
                    myPrimitive._RectangleInst.setInstanceColor(R, G, B, A);
                    myPrimitive._RectangleInst.setInstanceAngle(angle);
                    break;

                // Instanced triangles
                case 1:
                    myPrimitive._TriangleInst.setInstanceCoords(x, y, size2x, angle);
                    myPrimitive._TriangleInst.setInstanceColor(R, G, B, A);
                    break;

                // Instanced circles
                case 2:
                    myPrimitive._EllipseInst.setInstanceCoords(x, y, size2x, angle);
                    myPrimitive._EllipseInst.setInstanceColor(R, G, B, A);
                    break;

                // Instanced pentagons
                case 3:
                    myPrimitive._PentagonInst.setInstanceCoords(x, y, size2x, angle);
                    myPrimitive._PentagonInst.setInstanceColor(R, G, B, A);
                    break;

                // Instanced hexagons
                case 4:
                    myPrimitive._HexagonInst.setInstanceCoords(x, y, size2x, angle);
                    myPrimitive._HexagonInst.setInstanceColor(R, G, B, A);
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

            while (list.Count < N)
            {
                list.Add(new myObj_1220());
            }

            stopwatch = new StarfieldPlus.OpenGL.myUtils.myStopwatch(true);

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
                {
                    inst.ResetBuffer();

                    for (int i = 0; i != Count; i++)
                    {
                        var obj = list[i] as myObj_1220;

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
                    list.Add(new myObj_1220());
                }

                stopwatch.WaitAndRestart();
                cnt++;
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void initShapes()
        {
            myPrimitive.init_ScrDimmer();
            base.initShapes(shape, N, 0);

            grad = new myScreenGradient();
            grad.SetRandomColors(rand, 0.2f);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------
    }
};
