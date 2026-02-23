using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;


/*
    - 
*/


namespace my
{
    public class myObj_1390 : myObject
    {
        // Priority
        public static int Priority => 10;
		public static System.Type Type => typeof(myObj_1390);

        private float x, y, y0, h;
        private float size, A, R, G, B, angle = 0;

        private static int N = 0, shape = 0, rad = 0, curvMode = 0, moveMode = 0;
        private static bool doFillShapes = false;
        private static float dimAlpha = 0.05f, t = 0, dt = 0, r0 = 0, g0 = 0, b0 = 0;
        private static float maxSize = 5;
        private static float rFactor = 0, gFactor = 0, bFactor = 0;

        private static myScreenGradient grad = null;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_1390()
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
                N = 200;

                do
                {
                    shape = rand.Next(5);
                }
                while (shape == 1);

                maxSize = gl_Width / N / 2 - 2;
                maxSize = 5;

                curvMode = rand.Next(13);
                moveMode = rand.Next(5);

                rFactor = 0.5f + myUtils.randFloat(rand) * 2;
                gFactor = 0.5f + myUtils.randFloat(rand) * 2;
                bFactor = 0.5f + myUtils.randFloat(rand) * 2;

                myUtils.getRandomColor(rand, ref r0, ref b0, ref g0, 0.33f);
            }

            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time local initialization
        private void initLocal()
        {
            doClearBuffer = myUtils.randomChance(rand, 100, 101);
            doFillShapes = true;

            rad = 75;

            dt = 0.005f + myUtils.randFloat(rand) * 0.01f;

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            height = 600;

            string str = $"Obj = {Type}\n\n"                  +
                            myUtils.strCountOf(list.Count, N) +
                            $"moveMode = {moveMode}\n"        +
                            $"curvMode = {curvMode}\n"        +
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
            x = id * (gl_Width / N);
            h = rad * (float)Math.Sin(x);

            // Global curvature
            switch (curvMode)
            {
                case 0:
                    y0 = gl_y0;
                    break;

                case 1: case 2: case 3: case 4:
                case 5: case 6: case 7: case 8:
                    y0 = gl_y0 + (float)Math.Cos(x * 0.001) * 100;
                    break;

                case 9:
                    y0 = gl_y0 + (float)Math.Cos(x * 0.001 + 0.01 * h) * 100;
                    break;

                case 10:
                    y0 = gl_y0 + (float)Math.Cos(x * 0.001 + myUtils.randFloat(rand) * 0.25) * 123;
                    break;

                case 11:
                    y0 = gl_y0 + (float)Math.Cos(x * 0.001 * Math.Sin(h * 0.01)) * 100;
                    break;

                case 12:
                    y0 = gl_y0 + (float)Math.Cos(x * 0.001 * Math.Sin(33 * h / x)) * 100;
                    break;
            }

            A = 0.85f;
            R = r0;
            G = g0 + 0.0025f * id;
            B = b0;

            //colorPicker.getColor(x, y, ref R, ref G, ref B);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            switch (moveMode)
            {
                case 0:
                    h = (float)(rad * Math.Sin(x + t)) + (float)Math.Sin(id) * 11;
                    break;

                default:
                    h = (float)(rad * Math.Sin(x + t));
                    break;
            }
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Show()
        {
            float y1 = 0, y2 = 0, r = R, g = G, b = B;

            for (int i = 0; i < 2; i++)
            {
                switch (i)
                {
                    case 0:
                        y = y1 = y0 + h;
                        size = maxSize + (float)Math.Sin(x + t + Math.PI / 2) * 3;
                        break;

                    case 1:
                        y = y2 = y0 - h;
                        size = maxSize + (float)Math.Sin(x + t - Math.PI / 2) * 3;
                        r *= rFactor;
                        g *= gFactor;
                        b *= bFactor;
                        break;
                }

                float size2x = size * 2;

                switch (shape)
                {
                    // Instanced squares
                    case 0:
                        myPrimitive._RectangleInst.setInstanceCoords(x - size, y - size, size2x, size2x);
                        myPrimitive._RectangleInst.setInstanceColor(r, g, b, A);
                        myPrimitive._RectangleInst.setInstanceAngle(angle);
                        break;

                    // Instanced circles
                    case 2:
                        myPrimitive._EllipseInst.setInstanceCoords(x, y, size2x, angle);
                        myPrimitive._EllipseInst.setInstanceColor(r, g, b, A);
                        break;

                    // Instanced pentagons
                    case 3:
                        myPrimitive._PentagonInst.setInstanceCoords(x, y, size2x, angle);
                        myPrimitive._PentagonInst.setInstanceColor(r, g, b, A);
                        break;

                    // Instanced hexagons
                    case 4:
                        myPrimitive._HexagonInst.setInstanceCoords(x, y, size2x, angle);
                        myPrimitive._HexagonInst.setInstanceColor(r, g, b, A);
                        break;
                }
            }

            myPrimitive._LineInst.setInstanceCoords(x, y1, x, y2);
            myPrimitive._LineInst.setInstanceColor(1, 1, 1, (float)Math.Abs(h)/rad);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Process(Window window)
        {
            uint cnt = 0;
            initShapes();

            clearScreenSetup(doClearBuffer, 0.1f);

            while (list.Count < N)
            {
                list.Add(new myObj_1390());
            }

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
                        var obj = list[i] as myObj_1390;

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
                    list.Add(new myObj_1390());
                }

                stopwatch.WaitAndRestart();
                cnt++;
                t += dt;
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void initShapes()
        {
            myPrimitive.init_ScrDimmer();
            base.initShapes(shape, N*2, 0);

            myPrimitive.init_LineInst(N);

            grad = new myScreenGradient();
            grad.SetRandomColors(rand, 0.2f);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------
    }
};
