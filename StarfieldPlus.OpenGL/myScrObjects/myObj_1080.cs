using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;


/*
    - ...
*/


namespace my
{
    public class myObj_1080 : myObject
    {
        // Priority
        public static int Priority => 999910;
		public static System.Type Type => typeof(myObj_1080);

        private float x, y, dy, rad;
        private float size, a, A, R, G, B, rotAngle = 0, angle = 0, dAngle = 0;

        private static int N = 0, shape = 0, opacityMode = 0, radMode = 0, rotationMode = 0, nTrail = 100, trailMode = 0, minSize = 4, maxSize = 6, Rad = 100;
        private static bool doFillShapes = false, doAllocateAtOnce = false;
        private static float dimAlpha = 0.05f, dAngleStatic = 0.01f, rotationFactor = 0;

        private myParticleTrail trail = null;

        private static myScreenGradient grad = null;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_1080()
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
                N = 100;

                switch (rand.Next(3))
                {
                    case 0:
                        N += rand.Next(500);
                        break;

                    case 1:
                        N += rand.Next(3000);
                        break;

                    case 2:
                        N += rand.Next(10000);
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
            doAllocateAtOnce = myUtils.randomBool(rand);
            doClearBuffer = myUtils.randomBool(rand);
            doFillShapes = myUtils.randomBool(rand);
            doClearBuffer = myUtils.randomChance(rand, 19, 20);

            radMode = rand.Next(2);
            opacityMode = rand.Next(2);
            rotationMode = rand.Next(5);
            trailMode = rand.Next(2);

            renderDelay = rand.Next(3) + 3;

            rotationFactor = myUtils.randFloat(rand) * 0.05f;
            dAngleStatic = 0.01f;

            maxSize = 3 + rand.Next(5);
            minSize = rand.Next(maxSize);

            Rad = 200;

            switch (rand.Next(3))
            {
                case 0: Rad += rand.Next(100); break;
                case 1: Rad += rand.Next(300); break;
                case 2: Rad += rand.Next(500); break;
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            height = 600;

            string str = $"Obj = {Type}\n\n"                    +
                            myUtils.strCountOf(list.Count, N)   +
                            $"maxSize = {maxSize}\n"            +
                            $"minSize = {minSize}\n"            +
                            $"radMode = {radMode}\n"            +
                            $"opacityMode = {opacityMode}\n"    +
                            $"rotationMode = {rotationMode}\n"  +
                            $"renderDelay = {renderDelay}\n"    +
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
            angle = myUtils.randFloatSigned(rand) * 321;
            rotAngle = myUtils.randFloatSigned(rand) * 321;

            switch (radMode)
            {
                case 0:
                    rad = 33 + rand.Next(gl_y0 / 3);
                    break;

                case 1:
                    rad = Rad;
                    break;
            }

            switch (rotationMode)
            {
                case 0:
                case 1:
                    dAngle = 0;
                    break;

                case 2:
                    dAngle = myUtils.randFloat(rand) * rotationFactor * (+1);
                    break;

                case 3:
                    dAngle = myUtils.randFloat(rand) * rotationFactor * (-1);
                    break;

                case 4:
                    dAngle = myUtils.randFloatSigned(rand) * rotationFactor;
                    break;
            }

            x = rand.Next(gl_Width);
            y = gl_y0 + (float)Math.Sin(rotAngle) * rad;

            dy = 0;
            size = 1;

            A = myUtils.randFloat(rand, 0.25f);
            colorPicker.getColor(x, y, ref R, ref G, ref B);

            if (trail == null)
                trail = new myParticleTrail(nTrail, x, y);

            trail.updateDa(A);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            trail.update(x, y);

            var sin = (float)Math.Sin(rotAngle);
            var cos = (float)Math.Cos(rotAngle);

            y = gl_y0 + sin * rad;
            rotAngle += dAngleStatic;
            angle += dAngle;

            switch (opacityMode)
            {
                case 0:
                    a = cos > 0 ? A * 0.5f : A;
                    break;

                case 1:
                    a = A - cos * (A * 0.5f);
                    break;
            }

            size = maxSize - cos * (minSize > 0 ? minSize : 0.5f);
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

            switch (trailMode)
            {
                case 0:
                    myPrimitive._LineInst.setInstanceCoords(x, y, x, gl_y0);
                    myPrimitive._LineInst.setInstanceColor(R, G, B, 0.1f);
                    break;

                case 1:
                    if (list.Count < 2000)
                        trail.Show(R, G, B, a * 0.33f);
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

            while (doAllocateAtOnce && list.Count < N)
            {
                list.Add(new myObj_1080());
            }

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
                    myPrimitive._LineInst.ResetBuffer();

                    for (int i = 0; i != Count; i++)
                    {
                        var obj = list[i] as myObj_1080;

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
                    list.Add(new myObj_1080());
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

            myPrimitive.init_LineInst(N * nTrail);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------
    }
};
