using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;


/*
    - Triangulation
*/


namespace my
{
    public class myObj_310 : myObject
    {
        // ---------------------------------------------------------------------------------------------------------------

        private static int x0, y0, N = 100, objN = 0;
        private static int shapeType = 0;

        private float x, y, dx, dy, size, R, G, B, A;
        private int shape = 0;

        private myObj_310 left = null, right = null;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_310()
        {
            if (colorPicker == null)
            {
                colorPicker = new myColorPicker(gl_Width, gl_Height);
                list = new List<myObject>();

                objN = 25;

                init();
            }

            generateNew();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time initialization
        private void init()
        {
            x0 = gl_Width  / 2;
            y0 = gl_Height / 2;

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            string str = $"Obj = myObj_310\n\n";

            return str;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void generateNew()
        {
            x = rand.Next(gl_Width);
            y = rand.Next(gl_Height);

            dx = (rand.Next(111) + 11) * 0.1f * myUtils.randomSign(rand);
            dy = (rand.Next(111) + 11) * 0.1f * myUtils.randomSign(rand);

            //colorPicker.getColor(x, y, ref R, ref G, ref B);

            R = 1;
            G = 1;
            B = 1;

            A = 0.85f;

            size = 5;

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
            x += dx;
            y += dy;

            if (x < 0 || x > gl_Width)
            {
                dx *= -1;
            }

            if (y < 0 || y > gl_Height)
            {
                dy *= -1;
            }
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Show()
        {
            // Render connecting lines
            for (int i = 0; i < list.Count; i++)
            {
                var obj = list[i] as myObj_310;

                if (obj != this)
                {
                    myPrimitive._LineInst.setInstanceCoords(obj.x, obj.y, x, y);
                    myPrimitive._LineInst.setInstanceColor(1, 1, 1, 0.1f);
                }
            }

            myPrimitive._LineInst.setInstanceCoords(x, 0, x, gl_Height);
            myPrimitive._LineInst.setInstanceColor(1, 1, 1, 0.2f);

            myPrimitive._LineInst.setInstanceCoords(0, y, gl_Width, y);
            myPrimitive._LineInst.setInstanceColor(1, 1, 1, 0.2f);

            switch (shape)
            {
                // Instanced squares
                case 0:
                    var rectInst = inst as myRectangleInst;

                    rectInst.setInstanceCoords(x - size, y - size, 2*size, 2*size);
                    rectInst.setInstanceColor(R, G, B, A);
                    rectInst.setInstanceAngle(0);
                    break;
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Process(Window window)
        {
            uint cnt = 0;

            initShapes();

            while (list.Count < objN)
            {
                list.Add(new myObj_310());
            }

            list.Add(new myObj_310());
            (list[list.Count - 1] as myObj_310).x = 0;
            (list[list.Count - 1] as myObj_310).y = 0;

            list.Add(new myObj_310());
            (list[list.Count - 1] as myObj_310).x = 0;
            (list[list.Count - 1] as myObj_310).y = gl_Height;

            list.Add(new myObj_310());
            (list[list.Count - 1] as myObj_310).x = gl_Width;
            (list[list.Count - 1] as myObj_310).y = 0;

            list.Add(new myObj_310());
            (list[list.Count - 1] as myObj_310).x = gl_Width;
            (list[list.Count - 1] as myObj_310).y = gl_Height;

            glDrawBuffer(GL_FRONT_AND_BACK);
            glClearColor(0, 0, 0, 1);

            while (!Glfw.WindowShouldClose(window))
            {
                cnt++;

                processInput(window);

                // Swap fore/back framebuffers, and poll for operating system events.
                Glfw.SwapBuffers(window);
                Glfw.PollEvents();

                glClear(GL_COLOR_BUFFER_BIT);

                // Render Frame
                {
                    inst.ResetBuffer();
                    myPrimitive._LineInst.ResetBuffer();

                    for (int i = 0; i < list.Count; i++)
                    {
                        var obj = list[i] as myObj_310;
                        obj.Show();
                        obj.Move();
                    }

                    myPrimitive._LineInst.Draw();

                    inst.SetColorA(0);
                    inst.Draw(false);
                }

                System.Threading.Thread.Sleep(renderDelay);
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void initShapes()
        {
            myPrimitive.init_Rectangle();
            myPrimitive.init_LineInst(N*N);

            base.initShapes(shapeType, N, 0);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void Triangulate()
        {
            for (int i = 0; i < list.Count; i++)
            {
                var obj = list[i] as myObj_310;

                if (obj.left != null || obj.right != null)
                {
                    for (int j = 0; j < list.Count; j++)
                    {
                        if (i != j)
                        {
                            var other = list[i] as myObj_310;
                            float dist = (obj.x - other.x) * (obj.x - other.x) + (obj.y - other.y) * (obj.y - other.y);
                        }
                    }
                }
            }
        }

        // ---------------------------------------------------------------------------------------------------------------
    }
};
