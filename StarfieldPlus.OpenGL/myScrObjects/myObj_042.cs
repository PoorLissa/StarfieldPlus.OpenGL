using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;


/*
    - Lines 3: Patchwork / Micro Schematics
*/


namespace my
{
    public class myObj_042 : myObject
    {
        private int x, y, dx, dy, oldx, oldy, iterCounter, colorCounter;
        private float size, A, R, G, B, dR, dG, dB;
        private bool isStatic = false, doClearScreen = false;

        private static int N = 0, moveMode = 0, colorMode = 0, shape = 0, baseSize = 0, spd = 0, divider = 0, angle = 0, divX = 1, divY = 1, divMax = 1;
        private static float moveConst = 0.0f, time = 0.0f, dimAlpha = 0.0f, maxA = 0.33f, dRstatic, dGstatic, dBstatic;
        private static bool showStatics = false, reuseStatics = false;

        private static myInstancedPrimitive inst = null;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_042()
        {
            if (colorPicker == null)
            {
                colorPicker = new myColorPicker(gl_Width, gl_Height);
                list = new List<myObject>();

                init();
            }

            generateNew();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time initialization
        private void init()
        {
            gl_x0 = gl_Width  / 2;
            gl_y0 = gl_Height / 2;

            N = (N == 0) ? 333 + rand.Next(111) : N;
            renderDelay = 2;

            dimAlpha = 0.001f * (rand.Next(10) + 1);

            spd = (rand.Next(2) == 0) ? -1 : rand.Next(20) + 1;
            baseSize = (rand.Next(7))/3 + 1;
            shape = rand.Next(5);
            divMax = 111 + rand.Next(3333);
            moveMode = rand.Next(26);
            colorMode = rand.Next(4);

            showStatics = myUtils.randomChance(rand, 1, 2);
            reuseStatics = showStatics && myUtils.randomChance(rand, 1, 2);

            // Get moveConst as a Gaussian distribution [1 .. 10] skewed to the left
            {
                int n = 3, moveConst_i = 0;

                for (int i = 0; i < n; i++)
                {
                    // Get symmetrical distribution...
                    moveConst_i += rand.Next(999 / n);

                    if (rand.Next(2) == 0)
                    {
                        // ... and skew it to the left
                        moveConst_i -= 2 * rand.Next(moveConst_i) / 3;
                    }
                }

                moveConst = 1.0f + moveConst_i * 0.01f;
            }

            // Get divider:
            // More often this divider will be 1, but sometimes it can be [1..4]
            {
                divider = rand.Next(10) + 1;
                divider = divider > 4 ? 1 : divider;

                // In case the divider is less than moveConst, all the particles become static;
                // Avoid that:
                while (divider > moveConst)
                    divider--;
            }

#if false
            moveConst = 4.01f;
            divider = 2;
#endif

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void setNextMode()
        {
            var oldShape = shape;

            init();

            doClearScreen = true;
            shape = oldShape;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            return $"Obj = myObj_042\n\n" +
                            $"N = {N}\n" +
                            $"shape = {shape}\n" +
                            $"moveMode = {moveMode}\n" +
                            $"colorMode = {colorMode}\n" +
                            $"moveConst = {moveConst}\n" +
                            $"divider = {divider}\n" +
                            $"spd = {spd}\n" +
                            $"divMax = {divMax}\n" +
                            $"dimAlpha = {dimAlpha}\n" +
                            $"baseSize = {baseSize}\n" +
                            $"showStatics = {showStatics}\n" +
                            $"reuseStatics = {reuseStatics}\n" +
                            $"renderDelay = {renderDelay}";
            }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void generateNew()
        {
            dx = 0;
            dy = 0;

            R = 1;
            G = 1;
            B = 1;

            A = maxA;
            size = baseSize;

            isStatic = false;
            iterCounter = 0;
            colorCounter = rand.Next(777) + 333;

#if true

            do
            {

                x = rand.Next(gl_Width);
                y = rand.Next(gl_Height);

                int speed = (spd > 0) ? spd : rand.Next(20) + 1;

                int dist = (int)Math.Sqrt((x - gl_x0) * (x - gl_x0) + (y - gl_y0) * (y - gl_y0));

                dx = (x - gl_x0) * speed / dist;
                dy = (y - gl_y0) * speed / dist;

                oldx = x;
                oldy = y;

            }
            while (false);

#else

            do
            {

                x = rand.Next(gl_Width);
                y = rand.Next(gl_Height);

                int speed = 5;

                double dist = Math.Sqrt((x - gl_x0) * (x - gl_x0) + (y - gl_y0) * (y - gl_y0));

                dx = (int)((x - gl_x0) * speed / dist);
                dy = (int)((y - gl_y0) * speed / dist);

            }
            while (dx == 0 && dy == 0);

#endif

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        float sinsin(float val, int cnt)
        {
            cnt--;
            double d = Math.Sin(val);

            while (cnt != 0)
            {
                d = Math.Sin(d);
                cnt--;
            }

            return (float)d;
        }

        protected override void Move()
        {
            switch (colorMode)
            {
                case 0:
                    break;

                case 1:
                    if (myUtils.randomChance(rand, 1, 10001))
                    {
                        R = (float)rand.NextDouble();
                        G = (float)rand.NextDouble();
                        B = (float)rand.NextDouble();
                    }
                    break;

                case 2:
                    R = (float)rand.NextDouble();
                    G = (float)rand.NextDouble();
                    B = (float)rand.NextDouble();
                    break;

                // Gradual change of color for each particle;
                // In mode 4, the only difference is, all the particles use the same static dR, dG, dB
                case 3:
                case 4:

                    if (colorCounter == 0)
                    {
                        // Current color life ends
                        float targetR = (float)rand.NextDouble();
                        float targetG = (float)rand.NextDouble();
                        float targetB = (float)rand.NextDouble();

                        colorCounter = rand.Next(333) + 111 - 1;
                        colorCounter *= -1;

                        dR = (R - targetR) / colorCounter;
                        dG = (G - targetG) / colorCounter;
                        dB = (B - targetB) / colorCounter;

                        dRstatic = dR;
                        dGstatic = dG;
                        dBstatic = dB;
                        break;
                    }

                    if (colorCounter == -1)
                    {
                        // Target color transition ends, current color life starts
                        colorCounter = rand.Next(777) + 333;
                        break;
                    }

                    if (colorCounter > 0)
                    {
                        // Current color life goes on
                        colorCounter--;
                        break;
                    }

                    if (colorCounter < 0)
                    {
                        // Transition to the target color in progress
                        R += colorMode == 3 ? dR : dRstatic;
                        G += colorMode == 3 ? dG : dGstatic;
                        B += colorMode == 3 ? dB : dBstatic;
                        colorCounter++;
                        break;
                    }
                    break;
            }

            // todo: experiment with this

#if true
            moveConst = 4.41f;
            divider = 1;

            int n = rand.Next(3) + 1;

            n = 3;

            int dx = (int)(sinsin(y, n) * moveConst) / divider;
            int dy = (int)(sinsin(x, n) * moveConst) / divider;

            x += (int)(sinsin(y * 333, n) * moveConst) / divider;
            y += (int)(sinsin(x * 333, n) * moveConst) / divider;
#endif

#if false
            moveConst = 4.41f;
            divider = 1;

            int n = rand.Next(3) + 1;
            n = 2;

            x += (int)(sinsin(y, n) * moveConst) / divider;
            y += (int)(sinsin(x, n) * moveConst) / divider;
#endif

#if false
            moveConst = 4.41f;
            divider = 1;
            x += (int)(Math.Sin(Math.Sin(y)) * moveConst) / divider;
            y += (int)(Math.Sin(Math.Sin(x)) * moveConst) / divider;
#endif
            return;

            switch (moveMode)
            {
                case 0:
                    x += (int)(Math.Sin(y) * moveConst) / divider;
                    y += (int)(Math.Sin(x) * moveConst) / divider;
                    break;

                case 1:
                    x += (int)(Math.Sin(y) * moveConst) / divider;
                    y += (int)(Math.Cos(x) * moveConst) / divider;
                    break;

                case 2:
                    x += (int)(Math.Sin(y + dy) * moveConst) / divider;
                    y += (int)(Math.Sin(x + dx) * moveConst) / divider;
                    break;

                case 3:
                    x += (int)(Math.Sin(y + dy) * moveConst) / divider;
                    y += (int)(Math.Cos(x + dx) * moveConst) / divider;
                    break;

                case 4:
                    x += (int)(Math.Sin(y + dx) * moveConst) / divider;
                    y += (int)(Math.Sin(x + dy) * moveConst) / divider;
                    break;

                case 5:
                    x += (int)(Math.Sin(y + dx) * moveConst) / divider;
                    y += (int)(Math.Cos(x + dy) * moveConst) / divider;
                    break;

                case 6:
                    x += (int)(Math.Sin(y + dx) * moveConst) / divider;
                    y += (int)(Math.Sin(x + dx) * moveConst) / divider;
                    break;

                case 7:
                    x += (int)(Math.Sin(y + dx) * moveConst) / divider;
                    y += (int)(Math.Cos(x + dx) * moveConst) / divider;
                    break;

                case 8:
                    x += (int)(Math.Sin(y + dy) * moveConst) / divider;
                    y += (int)(Math.Sin(x + dy) * moveConst) / divider;
                    break;

                case 9:
                    x += (int)(Math.Sin(y + dy) * moveConst) / divider;
                    y += (int)(Math.Cos(x + dy) * moveConst) / divider;
                    break;

                case 10:
                    x += (int)(Math.Sin(y * dy) * moveConst) / divider;
                    y += (int)(Math.Sin(x * dx) * moveConst) / divider;
                    break;

                case 11:
                    x += (int)(Math.Sin(y * dy) * moveConst) / divider;
                    y += (int)(Math.Cos(x * dx) * moveConst) / divider;
                    break;

                case 12:
                    x += (int)(Math.Sin(y * dx) * moveConst) / divider;
                    y += (int)(Math.Sin(x * dy) * moveConst) / divider;
                    break;

                case 13:
                    x += (int)(Math.Sin(y * dx) * moveConst) / divider;
                    y += (int)(Math.Cos(x * dy) * moveConst) / divider;
                    break;

                case 14:
                    x += (int)(Math.Sin(y * dx) * moveConst) / divider;
                    y += (int)(Math.Sin(x * dx) * moveConst) / divider;
                    break;

                case 15:
                    x += (int)(Math.Sin(y * dx) * moveConst) / divider;
                    y += (int)(Math.Cos(x * dx) * moveConst) / divider;
                    break;

                case 16:
                    x += (int)(Math.Sin(y * dy) * moveConst) / divider;
                    y += (int)(Math.Sin(x * dy) * moveConst) / divider;
                    break;

                case 17:
                    x += (int)(Math.Sin(y * dy) * moveConst) / divider;
                    y += (int)(Math.Cos(x * dy) * moveConst) / divider;
                    break;

                // --- % variations ---

                case 18:
                    divX = divY = rand.Next(divMax) + 1;
                    x += (int)(Math.Sin(y % divY) * moveConst) / divider;
                    y += (int)(Math.Sin(x % divX) * moveConst) / divider;
                    break;

                case 19:
                    divX = 1 + rand.Next(divMax);
                    divY = 1 + rand.Next(divMax);
                    x += (int)(Math.Sin(y % divY) * moveConst) / divider;
                    y += (int)(Math.Sin(x % divX) * moveConst) / divider;
                    break;

                case 20:
                    divX = divY = rand.Next(divMax) + 1;
                    x += (int)(Math.Sin(y % divY) * moveConst) / divider;
                    y += (int)(Math.Cos(x % divX) * moveConst) / divider;
                    break;

                case 21:
                    divX = 1 + rand.Next(divMax);
                    divY = 1 + rand.Next(divMax);
                    x += (int)(Math.Sin(y % divY) * moveConst) / divider;
                    y += (int)(Math.Cos(x % divX) * moveConst) / divider;
                    break;

                case 22:
                    x += (int)(Math.Sin(y % divMax) * moveConst) / divider;
                    y += (int)(Math.Sin(x % divMax) * moveConst) / divider;
                    break;

                case 23:
                    x += (int)(Math.Sin(y % divMax) * moveConst) / divider;
                    y += (int)(Math.Cos(x % divMax) * moveConst) / divider;
                    break;

                case 24:
                    x += (int)(Math.Sin(y % divMax + y) * moveConst) / divider;
                    y += (int)(Math.Sin(x % divMax + x) * moveConst) / divider;
                    break;

                case 25:
                    x += (int)(Math.Sin(y % divMax + y) * moveConst) / divider;
                    y += (int)(Math.Cos(x % divMax + x) * moveConst) / divider;
                    break;

                case 199:
                    break;


                    // --- % variations ---

#if false
                case 995:
                    X += (int)(Math.Sin(Y * Math.Tan(time)) * 3);
                    Y += (int)(Math.Sin(X * Math.Tan(time)) * 3);
                    break;

                case 994:
                    X += (int)(Math.Sin(Y * Math.Sin(time)) * 3);
                    Y += (int)(Math.Sin(X * Math.Cos(time)) * 3);
                    break;

                case 993:
                    X += (int)(Math.Sin(Y * Math.Sin(time)) * 3);
                    Y += (int)(Math.Sin(X * Math.Sin(time)) * 3);
                    break;

                case 992:
                    X += (int)(Math.Sin(Y * time) * 3);
                    Y += (int)(Math.Sin(X * time) * 3);
                    break;

                case 991:
                    X += (int)(Math.Sin(Y * time) * 3);
                    Y += (int)(Math.Cos(X * time) * 3);
                    break;
#endif
            }

            if (!isStatic)
            {
                // Find the shapes that are relatively small and static
                // Set their opacity to random low values
                if (x == oldx && y == oldy && iterCounter < 1000)
                {
                    if (reuseStatics)
                    {
                        iterCounter = 0;
                        x = rand.Next(gl_Width);
                        y = rand.Next(gl_Height);
                        oldx = x;
                        oldy = y;
                    }
                    else
                    {
                        isStatic = true;
                        A = (float)rand.NextDouble() / 10;
                        oldx = oldy = -12345;
                    }
                }

                iterCounter++;
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Show()
        {
            if (!isStatic || showStatics)
            {
                switch (shape)
                {
                    case 0:
                        var rectInst = inst as myRectangleInst;

                        rectInst.setInstanceCoords(x - size, y - size, 2 * size, 2 * size);
                        rectInst.setInstanceColor(R, G, B, A);
                        rectInst.setInstanceAngle(angle);
                        break;

                    case 1:
                        var triangleInst = inst as myTriangleInst;

                        triangleInst.setInstanceCoords(x, y, size, angle);
                        triangleInst.setInstanceColor(R, G, B, A);
                        break;

                    case 2:
                        var ellipseInst = inst as myEllipseInst;

                        ellipseInst.setInstanceCoords(x, y, 2 * size, angle);
                        ellipseInst.setInstanceColor(R, G, B, A);
                        break;

                    case 3:
                        var pentagonInst = inst as myPentagonInst;

                        pentagonInst.setInstanceCoords(x, y, 2 * size, angle);
                        pentagonInst.setInstanceColor(R, G, B, A);
                        break;

                    case 4:
                        var hexagonInst = inst as myHexagonInst;

                        hexagonInst.setInstanceCoords(x, y, 2 * size, angle);
                        hexagonInst.setInstanceColor(R, G, B, A);
                        break;
                }
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Process(Window window)
        {
            int cnt = 0, maxIter = 3333 + rand.Next(3333);
            initShapes();

            glDrawBuffer(GL_FRONT_AND_BACK);

            // Disable VSYNC, as we nee to draw fast in this mode
            Glfw.SwapInterval(0);

            for (int i = 0; i < N; i++)
                list.Add(new myObj_042());

            while (!Glfw.WindowShouldClose(window))
            {
                int staticsCnt = 0;
                time += 0.01f;

                processInput(window);

                // Swap fore/back framebuffers, and poll for operating system events.
                Glfw.SwapBuffers(window);
                Glfw.PollEvents();

                dimScreen(useStrongerDimFactor: dimAlpha < 0.05f);

                if (doClearScreen)
                {
                    doClearScreen = false;
                    glClear(GL_COLOR_BUFFER_BIT);
                }

                inst.ResetBuffer();

                for (int i = 0; i < list.Count; i++)
                {
                    var obj = list[i] as myObj_042;

                    obj.Show();
                    obj.Move();

                    if (obj.isStatic)
                        staticsCnt++;
                }

                inst.SetColorA(0);
                inst.Draw(false);

                cnt++;

                if (renderDelay >= 0)
                {
                    System.Threading.Thread.Sleep(renderDelay);
                }

                if (cnt > 1000)
                {
                    ;
                }

                if (++cnt > maxIter)
                {
                    R = (float)rand.NextDouble();
                    G = (float)rand.NextDouble();
                    B = (float)rand.NextDouble();
                    cnt = 0;

/*
                    bool gotNewBrush = colorPicker.getNewBrush(br, cnt == (maxIter + 1));

                    if (gotNewBrush)
                    {
                        g.FillRectangle(dimBrush, 0, 0, Width, Height);
                        cnt = 0;
                    }
*/
                }
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void initShapes()
        {
            int lineN = N, shapeN = N;

            myPrimitive.init_Rectangle();
            myPrimitive.init_LineInst(lineN);

            int rotationSubMode = 0;

            switch (shape)
            {
                case 0:
                    myPrimitive.init_RectangleInst(shapeN);
                    myPrimitive._RectangleInst.setRotationMode(rotationSubMode);
                    inst = myPrimitive._RectangleInst;
                    break;

                case 1:
                    myPrimitive.init_TriangleInst(shapeN);
                    myPrimitive._TriangleInst.setRotationMode(rotationSubMode);
                    inst = myPrimitive._TriangleInst;
                    break;

                case 2:
                    myPrimitive.init_EllipseInst(shapeN);
                    myPrimitive._EllipseInst.setRotationMode(rotationSubMode);
                    inst = myPrimitive._EllipseInst;
                    break;

                case 3:
                    myPrimitive.init_PentagonInst(shapeN);
                    myPrimitive._PentagonInst.setRotationMode(rotationSubMode);
                    inst = myPrimitive._PentagonInst;
                    break;

                case 4:
                    myPrimitive.init_HexagonInst(shapeN);
                    myPrimitive._HexagonInst.setRotationMode(rotationSubMode);
                    inst = myPrimitive._HexagonInst;
                    break;
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        // Dim the screen constantly
        private void dimScreen(bool useStrongerDimFactor = false)
        {
            int rnd = rand.Next(101), dimFactor = 1;

            if (useStrongerDimFactor && rnd < 11)
            {
                dimFactor = (rnd == 0) ? 5 : 2;
            }

            myPrimitive._Rectangle.SetAngle(0);

            // Shift background color just a bit, to hide long lasting traces of shapes
            myPrimitive._Rectangle.SetColor(rand.Next(5) * 0.01f, rand.Next(5) * 0.01f, rand.Next(5) * 0.01f, dimAlpha * dimFactor);
            myPrimitive._Rectangle.Draw(0, 0, gl_Width, gl_Height, true);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------
    };
};
