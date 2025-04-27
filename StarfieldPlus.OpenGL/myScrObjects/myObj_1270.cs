using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;


/*
    - Straight lines symmetrically originating from starting points
*/


namespace my
{
    public class myObj_1270 : myObject
    {
        // Priority
        public static int Priority => 10;
		public static System.Type Type => typeof(myObj_1270);

        int n, cnt;
        private float x, y;
        private float A, R, G, B, angle = 0;

        private static int N = 0, nMin = 0, nMax = 0, shape = 0;
        private static int colorMode = 0, angleMode = 0, step = 0, nMode = 0, nGlobal = 0;
        private static float dimAlpha = 0.05f, angleGlobal = 0;

        private static myScreenGradient grad = null;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_1270()
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
                N = rand.Next(5) + 7;

                nMin = 3;
                nMax = 1 + rand.Next(11);

                switch (rand.Next(5))
                {
                    case 0: nGlobal = 3; break;
                    case 1: nGlobal = 4; break;
                    case 2: nGlobal = nMin + rand.Next( 3); break;
                    case 3: nGlobal = nMin + rand.Next( 7); break;
                    case 4: nGlobal = nMin + rand.Next(11); break;
                }

                shape = rand.Next(5);

                switch (rand.Next(3))
                {
                    case 0:
                        dimAlpha = 0.05f;
                        break;

                    case 1:
                        dimAlpha = 0.2f;
                        break;

                    case 2:
                        dimAlpha = 0.05f + myUtils.randFloat(rand) * 0.02f;
                        break;
                }
            }

            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time local initialization
        private void initLocal()
        {
            doClearBuffer = false;

            angleGlobal = myUtils.randFloat(rand) * 321;

            colorMode = rand.Next(2);
            nMode = rand.Next(2);
            angleMode = rand.Next(3);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            height = 600;

            string str = $"Obj = {Type}\n\n"                         +
                            myUtils.strCountOf(list.Count, N)        +
                            $"nMin = {nMin}\n"                       +
                            $"nMax = {nMax}\n"                       +
                            $"nMode = {nMode}\n"                     +
                            $"angleMode = {angleMode}\n"             +
                            $"colorMode = {colorMode}\n"             +
                            $"nGlobal = {nGlobal}\n"                 +
                            $"dimAlpha = {myUtils.fStr(dimAlpha)}\n" +
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
            cnt = 666 + rand.Next(1234);
            n = nMin + rand.Next(nMax);

            x = rand.Next(gl_Width);
            y = rand.Next(gl_Height);

            switch (nMode)
            {
                case 0:
                    n = nGlobal;
                    break;

                case 1:
                    n = nMin + rand.Next(nMax);
                    break;
            }

            switch (angleMode)
            {
                case 0:
                    angle = 0;
                    break;

                case 1:
                    angle = myUtils.randFloat(rand) * 321;
                    break;

                case 2:
                    angle = angleGlobal;
                    break;
            }

            A = 0.01f + myUtils.randFloat(rand) * 0.05f;

            switch (colorMode)
            {
                case 0:
                    R = (float)rand.NextDouble();
                    G = (float)rand.NextDouble();
                    B = (float)rand.NextDouble();
                    break;

                case 1:
                    colorPicker.getColor(x, y, ref R, ref G, ref B);
                    break;
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            if (--cnt == 0)
            {
                generateNew();
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Show()
        {
            var currentAngle = angle;

            float a = step == 1 ? A * 0.1f : A;

            for (int i = 0; i < n; i++)
            {
                var X = x + (float)Math.Sin(currentAngle) * gl_Width;
                var Y = y + (float)Math.Cos(currentAngle) * gl_Width;

                myPrimitive._LineInst.setInstanceCoords(x, y, X, Y);
                myPrimitive._LineInst.setInstanceColor(R, G, B, a);

                currentAngle += (float)(2.0 * Math.PI / n);
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Process(Window window)
        {
            uint cnt = 0;
            initShapes();

            clearScreenSetup(doClearBuffer, 0.1f);

            grad.SetOpacity(1);
            grad.Draw();
            Glfw.SwapBuffers(window);
            grad.Draw();
            grad.SetOpacity(dimAlpha);

            stopwatch = new StarfieldPlus.OpenGL.myUtils.myStopwatch(true);

            while (!Glfw.WindowShouldClose(window))
            {
                int Count = list.Count;

                processInput(window);

                // Swap fore/back framebuffers, and poll for operating system events.
                Glfw.SwapBuffers(window);
                Glfw.PollEvents();

                // Dim screen
                {
                    if (doClearBuffer)
                        glClear(GL_COLOR_BUFFER_BIT);

                    grad.Draw();
                }

                // Render Frame 1
                {
                    step = 1;
                    myPrimitive._LineInst.setLineWidth(5);
                    myPrimitive._LineInst.ResetBuffer();

                    for (int i = 0; i != Count; i++)
                    {
                        var obj = list[i] as myObj_1270;

                        obj.Show();
                        obj.Move();
                    }

                    myPrimitive._LineInst.Draw();
                }

                // Render Frame 2
                {
                    step = 2;
                    myPrimitive._LineInst.setLineWidth(2);
                    myPrimitive._LineInst.ResetBuffer();

                    for (int i = 0; i != Count; i++)
                    {
                        var obj = list[i] as myObj_1270;

                        obj.Show();
                    }

                    myPrimitive._LineInst.Draw();
                }

                if (Count < N && myUtils.randomChance(rand, 1, 333))
                {
                    list.Add(new myObj_1270());
                }

                stopwatch.WaitAndRestart();
                cnt++;
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void initShapes()
        {
            myPrimitive.init_ScrDimmer();
            base.initShapes(shape, N, 0);

            myPrimitive.init_LineInst(N * (nMin + nMax));
            myPrimitive._LineInst.setAntialized(true);

            grad = new myScreenGradient();
            grad.SetRandomColors(rand, 0.2f);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------
    }
};
