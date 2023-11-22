using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;


/*
    - Drawing random symbols using the color sampled from an image
*/


namespace my
{
    public class myObj_650 : myObject
    {
        // Priority
        public static int Priority => 10;
		public static System.Type Type => typeof(myObj_650);

        private int index, cnt;
        private float x, y, A, R, G, B, angle = 0, sizeFactor = 1;

        private static int N = 0, size = 20, moveMode = 0, sizeMode = 0, angleMode = 0;
        private static float dimAlpha = 0.01f;
        private static bool doGenerateFast = false;

        private static TexText tTex = null;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_650()
        {
            if (id != uint.MaxValue)
                generateNew();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time global initialization
        protected override void initGlobal()
        {
            int mode = -1;

            if (myUtils.randomChance(rand, 1, 2))
                mode = (int)myColorPicker.colorMode.SNAPSHOT_OR_IMAGE;

            colorPicker = new myColorPicker(gl_Width, gl_Height, mode);
            list = new List<myObject>();

            // Global unmutable constants
            {
                N = 1000 + rand.Next(3333);
            }

            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time local initialization
        private void initLocal()
        {
            doClearBuffer  = myUtils.randomChance(rand, 1, 3);
            doGenerateFast = myUtils.randomChance(rand, 1, 2);

            switch (rand.Next(3))
            {
                case 0: size = 20 + rand.Next(10); break;
                case 1: size = 20 + rand.Next(30); break;
                case 2: size = 20 + rand.Next(50); break;
            }

            sizeMode = rand.Next(2);
            moveMode = rand.Next(2);
            angleMode = rand.Next(6);

            renderDelay = rand.Next(31) + 3;

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            height = 600;

            string nStr(int   n) { return n.ToString("N0");    }
            //string fStr(float f) { return f.ToString("0.000"); }

            string str = $"Obj = {Type}\n\n"                         +
                            $"N = {nStr(list.Count)} of {nStr(N)}\n" +
                            $"doClearBuffer = {doClearBuffer}\n"     +
                            $"doGenerateFast = {doGenerateFast}\n"   +
                            $"size = {size}\n"                       +
                            $"moveMode = {moveMode}\n"               +
                            $"sizeMode = {sizeMode}\n"               +
                            $"angleMode = {angleMode}\n"             +
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
            cnt = doClearBuffer
                ? 100 + rand.Next(111)
                :  50 + rand.Next(33);

            // Size factor (should only reduce the symbols, as enlarging makes them pixelated)
            sizeFactor = sizeMode == 1 && size > 21
                ? 0.5f + myUtils.randFloat(rand) * 20 / (float)size
                : 1.0f;

            switch (angleMode)
            {
                case 0:
                case 1:
                case 2:
                    angle = 0;
                    break;

                case 3: angle = myUtils.randFloatSigned(rand) * 0.1f; break;
                case 4: angle = myUtils.randFloatSigned(rand) * 0.3f; break;
                case 5: angle = myUtils.randFloatSigned(rand) * 1.0f; break;
            }

            x = rand.Next(gl_Width);
            y = rand.Next(gl_Height);

            A = doClearBuffer
                ? myUtils.randFloat(rand, 0.5f)
                : myUtils.randFloat(rand, 0.1f);

            colorPicker.getColor(x, y, ref R, ref G, ref B);

            // Map the particle to a symbol
            index = rand.Next(tTex.Lengh());

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            if (doGenerateFast)
            {
                generateNew();
            }
            else
            {
                if (doClearBuffer)
                {
                    if (--cnt < 0)
                    {
                        A -= 0.01f;

                        if (A < 0)
                            generateNew();
                    }
                }
                else
                {
                    switch (moveMode)
                    {
                        case 0:
                            if (--cnt < 0)
                            {
                                A -= 0.01f;

                                if (A < 0)
                                    generateNew();
                            }
                            break;

                        case 1:
                            if (--cnt == 0)
                            {
                                generateNew();
                            }
                            break;
                    }
                }
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Show()
        {
            tTex.Draw(x, y, index, A, angle, sizeFactor, R, G, B);
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Process(Window window)
        {
            uint cnt = 0;
            initShapes();


            clearScreenSetup(doClearBuffer, 0.1f, front_and_back: true);
            glDrawBuffer(GL_FRONT_AND_BACK);


            while (!Glfw.WindowShouldClose(window))
            {
                int Count = list.Count;

                processInput(window);

                // Swap fore/back framebuffers, and poll for operating system events.
                Glfw.SwapBuffers(window);
                Glfw.PollEvents();

                // Clear screen
                {
                    dimScreen(doClearBuffer ? 0.05f : dimAlpha);
                }

                // Render Frame
                {
                    for (int i = 0; i != Count; i++)
                    {
                        var obj = list[i] as myObj_650;

                        obj.Show();
                        obj.Move();
                    }
                }

                if (Count < N)
                {
                    list.Add(new myObj_650());
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

            TexText.setScrDimensions(gl_Width, gl_Height);
            tTex = new TexText(size, true, fontStyle: 0);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------
    }
};
