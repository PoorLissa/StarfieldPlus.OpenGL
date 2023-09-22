﻿using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;


/*
    - Particles with real trails again
*/


namespace my
{
    public class myObj_011b : myObject
    {
        // Priority
        public static int Priority => 999910;

        private float x, y, dx, dy;
        private float size, A, R, G, B, angle = 0;

        private myParticleTrail trail = null;

        private static int N = 0, shape = 0, nTrailMin = 50, nTrailMax = 111, moveMode = 0, trailMode = 0;
        private static bool doFillShapes = true;

        private static float[] f_arr = null;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_011b()
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

            f_arr = new float[3];

            // Global unmutable constants
            {
                N = rand.Next(100) + 100;
                N = 1000;

                shape = rand.Next(5);

                nTrailMin = 50 + rand.Next(25);

                switch (rand.Next(3))
                {
                    case 0: nTrailMax = 50 + rand.Next(111); break;
                    case 1: nTrailMax = 75 + rand.Next(333); break;
                    case 2: nTrailMax = 99 + rand.Next(666); break;
                }
            }

            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time local initialization
        private void initLocal()
        {
            doClearBuffer = true;
            doFillShapes = myUtils.randomChance(rand, 1, 2);
            renderDelay = 0;

            moveMode  = rand.Next(3);
            trailMode = rand.Next(4);

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
                        f_arr[0] = 02 + rand.Next(05);
                        f_arr[1] = 20 + rand.Next(50);
                    }
                    else
                    {
                        f_arr[0] = 20 + rand.Next(50);
                        f_arr[1] = 02 + rand.Next(05);
                    }
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

            string f_arrStr = $"{fStr(f_arr[0])}; {fStr(f_arr[1])}; {fStr(f_arr[2])};";

            string str = $"Obj = myObj_011b\n\n"                     +
                            $"N = {nStr(list.Count)} of {nStr(N)}\n" +
                            $"moveMode= {moveMode}\n"                +
                            $"trailMode = {trailMode}\n"             +
                            $"nTrailMin = {nTrailMin}\n"             +
                            $"nTrailMax = {nTrailMax}\n"             +
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
            int nTrail = nTrailMin + rand.Next(nTrailMax);

            A = 0.5f;
            size = rand.Next(3) + 3;

            switch (moveMode)
            {
                case 0:
                case 1:
                case 2:
                    {
                        A = myUtils.randFloat(rand, 0.1f);
                        colorPicker.getColorRand(ref R, ref G, ref B);

                        x = rand.Next(gl_Width);
                        y = gl_Height;

                        dx = 0;
                        dy = -myUtils.randFloat(rand, 0.01f) * 3;
                    }
                    break;
            }

            // Initialize Trail
            if (trail == null)
            {
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

        private void endOfLife(bool condition)
        {
            if (condition)
            {
                A -= 0.001f;

                if (A < 0)
                    generateNew();
            }
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            switch (moveMode)
            {
                // Straight bottom to top
                case 0:
                    {
                        x += dx;
                        y += dy;

                        endOfLife(y < 0);
                    }
                    break;

                // Angle rotations, speed maintained
                case 1:
                    {
                        x += dx;
                        y += dy;

                        if (dx == 0 && myUtils.randomChance(rand, 1, 33))
                        {
                            dx = dy * myUtils.randomSign(rand);
                            dy = 0;
                        }

                        if (dy == 0 && myUtils.randomChance(rand, 1, 15))
                        {
                            dy = dx < 0 ? dx : -dx;
                            dx = 0;
                        }

                        endOfLife(y < 0);
                    }
                    break;

                // Angle rotations, speed random each time
                case 2:
                    {
                        x += dx;
                        y += dy;

                        if (dx == 0 && myUtils.randomChance(rand, 1, 33))
                        {
                            dx = myUtils.randFloatSigned(rand, 0.01f) * 3;
                            dy = 0;
                        }

                        if (dy == 0 && myUtils.randomChance(rand, 1, 15))
                        {
                            dy = -myUtils.randFloat(rand, 0.01f) * 3;
                            dx = 0;
                        }

                        endOfLife(y < 0);
                    }
                    break;
            }

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
                        float extraX = myUtils.randFloat(rand) * 5;
                        float extraY = myUtils.randFloat(rand) * 5;

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
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Show()
        {
            trail.Show(R, G, B, A);

            float size2x = size * 2;

            switch (shape)
            {
                // Instanced squares
                case 0:
                    var rectInst = inst as myRectangleInst;

                    rectInst.setInstanceCoords(x - size, y - size, size2x, size2x);
                    rectInst.setInstanceColor(R, G, B, A);
                    rectInst.setInstanceAngle(angle);
                    break;

                // Instanced triangles
                case 1:
                    var triangleInst = inst as myTriangleInst;

                    triangleInst.setInstanceCoords(x, y, size2x, angle);
                    triangleInst.setInstanceColor(R, G, B, A);
                    break;

                // Instanced circles
                case 2:
                    var ellipseInst = inst as myEllipseInst;

                    ellipseInst.setInstanceCoords(x, y, size2x, angle);
                    ellipseInst.setInstanceColor(R, G, B, A);
                    break;

                // Instanced pentagons
                case 3:
                    var pentagonInst = inst as myPentagonInst;

                    pentagonInst.setInstanceCoords(x, y, size2x, angle);
                    pentagonInst.setInstanceColor(R, G, B, A);
                    break;

                // Instanced hexagons
                case 4:
                    var hexagonInst = inst as myHexagonInst;

                    hexagonInst.setInstanceCoords(x, y, size2x, angle);
                    hexagonInst.setInstanceColor(R, G, B, A);
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
                        var obj = list[i] as myObj_011b;

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
                    list.Add(new myObj_011b());
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
