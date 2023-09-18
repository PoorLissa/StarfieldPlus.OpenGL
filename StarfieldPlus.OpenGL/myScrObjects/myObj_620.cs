using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;


/*
    - rectangles, where lenght/height are changing constantly; while lenght is increasing, height is decreasing
*/


namespace my
{
    public class myObj_620 : myObject
    {
        // Priority
        public static int Priority => 99910;

        private float x, y, width, height, dx, dy, dWidth, dHeight;
        private float A, R, G, B;

        private static int N = 0, mode = 0, max = 666;
        private static bool doFillShapes = false;
        private static float dimAlpha = 0.05f;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_620()
        {
            if (id != uint.MaxValue)
                generateNew();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time global initialization
        protected override void initGlobal()
        {
            colorPicker = new myColorPicker(gl_Width, gl_Height);
            list = new List<myObject>();

            // Global unmutable constants
            {
                N = rand.Next(10) + 10;
                N = 100;
            }

            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time local initialization
        private void initLocal()
        {
            doClearBuffer = myUtils.randomBool(rand);
            doFillShapes  = myUtils.randomChance(rand, 1, 3);

            renderDelay = rand.Next(11) + 3;

            mode = 0;

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            height = 600;

            string nStr(int   n) { return n.ToString("N0");    }
            //string fStr(float f) { return f.ToString("0.000"); }

            string str = $"Obj = myObj_620\n\n"                       +
                            $"N = {nStr(list.Count)} of {nStr(N)}\n"  +
                            $"renderDelay = {renderDelay}\n"          +
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

            width  = 100 + rand.Next(max);
            height = 100 + rand.Next(max);

            dx = 0;
            dy = 0;

            switch (mode)
            {
                // dWidth/dHeight are the same
                case 0:
                    dWidth  = myUtils.randFloat(rand, 0.1f);
                    dHeight = dWidth;
                    break;

                // dWidth/dHeight are different
                case 1:
                    dWidth  = myUtils.randFloat(rand, 0.1f);
                    dHeight = myUtils.randFloat(rand, 0.1f);
                    break;
            }

            switch (rand.Next(2))
            {
                case 0:
                    dWidth *= -1;
                    break;

                case 1:
                    dHeight *= -1;
                    break;
            }

            A = 1;
            colorPicker.getColor(x, y, ref R, ref G, ref B);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            x += dx;
            y += dy;

            width  += dWidth;
            height += dHeight;

            if (x < 0 || x > gl_Width || y < 0 || y > gl_Height)
            {
                generateNew();
            }

            if (width < 1 || height < 1)
            {
                dWidth  *= -1;
                dHeight *= -1;
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Show()
        {
            var rectInst = inst as myRectangleInst;

            rectInst.setInstanceCoords(x - width, y - height, 2 * width, 2 * height);
            rectInst.setInstanceColor(R, G, B, A);
            rectInst.setInstanceAngle(0);

            return;
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
                    inst.ResetBuffer();

                    for (int i = 0; i != Count; i++)
                    {
                        var obj = list[i] as myObj_620;

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
                    list.Add(new myObj_620());
                }

                cnt++;
                System.Threading.Thread.Sleep(renderDelay);
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void initShapes()
        {
            myPrimitive.init_ScrDimmer();
            base.initShapes(0, N, 0);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------
    }
};
