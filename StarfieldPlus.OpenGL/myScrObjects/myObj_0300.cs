﻿using GLFW;
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
    public class myObj_0300 : myObject
    {
        // Priority
        public static int Priority => 35;
		public static System.Type Type => typeof(myObj_0300);

        private class myObj_0300_Particle
        {
            public bool isFirstIteration = false;
            public float x, y, r, dx, dy, dr, a, angle, dAngle, time, dTime;
            public int i1, i2, i3;
        };

        // ---------------------------------------------------------------------------------------------------------------

        private float x, y, R, G, B, A, lineTh;
        private int shape = 0, lifeCounter = 0, lifeMax = 0, objN = 0;

        private List<myObj_0300_Particle> structsList = null;

        private static bool doFillShapes = false, doUseCenterRepel = false,
                            doUseBorderRepel = false, doGenerateAtCenter = false, doAddObjGradually = false, doShowConnections = false;
        private static int N = 1, gravityRate = 0, maxParticles = 25, maxSize = 6;
        private static int shapeType = 0, moveType = 0, radiusMode = 0, fastExplosion = 0, rotationMode = 0, rotationSubMode = 0, colorMode = 0;

        private static float const_f1 = 0, const_f2 = 0;
        private static int   const_i1 = 0, const_i2 = 0;

        private static myScreenGradient grad = null;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_0300()
        {
            if (colorPicker == null)
            {
                init();
            }

            structsList = new List<myObj_0300_Particle>();

            for (int i = 0; i < maxParticles; i++)
            {
                structsList.Add(new myObj_0300_Particle());
            }

            generateNew();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time initialization
        private void init()
        {
            colorPicker = new myColorPicker(gl_Width, gl_Height);
            list = new List<myObject>();

            doClearBuffer      = myUtils.randomChance(rand, 2, 3);
            doFillShapes       = myUtils.randomBool(rand);
            doAddObjGradually  = myUtils.randomBool(rand);
            doUseCenterRepel   = myUtils.randomChance(rand, 1, 11);
            doUseBorderRepel   = myUtils.randomChance(rand, 1, 11);
            doGenerateAtCenter = myUtils.randomChance(rand, 1, 11);
            doShowConnections  = myUtils.randomChance(rand, 1, 3);

            // In case the colorPicker has an underlying image, we might want to draw every particle using the image color at this particular point
            if (colorPicker.isImage())
            {
                if (myUtils.randomChance(rand, 1, 7))
                {
                    colorMode = 1;
                }
            }

            // rotationMode: 0, 1 = rotation; 2 = no rotation, angle is 0; 3 = no rotation, angle is not 0
            rotationMode  = rand.Next(4);
            gravityRate   = rand.Next(101) + 1;
            shapeType     = rand.Next(5);
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
            {
                switch (rand.Next(8))
                {
                    case 0:
                        N = rand.Next(10) + 5;
                        break;

                    case 1:
                    case 2:
                        N = rand.Next(33) + 100;
                        break;

                    case 3:
                    case 4:
                        N = rand.Next(333) + 1000;
                        break;

                    case 5:
                    case 6:
                        N = rand.Next(3333) + 1000;
                        break;

                    case 7:
                        N = rand.Next(3333) + 3333;
                        break;
                }
            }

            // Set max size of a particle:
            if (myUtils.randomChance(rand, 1, 3))
            {
                maxSize += rand.Next(66);
            }

            renderDelay = 1;

#if false
            shapeType = 0;  // instanced square
            shapeType = 1;  // instanced triangle
            shapeType = 2;  // instanced circle
            shapeType = 3;  // instanced pentagon
            shapeType = 4;  // instanced hexagon
            //doClearBuffer = true;
            //doClearBuffer = false;
            //radiusMode = 2;
            //moveType = 3333;
            N = 3333;
#endif

            // Generation at center of the screen does not look right for some of the move modes;
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

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            height = 700;

            string str = $"Obj = {Type}\n\n"                             	      +
                            $"N = {list.Count} of {N} x {maxParticles}\n"         +
                            $"doClearBuffer = {doClearBuffer}\n"                  +
                            $"grad.Opacity = {myUtils.fStr(grad.GetOpacity())}\n" +
                            $"doShowConnections = {doShowConnections}\n"          +
                            $"moveType = {moveType}\n"                            +
                            $"shapeType = {shapeType}\n"                          +
                            $"rotationMode = {rotationMode}\n"                    +
                            $"rotationSubMode = {rotationSubMode}\n"              +
                            $"colorMode = {colorMode}\n"                          +
                            $"maxSize = {maxSize}\n"                              +
                            $"doGenerateAtCenter = {doGenerateAtCenter}\n"        +
                            $"const_i1 = {const_i1}\n"                            +
                            $"const_i2 = {const_i2}\n"                            +
                            $"const_f1 = {myUtils.fStr(const_f1)}\n"              +
                            $"const_f2 = {myUtils.fStr(const_f2)}\n"              +
                            $"renderDelay = {renderDelay}\n"                      +
                            $"file: {colorPicker.GetFileName()}"
                ;
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

            for (int i = 0; i != objN; i++)
            {
                var obj = structsList[i];

                obj.isFirstIteration = true;

                // Generate every particle the the center of the screen or at the position of our object
                if (doGenerateAtCenter)
                {
                    obj.x = gl_x0;
                    obj.y = gl_y0;

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

                for (int i = 0; i != objN; i++)
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
                            if (obj.y > gl_Height / 2 + 100)
                                obj.x += const_f1 * 10;
                            else if (obj.y < gl_Height / 2 - 100)
                                obj.x -= const_f1 * 10;
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
                            obj.dy += (rand.Next(10) + 1) * (obj.y > gl_y0 ? -0.1f : 0.1f);
                            break;

                        // Horizontal oscillation -- global
                        case 40:
                            obj.dx += (rand.Next(10) + 1) * (obj.x > gl_x0 ? -0.1f : 0.1f);
                            obj.dy *= 0.75f;
                            break;

                        // Horizontal + Vertical oscillation -- global
                        case 41:
                            if (doGenerateAtCenter)
                            {
                                obj.dx += (rand.Next(1111) + 1) * (obj.x > gl_x0 ? -0.01f : 0.01f);
                                obj.dy += (rand.Next(1111) + 1) * (obj.y > gl_y0 ? -0.01f : 0.01f);
                            }
                            else
                            {
                                obj.dx += (rand.Next(100) + 1) * (obj.x > gl_x0 ? -0.01f : 0.01f);
                                obj.dy += (rand.Next(100) + 1) * (obj.y > gl_y0 ? -0.01f : 0.01f);
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

                            if (myUtils.randomChance(rand, 1, obj.i3))
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

                            if (myUtils.randomChance(rand, 1, obj.i3))
                            {
                                if (myUtils.randomChance(rand, 1, const_i2))
                                    obj.i1 *= -1;

                                if (myUtils.randomChance(rand, 1, const_i2))
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

                        if (obj.x > gl_x0 - radius && obj.x < gl_x0 + radius && obj.y > gl_y0 - radius && obj.y < gl_y0 + radius)
                        {
                            float distSq = (gl_x0 - obj.x) * (gl_x0 - obj.x) + (gl_y0 - obj.y) * (gl_y0 - obj.y);

                            if (distSq < radius*radius)
                            {
                                float dx = (obj.x - gl_x0) / (distSq / 2 / radius);
                                float dy = (obj.y - gl_y0) / (distSq / 2 / radius);

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
                // Render connecting lines
                if (doShowConnections)
                {
                    for (int i = 0; i != objN; i++)
                    {
                        myPrimitive._LineInst.setInstanceCoords(structsList[i].x, structsList[i].y, x, y);
                        myPrimitive._LineInst.setInstanceColor(1, 1, 1, 0.025f);
                    }
                }

                switch (shape)
                {
                    // Instanced squares
                    case 0:
                        for (int i = 0; i != objN; i++)
                        {
                            var obj = structsList[i];

                            if (obj.a > 0)
                            {
                                if (colorMode == 1)
                                    colorPicker.getColor(obj.x, obj.y, ref R, ref G, ref B);

                                myPrimitive._RectangleInst.setInstanceCoords(obj.x - obj.r, obj.y - obj.r, 2*obj.r, 2*obj.r);
                                myPrimitive._RectangleInst.setInstanceColor(R, G, B, obj.a);
                                myPrimitive._RectangleInst.setInstanceAngle(obj.angle);
                            }
                        }
                        break;

                    // Instanced triangles
                    case 1:
                        for (int i = 0; i != objN; i++)
                        {
                            var obj = structsList[i];

                            if (obj.a > 0)
                            {
                                if (colorMode == 1)
                                    colorPicker.getColor(obj.x, obj.y, ref R, ref G, ref B);

                                myPrimitive._TriangleInst.setInstanceCoords(obj.x, obj.y, 2*obj.r, obj.angle);
                                myPrimitive._TriangleInst.setInstanceColor(R, G, B, obj.a);
                            }
                        }
                        break;

                    // Instanced circles
                    case 2:
                        for (int i = 0; i != objN; i++)
                        {
                            var obj = structsList[i];

                            if (obj.a > 0)
                            {
                                if (colorMode == 1)
                                    colorPicker.getColor(obj.x, obj.y, ref R, ref G, ref B);

                                myPrimitive._EllipseInst.setInstanceCoords(obj.x, obj.y, 2*obj.r, obj.angle);
                                myPrimitive._EllipseInst.setInstanceColor(R, G, B, obj.a);
                            }
                        }
                        break;

                    // Instanced pentagons
                    case 3:
                        for (int i = 0; i != objN; i++)
                        {
                            var obj = structsList[i];

                            if (obj.a > 0)
                            {
                                if (colorMode == 1)
                                    colorPicker.getColor(obj.x, obj.y, ref R, ref G, ref B);

                                myPrimitive._PentagonInst.setInstanceCoords(obj.x, obj.y, 2*obj.r, obj.angle);
                                myPrimitive._PentagonInst.setInstanceColor(R, G, B, obj.a);
                            }
                        }
                        break;

                    // Instanced hexagons
                    case 4:
                        for (int i = 0; i != objN; i++)
                        {
                            var obj = structsList[i];

                            if (obj.a > 0)
                            {
                                if (colorMode == 1)
                                    colorPicker.getColor(obj.x, obj.y, ref R, ref G, ref B);

                                myPrimitive._HexagonInst.setInstanceCoords(obj.x, obj.y, 2*obj.r, obj.angle);
                                myPrimitive._HexagonInst.setInstanceColor(R, G, B, obj.a);
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

            initShapes();

            while (!doAddObjGradually && list.Count < N)
            {
                list.Add(new myObj_0300());
            }


            clearScreenSetup(doClearBuffer, 0.1f);

            if (doClearBuffer == false)
            {
                grad.SetOpacity(0.15f + myUtils.randFloat(rand) * 0.15f);
            }


            // https://stackoverflow.com/questions/25548179/opengl-alpha-blending-suddenly-stops

            while (!Glfw.WindowShouldClose(window))
            {
                int Count = list.Count;

                cnt++;

                processInput(window);

                // Swap fore/back framebuffers, and poll for operating system events.
                Glfw.SwapBuffers(window);
                Glfw.PollEvents();

                // Clear or dim the screen
                grad.Draw();

                // Render Frame
                {
                    inst.ResetBuffer();

                    if (doShowConnections)
                    {
                        myPrimitive._LineInst.ResetBuffer();
                    }

                    for (int i = 0; i != Count; i++)
                    {
                        var obj = list[i] as myObj_0300;
                        obj.Show();
                        obj.Move();
                    }

                    if (doShowConnections)
                    {
                        myPrimitive._LineInst.Draw();
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

                if (doAddObjGradually && list.Count < N)
                {
                    list.Add(new myObj_0300());
                }

                System.Threading.Thread.Sleep(renderDelay);
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void initShapes()
        {
            myPrimitive.init_LineInst(N * maxParticles);
            base.initShapes(shapeType, N * maxParticles, rotationSubMode);

            grad = new myScreenGradient();
            grad.SetRandomColors(rand, 0.2f);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------
    }
};
