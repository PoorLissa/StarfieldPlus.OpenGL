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

        private int cnt;
        private float x1, y1, x2, y2;
        private float A, R, G, B;

        private static int N = 0, mode, subMode, Rad = 666;
        private static float t = 0, dt = 0.003f, tFactorInv = 1;
        private static float sinT0 = 0, cosT0 = 0, cosT1 = 0;

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
            tFactorInv = 1.0f / (1.0f + myUtils.randFloat(rand));

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
            cnt = 11 + rand.Next(11);

            switch (mode)
            {
                // Circle
                case 0:
                    {
                        int rad = Rad + (int)(333 * sinT0);

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

                // Double circle
                case 1:
                    {
                        cnt = 11 + rand.Next(111);

                        int rad1 = Rad + (int)(333 * sinT0);
                        int rad2 = Rad + (int)(333 * cosT1);

                        int r1 = rad1 + rand.Next(21) - 10;
                        int r2 = rad2 + rand.Next(101) - 50;

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
                        int rad1 = Rad + (int)(333 * sinT0);
                        int rad2 = Rad + (int)(333 * cosT1);

                        int r1 = rad1 + rand.Next(21) - 10;
                        int r2 = rad2 + rand.Next(21) - 10;

                        float angle1 = rand.Next(333) + (float)rand.NextDouble();
                        x1 = gl_x0 + r1 * (float)Math.Sin(angle1);
                        y1 = gl_y0 + r1 * (float)Math.Cos(angle1);

                        if (myUtils.randomChance(rand, 1, 2))
                        {
                            x2 = gl_x0 + r2 * (float)Math.Sin(angle1);
                            y2 = gl_y0 + r2 * (float)Math.Cos(angle1);
                        }
                        else
                        {
                            float angle2 = rand.Next(333) + (float)rand.NextDouble();
                            x2 = gl_x0 + r2 * (float)Math.Sin(angle2);
                            y2 = gl_y0 + r2 * (float)Math.Cos(angle2);
                        }
                    }
                    break;

                // Ring with 2 sides
                case 3:
                    {
                        int rad1 = Rad + (int)(333 * sinT0);
                        int rad2 = Rad + (int)(333 * cosT1);

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
                        int rad1 = Rad + (int)(333 * sinT0);
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

                        r1 += 33;

                        x2 = gl_x0 + r1 * (float)Math.Sin(angle2);
                        y2 = gl_y0 + r1 * (float)Math.Cos(angle2);
                    }
                    break;

                // Ring with a single side and an opera effect
                case 5:
                    {
                        int rad1 = Rad + (int)(333 * sinT0);
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

                        x2 = gl_x0 + r1 * (float)Math.Sin(angle2) * sinT0;
                        y2 = gl_y0 + r1 * (float)Math.Cos(angle2) * cosT0;
                    }
                    break;
            }

            A = 0.005f + myUtils.randFloat(rand) * 0.0075f;
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

            t = 0;
            sinT0 = (float)Math.Sin(t);
            cosT0 = (float)Math.Cos(t);
            cosT1 = (float)Math.Cos(t * tFactorInv);

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

                sinT0 = (float)Math.Sin(t);
                cosT0 = (float)Math.Cos(t);
                cosT1 = (float)Math.Cos(t * tFactorInv);
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
