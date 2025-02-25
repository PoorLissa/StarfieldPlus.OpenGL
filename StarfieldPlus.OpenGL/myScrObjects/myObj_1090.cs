using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;
using StarfieldPlus.OpenGL.myUtils;


/*
    - Like Starfield, but instead of flying dots we have flying lines (made of 2 dots with the same angle, but slightly different speed)
*/


namespace my
{
    public class myObj_1090 : myObject
    {
        // Priority
        public static int Priority => 99999910;
		public static System.Type Type => typeof(myObj_1090);

        private int cnt;
        private float x1, y1, dx1, dy1;
        private float x2, y2, dx2, dy2;
        private float size, a, A, R, G, B, angle, dAngle;

        private static int N = 0, shape = 0, colorMode = 0;
        private static bool doFillShapes = false, doAllocateAtOnce = false;
        private static float dimAlpha = 0.05f;

        private static myScreenGradient grad = null;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_1090()
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
                N = rand.Next(5000) + 1000;

                shape = rand.Next(5);
            }

            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time local initialization
        private void initLocal()
        {
            doAllocateAtOnce = myUtils.randomChance(rand, 1, 2);
            doClearBuffer = myUtils.randomChance(rand, 4, 5);

            colorMode = rand.Next(2);

            renderDelay = rand.Next(11) + 3;

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            height = 600;

            string str = $"Obj = {Type}\n\n"                        +
                            myUtils.strCountOf(list.Count, N)       +
                            $"colorMode = {colorMode}\n"            +
                            $"frameRate = {stopwatch.GetRate()}\n"  +
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
            cnt = 33 + rand.Next(33);

            float alpha = myUtils.randFloat(rand) * 321;
            float sin = (float)Math.Sin(alpha);
            float cos = (float)Math.Cos(alpha);

            x1 = x2 = gl_x0;
            y1 = y2 = gl_y0;

            float spd = 3.0f;

            dx1 = myUtils.randFloatSigned(rand, 0.1f) * spd;
            dy1 = myUtils.randFloatSigned(rand, 0.1f) * spd;

            float ratio = 1.0f + myUtils.randFloat(rand, 0.1f) * 0.2f;

            dx2 = dx1 * ratio;
            dy2 = dy1 * ratio;

            x1 += dx1 * 10;
            y1 += dy1 * 10;
            x2 += dx1 * 10;
            y2 += dy1 * 10;

            if (!false)
            {
                int rad = 123;

                x1 = gl_x0 + sin * rad;
                y1 = gl_y0 + cos * rad;

                if (true)
                {
                    x2 = x1;
                    y2 = y1;
                }

                x2 = gl_x0 + sin * (rad + 50);
                y2 = gl_y0 + cos * (rad + 50);

                //x2 = gl_x0 + sin * (rad - 33);
                //y2 = gl_y0 + cos * (rad - 33);

                var localSpd = 1.0f + myUtils.randFloat(rand) * rand.Next(3);

                //localSpd = spd;

                dx1 = sin * localSpd;
                dy1 = cos * localSpd;

                dx2 = dx1 * ratio;
                dy2 = dy1 * ratio;
            }

            size = 3;

            a = 0.1f + myUtils.randFloat(rand) * 0.20f;
            A = 0.1f + myUtils.randFloat(rand) * 0.25f;

            switch (colorMode)
            {
                case 0:
                    colorPicker.getColor(rand.Next(gl_Width), gl_Height, ref R, ref G, ref B);
                    break;

                case 1:
                    R = (float)rand.NextDouble();
                    G = (float)rand.NextDouble();
                    B = (float)rand.NextDouble();
                    break;
            }

            dAngle = myUtils.randFloat(rand) * 0.1f;

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            if (--cnt < 0)
            {
                x1 += dx1;
                y1 += dy1;

                x2 += dx2;
                y2 += dy2;

                angle += dAngle;

                if (x1 < 0 && x2 < 0)
                {
                    generateNew();
                    return;
                }

                if (x1 > gl_Width && x2 > gl_Width)
                {
                    generateNew();
                    return;
                }

                if (y1 < 0 && y2 < 0)
                {
                    generateNew();
                    return;
                }

                if (y1 > gl_Height && y2 > gl_Height)
                {
                    generateNew();
                    return;
                }
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Show()
        {
            float size2x = size * 2;

            void doDraw(float x, float y)
            {
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
                        myPrimitive._TriangleInst.setInstanceCoords(x, y, size2x, angle);
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
            }

            // Draw line
            myPrimitive._LineInst.setInstanceCoords(x1, y1, x2, y2);
            myPrimitive._LineInst.setInstanceColor(R, G, B, a);

            doDraw(x1, y1);
            doDraw(x2, y2);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Process(Window window)
        {
            uint cnt = 0;
            initShapes();

            clearScreenSetup(doClearBuffer, 0.1f);

            while (doAllocateAtOnce && list.Count < N)
            {
                list.Add(new myObj_1090());
            }

            stopwatch = new myStopwatch();
            stopwatch.Start();

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
                        var obj = list[i] as myObj_1090;

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
                    list.Add(new myObj_1090());
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
            base.initShapes(shape, N * 2, 0);

            grad = new myScreenGradient();
            grad.SetRandomColors(rand, 0.2f);

            float lineWidth = 1.0f + myUtils.randFloat(rand) * rand.Next(9);

            myPrimitive.init_LineInst(N);
            myPrimitive._LineInst.setLineWidth(lineWidth);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------
    }
};
