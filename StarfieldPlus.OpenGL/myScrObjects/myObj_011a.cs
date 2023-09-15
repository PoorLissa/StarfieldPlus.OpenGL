using GLFW;
using static OpenGL.GL;
using System.Collections.Generic;


/*
    - Particles with real trails
*/


namespace my
{
    public class myObj_011a : myObject
    {
        // Priority
        public static int Priority => 999910;

        private float x, y, dx, dy, a, da, angle, dAngle, radx, rady;
        private float A, R, G, B;
        private myParticleTrail trail = null;

        private static int N = 0, nTrail = 0, x0 = gl_x0, y0 = gl_y0;
        private static int moveMode = 0, lineWidth = 1, startMode = 0, offset = 0, randomizeTrail1Mode = 0, randomizeTrail2Mode = 0, trailModifyMode = 0;
        private static bool doRandomizeSpeed = true, doRandomizeTrail1 = true, doRandomizeTrail2 = true, doVaryRadius = true;
        private static float randomizeTrail1Factor = 0, randomizeTrail2Factor = 0;
        private static float t = 0, dt = 0;
        private static float borderRepulsionFactor = 0;

        private static myFreeShader shader = null;
        static myTexRectangle tex = null;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_011a()
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
                dt = 0.01f;

                switch (rand.Next(4))
                {
                    case 0:
                        {
                            N = 3 + rand.Next(33);
                            nTrail = 100 + rand.Next(500);
                        }
                        break;

                    case 1:
                        {
                            N = 3 + rand.Next(111);
                            nTrail = 100 + rand.Next(333);
                        }
                        break;

                    case 2:
                        {
                            N = 111 + rand.Next(666);
                            nTrail = 50 + rand.Next(111);
                        }
                        break;

                    case 3:
                        {
                            N = 111 + rand.Next(777);
                            nTrail = 25 + rand.Next(50);
                        }
                        break;
                }
            }

            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time local initialization
        private void initLocal()
        {
            doClearBuffer = true;

            doVaryRadius = myUtils.randomChance(rand, 1, 3);

            doRandomizeTrail1 = myUtils.randomBool(rand);
            doRandomizeTrail2 = myUtils.randomBool(rand);

            randomizeTrail1Mode = rand.Next(2);
            randomizeTrail2Mode = rand.Next(4);

            randomizeTrail1Factor = rand.Next(23) + 1;
            randomizeTrail2Factor = rand.Next(15) + 1;

            moveMode = rand.Next(6);
            startMode = rand.Next(4);

            // line width
            switch (rand.Next(3))
            {
                case 0: lineWidth = rand.Next(3) + 1; break;
                case 1: lineWidth = rand.Next(5) + 1; break;
                case 2: lineWidth = rand.Next(7) + 1; break;
            }

            offset = rand.Next(666);
            doRandomizeSpeed = myUtils.randomChance(rand, 1, 5);
            trailModifyMode = rand.Next(13);

            borderRepulsionFactor = 0.1f + myUtils.randFloat(rand) * 0.5f;

            // Set central point for the linear move modes
            if (myUtils.randomChance(rand, 2, 3))
            {
                x0 = rand.Next(gl_Width);
                y0 = rand.Next(gl_Height);
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            height = 600;

            string nStr(int   n) { return n.ToString("N0");    }
            string fStr(float f) { return f.ToString("0.000"); }

            if (moveMode < 3)
            {
                string brf = $"{fStr(borderRepulsionFactor)}";

                string str = $"Obj = myObj_011a\n\n"                       +
                                $"N = {nStr(list.Count)} of {nStr(N)}\n"   +
                                $"doClearBuffer = {doClearBuffer}\n"       +
                                $"moveMode = {moveMode}\n"                 +
                                $"startMode = {startMode}\n"               +
                                $"lineWidth = {lineWidth}\n"               +
                                $"borderRepulsionFactor = {brf}\n"         +
                                $"nTrail = {nTrail}\n"                     +
                                $"offset = {offset}\n"                     +
                                $"doRandomizeSpeed = {doRandomizeSpeed}\n" +
                                $"renderDelay = {renderDelay}\n"           +
                                $"file: {colorPicker.GetFileName()}"
                    ;
                return str;
            }
            else
            {
                string str = $"Obj = myObj_011a\n\n"                     +
                                $"N = {nStr(list.Count)} of {nStr(N)}\n" +
                                $"doClearBuffer = {doClearBuffer}\n"     +
                                $"moveMode = {moveMode}\n"               +
                                $"startMode = {startMode}\n"             +
                                $"lineWidth = {lineWidth}\n"             +
                                $"nTrail = {nTrail}\n"                   +
                                $"doVaryRadius = {doVaryRadius}\n"       +
                                $"renderDelay = {renderDelay}\n"         +
                                $"file: {colorPicker.GetFileName()}"
                    ;
                return str;
            }
        }

        // ---------------------------------------------------------------------------------------------------------------

        // 
        protected override void setNextMode()
        {
            System.Threading.Thread.Sleep(17);

            initLocal();
            myPrimitive._LineInst.setLineWidth(lineWidth);

            System.Threading.Thread.Sleep(17);
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void generateNew()
        {
            float spdFactor = 10;

            x = rand.Next(gl_Width);
            y = rand.Next(gl_Height);
            dx = myUtils.randFloat(rand, 0.1f) * myUtils.randomSign(rand) * spdFactor;
            dy = myUtils.randFloat(rand, 0.1f) * myUtils.randomSign(rand) * spdFactor;

            // Initial position of all the trail points
            switch (startMode)
            {
                case 0:
                    break;

                case 1:
                    x = x0;
                    break;

                case 2:
                    y = y0;
                    break;

                case 3:
                    x = x0;
                    y = y0;
                    break;
            }

            // Circular motion setup
            if (moveMode >= 3)
            {
                switch (moveMode)
                {
                    // Circle
                    case 3:
                        radx = rady = rand.Next(gl_x0) + 3;
                        break;

                    // Horizontal Ellipse
                    case 4:
                        radx = rand.Next(gl_x0) + 3;
                        rady = radx * myUtils.randFloat(rand);
                        break;

                    // Random Ellipse
                    case 5:
                        radx = rand.Next(gl_x0) + 3;
                        rady = rand.Next(gl_x0) + 3;
                        break;
                }

                angle = myUtils.randFloat(rand) * rand.Next(66) * myUtils.randomSign(rand);
                dAngle = myUtils.randFloat(rand, 0.1f) * 0.05f * myUtils.randomSign(rand);

                x = gl_x0 + radx * (float)System.Math.Sin(angle);
                y = gl_y0 + rady * (float)System.Math.Cos(angle);
            }

            A = 0.25f + myUtils.randFloat(rand) * 0.25f;
            da = A / (nTrail + 1);
            colorPicker.getColorRand(ref R, ref G, ref B);

            // Initialize Trail
            if (trail == null)
            {
                trail = new myParticleTrail(nTrail, x, y);
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            // Update trail info
            {
                if (doRandomizeTrail1)
                {
                    float r1 = 0, r2 = 0;
                    randomizeTrail1Factor = 10;

                    switch (randomizeTrail1Mode)
                    {
                        case 0:
                            r1 = myUtils.randFloat(rand) * myUtils.randomSign(rand) * randomizeTrail1Factor;
                            r2 = myUtils.randFloat(rand) * myUtils.randomSign(rand) * randomizeTrail1Factor;
                            break;

                        case 1:
                            r1 = (float)System.Math.Sin(x) * 3;
                            r2 = (float)System.Math.Cos(y) * 3;
                            break;
                    }

                    trail.update(x + r1, y + r2);
                }
                else
                {
                    trail.update(x, y);
                }
            }

            a = A;

            switch (moveMode)
            {
                case 0:
                    {
                        x += dx;
                        y += dy;

                        if (x < 0 && dx < 0)
                            dx *= -1;

                        if (y < 0 && dy < 0)
                            dy *= -1;

                        if (x > gl_Width && dx > 0)
                            dx *= -1;

                        if (y > gl_Height && dy > 0)
                            dy *= -1;
                    }
                    break;

                case 1:
                    {
                        x += dx;
                        y += dy;

                        if (x < offset)
                            dx += borderRepulsionFactor;

                        if (y < offset)
                            dy += borderRepulsionFactor;

                        if (x > gl_Width - offset)
                            dx -= borderRepulsionFactor;

                        if (y > gl_Height - offset)
                            dy -= borderRepulsionFactor;
                    }
                    break;

                case 2:
                    {
                        x += dx;
                        y += dy;

                        float val = myUtils.randFloat(rand) * 0.15f;

                        if (x < offset)
                            dx += val;

                        if (y < offset)
                            dy += val;

                        if (x > gl_Width - offset)
                            dx -= val;

                        if (y > gl_Height - offset)
                            dy -= val;
                    }
                    break;

                // Circular motion
                case 3:
                case 4:
                case 5:
                    {
                        x = gl_x0 + radx * (float)System.Math.Sin(angle);
                        y = gl_y0 + rady * (float)System.Math.Cos(angle);

                        //x += (float)System.Math.Sin(t) * 333;
                        //y += (float)System.Math.Cos(t) * 333;

                        angle += dAngle;

                        if (doVaryRadius && myUtils.randomChance(rand, 1, 11))
                        {
                            radx += myUtils.randFloatSigned(rand);
                            rady += myUtils.randFloatSigned(rand);
                        }
                    }
                    break;
            }

            if (doRandomizeSpeed)
            {
                if (myUtils.randomChance(rand, 1, 5))
                {
                    dx += myUtils.randFloat(rand) * myUtils.randomSign(rand) * 0.5f;
                }

                if (myUtils.randomChance(rand, 1, 5))
                {
                    dy += myUtils.randFloatSigned(rand) * 0.5f;
                }
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Show()
        {
            // Draw the trail
            {
                float x1 = 0, y1 = 0, x2 = 0, y2 = 0;
                int i = 0;

                // Get the first pair of coordinates
                trail.getXY(i++, ref x1, ref y1);

                for (; i < nTrail; i++)
                {
                    // Get the second pair of coordinates
                    trail.getXY(i, ref x2, ref y2);

                    switch (trailModifyMode)
                    {
                        case 0:
                            drawTailSegment(x1, y1 + i/2, x2, y2 - i/2);
                            break;

                        default:
                            drawTailSegment(x1, y1, x2, y2);
                            break;
                    }

                    // Shift the first pair 1 position towards the end
                    x1 = x2;
                    y1 = y2;

                    a -= da;
                }
            }

            // Draw the particle
            {
                shader.SetColor(R, G, B, A * 1.5f);
                shader.Draw(x, y, 8, 8, 10);
            }
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void drawTailSegment(float x1, float y1, float x2, float y2)
        {
            if (x1 == x2 && y1 == y2)
                return;

            if (doRandomizeTrail2)
            {
                float r1 = myUtils.randFloat(rand) * myUtils.randomSign(rand) * randomizeTrail2Factor;
                float r2 = myUtils.randFloat(rand) * myUtils.randomSign(rand) * randomizeTrail2Factor;

                switch (randomizeTrail2Mode)
                {
                    case 0:
                        myPrimitive._LineInst.setInstanceCoords(x1 + r1, y1 + r2, x2 + r1, y2 + r2);
                        break;

                    case 1:
                        myPrimitive._LineInst.setInstanceCoords(x1, y1, x2 + r1, y2 + r2);
                        break;

                    case 2:
                        myPrimitive._LineInst.setInstanceCoords(x1 + r1, y1, x2 + r2, y2);
                        break;

                    case 3:
                        r1 = (float)System.Math.Sin(x1 + y1) * randomizeTrail2Factor;
                        r2 = (float)System.Math.Sin(x2 + y2) * randomizeTrail2Factor;
                        myPrimitive._LineInst.setInstanceCoords(x1 + r1, y1 + r2, x2 - r1, y2 - r2);
                        break;
                }
            }
            else
            {
                myPrimitive._LineInst.setInstanceCoords(x1, y1, x2, y2);
            }

            myPrimitive._LineInst.setInstanceColor(R, G, B, a);

#if false
            int zz = 2;

            myPrimitive._LineInst.setInstanceCoords(x1 - zz, y1 - zz, x2 - zz, y2 - zz);
            myPrimitive._LineInst.setInstanceColor(R/2, G/2, B/2, a/2);

            myPrimitive._LineInst.setInstanceCoords(x1 + zz, y1 + zz, x2 + zz, y2 + zz);
            myPrimitive._LineInst.setInstanceColor(R/2, G/2, B/2, a/2);
#endif
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Process(Window window)
        {
            uint cnt = 0;
            initShapes();


            clearScreenSetup(doClearBuffer, 0.13f);
            glDrawBuffer(GL_BACK);


            while (!Glfw.WindowShouldClose(window))
            {
                int Count = list.Count;

                processInput(window);

                // Swap fore/back framebuffers, and poll for operating system events.
                Glfw.SwapBuffers(window);
                Glfw.PollEvents();

                // Dim screen
                {
                    glClear(GL_COLOR_BUFFER_BIT);

                    if (tex != null)
                    {
                        tex.setOpacity(0.9f);
                        tex.Draw(0, 0, gl_Width, gl_Height);
                    }
                }

                // Render Frame
                {
                    myPrimitive._LineInst.ResetBuffer();

                    for (int i = 0; i != Count; i++)
                    {
                        var obj = list[i] as myObj_011a;

                        obj.Show();
                        obj.Move();
                    }

                    myPrimitive._LineInst.Draw();
                }

                if (Count < N)
                {
                    list.Add(new myObj_011a());
                }

                cnt++;
                t += dt;
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void initShapes()
        {
            myPrimitive.init_LineInst(N * nTrail);
            myPrimitive._LineInst.setLineWidth(lineWidth);

            getShader();

            var mode = (myColorPicker.colorMode)colorPicker.getMode();

            if (mode == myColorPicker.colorMode.IMAGE || mode == myColorPicker.colorMode.SNAPSHOT)
            {
                if (myUtils.randomChance(rand, 1, 3))
                {
                    tex = new myTexRectangle(colorPicker.getImg());
                }
            }
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void getShader()
        {
            string header = "";
            string main = "";

            my.myShaderHelpers.Shapes.getShader_000(ref rand, ref header, ref main);
            shader = new myFreeShader(header, main);
        }

        // ---------------------------------------------------------------------------------------------------------------

    }
};
