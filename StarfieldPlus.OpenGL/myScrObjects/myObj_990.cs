using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Collections;


/*
    - ...
*/


namespace my
{
    public class myObj_990 : myObject
    {
        // Priority
        public static int Priority => 10;
		public static System.Type Type => typeof(myObj_990);

        private int cnt;
        private bool isAlive;
        private float x, y, w, h;
        private float size, dSize, A, R, G, B, angle = 0;

        private static int N = 0, shape = 0, maxCnt = 1;
        private static bool doFillShapes = false, newOption = false;
        private static float dimAlpha = 0.05f, whRatio = 1, speedFactor = 0, t = 0, dt = 0, xtFactor = 0, ytFactor = 0, X1, Y1, X2, Y2;

        private static myScreenGradient grad = null;

        private static List<myObj_990> sortedList = null;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_990()
        {
            if (id != uint.MaxValue)
                generateNew();

            if (id > 0)
                isAlive = false;
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time global initialization
        protected override void initGlobal()
        {
            colorPicker = new myColorPicker(gl_Width, gl_Height);
            list = new List<myObject>();
            sortedList = new List<myObj_990>();

            // Global unmutable constants
            {
                N = 333;

                whRatio = 1.0f + myUtils.randFloat(rand) * 3;

                maxCnt = 33;
                speedFactor = 1.01f;

                dt = 0.1f * (rand.Next(5) + 1);

                shape = myUtils.randomChance(rand, 1, 2) ? 0 : 2;

shape = 0;
newOption = true;

                xtFactor = (myUtils.randFloat(rand) + rand.Next(3)) * myUtils.randomSign(rand);
                ytFactor = (myUtils.randFloat(rand) + rand.Next(3)) * myUtils.randomSign(rand);
            }

            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time local initialization
        private void initLocal()
        {
            doClearBuffer = myUtils.randomBool(rand);
            doClearBuffer = true;
            doFillShapes = true;
doFillShapes = false;

            renderDelay = rand.Next(3) + 1;

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
                            $"whRatio = {fStr(whRatio)}\n"           +
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
            if (id == 0)
            {
                isAlive = false;

                cnt = maxCnt;

                h = 20;
                w = (int)(h * 1.0 * gl_Width / gl_Height);

                x = gl_x0;
                y = gl_y0;

                dSize = 0.5f + myUtils.randFloat(rand) * 0.75f;

                R = (float)rand.NextDouble();
                G = (float)rand.NextDouble();
                B = (float)rand.NextDouble();
            }
            else
            {
                var parent = list[0] as myObj_990;

                isAlive = true;

                x = parent.x;
                y = parent.y;
                w = parent.w;
                h = parent.h;
                dSize = parent.dSize;

                A = doFillShapes ? 0.25f : 0.5f;

                R = parent.R;
                G = parent.G;
                B = parent.B;

                R = G = B = 0.5f;

                R = (float)rand.NextDouble();
                G = (float)rand.NextDouble();
                B = (float)rand.NextDouble();

                R = 0.5f + (float)rand.NextDouble() * 0.1f;
                G = 0.5f + (float)rand.NextDouble() * 0.1f;
                B = 0.5f + (float)rand.NextDouble() * 0.1f;

                R = 0.33f + (float)rand.NextDouble() * 0.66f;
                G = 0.33f + (float)rand.NextDouble() * 0.66f;
                B = 0.33f + (float)rand.NextDouble() * 0.66f;
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            if (id == 0)
            {
                if (--cnt == 0)
                {
                    cnt = maxCnt;

                    x += 50 * (float)Math.Cos(t * xtFactor);
                    y += 33 * (float)Math.Cos(t * ytFactor);
                    t += dt;

                    for (int i = 1; i < list.Count; i++)
                    {
                        var obj = list[i] as myObj_990;

                        if (obj.isAlive == false)
                        {
                            obj.generateNew();
                            break;
                        }
                    }
                }
            }
            else
            {
                if (isAlive)
                {
                    w *= speedFactor;
                    h *= speedFactor;

                    if (w > gl_x0 || h > gl_y0)
                    {
                        A -= 0.0025f;

                        if (A <= 0)
                        {
                            isAlive = false;
                        }
                    }
                    else
                    {
                        float f = 0.00025f;

                        switch (rand.Next(3))
                        {
                            case 0: R -= f; break;
                            case 1: G -= f; break;
                            case 2: B -= f; break;
                        }
                    }
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
                        {
                            float x1 = x - w;
                            float y1 = y - h;
                            float x2 = x1 + w * 2;
                            float y2 = y1 + h * 2;

                            if (newOption == true)
                            {
                                if (X1 == 0 && Y1 == 0 && X2 == 0 && Y2 == 0)
                                {
                                    X1 = x1;
                                    Y1 = y1;
                                    X2 = x2;
                                    Y2 = y2;
                                }
                                else
                                {
                                    if (x1 < X1)
                                        x1 = X1;
                                    else if (x1 >= X2)
                                        return;
                                    else
                                        X1 = x1;

                                    if (y1 < Y1)
                                        y1 = Y1;
                                    else if (y1 >= Y2)
                                        return;
                                    else
                                        Y1 = y1;

                                    if (x2 > X2)
                                        x2 = X2;
                                    else if (x2 <= X1)
                                        return;
                                    else
                                        X2 = x2;

                                    if (y2 > Y2)
                                        y2 = Y2;
                                    else if (y2 <= Y1)
                                        return;
                                    else
                                        Y2 = y2;
                                }
                            }

                            myPrimitive._RectangleInst.setInstanceCoords(x1, y1, x2 - x1, y2 - y1);
                            myPrimitive._RectangleInst.setInstanceColor(R, G, B, A);
                            myPrimitive._RectangleInst.setInstanceAngle(angle);
                        }
                        break;

                    // Instanced triangles
                    case 1:
                        myPrimitive._TriangleInst.setInstanceCoords(x, y, size2x, angle);
                        myPrimitive._TriangleInst.setInstanceColor(R, G, B, A);
                        break;

                    // Instanced circles
                    case 2:
                        myPrimitive._EllipseInst.setInstanceCoords(x, y, w * 2, angle);
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

            while (!Glfw.WindowShouldClose(window))
            {
                int Count = sortedList.Count;

                SortParticles();

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

                    X1 = Y1 = X2 = Y2 = 0;

                    for (int i = 0; i != Count; i++)
                    {
                        var obj = sortedList[i];

                        obj.Show();
                        obj.Move();
                    }

                    if (doFillShapes)
                    {
                        // Tell the fragment shader to multiply existing instance opacity by 0.5:
                        inst.SetColorA(-0.1f);
                        inst.Draw(true);
                    }

                    // Tell the fragment shader to do nothing with the existing instance opacity:
                    inst.SetColorA(0);
                    inst.Draw(false);
                }

                if (Count < N)
                {
                    var obj = new myObj_990();

                    list.Add(obj);
                    sortedList.Add(obj);
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

        private void SortParticles()
        {
            sortedList.Sort(delegate (myObj_990 obj1, myObj_990 obj2)
            {
                return obj1.w > obj2.w
                    ? -1
                    : obj1.w < obj2.w
                        ? 1
                        : 0;
            });
        }

        // ---------------------------------------------------------------------------------------------------------------
    }
};
