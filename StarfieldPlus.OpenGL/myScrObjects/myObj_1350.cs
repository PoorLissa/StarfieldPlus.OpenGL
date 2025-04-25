using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;


/*
    - 
*/


namespace my
{
    public class myObj_1350 : myObject
    {
        // Priority
        public static int Priority => 10;
		public static System.Type Type => typeof(myObj_1350);

        private int dir;
        private float x, y, d, alpha, dAlpha;
        private float size, A, R, G, B, angle = 0;

        private static int N = 0, shape = 0, mode = 0, dirMode = 0, dMode = 0;
        private static bool doFillShapes = false;
        private static float dimAlpha = 0.05f, Rad = 0, theta = 0;

        private static myScreenGradient grad = null;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_1350()
        {
            if (id != uint.MaxValue)
                generateNew();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time global initialization
        protected override void initGlobal()
        {
            var clrMode = myUtils.randomChance(rand, 1, 5)
                ? myColorPicker.colorMode.RANDOM_MODE
                : myColorPicker.colorMode.SNAPSHOT_OR_IMAGE;

            colorPicker = new myColorPicker(gl_Width, gl_Height, mode: clrMode);
            list = new List<myObject>();

            // Global unmutable constants
            {
                N = 200000 + rand.Next(123456);

                shape = rand.Next(5);
            }

            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time local initialization
        private void initLocal()
        {
            doClearBuffer = myUtils.randomChance(rand, 1, 2);
            doFillShapes = myUtils.randomChance(rand, 1, 2);

            mode = rand.Next(2);
            dirMode = rand.Next(3);
            dMode = rand.Next(2);

            Rad = 0;

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            height = 600;

            string str = $"Obj = {Type}\n\n"                     +
                            myUtils.strCountOf(list.Count, N)    +
                            $"doClearBuffer = {doClearBuffer}\n" +
                            $"doFillShapes = {doFillShapes}\n"   +
                            $"mode = {mode}\n"                   +
                            $"dirMode = {dirMode}\n"             +
                            $"dMode = {dMode}\n"                 +
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
            size = rand.Next(3) + 1;

            A = 0.45f + myUtils.randFloat(rand) * 0.25f;

            do
            {
                d = 1 + rand.Next(gl_x0 + 100);
                alpha = myUtils.randFloat(rand) * 321;

                x = gl_x0 + d * (float)Math.Sin(alpha);
                y = gl_y0 + d * (float)Math.Cos(alpha);
            }
            while (x < 0 || y < 0 || x > gl_Width || y > gl_Height);

            dAlpha = d * 0.00001f;
            dAlpha = 1 * 0.0005f;
            angle = alpha;

            switch (dirMode)
            {
                case 0:
                case 1:
                    dir = dirMode;
                    break;

                case 2:
                    dir = rand.Next(2);
                    break;
            }

            colorPicker.getColor(x, y, ref R, ref G, ref B);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            switch (mode)
            {
                case 0:
                    {
                        if (d < Rad)
                        {
                            x = gl_x0 + d * (float)Math.Sin(alpha);
                            y = gl_y0 + d * (float)Math.Cos(alpha);
                            alpha += dir == 0 ? dAlpha : -dAlpha;
                        }
                    }
                    break;

                case 1:
                    {
                        if (Math.Abs(Rad - d) < 33.0)
                        {
                            x = gl_x0 + d * (float)Math.Sin(alpha);
                            y = gl_y0 + d * (float)Math.Cos(alpha);
                            alpha += dir == 0 ? dAlpha * 3 : -dAlpha * 3;
                        }
                    }
                    break;
            }

            switch (dMode)
            {
                case 0:
                    break;

                case 1:
                    alpha += dir == 0 ? d * 0.00001f : d * -0.00001f;
                    break;
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Show()
        {
            float size2x = size * 2;

            angle = alpha;

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

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Process(Window window)
        {
            uint cnt = 0;
            initShapes();

            clearScreenSetup(doClearBuffer, 0.1f);

            while (list.Count < N)
            {
                list.Add(new myObj_1350());
            }

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
                        var obj = list[i] as myObj_1350;

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

                theta += 0.0005f;
                Rad = (float)Math.Abs(Math.Sin(theta)) * gl_x0;

                stopwatch.WaitAndRestart();
                cnt++;
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
