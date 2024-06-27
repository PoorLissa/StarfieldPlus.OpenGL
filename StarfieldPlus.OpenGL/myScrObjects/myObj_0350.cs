using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;


/*
    - Moving groups of small particles.
    - Particles within the group are connected to each other.
*/


namespace my
{
    public class myObj_0350 : myObject
    {
        // Priority
        public static int Priority => 10;
		public static System.Type Type => typeof(myObj_0350);

        private List<myObj_0350> children = null;
        private myObj_0350 parent = null;

        private int level;
        private float x, y, dx, dy, rad1, rad2;
        private float size, A, R, G, B, angle = 0, dAngle, gravityRate;

        private myParticleTrail trail = null;

        private static int N = 0, shape = 0, levelBase = 0, maxChildren = 0, childMoveMode = 0, nTrail = 0;
        private static bool doFillShapes = false, doUseOpacityAsSpeed = true, doConnectToCenter = true, doUseTrails = false;
        private static float dimAlpha = 0.05f;

        private static myScreenGradient grad = null;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_0350()
        {
            level = levelBase;
            generateNew();

            if (level == 0)
            {
                children = new List<myObj_0350>();

                levelBase++;

                int n = rand.Next(maxChildren) + 3;

                for (int i = 0; i < n; i++)
                {
                    var obj = new myObj_0350();

                    obj.parent = this;

                    obj.x = x;
                    obj.y = y;

                    obj.size = 3;
                    obj.gravityRate = gravityRate;
                    obj.dAngle = myUtils.randFloatSigned(rand) * 0.025f;

                    obj.R = R;
                    obj.G = G;
                    obj.B = B;
                    obj.A = A;

                    obj.dx = myUtils.randomSign(rand) * myUtils.randFloat(rand, 0.1f) * (rand.Next(3) + 1);
                    obj.dy = myUtils.randomSign(rand) * myUtils.randFloat(rand, 0.1f) * (rand.Next(3) + 1);

                    // Rotating mode
                    if (childMoveMode == 4 || childMoveMode == 5)
                    {
                        obj.rad1 = 3 + rand.Next((int)obj.parent.size);
                        obj.rad2 = 3 + rand.Next((int)obj.parent.size);

                        if (childMoveMode == 4)
                            obj.rad2 = obj.rad1;

                        obj.dx = myUtils.randFloatSigned(rand) * rand.Next(123);
                        obj.dy = myUtils.randFloatSigned(rand) * 0.1f;
                        obj.dy = (0.1f + myUtils.randFloat(rand) * 0.05f) * myUtils.randomSign(rand);
                    }

                    // Initialize Trail
                    {
                        if (doUseTrails && obj.trail == null)
                        {
                            obj.trail = new myParticleTrail(nTrail, obj.x, obj.y);
                        }

                        if (obj.trail != null)
                        {
                            obj.trail.updateDa(obj.A * 2);
                        }
                    }

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

                doUseTrails = true;

                switch (rand.Next(7))
                {
                    case 0: nTrail = 10 + rand.Next(  5); break;
                    case 1: nTrail = 10 + rand.Next(  9); break;
                    case 2: nTrail = 10 + rand.Next( 13); break;
                    case 3: nTrail = 33 + rand.Next( 33); break;
                    case 4: nTrail = 33 + rand.Next( 99); break;
                    case 5: nTrail = 33 + rand.Next(199); break;
                    case 6: nTrail = 33 + rand.Next(299); break;
                }
            }

            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time local initialization
        private void initLocal()
        {
            doClearBuffer = myUtils.randomChance(rand, 10, 11);
            doUseOpacityAsSpeed = myUtils.randomBool(rand);

            dimAlpha = 0.1f + myUtils.randFloat(rand) * 0.5f;

            childMoveMode = rand.Next(6);

            doUseOpacityAsSpeed = true;
            doConnectToCenter = (childMoveMode > 3) && myUtils.randomChance(rand, 1, 2);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            height = 800;

            string str = $"Obj = {Type}\n\n"                         	 +
                            myUtils.strCountOf(list.Count, N)            +
                            $"shape = {shape}\n"                         +
                            $"maxChildren = {maxChildren}\n"             +
                            $"childMoveMode = {childMoveMode}\n"         +
                            $"doConnectToCenter = {doConnectToCenter}\n" +
                            $"doUseTrails = {doUseTrails}\n"             +
                            $"nTrail = {nTrail}\n"                       +
                            $"dimAlpha = {dimAlpha.ToString("0.000")}\n" +
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

                foreach (myObj_0350 child in children)
                {
                    child.Move();
                }
            }
            else
            {
                angle += dAngle;

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

                    // Rotate around parent's center
                    case 4:
                    case 5:
                        {
                            x = parent.x + rad1 * (float)Math.Sin(dx);
                            y = parent.y + rad2 * (float)Math.Cos(dx);
                            dx += dy;
                        }
                        break;
                }

                // Update trail info
                if (doUseTrails)
                {
                    trail.update(x, y);
                }
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Show()
        {
            if (parent == null)
            {
                // Draw connecting lines between all the children
                if (doConnectToCenter)
                {
                    for (int i = 0; i < children.Count; i++)
                    {
                        var obj = children[i] as myObj_0350;

                        myPrimitive._LineInst.setInstanceCoords(obj.x, obj.y, x, y);
                        myPrimitive._LineInst.setInstanceColor(R, G, B, 0.175f);

                        obj.Show();
                    }
                }
                else
                {
                    for (int i = 0; i < children.Count; i++)
                    {
                        var obj = children[i] as myObj_0350;

                        for (int j = i + 1; j < children.Count; j++)
                        {
                            var other = children[j] as myObj_0350;

                            myPrimitive._LineInst.setInstanceCoords(obj.x, obj.y, other.x, other.y);
                            myPrimitive._LineInst.setInstanceColor(R, G, B, 0.175f);
                        }

                        obj.Show();
                    }
                }

                return;
            }

            // Draw the trail
            if (doUseTrails)
            {
                trail.Show(R, G, B, A);
            }

            switch (shape)
            {
                // Instanced squares
                case 0:
                    myPrimitive._RectangleInst.setInstanceCoords(x - size, y - size, 2 * size, 2 * size);
                    myPrimitive._RectangleInst.setInstanceColor(R, G, B, A);
                    myPrimitive._RectangleInst.setInstanceAngle(angle);
                    break;

                // Instanced triangles
                case 1:
                    myPrimitive._TriangleInst.setInstanceCoords(x, y, 2 * size, angle);
                    myPrimitive._TriangleInst.setInstanceColor(R, G, B, A);
                    break;

                // Instanced circles
                case 2:
                    myPrimitive._EllipseInst.setInstanceCoords(x, y, 2 * size, angle);
                    myPrimitive._EllipseInst.setInstanceColor(R, G, B, A);
                    break;

                // Instanced pentagons
                case 3:
                    myPrimitive._PentagonInst.setInstanceCoords(x, y, 2 * size, angle);
                    myPrimitive._PentagonInst.setInstanceColor(R, G, B, A);
                    break;

                // Instanced hexagons
                case 4:
                    myPrimitive._HexagonInst.setInstanceCoords(x, y, 2 * size, angle);
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
                    myPrimitive._LineInst.ResetBuffer();

                    for (int i = 0; i < Count; i++)
                    {
                        var obj = list[i] as myObj_0350;

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
                    list.Add(new myObj_0350());
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

            int lineInstN = N * nChildren * (nChildren - 1);
            lineInstN += doUseTrails ? (N * nChildren * nTrail) : 0;

            myPrimitive.init_ScrDimmer();
            myPrimitive.init_LineInst(lineInstN);

            base.initShapes(shape, N * (nChildren + 1), 0);

            grad = new myScreenGradient();
            grad.SetRandomColors(rand, 0.2f);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------
    }
};
