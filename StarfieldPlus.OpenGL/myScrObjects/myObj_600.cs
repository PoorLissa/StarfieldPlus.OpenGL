using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;


/*
    - Pendulum
*/


namespace my
{
    public class myObj_600 : myObject
    {
        // Priority
        public static int Priority => 10;
        public static System.Type Type => typeof(myObj_600);

        private float x, y, rad, A, R, G, B, angle, dAngle;

        private myParticleTrail trail = null;

        private static int N = 0, mode = 0, subMode = 0, radMode = 0, dAngleMode = 0, nTrail = 100;
        private static float dimAlpha = 0.05f;
        private static bool doUseTrails = true;

        private static float dRad = 0, dAngleStep = 0;

        private static myFreeShader shader = null;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_600()
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
                switch (rand.Next(5))
                {
                    case 0: N = 111 + rand.Next(0123); break;
                    case 1: N = 111 + rand.Next(1234); break;
                    case 2: N = 333 + rand.Next(0123); break;
                    case 3: N = 333 + rand.Next(1234); break;
                    case 4: N = 333 + rand.Next(3333); break;
                }

                mode = 0;
                subMode = rand.Next(3);
                dAngleMode = rand.Next(5);
                radMode = rand.Next(3);
                dAngleStep = 0.001f;

                switch (rand.Next(2))
                {
                    case 0: dAngleStep = 0.001f / (rand.Next(05) + 1); break;
                    case 1: dAngleStep = 0.001f / (rand.Next(10) + 1); break;
                }

                dRad = 0.5f * gl_Width / N;

                doUseTrails = N < 333;
            }

            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time local initialization
        private void initLocal()
        {
            if (doUseTrails)
            {
                doClearBuffer = true;
                renderDelay = 0;
            }
            else
            {
                doClearBuffer = myUtils.randomBool(rand);
                renderDelay = rand.Next(11) + 3;
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            height = 600;

            string nStr(int   n) { return n.ToString("N0");     }
            string fStr(float f) { return f.ToString("0.0000"); }

            string str = $"Obj = myObj_600\n\n"                      +
                            $"N = {nStr(list.Count)} of {nStr(N)}\n" +
                            $"doClearBuffer = {doClearBuffer}\n"     +
                            $"doUseTrails = {doUseTrails}\n"         +
                            $"mode = {mode}\n"                       +
                            $"dAngleMode = {dAngleMode}\n"           +
                            $"radMode = {radMode}\n"                 +
                            $"dAngleStep = {fStr(dAngleStep)}\n"     +
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
            angle = 0;

            switch (dAngleMode)
            {
                case 0:
                    dAngle = 0.001f + id * dAngleStep;
                    dAngle *= 0.25f;
                    break;

                case 1:
                    dAngle = 0.001f + (N - id) * dAngleStep;
                    dAngle *= 0.25f;
                    break;

                case 2:
                    dAngle = 0.001f + id * (float)Math.Sin(id * 0.131729) * 0.01f;
                    dAngle *= 0.05f;
                    break;

                case 3:
                    dAngle = 0.001f + (id % 10) * 0.01f;
                    dAngle *= 0.25f;
                    break;

                case 4:
                    dAngle = 0.001f + (id % 10) * 0.01f + id * 0.0001f;
                    dAngle *= 0.25f;
                    break;
            }

            switch (subMode)
            {
                case 0:
                    break;

                case 1:
                    if (id % 2 == 0)
                        dAngle *= -1;
                    break;

                case 2:
                    if (id > N/2)
                        dAngle *= -1;
                    break;
            }

            rad = (id + 1) * dRad;

            x = gl_x0;
            y = gl_x0 + gl_y0 - rad;

            if (mode == 1)
            {
                y = rad;
            }

            A = 1.0f;
            colorPicker.getColorRand(ref R, ref G, ref B);

            // Initialize Trail
            if (doUseTrails && trail == null)
            {
                trail = new myParticleTrail(nTrail, x, y);
            }

            if (trail != null)
            {
                trail.updateDa(A);
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            // Update trail info
            if (doUseTrails)
            {
                trail.update(x, y);
            }

            angle += dAngle;

            switch (mode)
            {
                case 0:
                    {
                        x = gl_x0 + rad * (float)Math.Sin(angle);
                        y = gl_y0 + rad * (float)Math.Cos(angle);

                        // Change radius as well
                        switch (radMode)
                        {
                            case 1:
                                rad += (float)Math.Sin(angle) * 3;
                                break;

                            case 2:
                                rad += (float)Math.Sin(x + y) * 3;
                                break;
                        }
                    }
                    break;

                case 1:
                    {
                        x = gl_x0 + rad * (float)Math.Sin(angle);
                    }
                    break;
            }
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Show()
        {
            // Draw the trail
            if (doUseTrails)
            {
                trail.Show(R, G, B, A);
            }

            shader.SetColor(R, G, B, A);
            shader.Draw(x, y, 10, 10, 10);

            // Draw connecting lines
            if (id > 0)
            {
                var prev = list[(int)id - 1] as myObj_600;

                myPrimitive._LineInst.setInstanceCoords(x, y, prev.x, prev.y);
                myPrimitive._LineInst.setInstanceColor(1, 1, 1, doUseTrails ? 0.05f : 0.1f);
            }
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Process(Window window)
        {
            uint cnt = 0;
            initShapes();

            clearScreenSetup(doClearBuffer, 0.13f);

            while (list.Count < N)
            {
                list.Add(new myObj_600());
            }

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
                    }
                    else
                    {
                        dimScreen(dimAlpha);
                    }
                }

                // Render Frame
                {
                    myPrimitive._LineInst.ResetBuffer();

                    for (int i = 0; i != Count; i++)
                    {
                        var obj = list[i] as myObj_600;

                        obj.Show();
                        obj.Move();
                    }

                    myPrimitive._LineInst.Draw();
                }

                cnt++;
                System.Threading.Thread.Sleep(renderDelay);
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void initShapes()
        {
            int numLines = doUseTrails
                ? N + N * nTrail
                : N;

            myPrimitive.init_ScrDimmer();
            myPrimitive.init_LineInst(numLines);

            getShader();

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void getShader()
        {
            shader = new myFreeShader($@"
                        float circle(vec2 uv, float rad) {{ return smoothstep(rad, rad - 0.005, length(uv)); }}
                    ",

                    $@"vec2 uv = (gl_FragCoord.xy / iResolution.xy * 2.0 - 1.0);
                        uv -= Pos.xy;
                        uv *= aspect;
                        result = vec4(myColor * circle(uv, Pos.z));
                    "
            );
        }

        // ---------------------------------------------------------------------------------------------------------------
    }
};
