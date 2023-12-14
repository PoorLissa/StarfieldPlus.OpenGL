using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;


/*
    - 3 rotating points per particle, making a rotating triangle
*/


namespace my
{
    public class myObj_850 : myObject
    {
        // Priority
        public static int Priority => 10;
		public static System.Type Type => typeof(myObj_850);

        private int cnt;
        private float x, y, x1, y1, r1, a1, da1, x2, y2, r2, a2, da2, x3, y3, r3, a3, da3;
        private float A, R, G, B;
        private myParticleTrail[] trails = null;

        private static int N = 0, shape = 0, nTrail = 0;
        private static bool doFillShapes = false;
        private static float dimAlpha = 0.05f, lineWidth = 1.0f;

        private static myScreenGradient grad = null;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_850()
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
                N = rand.Next(10) + 10;
                N = 11 + rand.Next(150);

                shape = 0;

                nTrail = 111;

                lineWidth = 0.5f + myUtils.randFloat(rand) * rand.Next(7);
            }

            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time local initialization
        private void initLocal()
        {
            doClearBuffer = myUtils.randomChance(rand, 2, 3);

            dimAlpha = 0.025f + myUtils.randFloat(rand) * 0.075f;

            renderDelay = rand.Next(3);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            height = 600;

            string nStr(int   n) { return n.ToString("N0");    }
            string fStr(float f) { return f.ToString("0.000"); }

            string str = $"Obj = {Type}\n\n"                         +
                            $"N = {nStr(list.Count)} of {nStr(N)}\n" +
                            $"nTrail = {nTrail}\n"                   +
                            $"lineWidth = {fStr(lineWidth)}\n"       +
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
            cnt = 1111 + rand.Next(1111);

            int max = 333;

            x = rand.Next(gl_Width);
            y = rand.Next(gl_Height);

            r1 = rand.Next(max);
            r2 = rand.Next(max);
            r3 = rand.Next(max);

            a1 = myUtils.randFloat(rand) * rand.Next(11);
            a2 = myUtils.randFloat(rand) * rand.Next(11);
            a3 = myUtils.randFloat(rand) * rand.Next(11);

            da1 = myUtils.randFloat(rand, 0.1f) * 0.02f * myUtils.randomSign(rand);
            da2 = myUtils.randFloat(rand, 0.1f) * 0.02f * myUtils.randomSign(rand);
            da3 = myUtils.randFloat(rand, 0.1f) * 0.02f * myUtils.randomSign(rand);

            x1 = x + r1 * (float)Math.Sin(a1);
            y1 = y + r1 * (float)Math.Cos(a1);

            x2 = x + r2 * (float)Math.Sin(a2);
            y2 = y + r2 * (float)Math.Cos(a2);

            x3 = x + r3 * (float)Math.Sin(a3);
            y3 = y + r3 * (float)Math.Cos(a3);

            A = 0.33f + myUtils.randFloat(rand) * 0.5f;
            colorPicker.getColor(x, y, ref R, ref G, ref B);

            if (trails == null)
            {
                trails = new myParticleTrail[3];

                trails[0] = new myParticleTrail(nTrail, x1, y1);
                trails[1] = new myParticleTrail(nTrail, x2, y2);
                trails[2] = new myParticleTrail(nTrail, x3, y3);
            }
            else
            {
                trails[0].reset(x1, y1);
                trails[1].reset(x2, y2);
                trails[2].reset(x3, y3);
            }

            trails[0].updateDa(1);
            trails[1].updateDa(1);
            trails[2].updateDa(1);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            a1 += da1;
            a2 += da2;
            a3 += da3;

            x1 = x + r1 * (float)Math.Sin(a1);
            y1 = y + r1 * (float)Math.Cos(a1);

            x2 = x + r2 * (float)Math.Sin(a2);
            y2 = y + r2 * (float)Math.Cos(a2);

            x3 = x + r3 * (float)Math.Sin(a3);
            y3 = y + r3 * (float)Math.Cos(a3);

            trails[0].update(x1, y1);
            trails[1].update(x2, y2);
            trails[2].update(x3, y3);

            if (--cnt < 0)
            {
                A -= myUtils.randFloat(rand) * 0.0025f;

                if (A < 0)
                    generateNew();
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Show()
        {
            int size = 2;

            trails[0].Show(R, G, B, A > 1 ? 1 : A);
            trails[1].Show(R, G, B, A > 1 ? 1 : A);
            trails[2].Show(R, G, B, A > 1 ? 1 : A);

            myPrimitive._RectangleInst.setInstanceCoords(x1 - size, y1 - size, size * 2, size * 2);
            myPrimitive._RectangleInst.setInstanceColor(R, G, B, A);
            myPrimitive._RectangleInst.setInstanceAngle(0);

            myPrimitive._RectangleInst.setInstanceCoords(x2 - size, y2 - size, size * 2, size * 2);
            myPrimitive._RectangleInst.setInstanceColor(R, G, B, A);
            myPrimitive._RectangleInst.setInstanceAngle(0);

            myPrimitive._RectangleInst.setInstanceCoords(x3 - size, y3 - size, size * 2, size * 2);
            myPrimitive._RectangleInst.setInstanceColor(R, G, B, A);
            myPrimitive._RectangleInst.setInstanceAngle(0);

            float a = A * 0.333f;

            myPrimitive._LineInst.setLineWidth(lineWidth);

            myPrimitive._LineInst.setInstanceCoords(x1, y1, x2, y2);
            myPrimitive._LineInst.setInstanceColor(R, G, B, a);

            myPrimitive._LineInst.setInstanceCoords(x2, y2, x3, y3);
            myPrimitive._LineInst.setInstanceColor(R, G, B, a);

            myPrimitive._LineInst.setInstanceCoords(x3, y3, x1, y1);
            myPrimitive._LineInst.setInstanceColor(R, G, B, a);

            myPrimitive._LineInst.setLineWidth(1);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Process(Window window)
        {
            uint cnt = 0;
            initShapes();

            // Disable VSYNC if needed
            // Glfw.SwapInterval(0);

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
                    grad.Draw();
                }

                // Render Frame
                {
                    inst.ResetBuffer();
                    myPrimitive._LineInst.ResetBuffer();

                    for (int i = 0; i != Count; i++)
                    {
                        var obj = list[i] as myObj_850;

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
                    list.Add(new myObj_850());
                }

                cnt++;
                System.Threading.Thread.Sleep(renderDelay);
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void initShapes()
        {
            base.initShapes(shape, N * 3, 0);

            myPrimitive.init_LineInst(N * 3 + N * 3 * nTrail);

            grad = new myScreenGradient();
            grad.SetRandomColors(rand, 0.2f);

            if (doClearBuffer == false)
                grad.SetOpacity(dimAlpha);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------
    }
};
