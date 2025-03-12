using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;


/*
    - Hexagon grid with a pseudo 3d effect
*/


namespace my
{
    public class myObj_1160 : myObject
    {
        // Priority
        public static int Priority => 99999910;
		public static System.Type Type => typeof(myObj_1160);

        private int lifeCounter;
        private float x, y, dx, dy;
        private float size, A, R, G, B, angle = 0;

        private static int N = 0, n = 0, baseSize = 0;
        private static bool doFillShapes = false;
        private static float dimAlpha = 0.05f, t = 0, dt = 0, sinPi3 = 0, lineWidth = 1;

        private static myScreenGradient grad = null;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_1160()
        {
            if (id != uint.MaxValue)
                generateNew();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time global initialization
        protected override void initGlobal()
        {
            var colorMode = myUtils.randomChance(rand, 1, 5)
                ? myColorPicker.colorMode.RANDOM_MODE
                : myColorPicker.colorMode.SNAPSHOT_OR_IMAGE;

            colorPicker = new myColorPicker(gl_Width, gl_Height, mode: colorMode);
            list = new List<myObject>();

            // Global unmutable constants
            {
                n = 1 + rand.Next(3);
                N = 11 + rand.Next(50);

                baseSize = 20;
                sinPi3 = (float)Math.Sin(Math.PI / 3);
            }

            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time local initialization
        private void initLocal()
        {
            doClearBuffer = myUtils.randomBool(rand);
            doClearBuffer = false;
            doFillShapes = myUtils.randomChance(rand, 4, 5);

            renderDelay = rand.Next(11) + 3;

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            height = 600;

            string str = $"Obj = {Type}\n\n"                  +
                            myUtils.strCountOf(list.Count, N) +
                            $"n = {n}\n"                      +
                            $"baseSize = {baseSize}\n"        +
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
                x = rand.Next(gl_Width);
                y = rand.Next(gl_Height);

                dx = myUtils.randFloatSigned(rand, 0.25f) * 3;
                dy = myUtils.randFloatSigned(rand, 0.25f) * 3;
            }
            else
            {
                // Aligh hex to the grid
                {
                    //x = rand.Next(gl_Width + 100);
                    //y = rand.Next(gl_Height + 200);

                    int parent_id = rand.Next(n);
                    var parent = list[parent_id] as myObj_1160;

                    x = parent.x + rand.Next(401) - 200;
                    y = parent.y + rand.Next(401) - 200;

                    x -= x % (3 * baseSize / 2);
                    y -= y % (int)(baseSize * sinPi3 * 2);

                    if (x % baseSize / 2 != 0)
                    {
                        y -= (int)(baseSize * sinPi3);
                    }
                }

                A = myUtils.randFloat(rand, 0.25f);
                colorPicker.getColor(x, y, ref R, ref G, ref B);

                size = baseSize - 2;
                lifeCounter = rand.Next(33) + 1;
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            if (id < n)
            {
                x += dx;
                y += dy;

                if (x < 0)
                    dx += 0.2f;

                if (y < 0)
                    dy += 0.2f;

                if (x > gl_Width)
                    dx -= 0.2f;

                if (y > gl_Height)
                    dy -= 0.2f;
            }
            else
            {
                if (--lifeCounter == 0)
                {
                    generateNew();
                }
            }
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Show()
        {
            myPrimitive._HexagonInst.setInstanceCoords(x, y, size * 3.0f, angle);
            myPrimitive._HexagonInst.setInstanceColor(R, G, B, 0.1f);

            myPrimitive._HexagonInst.setInstanceCoords(x, y, size * 2, angle);
            myPrimitive._HexagonInst.setInstanceColor(R, G, B, A);
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

                    for (int i = 0; i != Count; i++)
                    {
                        var obj = list[i] as myObj_1160;

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
                    list.Add(new myObj_1160());
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

            //int qty = (int)((gl_Width / size) * (gl_Height / size)) + 333;
            //base.initShapes(4, qty, 0);

            base.initShapes(4, N * 2, 0);

            grad = new myScreenGradient();
            grad.SetRandomColors(rand, 0.2f);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------
    }
};
