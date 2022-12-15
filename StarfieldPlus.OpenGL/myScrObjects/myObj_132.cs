using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;


/*
    - Splines

    -- mode 10 only works once
    -- mode 89 only starts once, then it draws outside of the screen, apparently
*/


namespace my
{
    public class myObj_132 : myObject
    {
        private static int N = 1;

        private enum ScreenMode { Active, Start = 10, ManualSwitch = 33, Transition = 125 };

        private static int max_dSize = 0, t = 0, tDefault = 0, mode = 0, si1 = 0, si2 = 0, repeatMode = 1;
        private static int [] prm_i = new int[5];
        private static bool isDimmableGlobal = true, isDimmableLocal = false;
        private static float sf1 = 0, sf2 = 0, sf3 = 0, sf4 = 0, sf5 = 0, sf6 = 0, sf7 = 0, sf8 = 0, fLifeCnt = 0, fdLifeCnt = 0;
        private static float a = 0, b = 0, c = 0;
        private static float dimAlpha = 0.05f;
        private static ScreenMode scrMode = ScreenMode.Start;

        private int maxSize = 0, R = 0, G = 0, B = 0, dA = 0, dA_Filling = 0;
        private float x, y, dx, dy, size, dSize, a1, r1, g1, b1, a2, r2, g2, b2, angle = 0, time1, time2, dt1, dt2, float_B, x1, y1, x2, y2, x3, y3, x4, y4;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_132()
        {
            generateNew();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time global initialization
        protected override void initGlobal()
        {
            colorPicker = new myColorPicker(gl_Width, gl_Height);
            list = new List<myObject>();

            doClearBuffer = false;

            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time initialization
        private void initLocal()
        {
            max_dSize = rand.Next(15) + 3;
            isDimmableGlobal = rand.Next(2) == 0;
            tDefault = 33;

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            height = 800;

            string str_params = "";
            for (int i = 0; i < prm_i.Length; i++)
            {
                str_params += i == 0 ? $"{prm_i[i]}" : $", {prm_i[i]}";
            }

            string str = $"Obj = myObj_132\n\n" +
                            $"N = {N} of {list.Count}\n" +
                            $"mode = {mode}\n" +
                            $"repeatMode = {repeatMode}\n" +
                            $"dimAlpha = {dimAlpha}\n" +
                            $"dSize = {dSize}\n" +
                            $"fLifeCnt = {fLifeCnt}\n" +
                            $"param: [{str_params}]\n\n" +
                            $"file: {colorPicker.GetFileName()}" +
                            $""
                ;
            return str;
        }

        // ---------------------------------------------------------------------------------------------------------------

        // 
        protected override void setNextMode()
        {
            initLocal();
            generateNew();

            scrMode = ScreenMode.ManualSwitch;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void generateNew()
        {
            // Reset parameter values
            {
                for (int i = 0; i < prm_i.Length; i++)
                    prm_i[i] = 0;
            }

            // Restore dim speed to its original value
            dimAlpha = 0.01f / (rand.Next(10) + 1);

            fLifeCnt = 255.0f;
            fdLifeCnt = 0.25f;

            // Number of particle iterations between 2 consequent frame renders
            if (myUtils.randomChance(rand, 1, 13))
            {
                repeatMode = rand.Next(123) + 1;
            }
            else
            {
                repeatMode = rand.Next(7) + 1;
            }

            // Adjust dimming rate for the repeating speed
            if (myUtils.randomChance(rand, 1, 2))
            {
                dimAlpha /= repeatMode;
            }
            else
            {
                dimAlpha *= repeatMode;
            }

            x = rand.Next(gl_Width);
            y = rand.Next(gl_Height);

            // Color to draw the lines
            {
                colorPicker.getColor(x, y, ref r1, ref g1, ref b1);
                colorPicker.getColorRand(ref r2, ref g2, ref b2);

                if (r1 + g1 + b1 < 0.25f)
                {
                    r1 += 0.1f; g1 += 0.1f; b1 += 0.1f;
                }

                if (r2 + g2 + b2 < 0.25f)
                {
                    r2 += 0.1f; g2 += 0.1f; b2 += 0.1f;
                }
            }

            maxSize = rand.Next(333) + 33;
            mode = rand.Next(91);
            isDimmableGlobal = rand.Next(2) == 0;
            isDimmableLocal = false;

            t = tDefault;
            t -= isDimmableGlobal ? 13 : 0;

            //mode = 91;

#if false
            fdLifeCnt = 0.01f;
            mode= 1300;
            mode= 91;
            t = 3;
#endif

            size = 1;
            dSize = rand.Next(max_dSize) + 1;
            dSize = 0.1f + 0.1f * rand.Next(max_dSize * 10);
            dA = rand.Next(5) + 1;
            dA = 1;
            dA_Filling = rand.Next(5) + 2;
            float_B = 1.0f;

            time1 = 0.0f;
            time2 = 0.0f;
            dt1 = 0.1f;
            dt2 = 0.01f;

            scrMode = (scrMode == ScreenMode.Start) ? ScreenMode.Start : ScreenMode.Transition;

            setUpConstants();

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void setUpConstants()
        {
            switch (mode)
            {
                case 00:
                    fdLifeCnt = 0.5f;
                    dSize = 0.1f + 0.1f * rand.Next(max_dSize * 10);
                    dimAlpha = 0.02f;
                    break;

                case 01:
                    fdLifeCnt = 0.5f;
                    dSize = 0.1f + 0.1f * rand.Next(max_dSize * 10);
                    dimAlpha = 0.005f;
                    break;

                case 02:
                    fdLifeCnt = 0.5f;
                    dSize = 0.1f + 0.1f * rand.Next(max_dSize * 10);
                    dimAlpha = 0.005f;
                    break;

                case 03:case 04:case 05:
                    fdLifeCnt = 0.5f;
                    dimAlpha = 0.005f + 0.005f * rand.Next(5);
                    break;

                case 09:case 10:case 11:case 12:case 13:case 14:
                case 15:case 16:case 17:case 18:case 19:case 20:
                    si1 = 50;
                    dSize = 0.5f + 0.1f * rand.Next(max_dSize * 10);
                    break;

                case 44:
                    sf1 = rand.Next(333) + 100;
                    sf2 = rand.Next(10) + 10;
                    sf3 = rand.Next(100) * 0.25f;
                    fdLifeCnt = 0.5f;
                    break;

                case 45:case 46:case 47:
                    sf1 = rand.Next(333) + 100;
                    sf2 = rand.Next(333) + 10;
                    sf3 = rand.Next(100) * 0.25f;
                    fdLifeCnt = 0.5f;
                    break;

                case 48:
                    sf1 = rand.Next(333) + 100;
                    sf2 = rand.Next(333) + 100;
                    sf3 = rand.Next(1000) * 0.05f;
                    sf4 = rand.Next(1000) * 0.05f;
                    fdLifeCnt = 0.5f;
                    break;

                case 49:case 50:case 51:case 52:case 53:case 54:case 55:case 56:
                case 57:case 58:case 59:case 60:case 61:case 62:case 63:case 64:
                case 65:case 66:case 67:case 68:case 69:case 70:case 71:case 72:
                case 73:case 74:case 75:case 76:
                    constSetup1();
                    t = 11;
                    break;

                case 77:
                    constSetup2();
                    t = 11;
                    break;

                case 78:
                    constSetup3();
                    t = 11;
                    break;

                case 79:
                    constSetup4();
                    t = 11;
                    break;

                case 80:
                    constSetup1();
                    sf1 = 333;
                    sf2 = 666;
                    sf3 = 0.05f * (rand.Next(50) + 1);
                    sf4 = 0.05f * (rand.Next(50) + 1);
                    t = 11;
                    break;

                case 81:
                case 82:
                    constSetup1();

                    switch (rand.Next(4))
                    {
                        case 0: x1 = -100; break;
                        case 1: x1 = gl_x0; break;
                        case 2: x1 = gl_Width + 100; break;
                        case 3: x1 = rand.Next(gl_Width); break;
                    }

                    si1 = rand.Next(150) + 5;
                    si2 = rand.Next(500);
                    sf1 = 0.1f * (rand.Next(40) + 3);
                    sf2 = rand.Next(gl_y0);

                    t = 3; // tmp, remove later
                    break;

                case 83:
                    constSetup5();
                    t = 3;
                    break;

                case 84:
                    constSetup5();
                    t = 3;
                    break;

                case 85:
                case 86:
                    constSetup5();

                    x1 = rand.Next(gl_Width);
                    y1 = rand.Next(gl_Height);

                    x2 = rand.Next(gl_Width);
                    y2 = rand.Next(gl_Height);

                    sf1 = 0.001f * rand.Next(1111) * rand.Next(333) * myUtils.randomSign(rand);
                    sf2 = 0.001f * rand.Next(1111) * rand.Next(333) * myUtils.randomSign(rand);
                    sf3 = 0.001f * rand.Next(1111) * rand.Next(333) * myUtils.randomSign(rand);
                    sf4 = 0.001f * rand.Next(1111) * rand.Next(333) * myUtils.randomSign(rand);

                    t = 3;
                    scrMode = ScreenMode.Active;
                    break;

                case 87:
                    constSetup6();
                    break;

                case 88:
                    constSetup1();

                    a = rand.Next(2);

                    sf5 = 0.5f + 0.5f * rand.Next(50);
                    sf6 = 0.5f + 0.5f * rand.Next(50);

                    si1 = rand.Next(20) + 10;
                    si2 = rand.Next(20) + 10;
                    break;

                case 89:
                    constSetup1();

                    a = 2 + rand.Next(10);
                    si1 = rand.Next(4);

                    sf1 = 0.01f + 0.01f * rand.Next(100);
                    sf2 = 0.5f + 0.5f * rand.Next(50);

                    scrMode = ScreenMode.Active;
                    break;

                case 90:
                    constSetup1();
                    fdLifeCnt = 0.05f;
                    sf4 = 0.01f + rand.Next(21) * 0.01f;

                    //invalidateRate = 2; removed this, see what it was doing before
                    isDimmableLocal = true;

                    a = rand.Next(1111);

                    x1 = gl_x0;
                    y1 = gl_y0;
                    x3 = gl_x0 - gl_Height / 3 + rand.Next(2 * gl_Height / 3);
                    y3 = gl_y0 - gl_Height / 3 + rand.Next(2 * gl_Height / 3);
                    x4 = -10;
                    y4 = -10;
                    t = 3;
                    break;

                case 91:
                    sf1 = 1.0f + myUtils.randFloat(rand) * rand.Next(3);            // elliptic factor 1
                    sf2 = 1.0f + myUtils.randFloat(rand) * rand.Next(3);            // elliptic factor 2
                    sf3 = rand.Next(555) + 333;                                     // radius 1
                    sf4 = rand.Next(555) + 333;                                     // radius 2
                    prm_i[0] = rand.Next(4);                                        // Circle vs ellipse (4 modes)
                    prm_i[1] = rand.Next(7);                                        // time factors for sin/cos (see switch-case just below)
                    prm_i[2] = rand.Next(6);                                        // Radius changing mode

                    dt1 = 0.001f * (rand.Next(11) + 1);
                    si1 = si2 = 0;

                    switch (prm_i[1])
                    {
                        case 0: case 1: case 2:
                            prm_i[1] = 101 * (rand.Next(10) + 1);
                            break;

                        case 3: case 4:
                            prm_i[1] = 101 * (rand.Next(33) + 1);
                            break;

                        case 5:
                            prm_i[1] = 100 * (rand.Next(10) + 1) + rand.Next(10) + 1;
                            break;

                        case 6:
                            prm_i[1] = 100 * (rand.Next(33) + 1) + rand.Next(33) + 1;
                            break;
                    }

                    break;

                case 1300:
                    constSetup1();
                    t = 3;      // tmp, remove later
                    break;
            }
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            if (scrMode == ScreenMode.Active)
            {
                size  += dSize;
                time1 += dt1;
                time2 += dt2;

                dx = (float)(Math.Sin(time1)) * 5 * size / 10;
                dy = (float)(Math.Cos(time1)) * 5 * size / 10;

                x += dx;
                y += dy;

                move_0();

                if ((fLifeCnt -= fdLifeCnt / repeatMode) < 0)
                {
                    generateNew();
                }
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void move_0()
        {
            float dx2 = (float)(Math.Cos(time1)) * 10;
            float dy2 = (float)(Math.Sin(time1)) * 10;

            //g.DrawLine(p, X, Y, Width/2, 10);
            //g.DrawLine(p, Width / 2, 10, Y, X);
            //g.DrawLine(p, X/2, Y/2, Size + dx, Size + dy);
            //g.DrawLine(p, X * dx2, Y * dy2, Size * dx, Size * dy);
            //g.DrawLine(p, X - Size, Y - Size, 2 * Size + dx, 2 * Size + dy);
            //g.DrawLine(p, X, Y, 2 * Size + dx, 2 * Size + dy);

            switch (mode)
            {
                case 00:
                    x1 = 2 * size;
                    y1 = 2 * size - dx;
                    x2 = size * dx;
                    y2 = size * dy;
                    break;

                case 01:
                    x1 = size + dx2;
                    y1 = size/2 + dy2;
                    x2 = x/2;
                    y2 = y/2;
                    break;

                case 02:
                    x1 = size + dx2;
                    y1 = size + dy2;

                    x1 = size;
                    y1 = 1 * gl_Height/5 + dy2 * dx / 100;

                    x2 = x / 2;
                    y2 = y / 2;
                    break;

                case 03:
                    x1 = size + dx2;
                    y1 = size + dy2;
                    x2 = gl_Width - size + dx2;
                    y2 = gl_Height - size + dy2;

                    x3 = gl_Width - size + dx2;
                    y3 = size + dy2;
                    x4 = size + dx2;
                    y4 = gl_Height - size + dy2;
                    break;

                case 04:
                    x1 = size + dx2 * dx/10;
                    y1 = size + dy2;
                    x2 = gl_Width - size + dx2 * dx/10;
                    y2 = gl_Height - size + dy2;

                    x3 = gl_Width - size + dx2 * dx/10;
                    y3 = size + dy2;                        // * dy/10;
                    x4 = size + dx2 * dx/10;
                    y4 = gl_Height - size + dy2;
                    break;

                case 05:
                    x1 = size + dx2;
                    y1 = size + dy2 * dy / 20;

                    x1 += float_B;

                    x2 = gl_Width - x1;
                    y2 = gl_Height - y1;

                    float_B += 1.123f;  // try changing this value
                    break;

                case 06:
                    x1 = size + dx2;
                    y1 = size + dy2 * dy / 20;

                    x1 += (float)Math.Sin(float_B) * 10;
                    y1 += (float)Math.Cos(time1) * 5;

                    x2 = gl_Width  - x1;
                    y2 = gl_Height - y1;

                    float_B += 0.23f;
                    break;

                case 07:
                    x1 = size + dx2;
                    y1 = size + dy2 * dy/20;

                    x1 += (float)Math.Sin(float_B) * 10;
                    y1 += (float)Math.Cos(time1) * 5;

                    x1 += 2 * gl_Width / 5;

                    x2 = gl_Width - x1;
                    y2 = y1;

                    float_B += 0.23f;
                    break;

                case 08:
                    x1 = size + dx2;
                    y1 = size + dy2;
                    x2 = 4 * gl_Width /5 + dx2 * size/20;
                    y2 = 2 * gl_Height/5 + dy2 * size/20;
                    break;

                case 09:
                    x1 = 1 * gl_Width  / 5 + dx2 * size / si1;
                    y1 = 1 * gl_Height / 2 + dy2 * size / si1;
                    x2 = 4 * gl_Width  / 5 - dx2 * size / si1;
                    y2 = 1 * gl_Height / 2 - dy2 * size / si1;
                    break;

                case 10:
                    x1 = 1 * gl_Width  / 5 + dx2 * size / si1;
                    y1 = 1 * gl_Height / 2 - dy2 * size / si1;
                    x2 = 4 * gl_Width  / 5 + dx2 * size / si1;
                    y2 = 1 * gl_Height / 2 - dy2 * size / si1;
                    break;

                case 11:
                    x1 = 1 * gl_Width  / 5 - dy2 * size / si1;
                    y1 = 1 * gl_Height / 2 + dx2 * size / si1;
                    x2 = 4 * gl_Width  / 5 + dx2 * size / si1;
                    y2 = 1 * gl_Height / 2 - dy2 * size / si1;
                    break;

                case 12:
                    x1 = 1 * gl_Width  / 5 + dx  * size / si1;
                    y1 = 1 * gl_Height / 2 + dy  * size / si1;
                    x2 = 4 * gl_Width  / 5 + dx2 * size / si1;
                    y2 = 1 * gl_Height / 2 - dy2 * size / si1;
                    break;

                case 13:
                    x1 = 1 * gl_Width  / 5 + dx  * size / si1 / si1;
                    y1 = 1 * gl_Height / 2 + dy  * size / si1 / si1;
                    x2 = 4 * gl_Width  / 5 + dx2 * size / si1;
                    y2 = 1 * gl_Height / 2 - dy2 * size / si1;
                    break;

                case 14:
                    x1 = 1 * gl_Width  / 5 - dx  * size / si1 / si1;
                    y1 = 1 * gl_Height / 2 - dy  * size / si1 / si1;
                    x2 = 4 * gl_Width  / 5 + dx2 * size / si1;
                    y2 = 1 * gl_Height / 2 - dy2 * size / si1;
                    break;

                case 15:
                    x1 = 1 * gl_Width  / 5 + dx  * size / si1 / si1;
                    y1 = 1 * gl_Height / 2 - dy  * size / si1 / si1;
                    x2 = 4 * gl_Width  / 5 + dx2 * size / si1;
                    y2 = 1 * gl_Height / 2 - dy2 * size / si1;
                    break;

                case 16:
                    x1 = 1 * gl_Width  / 5 + dx  * size / si1 / 2;
                    y1 = 1 * gl_Height / 2 - dy  * size / si1 / 2;
                    x2 = 4 * gl_Width  / 5 + dx2 * size / si1 / 1;
                    y2 = 1 * gl_Height / 2 - dy2 * size / si1 / 1;
                    break;

                case 17:
                    x1 = 1 * gl_Width  / 5 + dx  * size / si1 / 2;
                    y1 = 1 * gl_Height / 2 - dy  * size / si1 / 2;
                    x2 = 4 * gl_Width  / 5 + dx2 * size / si1 * float_B;
                    y2 = 1 * gl_Height / 2 - dy2 * size / si1 * float_B;

                    float_B += 0.0001f;
                    break;

                case 18:
                    x1 = 1 * gl_Width  / 5 + dx  * size / si1 / 2;
                    y1 = 1 * gl_Height / 2 - dy  * size / si1 / 2;
                    x2 = 4 * gl_Width  / 5 + dx2 * 33;
                    y2 = 1 * gl_Height / 2 - dy2 * 33;
                    break;

                case 19:
                    x1 = 1 * gl_Width  / 5 + dx  * size / si1 / 33;
                    y1 = 1 * gl_Height / 2 - dy  * size / si1 / 33;
                    x2 = 4 * gl_Width  / 5 + dx2 * 33;
                    y2 = 1 * gl_Height / 2 - dy2 * 33;
                    break;

                case 20:
                    x1 = 1 * gl_Width  / 5 + dx  * size / si1 / 33;
                    y1 = 1 * gl_Height / 2 - dy  * size / si1 / 33;
                    x2 = 4 * gl_Width  / 5 + dx2 * 33 / float_B;
                    y2 = 1 * gl_Height / 2 - dy2 * 33 / float_B;

                    float_B += 0.001f;
                    break;

                case 21:
                    x1 = 1 * gl_Width  / 5 + dx2 * 33;
                    y1 = 1 * gl_Height / 2 - dy2 * 33;
                    x2 = 4 * gl_Width  / 5 + dx2 * 33;
                    y2 = 1 * gl_Height / 2 - dy2 * 33;

                    float_B += 0.001f;
                    break;

                case 22:
                    x1 = 1 * gl_Width  / 5 + dy2 * 33 / float_B;
                    y1 = 1 * gl_Height / 2 - dx2 * 33 / float_B;
                    x2 = 4 * gl_Width  / 5 + dx2 * 33 * float_B;
                    y2 = 1 * gl_Height / 2 - dy2 * 33 * float_B;

                    float_B += 0.01f;
                    break;

                case 23:
                    x1 = 1 * gl_Width  / 5 - dy2 * 33 / float_B;
                    y1 = 1 * gl_Height / 2 - dx2 * 33 / float_B;
                    x2 = 4 * gl_Width  / 5 + dx2 * 33 * float_B;
                    y2 = 1 * gl_Height / 2 - dy2 * 33 * float_B;

                    float_B += 0.01f;
                    break;

                case 24:
                    x1 = 1 * gl_Width  / 5 + dx2 * 33 * (float)Math.Sin(float_B);
                    y1 = 1 * gl_Height / 2 + dy2 * 33 * (float)Math.Sin(float_B);

                    x2 = 4 * gl_Width  / 5 + dx2 * 33 * (float)Math.Sin(float_B);
                    y2 = 1 * gl_Height / 2 + dy2 * 33 * (float)Math.Sin(float_B);

                    float_B += 0.01f;
                    break;

                case 25:
                    x1 = 1 * gl_Width  / 5 + dx2 * 33 * (float)Math.Sin(float_B);
                    y1 = 2 * gl_Height / 5 + dy2 * 33 * (float)Math.Sin(float_B);

                    x2 = 4 * gl_Width  / 5 + dy2 * 33 * (float)Math.Sin(float_B);
                    y2 = 3 * gl_Height / 5 + dx2 * 33 * (float)Math.Sin(float_B);

                    float_B += 0.01f;
                    break;

                case 26:
                    x1 = 1 * gl_Width  / 5 + dx2 * 33 * (float)Math.Sin(float_B);
                    y1 = 1 * gl_Height / 2 + dy2 * 33;

                    x2 = 4 * gl_Width  / 5 + dy2 * 33 * (float)Math.Sin(float_B);
                    y2 = 1 * gl_Height / 2 + dx2 * 33;

                    float_B += 0.01f;
                    break;

                case 27:
                    x1 = 1 * gl_Width  / 5 + dx2 * 33 * (float)Math.Sin(float_B);
                    y1 = 1 * gl_Height / 2 + dy2 * 33 / float_B;

                    x2 = 4 * gl_Width  / 5 + dx2 * 33 * (float)Math.Sin(float_B);
                    y2 = 1 * gl_Height / 2 + dy2 * 33 / float_B;

                    float_B += 0.01f;
                    break;

                case 28:
                    x1 = 1 * gl_Width  / 5 + 600 * (float)Math.Sin(float_B);
                    y1 = 1 * gl_Height / 2 + dy2 * 66 / float_B;

                    x2 = 4 * gl_Width  / 5 - 600 * (float)Math.Sin(float_B);
                    y2 = 1 * gl_Height / 2 - dy2 * 33 / float_B;

                    float_B += 0.01f;       // try changing it
                    //float_B += 0.001f;
                    break;

                case 29:
                    x1 = (int)(Math.Sin(float_B) * 5.0f) - 50;
                    y1 = gl_Height / 2 + (int)(dy2 * 33 / float_B) * 10;

                    float_B += 0.01f;

                    x2 = 2 * gl_Width / 5 - 1500 * (float)Math.Sin(float_B);
                    y2 = 1 * gl_Height / 2 - dy2 * 33 / float_B;
                    break;

                case 30:
                    x1 = (int)(Math.Sin(float_B) * 5.0f) - 100;
                    y1 = gl_Height / 2 + (int)(dy2 * 33 / float_B) * 10 - 10000;

                    float_B += 0.01f;

                    x2 = 6 * gl_Width / 12 - 1500 * (float)(Math.Sin(Math.Cos(float_B) * Math.Cos(1 / float_B)) * 1.25f);
                    y2 = 3 * gl_Height / 4 - dy2 * 33 / float_B;
                    break;

                case 31:
                    x1 = (int)(Math.Sin(float_B) * 5.0f) - 100;
                    y1 = 1 * gl_Height / 2 + (int)(dy2 * 33 / float_B) * 10;

                    float_B = 1.0f;

                    x2 = 8 * gl_Width / 12 - 1300 * (float)(Math.Sin(Math.Cos(float_B) + Math.Cos(time1)) * 1.25f);
                    y2 = 1 * gl_Height / 2 - dy2 * 33 / float_B;
                    break;

                case 32:
                    x1 = (int)(Math.Sin(float_B) * 5.0f) - 100;
                    y1 = 1 * gl_Height / 2 + (int)(dy2 * 33 / float_B) * 10;

                    float_B = 1.0f;

                    x2 = 7 * gl_Width  / 12 - 1000 * (float)(Math.Sin(Math.Cos(time1) + Math.Cos(time1)) * 1.25f);
                    y2 = 1 * gl_Height / 2 - dy2 * 33 / float_B;
                    break;

                case 33:
                    x1 = (int)(Math.Sin(float_B) * 5.0f) - 100;
                    y1 = 1 * gl_Height / 2 + (int)(dy2 * 33 / float_B) * 10;

                    float_B = 1.0f;

                    x2 = 7 * gl_Width  / 12 - 1000 * (float)(Math.Sin(Math.Sin(time1) + Math.Cos(time1)) * 1.25f);
                    y2 = 1 * gl_Height / 2 - dy2 * 33 / float_B;
                    break;

                case 34:
                    x1 = (int)(Math.Sin(float_B) * 5.0f) - 100;
                    y1 = 1 * gl_Height / 2 + (int)(dy2 * 33 / float_B) * 10;

                    float_B = 1.0f;

                    // try different values of 2
                    x2 = 7 * gl_Width  / 12 - 1000 * (float)(Math.Sin(Math.Sin(2 * time1) + Math.Cos(2 * time1)) * 1.25f);
                    y2 = 1 * gl_Height / 2 - dy2 * 33 / float_B;
                    break;

                case 35:
                    x1 = (int)(Math.Sin(float_B) * 5.0f) - 100;
                    y1 = 1 * gl_Height / 2 + (int)(dy2 * 33 / float_B) * 10;

                    float_B = 1.0f;

                    // try different values of 2
                    x2 = 7 * gl_Width  / 12 - 1000 * (float)(Math.Sin(2.5f * Math.Sin(time1)) * 1.25f);
                    y2 = 1 * gl_Height / 2 - dy2 * 33 / float_B;
                    break;

                case 36:
                    x1 = (int)(Math.Sin(float_B) * 5.0f);
                    y1 = 1 * gl_Height / 2 + (int)(dy2 * 33 / float_B) * 2; // <-- diff

                    float_B = 1.0f;

                    x2 = 8 * gl_Width / 12 - 1300 * (float)(Math.Sin(Math.Cos(float_B) + Math.Cos(time1)) * 1.25f);
                    y2 = 1 * gl_Height / 2 - dy2 * 33 / float_B;
                    break;

                case 37:
                    x1 = (int)(Math.Sin(float_B) * 5.0f) + 1111;
                    y1 = 1 * gl_Height / 2 + (int)(dy2 * 33 / float_B) * 2; // <-- diff

                    float_B = 1.0f;

                    x2 = 8 * gl_Width / 12 - 1300 * (float)(Math.Sin(Math.Cos(float_B) + Math.Cos(time1)) * 1.25f);
                    y2 = 1 * gl_Height / 2 - dy2 * 33 / float_B;
                    break;

                case 38:
                    x1 = (int)(Math.Sin(float_B) * 5.0f) + 2500; // randomize this value 2500
                    y1 = 1 * gl_Height / 2 + (int)(dy2 * 33 / float_B) * 2; // <-- diff

                    float_B = 1.0f;

                    x2 = 8 * gl_Width / 12 - 1300 * (float)(Math.Sin(Math.Cos(float_B) + Math.Cos(time1)) * 1.25f);
                    y2 = 1 * gl_Height / 2 - dy2 * 33 / float_B;
                    break;

                case 39:
                    x1 = (int)(Math.Sin(float_B) * 5.0f) + 2500; // randomize this value 2500
                    y1 = 1 * gl_Height / 2 + (int)(dy2 * 33 / float_B) * 2; // <-- diff

                    float_B = 1.0f;

                    x2 = 6 * gl_Width / 12 - 1300 * (float)(Math.Sin(Math.Cos(0.2f * time1) + Math.Cos(time1)) * 1.0f);
                    y2 = 1 * gl_Height / 2 - dy2 * 33 / float_B;
                    break;

                case 40:
                    x1 = (float)(Math.Sin(time1 + time1) * 100.0f) + 2500; // randomize this value 2500
                    y1 = 1 * gl_Height / 2 + (int)(dy2 * 33 / float_B) * 2; // <-- diff

                    float_B = 1.0f;

                    x2 = 8 * gl_Width / 12 - 1300 * (float)(Math.Sin(Math.Cos(float_B) + Math.Cos(time1)) * 1.25f);
                    y2 = 1 * gl_Height / 2 - dy2 * 33 / float_B;
                    break;

                case 41:
                    x1 = (float)(Math.Sin(time1 + 1.33f) * 100.0f) + 2500; // randomize this value 2500
                    y1 = 1 * gl_Height / 2 + (int)(dy2 * 33 / float_B) * 2; // <-- diff

                    float_B = 1.0f;

                    x2 = 8 * gl_Width / 12 - 1300 * (float)(Math.Sin(Math.Cos(float_B) + Math.Cos(time1)) * 1.25f);
                    y2 = 1 * gl_Height / 2 - dy2 * 33 / float_B;
                    break;

                case 42:
                    x1 = (float)(Math.Sin(time1 + 2.0f) * 300.0f) + 2500; // randomize this value 2500
                    y1 = 1 * gl_Height / 2 + (float)(dy2 * 33 / float_B) * 0.5f; // <-- diff

                    float_B = 1.0f;

                    x2 = 8 * gl_Width / 12 - 1300 * (float)(Math.Sin(Math.Cos(float_B) + Math.Cos(time1)) * 1.25f);
                    y2 = 1 * gl_Height / 2 - dy2 * 33 / float_B;
                    break;

                case 43:
                    x1 = (float)(Math.Sin(time1 + 2.0f) * 1500.0f) + 2000; // randomize this value 2500
                    y1 = 1 * gl_Height / 2 + (float)(dy2 * 33 / float_B) * 1.5f; // <-- diff

                    float_B = 1.0f;

                    x2 = 8 * gl_Width / 12 - 1300 * (float)(Math.Sin(Math.Cos(float_B) + Math.Cos(time1)) * 1.25f);
                    y2 = 1 * gl_Height / 2 - dy2 * 33 / float_B;
                    break;

                case 44:
/*
                    x1 = 1 * Width  / 5 + (float)(Math.Sin(time)) * sf1;
                    y1 = 1 * Height / 2 + (float)(Math.Cos(time)) * sf1;

                    x2 = 4 * Width  / 5 + (float)(Math.Cos(time)) * sf2;
                    y2 = 1 * Height / 2 + (float)(Math.Sin(time)) * sf2;
*/
                    float_B = 1.0f;

                    // Changing shape
                    x1 = sf3 * (float)(Math.Sin(time1 + sf2) * sf1) + sf1 * 10;
                    y1 = 1 * gl_Height / 2 + sf3 * (float)(dy2 * 33 / float_B) * 1;

                    // Static oval shape
                    x2 = 8 * gl_Width / 12 - 1300 * (float)(Math.Sin(Math.Cos(float_B) + Math.Cos(time1)) * 1.25f);
                    y2 = 1 * gl_Height / 2 - dy2 * 33 / float_B;
                    break;

                case 45:
                    x1 = 1 * gl_Width  / 5 + (float)(Math.Sin(time1)) * sf1;
                    y1 = 1 * gl_Height / 2 + (float)(Math.Cos(time1)) * sf1;

                    x2 = 4 * gl_Width  / 5 + (float)(Math.Cos(time1)) * sf2;
                    y2 = 1 * gl_Height / 2 + (float)(Math.Sin(time1)) * sf2;
                    break;

                case 46:
                    x1 = 1 * gl_Width  / 5 + (float)(Math.Sin(time1)) * sf1;
                    y1 = 1 * gl_Height / 2 + (float)(Math.Cos(time1)) * sf1;

                    x2 = 4 * gl_Width  / 5 + (float)(Math.Cos(fLifeCnt + time1)) * sf2;
                    y2 = 1 * gl_Height / 2 + (float)(Math.Sin(fLifeCnt)) * sf2;
                    break;

                case 47:
                    x1 = 1 * gl_Width  / 5 + (float)(Math.Sin(time1)) * sf1;
                    y1 = 1 * gl_Height / 2 + (float)(Math.Cos(time1)) * sf1;

                    x2 = 4 * gl_Width  / 5 + (float)(Math.Cos(fLifeCnt + time1)) * sf2;
                    y2 = 1 * gl_Height / 2 + (float)(Math.Sin(fLifeCnt + time1)) * sf2;
                    break;

                case 48:
                    x1 = 1 * gl_Width  / 5 + (int)(Math.Sin(time1 * 1.0f) * sf3) / sf3 * sf1;
                    y1 = 1 * gl_Height / 2 + (int)(Math.Cos(time1 * 1.0f) * sf3) / sf3 * sf1;

                    x2 = 4 * gl_Width  / 5 + (int)(Math.Sin(time1 * 1.1f) * sf4) / sf4 * 333;
                    y2 = 1 * gl_Height / 2 + (int)(Math.Cos(time1 * 1.1f) * sf4) / sf4 * 333;
                    break;

                // Cool one -- ok
                case 49:
                    x1 = gl_x0 + (float)(Math.Sin(time1 * sf3)) * sf1;
                    y1 = gl_y0 + (float)(Math.Cos(time1 * sf3)) * sf1;

                    x2 = gl_x0 + (float)(Math.Sin(time1 * sf4)) * sf2;
                    y2 = gl_y0 + (float)(Math.Cos(time1 * sf4)) * sf2;
                    break;

                case 50:
                    x1 = gl_x0 + (float)(Math.Sin(time1 * sf3)) * sf1;
                    y1 = gl_y0 + (float)(Math.Cos(time1 * sf3)) * sf1;

                    x2 = gl_x0 + (float)(Math.Sin(time1 * sf4)) * sf2;
                    y2 = gl_y0 + (float)(Math.Cos(time1 * sf4)) * sf2;

                    sf1 += a;
                    sf2 += b;
                    break;

                case 51:
                    x1 = gl_x0 + (float)(Math.Sin(time1 + sf3)) * sf1;
                    y1 = gl_y0 + (float)(Math.Cos(time1 + sf3)) * sf1;

                    x2 = gl_x0 + (float)(Math.Sin(time1 + sf4)) * sf2;
                    y2 = gl_y0 + (float)(Math.Cos(time1 + sf4)) * sf2;

                    sf3 += a;
                    sf4 += b;
                    break;

                case 52:
                    x1 = gl_x0 + (float)(Math.Sin(time1 * sf3)) * sf1;
                    y1 = gl_y0 + (float)(Math.Cos(time1 * sf4)) * sf1;

                    x2 = gl_x0 + (float)(Math.Sin(time1 * sf3)) * sf2;
                    y2 = gl_y0 + (float)(Math.Cos(time1 * sf4)) * sf2;

                    sf3 += a;
                    sf4 += b;
                    break;

                case 53:
                    x1 = gl_x0 + (float)(Math.Sin(time1 * sf3)) * sf1 + (float)(Math.Cos(time1 + sf3)) * sf1;
                    y1 = gl_y0 + (float)(Math.Cos(time1 * sf3)) * sf1 + (float)(Math.Sin(time1 + sf3)) * sf1;

                    x2 = gl_x0 + (float)(Math.Sin(time1 * sf4)) * sf2;
                    y2 = gl_y0 + (float)(Math.Cos(time1 * sf4)) * sf2;
                    break;

                case 54:
                    x1 = gl_x0 + (float)(Math.Sin(time1 * sf3)) * sf1 + (float)(Math.Sin(time1 / si1)) * sf1 / si1;
                    y1 = gl_y0 + (float)(Math.Cos(time1 * sf3)) * sf1 + (float)(Math.Cos(time1 / si1)) * sf1 / si1;

                    x2 = gl_x0 + (float)(Math.Sin(time1 * sf4)) * sf2;
                    y2 = gl_y0 + (float)(Math.Cos(time1 * sf4)) * sf2;
                    break;

                case 55:
                    sf1 /= sf1 > 1000 ? 2 : 1;

                    x1 = gl_x0 + (float)(Math.Sin(time1 * sf3)) * sf1 + (float)(Math.Sin(time1 * si1)) * sf1;
                    y1 = gl_y0 + (float)(Math.Cos(time1 * sf3)) * sf1 + (float)(Math.Cos(time1 * si1)) * sf1;

                    x2 = gl_x0 + (float)(Math.Sin(time1 * sf4)) * sf2;
                    y2 = gl_y0 + (float)(Math.Cos(time1 * sf4)) * sf2;
                    break;

                case 56:
                    x1 = gl_x0 + (float)(Math.Sin(time1 * sf3)) * sf1 * (float)(Math.Sin(time1 * si1)) * sf1;
                    y1 = gl_y0 + (float)(Math.Cos(time1 * sf3)) * sf1 * (float)(Math.Cos(time1 * si1)) * sf1;

                    x2 = gl_x0 + (float)(Math.Sin(time1 * sf4)) * sf2;
                    y2 = gl_y0 + (float)(Math.Cos(time1 * sf4)) * sf2;
                    break;

                case 57:
                    x1 = (float)(Math.Sin(time1 * sf3)) * (float)(Math.Sin(time1 * si1)) * sf1 * 2;
                    y1 = (float)(Math.Cos(time1 * sf3)) * (float)(Math.Cos(time1 * si1)) * sf1 * 2;

                    x1 /= si1;
                    y1 /= si1;
                    x1 += gl_x0;
                    y1 += gl_y0;

                    x2 = gl_x0 + (float)(Math.Sin(time1 * sf4)) * sf2;
                    y2 = gl_y0 + (float)(Math.Cos(time1 * sf4)) * sf2;
                    break;

                case 58:
                    x1 = gl_x0 + (float)(Math.Sin(time1 * sf3)) * (float)(Math.Cos(time1 * si1)) * sf1 / 2;
                    y1 = gl_y0 + (float)(Math.Cos(time1 * sf3)) * (float)(Math.Sin(time1 * si1)) * sf1 / 2;

                    x2 = gl_x0 + (float)(Math.Sin(time1 * sf4)) * sf2;
                    y2 = gl_y0 + (float)(Math.Cos(time1 * sf4)) * sf2;
                    break;

                case 59:
                    x1 = gl_x0 + (float)(Math.Sin(time1 * sf3) * Math.Sin(1 + time1 / si1)) * sf1;
                    y1 = gl_y0 + (float)(Math.Cos(time1 * sf3) * Math.Sin(1 + time1 / si1)) * sf1;

                    x2 = gl_x0 + (float)(Math.Sin(time1 * sf4)) * sf2;
                    y2 = gl_y0 + (float)(Math.Cos(time1 * sf4)) * sf2;
                    break;

                case 60:
                    x1 = gl_x0 + (float)(Math.Sin(time1 * sf3) * Math.Sin(si1 + time1 / si1)) * sf1;
                    y1 = gl_y0 + (float)(Math.Cos(time1 * sf3) * Math.Sin(si1 + time1 / si1)) * sf1;

                    x2 = gl_x0 + (float)(Math.Sin(time1 * sf4)) * sf2;
                    y2 = gl_y0 + (float)(Math.Cos(time1 * sf4)) * sf2;
                    break;

                case 61:
                    x1 = gl_x0 + (float)(Math.Sin(time1 * sf3) + Math.Sin(time1 / si1 / si1)) * sf1;
                    y1 = gl_y0 + (float)(Math.Cos(time1 * sf3) + Math.Cos(time1 / si1 / si1)) * sf1;

                    x2 = gl_x0 + (float)(Math.Sin(time1 * sf4)) * sf2;
                    y2 = gl_y0 + (float)(Math.Cos(time1 * sf4)) * sf2;
                    break;

                case 62:
                    x1 = gl_x0 + (int)(Math.Sin(a * time1) * sf1);
                    y1 = gl_y0 + (int)(Math.Cos(b * time1) * sf1);

                    x2 = gl_x0 + (float)(Math.Sin(time1 * sf4)) * sf2;
                    y2 = gl_y0 + (float)(Math.Cos(time1 * sf4)) * sf2;
                    break;

                case 63:
                    sf1 /= sf1 > 1000 ? 2 : 1;
                    sf2 /= sf1 > 1000 ? 2 : 1;

                    x1 = gl_x0 + (int)(Math.Sin(a * time1) * sf1);
                    y1 = gl_y0 + (int)(Math.Cos(b * time1) * sf1);

                    x2 = gl_x0 + (float)(Math.Sin(time1 * sf4 * a)) * sf2;
                    y2 = gl_y0 + (float)(Math.Cos(time1 * sf4 * b)) * sf2;
                    break;

                case 64:
                    sf1 /= sf1 > 1000 ? 2 : 1;
                    sf2 /= sf1 > 1000 ? 2 : 1;

                    x1 = gl_x0 + (float)(Math.Sin(a * time1) * sf1);
                    y1 = gl_y0 + (float)(Math.Cos(b * time1) * sf1);

                    x2 = gl_x0 + (float)(Math.Sin(time1 * sf4 * a)) * sf2;
                    y2 = gl_y0 + (float)(Math.Cos(time1 * sf4 * b)) * sf2;
                    break;

                case 65:
                    x1 = gl_x0 + (float)(Math.Sin(a * time1) * Math.Sin(b * time1) * sf1);
                    y1 = gl_y0 + (float)(Math.Cos(a * time1) * Math.Cos(b * time1) * sf1);

                    x2 = gl_x0 + (float)(Math.Sin(time1 * sf4)) * sf2;
                    y2 = gl_y0 + (float)(Math.Cos(time1 * sf4)) * sf2;
                    break;

                case 66:
                    x1 = gl_x0 + (float)((Math.Sin(a * time2) + Math.Cos(b * time2)) * sf1);
                    y1 = gl_y0 + (float)((Math.Cos(a * time2) + Math.Sin(b * time2)) * sf1);

                    x2 = gl_x0 + (float)(Math.Sin(time1 * sf4)) * sf2;
                    y2 = gl_y0 + (float)(Math.Cos(time1 * sf4)) * sf2;
                    break;

                // probably is the same as 137
                case 67:
                    x1 = gl_x0 + (float)((Math.Sin(a * time1 * sf3) + Math.Cos(b * time1 * sf3)) * sf1);
                    y1 = gl_y0 + (float)((Math.Cos(a * time1 * sf3) + Math.Sin(b * time1 * sf3)) * sf1);

                    x2 = gl_x0 + (float)(Math.Sin(time1 * sf4)) * sf2;
                    y2 = gl_y0 + (float)(Math.Cos(time1 * sf4)) * sf2;
                    break;

                case 68:
                    x1 = gl_x0 + (int)((Math.Sin(a * time1 * sf3) + Math.Cos(b * time1 * sf3))) * sf1;
                    y1 = gl_y0 + (int)((Math.Cos(a * time1 * sf3) + Math.Sin(b * time1 * sf3))) * sf1;

                    x2 = gl_x0 + (float)(Math.Sin(time1 * sf4)) * sf2;
                    y2 = gl_y0 + (float)(Math.Cos(time1 * sf4)) * sf2;
                    break;

                case 69:
                    x1 = gl_x0 + (int)((Math.Sin(a * time1 * sf3) + Math.Cos(b * time1 * sf3)) * a) * sf1;
                    y1 = gl_y0 + (int)((Math.Cos(a * time1 * sf3) + Math.Sin(b * time1 * sf3)) * a) * sf1;

                    x2 = gl_x0 + (float)(Math.Sin(time1 * sf4)) * sf2;
                    y2 = gl_y0 + (float)(Math.Cos(time1 * sf4)) * sf2;
                    break;

                case 70:
                    x1 = gl_x0 + (int)((Math.Sin(a * time1 * sf3) + Math.Cos(b * time1 * sf3)) * c) * sf1;
                    y1 = gl_y0 + (int)((Math.Cos(a * time1 * sf3) + Math.Sin(b * time1 * sf3)) * c) * sf1;

                    x2 = gl_x0 + (float)(Math.Sin(time1 * sf4)) * sf2;
                    y2 = gl_y0 + (float)(Math.Cos(time1 * sf4)) * sf2;
                    break;

                case 71:
                    x1 = gl_x0 + (int)((Math.Sin(a * time1 * sf3) * Math.Cos(b * time1 * sf3)) * c) * sf1;
                    y1 = gl_y0 + (int)((Math.Cos(a * time1 * sf3) * Math.Sin(b * time1 * sf3)) * c) * sf1;

                    x2 = gl_x0 + (float)(Math.Sin(time1 * sf4)) * sf2;
                    y2 = gl_y0 + (float)(Math.Cos(time1 * sf4)) * sf2;
                    break;

                case 72:
                    x1 = gl_x0 + (int)(Math.Sin(time1 * sf3) * c) * sf1;
                    y1 = gl_y0 + (int)(Math.Cos(time1 * sf3) * c) * sf1;

                    x2 = gl_x0 + (float)(Math.Sin(time1 * sf4)) * sf2;
                    y2 = gl_y0 + (float)(Math.Cos(time1 * sf4)) * sf2;
                    break;

                case 73:
                    x1 = gl_x0 + (int)(Math.Sin(time1 * sf3 / 10) * c) * sf1;
                    y1 = gl_y0 + (int)(Math.Cos(time1 * sf3 / 10) * c) * sf1;

                    x2 = gl_x0 + (float)(Math.Sin(time1 * sf4)) * sf2;
                    y2 = gl_y0 + (float)(Math.Cos(time1 * sf4)) * sf2;
                    break;

                case 74:
                    x1 = gl_x0 + (float)(Math.Sin(time1 * sf3) * c) * sf1;
                    y1 = gl_y0 + (float)(Math.Cos(time1 * sf3) * c) * sf1;

                    x2 = gl_x0 + (float)(Math.Sin(time1 * sf4)) * sf2;
                    y2 = gl_y0 + (float)(Math.Cos(time1 * sf4)) * sf2;
                    break;

                case 75:
                    x1 = gl_x0 + (int)(Math.Sin(time1 * sf3) * 10) / 10 * sf1;
                    y1 = gl_y0 + (int)(Math.Cos(time1 * sf3) * 10) / 10 * sf1;

                    x2 = gl_x0 + (float)(Math.Sin(time1 * sf4)) * sf2;
                    y2 = gl_y0 + (float)(Math.Cos(time1 * sf4)) * sf2;
                    break;

                case 76:
                    x1 = gl_x0 + (int)(Math.Sin(time1 * sf3) * si1) * sf1 / (1.0f * si1);
                    y1 = gl_y0 + (int)(Math.Cos(time1 * sf3) * si1) * sf1 / (1.0f * si1);

                    x2 = gl_x0 + (float)(Math.Sin(time1 * sf4)) * sf2;
                    y2 = gl_y0 + (float)(Math.Cos(time1 * sf4)) * sf2;
                    break;

                // Various shapes vs a circle
                // The shapes are of a single family, so we'll have a separate setup func for these
                case 77:
                    x1 = gl_x0 + ((float)(Math.Sin(time1 * sf3 + c) * sf1) + (float)(Math.Sin(time1 + sf3) * (sf1 / 2)));
                    y1 = gl_y0 + ((float)(Math.Cos(time1 * sf3 - c) * sf1) + (float)(Math.Cos(time1 + sf3) * (sf1 / 2)));

                    x2 = gl_x0 + (float)(Math.Sin(time1 * sf4)) * sf2;
                    y2 = gl_y0 + (float)(Math.Cos(time1 * sf4)) * sf2;
                    break;

                // The same as the one before, but with more params at play
                case 78:
                    x1 = gl_x0 + ((float)(Math.Sin(time1 * sf3 + c) * sf1) + (float)(Math.Sin(time1 + sf3) * (sf1 / a)));
                    y1 = gl_y0 + ((float)(Math.Cos(time1 * sf3 - c) * sf1) + (float)(Math.Cos(time1 + sf3) * (sf1 / a)));

                    x2 = gl_x0 + (float)(Math.Sin(time1 * sf4)) * sf2;
                    y2 = gl_y0 + (float)(Math.Cos(time1 * sf4)) * sf2;
                    break;

                // Water Drop
                case 79:
                    sf1 = 150;

                    x1 = gl_x0 + ((float)(Math.Sin(sf5 * time1 * sf3) * sf1) + (float)(Math.Sin(sf6 * time1 * sf3) * sf1 * a));
                    y1 = gl_y0 + ((float)(Math.Cos(sf7 * time1 * sf3) * sf1) + (float)(Math.Cos(sf8 * time1 * sf3) * sf1 * b));

                    x2 = gl_x0 + (float)(Math.Sin(time1 * sf4)) * sf2;
                    y2 = gl_y0 + (float)(Math.Cos(time1 * sf4)) * sf2;
                    break;

                case 80:
                    x1 = gl_x0 + (float)(Math.Sin(time1 * sf3)) * sf1 + (float)(Math.Cos(time1 * sf4)) * sf1;
                    y1 = gl_y0 + (float)(Math.Cos(time1 * sf3)) * sf1;

                    x2 = gl_x0 + (float)(Math.Sin(time1 * sf4)) * sf2;
                    y2 = gl_y0 + (float)(Math.Cos(time1 * sf4)) * sf2;
                    break;

                case 81:
                    y1 = 1 * gl_Height / 2 + (int)(dy2 * 33 / float_B) * sf1;

                    x2 = gl_x0 + dx2 * 150 / float_B;
                    y2 = gl_y0;
                    break;

                case 82:
                    x1 = gl_x0 + (int)(dy2 * si2 / float_B) * sf1;
                    y1 = gl_Height;

                    x2 = gl_x0 + dx2 * si1 / float_B;
                    y2 = sf2;
                    break;

                case 83:
                    y1 = si1 + (float)(Math.Sin(time1 * sf1)) * sf3;
                    y2 = si2 + (float)(Math.Sin(time1 * sf2)) * sf4;
                    break;

                case 84:
                    x3 = gl_x0;
                    y3 = gl_Height / 2 + (float)(Math.Sin(time1 * sf2)) * sf4;
                    y2 = y1 = gl_y0 + (float)(Math.Sin(time1 * sf1)) * sf3;
                    break;

                case 85:
                    x1 = rand.Next(gl_Width);
                    x2 = rand.Next(gl_Width);

                    y1 = rand.Next(gl_Height);
                    y2 = rand.Next(gl_Height);
                    break;

                case 86:
                    x1 += (float)(Math.Sin(time1 * sf1 * sf2) * sf1);
                    y1 += (float)(Math.Sin(time1 * sf1 * sf2) * sf2);

                    x2 += (float)(Math.Sin(time1 * sf3 * sf4) * sf3);
                    y2 += (float)(Math.Sin(time1 * sf3 * sf4) * sf4);
                    break;

                // Spiral
                case 87:
                    x1 = (float)(Math.Sin(time1));
                    y1 = (float)(Math.Cos(time1));

                    x1 += x1 * time1 / si1;
                    y1 += y1 * time1 / si1;

                    if (si2 == 0)
                    {
                        x1 = (int)(x1 * time1) * sf1;
                        y1 = (int)(y1 * time1) * sf1;
                    }
                    else
                    {
                        x1 = x1 * sf1 * time1;
                        y1 = y1 * sf1 * time1;
                    }

                    x1 = gl_x0 + x1;
                    y1 = gl_y0 + y1;

                    x2 = gl_x0 + (float)(Math.Sin(time1 * sf4)) * sf2;
                    y2 = gl_y0 + (float)(Math.Cos(time1 * sf4)) * sf2;
                    break;

                // Irregularly shaped circles
                case 88:
                    x1 = gl_x0 + (float)(Math.Sin(time1)) * 500 + (float)(Math.Sin(time1 * sf5)) * si1;
                    y1 = gl_y0 + (float)(Math.Cos(time1)) * 500 + (float)(Math.Sin(time1 * sf6)) * si1;

                    if (a == 0.0f)
                    {
                        x2 = gl_x0 + (float)(Math.Sin(time1 * sf4)) * sf2 + (float)(Math.Cos(time1)) * si2;
                        y2 = gl_y0 + (float)(Math.Cos(time1 * sf4)) * sf2 + (float)(Math.Sin(time1)) * si2;
                    }
                    else
                    {
                        x2 = gl_x0 + (float)(Math.Sin(time1 * sf4)) * sf2 + (float)(Math.Sin(time1)) * si2;
                        y2 = gl_y0 + (float)(Math.Cos(time1 * sf4)) * sf2 + (float)(Math.Sin(time1)) * si2;
                    }
                    break;

                // Sine/Cosine functions
                case 89:
                    x1 = x2 = x3 = x4 = time1 * 30;

                    switch (si1)
                    {
                        case 0:
                            y1 = (float)(Math.Sin(time1 * sf1)) * 100;
                            y2 = (float)(Math.Cos(time1 * sf2)) * 50;
                            break;

                        case 1:
                            y1 = (int)(Math.Sin(time1 * sf1) * a) * 100 / a;
                            y2 = (int)(Math.Cos(time1 * sf2) * a) * 050 / a;
                            break;

                        case 2:
                            y1 = (float)(Math.Sin(time1 * sf1)) * 100;
                            y2 = (  int)(Math.Cos(time1 * sf2) * a) * 50 / a;
                            break;

                        case 3:
                            y1 = (int  )(Math.Sin(time1 * sf1) * a) * 100 / a;
                            y2 = (float)(Math.Cos(time1 * sf2)) * 50;
                            break;
                    }

                    y3 = y1 + y2;
                    y4 = y1 * y2 / 100;

                    y1 += 333;
                    y2 += gl_Height - 333;

                    y3 += gl_y0 - 300;
                    y4 += gl_y0 + 300;
                    break;

                // Locator
                case 90:
                    x2 = gl_x0 + (float)(Math.Sin(time1 * sf4 + a)) * gl_Height / 3;
                    y2 = gl_y0 + (float)(Math.Cos(time1 * sf4 + a)) * gl_Height / 3;

                    {
                        x3 += rand.Next(11) * 0.1f - 0.5f;
                        y3 += rand.Next(11) * 0.1f - 0.5f;
                    }

                    if ((x2 > gl_x0) == (x3 > gl_x0) && (y2 > gl_y0) == (y3 > gl_y0) && Math.Abs((x2 - gl_x0) / (y2 - gl_y0) - (x3 - gl_x0) / (y3 - gl_y0)) < 0.05f)
                    {
                        x4 = x3;
                        y4 = y3;
                    }
                    else
                    {
                        x4 = -33;
                        y4 = -33;
                    }
                    break;

                case 91:

                    x1 = (float)(Math.Sin(time1 * (prm_i[1] / 100))) * sf3;
                    y1 = (float)(Math.Cos(time1 * (prm_i[1] / 100))) * sf3;

                    x2 = (float)(Math.Sin(time1 * (prm_i[1] % 100))) * sf4;
                    y2 = (float)(Math.Cos(time1 * (prm_i[1] % 100))) * sf4;

                    // Circle vs ellipse mode
                    switch (prm_i[0])
                    {
                        case 0: break;
                        case 1: x1 *= sf1; break;
                        case 2: x2 *= sf2; break;
                        case 3: x1 *= sf1; x2 *= sf2; break;
                    }

                    // Move to the center
                    x1 += gl_x0; y1 += gl_y0; x2 += gl_x0; y2 += gl_y0;

                    // Vary the raduis
                    switch (prm_i[2])
                    {
                        case 0:
                            break;

                        case 1:
                            sf3 += 0.1f * (rand.Next(3) - 1);
                            sf4 += 0.1f * (rand.Next(3) - 1);
                            break;

                        case 2:
                            sf3 += 0.1f * (rand.Next(7) - 3);
                            sf4 += 0.1f * (rand.Next(7) - 3);
                            break;

                        case 3:
                            sf3 += 0.1f * (rand.Next(5));
                            sf4 -= 0.1f * (rand.Next(5));
                            break;

                        case 4:
                            if (si1 == 0)
                            {
                                if (myUtils.randomChance(rand, 1, 13))
                                    si1 = (rand.Next(123) + 1) * myUtils.randomSign(rand);
                            }
                            else
                            {
                                if (si1 > 0)
                                {
                                    sf3 += 0.1f;
                                    sf3 += myUtils.randFloat(rand);
                                    si1--;
                                }
                                else
                                {
                                    sf3 -= 0.1f;
                                    sf3 -= myUtils.randFloat(rand);
                                    si1++;
                                }
                            }

                            if (si2 == 0)
                            {
                                if (myUtils.randomChance(rand, 1, 13))
                                    si2 = (rand.Next(123) + 1) * myUtils.randomSign(rand);
                            }
                            else
                            {
                                if (si2 > 0)
                                {
                                    sf4 += myUtils.randFloat(rand, 0.1f);
                                    si2--;
                                }
                                else
                                {
                                    sf4 -= myUtils.randFloat(rand, 0.1f);
                                    si2++;
                                }
                            }
                            break;

                        case 5:
                            sf3 += 7 * (float)Math.Sin(time1 * 10);
                            sf4 += 7 * (float)Math.Cos(time1 * 10);
                            break;
                    }
                    break;

                default:
                    break;
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Show()
        {
            if (scrMode != ScreenMode.Active)
            {
                // Decrease scrMode, until it becomes Active
                scrMode--;

                // Clear the screen between modes
                dimScreen(0.075f, doShiftColor: true);
            }
            else
            {
                switch (mode)
                {
                    case 00:
                        myPrimitive._Line.SetColor(r1, g1, b1, 0.5f);
                        myPrimitive._Line.Draw(x, y, x1, y1);
                        myPrimitive._Line.SetColor(r2, g2, b2, 0.5f);
                        myPrimitive._Line.Draw(x, y, x2, y2);
                        break;

                    case 01:case 02:
                        myPrimitive._Line.SetColor(r2, g2, b2, 0.5f);
                        myPrimitive._Line.Draw(x2, y2, x1, y1);
                        myPrimitive._Rectangle.SetColor(1.0f, 0.55f, 0.0f, 1.0f);
                        myPrimitive._Rectangle.Draw(x1, y1, 3, 3, false);
                        break;

                    case 03:case 04:
                        myPrimitive._Line.SetColor(r2, g2, b2, 0.5f);
                        myPrimitive._Line.Draw(x1, y1, x2, y2);
                        myPrimitive._Line.Draw(x3, y3, x4, y4);

                        myPrimitive._Rectangle.SetColor(1.0f, 0.55f, 0.0f, 1.0f);
                        myPrimitive._Rectangle.Draw(x1, y1, 3, 3, false);
                        myPrimitive._Rectangle.Draw(x2, y2, 3, 3, false);
                        myPrimitive._Rectangle.Draw(x3, y3, 3, 3, false);
                        myPrimitive._Rectangle.Draw(x4, y4, 3, 3, false);
                        break;

                    case 05:case 06:case 07:case 08:case 09:case 10:case 11:case 12:case 13:case 14:case 15:case 16:
                    case 17:case 18:case 19:case 20:case 21:case 22:case 23:case 24:case 25:case 26:case 27:case 28:
                        myPrimitive._Line.SetColor(r2, g2, b2, 0.5f);
                        myPrimitive._Line.Draw(x1, y1, x2, y2);

                        myPrimitive._Rectangle.SetColor(1.0f, 0.55f, 0.0f, 1.0f);
                        myPrimitive._Rectangle.Draw(x1, y1, 3, 3, false);
                        myPrimitive._Rectangle.Draw(x2, y2, 3, 3, false);
                        break;

                    case 29:case 30:case 31:case 32:case 33:case 34:case 35:case 36:case 37:
                    case 38:case 39:
                        myPrimitive._Line.SetColor(r2, g2, b2, 0.5f);
                        myPrimitive._Line.Draw(x1, y1, x2, y2);

                        myPrimitive._Rectangle.SetColor(1.0f, 0.55f, 0.0f, 1.0f);
                        myPrimitive._Rectangle.Draw(x2, y2, 3, 3, false);
                        break;

                    case 40:case 41:case 42:case 43:case 44:case 45:case 46:case 47:case 48:case 49:case 50:case 51:
                    case 52:case 53:case 54:case 55:case 56:case 57:case 58:case 59:case 60:case 61:case 62:case 63:
                    case 64:case 65:case 66:case 67:case 68:case 69:case 70:case 71:case 72:case 73:case 74:case 75:
                    case 76:case 77:case 78:case 79:case 80:case 81:case 82:case 83:case 85:case 86:case 87:case 88:
                        myPrimitive._Line.SetColor(r2, g2, b2, 0.5f);
                        myPrimitive._Line.Draw(x1, y1, x2, y2);

                        myPrimitive._Rectangle.SetColor(0.5451f, 0.0f, 0.0f, 1.0f);
                        myPrimitive._Rectangle.Draw(x1, y1, 3, 3, false);

                        myPrimitive._Rectangle.SetColor(1.0f, 0.55f, 0.0f, 1.0f);
                        myPrimitive._Rectangle.Draw(x2, y2, 3, 3, false);
                        break;

                    case 84:
                        myPrimitive._Line.SetColor(r2, g2, b2, 0.5f);
                        myPrimitive._Line.Draw(x1, y1, x3, y3);
                        myPrimitive._Line.Draw(x2, y2, x3, y3);

                        myPrimitive._Rectangle.SetColor(0.5451f, 0.0f, 0.0f, 1.0f);
                        myPrimitive._Rectangle.Draw(x1, y1, 3, 3, false);

                        myPrimitive._Rectangle.SetColor(1.0f, 0.55f, 0.0f, 1.0f);
                        myPrimitive._Rectangle.Draw(x2, y2, 3, 3, false);
                        break;

                    case 89:
                        myPrimitive._Line.SetColor(r2, g2, b2, 0.5f);
                        myPrimitive._Line.Draw(x1, y1, x2, y2 + 500);

                        myPrimitive._Rectangle.SetColor(0.5451f, 0.0f, 0.0f, 1.0f);
                        myPrimitive._Rectangle.Draw(x1, y1, 3, 3, false);

                        myPrimitive._Rectangle.SetColor(1.0f, 0.55f, 0.0f, 1.0f);
                        myPrimitive._Rectangle.Draw(x2, y2, 3, 3, false);

                        myPrimitive._Rectangle.SetColor(0.5451f, 0.0f, 0.5451f, 1.0f);
                        myPrimitive._Rectangle.Draw(x3, y3, 3, 3, false);

                        myPrimitive._Rectangle.SetColor(0.58f, 0.0f, 0.827f, 1.0f);
                        myPrimitive._Rectangle.Draw(x4, y4, 3, 3, false);
                        break;

                    case 90:
                        myPrimitive._Ellipse.SetColor(1.0f, 0.55f, 0.0f, 0.01f);
                        myPrimitive._Ellipse.Draw(gl_x0 - 100, gl_y0 - 100, 0200, 0200);
                        myPrimitive._Ellipse.Draw(gl_x0 - 300, gl_y0 - 300, 0600, 0600);
                        myPrimitive._Ellipse.Draw(gl_x0 - 500, gl_y0 - 500, 1000, 1000);

                        myPrimitive._Line.SetColor(r2, g2, b2, 0.5f);
                        myPrimitive._Line.Draw(x1, y1, x2, y2);

                        myPrimitive._Rectangle.SetColor(0.5451f, 0.0f, 0.0f, 1.0f);
                        myPrimitive._Rectangle.Draw(x1-1, y1-1, 3, 3, false);

                        myPrimitive._Rectangle.SetColor(1.0f, 0.55f, 0.0f, 1.0f);
                        myPrimitive._Rectangle.Draw(x2, y2, 3, 3, false);

                        if (x4 > 0)
                        {
                            x4 = (int)(x4);
                            y4 = (int)(y4);

                            myPrimitive._Rectangle.SetColor(1.0f, 0.65f, 0.0f, 1.0f);
                            myPrimitive._Rectangle.Draw(x4 + 2, y4 + 2, 7, 7, true);

                            myPrimitive._Rectangle.SetColor(1.0f, 0.55f, 0.0f, 1.0f);
                            myPrimitive._Rectangle.Draw(x4, y4, 10, 10, false);
                        }
                        break;

                    case 91:
                        myPrimitive._Line.SetColor(r2, g2, b2, 0.5f);
                        myPrimitive._Line.Draw(x1, y1, x2, y2);

                        drawRect(0.5451f, 0.00f, 0.0f, 1.0f, x1, y1, 3);
                        drawRect(1.0f,    0.55f, 0.0f, 1.0f, x2, y2, 3);
                        break;

                    case 1300:
                        myPrimitive._Line.SetColor(r2, g2, b2, 0.5f);
                        myPrimitive._Line.Draw(x1, y1, x2, y2);

                        myPrimitive._Rectangle.SetColor(0.5451f, 0.0f, 0.0f, 1.0f);
                        myPrimitive._Rectangle.Draw(x1, y1, 3, 3, false);

                        myPrimitive._Rectangle.SetColor(1.0f, 0.55f, 0.0f, 1.0f);
                        myPrimitive._Rectangle.Draw(x2, y2, 3, 3, false);
                        break;

                    default:
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

            list.Add(new myObj_132());

            // Disable VSYNC if needed
            // Glfw.SwapInterval(0);

            if (doClearBuffer)
            {
                glDrawBuffer(GL_FRONT_AND_BACK | GL_DEPTH_BUFFER_BIT);
                glClearColor(0, 0, 0, 1);
            }
            else
            {
                glDrawBuffer(GL_FRONT_AND_BACK);
            }

            float ddalpha = 0.0001f;

            while (!Glfw.WindowShouldClose(window))
            {
                cnt++;

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
                    dimScreen(dimAlpha, doShiftColor: true);
/*
                    dimAlpha -= ddalpha;

                    if (dimAlpha < 0 || dimAlpha > 0.01f )
                        ddalpha *= -1;
*/
                }

                // Render Frame
                for (int repeat = 0; repeat < repeatMode; repeat++)
                {
                    for (int i = 0; i < list.Count; i++)
                    {
                        var obj = list[i] as myObj_132;

                        obj.Show();
                        obj.Move();
                    }
                }

                System.Threading.Thread.Sleep(renderDelay);
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void initShapes()
        {
            myPrimitive.init_Line();
            myPrimitive.init_Rectangle();
            myPrimitive.init_Ellipse();

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void drawRect(float r, float g, float b, float a, float x, float y, int size)
        {
            myPrimitive._Rectangle.SetColor(r, g, b, a);
            myPrimitive._Rectangle.Draw(x, y, size, size, false);
            size += 4;
            myPrimitive._Rectangle.SetColor(r, g, b, a * 0.33f);
            myPrimitive._Rectangle.Draw(x - 2, y - 2, size, size, false);
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void constSetup1()
        {
            switch (rand.Next(2))
            {
                case 0:
                    sf1 = rand.Next(2000) + 100;
                    break;

                case 1:
                    sf1 = rand.Next(1000) + 100;
                    break;
            }

            switch (rand.Next(2))
            {
                case 0:
                    sf2 = rand.Next(2000) + 100;
                    break;

                case 1:
                    sf2 = rand.Next(1000) + 100;
                    break;
            }

            switch (rand.Next(3))
            {
                case 0:
                    sf3 = rand.Next(5000);
                    break;

                case 1:
                    sf3 = rand.Next(500);
                    break;

                case 2:
                    sf3 = rand.Next(50);
                    break;
            }

            switch (rand.Next(3))
            {
                case 0:
                    sf4 = rand.Next(5000);
                    break;

                case 1:
                    sf4 = rand.Next(500);
                    break;

                case 2:
                    sf4 = rand.Next(50);
                    break;
            }

            sf3 = (sf3 + 1) * 0.01f;
            sf4 = (sf4 + 1) * 0.01f;

#if false
            // Old
            sf1 = rand.Next(2000) + 100;
            sf2 = rand.Next(2000) + 100;

            sf3 = rand.Next(1000) * 0.05f;
            sf4 = rand.Next(1000) * 0.05f;
#endif

            si1 = rand.Next(100) + 1;

            a = (rand.Next(201) * 0.01f) - 1.0f;
            b = (rand.Next(201) * 0.01f) - 1.0f;
            c = 1.0f + rand.Next(300) * 0.01f;

            if (rand.Next(3) == 0)
                a *= (rand.Next(11) + 1);

            if (rand.Next(3) == 0)
                b *= (rand.Next(11) + 1);

            fdLifeCnt = 0.25f;

            // Affects moving speed of the red dot along its path
            dt2 = 0.01f + rand.Next(20) * 0.005f;

            // Affects life span of the shape
            fdLifeCnt = 0.20f;

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void constSetup2()
        {
            // Set up all the constants
            constSetup1();

            // Adjust both radiuses a bit
            sf2 *= (sf2 > 333) ? 1 : 2;
            sf1 = sf2 / 2;

            // sf3 here will affect the form of the shape.
            // These values were found manually and result in closed-loop shapes:

            switch (rand.Next(14))
            {
                case  0: sf3 = 2.000f; break;       //
                case  1: sf3 = 1.500f; break;       //
                case  2: sf3 = 1.000f; break;       // Star
                case  3: sf3 = 0.750f; break;       // Star
                case  4: sf3 = 0.500f; break;       // Triangle-Apple-Fish
                case  5: sf3 = 0.300f; break;       // Many-leaves spiral
                case  6: sf3 = 0.250f; break;       // 3-leaf spiral
                case  7: sf3 = 0.200f; break;       // 4-leaf spiral
                case  8: sf3 = 0.075f; break;       // elliptic spiral
                case  9: sf3 = 0.050f; break;       // elliptic spiral
                case 10: sf3 = 0.025f; break;       // spiraling circle
                case 11: sf3 = 0.010f; break;       // spiraling circle
                case 12: sf3 = 0.000f; break;       // circle

                default:
                    sf3 = 0.0001f * rand.Next(10000);
                    break;
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void constSetup3()
        {
            // Set up all the constants
            constSetup1();

            // Adjust both radiuses a bit
            sf2 *= (sf2 > 333) ? 1 : 2;
            sf1 = sf2 / 2;

            // Let's change [a] and [sf3]
            // sf3 here will affect the form of the shape.
            // These values were found manually and result in closed-loop shapes:

            int aRnd = rand.Next(5);

            switch (rand.Next(13))
            {
                // circle
                // no real impact from a. a < 1 makes the circle larger than the other circle
                case 0:
                    sf3 = 0.0f;
                    break;

                // spiraling circle
                case 1:
                    sf3 = 0.01f;
                    a = (aRnd < 3)
                        ? 0.01f * (rand.Next(100) + 1)      // [0.01 .. 1]
                        : 0.10f * (rand.Next(3000) + 10);   // [1 .. 300]
                    break;

                // spiraling circle
                case 2:
                    sf3 = 0.025f;
                    a = (aRnd < 3)
                        ? 0.001f * (rand.Next(1000) + 1)        // [0.001 .. 1]
                        : 0.100f * (rand.Next(2000) + 10);      // [1 .. 200]
                    break;

                // elliptic spiral -- need more life span
                case 3:
                    sf3 = 0.05f;
                    a = 0.100f * (rand.Next(3000) + 10);      // [1 .. 300] -- no need to go less than 1.0
                    break;

                // elliptic spiral -- need more life span
                case 4:
                    sf3 = 0.075f;
                    a = 0.100f * (rand.Next(1000) + 10);      // [1 .. 100] -- no need to go less than 1.0
                    break;

                // 4-leaf spiral
                case 5:
                    sf3 = 0.2f;
                    a = (aRnd < 4)
                        ? 0.1f * (rand.Next(991) + 10)          // [1 .. 100]       -- var shapes
                        : 0.1f * (rand.Next(50000) + 1000);     // [100 .. 5000]    -- elliptical shape
                    break;

                // 3-leaf spiral
                case 6:
                    sf3 = 0.25f;

                    // [0.01 .. 1], the second circle must not be larger than the screen
                    if (aRnd == 0)
                    {
                        a = 0.01f * (rand.Next(99) + 1);

                        sf2 = rand.Next(gl_y0) + gl_y0;
                        sf1 = sf2 / 2;
                        break;
                    }

                    // [30 .. 1000]
                    if (aRnd == 1)
                    {
                        a = 0.1f * (rand.Next(10000) + 300);
                        break;
                    }

                    // [1..30] is the most interesting part
                    {
                        a = 0.01f * (rand.Next(2901) + 100);
                    }
                    break;

                // Many-leaves spiral
                case 7:
                    sf3 = 0.3f;
                    a = (aRnd < 3)
                        ? 0.10f * (rand.Next(5000) + 200)       // [20 .. 500]
                        : 0.01f * (rand.Next(1900) + 100);      // [1 .. 20]
                    break;

                // Triangle-Apple-Fish
                case 8:
                    sf3 = 0.5f;

                    switch (aRnd)
                    {
                        // [20 .. 100]
                        case 0:
                            a = 0.1f * (rand.Next(800) + 200);
                            break;

                        // [5 .. 20]
                        case 1:
                            a = 0.01f * (rand.Next(1500) + 500);
                            break;

                        // [1 .. 5]
                        case 2:
                            a = 0.01f * (rand.Next(401) + 100);
                            break;

                        // [0.5 .. 1]
                        case 3:
                            a = 0.5f + 0.01f * (rand.Next(51));
                            sf1 = 100;
                            break;

                        // [0.01 .. 0.5]
                        case 4:
                            a = 0.01f * (rand.Next(50) + 1);
                            sf1 = 10;
                            break;
                    }
                    break;

                // todo: finish [a]'s setup:

                // Star
                case 9:
                    sf3 = 0.75f;
                    a = 2.0f;
                    break;

                // Star
                case 10:
                    sf3 = 1.0f;
                    a = 2.0f;
                    break;

                // 
                case 11:
                    sf3 = 1.5f;
                    a = 2.0f;
                    break;

                // 
                case 12:
                    sf3 = 2.0f;
                    a = 2.0f;
                    break;
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

private void constSetup4()
        {
            void randInts(ref float val)
            {
                switch (rand.Next(9))
                {
                    case 0:
                        val = rand.Next(3001) + 1;
                        break;

                    case 1: case 2:
                        val = rand.Next(301) + 1;
                        break;

                    case 3: case 4: case 5:
                        val = rand.Next(31) + 1;
                        break;

                    case 6: case 7: case 8:
                        val = rand.Next(11) + 1;
                        break;
                }
            }

            void randFloats(ref float val)
            {
                switch (rand.Next(3))
                {
                    case 0:
                        val = 0.001f * rand.Next(3001);
                        break;

                    case 1: case 2:
                        val = 0.25f * rand.Next(41);
                        break;
                }
            }

            void set2params(ref float a, ref float b, float val)
            {
                a = b = val;
            }

            // Get value from [0.025, 0.05, 0.1, 0.15, 0.20 ... 1.0]
            void getParam1(ref float val)
            {
                switch (rand.Next(21))
                {
                    case 0: val = 0.025f; break;
                    case 1: val = 0.050f; break;

                    case 2: case 3: case 4: case 5: case 6: case 7: case 8: case 9:
                    case 10: case 11: case 12: case 13: case 14: case 15: case 16:
                    case 17: case 18: case 19: case 20:
                        val = 0.10f + 0.05f * rand.Next(19);
                        break;
                }
            }

            // ---------------------------

            constSetup1();

            a = 1;
            b = 1;

            switch (rand.Next(46))
            {
                // Circle / Ellipse
                case 0:
                    sf5 = sf6 = sf7 = sf8 = 1.0f;

                    a = 0.01f * rand.Next(300);
                    b = 0.01f * rand.Next(300);
                    break;

                // Beaker - Pure Form
                case 1:
                    sf5 = sf7 = sf8 = 1.0f;
                    sf6 = 2.0f;

                    a = 0.9f + 0.01f * rand.Next(21);
                    b = 0.9f + 0.01f * rand.Next(21);
                    break;

                // Beaker - Pure Form + Randomized a, b
                case 2:
                    sf5 = sf7 = sf8 = 1.0f;
                    sf6 = 2;

                    a = 0.01f * rand.Next(300);
                    b = 0.01f * rand.Next(300);
                    break;

                // Beaker - Randomized Ints
                case 3:
                    sf5 = sf7 = sf8 = 1.0f;
                    randInts(ref sf6);
                    break;

                // Beaker - Randomized Ints + Randomized a, b
                case 4:
                    sf5 = sf7 = sf8 = 1.0f;
                    randInts(ref sf6);

                    a = 0.01f * rand.Next(300);
                    b = 0.01f * rand.Next(300);
                    break;

                // Beaker - Randomized Floats
                case 5:
                    sf5 = sf7 = sf8 = 1.0f;
                    randFloats(ref sf6);
                    break;

                // Beaker - Randomized Floats + Randomized a, b
                case 6:
                    sf5 = sf7 = sf8 = 1.0f;
                    randFloats(ref sf6);

                    a = 0.01f * rand.Next(300);
                    b = 0.01f * rand.Next(300);
                    break;

                // Rain Drop - Pure Form
                case 7:
                    sf5 = sf7 = sf8 = 1.0f;
                    sf6 = 2.0f;

                    a = 0.45f + 0.01f * rand.Next(11);
                    b = 1.9f + 0.01f * rand.Next(21);
                    break;

                // Rain Drop - Pure Form - Width randomized
                case 8:
                    sf5 = sf7 = sf8 = 1.0f;
                    sf6 = 2.0f;

                    a = 0.1f + 0.01f * rand.Next(41);
                    b = 1.0f + 0.01f * rand.Next(101);
                    break;

                // Rain Drop - Eggish
                case 9:
                    sf5 = sf7 = sf8 = 1.0f;
                    sf6 = 2.0f;

                    a = 0.10f + 0.01f * rand.Next(25);
                    b = 0.25f + 0.01f * rand.Next(75);
                    break;

                // Rain Drop - Truffle
                case 10:
                    sf5 = sf7 = sf8 = 1.0f;
                    sf6 = 2.0f;

                    a = 0.400f + 0.001f * rand.Next(101);
                    b = 0.005f * rand.Next(151);
                    break;

                // Rain Drop - Trianglic
                case 11:
                    sf5 = sf7 = sf8 = 1.0f;
                    sf6 = 2.0f;

                    a = 0.250f + 0.001f * rand.Next(101);
                    b = 0.005f * rand.Next(51);
                    break;

                // Smile - Pure Form
                case 12:
                    sf5 = sf6 = sf8 = 1.0f;
                    sf7 = 2.0f;

                    a = 0.9f + 0.01f * rand.Next(21);
                    b = 0.9f + 0.01f * rand.Next(21);
                    return;

                // Smile -- Stable Constants
                case 13:
                    sf5 = sf6 = sf8 = 1.0f;

                    switch (rand.Next(20))
                    {
                        case 00: sf7 = 0.01f; break;
                        case 01: sf7 = 0.20f; break;
                        case 02: sf7 = 0.25f; break;
                        case 03: sf7 = 0.50f; break;
                        case 04: sf7 = 0.75f; break;
                        case 05: sf7 = 1.10f; break;
                        case 06: sf7 = 1.15f; break;
                        case 07: sf7 = 1.20f; break;
                        case 08: sf7 = 1.25f; break;
                        case 09: sf7 = 1.50f; break;
                        case 10: sf7 = 1.75f; break;
                        case 11: sf7 = 2.00f; break;
                        case 12: sf7 = 2.25f; break;
                        case 13: sf7 = 2.50f; break;
                        case 14: sf7 = 2.75f; break;
                        case 15: sf7 = 3.00f; break;
                        case 16: sf7 = 3.25f; break;
                        case 17: sf7 = 3.50f; break;
                        case 18: sf7 = 4.00f; break;
                        case 19: sf7 = 5.00f; break;
                    }

                    a = 0.9f + 0.01f * rand.Next(21);
                    b = 0.9f + 0.01f * rand.Next(21);
                    return;

                // Smile - Pure Form + Size Randomized
                case 14:
                    sf5 = sf6 = sf8 = 1.0f;
                    sf7 = 2.0f;

                    a = 0.01f * rand.Next(1001);
                    b = 0.01f * rand.Next(501);
                    break;

                // Smile -- Stable Constants + Size Randomized
                case 15:
                    sf5 = sf6 = sf8 = 1.0f;

                    switch (rand.Next(20))
                    {
                        case 00: sf7 = 0.01f; break;
                        case 01: sf7 = 0.20f; break;
                        case 02: sf7 = 0.25f; break;
                        case 03: sf7 = 0.50f; break;
                        case 04: sf7 = 0.75f; break;
                        case 05: sf7 = 1.10f; break;
                        case 06: sf7 = 1.15f; break;
                        case 07: sf7 = 1.20f; break;
                        case 08: sf7 = 1.25f; break;
                        case 09: sf7 = 1.50f; break;
                        case 10: sf7 = 1.75f; break;
                        case 11: sf7 = 2.00f; break;
                        case 12: sf7 = 2.25f; break;
                        case 13: sf7 = 2.50f; break;
                        case 14: sf7 = 2.75f; break;
                        case 15: sf7 = 3.00f; break;
                        case 16: sf7 = 3.25f; break;
                        case 17: sf7 = 3.50f; break;
                        case 18: sf7 = 4.00f; break;
                        case 19: sf7 = 5.00f; break;
                    }

                    a = 0.01f * rand.Next(1001);
                    b = 0.01f * rand.Next(501);
                    break;

                // Smile -- Random constant + Size Randomized
                case 16:
                    sf5 = sf6 = sf8 = 1.0f;
                    sf7 = 1.0f + 0.01f * rand.Next(1001);

                    a = 0.01f * rand.Next(1001);
                    b = 0.01f * rand.Next(501);
                    break;

                // Apple-Clover-Mushroom-Mouse-1 - Pure Form
                case 17:
                    sf5 = sf6 = sf7 = sf8 = 1.0f;
                    set2params(ref sf5, ref sf7, 2.0f);

                    if (rand.Next(2) == 0)
                    {
                        a = 0.01f * rand.Next(1001);
                        b = 0.01f * rand.Next(501);
                    }
                    else
                    {
                        a = 0.9f + 0.01f * rand.Next(21);
                        b = 0.9f + 0.01f * rand.Next(21);
                    }
                    break;

                // Apple-Clover-Mushroom-Mouse-1 - Param Randomized
                case 18:
                    sf5 = sf6 = sf7 = sf8 = 1.0f;

                    switch (rand.Next(5))
                    {
                        case 0: case 1:
                            c = 0.01f * rand.Next(201);
                            break;

                        case 2: case 3:
                            c = 2.0f + 0.01f * rand.Next(501);
                            break;

                        case 4:
                            c = 2.0f + 0.01f * rand.Next(5001);
                            break;
                    }

                    set2params(ref sf5, ref sf7, c);

                    if (rand.Next(2) == 0)
                    {
                        a = 0.01f * rand.Next(1001);
                        b = 0.01f * rand.Next(501);
                    }
                    else
                    {
                        a = 0.9f + 0.01f * rand.Next(21);
                        b = 0.9f + 0.01f * rand.Next(21);
                    }
                    break;

                // Apple-Clover-Mushroom-Mouse-1 - Both Params Randomized
                case 19:
                    sf5 = sf6 = sf7 = sf8 = 1.0f;

                    switch (rand.Next(5))
                    {
                        case 0: case 1:
                            sf5 = 0.01f * rand.Next(201);
                            break;

                        case 2: case 3:
                            sf5 = 2.0f + 0.01f * rand.Next(501);
                            break;

                        case 4:
                            sf5 = 2.0f + 0.01f * rand.Next(5001);
                            break;
                    }

                    switch (rand.Next(5))
                    {
                        case 0: case 1:
                            sf7 = 0.01f * rand.Next(201);
                            break;

                        case 2: case 3:
                            sf7 = 2.0f + 0.01f * rand.Next(501);
                            break;

                        case 4:
                            sf7 = 2.0f + 0.01f * rand.Next(5001);
                            break;
                    }

                    if (rand.Next(2) == 0)
                    {
                        a = 0.01f * rand.Next(1001);
                        b = 0.01f * rand.Next(501);
                    }
                    else
                    {
                        a = 0.9f + 0.01f * rand.Next(21);
                        b = 0.9f + 0.01f * rand.Next(21);
                    }
                    break;

                // Apple-Clover-Mushroom-Mouse-1 - One param from predefined list, the second is its multiplicative
                case 20:
                    sf5 = sf6 = sf7 = sf8 = 1.0f;

                    // Get value from [0.025, 0.05, 0.1, 0.15, 0.20 ... 1.0]
                    switch (rand.Next(21))
                    {
                        case 0: sf5 = 0.025f; break;
                        case 1: sf5 = 0.050f; break;

                        case 2: case 3: case 4: case 5: case 6: case 7: case 8: case 9:
                        case 10: case 11: case 12: case 13: case 14: case 15: case 16:
                        case 17: case 18: case 19: case 20:
                            sf5 = 0.10f + 0.05f * rand.Next(19);
                            break;
                    }

                    // Increase by int sometimes
                    if (rand.Next(50) < 10)
                    {
                        sf5 += rand.Next(5);
                    }

                    sf7 = sf5 * (rand.Next(5) + 1);

                    // Swap
                    if (rand.Next(2) == 0)
                    {
                        c = sf5;
                        sf5 = sf7;
                        sf7 = c;
                    }

                    if (rand.Next(2) == 0)
                    {
                        a = 0.01f * rand.Next(1001);
                        b = 0.01f * rand.Next(501);
                    }
                    else
                    {
                        a = 0.9f + 0.01f * rand.Next(21);
                        b = 0.9f + 0.01f * rand.Next(21);
                    }
                    break;

                // Apple-Clover-Mushroom-Mouse-1 - Multiplicatives of our magic numbers
                case 21:
                    sf5 = sf6 = sf7 = sf8 = 1.0f;

                    // Get multiplier
                    switch (rand.Next(17))
                    {
                        case 0:
                            si1 = 31;
                            break;

                        case 1: case 2:
                            si1 = 21;
                            break;

                        case 3: case 4: case 5:
                            si1 = 11;
                            break;

                        case 6: case 7: case 8: case 9: case 10:
                            si1 = 5;
                            break;

                        case 11: case 12: case 13: case 14: case 15: case 16:
                            si1 = 3;
                            break;
                    }

                    // Get magic number
                    getParam1(ref c);

                    sf5 = c * (rand.Next(si1));
                    sf7 = c * (rand.Next(si1));

                    if (rand.Next(3) == 0)
                    {
                        a = 0.01f * rand.Next(1001);
                        b = 0.01f * rand.Next(501);
                    }
                    else
                    {
                        a = 0.9f + 0.01f * rand.Next(21);
                        b = 0.9f + 0.01f * rand.Next(21);
                    }
                    break;

                // Apple-Clover-Mushroom-Mouse-2 - Pure Form
                case 22:
                    sf5 = sf6 = sf7 = sf8 = 1.0f;
                    set2params(ref sf5, ref sf8, 2.0f);

                    if (rand.Next(3) == 0)
                    {
                        a = 0.01f * rand.Next(1001);
                        b = 0.01f * rand.Next(501);
                    }
                    else
                    {
                        a = 0.9f + 0.01f * rand.Next(21);
                        b = 0.9f + 0.01f * rand.Next(21);
                    }
                    break;

                // Apple-Clover-Mushroom-Mouse-2 - Param Randomized
                case 23:
                    sf5 = sf6 = sf7 = sf8 = 1.0f;

                    switch (rand.Next(5))
                    {
                        case 0: case 1:
                            c = 0.01f * rand.Next(201);
                            break;

                        case 2: case 3:
                            c = 2.0f + 0.01f * rand.Next(501);
                            break;

                        case 4:
                            c = 2.0f + 0.01f * rand.Next(5001);
                            break;
                    }

                    set2params(ref sf5, ref sf8, c);

                    if (rand.Next(3) == 0)
                    {
                        a = 0.01f * rand.Next(1001);
                        b = 0.01f * rand.Next(501);
                    }
                    else
                    {
                        a = 0.9f + 0.01f * rand.Next(21);
                        b = 0.9f + 0.01f * rand.Next(21);
                    }
                    break;

                // Apple-Clover-Mushroom-Mouse-2 - Both Params Randomized
                case 24:
                    sf5 = sf6 = sf7 = sf8 = 1.0f;

                    switch (rand.Next(5))
                    {
                        case 0: case 1:
                            sf5 = 0.01f * rand.Next(201);
                            break;

                        case 2: case 3:
                            sf5 = 2.0f + 0.01f * rand.Next(501);
                            break;

                        case 4:
                            sf5 = 2.0f + 0.01f * rand.Next(5001);
                            break;
                    }

                    switch (rand.Next(5))
                    {
                        case 0: case 1:
                            sf8 = 0.01f * rand.Next(201);
                            break;

                        case 2: case 3:
                            sf8 = 2.0f + 0.01f * rand.Next(501);
                            break;

                        case 4:
                            sf8 = 2.0f + 0.01f * rand.Next(5001);
                            break;
                    }

                    if (rand.Next(2) == 0)
                    {
                        a = 0.01f * rand.Next(1001);
                        b = 0.01f * rand.Next(501);
                    }
                    else
                    {
                        a = 0.9f + 0.01f * rand.Next(21);
                        b = 0.9f + 0.01f * rand.Next(21);
                    }
                    break;

                // Apple-Clover-Mushroom-Mouse-2 - One param from predefined list, the second is its multiplicative
                case 25:
                    sf5 = sf6 = sf7 = sf8 = 1.0f;

                    // Get value from [0.025, 0.05, 0.1, 0.15, 0.20 ... 1.0]
                    switch (rand.Next(21))
                    {
                        case 0: sf5 = 0.025f; break;
                        case 1: sf5 = 0.050f; break;

                        case 2: case 3: case 4: case 5: case 6: case 7: case 8: case 9:
                        case 10: case 11: case 12: case 13: case 14: case 15: case 16:
                        case 17: case 18: case 19: case 20:
                            sf5 = 0.10f + 0.05f * rand.Next(19);
                            break;
                    }

                    // Increase by int sometimes
                    if (rand.Next(50) < 10)
                    {
                        sf5 += rand.Next(5);
                    }

                    sf8 = sf5 * (rand.Next(5) + 1);

                    // Swap
                    if (rand.Next(2) == 0)
                    {
                        c = sf5;
                        sf5 = sf8;
                        sf8 = c;
                    }

                    if (rand.Next(2) == 0)
                    {
                        a = 0.01f * rand.Next(1001);
                        b = 0.01f * rand.Next(501);
                    }
                    else
                    {
                        a = 0.9f + 0.01f * rand.Next(21);
                        b = 0.9f + 0.01f * rand.Next(21);
                    }
                    break;

                // Apple-Clover-Mushroom-Mouse-2 - Multiplicatives of our magic numbers
                case 26:
                    sf5 = sf6 = sf7 = sf8 = 1.0f;

                    // Get multiplier
                    switch (rand.Next(17))
                    {
                        case 0:
                            si1 = 31;
                            break;

                        case 1: case 2:
                            si1 = 21;
                            break;

                        case 3: case 4: case 5:
                            si1 = 11;
                            break;

                        case 6: case 7: case 8: case 9: case 10:
                            si1 = 5;
                            break;

                        case 11: case 12: case 13: case 14: case 15: case 16:
                            si1 = 3;
                            break;
                    }

                    // Get magic number
                    getParam1(ref c);

                    sf5 = c * (rand.Next(si1));
                    sf8 = c * (rand.Next(si1));

                    if (rand.Next(3) == 0)
                    {
                        a = 0.01f * rand.Next(1001);
                        b = 0.01f * rand.Next(501);
                    }
                    else
                    {
                        a = 0.9f + 0.01f * rand.Next(21);
                        b = 0.9f + 0.01f * rand.Next(21);
                    }
                    break;

                // Apple-Clover-Mushroom-Mouse-3 - Pure Form
                case 27:
                    sf5 = sf6 = sf7 = sf8 = 1.0f;
                    set2params(ref sf6, ref sf8, 2.0f);

                    if (rand.Next(3) == 0)
                    {
                        a = 0.01f * rand.Next(1001);
                        b = 0.01f * rand.Next(501);
                    }
                    else
                    {
                        a = 0.9f + 0.01f * rand.Next(21);
                        b = 0.9f + 0.01f * rand.Next(21);
                    }
                    break;

                // Apple-Clover-Mushroom-Mouse-3 - Param Randomized
                case 28:
                    sf5 = sf6 = sf7 = sf8 = 1.0f;

                    switch (rand.Next(5))
                    {
                        case 0: case 1:
                            c = 0.01f * rand.Next(201);
                            break;

                        case 2: case 3:
                            c = 2.0f + 0.01f * rand.Next(501);
                            break;

                        case 4:
                            c = 2.0f + 0.01f * rand.Next(5001);
                            break;
                    }

                    set2params(ref sf6, ref sf8, c);

                    if (rand.Next(3) == 0)
                    {
                        a = 0.01f * rand.Next(1001);
                        b = 0.01f * rand.Next(501);
                    }
                    else
                    {
                        a = 0.9f + 0.01f * rand.Next(21);
                        b = 0.9f + 0.01f * rand.Next(21);
                    }
                    break;

                // Apple-Clover-Mushroom-Mouse-3 - One param from predefined list, the second is its multiplicative
                case 29:
                    sf5 = sf6 = sf7 = sf8 = 1.0f;

                    // Get value from [0.025, 0.05, 0.1, 0.15, 0.20 ... 1.0]
                    switch (rand.Next(21))
                    {
                        case 0: sf6 = 0.025f; break;
                        case 1: sf6 = 0.050f; break;

                        case 2: case 3: case 4: case 5: case 6: case 7: case 8: case 9:
                        case 10: case 11: case 12: case 13: case 14: case 15: case 16:
                        case 17: case 18: case 19: case 20:
                            sf6 = 0.10f + 0.05f * rand.Next(19);
                            break;
                    }

                    // Increase by int sometimes
                    if (rand.Next(50) < 10)
                    {
                        sf6 += rand.Next(5);
                    }

                    sf8 = sf6 * (rand.Next(5) + 1);

                    // Swap
                    if (rand.Next(2) == 0)
                    {
                        c = sf6;
                        sf6 = sf8;
                        sf8 = c;
                    }

                    if (rand.Next(2) == 0)
                    {
                        a = 0.01f * rand.Next(1001);
                        b = 0.01f * rand.Next(501);
                    }
                    else
                    {
                        a = 0.9f + 0.01f * rand.Next(21);
                        b = 0.9f + 0.01f * rand.Next(21);
                    }
                    break;

                // Apple-Clover-Mushroom-Mouse-3 - Multiplicatives of our magic numbers
                case 30:
                    sf5 = sf6 = sf7 = sf8 = 1.0f;

                    // Get multiplier
                    switch (rand.Next(17))
                    {
                        case 0:
                            si1 = 31;
                            break;

                        case 1: case 2:
                            si1 = 21;
                            break;

                        case 3: case 4: case 5:
                            si1 = 11;
                            break;

                        case 6: case 7: case 8: case 9: case 10:
                            si1 = 5;
                            break;

                        case 11: case 12: case 13: case 14: case 15: case 16:
                            si1 = 3;
                            break;
                    }

                    // Get magic number
                    getParam1(ref c);

                    sf6 = c * (rand.Next(si1));
                    sf8 = c * (rand.Next(si1));

                    if (rand.Next(3) == 0)
                    {
                        a = 0.01f * rand.Next(1001);
                        b = 0.01f * rand.Next(501);
                    }
                    else
                    {
                        a = 0.9f + 0.01f * rand.Next(21);
                        b = 0.9f + 0.01f * rand.Next(21);
                    }
                    break;

                // Apple-Clover-Mushroom-Mouse-4 - Pure Form
                case 31:
                    sf5 = sf6 = sf7 = sf8 = 1.0f;
                    set2params(ref sf6, ref sf7, 2.0f);

                    if (rand.Next(3) == 0)
                    {
                        a = 0.01f * rand.Next(1001);
                        b = 0.01f * rand.Next(501);
                    }
                    else
                    {
                        a = 0.9f + 0.01f * rand.Next(21);
                        b = 0.9f + 0.01f * rand.Next(21);
                    }
                    break;

                // Apple-Clover-Mushroom-Mouse-4 - Param Randomized
                case 32:
                    sf5 = sf6 = sf7 = sf8 = 1.0f;

                    switch (rand.Next(5))
                    {
                        case 0:
                        case 1:
                            c = 0.01f * rand.Next(201);
                            break;

                        case 2:
                        case 3:
                            c = 2.0f + 0.01f * rand.Next(501);
                            break;

                        case 4:
                            c = 2.0f + 0.01f * rand.Next(5001);
                            break;
                    }

                    set2params(ref sf6, ref sf7, c);

                    if (rand.Next(3) == 0)
                    {
                        a = 0.01f * rand.Next(1001);
                        b = 0.01f * rand.Next(501);
                    }
                    else
                    {
                        a = 0.9f + 0.01f * rand.Next(21);
                        b = 0.9f + 0.01f * rand.Next(21);
                    }
                    break;

                // Apple-Clover-Mushroom-Mouse-4 - One param from predefined list, the second is its multiplicative
                case 33:
                    sf5 = sf6 = sf7 = sf8 = 1.0f;

                    // Get value from [0.025, 0.05, 0.1, 0.15, 0.20 ... 1.0]
                    switch (rand.Next(21))
                    {
                        case 0: sf6 = 0.025f; break;
                        case 1: sf6 = 0.050f; break;

                        case 2: case 3: case 4: case 5: case 6: case 7: case 8: case 9:
                        case 10: case 11: case 12: case 13: case 14: case 15: case 16:
                        case 17: case 18: case 19: case 20:
                            sf6 = 0.10f + 0.05f * rand.Next(19);
                            break;
                    }

                    // Increase by int sometimes
                    if (rand.Next(50) < 10)
                    {
                        sf6 += rand.Next(5);
                    }

                    sf7 = sf6 * (rand.Next(5) + 1);

                    // Swap
                    if (rand.Next(2) == 0)
                    {
                        c = sf6;
                        sf6 = sf7;
                        sf7 = c;
                    }

                    if (rand.Next(2) == 0)
                    {
                        a = 0.01f * rand.Next(1001);
                        b = 0.01f * rand.Next(501);
                    }
                    else
                    {
                        a = 0.9f + 0.01f * rand.Next(21);
                        b = 0.9f + 0.01f * rand.Next(21);
                    }
                    break;

                // Apple-Clover-Mushroom-Mouse-4 - Multiplicatives of our magic numbers
                case 34:
                    sf5 = sf6 = sf7 = sf8 = 1.0f;

                    // Get multiplier
                    switch (rand.Next(17))
                    {
                        case 0:
                            si1 = 31;
                            break;

                        case 1: case 2:
                            si1 = 21;
                            break;

                        case 3: case 4: case 5:
                            si1 = 11;
                            break;

                        case 6: case 7: case 8: case 9: case 10:
                            si1 = 5;
                            break;

                        case 11: case 12: case 13: case 14: case 15: case 16:
                            si1 = 3;
                            break;
                    }

                    // Get magic number
                    getParam1(ref c);

                    sf6 = c * (rand.Next(si1));
                    sf7 = c * (rand.Next(si1));

                    if (rand.Next(3) == 0)
                    {
                        a = 0.01f * rand.Next(1001);
                        b = 0.01f * rand.Next(501);
                    }
                    else
                    {
                        a = 0.9f + 0.01f * rand.Next(21);
                        b = 0.9f + 0.01f * rand.Next(21);
                    }
                    break;

                // Eight - Pure Form
                case 35:
                    sf5 = sf6 = sf7 = sf8 = 1.0f;

                    sf5 = 2.0f;
                    sf6 = 2.0f;

                    if (rand.Next(3) == 0)
                    {
                        a = 0.01f * rand.Next(1001);
                        b = 0.01f * rand.Next(501);
                    }
                    else
                    {
                        a = 0.9f + 0.01f * rand.Next(21);
                        b = 0.9f + 0.01f * rand.Next(21);
                    }
                    return;

                // Eight - Param Randomized
                case 36:
                    sf5 = sf6 = sf7 = sf8 = 1.0f;

                    switch (rand.Next(5))
                    {
                        case 0: case 1:
                            c = 0.01f * rand.Next(201);
                            break;

                        case 2: case 3:
                            c = 2.0f + 0.01f * rand.Next(501);
                            break;

                        case 4:
                            c = 2.0f + 0.01f * rand.Next(5001);
                            break;
                    }

                    set2params(ref sf5, ref sf6, c);

                    if (rand.Next(3) == 0)
                    {
                        a = 0.01f * rand.Next(1001);
                        b = 0.01f * rand.Next(501);
                    }
                    else
                    {
                        a = 0.9f + 0.01f * rand.Next(21);
                        b = 0.9f + 0.01f * rand.Next(21);
                    }
                    break;

                // Eight - One param from predefined list, the second is its multiplicative
                case 37:
                    sf5 = sf6 = sf7 = sf8 = 1.0f;

                    // Get value from [0.025, 0.05, 0.1, 0.15, 0.20 ... 1.0]
                    switch (rand.Next(21))
                    {
                        case 0: sf5 = 0.025f; break;
                        case 1: sf5 = 0.050f; break;

                        case 2: case 3: case 4: case 5: case 6: case 7: case 8: case 9:
                        case 10: case 11: case 12: case 13: case 14: case 15: case 16:
                        case 17: case 18: case 19: case 20:
                            sf5 = 0.10f + 0.05f * rand.Next(19);
                            break;
                    }

                    // Increase by int sometimes
                    if (rand.Next(50) < 10)
                    {
                        sf5 += rand.Next(5);
                    }

                    sf6 = sf5 * (rand.Next(5) + 1);

                    // Swap
                    if (rand.Next(2) == 0)
                    {
                        c = sf5;
                        sf5 = sf6;
                        sf6 = c;
                    }

                    if (rand.Next(2) == 0)
                    {
                        a = 0.01f * rand.Next(1001);
                        b = 0.01f * rand.Next(501);
                    }
                    else
                    {
                        a = 0.9f + 0.01f * rand.Next(21);
                        b = 0.9f + 0.01f * rand.Next(21);
                    }
                    break;

                // Eight - Multiplicatives of our magic numbers
                case 38:
                    sf5 = sf6 = sf7 = sf8 = 1.0f;

                    // Get multiplier
                    switch (rand.Next(17))
                    {
                        case 0:
                            si1 = 31;
                            break;

                        case 1: case 2:
                            si1 = 21;
                            break;

                        case 3: case 4: case 5:
                            si1 = 11;
                            break;

                        case 6: case 7: case 8: case 9: case 10:
                            si1 = 5;
                            break;

                        case 11: case 12: case 13: case 14: case 15: case 16:
                            si1 = 3;
                            break;
                    }

                    // Get magic number
                    getParam1(ref c);

                    sf5 = c * (rand.Next(si1));
                    sf6 = c * (rand.Next(si1));

                    if (rand.Next(3) == 0)
                    {
                        a = 0.01f * rand.Next(1001);
                        b = 0.01f * rand.Next(501);
                    }
                    else
                    {
                        a = 0.9f + 0.01f * rand.Next(21);
                        b = 0.9f + 0.01f * rand.Next(21);
                    }
                    break;

                // Parabola -- Pure Form
                case 39:
                    sf5 = sf6 = sf7 = sf8 = 1.0f;
                    set2params(ref sf7, ref sf8, 2.0f);

                    if (rand.Next(3) == 0)
                    {
                        a = 0.01f * rand.Next(1001);
                        b = 0.01f * rand.Next(501);
                    }
                    else
                    {
                        a = 0.9f + 0.01f * rand.Next(21);
                        b = 0.9f + 0.01f * rand.Next(21);
                    }
                    break;

                // Parabola - Param Randomized
                case 40:
                    sf5 = sf6 = sf7 = sf8 = 1.0f;

                    switch (rand.Next(5))
                    {
                        case 0: case 1:
                            c = 0.01f * rand.Next(201);
                            break;

                        case 2: case 3:
                            c = 2.0f + 0.01f * rand.Next(501);
                            break;

                        case 4:
                            c = 2.0f + 0.01f * rand.Next(5001);
                            break;
                    }

                    set2params(ref sf7, ref sf8, c);

                    if (rand.Next(3) == 0)
                    {
                        a = 0.01f * rand.Next(1001);
                        b = 0.01f * rand.Next(501);
                    }
                    else
                    {
                        a = 0.9f + 0.01f * rand.Next(21);
                        b = 0.9f + 0.01f * rand.Next(21);
                    }
                    break;

                // Parabola - One param from predefined list, the second is its multiplicative
                case 41:
                    sf5 = sf6 = sf7 = sf8 = 1.0f;

                    // Get value from [0.025, 0.05, 0.1, 0.15, 0.20 ... 1.0]
                    switch (rand.Next(21))
                    {
                        case 0: sf7 = 0.025f; break;
                        case 1: sf7 = 0.050f; break;

                        case 2: case 3: case 4: case 5: case 6: case 7: case 8: case 9:
                        case 10: case 11: case 12: case 13: case 14: case 15: case 16:
                        case 17: case 18: case 19: case 20:
                            sf7 = 0.10f + 0.05f * rand.Next(19);
                            break;
                    }

                    // Increase by int sometimes
                    if (rand.Next(50) < 10)
                    {
                        sf7 += rand.Next(5);
                    }

                    sf8 = sf7 * (rand.Next(5) + 1);

                    // Swap
                    if (rand.Next(2) == 0)
                    {
                        c = sf7;
                        sf7 = sf8;
                        sf8 = c;
                    }

                    if (rand.Next(2) == 0)
                    {
                        a = 0.01f * rand.Next(1001);
                        b = 0.01f * rand.Next(501);
                    }
                    else
                    {
                        a = 0.9f + 0.01f * rand.Next(21);
                        b = 0.9f + 0.01f * rand.Next(21);
                    }
                    break;

                // Parabola - Multiplicatives of our magic numbers
                case 42:
                    sf5 = sf6 = sf7 = sf8 = 1.0f;

                    // Get multiplier
                    switch (rand.Next(17))
                    {
                        case 0:
                            si1 = 31;
                            break;

                        case 1: case 2:
                            si1 = 21;
                            break;

                        case 3: case 4: case 5:
                            si1 = 11;
                            break;

                        case 6: case 7: case 8: case 9: case 10:
                            si1 = 5;
                            break;

                        case 11: case 12: case 13: case 14: case 15: case 16:
                            si1 = 3;
                            break;
                    }

                    // Get magic number
                    getParam1(ref c);

                    sf7 = c * (rand.Next(si1));
                    sf8 = c * (rand.Next(si1));

                    if (rand.Next(3) == 0)
                    {
                        a = 0.01f * rand.Next(1001);
                        b = 0.01f * rand.Next(501);
                    }
                    else
                    {
                        a = 0.9f + 0.01f * rand.Next(21);
                        b = 0.9f + 0.01f * rand.Next(21);
                    }
                    break;

                // 4 randomized params - 1
                case 43:
                    // Get multiplier
                    switch (rand.Next(17))
                    {
                        case 0:
                            si1 = 31;
                            break;

                        case 1: case 2:
                            si1 = 21;
                            break;

                        case 3: case 4: case 5:
                            si1 = 11;
                            break;

                        case 6: case 7: case 8: case 9: case 10:
                            si1 = 5;
                            break;

                        case 11: case 12: case 13: case 14: case 15: case 16:
                            si1 = 3;
                            break;
                    }

                    sf5 = 1.0f * rand.Next(si1);
                    sf6 = 1.0f * rand.Next(si1);
                    sf7 = 1.0f * rand.Next(si1);
                    sf8 = 1.0f * rand.Next(si1);

                    if (rand.Next(3) == 0)
                    {
                        a = 0.01f * rand.Next(1001);
                        b = 0.01f * rand.Next(501);
                    }
                    else
                    {
                        a = 0.9f + 0.01f * rand.Next(21);
                        b = 0.9f + 0.01f * rand.Next(21);
                    }
                    break;

                case 44:
                    // Get multiplier
                    switch (rand.Next(17))
                    {
                        case 0:
                            si1 = 31;
                            break;

                        case 1: case 2:
                            si1 = 21;
                            break;

                        case 3: case 4: case 5:
                            si1 = 11;
                            break;

                        case 6: case 7: case 8: case 9: case 10:
                            si1 = 5;
                            break;

                        case 11: case 12: case 13: case 14: case 15: case 16:
                            si1 = 3;
                            break;
                    }

                    // Get magic number
                    getParam1(ref c);

                    sf5 = c * rand.Next(si1);
                    sf6 = c * rand.Next(si1);
                    sf7 = c * rand.Next(si1);
                    sf8 = c * rand.Next(si1);

                    if (rand.Next(3) == 0)
                    {
                        a = 0.01f * rand.Next(1001);
                        b = 0.01f * rand.Next(501);
                    }
                    else
                    {
                        a = 0.9f + 0.01f * rand.Next(21);
                        b = 0.9f + 0.01f * rand.Next(21);
                    }
                    break;

                // Total Random
                case 45:
                    sf5 = 0.01f * rand.Next(100);
                    sf6 = 0.01f * rand.Next(100);
                    sf7 = 0.01f * rand.Next(100);
                    sf8 = 0.01f * rand.Next(100);
                    break;
            }

            // Random sign for each of the params
            sf5 *= myUtils.randomSign(rand);
            sf6 *= myUtils.randomSign(rand);
            sf7 *= myUtils.randomSign(rand);
            sf8 *= myUtils.randomSign(rand);

            return;
        }
        // ---------------------------------------------------------------------------------------------------------------

        private void constSetup5()
        {
            constSetup1();
            x1 = rand.Next(100);
            x2 = gl_Width - x1;

            si1 = gl_y0;
            si2 = gl_y0;

            sf1 = 0.001f * rand.Next(1111);

            switch (rand.Next(2))
            {
                case 0:
                    sf2 = 0.001f * rand.Next(1111);
                    break;

                case 1:
                    sf2 = sf1 * (rand.Next(111) + 1);
                    break;
            }

            switch (rand.Next(3))
            {
                case 0:
                    sf3 = gl_y0;
                    sf4 = gl_y0;
                    break;

                case 1:
                    sf3 = rand.Next(gl_y0);
                    sf4 = rand.Next(gl_y0);
                    break;

                case 2:
                    si1 = rand.Next(gl_Height);
                    si2 = rand.Next(gl_Height);
                    sf3 = rand.Next(gl_y0);
                    sf4 = rand.Next(gl_y0);
                    break;
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void constSetup6()
        {
            constSetup1();

            sf1 = 1.0f + 0.1f * rand.Next(101);
            si1 = 100 + rand.Next(3000);
            si2 = rand.Next(2);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------
    }
};
