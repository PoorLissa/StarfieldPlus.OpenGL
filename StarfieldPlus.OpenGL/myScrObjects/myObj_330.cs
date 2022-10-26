using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;


/*
    -- Textures, Take 1
    https://www.youtube.com/watch?v=sA4p3wuDLo8&ab_channel=FranklyGaming
*/


namespace my
{
    public class myObj_330 : myObject
    {
        public float x, y, X, Y, dx, dy, a, da;
        public int width, height, cnt;

        static int N = 1, max1 = 1, max2 = 1, opacityFactor = 1;
        static int[] param = new int[6];
        static int mode = 0;

        static bool doClearBuffer = false, doCreateAtOnce = true, doSampleOnce = false, doUseRandDxy = false, doDrawSrcImg = false;
        static float dimAlpha = 0.05f;

        static myTexRectangle tex = null;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_330()
        {
            if (colorPicker == null)
            {
                colorPicker = new myColorPicker(gl_Width, gl_Height, mode: myColorPicker.colorMode.SNAPSHOT_OR_IMAGE);
                list = new List<myObject>();

                init();
            }

            generateNew();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time initialization
        private void init()
        {
            gl_x0 = gl_Width  / 2;
            gl_y0 = gl_Height / 2;

            // Set default params
            for (int i = 0; i < param.Length; i++)
                param[i] = 0;

            // todo: another mode. like 24, but the waves are wider and are going maybe in radial direction. the objects are generated with sin or cos or smth
            mode = rand.Next(32);
            mode = 31;

            opacityFactor = rand.Next(3) + 1 + (myUtils.randomChance(rand, 1, 7) ? rand.Next(3) : 0);

            doCreateAtOnce = myUtils.randomBool(rand);
            doClearBuffer  = myUtils.randomBool(rand);
            doUseRandDxy   = myUtils.randomBool(rand);
            doSampleOnce   = false;
            doDrawSrcImg   = false;

            switch (mode)
            {
                // Random rectangles appear at random locations each iteration
                // The narrower the rectangle, the higher is its opacity
                case 0:
                    doClearBuffer = false;
                    dimAlpha /= 10;
                    N = 10;
                    break;

                // Random very narrow rectangles (1 or 2 px) appear at random locations each iteration
                case 1:
                    doClearBuffer = false;
                    dimAlpha /= 10;
                    max1 = 333 + rand.Next(666);
                    N = 10;
                    break;

                // Random squares moving through the screen using various movement patterns
                case 2:
                case 3:
                case 4:
                    doSampleOnce = doClearBuffer ? myUtils.randomBool(rand) : false;
                    dimAlpha /= 3;
                    N = 999 + rand.Next(666);
                    break;

                // 5; Pieces appear on the central line and then move up or down
                // 6; Pieces appear at the top part of the screen and then move down
                case 5:
                case 6:
                    dimAlpha /= 3;
                    max1 = rand.Next(45) + 3;
                    N = 999 + rand.Next(666);
                    break;

                // Random pieces of the image constantly appearing at their own locations
                // Each piece's opacity grows, then fades away
                case 7:
                    dimAlpha /= 3;
                    doClearBuffer = true;
                    N = 111 + rand.Next(666);
                    max1 = N > 450 ? 99 : 125;
                    break;

                // Random pieces of the image constantly appearing at their own locations
                // The screen IS cleared between frames
                case 8:
                    doClearBuffer = true;
                    N = 999 + rand.Next(111);
                    max1 = 50;
                    break;

                // Random pieces of the image constantly appearing at their own locations (with or without some offset)
                // The screen is NOT cleared between frames
                case 9:
                    if (rand.Next(3) == 0)
                    {
                        dimAlpha /= (rand.Next(11) + 1);
                    }
                    else
                    {
                        dimAlpha /= 5;
                    }

                    doClearBuffer = false;
                    doUseRandDxy = myUtils.randomChance(rand, 2, 3);
                    N = 999 + rand.Next(111);
                    max1 = rand.Next(50) + 25;

                    param[0] = rand.Next(2);

                    // Bluring effect strength
                    if (rand.Next(3) == 0)
                    {
                        max2 = rand.Next(23) + 1;
                    }
                    else
                    {
                        max2 = rand.Next(5) + 1;
                    }
                    break;

                // Random pieces of the image constantly appearing at random locations
                case 10:
                    if (rand.Next(3) == 0)
                    {
                        dimAlpha /= (rand.Next(11) + 1);
                    }
                    else
                    {
                        dimAlpha /= 5;
                    }

                    doClearBuffer = false;
                    N = (999 + rand.Next(111)) / (rand.Next(11) + 1);
                    max1 = rand.Next(50) + 25;
                    break;

                // Squares moving on the screen, while decreasing in size
                case 11:
                    doClearBuffer = false;
                    N = (999 + rand.Next(111)) / (rand.Next(11) + 1);
                    max1 = rand.Next(150) + 25;
                    break;

                // Long thin lines randomly flickering on the screen (both horizontal and vertical)
                case 12:
                    N = 2000 + rand.Next(3333);
                    max1 = rand.Next(333) + 125;
                    break;

                // Long thin lines randomly flickering on the screen (horizontal, vertical)
                case 13:
                case 14:
                    N = 2000 + rand.Next(1111);
                    max1 = rand.Next(333) + 125;
                    break;

                // Long parallel lines slowly moving into the view from the outside of the screen
                // todo: 16 and 17 to be implemented yet (vertical and cross movement) 
                case 15:
                case 16:
                case 17:
                    N = 1000 + rand.Next(333);
                    max1 = rand.Next(666) + 125;
                    max2 = rand.Next( 33) +  12;
                    param[0] = rand.Next(7);
                    break;

                // Squares moving around, changing direction of movement occasionally
                // The moving pattern is based on 90-degrees turns
                case 18:
                    N = rand.Next(1111) + 100;
                    param[0] = rand.Next(3);
                    param[1] = rand.Next(4);
                    max1 = rand.Next(33) + 25;
                    max2 = rand.Next(333) + 25;
                    break;

                // Snake-alike chain of squares
                case 19:
                    N = rand.Next(1111) + 111;
                    max1 = rand.Next(111) + 25;
                    param[0] = rand.Next(2);
                    break;

                // Squares moving sideways, bouncing off the walls
                case 20:
                    N = rand.Next(3333) + 333;
                    max1 = rand.Next(50) + 1;
                    param[0] = rand.Next(2);
                    param[1] = rand.Next(2);
                    break;

                // Random pieces of the image constantly appear at their own locations
                // Each piece then fades away
                // 21; Grid alignment: NO
                // 22; Grid alignment: YES
                case 21:
                case 22:
                    N = rand.Next(500) + 111;
                    max1 = rand.Next(200) + 13;                                             // Max size of a cell
                    param[0] = rand.Next(2);                                                // Random size
                    param[1] = rand.Next(2) == 0 ? rand.Next(33) : rand.Next(333);          // Number of iterations before the fading begins
                    param[2] = rand.Next(10) + 1;                                           // Distance between the grid cells
                    param[3] = rand.Next(2);                                                // Instead of centering smaller squares, randomly move them within the cell
                    param[4] = rand.Next(2) == 0 ? 0 : rand.Next(7);                        // Forcefully make every square smaller then its cell -- to enchance param[3]'s effect
                    param[5] = rand.Next(9);                                                // Cell rotation mode
                    doClearBuffer = false;
                    dimAlpha /= (rand.Next(3) + 1);
                    break;

                // undeveloped yet -- drawing textures using lines with 1px empty intervals between the lines
                case 23:
                    N = 111;
                    max1 = rand.Next(200) + 13;                                             // Max size
                    doClearBuffer = false;
                    dimAlpha /= (rand.Next(5) + 1);
                    break;

                // Drawing horizontal rows or vertical columns using 1px thick lines;
                // The lines are drawn with some interval between them
                case 24:
                    N = rand.Next(500) + 33;
                    max1 = myUtils.randomChance(rand, 1, 10) ? (rand.Next(3333) + 111) : (rand.Next(222) + 111);
                    param[0] = rand.Next(11) + 1;                                           // Min interval between the lines
                    param[1] = rand.Next(111) + 11;                                         // Min width/height of a line
                    param[2] = rand.Next(7);                                                // Drawing mode: up, down, left, right, up+down, left+right, up+down+left+right
                    param[3] = rand.Next(2) * rand.Next(11);                                // Multiplier for sin/cos
                    param[4] = rand.Next(2) * rand.Next(11);                                // Multiplier for sin/cos
                    doClearBuffer = myUtils.randomChance(rand, 1, 11);                      // In case the buffer is cleared, line thickness is going to be more than 1
                    N += doClearBuffer ? rand.Next(333) : 0;
                    dimAlpha /= (rand.Next(5) + 1);
                    break;

                // Moving in increasing squares
                case 25:
                    N = rand.Next(333) + 666;
                    max1 = rand.Next(66) + 1;
                    doClearBuffer = myUtils.randomChance(rand, 1, 2);
                    dimAlpha /= (rand.Next(5) + 1);
                    break;

                // 'Gravitational pull' towards the center axes of the screen
                case 26:
                    N = rand.Next(333) + 666;
                    max1 = rand.Next(50) + 3;
                    param[0] = rand.Next(6);                                                // x-axis, y-axis, both axes
                    break;

                // Lots of static rotating textures
                case 27:
                    N = rand.Next(666) + 3333;
                    doClearBuffer = myUtils.randomChance(rand, 1, 5);
                    param[0] = rand.Next(222) + 1;
                    dimAlpha /= (rand.Next(11) + 1);
                    break;

                // Lots of static slightly rocking textures
                case 28:
                    N = rand.Next(666) + 3333;
                    doClearBuffer = myUtils.randomChance(rand, 1, 5);
                    param[0] = rand.Next(222) + 1;
                    dimAlpha /= (rand.Next(11) + 1);
                    break;

                // Draw cells, where each cell is rotated (once or several times)
                // 29: grid-aligned
                // 30: not aligned
                case 29:
                case 30:
                    N = rand.Next(666) + 333;
                    doClearBuffer = myUtils.randomChance(rand, 1, 5);
                    dimAlpha /= (rand.Next(11) + 1);
                    max1 = rand.Next(200) + 13;                                             // Max size of a cell
                    param[0] = rand.Next(4);                                                // Drawing mode
                    param[2] = rand.Next(10) + 1;                                           // Distance between the grid cells
                    break;

                // 
                case 31:
                    N = rand.Next(666) + 333;
                    doClearBuffer = myUtils.randomChance(rand, 1, 5);
                    dimAlpha /= (rand.Next(11) + 1);
                    max1 = rand.Next(200) + 13;                                             // Max size of a cell
                    param[0] = rand.Next(2);                                                // Drawing mode
                    param[2] = rand.Next(10) + 1;                                           // Distance between the grid cells
                    break;
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            string str_params = "";

            for (int i = 0; i < param.Length; i++)
            {
                str_params += i == 0 ? $"{param[i]}" : $", {param[i]}";
            }

            string str = $"Obj = myObj_330\n\n" +
                            $"N = {N} ({list.Count})\n" +
                            $"mode = {mode}\n" +
                            $"dimAlpha = {dimAlpha}\n" +
                            $"doClearBuffer = {doClearBuffer}\n" +
                            $"doSampleOnce  = {doSampleOnce}\n" +
                            $"opacityFactor = {opacityFactor}\n" +
                            $"doUseRandDxy  = {doUseRandDxy}\n" +
                            $"param: [{str_params}]"
            ;
            return str;
        }

        // ---------------------------------------------------------------------------------------------------------------

        // 
        protected override void setNextMode()
        {
            init();
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void generateNew()
        {
            a = (float)rand.NextDouble() / opacityFactor;
            cnt = 0;

            switch (mode)
            {
                case 2:
                    width = rand.Next(33) + 5;
                    x = X = rand.Next(gl_Width);
                    y = Y = rand.Next(gl_Height);
                    dx = (float)rand.NextDouble() * myUtils.randomSign(rand) * 5;
                    dy = (float)rand.NextDouble() * myUtils.randomSign(rand) * 5;
                    break;

                case 3:
                    width = rand.Next(33) + 5;
                    x = X = rand.Next(gl_Width);
                    y = Y = rand.Next(gl_Height);
                    dx = dy = 0;

                    if (rand.Next(2) == 0)
                        dx = (float)rand.NextDouble() * myUtils.randomSign(rand) * 5;
                    else
                        dy = (float)rand.NextDouble() * myUtils.randomSign(rand) * 5;
                    break;

                case 4:
                    width = rand.Next(33) + 5;
                    x = X = rand.Next(gl_Width);
                    y = Y = rand.Next(gl_Height);
                    dx = (float)rand.NextDouble() * myUtils.randomSign(rand) * 5;
                    dy = (float)rand.NextDouble() * myUtils.randomSign(rand) * 5;

                    if (rand.Next(2) == 0)
                    {
                        dx = dy = 0;

                        if (rand.Next(2) == 0)
                            dx = (float)rand.NextDouble() * myUtils.randomSign(rand) * 5;
                        else
                            dy = (float)rand.NextDouble() * myUtils.randomSign(rand) * 5;
                    }
                    break;

                case 5:
                    width = rand.Next(max1) + 1;
                    x = X = rand.Next(gl_Width);
                    y = Y = gl_y0;
                    dx = 0;
                    dy = (float)rand.NextDouble() * myUtils.randomSign(rand) * 5;
                    da = (float)rand.NextDouble() / 25;
                    break;

                case 6:
                    width = rand.Next(max1) + 1;
                    x = X = rand.Next(gl_Width);
                    y = Y = rand.Next(gl_Height / 5);
                    dx = 0;
                    dy = (float)rand.NextDouble() * 5;
                    da = (float)rand.NextDouble() / 33;
                    break;

                case 7:
                    width = rand.Next(max1) + 1;
                    x = X = rand.Next(gl_Width);
                    y = Y = rand.Next(gl_Height);
                    dx = 0;
                    dy = 0;
                    da = (float)rand.NextDouble() / (rand.Next(N/10) + 1);

                    if (doUseRandDxy)
                    {
                        dx = myUtils.randomSign(rand) * (float)rand.NextDouble() / 33;
                        dy = myUtils.randomSign(rand) * (float)rand.NextDouble() / 33;
                    }

                    if (da < 0.0005f)
                        da = 0.0005f;

                    if (da > 0.01f)
                        da = 0.01f;

                    a = 0;
                    break;

                case 8:
                    a = (float)rand.NextDouble() / 3;
                    X = rand.Next(11) + 1;
                    break;

                case 9:
                case 10:
                    break;

                case 11:
                    width = height = rand.Next(max1) + 1;
                    x = X = rand.Next(gl_Width);
                    y = Y = rand.Next(gl_Height);
                    dx = myUtils.randomSign(rand) * (float)rand.NextDouble() * 5;
                    dy = myUtils.randomSign(rand) * (float)rand.NextDouble() * 5;
                    da = (float)rand.NextDouble() / 33;
                    break;

                case 12:
                case 13:
                case 14:
                    break;

                case 15:
                case 16:
                case 17:
                    width  = (myUtils.randomChance(rand, 1, 33) ? rand.Next(3*max1) : rand.Next(max1)) + 100;
                    height = rand.Next(max2) + 3;

                    switch (param[0])
                    {
                        case 0: case 1: case 2:
                            height = param[0] + 1;
                            break;

                        case 3:
                            height = rand.Next(3) + 1;
                            break;
                    }

                    a = (float)rand.NextDouble();
                    a /= doClearBuffer ? 1 : (rand.Next(3)+1);
                    y = Y = rand.Next(gl_Height);

                    if (rand.Next(2) == 0)
                    {
                        x = X = 0 - width - rand.Next(width);
                        dx = rand.Next(5) * (float)rand.NextDouble() + 0.1f;
                    }
                    else
                    {
                        x = X = gl_Width + width + rand.Next(width);
                        dx = rand.Next(5) * -(float)rand.NextDouble() - 0.1f;
                    }
                    break;

                case 18:

                    switch (param[0])
                    {
                        case 0:
                            width = height = max1;
                            break;

                        case 1:
                            width = height = rand.Next(max1 * 2) + 15;
                            break;

                        case 2:
                            width = height = rand.Next(max1 * 3) + 10;
                            break;
                    }

                    a = (float)rand.NextDouble();
                    x = X = rand.Next(gl_Width);
                    y = Y = rand.Next(gl_Height);

                    dx = myUtils.randomSign(rand) * (float)rand.NextDouble() * (rand.Next(5)+1);

                    switch (param[1])
                    {
                        case 0:
                            dy = 0;
                            break;

                        case 1:
                            dy = dx * myUtils.randomSign(rand) * 0.1f;
                            break;

                        case 2:
                            dy = dx * myUtils.randomSign(rand) / (rand.Next(23) + 1);
                            break;

                        case 3:
                            dy = myUtils.randomSign(rand) * (float)rand.NextDouble() * (rand.Next(5)+1);
                            break;
                    }

                    if (rand.Next(2) == 0)
                    {
                        float tmp = dx; dx = dy; dy = tmp;
                    }
                    break;

                case 19:
                    switch (param[0])
                    {
                        case 0:
                            width = height = max1;
                            break;

                        case 1:
                            width = height = rand.Next(max1) + 11;
                            break;
                    }

                    a = (float)rand.NextDouble();
                    x = X = 0 - rand.Next(5*width) - width;
                    y = Y = gl_Height - height;

                    dx = (float)rand.NextDouble() * (rand.Next(111) + 1) + 0.01f;
                    break;

                case 20:
                    switch (param[0])
                    {
                        case 0: width = max1;                break;
                        case 1: width = rand.Next(max1) + 1; break;
                    }

                    a = (float)rand.NextDouble();
                    x = X = rand.Next(gl_Width);
                    y = Y = rand.Next(gl_Height);
                    height = width;

                    switch (param[1])
                    {
                        case 1: y = Y = Y - Y % height; break;
                    }

                    dx = myUtils.randomSign(rand) * (float)rand.NextDouble() * (rand.Next(33) + 1) + 0.01f;
                    break;

                case 21:
                case 22:
                    switch (param[0])
                    {
                        case 0: width = height = max1; break;
                        case 1: width = height = rand.Next(max1) + 1; break;
                    }

                    a = (float)rand.NextDouble();
                    x = X = rand.Next(gl_Width);
                    y = Y = rand.Next(gl_Height);

                    // Align squares to grid
                    if (mode == 22)
                    {
                        // 
                        if (param[4] > 0 && param[0] == 1 && width > param[4])
                        {
                            width  -= param[4];
                            height -= param[4];
                        }

                        // Align to grid
                        x -= x % (max1 + param[2]);
                        y -= y % (max1 + param[2]);

                        // Center smaller squares in the grid cell
                        if (width < max1)
                        {
                            x += (max1 - width) / 2;
                            y += (max1 - width) / 2;

                            // Or randomly move the squares within the bounds of the cell
                            if (param[3] != 0)
                            {
                                int n = (max1 - width) / 2;

                                x += myUtils.randomSign(rand) * rand.Next(n);
                                y += myUtils.randomSign(rand) * rand.Next(n);
                            }
                        }

                        // Offset grid cells, so the pattern is symmetrical on the screen
                        {
                            int w = max1 + param[2];
                            int n = (gl_Height + param[2]) % w;

                            if (n != 0)
                            {
                                n = (gl_Height) % w;
                                y -= (max1 - (max1 + n)/2);
                            }

                            n = (gl_Width + param[2]) % w;

                            if (n != 0)
                            {
                                n = (gl_Width) % w;
                                x -= (max1 - (max1 + n)/2);
                            }
                        }
                    }

                    da = -1 * (0.01f + (float)rand.NextDouble() / 10);
                    cnt = 0;
                    break;

                case 23:
                    a = (float)rand.NextDouble();
                    x = rand.Next(gl_Width);
                    y = rand.Next(gl_Height);
                    height = rand.Next(max1) + 10;
                    width = height / 2;
                    width = rand.Next(max1) + 10;
                    cnt = 0;
                    X = rand.Next(20) + 1;
                    break;

                case 24:
                    a = (float)rand.NextDouble();
                    width = height = rand.Next(max1) + param[1];

                    bool isVertical = false, isRight = false, isDown = false;

                    // Drawing mode: up, down, left, right, up + down, left + right, up + down + left + right
                    switch (param[2])
                    {
                        case 0:
                            isVertical = true;
                            isDown = false;
                            break;

                        case 1:
                            isVertical = true;
                            isDown = true;
                            break;

                        case 2:
                            isVertical = false;
                            isRight = false;
                            break;

                        case 3:
                            isVertical = false;
                            isRight = true;
                            break;

                        case 4:
                            isVertical = true;
                            isDown = myUtils.randomBool(rand);
                            break;

                        case 5:
                            isVertical = false;
                            isRight = myUtils.randomBool(rand);
                            break;

                        default:
                            isVertical = myUtils.randomBool(rand);
                            isRight    = myUtils.randomBool(rand);
                            isDown     = myUtils.randomBool(rand);
                            break;
                    }

                    if (isVertical)
                    {
                        // Vertical movement
                        height = doClearBuffer ? rand.Next(23) + 5 : 1;
                        x = rand.Next(gl_Width);
                        y = isDown ? -1 : gl_Height + 1;
                        dx = 0;
                        dy = (rand.Next(33) + param[0]) * (isDown ? 1 : -1);
                    }
                    else
                    {
                        // Horizontal movement
                        width = doClearBuffer ? rand.Next(23) + 5 : 1;
                        x = isRight ? -1 : gl_Width + 1;
                        y = rand.Next(gl_Height);
                        dx = (rand.Next(33) + param[0]) * (x < 0 ? 1 : -1);
                        dy = 0;
                    }

                    cnt = 0;
                    break;

                case 25:
                    a = (float)rand.NextDouble();
                    x = rand.Next(gl_Width);
                    y = rand.Next(gl_Height);
                    width = height = rand.Next(max1) + 10;

                    dx = (float)rand.NextDouble() * (rand.Next(111) + 1) * 0.1f + 0.001f;
                    dy = 0;
                    cnt = 0;
                    X = 2;
                    break;

                case 26:
                    a = (float)rand.NextDouble();
                    x = rand.Next(gl_Width);
                    y = rand.Next(gl_Height);
                    width = height = rand.Next(max1) + 3;

                    dx = 0;
                    dy = 0;
                    X = (float)rand.NextDouble() * (x < gl_Width /2 ? 1 : -1);
                    Y = (float)rand.NextDouble() * (y < gl_Height/2 ? 1 : -1);
                    break;

                case 27:
                    x = rand.Next(gl_Width);
                    y = rand.Next(gl_Height);
                    a = (float)rand.NextDouble() / 3;
                    X = 0;
                    cnt = rand.Next(500) + 33;
                    dx = myUtils.randomSign(rand) * (float)rand.NextDouble() * (rand.Next(33) + 1);
                    width  = rand.Next(123) + 33;
                    height = rand.Next(param[0]) + 1;
                    break;

                case 28:
                    x = rand.Next(gl_Width + 400) - 200;
                    y = rand.Next(gl_Height);
                    a = (float)rand.NextDouble()/7;
                    width = rand.Next(1111) + 111;
                    height = rand.Next(21) + 1;
                    X = 0;
                    cnt = rand.Next(500) + 33;
                    dx = myUtils.randomSign(rand) * (float)rand.NextDouble() * (rand.Next(33) + 1);
                    cnt = 0;
                    break;

                case 29:
                case 30:
                case 31:
                    a = (float)rand.NextDouble();
                    x = rand.Next(gl_Width);
                    y = rand.Next(gl_Height);

                    X = Y = -1111;
                    width = height = max1;

                    // Align to grid
                    if (mode == 29 || mode == 31)
                    {
                        x -= x % (max1 + param[2]);
                        y -= y % (max1 + param[2]);

                        // Offset grid cells, so the pattern is symmetrical on the screen
                        {
                            int w = max1 + param[2];
                            int n = (gl_Height + param[2]) % w;

                            if (n != 0)
                            {
                                n = (gl_Height) % w;
                                y -= (max1 - (max1 + n) / 2);
                            }

                            n = (gl_Width + param[2]) % w;

                            if (n != 0)
                            {
                                n = (gl_Width) % w;
                                x -= (max1 - (max1 + n) / 2);
                            }
                        }
                    }

                    da = -1 * (0.01f + (float)rand.NextDouble() / 10);

                    // Different cnt for different modes
                    cnt = mode == 31 ? 333: 111;
                    cnt = rand.Next(cnt) + 11;
                    break;
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            switch (mode)
            {
                case 0:
                    x = X = rand.Next(gl_Width);
                    y = Y = rand.Next(gl_Height);
                    width = rand.Next(133) + 1;
                    height = rand.Next(133) + 1;
                    a = 133.0f / width / height;

                    if (doUseRandDxy)
                    {
                        x += myUtils.randomSign(rand) * rand.Next(3);
                        y += myUtils.randomSign(rand) * rand.Next(3);
                    }
                    break;

                case 1:
                    x = X = rand.Next(gl_Width);
                    y = Y = rand.Next(gl_Height);

                    if (rand.Next(2) == 0)
                    {
                        width = rand.Next(max1) + 1;
                        height = myUtils.randomChance(rand, 1, 11) ? 2 : 1;
                        a = 0.1f * max1 / width;
                        //width = gl_Width;
                    }
                    else
                    {
                        height = rand.Next(max1) + 1;
                        width = myUtils.randomChance(rand, 1, 11) ? 2 : 1;
                        a = 0.1f * max1 / height;
                        //height = gl_Height;
                    }

                    if (doUseRandDxy)
                    {
                        x += myUtils.randomSign(rand) * rand.Next(3);
                        y += myUtils.randomSign(rand) * rand.Next(3);
                    }
                    break;

                case 2:
                case 3:
                case 4:
                    if (doUseRandDxy && rand.Next(10) == 0)
                    {
                        dx += myUtils.randomSign(rand) * (float)rand.NextDouble();
                        dy += myUtils.randomSign(rand) * (float)rand.NextDouble();
                    }

                    x += dx;
                    y += dy;

                    if (x < 0 || x > gl_Width)
                        dx *= -1;

                    if (y < 0 || y > gl_Height)
                        dy *= -1;
                    break;

                case 5:
                case 6:
                    y += dy;
                    a -= da;

                    if (y < -width || y > gl_Height + width)
                    {
                        generateNew();
                        return;
                    }
                    break;

                case 7:
                    if (a > 0.75f && da > 0)
                        da *= -1;

                    x += dx;
                    y += dy;
                    a += da;
                    break;

                case 8:
                    if (--X <= 0)
                    {
                        width = rand.Next(max1) + 1;
                        height = rand.Next(max1) + 1;
                        x = rand.Next(gl_Width);
                        y = rand.Next(gl_Height);
                        X = rand.Next(66) + 1;
                    }
                    break;

                case 9:
                    x = X = rand.Next(gl_Width);
                    y = Y = rand.Next(gl_Height);

                    doUseRandDxy = true;
                    param[0] = 1;

                    if (param[0] == 0)
                    {
                        width = rand.Next(max1) + 1;
                        height = rand.Next(max1) + 1;
                    }
                    else
                    {
                        width = height = rand.Next(max1) + 1;
                    }

                    if (doUseRandDxy)
                    {
                        X += myUtils.randomSign(rand) * (rand.Next(2*max2+1) - max2);
                        Y += myUtils.randomSign(rand) * (rand.Next(2*max2+1) - max2);
                    }
                    break;

                case 10:
                    width = rand.Next(max1) + 1;
                    height = rand.Next(max1) + 1;
                    x = rand.Next(gl_Width);
                    y = rand.Next(gl_Height);
                    X = rand.Next(gl_Width);
                    Y = rand.Next(gl_Height);
                    break;

                case 11:
                    x += dx;
                    y += dy;
                    a -= da;
                    width -= 1;
                    height -= 1;

                    if (true)
                    {
                        dx += (float)rand.NextDouble() * myUtils.randomSign(rand);
                        dy += (float)rand.NextDouble() * myUtils.randomSign(rand);
                    }

                    if (x < 0 || x > gl_Width || y < 0 || y > gl_Height || width < 0 || height < 0)
                    {
                        generateNew();
                        return;
                    }
                    break;

                case 12:
                    tex.setOpacity(rand.NextDouble());
                    width = height = 1;
                    x = rand.Next(gl_Width);
                    y = rand.Next(gl_Height);

                    if (rand.Next(2) == 0)
                    {
                        if (rand.Next(9) == 0)
                            height++;
                        width += rand.Next(max1);
                    }
                    else
                    {
                        if (rand.Next(9) == 0)
                            width++;
                        height += rand.Next(max1);
                    }
                    break;

                case 13:
                    tex.setOpacity(rand.NextDouble());
                    height = 1;
                    x = rand.Next(gl_Width+max1)-max1;
                    y = rand.Next(gl_Height);

                    if (rand.Next(9) == 0)
                        height++;
                    width = rand.Next(max1);
                    break;

                case 14:
                    tex.setOpacity(rand.NextDouble());
                    width = 1;
                    x = rand.Next(gl_Width);
                    y = rand.Next(gl_Height + max1) - max1;

                    if (rand.Next(9) == 0)
                        width++;
                    height = rand.Next(max1);
                    break;

                case 15:
                case 16:
                case 17:
                    if ((dx > 0 && x > gl_Width) || (dx < 0 && x < -width))
                        generateNew();
                    x += dx;
                    break;

                case 18:
                    if (x < -100 || x > gl_Width + 100 || y < -100 || y > gl_Height + 100)
                        generateNew();

                    if (--cnt <= 0)
                    {
                        cnt = rand.Next(max2) + 1;

                        if (param[1] == 3)
                        {
                            dx = myUtils.randomSign(rand) * (float)rand.NextDouble() * (rand.Next(5) + 1);
                            dy = myUtils.randomSign(rand) * (float)rand.NextDouble() * (rand.Next(5) + 1);
                        }
                        else
                        {
                            float tmp = dx; dx = dy; dy = tmp;  // swap dx-dy

                            dx *= rand.Next(2) == 0 ? -1 : 1;
                            dy *= rand.Next(2) == 0 ? -1 : 1;
                        }
                    }

                    x += dx;
                    y += dy;
                    break;

                case 19:
                    if (y < -height)
                        generateNew();

                    x += dx;

                    if ((dx > 0 && x > gl_Width - width) || (dx < 0 && x < width))
                    {
                        dx *= -1;
                        y -= height + 1;

                        a *= (dx > 0) ? 2.0f : 0.5f;
                    }
                    break;

                case 20:
                    x += dx;

                    if ((dx > 0 && x > gl_Width) || (dx < 0 && x < -width))
                    {
                        dx *= -1;
                    }
                    break;

                case 21:
                case 22:
                    a += da;
                    cnt++;
                    break;

                case 23:
                    if (++cnt >= X)
                        generateNew();
                    break;

                case 24:
                    if ((dx > 0 && x > gl_Width) || (dx < 0 && x < 0) || (dy > 0 && y > gl_Height) || (dy < 0 && y < 0))
                        generateNew();

                    cnt++;
                    x += dx;
                    y += dy;

                    x += (float)Math.Cos(y) * param[3];
                    y += (float)Math.Sin(x) * param[4];
                    break;

                case 25:
                    if (x > gl_Width || x < 0 || y > gl_Height || y < 0)
                        generateNew();

                    cnt++;
                    x += dx;
                    y += dy;

                    if (cnt > X)
                    {
                        cnt = 0;
                        X += (float)rand.NextDouble();

                        if (dx == 0)
                        {
                            dx = dy > 0 ? -dy : -dy;
                            dy = 0;
                        }
                        else
                        {
                            dy = dx > 0 ? dx : dx;
                            dx = 0;
                        }
                    }
                    break;

                case 26:

                    switch (param[0])
                    {
                        case 0:
                            x += dx;
                            break;

                        case 1:
                            x += dx;
                            y += dy * 0.01f;
                            break;

                        case 2:
                            y += dy;
                            break;

                        case 3:
                            x += dx * 0.01f;
                            y += dy;
                            break;

                        default:
                            x += dx;
                            y += dy;
                            break;
                    }

                    if (Y > 0)
                    {
                        dy += y < gl_Height/2 ? Y : -Y;
                    }
                    else
                    {
                        dy += y > gl_Height/2 ? Y : -Y;
                    }

                    if (X > 0)
                    {
                        dx += x < gl_Width/2 ? X : -X;
                    }
                    else
                    {
                        dx += x > gl_Width/2 ? X : -X;
                    }

                    // Damping the oscillation
                    X *= 0.9975f;
                    Y *= 0.9975f;

                    if (x < -123 || x > gl_Width + 123 || y < -123 || y > gl_Height + 123)
                    {
                        generateNew();
                        return;
                    }
                    break;

                case 27:
                    if (--cnt < 0)
                        a -= 0.01f;

                    X += dx;
                    break;

                case 28:
                    if (--cnt < 0)
                        a -= 0.01f;

                    X += dx;
                    break;

                case 29:
                case 30:
                case 31:
                    if (--cnt <= 0)
                        generateNew();
                    break;
            }

            if (a <= 0)
            {
                generateNew();
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Show()
        {
            tex.setOpacity(a);

            switch (mode)
            {
                case 0:
                case 1:
                    tex.Draw((int)x, (int)y, width, height, (int)X, (int)Y, width, height);
                    break;

                case 2:
                case 3:
                case 4:
                    if (doSampleOnce)
                    {
                        tex.Draw((int)x - width, (int)y - width, 2 * width, 2 * width, (int)X - width, (int)X - width, 2 * width, 2 * width);
                    }
                    else
                    {
                        tex.Draw((int)x - width, (int)y - width, 2 * width, 2 * width, (int)x - width, (int)y - width, 2 * width, 2 * width);
                    }
                    break;

                case 5:
                case 6:
                case 7:
                    tex.Draw((int)x - width, (int)y - width, 2 * width, 2 * width, (int)x - width, (int)y - width, 2 * width, 2 * width);
                    break;

                case 8:
                    tex.Draw((int)x - width, (int)y - height, 2 * width, 2 * height, (int)x - width, (int)y - height, 2 * width, 2 * height);
                    break;

                case 9:
                case 10:
                    tex.Draw((int)X - width, (int)Y - height, 2 * width, 2 * height, (int)x - width, (int)y - height, 2 * width, 2 * height);
                    break;

                case 11:
                case 12:
                case 13:
                case 14:
                    tex.Draw((int)x, (int)y, width, height, (int)x, (int)y, width, height);
                    break;

                case 15:
                case 16:
                case 17:
                case 18:
                case 19:
                case 20:
                    tex.setOpacity(a);
                    tex.Draw((int)x, (int)y, width, height, (int)x, (int)y, width, height);
                    break;

                case 21:
                case 22:
                    if (cnt <= param[1])
                    {
                        // Add some rotation
                        switch (param[5])
                        {
                            case 0:
                                tex.SetAngle(Math.PI/2);
                                break;

                            case 1:
                                tex.SetAngle(Math.PI);
                                break;

                            case 2:
                                tex.SetAngle(3 * Math.PI/2);
                                break;

                            case 3:
                                tex.SetAngle(rand.Next(4) * Math.PI/2);
                                break;
                        }

                        tex.setOpacity(a);
                        tex.Draw((int)x, (int)y, width, height, (int)x, (int)y, width, height);
                    }
                    break;

                case 23:
                    if (cnt == 0)
                    {
                        //tex.setOpacity(a);

                        if (true)
                        {
                            float op = 0.0f;
                            float dOp = 2.0f * a / width;

                            for (int i = 0; i < width; i++)
                            {
                                tex.setOpacity(op);

                                op += dOp;

                                if (op >= a)
                                    dOp *= -1;

                                tex.Draw((int)x - width / 2 + i * 2, (int)y, 1, height, (int)x, (int)y, 1, height);
                            }
                        }

                        if (false)
                        {
                            if (rand.Next(2) == 0)
                                for (int i = 0; i < width; i++)
                                    tex.Draw((int)x - width / 2 + i * 2, (int)y, 1, height, (int)x, (int)y, 1, height);
                            else
                                for (int i = 0; i < height; i++)
                                    tex.Draw((int)x, (int)y - height / 2 + i * 2, width, 1, (int)x, (int)y, width, 1);
                        }
                    }
                    break;

                case 24:
                    {
                        int rx = doUseRandDxy ? rand.Next(11) - 5 : 0;
                        int ry = doUseRandDxy ? rand.Next(11) - 5 : 0;

                        // Draw the whole line with half opacity
                        tex.setOpacity(a / 2);
                        tex.Draw((int)x, (int)y, width, height, (int)x + rx, (int)y + ry, width, height);

                        int part = rand.Next(11) + 3;

                        // Draw the part of the line with half opacity
                        if (width > height)
                        {
                            int w1 = width / part;
                            int w2 = (part-2) * width / part;

                            tex.Draw((int)x + w1, (int)y, w2, height, (int)x + rx + w1, (int)y + ry, w2, height);
                        }
                        else
                        {
                            int h1 = height / part;
                            int h2 = (part-2) * height / part;

                            tex.Draw((int)x, (int)y + h1, width, h2, (int)x + rx, (int)y + ry + h1, width, h2);
                        }
                    }
                    break;

                case 25:
                    tex.setOpacity(a);
                    tex.Draw((int)x, (int)y, width, height, (int)x, (int)y, width, height);
                    break;

                case 26:
                    tex.setOpacity(a);
                    tex.Draw((int)x, (int)y, width, height, (int)x, (int)y, width, height);
                    break;

                case 27:
                    tex.setOpacity(a);
                    tex.SetAngle(X);
                    tex.Draw((int)x, (int)y, width, height, (int)x, (int)y, width, height);
                    break;

                case 28:
                    tex.setOpacity(a);
                    tex.SetAngle(Math.Sin(dx * cnt / 250) / 17);
                    tex.Draw((int)x, (int)y, width, height, (int)x, (int)y, width, height);
                    break;

                case 29:
                case 30:
                    switch (param[0])
                    {
                        case 0:
                            tex.setOpacity(a);
                            tex.SetAngle(Math.PI);
                            tex.Draw((int)x, (int)y, width, height, (int)x, (int)y, width, height);
                            break;

                        case 1:
                            tex.setOpacity(a/2);

                            tex.SetAngle(0);
                            tex.Draw((int)x, (int)y, width, height, (int)x, (int)y, width, height);

                            tex.SetAngle(Math.PI);
                            tex.Draw((int)x, (int)y, width, height, (int)x, (int)y, width, height);
                            break;

                        case 2:
                            tex.setOpacity(a/4);

                            tex.SetAngle(0);
                            tex.Draw((int)x, (int)y, width, height, (int)x, (int)y, width, height);

                            tex.SetAngle(Math.PI);
                            tex.Draw((int)x, (int)y, width, height, (int)x, (int)y, width, height);

                            tex.SetAngle(Math.PI / 2);
                            tex.Draw((int)x, (int)y, width, height, (int)x, (int)y, width, height);

                            tex.SetAngle(3 * Math.PI / 2);
                            tex.Draw((int)x, (int)y, width, height, (int)x, (int)y, width, height);
                            break;

                        // Mirror the cell left-to-right / right-to-left
                        case 3:
                            tex.setOpacity(a);
                            tex.Draw((int)x, (int)y, width, height, (int)x, (int)y, -width, height);
                            break;

                        // Not to be used. Just for testing purposes
                        default:
                            tex.setOpacity(a);
                            tex.Draw((int)x, (int)y, width, height, (int)x, (int)y, 2*width, 2*height);
                            break;
                    }
                    break;

                case 31:
                    tex.setOpacity(a);
                    showCase31(param[0]);
                    break;
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Process(Window window)
        {
            int maxCnt = 333;
            int cnt = rand.Next(maxCnt) + 11;
            int mode = rand.Next(2);

            initShapes();

            if (doClearBuffer)
            {
                glDrawBuffer(GL_FRONT_AND_BACK | GL_DEPTH_BUFFER_BIT);
                glClearColor(0, 0, 0, 1);
            }
            else
            {
                glDrawBuffer(GL_FRONT_AND_BACK);
            }

            if (doDrawSrcImg)
            {
                tex.Draw(0, 0, gl_Width, gl_Height);
            }

            if (doCreateAtOnce)
            {
                for (int i = 0; i < N; i++)
                {
                    list.Add(new myObj_330());
                }
            }

            while (!Glfw.WindowShouldClose(window))
            {
                processInput(window);

                Glfw.SwapBuffers(window);
                Glfw.PollEvents();

                if (false)
                {
                    glClear(GL_COLOR_BUFFER_BIT);

                    myPrimitive._Rectangle.SetColor(0.5f, 0.1f, 0.1f, 1.0f);
                    myPrimitive._Rectangle.Draw(100, 100, 666, 666, true);

                    continue;
                }

                if (doClearBuffer)
                {
                    glClear(GL_COLOR_BUFFER_BIT);
                }
                else
                {
                    dimScreen(dimAlpha, false);

                    if (false)
                    {
                        if (cnt > 0)
                            cnt--;
                        else
                        {
                            cnt = rand.Next(maxCnt) + 11;
                            glClear(GL_COLOR_BUFFER_BIT);
                        }
                    }
                }

                // Render Frame
                {
                    for (int i = 0; i < list.Count; i++)
                    {
                        var obj = list[i] as myObj_330;

                        obj.Move();
                        obj.Show();
                    }
                }

                if (list.Count < N)
                {
                    list.Add(new myObj_330());
                }

                System.Threading.Thread.Sleep(renderDelay);
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void initShapes()
        {
            myPrimitive.init_Rectangle();
            tex = new myTexRectangle(colorPicker.getImg());
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

        private void showCase31(int mode)
        {
            int x1 = (int)x, y1 = (int)y, x2 = x1, y2 = y1;
            int w1 = width, h1 = height, w2 = w1, h2 = h1;

            bool isFirst = (X == -1111 && Y == -1111);
            int offset = 0;

            mode = 4;

            switch (mode)
            {
                // Each cell is scrolled sideways
                case 0:
                    if (isFirst)
                    {
                        X = (rand.Next(5) + 1) * myUtils.randomSign(rand);
                        Y = rand.Next(67) - 33;
                    }

                    x2 += (int)Y + cnt / (int)X;
                    break;

                // Each cell is displayed with x-offset
                case 1:
                    if (isFirst)
                        Y = rand.Next(67) - 33;

                    x2 += (int)Y;
                    break;

                // Each cell is zoomed out
                case 2:
                    if (isFirst)
                        Y = 0;

                    x2 -= 1*(int)Y;
                    y2 -= 1*(int)Y;
                    w2 += 2*(int)Y;
                    h2 += 2*(int)Y;

                    Y += (float)rand.NextDouble();
                    break;

                // Each cell is zoomed in
                case 3:
                    if (isFirst)
                        Y = 0;

                    x2 -= 1*(int)Y;
                    y2 -= 1*(int)Y;
                    w2 += 2*(int)Y;
                    h2 += 2*(int)Y;

                    Y -= (Y > (-width / 2 + 1)) ? (float)rand.NextDouble() : 0;
                    break;

                // Each cell is zoomed in with overflow into zooming out
                case 4:
                    if (isFirst)
                        Y = 0;

                    x2 -= 1*(int)Y;
                    y2 -= 1*(int)Y;
                    w2 += 2*(int)Y;
                    h2 += 2*(int)Y;

                    Y -= (float)rand.NextDouble();
                    break;

                case 5:
                    break;




                case 11:
                    offset = -width/2;
                    x2 -= offset;
                    y2 -= offset;
                    w2 += offset;
                    h2 += offset;
                    break;

                case 21:
                    x2 -= offset;
                    y2 -= offset;
                    w2 += offset;
                    h2 += offset;

                    X = Y;

                    if (Y == 0)
                    {
                        Y = myUtils.random101(rand) * 0.25f;
                    }
                    else
                    {
                        int sign = (Y > 0)
                            ? myUtils.randomChance(rand, 1, 3) ? -1 : +1
                            : myUtils.randomChance(rand, 1, 3) ? +1 : -1;

                        Y += sign * 0.25f;
                    }
                    break;

                case 31:
                    x2 -= offset;
                    y2 -= offset;
                    w2 += offset;
                    h2 += offset;

                    X += (float)Math.Sin(cnt/10) * 1.0001f;
                    break;

                case 41:
                    w2 = w1 * 2;
                    h2 = h1 * 2;
                    break;
            }

            tex.Draw(x1, y1, w1, h1, x2, y2, w2, h2);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------
    }
};
