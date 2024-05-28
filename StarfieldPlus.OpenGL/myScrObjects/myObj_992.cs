using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Collections;
using System.Xml.Linq;


/*
    - ...
*/


namespace my
{
    public class myObj_992 : myObject
    {
        // Priority
        public static int Priority => 999910;
		public static System.Type Type => typeof(myObj_992);

        private int cnt;
        private bool isAlive;
        private float x, y, w, h;
        private float A, R, G, B, angle = 0;

        private static int N = 0, shape = 0, colorMode = 0, maxCnt = 1, colorCnt = 0;
        private static bool doFillShapes = false, doMoveCenter = false;
        private static float dimAlpha = 0.05f, whRatio = 1, t = 0, dt = 0, ytFactor = 0;

        private static myScreenGradient grad = null;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_992()
        {
            if (id != uint.MaxValue)
                generateNew();

            if (id > 0)
                isAlive = false;
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time global initialization
        protected override void initGlobal()
        {
            colorPicker = new myColorPicker(gl_Width, gl_Height);
            list = new List<myObject>();

            // Global unmutable constants
            {
                N = 333;

                whRatio = 1.0f + myUtils.randFloat(rand) * 3;

                maxCnt = 33;
                colorCnt = 33;

                dt = 0.1f * (rand.Next(5) + 1);

                shape = myUtils.randomChance(rand, 4, 5) ? 2 : rand.Next(5);
                shape = rand.Next(5);

                ytFactor = myUtils.randFloat(rand) + rand.Next(3);
            }

            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time local initialization
        private void initLocal()
        {
            doClearBuffer = myUtils.randomBool(rand);
            doClearBuffer = true;
            doFillShapes = true;
            doMoveCenter = myUtils.randomChance(rand, 4, 5);

            colorMode = doMoveCenter ? 0 : rand.Next(2);

            renderDelay = rand.Next(3) + 1;

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
                            $"shape = {shape}\n"                     +
                            $"colorMode = {colorMode}\n"             +
                            $"doMoveCenter = {doMoveCenter}\n"       +
                            $"whRatio = {fStr(whRatio)}\n"           +
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
            if (id == 0)
            {
                isAlive = false;

                cnt = maxCnt;

                h = 50;
                w = h;

                x = gl_x0;
                y = gl_y0;

                R = (float)rand.NextDouble();
                G = (float)rand.NextDouble();
                B = (float)rand.NextDouble();
            }
            else
            {
                var parent = list[0] as myObj_992;

                isAlive = true;

                angle = myUtils.randFloatSigned(rand) * rand.Next(123);

                x = parent.x;
                y = parent.y;
                w = parent.w;
                h = parent.h;

                A = 0.25f;

                switch (colorMode)
                {
                    case 0:
                        R = (float)rand.NextDouble();
                        G = (float)rand.NextDouble();
                        B = (float)rand.NextDouble();
                        break;

                    case 1:
                        if (--colorCnt == 0)
                        {
                            parent.R = (float)rand.NextDouble();
                            parent.G = (float)rand.NextDouble();
                            parent.B = (float)rand.NextDouble();

                            R = parent.R;
                            G = parent.G;
                            B = parent.B;

                            colorCnt = 11 + rand.Next(23);
                        }
                        else
                        {
                            R = parent.R + myUtils.randFloatSigned(rand) * 0.1f;
                            G = parent.G + myUtils.randFloatSigned(rand) * 0.1f;
                            B = parent.B + myUtils.randFloatSigned(rand) * 0.1f;
                        }
                        break;
                }
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            if (id == 0)
            {
                if (--cnt == 0)
                {
                    cnt = maxCnt;

                    if (doMoveCenter)
                    {
                        x += 50 * (float)Math.Cos(t);
                        y += 33 * (float)Math.Cos(t * ytFactor);
                        t += dt;
                    }

                    for (int i = 1; i < list.Count; i++)
                    {
                        var obj = list[i] as myObj_992;

                        if (obj.isAlive == false)
                        {
                            obj.generateNew();
                            break;
                        }
                    }
                }
            }
            else
            {
                if (isAlive)
                {
                    float factor = 1.001f;

                    w *= factor;
                    h *= factor;

                    if (w > gl_x0 || h > gl_y0)
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
                        myPrimitive._RectangleInst.setInstanceCoords(x - w, y - h, w * 2, h * 2);
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
                        myPrimitive._EllipseInst.setInstanceCoords(x, y, w * 2, angle);
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
                        var obj = list[i] as myObj_992;

                        obj.Show();
                        obj.Move();
                    }

                    if (doFillShapes)
                    {
                        // Tell the fragment shader to multiply existing instance opacity by 0.5:
                        inst.SetColorA(-0.1f);
                        inst.Draw(true);
                    }

                    // Tell the fragment shader to do nothing with the existing instance opacity:
                    inst.SetColorA(0);
                    inst.Draw(false);
                }

                if (Count < N)
                {
                    list.Add(new myObj_992());
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
            base.initShapes(shape, N, 0);

            grad = new myScreenGradient();
            grad.SetRandomColors(rand, 0.2f);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------
    }
};
