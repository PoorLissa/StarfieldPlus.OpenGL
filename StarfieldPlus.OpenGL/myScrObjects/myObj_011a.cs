using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;


/*
    - lines with trail history -- test
*/


namespace my
{
    public class myObj_011a : myObject
    {
        private class point
        {
            public float x, y, dx, dy;
        };

        // ---------------------------------------------------------------------------------------------------------------

        // Priority
        public static int Priority => 9999910;

        private point pt1, pt2;
        private float A, R, G, B;
        private float[] trail = null;

        private static int N = 0, nTrail = 25;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_011a()
        {
            if (id != uint.MaxValue)
            {
                trail = new float[nTrail * 4];

                generateNew();
            }
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time global initialization
        protected override void initGlobal()
        {
            colorPicker = new myColorPicker(gl_Width, gl_Height);
            list = new List<myObject>();

            // Global unmutable constants
            {
                N = 3 + rand.Next(11);
            }

            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time local initialization
        private void initLocal()
        {
            doClearBuffer = true;
            renderDelay = rand.Next(11) + 3;

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            height = 600;

            string nStr(int   n) { return n.ToString("N0");    }
            string fStr(float f) { return f.ToString("0.000"); }

            string str = $"Obj = myObj_011a\n\n"                     +
                            $"N = {nStr(list.Count)} of {nStr(N)}\n" +
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
            if (pt1 == null)
                pt1 = new point();

            if (pt2 == null)
                pt2 = new point();

            float spdFactor = 20;

            pt1.x = rand.Next(gl_Width);
            pt1.y = rand.Next(gl_Height);
            pt1.dx = myUtils.randFloat(rand, 0.1f) * myUtils.randomSign(rand) * spdFactor;
            pt1.dy = myUtils.randFloat(rand, 0.1f) * myUtils.randomSign(rand) * spdFactor;

            pt2.x = rand.Next(gl_Width);
            pt2.y = rand.Next(gl_Height);
            pt2.dx = myUtils.randFloat(rand, 0.1f) * myUtils.randomSign(rand) * spdFactor;
            pt2.dy = myUtils.randFloat(rand, 0.1f) * myUtils.randomSign(rand) * spdFactor;

            A = 1;
            R = G = B = 0.2f;
            colorPicker.getColorRand(ref R, ref G, ref B);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void movePt(ref point p)
        {
            p.x += p.dx;
            p.y += p.dy;

            if (p.x < 0 && p.dx < 0)
                p.dx *= -1;

            if (p.y < 0 && p.dy < 0)
                p.dy *= -1;

            if (p.x > gl_Width && p.dx > 0)
                p.dx *= -1;

            if (p.y > gl_Height && p.dy > 0)
                p.dy *= -1;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            // Update trail info
            {
                for (int i = nTrail - 1; i > 0; i--)
                {
                    for (int j = 0; j < 4; j++)
                    {
                        int idx1 = ((i - 0) * 4) + j;
                        int idx2 = ((i - 1) * 4) + j;

                        trail[idx1] = trail[idx2];
                    }
                }
            }

            movePt(ref pt1);
            movePt(ref pt2);

            trail[0] = pt1.x;
            trail[1] = pt1.y;
            trail[2] = pt2.x;
            trail[3] = pt2.y;

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Show()
        {
            float a = 1.0f;
            float da = 1.0f / (nTrail + 1);

            myPrimitive._LineInst.setInstanceCoords(pt1.x, pt1.y, pt2.x, pt2.y);
            myPrimitive._LineInst.setInstanceColor(R, G, B, 1);

            a -= da;

            for (int i = 0; i < nTrail; i++)
            {
                int idx = i * 4;

                myPrimitive._LineInst.setInstanceCoords(trail[idx + 0], trail[idx + 1], trail[idx + 2], trail[idx + 3]);
                myPrimitive._LineInst.setInstanceColor(R, G, B, a);
                a -= da;
            }
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Process(Window window)
        {
            uint cnt = 0;
            initShapes();

            clearScreenSetup(doClearBuffer, 0.13f);

            while (!Glfw.WindowShouldClose(window))
            {
                int Count = list.Count;

                processInput(window);

                // Swap fore/back framebuffers, and poll for operating system events.
                Glfw.SwapBuffers(window);
                Glfw.PollEvents();

                // Dim screen
                {
                    glClear(GL_COLOR_BUFFER_BIT);
                }

                // Render Frame
                {
                    myPrimitive._LineInst.ResetBuffer();

                    for (int i = 0; i != Count; i++)
                    {
                        var obj = list[i] as myObj_011a;

                        obj.Show();
                        obj.Move();
                    }

                    myPrimitive._LineInst.Draw();
                }

                if (Count < N)
                {
                    list.Add(new myObj_011a());
                }

                cnt++;
                System.Threading.Thread.Sleep(renderDelay);
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void initShapes()
        {
            myPrimitive.init_LineInst(N * nTrail + N);
            myPrimitive._LineInst.setLineWidth(3);
        }

        // ---------------------------------------------------------------------------------------------------------------
    }
};
