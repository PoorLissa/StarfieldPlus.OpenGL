using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;
using System.Windows.Forms;


/*
    - Test for instanced lines
*/


namespace my
{
    public class myObj_9999_test_004 : myObject
    {
        // Priority
        public static int Priority => 9999910;
        public static System.Type Type => typeof(myObj_9999_test_004);

        private int cnt;
        private float x1, y1, x2, y2;
        private float A, R, G, B;

        private static int N = 0, Rad = 666;
        private static float t = 0, dt = 0.003f;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_9999_test_004()
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
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            height = 600;

            string nStr(int n) { return n.ToString("N0"); }

            string str = $"Obj = {Type}\n\n"                         +
                            $"N = {nStr(list.Count)} of {nStr(N)}\n" +
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
            if (false)
            {
                cnt = 11 + rand.Next(11);

                x1 = rand.Next(gl_Width);
                y1 = rand.Next(gl_Height);
                x2 = rand.Next(gl_Width);
                y2 = rand.Next(gl_Height);
            }

            if (false)
            {
                cnt = 111 + rand.Next(111);

                cnt = 11 + rand.Next(11);

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

            if (true)
            {
                cnt = 11 + rand.Next(11);

                int rad1 = Rad + (int)(333 * Math.Sin(t));
                int rad2 = Rad + (int)(333 * Math.Cos(t/1.73));

                int r1 = rad1 + rand.Next(21) - 10;
                int r2 = rad2 + rand.Next(21) - 10;

                float angle1 = rand.Next(333) + (float)rand.NextDouble();

                x1 = gl_x0 + r1 * (float)Math.Sin(angle1);
                y1 = gl_y0 + r1 * (float)Math.Cos(angle1);

                float angle2 = rand.Next(333) + (float)rand.NextDouble();

                x2 = gl_x0 + r2 * (float)Math.Sin(angle2);
                y2 = gl_y0 + r2 * (float)Math.Cos(angle2);
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
            initShapes();

            clearScreenSetup(true, 0.1f);

            while (list.Count < N)
                list.Add(new myObj_9999_test_004());

            long time = DateTime.Now.Ticks;

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
                        var obj = list[i] as myObj_9999_test_004;

                        obj.Show();
                        obj.Move();
                    }

                    myPrimitive._LineInst.Draw();
                }

                t += dt;

                //if (++cnt == 100) Glfw.SetWindowShouldClose(window, true);
            }

            //double t = (long)TimeSpan.FromTicks(DateTime.Now.Ticks - time).TotalMilliseconds;

            // 1651
            // 2201
            //MessageBox.Show($@"fps = {t}", "fps", MessageBoxButtons.OK);

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
