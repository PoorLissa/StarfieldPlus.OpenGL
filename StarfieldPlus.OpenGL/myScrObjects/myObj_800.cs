using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;


/*
    - Rows of Triangles
*/


namespace my
{
    public class myObj_800 : myObject
    {
        // Priority
        public static int Priority => 999910;
		public static System.Type Type => typeof(myObj_800);

        private int cnt;
        private float x, y;
        private float size, A, R, G, B, angle = 0;

        private static int N = 0, sSize = 0;
        private static bool doFillShapes = false, doUseRandOffsets = false;
        private static float dimAlpha = 0.05f, sizeFactor = 1.0f;

        private static myScreenGradient grad = null;

        private static Dictionary<int, int> dic = new Dictionary<int, int>();

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_800()
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
                N = rand.Next(666) + 1234;
            }

            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time local initialization
        private void initLocal()
        {
            doClearBuffer = myUtils.randomBool(rand);
            doClearBuffer = false;
            doUseRandOffsets = myUtils.randomBool(rand);
            doFillShapes = myUtils.randomChance(rand, 1, 3);

            sSize = 20 + rand.Next(30);

            sizeFactor = myUtils.randomChance(rand, 2, 3)
                ? 1.0f
                : 1.0f + myUtils.randFloat(rand) * 0.5f;

            renderDelay = rand.Next(11) + 3;

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            height = 600;

            string nStr(int   n) { return n.ToString("N0");    }
            string fStr(float f) { return f.ToString("0.000"); }

            string str = $"Obj = {Type}\n\n"                           +
                            $"N = {nStr(list.Count)} of {nStr(N)}\n"   +
                            $"sSize = {sSize}\n"                       +
                            $"sizeFactor = {fStr(sizeFactor)}\n"       +
                            $"doUseRandOffsets = {doUseRandOffsets}\n" +
                            $"renderDelay = {renderDelay}\n"           +
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
            size = sSize;

            x = rand.Next(gl_Width);
            x -= x % size;

            bool isUp = (x / size) % 2 == 0;

            angle = isUp ? 0 : (float)Math.PI;

            y = rand.Next(gl_Height);
            y -= y % (size * 1.5f + 5);

            if (doUseRandOffsets)
            {
                if (dic.ContainsKey((int)y))
                {
                    x += dic[(int)y];
                }
                else
                {
                    dic[(int)y] = rand.Next(33);
                }
            }

            if (!isUp)
            {
                y -= size / 2;
            }

            A = 0.1f;
            colorPicker.getColor(x, y, ref R, ref G, ref B);

            cnt = 100 + rand.Next(100);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            if (--cnt < 0)
            {
                A -= 0.01f;

                if (A <= 0)
                {
                    generateNew();
                }
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Show()
        {
            float Size = size * 1.33f;

            var triangleInst = inst as myTriangleInst;

            triangleInst.setInstanceCoords(x, y, Size, angle);
            triangleInst.setInstanceColor(R, G, B, A);

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
                        var obj = list[i] as myObj_800;

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
                    list.Add(new myObj_800());
                }

                cnt++;
                System.Threading.Thread.Sleep(renderDelay);
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void initShapes()
        {
            myPrimitive.init_ScrDimmer();
            base.initShapes(1, N, 0);

            grad = new myScreenGradient();
            grad.SetRandomColors(rand, 0.2f, 0);

            myUtils.SetAntializingMode(true);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------
    }
};
