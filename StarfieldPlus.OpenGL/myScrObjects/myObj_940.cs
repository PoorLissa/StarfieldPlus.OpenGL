using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;
using System.Windows.Forms;


/*
    - Circular shapes made of instanced lines
*/


namespace my
{
    public class myObj_940 : myObject
    {
        // Priority
        public static int Priority => 9999910;
        public static System.Type Type => typeof(myObj_940);

        private int cnt, mode, subMode;
        private float x1, y1, x2, y2;
        private float A, R, G, B;

        private static int N = 0, Rad = 666;
        private static float t = 0, dt = 0.003f;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_940()
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
                N = 100000;
            }

            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time local initialization
        private void initLocal()
        {
            mode    = rand.Next(6);
            subMode = rand.Next(2);

            switch (mode)
            {
                case 4:
                    subMode = rand.Next(2);
                    break;
            }
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            height = 600;

            string nStr(int n) { return n.ToString("N0"); }

            string str = $"Obj = {Type}\n\n"                         +
                            $"N = {nStr(list.Count)} of {nStr(N)}\n" +
                            $"mode = {mode}\n"                       +
                            $"subMode = {subMode}\n"                 +
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
            // todo: sin(t), cos(t) must be calculated only once per iteration

            cnt = 111 + rand.Next(111);
            cnt = 11 + rand.Next(11);

            switch (mode)
            {
                case 0:
                    {
                        int rad = Rad + (int)(333 * Math.Sin(t));

                        int r1 = rad + rand.Next(21) - 10;
                        int r2 = rad + rand.Next(21) - 10;

                        float angle1 = rand.Next(333) + (float)rand.NextDouble();

                        x1 = gl_x0 + r1 * (float)Math.Sin(angle1);
                        y1 = gl_y0 + r1 * (float)Math.Cos(angle1);

                        float angle2 = rand.Next(333) + (float)rand.NextDouble();

                        x2 = gl_x0 + r2 * (float)Math.Sin(angle2);
                        y2 = gl_y0 + r2 * (float)Math.Cos(angle2);
                    }
                    break;

                case 1:
                    {
                        int rad1 = Rad + (int)(333 * Math.Sin(t));
                        int rad2 = Rad + (int)(333 * Math.Cos(t / 1.73));

                        int r1 = rad1 + rand.Next(21) - 10;
                        int r2 = rad2 + rand.Next(21) - 10;

                        float angle1 = rand.Next(333) + (float)rand.NextDouble();
                        x1 = gl_x0 + r1 * (float)Math.Sin(angle1);
                        y1 = gl_y0 + r1 * (float)Math.Cos(angle1);

                        float angle2 = rand.Next(333) + (float)rand.NextDouble();
                        x2 = gl_x0 + r2 * (float)Math.Sin(angle2);
                        y2 = gl_y0 + r2 * (float)Math.Cos(angle2);
                    }
                    break;

                case 2:
                    {
                        int rad1 = Rad + (int)(333 * Math.Sin(t));
                        int rad2 = Rad + (int)(333 * Math.Cos(t / 1.73));

                        int r1 = rad1 + rand.Next(21) - 10;
                        int r2 = rad2 + rand.Next(21) - 10;

                        float angle1 = rand.Next(333) + (float)rand.NextDouble();
                        x1 = gl_x0 + r1 * (float)Math.Sin(angle1);
                        y1 = gl_y0 + r1 * (float)Math.Cos(angle1);

                        if (myUtils.randomChance(rand, 1, 2))
                        {
                            float angle2 = rand.Next(333) + (float)rand.NextDouble();
                            x2 = gl_x0 + r2 * (float)Math.Sin(angle2);
                            y2 = gl_y0 + r2 * (float)Math.Cos(angle2);
                        }
                        else
                        {
                            x2 = gl_x0 + r2 * (float)Math.Sin(angle1);
                            y2 = gl_y0 + r2 * (float)Math.Cos(angle1);
                        }
                    }
                    break;

                // Ring with 2 sides
                case 3:
                    {
                        int rad1 = Rad + (int)(333 * Math.Sin(t));
                        int rad2 = Rad + (int)(333 * Math.Cos(t / 1.73));

                        int r1 = rad1 + rand.Next(21) - 10;
                        int r2 = rad2 + rand.Next(21) - 10;

                        float angle1 = rand.Next(333) + (float)rand.NextDouble();
                        float angle2 = angle1;
                        x1 = gl_x0 + r1 * (float)Math.Sin(angle1);
                        y1 = gl_y0 + r1 * (float)Math.Cos(angle1);

                        switch (subMode)
                        {
                            case 0:
                                angle2 += myUtils.randFloat(rand) * 0.1f;
                                break;

                            case 1:
                                angle2 += myUtils.randFloat(rand) * 0.1f * myUtils.randomSign(rand);
                                break;
                        }

                        x2 = gl_x0 + r2 * (float)Math.Sin(angle2);
                        y2 = gl_y0 + r2 * (float)Math.Cos(angle2);
                    }
                    break;

                // Ring with a single side
                case 4:
                    {
                        int rad1 = Rad + (int)(333 * Math.Sin(t));
                        int r1 = rad1 + rand.Next(21) - 10;

                        float angle1 = rand.Next(333) + (float)rand.NextDouble();
                        float angle2 = angle1;
                        x1 = gl_x0 + r1 * (float)Math.Sin(angle1);
                        y1 = gl_y0 + r1 * (float)Math.Cos(angle1);

                        switch (subMode)
                        {
                            case 0:
                                angle2 += myUtils.randFloat(rand) * 0.95f;
                                break;

                            case 1:
                                angle2 += myUtils.randFloat(rand) * 0.95f * myUtils.randomSign(rand);
                                break;
                        }

                        x2 = gl_x0 + r1 * (float)Math.Sin(angle2);
                        y2 = gl_y0 + r1 * (float)Math.Cos(angle2);
                    }
                    break;

                // Ring with a single side and an opera effect
                case 5:
                    {
                        int rad1 = Rad + (int)(333 * Math.Sin(t));
                        int r1 = rad1 + rand.Next(21) - 10;

                        float angle1 = rand.Next(333) + (float)rand.NextDouble();
                        float angle2 = angle1;
                        x1 = gl_x0 + r1 * (float)Math.Sin(angle1);
                        y1 = gl_y0 + r1 * (float)Math.Cos(angle1);

                        switch (subMode)
                        {
                            case 0:
                                angle2 += myUtils.randFloat(rand) * 0.95f;
                                break;

                            case 1:
                                angle2 += myUtils.randFloat(rand) * 0.95f * myUtils.randomSign(rand);
                                break;
                        }

                        x2 = gl_x0 + r1 * (float)Math.Sin(angle2) * (float)Math.Sin(t);
                        y2 = gl_y0 + r1 * (float)Math.Cos(angle2) * (float)Math.Cos(t);
                    }
                    break;
            }

            A = 0.005f + myUtils.randFloat(rand) * 0.005f;
            R = (float)rand.NextDouble();
            G = (float)rand.NextDouble();
            B = (float)rand.NextDouble();

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            if (--cnt == 0)
            {
                generateNew();
            }
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Show()
        {
            myPrimitive._LineInst.setInstance(x1, y1, x2, y2, R, G, B, A);
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Process(Window window)
        {
            uint cnt = 0;
            initShapes();

            clearScreenSetup(true, 0.1f);

            while (list.Count < N)
                list.Add(new myObj_940());

            while (!Glfw.WindowShouldClose(window))
            {
                int Count = list.Count;

                processInput(window);

                // Swap fore/back framebuffers, and poll for operating system events.
                Glfw.SwapBuffers(window);
                Glfw.PollEvents();

                glClear(GL_COLOR_BUFFER_BIT);

                // Render Frame
                {
                    myPrimitive._LineInst.ResetBuffer();

                    for (int i = 0; i != Count; i++)
                    {
                        var obj = list[i] as myObj_940;

                        obj.Show();
                        obj.Move();
                    }

                    myPrimitive._LineInst.Draw();
                }

                t += dt;
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void initShapes()
        {
            myPrimitive.init_LineInst(N);
            myPrimitive._LineInst.setAntialized(false);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------
    }
};
