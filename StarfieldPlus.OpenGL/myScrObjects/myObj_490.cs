using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;


/*
    - F (x, y)

    https://www.desmos.com/calculator/sjm3wwv9bs
*/


namespace my
{
    public class myObj_490 : myObject
    {
        private float x, y;
        private float A, R, G, B, angle = 0;

        private static int N = 0, n = 0, shape = 0, funcNo = 0, passConditionMode = 0, additiveFunc = 0;
        private static int size1x = 1, size2x = 2;
        private static float dimAlpha = 0.05f, nInvert = 0, t = 0, dt = 0;
        private static bool doUseVariations = true;

        private static ptrArray arrPtr = null;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_490()
        {
            generateNew();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time global initialization
        protected override void initGlobal()
        {
            colorPicker = new myColorPicker(gl_Width, gl_Height);
            list = new List<myObject>();

            unsafe
            {
                fixed (float *ptr_t = &t)
                {
                    arrPtr = new ptrArray(ptr_t, 10, defaultValue: 1.0f);
                }
            }

            // Global unmutable constants
            {
                doClearBuffer = false;

                N = 1;
                n = rand.Next(333) + 200;

                nInvert = 1.0f / n;
                shape = 0;

                t = 0;
            }

            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time local initialization
        private void initLocal()
        {
            dt = 0.001f;

            funcNo = rand.Next(8);
            passConditionMode = rand.Next(3);
            additiveFunc = rand.Next(5);
additiveFunc = 0;

            doUseVariations = myUtils.randomChance(rand, 1, 3);

            // Set up variations engine
            if (doUseVariations)
            {
                generateVariations(10);
            }
            else
            {
                arrPtr.Reset();
            }

            size1x = myUtils.randomChance(rand, 2, 3)
                ? rand.Next(03) + 1
                : rand.Next(20) + 1;
            size2x = 2 * size1x;

            renderDelay = rand.Next(11) + 3;

            dimAlpha = 0.1f;

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            height = 600;

            string nStr(int   n) { return n.ToString("N0");    }
            string fStr(float f) { return f.ToString("0.000"); }

            string str = $"Obj = myObj_490\n\n"                         +
                            $"N = {nStr(list.Count)} of {nStr(N)}\n"    +
                            $"n = {nStr(n)}\n"                          +
                            $"funcNo = {funcNo}\n"                      +
                            $"size = {size2x}\n"                        +
                            $"doUseVariations = {doUseVariations}\n"    +
                            $"additiveFunc = {additiveFunc}\n"          +
                            $"passConditionMode = {passConditionMode}\n"+
                            $"doClearBuffer = {doClearBuffer}\n"        +
                            $"renderDelay = {renderDelay}\n"            +
                            $"dimAlpha = {fStr(dimAlpha)}\n"            +
                            $"file: {colorPicker.GetFileName()}"
                ;
            return str;
        }

        // ---------------------------------------------------------------------------------------------------------------

        // 
        protected override void setNextMode()
        {
            initLocal();

            foreach (myObj_490 obj in list)
                obj.generateNew();

            dimScreen(0.5f);
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void generateNew()
        {
            R = 1.0f - myUtils.randFloat(rand) * 0.2f;
            G = 1.0f - myUtils.randFloat(rand) * 0.2f;
            B = 1.0f - myUtils.randFloat(rand) * 0.2f;

            A = 0;

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            var rectInst = inst as myRectangleInst;

            int min = -33;
            int max = +33;
            int len = max - min;

            float stepx = len * nInvert;
            float stepy = len * nInvert;

            float fToScr = gl_Width / len;

            for (float fx = min; fx < max; fx += stepx)
            {
                for (float fy = min; fy < max; fy += stepy)
                {
                    double F = getFunc(fx, fy);

                    switch (additiveFunc)
                    {
                        case 0:
                        case 1:
                            break;

                        case 2:
                            F += (fx * fx + fy * fy) * 0.1;
                            break;

                        case 3:
                            F += Math.Sin(fx * fx + fy * fy);
                            break;

                        case 4:
                            F += (fx * fx + fy * fy) * 0.1;
                            F += Math.Sin(fx * fx + fy * fy);
                            break;
                    }

                    if (false)
                    {
                        //F = fx * fx + fy * fy;

                        if (F > 1 && F < 3)
                        {
                            A = 1;

                            // Translate fx, fy to screen coordinates:
                            x = (float)(fx * fToScr) + gl_x0;
                            y = (float)(fy * fToScr) + gl_y0;

                            rectInst.setInstanceCoords(x - size1x, y - size1x, size2x, size2x);
                            rectInst.setInstanceColor(R, G, B, A);
                            rectInst.setInstanceAngle(angle);
                            angle += 0.0001f;
                        }

                        continue;
                    }

                    double dF = F - fy;

                    if (isOk(dF))
                    {
                        A = (float)(dF * 1.0);

                        // Translate fx, fy to screen coordinates:
                        x = (float)(fx * fToScr) + gl_x0;
                        y = (float)(fy * fToScr) + gl_y0;

                        //colorPicker.getColor(x, y, ref R, ref G, ref B);

                        // Draw the point
                        rectInst.setInstanceCoords(x - size1x, y - size1x, size2x, size2x);
                        rectInst.setInstanceColor(R, G, B, A);
                        rectInst.setInstanceAngle(angle);
                        angle += 0.0001f;
                    }

                    if (false)
                    {
                        double F2 = fx * Math.Sin(fx * fy * t);

                        double dfx = Math.Abs(F2 - fx);
                        double dfy = Math.Abs(F2 - fy);

                        if (dfy < 0.5 || dfx < 0.1)
                        {
                            // draw
                        }
                    }

/*
                    if (Math.Abs(F - fy) < 0.5 || Math.Abs(F - fx) < 0.1)
                    {
                        x = (int)(fx * gl_Width / (len)) + gl_x0;
                        y = (int)(fy * gl_Width / (len)) + gl_y0;

                        rectInst.setInstanceCoords(x - 1, y - 1, 2, 2);
                        rectInst.setInstanceColor(R, G, B, A);
                        rectInst.setInstanceAngle(angle);
                    }
*/
                }
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Show()
        {
            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Process(Window window)
        {
            uint cnt = 0;
            initShapes();

            // Disable VSYNC if needed
            // Glfw.SwapInterval(0);

            if (doClearBuffer)
            {
                glDrawBuffer(GL_FRONT_AND_BACK | GL_DEPTH_BUFFER_BIT);
                glClearColor(0, 0, 0, 1);
            }
            else
            {
                dimScreenRGB_SetRandom(0.1f);
                glDrawBuffer(GL_FRONT_AND_BACK);
            }

            while (list.Count < N)
            {
                list.Add(new myObj_490());
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
                    inst.ResetBuffer();

                    for (int i = 0; i != list.Count; i++)
                    {
                        var obj = list[i] as myObj_490;

                        obj.Show();
                        obj.Move();
                    }

                    // Tell the fragment shader to do nothing with the existing instance opacity:
                    inst.SetColorA(0);
                    inst.Draw(false);
                }

                cnt++;
                t += dt;
                System.Threading.Thread.Sleep(renderDelay);

                if (cnt == 1000)
                {
                    cnt = 0;
                    setNextMode();
                }
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void initShapes()
        {
            myPrimitive.init_ScrDimmer();
            base.initShapes(shape, n * n + 2, 0);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        // Calculate current function value
        private double getFunc(float x, float y)
        {
            switch (funcNo)
            {
                // x * sin(x) * cos(y)
                case 000:
                    return x * arrPtr.Get(0) * Math.Sin(x * arrPtr.Get(1)) * Math.Cos(y * arrPtr.Get(2));

                // x * y * sin(x) * cos(y)
                case 001:
                    return x * y * arrPtr.Get(0) * Math.Sin(x * arrPtr.Get(1)) * Math.Cos(y * arrPtr.Get(2));

                // x * y * cos(x * y) * sin(x + y)
                case 002:
                    return x * y * arrPtr.Get(0) * Math.Cos(x * y * arrPtr.Get(1)) * Math.Sin(x + y + arrPtr.Get(2));

                // x^3 * sin(y) * sin(y) + cos(x)
                case 003:
                    return x * x * x * arrPtr.Get(0) * Math.Sin(y * arrPtr.Get(1)) * Math.Sin(y * arrPtr.Get(2)) + Math.Cos(x * arrPtr.Get(3));

                // x^2 * y^2
                case 004:
                    return x * x * y * y * arrPtr.Get(0);

                // x * y + (x^2 + y^2)
                case 005:
                    return x * y + (x * x + y * y) * arrPtr.Get(1);

                // t * sin(x + y)
                case 006:
                    return t * Math.Sin(x + y);

                // sin(cos(x * y) * t) * 25
                case 007:
                    return Math.Sin(Math.Cos(x * y) * t) * 25;

                // test, not working yet
                case 008:
                    return Math.Abs(x * t) + Math.Abs(y * t);
            }

            //double F = fx * Math.Sin(fx * fy);    // this is a good one -- needs F(x or y)

            return 1.0;
        }

        // ---------------------------------------------------------------------------------------------------------------

        // Check if the function value meets the displaying condition
        private bool isOk(double dF)
        {
            switch (passConditionMode)
            {
                // Kind of resizes the image
                case 00:
                    return dF > 0 && dF < 1;

                // Shifts everything
                case 01:
                    return dF > 10 && dF < 11;

                // Should look like moving along z-axis
                case 02:
                    return dF > 1.0f * t && dF < 2.0f * t;
            }

            return false;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void generateVariations(int n)
        {
            for (int i = 0; i < n; i++)
            {
                arrPtr.Set(i, myUtils.randomBool(rand));
            }
        }

        // ---------------------------------------------------------------------------------------------------------------
    }
};
