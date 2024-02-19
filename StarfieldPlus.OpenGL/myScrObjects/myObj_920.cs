using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;


/*
    - 
*/


namespace my
{
    public class myObj_920 : myObject
    {
        // Priority
        public static int Priority => 10;
		public static System.Type Type => typeof(myObj_920);

        private float x, y, dx, dy;
        private float size, a, A, R, G, B, angle = 0;

        private static int N = 0, shape = 0, moveDir = 0, maxSize = 1, colorMode = 0, moveMode = 0, cntMax = 1;
        private static bool doFillShapes = false, doAllocateAll = false;
        private static float rDist = 0, gDist = 0, bDist = 0, rFactor = 0, gFactor = 0, bFactor = 0, dA = 1;

        private static myScreenGradient grad = null;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_920()
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
                N = rand.Next(3333) + 33333;

                shape = rand.Next(5);
            }

            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time local initialization
        private void initLocal()
        {
            cntMax = 333 + rand.Next(999);
            renderDelay = rand.Next(3);

            doClearBuffer = myUtils.randomChance(rand, 10, 11);
            doFillShapes  = myUtils.randomBool(rand);
            doAllocateAll = myUtils.randomBool(rand);

            moveMode = rand.Next(2);
            colorMode = rand.Next(3);
            moveDir = rand.Next(3);

            getColorDistances();

            switch (rand.Next(10))
            {
                case 00: maxSize = 001; break;
                case 01: maxSize = 002; break;
                case 02: maxSize = 003; break;
                case 03: maxSize = 004; break;
                case 04: maxSize = 005; break;
                case 05: maxSize = 007; break;
                case 06: maxSize = 017; break;
                case 07: maxSize = 033; break;
                case 08: maxSize = 066; break;
                case 09: maxSize = 099; break;
                case 10: maxSize = 123; break;
            }

            // Color factors
            getColorFactors();

            dA = 0.001f + myUtils.randFloat(rand) * 0.01f;

#if DEBUG
            doAllocateAll = true;
#endif

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
                            $"moveDir = {moveDir}\n"                 +
                            $"maxSize = {maxSize}\n"                 +
                            $"colorMode = {colorMode}\n"             +
                            $"moveMode = {moveMode}\n"               +
                            $"dA = {fStr(dA)}\n"                     +
                            $"rDist = {rDist}\n"                     +
                            $"gDist = {gDist}\n"                     +
                            $"bDist = {bDist}\n"                     +
                            $"rFactor = {fStr(rFactor)}\n"           +
                            $"gFactor = {fStr(gFactor)}\n"           +
                            $"bFactor = {fStr(bFactor)}\n"           +
                            $"renderDelay = {renderDelay}\n"         +
                            $"cntMax = {cntMax}\n"
                ;
            return str;
        }

        // ---------------------------------------------------------------------------------------------------------------

        // 
        protected override void setNextMode()
        {
            initLocal();

            grad.SetOpacity(doClearBuffer ? 1.0f : 0.1f);
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void generateNew()
        {
            x = rand.Next(gl_Width);
            y = rand.Next(gl_Width);

            dx = x - gl_x0;
            dy = y - gl_y0;

            float dist = (float)Math.Sqrt(dx * dx + dy * dy) + 0.01f;
            float distInv = 1.0f / dist;

            switch (moveMode)
            {
                case 0:
                    dx *= 0.5f * distInv;
                    dy *= 0.5f * distInv;
                    break;

                case 1:
                    dx = myUtils.randFloatSigned(rand) * 0.5f;
                    dy = myUtils.randFloatSigned(rand) * 0.5f;
                    break;
            }

            angle = myUtils.randFloat(rand) * rand.Next(123);

            switch (moveDir)
            {
                case 0:
                    break;

                case 1:
                    dx *= -1;
                    dy *= -1;
                    break;

                case 2:
                    if (myUtils.randomBool(rand))
                    {
                        dx *= -1;
                        dy *= -1;
                    }
                    break;
            }

            if (maxSize < 5)
            {
                size = maxSize;
            }
            else
            {
                size = rand.Next(maxSize) + 1;
            }

            a = 0;
            A = 0.25f + myUtils.randFloat(rand) * 0.33f;

            switch (colorMode)
            {
                case 0:
                    {
                        R = rDist * distInv * rFactor;
                        G = gDist * distInv * gFactor;
                        B = bDist * distInv * bFactor;
                    }
                    break;

                case 1:
                    {
                        R = (dist >= rDist ? rDist * distInv : dist / rDist) * rFactor;
                        G = (dist >= gDist ? gDist * distInv : dist / gDist) * gFactor;
                        B = (dist >= bDist ? bDist * distInv : dist / bDist) * bFactor;
                    }
                    break;

                case 2:
                    {
                        R = dist / rDist;
                        G = dist / gDist;
                        B = dist / bDist;
                    }
                    break;
            }

/*
            dx = dy = 0;
            x = gl_x0;
            y = gl_y0;
            size = dist;
            angle = rand.Next(666);
*/
            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            if (a < A)
            {
                a += myUtils.randFloat(rand) * 0.075f;

                if (a >= A)
                    A = -1;
            }
            else
            {
                x += dx;
                y += dy;
                a -= dA;

                if (a < 0 || x < 0 || x > gl_Width || y < 0 || y > gl_Height)
                {
                    generateNew();
                }
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Show()
        {
            float size2x = size * 2;

            switch (shape)
            {
                // Instanced squares
                case 0:
                    myPrimitive._RectangleInst.setInstanceCoords(x - size, y - size, size2x, size2x);
                    myPrimitive._RectangleInst.setInstanceColor(R, G, B, a);
                    myPrimitive._RectangleInst.setInstanceAngle(angle);
                    break;

                // Instanced triangles
                case 1:
                    myPrimitive._TriangleInst.setInstanceCoords(x, y, size, angle);
                    myPrimitive._TriangleInst.setInstanceColor(R, G, B, a);
                    break;

                // Instanced circles
                case 2:
                    myPrimitive._EllipseInst.setInstanceCoords(x, y, size2x, angle);
                    myPrimitive._EllipseInst.setInstanceColor(R, G, B, a);
                    break;

                // Instanced pentagons
                case 3:
                    myPrimitive._PentagonInst.setInstanceCoords(x, y, size2x, angle);
                    myPrimitive._PentagonInst.setInstanceColor(R, G, B, a);
                    break;

                // Instanced hexagons
                case 4:
                    myPrimitive._HexagonInst.setInstanceCoords(x, y, size2x, angle);
                    myPrimitive._HexagonInst.setInstanceColor(R, G, B, a);
                    break;
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override async void Process(Window window)
        {
            uint cnt = 0;
            initShapes();

            // todo: check how this affects the fps
            Glfw.SwapInterval(1);

            clearScreenSetup(doClearBuffer, 0.1f);

            if (doAllocateAll)
                while (list.Count < N)
                    list.Add(new myObj_920());

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
                    inst.ResetBuffer();

                    for (int i = 0; i != Count; i++)
                    {
                        var obj = list[i] as myObj_920;

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
                    list.Add(new myObj_920());
                }

                System.Threading.Thread.Sleep(0);

                // Fluctuate color factors
                if (++cnt == cntMax)
                {
                    cnt = 0;
                    getColorFactors();

                    // Also, fluctuate color distances
                    if (myUtils.randomChance(rand, 1, 5))
                    {
                        getColorDistances();
                    }
                }
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
            grad.SetOpacity(doClearBuffer ? 1.0f : 0.1f);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void getSingleFactor()
        {
            switch (rand.Next(3))
            {
                case 0: rFactor = myUtils.randFloat(rand); break;
                case 1: gFactor = myUtils.randFloat(rand); break;
                case 2: bFactor = myUtils.randFloat(rand); break;
            }
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void getDoubleFactor()
        {
            switch (rand.Next(3))
            {
                case 0:
                    rFactor = myUtils.randFloat(rand);
                    gFactor = myUtils.randFloat(rand);
                    break;

                case 1:
                    rFactor = myUtils.randFloat(rand);
                    bFactor = myUtils.randFloat(rand);
                    break;

                case 2:
                    gFactor = myUtils.randFloat(rand);
                    bFactor = myUtils.randFloat(rand);
                    break;
            }
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void getTripleFactor()
        {
            do
            {
                rFactor = myUtils.randFloat(rand);
                gFactor = myUtils.randFloat(rand);
                bFactor = myUtils.randFloat(rand);
            }
            while (rFactor + bFactor + gFactor < 1.5f);
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void getColorFactors()
        {
            rFactor = gFactor = bFactor = 1.0f;

            switch (rand.Next(6))
            {
                case 0:
                    break;

                case 1:
                case 2:
                    getSingleFactor();
                    break;

                case 3:
                case 4:
                    getDoubleFactor();
                    break;

                case 5:
                    getTripleFactor();
                    break;
            }
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void getColorDistances()
        {
            rDist = 100 + rand.Next(gl_x0);
            gDist = 100 + rand.Next(gl_x0);
            bDist = 100 + rand.Next(gl_x0);
        }

        // ---------------------------------------------------------------------------------------------------------------
    }
};
