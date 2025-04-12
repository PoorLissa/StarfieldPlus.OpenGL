using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;


/*
    - ...
*/


namespace my
{
    public class myObj_1300 : myObject
    {
        // Priority
        public static int Priority => 10;
		public static System.Type Type => typeof(myObj_1300);

        private int cnt;
        private float x, y, dx, dy;
        private float size, A, R, G, B, angle = 0;

        private static int N = 0, shape = 0, mode = 0, angleMode = 0, sizeMode = 0;
        private static bool doFillShapes = false;
        private static float dimAlpha = 0.05f, s_angle = 0;
        private static float R0, G0, B0;

        private static myScreenGradient grad = null;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_1300()
        {
            if (id != uint.MaxValue)
                generateNew();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time global initialization
        protected override void initGlobal()
        {
            colorPicker = new myColorPicker(gl_Width, gl_Height, mode: myColorPicker.colorMode.SNAPSHOT_OR_IMAGE);
            list = new List<myObject>();

            // Global unmutable constants
            {
                N = rand.Next(50000) + 100000;

                shape = rand.Next(5);
            }

            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time local initialization
        private void initLocal()
        {
            doClearBuffer = myUtils.randomChance(rand, 1, 2);
            doFillShapes = myUtils.randomChance(rand, 1, 2);

            mode = rand.Next(3);
            angleMode = rand.Next(3);
            sizeMode = rand.Next(3);

            s_angle = myUtils.randFloat(rand) * rand.Next(321);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            height = 600;

            string str = $"Obj = {Type}\n\n"                  +
                            myUtils.strCountOf(list.Count, N) +
                            $"mode = {mode}\n"                +
                            $"angleMode = {angleMode}\n"      +
                            $"sizeMode = {sizeMode}\n"        +
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
            x = rand.Next(gl_Width);
            y = rand.Next(gl_Height);

            switch (mode)
            {
                case 0:
                    dx = dy = 0;
                    break;

                case 1:
                    dx = myUtils.randFloatSigned(rand) * 0.1f;
                    dy = myUtils.randFloatSigned(rand) * 0.1f;
                    break;

                case 2:
                    dx = x - gl_x0;
                    dy = y - gl_y0;

                    float dist = (float)Math.Sqrt(dx * dx + dy * dy);

                    dx = 0.2f * dx / dist;
                    dy = 0.2f * dy / dist;
                    break;
            }

            switch (angleMode)
            {
                case 0:
                    angle = 0;
                    break;

                case 1:
                    angle = s_angle;
                    break;

                case 2:
                    angle = myUtils.randFloat(rand) * rand.Next(321);
                    break;

            }

            A = myUtils.randFloatClamped(rand, 0.1f) * 0.25f;
            colorPicker.getColor(x, y, ref R, ref G, ref B);

            // Exclude the most popular color
            var dR = Math.Abs(R - R0);
            var dG = Math.Abs(G - G0);
            var dB = Math.Abs(B - B0);

            switch (sizeMode)
            {
                case 0:
                    size = rand.Next(3) + 1;
                    break;

                case 1:
                    size = rand.Next(13) + 1;
                    break;

                case 2:
                    size = (R + G + B) * 5;
                    break;
            }

            if(dR < 0.1 && dG < 0.1 && dB < 0.1)
            {
                size = 0;
                A = 0;
            }

            cnt = rand.Next(111) + 1;

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            x += dx; y += dy;

            if (--cnt == 0)
            {
                generateNew();
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Show()
        {
            float size2x = size * 2;

            switch (shape)
            {
                // Instanced squares
                case 0:
                    myPrimitive._RectangleInst.setInstanceCoords(x - size, y - size, size2x, size2x);
                    myPrimitive._RectangleInst.setInstanceColor(R, G, B, A);
                    myPrimitive._RectangleInst.setInstanceAngle(angle);
                    break;

                // Instanced triangles
                case 1:
                    myPrimitive._TriangleInst.setInstanceCoords(x, y, size, angle);
                    myPrimitive._TriangleInst.setInstanceColor(R, G, B, A);
                    break;

                // Instanced circles
                case 2:
                    myPrimitive._EllipseInst.setInstanceCoords(x, y, size2x, angle);
                    myPrimitive._EllipseInst.setInstanceColor(R, G, B, A);
                    break;

                // Instanced pentagons
                case 3:
                    myPrimitive._PentagonInst.setInstanceCoords(x, y, size2x, angle);
                    myPrimitive._PentagonInst.setInstanceColor(R, G, B, A);
                    break;

                // Instanced hexagons
                case 4:
                    myPrimitive._HexagonInst.setInstanceCoords(x, y, size2x, angle);
                    myPrimitive._HexagonInst.setInstanceColor(R, G, B, A);
                    break;
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Process(Window window)
        {
            uint cnt = 0;
            initShapes();

            // Find the most popular color in this image
            {
                var dic = new Dictionary<(float, float, float), int>();

                for (int i = 0; i < 10000; i++)
                {
                    int x = rand.Next(gl_Width);
                    int y = rand.Next(gl_Height);

                    colorPicker.getColor(x, y, ref R, ref G, ref B);

                    var color = (R, G, B);

                    if (dic.ContainsKey(color))
                    {
                        dic[color]++;
                    }
                    else
                    {
                        dic.Add(color, 1);
                    }
                }

                int max = 0;

                foreach (var item in dic)
                {
                    var _key = item.Key;
                    var _cnt = item.Value;

                    if (_cnt > max)
                    {
                        max = _cnt;
                        R0 = item.Key.Item1;
                        G0 = item.Key.Item2;
                        B0 = item.Key.Item3;
                    }
                }
            }


            clearScreenSetup(doClearBuffer, 0.1f);


            while (list.Count < N/2)
            {
                list.Add(new myObj_1300());
            }

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
                    {
                        glClear(GL_COLOR_BUFFER_BIT);
                        grad.Draw();
                    }
                    else
                    {
                        dimScreen(dimAlpha);
                    }
                }

                // Render Frame
                {
                    inst.ResetBuffer();

                    for (int i = 0; i != Count; i++)
                    {
                        var obj = list[i] as myObj_1300;

                        obj.Show();
                        obj.Move();
                    }

                    if (doFillShapes)
                    {
                        // Tell the fragment shader to multiply existing instance opacity by 0.5:
                        inst.SetColorA(-0.5f);
                        inst.Draw(true);
                    }

                    // Tell the fragment shader to do nothing with the existing instance opacity:
                    inst.SetColorA(0);
                    inst.Draw(false);
                }

                if (Count < N)
                {
                    list.Add(new myObj_1300());
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

            grad = new myScreenGradient();
            grad.SetRandomColors(rand, 0.2f);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------
    }
};
