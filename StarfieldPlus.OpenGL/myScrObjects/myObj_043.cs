using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;


/*
    - Various shapes growing out from a single starting point
    - Based initially on the starfield class -- where all the stars are generated at a center point

    -- case 100 x drawMode 5
*/


namespace my
{
    delegate double trigonometricFunc(double d);

    public class myObj_043 : myObject
    {
        private const int N = 50;

        private int X, Y, oldX, oldY;
        private float dxf = 0, dyf = 0, x = 0, y = 0, time, A, size, dSize;
        private bool isActive = false;

        static int x0 = 0, y0 = 0, moveMode = -1, speedMode = -1, colorMode = -1, connectionMode = -1, t = -1, shape = 0;
        static bool generationAllowed = false, isRandomMove = false, isBorderScared = false, isFirstIteration = true, doUpdateConstants = true;
        static bool doClearBuffer = false, doFillShapes = false;
        static float time_global = 0, dtGlobal = 0, dtCommon = 0;

        static int si1 = 0, si2 = 0, moveModeCnt = 114;
        static float sf2 = 0, sf3 = 0;
        static float R, G, B, a = 0.0f, b = 0.0f, c = 0.0f, da = 1.0f/256, lineA = 0.1f;
        static float dimAlpha = 0.05f;

        static trigonometricFunc trigFunc1 = null;
        static trigonometricFunc trigFunc2 = null;
        static trigonometricFunc trigFunc3 = null;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_043()
        {
            if (colorPicker == null)
            {
                colorPicker = new myColorPicker(gl_Width, gl_Height);
                list = new List<myObject>();

                init();
            }

            generateNew();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time initialization
        private void init()
        {
            gl_x0 = x0 = gl_Width  / 2;
            gl_y0 = y0 = gl_Height / 2;

            shape = rand.Next(5);
            moveMode = rand.Next(moveModeCnt + 1);
//moveMode = 114;
            doFillShapes = myUtils.randomBool(rand);
            connectionMode = rand.Next(9);

            speedMode = rand.Next(2);
            colorMode = rand.Next(2);
            t = rand.Next(15) + 10;
            dtGlobal = 0.15f;

            getNewColor();
            updateConstants();

            dtCommon = 0;

            generationAllowed = true;
            isRandomMove = myUtils.randomChance(rand, 1, 10);

            lineA += (float)rand.NextDouble() / 8;

#if false
            // Override Move()
            moveMode = 106;
            drawMode = 2;
            t = 1;
            isRandomMove = false;
            updateConstants();
#endif

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            string str = $"Obj = myObj_043\n\n" +
                            $"N = {N} ({list.Count})\n" +
                            $"shape = {shape}\n" +
                            $"colorMode = {colorMode}\n" +
                            $"moveMode = {moveMode}\n" +
                            $"connectionMode = {connectionMode}\n" +
                            $"a = {a}; b = {b}; c = {c}\nsi1 = {si1}\n sf2 = {sf2}\n sf3 = {sf3}\n" +
                            $"renderDelay = {renderDelay}\n"
;
            return str;
        }

        // ---------------------------------------------------------------------------------------------------------------

        // 
        protected override void setNextMode()
        {
            var oldShape = shape;

            init();

            shape = oldShape;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void generateNew()
        {
            if (generationAllowed)
            {
                int speed = (speedMode == 0) ? 5 : 3 + rand.Next(5);

                A = (float)rand.NextDouble() / 5;
                size = rand.Next(6) + 1;
                dSize = 0.01f * rand.Next(11);

                //getDxDy(speed, ref dxi, ref dyi);
                getDxDy(speed, ref dxf, ref dyf);

                x = X = oldX = x0;
                y = Y = oldY = y0;
                time = 0;

                isActive = true;
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        void getDxDy(int speed, ref int dxi, ref int dyi)
        {
            int x = rand.Next(gl_Width);
            int y = rand.Next(gl_Width);

            double dist = Math.Sqrt((x - gl_x0) * (x - gl_x0) + (y - gl_x0) * (y - gl_x0));

            dxi = (int)((x - gl_x0) * speed / dist);
            dyi = (int)((y - gl_x0) * speed / dist);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        void getDxDy(int speed, ref float dxf, ref float dyf)
        {
            int x = rand.Next(gl_Width);
            int y = rand.Next(gl_Width);

            double dist = Math.Sqrt((x - gl_x0) * (x - gl_x0) + (y - gl_x0) * (y - gl_x0));

            dxf = (float)((x - gl_x0) * speed / dist);
            dyf = (float)((y - gl_x0) * speed / dist);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            oldX = X;
            oldY = Y;

            // Every option that uses constants will have them changed in updateConstants()

            switch (moveMode)
            {
                // --- option 1 ---
                case 0:
                case 1:
                case 2:
                case 3:
                case 4:
                    // si1 : Lower values for rather straigt beams;
                    // Higher values make the beams lightning-like
                    // Hight values make the beams erratic

                    x += dxf * sf2;
                    y += dyf * sf2;

                    x += (int)(Math.Sin(Y) * si1);
                    y += (int)(Math.Sin(X) * si1);
                    break;

                // --- option 2 ---
                case 5:
                    x += dxf;
                    y += dyf;
                    break;

                // --- option 3 ---
                case 6:
                    time += (float)(rand.Next(999) / 1000.0f);

                    x += dxf + (float)(Math.Sin(time) * sf2);
                    y += dyf + (float)(Math.Cos(time) * sf2);
                    break;

                // --- option 4 ---
                case 7:
                    time += 0.1f;

                    x += dxf + (float)(Math.Sin(time) * sf2);
                    y += dyf + (float)(Math.Cos(time) * sf2);
                    break;

                // --- option 5 ---
                case 8:
                    time += 0.001f + (rand.Next(10)) * 0.005f;

                    x += dxf + (float)(Math.Sin(time) * sf2);
                    y += dyf + (float)(Math.Cos(time) * sf2);
                    break;

                // --- option 6 --- Hair like 
                case 9:
                case 10:
                case 11:
                case 12:
                case 13:
                case 14:
                case 15:
                case 16:
                    time += dtCommon;

                    x += dxf + (float)(Math.Sin(time * dxf) * sf2);
                    y += dyf + (float)(Math.Sin(time * dyf) * sf2);
                    break;

                // --- option 7 --- Spiraling Wheels
                case 17:
                case 18:
                case 19:
                case 20:
                case 21:
                    time += dtCommon;

                    x += (float)Math.Sin(time + dyf) * time * sf2;
                    y += (float)Math.Cos(time + dyf) * time * sf2;
                    break;

                // --- option 8 --- Stars with Spiraling Rays
                case 22:
                case 23:
                    time += dtCommon;

                    x += (float)(Math.Sin(time + dyf) * sf2) * time;
                    y += (float)(Math.Cos(time + dyf) * sf2) * time;
                    break;

                // --- option 9 --- Spiraling Squares
                case 24:
                    time += dtCommon;

                    x += (float)Math.Sin(time + dyf) * time * sf2;
                    y += (float)Math.Cos(time + dxf) * time * sf2;
                    break;

                // --- option 10 --- Spiraling Eights
                case 25:
                    time += dtCommon;

                    x += (float)Math.Sin(time * 1) * time * sf2;
                    y += (float)Math.Cos(time * 2) * time * sf2;
                    break;

                // --- option 11 --- Balls of Strings
                case 26:
                case 27:
                    time += dtCommon;

                    x += (float)Math.Sin(time * dyf) * time * sf2;
                    y += (float)Math.Cos(time * dyf) * time * sf2;
                    break;

                // --- option 12 --- Spiraling Wheels, ver2
                case 28:
                    time += dtCommon;

                    x += (float)Math.Sin(time + dxf) * time * dyf * sf2;
                    y += (float)Math.Cos(time + dxf) * time * dyf * sf2;
                    break;

                // --- option 13 --- Zigzagging Rays
                case 29:
                case 30:
                case 31:
                case 32:
                    time += dtCommon;
                    time += (float)(Math.Sin(time) * sf3);

                    x += (float)Math.Sin(time + dxf) * time * dyf * sf2;
                    y += (float)Math.Cos(time + dxf) * time * dyf * sf2;
                    break;

                // --- option 14 --- Straight rays from spiralling center
                case 33:
                    time += dtCommon;
                    time += (float)(Math.Sin(time) * sf3);

                    x += (float)Math.Sin(time + dxf) * time * dyf * sf2;
                    y += (float)Math.Cos(time + dxf) * time * dyf * sf2;
                    break;

                // --- option 15 --- Stars with straight rays
                case 34:
                case 35:
                case 36:
                    time += dtCommon;
                    time += (float)(Math.Sin(time) * sf3);

                    x += (float)Math.Sin(time + dxf) * time * dyf * sf2;
                    y += (float)Math.Cos(time + dxf) * time * dyf * sf2;
                    break;

                // --- option 16 --- Curled fishing line
                case 37:
                case 38:
                case 39:
                    time += dtCommon;
                    time += (float)Math.Tan(time) / sf3;

                    x += (float)Math.Sin(time + dxf) * dyf * time * sf2;
                    y += (float)Math.Cos(time + dxf) * dyf * time * sf2;
                    break;

                // --- option 17 --- Various shapes with ever increasing dtCommon
                case 40:
                case 41:
                    time += dtCommon;

                    x += (float)Math.Sin(time + dxf) * dyf * time * sf2;
                    y += (float)Math.Cos(time + dxf) * dyf * time * sf2;
                    break;

                // --- option 18 --- Various shapes with ever increasing dtCommon
                case 42:
                    time += dtCommon;
                    time += (float)(Math.Sin(time)) / sf3;

                    x += (float)Math.Sin(time + dxf) * dyf * time * sf2;
                    y += (float)Math.Cos(time + dxf) * dyf * time * sf2;
                    break;

                // --- option 19 --- Waves of Spirals
                case 43:
                    time += dtCommon;

                    x += (float)Math.Sin(time + dxf) * time * time * sf2;
                    y += (float)Math.Cos(time + dxf) * time * time * sf2;
                    break;

                // --- option 20 --- Shifting Spirals Family
                case 44:
                case 45:
                case 46:
                case 47:
                    time += dtCommon;

                    x += dxf * c;
                    y += dyf * c;

                    x += (float)Math.Sin(a * time + b * dxf) * time_global * sf2;
                    y += (float)Math.Cos(a * time + b * dxf) * time_global * sf2;
                    break;

                // --- option 21 --- Circles -- do I have it already?..
                case 48:
                    time += dtCommon;

                    x += (float)Math.Sin(time + dyf * sf3) * time * sf2;
                    y += (float)Math.Cos(time + dyf * sf3) * time * sf2;
                    break;

                // --- option 22 --- Square stuff
                case 49:
                    time += dtCommon;

                    x += (float)Math.Sin(time) * dxf * time * sf2;
                    y += (float)Math.Cos(time) * dyf * time * sf2;
                    break;

                // --- option 23 ---
                case 50:
                    time += dtCommon;

                    x += dxf;
                    y += dyf * (float)Math.Cos(time) * sf2;
                    break;

                // --- option 24 ---
                case 51:
                    time += dtCommon;

                    x += dxf;
                    y += dyf * (float)Math.Sin(time) * sf2;
                    break;

                // --- option 25 ---
                case 52:
                    time += dtCommon;

                    x += dxf;
                    y += dyf * (float)Math.Tan(time) * sf2;
                    break;

                // --- option 26 ---
                case 53:
                    time += dtCommon;

                    x += dxf * (float)Math.Sin(time) * sf2;
                    y += dyf / sf3;
                    break;

                // --- option 27 ---
                case 54:
                    time += dtCommon;

                    x += (int)(Math.Sin(time * dyf) * sf2 + time * sf3);
                    y += (int)(Math.Cos(time * dxf) * sf2 + time * sf3);
                    break;

                // --- option 26 --- Scepter Jellyfishes
                case 55:
                    time += dtCommon;

                    x += (float)(Math.Sin(time + dyf) * a + time * si1) * sf2;
                    y += (float)(Math.Cos(time + dxf) * a + time * si1) * sf2;
                    break;

                // --- option 27 --- Metro Maps
                case 56:
                case 57:
                    time += dtCommon;

                    x += (int)(Math.Sin(time * a + dyf * b) * sf3 + time * sf2);
                    y += (int)(Math.Cos(time * a + dxf * b) * sf3 + time * sf2);
                    break;

                // --- option 28 ---
                case 58:
                    time += dtCommon;

                    x += dxf * sf3;
                    y += dyf * sf3;

                    x += (int)(Math.Sin(time) * sf2) * a;
                    y += (int)(Math.Cos(time) * sf2) * a;
                    break;

                // --- option 29 ---
                case 59:
                    time += dtCommon;

                    x += dxf + (int)(Math.Sin(time * dxf) * sf2) * sf3;
                    y += dyf + (int)(Math.Sin(time * dyf) * sf2) * sf3;
                    break;

                // --- option 30 --- Pasta Monsters - 1
                case 60:
                case 61:
                    time += dtCommon;
                    c = (float)(trigFunc3(time_global));    // This turns spirals into random tentacles

                    x += dxf * c * sf3;
                    y += dyf * c * sf3;

                    x += (float)(trigFunc1(a * time + b * dxf)) * sf2 * time_global;
                    y += (float)(trigFunc2(a * time + b * dxf)) * sf2 * time_global;
                    break;

                // --- option 31 --- Pasta Monsters - 2
                case 62:
                case 63:
                case 64:
                    time += dtCommon;

                    switch (moveMode)
                    {
                        case 62:
                            sf3 = (float)(Math.Sin(time_global)); // <------ + play with dt
                            break;

                        case 63:
                            sf3 = (float)(Math.Sin(time_global * dyf * dxf));   // This produces Alien Tail Shapes
                            break;

                        case 64:
                            a = 0.25f * (rand.Next(3) - 1);
                            sf3 = 1.55f;
                            c = 0.10f;
                            break;
                    }

                    x += dxf * c;
                    y += dyf * c;

                    x += (float)(Math.Sin(a * time + sf3 * dxf)) * sf2 * time_global;
                    y += (float)(Math.Cos(a * time + sf3 * dxf)) * sf2 * time_global;
                    break;

                // --- option 32 --- Fractal-like Flowers - 1
                case 65:
                case 66:
                case 67:
                    time += dtCommon;

                    // This is what makes it fractal-like
                    sf3 = b * time_global * time;

                    x += dxf * c;
                    y += dyf * c;

                    if (moveMode != 67)
                    {
                        x += (float)Math.Sin(a * time + sf3 * dxf) * sf2 * time_global;
                        y += (float)Math.Cos(a * time + sf3 * dxf) * sf2 * time_global;
                    }
                    else
                    {
                        // todo: mostly everything with sin/cos can be turned square this way
                        x += (int)(Math.Sin(a * time + sf3 * dxf) * 1.25f) * sf2 * time_global;
                        y += (int)(Math.Cos(a * time + sf3 * dxf) * 1.25f) * sf2 * time_global;
                    }
                    break;

                // --- option 33 --- Fractal-like Flowers - 2
                case 68:
                case 69:
                case 70:
                case 71:
                    time += dtCommon;

                    sf3 = time_global * time * b;

                    x += (float)Math.Sin(a * time + sf3) * sf2;
                    y += (float)Math.Cos(a * time + sf3) * sf2;
                    break;

                // --- option 34 --- Fractal-like Flowers - 3 -- to int
                case 72:
                case 73:
                    time += dtCommon;

                    sf3 = time_global * time * b;

                    x += (int)(Math.Sin(a * time_global + sf3) * c) * sf2;
                    y += (int)(Math.Cos(a * time_global + sf3) * c) * sf2;
                    break;

                // --- option 35 ---
                case 74:
                case 75:
                case 76:
                case 77:
                    time += dtCommon;

                    sf3 = time_global * time;

                    x += (float)Math.Sin(a * time + sf3) * (float)Math.Sin(sf3) * sf2;
                    y += (float)Math.Cos(a * time + sf3) * (float)Math.Sin(sf3) * sf2;
                    break;

                // --- option 36 --- Diagonal Strangeness
                case 78:
                case 79:
                case 80:
                    sf2 = dxf;  // tmp to swap
                    dxf = dyf;
                    dyf = sf2;

                    x += (dxf * a);
                    y += (dyf * b / sf3);
                    break;

                // --- option 37 ---
                case 81:
                    dxf *= (float)Math.Sin(dxf);
                    dyf *= (float)Math.Cos(dyf);
                    x += (int)(dxf * a);
                    y += (int)(dyf * b);
                    break;

                // --- option 38 ---
                case 82:
                    dxf += (float)Math.Sin(dxf);
                    dyf += (float)Math.Sin(dyf);
                    x += (dxf * a);
                    y += (dyf * b);
                    break;

                case 83:
                    dxf += (float)Math.Sin(dxf);
                    dyf += (float)Math.Sin(dyf);
                    x += (int)(dxf * a);
                    y += (int)(dyf * b);
                    break;

                // --- option 39 ---
                case 84:
                    dxf += dxf > 0 ? a : -a;
                    dyf += dyf > 0 ? b : -b;

                    x += dxf / c;
                    y += dyf / c;
                    break;

                // --- option 40 ---
                case 85:
                    x += (float)Math.Sin(X * dxf) * c;
                    y += (float)Math.Cos(Y * dyf) * c;
                    break;

                case 86:
                    x += (int)(Math.Sin(X * dxf) * 2.0f) * c;
                    y += (int)(Math.Cos(Y * dyf) * 2.0f) * c;
                    break;

                // --- option 41 ---
                case 87:
                    x += (float)Math.Tan(X * dxf) * c;
                    y += (float)Math.Tan(Y * dyf) * c;
                    break;

                case 88:
                    x += (int)Math.Tan(X * dxf) * c;
                    y += (int)Math.Tan(Y * dyf) * c;
                    break;

                // --- option 42 ---
                case 89:
                    x += (int)(Math.Sin(x + dxf) * c);
                    y += (int)(Math.Sin(y + dyf) * c);
                    break;

                // --- option 43 ---
                case 90:
                    x += (int)(Math.Sin(x + dxf / 5000) * c);
                    y += (int)(Math.Cos(y + dyf / 5000) * c);
                    break;

                // --- option 44 ---
                case 91:
                    x += (int)(Math.Sin(x + dxf / si1) * c);
                    y += (int)(Math.Cos(y + dyf / si1) * c);
                    break;

                // --- option 45 ---
                case 92:
                    // 9: hair like and also with less dt is a better distributed metro maps
                    time += dtCommon;
                    x += (int)(Math.Sin(time + dyf) * sf2 + dxf * time) / si1;
                    y += (int)(Math.Cos(time + dxf) * sf2 + dyf * time) / si1;
                    break;

                // --- option 46 ---
                case 93:
                case 94:
                case 95:
                case 96:
                case 97:
                    time += dtCommon;

                    x += (int)(Math.Sin(Y + time * dxf) * sf2 * time) * si1;
                    y += (int)(Math.Cos(X + time * dyf) * sf2 * time) * si1;
                    break;

                case 98:
                    x += (int)(Math.Sin(x + dxf) * c) * 3;
                    y += (float)(Math.Sin(y + dyf) * c) * 3;
                    break;

                case 99:
                    time += dtCommon;

                    x += dxf * (int)(Math.Sin(time) * 1.33f) * sf2;
                    y += dyf * (float)(Math.Sin(time) * 1.33f) * sf2;
                    break;

                // --- option 47 ---
                case 100:
                    a = rand.Next(si1) + 2;
                    b = rand.Next(si1) + 2;

                    x += (int)(dxf / a) * a * sf2;
                    y += (int)(dyf / b) * b * sf2;
                    break;

                // --- option 48 --- Swasticas
                case 101:
                case 102:
                    time += dtCommon;

                    x += (int)(Math.Sin(time + dxf * dyf) * a) * sf2 * time / 5;
                    y += (int)(Math.Cos(time + dxf * dyf) * b) * sf2 * time / 5;
                    break;

                case 103:
                    time += dtCommon;

                    x += (float)(Math.Sin(time * dxf) * a);
                    y += (float)(Math.Sin(time * dyf) * b);

                    x += (int)(Math.Sin(time + dxf * dyf) * a) * sf2 * time / 5;
                    y += (int)(Math.Cos(time + dxf * dyf) * b) * sf2 * time / 5;
                    break;

                // --- option 49 ---
                case 104:
                    time += dtCommon;
                    x += (int)(Math.Sin(time * dxf) * a);
                    y += (int)(Math.Sin(time * dyf) * b);
                    break;

                // --- option 50 ---
                case 105:
                    time += dtCommon;

                    sf2 = (float)(Math.Sin(time_global * time) * a);

                    if (sf2 != 0)
                    {
                        x += 5 * dxf / (sf2);
                        y += 5 * dyf / (sf2);
                    }
                    break;

                // --- option 51 --- Submarine Blueprints
                case 106:
                    time += dtCommon;

                    sf2 = (float)(Math.Sin(time_global * time) * a + 0.0001f);
                    sf3 = (float)(Math.Cos(time_global * time) * a + 0.0001f);

                    x += 5 * dxf / (sf2);
                    y += 5 * dyf / (sf3);
                    break;

                // --- option 52 ---
                case 107:
                case 108:
                    x += dxf;
                    y += dyf;

                    x += (int)(Math.Sin(dxf) * si1);
                    y += (int)(Math.Sin(dyf) * si1);

                    dxf *= sf3;
                    dyf *= sf3;

                    dxf *= (float)rand.NextDouble() * sf2;
                    dyf *= (float)rand.NextDouble() * sf2;
                    break;

                // --- option 53 ---
                case 109:
                    if (rand.Next(2) == 0)
                    {
                        x += (dyf / dxf) * myUtils.randomSign(rand) * sf2;
                        y += (dxf / dyf) * myUtils.randomSign(rand) * sf2;
                    }
                    else
                    {
                        y += (dyf / dxf) * myUtils.randomSign(rand) * sf2;
                        x += (dxf / dyf) * myUtils.randomSign(rand) * sf2;
                    }
                    break;

                // --- option 54 ---
                case 110:
                    x += myUtils.randomSign(rand) * sf2;
                    y += myUtils.randomSign(rand) * sf2;
                    sf2 += sf3;
                    break;

                case 111:
                    x += myUtils.random101(rand) * sf2;
                    y += myUtils.random101(rand) * sf2;
                    sf2 += sf3;
                    break;

                case 112:
                    if (rand.Next(2) == 0)
                        x += myUtils.random101(rand) * sf2;
                    else
                        y += myUtils.random101(rand) * sf2;
                    sf2 += sf3;
                    break;

                case 113:
                    if (myUtils.randomChance(rand, si1, si2))
                        x += dxf * 5;
                    else
                        x -= dxf * 5;

                    if (myUtils.randomChance(rand, si1, si2))
                        y += dyf * 5;
                    else
                        y -= dyf * 5;

                    dxf *= (1.0f + sf3);
                    dyf *= (1.0f + sf3);
                    break;

                case 114:
                    if (myUtils.randomChance(rand, 1, 2))
                    {
                        if (myUtils.randomChance(rand, si1, si2))
                            x += dxf * 5;
                        else
                            x -= dxf * 5;
                    }
                    else
                    {
                        if (myUtils.randomChance(rand, si1, si2))
                            y += dyf * 5;
                        else
                            y -= dyf * 5;
                    }

                    dxf *= (1.01f + sf3);
                    dyf *= (1.01f + sf3);
                    break;

                default:

                    //x += (dyf / dxf);
                    //y += (dxf / dyf);

                    //x += (dyf / dxf) + (float)(Math.Sin(time_global - time) * 2);
                    //y += (dxf / dyf) + (float)(Math.Sin(time_global + time) * 2);

                    break;
            }

#if false
                    // 1
                    dxf += (float)(Math.Sin(time * dxf) * a);
                    dyf += (float)(Math.Sin(time * dyf) * b);

                    x += dxf * 0.1f;
                    y += dyf * 0.1f;

                    // 2
                    dxf += (float)(Math.Sin(time * dxf) * a);
                    dyf += (float)(Math.Cos(time * dyf) * b);

                    x += dxf * 0.1f;
                    y += dyf * 0.1f;

                    // 3
                    dtCommon = 0.1f;

                    a = 13.0f;
                    b = 13.0f;

                    time += dtCommon;

                    x += (float)(Math.Sin(time_global) * a) + (float)(Math.Sin(time) * b);
                    y += (float)(Math.Cos(time_global) * a) + (float)(Math.Cos(time) * b);

                    // 4

#endif

            if (isActive)
            {
                X = (int)x;
                Y = (int)y;
            }

            A -= da;
            size -= dSize;

            // todo: 0.25 is a min size for a rectangle; see what's this size is for the rest of the shapes
            if (size < 0.25f)
                size = 0.25f;

            if (isBorderScared)
            {
                if (X < 0 || X > gl_Width || Y < 0 || Y > gl_Height || A <= 0)
                {
                    isActive = false;
                    generateNew();
                }
            }
            else
            {
                if (A <= 0)
                {
                    isActive = false;
                    generateNew();
                }
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Show()
        {
            switch (connectionMode)
            {
                // Only shapes, no lines at all
                case 0:
                    break;

                // Small shapes of const size + white lines
                case 1:
                case 2:
                case 3:
                    size = connectionMode;
                    myPrimitive._LineInst.setInstanceCoords(X, Y, oldX, oldY);
                    myPrimitive._LineInst.setInstanceColor(1, 1, 1, lineA);
                    break;

                // Normal shapes + white lines
                case 4:
                case 5:
                case 6:
                    myPrimitive._LineInst.setInstanceCoords(X, Y, oldX, oldY);
                    myPrimitive._LineInst.setInstanceColor(1, 1, 1, lineA);
                    break;

                // Only ARGB lines
                case 7:
                case 8:
                    myPrimitive._LineInst.setInstanceCoords(X, Y, oldX, oldY);
                    myPrimitive._LineInst.setInstanceColor(R, G, B, A*2);
                    return;
            }

            switch (shape)
            {
                // Instanced squares
                case 0:
                    var rectInst = inst as myRectangleInst;

                    rectInst.setInstanceCoords(X - size, Y - size, 2 * size, 2 * size);
                    rectInst.setInstanceColor(R, G, B, A);
                    rectInst.setInstanceAngle(0);
                    break;

                // Instanced triangles
                case 1:
                    var triangleInst = inst as myTriangleInst;

                    triangleInst.setInstanceCoords(x, y, 2 * size, 0);
                    triangleInst.setInstanceColor(R, G, B, A);
                    break;

                // Instanced circles
                case 2:
                    var ellipseInst = inst as myEllipseInst;

                    ellipseInst.setInstanceCoords(x, y, 2 * size, 0);
                    ellipseInst.setInstanceColor(R, G, B, A);
                    break;

                // Instanced pentagons
                case 3:
                    var pentagonInst = inst as myPentagonInst;

                    pentagonInst.setInstanceCoords(x, y, 2 * size, 0);
                    pentagonInst.setInstanceColor(R, G, B, A);
                    break;

                // Instanced hexagons
                case 4:
                    var hexagonInst = inst as myHexagonInst;

                    hexagonInst.setInstanceCoords(x, y, 2 * size, 0);
                    hexagonInst.setInstanceColor(R, G, B, A);
                    break;
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Process(Window window)
        {
            uint cnt = 0, sleepCnt = 0;
            int threshold = 200;
            initShapes();

            // Disable VSYNC if needed
            //Glfw.SwapInterval(0);

            if (doClearBuffer)
            {
                glDrawBuffer(GL_FRONT_AND_BACK | GL_DEPTH_BUFFER_BIT);
                glClearColor(0, 0, 0, 1);
            }
            else
            {
                glDrawBuffer(GL_FRONT_AND_BACK);
            }

            while (list.Count < N)
            {
                list.Add(new myObj_043());
            }

            while (!Glfw.WindowShouldClose(window))
            {
                int cntActive = 0;

                processInput(window);

                // Swap fore/back framebuffers, and poll for operating system events.
                Glfw.SwapBuffers(window);
                Glfw.PollEvents();

                if (sleepCnt > 0)
                {
                    sleepCnt--;
                    System.Threading.Thread.Sleep(rand.Next(10));
                    continue;
                }

                if (doClearBuffer)
                {
                    glClear(GL_COLOR_BUFFER_BIT);
                }
                else
                {
                    dimScreen(dimAlpha/10, false);
                }

                // Render frame
                {
                    inst.ResetBuffer();
                    myPrimitive._LineInst.ResetBuffer();

                    for (int i = 0; i < list.Count; i++)
                    {
                        var obj = list[i] as myObj_043;

                        obj.Show();
                        obj.Move();

                        cntActive += obj.isActive ? 1 : 0;
                    }

                    if (connectionMode > 0)
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

                time_global += dtGlobal;

                // Wait untill every object finishes, then start from new point
                if (++cnt > threshold)
                {
                    generationAllowed = false;

                    if (cntActive == 0)
                    {
                        x0 = rand.Next(gl_Width);
                        y0 = rand.Next(gl_Height);

                        moveMode = isRandomMove ? rand.Next(moveModeCnt + 1) : moveMode;

                        updateConstants();
                        getNewColor();

                        time_global = 0.0f;

                        cnt = 0;
                        generationAllowed = true;

                        // Dim traces constantly
                        dimScreen(dimAlpha/3, false);

                        threshold = rand.Next(100) + 50;
                        sleepCnt = 66 + (uint)rand.Next(166);
                    }
                }

                System.Threading.Thread.Sleep(renderDelay);
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void initShapes()
        {
            myPrimitive.init_Rectangle();
            myPrimitive.init_LineInst(N);

            base.initShapes(shape, N, 0);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        // Dim the screen constantly
        private void dimScreen(float dimAlpha, bool useStrongerDimFactor = false)
        {
            int rnd = rand.Next(101), dimFactor = 1;

            if (useStrongerDimFactor && rnd < 11)
            {
                dimFactor = (rnd == 0) ? 5 : 2;
            }

            myPrimitive._Rectangle.SetAngle(0);

            // Shift background color just a bit, to hide long lasting traces of shapes
            myPrimitive._Rectangle.SetColor(rand.Next(5) * 0.01f, rand.Next(5) * 0.01f, rand.Next(5) * 0.01f, dimAlpha * dimFactor);
            myPrimitive._Rectangle.Draw(0, 0, gl_Width, gl_Height, true);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        // Update global constants
        private void updateConstants()
        {
            if (doUpdateConstants)
            {
                // Mostly, we'll want to stop processing when the object goes out of the screen
                // But some of the shapes will have objects that rotate around its center -
                // - and we don't want to stop those, as they still might return back to screen
                isBorderScared = true;

                switch (moveMode)
                {
                    // --- option 1 ---
                    case 0:
                    case 1:
                        si1 = rand.Next(10) + 1;
                        sf2 = (rand.Next(10) + 1) / 100.0f;
                        break;

                    case 2:
                    case 3:
                        si1 = rand.Next(20) + 1;
                        sf2 = (rand.Next(100) + 1) / 100.0f;
                        break;

                    case 4:
                        si1 = rand.Next(30) + 1;
                        sf2 = (rand.Next(300) + 1) / 100.0f;
                        break;

                    // --- option 2 ---
                    case 5:
                        break;

                    // --- option 3 ---
                    case 6:
                        sf2 = rand.Next(7) + 2;          // 0.2 - 8

                        if (rand.Next(11) > 3)
                            sf2 *= 0.1f;
                        break;

                    // --- option 4 ---
                    case 7:
                        sf2 = rand.Next(19) + 2;         // 0.2 - 2
                        sf2 *= 0.1f;
                        break;

                    // --- option 5 ---
                    case 8:
                        sf2 = rand.Next(30) + 1;
                        sf2 *= 0.1f;
                        break;

                    // --- option 6 ---
                    case 9:
                        dtCommon = 0.025f + rand.Next(5) * 0.025f;
                        sf2 = 0.5f + rand.Next(11) * 0.1f;
                        break;

                    case 10:
                        dtCommon = 0.5f + rand.Next(115) * 0.25f;
                        sf2 = 0.5f + rand.Next(11) * 0.1f;
                        break;

                    case 11:
                        dtCommon = 0.025f + rand.Next(5) * 0.025f;
                        sf2 = 1.0f + rand.Next(21) * 0.1f;
                        break;

                    case 12:
                        dtCommon = 0.5f + rand.Next(115) * 0.25f;
                        sf2 = 1.0f + rand.Next(21) * 0.1f;
                        break;

                    case 13:
                        dtCommon = 0.025f + rand.Next(5) * 0.025f;
                        sf2 = 1.5f + rand.Next(123) * 0.1f;
                        break;

                    case 14:
                        dtCommon = 0.5f + rand.Next(115) * 0.25f;
                        sf2 = 1.5f + rand.Next(123) * 0.1f;
                        break;

                    case 15:
                        dtCommon = 0.005f + rand.Next(7) * 0.0025f;
                        sf2 = 1.0f + rand.Next(10) * 0.5f;
                        break;

                    case 16:
                        dtCommon = 0.005f + rand.Next(7) * 0.0025f;
                        sf2 = 3.0f + rand.Next(33) * 0.5f;
                        break;

                    // --- option 7 ---
                    case 17:
                        dtCommon = (rand.Next(2) == 0) ? 0.1f : -0.1f;
                        sf2 = 0.5f;
                        sf2 += rand.Next(20) * 0.0125f;
                        sf2 *= 2.0f;
                        break;

                    case 18:
                        dtCommon = (rand.Next(2) == 0) ? 0.1f : -0.1f;
                        sf2 = 0.5f;
                        sf2 += rand.Next(100) * 0.125f;
                        break;

                    case 19:
                        dtCommon = (rand.Next(2) == 0) ? 0.1f : -0.1f;
                        dtCommon *= (rand.Next(101) * 0.025f + 1.0f);

                        sf2 = 0.5f;
                        sf2 += rand.Next(20) * 0.0125f;
                        sf2 *= (rand.Next(3) + 1);
                        break;

                    case 20:
                        dtCommon += 0.05f;
                        sf2 = 1.33f;
                        break;

                    case 21:
                        dtCommon += 0.001f * rand.Next(301);
                        sf2 = 1.33f;
                        break;

                    // --- option 8 ---
                    case 22:
                    case 23:
                        dtCommon = (rand.Next(2) == 0) ? 0.01f : -0.01f;
                        dtCommon *= (rand.Next(333) + 1) * 0.01f;

                        sf2 = rand.Next(201) + 50;
                        sf2 *= (moveMode == 22) ? 0.01f : 0.5f;
                        break;

                    // --- option 9 ---
                    case 24:
                        dtCommon += 0.05f;
                        sf2 = 1.33f;
                        break;

                    // --- option 10 ---
                    case 25:
                        dtCommon += 0.05f;
                        sf2 = 1.33f;
                        break;

                    // --- option 11 ---
                    case 26:
                        dtCommon = 0.01f;
                        dtCommon *= (rand.Next(30) + 5);

                        sf2 = rand.Next(201) + 100;
                        sf2 *= 0.01f;
                        break;

                    case 27:
                        dtCommon = (rand.Next(2) == 0) ? 0.1f : -0.1f;
                        dtCommon *= (rand.Next(33) + 1);

                        sf2 = rand.Next(201) + 100;
                        sf2 *= 0.0033f;
                        break;

                    // --- option 12 ---
                    case 28:
                        dtCommon = (rand.Next(2) == 0) ? 0.05f : -0.05f;
                        dtCommon *= (rand.Next(9) + 2);

                        sf2 = rand.Next(201) + 100;
                        sf2 *= 0.0025f;
                        break;

                    // --- option 13 ---
                    case 29:
                        dtCommon = 0.0001f;
                        sf2 = (rand.Next(15) + 11) * 0.05f;
                        sf3 = 10.0f;
                        break;

                    case 30:
                        dtCommon = 0.0001f;
                        sf2 = (rand.Next(15) + 11) * 0.05f;
                        sf3 = 4.0f;
                        break;

                    case 31:
                        dtCommon = 0.001f;
                        dtCommon *= rand.Next(101);

                        sf2 = (rand.Next(15) + 11) * 0.05f;
                        sf3 = 4.0f;
                        break;

                    case 32:
                        dtCommon = 0.001f;

                        sf3 = 2.1f;     // 2.1f - 3.9f
                        sf3 += rand.Next(200) * 0.01f;

                        sf2 = (rand.Next(25) + 11) * 0.05f;
                        break;

                    // --- option 14 ---
                    case 33:
                        dtCommon = (rand.Next(2) == 0) ? 0.001f : -0.001f;
                        dtCommon *= (rand.Next(50) + 1);

                        sf3 = 0.1f;
                        sf3 += rand.Next(200) * 0.01f;

                        sf2 = 2;
                        break;

                    // --- option 15 ---
                    case 34:
                        dtCommon = 0.001f;
                        sf3 = 0;
                        sf2 = (rand.Next(66) + 1) * 0.1f;
                        break;

                    case 35:
                        dtCommon = 0.001f;
                        sf3 = 0;
                        sf2 = (rand.Next(366) + 1) * 0.1f;
                        break;

                    case 36:
                        dtCommon = (rand.Next(2) == 0) ? 0.001f : -0.001f;
                        sf3 = 0.0001f;
                        sf3 *= rand.Next(333);
                        sf2 = (rand.Next(200) + 1) * 0.1f;
                        break;

                    // --- option 16 --- 
                    case 37:
                        dtCommon = (rand.Next(2) == 0) ? 1.0f : -1.0f;
                        dtCommon *= 0.25f;

                        sf3 = 12.0f;   // try various si1 and dt
                        sf2 = 0.25f + rand.Next(100) * 0.01f;
                        break;

                    case 38:
                        // So many different options, I don't know how to choose yet...
                        // todo: find a way
                        /*
                                            dtCommon *= 0.01f * rand.Next(1000);  // random
                                            sf3 = 12.0f;   // try various si1 and dt
                                            sf2 = 0.05f;
                        */
                        // Taking turns with large and small scale
                        if (si1 == 0)
                        {
                            si1 = 1;
                            dtCommon = (rand.Next(2) == 0) ? 1.0f : -1.0f;
                            dtCommon *= 0.01f * rand.Next(1000);
                            sf3 = 0.01f * (rand.Next(3001) + 1);
                            sf2 = 5.0f;
                        }
                        else
                        {
                            si1 = 0;
                            sf2 = 0.5f;
                        }

                        // some good ones. require that dtCommon == +-1.0
                        if (false) { dtCommon *= 123.321f; sf2 = 0.0075f; }
                        if (false) { dtCommon *= 75.4328f; sf2 = 0.00075f; }
                        if (false) { dtCommon *= 1.2573f; sf2 = 0.25f; }
                        if (false) { dtCommon *= 12.2591f; sf2 = 0.025f; }

                        //sf2 = 0.25f + rand.Next(100) * 0.01f;
                        break;

                    case 39:
                        dtCommon = (rand.Next(2) == 0) ? 1.0f : -1.0f;
                        dtCommon *= 0.25f;

                        sf3 = 118.0f;   // todo: try various sf3
                        sf2 = sf2 > 1 ? 0.5f : 5.0f;
                        break;

                    // --- option 17 ---
                    case 40:
                        dtCommon += 0.05f;      // todo: there are some nice shapes. Remember the dt
                        sf2 = 1.0f;             // also, looks like the shapes with the same set of params can be different. Look into this.
                        break;

                    case 41:
                        dtCommon = 21 * 0.05f;
                        sf2 = 1.0f;
                        break;

                    // --- option 18 ---
                    case 42:
                        dtCommon += (dtCommon == 0) ? 0.25f : 0.05f;
                        sf2 = 1.0f;
                        sf3 = 100.0f;           // todo: try changing this
                        break;

                    // --- option 19 ---
                    case 43:
                        dtCommon = 0.05f;
                        sf2 = 0.5f + 0.1f * rand.Next(11);
                        break;

                    // --- option 20 ---
                    case 44:
                        dtCommon = 0.08f;       // more straight <--> more curving towards the circle
                        dtCommon = 0.25f;       // todo: see if we already have this circlic pattern and this more straight one, and add it we don't

                        a = (rand.Next(2) == 0) ? 1.0f : -1.0f;
                        a *= 0.25f;                 // a's sign affects the direction of spiralling. Value +/- acts like b

                        b = (b == 0) ? 1.0f : b;    // b affects distribution of rays around the central point

                        b += 0.001f;
                        c += 0.01f;                 // c here affects straight/curve ratio of the spiral

                        sf2 = 0.33f;                // General size of the shape
                        break;

                    case 45:
                        dtCommon = 0.25f;

                        a = 0.0f;
                        b += 0.0001f;
                        c += 0.001f;

                        sf2 = 0.33f;
                        break;

                    case 46:
                        dtCommon = 0.25f;

                        a = 0.0f;
                        b += (b < 0.3f) ? 0.0001f : 0.0f;
                        c = 0.0f;

                        sf2 = 0.33f;
                        break;

                    case 47:
                        dtCommon = 0.25f;

                        a = 0.0f;
                        b += 0.001f;
                        c += 0.001f;

                        sf2 = 0.33f;
                        break;

                    // --- option 21 ---
                    case 48:
                        dtCommon = 0.001f * rand.Next(9999);

                        sf3 = 0.25f * rand.Next(100);
                        sf2 = 0.01f * (rand.Next(200) + 1);
                        break;

                    // --- option 22 ---
                    case 49:
                        dtCommon += 0.01f + rand.Next(51) * 0.01f;

                        sf2 = 0.01f * (rand.Next(100) + 1);
                        break;

                    // --- option 23-24-25 ---
                    case 50:
                    case 51:
                    case 52:
                        dtCommon = 0.05f;

                        sf2 = 0.025f * (rand.Next(50) + 1);
                        break;

                    // --- option 26 ---
                    case 53:
                        dtCommon = 0.01f;

                        sf2 = 0.1f * (rand.Next(50) + 1);
                        sf3 = 1.25f + rand.Next(10) * 0.25f;
                        break;

                    // --- option 27 ---
                    case 54:
                        dtCommon = (rand.Next(2) == 0) ? 1.0f : -1.0f;
                        dtCommon *= 0.01f;

                        sf2 = 1.0f + rand.Next(3000) * 0.01f;
                        sf3 = rand.Next(100) * 0.1f;
                        break;

                    // --- option 26 ---
                    case 55:
                        dtCommon = (rand.Next(2) == 0) ? 1.0f : -1.0f;
                        dtCommon *= 0.5f;

                        a = 5.0f + rand.Next(333) * 0.1f;

                        si1 = rand.Next(20) + 1;
                        sf2 = rand.Next(150) * 0.005f;
                        break;

                    // --- option 27 ---
                    case 56:
                        dtCommon = 0.01f;

                        a = rand.Next(30) * 0.01f;
                        a *= (rand.Next(2) == 0) ? 1.0f : -1.0f;
                        a += 1.0f;

                        b = rand.Next(30) * 0.01f;
                        b *= (rand.Next(2) == 0) ? 1.0f : -1.0f;
                        b += 1.0f;

                        sf2 = 1.0f + rand.Next(100) * 0.01f;
                        sf3 = 1.2f + rand.Next(200) * 0.01f;
                        sf2 *= (rand.Next(2) == 0) ? 1.0f : -1.0f;
                        sf3 *= (rand.Next(2) == 0) ? 1.0f : -1.0f;
                        break;

                    case 57:
                        // todo: fine tune it later
                        dtCommon = 0.001f;

                        a = rand.Next(30) * 0.01f;
                        a *= (rand.Next(2) == 0) ? 1.0f : -1.0f;
                        a += 1.0f;

                        b = rand.Next(30) * 0.01f;
                        b *= (rand.Next(2) == 0) ? 1.0f : -1.0f;
                        b += 1.0f;

                        sf2 = 1.0f + rand.Next(100) * 0.01f;
                        sf3 = 1.2f + rand.Next(200) * 0.01f;
                        sf2 *= (rand.Next(2) == 0) ? 1.0f : -1.0f;
                        sf3 *= (rand.Next(2) == 0) ? 1.0f : -1.0f;
                        break;

                    // --- option 28 ---
                    case 58:
                        isBorderScared = false;
                        // todo:
                        // - there's way more to it than just 1 option
                        // - also, check why does it break up on drawing near the edges of the screen

                        //dtCommon = 1.5f; sf2 = 33; a = 1.0f; sf3 = 3.0f;      // sf2 = [10 .. 2000]
                        //dtCommon = 1.5f; sf2 = 33; a = 1.0f; sf3 = 0.5f;
                        //dtCommon = 1.5f; sf2 = 133; a = 1.0f; sf3 = 0.5f;
                        //dtCommon = 0.66f; sf2 = 133; a = 1.0f; sf3 = 0.5f;
                        //dtCommon = 0.5f; sf2 = 33; a = 1.0f; sf3 = 0.5f;

                        // increasing dt will change the shape's form
                        dtCommon = 0.66f; sf2 = 133; a = 1.0f; sf3 = 0.5f;
                        break;

                    // --- option 29 ---
                    case 59:
                        dtCommon = 0.9f; sf2 = 3.0f; sf3 = 6.0f;
                        dtCommon = 0.1f; sf2 = 3.0f; sf3 = 3.0f;
                        break;

                    // --- option 30 ---
                    case 60:
                    case 61:
                        dtCommon = (moveMode == 60) ? 0 : 0.01f * rand.Next(500);

                        // General size of the shape
                        sf2 = (moveMode == 60) ? 0.33f : 0.66f + rand.Next(50) * 0.05f;

                        trigFunc1 = Math.Sin;
                        trigFunc2 = Math.Cos;

                        if (rand.Next(2) == 0)
                        {
                            trigFunc3 = Math.Tan; sf3 = 0.1f;
                        }
                        else
                        {
                            trigFunc3 = Math.Sin; sf3 = 1.0f;
                        }

                        a = 0.25f;
                        b = 1.55f;
                        break;

                    // --- option 31 ---
                    case 62:
                    case 63:
                    case 64:
                        dtCommon = 0.08f;
                        a = 0.25f;
                        c = 0.10f;
                        sf2 = 0.25f + rand.Next(101) * 0.01f;
                        break;

                    // --- option 32 ---
                    case 65:
                    case 66:
                    case 67:
                        isBorderScared = false;
                        dtCommon = 0.08f;

                        a = 0.25f;

                        if (moveMode == 65)
                        {
                            b = 1.55f;
                        }
                        else
                        {
                            b = (b == 0) ? 0.01f : b + 0.01f;
                        }

                        // c also is worth changing
                        c = 0.10f;          // c here affects straight/curve ratio of the spiral

                        // General size of the shape
                        sf2 = 1.0f + 0.01f * rand.Next(101);
                        break;

                    // --- option 33 ---
                    case 68:
                    case 69:
                        isBorderScared = false;
                        dtCommon = (moveMode == 68)
                                        ? 0.08f
                                        : rand.Next(1000) * 0.1f;

                        a = 0.1f + rand.Next(1000) * 0.001f;
                        b = 0.2f + rand.Next(100) * 0.1f;

                        sf2 = 15.0f + 0.1f * rand.Next(500);
                        break;

                    case 70:
                        isBorderScared = false;
                        dtCommon = 4.25f + rand.Next(45) * 0.01f;       // Dragon Tails
                        a = 1.1f;
                        b = 1.2f;
                        sf2 = 15.0f + 0.1f * rand.Next(100);
                        break;

                    case 71:
                        if (isFirstIteration)
                        {
                            isFirstIteration = false;
                            a = 1.0f + rand.Next(700) * 0.01f;
                            b = 1.0f + rand.Next(700) * 0.01f;
                            dtCommon = 0.0f;
                        }

                        isBorderScared = false;
                        dtCommon += 0.1f;                               // Ever increasing dt with fixed 'a' and 'b'
                        sf2 = 15.0f + 0.1f * rand.Next(100);
                        break;

                    // --- option 34 ---
                    case 72:
                        isBorderScared = false;
                        dtCommon += 0.1f;
                        a = 1.23f;
                        b = 1.32f;
                        c = 1.25f;
                        sf2 = 10 + 0.1f * rand.Next(33);
                        break;

                    case 73:
                        isBorderScared = false;
                        dtCommon += 0.1f;
                        a = 1.23f;
                        b = 1.32f;
                        c = 2.5f;
                        sf2 = 10 + 0.1f * rand.Next(33);
                        break;

                    // --- option 35 ---
                    case 74:
                    case 75:
                        if (isFirstIteration)
                        {
                            isFirstIteration = false;
                            dtCommon = 0.0f;
                        }

                        isBorderScared = false;
                        dtCommon += 0.01f;
                        a = (moveMode == 74) ? 0.1f : 1.1f;
                        sf2 = 10.0f + rand.Next(333) * 0.1f;
                        break;

                    case 76:
                        if (isFirstIteration)
                        {
                            isFirstIteration = false;
                            dtCommon = 0.0f;
                            a = rand.Next(1234) * 0.001f;
                        }

                        isBorderScared = false;
                        dtCommon += 0.0025f;
                        a += 0.01f;
                        sf2 = 10.0f + rand.Next(333) * 0.1f;
                        break;

                    case 77:
                        dtCommon = 0.08f;
                        a = 0.1f;
                        sf2 = 25.0f;
                        break;

                    // --- option 36 ---
                    case 78:
                        isBorderScared = false;
                        a = b = (rand.Next(300) + 33) * 0.1f;
                        sf3 = 1.0f;
                        break;

                    case 79:
                        if (isFirstIteration)
                        {
                            isFirstIteration = false;
                            sf3 = (rand.Next(10) + 1);
                        }

                        isBorderScared = false;
                        a = b = (rand.Next(300) + 33) * 0.1f;
                        break;

                    case 80:
                        if (isFirstIteration)
                        {
                            isFirstIteration = false;
                            c = (rand.Next(100) + 1) * 0.001f;
                            sf3 = 1.0f;
                        }

                        isBorderScared = false;
                        a = (rand.Next(300) + 33) * c;
                        b = (rand.Next(300) + 33) * c;

                        a *= (rand.Next(2) == 0) ? 1 : -1;
                        b *= (rand.Next(2) == 0) ? 1 : -1;
                        break;

                    // --- option 37 ---
                    case 81:
                        a = (rand.Next(333) + 1) * 0.1f;
                        b = (rand.Next(333) + 1) * 0.1f;
                        break;

                    // --- option 38 ---
                    case 82:
                    case 83:
                        a = b = (rand.Next(33) + 1) * 0.1f;
                        break;

                    // --- option 39 ---
                    case 84:
                        a = b = 0.25f + rand.Next(10) * 0.05f;
                        c = (rand.Next(333) + 1) * 0.1f;
                        break;

                    // --- option 40 ---
                    case 85:
                    case 86:
                        c = rand.Next(20) + 7;
                        break;

                    // --- option 41 ---
                    case 87:
                    case 88:
                        c = 1.0f + rand.Next(401) * 0.01f;
                        break;

                    // --- option 42 ---
                    case 89:
                        c = 10.0f + rand.Next(3000) * 0.01f;
                        break;

                    // --- option 43 ---
                    case 90:
                        c = 15 + rand.Next(60);
                        si1 = rand.Next(10000) + 1;
                        break;

                    // --- option 44 ---
                    case 91:
                        c = 15 + rand.Next(60);
                        si1 = rand.Next(10000) + 1;
                        break;

                    // --- option 45 ---
                    case 92:
                        if (isFirstIteration)
                        {
                            isFirstIteration = false;
                            dtCommon = 0.0001f;
                        }

                        dtCommon += 0.005f;
                        sf2 = (rand.Next(333) + 1) * 0.1f;
                        si1 = rand.Next(11) + 1;
                        break;

                    // --- option 46 ---
                    case 93:
                        dtCommon = 0.001f;
                        si1 = 20;
                        sf2 = 30.0f;
                        break;

                    case 94:
                        dtCommon = 0.001f;
                        si1 = 15;
                        sf2 = 30.0f;
                        break;

                    case 95:
                        dtCommon = 0.001f;
                        si1 = 25;
                        sf2 = 30.0f;
                        break;

                    case 96:
                        dtCommon = 0.0005f;
                        si1 = 66;
                        sf2 = 66.0f;
                        break;

                    case 97:
                        dtCommon = 0.01f;
                        si1 = 2;
                        sf2 = 20.0f;
                        break;

                    case 98:
                        if (isFirstIteration)
                        {
                            isFirstIteration = false;
                            c = 0.95f;
                        }

                        c += 0.05f;
                        break;

                    case 99:
                        if (isFirstIteration)
                        {
                            isFirstIteration = false;
                            dtCommon = 0.001f;
                        }

                        dtCommon += 0.0015f;
                        sf2 = 0.5f * (rand.Next(11) + 1) * 0.25f;
                        break;

                    // --- option 47 ---
                    case 100:
                        si1 = rand.Next(20) + 20;
                        sf2 = (rand.Next(150) + 13) * 0.1f;
                        break;

                    // --- option 48 ---
                    case 101:
                        isBorderScared = false;
                        dtCommon = 0.1f;

                        a = b = 1.0f + 0.01f * (rand.Next(200) + 1);
                        sf2 = (rand.Next(150) + 13) * 0.1f;
                        break;

                    case 102:
                    case 103:
                        if (isFirstIteration)
                        {
                            isFirstIteration = false;
                            dtCommon = 0.0f;
                        }

                        isBorderScared = false;
                        dtCommon += 0.005f;

                        a = b = 1.0f + 0.01f * (rand.Next(200) + 1);
                        sf2 = (rand.Next(150) + 13) * 0.1f;
                        break;

                    // --- option 49 ---
                    case 104:
                        dtCommon = 0.1f;
                        a = b = 10;
                        break;

                    // --- option 50 ---
                    case 105:
                        dtCommon = 0.1f;
                        a = 13.5f;
                        break;

                    // --- option 51 ---
                    case 106:
                        dtCommon = 0.1f;
                        a = 13.5f;
                        break;

                    // --- option 52 ---
                    case 107:
                        si1 = 3;
                        sf2 = 2.0f;
                        sf3 = 0.975f;
                        break;

                    case 108:
                        if (isFirstIteration)
                        {
                            isFirstIteration = false;
                            si1 = rand.Next(50) + 1;
                            sf2 = (float)rand.NextDouble() * 5;
                            sf3 = 0.9f + (float)rand.NextDouble() / 10;
                        }
                        break;

                    // --- option 53 ---
                    case 109:
                        if (isFirstIteration)
                        {
                            isFirstIteration = false;
                            sf2 = 0.25f + (float)rand.NextDouble() / 3;
                        }
                        break;

                    // --- option 54 ---
                    case 110:
                    case 111:
                    case 112:
                    case 113:
                    case 114:
                        sf2 = 1.0f;
                        sf3 = 0.001f * rand.Next(23);

                        if (isFirstIteration)
                        {
                            si2 = rand.Next(26) + 3;

                            do {
                                si1 = rand.Next(25) + 1;
                            } while (si1 >= si2);
                        }
                        break;

                    default:
                        isBorderScared = false;
                        break;
                }
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void getNewColor()
        {
            if (colorMode == 0)
            {
                colorPicker.getColorRand(ref R, ref G, ref B);
            }
            else
            {
                colorPicker.getColor(x0, y0, ref R, ref G, ref B);
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------
    }
};
