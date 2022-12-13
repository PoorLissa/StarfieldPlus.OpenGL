using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;


/*
    - Splines
*/


namespace my
{
    public class myObj_132 : myObject
    {
        private static int N = 1;

        static int max_dSize = 0, t = 0, tDefault = 0, shape = 0, si1 = 0, si2 = 0;
        static bool isDimmableGlobal = true, isDimmableLocal = false, needNewScreen = false, doFillShapes = false;
        static float sf1 = 0, sf2 = 0, sf3 = 0, sf4 = 0, sf5 = 0, sf6 = 0, sf7 = 0, sf8 = 0, fLifeCnt = 0, fdLifeCnt = 0;
        static float a = 0, b = 0, c = 0;
        private static float dimAlpha = 0.05f;

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

            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time initialization
        private void initLocal()
        {
            gl_x0 = gl_Width  / 2;
            gl_y0 = gl_Height / 2;

            doClearBuffer = false;

            N = (N == 0) ? 10 + rand.Next(10) : N;

            max_dSize = rand.Next(15) + 3;
            isDimmableGlobal = rand.Next(2) == 0;
            tDefault = 33;

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            height = 800;

            string str = $"Obj = myObj_132\n\n" +
                            $"N = {N} of {list.Count}\n" +
                            $"shape = {shape}\n" +
                            $"dimAlpha = {dimAlpha}\n" +
                            $"dSize = {dSize}\n" +
                            $""
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
            dimAlpha = 0.05f;                                           // Restore dim speed to its original value

            fLifeCnt = 255.0f;
            fdLifeCnt = 0.25f;

            x = rand.Next(gl_Width);
            y = rand.Next(gl_Height);

            float_B = 1.0f;

            colorPicker.getColor(x, y, ref r1, ref g1, ref b1);
            if (r1 + g1 + b1 < 0.25f)
            {
                r1 += 0.1f; g1 += 0.1f; b1 += 0.1f;
            }

            colorPicker.getColorRand(ref r2, ref g2, ref b2);
            if (r2 + g2 + b2 < 0.25f)
            {
                r2 += 0.1f; g2 += 0.1f; b2 += 0.1f;
            }

            maxSize = rand.Next(333) + 33;
            shape = rand.Next(91);
            isDimmableGlobal = rand.Next(2) == 0;
            isDimmableLocal = false;

            t = tDefault;
            t -= isDimmableGlobal ? 13 : 0;

#if false
            fdLifeCnt = 0.01f;
            shape = 1300;
            shape = 91;
            t = 3;
#endif

shape = 76;

            size = 1;
            dSize = rand.Next(max_dSize) + 1;
            dSize = 0.1f + 0.1f * rand.Next(max_dSize * 10);
            dA = rand.Next(5) + 1;
            dA = 1;
            dA_Filling = rand.Next(5) + 2;

            time1 = 0.0f;
            time2 = 0.0f;
            dt1 = 0.1f;
            dt2 = 0.01f;

            needNewScreen = true;

            setUpConstants();

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void setUpConstants()
        {
            switch (shape)
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
                    needNewScreen = false;
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

                    needNewScreen = false;
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
            int tNow = System.DateTime.Now.Millisecond;

            size += dSize;
            time1 += dt1;
            time2 += dt2;

            dx = (float)(Math.Sin(time1)) * 5 * size / 10;
            dy = (float)(Math.Cos(time1)) * 5 * size / 10;

            x += dx;
            y += dy;

            move_0();

            if ((fLifeCnt -= fdLifeCnt) < 0)
            {
                generateNew();
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

            switch (shape)
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

            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Show()
        {
            //p.Color = Color.FromArgb(100, R, G, B);

            switch (shape)
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

                case 05:case 06:case 07:case 08:case 09:case 10:case 11:case 12:case 13:
                case 14:case 15:case 16:case 17:case 18:case 19:case 20:case 21:case 22:
                case 23:case 24:case 25:case 26:case 27:case 28:
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

                case 40:case 41:case 42:case 43:case 44:case 45:case 46:case 47:case 48:
                case 49:case 50:case 51:case 52:case 53:case 54:case 55:case 56:case 57:
                case 58:case 59:case 60:case 61:case 62:case 63:case 64:case 65:case 66:
                case 67:case 68:case 69:case 70:case 71:case 72:case 73:case 74:case 75:
                case 76:case 77:case 78:case 79:case 80:case 81:case 82:case 83:case 85:
                case 86:case 87:case 88:
                    myPrimitive._Line.SetColor(r2, g2, b2, 0.5f);
                    myPrimitive._Line.Draw(x1, y1, x2, y2);

                    myPrimitive._Rectangle.SetColor(0.5451f, 0.0f, 0.0f, 1.0f);
                    myPrimitive._Rectangle.Draw(x1, y1, 3, 3, false);

                    myPrimitive._Rectangle.SetColor(1.0f, 0.55f, 0.0f, 1.0f);
                    myPrimitive._Rectangle.Draw(x2, y2, 3, 3, false);
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

            if (doClearBuffer)
            {
                glDrawBuffer(GL_FRONT_AND_BACK | GL_DEPTH_BUFFER_BIT);
                glClearColor(0, 0, 0, 1);
            }
            else
            {
                glDrawBuffer(GL_FRONT_AND_BACK);
            }

            list.Add(new myObj_132());

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
                }

                // Render Frame
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

            //base.initShapes(shape, N, 0);

            return;
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
            throw new System.Exception("joppa2");
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void constSetup3()
        {
            throw new System.Exception("joppa3");
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void constSetup4()
        {
            throw new System.Exception("joppa4");
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void constSetup5()
        {
            throw new System.Exception("joppa5");
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void constSetup6()
        {
            throw new System.Exception("joppa6");
        }

        // ---------------------------------------------------------------------------------------------------------------
    }
};
