using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;


/*
    - 
*/


namespace my
{
    public class myObj_1150 : myObject
    {
        // Priority
        public static int Priority => 9910;
		public static System.Type Type => typeof(myObj_1150);

        private float x, y, dx, dy;
        private float size, A, R, G, B, angle = 0, dAngle;

        private static int N = 0, n = 1, shape = 0, angleMode = 0, moveMode = 0;
        private static bool doFillShapes = false;
        private static float dimAlpha = 0.05f, dA = 1;

        private static myScreenGradient grad = null;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_1150()
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
                N = 3333;
                n = 1 + rand.Next(3);

                shape = rand.Next(5);
            }

            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time local initialization
        private void initLocal()
        {
            doClearBuffer = myUtils.randomBool(rand);
            doFillShapes = myUtils.randomBool(rand);

            angleMode = rand.Next(4);
            moveMode = rand.Next(3);
            renderDelay = rand.Next(3) + 1;

            dA = 0.0002f;
            dA = 0.0001f + myUtils.randFloat(rand) * 0.002f;

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            height = 600;

            string str = $"Obj = {Type}\n\n"                  +
                            myUtils.strCountOf(list.Count, N) +
                            $"n = {n}\n"                      +
                            $"dA = {myUtils.fStr(dA)}\n"      +
                            $"renderDelay = {renderDelay}\n"  +
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
            if (id < n)
            {
                float speedFactor = 2.0f;

                x = rand.Next(gl_Width);
                y = rand.Next(gl_Height);

                dx = myUtils.randFloatSigned(rand, 0.25f) * speedFactor;
                dy = myUtils.randFloatSigned(rand, 0.25f) * speedFactor;

                size = 3;

                A = 0.5f;
                R = G = B = 1;
            }
            else
            {
                var parent_id = rand.Next(n);
                var parent = list[parent_id] as myObj_1150;

                x = parent.x;
                y = parent.y;
                dx = dy = 0;
                size = 1;

                A = myUtils.randFloat(rand);
                colorPicker.getColor(x, y, ref R, ref G, ref B);

                angle = myUtils.randFloat(rand) * 321;

                float dAngleFactor = 0.001f;

                switch (angleMode)
                {
                    case 0:
                    case 1:
                        dAngle = myUtils.randFloatSigned(rand) * dAngleFactor;
                        break;

                    case 2:
                        dAngle = myUtils.randFloat(rand) * dAngleFactor * +1;
                        break;

                    case 3:
                        dAngle = myUtils.randFloat(rand) * dAngleFactor * -1;
                        break;
                }
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            x += dx;
            y += dy;

            if (id < n)
            {
                switch (moveMode)
                {
                    case 0:
                        {
                            if (x <= 0 || x >= gl_Width)
                                dx *= -1;

                            if (y <= 0 || y >= gl_Height)
                                dy *= -1;
                        }
                        break;

                    case 1:
                        {
                            if (x < 0)
                                dx += 0.01f;

                            if (x > gl_Width)
                                dx -= 0.01f;

                            if (y < 0)
                                dy += 0.01f;

                            if (y > gl_Height)
                                dy -= 0.01f;
                        }
                        break;

                    case 2:
                        {
                            if (myUtils.randomChance(rand, 1, 333))
                            {
                                if (dx != 0)
                                {
                                    dx = 0;
                                    dy = myUtils.randFloatSigned(rand, 0.25f) * 2;
                                }
                                else
                                {
                                    dy = 0;
                                    dx = myUtils.randFloatSigned(rand, 0.25f) * 2;
                                }
                            }

                            if (x < 0)
                            {
                                x = 0;
                                dx = 0;
                                dy = myUtils.randFloatSigned(rand, 0.25f) * 2;
                            }

                            if (x > gl_Width)
                            {
                                x = gl_Width;
                                dx = 0;
                                dy = myUtils.randFloatSigned(rand, 0.25f) * 2;
                            }

                            if (y < 0)
                            {
                                y = 0;
                                dy = 0;
                                dx = myUtils.randFloatSigned(rand, 0.25f) * 2;
                            }

                            if (y > gl_Height)
                            {
                                y = gl_Height;
                                dy = 0;
                                dx = myUtils.randFloatSigned(rand, 0.25f) * 2;
                            }
                        }
                        break;
                }
            }
            else
            {
                A -= dA;
                size += 0.1f;
                angle += dAngle;

                if (A < 0)
                    generateNew();
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
                    myPrimitive._RectangleInst.setInstanceCoords(x - size, y - size, size2x, size2x);
                    myPrimitive._RectangleInst.setInstanceColor(R, G, B, A);
                    myPrimitive._RectangleInst.setInstanceAngle(angle);
                    break;

                // Instanced triangles
                case 1:
                    myPrimitive._TriangleInst.setInstanceCoords(x, y, size2x, angle);
                    myPrimitive._TriangleInst.setInstanceColor(R, G, B, A);
                    break;

                // Instanced circles
                case 2:
                    myPrimitive._EllipseInst.setInstanceCoords(x, y, size2x, angle);
                    myPrimitive._EllipseInst.setInstanceColor(R, G, B, A);
                    break;

                // Instanced pentagons
                case 3:
                    myPrimitive._PentagonInst.setInstanceCoords(x, y, size2x, angle);
                    myPrimitive._PentagonInst.setInstanceColor(R, G, B, A);
                    break;

                // Instanced hexagons
                case 4:
                    myPrimitive._HexagonInst.setInstanceCoords(x, y, size2x, angle);
                    myPrimitive._HexagonInst.setInstanceColor(R, G, B, A);
                    break;
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
                        glClear(GL_COLOR_BUFFER_BIT);

                    grad.Draw();
                }

                // Render Frame
                {
                    inst.ResetBuffer();

                    for (int i = 0; i != Count; i++)
                    {
                        var obj = list[i] as myObj_1150;

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

                if (Count < N)
                {
                    list.Add(new myObj_1150());
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

            grad = new myScreenGradient();
            grad.SetRandomColors(rand, 0.2f);

            grad.SetOpacity(doClearBuffer ? 1 : 0.2f);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------
    }
};
