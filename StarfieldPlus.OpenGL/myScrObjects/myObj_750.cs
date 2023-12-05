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
        private float size, a, A, R, G, B, angle = 0, dAngle;

        private static int N = 0, shape = 0, offset, X, Y, moveMode = 0, genMode = 0, speedMode = 0, angleMode = 0;
        private static bool doFillShapes = false, doAllocateAll = false;
        private static float dimAlpha = 0.05f, DX, DY, speedFactor;

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

                moveMode = rand.Next(3);
                genMode = rand.Next(2);
                speedMode = rand.Next(3);
                angleMode = rand.Next(3);
            }

            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time local initialization
        private void initLocal()
        {
            doClearBuffer = myUtils.randomBool(rand);
            doAllocateAll = speedMode > 0 && myUtils.randomChance(rand, 1, 3);

            renderDelay = rand.Next(2);

            X = rand.Next(gl_Width);
            Y = rand.Next(gl_Height);

            speedFactor = 11.0f + myUtils.randFloat(rand) * 22.0f;

            DX = 0.01f + myUtils.randFloatSigned(rand) * speedFactor;
            DY = 0.01f + myUtils.randFloatSigned(rand) * speedFactor;

            offset = rand.Next(gl_y0/2);

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
                            $"genMode = {genMode}\n"                 +
                            $"moveMode = {moveMode}\n"               +
                            $"speedMode = {speedMode}\n"             +
                            $"angleMode = {angleMode}\n"             +
                            $"DX = {fStr(Math.Abs(DX))}\n"           +
                            $"DY = {fStr(Math.Abs(DY))}\n"           +
                            $"offset = {offset}\n"                   +
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

            float spd = 1.0f;

            switch (speedMode)
            {
                case 0:
                    spd = 1.0f;
                    break;

                case 1:
                    spd = 0.5f + myUtils.randFloat(rand) * 0.5f;
                    break;

                case 2:
                    spd = 0.1f * (rand.Next(11) + 1);
                    break;
            }

            dx *= spd;
            dy *= spd;

            switch (angleMode)
            {
                case 0:
                    angle = 0;
                    break;

                case 1:
                    angle = myUtils.randFloatSigned(rand) * rand.Next(123);
                    break;

                case 2:
                    angle = 0;
                    dAngle = myUtils.randFloatSigned(rand) * 0.01f;
                    break;
            }

            size = rand.Next(3) + 3;

            A = 0.1f + myUtils.randFloat(rand) * 0.5f;
            a = 0;
            colorPicker.getColor(x, y, ref R, ref G, ref B);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            x += dx;
            y += dy;
            angle += dAngle;

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

                        if (x < offset)
                            dx += dd;

                        if (y < offset)
                            dy += dd;

                        if (x > gl_Width - offset)
                            dx -= dd;

                        if (y > gl_Height - offset)
                            dy -= dd;
                    }
                    break;

                case 2:
                    {
                        if (x < 0)
                        {
                            x = gl_Width;
                            y += y % 100;
                        }

                        if (y < 0)
                        {
                            y = gl_Height;
                            x += x % 100;
                        }

                        if (x > gl_Width)
                        {
                            x = 0;
                            y -= y % 100;
                        }

                        if (y > gl_Height)
                        {
                            y = 0;
                            x -= x % 100;
                        }
                    }
                    break;
            }

            if (myUtils.randomChance(rand, 1, 23))
            {
                colorPicker.getColor(x, y, ref R, ref G, ref B);
            }

            if (a < A)
            {
                a += myUtils.randFloat(rand) * 0.005f;
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


            if (doAllocateAll)
                while (list.Count < N)
                    list.Add(new myObj_750());


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
            grad.SetRandomColors(rand, 0.2f);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------
    }
};
