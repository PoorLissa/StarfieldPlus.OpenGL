using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;
using StarfieldPlus.OpenGL.myUtils;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;


/*
    - Slowly growing shapes (similar to myObj_0992), originating from multiple generators
*/


namespace my
{
    public class myObj_0993 : myObject
    {
        // Priority
        public static int Priority => 9910;
		public static System.Type Type => typeof(myObj_0993);

        private int cnt, parent_id, colorCnt;
        private bool isAlive;
        private float x, y, w, h;
        private float A, R, G, B, angle = 0;
        private float ytFactor, growFactor;

        private static int N = 0, n = 0, shape = 0, maxCnt = 1, maxSize = 1, initSize = 1, colorMode = 0, spawnFreqMode = 0, initSizeMode = 0, growMode = 0;
        private static bool doFillShapes = false, doMoveParent = false;
        private static float dimAlpha = 0.05f, t = 0, dt = 0;

        private static myScreenGradient grad = null;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_0993()
        {
            parent_id = -1;

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
                N = 3333;
                n = rand.Next(13) + 3;

                maxCnt = 33;
                maxSize = 200;

                dt = 0.1f * (rand.Next(5) + 1);

                shape = rand.Next(5);
            }

            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time local initialization
        private void initLocal()
        {
            doClearBuffer = true;
            doMoveParent = myUtils.randomChance(rand, 4, 5);
            doFillShapes = myUtils.randomChance(rand, 4, 5);

            colorMode = rand.Next(3);
            spawnFreqMode = rand.Next(2);
            initSizeMode = rand.Next(3);
            growMode = rand.Next(3);

            initSize = 1 + rand.Next(50);

            renderDelay = rand.Next(3) + 1;

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            height = 600;

            string str = $"Obj = {Type}\n\n"                       +
                            myUtils.strCountOf(list.Count, N)      +
                            $"n = {n}\n"                           +
                            $"shape = {shape}\n"                   +
                            $"colorMode = {colorMode}\n"           +
                            $"initSizeMode = {initSizeMode}\n"     +
                            $"spawnFreqMode = {spawnFreqMode}\n"   +
                            $"growMode = {growMode}\n"             +
                            $"doMoveParent = {doMoveParent}\n"     +
                            $"frameRate = {stopwatch.GetRate()}\n" +
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
                // Generator
                isAlive = false;

                x = rand.Next(gl_Width);
                y = rand.Next(gl_Height);

                switch (growMode)
                {
                    case 0:
                        growFactor = 1.001f;
                        break;

                    case 1:
                        growFactor = 1.001f + myUtils.randFloat(rand) * 0.003f;
                        break;

                    case 2:
                        // Child particle will have its own growFactor
                        break;
                }

                switch (initSizeMode)
                {
                    case 0:
                        h = w = initSize;
                        break;

                    case 1:
                        h = w = rand.Next(50);
                        break;

                    case 2:
                        // Child particle will generate its own size
                        break;
                }

                cnt = maxCnt + rand.Next(maxCnt);           // The first cnt is set to be different for every generator; this way, they're out of sync
                colorCnt = 33 + rand.Next(21);

                ytFactor = myUtils.randFloat(rand) + rand.Next(3);

                R = (float)rand.NextDouble();
                G = (float)rand.NextDouble();
                B = (float)rand.NextDouble();
            }
            else
            {
                // Particle
                if (parent_id < 0)
                {
                    isAlive = false;
                    parent_id = rand.Next(n);
                }
                else
                {
                    isAlive = true;
                }

                var parent = list[parent_id] as myObj_0993;

                x = parent.x;
                y = parent.y;

                switch (initSizeMode)
                {
                    case 0:
                    case 1:
                        w = parent.w;
                        h = parent.h;
                        break;

                    case 2:
                        w = h = rand.Next(50);
                        break;
                }

                angle = myUtils.randFloatSigned(rand) * rand.Next(123);

                A = 0.25f;

                switch (colorMode)
                {
                    case 0:
                        R = (float)rand.NextDouble();
                        G = (float)rand.NextDouble();
                        B = (float)rand.NextDouble();
                        break;

                    case 1:
                        R = parent.R + myUtils.randFloatSigned(rand) * 0.1f;
                        G = parent.G + myUtils.randFloatSigned(rand) * 0.1f;
                        B = parent.B + myUtils.randFloatSigned(rand) * 0.1f;
                        break;

                    case 2:
                        colorPicker.getColor(x, y, ref R, ref G, ref B);
                        break;
                }

                switch (growMode)
                {
                    case 0:
                    case 1:
                        growFactor = parent.growFactor;
                        break;

                    case 2:
                        growFactor = 1.001f + myUtils.randFloat(rand) * 0.003f;
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
                if (--cnt == 0)
                {
                    switch (spawnFreqMode)
                    {
                        case 0:
                            cnt = maxCnt;
                            break;

                        case 1:
                            cnt = 5 + rand.Next(2 * maxCnt);
                            break;
                    }

                    if (myUtils.randomChance(rand, 1, 666))
                        generateNew();

                    if (doMoveParent)
                    {
                        x += 33 * (float)Math.Cos(t);
                        y += 33 * (float)Math.Cos(t * ytFactor);
                        t += dt;
                    }

                    for (int i = n; i < list.Count; i++)
                    {
                        var obj = list[i] as myObj_0993;

                        // Restore to life one single child particle
                        if (obj.parent_id == this.id && obj.isAlive == false)
                        {
                            obj.generateNew();
                            break;
                        }
                    }

                    if (colorMode == 1)
                    {
                        // Change parent color
                        if (--colorCnt == 0)
                        {
                            do
                            {
                                R = (float)rand.NextDouble();
                                G = (float)rand.NextDouble();
                                B = (float)rand.NextDouble();
                            }
                            while (R + G + B < 0.25f);

                            colorCnt = 33 + rand.Next(21);
                        }
                    }
                }
            }
            else
            {
                if (isAlive)
                {
                    w *= growFactor;
                    h *= growFactor;

                    if (w > maxSize || h > maxSize)
                    {
                        A -= 0.0025f;

                        if (A <= 0)
                        {
                            isAlive = false;
                        }
                    }
                }
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Show()
        {
            if (isAlive)
            {
                float size2x = w * 2;

                switch (shape)
                {
                    // Instanced squares
                    case 0:
                        myPrimitive._RectangleInst.setInstanceCoords(x - w, y - w, size2x, size2x);
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

            clearScreenSetup(doClearBuffer, 0.1f);

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

                    for (int i = 0; i != Count; i++)
                    {
                        var obj = list[i] as myObj_0993;

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
                    list.Add(new myObj_0993());
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
