using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;


/*
    - Grid-based 'blur' on a texture
*/


namespace my
{
    public class myObj_1530 : myObject
    {
        // Priority
        public static int Priority => 10;
		public static System.Type Type => typeof(myObj_1530);

        private int x, y, xTarget, yTarget, lifeCnt;

        private static int N = 0, size = 25, gap = 1, mode = 0, maxDist = 0;

        static myTexRectangle tex = null;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_1530()
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
                gap = rand.Next(3) + 1;

                switch (rand.Next(3))
                {
                    case 0:
                        size = 20 + rand.Next(5);
                        N = rand.Next(10) + 15;
                        break;

                    case 1:
                        size = 10 + rand.Next(10);
                        N = rand.Next(20) + 35;
                        break;

                    case 2:
                        size = 5 + rand.Next(5);
                        N = rand.Next(30) + 75;
                        break;
                }
            }

            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time local initialization
        private void initLocal()
        {
            doClearBuffer = false;

            mode = rand.Next(4);
            maxDist = rand.Next(2) + 2;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            height = 600;

            string str = $"Obj = {Type}\n\n"                    +
                            myUtils.strCountOf(list.Count, N)   +
                            $"mode = {mode}\n"                  +
                            $"size = {size}\n"                  +
                            $"gap = {gap}\n"                    +
                            $"maxDist = {maxDist}\n"            +
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

            x -= x % (size + gap);
            y -= y % (size + gap);

            xTarget = x + myUtils.randomSign(rand) * rand.Next(size * maxDist);
            yTarget = y + myUtils.randomSign(rand) * rand.Next(size * maxDist);

            xTarget -= xTarget % (size + gap);
            yTarget -= yTarget % (size + gap);

            if (xTarget < 0 || xTarget > gl_Width)
                xTarget = x;

            if (yTarget < 0 || yTarget > gl_Height)
                yTarget = y;

            lifeCnt = 10 + rand.Next(11);

            switch (mode)
            {
                case 0:
                case 1:
                    break;

                case 2:
                    xTarget = x;
                    break;

                case 3:
                    yTarget = y;
                    break;
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            if (--lifeCnt <= 0)
            {
                generateNew();
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Show()
        {
            tex.setOpacity(1);
            tex.Draw(x, y, size, size, xTarget, yTarget, size, size);
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
/*
                    if (doClearBuffer)
                    {
                        glClear(GL_COLOR_BUFFER_BIT);
                        grad.Draw();
                    }
                    else
                    {
                        dimScreen(dimAlpha);
                    }
*/
                }

                // Render Frame
                {
                    for (int i = 0; i != Count; i++)
                    {
                        var obj = list[i] as myObj_1530;

                        obj.Show();
                        obj.Move();
                    }
                }

                if (Count < N)
                {
                    list.Add(new myObj_1530());
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

            tex = new myTexRectangle(colorPicker.getImg());

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------
    }
};
