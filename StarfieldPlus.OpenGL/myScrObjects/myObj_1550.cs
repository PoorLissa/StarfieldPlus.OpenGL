using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Security.AccessControl;


/*
    - Invisible static dots + one moving point. The moving point builds connections to invisible dots while it travels around
*/


namespace my
{
#pragma warning disable 0162
    public class myObj_1550 : myObject
    {
        // Priority
        public static int Priority => 10;
        public static System.Type Type => typeof(myObj_1550);

        private int cnt, lifeCnt, cellId = -1000, interactCnt;
        private float x, y, dx, dy;
        private float size, angle;
        private float A, R, G, B, dA;
        private bool isAlive;

        private static int N = 0, n = 0, shape = 0, colorMode = 0, colorModeMain = 0, mainMotionMode = 0, actorStartMode = 0;
        private static int cellSize = 100, interactMode = 0;
        private static bool doDestroy = false, doUseInteractCnt = false;
        private static float dimAlpha = 0.05f, spdSmall = 0, rR = 0, gG = 0, bB = 0;
        private static float minA = 0;

        private static myScreenGradient grad = null;
        private static myCellManager<myObj_1550> cellManager = null;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_1550()
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

            cellManager = new myCellManager<myObj_1550>(50, 0, gl_Width, gl_Height);

            // Global unmutable constants
            {
                n = 11;
                N = 111111;
                shape = rand.Next(5);

                actorStartMode = rand.Next(3);
            }

            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time local initialization
        private void initLocal()
        {
            doClearBuffer = true;
            doDestroy = myUtils.randomBool(rand);
            doUseInteractCnt = myUtils.randomBool(rand);

            colorMode = rand.Next(4);
            colorModeMain = rand.Next(3);
            mainMotionMode = rand.Next(2);

            spdSmall = myUtils.randFloat(rand) * (rand.Next(3) + 1);

            rR = myUtils.randFloat(rand);
            gG = myUtils.randFloat(rand);
            bB = myUtils.randFloat(rand);

            // Min opacity of background particles
            minA = myUtils.randFloat(rand) * 0.1f;

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            height = 600;

            string str = $"Obj = {Type}\n\n"                           +
                            $"n = {n}\n"                               +
                            myUtils.strCountOf(list.Count, N)          +
                            $"doClearBuffer = {doClearBuffer}\n"       +
                            $"mainMotionMode = {mainMotionMode}\n"     +
                            $"colorMode = {colorMode}\n"               +
                            $"colorModeMain = {colorModeMain}\n"       +
                            $"doDestroy = {doDestroy}\n"               +
                            $"doUseInteractCnt = {doUseInteractCnt}\n" +
                            $"spdSmall = {myUtils.fStr(spdSmall)}\n"   +
                            $"actorStartMode = {actorStartMode}\n"     +
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
                switch (actorStartMode)
                {
                    case 0:
                        x = gl_x0;
                        y = gl_y0;
                        break;

                    case 1:
                        x = rand.Next(gl_Width);
                        y = rand.Next(gl_Height);
                        break;

                    case 2:
                        x = gl_x0 + rand.Next(400) * myUtils.randomSign(rand);
                        y = gl_y0 + rand.Next(300) * myUtils.randomSign(rand);
                        break;
                }

                float spd = 1.5f;

                dx = (0.5f + myUtils.randFloat(rand) * spd) * myUtils.randomSign(rand);
                dy = (0.5f + myUtils.randFloat(rand) * spd) * myUtils.randomSign(rand);

                A = 0.5f;
                cnt = 50 + rand.Next(111); // used as a dist
                cnt = 200 + rand.Next(50);

                cnt = 50 + rand.Next(100);

                do
                {
                    R = myUtils.randFloat(rand);
                    G = myUtils.randFloat(rand);
                    B = myUtils.randFloat(rand);
                }
                while (R + G + B < 1);

                size = 2;
            }
            else
            {
                size = 1;
                angle = 0;

                x = rand.Next(gl_Width);
                y = rand.Next(gl_Height);

                dx = (myUtils.randFloat(rand)) * myUtils.randomSign(rand);
                dy = (myUtils.randFloat(rand)) * myUtils.randomSign(rand);

                dx *= spdSmall;
                dy *= spdSmall;

                A = minA;
                dA = 0.005f;

                switch (colorModeMain)
                {
                    case 0:
                        R = G = B = 1;
                        break;

                    case 1:
                        R = myUtils.randFloat(rand);
                        G = myUtils.randFloat(rand);
                        B = myUtils.randFloat(rand);
                        break;

                    case 2:
                        R = rR;
                        G = gG;
                        B = bB;
                        break;
                }

                cnt = 333 + rand.Next(999);
                lifeCnt = 33;
                interactCnt = 0;

                isAlive = true;
            }

            cellId = cellManager.Move(x, y, id, cellId, this);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            x += dx;
            y += dy;

            if (id < n)
            {
                switch (mainMotionMode)
                {
                    case 0:
                        {
                            if (x < 0)
                                dx += 0.01f;

                            if (y < 0)
                                dy += 0.01f;

                            if (x > gl_Width)
                                dx -= 0.01f;

                            if (y > gl_Height)
                                dy -= 0.01f;
                        }
                        break;

                    case 1:
                        {
                            int dist = 150;
                            float dd = 0.5f;

                            if (x < gl_x0 - dist)
                                dx += dd;

                            if (y < gl_y0 - dist)
                                dy += dd;

                            if (x > gl_x0 + dist)
                                dx -= dd;

                            if (y > gl_y0 + dist)
                                dy -= dd;
                        }
                        break;
                }

                // Use cell manager
                {
                    float dX, dY, distSq;

                    int rayDist = cnt / cellManager._cellSize;

                    int min = -rayDist - 1;
                    int max = +rayDist + 2;

                    int cnt2 = cnt * cnt;

                    for (int i = min; i < max; i++)
                    {
                        int cell_i = cellId + cellManager._cellRowSize * i;

                        for (int j = min; j < max; j++)
                        {
                            int cell = cell_i + j;

                            if (cell >= cellManager._minId && cell <= cellManager._maxId)
                            {
                                foreach (var other in cellManager._dic[cell].items)
                                {
                                    if (other.Value.id >= n)
                                    {
                                        dX = x - other.Value.x;
                                        dY = y - other.Value.y;

                                        distSq = dX * dX + dY * dY;

                                        // Other particle is within reach of the first one
                                        if (distSq < cnt2)
                                        {
                                            other.Value.interactCnt++;

                                            // Give the other particle the color of its parent (or smth else)
                                            switch (colorMode)
                                            {
                                                case 0:
                                                    other.Value.R = (R + other.Value.R) * 0.5f;
                                                    other.Value.G = (G + other.Value.G) * 0.5f;
                                                    other.Value.B = (B + other.Value.B) * 0.5f;
                                                    break;

                                                case 1:
                                                    other.Value.R = 0.05f * R + 0.95f * other.Value.R;
                                                    other.Value.G = 0.05f * G + 0.95f * other.Value.G;
                                                    other.Value.B = 0.05f * B + 0.95f * other.Value.B;
                                                    break;

                                                case 2:
                                                    other.Value.R += 0.01f;
                                                    other.Value.G += 0.01f;
                                                    other.Value.B += 0.01f;
                                                    break;

                                                case 3:
                                                    other.Value.R -= 0.1f;
                                                    other.Value.G -= 0.1f;
                                                    other.Value.B -= 0.1f;
                                                    break;
                                            }
#if false
                                            if (doUseInteractCnt)
                                            {
                                                if (other.Value.interactCnt > 3)
                                                    continue;
                                            }
#endif
                                            interactMode = 0;

                                            float slowFactor = 0.98f;
                                            //slowFactor = 1.001f;

                                            // Slower/fasten the other particle
                                            if (false)
                                            {
                                                other.Value.dx *= slowFactor;
                                                other.Value.dy *= slowFactor;
                                            }

                                            switch (interactMode)
                                            {
                                                case 0:
                                                    {
                                                        if (other.Value.A <= minA)
                                                        {
                                                            other.Value.A = (float)(distSq / cnt2);
                                                        }

                                                        other.Value.dx -= dX * 0.001f;
                                                        other.Value.dy -= dY * 0.001f;

                                                        other.Value.x -= dX * 0.05f;
                                                        other.Value.y -= dY * 0.05f;

                                                        if (other.Value.interactCnt == 1)
                                                        {
                                                            other.Value.dx *= -1;
                                                            other.Value.dy *= -1;
                                                        }

                                                        other.Value.size += 0.01f;
                                                        other.Value.angle += 0.001f;
                                                    }
                                                    break;

                                                case 1:
                                                    {
                                                        other.Value.dx = dX;
                                                        other.Value.dy = dY;
                                                    }
                                                    break;
                                            }

                                            continue;

                                            //other.Value.isAlive = false; continue;

                                            //other.Value.A = 0.75f * myUtils.randFloat(rand);

                                            if (other.Value.A <= minA)
                                            {
                                                other.Value.A = 0.75f * myUtils.randFloat(rand);
                                            }

                                            if (other.Value.interactCnt == 1)
                                            {
                                                other.Value.dx *= -1;
                                                other.Value.dy *= -1;

                                                {
                                                    float alpha = myUtils.randFloat(rand) + rand.Next(321);
                                                    //other.Value.x = x + (float)(cnt * Math.Sin(alpha));
                                                    //other.Value.y = y + (float)(cnt * Math.Cos(alpha));

                                                    //other.Value.dx = (float)Math.Sin(alpha) * 0.5f;
                                                    //other.Value.dy = (float)Math.Cos(alpha) * 0.5f;
                                                }
                                            }

                                            other.Value.size += 0.01f;
                                            other.Value.angle += 0.001f;

                                            //other.Value.dA = 0.001f;
                                        }
                                        else
                                        {
                                            /*
                                            other.Value.R = 1;
                                            other.Value.G = 1;
                                            other.Value.B = 1;*/
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                if (A > minA)
                    A -= dA;

                if (--cnt == 0)
                {
                    generateNew();
                }

                if (doDestroy && lifeCnt == 0)
                {
                    generateNew();
                }

                if (isAlive == false)
                {
                    generateNew();
                }
            }

            cellId = cellManager.Move(x, y, id, cellId, this);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Show()
        {
            float size2x = size * 2;

            {
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

            if (!false)
                while (list.Count < N)
                    list.Add(new myObj_1550());

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
                    if (doClearBuffer == true)
                    {
                        glClear(GL_COLOR_BUFFER_BIT);
                        grad.Draw();
                    }
                }

                // Render Frame
                {
                    inst.ResetBuffer();

                    for (int i = 0; i != Count; i++)
                    {
                        var obj = list[i] as myObj_1550;

                        obj.Show();
                        obj.Move();
                    }

                    // Tell the fragment shader to do nothing with the existing instance opacity:
                    inst.SetColorA(0);
                    inst.Draw(false);
                }

                if (Count < N)
                {
                    list.Add(new myObj_1550());
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
            base.initShapes(shape, N * 3, 0);

            grad = new myScreenGradient();
            grad.SetRandomColors(rand, 0.2f);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------
    }
};
