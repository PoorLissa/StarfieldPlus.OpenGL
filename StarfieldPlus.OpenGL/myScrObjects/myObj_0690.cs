﻿using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;


/*
    - Circularly moving particles with discrete curvature
*/


namespace my
{
    public class myObj_0690 : myObject
    {
        // Priority
        public static int Priority => 10;
		public static System.Type Type => typeof(myObj_0690);

        private int cnt;
        private float x, y, r, angle, dx, dy, dAngle;
        private float size, A, R, G, B;
        private myParticleTrail trail = null;

        private static int N = 0, shape = 0, nTrail = 10, rMin = 1, rMax = 100, rStep = 1, angleMode = 0, dAngleMode = 0, colorMode = 0, cntMode = 0, addMode = 0;
        private static bool doFillShapes = false, doUseTrails = false;
        private static float dimAlpha = 0.05f, gl_R = 1, gl_G = 1, gl_B = 1, dAngleStatic = 1;

        private static myScreenGradient grad = null;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_0690()
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
                doClearBuffer = myUtils.randomChance(rand, 2, 3);
                doUseTrails   = myUtils.randomChance(rand, 2, 3);
                doFillShapes  = myUtils.randomChance(rand, 1, 3);

                N = doUseTrails
                    ? 222 + rand.Next(1111)
                    : 333 + rand.Next(3333);

                shape = rand.Next(5);
            }

            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time local initialization
        private void initLocal()
        {
            dimAlpha = 0.1f;

            rMin = 100 + rand.Next(333);
            rMax = 500 + rand.Next(500);
            rStep = 50 + rand.Next(250);
            nTrail = 66 + rand.Next(333);

            colorMode = rand.Next(3);
            angleMode = rand.Next(6);
            dAngleMode = rand.Next(2);
            cntMode = rand.Next(3);
            addMode = rand.Next(5);

            dAngleStatic = 0.001f + myUtils.randFloat(rand) * 0.004f;

            // Global color
            {
                do
                {
                    R = myUtils.randFloat(rand);
                    G = myUtils.randFloat(rand);
                    B = myUtils.randFloat(rand);

                } while (R + G + B < 0.5f);
            }

            renderDelay = 0;

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
                            $"doClearBuffer = {doClearBuffer}\n"     +
                            $"doUseTrails = {doUseTrails}\n"         +
                            $"angleMode = {angleMode}\n"             +
                            $"dAngleMode = {dAngleMode}\n"           +
                            $"colorMode = {colorMode}\n"             +
                            $"cntMode = {cntMode}\n"                 +
                            $"addMode = {addMode}\n"                 +
                            $"dAngleStatic = {fStr(dAngleStatic)}\n" +
                            $"rMin = {rMin}\n"                       +
                            $"rMax = {rMax}\n"                       +
                            $"nTrail = {nTrail}\n"                   +
                            $"dimAlpha = {fStr(dimAlpha)}\n"         +
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
            r = rMin + rand.Next(rMax);
            angle = myUtils.randFloatSigned(rand) * rand.Next(123);

            x = gl_x0 + r * (float)Math.Sin(angle);
            y = gl_y0 + r * (float)Math.Cos(angle);

            switch (dAngleMode)
            {
                case 0:
                    dAngle = dAngleStatic;
                    break;

                case 1:
                    dAngle = 0.001f + myUtils.randFloat(rand) * 0.004f;
                    break;
            }

            switch (angleMode)
            {
                case 0:
                case 1:
                    dAngle *= myUtils.randomSign(rand);
                    break;

                case 2: dAngle *= +1; break;
                case 3: dAngle *= -1; break;

                case 4:
                case 5:
                    dAngle *= ((((int)r / rStep) % 2) == 0) ? +1 : -1;
                    break;
            }

            switch (cntMode)
            {
                case 0: cnt = 100 + rand.Next(100);   break;
                case 1: cnt =  50 + (int)(r * 0.10f); break;
                case 2: cnt =  50 + (int)(r * 0.11f); break;
            }

            float x2 = gl_x0 + r * (float)Math.Sin(angle + cnt * dAngle);
            float y2 = gl_y0 + r * (float)Math.Cos(angle + cnt * dAngle);

            float DX = x2 - x;
            float DY = y2 - y;

            dx = DX / cnt;
            dy = DY / cnt;

            size = 3;

            A = 0.33f + myUtils.randFloat(rand) * 0.33f;

            switch (colorMode)
            {
                case 0:
                    colorPicker.getColor(x, y, ref R, ref G, ref B);
                    break;

                case 1:
                    R = G = B = 1;
                    break;

                case 2:
                    R = gl_R;
                    G = gl_G;
                    B = gl_B;
                    break;
            }

            // Initialize Trail
            if (doUseTrails)
            {
                if (trail == null)
                    trail = new myParticleTrail(nTrail, x, y);

                trail.updateDa(A);
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            if (doUseTrails)
            {
                trail.update(x, y);
            }

            // Add some extra moving pattern:
            switch (addMode)
            {
                case 0:
                    x += (float)Math.Cos(angle) * 1;
                    break;
            }

            x += dx;
            y += dy;
            angle += dAngle;

            if (--cnt == 0)
            {
                switch (cntMode)
                {
                    case 0: cnt = 100 + rand.Next(100); break;
                    case 1: cnt = 50 + (int)(r * 0.1f); break;
                    case 2:
                        dx *= -1;
                        dy *= -1;
                        cnt = 50 + (int)(r * 0.25f);
                        return;
                }

                float x2 = gl_x0 + r * (float)Math.Sin(angle + cnt * dAngle);
                float y2 = gl_y0 + r * (float)Math.Cos(angle + cnt * dAngle);

                float DX = x2 - x;
                float DY = y2 - y;

                dx = DX / cnt;
                dy = DY / cnt;
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Show()
        {
            // Draw the trail
            if (doUseTrails)
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

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Process(Window window)
        {
            uint cnt = 0;
            initShapes();

            clearScreenSetup(doClearBuffer, 0.1f);

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
                    }

                    grad.Draw();
                }

                // Render Frame
                {
                    inst.ResetBuffer();

                    if (doUseTrails)
                        myPrimitive._LineInst.ResetBuffer();

                    for (int i = 0; i != Count; i++)
                    {
                        var obj = list[i] as myObj_0690;

                        obj.Show();
                        obj.Move();
                    }

                    if (doUseTrails)
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
                    list.Add(new myObj_0690());
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

            if (doUseTrails)
            {
                myPrimitive.init_LineInst(N * nTrail);
            }

            grad = new myScreenGradient();
            grad.SetRandomColors(rand, 0.2f);
            grad.SetOpacity(doClearBuffer ? 1 : dimAlpha);

            // grad.SetColor (0.8f, 0.8f, 0.8f, 1);
            // grad.SetColor2(0.2f, 0.2f, 0.2f, 1);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------
    }
};
