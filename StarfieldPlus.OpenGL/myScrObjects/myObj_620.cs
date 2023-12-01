using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;


/*
    - rectangles with width/height that are changing constantly;
      while width is increasing, height is decreasing, and vice versa

    z-ordering:
    https://www.reddit.com/r/opengl/comments/6ctlrj/how_to_get_2d_z_ordering_working/
    https://stackoverflow.com/questions/58386635/global-translucency-sorting-with-instanced-rendering
*/


namespace my
{
    public class myObj_620 : myObject
    {
        // Priority
        public static int Priority => 10;
        public static System.Type Type => typeof(myObj_620);

        private float x, y, width, height, dx, dy, dWidth, dHeight;
        private float A, R, G, B;

        private static int N = 0, mode = 0, max = 666, addSpeed = 0;
        private static bool doFillShapes = false, doShowDots = true, doAccountForA = true;
        private static float dimAlpha = 0.05f, spdFactor = 1.0f;

        private static myFreeShader shader = null;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_620()
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
                N = 33;

                switch (rand.Next(11))
                {
                    case 0 : N += rand.Next(300); break;
                    case 1 : N += rand.Next(200); break;
                    default: N += rand.Next(100); break;
                }
            }

            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time local initialization
        private void initLocal()
        {
            doClearBuffer = myUtils.randomChance(rand, 11, 13);
            doFillShapes  = myUtils.randomChance(rand, 1, 9);
            doShowDots    = myUtils.randomChance(rand, 4, 5);

            doAccountForA = myUtils.randomBool(rand);           // Max size depends on opacity

            mode = rand.Next(2);

            addSpeed = 50 + rand.Next(100);
            renderDelay = 0;

            spdFactor = 1.0f + 0.1f * rand.Next(31);            // 1 + [0.0 .. 3.0]

            switch (rand.Next(2))
            {
                case 0:
                    max = 666;
                    break;

                case 1:
                    max = 100 + rand.Next(333);
                    break;
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            height = 600;

            string nStr(int   n) { return n.ToString("N0");    }
            string fStr(float f) { return f.ToString("0.000"); }

            string str = $"Obj = {Type}\n\n"                       	  +
                            $"N = {nStr(list.Count)} of {nStr(N)}\n"  +
                            $"doClearBuffer = {doClearBuffer}\n"      +
                            $"doFillShapes = {doFillShapes}\n"        +
                            $"doShowDots = {doShowDots}\n"            +
                            $"doAccountForA = {doAccountForA}\n"      +
                            $"mode = {mode}\n"                        +
                            $"max = {max}\n"                          +
                            $"spdFactor = {fStr(spdFactor)}\n"        +
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
            x = rand.Next(gl_Width);
            y = rand.Next(gl_Height);

            width  = 100 + rand.Next(max);
            height = 100 + rand.Next(max);

            dx = 0;
            dy = 0;

            switch (mode)
            {
                // dWidth/dHeight are the same
                case 0:
                    dWidth  = myUtils.randFloat(rand, 0.1f) * spdFactor;
                    dHeight = dWidth;
                    break;

                // dWidth/dHeight are different
                case 1:
                    dWidth  = myUtils.randFloat(rand, 0.1f) * spdFactor;
                    dHeight = myUtils.randFloat(rand, 0.1f) * spdFactor;
                    break;
            }

            switch (rand.Next(2))
            {
                case 0:
                    dWidth *= -1;
                    break;

                case 1:
                    dHeight *= -1;
                    break;
            }

            A = myUtils.randFloat(rand, 0.1f);
            colorPicker.getColor(x, y, ref R, ref G, ref B);

            if (doAccountForA)
            {
                width *= A;
                height *= A;
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            x += dx;
            y += dy;

            width  += dWidth;
            height += dHeight;

            if (x < 0 || x > gl_Width || y < 0 || y > gl_Height)
            {
                generateNew();
            }

            if (width < 1 || height < 1)
            {
                dWidth  *= -1;
                dHeight *= -1;
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Show()
        {
            myPrimitive._RectangleInst.setInstanceCoords(x - width, y - height, 2 * width, 2 * height);
            myPrimitive._RectangleInst.setInstanceColor(R, G, B, A);
            myPrimitive._RectangleInst.setInstanceAngle(0);

            if (doShowDots)
            {
                float size = 14;

                shader.SetColor(R, G, B, A);
                shader.Draw(x - width, y - height, size, size, 10);
                shader.Draw(x - width, y + height, size, size, 10);
                shader.Draw(x + width, y - height, size, size, 10);
                shader.Draw(x + width, y + height, size, size, 10);
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Process(Window window)
        {
            uint cnt = 0;
            initShapes();

            clearScreenSetup(doClearBuffer, 0.1f);

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
                    inst.ResetBuffer();

                    for (int i = 0; i != Count; i++)
                    {
                        var obj = list[i] as myObj_620;

                        obj.Show();
                        obj.Move();
                    }

                    if (doFillShapes)
                    {
                        inst.SetColorA(0.1f);
                        inst.Draw(true);
                    }

                    inst.SetColorA(0);
                    inst.Draw(false);
                }

                if (Count < N && cnt == addSpeed)
                {
                    cnt = 0;
                    list.Add(new myObj_620());
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
            base.initShapes(0, N, 0);

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
