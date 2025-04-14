using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;
using System.Windows.Forms;


/*
    - 
*/


namespace my
{
    public class myObj_1310 : myObject
    {
        // Priority
        public static int Priority => 10;
		public static System.Type Type => typeof(myObj_1310);

        private int cnt;
        private float x, y, dx, dy;
        private float a, b, c;
        private float size, A, R, G, B, angle = 0;

        private static int N = 0, n = 0, gridStep = 1,  shape = 0, mode = 0, placeMode = 0, moveMode = 0, opacityMode = 0;
        private static bool doFillShapes = false;
        private static float dimAlpha = 0.05f;

        private static myScreenGradient grad = null;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_1310()
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
                N = rand.Next(30000) + 30000;
                n = 3 + rand.Next(11);

                shape = rand.Next(5);
            }

            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time local initialization
        private void initLocal()
        {
            doClearBuffer = true;
            doFillShapes = myUtils.randomChance(rand, 1, 2);

            gridStep = 5 + rand.Next(16);

            mode = rand.Next(4);
            moveMode = rand.Next(2);
            placeMode = rand.Next(2);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            height = 600;

            string str = $"Obj = {Type}\n\n"                  +
                            myUtils.strCountOf(list.Count, N) +
                            $"n = {n}\n"                      +
                            $"gridStep = {gridStep}\n"        +
                            $"moveMode = {moveMode}\n"        +
                            $"placeMode = {placeMode}\n"      +
                            $"opacityMode = {opacityMode}\n"  +
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

            switch (placeMode)
            {
                case 1:
                    x -= x % gridStep;
                    y -= y % gridStep;
                    break;
            }

            if (id < n)
            {
                switch (mode)
                {
                    // Circular shapes
                    case 0:
                        {
                            dx = myUtils.randFloatSigned(rand, 0.2f) * 5;
                            dy = myUtils.randFloatSigned(rand, 0.2f) * 5;

                            size = 1;

                            R = G = B = A = 0;

                        }
                        break;

                    case 1:
                        {
                            switch (rand.Next(2))
                            {
                                case 0:
                                    x = 0;
                                    dx = myUtils.randFloat(rand, 0.2f) * +5;
                                    break;

                                case 1:
                                    x = gl_Width;
                                    dx = myUtils.randFloat(rand, 0.2f) * -5;
                                    break;
                            }
                        }
                        break;

                    case 2:
                        {
                            switch (rand.Next(2))
                            {
                                case 0:
                                    y = 0;
                                    dy = myUtils.randFloat(rand, 0.2f) * +5;
                                    break;

                                case 1:
                                    y = gl_Height;
                                    dy = myUtils.randFloat(rand, 0.2f) * -5;
                                    break;
                            }
                        }
                        break;

                    case 3:
                        {
                            switch (rand.Next(4))
                            {
                                case 0:
                                    x = y = 0;
                                    dx = myUtils.randFloat(rand, 0.2f) * +5;
                                    dy = 0;
                                    break;

                                case 1:
                                    x = y = gl_Width;
                                    dx = myUtils.randFloat(rand, 0.2f) * -5;
                                    dy = 0;
                                    break;

                                case 2:
                                    x = y = 0;
                                    dx = 0;
                                    dy = myUtils.randFloat(rand, 0.2f) * +5;
                                    break;

                                case 3:
                                    x = y = gl_Height;
                                    dx = 0;
                                    dy = myUtils.randFloat(rand, 0.2f) * -5;
                                    break;
                            }
                        }
                        break;

                    case 11:
                        {
                            a = 7;
                            b = -3;
                            c = 5000;
                        }
                        break;
                }
            }
            else
            {
                cnt = 222 + rand.Next(333);
                size = rand.Next(3) + 3;
                size = 2;
                angle = myUtils.randFloat(rand) * 321;

                A = 0.1f;
                colorPicker.getColor(x, y, ref R, ref G, ref B);
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            switch (mode)
            {
                case 0: move_0(); break;
                case 1: move_1(); break;
                case 2: move_2(); break;
                case 3: move_3(); break;
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Show()
        {
            if (id >= n)
            {
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
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Process(Window window)
        {
            uint cnt = 0;
            initShapes();

            clearScreenSetup(doClearBuffer, 0.1f);

            while (list.Count < N / 2)
            {
                list.Add(new myObj_1310());
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

                    for (int i = 0; i != Count; i++)
                    {
                        var obj = list[i] as myObj_1310;

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
                    list.Add(new myObj_1310());
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

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void move_0()
        {
            if (id < n)
            {
                x += dx;
                y += dy;

                int maxDist = 333;

                for (int i = n; i < list.Count; i++)
                {
                    var other = list[i] as myObj_1310;

                    var dX = x - other.x;
                    var dY = y - other.y;

                    var dist = Math.Sqrt(dX * dX + dY * dY);

                    switch (opacityMode)
                    {
                        case 0:
                            {
                                if (dist > maxDist)
                                {
                                    other.A = other.A > 0.1f ? other.A : 0.1f;
                                }
                                else
                                {
                                    other.A = (float)((1.0 * maxDist - dist) / maxDist);
                                }
                            }
                            break;

                        case 1:
                            {
                                if (dist > maxDist)
                                {
                                    other.A -= (other.A > 0.1f) ? 0.003f : 0;
                                }
                                else
                                {
                                    //other.A = (float)((1.0 * maxDist - dist) / maxDist);

                                    other.A += 0.03f * (float)((1.0 * maxDist - dist) / maxDist);
                                }
                            }
                            break;
                    }
                }

                switch (moveMode)
                {
                    case 0:
                        {
                            if (x < 0 && dx < 0)
                                dx *= -1;

                            if (y < 0 && dy < 0)
                                dy *= -1;

                            if (x > gl_Width && dx > 0)
                                dx *= -1;

                            if (y > gl_Height && dy > 0)
                                dy *= -1;
                        }
                        break;

                    case 1:
                        {
                            if (x < 0)
                                dx += 0.023f;

                            if (y < 0)
                                dy += 0.023f;

                            if (x > gl_Width)
                                dx -= 0.023f;

                            if (y > gl_Height)
                                dy -= 0.023f;
                        }
                        break;
                }
            }
            else
            {
                if (--cnt < 0)
                {
                    generateNew();
                }
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void move_1()
        {
            if (id < n)
            {
                x += dx;
                y += dy;

                int maxDist = 25;

                for (int i = n; i < list.Count; i++)
                {
                    var other = list[i] as myObj_1310;
                    var dist = (float)Math.Abs(x - other.x);

                    switch (opacityMode)
                    {
                        case 0:
                            {
                                if (dist > maxDist)
                                {
                                    other.A = other.A > 0.1f ? other.A : 0.1f;
                                }
                                else
                                {
                                    other.A = (float)((1.0 * maxDist - dist) / maxDist);
                                }
                            }
                            break;

                        case 1:
                            {
                                if (dist > maxDist)
                                {
                                    other.A -= (other.A > 0.1f) ? 0.003f : 0;
                                }
                                else
                                {
                                    other.A += 0.03f * (float)((1.0 * maxDist - dist) / maxDist);
                                }
                            }
                            break;
                    }
                }

                if (x < -100 || x > gl_Width + 100)
                    generateNew();
            }
            else
            {
                if (--cnt < 0)
                {
                    generateNew();
                }
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void move_2()
        {
            if (id < n)
            {
                x += dx;
                y += dy;

                int maxDist = 25;

                for (int i = n; i < list.Count; i++)
                {
                    var other = list[i] as myObj_1310;
                    var dist = (float)Math.Abs(y - other.y);

                    switch (opacityMode)
                    {
                        case 0:
                            {
                                if (dist > maxDist)
                                {
                                    other.A = other.A > 0.1f ? other.A : 0.1f;
                                }
                                else
                                {
                                    other.A = (float)((1.0 * maxDist - dist) / maxDist);
                                }
                            }
                            break;

                        case 1:
                            {
                                if (dist > maxDist)
                                {
                                    other.A -= (other.A > 0.1f) ? 0.003f : 0;
                                }
                                else
                                {
                                    other.A += 0.03f * (float)((1.0 * maxDist - dist) / maxDist);
                                }
                            }
                            break;
                    }
                }

                if (x < -100 || x > gl_Width + 100)
                    generateNew();
            }
            else
            {
                if (--cnt < 0)
                {
                    generateNew();
                }
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void move_3()
        {
            if (id < n)
            {
                x += dx;
                y += dy;

                int maxDist = 25;

                for (int i = n; i < list.Count; i++)
                {
                    var other = list[i] as myObj_1310;
                    var dist = dy == 0
                        ? (float)Math.Abs(x - other.x)
                        : (float)Math.Abs(y - other.y);

                    switch (opacityMode)
                    {
                        case 0:
                            {
                                if (dist > maxDist)
                                {
                                    other.A = other.A > 0.1f ? other.A : 0.1f;
                                }
                                else
                                {
                                    other.A = (float)((1.0 * maxDist - dist) / maxDist);
                                }
                            }
                            break;

                        case 1:
                            {
                                if (dist > maxDist)
                                {
                                    other.A -= (other.A > 0.1f) ? 0.003f : 0;
                                }
                                else
                                {
                                    other.A += 0.03f * (float)((1.0 * maxDist - dist) / maxDist);
                                }
                            }
                            break;
                    }
                }

                if (x < -100 || x > gl_Width + 100)
                    generateNew();

                if (y < -100 || y > gl_Height + 100)
                    generateNew();
            }
            else
            {
                if (--cnt < 0)
                {
                    generateNew();
                }
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

    }
};
