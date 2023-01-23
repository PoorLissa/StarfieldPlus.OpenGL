using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;


/*
    - ...
*/


namespace my
{
    public class myObj_350 : myObject
    {
        private myObj_350 parent = null;

        private int level;
        private float x, y, dx, dy;
        private float size, A, R, G, B, angle = 0;

        private static int N = 0, nChildren = 3, shape = 0, levelBase = 0;
        private static bool doFillShapes = false;
        private static float dimAlpha = 0.05f;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_350()
        {
            level = levelBase;
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
                N = 123;
                nChildren = 11;
                shape = 0;
            }

            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time local initialization
        private void initLocal()
        {
            doClearBuffer = myUtils.randomBool(rand);

            doClearBuffer = true;

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            height = 800;

            string str = $"Obj = myObj_350\n\n"                     +
                            $"N = {list.Count} of {N}\n"            +
                            $"file: {colorPicker.GetFileName()}"    +
                            $""
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
            level = levelBase;

            if (id != uint.MaxValue)
            {
                if (level == 0)
                {
                    x = rand.Next(gl_Width);
                    y = rand.Next(gl_Height);

                    size = 50;
                    dx = myUtils.randomSign(rand) * myUtils.randFloat(rand, 0.5f) * (rand.Next(5) + 3);
                    dy = myUtils.randomSign(rand) * myUtils.randFloat(rand, 0.5f) * (rand.Next(5) + 3);

                    A = 0;
                    R = (float)rand.NextDouble();
                    G = (float)rand.NextDouble();
                    B = (float)rand.NextDouble();

                    // Add child objects
                    this.addChildren();
                }
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void addChildren()
        {
            levelBase++;

            for (int i = 0; i < nChildren; i++)
            {
                var obj = new myObj_350();

                obj.parent = this;

                obj.x = obj.parent.x;
                obj.y = obj.parent.y;

                obj.size = 3;
                obj.R = obj.parent.R;
                obj.G = obj.parent.G;
                obj.B = obj.parent.B;
                obj.A = 0.5f;

                obj.dx = myUtils.randomSign(rand) * myUtils.randFloat(rand, 0.1f) * (rand.Next(3) + 1);
                obj.dy = myUtils.randomSign(rand) * myUtils.randFloat(rand, 0.1f) * (rand.Next(3) + 1);

                list.Add(obj);
            }

            levelBase--;
        }

        protected override void Move()
        {
            x += dx;
            y += dy;

            if (parent == null)
            {
                if (x < 100)
                    dx += 0.1f;

                if (y < 100)
                    dy += 0.1f;

                if (x > gl_Width - 100)
                    dx -= 0.1f;

                if (y > gl_Height - 100)
                    dy -= 0.1f;
            }
            else
            {
                float rate = 0.9f;

                if (x < parent.x - parent.size)
                    dx += rate;

                if (y < parent.y - parent.size)
                    dy += rate;

                if (x > parent.x + parent.size)
                    dx -= rate;

                if (y > parent.y + parent.size)
                    dy -= rate;
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Show()
        {
            switch (shape)
            {
                // Instanced squares
                case 0:
                    var rectInst = inst as myRectangleInst;

                    rectInst.setInstanceCoords(x - size, y - size, 2 * size, 2 * size);
                    rectInst.setInstanceColor(R, G, B, A);
                    rectInst.setInstanceAngle(angle);
                    break;

                // Instanced triangles
                case 1:
                    var triangleInst = inst as myTriangleInst;

                    triangleInst.setInstanceCoords(x, y, 2 * size, angle);
                    triangleInst.setInstanceColor(R, G, B, A);
                    break;

                // Instanced circles
                case 2:
                    var ellipseInst = inst as myEllipseInst;

                    ellipseInst.setInstanceCoords(x, y, 2 * size, angle);
                    ellipseInst.setInstanceColor(R, G, B, A);
                    break;

                // Instanced pentagons
                case 3:
                    var pentagonInst = inst as myPentagonInst;

                    pentagonInst.setInstanceCoords(x, y, 2 * size, angle);
                    pentagonInst.setInstanceColor(R, G, B, A);
                    break;

                // Instanced hexagons
                case 4:
                    var hexagonInst = inst as myHexagonInst;

                    hexagonInst.setInstanceCoords(x, y, 2 * size, angle);
                    hexagonInst.setInstanceColor(R, G, B, A);
                    break;
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Process(Window window)
        {
            uint cnt = 0;
            int objCnt = N;
            initShapes();

            // Disable VSYNC if needed
            // Glfw.SwapInterval(0);

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

                    for (int i = 0; i < list.Count; i++)
                    {
                        var obj = list[i] as myObj_350;

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

                if (objCnt != 0)
                {
                    list.Add(new myObj_350());
                    objCnt--;
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
            base.initShapes(shape, N * (nChildren + 1), 0);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------
    }
};
