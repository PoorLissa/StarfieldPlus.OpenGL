using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;


/*
    - Moving Shooters vs static Targets
*/


namespace my
{
    public class myObj_0510 : myObject
    {
        // Priority
        public static int Priority => 10;
        public static System.Type Type => typeof(myObj_0510);

        private int   rcvId;
        private float x, y, X, Y, dx, dy;
        private float size, A, R, G, B, angle = 0;

        private myParticleTrail trail = null;

        private static int N = 0, shape = 0, nSrc = 0, nRcv = 0, nTrail = 10, bulletSpdFactor = 0;
        private static bool doFillShapes = false, doUseSingletarget = true;
        private static float dimAlpha = 0.05f;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_0510()
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
                nSrc = rand.Next(25) + 3;
                nRcv = rand.Next(25) + 3;

                N = nSrc + nRcv + 1000;

                shape = rand.Next(5);

                nTrail = 10 + rand.Next(100);
            }

            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time local initialization
        private void initLocal()
        {
            doClearBuffer = true;
            doFillShapes  = myUtils.randomBool(rand);
            doUseSingletarget = myUtils.randomChance(rand, 2, 3);

            bulletSpdFactor = 1 + rand.Next(15);

            float _R = 0, _G = 0, _B = 0;

            // Source nodes
            {
                myUtils.getRandomColor(rand, ref _R, ref _G, ref _B, min: 0.5f);

                for (int i = 0; i < nSrc; i++)
                {
                    if (list.Count < i + 1)
                    {
                        list.Add(new myObj_0510());
                    }

                    myObj_0510 obj = list[i] as myObj_0510;

                    obj.rcvId = rand.Next(nRcv) + nSrc;

                    obj.x = rand.Next(gl_Width);
                    obj.y = rand.Next(gl_Height);

                    obj.dx = 0;
                    obj.dy = 0;

                    obj.dx = myUtils.randFloat(rand, 0.1f) * 1.5f;
                    obj.dy = myUtils.randFloat(rand, 0.1f) * 1.5f;

                    obj.size = 10;
                    obj.R = _R;
                    obj.G = _G;
                    obj.B = _B;
                    obj.A = 0.5f;
                }
            }

            // Receiving nodes
            {
                myUtils.getRandomColor(rand, ref _R, ref _G, ref _B, min: 0.5f);

                for (int i = nSrc; i < nSrc + nRcv; i++)
                {
                    if (list.Count < i + 1)
                    {
                        list.Add(new myObj_0510());
                    }

                    myObj_0510 obj = list[i] as myObj_0510;

                    obj.rcvId = -1;

                    obj.x = rand.Next(gl_Width);
                    obj.y = rand.Next(gl_Height);
                    obj.dx = obj.dy = 0;

                    obj.size = 10;
                    obj.R = _R;
                    obj.G = _G;
                    obj.B = _B;
                    obj.A = 0.5f;
                }
            }

            renderDelay = 1;

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            height = 600;

            string nStr(int   n) { return n.ToString("N0");    }
            //string fStr(float f) { return f.ToString("0.000"); }

            string str = $"Obj = {Type}\n\n"                          	 +
                            $"N = {nStr(list.Count)} of {nStr(N)}\n"     +
                            $"doUseSingletarget = {doUseSingletarget}\n" +
                            $"nSrc = {nSrc}\n"                           +
                            $"nRcv = {nRcv}\n"                           +
                            $"nTrail = {nTrail}\n"                       +
                            $"bulletSpdFactor = {bulletSpdFactor}\n"     +
                            $"renderDelay = {renderDelay}\n"             +
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
            // Generate bullet
            if (id > nSrc + nRcv)
            {
                int i = rand.Next(nSrc);
                int j = rand.Next(nRcv) + nSrc;

                if (doUseSingletarget)
                {
                    j = (list[i] as myObj_0510).rcvId;
                }

                var snd = list[i] as myObj_0510;
                var rcv = list[j] as myObj_0510;

                x = snd.x;
                y = snd.y;
                X = rcv.x;
                Y = rcv.y;

                dx = X - x;
                dy = Y - y;

                float dist = (float)(Math.Sqrt(dx * dx + dy * dy));

                float spd = 0.5f + myUtils.randFloat(rand) * 0.25f;
                spd *= bulletSpdFactor;

                dx = (dx / dist) * spd;
                dy = (dy / dist) * spd;

                size = 3;

                A = 0.5f;
                //colorPicker.getColor(x, y, ref R, ref G, ref B);
                R = snd.R;
                G = snd.G;
                B = snd.B;

                // Initialize Trail
                if (trail == null)
                {
                    trail = new myParticleTrail(nTrail, x, y);
                }
                else
                {
                    trail.reset(x, y);
                }

                trail.updateDa(A * 2);
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            bool checkCollision(float val, float dVal, float target)
            {
                bool sign1 = (target - val) >= 0;
                bool sign2 = (target - val - dVal) >= 0;

                return (sign1 != sign2);
            }

            if (trail != null)
            {
                trail.update(x, y);
            }

            x += dx;
            y += dy;

            if (id < nSrc + nRcv)
            {
                if (x < 0)
                    dx += myUtils.randFloat(rand) * 0.05f;

                if (x > gl_Width)
                    dx -= myUtils.randFloat(rand) * 0.05f;

                if (y < 0)
                    dy += myUtils.randFloat(rand) * 0.05f;

                if (y > gl_Height)
                    dy -= myUtils.randFloat(rand) * 0.05f;

                if (myUtils.randomChance(rand, 1, 1001))
                {
                    rcvId = rand.Next(nRcv) + nSrc;
                }
            }
            else
            {
                if (checkCollision(x, dx, X) || checkCollision(y, dy, Y) ||
                    (x < 0 && dx < 0) || (y < 0 && dy < 0) || (x > gl_Width && dx > 0) || (y > gl_Height && dy > 0))
                {
                    generateNew();
                }
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Show()
        {
            if (trail != null)
            {
                trail.Show(R, G, B, A);
            }

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

            clearScreenSetup(doClearBuffer, 0.15f);

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
                        var obj = list[i] as myObj_0510;

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
                    list.Add(new myObj_0510());
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
            myPrimitive.init_LineInst(N * nTrail);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

    }
};
