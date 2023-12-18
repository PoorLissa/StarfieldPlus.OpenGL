using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;


/*
    - Pseudo 3d based off myObj_690
*/


namespace my
{
    public class myObj_691 : myObject
    {
        // Priority
        public static int Priority => 10;
		public static System.Type Type => typeof(myObj_691);

        private int dir;
        private float x, y, r, yRad, angle, dAngle;
        private float size, A, R, G, B;
        private myParticleTrail trail = null;

        private static int N = 0, shape = 0, nTrail = 10, rMin = 1, rMax = 100, rStep = 1, gl_Dir = 0;
        private static int angleMode = 0, dAngleMode1 = 0, dAngleMode2 = 0, colorMode = 0, cntMode = 0, addMode = 0, drawMode = 0;
        private static bool doFillShapes = false;
        private static float dimAlpha = 0.05f, gl_R = 1, gl_G = 1, gl_B = 1, dAngleStatic = 1;

        private static myScreenGradient grad = null;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_691()
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
                doFillShapes  = myUtils.randomChance(rand, 1, 3);

                drawMode = rand.Next(8);

                switch (drawMode)
                {
                    // No trail modes:
                    case 00:
                    case 01:
                    case 02:
                        N = 333 + rand.Next(3333);
                        break;

                    // Trail modes:
                    case 03:
                    case 04:
                    case 05:
                        N = 222 + rand.Next(1111);
                        break;

                    // Ray modes:
                    case 06:
                    case 07:
                        N = 666 + rand.Next(3333);
                        break;
                }

                shape = rand.Next(5);
            }

            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time local initialization
        private void initLocal()
        {
            dimAlpha = 0.1f;

            rMin = 100;
            rMax = 333 + gl_y0 / (myUtils.randomChance(rand, 1, 2) ? 1 : 2);
            rStep = 50 + rand.Next(250);
            nTrail = 66 + rand.Next(333);

            colorMode = rand.Next(3);
            angleMode = rand.Next(2);
            dAngleMode1 = rand.Next(2);
            dAngleMode2 = rand.Next(6);
            cntMode = rand.Next(3);
            addMode = rand.Next(5);

            gl_Dir = myUtils.randomSign(rand);

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
                            $"drawMode = {drawMode}\n"               +
                            $"angleMode = {angleMode}\n"             +
                            $"dAngleMode1 = {dAngleMode1}\n"         +
                            $"dAngleMode2 = {dAngleMode2}\n"         +
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
            switch (angleMode)
            {
                case 0:
                    angle = 0;
                    break;

                case 1:
                    angle = myUtils.randFloatSigned(rand) * rand.Next(123);
                    break;
            }

            x = gl_x0 + r * (float)Math.Sin(angle);
            y = gl_y0 + r * (float)Math.Cos(angle);

            r = rMax;
            y = gl_y0 + rand.Next((int)r) * myUtils.randomSign(rand);
            yRad = (float)Math.Sqrt(r * r - (gl_y0 - y) * (gl_y0 - y));

            switch (dAngleMode1)
            {
                case 0:
                    dAngle = dAngleStatic;
                    break;

                case 1:
                    dAngle = 0.001f + myUtils.randFloat(rand) * 0.004f;
                    break;
            }

            switch (dAngleMode2)
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

            switch (drawMode)
            {
                // No trail modes:
                case 00:
                    size = 3;
                    break;

                case 01:
                    size = 3 + rand.Next(13);
                    break;

                case 02:
                    size = 3 + rand.Next(23);
                    break;

                // Trail modes:
                case 03:
                case 04:
                case 05:
                    size = 3;
                    break;

                // Ray modes:
                case 06:
                case 07:
                    size = 2;
                    break;
            }

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
            switch (drawMode)
            {
                // Trail modes:
                case 03:
                case 04:
                case 05:
                    {
                        if (trail == null)
                            trail = new myParticleTrail(nTrail, x, y);

                        trail.updateDa(A);
                    }
                    break;
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            switch (drawMode)
            {
                // Trail modes:
                case 03:
                case 04:
                case 05:
                    trail.update(x, y);
                    break;
            }

            angle += dAngle;

            float xNew = gl_x0 + (float)Math.Sin(angle) * yRad;
            dir = xNew > x ? 1 : -1;
            x = xNew;

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Show()
        {
            float a = dir == gl_Dir ? A : A/2;

            switch (drawMode)
            {
                // No trail modes:
                case 00:
                case 01:
                case 02:
                    break;

                // Trail modes:
                case 03:
                case 04:
                case 05:
                    trail.Show(R, G, B, a);
                    break;

                // Ray modes:
                case 06:
                    myPrimitive._LineInst.setInstanceCoords(x, y, gl_x0, gl_y0);
                    myPrimitive._LineInst.setInstanceColor(R, G, B, a * 0.2f);
                    break;

                case 07:
                    myPrimitive._LineInst.setInstanceCoords(x, y, gl_x0, gl_y0);
                    myPrimitive._LineInst.setInstanceColor(R, G, B, a * 0.2f);
                    return;
            }

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
                    myPrimitive._TriangleInst.setInstanceCoords(x, y, size2x, angle);
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
                    myPrimitive._LineInst.ResetBuffer();

                    for (int i = 0; i != Count; i++)
                    {
                        var obj = list[i] as myObj_691;

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
                    list.Add(new myObj_691());
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

            switch (drawMode)
            {
                // No trail modes:
                case 00:
                case 01:
                case 02:
                    myPrimitive.init_LineInst(1);
                    break;

                // Trail modes:
                case 03:
                case 04:
                case 05:
                    myPrimitive.init_LineInst(N * nTrail);
                    break;

                // Ray modes:
                case 06:
                case 07:
                    myPrimitive.init_LineInst(N);
                    break;
            }

            grad = new myScreenGradient();
            grad.SetRandomColors(rand, 0.2f);
            grad.SetOpacity(doClearBuffer ? 1 : dimAlpha);

            // grad.SetColor (0.9f, 0.9f, 0.9f, 1);
            // grad.SetColor2(0.1f, 0.1f, 0.1f, 1);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------
    }
};
