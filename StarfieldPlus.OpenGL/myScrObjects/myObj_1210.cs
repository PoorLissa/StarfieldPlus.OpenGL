
using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;


/*
    - Rotating lines aligned to grid
*/


namespace my
{
    public class myObj_1210 : myObject
    {
        // Priority
        public static int Priority => 10;
		public static System.Type Type => typeof(myObj_1210);

        private float x, y, len, angle, dAngle;
        private float A, R, G, B;

        private static int N = 0, gridStepX = 0, gridStepY = 0, lenMin = 0, lenMax = 0;
        private static int angleMode = 0, allocMode = 0, rotateMode = 0, placeMode = 0;
        private static bool doFillShapes = false;
        private static float dimAlpha = 0.05f;

        private static myScreenGradient grad = null;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_1210()
        {
            if (id != uint.MaxValue)
                generateNew();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time global initialization
        protected override void initGlobal()
        {
            var colorMode = myUtils.randomChance(rand, 4, 5)
                ? myColorPicker.colorMode.SNAPSHOT_OR_IMAGE
                : myColorPicker.colorMode.RANDOM_MODE;

            colorPicker = new myColorPicker(gl_Width, gl_Height, colorMode);
            list = new List<myObject>();

            // Global unmutable constants
            {
                N = 11111;
            }

            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time local initialization
        private void initLocal()
        {
            doClearBuffer = myUtils.randomChance(rand, 2, 3);

            angleMode = rand.Next(2);
            allocMode = rand.Next(3);
            rotateMode = rand.Next(3);
            placeMode = myUtils.randomChance(rand, 1, 10) ? 1 : 0;

            gridStepX = rand.Next(222) + 50;
            gridStepY = rand.Next(222) + 50;

            lenMin = 66;
            lenMax = 666;

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            height = 600;

            string str = $"Obj = {Type}\n\n"                     +
                            myUtils.strCountOf(list.Count, N)    +
                            $"doClearBuffer = {doClearBuffer}\n" +
                            $"angleMode = {angleMode}\n"         +
                            $"allocMode = {allocMode}\n"         +
                            $"placeMode = {placeMode}\n"         +
                            $"rotateMode = {rotateMode}\n"       +
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
            // Align to grid
            if (placeMode == 0)
            {
                int offsetX = (gl_Width  % gridStepX) / 2;
                int offsetY = (gl_Height % gridStepY) / 2;

                x = rand.Next(gl_Width  + gridStepX);
                y = rand.Next(gl_Height + gridStepY);

                x -= x % gridStepX;
                y -= y % gridStepY;

                x += offsetX;
                y += offsetY;

                x += rand.Next(3) - 1;
                y += rand.Next(3) - 1;
            }
            else
            {
                x = rand.Next(gl_Width);
                y = rand.Next(gl_Height);
            }

            len = rand.Next(lenMax) + lenMin;

            switch (angleMode)
            {
                case 0:
                    angle = myUtils.randFloat(rand) * 321;
                    dAngle = myUtils.randFloat(rand, 0.1f) * 0.0025f;
                    break;

                case 1:
                    angle = 0;
                    dAngle = 0.001f * (rand.Next(11) + 1);
                    break;
            }

            switch (rotateMode)
            {
                case 0: dAngle *= +1; break;
                case 1: dAngle *= -1; break;
                case 2: dAngle *= myUtils.randomSign(rand); break;
            }

            A = myUtils.randFloatClamped(rand, 0.25f) * 0.1f;
            colorPicker.getColor(x, y, ref R, ref G, ref B);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            angle += dAngle;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Show()
        {
            float sin = (float)Math.Sin(angle) * len;
            float cos = (float)Math.Cos(angle) * len;

            myPrimitive._LineInst.setInstanceCoords(x + sin, y + cos, x - sin, y - cos);
            myPrimitive._LineInst.setInstanceColor(R, G, B, A);
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Process(Window window)
        {
            uint cnt = 0;
            initShapes();

            clearScreenSetup(doClearBuffer, 0.1f);

            {
                if (allocMode > 0)
                {
                    int n = allocMode == 1 ? N/2 : N;

                    while (list.Count < n)
                        list.Add(new myObj_1210());
                }
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
                    myPrimitive._LineInst.ResetBuffer();

                    for (int i = 0; i != Count; i++)
                    {
                        var obj = list[i] as myObj_1210;

                        obj.Show();
                        obj.Move();
                    }

                    myPrimitive._LineInst.Draw();
                }

                if (Count < N)
                {
                    list.Add(new myObj_1210());
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
            myPrimitive.init_LineInst(N);

            grad = new myScreenGradient();
            grad.SetRandomColors(rand, 0.2f);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------
    }
};
