using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;


/*
    - Rectangular shapes made of particles moving along the rectangle's outline
*/


namespace my
{
    public class myObj_380 : myObject
    {
        // Priority
        public static int Priority => 10;
		public static System.Type Type => typeof(myObj_380);

        private myObj_380 parent = null;

        private bool dir;
        private int Width, Height;
        private float x, y, dx, dy;
        private float size, A, R, G, B, angle = 0, dAngle = 0;

        private static int N = 0, n = 0, shape = 0, moveMode = 0, angleMode = 0, genMode = 0, sizeMode = 0, speedMode = 0,
                           startingSide = -1, maxSize = 3, opacityFactor = 0;
        private static bool doFillShapes = false;
        private static float dimAlpha = 0.05f, speedConst = 0;

        // ---------------------------------------------------------------------------------------------------------------

        // Default constructor; we need it because myObj_Prioritizer won't work with the parametrized one
        public myObj_380()
        {
        }

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_380(myObj_380 p = null)
        {
            parent = p;

            generateNew();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // Use this method to add new parent objects
        public void addNode()
        {
            myObj_380 node = new myObj_380(null);

            if (node != null)
            {
                list.Add(node);

                for (int i = 0; i < n; i++)
                {
                    list.Add(new myObj_380(node));
                }
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
                N = rand.Next(111) + 3;
                n = rand.Next(3333) + 111;

                shape = rand.Next(5);
            }

            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time local initialization
        private void initLocal()
        {
            doClearBuffer = myUtils.randomBool(rand);
            doFillShapes  = myUtils.randomBool(rand);

            genMode   = rand.Next(3);
            moveMode  = rand.Next(3);
            sizeMode  = rand.Next(2);
            angleMode = rand.Next(6);
            speedMode = rand.Next(3);

            maxSize = rand.Next(11) + 2;
            opacityFactor = rand.Next(301) + 100;

            speedConst = myUtils.randFloat(rand, 0.1f) * (rand.Next(11) + 1);

            if (myUtils.randomChance(rand, 1, 2))
            {
                startingSide = -1;
            }
            else
            {
                startingSide = rand.Next(4);
            }

            renderDelay = rand.Next(10) + 15;

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            height = 600;

            string nStr(int   n) { return n.ToString("N0");    }
            string fStr(float f) { return f.ToString("0.000"); }

            string str = $"Obj = myObj_380\n\n"                         +
                            $"N = {nStr(list.Count)} of ({N} x {n})\n"  +
                            $"doClearBuffer = {doClearBuffer}\n"        +
                            $"doFillShapes = {doFillShapes}\n"          +
                            $"shape = {shape}\n"                        +
                            $"maxSize = {maxSize}\n"                    +
                            $"sizeMode = {sizeMode}\n"                  +
                            $"genMode = {genMode}\n"                    +
                            $"moveMode = {moveMode}\n"                  +
                            $"angleMode = {angleMode}\n"                +
                            $"speedMode = {speedMode}\n"                +
                            $"speedConst = {fStr(speedConst)}\n"        +
                            $"opacityFactor = {opacityFactor}\n"        +
                            $"dimAlpha = {fStr(dimAlpha)}\n"            +
                            $"renderDelay = {renderDelay}\n"            +
                            $"file: {colorPicker.GetFileName()}"
                ;
            return str;
        }

        // ---------------------------------------------------------------------------------------------------------------

        // 
        protected override void setNextMode()
        {
            dimScreen(0.5f);

            initLocal();

            dimScreen(0.5f);
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void generateNew()
        {
            if (parent == null)
            {
                // Position parent shapes
                switch (genMode)
                {
                    case 0:
                        x = rand.Next(gl_x0);
                        y = rand.Next(gl_y0);

                        Width  = gl_Width  - (int)(2 * x);
                        Height = gl_Height - (int)(2 * y);
                        break;

                    case 1:
                        y = rand.Next(gl_y0);
                        x = y;

                        Width  = gl_Width  - (int)(2 * x);
                        Height = gl_Height - (int)(2 * y);
                        break;

                    case 2:
                        x = rand.Next(gl_Width  + 200) - 100;
                        y = rand.Next(gl_Height + 200) - 100;

                        Width  = rand.Next(666) + 111;
                        Height = rand.Next(666) + 111;
                        break;

                    // Non-intersecting rectangles -- does not work yet
                    case 3:
                        {
                            // Check whether a point lies within a rectangle
                            bool isPtInsideRect(int ptx, int pty, int x, int y, int w, int h) {
                                return (ptx >= x && pty >= y && ptx <= x + w && pty <= y + h);
                            }

                            bool isOk = false;

                            do {

                                bool ok = true;

                                // 1. Find a point that does not belong to any of the existing parent shapes
                                x = rand.Next(gl_Width  + 200) - 100;
                                y = rand.Next(gl_Height + 200) - 100;

                                for (int i = 0; i < list.Count; i++)
                                {
                                    var obj = list[i] as myObj_380;

                                    // Find other parent shapes
                                    if (obj.parent == null)
                                    {
                                        if (isPtInsideRect((int)x, (int)y, (int)obj.x, (int)obj.y, obj.Width, obj.Height))
                                        {
                                            ok = false;
                                            break;
                                        }
                                    }
                                }

                                // The point does not belong to any of the parent shapes
                                if (ok)
                                {
                                    // Try k times to find the rect that would not intersect any other parent rects
                                    for (int k = 0; k < 10; k++)
                                    {
                                        Width  = rand.Next(666) + 111;
                                        Height = rand.Next(666) + 111;

                                        for (int i = 0; i < list.Count; i++)
                                        {
                                            var obj = list[i] as myObj_380;

                                            if (obj.parent == null)
                                            {
                                                ok &= !isPtInsideRect((int)(x + Width), (int)y, (int)obj.x, (int)obj.y, obj.Width, obj.Height);
                                                ok &= !isPtInsideRect((int)x, (int)(y + Height), (int)obj.x, (int)obj.y, obj.Width, obj.Height);
                                                ok &= !isPtInsideRect((int)(x + Width), (int)(y + Height), (int)obj.x, (int)obj.y, obj.Width, obj.Height);

                                                if (ok == false)
                                                    break;
                                            }
                                        }

                                        if (ok == true)
                                        {
                                            isOk = true;
                                            break;
                                        }
                                    }
                                }
                            }
                            while (!isOk);
                        }
                        break;
                }

                dir = myUtils.randomBool(rand);
            }
            else
            {
                // Generate child particles
                float spd = 1;
                int side = -1;

                switch (moveMode)
                {
                    case 0:
                        side = (startingSide < 0) ? rand.Next(4) : startingSide;
                        break;

                    // In this mode, particles always stay on the same side;
                    // Thus, need to adjust their probability of appearing on the longer/shorter side
                    case 1:
                        side = myUtils.randomChance(rand, parent.Width, parent.Width + parent.Height)
                            ? rand.Next(2)
                            : rand.Next(2) + 2;
                        break;

                    case 2:
                        side = rand.Next(4);
                        break;
                }

                switch (speedMode)
                {
                    // Continuous values [0.1 ... 11.0]
                    case 0:
                        spd = myUtils.randFloat(rand, 0.1f) * (rand.Next(11) + 1);
                        break;

                    // Discreet values [0.5 ... 11.0]
                    case 1:
                        spd = 0.5f * (rand.Next(21) + 1);
                        break;

                    // Const value [0.1 ... 11.0]
                    case 2:
                        spd = speedConst;
                        break;
                }

                switch (side)
                {
                    // top
                    case 0:
                        dx = spd;
                        dy = 0;
                        x = parent.x + rand.Next(parent.Width);
                        y = parent.y;
                        break;

                    // bottom
                    case 1:
                        dx = -spd;
                        dy = 0;
                        x = parent.x + rand.Next(parent.Width);
                        y = parent.y + parent.Height;
                        break;

                    // right
                    case 2:
                        dx = 0;
                        dy = spd;
                        x = parent.x + parent.Width;
                        y = parent.y + rand.Next(parent.Height);
                        break;

                    // left
                    case 3:
                        dx = 0;
                        dy = -spd;
                        x = parent.x;
                        y = parent.y + rand.Next(parent.Height);
                        break;
                }

                if (parent.dir == false)
                {
                    dx *= -1;
                    dy *= -1;
                }

                A = opacityFactor * myUtils.randFloat(rand, 0.05f) / n;
                colorPicker.getColor(x, y, ref R, ref G, ref B);

                size = (sizeMode == 0) ? maxSize : rand.Next(maxSize) + 1;
            }

            switch (angleMode)
            {
                // No rotation
                case 0:
                    angle = 0;
                    dAngle = 0;
                    break;

                // No rotation, the angle is the angle static (comes from a parent node)
                case 1:
                    angle = (parent == null) ? myUtils.randFloat(rand) * rand.Next(123) : parent.angle;
                    dAngle = 0;
                    break;

                // No rotation, the angle is random
                case 2:
                    angle = myUtils.randFloat(rand) * rand.Next(123);
                    dAngle = 0;
                    break;

                // Rotating particles, dAngle comes from parent node
                case 3:
                    angle = 0;
                    dAngle = (parent == null) ? myUtils.randomSign(rand) * myUtils.randFloat(rand, 0.05f) * 0.1f : parent.dAngle;
                    break;

                // Rotating particles, dAngle comes from parent node (randomly positive OR negative)
                case 4:
                    angle = 0;
                    dAngle = (parent == null) ? myUtils.randomSign(rand) * myUtils.randFloat(rand, 0.05f) * 0.1f : myUtils.randomSign(rand) * parent.dAngle;
                    break;

                // Rotating particles, fully randomized
                case 5:
                    angle = myUtils.randFloat(rand);
                    dAngle = myUtils.randomSign(rand) * myUtils.randFloat(rand, 0.05f) * 0.1f;
                    break;
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            if (parent != null)
            {
                x += dx;
                y += dy;
                angle += dAngle;

                switch (moveMode)
                {
                    // Particles turn at the angles, not ever leaving the screen
                    case 0:
                        {
                            if (dx == 0)
                            {
                                if (y < parent.y)
                                {
                                    y = parent.y;
                                    dx = parent.dir ? -dy : +dy;
                                    dy = 0;
                                }

                                if (y > parent.y + parent.Height)
                                {
                                    y = parent.y + parent.Height;
                                    dx = parent.dir ? -dy: +dy;
                                    dy = 0;
                                }
                            }
                            else
                            {
                                if (x < parent.x)
                                {
                                    x = parent.x;
                                    dy = parent.dir ? +dx : -dx;
                                    dx = 0;
                                }

                                if (x > parent.x + parent.Width)
                                {
                                    x = parent.x + parent.Width;
                                    dy = parent.dir ? +dx : -dx;
                                    dx = 0;
                                }
                            }
                        }
                        break;

                    // Particles move from corner to corner and back
                    case 1:
                        {
                            if (dx == 0)
                            {
                                if (y < parent.y)
                                {
                                    y = parent.y;
                                    dy *= -1;
                                }

                                if (y > parent.y + parent.Height)
                                {
                                    y = parent.y + parent.Height;
                                    dy *= -1;
                                }
                            }
                            else
                            {
                                if (x < parent.x)
                                {
                                    x = parent.x;
                                    dx *= -1;
                                }

                                if (x > parent.x + parent.Width)
                                {
                                    x = parent.x + parent.Width;
                                    dx *= -1;
                                }
                            }
                        }
                        break;

                    // Particles move in straight line until they leave the screen
                    case 2:
                        {
                            if (x < parent.x || y < parent.y || x > parent.x + parent.Width || y > parent.y + parent.Height)
                            {
                                A -= 0.001f;
                            }

                            if (x > gl_Width || x < 0 || y > gl_Height || y < 0)
                            {
                                generateNew();
                            }
                        }
                        break;
                }
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Show()
        {
            if (parent != null)
            {
                float size2x = size * 2;

                switch (shape)
                {
                    // Instanced squares
                    case 0:
                        var rectInst = inst as myRectangleInst;

                        rectInst.setInstanceCoords(x - size, y - size, size2x, size2x);
                        rectInst.setInstanceColor(R, G, B, A);
                        rectInst.setInstanceAngle(angle);
                        break;

                    // Instanced triangles
                    case 1:
                        var triangleInst = inst as myTriangleInst;

                        triangleInst.setInstanceCoords(x, y, size2x, angle);
                        triangleInst.setInstanceColor(R, G, B, A);
                        break;

                    // Instanced circles
                    case 2:
                        var ellipseInst = inst as myEllipseInst;

                        ellipseInst.setInstanceCoords(x, y, size2x, angle);
                        ellipseInst.setInstanceColor(R, G, B, A);
                        break;

                    // Instanced pentagons
                    case 3:
                        var pentagonInst = inst as myPentagonInst;

                        pentagonInst.setInstanceCoords(x, y, size2x, angle);
                        pentagonInst.setInstanceColor(R, G, B, A);
                        break;

                    // Instanced hexagons
                    case 4:
                        var hexagonInst = inst as myHexagonInst;

                        hexagonInst.setInstanceCoords(x, y, size2x, angle);
                        hexagonInst.setInstanceColor(R, G, B, A);
                        break;
                }
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

            dimScreenRGB_SetRandom(0.1f);

            if (doClearBuffer)
            {
                glDrawBuffer(GL_FRONT_AND_BACK | GL_DEPTH_BUFFER_BIT);
                glClearColor(myObject.bgrR, myObject.bgrG, myObject.bgrB, 1);
            }
            else
            {
                glDrawBuffer(GL_FRONT_AND_BACK);
                //glDrawBuffer(GL_DEPTH_BUFFER_BIT);
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

                    for (int i = 0; i != list.Count; i++)
                    {
                        var obj = list[i] as myObj_380;

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

                if (list.Count < (N * n + N))
                {
                    addNode();
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
            base.initShapes(shape, N * n + N, 0);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------
    }
};
