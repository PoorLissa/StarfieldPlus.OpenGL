using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;
using System.Drawing;


/*
    - lines with trail history -- test
*/


namespace my
{
    public class myObj_011a : myObject
    {
        private class point
        {
            public float x, y, dx, dy;
        };

        // ---------------------------------------------------------------------------------------------------------------

        // Priority
        public static int Priority => 99910;

        private point pt1, pt2;
        private float A, R, G, B;
        private float[] trail = null;

        private static int N = 0, nTrail = 250;

        private static myFreeShader shader = null;
        static myTexRectangle tex = null;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_011a()
        {
            if (id != uint.MaxValue)
            {
                trail = new float[nTrail * 4];

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

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            height = 600;

            string nStr(int   n) { return n.ToString("N0");    }
            string fStr(float f) { return f.ToString("0.000"); }

            string str = $"Obj = myObj_011a\n\n"                     +
                            $"N = {nStr(list.Count)} of {nStr(N)}\n" +
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
            if (pt1 == null)
                pt1 = new point();

            if (pt2 == null)
                pt2 = new point();

            float spdFactor = 10;

            pt1.x = rand.Next(gl_Width);
            pt1.y = rand.Next(gl_Height);
            pt1.dx = myUtils.randFloat(rand, 0.1f) * myUtils.randomSign(rand) * spdFactor;
            pt1.dy = myUtils.randFloat(rand, 0.1f) * myUtils.randomSign(rand) * spdFactor;

            pt2.x = rand.Next(gl_Width);
            pt2.y = rand.Next(gl_Height);
            pt2.dx = myUtils.randFloat(rand, 0.1f) * myUtils.randomSign(rand) * spdFactor;
            pt2.dy = myUtils.randFloat(rand, 0.1f) * myUtils.randomSign(rand) * spdFactor;

            A = 0.25f + myUtils.randFloat(rand) * 0.25f;
            colorPicker.getColorRand(ref R, ref G, ref B);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void movePt(ref point p)
        {
            int offset = 333;
            int moveMode = 1;

            p.x += p.dx;
            p.y += p.dy;

            switch (moveMode)
            {
                case 0:
                    {
                        if (p.x < 0 && p.dx < 0)
                            p.dx *= -1;

                        if (p.y < 0 && p.dy < 0)
                            p.dy *= -1;

                        if (p.x > gl_Width && p.dx > 0)
                            p.dx *= -1;

                        if (p.y > gl_Height && p.dy > 0)
                            p.dy *= -1;
                    }
                    break;

                case 1:
                    {
                        float val = 0.13f;

                        if (p.x < offset)
                            p.dx += val;

                        if (p.y < offset)
                            p.dy += val;

                        if (p.x > gl_Width - offset)
                            p.dx -= val;

                        if (p.y > gl_Height - offset)
                            p.dy -= val;
                    }
                    break;

                case 2:
                    {
                        float val = myUtils.randFloat(rand) * 0.15f;

                        if (p.x < offset)
                            p.dx += val;

                        if (p.y < offset)
                            p.dy += val;

                        if (p.x > gl_Width - offset)
                            p.dx -= val;

                        if (p.y > gl_Height - offset)
                            p.dy -= val;
                    }
                    break;
            }
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            // Update trail info
            {
                for (int i = nTrail - 1; i > 0; i--)
                {
                    for (int j = 0; j < 4; j++)
                    {
                        int idx1 = ((i - 0) * 4) + j;
                        int idx2 = ((i - 1) * 4) + j;

                        trail[idx1] = trail[idx2];
                    }
                }
            }

            movePt(ref pt1);
            movePt(ref pt2);

            trail[0] = pt1.x;
            trail[1] = pt1.y;
            trail[2] = pt2.x;
            trail[3] = pt2.y;

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Show()
        {
            float a = A;
            float da = A / (nTrail + 1);
  
            {
                for (int i = 0; i < nTrail-1; i++)
                {
                    int idx1 = i * 4;
                    int idx2 = i * 4 + 4;

                    myPrimitive._LineInst.setInstanceCoords(trail[idx1 + 0], trail[idx1 + 1], trail[idx2 + 0], trail[idx2 + 1]);
                    myPrimitive._LineInst.setInstanceColor(R, G, B, a);

                    myPrimitive._LineInst.setInstanceCoords(trail[idx1 + 2], trail[idx1 + 3], trail[idx2 + 2], trail[idx2 + 3]);
                    myPrimitive._LineInst.setInstanceColor(R, G, B, a);

                    a -= da;
                }

                shader.SetColor(R, G, B, A*1.5f);
                shader.Draw(trail[0], trail[1], 8, 8, 10);
                shader.Draw(trail[2], trail[3], 8, 8, 10);

                return;
            }


            myPrimitive._LineInst.setInstanceCoords(pt1.x, pt1.y, pt2.x, pt2.y);
            myPrimitive._LineInst.setInstanceColor(R, G, B, 1);

            a -= da;

            for (int i = 0; i < nTrail; i++)
            {
                int idx = i * 4;

                myPrimitive._LineInst.setInstanceCoords(trail[idx + 0], trail[idx + 1], trail[idx + 2], trail[idx + 3]);
                myPrimitive._LineInst.setInstanceColor(R, G, B, a);
                a -= da;
            }
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
            myPrimitive.init_LineInst(2 * N * nTrail);
            myPrimitive._LineInst.setLineWidth(1 + rand.Next(11));

            getShader();

            if (colorPicker.getImg() != null && myUtils.randomChance(rand, 1, 3))
            {
                tex = new myTexRectangle(colorPicker.getImg());
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
