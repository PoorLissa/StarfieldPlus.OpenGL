using GLFW;
using static OpenGL.GL;
using System.Collections.Generic;


/*
    - Desktop pieces swapping
*/


namespace my
{
    public class myObj_050 : myObject
    {
        private int x, y, sizeX, sizeY;
        private bool isVertical;

        private static bool doShowImage = false;
        private static int N = 0, mode = 0, opacityMode = 0;

        static myTexRectangle tex = null;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_050()
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
            N = rand.Next(3) + 1;

            mode = rand.Next(3);
            opacityMode = rand.Next(7);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            height = 800;

            string str = $"Obj = myObj_050\n\n" +
                            $"N = {list.Count} of {N}\n" +
                            $"doClearBuffer = {doClearBuffer}\n" +
                            $"mode = {mode}\n" +
                            $"opacityMode = {opacityMode}\n" +
                            $"file: {colorPicker.GetFileName()}" +
                            $""
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
            sizeX = rand.Next(11) + 1;
            sizeY = rand.Next(11) + 1;

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            switch (mode)
            {
                case 0:
                    {
                        x = rand.Next(gl_Height / sizeY);
                        y = rand.Next(gl_Height / sizeY);
                        x *= sizeY;
                        y *= sizeY;
                    }
                    break;

                case 1:
                    {
                        x = rand.Next(gl_Width / sizeX);
                        y = rand.Next(gl_Width / sizeX);
                        x *= sizeX;
                        y *= sizeX;
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
                    tex.Draw(0, (int)x, gl_Width, sizeY, 0, (int)y, gl_Width, sizeY);
                    break;

                // Draw a vertical line at a random x
                case 1:
                    tex.Draw((int)x, 0, sizeX, gl_Height, (int)y, 0, sizeX, gl_Height);
                    break;

                // Draw horizontal or vertical line
                case 2:
                    if (isVertical)
                    {
                        tex.Draw((int)x, 0, sizeX, gl_Height, (int)y, 0, sizeX, gl_Height);
                    }
                    else
                    {
                        tex.Draw(0, (int)x, gl_Width, sizeY, 0, (int)y, gl_Width, sizeY);
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
                        var obj = list[i] as myObj_050;

                        obj.Show();
                        obj.Move();
                    }
                }

                if (list.Count < N)
                {
                    list.Add(new myObj_050());
                }

                System.Threading.Thread.Sleep(renderDelay);
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void initShapes()
        {
            myPrimitive.init_Rectangle();

            tex = new myTexRectangle(colorPicker.getImg());
        }

        // ---------------------------------------------------------------------------------------------------------------
    }
};
