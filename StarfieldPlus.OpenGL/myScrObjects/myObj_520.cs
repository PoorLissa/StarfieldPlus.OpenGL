using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;


/*
    - Static pulsating shapes
*/


namespace my
{
    public class myObj_520 : myObject
    {
        // Priority
        public static int Priority => 10;
        public static System.Type Type => typeof(myObj_520);

        private float x, y, dSize, maxSize;
        private float size, A, R, G, B, angle = 0, dAngle = 0;

        private static int N = 0, shape = 0, mode = 0, dSizeMode = 0, s_maxSize = 0;
        private static bool doFillShapes = false, doRotate = false;
        private static float dimAlpha = 0.05f, dSizeStatic = 0;

        private static myFreeShader shader = null;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_520()
        {
            if (id != uint.MaxValue)
                generateNew();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time global initialization
        protected override void initGlobal()
        {
            colorPicker = new myColorPicker(gl_Width, gl_Height, mode: myColorPicker.colorMode.SNAPSHOT_OR_IMAGE);
            list = new List<myObject>();

            // Global unmutable constants
            {
                N = rand.Next(11111) + 111;

                shape = rand.Next(6);

                if (myUtils.randomChance(rand, 7, 10))
                {
                    shape = myUtils.randomChance(rand, 5, 10) ? 2 : 5;

                    if (shape == 5)
                    {
                        // Custom shader is not instanced, so too much objects is a no-go
                        N = rand.Next(2345) + 1111;
                    }
                }
            }

            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time local initialization
        private void initLocal()
        {
            doClearBuffer = myUtils.randomBool(rand);
            doFillShapes  = myUtils.randomChance(rand, 1, 3);
            doRotate = myUtils.randomChance(rand, 1, 3);

            s_maxSize = rand.Next(50) + 7;
            mode = rand.Next(2);
            dSizeMode = rand.Next(3);

            dSizeStatic = myUtils.randFloat(rand, 0.1f) * 0.25f;

            if (doFillShapes)
                doClearBuffer = true;

            renderDelay = rand.Next(11) + 1;

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            height = 600;

            string nStr(int   n) { return n.ToString("N0");    }
            string fStr(float f) { return f.ToString("0.000"); }

            string str = $"Obj = {Type}\n\n"                      	 +
                            $"N = {nStr(list.Count)} of {nStr(N)}\n" +
                            $"mode = {mode}\n"                       +
                            $"dSizeMode = {dSizeMode}\n"             +
                            $"dSizeStatic = {fStr(dSizeStatic)}\n"   +
                            $"shape = {shape}\n"                     +
                            $"doClearBuffer = {doClearBuffer}\n"     +
                            $"doRotate = {doRotate}\n"               +
                            $"s_maxSize = {s_maxSize}\n"             +
                            $"renderDelay = {renderDelay}\n"         +
                            $"file: {colorPicker.GetFileName()}"
                ;
            return str;
        }

        // ---------------------------------------------------------------------------------------------------------------

        // 
        protected override void setNextMode()
        {
            getShader();

            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void generateNew()
        {
            x = rand.Next(gl_Width);
            y = rand.Next(gl_Height);

            switch (mode)
            {
                case 0:
                    maxSize = s_maxSize;
                    break;

                case 1:
                    maxSize = rand.Next(33) + 3;
                    break;
            }

            size = 0;

            switch (dSizeMode)
            {
                case 0:
                    dSize = myUtils.randFloat(rand, 0.1f) * 0.1f;
                    break;

                case 1:
                    dSize = dSizeStatic;
                    break;

                case 2:
                    dSize = dSizeStatic * 0.5f + myUtils.randFloat(rand, 0.1f) * 0.05f;
                    break;
            }

            colorPicker.getColor(x, y, ref R, ref G, ref B);

            A = myUtils.randFloat(rand, 0.1f);

            if (doRotate)
            {
                dAngle = myUtils.randomSign(rand) * myUtils.randFloat(rand) * 0.025f;
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            size += dSize;
            angle += dAngle;

            if (size < 0 || size > maxSize)
            {
                dSize *= -1;

                if (size < 0)
                {
                    if (myUtils.randomChance(rand, 1, 10))
                    {
                        generateNew();
                    }
                }
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Show()
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

                // Custom shader
                case 5:
                    shader.SetColor(R + 0.1f, G + 0.1f, B + 0.1f, A);
                    //shader.Draw((int)x, (int)y, size, size, 10);
                    shader.Draw((int)x, (int)y, size2x, size2x, 10);
                    break;
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Process(Window window)
        {
            if (false)
            {
                int NN = 123;

                var mfsi = new myFreeShaderInst(NN);

                glDrawBuffer(GL_FRONT_AND_BACK | GL_DEPTH_BUFFER_BIT);
                glClearColor(0, 0, 0, 1);

                while (!Glfw.WindowShouldClose(window))
                {
                    processInput(window);

                    // Swap fore/back framebuffers, and poll for operating system events.
                    Glfw.SwapBuffers(window);
                    Glfw.PollEvents();

                    // Dim screen
                    glClear(GL_COLOR_BUFFER_BIT);

                    // Render frame
                    {
                        mfsi.ResetBuffer();

                        for (int i = 0; i != list.Count; i++)
                        {
                            var obj = list[i] as myObj_520;

                            //obj.Show();

                            mfsi.setInstanceCoords(obj.x - obj.size, obj.y - obj.size, obj.size * 2, obj.size * 2);
                            mfsi.setInstanceColor(obj.R, obj.G, obj.B, obj.A);
                            mfsi.setInstanceAngle(obj.angle);

                            obj.Move();
                        }

                        if (doFillShapes)
                        {
                            // Tell the fragment shader to multiply existing instance opacity by 0.5:
                            mfsi.SetColorA(-0.5f);
                            mfsi.Draw(true);
                        }

                        // Tell the fragment shader to do nothing with the existing instance opacity:
                        mfsi.SetColorA(0);
                        mfsi.Draw(false);
                    }

                    if (list.Count < NN)
                    {
                        list.Add(new myObj_520());
                    }

                    System.Threading.Thread.Sleep(renderDelay);
                }

                return;
            }



            uint cnt = 0;
            initShapes();

            getShader();

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
                    if (shape == 5)
                    {
                        for (int i = 0; i != Count; i++)
                        {
                            var obj = list[i] as myObj_520;

                            obj.Show();
                            obj.Move();
                        }
                    }
                    else
                    {
                        inst.ResetBuffer();

                        for (int i = 0; i != Count; i++)
                        {
                            var obj = list[i] as myObj_520;

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
                }

                if (Count < N)
                {
                    list.Add(new myObj_520());
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

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void getShader()
        {
            if (shape == 5)
            {
                string func = "";

                switch (rand.Next(3))
                {
                    case 0: func = "circle1"; break;
                    case 1: func = "circle2"; break;
                    case 2: func = "circle3"; break;
                }

                shader = new myFreeShader($@"
                        float circle1(vec2 uv, float rad) {{ return smoothstep(rad, rad - 0.005, length(uv)); }}
                        float circle2(vec2 uv, float rad) {{ return 1.0 - smoothstep(0.0, 0.00275, abs(rad-length(uv))); }}
                        float circle3(vec2 uv, float rad) {{ return 0.25 * circle1(uv, Pos.z) + circle2(uv, Pos.z); }}
                    ",

                        $@"
                            vec2 uv = (gl_FragCoord.xy / iResolution.xy * 2.0 - 1.0);

                            uv -= Pos.xy;
                            uv *= aspect;

                            float circ = {func}(uv, Pos.z);
                            result = vec4(myColor * circ);
                        "
                );
            }
        }

        // ---------------------------------------------------------------------------------------------------------------

    }
};
