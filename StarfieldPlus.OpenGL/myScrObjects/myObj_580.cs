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

        private float x, y, dx, dy;
        private float size, A, R, G, B, mass, angle = 0;
        private bool isFirst;

        private static int N = 0, shape = 0;
        private static bool doFillShapes = false;
        private static float dimAlpha = 0.05f;

        private static int n = 5;

        private static myFreeShader shader = null;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_580()
        {
            isFirst = true;

            if (id != uint.MaxValue)
            {
                mass = id < n ? 10000 : (rand.Next(25) + 1);

                if (id == 1)
                    mass = 100000;

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
                n = 3 + rand.Next(5);

                N = rand.Next(1000) + 10000;
                shape = 2;

                dimAlpha = 0.25f;
            }

            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time local initialization
        private void initLocal()
        {
            doClearBuffer = myUtils.randomBool(rand);

            renderDelay = rand.Next(11) + 3;

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            height = 600;

            string nStr(int   n) { return n.ToString("N0");    }
            //string fStr(float f) { return f.ToString("0.000"); }

            string str = $"Obj = myObj_580\n\n"                       +
                            $"N = {nStr(list.Count)} of {nStr(N)}\n"  +
                            $"doClearBuffer = {doClearBuffer}\n"      +
                            $"renderDelay = {renderDelay}\n"          +
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

            if (isFirst)
            {
                isFirst = false;

                size = mass < 100 ? 2 : 11;
                colorPicker.getColor(x, y, ref R, ref G, ref B);
                A = mass < 100 ? 0.25f : 1.0f;
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            x += dx;
            y += dy;

            for (int i = 0; i < n; i++)
            {
                var obj = list[i] as myObj_580;

                if (id != obj.id)
                {
                    float dX = obj.x - x;
                    float dY = obj.y - y;

                    float dist = 1.0f / (float)Math.Sqrt(dX * dX + dY * dY);

#if false
                    float factor = mass > 100
                        ? mass * obj.mass * 0.0000000005f
                        : mass * obj.mass * 0.0000001f;
#else
                    float factor = mass > 100
                        ? obj.mass * 0.000001f
                        : obj.mass * 0.0000001f;
#endif

                    dx += factor * dX * dist;
                    dy += factor * dY * dist;
                }
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Show()
        {
            if (mass > 100)
            {
                shader.SetColor(R, G, B, A);
                shader.Draw(x, y, size*2, size*2, 10);
            }
            else
            {
                float size2x = size * 2;

                switch (shape)
                {
                    // Instanced squares
                    case 0:
                        var rectInst = inst as myRectangleInst;

                        rectInst.setInstanceCoords(x - size, y - size, size2x, size2x);
                        rectInst.setInstanceColor(R, G, B, A);
                        rectInst.setInstanceAngle(angle);
                        break;

                    // Instanced triangles
                    case 1:
                        var triangleInst = inst as myTriangleInst;

                        triangleInst.setInstanceCoords(x, y, size2x, angle);
                        triangleInst.setInstanceColor(R, G, B, A);
                        break;

                    // Instanced circles
                    case 2:
                        var ellipseInst = inst as myEllipseInst;

                        ellipseInst.setInstanceCoords(x, y, size2x, angle);
                        ellipseInst.setInstanceColor(R, G, B, A);
                        break;

                    // Instanced pentagons
                    case 3:
                        var pentagonInst = inst as myPentagonInst;

                        pentagonInst.setInstanceCoords(x, y, size2x, angle);
                        pentagonInst.setInstanceColor(R, G, B, A);
                        break;

                    // Instanced hexagons
                    case 4:
                        var hexagonInst = inst as myHexagonInst;

                        hexagonInst.setInstanceCoords(x, y, size2x, angle);
                        hexagonInst.setInstanceColor(R, G, B, A);
                        break;
                }
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
                        var obj = list[i] as myObj_580;

                        obj.Show();
                        obj.Move();
                    }

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

                if (list.Count < N)
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
