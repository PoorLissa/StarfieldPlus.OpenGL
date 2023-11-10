using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;


/*
    - Particles generated with the same movement direction, bouncing off the walls
*/


namespace my
{
    public class myObj_750 : myObject
    {
        // Priority
        public static int Priority => 10;
		public static System.Type Type => typeof(myObj_750);

        private float x, y, dx, dy;
        private float size, A, R, G, B, angle = 0;

        private static int N = 0, shape = 0, X, Y, moveMode = 0, genMode = 0, speedMode = 0;
        private static bool doFillShapes = false;
        private static float dimAlpha = 0.05f, DX, DY;

        private static myScreenGradient grad = null;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_750()
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
                N = rand.Next(10) + 99999;

                shape = rand.Next(5);

                moveMode = rand.Next(2);
                genMode = rand.Next(2);
                speedMode = rand.Next(2);
            }

            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time local initialization
        private void initLocal()
        {
            doClearBuffer = myUtils.randomBool(rand);

            renderDelay = rand.Next(2) + 1;

            X = rand.Next(gl_Width);
            Y = rand.Next(gl_Height);

            DX = 0.01f + myUtils.randFloatSigned(rand) * 13.0f;
            DY = 0.01f + myUtils.randFloatSigned(rand) * 13.0f;

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            height = 600;

            string nStr(int   n) { return n.ToString("N0");    }
            //string fStr(float f) { return f.ToString("0.000"); }

            string str = $"Obj = {Type}\n\n"                         +
                            $"N = {nStr(list.Count)} of {nStr(N)}\n" +
                            $"genMode = {genMode}\n"                 +
                            $"moveMode = {moveMode}\n"               +
                            $"speedMode = {speedMode}\n"             +
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
            x = X;
            y = Y;

            switch (genMode)
            {
                case 0:
                    dx = DX;
                    dy = DY;
                    break;

                case 1:
                    dx = myUtils.randomChance(rand, 1, 2) ? +DX : -DX;
                    dy = myUtils.randomChance(rand, 1, 2) ? +DY : -DY;
                    break;
            }

            switch (speedMode)
            {
                case 0:
                    break;

                case 1:
                    {
                        float spd = 0.5f + myUtils.randFloat(rand) * 0.5f;
                        dx *= spd;
                        dy *= spd;
                    }
                    break;
            }

            size = rand.Next(3) + 3;

            A = 0.1f + myUtils.randFloat(rand) * 0.5f;
            colorPicker.getColor(x, y, ref R, ref G, ref B);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            x += dx;
            y += dy;

            switch (moveMode)
            {
                case 0:
                    {
                        if (x < 0 && dx < 0)
                            dx *= -1;

                        if (y < 0 && dy < 0)
                            dy *= -1;

                        if (x > gl_Width && dx > 0)
                            dx *= -1;

                        if (y > gl_Height && dy > 0)
                            dy *= -1;
                    }
                    break;

                case 1:
                    {
                        float dd = 0.1f;

                        if (x < 0)
                            dx += dd;

                        if (y < 0)
                            dy += dd;

                        if (x > gl_Width)
                            dx -= dd;

                        if (y > gl_Height)
                            dy -= dd;
                    }
                    break;
            }

            if (myUtils.randomChance(rand, 1, 23))
            {
                colorPicker.getColor(x, y, ref R, ref G, ref B);
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Show()
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
                        var obj = list[i] as myObj_750;

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

                if (Count < N)
                {
                    list.Add(new myObj_750());
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
            grad.SetRandomColors(rand, 0.2f, 0);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------
    }
};
