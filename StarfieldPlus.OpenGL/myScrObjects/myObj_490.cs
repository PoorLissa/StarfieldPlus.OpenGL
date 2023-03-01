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

        private static int N = 0, n = 0, shape = 0;
        private static float dimAlpha = 0.05f, nInvert = 0, t = 0, dt = 0;

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
                n = 333;
                
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

            // Use larger size as an option!
            int size1x = 2;
            int size2x = 2 * size1x;

            //for (int i = 0; i != n; i++)
            for (float i = min; i < max; i += stepx)
            {
                for (float j = min; j < max; j += stepy)
                {
                    //float fx = (rand.Next(len + 1) - max) + myUtils.randFloat(rand);
                    //float fy = (rand.Next(len + 1) - max) + myUtils.randFloat(rand);

                    float fx = i;
                    float fy = j;

                    double F = fx * Math.Sin(fx * t) * Math.Cos(fy * t);

                    //double F = fx * Math.Sin(fx) * Math.Cos(fy);

                    // !!!
                    //F += (fx * fx + fy * fy) * 0.1;

                    // !!!
                    //F += Math.Sin(fx * fx + fy * fy);

                    //double F = t * fx * Math.Sin(fx) * Math.Cos(fy);          // 1st variation
                    //double F = t * fx * Math.Sin(fx * t) * Math.Cos(fy * t);  // 2nd variation


                    //double F = fx * Math.Sin(fx * fy);    // this is a good one -- needs F(x or y)
                    //double F = fx * fy * Math.Cos(fx * fy) * Math.Sin(fx + fy);
                    //double F = fx * fx * fx * Math.Sin(fy) * Math.Sin(fy) + Math.Cos(fx);

                    //double F = fx * fx  + fy * fy;
                    //double F = fx * fx * fy * fy;

                    double df = F - fy;

                    if (df > 0 && df < 1)                 // this one just kind of resizes the image
                    //if (df > 10 && df < 11)               // this one makes it also shift
                    //if (df > 1.0f * t && df < 2.0f * t)     // should look like moving along z-axis
                    {
                        A = (float)(df * 1.0);

                        // Translate fx, fy to screen coordinates:
                        x = (int)(fx * fToScr) + gl_x0;
                        y = (int)(fy * fToScr) + gl_y0;

                        //colorPicker.getColor(x, y, ref R, ref G, ref B);

                        rectInst.setInstanceCoords(x - size1x, y - size1x, size2x, size2x);
                        rectInst.setInstanceColor(R, G, B, A);
                        rectInst.setInstanceAngle(angle);
                        angle += 0.0001f;
                    }
                    else
                    {
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
    }
};
