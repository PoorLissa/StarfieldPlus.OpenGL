using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;


/*
    - 
*/


namespace my
{
    public class myObj_1190 : myObject
    {
        // Priority
        public static int Priority => 9999910;
		public static System.Type Type => typeof(myObj_1190);

        private bool isAlive;
        private int parent_id;
        private float x, y, dx, dy;
        private float size, dSize, A, R, G, B, angle = 0;

        private static int N = 0, n = 0, shape = 0, colorMode = 0, angleMode = 0, spawnQtyMode = 0;
        private static bool doFillShapes = false;
        private static float dimAlpha = 0.05f, dSizeGlobal = 0;

        private static myScreenGradient grad = null;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_1190()
        {
            if (id != uint.MaxValue)
                generateNew();

            isAlive = false;
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time global initialization
        protected override void initGlobal()
        {
            var colorMode = myUtils.randomChance(rand, 4, 5)
                ? myColorPicker.colorMode.SNAPSHOT_OR_IMAGE
                : myColorPicker.colorMode.RANDOM_MODE;

            colorPicker = new myColorPicker(gl_Width, gl_Height, colorMode);
            list = new List<myObject>();

            // Global unmutable constants
            {
                n = rand.Next(3) + 1;
                N = n + rand.Next(10) + 10000;

                shape = rand.Next(5);
            }

            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time local initialization
        private void initLocal()
        {
            doClearBuffer = myUtils.randomBool(rand);
            doClearBuffer = true;

            angleMode = rand.Next(3);
            colorMode = rand.Next(4);
            spawnQtyMode = rand.Next(3);

            switch (rand.Next(3))
            {
                case 0:
                    dSizeGlobal = 0.025f + myUtils.randFloat(rand) * 0.025f;
                    break;

                case 1:
                    dSizeGlobal = 0.050f + myUtils.randFloat(rand) * 0.05f;
                    break;

                case 2:
                    dSizeGlobal = 0.10f + myUtils.randFloat(rand) * 0.35f;
                    break;
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            height = 600;

            string str = $"Obj = {Type}\n\n"                               +
                            myUtils.strCountOf(list.Count, N)              +
                            $"n = {n}\n"                                   +
                            $"angleMode = {angleMode}\n"                   +
                            $"colorMode = {colorMode}\n"                   +
                            $"spawnQtyMode = {spawnQtyMode}\n"             +
                            $"dSizeGlobal = {myUtils.fStr(dSizeGlobal)}\n" +
                            $"colorPicker = {colorPicker.getModeStr()}\n"  +
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
            isAlive = true;

            if (id < n)
            {
                x = rand.Next(gl_Width);
                y = rand.Next(gl_Height);

                size = 1;
                dSize = 0.9f;
                dSize = 0.75f + myUtils.randFloat(rand) * 2;
                angle = myUtils.randFloat(rand) * 321;

                // Set parent color
                colorPicker.getColor(x, y, ref R, ref G, ref B);
            }
            else
            {
                size = 1;
                dSize = dSizeGlobal;

                var parent = list[parent_id] as myObj_1190;

                var alpha = myUtils.randFloat(rand) * 321;

                x = parent.x + (float)Math.Sin(alpha) * parent.size;
                y = parent.y + (float)Math.Cos(alpha) * parent.size;

                switch (angleMode)
                {
                    case 0:
                        angle = parent.angle;
                        break;

                    case 1:
                        angle = alpha;
                        break;

                    case 2:
                        angle = myUtils.randFloat(rand) * 321;
                        break;
                }

                A = 0.45f + myUtils.randFloat(rand) * 0.1f;
                R = (float)rand.NextDouble();
                G = (float)rand.NextDouble();
                B = (float)rand.NextDouble();

                R = G = B = 0.33f;

                switch (colorMode)
                {
                    // Parent color
                    case 0:
                        R = parent.R;
                        G = parent.G;
                        B = parent.B;
                        break;

                    // Parent color -- varied
                    case 1:
                        R = parent.R + myUtils.randFloatSigned(rand) * 0.1f;
                        G = parent.G + myUtils.randFloatSigned(rand) * 0.1f;
                        B = parent.B + myUtils.randFloatSigned(rand) * 0.1f;
                        break;

                    // Underlying color
                    case 2:
                    case 3:
                        colorPicker.getColor(x, y, ref R, ref G, ref B);
                        break;
                }
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            if (id < n)
            {
                size += dSize;

                if (size > 999)
                {
                    generateNew();
                    return;
                }
                else
                {
                    int nToSpawn = 0;

                    // Number of spawns per iteration
                    switch (spawnQtyMode)
                    {
                        case 0:
                            nToSpawn = 10;
                            break;

                        case 1:
                            nToSpawn = 20;
                            break;

                        case 2:
                            nToSpawn = 10 + (int)(size) / 100;
                            break;

                        case 3:
                            nToSpawn = (int)(size) / 20;
                            break;
                    }

                    for (int i = n; i < N; i++)
                    {
                        var obj = list[i] as myObj_1190;

                        if (obj.isAlive == false)
                        {
                            obj.parent_id = (int)id;
                            obj.generateNew();

                            if (--nToSpawn == 0)
                                break;
                        }
                    }
                }
            }
            else
            {
                if (isAlive)
                {
                    size += dSize;
                    A -= 0.0025f;

                    if (A < 0 || size > 333)
                        isAlive = false;
                }
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Show()
        {
            if (isAlive && id >= n)
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

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Process(Window window)
        {
            uint cnt = 0;
            initShapes();

            while (list.Count < N)
            {
                list.Add(new myObj_1190());
            }

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
                        var obj = list[i] as myObj_1190;

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
    }
};
