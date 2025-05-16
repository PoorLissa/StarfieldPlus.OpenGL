using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;


/*
    - Depth focus test
    - Reference: https://www.youtube.com/watch?v=R5jIoLnL_nE&ab_channel=JosuRelax
*/


namespace my
{
    public class myObj_1430 : myObject
    {
        // Priority
        public static int Priority => 10;
		public static System.Type Type => typeof(myObj_1430);

        private float x, y, z, dx, dy, dz;
        private float rad, alpha, dAlpha;
        private float size, Size, A, R, G, B;

        private static int N = 0;
        private static int moveMode = 0, move2Mode = 0, dirMode = 0, sizeMode = 0, focusMode = 0, focusCntMax = 0, opacityMode = 0, colorMode = 0;
        private static int hugeChance = 0, colorCnt = 0, linearSpeed = 0;
        private static bool doUseMediums = false;
        private static float dimAlpha = 0.05f, focus = 0, minDepth = 0, maxDepth = 0.03f, currentFocus = 0, dFocus = 0, t = 0, dt = 0;
        private static float gl_R = -1, gl_G = -1, gl_B = -1;

        private static List<myObj_1430> sortedList = null;
        private static myScreenGradient grad = null;
        private static myFreeShader_001 shader = null;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_1430()
        {
            if (id != uint.MaxValue)
                generateNew();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time global initialization
        protected override void initGlobal()
        {
            colorPicker = new myColorPicker(gl_Width, gl_Height);
            sortedList = new List<myObj_1430>();

            // Global unmutable constants
            {
                N = 1234;

                currentFocus = 0.03f * 0.5f;
            }

            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time local initialization
        private void initLocal()
        {
            doClearBuffer = true;
            doUseMediums = myUtils.randomChance(rand, 1, 2);

            moveMode = rand.Next(2);
            move2Mode = rand.Next(2);               // If particles move straight or diagonally
            dirMode = rand.Next(3);                 // Top-down or left-right motion or free motion
            opacityMode = rand.Next(3);
            focusCntMax = 100 + rand.Next(777);     // In focusMode 1, time between focus switches

            linearSpeed = myUtils.randomChance(rand, 2, 3)
                ? rand.Next(3) + 2
                : rand.Next(12) + 2;
            colorMode = myUtils.randomChance(rand, 2, 3)
                ? 0
                : 1;
            sizeMode = myUtils.randomChance(rand, 6, 7)
                ? 0
                : 1;
            focusMode = myUtils.randomChance(rand, 1, 2)
                ? 1
                : rand.Next(3);
            hugeChance = myUtils.randomChance(rand, 1, 2)
                ? 66
                : 22 + rand.Next(44);

            t = 0;
            dt = 0.0001f;
            dFocus = 0.0001f + myUtils.randFloat(rand) * 0.001f;

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            height = 600;

            string _dFocus = focusMode > 0 ?
                myUtils.fStr(Math.Abs(dFocus), 5)
                : "n/a";

            string str = $"Obj = {Type}\n\n"                        +
                            myUtils.strCountOf(sortedList.Count, N) +
                            $"doUseMediums = {doUseMediums}\n"      +
                            $"dirMode = {dirMode}\n"                +
                            $"linearSpeed = {linearSpeed}\n"        +
                            $"sizeMode = {sizeMode}\n"              +
                            $"moveMode = {moveMode}\n"              +
                            $"move2Mode = {move2Mode}\n"            +
                            $"opacityMode = {opacityMode}\n"        +
                            $"colorMode = {colorMode}\n"            +
                            $"focusMode = {focusMode}\n"            +
                            $"focusCntMax = {focusCntMax}\n"        +
                            $"dFocus = {_dFocus}\n"                 +
                            $"hugeChance = {hugeChance}\n"          +
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
            Size = 0;

            // Pick size
            {
                if (myUtils.randomChance(rand, 1, 3))
                {
                    // Normal
                    size = rand.Next(66) + 11;
                }
                else
                {
                    if (myUtils.randomChance(rand, 1, 3))
                    {
                        // Small
                        size = rand.Next(11) + 3;
                    }
                    else
                    {
                        // Tiny
                        size = rand.Next(5) + 3;
                    }
                }
            }

            switch (dirMode)
            {
                case 0:
                    dx = move2Mode == 0 ? 0 : myUtils.randFloatSigned(rand) * 0.33f;
                    dy = myUtils.randFloat(rand, 0.1f) * linearSpeed;

                    x = rand.Next(gl_Width);
                    y = -(33 + size);
                    break;

                case 1:
                    dx = myUtils.randFloat(rand, 0.1f) * linearSpeed;
                    dy = move2Mode == 0 ? 0 : myUtils.randFloatSigned(rand) * 0.33f;

                    x = -(33 + size);
                    y = rand.Next(gl_Height);
                    break;

                case 2:
                    dx = myUtils.randFloatSigned(rand, 0.1f) * 1;
                    dy = myUtils.randFloatSigned(rand, 0.1f) * 1;

                    x = rand.Next(gl_Width);
                    y = rand.Next(gl_Height);
                    break;
            }

            x = rand.Next(gl_Width);
            y = rand.Next(gl_Height);
            dx = dy = 0;

            dx = myUtils.randFloatSigned(rand, 0.1f) * 1;
            dy = myUtils.randFloatSigned(rand, 0.1f) * 1;

            z = myUtils.randFloat(rand);
            dz = myUtils.randFloatSigned(rand, 0.1f) * 0.002f;

            alpha = myUtils.randFloat(rand) * 321;
            dAlpha = myUtils.randFloatClamped(rand, 0.1f) * 0.0025f;
            rad = 10 + rand.Next(gl_x0);

            switch (moveMode)
            {
                case 1:
                    x = gl_x0 + rad * (float)Math.Sin(alpha);
                    y = gl_y0 + rad * (float)Math.Cos(alpha);
                    break;
            }

            switch (opacityMode)
            {
                case 0:
                    A = 0.25f + myUtils.randFloat(rand) * 0.175f;
                    break;

                case 1:
                    A = 0.5f + myUtils.randFloat(rand) * 0.5f;
                    break;

                case 2:
                    A = 0.85f + myUtils.randFloat(rand) * 0.15f;
                    break;
            }

            switch (colorMode)
            {
                // Generate color every time
                case 0:
                    colorPicker.getColorRand(ref R, ref G, ref B);
                    break;

                // Reuse the color, generate only sometimes
                case 1:
                    {
                        if (gl_R < 0 || --colorCnt == 0)
                        {
                            colorCnt = rand.Next(100) + 123;
                            colorPicker.getColorRand(ref gl_R, ref gl_G, ref gl_B);
                        }

                        R = gl_R + myUtils.randFloatSigned(rand) * 0.1f;
                        G = gl_G + myUtils.randFloatSigned(rand) * 0.1f;
                        B = gl_B + myUtils.randFloatSigned(rand) * 0.1f;
                    }
                    break;
            }

            // Brighten up small/tiny particles
            if (size < 10)
            {
                do {

                    R += 0.0001f;
                    G += 0.0001f;
                    B += 0.0001f;

                } while (R + G + B < 2.33f);
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            z += dz;

            if (z < 0 && dz < 0)
                dz *= -1;

            Size = 1.0f + size * z;

            switch (moveMode)
            {
                case 0:
                    {
                        x += dx;
                        y += dy;

                        if (z > 100 && dz > 0)
                            dz *= -1;

                        if (y < -Size || y > gl_Height + Size)
                            generateNew();

                        if (x < -Size || x > gl_Width + Size)
                            generateNew();
                    }
                    break;

                case 1:
                    {
                        if (z > 10 && dz > 0)
                            dz *= -1;

                        x = gl_x0 + rad * (float)Math.Sin(alpha);
                        y = gl_y0 + rad * (float)Math.Cos(alpha);

                        alpha += dAlpha;
                    }
                    break;
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Show()
        {
            switch (moveMode)
            {
                case 0:
                    focus = (float)(Math.Abs(z * 0.03f - currentFocus));
                    break;

                case 1:
                    currentFocus = 0.1f;
                    focus = (float)(Math.Abs(z * 0.03f - currentFocus));
                    break;
            }

            if (focus < 0.001f)
                focus = 0.001f;

            int off = (int)Size;
            off = off < 50 ? 50 : off;

            shader.SetColor(R, G, B, A);
            shader.Draw(x, y, Size, Size, focus, off);
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Process(Window window)
        {
            uint cnt = 0;
            initShapes();

            clearScreenSetup(doClearBuffer, 0.1f);

            stopwatch = new StarfieldPlus.OpenGL.myUtils.myStopwatch(true);

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
                        grad.Draw();
                    }
                    else
                    {
                        dimScreen(dimAlpha);
                    }
                }

                SortParticles();

                int Count = sortedList.Count;

                // Render Frame
                {
                    for (int i = 0; i != Count; i++)
                    {
                        var obj = sortedList[i] as myObj_1430;

                        obj.Show();
                        obj.Move();
                    }
                }

                if (Count < N && myUtils.randomChance(rand, 1, 5))
                {
                    sortedList.Add(new myObj_1430());
                }

                stopwatch.WaitAndRestart();
                t += dt;
                cnt++;
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void initShapes()
        {
            myPrimitive.init_ScrDimmer();

            getShader();

            grad = new myScreenGradient();
            grad.SetRandomColors(rand, 0.2f);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        // General shader selector
        private void getShader()
        {
            string fHeader = "", fMain = "";

            myFreeShader_001.getShader_000(ref fHeader, ref fMain);
            shader = new myFreeShader_001(fHeader, fMain);
        }

        // ---------------------------------------------------------------------------------------------------------------

         // Sort the list, so the particles are drawn in correct z-order
        private void SortParticles()
        {
            sortedList.Sort(delegate (myObj_1430 obj1, myObj_1430 obj2)
            {
                return obj1.z < obj2.z
                    ? -1
                    : obj1.z > obj2.z
                        ? 1
                        : 0;
            });
        }

        // ---------------------------------------------------------------------------------------------------------------
    }
};
