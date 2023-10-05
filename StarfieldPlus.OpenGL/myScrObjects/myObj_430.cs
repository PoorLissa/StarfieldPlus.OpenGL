using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;


/*
    - Shooters move across the screen, shooting at each other
*/


namespace my
{
    public class myObj_430 : myObject
    {
        // Priority
        public static int Priority => 10;
		public static System.Type Type => typeof(myObj_430);

        private myObj_430 owner = null;

        private int bulletSpeed, closestTarget, trailLengthFactor;
        private bool alive;
        private float x, y, X, Y, dx, dy;
        private float size, A, R, G, B, angle, dAngle, r, g, b;
        private float randomSpeedFactor;

        private static int N = 0, n = 0, shape = 0, trailMode = 0, specialMode = 0;
        private static int shooterSpdFactor = 1;
        private static int bulletSize = 0, shooterSize = 0;
        private static int bulletMinX = 0, bulletMaxX = 0, bulletMinY = 0, bulletMaxY = 0;
        private static bool doFillShapes = false, doUseRandomSpeed = true, doUseBulletSpread = true;
        private static float dimAlpha = 0.05f;

        private static myScreenGradient grad = null;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_430()
        {
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
                shape = rand.Next(5);

                specialMode = myUtils.randomChance(rand, 1, 5) ? 1 : 0;

                switch (specialMode)
                {
                    // Default mode
                    case 0:
                        n = rand.Next(5) + 3;
                        break;

                    // Special mode 1
                    case 1:
                        n = rand.Next(2) + 2;
                        break;
                }

                N = 5000;
                N += n;

                bulletMinX = -500;
                bulletMinY = -500;
                bulletMaxX = gl_Width  + 500;
                bulletMaxY = gl_Height + 500;
            }

            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time local initialization
        private void initLocal()
        {
            doClearBuffer = myUtils.randomChance(rand, 4, 5);
            doUseRandomSpeed  = myUtils.randomChance(rand, 1, 2);
            doUseBulletSpread = myUtils.randomChance(rand, 1, 2);

            trailMode = rand.Next(6);

            bulletSize = rand.Next(2) + 1;
            shooterSize = rand.Next(5) + 7;

            shooterSpdFactor = myUtils.randomChance(rand, 1, 2)
                ? rand.Next(20) + 1
                : rand.Next(10) + 1;

            renderDelay = rand.Next(11) + 5;

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            height = 600;

            string nStr(int   n) { return n.ToString("N0");    }
            string fStr(float f) { return f.ToString("0.000"); }

            string str = $"Obj = {Type}\n\n"                                     	+
                            $"N = {nStr(list.Count - n)} of {nStr(N - n)} + {n}\n"  +
                            $"shape = {shape}\n"                                    +
                            $"specialMode = {specialMode}\n"                        +
                            $"trailMode = {trailMode}\n"                            +
                            $"doClearBuffer = {doClearBuffer}\n"                    +
                            $"doUseRandomSpeed = {doUseRandomSpeed}\n"              +
                            $"doUseBulletSpread = {doUseBulletSpread}\n"            +
                            $"dimAlpha = {fStr(dimAlpha)}\n"                        +
                            $"renderDelay = {renderDelay}\n"                        +
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
                bool isShooter = id < n;
                angle = 0;

                if (isShooter)
                {
                    alive = true;
                    closestTarget = -1;
                    bulletSpeed = myUtils.randomChance(rand, 1, 2)
                        ? rand.Next(066) + 11
                        : rand.Next(122) + 11;

                    trailLengthFactor = rand.Next(11) + 3;
                    randomSpeedFactor = myUtils.randFloat(rand, 0.1f);

                    x = rand.Next(gl_Width);
                    y = rand.Next(gl_Height);

                    dx = myUtils.randomSign(rand) * myUtils.randFloat(rand) * shooterSpdFactor;
                    dy = myUtils.randomSign(rand) * myUtils.randFloat(rand) * shooterSpdFactor;

                    size = shooterSize;

                    dAngle = myUtils.randomSign(rand) * myUtils.randFloat(rand, 0.05f) * 0.01f;

                    colorPicker.getColor(x, y, ref R, ref G, ref B);
                    A = 1;

                    do {
                        r = myUtils.randFloat(rand);
                        g = myUtils.randFloat(rand);
                        b = myUtils.randFloat(rand);
                    }
                    while (r + g + b < 0.75f);
                }
                else
                {
                    alive = true;
                    size = bulletSize;

                    // Get random shooter and its target;
                    // Shoot the target
                    {
                        owner = list[rand.Next(n)] as myObj_430;
                        var target  = list[owner.closestTarget] as myObj_430;

                        x = owner.x;
                        y = owner.y;

                        X = target.x;
                        Y = target.y;

                        trailLengthFactor = owner.trailLengthFactor;

                        // Add some bullet spread
                        if (doUseBulletSpread)
                        {
                            X += rand.Next(5) - 2;
                            Y += rand.Next(5) - 2;
                        }

                        float speed = owner.bulletSpeed;
                        float dist = (float)Math.Sqrt((x - X) * (x - X) + (y - Y) * (y - Y)) + 0.0001f;

                        if (doUseRandomSpeed)
                        {
                            speed += myUtils.randomSign(rand) * myUtils.randFloat(rand) * owner.randomSpeedFactor;
                        }

                        switch (specialMode)
                        {
                            case 0:
                                break;

                            case 1:
                                speed = 1.5f;
                                break;
                        }

                        dx = speed * (X - x) / dist;
                        dy = speed * (Y - y) / dist;

                        // Remember initial coordinates
                        switch (trailMode)
                        {
                            case 1:
                                X = x;
                                Y = y;
                                break;

                            case 5:
                                X = target.x;
                                Y = target.y;
                                break;
                        }

                        A = 0.750f + myUtils.randFloat(rand) * 0.2f;
                        //R = owner.R + myUtils.randomSign(rand) * myUtils.randFloat(rand) * 0.1f;
                        //G = owner.G + myUtils.randomSign(rand) * myUtils.randFloat(rand) * 0.1f;
                        //B = owner.B + myUtils.randomSign(rand) * myUtils.randFloat(rand) * 0.1f;

                        R = owner.r + myUtils.randomSign(rand) * myUtils.randFloat(rand) * 0.15f;
                        G = owner.g + myUtils.randomSign(rand) * myUtils.randFloat(rand) * 0.15f;
                        B = owner.b + myUtils.randomSign(rand) * myUtils.randFloat(rand) * 0.15f;
                    }
                }
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

                angle += dAngle;

                // Find the closest target and remember it
                if (closestTarget < 0 || myUtils.randomChance(rand, 1, 100))
                {
                    getClosest((int)id, ref closestTarget);
                }

                if (x < 0)
                    dx += 0.5f;

                if (y < 0)
                    dy += 0.5f;

                if (x > gl_Width)
                    dx -= 0.5f;

                if (y > gl_Height)
                    dy -= 0.5f;
            }
            else
            {
                if (trailMode == 3)
                {
                    X = x;
                    Y = y;
                }

                x += dx;
                y += dy;

                if (alive)
                {
                    if (x < bulletMinX || y < bulletMinY || x > bulletMaxX|| y > bulletMaxY)
                    {
                        generateNew();
                    }
                }
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Show()
        {
            float size2x = size * 2;
            bool isBullet = id >= n;

            if (isBullet)
            {
                switch (trailMode)
                {
                    // No trail
                    case 0:
                        break;

                    // Current position to origin position
                    case 1:
                        myPrimitive._LineInst.setInstanceCoords(x, y, X, Y);
                        myPrimitive._LineInst.setInstanceColor(1, 1, 1, 0.01f);
                        break;

                    // Current position to owner's position
                    case 2:
                        myPrimitive._LineInst.setInstanceCoords(x, y, owner.x, owner.y);
                        myPrimitive._LineInst.setInstanceColor(1, 1, 1, 0.01f);
                        break;

                    // Current position to old position
                    case 3:
                        myPrimitive._LineInst.setInstanceCoords(x, y, X, Y);
                        myPrimitive._LineInst.setInstanceColor(1, 1, 1, 0.05f);
                        break;

                    // Current position to calculated pseudo-old position
                    case 4:
                        myPrimitive._LineInst.setInstanceCoords(x, y, x - dx * trailLengthFactor, y - dy * trailLengthFactor);
                        myPrimitive._LineInst.setInstanceColor(1, 1, 1, 0.05f);
                        break;

                    // Current position to target original position
                    case 5:
                        myPrimitive._LineInst.setInstanceCoords(x, y, X, Y);
                        myPrimitive._LineInst.setInstanceColor(1, 1, 1, 0.05f);
                        break;
                }
            }

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

            // Add all the shooters
            for (int i = 0; i < n; i++)
            {
                list.Add(new myObj_430());
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

                    for (int i = 0; i != list.Count; i++)
                    {
                        var obj = list[i] as myObj_430;

                        obj.Move();
                        obj.Show();
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
                    list.Add(new myObj_430());
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
            myPrimitive.init_LineInst(N);

            base.initShapes(shape, N, 0);

            grad = new myScreenGradient();
            float factor = myUtils.randFloat(rand) * 0.2f;
            grad.SetRandomColors(rand, factor, mode: 0);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void getClosest(int id, ref int resId)
        {
            float min = float.MaxValue;

            var This = list[id] as myObj_430;

            for (int i = 0; i != n; i++)
            {
                if (i != id)
                {
                    var other = list[i] as myObj_430;
                    float distSquared = (This.x - other.x) * (This.x - other.x) + (This.y - other.y) * (This.y - other.y);

                    if (distSquared < min)
                    {
                        resId = i;
                        min = distSquared;
                    }
                }
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

    }
};
