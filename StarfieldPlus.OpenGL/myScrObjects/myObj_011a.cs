using GLFW;
using static OpenGL.GL;
using System.Collections.Generic;


/*
    - Particles with tails
*/


namespace my
{
    public class myObj_011a : myObject
    {
        // Priority
        public static int Priority => 999910;

        private float x, y, dx, dy, a, da;
        private float A, R, G, B;
        private myParticleTrail trail = null;

        private static int N = 0, nTrail = 250;
        private static int moveMode = 0, lineWidth = 1;

        private static myFreeShader shader = null;
        static myTexRectangle tex = null;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_011a()
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
                N = 3 + rand.Next(11);
            }

            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time local initialization
        private void initLocal()
        {
            doClearBuffer = true;
            renderDelay = rand.Next(11) + 3;
            renderDelay = 0;

            moveMode = rand.Next(3);
            lineWidth = rand.Next(11) + 1;

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            height = 600;

            string nStr(int   n) { return n.ToString("N0");    }
            //string fStr(float f) { return f.ToString("0.000"); }

            string str = $"Obj = myObj_011a\n\n"                     +
                            $"N = {nStr(list.Count)} of {nStr(N)}\n" +
                            $"doClearBuffer = {doClearBuffer}\n"     +
                            $"moveMode = {moveMode}\n"               +
                            $"lineWidth = {lineWidth}\n"             +
                            $"nTrail = {nTrail}\n"                   +
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

            myPrimitive._LineInst.setLineWidth(lineWidth);
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void generateNew()
        {
            float spdFactor = 10;

            x = rand.Next(gl_Width);
            y = rand.Next(gl_Height);
            dx = myUtils.randFloat(rand, 0.1f) * myUtils.randomSign(rand) * spdFactor;
            dy = myUtils.randFloat(rand, 0.1f) * myUtils.randomSign(rand) * spdFactor;

            A = 0.25f + myUtils.randFloat(rand) * 0.25f;
            da = A / (nTrail + 1);
            colorPicker.getColorRand(ref R, ref G, ref B);

            if (trail == null)
            {
                trail = new myParticleTrail(nTrail, x, y);
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            // Update trail info
            {
                trail.update(x, y);
            }

            int offset = 333;

            x += dx;
            y += dy;

            a = A;

            switch (moveMode)
            {
                case 0:
                    {
                        if (x < 0 && dx < 0)
                            dx *= -1;

                        if (y < 0 && dy < 0)
                            dy *= -1;

                        if (x > gl_Width && dx > 0)
                            dx *= -1;

                        if (y > gl_Height && dy > 0)
                            dy *= -1;
                    }
                    break;

                case 1:
                    {
                        float val = 0.13f;

                        if (x < offset)
                            dx += val;

                        if (y < offset)
                            dy += val;

                        if (x > gl_Width - offset)
                            dx -= val;

                        if (y > gl_Height - offset)
                            dy -= val;
                    }
                    break;

                case 2:
                    {
                        float val = myUtils.randFloat(rand) * 0.15f;

                        if (x < offset)
                            dx += val;

                        if (y < offset)
                            dy += val;

                        if (x > gl_Width - offset)
                            dx -= val;

                        if (y > gl_Height - offset)
                            dy -= val;
                    }
                    break;
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Show()
        {
            float x1 = 0, y1 = 0, x2 = 0, y2 = 0;
            int i = 0;

            trail.getXY(i++, ref x1, ref y1);

            for (; i < nTrail; i++)
            {
                trail.getXY(i, ref x2, ref y2);

                myPrimitive._LineInst.setInstanceCoords(x1, y1, x2, y2);
                myPrimitive._LineInst.setInstanceColor(R, G, B, a);

                x1 = x2;
                y1 = y2;

                a -= da;
            }

            shader.SetColor(R, G, B, A*1.5f);
            shader.Draw(x, y, 8, 8, 10);
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Process(Window window)
        {
            uint cnt = 0;
            initShapes();


            clearScreenSetup(doClearBuffer, 0.13f);
            glDrawBuffer(GL_BACK);


            while (!Glfw.WindowShouldClose(window))
            {
                int Count = list.Count;

                processInput(window);

                // Swap fore/back framebuffers, and poll for operating system events.
                Glfw.SwapBuffers(window);
                Glfw.PollEvents();

                // Dim screen
                {
                    glClear(GL_COLOR_BUFFER_BIT);

                    if (tex != null)
                    {
                        tex.setOpacity(0.9f);
                        tex.Draw(0, 0, gl_Width, gl_Height);
                    }
                }

                // Render Frame
                {
                    myPrimitive._LineInst.ResetBuffer();

                    for (int i = 0; i != Count; i++)
                    {
                        var obj = list[i] as myObj_011a;

                        obj.Show();
                        obj.Move();
                    }

                    myPrimitive._LineInst.Draw();
                }

                if (Count < N)
                {
                    list.Add(new myObj_011a());
                }

                cnt++;
                System.Threading.Thread.Sleep(renderDelay);
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void initShapes()
        {
            myPrimitive.init_LineInst(N * nTrail);
            myPrimitive._LineInst.setLineWidth(lineWidth);

            getShader();

            var mode = (myColorPicker.colorMode)colorPicker.getMode();

            if (mode == myColorPicker.colorMode.IMAGE || mode == myColorPicker.colorMode.SNAPSHOT)
            {
                if (myUtils.randomChance(rand, 1, 3))
                {
                    tex = new myTexRectangle(colorPicker.getImg());
                }
            }
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void getShader()
        {
            string header = "";
            string main = "";

            my.myShaderHelpers.Shapes.getShader_000(ref rand, ref header, ref main);
            shader = new myFreeShader(header, main);
        }

        // ---------------------------------------------------------------------------------------------------------------

    }
};
