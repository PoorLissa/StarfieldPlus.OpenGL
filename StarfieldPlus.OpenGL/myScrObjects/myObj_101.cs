using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;


/*
    - Random pieces of the screen are shown at their own slightly offset locations
*/


namespace my
{
    public class myObj_101 : myObject
    {
        // Priority
        public static int Priority => 10;
		public static System.Type Type => typeof(myObj_101);

        private int x, y, w, h;
        private int maxOffset, offX, offY;
        private float A, angle = 0;

        private static int N = 0, objN = 0, sizeMode = 0, opacityMode = 0, offsetMode = 0, angleMode = 0, maxX = 0, maxY = 0, maxZ = 0;
        private static float dimAlpha = 0.001f, dDimAlpha = 0.00001f;

        private static myTexRectangle tex = null;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_101()
        {
            generateNew();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time global initialization
        protected override void initGlobal()
        {
            colorPicker = new myColorPicker(gl_Width, gl_Height, mode: myColorPicker.colorMode.SNAPSHOT_OR_IMAGE);
            list = new List<myObject>();

            // Global unmutable constants
            {
                N = 13;
            }

            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time local initialization
        private void initLocal()
        {
            doClearBuffer = myUtils.randomBool(rand);

            objN = rand.Next(N) + 1;

            sizeMode = rand.Next(8);
            opacityMode = rand.Next(9);
            offsetMode = rand.Next(4);
            angleMode = rand.Next(3);

            maxX = rand.Next(100) + 1;
            maxY = rand.Next(100) + 1;
            maxZ = rand.Next(100) + 0;

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            height = 600;

            string str = $"Obj = myObj_101\n\n"                     +
                            $"N = {objN} of {N}\n"                  +
                            $"doClearBuffer = {doClearBuffer}\n"    +
                            $"maxX = {maxX}\n"                      +
                            $"maxY = {maxY}\n"                      +
                            $"maxZ = {maxZ}\n"                      +
                            $"sizeMode = {sizeMode}\n"              +
                            $"opacityMode = {opacityMode}\n"        +
                            $"offsetMode = {offsetMode}\n"          +
                            $"angleMode = {angleMode}\n"            +
                            $"renderDelay = {renderDelay}\n"        +
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
            angle = 0;

            w = rand.Next(maxX) + maxZ;
            h = rand.Next(maxY) + maxZ;

            maxOffset = rand.Next(25) + 5;

            if (maxOffset % 2 == 0)
                maxOffset++;

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            x = rand.Next(gl_Width  + 200) - 100;
            y = rand.Next(gl_Height + 200) - 100;

            switch (offsetMode)
            {
                case 0: case 1:
                    offX = rand.Next(maxOffset) - maxOffset / 2;
                    offY = rand.Next(maxOffset) - maxOffset / 2;
                    break;

                // offY is less than offX
                case 2:
                    offX = rand.Next(maxOffset) - maxOffset / 2;
                    offY = (rand.Next(maxOffset) - maxOffset / 2) / 2;
                    break;

                // offX is less than offY
                case 3:
                    offX = (rand.Next(maxOffset) - maxOffset / 2) / 2;
                    offY = rand.Next(maxOffset) - maxOffset / 2;
                    break;
            }

            switch (opacityMode)
            {
                case 0:
                case 1:
                    A = myUtils.randFloat(rand);
                    break;

                case 2:
                case 3:
                    A = myUtils.randFloat(rand) * 0.5f;
                    break;

                case 4:
                case 5:
                    A = myUtils.randFloat(rand) * 0.25f;
                    break;

                case 6:
                    A = 0.5f;
                    break;

                case 7:
                    A = 0.85f;
                    break;

                case 8:
                    A = 0.95f;
                    break;
            }

            switch (sizeMode)
            {
                case 0:
                case 1:
                    break;

                case 2:
                case 3:
                    w = rand.Next(maxX) + maxZ;
                    h = rand.Next(maxY) + maxZ;
                    break;

                case 4:
                    w = rand.Next(maxX) + maxZ;
                    break;

                case 5:
                    h = rand.Next(maxX) + maxZ;
                    break;

                case 6:
                    w = rand.Next(maxX) + maxZ;
                    h = rand.Next(3) + 1;
                    break;

                case 7:
                    w = rand.Next(3) + 1;
                    h = rand.Next(maxX) + maxZ;
                    break;
            }

            switch (angleMode)
            {
                case 0:
                case 1:
                    angle = 0;
                    break;

                case 2:
                    angle = myUtils.randomSign(rand) * myUtils.randFloat(rand) * 0.05f;
                    break;
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Show()
        {
            tex.setOpacity(A);
            tex.setAngle(angle);
            tex.Draw(x, y, w, h, x + offX, y + offY, w, h);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Process(Window window)
        {
            uint cnt = 0;
            initShapes();

            while (list.Count < N)
            {
                list.Add(new myObj_101());
            }

            // Disable VSYNC if needed
            // Glfw.SwapInterval(0);

            dimScreenRGB_SetRandom(0.2f, myUtils.randomBool(rand));
            glDrawBuffer(GL_FRONT_AND_BACK);

            while (!Glfw.WindowShouldClose(window))
            {
                processInput(window);

                // Swap fore/back framebuffers, and poll for operating system events.
                Glfw.SwapBuffers(window);
                Glfw.PollEvents();

                // Dim screen
                {
                    if (doClearBuffer)
                    {
                        dimScreen(dimAlpha);
                    }
                }

                // Render Frame
                {
                    for (int i = 0; i != objN; i++)
                    {
                        var obj = list[i] as myObj_101;

                        obj.Show();
                        obj.Move();
                    }
                }

                cnt++;
                System.Threading.Thread.Sleep(renderDelay);

                dimAlpha += dDimAlpha;

                if ((dimAlpha > 0.01f && dDimAlpha > 0) || (dimAlpha < 0.001f && dDimAlpha < 0))
                    dDimAlpha *= -1;
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void initShapes()
        {
            myPrimitive.init_ScrDimmer();

            tex = new myTexRectangle(colorPicker.getImg());

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------
    }
};
