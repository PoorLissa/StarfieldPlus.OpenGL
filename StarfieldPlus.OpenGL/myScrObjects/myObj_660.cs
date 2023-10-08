using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;


/*
    - 
*/


namespace my
{
    public class myObj_660 : myObject
    {
        // Priority
        public static int Priority => 10;
		public static System.Type Type => typeof(myObj_660);

        private int next;
        private float x, y, dx, dy;
        private float size, A, R, G, B, r, g, b, angle = 0;
        private STATE state;

        private static int N = 0, n = 0, nMode = 0, shape = 0;
        private static bool doFillShapes = false, doUseRandSpeed = true;
        private static float t = 0, dt = 0.001f, dimAlpha = 0.05f, randSpeedFactor = 0, lineWidth = 1;

        private enum STATE { ALIVE, WAITING, DEAD };

        private static myScreenGradient grad = null;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_660()
        {
            state = STATE.DEAD;
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
                N = 1000;

                shape = rand.Next(5);
            }

            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time local initialization
        private void initLocal()
        {
            doClearBuffer = myUtils.randomBool(rand);
            doClearBuffer = true;

            doUseRandSpeed = myUtils.randomChance(rand, 3, 5);
            randSpeedFactor = 0.01f + myUtils.randFloat(rand);

            lineWidth = 0.5f + myUtils.randFloat(rand, 0.1f) * rand.Next(5);

            nMode = rand.Next(11);
            getNewN();

            renderDelay = 0;

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            height = 600;

            string nStr(int   n) { return n.ToString("N0");    }
            string fStr(float f) { return f.ToString("0.000"); }

            string str = $"Obj = {Type}\n\n"                               +
                            $"N = {nStr(list.Count)} of {nStr(N)}\n"       +
                            $"doUseRandSpeed = {doUseRandSpeed}\n"         +
                            $"randSpeedFactor = {fStr(randSpeedFactor)}\n" +
                            $"nMode = {nMode}\n"                           +
                            $"lineWidth = {fStr(lineWidth)}\n"             +
                            $"renderDelay = {renderDelay}\n"               +
                            $"file: {colorPicker.GetFileName()}"
                ;
            return str;
        }

        // ---------------------------------------------------------------------------------------------------------------

        // 
        protected override void setNextMode()
        {
            initLocal();

            myPrimitive._LineInst.setLineWidth(lineWidth);
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void generateNew()
        {
            if (false)
            {
                x += rand.Next(201) - 100;
                y += rand.Next(201) - 100;
            }

            dx = x - gl_x0;
            dy = y - gl_y0;

            float speed = 1.0f;

            if (doUseRandSpeed)
            {
                speed += myUtils.randFloat(rand) * randSpeedFactor;
                speed *= 0.5f;
            }

            float dist = 1.0f / (float)Math.Sqrt(dx * dx + dy * dy);

            dx = speed * dx * dist;
            dy = speed * dy * dist;

            x = gl_x0;
            y = gl_y0;

            angle = myUtils.randFloat(rand) * rand.Next(123);
            size = 3;

            A = 1;
            R = 1;
            G = 1;
            B = 1;

            if (true)
            {
                float colorFactor = 0.75f;

                R = r * colorFactor;
                G = g * colorFactor;
                B = b * colorFactor;
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            x += dx;
            y += dy;

            if (true)
            {
                dx *= 1.0025f;
                dy *= 1.0025f;
            }

            if (false)
            {
                if (myUtils.randomChance(rand, 1, 3))
                    x += myUtils.randFloatSigned(rand) * 0.5f;

                if (myUtils.randomChance(rand, 1, 3))
                    y += myUtils.randFloatSigned(rand) * 0.5f;
            }

            if (false)
            {
                x += (float)Math.Sin(x + y) * 0.5f;
                y += (float)Math.Cos(x + y) * 0.5f;
            }

            if (false)
            {
                var nextObj = list[next] as myObj_660;

                float DX = (float)Math.Abs(dx) + (float)Math.Abs(nextObj.dx);
                float DY = (float)Math.Abs(dy) + (float)Math.Abs(nextObj.dy);

                dx += DX * 0.005f * myUtils.signOf(dx);
                dy += DY * 0.005f * myUtils.signOf(dy);
            }

            switch (state)
            {
                case STATE.ALIVE:
                    {
                        A -= 0.001f;

                        if (x < 0 || x > gl_Width || y < 0 || y > gl_Height)
                            A -= 0.005f;

                        if (A <= 0)
                            state = STATE.WAITING;
                    }
                    break;

                case STATE.WAITING:
                    {
                        bool everyoneIsWaiting = true;
                        int Next = next;
                        myObj_660 nextObj = null;

                        // Loop through the mini-linked-list:
                        // Check if every particle on the list is in a waiting state
                        do
                        {
                            nextObj = list[Next] as myObj_660;

                            if (nextObj.state == STATE.ALIVE)
                            {
                                everyoneIsWaiting = false;
                                break;
                            }

                            Next = nextObj.next;
                        }
                        while (nextObj.id != this.id);

                        // When every particle on the linked list is waiting, mark them all as dead:
                        if (everyoneIsWaiting)
                        {
                            Next = next;
                            do
                            {
                                nextObj = list[Next] as myObj_660;
                                nextObj.state = STATE.DEAD;
                                Next = nextObj.next;
                            }
                            while (nextObj.id != this.id);
                        }
                    }
                    break;
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

        public class pt
        {
            public float x, y, z;

            public void draw()
            {
                //myPrimitive._Rectangle.SetColor(1, 1, 1, 1);
                myPrimitive._Rectangle.Draw(x, y, 3, 3, false);
            }
        }


        protected override void Process(Window window)
        {
            uint cnt = 0;
            initShapes();

            clearScreenSetup(doClearBuffer, 0.1f);


            // todo: move it somewhere else and maybe make some sense of it
/*
            myPrimitive.init_Rectangle();
            myPrimitive.init_Line();
            int n = 500;
            var arr = new pt[n * 2];
            float step = 1.0f * gl_Width / (n-1);

            for (int i = 0; i < n*2; i += 2)
            {
                pt pt1 = new pt();
                pt pt2 = new pt();

                pt1.x = i/2 * step;
                pt1.y = gl_y0 - (float)Math.Sin(i / 2) * 200;
                pt1.z = 0;

                pt2.x = i/2 * step;
                pt2.y = gl_y0 + (float)Math.Sin(i / 2) * 200;
                pt2.z = 0;

                arr[i + 0] = pt1;
                arr[i + 1] = pt2;
            }

            float pt_angle = 0;
*/

            while (!Glfw.WindowShouldClose(window))
            {
                int Count = list.Count;
                int deadCnt = 0;

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

#if false
                for (int i = 0; i < n*2; i += 2)
                {
                    var pt1 = arr[i + 0];
                    var pt2 = arr[i + 1];

                    //pt1.y -= (float)Math.Sin(pt_angle + i) * (float)Math.Sin(i/2) * 2;
                    //pt2.y += (float)Math.Sin(pt_angle + i) * (float)Math.Sin(i/2) * 2;

                    pt1.y = gl_y0 - (float)Math.Sin(pt_angle + i / 2) * 300;
                    //pt_angle += 0.0001f;
                    pt2.y = gl_y0 + (float)Math.Sin(pt_angle + i / 2) * 300;
                    //pt_angle += 0.0001f;

                    pt1.y -= (float)Math.Sin(pt_angle + i / 1) * 11;
                    pt2.y += (float)Math.Sin(pt_angle + i / 1) * 11;

                    myPrimitive._Line.SetColor(1, 1, 1, 0.1f);
                    myPrimitive._Line.Draw(pt1.x, pt1.y, pt2.x, pt2.y);

                    myPrimitive._Rectangle.SetColor(1, 0, 0, 1);
                    pt1.draw();

                    myPrimitive._Rectangle.SetColor(0, 1, 0, 1);
                    pt2.draw();
                }

                pt_angle += 0.003f;
                continue;
#endif

                // Render Frame
                {
                    inst.ResetBuffer();

                    for (int i = 0; i != Count; i++)
                    {
                        var obj = list[i] as myObj_660;

                        if (obj.state != STATE.DEAD)
                        {
                            obj.Show();
                            obj.Move();
                        }
                        else
                        {
                            deadCnt++;
                        }
                    }

                    // Render connecting lines;
                    // Do this outside of the main loop, so every object completes its current move by this time
                    myPrimitive._LineInst.ResetBuffer();
                    {
                        for (int i = 0; i != Count; i++)
                        {
                            var obj = list[i] as myObj_660;

                            if (obj.state != STATE.DEAD)
                            {
                                var nextObj = list[obj.next] as myObj_660;

                                myPrimitive._LineInst.setInstanceCoords(obj.x, obj.y, nextObj.x, nextObj.y);
                                myPrimitive._LineInst.setInstanceColor(obj.r, obj.g, obj.b, (obj.A + nextObj.A) * 0.5f);

                                myPrimitive._LineInst.setInstanceCoords(obj.x, obj.y, gl_x0, gl_y0);
                                myPrimitive._LineInst.setInstanceColor(1, 1, 1, 0.015f);
                            }
                        }
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
                    list.Add(new myObj_660());
                }

                cnt++;
                System.Threading.Thread.Sleep(renderDelay);

                // Get new shape
                {
                    const int maxCnt = 33;

                    if (cnt > maxCnt && deadCnt >= n)
                    {
                        int First = -1, Prev = -1;
                        cnt = 0;

                        float angle = myUtils.randFloatSigned(rand) * rand.Next(1234);
                        float dAngle = 2.0f * (float)Math.PI / n;

                        float commonR = 0, commonG = 0, commonB = 0;
                        colorPicker.getColorRand(ref commonR, ref commonG, ref commonB);

                        for (int i = 0; i != Count; i++)
                        {
                            var obj = list[i] as myObj_660;

                            if (obj.state == STATE.DEAD)
                            {
                                if (First < 0)
                                    First = i;

                                if (Prev >= 0)
                                    obj.next = Prev;

                                Prev = i;

                                obj.state = STATE.ALIVE;

                                obj.x = gl_x0 + 1000 * (float)Math.Sin(angle);
                                obj.y = gl_y0 + 1000 * (float)Math.Cos(angle);

                                obj.r = commonR;
                                obj.g = commonG;
                                obj.b = commonB;

                                obj.generateNew();

                                angle += dAngle;
                                cnt++;
                            }

                            if (cnt == n)
                            {
                                (list[First] as myObj_660).next = i;

                                // Request new n
                                getNewN();

                                cnt = 0;
                                break;
                            }
                        }
                    }
                }
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void initShapes()
        {
            base.initShapes(shape, N, 0);
            myPrimitive.init_LineInst(N * 2);

            myPrimitive._LineInst.setLineWidth(lineWidth);

            grad = new myScreenGradient();
            grad.SetRandomColors(rand, 0.2f, 0);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void getNewN()
        {
            switch (nMode)
            {
                case 00: n = 03 + rand.Next(03); break;
                case 01: n = 03 + rand.Next(09); break;
                case 02: n = 03 + rand.Next(11); break;
                case 03: n = 05 + rand.Next(11); break;
                case 04: n = 05 + rand.Next(11); break;
                case 05: n = 05 + rand.Next(17); break;
                case 06: n = 15 + rand.Next(21); break;
                case 07: n = 15 + rand.Next(23); break;
                case 08: n = 25 + rand.Next(23); break;
                case 09: n = 35 + rand.Next(23); break;
                case 10: n = 45 + rand.Next(23); break;
            }
        }

        // ---------------------------------------------------------------------------------------------------------------
    }
};
