using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;


/*
    - 
*/


namespace my
{
    public class myObj_1460 : myObject
    {
        // Priority
        public static int Priority => 10;
		public static System.Type Type => typeof(myObj_1460);

        private float x, y, dx, dy;
        private float size, A, R, G, B, a, r, g, b, angle = 0;

        private myParticleTrail trail = null;

        private static int N = 0, shape = 0, nTrail = 1;
        private static bool doFillShapes = false;
        private static float dimAlpha = 0.05f, theta = 0, t = 0, dt = 0.0001f;
        private static float sA = 0, sR = 0, sG = 0, sB = 0;

        private static myScreenGradient grad = null;
        private static myFreeShader_001 shader = null;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_1460()
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
                N = 333;

                shape = rand.Next(5);
            }

            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time local initialization
        private void initLocal()
        {
            doClearBuffer = myUtils.randomChance(rand, 99, 111);

            nTrail = 350;
            nTrail = 50 + rand.Next(300);

            theta = 31;

            do {

                sR = myUtils.randFloat(rand);
                sG = myUtils.randFloat(rand);
                sB = myUtils.randFloat(rand);

            }
            while (sR + sG + sB < 1);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            height = 600;

            string str = $"Obj = {Type}\n\n"                  +
                            myUtils.strCountOf(list.Count, N) +
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
            x = rand.Next(gl_Width + 500);
            y = -10;

            dx = -0.50f;
            dy = +1.5f;

            float spd = (2 + rand.Next(4)) + myUtils.randFloat(rand);

            float Theta = theta;
            //Theta += myUtils.randFloatSigned(rand) * 0.05f;

            dx = (float)Math.Sin(Theta) * spd;
            dy = (float)Math.Cos(Theta) * spd;

            size = 2;

            A = 0.5f + myUtils.randFloat(rand) * 0.5f;
            //colorPicker.getColor(x, y, ref R, ref G, ref B);

            R = 0.75f;
            G = 0.75f;
            B = 0.75f;

            r = sR + (float)Math.Sin(x/gl_Width) * 0.5f;

            r = (float)Math.Sin(x / gl_Width);

            g = sG;
            b = sB;
            a = A;

            // Initialize Trail
            {
                if (trail == null)
                {
                    trail = new myParticleTrail(nTrail, x, y);
                }
                else
                {
                    trail.reset(x, y);
                }

                trail?.updateDa(A * 0.75f);
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            x += dx;
            y += dy;

            trail.update(x, y);

            //if (x < 0 || x > gl_Width || y < 0 || y > gl_Height)
            if (y > gl_Height)
            {
                a -= 0.005f;

                if (a < 0)
                    generateNew();
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Show()
        {
            float size2x = size * 2;

            trail.Show(r, g, b, a);

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
                    myPrimitive._LineInst.ResetBuffer();

                    shader.SetColor(0.75f, 0.23f, 0.66f, 0.25f);
                    shader.Draw(gl_Width, -111, 2345, 2345, 0.75f, 500);

                    for (int i = 0; i != Count; i++)
                    {
                        var obj = list[i] as myObj_1460;

                        obj.Show();
                        obj.Move();
                    }

                    myPrimitive._LineInst.setLineWidth(3);
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

                if (Count < N && myUtils.randomChance(rand, 1, 5))
                {
                    list.Add(new myObj_1460());
                }

                stopwatch.WaitAndRestart();
                cnt++;

                if (myUtils.randomChance(rand, 1, 1111))
                {
                    do
                    {

                        sR = myUtils.randFloat(rand);
                        sG = myUtils.randFloat(rand);
                        sB = myUtils.randFloat(rand);

                    }
                    while (sR + sG + sB < 1);
                }

                //theta += (float)Math.Sin(t) * 0.01f;
                t += dt;
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void initShapes()
        {
            myPrimitive.init_ScrDimmer();
            base.initShapes(shape, N, 0);

            myPrimitive.init_LineInst(N * nTrail);

            getShader();

            grad = new myScreenGradient();
            grad.SetRandomColors(rand, 0.2f);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        // General shader selector
        private void getShader()
        {
            string fHeader = "", fMain = "";

            switch (rand.Next(2))
            {
                case 0:
                    myFreeShader_001.getShader_000(ref fHeader, ref fMain);
                    break;

                case 1:
                    myFreeShader_001.getShader_001(ref fHeader, ref fMain);
                    break;
            }

            shader = new myFreeShader_001(fHeader, fMain);
        }

        // ---------------------------------------------------------------------------------------------------------------

    }
};
