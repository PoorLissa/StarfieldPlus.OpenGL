using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;


/*
    - Gravity n-body
*/


namespace my
{
    public class myObj_580 : myObject
    {
        // Priority
        public static int Priority => 10;
        public static System.Type Type => typeof(myObj_580);

        private int gravId;
        private float x, y, dx, dy, X, Y;
        private float size, A, R, G, B, mass, angle = 0;
        private bool isFirst;
        private myParticleTrail trail = null;

        private static int N = 0, shape = 0, gravMode = 0, nTrail = 0;
        private static bool doFillShapes = false, doUseInitSpeed = false, doUseTrails = false;
        private static float dimAlpha = 0.05f, gMode2_factor = 0;

        private static int n = 5;

        private static myFreeShader shader = null;
        private static myScreenGradient grad = null;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_580()
        {
            isFirst = true;

            if (id != uint.MaxValue)
            {
                mass = id < n ? 10000 : (rand.Next(25) + 1);

                if (id == 1)
                    mass = 100000 * (rand.Next(7) + 1);

                generateNew();
            }
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time global initialization
        protected override void initGlobal()
        {
            colorPicker = new myColorPicker(gl_Width, gl_Height);
            list = new List<myObject>();

            // Global unmutable constants
            {
                doUseTrails = myUtils.randomChance(rand, 10, 11);

                if (doUseTrails)
                {
                    switch (rand.Next(3))
                    {
                        case 0: nTrail = 100 + rand.Next(100); break;
                        case 1: nTrail = 100 + rand.Next(333); break;
                        case 2: nTrail = 100 + rand.Next(666); break;
                    }
                }

                n = 3 + rand.Next(5);

                N = rand.Next(1000) + 10000;
                shape = 2;

                gravMode = rand.Next(3);

                // gravMode = 3;

                gMode2_factor = myUtils.randFloat(rand) * 0.5f;

                dimAlpha = 0.25f;
            }

            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time local initialization
        private void initLocal()
        {
            doClearBuffer = myUtils.randomChance(rand, 4, 5);
            doUseInitSpeed = myUtils.randomBool(rand);

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
                            $"doClearBuffer = {doClearBuffer}\n"       +
                            $"doUseInitSpeed = {doUseInitSpeed}\n"     +
                            $"gravMode = {gravMode}\n"                 +
                            $"gMode2_factor = {fStr(gMode2_factor)}\n" +
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
            dx = dy = 0;

            if (doUseInitSpeed && id != 1)
            {
                dx = myUtils.randFloat(rand) * myUtils.randomSign(rand);
                dy = myUtils.randFloat(rand) * myUtils.randomSign(rand);
            }

            if (id < n)
            {
                x = gl_x0 + rand.Next(gl_Width /4) * myUtils.randomSign(rand);
                y = gl_y0 + rand.Next(gl_Height/4) * myUtils.randomSign(rand);
            }
            else
            {
                x = rand.Next(gl_Width);
                y = rand.Next(gl_Width);
                y -= (gl_Width - gl_Height) / 2;
            }

            X = x;
            Y = y;

            if (isFirst)
            {
                isFirst = false;

                size = mass < 100 ? 2 : 11;
                colorPicker.getColor(x, y, ref R, ref G, ref B);
                A = mass < 100 ? 0.25f : 1.0f;
            }

            gravId = rand.Next(n);

            // Initialize Trail
            if (doUseTrails && id < n && trail == null)
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
            if (doUseTrails && id < n)
            {
                trail.update(x, y);
            }

            x += dx;
            y += dy;

            for (int i = 0; i < n; i++)
            {
                var obj = list[i] as myObj_580;

                if (id != obj.id)
                {
                    // Flicker the smaller stars
                    if (mass < 100)
                    {
                        if (myUtils.randomChance(rand, 1, 100))
                        {
                            A += myUtils.randFloatSigned(rand) * 0.05f;
                        }
                    }

                    switch (gravMode)
                    {
                        // Large stars are attracted to each other;
                        // Small stars are attracted to each of large stars
                        case 0:
                            {
                                float dX = obj.x - x;
                                float dY = obj.y - y;

                                float dist = 1.0f / (float)Math.Sqrt(dX * dX + dY * dY);

                                float factor = mass > 100
                                    ? obj.mass * 0.000001f
                                    : obj.mass * 0.0000001f;

                                dx += factor * dX * dist;
                                dy += factor * dY * dist;
                            }
                            break;

                        // Large stars are attracted to each other;
                        // Small stars are attracted to only one of the large stars
                        case 1:
                            {
                                if ((mass > 100) || (mass < 100 && gravId == obj.id))
                                {
                                    float dX = obj.x - x;
                                    float dY = obj.y - y;

                                    float dist = 1.0f / (float)Math.Sqrt(dX * dX + dY * dY);

                                    float factor = mass > 100
                                        ? obj.mass * 0.0000001f
                                        : 0.00000015f;

                                    dx += factor * dX * dist;
                                    dy += factor * dY * dist;
                                }
                            }
                            break;

                        // Large stars are attracted to each other;
                        // Small stars are attracted to only one of the large stars (dx/dy are not added, but just set at each iteration)
                        case 2:
                            {
                                if ((mass > 100) || (mass < 100 && gravId == obj.id))
                                {
                                    float dX = obj.x - x;
                                    float dY = obj.y - y;

                                    float dist = 1.0f / (float)Math.Sqrt(dX * dX + dY * dY);

                                    float factor = mass > 100
                                        ? obj.mass * 0.0000001f
                                        : 1.00000015f;

                                    if (mass > 100)
                                    {
                                        dx += factor * dX * dist;
                                        dy += factor * dY * dist;
                                    }
                                    else
                                    {
                                        dx = factor * dX * dist + dx * gMode2_factor;
                                        dy = factor * dY * dist + dy * gMode2_factor;
                                    }
                                }
                            }
                            break;

                        // Large stars are attracted to each other;
                        // Small stars are attracted to each of large stars (but also are attracted to original location)

                        // needs fixing

                        case 3:
                            {
                                {
                                    float dX = obj.x - x;
                                    float dY = obj.y - y;

                                    float dist = 1.0f / (float)Math.Sqrt(dX * dX + dY * dY);

                                    float factor = mass > 100
                                        ? obj.mass * 0.0000001f
                                        : dist > 0.005f ? -0.2f : 0;

                                    dx += factor * dX * dist;
                                    dy += factor * dY * dist;

                                    if (mass < 100)
                                    {
                                        dX = X - x;
                                        dY = Y - y;

                                        dist = (float)Math.Sqrt(dX * dX + dY * dY);

                                        factor = 0.0001f;

                                        dx += factor * dX * dist;
                                        dy += factor * dY * dist;
                                    }
                                }
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
            // Draw the trail
            if (doUseTrails && id < n)
            {
                trail.Show(R, G, B, A);
            }

            if (mass > 100)
            {
                shader.SetColor(R, G, B, A);
                shader.Draw(x, y, size*2, size*2, 10);
            }
            else
            {
                var ellipseInst = inst as myEllipseInst;

                ellipseInst.setInstanceCoords(x, y, 2 * size, angle);
                ellipseInst.setInstanceColor(R, G, B, A);
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Process(Window window)
        {
            uint cnt = 0;
            initShapes();

            clearScreenSetup(doClearBuffer, 0.15f, true);

            while (list.Count < n)
            {
                list.Add(new myObj_580());
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
                    myPrimitive._LineInst.ResetBuffer();

                    for (int i = 0; i != Count; i++)
                    {
                        var obj = list[i] as myObj_580;

                        obj.Show();
                        obj.Move();
                    }

                    myPrimitive._LineInst.Draw();

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
                    list.Add(new myObj_580());
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
            base.initShapes(shape, N, 0);

            myPrimitive.init_LineInst(doUseTrails ? n * nTrail : N);

            grad = new myScreenGradient();
            grad.SetRandomColors(rand, 0.2f, mode: 0);

            getShader();

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void getShader()
        {
            shader = new myFreeShader($@"
                        float circle(vec2 uv, float rad) {{ return smoothstep(rad, rad - 0.005, length(uv)); }}
                    ",

                    $@"
                        vec2 uv = (gl_FragCoord.xy / iResolution.xy * 2.0 - 1.0);

                        uv -= Pos.xy;
                        uv *= aspect;

                        result = vec4(myColor * (circle(uv, Pos.z) + 0.75 * circle(uv, Pos.z * 0.75)));
                    "
            );
        }

        // ---------------------------------------------------------------------------------------------------------------
    }
};
