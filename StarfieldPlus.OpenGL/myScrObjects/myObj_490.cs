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

        private static int N = 0, n = 0, shape = 0, funcNo = 0, conditionMode = 0, additiveFunc = 0;
        private static float dimAlpha = 0.05f, nInvert = 0, t = 0, dt = 0;

        private static int size1x = 1, size2x = 2;

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

            funcNo = rand.Next(16);
            conditionMode = rand.Next(3);
            additiveFunc = rand.Next(5);

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
                            $"additiveFunc = {additiveFunc}\n"          +
                            $"conditionMode = {conditionMode}\n"        +
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

                    double dF = F - fy;

                    if (isOk(dF))
                    {
                        A = (float)(dF * 1.0);

                        // Translate fx, fy to screen coordinates:
                        x = (int)(fx * fToScr) + gl_x0;
                        y = (int)(fy * fToScr) + gl_y0;

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
            double res = 0;

            //funcNo = 99;

            switch (funcNo)
            {
                case 000:
                    res = x * Math.Sin(x * t) * Math.Cos(y * t);
                    break;

                // 1st variation of 000
                case 001:
                    res = t * x * Math.Sin(x) * Math.Cos(y);
                    break;

                // 2nd variation of 000
                case 002:
                    res = t * x * Math.Sin(x * t) * Math.Cos(y * t);
                    break;

                case 003:
                    res = t * x * x * x * Math.Sin(y) * Math.Sin(y) + Math.Cos(x);
                    break;

                // 1st variation of 003
                case 004:
                    res = x * x * x * Math.Sin(y * t) * Math.Sin(y) + Math.Cos(x);
                    break;

                // 2nd variation of 003
                case 005:
                    res = x * x * x * Math.Sin(y * t) * Math.Sin(y * t) + Math.Cos(x);
                    break;

                // 3rd variation of 003
                case 006:
                    res = x * x * x * Math.Sin(y) * Math.Sin(y * t) + Math.Cos(x * t);
                    break;

                // 4th variation of 003
                case 007:
                    res = x * x * x * Math.Sin(y * t) * Math.Sin(y * t) + Math.Cos(x * t);
                    break;

                // 5th variation of 003
                case 008:
                    res = t * x * x * x * Math.Sin(y * t) * Math.Sin(y * t) + Math.Cos(x * t);
                    break;

                case 009:
                    res = t * x * y * Math.Cos(x * y) * Math.Sin(x + y);
                    break;

                // 1st variation of 009
                case 010:
                    res = x * y * Math.Cos(x * y * t) * Math.Sin(x + y);
                    break;

                // 2nd variation of 009
                case 011:
                    res = x * y * Math.Cos(x * y * t) * Math.Sin(x + y + t);
                    break;

                // 3rd variation of 009
                case 012:
                    res = t * x * y * Math.Cos(x * y * t) * Math.Sin(x + y + t);
                    break;

                case 013:
                    res = x * x * y * y * t;
                    break;

                case 014:
                    res = x * y + t * (x * x + y * y);
                    break;

                case 015:
                    res = t * Math.Sin(x + y);
                    break;

                //double F = fx * Math.Sin(fx * fy);    // this is a good one -- needs F(x or y)

                default:
                    res = 1;
                    break;
            }

            return res;
        }

        // ---------------------------------------------------------------------------------------------------------------

        // Check if the function value meets the displaying condition
        private bool isOk(double dF)
        {
            switch (conditionMode)
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
    }
};
