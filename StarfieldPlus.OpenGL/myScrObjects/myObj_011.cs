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
            public float x, y, dx, dy;
        };

        // ---------------------------------------------------------------------------------------------------------------

        private static int N = 1, pN = 1;
        private static bool doClearBuffer = false;

        private float R, G, B, A;
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
            N = 2;
            pN = 2;

            doClearBuffer = false;
            renderDelay = 5;

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
            {
                while (particles.Count < pN)
                    particles.Add(new myObj_011_Particle());
            }

            foreach (myObj_011_Particle item in particles)
            {
                item.x = rand.Next(gl_Width);
                item.y = rand.Next(gl_Height);

                item.dx = (rand.Next(111) + 11) * 0.01f * myUtils.randomSign(rand);
                item.dy = (rand.Next(111) + 11) * 0.01f * myUtils.randomSign(rand);
            }

            R = 1;
            G = 1;
            B = 1;

            A = 0.25f;

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void setNextMode()
        {
            init();
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            foreach (myObj_011_Particle item in particles)
            {
                item.x += item.dx;
                item.y += item.dy;

                if (item.x < 0 || item.x > gl_Width)
                {
                    item.dx *= -1;
                }

                if (item.y < 0 || item.y > gl_Height)
                {
                    item.dy *= -1;
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

                if (doClearBuffer)
                {
                    glClear(GL_COLOR_BUFFER_BIT);
                }
                else
                {
                    // Dim the screen constantly;
                    myPrimitive._Rectangle.SetColor(0, 0, 0, 0.01f);
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
