using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;


/*
    - ...
*/


namespace my
{
    public class myObj_360 : myObject
    {
        private int[] others = null;

        private int nOthers;
        private float x, y, dx, dy;
        private float size, A, R, G, B, angle = 0;

        private static int N = 0, shape = 0;
        private static bool doFillShapes = false;
        private static float dimAlpha = 0.05f, changeFactor = 1;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_360()
        {
            others = new int[5];

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
                N = 1234;
                shape = rand.Next(5);
            }

            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time local initialization
        private void initLocal()
        {
            doClearBuffer = myUtils.randomBool(rand);
            dimAlpha = 0.01f + myUtils.randFloat(rand) * 0.1f;

            changeFactor = myUtils.randFloat(rand) * rand.Next(11);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            height = 800;

            string str = $"Obj = myObj_360\n\n"                         +
                            $"N = {list.Count} of {N}\n"                +
                            $"shape = {shape}\n"                        +
                            $"changeFactor = {changeFactor}\n"          +
                            $"dimAlpha = {dimAlpha.ToString("0.000")}\n"+
                            $"file: {colorPicker.GetFileName()}"        +
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
            if (myUtils.randomChance(rand, 1, 3))
            {
                nOthers = rand.Next(3) + 1;

                for (int i = 0; i < nOthers; i++)
                    others[i] = rand.Next(list.Count);
            }
            else
            {
                nOthers = 0;
            }

            x = rand.Next(gl_Width);
            y = rand.Next(gl_Height);

            size = 3;
            dx = myUtils.randomSign(rand) * myUtils.randFloat(rand, 0.5f) * (rand.Next(5) + 3);
            dy = myUtils.randomSign(rand) * myUtils.randFloat(rand, 0.5f) * (rand.Next(5) + 3);

            A = (float)rand.NextDouble();
            colorPicker.getColor(x, y, ref R, ref G, ref B);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            x += dx;
            y += dy;

            if (myUtils.randomChance(rand, 1, 11))
            {
                if (myUtils.randomChance(rand, 1, 2))
                {
                    dx += myUtils.randomSign(rand) * myUtils.randFloat(rand) * changeFactor;
                }

                if (myUtils.randomChance(rand, 1, 2))
                {
                    dy += myUtils.randomSign(rand) * myUtils.randFloat(rand) * changeFactor;
                }
            }

            if (x < 0 || y < 0 || x > gl_Width || y > gl_Height)
            {
                generateNew();
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Show()
        {
            for (int i = 0; i < nOthers; i++)
            {
                var other = list[others[i]] as myObj_360;

                myPrimitive._LineInst.setInstanceCoords(x, y, other.x, other.y);
                myPrimitive._LineInst.setInstanceColor(R, G, B, 0.175f);
            }

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
                    myPrimitive._LineInst.ResetBuffer();

                    for (int i = 0; i < list.Count; i++)
                    {
                        var obj = list[i] as myObj_360;

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

                if (list.Count < N)
                {
                    list.Add(new myObj_360());
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
            myPrimitive.init_LineInst(N * 5);
            base.initShapes(shape, N, 0);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------
    }
};
