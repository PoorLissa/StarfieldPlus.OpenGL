using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;


/*
    - Simplified gravity -- lots of light objects vs a few massive objects
    - Light objects do not interact with each other, only with massive ones
*/


namespace my
{
    public class myObj_1070 : myObject
    {
        // Priority
        public static int Priority => 9999910;
		public static System.Type Type => typeof(myObj_1070);

        private int cnt, lifeCnt;

        private float x, y, dx, dy, mass;
        private float size, A, R, G, B, angle = 0, dAngle = 0;

        private static int N = 0, n = 2, shape = 0, trailLength = 50, largeMassFactor = 1, rndMassMode = 0, rndMassN = 0, colorMode = 0, cntMax = 1500, genRate = 1, nOrigin = 1;
        private static bool doFillShapes = false, doUseInitSpd = false, doChangeLocation = false, doMoveLrgBodies = false, doUseLrgGravity = false, doUseShortLife = false;
        private static float dimAlpha = 0.05f, r1, r2, g1, g2, b1, b2, trailOpacity = 0.1f;

        private static float[] origin = null;

        private myParticleTrail trail = null;
        private static myScreenGradient grad = null;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_1070()
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
                N = rand.Next(1000) + 10000;

                switch (rand.Next(20))
                {
                    case 0:
                        n = 11 + rand.Next(66);
                        break;

                    case 1:
                    case 2:
                    case 3:
                        n = 7 + rand.Next(33);
                        break;

                    default:
                        n = 2 + rand.Next(7);
                        break;
                }

                shape = rand.Next(5);

                if (myUtils.randomChance(rand, 1, 3))
                {
                    trailLength = 3 + rand.Next(trailLength * 2);
                }

                if (myUtils.randomChance(rand, 1, 3))
                {
                    genRate = rand.Next(31) + 1;
                }

                switch (rand.Next(10))
                {
                    case 0:
                        nOrigin = 3;
                        break;

                    case 1:
                    case 2:
                    case 3:
                        nOrigin = 2;
                        break;

                    default:
                        nOrigin = 1;
                        break;
                }

                origin = new float[nOrigin * 2];
            }

            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time local initialization
        private void initLocal()
        {
            doClearBuffer = myUtils.randomChance(rand, 10, 11);
            doUseInitSpd = myUtils.randomBool(rand);
            doChangeLocation = myUtils.randomChance(rand, 1, 5);
            doMoveLrgBodies = myUtils.randomChance(rand, 1, 5);
            doUseLrgGravity = myUtils.randomChance(rand, 1, 5);
            doUseShortLife = myUtils.randomChance(rand, 1, 2);

            r1 = myUtils.randFloat(rand);
            g1 = myUtils.randFloat(rand);
            b1 = myUtils.randFloat(rand);

            colorMode = rand.Next(3);
            rndMassMode = rand.Next(6);
            rndMassN = 3 + rand.Next(8);

            largeMassFactor = 1 + rand.Next(11);

            trailOpacity = 0.33f;

            renderDelay = rand.Next(3) + 3;

            for (int i = 0; i < nOrigin*2; i+=2)
            {
                float x0 = rand.Next(gl_Width);
                float y0 = rand.Next(gl_Height);

                origin[i+0] = x0;
                origin[i+1] = y0;
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            height = 600;

            string str = $"Obj = {Type}\n\n"                            +
                            myUtils.strCountOf(list.Count, N)           +
                            $"n = {n}\n"                                +
                            $"nOrigin = {nOrigin}\n"                    +
                            $"doClearBuffer = {doClearBuffer}\n"        +
                            $"doUseInitSpd = {doUseInitSpd}\n"          +
                            $"doChangeLocation = {doChangeLocation}\n"  +
                            $"doMoveLrgBodies = {doMoveLrgBodies}\n"    +
                            $"doUseLrgGravity = {doUseLrgGravity}\n"    +
                            $"doUseShortLife = {doUseShortLife}\n"      +
                            $"colorMode = {colorMode}\n"                +
                            $"rndMassMode = {rndMassMode}\n"            +
                            $"largeMassFactor = {largeMassFactor}\n"    +
                            $"trailLength = {trailLength}\n"            +
                            $"genRate = {genRate}\n"                    +
                            $"renderDelay = {renderDelay}\n"            +
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

                dAngle = myUtils.randFloatSigned(rand) * 0.1f;
                dx = dy = 0;

                if (doMoveLrgBodies)
                {
                    dx = myUtils.randFloatSigned(rand) * 0.1f;
                    dy = myUtils.randFloatSigned(rand) * 0.1f;
                }

                size = 3;
                A = 0.5f;

                R = G = B = 0.75f;

                mass = (99999999.0f + rand.Next(99999999)) * largeMassFactor;

                cnt = cntMax + rand.Next(cntMax);
            }
            else
            {
                cnt = 100 + rand.Next(100);
                lifeCnt = 666 + rand.Next(666);

                x = rand.Next(gl_Width);
                y = rand.Next(gl_Width);

                switch (nOrigin)
                {
                    case 1:
                        x = origin[0];
                        y = origin[1];
                        break;

                    case 2:
                    case 3:
                        {
                            int i = rand.Next(nOrigin);
                            x = origin[i+0];
                            y = origin[i+1];
                        }
                        break;
                }

                // Initial speed of small particles
                if (doUseInitSpd)
                {
                    dx = myUtils.randFloatSigned(rand) * 0.1f;
                    dy = myUtils.randFloatSigned(rand) * 0.1f;
                }

                size = 2;
                A = 0.5f;

                getColor();

                switch (rndMassMode)
                {
                    case 0:
                        mass = 1.0f;
                        break;

                    case 1:
                        mass = 1.0f + myUtils.randFloat(rand) * 0.1f;
                        break;

                    case 2:
                        mass = 1.0f + rand.Next(7);
                        break;

                    case 3:
                        mass = 1.0f + rand.Next(7) * myUtils.randFloat(rand) * 2;
                        break;

                    case 4:
                        mass = 1.0f + rand.Next(7) + myUtils.randFloat(rand);
                        break;

                    case 5:
                        mass = 1.0f + 0.25f * rand.Next(rndMassN);
                        break;
                }

                // Trails
                if (true)
                {
                    // Initialize Trail
                    if (trail == null)
                    {
                        trail = new myParticleTrail(trailLength, x, y);
                    }
                    else
                    {
                        trail.reset(x, y);
                    }

                    if (trail != null)
                    {
                        trail.updateDa(A);
                    }
                }
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            if (id >= n)
            {
                double DX = 0, DY = 0, dist = 0, F = 0, factor = 0, d2 = 0;

                factor = 0.0000001f;
                factor *= 0.1f;

                for (int i = 0; i < n; i++)
                {
                    var bigObj = list[i] as myObj_1070;

                    DX = bigObj.x - x;
                    DY = bigObj.y - y;

                    d2 = DX * DX + DY * DY;

                    if (d2 > 0)
                    {
                        dist = Math.Sqrt(d2);

                        F = factor * mass * bigObj.mass / d2;

                        dx += (float)(F * DX);
                        dy += (float)(F * DY);
                    }
                }

                if (--cnt == 0)
                {
                    cnt = 100 + rand.Next(100);

                    A += myUtils.randFloatSigned(rand) * 0.2f;

                    if (A < 0.1f)
                        A = 0.1f;

                    if (A > 1.0f)
                        A = 1.0f;
                }

                if (--lifeCnt == 0)
                {
                    generateNew();
                }
            }
            else
            {
                // Large mass bodies

                if (doUseLrgGravity)
                {
                    float factor = 0.000000000001f;
                    factor *= 0.0000001f;

                    for (int i = 0; i < n; i++)
                    {
                        if (id != i)
                        {
                            var other = list[i] as myObj_1070;

                            float DX = other.x - x;
                            float DY = other.y - y;

                            float d2 = DX * DX + DY * DY;

                            if (d2 > 0)
                            {
                                float dist = (float)Math.Sqrt(d2);

                                float F = factor * mass * other.mass / d2;

                                dx += F * DX;
                                dy += F * DY;
                            }
                        }
                    }
                }

                if (doChangeLocation && --cnt == 0)
                {
                    cnt = cntMax + rand.Next(cntMax);

                    x = rand.Next(gl_Width);
                    y = rand.Next(gl_Height);
                }

                angle += dAngle;
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Show()
        {
            if (trail != null)
                trail.update(x, y);

            x += dx;
            y += dy;

            if (trail != null)
                trail.Show(R, G, B, trailOpacity);

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
                    myPrimitive._TriangleInst.setInstanceCoords(x, y, size, angle);
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

            // Populate large bodies
            while (list.Count < n)
            {
                list.Add(new myObj_1070());
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
                        glClear(GL_COLOR_BUFFER_BIT);

                    grad.Draw();
                }

                // Render Frame
                {
                    myPrimitive._LineInst.ResetBuffer();
                    inst.ResetBuffer();

                    for (int i = 0; i != Count; i++)
                    {
                        var obj = list[i] as myObj_1070;

                        obj.Show();
                        obj.Move();
                    }

                    if (doFillShapes)
                    {
                        // Tell the fragment shader to multiply existing instance opacity by 0.5:
                        inst.SetColorA(-0.5f);
                        inst.Draw(true);
                    }

                    myPrimitive._LineInst.Draw();

                    // Tell the fragment shader to do nothing with the existing instance opacity:
                    inst.SetColorA(0);
                    inst.Draw(false);
                }

                if (Count < N && cnt % genRate == 0)
                {
                    list.Add(new myObj_1070());
                }

                cnt++;
                System.Threading.Thread.Sleep(renderDelay);
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void initShapes()
        {
            base.initShapes(shape, N, 0);

            grad = new myScreenGradient();
            grad.SetRandomColors(rand, 0.2f);

            if (doClearBuffer == false)
                grad.SetOpacity(0.1f);

            myPrimitive.init_LineInst(N * trailLength);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void getColor()
        {
            switch (colorMode)
            {
                case 0:
                    {
                        R = G = B = 0.5f;
                    }
                    break;

                case 1:
                    {
                        if (myUtils.randomChance(rand, 1, 75))
                        {
                            R = (float)rand.NextDouble();
                            G = (float)rand.NextDouble();
                            B = (float)rand.NextDouble();
                        }
                        else
                        {
                            R = G = B = 0.5f;
                        }
                    }
                    break;

                case 2:
                    {
                        if (myUtils.randomChance(rand, 1, 123))
                        {
                            do
                            {
                                r1 = (float)rand.NextDouble();
                                g1 = (float)rand.NextDouble();
                                b1 = (float)rand.NextDouble();
                            }
                            while (r1 + g1 + b1 < 0.25f);
                        }
    
                        R = r1;
                        G = g1;
                        B = b1;
                    }
                    break;
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------
    }
};
