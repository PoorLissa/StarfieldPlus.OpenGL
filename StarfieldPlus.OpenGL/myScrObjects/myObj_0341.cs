using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;
using System.Xml.Linq;
using System.Windows.Forms;


/*
    - Grid of rhombuses
*/


namespace my
{
    public class myObj_0341 : myObject
    {
        // Priority
        public static int Priority => 10;
		public static System.Type Type => typeof(myObj_0341);

        private bool doFill;
        private int cnt;
        private float x, y;
        private float A, R, G, B;

        private static int N = 0, offset = 0, fillMode = 0, xyMode = 0;
        private static float dimAlpha = 0.05f;
        private static float sizex = 0, sizey = 0;

        private static myScreenGradient grad = null;
        private static Polygon4 p4 = null;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_0341()
        {
            if (id != uint.MaxValue)
                generateNew();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time global initialization
        protected override void initGlobal()
        {
            var colorMode = myUtils.randomChance(rand, 1, 3)
                ? myColorPicker.colorMode.RANDOM_MODE
                : myColorPicker.colorMode.SNAPSHOT_OR_IMAGE;

            colorPicker = new myColorPicker(gl_Width, gl_Height, colorMode);
            list = new List<myObject>();

            // Global unmutable constants
            {
                N = 3000;
            }

            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time local initialization
        private void initLocal()
        {
            doClearBuffer = myUtils.randomBool(rand);

            xyMode = rand.Next(20);
            fillMode = rand.Next(3);

            offset = myUtils.randomChance(rand, 2, 3)
                ? rand.Next(4)
                : rand.Next(7) - 3;

            switch (rand.Next(3))
            {
                case 0:
                case 1:
                    sizex = sizey = myUtils.randomChance(rand, 1, 2)
                        ? rand.Next(30) + 10
                        : rand.Next(60) + 10;
                    break;

                case 2:
                    sizex = rand.Next(60) + 10;
                    sizey = (int)(2 * sizex / 3);
                    break;
            }
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            height = 600;

            string str = $"Obj = {Type}\n\n"                         +
                            myUtils.strCountOf(list.Count, N)        +
                            $"doClearBuffer = {doClearBuffer}\n"     +
                            $"sizex = {sizex}\n"                     +
                            $"sizeн = {sizey}\n"                     +
                            $"xyMode = {xyMode}\n"                   +
                            $"filMode = {fillMode}\n"                +
                            $"offset = {offset}\n"                   +
                            $"colorMode = {colorPicker.getMode()}\n" +
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
            int X = rand.Next(gl_Width  + 300);
            int Y = rand.Next(gl_Height + 300);

            switch (xyMode)
            {
                // No grid, random distribution
                case 0:
                    break;

                // Grid with gaps
                case 1:
                case 2:
                    {
                        X -= X % (2 * (int)sizex);
                        Y -= Y % (2 * (int)sizey);
                    }
                    break;

                // Grid with overlap
                case 3:
                case 4:
                case 5:
                    {
                        X -= X % ((int)sizex);
                        Y -= Y % ((int)sizey);
                    }
                    break;

                // Normal grid
                default:
                    {
                        X -= X % (int)(1 * (sizex));
                        Y -= Y % (int)(2 * (sizey));

                        if ((X / (int)(sizex)) % 2 != 0)
                        {
                            Y -= (int)sizey;
                        }
                    }
                    break;
            }

            x = X;
            y = Y;

            cnt = 11 + rand.Next(33);

            switch (fillMode)
            {
                case 0:
                    doFill = false;
                    break;

                case 1:
                    doFill = true;
                    break;

                case 2:
                    doFill = myUtils.randomBool(rand);
                    break;
            }

            A = 0.5f + myUtils.randFloat(rand) * 0.25f;
            colorPicker.getColor(x, y, ref R, ref G, ref B);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            if (--cnt < 1)
            {
                if (doClearBuffer)
                {
                    A -= 0.0025f;
                }
                else
                {
                    A = -1;
                }

                if (A < 0)
                {
                    generateNew();
                }
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Show()
        {
            float x1 = x - sizex + offset;
            float y1 = y;
            float x2 = x;
            float y2 = y - sizey + offset;
            float x3 = x;
            float y3 = y + sizey - offset;
            float x4 = x + sizex - offset;
            float y4 = y;

            p4.SetColor(R, G, B, A);
            p4.Draw(x1, y1, x2, y2, x3, y3, x4, y4, doFill);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Process(Window window)
        {
            uint cnt = 0;
            initShapes();

            clearScreenSetup(doClearBuffer, 0.1f);

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
                    for (int i = 0; i != Count; i++)
                    {
                        var obj = list[i] as myObj_0341;

                        obj.Show();
                        obj.Move();
                    }
                }

                if (Count < N)
                {
                    list.Add(new myObj_0341());
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

            grad = new myScreenGradient();
            grad.SetRandomColors(rand, 0.2f);

            p4 = new Polygon4();

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------
    }
};
