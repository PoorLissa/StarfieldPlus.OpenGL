using GLFW;
using static OpenGL.GL;
using System.Collections.Generic;


/*
    - Periodic vertical or horizontal waves of particles
*/


namespace my
{
    public class myObj_0740 : myObject
    {
        // Priority
        public static int Priority => 10;
		public static System.Type Type => typeof(myObj_0740);

        private float x, y, dx, dy;
        private float size, A, R, G, B, angle = 0;

        private static int N = 0, n = 0, shape = 0, mode = 0, startMode = 0;
        private static bool doFillShapes = false;
        private static float dimAlpha = 0.05f, step = 1;

        private static myScreenGradient grad = null;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_0740()
        {
            if (id != uint.MaxValue)
                generateNew();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time global initialization
        protected override void initGlobal()
        {
            var colorMode = myUtils.randomChance(rand, 1, 3)
                ? myColorPicker.colorMode.RANDOM_MODE
                : myColorPicker.colorMode.SNAPSHOT_OR_IMAGE;

            colorPicker = new myColorPicker(gl_Width, gl_Height, mode: colorMode);
            list = new List<myObject>();

            // Global unmutable constants
            {
                N = rand.Next(10) + 99999;

                shape = rand.Next(5);

                mode = rand.Next(2);
                startMode = rand.Next(2);

                n = 10 + rand.Next(99);

                step = 0.05f + myUtils.randFloat(rand) * 0.075f;

                dimAlpha = 0.1f;
            }

            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time local initialization
        private void initLocal()
        {
            doClearBuffer = myUtils.randomChance(rand, 1, 2);

            renderDelay = rand.Next(1) + 1;

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
                            $"n = {n}\n"                             +
                            $"step = {fStr(step)}\n"                 +
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
            switch (mode)
            {
                case 0:
                    x = startMode == 0 ? gl_x0 : rand.Next(gl_Width);
                    y = gl_y0;

                    dy = step * (rand.Next(n) + 1) * myUtils.randomSign(rand);

                    dx = startMode == 0
                        ? 0.01f * (rand.Next(111)) * myUtils.randomSign(rand)
                        : 0.01f * (rand.Next( 11)) * myUtils.randomSign(rand);
                    break;

                case 1:
                    x = gl_x0;
                    y = startMode == 0 ? gl_y0 : rand.Next(gl_Height);

                    dx = step * (rand.Next(n) + 1) * myUtils.randomSign(rand);

                    dy = startMode == 0
                        ? 0.01f * (rand.Next(111)) * myUtils.randomSign(rand)
                        : 0.01f * (rand.Next( 11)) * myUtils.randomSign(rand);
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

            if (x < 0 && dx < 0)
            {
                dx *= -1;
            }

            if (y < 0 && dy < 0)
            {
                dy *= -1;
            }

            if (x > gl_Width && dx > 0)
            {
                dx *= -1;
            }

            if (y > gl_Height && dy > 0)
            {
                dy *= -1;
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


            while (list.Count < N)
                list.Add(new myObj_0740());


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
                        var obj = list[i] as myObj_0740;

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
                    for (int i = 0; i < 10; i++)
                        if (list.Count < N)
                            list.Add(new myObj_0740());
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
