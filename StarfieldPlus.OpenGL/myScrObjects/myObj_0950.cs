using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;
using System.Drawing;


/*
    - Very narrow window from a texture stretched to full-screen
*/


namespace my
{
    public class myObj_0950 : myObject
    {
        // Priority
        public static int Priority => 11;
		public static System.Type Type => typeof(myObj_0950);

        private static int size;
        private static float x, y, dx, dy;

        private static int N = 0, mode = 0, dirMode = 0, rectOffset1 = 0, rectOffset2 = 0;
        private static bool doShiftImg = false, doFillShapes = false;
        private static float dimAlpha = 0.05f;

        private static myTexRectangle tex = null;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_0950()
        {
            if (id != uint.MaxValue)
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
                N = 1;

                size = rand.Next(10) + 1;

                mode = rand.Next(3);        // Drawing : Stretch vs Repeat vs InstRectangles
                dirMode = rand.Next(3);     // Movement: Vertical, Horizontal or Both

                doShiftImg = myUtils.randomBool(rand);
                doFillShapes = myUtils.randomBool(rand);

                rectOffset1 = rand.Next(10);
                rectOffset2 = rectOffset1 * 2;
            }

            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time local initialization
        private void initLocal()
        {
            doClearBuffer = false;

            renderDelay = rand.Next(3) + 3;

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
                            $"mode = {mode}\n"                       +
                            $"dirMode = {dirMode}\n"                 +
                            $"size = {size}\n"                       +
                            $"dx = {fStr(dx)}\n"                     +
                            $"doShiftImg = {doShiftImg}\n"           +
                            $"doFillShapes = {doFillShapes}\n"       +
                            $"rectOffset1 = {rectOffset1}\n"         +
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
            x = 0;
            y = 0;

            switch (dirMode)
            {
                case 0:
                    dx = myUtils.randFloat(rand);
                    dx = 0.25f;
                    dy = 0;
                    break;

                case 1:
                    dx = 0;
                    dy = 0.25f;
                    break;

                case 2:
                    dx = 0.25f;
                    dy = 0.25f;
                    break;
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            x += dx;
            y += dy;

            if (x < 0 || x > gl_Width || y < 0 || y > gl_Height)
            {
                generateNew();
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Show()
        {
            int X = (int)x;
            int Y = (int)y;

            switch (dirMode)
            {
                // Horizontal movement
                case 0:
                    {
                        // Slightly shit Y coordinate up and down
                        if (doShiftImg)
                        {
                            Y = (int)(Math.Sin(x * 0.1) * 10);
                        }

                        switch (mode)
                        {
                            // Stretch
                            case 0:
                                {
                                    tex.Draw(0, Y, gl_Width, gl_Height, X, 0, size, gl_Height);
                                }
                                break;

                            // Repeat
                            case 1:
                                {
                                    for (int i = 0; i < gl_Width; i += size)
                                    {
                                        tex.Draw(i, Y, size, gl_Height, X, 0, size, gl_Height);
                                    }
                                }
                                break;

                            // Inst rectangles
                            case 2:
                                {
                                    float r = 0, g = 0, b = 0;

                                    for (int i = 0; i < gl_Height; i += size)
                                    {
                                        colorPicker.getColorAverage(X, i, size, size, ref r, ref g, ref b);

                                        myPrimitive._RectangleInst.setInstanceCoords(0, i - rectOffset1, gl_Width, size + rectOffset2);
                                        myPrimitive._RectangleInst.setInstanceColor(r, g, b, 0.1f);
                                        myPrimitive._RectangleInst.setInstanceAngle(0);
                                    }
                                }
                                break;
                        }
                    }
                    break;

                // Vertical movement
                case 1:
                    {
                        // Slightly shit X coordinate up and down
                        if (doShiftImg)
                        {
                            X = (int)(Math.Sin(y * 0.1) * 10);
                        }

                        switch (mode)
                        {
                            // Stretch
                            case 0:
                                {
                                    tex.Draw(X, 0, gl_Width, gl_Height, 0, Y, gl_Width, size);
                                }
                                break;

                            // Repeat
                            case 1:
                                {
                                    for (int i = 0; i < gl_Height; i += size)
                                    {
                                        tex.Draw(X, i, gl_Width, size, 0, X, gl_Width, size);
                                    }
                                }
                                break;

                            // Inst rectangles
                            case 2:
                                {
                                    float r = 0, g = 0, b = 0;

                                    for (int i = 0; i < gl_Width; i += size)
                                    {
                                        colorPicker.getColorAverage(i, Y, size, size, ref r, ref g, ref b);

                                        myPrimitive._RectangleInst.setInstanceCoords(i - rectOffset1, 0, size + rectOffset2, gl_Height);
                                        myPrimitive._RectangleInst.setInstanceColor(r, g, b, 0.1f);
                                        myPrimitive._RectangleInst.setInstanceAngle(0);
                                    }
                                }
                                break;
                        }
                    }
                    break;

                // Vertical + Horizontal movements
                case 2:
                    {
                        switch (mode)
                        {
                            // Stretch
                            case 0:
                                {
                                    tex.Draw(0, 0, gl_Width, gl_Height, X, 0, size, gl_Height);
                                    tex.Draw(0, 0, gl_Width, gl_Height, 0, Y, gl_Width, size);
                                }
                                break;

                            // Repeat
                            case 1:
                                {
                                    for (int i = 0; i < gl_Width; i += size)
                                    {
                                        tex.Draw(i, 0, size, gl_Height, X, 0, size, gl_Height);
                                    }

                                    for (int i = 0; i < gl_Height; i += size)
                                    {
                                        tex.Draw(0, i, gl_Width, size, 0, Y, gl_Width, size);
                                    }
                                }
                                break;

                            // Inst rectangles
                            case 2:
                                {
                                    float r = 0, g = 0, b = 0;

                                    for (int i = 0; i < gl_Height; i += size)
                                    {
                                        colorPicker.getColorAverage(X, i, size, size, ref r, ref g, ref b);

                                        myPrimitive._RectangleInst.setInstanceCoords(0, i - rectOffset1, gl_Width, size + rectOffset2);
                                        myPrimitive._RectangleInst.setInstanceColor(r, g, b, 0.1f);
                                        myPrimitive._RectangleInst.setInstanceAngle(0);
                                    }

                                    inst.Draw(doFillShapes);
                                    inst.ResetBuffer();

                                    for (int i = 0; i < gl_Width; i += size)
                                    {
                                        colorPicker.getColorAverage(i, Y, size, size, ref r, ref g, ref b);

                                        myPrimitive._RectangleInst.setInstanceCoords(i - rectOffset1, 0, size + rectOffset2, gl_Height);
                                        myPrimitive._RectangleInst.setInstanceColor(r, g, b, 0.1f);
                                        myPrimitive._RectangleInst.setInstanceAngle(0);
                                    }
                                }
                                break;
                        }
                    }
                    break;
            }
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
                if (false)
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
                    inst.ResetBuffer();

                    for (int i = 0; i != Count; i++)
                    {
                        var obj = list[i] as myObj_0950;

                        obj.Show();
                        obj.Move();
                    }

                    inst.Draw(doFillShapes);
                }

                if (Count < N)
                {
                    list.Add(new myObj_0950());
                }

                cnt++;
                System.Threading.Thread.Sleep(renderDelay);
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void initShapes()
        {
            base.initShapes(0, gl_Width / size + 1, 0);

            tex = new myTexRectangle(colorPicker.getImg());

            tex.setOpacity(0.1f);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------
    }
};
