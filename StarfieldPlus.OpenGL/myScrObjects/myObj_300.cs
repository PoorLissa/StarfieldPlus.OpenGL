using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;


/*
    - Small Explosions of Particles + Variations

    todo:
        - make another mode, where each square will display underlying image with its own opacity and size
*/


namespace my
{
    public class myObj_300 : myObject
    {
        private class myObj_300_Particle
        {
            public bool isFirstIteration = false;
            public float x, y, r, dx, dy, dr, a, angle, dAngle, time, dTime;
            public int i1, i2, i3;
        };

        // ---------------------------------------------------------------------------------------------------------------

        private static bool doClearBuffer = false, doFillShapes = false, doUseInstancing = false, doUseCenterRepel = false,
                            doUseBorderRepel = false, doGenerateAtCenter = false;
        private static int x0, y0, N = 1, gravityRate = 0, maxParticles = 25, maxSize = 6;
        private static int shapeType = 0, moveType = 0, radiusMode = 0, fastExplosion = 0, rotationMode = 0, rotationSubMode = 0, colorMode = 0;
        private static float dimAlpha = 0.1f;

        private static float const_f1 = 0, const_f2 = 0;
        private static int   const_i1 = 0, const_i2 = 0;

        private static myInstancedPrimitive inst = null;

        private float x, y, R, G, B, A, lineTh;
        private int shape = 0, lifeCounter = 0, lifeMax = 0, objN = 0;

        private List<myObj_300_Particle> structsList = null;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_300()
        {
            if (colorPicker == null)
            {
                init();
            }

            structsList = new List<myObj_300_Particle>();

            for (int i = 0; i < maxParticles; i++)
            {
                structsList.Add(new myObj_300_Particle());
            }

            generateNew();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time initialization
        private void init()
        {
            x0 = gl_Width  / 2;
            y0 = gl_Height / 2;

            colorPicker = new myColorPicker(gl_Width, gl_Height);
            //colorPicker = new myColorPicker(gl_Width, gl_Height, myColorPicker.colorMode.SNAPSHOT_OR_IMAGE);
            
            list = new List<myObject>();

            doClearBuffer      = myUtils.randomBool(rand);
            doFillShapes       = myUtils.randomBool(rand);
            doUseCenterRepel   = myUtils.randomChance(rand, 0, 11);
            doUseBorderRepel   = myUtils.randomChance(rand, 0, 11);
            doGenerateAtCenter = myUtils.randomChance(rand, 0, 11);

            // In case the colorPicker has an underlying image, we might want to draw every particle using the image color at this particular point
            if (colorPicker.getMode() == (int)myColorPicker.colorMode.IMAGE || colorPicker.getMode() == (int)myColorPicker.colorMode.SNAPSHOT)
            {
                if (myUtils.randomChance(rand, 0, 7))
                {
                    colorMode = 1;
                }
            }

            // rotationMode: 0, 1 = rotation; 2 = no rotation, angle is 0; 3 = no rotation, angle is not 0
            rotationMode  = rand.Next(4);
            gravityRate   = rand.Next(101) + 1;
            shapeType     = rand.Next(10);
            moveType      = rand.Next(54);
            radiusMode    = rand.Next(5);
            fastExplosion = rand.Next(11);

            // In case the rotation is enabled, we also may enable additional rotation option:
            if (rotationMode < 2)
            {
                rotationSubMode = rand.Next(7);
                rotationSubMode = rotationSubMode > 2 ? 0 : rotationSubMode + 1;     // [0, 1, 2] --> [1, 2, 3]; otherwise set to '0';
            }

            const_f1 = (float)rand.NextDouble() * myUtils.randomSign(rand);
            const_i1 = rand.Next(300) + 100;

            // Set number of objects N:
            N = rand.Next(666) + 100;

            if (myUtils.randomChance(rand, 0, 3))
            {
                N = rand.Next(11) + 1;
            }

            // Set max size of a particle:
            if (myUtils.randomChance(rand, 0, 3))
            {
                maxSize += rand.Next(66);
            }

            renderDelay = 1;

#if true
            shapeType = 0;
            shapeType = 5;  // instanced square
            shapeType = 6;  // instanced triangle
            shapeType = 7;  // instanced circle
            shapeType = 8;  // instanced pentagon
            shapeType = 9;  // instanced hexagon
            shapeType = 5 + rand.Next(5);
            //doClearBuffer = true;
            //doClearBuffer = false;
            //radiusMode = 2;
            //moveType = 3333;
            doUseInstancing = shapeType >= 5;
            N = 13333;
            //N = 30000;
#endif

            // Generation at center of the screen does not look good for some of the move modes;
            // Make sure it is turned off for those particular modes (at least most of the times, anyway)
            if (myUtils.randomChance(rand, 95, 100))
            {
                int[] noCenterModes = { 13, 17, 18, 19, 20, 22, 24, 25, 26, 27, 28, 29, 30, 31, 32, 33, 39, 40, 42, 43, 44, 49 };

                foreach (var mode in noCenterModes)
                    if (moveType == mode)
                        doGenerateAtCenter = false;
            }

            // Set up some constants, depending on the current movement mode
            {
                if (moveType == 51 || moveType == 52)
                {
                    const_i1 = rand.Next(15) + 1;
                    const_i2 = rand.Next(15) + 1;
                }

                if (moveType == 53)
                {
                    const_i1 = gl_Height / (rand.Next(20) + 5);
                    const_f1 = 0.03f * (rand.Next(100) + 10);
                }
            }
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo()
        {
            string str = $"Obj = myObj_300\n\n" +
                            $"N = {N}\n" +
                            $"renderDelay = {renderDelay}\n" +
                            $"moveType = {moveType}\n" +
                            $"shapeType = {shapeType}\n" +
                            $"rotationMode = {rotationMode}\n" +
                            $"rotationSubMode = {rotationSubMode}\n" +
                            $"colorMode = {colorMode}\n" +
                            $"maxSize = {maxSize}\n" +
                            $"const_i1 = {const_i1}\n" +
                            $"const_i2 = {const_i2}\n" +
                            $"const_f1 = {const_f1}\n" +
                            $"const_f2 = {const_f2}\n";
            return str;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void generateNew()
        {
            x = rand.Next(gl_Width);
            y = rand.Next(gl_Height);

            // Additional set up for some specific moving modes
            {
                // Let gravity-based particles sometimes be generated higher than the top of the screen
                if (moveType == 6)
                {
                    y = rand.Next(gl_Height + 333) - 333;
                }

                // For sideways/vertical moving, generate particles offscreen as well
                if (moveType == 24)
                {
                    x = rand.Next(gl_Width + 666) - 333;
                }

                if (moveType == 25)
                {
                    y = rand.Next(gl_Height + 666) - 333;
                }
            }

            colorPicker.getColor(x, y, ref R, ref G, ref B);

            A = (float)rand.NextDouble() + 0.1f;

            shape = shapeType;
            lineTh = rand.Next(2) + 1;

            lifeCounter = 0;
            lifeMax = rand.Next(333) + 33;

            objN = rand.Next(maxParticles - 4) + 5;

            int max = rand.Next(5000) + 1000;

            for (int i = 0; i < objN; i++)
            {
                var obj = structsList[i];

                obj.isFirstIteration = true;

                // Generate every particle the the center of the screen or at the position of our object
                if (doGenerateAtCenter)
                {
                    obj.x = x0;
                    obj.y = y0;

                    if (moveType == 6)
                    {
                        obj.y = 111;
                    }
                }
                else
                {
                    obj.x = x;
                    obj.y = y;
                }

                obj.r = rand.Next(maxSize) + 2;

                obj.dx = 0.001f * (rand.Next(max) - max/2);
                obj.dy = 0.001f * (rand.Next(max) - max/2);
                obj.a = (float)rand.NextDouble() + 0.33f;

                obj.angle = 0;
                obj.dAngle = rotationMode < 2 ? 0.001f * rand.Next(111) * myUtils.randomSign(rand) : 0;

                obj.time = 0 + 0.001f * rand.Next(1111);
                obj.dTime = 0.001f * rand.Next(111);

                // There will be no rotation, but the angle is set to non-zero
                if (rotationMode == 3)
                {
                    obj.angle = (float)rand.NextDouble() * 111;
                }

                obj.dr = (radiusMode == 0) ? 0.0005f * (rand.Next(100)+1) : 0;
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            lifeCounter++;

            if (lifeCounter > lifeMax)
            {
                int liveCnt = 0;

                for (int i = 0; i < objN; i++)
                {
                    var obj = structsList[i];

                    obj.x += obj.dx;
                    obj.y += obj.dy;
                    obj.a -= 0.005f;

                    obj.angle += obj.dAngle;
                    obj.time += obj.dTime;
                    obj.r += obj.dr;

/*
                    if (obj.y > gl_Height && moveType != 6)
                        obj.a = 0;
*/
                    liveCnt += obj.a > 0 ? 1 : 0;

                    switch (moveType)
                    {
                        // Const speed
                        case 0:
                        case 1:
                            break;

                        // Acceleration -- generic
                        case 2:
                        case 3:
                            obj.dx *= 1.005f;
                            obj.dy *= 1.005f;
                            break;

                        // Acceleration -- Radius related (the smaller -- the faster)
                        case 4:
                        case 5:
                            obj.dx *= (1.05f - obj.r / 120) * 0.99f;
                            obj.dy *= (1.05f - obj.r / 120) * 0.99f;
                            break;

                        // Gravity
                        case 6:
                            obj.dx *= 0.999f;
                            obj.dy += 0.0033f * rand.Next(gravityRate);

                            if (obj.y >= gl_Height)
                            {
                                if (rand.Next(5) == 0)
                                    obj.a = 0;

                                obj.dy *= -0.5f;
                            }
                            break;

                        // Deceleration -- generic
                        case 7:
                        case 8:
                            obj.dx *= 0.99f;
                            obj.dy *= 0.99f;
                            break;

                        // Deceleration -- Radius related (the smaller -- the faster)
                        case 9:
                        case 10:
                            obj.dx *= (1.0f - obj.r / 200);
                            obj.dy *= (1.0f - obj.r / 200);
                            break;

                        // Deceleration -- Radius related (the smaller -- the slower)
                        case 11:
                        case 12:
                            obj.dx *= (1.0f - 0.1f / obj.r);
                            obj.dy *= (1.0f - 0.1f / obj.r);
                            break;

                        // Move Sideways -- generic
                        case 13:
                            obj.dy = 0;
                            break;

                        // Move Sideways -- X-acceleration
                        case 14:
                            obj.dy = 0;
                            obj.dx *= 1.005f;
                            break;

                        // Move Sideways -- Radius related (the smaller -- the faster)
                        case 15:
                            obj.dy *= 0.1f;
                            obj.dx *= (1.05f - obj.r / 120);
                            break;

                        // Move Sideways -- Y-strong-deceleration
                        case 16:
                            obj.dy *= 0.95f;
                            break;

                        // Move up or down -- generic
                        case 17:
                            obj.dx = 0;
                            break;

                        // Move up or down -- larger shapes go up, smaller shapes go down
                        case 18:
                            obj.dx = 0;
                            obj.dy += (0.05f - obj.r / 100);
                            break;

                        // Move up or down -- larger shapes go up, smaller shapes go down + some X-axis dispersion
                        case 19:
                        case 20:
                            obj.dx *= 0.75f;
                            obj.dy += (0.05f - obj.r / 100);
                            break;

                        // Move in a cross fashion
                        case 21:
                            if (System.Math.Abs(obj.dx) >= System.Math.Abs(obj.dy))
                                obj.dy = 0;
                            else
                                obj.dx = 0;
                            break;

                        // dx = const * dy: the const is common between all the particles
                        case 22:
                            if (obj.isFirstIteration)
                            {
                                obj.isFirstIteration = false;
                                obj.dx = const_f1 * obj.dy;
                            }
                            break;

                        // dx = const * dy: each particle has its own const
                        case 23:
                            if (obj.isFirstIteration)
                            {
                                obj.isFirstIteration = false;
                                obj.dx = (float)rand.NextDouble() * obj.dy * myUtils.randomSign(rand);
                            }
                            break;

                        // Add sideways moving component
                        case 24:
                            obj.x += const_f1 * 10;
                            break;

                        // Add vertical moving component
                        case 25:
                            obj.y += const_f1 * 10;
                            break;

                        // Add harmonic motion - y-axis-1
                        case 26:
                            obj.dy = (float)System.Math.Sin(obj.x / const_i1);
                            break;

                        // Add harmonic motion - y-axis-2
                        case 27:
                            obj.dy = (float)System.Math.Sin(obj.y / const_i1);
                            break;

                        // Add harmonic motion - y-axis-3
                        case 28:
                            obj.dy = (float)System.Math.Sin(obj.x / const_i1) * (float)rand.NextDouble();
                            break;

                        // Add harmonic motion - y-axis-4
                        case 29:
                            obj.dy = (float)System.Math.Sin(obj.y / const_i1) * (float)rand.NextDouble();
                            break;

                        // Add harmonic motion - x-axis-1
                        case 30:
                            obj.dx = (float)System.Math.Sin(obj.y / const_i1);
                            break;

                        // Add harmonic motion - x-axis-2
                        case 31:
                            obj.dx = (float)System.Math.Sin(obj.x / const_i1);
                            break;

                        // Add harmonic motion - x-axis-3
                        case 32:
                            obj.dx = (float)System.Math.Sin(obj.y / const_i1) * (float)rand.NextDouble();
                            break;

                        // Add harmonic motion - x-axis-4
                        case 33:
                            obj.dx = (float)System.Math.Sin(obj.x / const_i1) * (float)rand.NextDouble();
                            break;

                        // Sideways movement in 2 streams
                        case 34:
                            obj.x += (obj.y > gl_Height/2) ? const_f1 * 10 : -const_f1 * 10;
                            break;

                        // Ever increasing rotation speed
                        case 35:
                            obj.dAngle += 0.005f;
                            break;

                        // Harmonic to int - 1
                        case 36:
                            if (const_f2 == 0)
                                const_f2 = 200.0f + rand.Next(100);

                            obj.dx += ((int)(Math.Sin(obj.x) * 500)) / const_f2;
                            obj.dy += ((int)(Math.Sin(obj.y) * 500)) / const_f2;
                            break;

                        // Harmonic to int - 2
                        case 37:
                            if (const_f2 == 0)
                                const_f2 = 200.0f + rand.Next(100);

                            obj.x += ((int)(Math.Sin(obj.x) * 500)) / const_f2;
                            obj.y += ((int)(Math.Sin(obj.y) * 500)) / const_f2;
                            break;

                        // Random changes in dx, dy
                        case 38:
                            obj.dx += 0.001f * rand.Next(333) * myUtils.randomSign(rand);
                            obj.dy += 0.001f * rand.Next(333) * myUtils.randomSign(rand);
                            break;

                        // Vertical oscillation -- global
                        case 39:
                            obj.dx *= 0.75f;
                            obj.dy += (rand.Next(10) + 1) * (obj.y > y0 ? -0.1f : 0.1f);
                            break;

                        // Horizontal oscillation -- global
                        case 40:
                            obj.dx += (rand.Next(10) + 1) * (obj.x > x0 ? -0.1f : 0.1f);
                            obj.dy *= 0.75f;
                            break;

                        // Horizontal + Vertical oscillation -- global
                        case 41:
                            if (doGenerateAtCenter)
                            {
                                obj.dx += (rand.Next(1111) + 1) * (obj.x > x0 ? -0.01f : 0.01f);
                                obj.dy += (rand.Next(1111) + 1) * (obj.y > y0 ? -0.01f : 0.01f);
                            }
                            else
                            {
                                obj.dx += (rand.Next(100) + 1) * (obj.x > x0 ? -0.01f : 0.01f);
                                obj.dy += (rand.Next(100) + 1) * (obj.y > y0 ? -0.01f : 0.01f);
                            }
                            renderDelay = 11;
                            break;

                        // Sinking/floating, where the weight is the particle's color
                        case 42:
                            obj.dx *= (float)rand.NextDouble();

                            if ((R + G + B) * obj.a * 2.0f > 1.0f)
                            {
                                obj.dy *= obj.dy > 0 ? 1 : -1;
                            }
                            else
                            {
                                obj.dy *= obj.dy < 0 ? 1 : -1;
                            }
                            break;

                        // Sinking/sticking, where the weight is the particle's color
                        case 43:
                            obj.dx *= (float)rand.NextDouble();

                            if ((R + G + B) * obj.a > 1.0f)
                            {
                                obj.dy *= obj.dy > 0 ? 1 : -1;
                            }
                            else
                            {
                                obj.dy = 0;
                            }
                            break;

                        // Vertical oscillation -- local 1
                        case 44:
                            obj.dx *= 0.85f;
                            obj.dy += (rand.Next(333) + 1) * (obj.y > y ? -0.01f : 0.01f);
                            renderDelay = 11;
                            break;

                        // Vertical oscillation -- local 2
                        case 45:
                            obj.dx *= 0.999f;
                            obj.dy += (rand.Next(333) + 1) * (obj.y > y ? -0.01f : 0.01f);
                            renderDelay = 11;
                            break;

                        // Horizontal oscillation -- local 1
                        case 46:
                            obj.dy *= 0.85f;
                            obj.dx += (rand.Next(333) + 1) * (obj.x > x ? -0.01f : 0.01f);
                            renderDelay = 11;
                            break;

                        // Horizontal oscillation -- local 2
                        case 47:
                            obj.dy *= 0.999f;
                            obj.dx += (rand.Next(333) + 1) * (obj.x > x ? -0.01f : 0.01f);
                            renderDelay = 11;
                            break;

                        // Horizontal + Vertical oscillation -- local
                        case 48:
                            obj.dx += (rand.Next(500) + 1) * (obj.x > x ? -0.01f : 0.01f);
                            obj.dy += (rand.Next(500) + 1) * (obj.y > y ? -0.01f : 0.01f);
                            renderDelay = 11;
                            break;

                        // Horizontal oscillation -- local + Vertical compression
                        case 49:
                            obj.dy *= 0.95f;
                            obj.dx += (rand.Next(500) + 1) * (obj.x > x ? -0.01f : 0.01f);
                            obj.y -= (obj.y - gl_Height/2) / 50.0f;
                            break;

                        // Horizontal + Vertical oscillation -- local + Vertical compression + Horizontal compression
                        case 50:
                            obj.dx += (rand.Next(500) + 1) * (obj.x > x ? -0.01f : 0.01f);
                            obj.dy += (rand.Next(500) + 1) * (obj.y > y ? -0.01f : 0.01f);
                            obj.x -= (obj.x - gl_Width /2) / 10.0f;
                            obj.y -= (obj.y - gl_Height/2) / 10.0f;
                            break;

                        // Vertical/Horizontal movement, taking random turns -- keep initial sign
                        case 51:
                            if (obj.isFirstIteration)
                            {
                                obj.isFirstIteration = false;

                                obj.i1 = obj.dx > 0 ? 1 : -1;   // keep the sign of dx and dy
                                obj.i2 = obj.dy > 0 ? 1 : -1;
                                obj.i3 = rand.Next(22) + 11;
                                obj.i3 *= const_i1;

                                if (myUtils.randomBool(rand))
                                    obj.dx = 0;
                                else
                                    obj.dy = 0;
                            }

                            if (myUtils.randomChance(rand, 0, obj.i3))
                            {
                                float obj_dz = obj.dx;
                                obj.dx = (float)Math.Abs(obj.dy) * obj.i1;
                                obj.dy = (float)Math.Abs(obj_dz) * obj.i2;
                            }
                            break;

                        // Vertical/Horizontal movement, taking random turns -- sign randomized as well
                        case 52:
                            if (obj.isFirstIteration)
                            {
                                obj.isFirstIteration = false;

                                obj.i1 = obj.dx > 0 ? 1 : -1;   // keep the sign of dx and dy
                                obj.i2 = obj.dy > 0 ? 1 : -1;
                                obj.i3 = rand.Next(22) + 11;
                                obj.i3 *= const_i1;

                                if (myUtils.randomBool(rand))
                                    obj.dx = 0;
                                else
                                    obj.dy = 0;
                            }

                            if (myUtils.randomChance(rand, 0, obj.i3))
                            {
                                if (myUtils.randomChance(rand, 0, const_i2))
                                    obj.i1 *= -1;

                                if (myUtils.randomChance(rand, 0, const_i2))
                                    obj.i2 *= -1;

                                float obj_dz = obj.dx;
                                obj.dx = (float)Math.Abs(obj.dy) * obj.i1;
                                obj.dy = (float)Math.Abs(obj_dz) * obj.i2;
                            }
                            break;

                        // Sideways movement in n streams (similar to case 34)
                        case 53:
                            int pos = (int)(obj.y / const_i1);
                            obj.x += (pos % 2 == 0) ? const_f1 : -const_f1;
                            break;

                        case 54:
                            testCase:
                            break;

                        // -----------------------------------------------------

                        case 3333:
                            goto testCase;

                            if (lifeCounter - lifeMax < 100)
                            {
                                obj.a += 0.0075f;
                            }
                            else
                            {
                                float dist = (float)((x - obj.x)*(x - obj.x) + (y - obj.y) * (y - obj.y));

                                if (dist > 10000)
                                {
                                    //obj.dy += 0.1f;
                                }

                                //obj.x += (float)Math.Sin(lifeCounter*0.01f) * 2;
                                //obj.y += (float)Math.Cos(lifeCounter*0.01f) * 2;

                                //obj.x = x + (float)Math.Sin(obj.time/10) * dist;
                                //obj.y = y + (float)Math.Cos(obj.time/10) * dist;

                                //obj.x += (float)Math.Sin(obj.time / 10) * 3;
                                //obj.y += (float)Math.Cos(obj.time / 10) * 3;

                                //obj.x += (float)Math.Sin(obj.time / 10) * 1;
                                //obj.y += (float)Math.Cos(obj.time / 10) * 1;

                            }

                            break;
                    }

                    // --- Apply some generic post-effects ---

                    // Push particles out of the centeral circle;
                    // Randomly applies to any moveType
                    // todo: optimize this, it seems to be lagging when the qty of particles is quite large
                    if (doUseCenterRepel)
                    {
                        float radius = 500.0f;

                        if (obj.x > x0 - radius && obj.x < x0 + radius && obj.y > y0 - radius && obj.y < y0 + radius)
                        {
                            float distSq = (x0 - obj.x) * (x0 - obj.x) + (y0 - obj.y) * (y0 - obj.y);

                            if (distSq < radius*radius)
                            {
                                float dx = (obj.x - x0) / (distSq / 2 / radius);
                                float dy = (obj.y - y0) / (distSq / 2 / radius);

                                obj.x += dx;
                                obj.y += dy;
                            }
                        }
                    }

                    // Fast explosion, then abrupt stop and slow motion;
                    // Randomly applies to any moveType
                    if (fastExplosion == 0)
                    {
                        if (lifeCounter < lifeMax + 66)
                        {
                            obj.dx *= 1.025f;
                            obj.dy *= 1.025f;
                        }

                        if (lifeCounter == lifeMax + 66)
                        {
                            obj.dx /= 3;
                            obj.dy /= 3;
                        }

                        if (lifeCounter == lifeMax + 67)
                        {
                            obj.dx /= 3;
                            obj.dy /= 3;
                        }

                        if (lifeCounter > lifeMax + 67)
                        {
                            obj.dx *= 1.005f;
                            obj.dy *= 1.005f;
                            obj.r -= 0.01f;
                        }
                    }

                    if (doUseBorderRepel)
                    {
                        float repelRate = 666.0f;

                        // Repel from side borders
                        obj.x -= repelRate / (gl_Width - obj.x);
                        obj.x += repelRate / (obj.x);

                        obj.y -= repelRate / (gl_Height - obj.y);
                        obj.y += repelRate / (obj.y);
                    }
                }

                if (liveCnt == 0)
                {
                    generateNew();
                }
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Show()
        {
            if (lifeCounter > lifeMax)
            {
                switch (shape)
                {
                    case 0:
                        glLineWidth(lineTh);
                        myPrimitive._Hexagon.SetColor(R, G, B, 1.0f);

                        for (int i = 0; i < objN; i++)
                        {
                            var obj = structsList[i];

                            if (obj.a > 0)
                            {
                                myPrimitive._Hexagon.SetAngle(obj.angle);

                                if (doFillShapes)
                                {
                                    myPrimitive._Hexagon.SetColorA(obj.a/2);
                                    myPrimitive._Hexagon.Draw(obj.x, obj.y, obj.r, true);
                                }

                                myPrimitive._Hexagon.SetColorA(obj.a);
                                myPrimitive._Hexagon.Draw(obj.x, obj.y, obj.r, false);
                            }
                        }
                        break;

                    case 1:
                        glLineWidth(lineTh);
                        myPrimitive._Triangle.SetColor(R, G, B, 1.0f);

                        for (int i = 0; i < objN; i++)
                        {
                            var obj = structsList[i];

                            if (obj.a > 0)
                            {
                                myPrimitive._Triangle.SetAngle(obj.angle);

                                if (doFillShapes)
                                {
                                    myPrimitive._Triangle.SetColorA(obj.a/2);
                                    myPrimitive._Triangle.Draw(obj.x, obj.y - obj.r, obj.x - 5 * obj.r / 6, obj.y + obj.r / 2, obj.x + 5 * obj.r / 6, obj.y + obj.r / 2, true);
                                }

                                myPrimitive._Triangle.SetColorA(obj.a);
                                myPrimitive._Triangle.Draw(obj.x, obj.y - obj.r, obj.x - 5 * obj.r / 6, obj.y + obj.r / 2, obj.x + 5 * obj.r / 6, obj.y + obj.r / 2, false);
                            }
                        }
                        break;

                    case 2:
                        glLineWidth(lineTh);
                        myPrimitive._Pentagon.SetColor(R, G, B, 1.0f);

                        for (int i = 0; i < objN; i++)
                        {
                            var obj = structsList[i];

                            if (obj.a > 0)
                            {
                                myPrimitive._Pentagon.SetAngle(obj.angle);

                                if (doFillShapes)
                                {
                                    myPrimitive._Pentagon.SetColorA(obj.a/2);
                                    myPrimitive._Pentagon.Draw(obj.x, obj.y, obj.r, true);
                                }

                                myPrimitive._Pentagon.SetColorA(obj.a);
                                myPrimitive._Pentagon.Draw(obj.x, obj.y, obj.r, false);
                            }
                        }
                        break;

                    case 3:
                        glLineWidth(lineTh);
                        myPrimitive._Rectangle.SetColor(R, G, B, 1.0f);

                        for (int i = 0; i < objN; i++)
                        {
                            var obj = structsList[i];

                            if (obj.a > 0)
                            {
                                myPrimitive._Rectangle.SetAngle(obj.angle);

                                if (doFillShapes)
                                {
                                    myPrimitive._Rectangle.SetColorA(obj.a/2);
                                    myPrimitive._Rectangle.Draw(obj.x - obj.r, obj.y - obj.r, 2 * obj.r, 2 * obj.r, true);
                                }

                                myPrimitive._Rectangle.SetColorA(obj.a);
                                myPrimitive._Rectangle.Draw(obj.x - obj.r, obj.y - obj.r, 2 * obj.r, 2 * obj.r, false);
                            }
                        }
                        break;

                    case 4:
                        glLineWidth(1);
                        myPrimitive._Ellipse.SetColor(R, G, B, 1.0f);

                        for (int i = 0; i < objN; i++)
                        {
                            var obj = structsList[i];

                            if (obj.a > 0)
                            {
                                if (doFillShapes)
                                {
                                    myPrimitive._Ellipse.SetColorA(obj.a/2);
                                    myPrimitive._Ellipse.Draw(obj.x - obj.r, obj.y - obj.r, 2 * obj.r, 2 * obj.r, true);
                                }

                                myPrimitive._Ellipse.SetColorA(obj.a);
                                myPrimitive._Ellipse.Draw(obj.x - obj.r, obj.y - obj.r, 2 * obj.r, 2 * obj.r, false);
                            }
                        }
                        break;

                    // Instanced squares
                    case 5:
                        var rectInst = inst as myRectangleInst;

                        for (int i = 0; i < objN; i++)
                        {
                            var obj = structsList[i];

                            if (obj.a > 0)
                            {
                                if (colorMode == 1)
                                    colorPicker.getColor(obj.x, obj.y, ref R, ref G, ref B);

                                rectInst.setInstanceCoords(obj.x - obj.r, obj.y - obj.r, 2*obj.r, 2*obj.r);
                                rectInst.setInstanceColor(R, G, B, obj.a);
                                rectInst.setInstanceAngle(obj.angle);
                            }
                        }
                        break;

                    // Instanced triangles
                    case 6:
                        var triangleInst = inst as myTriangleInst;

                        for (int i = 0; i < objN; i++)
                        {
                            var obj = structsList[i];

                            if (obj.a > 0)
                            {
                                if (colorMode == 1)
                                    colorPicker.getColor(obj.x, obj.y, ref R, ref G, ref B);

                                triangleInst.setInstanceCoords(obj.x, obj.y, 2*obj.r, obj.angle);
                                triangleInst.setInstanceColor(R, G, B, obj.a);
                            }
                        }
                        break;

                    // Instanced circles
                    case 7:
                        var ellipseInst = inst as myEllipseInst;

                        for (int i = 0; i < objN; i++)
                        {
                            var obj = structsList[i];

                            if (obj.a > 0)
                            {
                                if (colorMode == 1)
                                    colorPicker.getColor(obj.x, obj.y, ref R, ref G, ref B);

                                ellipseInst.setInstanceCoords(obj.x, obj.y, 2*obj.r, obj.angle);
                                ellipseInst.setInstanceColor(R, G, B, obj.a);
                            }
                        }
                        break;

                    // Instanced pentagons
                    case 8:
                        var pentagonInst = inst as myPentagonInst;

                        for (int i = 0; i < objN; i++)
                        {
                            var obj = structsList[i];

                            if (obj.a > 0)
                            {
                                if (colorMode == 1)
                                    colorPicker.getColor(obj.x, obj.y, ref R, ref G, ref B);

                                pentagonInst.setInstanceCoords(obj.x, obj.y, 2*obj.r, obj.angle);
                                pentagonInst.setInstanceColor(R, G, B, obj.a);
                            }
                        }
                        break;

                    // Instanced hexagons
                    case 9:
                        var hexagonInst = inst as myHexagonInst;

                        for (int i = 0; i < objN; i++)
                        {
                            var obj = structsList[i];

                            if (obj.a > 0)
                            {
                                if (colorMode == 1)
                                    colorPicker.getColor(obj.x, obj.y, ref R, ref G, ref B);

                                hexagonInst.setInstanceCoords(obj.x, obj.y, 2*obj.r, obj.angle);
                                hexagonInst.setInstanceColor(R, G, B, obj.a);
                            }
                        }
                        break;
                }
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Process(Window window)
        {
            uint cnt = 0;

            myPrimitive.init_Triangle();
            myPrimitive.init_Rectangle();
            myPrimitive.init_Pentagon();
            myPrimitive.init_Hexagon();
            myPrimitive.init_Ellipse();

            switch (shapeType)
            {
                case 5:
                    myPrimitive.init_RectangleInst(N * maxParticles);
                    myPrimitive._RectangleInst.setRotationMode(rotationSubMode);
                    inst = myPrimitive._RectangleInst;
                    break;

                case 6:
                    myPrimitive.init_TriangleInst(N * maxParticles);
                    myPrimitive._TriangleInst.setRotationMode(rotationSubMode);
                    inst = myPrimitive._TriangleInst;
                    break;

                case 7:
                    myPrimitive.init_EllipseInst(N * maxParticles);
                    myPrimitive._EllipseInst.setRotationMode(rotationSubMode);
                    inst = myPrimitive._EllipseInst;
                    break;

                case 8:
                    myPrimitive.init_PentagonInst(N * maxParticles);
                    myPrimitive._PentagonInst.setRotationMode(rotationSubMode);
                    inst = myPrimitive._PentagonInst;
                    break;

                case 9:
                    myPrimitive.init_HexagonInst(N * maxParticles);
                    myPrimitive._HexagonInst.setRotationMode(rotationSubMode);
                    inst = myPrimitive._HexagonInst;
                    break;
            }

            while (list.Count < N)
            {
                list.Add(new myObj_300());
            }

            if (doClearBuffer == false)
            {
                glDrawBuffer(GL_FRONT_AND_BACK);
            }

            // https://stackoverflow.com/questions/25548179/opengl-alpha-blending-suddenly-stops

            while (!Glfw.WindowShouldClose(window))
            {
                cnt++;

                processInput(window);

                // Swap fore/back framebuffers, and poll for operating system events.
                Glfw.SwapBuffers(window);
                Glfw.PollEvents();

                // Dim the screen constantly
                if (doClearBuffer == false)
                {
                    myPrimitive._Rectangle.SetAngle(0);
                    // Shift background color just a bit, to hide long lasting traces of shapes
                    myPrimitive._Rectangle.SetColor(rand.Next(5) * 0.01f, rand.Next(5) * 0.01f, rand.Next(5) * 0.01f, dimAlpha);
                    myPrimitive._Rectangle.Draw(0, 0, gl_Width, gl_Height, true);
                }
                else
                {
                    glClearColor(0, 0, 0, 1);
                    glClear(GL_COLOR_BUFFER_BIT);
                }

                if (doUseInstancing)
                {
                    inst.ResetBuffer();

                    for (int i = 0; i < list.Count; i++)
                    {
                        var obj = list[i] as myObj_300;
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
                else
                {
                    for (int i = 0; i < list.Count; i++)
                    {
                        var obj = list[i] as myObj_300;
                        obj.Show();
                        obj.Move();
                    }
                }

                System.Threading.Thread.Sleep(renderDelay);
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        unsafe void readPixel(int x, int y)
        {
            float[] pixel = new float[4];

            fixed (float * ppp = &pixel[0])
            {
                glReadPixels(x, y, 1, 1, GL_RGBA, GL_FLOAT, ppp);
            }
        }
    }
};
