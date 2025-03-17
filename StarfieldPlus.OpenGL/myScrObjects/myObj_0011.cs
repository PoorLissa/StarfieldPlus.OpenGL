using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;


/*
    - Randomly Roaming Lines (based on Randomly Roaming Squares)
*/


namespace my
{
    public class myObj_0011 : myObject
    {
        // Priority
        public static int Priority => 10;
		public static System.Type Type => typeof(myObj_0011);

        // ---------------------------------------------------------------------------------------------------------------

        private class myObj_0011_Particle
        {
            public int count, rad;
            public float x, y, dx, dy, x0, y0;
            public float angle, dAngle;
        };

        // ---------------------------------------------------------------------------------------------------------------

        private static int N = 1, pN = 1, moveMode = 0, rotationMode = 0, radMode = 0, borderOffset = 0;
        private static bool doCleanOnce = false, doAddAtOnce = false;
        private static float dimAlpha = 0.01f, t = 0, maxOpacity = 0;

        private int Cnt, x0, y0, rad;
        private float R, G, B, A, r, g, b;
        private List<myObj_0011_Particle> particles = null;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_0011()
        {
            particles = new List<myObj_0011_Particle>();

            generateNew();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time global initialization
        protected override void initGlobal()
        {
            colorPicker = new myColorPicker(gl_Width, gl_Height);
            list = new List<myObject>();

            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time local initialization
        private void initLocal()
        {
            N  = 3 + rand.Next(11);     // Total number of objects
            pN = 2 + rand.Next(11);     // Particles per object

            doClearBuffer = false;
            doAddAtOnce = myUtils.randomBool(rand);

            maxOpacity = 0.025f + myUtils.randFloat(rand) * 0.33f;
            renderDelay = rand.Next(11) + 10;
            stepsPerFrame = rand.Next(10) + 1;
            moveMode = rand.Next(3);
            rotationMode = rand.Next(4);
            radMode = rand.Next(4);

            dimAlpha = 0.1f;

            // Sometimes with the large number of objects, the opacity is too large or too small;
            // Don't know how to find the right value yet
            if (myUtils.randomChance(rand, 1, 13))
            {
                N = 99 + rand.Next(250);
                maxOpacity = 33.0f / (N * pN);
                maxOpacity = dimAlpha / stepsPerFrame + 0.01f;
            }

            switch (rand.Next(3))
            {
                case 0:
                    borderOffset = 0;
                    break;

                case 1:
                    borderOffset = -rand.Next(111);
                    break;

                case 2:
                    borderOffset = rand.Next(321);
                    break;
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            string str = $"Obj = {Type} -- Randomly Roaming Lines\n\n"       	+
                            myUtils.strCountOf(list.Count, N)                   +
                            $"pN = {pN}\n"                                      +
                            $"maxOpacity = {maxOpacity.ToString("0.000")}f\n"   +
                            $"moveMode = {moveMode}\n"                          +
                            $"radMode = {radMode}\n"                            +
                            $"rotationMode = {rotationMode}\n"                  +
                            $"borderOffset = {borderOffset}\n"                  +
                            $"doClearBuffer = {doClearBuffer}\n"                +
                            $"stepsPerFrame = {stepsPerFrame}\n"                +
                            $"renderDelay = {renderDelay}\n"                    +
                            $"file: {colorPicker.GetFileName()}"
                ;
            return str;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void generateNew()
        {
            rad = rand.Next(gl_Width) + 333;
            x0 = rand.Next(gl_Width);
            y0 = rand.Next(gl_Height);

            float speedFactor = 0.01f / stepsPerFrame;

            if (particles.Count == 0)
                while (particles.Count < pN)
                    particles.Add(new myObj_0011_Particle());

            foreach (myObj_0011_Particle item in particles)
            {
                item.x = rand.Next(gl_Width);
                item.y = rand.Next(gl_Height);

                float itemdAngle = myUtils.randFloatClamped(rand, 0.25f) * 0.01f;

                switch (moveMode)
                {
                    // Linear motion
                    case 00:
                    case 01:
                        {
                            item.dx = (rand.Next(1111) + 111) * myUtils.randomSign(rand) * speedFactor;
                            item.dy = (rand.Next(1111) + 111) * myUtils.randomSign(rand) * speedFactor;
                        }
                        break;

                    // Rotation around the common center
                    case 02:
                        {
                            switch (radMode)
                            {
                                case 0: item.rad = rad; break;
                                case 1: item.rad = rand.Next(rad) + 123; break;
                                case 2: item.rad = rad + rand.Next(123); break;
                            }

                            switch (rotationMode)
                            {
                                case 0:
                                    item.dAngle = itemdAngle;
                                    break;

                                case 1:
                                    item.dAngle = itemdAngle;
                                    item.dAngle *= myUtils.randomSign(rand);
                                    break;

                                case 2:
                                    item.dAngle = myUtils.randFloatClamped(rand, 0.25f) * 0.01f;
                                    break;

                                case 3:
                                    item.dAngle = myUtils.randFloatClamped(rand, 0.25f) * 0.01f;
                                    item.dAngle *= myUtils.randomSign(rand);
                                    break;
                            }

                            item.angle = rand.Next(123) * myUtils.randFloat(rand);
                            item.x = gl_x0 + (float)(Math.Sin(item.angle) * item.rad);
                            item.y = gl_y0 + (float)(Math.Cos(item.angle) * item.rad);
                        }
                        break;

                    // Rotation around different centers
                    case 03:
                        {
                            switch (radMode)
                            {
                                case 0: item.rad = rad; break;
                                case 1: item.rad = rand.Next(rad) + 123; break;
                                case 2: item.rad = rad + rand.Next(123); break;
                            }

                            switch (rotationMode)
                            {
                                case 0:
                                    item.dAngle = itemdAngle;
                                    break;

                                case 1:
                                    item.dAngle = itemdAngle;
                                    item.dAngle *= myUtils.randomSign(rand);
                                    break;

                                case 2:
                                    item.dAngle = myUtils.randFloatClamped(rand, 0.25f) * 0.01f;
                                    break;

                                case 3:
                                    item.dAngle = myUtils.randFloatClamped(rand, 0.25f) * 0.01f;
                                    item.dAngle *= myUtils.randomSign(rand);
                                    break;
                            }

                            item.angle = rand.Next(123) * myUtils.randFloat(rand);

                            item.x0 = x0;
                            item.y0 = y0;

                            item.x = item.x0 + (float)(Math.Sin(item.angle) * item.rad);
                            item.y = item.y0 + (float)(Math.Cos(item.angle) * item.rad);
                        }
                        break;

                    case 04:
                        {
                            item.dx = myUtils.randFloat(rand) * rand.Next(1234);
                            item.dy = myUtils.randFloat(rand) * myUtils.randomSign(rand);
                        }
                        break;

                    case 05:
                        {
                            if (myUtils.randomChance(rand, 1, 2))
                            {
                                item.dx = (rand.Next(1111) + 111) * myUtils.randomSign(rand) * speedFactor;
                                item.dy = 0;
                            }
                            else
                            {
                                item.dy = (rand.Next(1111) + 111) * myUtils.randomSign(rand) * speedFactor;
                                item.dx = 0;
                            }
                        }
                        break;
                }

                item.count = 0;
            }

            R = 1.0f - myUtils.randFloat(rand) * 0.05f;
            G = 1.0f - myUtils.randFloat(rand) * 0.05f;
            B = 1.0f - myUtils.randFloat(rand) * 0.05f;

            A = maxOpacity / stepsPerFrame;

            Cnt = rand.Next(1000) + 1000;

            x0 = rand.Next(gl_Width);
            y0 = rand.Next(gl_Height);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void setNextMode()
        {
            initLocal();
            doCleanOnce = true;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            Cnt--;

            foreach (myObj_0011_Particle item in particles)
            {
/*
                if (item.cnt > 0)
                {
                    item.cnt--;

                    item.x += (float)Math.Sin(item.x) * 3;
                    item.y += (float)Math.Cos(item.y) * 3;
                }
*/

                switch (moveMode)
                {
                    // Linear motion: Bounce off the borders
                    case 00:
                        {
                            item.x += item.dx;
                            item.y += item.dy;

                            if (item.x < 0 - borderOffset || item.x > gl_Width + borderOffset)
                            {
                                item.dx *= -1;
                                item.count = 100;
                            }

                            if (item.y < 0 - borderOffset || item.y > gl_Height + borderOffset)
                            {
                                item.dy *= -1;
                                item.count = 100;
                            }
                        }
                        break;

                    // Linear motion: Slowly change speed vector
                    case 01:
                        {
                            item.x += item.dx;
                            item.y += item.dy;

                            if (item.x < 0 - borderOffset)
                                item.dx += 0.01f;

                            if (item.x > gl_Width + borderOffset)
                                item.dx -= 0.01f;

                            if (item.y < 0 - borderOffset)
                                item.dy += 0.01f;

                            if (item.y > gl_Height + borderOffset)
                                item.dy -= 0.01f;
                        }
                        break;

                    // Rotation around the common center
                    case 02:
                        {
                            item.x = gl_x0 + (float)Math.Sin(item.angle) * item.rad;
                            item.y = gl_y0 + (float)Math.Cos(item.angle) * item.rad;

                            item.angle += item.dAngle;
                        }
                        break;

                    // Rotation around different centers
                    case 03:
                        {
                            item.x = item.x0 + (float)Math.Sin(item.angle) * item.rad;
                            item.y = item.y0 + (float)Math.Cos(item.angle) * item.rad;

                            item.angle += item.dAngle;
                        }
                        break;

                    case 04:
                        {
                            item.x = x0 + (int)(Math.Sin(item.dx) * (rand.Next(rad) + rad));
                            item.y = y0 + (int)(Math.Cos(item.dx) * (rand.Next(rad) + rad));
                            item.dx += item.dy;
                        }
                        break;

                    case 05:
                        {
                            item.x += item.dx;
                            item.y += item.dy;

                            if (item.x < 0 - borderOffset || item.x > gl_Width + borderOffset)
                            {
                                item.dx *= -1;
                                item.count = 100;
                            }

                            if (item.y < 0 - borderOffset || item.y > gl_Height + borderOffset)
                            {
                                item.dy *= -1;
                                item.count = 100;
                            }
                        }
                        break;
                }
            }

            // Shift color
            {
                if (Cnt == 0)
                {
                    colorPicker.getColorRand(ref r, ref g, ref b);
                }

                if (Cnt < 0)
                {
                    R += (R > r) ? -0.001f : 0.001f;
                    G += (G > g) ? -0.001f : 0.001f;
                    B += (B > b) ? -0.001f : 0.001f;

                    double rDiff = Math.Abs(R - r);
                    double gDiff = Math.Abs(G - g);
                    double bDiff = Math.Abs(B - b);

                    if (rDiff + gDiff + bDiff < 0.005)
                    {
                        Cnt = rand.Next(1000) + 1000;
                    }
                }
            }
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Show()
        {
            // Render connecting lines
            myObj_0011_Particle p = particles[particles.Count - 1];

            float xOld = p.x;
            float yOld = p.y;

            for (int i = 0; i != particles.Count; i++)
            {
                p = particles[i];

                myPrimitive._LineInst.setInstanceCoords(p.x, p.y, xOld, yOld);
                myPrimitive._LineInst.setInstanceColor(R, G, B, A);

                xOld = p.x;
                yOld = p.y;
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Process(Window window)
        {
            uint cnt = 0;
            int step, i;
            float bgrR = myUtils.randFloat(rand) * 0.05f;
            float bgrG = myUtils.randFloat(rand) * 0.05f;
            float bgrB = myUtils.randFloat(rand) * 0.05f;

            initShapes();

            if (doAddAtOnce)
            {
                while (list.Count < N)
                    list.Add(new myObj_0011());
            }

            glDrawBuffer(GL_FRONT_AND_BACK);
            glClearColor(0, 0, 0, 1);

            if (false)
            {
                Glfw.SwapInterval(0);
            }

            while (!Glfw.WindowShouldClose(window))
            {
                processInput(window);

                // Swap fore/back framebuffers, and poll for operating system events.
                Glfw.SwapBuffers(window);
                Glfw.PollEvents();

                if (doClearBuffer || doCleanOnce)
                {
                    glClear(GL_COLOR_BUFFER_BIT);
                    doCleanOnce = false;
                }
                else
                {
                    dimAlpha = (float)Math.Sin(t) * 0.01f;
                    t += 0.01f;

                    // Dim the screen constantly;
                    myPrimitive._Rectangle.SetColor(bgrR, bgrG, bgrB, dimAlpha);
                    myPrimitive._Rectangle.SetAngle(0);
                    myPrimitive._Rectangle.Draw(0, 0, gl_Width, gl_Height, true);
                }

                // Render Frame
                {
                    myPrimitive._LineInst.ResetBuffer();

                    for (step = 0; step < stepsPerFrame; step++)
                    {
                        for (i = 0; i < list.Count; i++)
                        {
                            var obj = list[i] as myObj_0011;
                            obj.Show();
                            obj.Move();
                        }
                    }

                    myPrimitive._LineInst.Draw();
                }

                if (list.Count < N)
                    list.Add(new myObj_0011());

                System.Threading.Thread.Sleep(renderDelay);
                cnt++;
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void initShapes()
        {
            myPrimitive.init_Rectangle();
            myPrimitive.init_LineInst(N * pN * stepsPerFrame);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------
    }
};
