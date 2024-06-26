﻿using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;


/*
    - Moving particles; each particle is connected to 5 other random particles
*/


namespace my
{
    public class myObj_0360 : myObject
    {
        // Priority
        public static int Priority => 10;
		public static System.Type Type => typeof(myObj_0360);

        private int[] others = null;

        private int nOthers, lineOpacity;
        private float x, y, dx, dy, x0, y0;
        private float size, A, R, G, B, angle = 0, radX, radY, t, dt;

        private static int N = 0, shape = 0, dir = 0, mode = 0;
        private static bool doFillShapes = false, isEllipse = true, doUseAlternativeRad = true, doUseDistAsOpacity = true;
        private static float dimAlpha = 0.05f, changeFactor = 1, distFromCenter = 0;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_0360()
        {
            others = new int[5];

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
                N = 1234;
                shape = rand.Next(5);
            }

            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time local initialization
        private void initLocal()
        {
            mode = rand.Next(4);
            dir = rand.Next(4);

            doClearBuffer = myUtils.randomBool(rand);
            dimAlpha = 0.01f + myUtils.randFloat(rand) * 0.1f;

            changeFactor = myUtils.randFloat(rand) * rand.Next(11);
            distFromCenter = myUtils.randFloat(rand);

            doUseDistAsOpacity = myUtils.randomBool(rand);

            doUseAlternativeRad = myUtils.randomBool(rand);
            isEllipse = myUtils.randomBool(rand);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            height = 800;

            string str = $"Obj = {Type}\n\n"                             	+
                            myUtils.strCountOf(list.Count, N)               +
                            $"mode = {mode}\n"                              +
                            $"shape = {shape}\n"                            +
                            $"changeFactor = {changeFactor}\n"              +
                            $"distFromCenter = {distFromCenter}\n"          +
                            $"doUseDistAsOpacity = {doUseDistAsOpacity}\n"  +
                            $"dimAlpha = {dimAlpha.ToString("0.000")}\n"    +
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
            if (myUtils.randomChance(rand, 1, 3))
            {
                lineOpacity = 1;
                nOthers = rand.Next(3) + 1;
            }
            else
            {
                lineOpacity = 0;
                nOthers = rand.Next(3);
            }

            for (int i = 0; i < nOthers; i++)
                others[i] = rand.Next(list.Count);

            x = rand.Next(gl_Width);
            y = rand.Next(gl_Height);

            size = 3;

            switch (mode)
            {
                // Straight lines
                case 0:
                    {
                        radX = radY = 0;

                        x = gl_x0 + (x - gl_x0) * distFromCenter;
                        y = gl_y0 + (y - gl_y0) * distFromCenter;

                        dx = myUtils.randomSign(rand) * myUtils.randFloat(rand, 0.5f) * (rand.Next(5) + 3);
                        dy = myUtils.randomSign(rand) * myUtils.randFloat(rand, 0.5f) * (rand.Next(5) + 3);
                    }
                    break;

                // Elliptic movement
                case 1:
                    {
                        dx = dy = 0;
                        x0 = gl_x0;
                        y0 = gl_y0;

                        radX = (float)Math.Sqrt((x - x0) * (x - x0) + (y - y0) * (y - y0));
                        radY = isEllipse ? myUtils.randFloat(rand) * radX : radX;

                        if (doUseAlternativeRad && radY < radX / 2)
                        {
                            radX = 777;
                            radY = myUtils.randFloat(rand) * radX;
                        }

                        t = myUtils.randFloat(rand) * rand.Next(1234);
                        dt = myUtils.randFloat(rand, 0.05f) * 0.01f;

                        switch (dir)
                        {
                            case 1: dt *= -1; break;
                            case 2: case 3: dt *= myUtils.randomSign(rand); break;
                        }
                    }
                    break;

                // Both lines and elliptic movement
                case 2:
                    {
                        if (myUtils.randomChance(rand, 1, 2))
                        {
                            goto case 0;
                        }
                        else
                        {
                            goto case 1;
                        }
                    }
                    break;

                // Several circles
                case 3:
                    {
                        dx = dy = 0;

                        x0 = rand.Next(gl_Width  + 666);
                        y0 = rand.Next(gl_Height + 666);

                        // Not ellipse, but combined centers instead
                        if (isEllipse)
                        {
                            x0 -= x0 % 666;
                            y0 -= y0 % 666;
                        }

                        radX = radY = rand.Next(500) + 100;

                        t = myUtils.randFloat(rand) * rand.Next(1234);
                        dt = myUtils.randFloat(rand, 0.05f) * 0.01f;

                        switch (dir)
                        {
                            case 1: dt *= -1; break;
                            case 2: case 3: dt *= myUtils.randomSign(rand); break;
                        }
                    }
                    break;
            }

            A = (float)rand.NextDouble();
            colorPicker.getColor(x, y, ref R, ref G, ref B);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            if (radX != 0 || radY != 0)
            {
                x = x0 + (float)Math.Sin(t) * radX;
                y = y0 + (float)Math.Cos(t) * radY;
                t += dt;
            }
            else
            {
                x += dx;
                y += dy;

                if (myUtils.randomChance(rand, 1, 11))
                {
                    if (myUtils.randomChance(rand, 1, 2))
                    {
                        dx += myUtils.randomSign(rand) * myUtils.randFloat(rand) * changeFactor;
                    }

                    if (myUtils.randomChance(rand, 1, 2))
                    {
                        dy += myUtils.randomSign(rand) * myUtils.randFloat(rand) * changeFactor;
                    }
                }
            }

            if (radX == 0 && radY == 0)
            {
                if (x < 0 || y < 0 || x > gl_Width || y > gl_Height)
                {
                    generateNew();
                }
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Show()
        {
            for (int i = 0; i < nOthers; i++)
            {
                var other = list[others[i]] as myObj_0360;

                if (doUseDistAsOpacity)
                {
                    float dist = (float)Math.Sqrt((x - other.x) * (x - other.x) + (y - other.y) * (y - other.y));

                    myPrimitive._LineInst.setInstanceCoords(x, y, other.x, other.y);
                    myPrimitive._LineInst.setInstanceColor(R, G, B, 100 * A / dist);
                }
                else
                {
                    myPrimitive._LineInst.setInstanceCoords(x, y, other.x, other.y);
                    myPrimitive._LineInst.setInstanceColor(R, G, B, lineOpacity == 1 ? 0.175f : 0.1f);
                }
            }

            switch (shape)
            {
                // Instanced squares
                case 0:
                    myPrimitive._RectangleInst.setInstanceCoords(x - size, y - size, 2 * size, 2 * size);
                    myPrimitive._RectangleInst.setInstanceColor(R, G, B, A);
                    myPrimitive._RectangleInst.setInstanceAngle(angle);
                    break;

                // Instanced triangles
                case 1:
                    myPrimitive._TriangleInst.setInstanceCoords(x, y, 2 * size, angle);
                    myPrimitive._TriangleInst.setInstanceColor(R, G, B, A);
                    break;

                // Instanced circles
                case 2:
                    myPrimitive._EllipseInst.setInstanceCoords(x, y, 2 * size, angle);
                    myPrimitive._EllipseInst.setInstanceColor(R, G, B, A);
                    break;

                // Instanced pentagons
                case 3:
                    myPrimitive._PentagonInst.setInstanceCoords(x, y, 2 * size, angle);
                    myPrimitive._PentagonInst.setInstanceColor(R, G, B, A);
                    break;

                // Instanced hexagons
                case 4:
                    myPrimitive._HexagonInst.setInstanceCoords(x, y, 2 * size, angle);
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

            // Disable VSYNC if needed
            // Glfw.SwapInterval(0);

            if (doClearBuffer)
            {
                glDrawBuffer(GL_FRONT_AND_BACK | GL_DEPTH_BUFFER_BIT);
                glClearColor(0, 0, 0, 1);
            }
            else
            {
                dimScreenRGB_SetRandom(0.1f);
                glDrawBuffer(GL_FRONT_AND_BACK);
            }

            while (!Glfw.WindowShouldClose(window))
            {
                processInput(window);

                // Swap fore/back framebuffers, and poll for operating system events.
                Glfw.SwapBuffers(window);
                Glfw.PollEvents();

                // Dim screen
                {
                    if (doClearBuffer)
                    {
                        glClear(GL_COLOR_BUFFER_BIT);
                    }
                    else
                    {
                        dimScreen(dimAlpha);
                    }
                }

                // Render Frame
                {
                    inst.ResetBuffer();
                    myPrimitive._LineInst.ResetBuffer();

                    int Count = list.Count;

                    for (int i = 0; i != Count; i++)
                    {
                        var obj = list[i] as myObj_0360;

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

                if (list.Count < N)
                {
                    list.Add(new myObj_0360());
                }

                cnt++;
                System.Threading.Thread.Sleep(renderDelay);
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void initShapes()
        {
            myPrimitive.init_ScrDimmer();
            myPrimitive.init_LineInst(N * 5);
            base.initShapes(shape, N, 0);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------
    }
};
