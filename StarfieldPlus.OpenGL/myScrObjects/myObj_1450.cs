using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;
using Microsoft.Win32.SafeHandles;


/*
    - Depth focus test: falling snow
    - Reference: https://www.youtube.com/watch?v=R5jIoLnL_nE&ab_channel=JosuRelax
*/


namespace my
{
    public class myObj_1450 : myObject
    {
        // Priority
        public static int Priority => 10;
		public static System.Type Type => typeof(myObj_1450);

        private int cnt;
        private float x, y, dx, dy;
        private float size, dSize, A, R, G, B;
        private float focus, dFocus;

        private static bool doChangeFocus = false;
        private static int N = 0, mode = 0, shaderNo = 0;
        private static int linearSpeed = 0;
        private static float dimAlpha = 0.05f, t = 0, dt = 0, maxFocus = 0.01f;
        private static float currentFocus = 0, targetFocus = 0, dTargetFocus = 0;

        private static myScreenGradient grad = null;
        private static myFreeShader_001 shader = null;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_1450()
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
                shaderNo = 0;
                N = 3333;

                currentFocus = 0.01f;
                targetFocus = 0;
                dTargetFocus = 0;
            }

            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time local initialization
        private void initLocal()
        {
            doClearBuffer = myUtils.randomChance(rand, 9, 10);
            doChangeFocus = myUtils.randomChance(rand, 1, 2);

            maxFocus *= (rand.Next(3)+1);                       // [0.01 .. 0.03]

            if (doChangeFocus)
                maxFocus = 0.03f;

            linearSpeed = myUtils.randomChance(rand, 2, 3)
                ? rand.Next(3) + 2
                : rand.Next(5) + 2;

            t = 0;
            dt = 0.0001f;

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            height = 600;

            string str = $"Obj = {Type}\n\n"                         +
                            myUtils.strCountOf(list.Count, N)        +
                            $"doClearBuffer = {doClearBuffer}\n"     +
                            $"doChangeFocus = {doChangeFocus}\n"     +
                            $"mode = {mode}\n"                       +
                            $"shaderNo = {shaderNo}\n"               +
                            $"linearSpeed = {linearSpeed}\n"         +
                            $"maxFocus = {myUtils.fStr(maxFocus)}\n" +
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
            // Pick size
            size = rand.Next(5) + 3;

            cnt = 10 + rand.Next(999);

            dx = 0;
            x = rand.Next(gl_Width);
            dy = myUtils.randFloatSigned(rand, 0.1f) * linearSpeed;
            y = -size;

            dSize = 0.05f + myUtils.randFloat(rand) * 0.1f;
            dSize *= 0.1f;
            dFocus = 0.00001f;
            dFocus = 0;

            A = 0.1f + myUtils.randFloat(rand) * 0.5f;
            focus = 0.0001f;

            if (doClearBuffer)
            {
                focus = 0.001f + myUtils.randFloat(rand) * maxFocus;
            }
            else
            {
                focus = 0.002f + myUtils.randFloat(rand) * 0.01f;
            }

            colorPicker.getColorRand(ref R, ref G, ref B);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            size += dSize;
            focus += dFocus;

            x += dx;
            y += dy;

            if (--cnt > 0)
            {
                if (size > 20)
                    dFocus = 0.0001f;

                if (myUtils.randomChance(rand, 1, 1234))
                {
                    A = 0.1f + myUtils.randFloat(rand) * 0.5f;
                }

                if (x < -size && dx < 0)
                    generateNew();

                if (x > gl_Width + size && dx > 0)
                    generateNew();
            }
            else
            {
                A -= 0.001f;

                if (A < 0)
                    generateNew();
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Show()
        {
            int off = 150 + (int)(size * 0.25f);

            if (doChangeFocus)
            {
                float Focus = (float)(Math.Abs(currentFocus - focus));

                shader.SetColor(R, G, B, A);
                shader.Draw(x, y, size, size, Focus, off);
            }
            else
            {
                shader.SetColor(R, G, B, A);
                shader.Draw(x, y, size, size, focus, off);
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
                    for (int i = 0; i != Count; i++)
                    {
                        var obj = list[i] as myObj_1450;

                        obj.Show();
                        obj.Move();
                    }
                }

                if (Count < N)
                {
                    list.Add(new myObj_1450());
                }

                if (doChangeFocus)
                {
                    if (dTargetFocus == 0 && myUtils.randomChance(rand, 1, 123))
                    {
                        targetFocus = myUtils.randFloat(rand) * maxFocus;
                        dTargetFocus = (targetFocus - currentFocus) / (100 + rand.Next(222));
                    }

                    currentFocus += dTargetFocus;

                    if (dTargetFocus < 0 && currentFocus < targetFocus)
                        dTargetFocus = 0;

                    if (dTargetFocus > 0 && currentFocus > targetFocus)
                        dTargetFocus = 0;
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

            switch (shaderNo)
            {
                case 0:
                    myFreeShader_001.getShader_000(ref fHeader, ref fMain);
                    break;

                case 1:
                    myFreeShader_001.getShader_001(ref fHeader, ref fMain);
                    break;
            }

            shader = new myFreeShader_001(fHeader, fMain);
        }

        // ---------------------------------------------------------------------------------------------------------------
    }
};
