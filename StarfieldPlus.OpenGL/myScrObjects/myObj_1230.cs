using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;


/*
    - 
*/


namespace my
{
    public class myObj_1230 : myObject
    {
        // Priority
        public static int Priority => 10;
		public static System.Type Type => typeof(myObj_1230);

        private bool isAlive;
        private float x, y, dx, dy;
        private float size, A, R, G, B, angle = 0, dAngle;

        private static int N = 0, shape = 0, mode = 0, alive;
        private static bool doFillShapes = false;
        private static float dimAlpha = 0.05f, alpha = 0, dAlpha = 0, aMin = 0, aMax = 0, spd = 0, t = 0;

        private static myObj_1230 gen = null;
        private static myScreenGradient grad = null;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_1230()
        {
            if (id != uint.MaxValue)
                generateNew();

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
                N = 3000;

                shape = rand.Next(5);
            }

            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time local initialization
        private void initLocal()
        {
            doClearBuffer = myUtils.randomChance(rand, 19, 20);
            doFillShapes = myUtils.randomChance(rand, 2, 3);

            mode = rand.Next(4);
            mode = 0;

            spd = 1.0f + myUtils.randFloat(rand) * rand.Next(5);
            dAlpha = 0.01f + myUtils.randFloat(rand) * 0.1f;

            dAlpha /= 3;

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            height = 600;

            string str = $"Obj = {Type}\n\n"                     +
                            myUtils.strCountOf(list.Count, N)    +
                            $"alive = {alive}\n"                 +
                            $"spd = {myUtils.fStr(spd)}\n"       +
                            $"dAlpha = {myUtils.fStr(dAlpha)}\n" +
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
                gen = this;

                switch (mode)
                {
                    case 0:
                        aMin = 0;
                        aMax = (float)Math.PI;
                        alpha = aMin;

                        x = gl_x0;
                        y = gl_Height;
                        break;
                }
            }
            else
            {
                isAlive = true;

                x = gen.x;
                y = gen.y;

                float ALPHA = alpha;

                switch (mode)
                {
                    case 0:
                        ALPHA += (float)Math.PI;
                        break;
                }

                dx = spd * (float)Math.Cos(ALPHA);
                dy = spd * (float)Math.Sin(ALPHA);

                size = 3;
                dAngle = myUtils.randFloat(rand) * 0.001f;

                A = 0.5f + myUtils.randFloat(rand) * 0.5f;
                R = (float)rand.NextDouble();
                G = (float)rand.NextDouble();
                B = (float)rand.NextDouble();

                R = G = B = 0.33f;
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            if (id == 0)
            {
                alpha += dAlpha;

                int Count = list.Count;
                int cnt = 0;

                for (int i = 1; i < Count; i++)
                {
                    var obj = list[i] as myObj_1230;

                    if (obj.isAlive == false)
                    {
                        obj.generateNew();
                        
                        if (++cnt == 3)
                            break;
                    }
                }

                if (alpha > aMax && dAlpha > 0)
                    dAlpha *= -1;

                if (alpha < aMin && dAlpha < 0)
                    dAlpha *= -1;

                //dAlpha += (float)Math.Sin(t) * 0.001f;
                t += 0.001f;

            }
            else
            {
                x += dx;
                y += dy;

                angle += dAngle;

                //colorPicker.getColor(x, y, ref R, ref G, ref B);

                if (x < 0 || x > gl_Width || y < 0 || y > gl_Height)
                {
                    isAlive = false;
                }
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Show()
        {
            if (isAlive)
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
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Process(Window window)
        {
            uint cnt = 0;
            initShapes();

            clearScreenSetup(doClearBuffer, 0.1f);

            while (list.Count < 100)
            {
                list.Add(new myObj_1230());
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
                    alive = 0;
                    inst.ResetBuffer();

                    for (int i = 0; i != Count; i++)
                    {
                        var obj = list[i] as myObj_1230;

                        obj.Show();
                        obj.Move();

                        alive += obj.isAlive ? 1 : 0;
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
                    list.Add(new myObj_1230());
                }

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
