﻿using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;


/*
    - Moving particles interconnected with each other
*/


namespace my
{
    public class myObj_310 : myObject
    {
        // ---------------------------------------------------------------------------------------------------------------

        // Static parameters
        private static int N = 0;
        private static int mode = 0, colorMode = 0, shape = 0;
        private static float dimAlpha = 0.05f, t = 0, dt = 0;

        private static short[] prm_i = new short[4];
        private static float[] prm_f = new float[1];
        private static int max = 0, xRad1 = 666, yRad1 = 666, xRad2 = 666, yRad2 = 666, lineMode = 0, lineStyle = 0, slowFactor = 1, axisMode = 0;
        private static bool moveStep = false;
        private static bool doShiftColor = false, doCreateAtOnce = false, isAggregateOpacity = false, isVerticalLine = false, isFastMoving = false, isRandomSize = false,
                            doShowAuxParticles = false;

        // Parameters for auxiliary invisible particles rotating around the center
        private static float X1 = 0, Y1 = 0, X2 = 0, Y2 = 0, t1 = 0, t2 = 0, dt1 = 0, dt2 = 0;

        // Common parameters
        private float x, y, dx, dy, size, r, g, b, a, pt, pdt;
        private int offset1 = 0, offset2 = 0;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_310()
        {
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
            N = rand.Next(500) + 25;
            mode = rand.Next(19);
            //mode = 17;

            doClearBuffer  = myUtils.randomChance(rand, 4, 5);
            doShiftColor   = myUtils.randomChance(rand, 1, 2);
            doCreateAtOnce = myUtils.randomChance(rand, 1, 2);
            colorMode = rand.Next(4);
            lineMode = rand.Next(6);                                                // Interconnection lines drawing mode (affects distance and opacity factor calculation)
            lineStyle = rand.Next(9);                                               // Drawing style for interconnection lines (parallel vs crossed)
            slowFactor = rand.Next(5) + 1;                                          // Slowness factor for dx and/or dy
            axisMode = rand.Next(7);                                                // In a number of modes, will cause only vertical and/or horizontal movement of particles
            max = rand.Next(11) + 3;                                                // Particle size
            shape = rand.Next(5);

            isAggregateOpacity = myUtils.randomChance(rand, 1, 02);                 // Const opacity vs a sum of all particle's connecting line opacities
            isVerticalLine     = myUtils.randomChance(rand, 1, 15);                 // Draw vertical lines
            isFastMoving       = myUtils.randomChance(rand, 1, 02);                 // For large N and when slowFactor > 3, chance to have fast moving particles
            isRandomSize       = myUtils.randomChance(rand, 1, 03);                 // Use particles of different size

            t = 0;                                                                  // Global time
            dt = 0.025f;
            t1 = myUtils.randFloat(rand);                                           // pt1 time
            t2 = myUtils.randFloat(rand);                                           // pt2 time

            xRad1 += myUtils.randomChance(rand, 1, 2) ? 0 : rand.Next(1234);
            xRad2 += myUtils.randomChance(rand, 1, 2) ? 0 : rand.Next(1234);

            X1 = gl_x0 + (float)Math.Sin(t1) * xRad1;                               // pt1 init
            Y1 = gl_y0 + (float)Math.Cos(t1) * yRad1;

            X2 = gl_x0 + (float)Math.Sin(t2) * xRad2;                               // pt2 init
            Y2 = gl_y0 + (float)Math.Cos(t2) * yRad2;

            // Reset parameter values
            {
                for (int i = 0; i < prm_i.Length; i++)
                    prm_i[i] = 0;

                for (int i = 0; i < prm_f.Length; i++)
                    prm_f[i] = 0;
            }

            dimAlpha /= (0.1f + 0.1f * rand.Next(20));

            switch (mode)
            {
                // Particles generate based on position of a point [X1, Y1], which is rotating around the center
                case 08:
                    doCreateAtOnce = false;
                    dt1 = myUtils.randomSign(rand) * myUtils.randFloat(rand, 0.1f) / (8 + rand.Next(3));
                    break;

                case 10:
                    prm_i[0] = (short)rand.Next(2);                                 // Generate particles in-screen or off-screen
                    break;

                case 11:
                    prm_i[0] = (short)(rand.Next(4) + 1);                           // Probability used in this mode; must be 1-2 or 8-9
                    prm_i[0] += (short)((prm_i[0] > 2) ? 5 : 0);
                    break;

                // Particles start as static, but eventually begin accelerating gradually
                case 12:
                    doClearBuffer = true;
                    prm_i[0] = (short)(50 + rand.Next(321));                        // Probability that the particle starts moving / continues accelerating
                    break;

                // Particles generate based on positions of 2 points, [X1, Y1] and [X2, Y2], which both are rotating around the center
                case 13:
                    prm_i[0] = (short)rand.Next(4);                                 // Particles generation mode
                    prm_i[1] = (short)myUtils.randomSign(rand);                     // Sign for dx
                    prm_i[2] = (short)myUtils.randomSign(rand);                     // Sign for dy
                    prm_i[3] = (short)rand.Next(21);                                // Particle speed factor
                    dt1 = myUtils.randFloat(rand, 0.1f) / (8 + rand.Next(3));
                    dt2 = myUtils.randFloat(rand, 0.1f) / (8 + rand.Next(3));
                    dt2 = myUtils.randomChance(rand, 1, 7) ? dt1 : dt2;             // Sometimes dt1 == dt2
                    doCreateAtOnce = false;
                    break;

                // Particles generate at the 4 corners of the screen
                case 14:
                    break;

                // Particles generate offscreen and move vertically / horizontally in a grid-based fashion
                case 15:
                    prm_i[0] = (short)rand.Next(3);                                 // Movement: vertical, horizontal, both
                    prm_i[1] = (short)(rand.Next(200) + 75);                        // Grid interval
                    break;

                // Starfield-like
                case 16:
                    prm_i[0] = (short)rand.Next(6);                                 // Particle initial speed factor
                    prm_i[1] = (short)rand.Next(3);                                 // Particle acceleration mode
                    prm_i[2] = myUtils.randomChance(rand, 2, 3)                     // Generate particles across the whole screen vs within some square in the center
                        ? (short)0
                        : (short)(rand.Next(777) + 111);
                    prm_f[0] = 1.0f + myUtils.randFloat(rand) / 33;                 // Acceleration factor
                    break;

                // Gravitation pull/push towards aux particles
                case 17:
                    doShowAuxParticles = true;
                    prm_i[0] = (short)rand.Next(2);                                 // Initial particle speed is zero/non-zero
                    prm_i[1] = (short)rand.Next(4);                                 // Mass factor sign of aux particles
                    prm_i[2] = (short)rand.Next(2);                                 // Additive / non-additive dx/dy
                    prm_i[3] = (short)(222 + rand.Next(333));                       // Aux particle min rotation radius

                    xRad1 = prm_i[3] + rand.Next(222) + rand.Next(222) + rand.Next(222);
                    yRad1 = prm_i[3] + rand.Next(222) + rand.Next(222) + rand.Next(222);
                    xRad2 = prm_i[3] + rand.Next(222) + rand.Next(222) + rand.Next(222);
                    yRad2 = prm_i[3] + rand.Next(222) + rand.Next(222) + rand.Next(222);

                    dt1 = myUtils.randomSign(rand) * myUtils.randFloat(rand, 0.1f) / (8 + rand.Next(3));
                    dt2 = myUtils.randomSign(rand) * myUtils.randFloat(rand, 0.1f) / (8 + rand.Next(3));
                    break;

                // Particles rotating around the center
                case 18:
                    prm_i[0] = (short)rand.Next(4);                                 // Direction of rotation
                    prm_i[1] = (short)(rand.Next(5)+1 + 10 * (rand.Next(5)+1));     // Radius changing speed (use prm_i[1] % 10 and prm_i[1] / 10 for 2 different values)
                    prm_i[2] = (short)(rand.Next(2));                               // Circular vs elliptic orbit

                    if (prm_i[2] == 0 && myUtils.randomChance(rand, 1, 3))          // For circular orbit, 33% chance to have the same changing speed for both radius offsets
                        prm_i[1] = (short)((rand.Next(5) + 1) * 11);
                    break;
            }

#if false
            N = 4;
            max = 20;
            lineMode = 0;
            lineStyle = 8;
            doClearBuffer = true;
#endif
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            height += 100;

            string str_params = "";
            for (int i = 0; i < prm_i.Length; i++)
            {
                str_params += i == 0 ? $"{prm_i[i]}" : $", {prm_i[i]}";
            }

            string str = $"Obj = myObj_310\n\n" +
                            $"N = {list.Count} of {N}\n" +
                            $"mode = {mode}\n" +
                            $"max = {max}\n" +
                            $"dimAlpha = {dimAlpha}\n" +
                            $"doClearBuffer = {doClearBuffer}\n" +
                            $"isAggregateOpacity = {isAggregateOpacity}\n" +
                            $"isVerticalLine = {isVerticalLine}\n" +
                            $"isFastMoving = {isFastMoving}\n" +
                            $"isRandomSize = {isRandomSize}\n" +
                            $"colorMode = {colorMode}\n" +
                            $"lineMode = {lineMode}\n" +
                            $"lineStyle = {lineStyle}\n" +
                            $"slowFactor = {slowFactor}\n" +
                            $"axisMode = {axisMode}\n" +
                            $"param: [{str_params}]\n\n" +
                            $"file: {colorPicker.GetFileName()}" + 
                            $""
            ;
            return str;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void generateNew()
        {
            x = rand.Next(gl_Width);
            y = rand.Next(gl_Height);

            switch (mode)
            {
                case 00:
                case 01:
                    dx = (rand.Next(111) + 11) * 0.1f * myUtils.randomSign(rand);
                    dy = (rand.Next(111) + 11) * 0.1f * myUtils.randomSign(rand);
                    break;

                case 02:
                case 03:
                case 04:
                    dx = myUtils.randFloat(rand, 0.1f) * (rand.Next(50) + 1) * myUtils.randomSign(rand);
                    dy = myUtils.randFloat(rand, 0.1f) * (rand.Next(50) + 1) * myUtils.randomSign(rand);
                    break;

                case 05:
                    dx = (0.5f + 0.5f * rand.Next(13)) * myUtils.randomSign(rand);
                    dy = (0.5f + 0.5f * rand.Next(13)) * myUtils.randomSign(rand);
                    break;

                case 06:
                case 07:
                    dx = (0.5f + 0.5f * rand.Next(17)) * myUtils.randomSign(rand);
                    dy = (0.5f + 0.5f * rand.Next(17)) * myUtils.randomSign(rand);
                    break;

                case 08:
                    {
                        switch (axisMode)
                        {
                            case 0:
                                y = gl_y0;
                                break;

                            case 1:
                                x = gl_x0;
                                break;

                            default:
                                x = gl_x0;
                                y = gl_y0;
                                break;
                        }

                        float dX = X1 - gl_x0;
                        float dY = Y1 - gl_y0;

                        float dist2 = (float)Math.Sqrt(dX * dX + dY * dY);
                        float sp_dist = (50 + rand.Next(100)) / dist2 / 10;

                        dx = dX * sp_dist;
                        dy = dY * sp_dist;
                    }
                    break;

                case 09:
                    dx = (0.5f + 0.5f * rand.Next(17)) * myUtils.randomSign(rand);
                    dy = (0.5f + 0.5f * rand.Next(17)) * myUtils.randomSign(rand);

                    offset1 = 100 + rand.Next(50 + N);

                    // Is modified into vertical/horizontal pattern a bit below
                    break;

                case 10:
                    offset1 = 100 + rand.Next(50 + N);

                    dx = (0.5f + 0.5f * rand.Next(99)) * myUtils.randomSign(rand);
                    dy = (0.5f + 0.5f * rand.Next(15)) * myUtils.randomSign(rand);

                    if (prm_i[0] == 1)
                    {
                        // generate particles off-screen -- fox x axis only
                        x = (dx > 0) ? -50 : gl_Width + 50;
                    }
                    break;

                case 11:
                    offset1 = 100 + rand.Next(50 + N);

                    dx = (0.5f + 0.5f * rand.Next(17)) * myUtils.randomSign(rand);
                    dy = (0.5f + 0.5f * rand.Next(17)) * myUtils.randomSign(rand);

                    // This translates to (1 out of 10), (2 out of 10), (8 out of 10) or (9 out of 10)
                    if (myUtils.randomChance(rand, prm_i[0], 10))
                    {
                        dx = 0;
                        dy *= 5;
                    }
                    else
                    {
                        dy = 0;
                        dx /= 5;
                    }
                    break;

                case 12:
                    offset1 = 100 + rand.Next(50 + N);
                    dx = dy = 0;
                    break;

                case 13:
                    {
                        offset1 = 100 + rand.Next(50 + N);

                        float dX = X1 - X2;
                        float dY = Y1 - Y2;

                        float dist2 = (float)Math.Sqrt(dX * dX + dY * dY) + 0.0001f;
                        float sp_dist = 1;

                        // Speed
                        switch (prm_i[3])
                        {
                            // random [50 .. 150]
                            case 0: case 1: case 2: case 3: case 4:
                                sp_dist = (50 + rand.Next(100)) / dist2 / 10;
                                break;

                            // const [50, 70, 90, 110, 130, 150]
                            case 5: case 6: case 7: case 8: case 9: case 10:
                                sp_dist = (50 + (prm_i[3] - 5) * 20) / dist2 / 10;
                                break;

                            // const [50, 75, 100, 125 , 150], with 50% chance of half-speed
                            case 11: case 12: case 13: case 14: case 15:
                                sp_dist = (50 + (prm_i[3] - 11) * 25) / dist2 / (10 + 10 * rand.Next(2));
                                break;

                            // Fast particles
                            case 16: case 17: case 18:
                                sp_dist = (200 + prm_i[3] * rand.Next(5)) / dist2 / 10;
                                break;

                            // Even faster particles
                            case 19: case 20:
                                sp_dist = (300 + prm_i[3] * rand.Next(7)) / dist2 / 10;
                                break;
                        }

                        // Position and move direction
                        switch (prm_i[0])
                        {
                            case 0: x = X1; y = Y1; break;
                            case 1: x = X2; y = Y2; break;
                            case 2: x = X1; y = Y2; break;
                            case 3: x = X2; y = Y1; break;
                        }

                        dx = dX * sp_dist * prm_i[1];
                        dy = dY * sp_dist * prm_i[2];
                    }
                    break;

                case 14:
                    offset1 = 100 + rand.Next(50 + N);

                    switch (rand.Next(4))
                    {
                        case 0:
                            x = 0; y = 0;
                            dx = (0.5f + 0.5f * rand.Next(17)) * +1;
                            dy = (0.5f + 0.5f * rand.Next(17)) * +1;
                            break;

                        case 1:
                            x = 0; y = gl_Height;
                            dx = (0.5f + 0.5f * rand.Next(17)) * +1;
                            dy = (0.5f + 0.5f * rand.Next(17)) * -1;
                            break;

                        case 2:
                            x = gl_Width; y = 0;
                            dx = (0.5f + 0.5f * rand.Next(17)) * -1;
                            dy = (0.5f + 0.5f * rand.Next(17)) * +1;
                            break;

                        case 3:
                            x = gl_Width; y = gl_Height;
                            dx = (0.5f + 0.5f * rand.Next(17)) * -1;
                            dy = (0.5f + 0.5f * rand.Next(17)) * -1;
                            break;
                    }
                    break;

                case 15:
                    offset1 = 100 + rand.Next(50 + N);

                    switch (prm_i[0])
                    {
                        // Vertical
                        case 0:
                            dx = 0;
                            dy = (0.5f + 0.5f * rand.Next(17)) * myUtils.randomSign(rand);

                            x -= x % prm_i[1];
                            y = myUtils.signOf(dy) > 0 ? -50 : gl_Height + 50;

                            x += (gl_Width % prm_i[1]) / 2;
                            break;

                        // Horizontal
                        case 1:
                            dx = (0.5f + 0.5f * rand.Next(17)) * myUtils.randomSign(rand);
                            dy = 0;

                            y -= y % prm_i[1];
                            x = myUtils.signOf(dx) > 0 ? -50 : gl_Width + 50;

                            y += (gl_Height % prm_i[1]) / 2;
                            break;

                        // Both
                        case 2:
                            if (myUtils.randomChance(rand, 1, 2))
                                goto case 0;
                            else
                                goto case 1;
                            break;
                    }
                    break;

                case 16:
                    {
                        offset1 = 100 + rand.Next(50 + N);

                        if (prm_i[2] > 0)
                        {
                            x = rand.Next(prm_i[2]) + gl_x0 - prm_i[2]/2;
                            y = rand.Next(prm_i[2]) + gl_y0 - prm_i[2]/2;
                        }

                        float dX = x - gl_x0;
                        float dY = y - gl_y0;

                        float dist2 = (float)Math.Sqrt(dX * dX + dY * dY) + 0.0001f;
                        float sp_dist = 1;

                        switch (prm_i[0])
                        {
                            case 0: case 1: case 2:
                                sp_dist = (50 + prm_i[0] * 25) / dist2 / 10;
                                break;

                            case 3: case 4: case 5:
                                sp_dist = (50 + rand.Next(100)) / dist2 / 10;
                                break;
                        }

                        dx = dX * sp_dist;
                        dy = dY * sp_dist;
                    }
                    break;

                case 17:
                    offset1 = 100 + rand.Next(50 + N);

                    switch (prm_i[0])
                    {
                        case 0:
                            dx = dy = 0;
                            break;

                        case 1:
                            dx = (0.5f + 0.5f * rand.Next(17)) * myUtils.randomSign(rand);
                            dy = (0.5f + 0.5f * rand.Next(17)) * myUtils.randomSign(rand);
                            break;
                    }
                    break;

                case 18:
                    x = gl_x0;
                    y = gl_y0;

                    offset1 = rand.Next(2 * gl_Height / 3);
                    offset2 = prm_i[2] == 0 ? offset1 : rand.Next(2 * gl_Height / 3);
                    pt = myUtils.randFloat(rand) * rand.Next(123);
                    pdt = myUtils.randFloat(rand) / 33;

                    // Direction of rotation
                    switch (prm_i[0])
                    {
                        case 0: pdt *= +1; break;
                        case 1: pdt *= -1; break;
                        default: pdt *= myUtils.randomSign(rand); break;
                    }

                    x = gl_x0 + (float)Math.Sin(pt) * offset1;
                    y = gl_y0 + (float)Math.Cos(pt) * offset2;
                    break;
            }

            // Transform movement into vertical and/or horizontal
            {
                if (mode < 9)
                {
                    switch (axisMode)
                    {
                        case 0:
                            dx = 0.0f;
                            break;

                        case 1:
                            dy = 0.0f;
                            break;

                        case 2:
                            if (myUtils.randomChance(rand, 1, 2))
                                dx = 0;
                            else
                                dy = 0;
                            break;

                        default:
                            break;
                    }
                }

                if (mode == 9)
                {
                    switch (axisMode)
                    {
                        case 0:
                        case 1:
                            dx = 0;
                            break;

                        case 2:
                        case 3:
                            dy = 0;
                            break;

                        case 4:
                            if (myUtils.randomChance(rand, 1, 7))
                                dx = 0;
                            else
                                dy = 0;
                            break;

                        case 5:
                            if (myUtils.randomChance(rand, 1, 7))
                                dy = 0;
                            else
                                dx = 0;
                            break;

                        default:
                            if (myUtils.randomChance(rand, 1, 2))
                                dx = 0;
                            else
                                dy = 0;
                            break;
                    }
                }
            }

            // Apply slowness factor
            if (N > 50 && isFastMoving && rand.Next(111) == 0)
            {
                dx *= 1.1f;
                dy *= 1.1f;
            }
            else
            {
                dx /= slowFactor;
                dy /= slowFactor;
            }

            // Get color
            switch (colorMode)
            {
                case 0:
                    r = g = b = 1.0f;
                    break;

                case 1:
                    r = 1.0f - myUtils.randFloat(rand) / 10;
                    g = 1.0f - myUtils.randFloat(rand) / 10;
                    b = 1.0f - myUtils.randFloat(rand) / 10;
                    break;

                case 2:
                    colorPicker.getColor(x, y, ref r, ref g, ref b);
                    break;

                case 3:
                    r = myUtils.randFloat(rand, 0.1f);
                    g = myUtils.randFloat(rand, 0.1f);
                    b = myUtils.randFloat(rand, 0.1f);
                    break;
            }

            a = 0.85f;
            size = isRandomSize ? rand.Next(max) + 3 : max;

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void setNextMode()
        {
            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            // Movement is calculated in 2 steps;
            //  - step 1 (moveStep == false) calculates all the dx/dy for each particle
            //  - step 2 (moveStep == true ) just recalcs all xs and ys
            if (moveStep == true)
            {
                x += dx;
                y += dy;

                if (id == 0)
                {
                    X1 = gl_x0 + (float)Math.Sin(t1) * xRad1;
                    Y1 = gl_y0 + (float)Math.Cos(t1) * yRad1;

                    X2 = gl_x0 + (float)Math.Sin(t2) * xRad2;
                    Y2 = gl_y0 + (float)Math.Cos(t2) * yRad2;

                    t1 += dt1;
                    t2 += dt2;
                }
            }
            else
            {
                switch (mode)
                {
                    case 00:
                        if (x < 0 || x > gl_Width)
                            dx *= -1;

                        if (y < 0 || y > gl_Height)
                            dy *= -1;
                        break;

                    case 01:
                        if (x < -6666 || x > gl_Width + 6666)
                            dx *= -1;

                        if (y < -6666 || y > gl_Height + 6666)
                            dy *= -1;
                        break;

                    case 02:
                        {
                            float factor = 0.25f;
                            int offset = 111;

                            if (x < offset)
                                dx += myUtils.randFloat(rand) * factor;

                            if (x > gl_Width - offset)
                                dx -= myUtils.randFloat(rand) * factor;

                            if (y < offset)
                                dy += myUtils.randFloat(rand) * factor;

                            if (y > gl_Height - offset)
                                dy -= myUtils.randFloat(rand) * factor;
                        }
                        break;

                    case 03:
                        {
                            int chance = N * 2;

                            if (x < 0 && dx < 0 && myUtils.randomChance(rand, 1, chance))
                                dx *= -1;

                            if (x > gl_Width && dx > 0 && myUtils.randomChance(rand, 1, chance))
                                dx *= -1;

                            if (y < 0 && dy < 0 && myUtils.randomChance(rand, 1, chance))
                                dy *= -1;

                            if (y > gl_Height && dy > 0 && myUtils.randomChance(rand, 1, chance))
                                dy *= -1;
                        }
                        break;

                    case 04:
                        {
                            int offset = N * 3;

                            dx += myUtils.randomSign(rand) * 0.01f * rand.Next(50);
                            dy += myUtils.randomSign(rand) * 0.01f * rand.Next(50);

                            if (x < -offset || x > gl_Width + offset)
                                dx *= -1;

                            if (y < -offset || y > gl_Height + offset)
                                dy *= -1;
                        }
                        break;

                    case 05:
                        if (x < -50)
                            x = gl_Width + 50;
                        else if (x > gl_Width + 50)
                            x = -50;

                        if (y < -50)
                            y = gl_Height + 50;
                        else if (y > gl_Height + 50)
                            y = -50;
                        break;

                    case 06:
                        if (x < -50 || x > gl_Width + 50 || y < -50 || y > gl_Height + 50)
                        {
                            x = gl_x0;
                            y = gl_y0;

                            if (myUtils.randomChance(rand, 1, 2))
                            {
                                myUtils.swap<float>(ref dx, ref dy);
                            }
                        }
                        break;

                    case 07:
                    case 08:
                        if (x < -50 || x > gl_Width + 50 || y < -50 || y > gl_Height + 50)
                        {
                            if (a > 1)
                                a = 1;

                            a -= 0.05f;

                            if (a <= 0)
                                generateNew();
                        }
                        break;

                    case 09:
                        if ((x < -offset1 && dx < 0) || (x > gl_Width + offset1 && dx > 0))
                            dx *= -1;

                        if ((y < -offset1 && dy < 0) || (y > gl_Height + offset1 && dy > 0))
                            dy *= -1;
                        break;

                    case 10:
                    case 11:
                        if ((x < -offset1 && dx < 0) || (x > gl_Width + offset1 && dx > 0))
                            generateNew();

                        if ((y < -offset1 && dy < 0) || (y > gl_Height + offset1 && dy > 0))
                            generateNew();
                        break;

                    case 12:
                        {
                            if (myUtils.randomChance(rand, 1, prm_i[0]) && axisMode != 0)
                            {
                                if (dx == 0)
                                {
                                    if (axisMode != 2 || (axisMode == 2 && dy == 0))
                                        dx += myUtils.randomSign(rand) * 0.1f;
                                }
                                else
                                {
                                    dx += myUtils.signOf(dx) * 0.1f;
                                }
                            }

                            if (myUtils.randomChance(rand, 1, prm_i[0]) && axisMode != 1)
                            {
                                if (dy == 0)
                                {
                                    if (axisMode != 2 || (axisMode == 2 && dx == 0))
                                        dy += myUtils.randomSign(rand) * 0.1f;
                                }
                                else
                                {
                                    dy += myUtils.signOf(dy) * 0.1f;
                                }
                            }

                            if ((x < -offset1 && dx < 0) || (x > gl_Width + offset1 && dx > 0))
                                generateNew();

                            if ((y < -offset1 && dy < 0) || (y > gl_Height + offset1 && dy > 0))
                                generateNew();
                        }
                        break;

                    case 13:
                    case 14:
                    case 15:
                        if ((x < -offset1 && dx < 0) || (x > gl_Width + offset1 && dx > 0))
                            generateNew();

                        if ((y < -offset1 && dy < 0) || (y > gl_Height + offset1 && dy > 0))
                            generateNew();
                        break;

                    case 16:
                        switch (prm_i[1])
                        {
                            // No acceleration
                            case 0:
                                break;

                            // Const acceleration
                            case 1:
                                dx *= prm_f[0];
                                dy *= prm_f[0];
                                break;

                            // Random acceleration
                            case 2:
                                dx *= 1.0f + myUtils.randFloat(rand)/33;
                                dy *= 1.0f + myUtils.randFloat(rand)/33;
                                break;
                        }

                        if ((x < -offset1 && dx < 0) || (x > gl_Width + offset1 && dx > 0))
                            generateNew();

                        if ((y < -offset1 && dy < 0) || (y > gl_Height + offset1 && dy > 0))
                            generateNew();
                        break;

                    case 17:
                        if (x < -offset1 || x > gl_Width + offset1 || y < -offset1 || y > gl_Height + offset1)
                            generateNew();
                        else
                        {
                            if (prm_i[2] == 0)
                            {
                                // Non-additive dx/dy: each iteration we forget the state of the previous one
                                dx = dy = 0;
                            }

                            float factor1 = 500;
                            float factor2 = 500;

                            switch (prm_i[1])
                            {
                                case 0: factor1 *= +1; factor2 *= +1; break;
                                case 1: factor1 *= +1; factor2 *= -1; break;
                                case 2: factor1 *= -1; factor2 *= +1; break;
                                case 3: factor1 *= -1; factor2 *= -1; break;
                            }

                            float dX = X1 - x;
                            float dY = Y1 - y;

                            float dist = (float)(dX * dX + dY * dY) + 0.0001f;
                            float sp_dist = factor1 / dist;

                            dx += dX * sp_dist;
                            dy += dY * sp_dist;

                            dX = X2 - x;
                            dY = Y2 - y;

                            dist = (float)(dX * dX + dY * dY) + 0.0001f;
                            sp_dist = factor2 / dist;

                            dx += dX * sp_dist; 
                            dy += dY * sp_dist;

                            if (prm_i[2] == 1)
                            {
                                // Additive dx/dy: each iteration's result just adds to the current state;
                                // Need to introduce reducing factor in order not to reach super high values
                                dx *= 0.75f;
                                dy *= 0.75f;
                            }
                        }
                        break;

                    case 18:
                        if ((offset1 <= offset2 && offset1 > gl_Width / 2 + 33) || (offset1 > offset2 && offset2 > gl_Width / 2 + 33))
                        {
                            a -= 0.05f;

                            if (a < 0)
                                generateNew();
                        }
                        else
                        {
                            dx = (gl_x0 + (float)Math.Sin(pt) * offset1) - x;
                            dy = (gl_y0 + (float)Math.Cos(pt) * offset2) - y;
                            pt += pdt;
                            offset1 += prm_i[1] % 10;   // 23 % 10 = 3  -- value 1
                            offset2 += prm_i[1] / 10;   // 23 / 10 = 2  -- value 2
                        }
                        break;
                }
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Show()
        {
            if (doShowAuxParticles)
            {
                myPrimitive._Rectangle.SetColor(1.0f, 0.33f, 0.33f, 0.85f);
                myPrimitive._Rectangle.Draw(X1 - 5, Y1 - 5, 10, 10, false);
                myPrimitive._Rectangle.Draw(X2 - 5, Y2 - 5, 10, 10, false);
            }

            if (isAggregateOpacity)
                a = 0;

            // Render connecting lines
            for (int i = 0; i < list.Count; i++)
            {
                var obj = list[i] as myObj_310;

                float lineOpacity = 0.1f;

                if (obj != this)
                {
                    float xx = obj.x - x;
                    float yy = obj.y - y;
                    float dist2 = 0.0001f;

                    switch (lineMode)
                    {
                        // Const opacity adjusted for N
                        case 0:
                            {
                                if (N > 500)
                                    lineOpacity -= N * 0.000175f;
                                else if (N > 450)
                                    lineOpacity -= N * 0.000185f;
                                else if (N > 333)
                                    lineOpacity -= N * 0.00020f;
                                else
                                    lineOpacity -= N * 0.00025f;
                            }
                            break;

                        case 1:
                            dist2 += xx * xx + yy * yy;
                            lineOpacity = (float)(10000.0 / dist2);
                            break;

                        case 2:
                            dist2 += xx * xx + yy * yy;
                            lineOpacity = (float)(20000.0 / dist2);
                            break;

                        case 3:
                            dist2 += xx * xx + yy * yy;
                            lineOpacity = (float)(20000.0 / dist2) + 0.05f;
                            break;

                        case 4:
                            dist2 += (float)Math.Sqrt(xx * xx + yy * yy);

                            if (N > 300)
                                lineOpacity = (float)((gl_Height * 0.01f) / dist2);
                            else if (N > 100)
                                lineOpacity = (float)((gl_Height * 0.02f) / dist2);
                            else if (N > 50)
                                lineOpacity = (float)((gl_Height * 0.04f) / dist2);
                            else
                                lineOpacity = (float)((gl_Height * 0.05f) / dist2);
                            break;

                        case 5:
                            {
                                lineOpacity = 0;
                                int zzz = 234;

                                if (xx > -zzz && xx < zzz && yy > -zzz && yy < zzz)
                                {
                                    dist2 += (float)Math.Sqrt(xx * xx + yy * yy);
                                    lineOpacity = (float)(zzz / dist2 / 3);
                                }
                            }
                            break;
                    }

                    if (doClearBuffer == false)
                    {
                        lineOpacity /= (N < 300) ? 3 : 7;
                    }

                    if (lineOpacity > 0)
                    {
                        switch (lineStyle)
                        {
                            case 0:
                                myPrimitive._LineInst.setInstanceCoords(obj.x, obj.y, x, y);
                                break;

                            // Parallel
                            case 1:
                            case 2:
                            case 3:
                                if (obj.id < id)
                                    myPrimitive._LineInst.setInstanceCoords(obj.x + lineStyle, obj.y, x + lineStyle, y);
                                else
                                    myPrimitive._LineInst.setInstanceCoords(obj.x - lineStyle, obj.y, x - lineStyle, y);
                                break;

                            // Crossed
                            case 4:
                            case 5:
                            case 6:
                                myPrimitive._LineInst.setInstanceCoords(obj.x + lineStyle - 3, obj.y, x - lineStyle + 3, y);
                                break;

                            // Parallel, the TL and BR angles are connected
                            case 7:
                                if (obj.id < id)
                                    myPrimitive._LineInst.setInstanceCoords(obj.x - size + 1, obj.y - size + 1, x - size + 1, y - size + 1);
                                else
                                    myPrimitive._LineInst.setInstanceCoords(obj.x + size - 1, obj.y + size - 1, x + size - 1, y + size - 1);
                                break;

                            // Parallel, the TL and BR angles are connected
                            case 8:
                                myPrimitive._LineInst.setInstanceCoords(obj.x - size + 1, obj.y - size + 1, x + size - 1, y + size - 1);
                                break;
                        }

                        myPrimitive._LineInst.setInstanceColor(r, g, b, lineOpacity);
                    }

                    if (isAggregateOpacity)
                        a += lineOpacity/3;
                }
            }

            // Draw vertical lines
            if (isVerticalLine && N < 50)
            {
                myPrimitive._LineInst.setInstanceCoords(x, 0, x, gl_Height);
                myPrimitive._LineInst.setInstanceColor(1, 1, 1, 0.13f);

                myPrimitive._LineInst.setInstanceCoords(0, y, gl_Width, y);
                myPrimitive._LineInst.setInstanceColor(1, 1, 1, 0.13f);
            }

            switch (shape)
            {
                // Instanced squares
                case 0:
                    var rectInst = inst as myRectangleInst;

                    for (int i = 0; i < 2; i++)
                    {
                        int val1 = (int)(size - 2 * i);
                        int val2 = val1 * 2;

                        rectInst.setInstanceCoords(x - val1, y - val1, val2, val2);
                        rectInst.setInstanceColor(r, g, b, i == 0 ? a/2 : a);
                        rectInst.setInstanceAngle(i == 0 ? t : 0);
                    }
                    break;

                case 1:
                    var triangleInst = inst as myTriangleInst;

                    for (int i = 0; i < 2; i++)
                    {
                        int val1 = (int)(size - 2 * i);

                        triangleInst.setInstanceCoords(x, y, val1, i == 0 ? t : 0);
                        triangleInst.setInstanceColor(r, g, b, i == 0 ? a/2 : a);
                    }
                    break;

                case 2:
                    var ellipseInst = inst as myEllipseInst;

                    for (int i = 0; i < 2; i++)
                    {
                        int val1 = (int)(size - 2 * i);

                        ellipseInst.setInstanceCoords(x, y, 2 * val1, 0);
                        ellipseInst.setInstanceColor(r, g, b, i == 0 ? a/2 : a);
                    }
                    break;

                case 3:
                    var pentagonInst = inst as myPentagonInst;

                    for (int i = 0; i < 2; i++)
                    {
                        int val1 = (int)(size - 2 * i);

                        pentagonInst.setInstanceCoords(x, y, 2 * val1, i == 0 ? t : 0);
                        pentagonInst.setInstanceColor(r, g, b, i == 0 ? a/2 : a);
                    }
                    break;

                case 4:
                    var hexagonInst = inst as myHexagonInst;

                    for (int i = 0; i < 2; i++)
                    {
                        int val1 = (int)(size - 2 * i);

                        hexagonInst.setInstanceCoords(x, y, 2 * val1, i == 0 ? t : 0);
                        hexagonInst.setInstanceColor(r, g, b, i == 0 ? a/2 : a);
                    }
                    break;
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Process(Window window)
        {
            initShapes();

            //Glfw.SwapInterval(0);

            if (doCreateAtOnce)
                while (list.Count < N)
                    list.Add(new myObj_310());

/*
            list.Add(new myObj_310());
            (list[list.Count - 1] as myObj_310).x = 0;
            (list[list.Count - 1] as myObj_310).y = 0;

            list.Add(new myObj_310());
            (list[list.Count - 1] as myObj_310).x = 0;
            (list[list.Count - 1] as myObj_310).y = gl_Height;

            list.Add(new myObj_310());
            (list[list.Count - 1] as myObj_310).x = gl_Width;
            (list[list.Count - 1] as myObj_310).y = 0;

            list.Add(new myObj_310());
            (list[list.Count - 1] as myObj_310).x = gl_Width;
            (list[list.Count - 1] as myObj_310).y = gl_Height;
*/
            if (doClearBuffer)
            {
                glDrawBuffer(GL_FRONT_AND_BACK | GL_DEPTH_BUFFER_BIT);

                float r = (float)rand.NextDouble() / 13;
                float g = (float)rand.NextDouble() / 13;
                float b = (float)rand.NextDouble() / 13;

                glClearColor(r, g, b, 1.0f);
            }
            else
            {
                glDrawBuffer(GL_FRONT_AND_BACK);
            }

            while (!Glfw.WindowShouldClose(window))
            {
                processInput(window);

                // Swap fore/back framebuffers, and poll for operating system events.
                Glfw.SwapBuffers(window);
                Glfw.PollEvents();

                if (doClearBuffer)
                {
                    glClear(GL_COLOR_BUFFER_BIT);
                }
                else
                {
                    dimScreen(dimAlpha, doShiftColor: doShiftColor, useStrongerDimFactor: false);
                }

                // Render Frame
                {
                    inst.ResetBuffer();
                    myPrimitive._LineInst.ResetBuffer();

                    moveStep = true;

                    for (int i = 0; i < list.Count; i++)
                    {
                        (list[i] as myObj_310).Move();
                    }

                    moveStep = false;

                    for (int i = 0; i < list.Count; i++)
                    {
                        var obj = list[i] as myObj_310;
                        obj.Show();
                        obj.Move();
                    }

                    myPrimitive._LineInst.Draw();

                    inst.SetColorA(0);
                    inst.Draw(false);
                }

                if (!doCreateAtOnce)
                    if (list.Count < N)
                        list.Add(new myObj_310());

                System.Threading.Thread.Sleep(renderDelay);
                t += dt;
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void initShapes()
        {
            myPrimitive.init_Rectangle();
            myPrimitive.init_LineInst(N * (N-1) + N * 2);

            base.initShapes(shape, 2*N, 0);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void Triangulate()
        {
/*
            for (int i = 0; i < list.Count; i++)
            {
                var obj = list[i] as myObj_310;

                if (obj.left != null || obj.right != null)
                {
                    for (int j = 0; j < list.Count; j++)
                    {
                        if (i != j)
                        {
                            var other = list[i] as myObj_310;
                            float dist = (obj.x - other.x) * (obj.x - other.x) + (obj.y - other.y) * (obj.y - other.y);
                        }
                    }
                }
            }
*/
        }

        // ---------------------------------------------------------------------------------------------------------------
    }
};
