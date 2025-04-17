using GLFW;
using static OpenGL.GL;
using System.Collections.Generic;


/*
    - Falling squares followed by a trail of smaller squares of darker color
*/


namespace my
{
    public class myObj_1320 : myObject
    {
        // Priority
        public static int Priority => 10;
		public static System.Type Type => typeof(myObj_1320);

        int n, dist;
        private float x, y, dx, dy;
        private float size, A, R, G, B, R0, G0, B0, angle = 0;

        private static int N = 0, shape = 0, nMax = 1;
        private static int colorMode = 0, nMode = 0;
        private static bool doFillShapes = false, doReduceOpacity = false;
        private static float dimAlpha = 0.05f;

        private static myScreenGradient grad = null;
        private static Dictionary<int, (float, float, float)> colorMap = null;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_1320()
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
            colorMap = new Dictionary<int, (float, float, float)>();

            // nMax must be > 5
            switch (rand.Next(3))
            {
                case 0:
                    nMax = 10;
                    break;

                case 1:
                    nMax = rand.Next(15) + 10;
                    break;

                case 2:
                    nMax = rand.Next(33) + 10;
                    break;
            }

            int colorCount = 3 + rand.Next(5);

            for (int i = 0; i < colorCount; i++)
            {
                float r, g, b;

                do {
                    r = myUtils.randFloat(rand);
                    g = myUtils.randFloat(rand);
                    b = myUtils.randFloat(rand);
                }
                while (r + g + b < 0.3f);

                colorMap.Add(i, (r, g, b));
            }

            // Global unmutable constants
            {
                N = rand.Next(1000) + 5000;

                shape = myUtils.randomChance(rand, 4, 5)
                    ? 0
                    : rand.Next(5);
            }

            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time local initialization
        private void initLocal()
        {
            doClearBuffer = true;
            doFillShapes = myUtils.randomChance(rand, 4, 5);
            doReduceOpacity = myUtils.randomChance(rand, 2, 3);

            colorMode = rand.Next(4);
            nMode = rand.Next(2);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            height = 600;

            string str = $"Obj = {Type}\n\n"                         +
                            myUtils.strCountOf(list.Count, N)        +
                            $"nMax = {nMax}\n"                       +
                            $"doFillShapes = {doFillShapes}\n"       +
                            $"doReduceOpacity = {doReduceOpacity}\n" +
                            $"nMode = {nMode}\n"                     +
                            $"colorMode = {colorMode}\n"             +
                            $"colorMap.Count = {colorMap.Count}\n"   +
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
            y = -10;

            dx = 0;
            dy = myUtils.randFloatClamped(rand, 0.1f) * 3;

            size = rand.Next(5) + 3;

            switch (nMode)
            {
                case 0:
                    n = 5 + rand.Next(nMax - 5);
                    break;

                case 1:
                    n = nMax;
                    break;
            }

            dist = 2 * (int)size + rand.Next(3);

            getColor();

            A = myUtils.randFloatClamped(rand, 0.05f) * 0.85f;
            dy = A * 3;

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            x += dx;
            y += dy;

            if (y > (gl_Height + n * (size + dist)))
            {
                generateNew();
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void Draw(float size, float r, float g, float b, float a, float yy)
        {
            float size2x = size * 2;

            switch (shape)
            {
                // Instanced squares
                case 0:
                    myPrimitive._RectangleInst.setInstanceCoords(x - size, yy - size, size2x, size2x);
                    myPrimitive._RectangleInst.setInstanceColor(r, g, b, a);
                    myPrimitive._RectangleInst.setInstanceAngle(angle);
                    break;

                // Instanced triangles
                case 1:
                    myPrimitive._TriangleInst.setInstanceCoords(x, yy, size, angle);
                    myPrimitive._TriangleInst.setInstanceColor(r, g, b, a);
                    break;

                // Instanced circles
                case 2:
                    myPrimitive._EllipseInst.setInstanceCoords(x, yy, size2x, angle);
                    myPrimitive._EllipseInst.setInstanceColor(r, g, b, a);
                    break;

                // Instanced pentagons
                case 3:
                    myPrimitive._PentagonInst.setInstanceCoords(x, yy, size2x, angle);
                    myPrimitive._PentagonInst.setInstanceColor(r, g, b, a);
                    break;

                // Instanced hexagons
                case 4:
                    myPrimitive._HexagonInst.setInstanceCoords(x, yy, size2x, angle);
                    myPrimitive._HexagonInst.setInstanceColor(r, g, b, a);
                    break;
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Show()
        {
            float Size = size;
            float r = R;
            float g = G;
            float b = B;
            float a = A;
            float yy = y;

            for (int i = 0; i < n; i++)
            {
                if (i == 0)
                {
                    if (myUtils.randomChance(rand, 1, 100))
                    {
                        R0 = R + myUtils.randFloat(rand) * 0.33f;
                        G0 = G + myUtils.randFloat(rand) * 0.33f;
                        B0 = B + myUtils.randFloat(rand) * 0.33f;
                    }

                    Draw(Size - 1, R0, G0, B0, a, yy);
                }

                if (i == 1)
                {
                    Size = size * 0.6f;
                    r = R * 0.5f;
                    g = G * 0.5f;
                    b = B * 0.5f;
                    a = A;
                }

                if (i > 0)
                {
                    yy -= dist;

                    if (i == 1)
                        yy -= size / 2;

                    if (doReduceOpacity)
                        a *= 0.8f; 
                }

                Draw(Size, r, g, b, a, yy);
            }

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
                    inst.ResetBuffer();

                    for (int i = 0; i != Count; i++)
                    {
                        var obj = list[i] as myObj_1320;

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
                    list.Add(new myObj_1320());
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
            base.initShapes(shape, N * nMax, 0);

            grad = new myScreenGradient();
            grad.SetRandomColors(rand, 0.2f);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void getColor()
        {
            switch (colorMode)
            {
                case 0:
                    {
                        var mode = colorPicker.getMode();

                        if (mode == myColorPicker.colorMode.SNAPSHOT || mode == myColorPicker.colorMode.IMAGE)
                        {
                            colorPicker.getColorRand(ref R, ref G, ref B);
                        }
                        else
                        {
                            colorPicker.getColor(x, y, ref R, ref G, ref B);
                        }
                    }
                    break;

                case 1:
                    {
                        R = 0.5f + myUtils.randFloat(rand) * 0.4f;
                        G = 0.5f + myUtils.randFloat(rand) * 0.4f;
                        B = 0.5f + myUtils.randFloat(rand) * 0.4f;
                    }
                    break;

                case 2:
                    {
                        int i = rand.Next(colorMap.Count);
                        var color = colorMap[i];

                        R = color.Item1;
                        G = color.Item2;
                        B = color.Item3;
                    }
                    break;

                case 3:
                    {
                        int i = rand.Next(colorMap.Count);
                        var color = colorMap[i];

                        R = color.Item1 + myUtils.randFloatSigned(rand) * 0.2f;
                        G = color.Item2 + myUtils.randFloatSigned(rand) * 0.2f;
                        B = color.Item3 + myUtils.randFloatSigned(rand) * 0.2f;
                    }
                    break;
            }

            R0 = R;
            G0 = G;
            B0 = B;

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------
    }
};
