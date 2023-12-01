using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;


/*
    - Particles with real trails again again
*/


namespace my
{
    public class myObj_011c : myObject
    {
        // Priority
        public static int Priority => 10;
		public static System.Type Type => typeof(myObj_011c);

        private int cnt;
        private float x, y, dx, dy, ddx, ddy;
        private float size, A, R, G, B;

        private myParticleTrail trail = null;

        private static int N = 0, shape = 0, nTrailMin = 50, nTrailMax = 111, startMode = 0, moveMode = 0, trailMode = 0, reflectMode = 0;
        private static bool doFillShapes = true, doDrawToWhite = true, doUseBlackBox = true, doUseLifeCnt = true;
        private static int X = gl_x0, Y = gl_y0, blackBoxW = 0, blackBoxH = 0, maxCnt = 0;

        private static int[] i_arr = null;
        private static float[] f_arr = null;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_011c()
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

            i_arr = new int[3];
            f_arr = new float[3];

            // Global unmutable constants
            {
                N = 1000;

                switch (rand.Next(3))
                {
                    case 0: N += rand.Next(00100); break;
                    case 1: N += rand.Next(01000); break;
                    case 2: N += rand.Next(10000); break;
                }

                shape = rand.Next(5);

                nTrailMin = 50 + rand.Next(25);

                switch (rand.Next(3))
                {
                    case 0: nTrailMax = 50 + rand.Next(111); break;
                    case 1: nTrailMax = 75 + rand.Next(333); break;
                    case 2: nTrailMax = 99 + rand.Next(666); break;
                }

                startMode = rand.Next(5);
            }

            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time local initialization
        private void initLocal()
        {
            X = rand.Next(gl_Width);
            Y = rand.Next(gl_Height);

            doClearBuffer = true;
            doFillShapes  = myUtils.randomChance(rand, 1, 2);
            doDrawToWhite = myUtils.randomChance(rand, 1, 3);
            doUseBlackBox = myUtils.randomChance(rand, 1, 2);
            doUseLifeCnt  = myUtils.randomChance(rand, 4, 5);
            renderDelay = 0;

            maxCnt = 333 + rand.Next(999);

            blackBoxW = 111 + rand.Next(gl_x0 / 2);
            blackBoxH = 111 + rand.Next(gl_y0 / 2);

            moveMode  = rand.Next(3);
            trailMode = rand.Next(8);
            reflectMode = rand.Next(2);

            i_arr[0] = 1 + rand.Next(3);                        // speed factor for dx, dy

            switch (moveMode)
            {
            }

            switch (trailMode)
            {
                case 1:
                case 2:
                    f_arr[0] = 2 + rand.Next(3);                // sin/cos amplitude factor [2 .. 4]
                    f_arr[1] = 1.0f / (2 + rand.Next(33));      // sin/cos argument divider 1.0 / [2 .. 34]
                    break;

                case 3:
                    if (myUtils.randomChance(rand, 1, 2))
                    {
                        f_arr[0] = 2 + rand.Next(5);
                        f_arr[1] = f_arr[0] + rand.Next((int)f_arr[0] * 3);
                    }
                    else
                    {
                        f_arr[1] = 2 + rand.Next(5);
                        f_arr[0] = f_arr[1] + rand.Next((int)f_arr[1] * 3);
                    }
                    break;

                case 4:
                    f_arr[0] = 1 + rand.Next(11);
                    break;

                case 5:
                    f_arr[0] = 2 + rand.Next(11);
                    f_arr[1] = 3 + rand.Next(33);
                    break;

                case 6:
                case 7:
                    f_arr[0] = 2 + rand.Next(20);
                    f_arr[1] = 2 + rand.Next(33);
                    f_arr[2] = 2 + rand.Next(7);
                    break;
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            height = 600;

            string nStr(int   n) { return n.ToString("N0");    }
            string fStr(float f) { return f.ToString("0.000"); }

            string i_arrStr = $"{nStr(i_arr[0])}; {nStr(i_arr[1])}; {nStr(i_arr[2])};";
            string f_arrStr = $"{fStr(f_arr[0])}; {fStr(f_arr[1])}; {fStr(f_arr[2])};";

            string str = $"Obj = {Type}\n\n"                     	 +
                            $"N = {nStr(list.Count)} of {nStr(N)}\n" +
                            $"doFillShapes = {doFillShapes}\n"       +
                            $"doDrawToWhite = {doDrawToWhite}\n"     +
                            $"doUseBlackBox = {doUseBlackBox}\n"     +
                            $"doUseLifeCnt = {doUseLifeCnt}\n"       +
                            $"moveMode = {moveMode}\n"               +
                            $"trailMode = {trailMode}\n"             +
                            $"nTrailMin = {nTrailMin}\n"             +
                            $"nTrailMax = {nTrailMax}\n"             +
                            $"i_arr = {i_arrStr}\n"                  +
                            $"f_arr = {f_arrStr}\n"                  +
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
            cnt = 333 + rand.Next(maxCnt);
            size = rand.Next(3) + 3;
            A = myUtils.randFloat(rand, 0.1f);
            colorPicker.getColorRand(ref R, ref G, ref B);

            // Set starting point
            switch (startMode)
            {
                case 0:
                    x = gl_x0;
                    y = gl_y0;
                    break;

                case 1:
                    x = X;
                    y = Y;
                    break;

                case 2:
                    x = rand.Next(gl_Width);
                    y = Y;
                    break;

                case 3:
                    y = rand.Next(gl_Height);
                    x = X;
                    break;

                case 4:
                    x = rand.Next(gl_Width);
                    y = rand.Next(gl_Height);
                    break;
            }

            // Set dx, dy, ddx, ddy
            switch (moveMode)
            {
                case 0:
                    {
                        dx = myUtils.randFloatSigned(rand, 0.01f) * 3;
                        ddx = 0;

                        dy = 0;
                        ddy = 0.01f + myUtils.randFloat(rand) * 0.01f * i_arr[0];

                        // Direction: up or down
                        ddy *= myUtils.randomSign(rand);
                    }
                    break;

                case 1:
                    {
                        dy = myUtils.randFloatSigned(rand, 0.01f) * 3;
                        ddy = 0;

                        dx = 0;
                        ddx = 0.01f + myUtils.randFloat(rand) * 0.01f * i_arr[0];

                        // Direction: left or right
                        ddx *= myUtils.randomSign(rand);
                    }
                    break;

                case 2:
                    {
                        dx = myUtils.randFloatSigned(rand, 0.01f) * 3;
                        dy = myUtils.randFloatSigned(rand, 0.01f) * 3;

                        ddx = (0.01f + myUtils.randFloat(rand) * 0.01f) * myUtils.randomSign(rand) * i_arr[0];
                        ddy = (0.01f + myUtils.randFloat(rand) * 0.01f) * myUtils.randomSign(rand) * i_arr[0];
                    }
                    break;
            }

            // Initialize Trail
            if (trail == null)
            {
                int nTrail = nTrailMin + rand.Next(nTrailMax);
                trail = new myParticleTrail(nTrail, x, y);
            }
            else
            {
                trail.reset(x, y);
            }

            if (trail != null)
            {
                trail.updateDa(A);
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void reflect1()
        {
            if (y > gl_Height && dy > 0)
                dy *= -1;

            if (y < 0 && dy < 0)
                dy *= -1;

            if (x < 0 && dx < 0)
                dx *= -1;

            if (x > gl_Width && dx > 0)
                dx *= -1;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void reflect2()
        {
            float val = 0.05f;

            if (y > gl_Height)
                dy -= val;

            if (y < 0)
                dy += val;

            if (x < 0)
                dx += val;

            if (x > gl_Width)
                dx -= val;
        }

        // ---------------------------------------------------------------------------------------------------------------


        protected override void Move()
        {
            if (doUseLifeCnt && --cnt < 0)
            {
                A -= 0.01f;

                if (A < 0)
                {
                    generateNew();
                }
            }

            switch (moveMode)
            {
                case 0:
                case 1:
                case 2:
                    {
                        x += dx;
                        y += dy;

                        // In blackBoxMode, acceleration is applied only when the particle is outside of the box:
                        if (doUseBlackBox)
                        {
                            if (x < blackBoxW || x > gl_Width - blackBoxW || y < blackBoxH || y > gl_Height - blackBoxH)
                            {
                                dx += ddx;
                                dy += ddy;
                            }
                        }
                        else
                        {
                            dx += ddx;
                            dy += ddy;
                        }

                        switch (reflectMode)
                        {
                            case 0: reflect1(); break;
                            case 1: reflect2(); break;
                        }
                    }
                    break;
            }

            // --- Trail ---

            switch (trailMode)
            {
                // Straight trail
                case 0:
                    {
                        trail.update(x, y);
                    }
                    break;

                // Sin trail, take 1
                case 1:
                    {
                        float addX = dx + (float)Math.Sin(y * f_arr[1]) * f_arr[0];
                        float addY = dy + (float)Math.Sin(x * f_arr[1]) * f_arr[0];

                        trail.update(x + addX, y + addY);
                    }
                    break;

                // Sin trail, take 2
                case 2:
                    {
                        float extraX = myUtils.randFloatSigned(rand) * 5;
                        float extraY = myUtils.randFloat(rand) * 10;

                        float addX = dx + (float)Math.Sin(y * f_arr[1]) * f_arr[0] + extraX;
                        float addY = dy + (float)Math.Sin(x * f_arr[1]) * f_arr[0] + extraY;

                        trail.update(x + addX, y + addY);
                    }
                    break;

                // Randomized trail offsets
                case 3:
                    {
                        float addX = myUtils.randFloatSigned(rand) * f_arr[0];
                        float addY = myUtils.randFloatSigned(rand) * f_arr[1];

                        trail.update(x + addX, y + addY);
                    }
                    break;

                case 4:
                    {
                        float addX = (float)Math.Sin(y) * f_arr[0];
                        trail.update(x + addX, y);
                    }
                    break;

                case 5:
                    {
                        float addX = (float)(Math.Sin(y) * f_arr[0] * Math.Sin(y / f_arr[1]));
                        trail.update(x + addX, y);
                    }
                    break;

                case 6:
                    {
                        float addX = (float)(Math.Sin(y / f_arr[0]) * f_arr[0] + Math.Cos(y * f_arr[1]) * f_arr[2]) * 0.5f;
                        trail.update(x + addX, y);
                    }
                    break;

                // The same as 6, but using (x % 33)
                case 7:
                    {
                        float addX = (float)(Math.Sin(y / (1 + x % 33)) * f_arr[0] + Math.Cos(y * f_arr[1]) * f_arr[2]) * 0.5f;
                        trail.update(x + addX, y);
                    }
                    break;
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Show()
        {
            if (doDrawToWhite)
            {
                trail.ShowToWhite(R, G, B, A);
            }
            else
            {
                trail.Show(R, G, B, A);
            }

            float size2x = size * 2;

            switch (shape)
            {
                // Instanced squares
                case 0:
                    myPrimitive._RectangleInst.setInstanceCoords(x - size, y - size, size2x, size2x);
                    myPrimitive._RectangleInst.setInstanceColor(R, G, B, A);
                    myPrimitive._RectangleInst.setInstanceAngle(0);
                    break;

                // Instanced triangles
                case 1:
                    myPrimitive._TriangleInst.setInstanceCoords(x, y, size2x, 0);
                    myPrimitive._TriangleInst.setInstanceColor(R, G, B, A);
                    break;

                // Instanced circles
                case 2:
                    myPrimitive._EllipseInst.setInstanceCoords(x, y, size2x, 0);
                    myPrimitive._EllipseInst.setInstanceColor(R, G, B, A);
                    break;

                // Instanced pentagons
                case 3:
                    myPrimitive._PentagonInst.setInstanceCoords(x, y, size2x, 0);
                    myPrimitive._PentagonInst.setInstanceColor(R, G, B, A);
                    break;

                // Instanced hexagons
                case 4:
                    myPrimitive._HexagonInst.setInstanceCoords(x, y, size2x, 0);
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

            clearScreenSetup(doClearBuffer, 0.13f);

            while (!Glfw.WindowShouldClose(window))
            {
                int Count = list.Count;

                processInput(window);

                // Swap fore/back framebuffers, and poll for operating system events.
                Glfw.SwapBuffers(window);
                Glfw.PollEvents();

                glClear(GL_COLOR_BUFFER_BIT);

                // Render Frame
                {
                    inst.ResetBuffer();
                    myPrimitive._LineInst.ResetBuffer();

                    for (int i = 0; i != Count; i++)
                    {
                        var obj = list[i] as myObj_011c;

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
                    list.Add(new myObj_011c());
                }

                cnt++;
                System.Threading.Thread.Sleep(renderDelay);
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void initShapes()
        {
            base.initShapes(shape, N, 0);
            myPrimitive.init_LineInst(N * (nTrailMin + nTrailMax));     // should be enough, right?

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------
    }
};
