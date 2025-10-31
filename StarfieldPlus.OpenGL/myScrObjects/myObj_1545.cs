using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;


/*
    - Invisible static dots + one moving point. The moving point builds connections to invisible dots while it travels around
*/


namespace my
{
#pragma warning disable 0162
    public class myObj_1545 : myObject
    {
        // Priority
        public static int Priority => 10;
        public static System.Type Type => typeof(myObj_1545);

        private int cnt, lifeCnt;
        private float x, y, dx, dy;
        private float A, R, G, B;

        private static int N = 0, n = 0, colorMode = 0;
        private static bool doDestroy = false;
        private static float dimAlpha = 0.05f;

        private static myScreenGradient grad = null;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_1545()
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
                n = 11;
                N = 10000;
            }

            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time local initialization
        private void initLocal()
        {
            doClearBuffer = false;
            doDestroy = myUtils.randomBool(rand);

            colorMode = rand.Next(2);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            height = 600;

            string str = $"Obj = {Type}\n\n"                       +
                            myUtils.strCountOf(list.Count, N)      +
                            $"doClearBuffer = {doClearBuffer}\n"   +
                            $"colorMode = {colorMode}\n"           +
                            $"doDestroy = {doDestroy}\n"           +
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
            if (id < n)
            {
                x = gl_x0;
                y = gl_y0;

                float spd = 1.5f;

                dx = (0.5f + myUtils.randFloat(rand) * spd) * myUtils.randomSign(rand);
                dy = (0.5f + myUtils.randFloat(rand) * spd) * myUtils.randomSign(rand);

                A = 1;
                R = G = B = 1;
                cnt = 50 + rand.Next(111); // used a a dist
            }
            else
            {
                x = rand.Next(gl_Width);
                y = rand.Next(gl_Height);

                dx = dy = 0;

                dx = (0.1f + myUtils.randFloat(rand)) * myUtils.randomSign(rand);
                dy = (0.1f + myUtils.randFloat(rand)) * myUtils.randomSign(rand);

                A = 0.15f + myUtils.randFloat(rand) * 0.1f;
                colorPicker.getColor(x, y, ref R, ref G, ref B);
                cnt = 333 + rand.Next(999);
                lifeCnt = 33;
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            x += dx;
            y += dy;

            if (id < n)
            {
                if (x < 0)
                    dx += 0.01f;

                if (y < 0)
                    dy += 0.01f;

                if (x > gl_Width)
                    dx -= 0.01f;

                if (y > gl_Height)
                    dy -= 0.01f;
            }
            else
            {
                if (--cnt == 0)
                {
                    generateNew();
                }

                if (doDestroy && lifeCnt == 0)
                {
                    generateNew();
                }

            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Show()
        {
            if (id < n)
            {
                int Count = list.Count;

                for (int i = n; i < Count; i++)
                {
                    var other = list[i] as myObj_1545;

                    float dx = x - other.x;
                    float dy = y - other.y;

                    float dist = (float)Math.Sqrt(dx * dx + dy * dy);

                    if (dist < cnt)
                    {
                        myPrimitive._LineInst.setInstanceCoords(x, y, other.x, other.y);

                        switch (colorMode)
                        {
                            case 0:
                                myPrimitive._LineInst.setInstanceColor(1, 1, 1, 0.05f);
                                break;

                            case 1:
                                myPrimitive._LineInst.setInstanceColor(other.R, other.G, other.B, 0.05f);
                                break;
                        }

                        other.lifeCnt--;

                        for (int j = n; j < Count; j++)
                        {
                            var oth = list[j] as myObj_1545;

                            dx = other.x - oth.x;
                            dy = other.y - oth.y;

                            dist = (float)Math.Sqrt(dx * dx + dy * dy);

                            if (dist < 50)
                            {
                                myPrimitive._LineInst.setInstanceCoords(other.x, other.y, oth.x, oth.y);
                                myPrimitive._LineInst.setInstanceColor(1, 1, 1, 0.05f);
                            }
                        }
                    }
                }
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Process(Window window)
        {
            uint cnt = 0;
            initShapes();

            clearScreenSetup(doClearBuffer, 0.1f);

            if (!false)
                while (list.Count < N)
                    list.Add(new myObj_1545());

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
                        var obj = list[i] as myObj_1545;

                        obj.Show();
                        obj.Move();
                    }

                    myPrimitive._LineInst.Draw();
                }

                if (Count < N)
                {
                    list.Add(new myObj_1545());
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

            myPrimitive.init_LineInst(n * 10000);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------
    }
#pragma warning restore 0162
};
