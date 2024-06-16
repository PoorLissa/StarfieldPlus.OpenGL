using GLFW;
using static OpenGL.GL;
using System.Collections.Generic;


/*
    - Desktop pieces get swapped
*/


namespace my
{
    public class myObj_0050 : myObject
    {
        // Priority
        public static int Priority => 10;
		public static System.Type Type => typeof(myObj_0050);

        private int x, y, sizeX, sizeY, sizeDst, sizeSrc;
        private bool isVertical;

        private static bool doShowImage = false, doShowInPlace = false, doShowInPlaceSometimes = true;
        private static int N = 0, mode = 0, opacityMode = 0, angleMode = 0, compressMode = 0;
        private static int inPlaceCounter = 0;

        static myTexRectangle tex = null;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_0050()
        {
            generateNew();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time global initialization
        protected override void initGlobal()
        {
            colorPicker = new myColorPicker(gl_Width, gl_Height, mode: myColorPicker.colorMode.SNAPSHOT_OR_IMAGE);
            list = new List<myObject>();

            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time local initialization
        private void initLocal()
        {
            doClearBuffer = myUtils.randomBool(rand);
            doShowImage = myUtils.randomBool(rand);
            doShowInPlace = myUtils.randomChance(rand, 1, 5);

            doShowInPlaceSometimes = doShowInPlace ? false : myUtils.randomChance(rand, 1, 2);

            N = rand.Next(3) + 1;

            mode = rand.Next(3);
            opacityMode = rand.Next(7);
            angleMode = rand.Next(6);
            compressMode = rand.Next(4);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            height = 800;

            string str = $"Obj = {Type}\n\n"                                       +
                            $"N = {list.Count} of {N}\n"                           +
                            $"doClearBuffer = {doClearBuffer}\n"                   +
                            $"doShowInPlace = {doShowInPlace}\n"                   +
                            $"doShowInPlaceSometimes = {doShowInPlaceSometimes}\n" +
                            $"mode = {mode}\n"                                     +
                            $"opacityMode = {opacityMode}\n"                       +
                            $"angleMode = {angleMode}\n"                           +
                            $"compressMode = {compressMode}\n"                     +
                            $"sizeX = {sizeX}\n"                                   +
                            $"sizeY = {sizeY}\n"                                   +
                            $"renderDelay = {renderDelay}\n"                       +
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
            // If the size is greater than 10, the dest image is going to be compressed
            sizeX = rand.Next(25) + 1;
            sizeY = rand.Next(25) + 1;

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            if (doShowInPlaceSometimes)
            {
                // Sometimes set 'doShowInPlace' to true, so the image would kind of 'appear' out of the chaos;
                // Set the counter, and when it reaches 0, set 'doShowInPlace' to false
                if (inPlaceCounter == 0)
                {
                    doShowInPlace = false;

                    if (myUtils.randomChance(rand, 1, 100))
                    {
                        if (myUtils.randomChance(rand, 1, 10))
                        {
                            doShowInPlace = true;
                            inPlaceCounter = rand.Next(500) + 500;
                        }
                    }
                }
                else
                {
                    inPlaceCounter--;
                }
            }

            switch (mode)
            {
                case 0:
                    {
                        x = rand.Next(gl_Height);
                        y = rand.Next(gl_Height);

                        if (doShowInPlace)
                            x = y;
                    }
                    break;

                case 1:
                    {
                        x = rand.Next(gl_Width);
                        y = rand.Next(gl_Width);

                        if (doShowInPlace)
                            x = y;
                    }
                    break;

                case 2:
                    {
                        isVertical = myUtils.randomBool(rand);

                        if (isVertical)
                        {
                            goto case 1;
                        }
                        else
                        {
                            goto case 0;
                        }
                    }
                    break;
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Show()
        {
            switch (angleMode)
            {
                case 0:
                case 1:
                    tex.setAngle(0);
                    break;

                case 2:
                    tex.setAngle(myUtils.randomSign(rand) * myUtils.randFloat(rand) * 0.01f);
                    break;

                case 3:
                    tex.setAngle(myUtils.randomSign(rand) * myUtils.randFloat(rand) * 0.02f);
                    break;

                case 4:
                    tex.setAngle(myUtils.randomSign(rand) * myUtils.randFloat(rand) * 0.03f);
                    break;

                case 5:
                    tex.setAngle(myUtils.randomSign(rand) * myUtils.randFloat(rand));
                    break;
            }

            switch (opacityMode)
            {
                case 0:
                    tex.setOpacity(1);
                    break;

                case 1:
                    tex.setOpacity(0.75f);
                    break;

                case 2:
                    tex.setOpacity(0.5f);
                    break;

                case 3:
                    tex.setOpacity(0.25f);
                    break;

                case 4:
                case 5:
                case 6:
                    tex.setOpacity(myUtils.randFloat(rand));
                    break;
            }

            switch (mode)
            {
                // Draw a horizontal line at a random y
                case 0:
                    sizeDst = sizeSrc = sizeY;

                    if (sizeDst > 10)
                        sizeDst /= 3;

                    tex.Draw(0, (int)x, gl_Width, sizeDst, 0, (int)y, gl_Width, sizeSrc);
                    break;

                // Draw a vertical line at a random x
                case 1:
                    sizeDst = sizeSrc = sizeX;

                    if (sizeDst > 10)
                        sizeDst /= 3;

                    tex.Draw((int)x, 0, sizeDst, gl_Height, (int)y, 0, sizeSrc, gl_Height);
                    break;

                // Draw horizontal or vertical line
                case 2:
                    if (isVertical)
                    {
                        goto case 1;
                    }
                    else
                    {
                        goto case 0;
                    }
                    break;

                // Swap 2 Rectangles
                case 3:
                    break;
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Process(Window window)
        {
            uint cnt = 0;
            initShapes();

            glDrawBuffer(GL_FRONT_AND_BACK);

            if (doShowImage)
            {
                if (myUtils.randomBool(rand))
                {
                    tex.setOpacity(1.0f);
                }
                else
                {
                    tex.setOpacity(myUtils.randFloat(rand, 0.25f));
                }

                tex.Draw(0, 0, gl_Width, gl_Height);
            }

            while (!Glfw.WindowShouldClose(window))
            {
                cnt++;

                processInput(window);

                // Swap fore/back framebuffers, and poll for operating system events.
                Glfw.SwapBuffers(window);
                Glfw.PollEvents();

                if (doClearBuffer)
                {
                    dimScreen(0.05f, false);
                }

                // Render Frame
                {
                    for (int i = 0; i < list.Count; i++)
                    {
                        var obj = list[i] as myObj_0050;

                        obj.Show();
                        obj.Move();
                    }
                }

                if (list.Count < N)
                {
                    list.Add(new myObj_0050());
                }

                System.Threading.Thread.Sleep(renderDelay);
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void initShapes()
        {
            myPrimitive.init_ScrDimmer();

            tex = new myTexRectangle(colorPicker.getImg());
        }

        // ---------------------------------------------------------------------------------------------------------------
    }
};
