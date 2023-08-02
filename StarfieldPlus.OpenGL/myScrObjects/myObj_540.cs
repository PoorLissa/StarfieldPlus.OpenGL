using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;

using System.Drawing;
using System.Drawing.Drawing2D;


/*
    - Falling alphabet letters (Matrix style)

    // Some reference (not really used here, jic)
    https://learnopengl.com/In-Practice/Text-Rendering
*/


namespace my
{
    public class myObj_540 : myObject
    {
        // --------------------------------------------------------------------------

        // Generator class, used to instanciate several generators
        // Each generator will create several particles in the same place, until its counter is above zero;
        // Then it will move to some other position
        public class generator
        {
            public int x, y, cnt;
            public float spd, opacity;

            public void getNew()
            {
                x = rand.Next(gl_Width);
                y = rand.Next(gl_Height/2) - 333;
                y = -111;

                cnt = rand.Next(33) + 11;

                spd = myUtils.randomChance(rand, 1, 2)
                    ? myUtils.randFloat(rand, 0.1f) * (rand.Next(maxSpeed) + 1)
                    : -1;

                opacity = myUtils.randomChance(rand, 1, 2)
                    ? myObj_540.getOpacity(0.5f, 13)
                    : -1;
            }
        };

        // --------------------------------------------------------------------------

        // Priority
        public static int Priority => 9999910;

        private float x, y, dy, angle, dAngle, sizeFactor;
        private float A, R, G, B;
        private int   index;

        private static int N = 0, nGenerators = 0;
        private static float dimAlpha = 0.05f;
        private static bool doUseRGB = false, doUseSizeFactor = false;

        private static int maxSpeed = 0, posXGenMode = 0, posYGenMode = 0, angleMode = 0, modX = 0, size = 20;

        private static List<generator> Generators = null;

        private static TexText tTex = null;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_540()
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
            Generators = new List<generator>();

            // Global unmutable constants
            {
                N = 10000;

                doUseRGB = myUtils.randomChance(rand, 1, 5);            // If true, paint alphabet in white and then setcustom color for each particle

                // Size
                switch (rand.Next(4))
                {
                    case 0: size = rand.Next(40) + 20; break;
                    case 1: size = rand.Next(60) + 20; break;
                    case 2: size = rand.Next(80) + 20; break;
                    case 3: size = rand.Next(99) + 20; break;
                }
            }

            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time local initialization
        private void initLocal()
        {
            doClearBuffer = myUtils.randomChance(rand, 4, 5);
            doUseSizeFactor = myUtils.randomChance(rand, 1, 2);

            maxSpeed = 3 + rand.Next(13);               // max falling speed
            posXGenMode = rand.Next(3);                 // where the particles are generated along the X-axis
            posYGenMode = rand.Next(3);                 // where the particles are generated along the Y-axis
            angleMode = rand.Next(4);                   // letter rotation mode

            renderDelay = rand.Next(11) + 1;

            modX = rand.Next(333) + 11;

            dimAlpha = 0.2f + myUtils.randFloat(rand) * 0.05f;

            // Set up Generators:
            {
                Generators.Clear();

                nGenerators = myUtils.randomChance(rand, 1, 2)
                    ? 0
                    : rand.Next(111) + 33;

                for (int i = 0; i < nGenerators; i++)
                {
                    var gen = new generator();
                    gen.getNew();
                    Generators.Add(gen);
                }
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            height = 600;

            string nStr(int   n) { return n.ToString("N0");    }
            string fStr(float f) { return f.ToString("0.000"); }

            string str = $"Obj = myObj_540\n\n"                           +
                            $"N = {nStr(list.Count)} of {nStr(N)}\n"      +
                            $"doClearBuffer = {doClearBuffer}\n"          +
                            $"doUseSizeFactor = {doUseSizeFactor}\n"      +
                            $"renderDelay = {renderDelay}\n"              +
                            $"generators = {nGenerators}\n"               +
                            $"font = '{tTex.FontFamily()}'\n"             +
                            $"size = {size}\n"                            +
                            $"doUseRGB = {doUseRGB}\n"                    +
                            $"maxSpeed = {maxSpeed}\n"                    +
                            $"xGenMode = {posXGenMode} (modX = {modX})\n" +
                            $"yGenMode = {posYGenMode}\n"                 +
                            $"angleMode = {angleMode}\n"                  +
                            $"dimAlpha = {fStr(dimAlpha)}\n"              +
                            $"file: {colorPicker.GetFileName()}"
                ;
            return str;
        }

        // ---------------------------------------------------------------------------------------------------------------

        // 
        protected override void setNextMode()
        {
            initLocal();

            clearScreenSetup(doClearBuffer, 0.2f);
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void generateNew()
        {
            A = getOpacity(0.5f, 13);

            // Size factor (should only reduce the symbols, as enlarging makes them pixelated)
            sizeFactor = doUseSizeFactor && size > 21
                ? 0.5f + myUtils.randFloat(rand) * 20 / (float)size
                : 1.0f;

            colorPicker.getColor(rand.Next(gl_Width), rand.Next(gl_Height), ref R, ref G, ref B);

            dy = myUtils.randFloat(rand, 0.1f) * (rand.Next(maxSpeed) + 1);

            if (nGenerators > 0)
            {
                generateNew_2();

                if (dy < 0)
                    dy = myUtils.randFloat(rand, 0.1f) * (rand.Next(maxSpeed) + 1);
            }
            else
            {
                generateNew_1();
            }

            // Set up letter rotation
            {
                angle = 0;
                dAngle = myUtils.randFloat(rand) * 0.001f * myUtils.randomSign(rand);

                if (angleMode == 2)
                    dAngle *= rand.Next(5) + 1;

                if (angleMode == 3)
                    dAngle *= rand.Next(13) + 10;

            }

            index = rand.Next(tTex.Lengh());

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        // No Generators
        private void generateNew_1()
        {
            x = rand.Next(gl_Width);

            // Set position X
            switch (posXGenMode)
            {
                case 0:
                    break;

                case 1:
                    x -= x % modX;
                    break;

                case 2:
                    {
                        int opacityFactor = (int)(A * 10);

                        if (opacityFactor > 0)
                        {
                            x -= x % (opacityFactor * modX / 10);
                        }
                    }
                    break;
            }

            // Set position Y
            switch (posYGenMode)
            {
                case 0:
                    y = rand.Next(gl_Height / 2);
                    break;

                case 1:
                    y = rand.Next(gl_Height / 2) - 333;
                    break;

                case 2:
                    y = -1 * rand.Next(333) - 100;
                    break;
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        // Use Generators
        private void generateNew_2()
        {
            int n = rand.Next(nGenerators);

            x = Generators[n].x;
            y = Generators[n].y;

            if (Generators[n].spd > 0)
                dy = Generators[n].spd;

            if (Generators[n].opacity > 0)
                A = Generators[n].opacity;

            Generators[n].cnt--;

            if (Generators[n].cnt < 0)
            {
                Generators[n].getNew();
            }
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            y += dy;

            switch (angleMode)
            {
                case 0:
                    break;

                case 1:
                    angle += dAngle;
                    break;

                case 2:
                    {
                        if ((angle > 0.2f && dAngle > 0) || (angle < -0.2f && dAngle < 0))
                            dAngle *= -1;
                    }
                    break;

                case 3:
                    angle += dAngle;
                    break;
            }

            if (y > gl_Height)
            {
                generateNew();
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Show()
        {
            if (doUseRGB)
            {
                tTex.Draw(x, y, index, A, angle, sizeFactor, R, B, G);
            }
            else
            {
                tTex.Draw(x, y, index, A, angle, sizeFactor);
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Process(Window window)
        {
            uint cnt = 0;
            initShapes();

            if (doClearBuffer)
            {
                clearScreenSetup(doClearBuffer, 0.2f);
            }
            else
            {
                dimScreenRGB_SetRandom(0.1f);
                glDrawBuffer(GL_FRONT_AND_BACK);
                glDrawBuffer(GL_BACK);
            }

            while (!Glfw.WindowShouldClose(window))
            {
                processInput(window);

                // Swap fore/back framebuffers, and poll for operating system events.
                Glfw.SwapBuffers(window);
                Glfw.PollEvents();

                // Dim screen
                {
                    if (doClearBuffer)
                    {
                        glClear(GL_COLOR_BUFFER_BIT);
                    }
                    else
                    {
                        dimScreen(dimAlpha);
                    }
                }

                // Render Frame
                {
                    int Count = list.Count;

                    for (int i = 0; i != Count; i++)
                    {
                        var obj = list[i] as myObj_540;

                        obj.Show();
                        obj.Move();
                    }
                }

                if (list.Count < N)
                {
                    list.Add(new myObj_540());
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
            myPrimitive.init_Line();

            tTex = new TexText(size, gl_Width, gl_Height, doUseRGB);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        public static float getOpacity(float Base, int Chance)
        {
            // Base opacity is 0.5f at most;
            float op = myUtils.randFloat(rand) * Base;

            // Then, for some particles we increase it
            if (myUtils.randomChance(rand, 1, Chance))
                op += myUtils.randFloat(rand) * (1.0f - Base);

            return op;
        }

        // ---------------------------------------------------------------------------------------------------------------

    }
};
