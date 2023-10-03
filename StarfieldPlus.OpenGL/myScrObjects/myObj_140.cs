using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;


/*
    - Grid with moving rectangle lenses -- test, looks strange

    Make it like a lense -- but with an area. The tiles closest to the center get larger scale factor
    Also, as an option: display a grid, where each cell is an avg color from this position;
    AND only where the active object is, display actual texture

    -- Change of plans! This is now a random image viewer which displays 2 different images at different opacity
*/


namespace my
{
    public class myObj_140 : myObject
    {
        // Priority
        public static int Priority => 10;
		public static System.Type Type => typeof(myObj_140);

        private float A, dA;

        private static int N = 0;
        private static float dimAlpha = 0.05f;

        private myTexRectangle tex = null;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_140()
        {
            if (id != uint.MaxValue)
                generateNew();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time global initialization
        protected override void initGlobal()
        {
            colorPicker = new myColorPicker(gl_Width, gl_Height, mode: myColorPicker.colorMode.IMAGE);
            list = new List<myObject>();

            // Global unmutable constants
            {
                N = 2;
            }

            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time local initialization
        private void initLocal()
        {
            doClearBuffer = true;
            renderDelay = 10;

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            height = 600;

            string nStr(int   n) { return n.ToString("N0");    }
            //string fStr(float f) { return f.ToString("0.000"); }

            string str = $"Obj = {Type} -- TBD\n\n"               	 +
                            $"N = {nStr(list.Count)} of {nStr(N)}\n" +
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
            A = 0;
            dA = 0.0001f + myUtils.randFloat(rand) * 0.0003f;

            dA *= 3;

            if (tex == null)
                tex = new myTexRectangle(colorPicker.getImg());
            else
                tex.reloadImg(colorPicker.getImg());

            colorPicker.reloadImage();

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            A += dA;

            if (A > 1 && dA > 0)
            {
                dA *= -1;
            }

            if (A < 0)
            {
                generateNew();
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Show()
        {
            tex.setOpacity(A);
            tex.Draw(0, 0, gl_Width, gl_Height);
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Process(Window window)
        {
            uint cnt = 0;
            initShapes();

            clearScreenSetup(doClearBuffer, 0.1f);

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
                    }
                    else
                    {
                        dimScreen(dimAlpha);
                    }
                }

                // Render Frame
                {
                    for (int i = 0; i != Count; i++)
                    {
                        var obj = list[i] as myObj_140;

                        obj.Show();
                        obj.Move();
                    }
                }

                if (Count < N)
                {
                    list.Add(new myObj_140());
                }

                cnt++;
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void initShapes()
        {
            myPrimitive.init_ScrDimmer();
        }

        // ---------------------------------------------------------------------------------------------------------------
    }
};
