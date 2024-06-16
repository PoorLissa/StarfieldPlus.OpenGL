using GLFW;
using static OpenGL.GL;
using System.Collections.Generic;
using System;


/*
    - Particles with real trails

    Smooth trail howto:
    https://kosmonautblog.wordpress.com/2016/07/29/geometry-trails-tire-tracks-tutorial/
*/


namespace my
{
    public class myObj_0011a : myObject
    {
        // Priority
        public static int Priority => 10;
		public static System.Type Type => typeof(myObj_0011a);

        private float x, y, dx, dy, X, Y, a, da, angle, dAngle, radx, rady;
        private float A, R, G, B;
        private int cnt;
        private myParticleTrail trail = null;

        private static int N = 0, nTrail = 0, x0 = gl_x0, y0 = gl_y0;
        private static int moveMode = 0, lineWidth = 1, startMode = 0, borderOffset = 0, randomizeTrail1Mode = 0, randomizeTrail2Mode = 0, trailModifyMode = 0;
        private static bool doRandomizeSpeed = true, doRandomizeTrail1 = true, doRandomizeTrail2 = true, doVaryRadius = true;
        private static float spdFactor = 10.0f;
        private static float randomizeTrail1Factor = 0, randomizeTrail2Factor = 0;
        private static float t = 0, dt = 0;
        private static float borderRepulsionFactor = 0;

        private static int int_01 = 0, int_02 = 0;

        private static myFreeShader shader = null;
        static myTexRectangle tex = null;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_0011a()
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

            moveMode = rand.Next(10);
            startMode = rand.Next(4);

            // line width
            switch (rand.Next(3))
            {
                case 0: lineWidth = rand.Next(3) + 1; break;
                case 1: lineWidth = rand.Next(5) + 1; break;
                case 2: lineWidth = rand.Next(7) + 1; break;
            }

            borderOffset = rand.Next(666);
            doRandomizeSpeed = myUtils.randomChance(rand, 1, 5);
            trailModifyMode = rand.Next(13);

            borderRepulsionFactor = 0.1f + myUtils.randFloat(rand) * 0.5f;

            // Set central point for the linear move modes
            if (myUtils.randomChance(rand, 2, 3))
            {
                x0 = rand.Next(gl_Width);
                y0 = rand.Next(gl_Height);
            }

            switch (moveMode)
            {
                case 7:
                case 8:
                    int_01 = 10 + rand.Next(90);
                    break;

                case 9:
                    doRandomizeSpeed = false;
                    int_01 = rand.Next(6);
                    break;

                case 10:
                    doRandomizeSpeed = myUtils.randomChance(rand, 1, 7);
                    int_01 = 1 + rand.Next(8);
                    int_02 = 2 + rand.Next(7);
                    break;
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            height = 600;

            string nStr(int n) { return n.ToString("N0"); }
            string fStr(float f) { return f.ToString("0.000"); }

            if (moveMode < 3)
            {
                string brf = $"{fStr(borderRepulsionFactor)}";

                string str = $"Obj = {Type}\n\n"                        	+
                                $"N = {nStr(list.Count)} of {nStr(N)}\n"    +
                                $"doClearBuffer = {doClearBuffer}\n"        +
                                $"doRandomizeSpeed = {doRandomizeSpeed}\n"  +
                                $"moveMode = {moveMode}\n"                  +
                                $"startMode = {startMode}\n"                +
                                $"lineWidth = {lineWidth}\n"                +
                                $"borderRepulsionFactor = {brf}\n"          +
                                $"nTrail = {nTrail}\n"                      +
                                $"borderOffset = {borderOffset}\n"          +
                                $"doRandomizeSpeed = {doRandomizeSpeed}\n"  +
                                $"renderDelay = {renderDelay}\n"            +
                                $"int_01 = {int_01}\n"                      +
                                $"file: {colorPicker.GetFileName()}"
                    ;
                return str;
            }
            else
            {
                string str = $"Obj = myObj_0011a\n\n"                       +
                                $"N = {nStr(list.Count)} of {nStr(N)}\n"    +
                                $"doClearBuffer = {doClearBuffer}\n"        +
                                $"doRandomizeSpeed = {doRandomizeSpeed}\n"  +
                                $"moveMode = {moveMode}\n"                  +
                                $"startMode = {startMode}\n"                +
                                $"lineWidth = {lineWidth}\n"                +
                                $"nTrail = {nTrail}\n"                      +
                                $"doVaryRadius = {doVaryRadius}\n"          +
                                $"renderDelay = {renderDelay}\n"            +
                                $"int_01 = {int_01}\n"                      +
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
            if ((moveMode >= 3 && moveMode <= 6) || (moveMode == 10))
            {
                angle = myUtils.randFloat(rand) * rand.Next(66) * myUtils.randomSign(rand);
                dAngle = myUtils.randFloat(rand, 0.1f) * 0.05f * myUtils.randomSign(rand);

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

                    // Circle-to-Ellipse-and-back
                    case 6:
                        radx = rady = rand.Next(gl_x0) + 3;
                        break;

                    case 10:
                        radx = rady = rand.Next(gl_x0) + 3;
                        cnt = 100 + rand.Next(333);
                        dAngle *= (1.0f / int_01);
                        break;
                }

                MoveParticle();
            }

            if (moveMode == 7 || moveMode == 8)
            {
                cnt = 3 + rand.Next(10);

                switch (rand.Next(2))
                {
                    case 0: dx = 0; break;
                    case 1: dy = 0; break;
                }
            }

            if (moveMode == 9)
            {
                cnt = 33;

                radx = rady = 500;

                x = gl_x0;
                y = gl_y0;

                X = rand.Next(gl_Width);
                Y = rand.Next(gl_Height);

                dx = myUtils.randFloat(rand, 0.1f) * myUtils.randomSign(rand) * 10;
                dy = myUtils.randFloat(rand, 0.1f) * myUtils.randomSign(rand) * 10;

                angle = myUtils.randFloat(rand) * rand.Next(66) * myUtils.randomSign(rand);
                dAngle = myUtils.randFloat(rand, 0.1f) * 0.05f * myUtils.randomSign(rand);

                MoveParticle();
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
                            r1 = (float)Math.Sin(x) * 3;
                            r2 = (float)Math.Cos(y) * 3;
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

            MoveParticle();

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void MoveParticle()
        {
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

                        if (x < borderOffset)
                            dx += borderRepulsionFactor;

                        if (y < borderOffset)
                            dy += borderRepulsionFactor;

                        if (x > gl_Width - borderOffset)
                            dx -= borderRepulsionFactor;

                        if (y > gl_Height - borderOffset)
                            dy -= borderRepulsionFactor;
                    }
                    break;

                case 2:
                    {
                        x += dx;
                        y += dy;

                        float val = myUtils.randFloat(rand) * 0.15f;

                        if (x < borderOffset)
                            dx += val;

                        if (y < borderOffset)
                            dy += val;

                        if (x > gl_Width - borderOffset)
                            dx -= val;

                        if (y > gl_Height - borderOffset)
                            dy -= val;
                    }
                    break;

                // Circular motion
                case 3:
                case 4:
                case 5:
                    {
                        x = gl_x0 + radx * (float)Math.Sin(angle);
                        y = gl_y0 + rady * (float)Math.Cos(angle);

                        angle += dAngle;

                        if (doVaryRadius && myUtils.randomChance(rand, 1, 11))
                        {
                            radx += myUtils.randFloatSigned(rand);
                            rady += myUtils.randFloatSigned(rand);
                        }
                    }
                    break;

                // Circular motion with varying y-radius
                case 6:
                    {
                        x = gl_x0 + radx * (float)Math.Sin(angle);
                        y = gl_y0 + rady * (float)Math.Cos(angle) * (float)Math.Sin(t * 0.05);

                        angle += dAngle;

                        if (doVaryRadius && myUtils.randomChance(rand, 1, 11))
                        {
                            radx += myUtils.randFloatSigned(rand);
                            rady += myUtils.randFloatSigned(rand);
                        }
                    }
                    break;

                case 7:
                case 8:
                    {
                        if (--cnt == 0)
                        {
                            switch (moveMode)
                            {
                                case 7: mode7GenerateNew(); break;
                                case 8: mode8GenerateNew(); break;
                            }
                        }
                        else
                        {
                            x += dx;
                            y += dy;
                        }
                    }
                    break;

                case 9:
                    {
                        if (--cnt == 0)
                        {
                            mode9GenerateNew();
                        }
                        else
                        {
                            x += dx * (float)Math.Sin(angle);
                            y += dy * (float)Math.Cos(angle);

                            angle += dAngle;
                        }
                    }
                    break;

                case 10:
                    {
                        if (--cnt < 1)
                        {
                            if (cnt == 0)
                            {
                                dx = x - gl_x0;
                                dy = y - gl_y0;

                                int speed = 1 + rand.Next(int_02);

                                float dist = speed / (float)Math.Sqrt(dx * dx + dy * dy);

                                dx *= dist;
                                dy *= dist;
                            }

                            if (cnt < -11)
                            {
                                x += dx;
                                y += dy;

                                if (x < 0 || y < 0 || x > gl_Width || y > gl_Height)
                                {
                                    A -= 0.01f;

                                    if (A < 0)
                                    {
                                        generateNew();
                                        trail.reset(x, y);
                                    }
                                }
                            }
                        }
                        else
                        {
                            x = gl_x0 + radx * (float)Math.Sin(angle);
                            y = gl_y0 + rady * (float)Math.Cos(angle);

                            angle += dAngle;
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
                            drawTailSegment(x1, y1 + i / 2, x2, y2 - i / 2);
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
                        var obj = list[i] as myObj_0011a;

                        obj.Show();
                        obj.Move();
                    }

                    myPrimitive._LineInst.Draw();
                }

                if (Count < N)
                {
                    list.Add(new myObj_0011a());
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

        private void mode7GenerateNew()
        {
            float val = myUtils.randFloat(rand, 0.1f) * spdFactor;
            cnt = 13 + rand.Next(int_01);

            switch (rand.Next(2))
            {
                case 0:
                    dx = 0;
                    dy = val;

                    if (y > gl_Height)
                        dy *= -1;
                    else if (y > 0)
                        dy *= myUtils.randomSign(rand);
                    break;

                case 1:
                    dx = val;
                    dy = 0;

                    if (x > gl_Width)
                        dx *= -1;
                    else if (x > 0)
                        dx *= myUtils.randomSign(rand);
                    break;
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void mode8GenerateNew()
        {
            float val = myUtils.randFloat(rand, 0.1f) * spdFactor;
            cnt = 13 + rand.Next(int_01);

            if (dx == 0)
            {
                dx = val;
                dy = 0;

                if (x > gl_Width)
                    dx *= -1;
                else if (x > 0)
                    dx *= myUtils.randomSign(rand);
            }
            else
            {
                dx = 0;
                dy = val;

                if (y > gl_Height)
                    dy *= -1;
                else if (y > 0)
                    dy *= myUtils.randomSign(rand);
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void mode9GenerateNew()
        {
            cnt = 33 + rand.Next(100);

            if (x > gl_Width + 666 || x < -666 || y > gl_Height + 666 || y < -666)
            {
                generateNew();
                trail.reset(x, y);
                return;
            }

            float signx = myUtils.signOf(dx);
            float signy = myUtils.signOf(dy);

            float val1 = myUtils.randFloat(rand, 0.1f);
            float val2 = myUtils.randFloat(rand, 0.1f);

            int factor1 = (3 + rand.Next(11));
            int factor2 = (3 + rand.Next(11));

            switch (int_01)
            {
                case 0:
                    dx = val1 * factor1 * myUtils.randomSign(rand);
                    dy = val2 * factor2 * myUtils.randomSign(rand);
                    break;

                case 1:
                    dx = val1 * factor1 * signx;
                    dy = val2 * factor2 * signy;
                    break;

                case 2:
                    dx = val1 * factor1;
                    dy = val1 * factor1;
                    break;

                case 3:
                    dx = val1 * factor1 * myUtils.randomSign(rand);
                    dy = val1 * factor1 * myUtils.randomSign(rand);
                    break;

                case 4:
                    dx = val1 * factor1 * signx;
                    dy = val1 * factor1 * signy;
                    break;

                case 5:
                    dx = Math.Abs(dx) * myUtils.randomSign(rand);
                    dy = Math.Abs(dy) * myUtils.randomSign(rand);
                    break;
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

    }
};
