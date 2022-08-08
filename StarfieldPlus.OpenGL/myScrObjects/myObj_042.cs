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
        private int x, y, dx, dy, oldx, oldy, iterCounter;
        private float size, A;
        private bool isStatic = false;

        private static int N = 0, moveMode = 0, shape = 0, maxSize = 0, spd = 0, divider = 0;
        private static float moveConst = 0.0f, time = 0.0f, dimAlpha = 0.0f, maxA = 0.33f;
        private static bool showStatics = false;

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
            renderDelay = 10;

            dimAlpha = 0.01f;

            spd = (rand.Next(2) == 0) ? -1 : rand.Next(20) + 1;
            maxSize = rand.Next(4) + 1;

            moveMode = rand.Next(18);

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

            divider = rand.Next(10) + 1;

            // More often this divider will be 1, but sometimes it can be [1..4]
            divider = divider > 4 ? 1 : divider;

            showStatics = rand.Next(3) == 0;
            isStatic = false;
            iterCounter = 0;


moveMode = 0;
maxSize = 2;
dimAlpha = 0.001f;
renderDelay = 2;


            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void setNextMode()
        {
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            return $"Obj = myObj_042\n\n" +
                            $"N = {N}\n" + 
                            $"moveMode = {moveMode}\n" +
                            $"dimAlpha = {dimAlpha}\n"
                            //$"fillMode = {fillMode}\n" + 
                            //$"lineMode = {lineMode}\n" +
                            //$"maxRnd = {maxRnd}\n"
                            ;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void generateNew()
        {
            dx = 0;
            dy = 0;
            A = maxA;
            size = maxSize;

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

        protected override void Move()
        {
            x += (int)(Math.Sin(y) * moveConst) / divider;
            y += (int)(Math.Sin(x) * moveConst) / divider;

#if false

            switch (moveMode)
            {
                case 0:
                    X += (int)(Math.Sin(Y) * moveConst) / divider;
                    Y += (int)(Math.Sin(X) * moveConst) / divider;
                    break;

                case 1:
                    X += (int)(Math.Sin(Y) * moveConst) / divider;
                    Y += (int)(Math.Cos(X) * moveConst) / divider;
                    break;

                case 2:
                    X += (int)(Math.Sin(Y + dy) * moveConst) / divider;
                    Y += (int)(Math.Sin(X + dx) * moveConst) / divider;
                    break;

                case 3:
                    X += (int)(Math.Sin(Y + dy) * moveConst) / divider;
                    Y += (int)(Math.Cos(X + dx) * moveConst) / divider;
                    break;

                case 4:
                    X += (int)(Math.Sin(Y + dx) * moveConst) / divider;
                    Y += (int)(Math.Sin(X + dy) * moveConst) / divider;
                    break;

                case 5:
                    X += (int)(Math.Sin(Y + dx) * moveConst) / divider;
                    Y += (int)(Math.Cos(X + dy) * moveConst) / divider;
                    break;

                case 6:
                    X += (int)(Math.Sin(Y + dx) * moveConst) / divider;
                    Y += (int)(Math.Sin(X + dx) * moveConst) / divider;
                    break;

                case 7:
                    X += (int)(Math.Sin(Y + dx) * moveConst) / divider;
                    Y += (int)(Math.Cos(X + dx) * moveConst) / divider;
                    break;

                case 8:
                    X += (int)(Math.Sin(Y + dy) * moveConst) / divider;
                    Y += (int)(Math.Sin(X + dy) * moveConst) / divider;
                    break;

                case 9:
                    X += (int)(Math.Sin(Y + dy) * moveConst) / divider;
                    Y += (int)(Math.Cos(X + dy) * moveConst) / divider;
                    break;

                case 10:
                    X += (int)(Math.Sin(Y * dy) * moveConst) / divider;
                    Y += (int)(Math.Sin(X * dx) * moveConst) / divider;
                    break;

                case 11:
                    X += (int)(Math.Sin(Y * dy) * moveConst) / divider;
                    Y += (int)(Math.Cos(X * dx) * moveConst) / divider;
                    break;

                case 12:
                    X += (int)(Math.Sin(Y * dx) * moveConst) / divider;
                    Y += (int)(Math.Sin(X * dy) * moveConst) / divider;
                    break;

                case 13:
                    X += (int)(Math.Sin(Y * dx) * moveConst) / divider;
                    Y += (int)(Math.Cos(X * dy) * moveConst) / divider;
                    break;

                case 14:
                    X += (int)(Math.Sin(Y * dx) * moveConst) / divider;
                    Y += (int)(Math.Sin(X * dx) * moveConst) / divider;
                    break;

                case 15:
                    X += (int)(Math.Sin(Y * dx) * moveConst) / divider;
                    Y += (int)(Math.Cos(X * dx) * moveConst) / divider;
                    break;

                case 16:
                    X += (int)(Math.Sin(Y * dy) * moveConst) / divider;
                    Y += (int)(Math.Sin(X * dy) * moveConst) / divider;
                    break;

                case 17:
                    X += (int)(Math.Sin(Y * dy) * moveConst) / divider;
                    Y += (int)(Math.Cos(X * dy) * moveConst) / divider;
                    break;
                    /*
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
                    */
            }

            if (!isStatic)
            {
                // Find the shapes that are relatively small and static
                // Set their opacity to random low values
                if (X == oldx && Y == oldy && iterCounter < 1000)
                {
                    isStatic = true;
                    a = rand.Next(10) + 1;
                    oldx = oldy = -12345;
                }

                iterCounter++;
            }
#endif
            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Show()
        {
            int angle = 0;

            switch (shape)
            {
                case 0:
                    var rectInst = inst as myRectangleInst;

                    rectInst.setInstanceCoords(x - size, y - size, 2 * size, 2 * size);
                    //rectInst.setInstanceColor(R, G, B, A);

                    rectInst.setInstanceColor(1, 1, 1, A);
                    rectInst.setInstanceAngle(angle);
                    break;
/*
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
*/
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Process(Window window)
        {
            int cnt = 0, maxIter = 500 + rand.Next(1500);
            initShapes();

            glDrawBuffer(GL_FRONT_AND_BACK);
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
