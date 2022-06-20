using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;


/*
    - Randomly Roaming Lines (based on Randomly Roaming Squares)
*/


namespace my
{
    public class myObj_011 : myObject
    {
        // ---------------------------------------------------------------------------------------------------------------

        private class myObj_011_Particle
        {
            public int cnt;
            public float x, y, dx, dy;
        };

        // ---------------------------------------------------------------------------------------------------------------

        private static int N = 1, pN = 1, borderOffset = 0;
        private static bool doClearBuffer = false, doCleanOnce = false;
        private static float dimAlpha = 0.01f;

        private int Cnt;
        private float R, G, B, A, r, g, b;
        private List<myObj_011_Particle> particles = null;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_011()
        {
            particles = new List<myObj_011_Particle>();

            if (colorPicker == null)
            {
                colorPicker = new myColorPicker(gl_Width, gl_Height);
                list = new List<myObject>();

                init();
            }

            generateNew();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time initialization
        private void init()
        {
            N = 33;
            pN = 2;

            borderOffset = myUtils.randomBool(rand) ? 0 : rand.Next(321);

            doClearBuffer = false;
            renderDelay = 15;

            dimAlpha = 0.001f;

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            string str = $"Obj = myObj_011\n\n";

            return str;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void generateNew()
        {
            if (particles.Count == 0)
                while (particles.Count < pN)
                    particles.Add(new myObj_011_Particle());

            foreach (myObj_011_Particle item in particles)
            {
                item.x = rand.Next(gl_Width);
                item.y = rand.Next(gl_Height);

                item.dx = (rand.Next(1111) + 111) * 0.01f * myUtils.randomSign(rand);
                item.dy = (rand.Next(1111) + 111) * 0.01f * myUtils.randomSign(rand);

                item.cnt = 0;
            }

            R = 1;
            G = 1;
            B = 1;

            //A = 1.0f / N;
            A = 0.33f;

            Cnt = rand.Next(1000) + 1000;

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void setNextMode()
        {
            init();
            doCleanOnce = true;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            Cnt--;

            foreach (myObj_011_Particle item in particles)
            {
                item.x += item.dx;
                item.y += item.dy;
/*
                if (item.cnt > 0)
                {
                    item.cnt--;

                    item.x += (float)Math.Sin(item.x) * 3;
                    item.y += (float)Math.Cos(item.y) * 3;
                }
*/
                if (item.x < 0 - borderOffset || item.x > gl_Width + borderOffset)
                {
                    item.dx *= -1;
                    item.cnt = 100;
                }

                if (item.y < 0 - borderOffset || item.y > gl_Height + borderOffset)
                {
                    item.dy *= -1;
                    item.cnt = 100;
                }
            }

            if (Cnt == 0)
            {
                r = (float)rand.NextDouble();
                g = (float)rand.NextDouble();
                b = (float)rand.NextDouble();
            }

            if (Cnt < 0)
            {
                R += (R > r) ? -0.001f : 0.001f;
                G += (G > g) ? -0.001f : 0.001f;
                B += (B > b) ? -0.001f : 0.001f;

                double rDiff = Math.Abs(R - r);
                double gDiff = Math.Abs(G - g);
                double bDiff = Math.Abs(B - b);

                if (rDiff + gDiff + bDiff < 0.005)
                {
                    Cnt = rand.Next(1000) + 1000;
                }
            }
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Show()
        {
            // Render connecting lines
            float xOld = -1111, yOld = -1111;

            foreach (myObj_011_Particle item in particles)
            {
                if (xOld == -1111 && yOld == -1111)
                {
                    xOld = item.x;
                    yOld = item.y;
                }
                else
                {
                    myPrimitive._LineInst.setInstanceCoords(item.x, item.y, xOld, yOld);
                    myPrimitive._LineInst.setInstanceColor(R, G, B, A);

                    xOld = item.x;
                    yOld = item.y;
                }
            }

            if (particles.Count > 2)
            {
                myPrimitive._LineInst.setInstanceCoords(particles[0].x, particles[0].y, xOld, yOld);
                myPrimitive._LineInst.setInstanceColor(R, G, B, A);
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Process(Window window)
        {
            uint cnt = 0;

            initShapes();

            while (list.Count < N)
            {
                list.Add(new myObj_011());
            }

            glDrawBuffer(GL_FRONT_AND_BACK);
            glClearColor(0, 0, 0, 1);

            while (!Glfw.WindowShouldClose(window))
            {
                cnt++;

                processInput(window);

                // Swap fore/back framebuffers, and poll for operating system events.
                Glfw.SwapBuffers(window);
                Glfw.PollEvents();

                if (doClearBuffer || doCleanOnce)
                {
                    glClear(GL_COLOR_BUFFER_BIT);
                    doCleanOnce = false;
                }
                else
                {
                    // Dim the screen constantly;
                    myPrimitive._Rectangle.SetColor(0, 0, 0, dimAlpha);
                    myPrimitive._Rectangle.SetAngle(0);
                    myPrimitive._Rectangle.Draw(0, 0, gl_Width, gl_Height, true);
                }

                // Render Frame
                {
                    myPrimitive._LineInst.ResetBuffer();

                    for (int i = 0; i < list.Count; i++)
                    {
                        var obj = list[i] as myObj_011;
                        obj.Show();
                        obj.Move();
                    }

                    myPrimitive._LineInst.Draw();
                }

                System.Threading.Thread.Sleep(renderDelay);
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void initShapes()
        {
            myPrimitive.init_Rectangle();
            myPrimitive.init_LineInst(N*pN);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------
    }
};
