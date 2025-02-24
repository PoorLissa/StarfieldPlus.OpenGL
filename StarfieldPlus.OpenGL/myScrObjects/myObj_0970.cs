using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;


/*
    - Static rectangles drawn inside each other with a color shift
*/


namespace my
{
    public class myObj_0970 : myObject
    {
        // Priority
        public static int Priority => 10;
		public static System.Type Type => typeof(myObj_0970);

        private int size, dSize, cnt;
        private float A, R, G, B;

        private static int N = 0, maxCnt = 1, mode = 0, sizeMode = 0, maxSize = gl_x0;
        private static bool doFillShapes = false, doFade = false;
        private static float factorDownMain = 0.9f, factorUpMain = 1.1f, factorDown = 0, factorUp = 0;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_0970()
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
                N = 1;
            }

            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time local initialization
        private void initLocal()
        {
            doClearBuffer = false;
            doFillShapes = true;

            mode = rand.Next(2);
            sizeMode = rand.Next(2);
            maxCnt = 111;

            maxSize = myUtils.randomChance(rand, 1, 3)
                ? gl_y0
                : gl_x0;

            renderDelay = rand.Next(11) + 3;

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            height = 600;

            string nStr(int   n) { return n.ToString("N0");    }

            string str = $"Obj = {Type}\n\n"                         +
                            $"N = {nStr(list.Count)} of {nStr(N)}\n" +
                            $"mode = {mode}\n"                       +
                            $"sizeMode = {sizeMode}\n"               +
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
            doFade = false;
            cnt = maxCnt;
            size = maxSize;
            dSize = 10 + rand.Next(222);

            factorUp = factorUpMain;
            factorDown = factorDownMain;

            switch (mode)
            {
                // Light color
                case 0:
                    do {

                        R = (float)rand.NextDouble();
                        G = (float)rand.NextDouble();
                        B = (float)rand.NextDouble();

                    } while (R + G + B < 1.0f);
                    break;

                // Dark color
                case 1:
                    do
                    {

                        R = (float)rand.NextDouble();
                        G = (float)rand.NextDouble();
                        B = (float)rand.NextDouble();

                    } while (R + G + B > 0.5f);
                    break;
            }

            A = 0.1f;

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            if (--cnt == 0)
            {
                cnt = maxCnt;

                switch (sizeMode)
                {
                    case 0:
                        size -= dSize;
                        break;

                    case 1:
                        size -= rand.Next(222) + 10;
                        break;
                }

                switch (mode)
                {
                    case 0:
                        R *= factorDown;
                        G *= factorDown;
                        B *= factorDown;
                        break;

                    case 1:
                        R *= factorUp;
                        G *= factorUp;
                        B *= factorUp;
                        break;
                }

                factorDown += 0.001f;
                factorUp   -= 0.001f;

                if (size < 100 || doFade == true)
                {
                    if (doFade == false)
                    {
                        doFade = true;
                        size = maxSize;
                        R = 0.1f;
                        G = 0.1f;
                        B = 0.1f;
                    }
                    else
                    {
                        glDrawBuffer(GL_FRONT_AND_BACK);
                        glClearColor(R, G, B, 1);
                        glClear(GL_COLOR_BUFFER_BIT);

                        generateNew();
                    }
                }
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Show()
        {
            int sizex = size;
            int sizey = maxSize > gl_y0
                ? (int)(1.0 * sizex * gl_Height / gl_Width)
                : sizex;

            myPrimitive._RectangleInst.setInstanceCoords(gl_x0 - sizex, gl_y0 - sizey, sizex * 2, sizey * 2);
            myPrimitive._RectangleInst.setInstanceColor(R, G, B, A);
            myPrimitive._RectangleInst.setInstanceAngle(0);

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
                int Count = list.Count;

                processInput(window);

                // Swap fore/back framebuffers, and poll for operating system events.
                Glfw.SwapBuffers(window);
                Glfw.PollEvents();

                // Render Frame
                {
                    inst.ResetBuffer();

                    for (int i = 0; i != Count; i++)
                    {
                        var obj = list[i] as myObj_0970;

                        obj.Show();
                        obj.Move();
                    }

                    if (doFillShapes)
                    {
                        // Tell the fragment shader to multiply existing instance opacity by 0.25:
                        inst.SetColorA(-0.25f);
                        inst.Draw(true);
                    }

                    // Tell the fragment shader to do nothing with the existing instance opacity:
                    inst.SetColorA(0);
                    inst.Draw(false);
                }

                if (Count < N)
                {
                    list.Add(new myObj_0970());
                }

                cnt++;
                System.Threading.Thread.Sleep(renderDelay);
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void initShapes()
        {
            base.initShapes(0, N, 0);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------
    }
};
