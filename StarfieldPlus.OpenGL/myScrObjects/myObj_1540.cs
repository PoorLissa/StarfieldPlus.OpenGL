using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;


/*
    - Invisible static dots + one moving point. The moving point builds connections to invisible dots while it travels around
*/


namespace my
{
    public class myObj_1540 : myObject
    {
        // Priority
        public static int Priority => 10;
        public static System.Type Type => typeof(myObj_1540);

        private int cnt, cnt2;
        private float x, y, dx, dy;
        private float size, A, R, G, B, angle = 0;

        private static int N = 0, n = 0, shape = 0, colorMode = 0;
        private static bool doFillShapes = false, doDestroy = false, doDraw2ndLayer = false;
        private static float dimAlpha = 0.05f;

        private static myScreenGradient grad = null;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_1540()
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
                doDraw2ndLayer = myUtils.randomBool(rand);

                n = 11;
                N = 25000;

                if (doDraw2ndLayer)
                {
                    N = 5000;
                }

                shape = rand.Next(5);
            }

            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time local initialization
        private void initLocal()
        {
            doClearBuffer = myUtils.randomChance(rand, 1, 2);
            doClearBuffer = true;
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
                            $"colorMode = {colorMode}\n"           +
                            $"doDestroy = {doDestroy}\n"           +
                            $"doDraw2ndLayer = {doDraw2ndLayer}\n" +
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
                size = 3;
                cnt = 50 + rand.Next(111); // used a a dist
            }
            else
            {
                x = rand.Next(gl_Width);
                y = rand.Next(gl_Height);

                dx = dy = 0;

                dx = (0.1f + myUtils.randFloat(rand)) * myUtils.randomSign(rand);
                dy = (0.1f + myUtils.randFloat(rand)) * myUtils.randomSign(rand);

                size = 1;

                A = 0.15f + myUtils.randFloat(rand) * 0.1f;
                colorPicker.getColor(x, y, ref R, ref G, ref B);
                cnt = 333 + rand.Next(999);
                cnt2 = 0;
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

                if (doDestroy && cnt2 > 33)
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
                    var other = list[i] as myObj_1540;

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

                        other.cnt2++;

                        if (doDraw2ndLayer)
                        {
                            for (int j = n; j < Count; j++)
                            {
                                var oth = list[j] as myObj_1540;

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
            }

            float size2x = size * 2;

            switch (shape)
            {
                // Instanced squares
                case 0:
                    myPrimitive._RectangleInst.setInstanceCoords(x - size, y - size, size2x, size2x);
                    myPrimitive._RectangleInst.setInstanceColor(R, G, B, A);
                    myPrimitive._RectangleInst.setInstanceAngle(angle);
                    break;

                // Instanced triangles
                case 1:
                    myPrimitive._TriangleInst.setInstanceCoords(x, y, size, angle);
                    myPrimitive._TriangleInst.setInstanceColor(R, G, B, A);
                    break;

                // Instanced circles
                case 2:
                    myPrimitive._EllipseInst.setInstanceCoords(x, y, size2x, angle);
                    myPrimitive._EllipseInst.setInstanceColor(R, G, B, A);
                    break;

                // Instanced pentagons
                case 3:
                    myPrimitive._PentagonInst.setInstanceCoords(x, y, size2x, angle);
                    myPrimitive._PentagonInst.setInstanceColor(R, G, B, A);
                    break;

                // Instanced hexagons
                case 4:
                    myPrimitive._HexagonInst.setInstanceCoords(x, y, size2x, angle);
                    myPrimitive._HexagonInst.setInstanceColor(R, G, B, A);
                    break;
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Process(Window window)
        {
            uint cnt = 0;
            initShapes();

            clearScreenSetup(doClearBuffer, 0.1f);

/*
            if (false)
            {
                while (list.Count < N)
                    list.Add(new myObj_1540());
            }
*/
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
                    inst.ResetBuffer();
                    myPrimitive._LineInst.ResetBuffer();

                    for (int i = 0; i != Count; i++)
                    {
                        var obj = list[i] as myObj_1540;

                        obj.Show();
                        obj.Move();
                    }

                    myPrimitive._LineInst.Draw();

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
                    list.Add(new myObj_1540());
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
            base.initShapes(shape, N, 0);

            grad = new myScreenGradient();
            grad.SetRandomColors(rand, 0.2f);

            myPrimitive.init_LineInst(n * 10000);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------
    }
};
