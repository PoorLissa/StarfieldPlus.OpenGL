using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;


/*
    - Procedural animation
    https://www.youtube.com/watch?v=qlfh_rv6khY
*/


namespace my
{
    public class node_1470
    {
        public float x, y;
        public float size;
        public float angle;
    }

    public class myObj_1470 : myObject
    {
        // Priority
        public static int Priority => 10;
		public static System.Type Type => typeof(myObj_1470);

        private float x, y, dx, dy, size;
        private float A, R, G, B, angle = 0;

        private List<node_1470> _chldren = null;

        private static int N = 0, shape = 0, mode = 0, moveMode = 0, colorMode = 0, tailInitMode = 0, sizeMode = 0, nMax = 1;
        private static bool doFillShapes = false;
        private static float rad = 1;
        private static float dimAlpha = 0.05f;

        private static double _1pi2 = 1.0 * Math.PI / 2;
        private static double _3pi2 = 3.0 * Math.PI / 2;

        private static myScreenGradient grad = null;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_1470()
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

                mode = rand.Next(3);

                switch (mode)
                {
                    case 0:
                        N = 5 + rand.Next(10);
                        nMax = 33;
                        break;

                    case 1:
                        N = 3;
                        nMax = 666;
                        break;

                    case 2:
                        N = 13;
                        nMax = 666;
                        break;
                }

                rad = 25;
                shape = rand.Next(5);

                if (mode == 1 || mode == 2)
                {
                    if (myUtils.randomChance(rand, 1, 2))
                        rad = 9 + rand.Next(15);
                }
            }

            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time local initialization
        private void initLocal()
        {
            doClearBuffer = myUtils.randomChance(rand, 11, 12);

            colorMode = rand.Next(2);
            tailInitMode = rand.Next(2);
            sizeMode = rand.Next(2);
            moveMode = rand.Next(2);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            height = 600;

            string str = $"Obj = {Type}\n\n"                     +
                            myUtils.strCountOf(list.Count, N)    +
                            $"doClearBuffer = {doClearBuffer}\n" +
                            $"nMax = {nMax}\n"                   +
                            $"mode = {mode}\n"                   +
                            $"moveMode = {moveMode}\n"           +
                            $"colorMode = {colorMode}\n"         +
                            $"tailInitMode = {tailInitMode}\n"   +
                            $"sizeMode = {sizeMode}\n"           +
                            $"rad = {rad}\n"                     +
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

            switch (moveMode)
            {
                case 0:
                    dx = myUtils.randFloatClamped(rand, 0.5f) * 3;
                    dy = myUtils.randFloatClamped(rand, 0.5f) * 3;
                    break;

                case 1:
                    {
                        if (myUtils.randomChance(rand, 1, 2))
                        {
                            dx = myUtils.randFloatClamped(rand, 0.5f) * 3;
                        }
                        else
                        {
                            dy = myUtils.randFloatClamped(rand, 0.5f) * 3;
                        }
                    }
                    break;
            }

            size = 10 + rand.Next(13);

            A = 1;
            R = (float)rand.NextDouble();
            G = (float)rand.NextDouble();
            B = (float)rand.NextDouble();

            colorPicker.getColor(x, y, ref R, ref G, ref B);

            switch (colorMode)
            {
                case 0:
                    R = G = B = 0.8f;
                    break;

                case 1:
                    {
                        do {
                            R = myUtils.randFloat(rand);
                            G = myUtils.randFloat(rand);
                            B = myUtils.randFloat(rand);
                        }
                        while (R + G + B < 1);
                    }
                    break;
            }

            if (_chldren == null)
            {
                _chldren = new List<node_1470>();

                float dSize = 0.1f + myUtils.randFloat(rand) * 0.5f;

                for (int i = 1; i < nMax; i++)
                {
                    var node = new node_1470();

                    switch (tailInitMode)
                    {
                        case 0:
                            node.x = x + rad * i * 0.5f;
                            node.y = y;
                            break;

                        case 1:
                            node.x = x + myUtils.randFloat(rand) * rand.Next(23);
                            node.y = y + myUtils.randFloat(rand) * rand.Next(23);
                            break;
                    }

                    switch (sizeMode)
                    {
                        case 0:
                            node.size = size - i;
                            break;

                        case 1:
                            node.size = size - i * dSize;
                            break;
                    }

                    if (node.size < 2)
                        node.size = 2;

                    _chldren.Add(node);
                }
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            x += dx;
            y += dy;

            float dd = 0.1f;

            if (x < 0)
                dx += dd;

            if (y < 0)
                dy += dd;

            if (x > gl_Width)
                dx -= dd;

            if (y > gl_Height)
                dy -= dd;

            switch (moveMode)
            {
                case 0:
                    {
                        if (x > 111 && x < gl_Width - 111 && y > 111 & y < gl_Height - 111)
                        {
                            if (myUtils.randomChance(rand, 1, 333))
                                dx *= -1;

                            if (myUtils.randomChance(rand, 1, 333))
                                dy *= -1;
                        }
                    }
                    break;

                case 1:
                    {
                        if (x > 111 && x < gl_Width - 111 && y > 111 & y < gl_Height - 111)
                        {
                            if (dx != 0)
                            {
                                if (myUtils.randomChance(rand, 1, 333))
                                {
                                    dx = 0;
                                    dy = myUtils.randFloatClamped(rand, 0.5f) * 3;
                                }
                            }
                            else
                            {
                                if (myUtils.randomChance(rand, 1, 333))
                                {
                                    dy = 0;
                                    dx = myUtils.randFloatClamped(rand, 0.5f) * 3;
                                }
                            }
                        }
                    }
                    break;
            }

            float x0 = x;
            float y0 = y;

            int i = 0;

            foreach (var node in _chldren)
            {
                float dX = x0 - node.x;
                float dY = y0 - node.y;

                double theta = 0;

                if (dX < 0)
                {
                    theta = _1pi2 - Math.Atan(dY / dX);

                    if (theta > _3pi2)
                        theta = _3pi2;
                }
                else
                {
                    theta = _3pi2 - Math.Atan(dY / dX);

                    if (theta < _1pi2)
                        theta = _1pi2;
                }

                node.x = x0 + rad * (float)Math.Sin(theta);
                node.y = y0 + rad * (float)Math.Cos(theta);

                x0 = node.x;
                y0 = node.y;

                node.angle = (float)theta;

                if (i == 0)
                {
                    i++;
                    angle = (float)theta;
                }
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Show()
        {
            void show(float R, float G, float B, float x, float y, float size, float angle)
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

            show(R, G, B, x, y, size, angle);

            float x0 = x;
            float y0 = y;

            float r = R, dR = (R - 0.1f) / _chldren.Count;
            float g = G, dG = (G - 0.1f) / _chldren.Count;
            float b = B, dB = (B - 0.1f) / _chldren.Count;

            foreach (var node in _chldren)
            {
                show(r, g, b, node.x, node.y, node.size, node.angle);

                myPrimitive._LineInst.setInstanceCoords(x0, y0, node.x, node.y);
                myPrimitive._LineInst.setInstanceColor(r, g, b, A);

                r -= dR;
                g -= dG;
                b -= dB;

                x0 = node.x;
                y0 = node.y;
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
                        glClear(GL_COLOR_BUFFER_BIT);

                    grad.Draw();
                }

                // Render Frame
                {
                    inst.ResetBuffer();
                    myPrimitive._LineInst.ResetBuffer();

                    for (int i = 0; i != Count; i++)
                    {
                        var obj = list[i] as myObj_1470;

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
                    list.Add(new myObj_1470());
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

            int lineWidth = 1 + rand.Next(2);
            myPrimitive.init_LineInst(N * nMax);
            myPrimitive._LineInst.setLineWidth(lineWidth);

            grad = new myScreenGradient();
            grad.SetRandomColors(rand, 0.2f);

            if (doClearBuffer == false)
                grad.SetOpacity(0.05f);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------
    }
};
