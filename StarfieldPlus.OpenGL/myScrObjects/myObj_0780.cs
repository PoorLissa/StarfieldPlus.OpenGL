﻿using GLFW;
using static OpenGL.GL;
using System.Collections.Generic;


/*
    - Instanced shapes in large numbers revealing an underlying image
*/


namespace my
{
    public class myObj_0780 : myObject
    {
        // Priority
        public static int Priority => 33;
		public static System.Type Type => typeof(myObj_0780);

        private int cnt;
        private float x, y, dx, dy;
        private float size, A, R, G, B, angle = 0, dAngle;

        private static int N = 0, shape = 0, dirMode = 0, maxSize = 0, gridStepX = 33, gridStepY = 33;
        private static bool doFillShapes = false, doRotate = false, doUseGrid = false;
        private static float maxSpeed = 1;

        private static myScreenGradient grad = null;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_0780()
        {
            if (id != uint.MaxValue)
                generateNew();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time global initialization
        protected override void initGlobal()
        {
            var mode = myUtils.randomChance(rand, 1, 5)
                ? myColorPicker.colorMode.RANDOM_MODE
                : myColorPicker.colorMode.SNAPSHOT_OR_IMAGE;

            colorPicker = new myColorPicker(gl_Width, gl_Height, mode);
            list = new List<myObject>();

            // Global unmutable constants
            {
                N = colorPicker.isImage()
                    ? 100000 + rand.Next(333000)        // for a picture
                    :  50000 + rand.Next(50000);        // for a non-picture

                shape = rand.Next(5);
            }

            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time local initialization
        private void initLocal()
        {
            doClearBuffer = myUtils.randomChance(rand, 1, 2);
            doFillShapes = myUtils.randomChance(rand, 1, 3);
            doRotate = myUtils.randomChance(rand, 1, 2);
            doUseGrid = myUtils.randomChance(rand, 1, 2);

            maxSpeed = myUtils.randFloat(rand, 0.1f) * (myUtils.randomChance(rand, 4, 5)  ? 0.5f : 1.0f);
            maxSize = 3 + rand.Next(11);

            dirMode = rand.Next(7);

            renderDelay = 0;

            if (myUtils.randomChance(rand, 1, 3))
            {
                gridStepX = rand.Next(100) + 33;
                gridStepY = rand.Next(100) + 33;
            }
            else
            {
                gridStepX = rand.Next(100) + 33;
                gridStepY = gridStepX;
            }

#if false
            doClearBuffer = true;
            doFillShapes = false;
            doRotate = false;
            maxSpeed = 0.25f;
            maxSize = 5;
            dirMode = 3;
#endif

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            height = 600;

            string nStr(int   n) { return n.ToString("N0");    }
            string fStr(float f) { return f.ToString("0.000"); }

            string str = $"Obj = {Type}\n\n"                         +
                            $"N = {nStr(list.Count)} of {nStr(N)}\n" +
                            $"doClearBuffer = {doClearBuffer}\n"     +
                            $"doFillShapes = {doFillShapes}\n"       +
                            $"doRotate = {doRotate}\n"               +
                            $"doUseGrid = {doUseGrid}\n"             +
                            $"gridStepX = {gridStepX}\n"             +
                            $"gridStepY = {gridStepY}\n"             +
                            $"dirMode = {dirMode}\n"                 +
                            $"maxSize = {maxSize}\n"                 +
                            $"maxSpeed = {fStr(maxSpeed)}\n"         +
                            $"renderDelay = {renderDelay}\n"         +
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
            x = rand.Next(gl_Width);
            y = rand.Next(gl_Height);

            if (doUseGrid)
            {
                x -= x % gridStepX;
                y -= y % gridStepY;
            }

            switch (dirMode)
            {
                case 0:
                    dx = myUtils.randFloatSigned(rand, 0.1f) * maxSpeed;
                    dy = 0;
                    break;

                case 1:
                    dx = 0;
                    dy = myUtils.randFloatSigned(rand, 0.1f) * maxSpeed;
                    break;

                default:
                    dx = myUtils.randFloatSigned(rand, 0.1f) * maxSpeed;
                    dy = myUtils.randFloatSigned(rand, 0.1f) * maxSpeed;
                    break;
            }

            size = rand.Next(maxSize) + 3;

            angle = myUtils.randFloat(rand) * rand.Next(123);

            dAngle = doRotate
                ? myUtils.randFloatSigned(rand) * 0.1f
                : 0;

            A = myUtils.randFloat(rand) * 0.85f;
            colorPicker.getColorSafe(x, y, ref R, ref G, ref B);

            cnt = 111 + rand.Next(333);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            x += dx;
            y += dy;
            angle += dAngle;

            if (--cnt < 0)
            {
                A -= 0.01f;

                if (A < 0)
                    generateNew();
            }

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
                    myPrimitive._TriangleInst.setInstanceCoords(x, y, size, angle);
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

            if (doClearBuffer == false)
            {
                grad.SetOpacity(0.25f);
            }

            while (list.Count < N)
            {
                list.Add(new myObj_0780());
            }

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
                        grad.Draw();
                    }
                }

                // Render Frame
#if !false
                // Render instanced primitives several times per frame (test this)
                {
                    inst.ResetBuffer();

                    int itemCnt = 0;

                    for (int i = 0; i != Count; i++)
                    {
                        var obj = list[i] as myObj_0780;

                        obj.Show();
                        obj.Move();

                        if (++itemCnt == 10000)
                        {
                            if (doFillShapes)
                            {
                                // Tell the fragment shader to multiply existing instance opacity by 0.5:
                                inst.SetColorA(-0.5f);
                                inst.Draw(true);
                            }

                            // Tell the fragment shader to do nothing with the existing instance opacity:
                            inst.SetColorA(0);
                            inst.Draw(false);

                            itemCnt = 0;
                            inst.ResetBuffer();
                        }
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
#else
                {
                    inst.ResetBuffer();

                    for (int i = 0; i != Count; i++)
                    {
                        var obj = list[i] as myObj_0780;

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
#endif
                cnt++;
                System.Threading.Thread.Sleep(renderDelay);
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

            //grad.SetColor (1, 1, 1, 1);
            //grad.SetColor2(0, 0, 0, 1);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------
    }
};
