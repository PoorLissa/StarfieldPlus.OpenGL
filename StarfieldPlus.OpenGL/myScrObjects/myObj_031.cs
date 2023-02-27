using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;


/*
    - ...

    pseudo-3d-rain
    requires an ellipse to draw the trace on the ground. Ellipse is still undeveloped :(
*/


namespace my
{
    public class myObj_031 : myObject
    {
        private int stage, depth, bottom;
        private float x, y, dx, dy;
        private float size, A, R, G, B, angle = 0;

        private static int N = 0, shape = 0;
        private static bool doFillShapes = false;
        private static float dimAlpha = 0.05f;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_031()
        {
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
                N = rand.Next(10) + 10;
                N = 3000;
                shape = 2;
            }

            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time local initialization
        private void initLocal()
        {
            doClearBuffer = myUtils.randomBool(rand);
            doClearBuffer = true;

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            height = 600;

            string nStr(int   n) { return n.ToString("N0");    }
            //string fStr(float f) { return f.ToString("0.000"); }

            string str = $"Obj = myObj_031 -- TBD: implement ellipse at last!\n\n" +
                            $"N = {nStr(list.Count)} of {nStr(N)}\n"               +
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
            stage = 0;

            // Get depth
            {
                int maxDepth = 100;

                depth = rand.Next(maxDepth) + 1;

                if (depth < maxDepth)
                {
                    depth += rand.Next(maxDepth);
                }

                if (depth > maxDepth)
                    depth = maxDepth - rand.Next(maxDepth / 2);
            }

            bottom = gl_Height - depth * 15;
            bottom += myUtils.randomSign(rand) * rand.Next(bottom/5);

            x = rand.Next(gl_Width + 200) - 100;
            y = 0;

            dx = rand.Next(3) - 1;
            dy = 500.0f / depth;

            dy = 200 / (float)Math.Log10(depth);

            size = 3 / (float)Math.Log10(depth);

            if (size > 5)
            {
                size = 3;
            }

            if (size < 1.0)
                size = 1;

            A = 2.5f * 10.0f / depth;

            if (A > 1)
                A = 1;

            if (A < 0.1f)
                A = 0.1f;

            R = 1;
            G = 1;
            B = 1;

            //colorPicker.getColor(x, y, ref R, ref G, ref B);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            switch (stage)
            {
                case 0:
                    {
                        x += dx;
                        y += dy;

                        if (y >= bottom)
                        {
                            stage = 1;
                        }
                    }
                    break;

                case 1:
                    {
                        size += 0.1f * (101 - depth);
                        A -= 0.05f * (1.0f / (depth * 0.1f));

                        if (A < 0)
                        {
                            generateNew();
                        }
                    }
                    break;
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Show()
        {
            if (stage == 0)
            {
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
            }

            if (stage == 1)
            {
                int SizeX = (int)(size * 2);
                int SizeY = (int)(size * 1);
                float a = A;

                while (SizeY > 3 && a > 0.05f)
                {
                    myPrimitive._Ellipse.SetColor(1, 1, 1, a);
                    myPrimitive._Ellipse.Draw(x - SizeX, y - SizeX, SizeX * 2, SizeY * 2, false);

                    SizeX -= SizeX / 3;
                    SizeY -= SizeY / 3;
                    a *= 0.5f;
                }
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

            doClearBuffer = false;

            if (doClearBuffer)
            {
                glDrawBuffer(GL_FRONT_AND_BACK | GL_DEPTH_BUFFER_BIT);
                glClearColor(0, 0, 0, 1);
            }
            else
            {
                dimScreenRGB_SetRandom(0.1f);
                glDrawBuffer(GL_FRONT_AND_BACK);
                //glDrawBuffer(GL_DEPTH_BUFFER_BIT);
            }

            myPrimitive._Ellipse.setLineThickness(9);

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

                    for (int i = 0; i != list.Count; i++)
                    {
                        var obj = list[i] as myObj_031;

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

                if (list.Count < N)
                {
                    list.Add(new myObj_031());
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

            myPrimitive.init_Ellipse();

            base.initShapes(shape, N, 0);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------
    }
};
