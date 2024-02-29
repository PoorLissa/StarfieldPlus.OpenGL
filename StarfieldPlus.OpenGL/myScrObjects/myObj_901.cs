using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;
using static System.Net.Mime.MediaTypeNames;


/*
    - Waveforms moving sideways, v2
*/


namespace my
{
    public class myObj_901 : myObject
    {
        // Priority
        public static int Priority => 999910;
		public static System.Type Type => typeof(myObj_901);

        private int cnt, ptr;
        private float x, y, dx, dy;
        private float size, A, R, G, B, angle = 0, dAngle = 0;

        private static int N = 0, n = 0, DX = 1, cntMax = 100, shape = 0, dyMode = 0, dyGenerateMode = 0;
        private static bool doFillShapes = false;
        private static float dimAlpha = 0.05f, lineA = 1, lineWidth = 1, speedFactor = 1, t = 0, dt = 0;

        private static int cellSize = 1, cellX = 0, cellY = 0;

        private static Polygon4 p4 = null;
        private static myScreenGradient grad = null;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_901()
        {
            ptr = -1;

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
                dt = 0.001f;
                DX = 20 + rand.Next(66);

                switch (rand.Next(3))
                {
                    case 0: n = 3 + rand.Next( 3); break;
                    case 1: n = 3 + rand.Next( 7); break;
                    case 2: n = 3 + rand.Next(12); break;
                }

                N = n + (3 + gl_Width / DX) * n;

                shape = rand.Next(5);

                lineA = 0.5f;
                lineWidth = 3.0f + rand.Next(7);

                speedFactor = 2.0f + myUtils.randFloat(rand) * rand.Next(7);

                // Grid setup
                cellSize = 50 + rand.Next(150);
                cellX = (gl_Width  % cellSize) / 2;
                cellY = (gl_Height % cellSize) / 2;
            }

            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time local initialization
        private void initLocal()
        {
            doClearBuffer = myUtils.randomChance(rand, 20, 21);
            doFillShapes = myUtils.randomChance(rand, 1, 2);

            dyGenerateMode = rand.Next(3);

            renderDelay = rand.Next(2);

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
                            $"n = {nStr(n)}\n"                       +
                            $"cntMax = {cntMax}\n"                   +
                            $"DX = {DX}\n"                           +
                            $"shape = {shape}\n"                     +
                            $"dyMode = {dyMode}\n"                   +
                            $"dyGenerateMode = {dyGenerateMode}\n"   +
                            $"lineA = {fStr(lineA)}\n"               +
                            $"lineWidth = {fStr(lineWidth)}\n"       +
                            $"speedFactor = {fStr(speedFactor)}\n"   +
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
            if (id < n)
            {
                ptr = -1;

                cnt = rand.Next(cntMax) + 1;

                x = -50;
                y = rand.Next(gl_Height);

                dx = 0.5f + myUtils.randFloat(rand) * speedFactor;
                dy = myUtils.randFloatSigned(rand);

                R = (float)rand.NextDouble();
                G = (float)rand.NextDouble();
                B = (float)rand.NextDouble();
                A = 0.75f;
            }
            else
            {
                if (ptr >= 0)
                {
                    var parent = list[ptr] as myObj_901;

                    x = parent.x;
                    y = parent.y;
                    dx = parent.dx;

                    A = parent.A;
                    R = parent.R;
                    G = parent.G;
                    B = parent.B;

                    angle = 0;
                    dAngle = myUtils.randFloatSigned(rand) * 0.05f;
                    size = 3;
                }
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            if (id < n)
            {
                y += dy;
                y += (float)Math.Sin(t*10) * 1;

                // Parent objects moving along y-axis
                if (--cnt == 0)
                {
                    cnt = rand.Next(cntMax) + 1;

                    switch (dyGenerateMode)
                    {
                        case 0: dy = myUtils.randFloatSigned(rand); break;
                        case 1: dy += myUtils.randFloatSigned(rand) * 0.1f; break;
                        case 2: y = y + rand.Next(101) - 50; break;
                    }
                }

                if (y < 0)
                    dy += 0.01f;

                if (y > gl_Height)
                    dy -= 0.01f;

                if (ptr < 0)
                {
                    // Generate first child
                    getNextAvailable();

                    if (ptr > 0)
                    {
                        var obj = list[ptr] as myObj_901;
                        obj.ptr = (int)id;
                        obj.generateNew();
                        obj.ptr = 0;
                    }
                }
                else
                {
                    // Already has a child; check on it
                    var child = list[ptr] as myObj_901;

                    if (child.x - x >= DX)
                    {
                        getNextAvailable();

                        if (ptr > 0)
                        {
                            var obj = list[ptr] as myObj_901;

                            obj.ptr = (int)id;
                            obj.generateNew();
                            obj.ptr = (int)child.id;
                        }
                    }
                }
            }
            else if (ptr >= 0)
            {
                {
                    // Child objects moving along x-axis
                    x += dx;
                    angle += dAngle;

                    if (x > gl_Width + DX)
                    {
                        ptr = -1;
                    }
                }
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Show()
        {
            if (ptr >= 0)
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

                // Connection lines and the area filled with color
                {
                    var next = list[ptr] as myObj_901;

                    if (x < next.x)
                    {
                        if (dyGenerateMode == 2)
                        {
                            myPrimitive._LineInst.setInstance(x, y, next.x, y, R, G, B, lineA);
                            p4.SetColor(R, G, B, 0.3f);
                            p4.Draw(x, y, next.x, y, x, gl_Height, next.x, gl_Height, true);
                        }
                        else
                        {
                            myPrimitive._LineInst.setInstance(x, y, next.x, next.y, R, G, B, lineA);
                            p4.SetColor(R, G, B, 0.1f);
                            p4.Draw(x, y, next.x, next.y, x, gl_Height, next.x, gl_Height, true);
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

            if (list.Count < N/3)
            {
                list.Add(new myObj_901());
            }

            while (!Glfw.WindowShouldClose(window))
            {
                int Count = list.Count;

                processInput(window);

                // Swap fore/back framebuffers, and poll for operating system events.
                Glfw.SwapBuffers(window);
                Glfw.PollEvents();

                // Dim screen
                {
                    grad.Draw();
                }

                // Render Frame
                {
                    showGrid();

                    inst.ResetBuffer();
                    myPrimitive._LineInst.ResetBuffer();

                    for (int i = 0; i != Count; i++)
                    {
                        var obj = list[i] as myObj_901;

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

                    myPrimitive._LineInst.Draw();
                }

                if (Count < N)
                {
                    list.Add(new myObj_901());
                }

                cnt++;
                t += dt;
                System.Threading.Thread.Sleep(renderDelay);
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void initShapes()
        {
            base.initShapes(shape, N, 0);

            grad = new myScreenGradient();
            grad.SetRandomColors(rand, 0.23f);

            if (doClearBuffer == false)
            {
                grad.SetOpacity(0.01f);
            }

            p4 = new Polygon4();

            int gridN = gl_Width / cellSize + gl_Height / cellSize + 4;

            myPrimitive.init_LineInst(gridN > N ? gridN : N);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void showGrid()
        {
            myPrimitive._LineInst.ResetBuffer();
            myPrimitive._LineInst.setLineWidth(1);

            for (int i = cellX; i < gl_Width; i += cellSize)
            {
                myPrimitive._LineInst.setInstance(i, 0, i, gl_Height, 1, 1, 1, 0.1f);
            }

            for (int i = cellY; i < gl_Height; i += cellSize)
            {
                myPrimitive._LineInst.setInstance(0, i, gl_Width, i, 1, 1, 1, 0.1f);
            }

            myPrimitive._LineInst.Draw();
            myPrimitive._LineInst.setLineWidth(lineWidth);
        }

        // ---------------------------------------------------------------------------------------------------------------

        // Find an unused object
        private void getNextAvailable()
        {
            for (int i = n; i < list.Count; i++)
            {
                var obj = list[i] as myObj_901;

                if (obj.ptr < 0)
                {
                    ptr = (int)obj.id;
                    break;
                }
            }
        }

        // ---------------------------------------------------------------------------------------------------------------
    }
};
