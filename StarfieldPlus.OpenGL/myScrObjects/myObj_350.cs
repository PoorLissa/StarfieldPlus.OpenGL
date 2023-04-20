using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;


/*
    - Moving groups of small particles. Particles within the group are connected to each other.
*/


namespace my
{
    public class myObj_350 : myObject
    {
        // Priority
        public static int Priority => 10;

        private List<myObj_350> children = null;
        private myObj_350 parent = null;

        private int level;
        private float x, y, dx, dy;
        private float size, A, R, G, B, angle = 0, gravityRate;

        private static int N = 0, shape = 0, levelBase = 0, maxChildren = 0, childMoveMode = 0;
        private static bool doFillShapes = false, doUseOpacityAsSpeed = true;
        private static float dimAlpha = 0.05f;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_350()
        {
            level = levelBase;
            generateNew();

            if (level == 0)
            {
                children = new List<myObj_350>();

                levelBase++;

                int n = rand.Next(maxChildren) + 3;

                for (int i = 0; i < n; i++)
                {
                    var obj = new myObj_350();

                    obj.parent = this;

                    obj.x = x;
                    obj.y = y;

                    obj.size = 3;
                    obj.gravityRate = gravityRate;

                    obj.R = R;
                    obj.G = G;
                    obj.B = B;
                    obj.A = A;

                    obj.dx = myUtils.randomSign(rand) * myUtils.randFloat(rand, 0.1f) * (rand.Next(3) + 1);
                    obj.dy = myUtils.randomSign(rand) * myUtils.randFloat(rand, 0.1f) * (rand.Next(3) + 1);

                    children.Add(obj);
                }

                levelBase--;
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
                N = rand.Next(100) + 13;
                maxChildren = rand.Next(13);
                shape = rand.Next(5);
            }

            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time local initialization
        private void initLocal()
        {
            doClearBuffer = myUtils.randomBool(rand);
            doUseOpacityAsSpeed = myUtils.randomBool(rand);

            dimAlpha = 0.1f + myUtils.randFloat(rand) * 0.5f;

            childMoveMode = rand.Next(4);

            doUseOpacityAsSpeed = true;

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            height = 800;

            string str = $"Obj = myObj_350\n\n"                         +
                            $"N = {list.Count} of {N}\n"                +
                            $"shape = {shape}\n"                        +
                            $"maxChildren = {maxChildren}\n"            +
                            $"childMoveMode = {childMoveMode}\n"        +
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
            if (id != uint.MaxValue)
            {
                if (level == 0)
                {
                    x = rand.Next(gl_Width);
                    y = rand.Next(gl_Height);

                    size = 20 + rand.Next(50);
                    dx = myUtils.randomSign(rand) * myUtils.randFloat(rand, 0.5f) * (rand.Next(5) + 3);
                    dy = myUtils.randomSign(rand) * myUtils.randFloat(rand, 0.5f) * (rand.Next(5) + 3);

                    A = (float)rand.NextDouble();
                    colorPicker.getColor(x, y, ref R, ref G, ref B);

                    gravityRate = myUtils.randFloat(rand, 0.1f) * (rand.Next(2) + 1);

                    if (doUseOpacityAsSpeed)
                    {
                        dx *= A;
                        dy *= A;

                        //gravityRate /= A;
                    }
                }
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

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

                foreach (myObj_350 child in children)
                {
                    child.Move();
                }
            }
            else
            {
                switch (childMoveMode)
                {
                    case 0:
                        {
                            if (x < parent.x - parent.size && dx < 0)
                                dx *= -1;

                            if (y < parent.y - parent.size && dy < 0)
                                dy *= -1;

                            if (x > parent.x + parent.size && dx > 0)
                                dx *= -1;

                            if (y > parent.y + parent.size && dy > 0)
                                dy *= -1;
                        }
                        break;

                    case 1:
                        {
                            x += parent.dx;
                            y += parent.dy;

                            if (x < parent.x - parent.size && dx < 0)
                                dx *= -1;

                            if (y < parent.y - parent.size && dy < 0)
                                dy *= -1;

                            if (x > parent.x + parent.size && dx > 0)
                                dx *= -1;

                            if (y > parent.y + parent.size && dy > 0)
                                dy *= -1;
                        }
                        break;

                    case 2:
                        {
                            if (x < parent.x - parent.size)
                                dx += gravityRate;

                            if (y < parent.y - parent.size)
                                dy += gravityRate;

                            if (x > parent.x + parent.size)
                                dx -= gravityRate;

                            if (y > parent.y + parent.size)
                                dy -= gravityRate;
                        }
                        break;

                    case 3:
                        {
                            x += parent.dx;
                            y += parent.dy;

                            if (x < parent.x - parent.size)
                                dx += gravityRate;

                            if (y < parent.y - parent.size)
                                dy += gravityRate;

                            if (x > parent.x + parent.size)
                                dx -= gravityRate;

                            if (y > parent.y + parent.size)
                                dy -= gravityRate;
                        }
                        break;
                }
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Show()
        {
            // Show all children
            if (parent == null)
            {
                for (int i = 0; i < children.Count; i++)
                {
                    var obj = children[i] as myObj_350;

                    for (int j = i+1; j < children.Count; j++)
                    {
                        var other = children[j] as myObj_350;

                        myPrimitive._LineInst.setInstanceCoords(obj.x, obj.y, other.x, other.y);
                        myPrimitive._LineInst.setInstanceColor(R, G, B, 0.175f);
                    }

                    obj.Show();
                }

                return;
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
                        var obj = list[i] as myObj_350;

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
                    list.Add(new myObj_350());
                }

                cnt++;
                System.Threading.Thread.Sleep(renderDelay);
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void initShapes()
        {
            int nChildren = maxChildren + 3;

            myPrimitive.init_ScrDimmer();
            myPrimitive.init_LineInst(N * nChildren * (nChildren - 1));
            base.initShapes(shape, N * (nChildren + 1), 0);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------
    }
};
