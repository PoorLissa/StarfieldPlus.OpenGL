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
    public class myObj_1420 : myObject
    {
        // Priority
        public static int Priority => 10;
		public static System.Type Type => typeof(myObj_1420);

        private float x, y, dx, dy;
        private float size, A, R, G, B, depth = 0;

        private static int N = 0;
        private static int move2Mode = 0, dirMode = 0, sizeMode = 0, focusMode = 0, focusCnt = 0, focusCntMax = 0, opacityMode = 0, colorMode = 0;
        private static int hugeChance = 0, colorCnt = 0, linearSpeed = 0;
        private static bool doUseAmoebas = false, doUseMediums = false;
        private static float dimAlpha = 0.05f, minDepth = 0, maxDepth = 0.03f, currentFocus = 0, targetFocus = 0, dFocus = 0, t = 0, dt = 0;
        private static float gl_R = -1, gl_G = -1, gl_B = -1;

        private static List<myObj_1420> sortedList = null;
        private static myScreenGradient grad = null;
        private static myFreeShader_001 shader = null;
        private static myFreeShader_001 shaderBig = null;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_1420()
        {
            if (id != uint.MaxValue)
                generateNew();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time global initialization
        protected override void initGlobal()
        {
            colorPicker = new myColorPicker(gl_Width, gl_Height);
            sortedList = new List<myObj_1420>();

            // Global unmutable constants
            {
                N = 1234;
            }

            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time local initialization
        private void initLocal()
        {
            doClearBuffer = true;
            doUseAmoebas = myUtils.randomChance(rand, 1, 2);
            doUseMediums = myUtils.randomChance(rand, 1, 2);

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

            focusCnt = 10;
            targetFocus = minDepth + myUtils.randFloat(rand) * maxDepth;
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

            string str = $"Obj = {Type}\n\n"                                    +
                            myUtils.strCountOf(sortedList.Count, N)             +
                            $"doUseAmoebas = {doUseAmoebas}\n"                  +
                            $"doUseMediums = {doUseMediums}\n"                  +
                            $"dirMode = {dirMode}\n"                            +
                            $"linearSpeed = {linearSpeed}\n"                    +
                            $"sizeMode = {sizeMode}\n"                          +
                            $"move2Mode = {move2Mode}\n"                        +
                            $"opacityMode = {opacityMode}\n"                    +
                            $"colorMode = {colorMode}\n"                        +
                            $"focusMode = {focusMode}\n"                        +
                            $"focusCntMax = {focusCntMax}\n"                    +
                            $"currentFocus = {myUtils.fStr(currentFocus, 5)}\n" +
                            $"dFocus = {_dFocus}\n"                             +
                            $"hugeChance = {hugeChance}\n"                      +
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
            depth = myUtils.randFloat(rand) * 0.03f;

            // Pick size
            {
                if (myUtils.randomChance(rand, 1, hugeChance))
                {
                    // Huge
                    size = rand.Next(266) + 133;
                }
                else if (doUseMediums && myUtils.randomChance(rand, 1, 3))
                {
                    // Medium
                    size = rand.Next(99) + 30;
                }
                else if (myUtils.randomChance(rand, 1, 3))
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

            // In Size mode 1, show only huge particles
            if (sizeMode == 1)
            {
                if (size < 130)
                    A = 0.01f;
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

            // Huge particles additional setup
            if (size > 130)
            {
                A = 0.1f + myUtils.randFloat(rand) * 0.175f;
                depth = -0.02f - myUtils.randFloat(rand) * 0.03f;

                if (dirMode == 0)
                    y = -(33 + size);

                if (dirMode == 1)
                    x = -(33 + size);
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
            x += dx;
            y += dy;

            switch (dirMode)
            {
                case 0:
                    {
                        if (y > gl_Height + size)
                            generateNew();
                    }
                    break;

                case 1:
                    {
                        if (x > gl_Width + size)
                            generateNew();
                    }
                    break;

                case 2:
                    {
                        if (y < - size || y > gl_Height + size)
                            generateNew();

                        if (x < -size || x > gl_Width + size)
                            generateNew();
                    }
                    break;
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Show()
        {
            float focus = (float)(Math.Abs(depth - currentFocus));

            if (focus < 0.001f)
                focus = 0.001f;

            int off = (int)size;
            off = off < 50 ? 50 : off;

            if (doUseAmoebas == false)
            {
                shader.SetColor(R, G, B, A);
                shader.Draw(x, y, size, size, focus, off);
            }
            else
            {
                if (size < 130)
                {
                    // Tiny - Small - Normal - Medium
                    shader.SetColor(R, G, B, A);
                    shader.Draw(x, y, size, size, focus, off);
                }
                else 
                {
                    // Huge
                    shaderBig.SetColor(R, G, B, A);
                    shaderBig.Draw(x, y, size, size, focus, off);
                }
            }
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
                int Count = sortedList.Count;

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

                // Render Frame
                {
                    for (int i = 0; i != Count; i++)
                    {
                        var obj = sortedList[i] as myObj_1420;

                        obj.Show();
                        obj.Move();
                    }
                }

                if (Count < N && myUtils.randomChance(rand, 1, 5))
                {
                    sortedList.Add(new myObj_1420());
                }

                stopwatch.WaitAndRestart();
                t += dt;
                cnt++;

                setFocus();
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

            myFreeShader_001.getShader_001(ref fHeader, ref fMain);
            shaderBig = new myFreeShader_001(fHeader, fMain);
        }

        // ---------------------------------------------------------------------------------------------------------------

        // Sort the list, so the particles are drawn in correct z-order
        private void SortParticles()
        {
            sortedList.Sort(delegate (myObj_1420 obj1, myObj_1420 obj2)
            {
                return obj1.depth < obj2.depth
                    ? -1
                    : obj1.depth > obj2.depth
                        ? 1
                        : 0;
            });
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void setFocus()
        {
            // dist = [0 .. 0.03]
            // focusDist = [-0.01 .. 0.04]

            switch (focusMode)
            {
                case 0:
                    {
                        currentFocus = (float)(Math.Abs(Math.Sin(t * 10) * 0.05f)) - 0.01f;
                    }
                    break;

                case 1:
                    {
                        if (--focusCnt == 0)
                        {
                            focusCnt = 100 + rand.Next(focusCntMax);

                            targetFocus = minDepth + myUtils.randFloat(rand) * maxDepth;

                            targetFocus = -0.03f + myUtils.randFloat(rand) * 0.07f;

                            if (currentFocus > targetFocus)
                            {
                                dFocus = -1 * (float)Math.Abs(dFocus);
                            }
                            else
                            {
                                dFocus = +1 * (float)Math.Abs(dFocus);
                            }
                        }

                        if (dFocus > 0 && currentFocus < targetFocus)
                        {
                            currentFocus += dFocus;
                        }

                        if (dFocus < 0 && currentFocus > targetFocus)
                        {
                            currentFocus += dFocus;
                        }
                    }
                    break;

                case 2:
                    {
                        if (focusCnt == 0)
                        {
                            focusCnt = 100 + rand.Next(123);

                            if (currentFocus <= minDepth)
                            {
                                currentFocus = minDepth + 0.000001f;
                                targetFocus = maxDepth;
                                dFocus = +0.00025f;
                            }
                            else
                            {
                                currentFocus = maxDepth - 0.000001f;
                                targetFocus = minDepth;
                                dFocus = -0.00025f;
                            }
                        }

                        if (currentFocus < maxDepth && currentFocus > minDepth)
                        {
                            currentFocus += dFocus;
                        }
                        else
                        {
                            focusCnt--;
                        }
                    }
                    break;
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------
    }
};
