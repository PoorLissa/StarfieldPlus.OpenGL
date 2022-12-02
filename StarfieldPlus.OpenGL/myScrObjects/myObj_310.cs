using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;


/*
    - Triangulation (experimental unfinished)
*/


namespace my
{
    public class myObj_310 : myObject
    {
        // ---------------------------------------------------------------------------------------------------------------

        private static int N = 0;
        private static int shapeType = 0, mode = 0;

        static int[] prm_i = new int[5];

        private float x, y, dx, dy, size, R, G, B, A;
        private int shape = 0;

        private myObj_310 left = null, right = null;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_310()
        {
            generateNew();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time global initialization
        protected override void initGlobal()
        {
            colorPicker = new myColorPicker(gl_Width, gl_Height);
            list = new List<myObject>();

            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time local initialization
        private void initLocal()
        {
            N = 25 + 4;

            mode = rand.Next(2);
            //mode = 0;

            // Reset parameter values
            {
                for (int i = 0; i < prm_i.Length; i++)
                    prm_i[i] = 0;
            }

            switch (mode)
            {
                case 00:
                case 01:
                case 02:
                    prm_i[0] = rand.Next(4);                                                // Interconnection lines drawing mode
                    break;
            }
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            string str_params = "";

            for (int i = 0; i < prm_i.Length; i++)
            {
                str_params += i == 0 ? $"{prm_i[i]}" : $", {prm_i[i]}";
            }

            string str = $"Obj = myObj_310\n\n" + 
                            $"mode = {mode}\n" +
                            $"param: [{str_params}]\n\n" +
                            $"file: {colorPicker.GetFileName()}" + 
                            $""
            ;
            return str;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void generateNew()
        {
            x = rand.Next(gl_Width);
            y = rand.Next(gl_Height);

            switch (mode)
            {
                case 00:
                case 01:
                    dx = (rand.Next(111) + 11) * 0.1f * myUtils.randomSign(rand);
                    dy = (rand.Next(111) + 11) * 0.1f * myUtils.randomSign(rand);
                    break;

                case 02:
                    dx = myUtils.randFloat(rand, 0.1f) * (rand.Next(50) + 1) * myUtils.randomSign(rand);
                    dy = myUtils.randFloat(rand, 0.1f) * (rand.Next(50) + 1) * myUtils.randomSign(rand);
                    break;
            }

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
            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            x += dx;
            y += dy;

            switch (mode)
            {
                case 00:
                    if (x < 0 || x > gl_Width)
                        dx *= -1;

                    if (y < 0 || y > gl_Height)
                        dy *= -1;
                    break;

                case 01:
                    if (x < -6666 || x > gl_Width + 6666)
                        dx *= -1;

                    if (y < -6666 || y > gl_Height + 6666)
                        dy *= -1;
                    break;

                case 02:
                    {
                        float factor = 0.25f;
                        int offset = 111;

                        if (x < offset)
                            dx += myUtils.randFloat(rand) * factor;

                        if (x > gl_Width - offset)
                            dx -= myUtils.randFloat(rand) * factor;

                        if (y < offset)
                            dy += myUtils.randFloat(rand) * factor;

                        if (y > gl_Height - offset)
                            dy -= myUtils.randFloat(rand) * factor;
                    }
                    break;
            }
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Show()
        {
            float lineOpacity = 0.1f;

            // Render connecting lines
            for (int i = 0; i < list.Count; i++)
            {
                var obj = list[i] as myObj_310;

                if (obj != this)
                {
                    switch (prm_i[0])
                    {
                        case 0:
                            break;

                        case 1:
                            {
                                double dist2 = (obj.x - x) * (obj.x - x) + (obj.y - y) * (obj.y - y) + 0.0001;
                                lineOpacity = (float)(10000.0 / dist2);
                            }
                            break;

                        case 2:
                            {
                                double dist2 = (obj.x - x) * (obj.x - x) + (obj.y - y) * (obj.y - y) + 0.0001;
                                lineOpacity = (float)(20000.0 / dist2);
                            }
                            break;

                        case 3:
                            {
                                double dist2 = (obj.x - x) * (obj.x - x) + (obj.y - y) * (obj.y - y) + 0.0001;
                                lineOpacity = (float)(20000.0 / dist2) + 0.05f;
                            }
                            break;
                    }

                    myPrimitive._LineInst.setInstanceCoords(obj.x, obj.y, x, y);
                    myPrimitive._LineInst.setInstanceColor(1, 1, 1, lineOpacity);
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

            while (list.Count < N - 4)
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
            myPrimitive.init_LineInst(N * N + N * 2);

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
