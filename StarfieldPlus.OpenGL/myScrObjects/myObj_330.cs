using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;


/*
    -- Textures, Take 1
    https://www.youtube.com/watch?v=sA4p3wuDLo8&ab_channel=FranklyGaming

    // todo:
    // mode. like 24, but the waves are wider and are going maybe in radial direction. the objects are generated with sin or cos or smth
    // mode. like 42, but rand length rectangles instead of squares
    // Vertical lines move out of the screen's edge, but at some point start moving in rectangle, along the screen's edge
    // grid based. 2 neighbour cells are compared by avg color and are swapped. the larger goes left, the lesser goes right
*/


namespace my
{
    public class myObj_330 : myObject
    {
        private myObjectParams p = null;

        public float x, y, X, Y, dx, dy, a, da, r, g, b;
        public int width, height, cnt;

        static int N = 1, max = 1, opacityFactor = 1;
        static int[] param = new int[7];
        static int mode = 0, oldRenderDelay = -1;

        static bool doClearBuffer = false, doCreateAtOnce = true, doSampleOnce = false, doUseRandDxy = false, doDrawSrcImg = false;
        static float dimAlpha = 0.05f, t = 0, dt = 0;
        static float[] paramf = new float[2];

        static myTexRectangle tex = null;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_330()
        {
            // This value means, this is the first iteration ever for this object
            cnt = -12345;

            generateNew();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time global initialization
        protected override void initGlobal()
        {
            colorPicker = new myColorPicker(gl_Width, gl_Height, mode: myColorPicker.colorMode.SNAPSHOT_OR_IMAGE);
            list = new List<myObject>();

            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time local initialization
        private void initLocal()
        {
            mode = rand.Next(59);
            mode = 58;

            // Reset parameter values
            {
                for (int i = 0; i < param.Length; i++)
                    param[i] = 0;

                for (int i = 0; i < paramf.Length; i++)
                    paramf[i] = 0.0f;
            }

            opacityFactor = rand.Next(3) + 1 + (myUtils.randomChance(rand, 1, 7) ? rand.Next(3) : 0);

            doCreateAtOnce = myUtils.randomBool(rand);
            doClearBuffer  = myUtils.randomBool(rand);
            doUseRandDxy   = myUtils.randomBool(rand);
            doSampleOnce   = false;
            doDrawSrcImg   = false;
            t = 0;

            switch (mode)
            {
                // Random rectangles appear at random locations each iteration
                // The narrower the rectangle, the higher is its opacity
                case 00:
                    doClearBuffer = false;
                    dimAlpha /= 10;
                    N = 10;
                    break;

                // Random very narrow rectangles (1 or 2 px) appear at random locations each iteration
                case 01:
                    doClearBuffer = false;
                    dimAlpha /= 10;
                    max = 333 + rand.Next(666);
                    N = 10;
                    break;

                // Random squares moving through the screen using various movement patterns
                case 02:
                case 03:
                case 04:
                    doSampleOnce = doClearBuffer ? myUtils.randomBool(rand) : false;
                    dimAlpha /= 3;
                    N = 999 + rand.Next(666);
                    param[0] = rand.Next(2);                                                // Squares (0) vs Lines (1)
                    param[1] = rand.Next(2);                                                // Resample texture periodically (if doSampleOnce)
                    break;

                // 5; Pieces appear on the central line and then move up or down
                // 6; Pieces appear at the top part of the screen and then move down
                case 05:
                case 06:
                    dimAlpha /= 3;
                    max = rand.Next(45) + 3;
                    N = 999 + rand.Next(666);
                    param[0] = myUtils.randomChance(rand, 1, 2)                             // 5: Central line vs random line
                        ? gl_y0 : rand.Next(gl_Height);
                    break;

                // Random pieces of the image constantly appearing at their own locations
                // Each piece's opacity grows, then fades away
                case 07:
                    dimAlpha /= 3;
                    doClearBuffer = true;
                    N = 111 + rand.Next(666);
                    max = N > 450 ? 99 : 125;
                    break;

                // Random pieces of the image constantly appearing at their own locations
                // The screen IS cleared between frames
                case 08:
                    param[0] = rand.Next(2) == 0 ? 0 : rand.Next(50);
                    doClearBuffer = true;
                    N = 999 + rand.Next(111);
                    max = 50;
                    break;

                // Random pieces of the image constantly appearing at their own locations (with or without some offset, i.e. blurring)
                // The screen is NOT cleared between frames
                case 09:
                    param[0] = rand.Next(5);                                                // Random Rect vs Random Square vs Const Square vs Const Rect vs Very Wide Rect
                    param[1] = rand.Next(3) == 0 ? (rand.Next(23)+1) : (rand.Next(5)+1);    // Blurring effect strength
                    param[2] = rand.Next(111);                                              // Speed factor
                    param[3] = rand.Next(4);                                                // Opacity mode
                    dimAlpha /= (rand.Next(3) == 0 ? (rand.Next(11) + 1) : (5));
                    doClearBuffer = false;
                    doUseRandDxy = myUtils.randomChance(rand, 2, 3);
                    N = 999 + rand.Next(111);
                    max = rand.Next(50) + 25;
                    dt = 0.023f;
                    break;

                // Random pieces of the image constantly appearing at random locations
                case 10:
                    param[0] = rand.Next(4);                                                // Random Rectangle vs Random Square vs Const Square vs Const Rectangle
                    param[1] = rand.Next(111);                                              // Speed factor
                    param[2] = rand.Next(2) == 0 ? 0 : 100 + rand.Next(500);                // Vingette factor
                    param[3] = rand.Next(4);                                                // Opacity mode
                    dimAlpha /= (rand.Next(3) == 0) ? (rand.Next(11) + 1) : (5);
                    doClearBuffer = false;
                    N = (999 + rand.Next(111)) / (rand.Next(11) + 1);
                    max = rand.Next(50) + 25;
                    dt = 0.01f;
                    break;

                // Squares moving on the screen, while decreasing in size
                case 11:
                    doClearBuffer = false;
                    N = (999 + rand.Next(111)) / (rand.Next(11) + 1);
                    max = rand.Next(150) + 25;
                    break;

                // Long thin lines randomly flickering on the screen (both horizontal and vertical)
                case 12:
                    N = 2000 + rand.Next(3333);
                    max = rand.Next(333) + 125;
                    break;

                // Long thin lines randomly flickering on the screen (horizontal, vertical)
                case 13:
                case 14:
                    N = 2000 + rand.Next(1111);
                    max = rand.Next(333) + 125;
                    break;

                // Long parallel lines slowly moving into the view from the outside of the screen
                case 15:
                case 16:
                case 17:
                    N = 1000 + rand.Next(333);
                    max = rand.Next(666) + 125;
                    param[0] = rand.Next(7);
                    param[1] = rand.Next(33) + 12;
                    break;

                // Squares moving around, changing direction of movement occasionally
                // The moving pattern is based on 90-degrees turns
                case 18:
                    N = rand.Next(1111) + 100;
                    param[0] = rand.Next(3);
                    param[1] = rand.Next(4);
                    param[2] = rand.Next(333) + 25;
                    max = rand.Next(33) + 25;
                    break;

                // Snake-alike chain of squares running from side to side, bottom to top
                case 19:
                    N = rand.Next(1111) + 111;
                    max = rand.Next(111) + 25;
                    param[0] = rand.Next(3);                                                // Const Square size vs Random Square size vs Rect
                    param[1] = myUtils.randomChance(rand, 1, 2) ? 0 : rand.Next(9) + 1;     // Grid-aligned, if not 0
                    param[2] = rand.Next(3);                                                // dx generation mode
                    break;

                // Squares moving sideways, bouncing off the walls
                case 20:
                    N = rand.Next(3333) + 333;
                    max = rand.Next(50) + 1;
                    param[0] = rand.Next(3);                                                // Size/opacity option
                    param[1] = rand.Next(2);                                                // 
                    param[2] = rand.Next(7);                                                // Movement pattern
                    break;

                // Random pieces of the image constantly appear at their own locations
                // Each piece then fades away
                // 21; Grid alignment: NO
                // 22; Grid alignment: YES
                case 21:
                case 22:
                    N = rand.Next(500) + 111;
                    max = rand.Next(200) + 13;                                              // Max size of a cell
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
                    max = rand.Next(200) + 13;                                              // Max size
                    doClearBuffer = false;
                    dimAlpha /= (rand.Next(5) + 1);
                    break;

                // Drawing horizontal rows or vertical columns using 1px thick lines;
                // The lines are drawn with some interval between them
                case 24:
                    N = rand.Next(500) + 33;
                    max = myUtils.randomChance(rand, 1, 10) ? (rand.Next(3333) + 111) : (rand.Next(222) + 111);
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
                    max = rand.Next(66) + 1;
                    doClearBuffer = myUtils.randomChance(rand, 1, 2);
                    dimAlpha /= (rand.Next(5) + 1);
                    break;

                // 'Gravitational pull' towards the center axes of the screen
                case 26:
                    N = rand.Next(333) + 666;
                    max = rand.Next(50) + 3;
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
                    max = rand.Next(200) + 13;                                              // Max size of a cell
                    param[0] = rand.Next(4);                                                // Drawing mode
                    param[2] = rand.Next(10) + 1;                                           // Distance between the grid cells
                    break;

                // Grid-based squares, with different zoom (in or out) factors
                case 31:
                    N = rand.Next(666) + 333;
                    doClearBuffer = myUtils.randomChance(rand, 1, 5);
                    dimAlpha /= (rand.Next(11) + 1);
                    max = rand.Next(200) + 13;                                              // Max size of a cell
                    param[0] = rand.Next(13);                                               // Drawing mode
                    param[1] = rand.Next(7) + 1;                                            // Various int [1...7]
                    param[2] = rand.Next(10) + 1;                                           // Distance between the grid cells
                    paramf[0] = (float)rand.NextDouble();
                    break;

                // Square-angleed snake-style movements
                case 32:
                    N = rand.Next(111) + 11;
                    doClearBuffer = myUtils.randomChance(rand, 1, 5);
                    dimAlpha /= (rand.Next(11) + 1);
                    max = 50;                                                               // Max length of a line
                    param[0] = rand.Next(25) + 1;                                           // Width of a line
                    param[1] = rand.Next(5);                                                // Randomly vary max length of a line
                    param[2] = rand.Next(5);                                                // Randomly vary width of a line
                    param[3] = myUtils.randomChance(rand, 1, 7)                             // Progress delay
                        ? rand.Next(7) + 1 : 0;

                    N += param[0] < 5 ? rand.Next(222) : 0;                                 // Possibly increase the number of N for lesser line widths
                    N += param[0] < 3 ? rand.Next(333) : 0;
                    N += doClearBuffer ? rand.Next(1234) + 666 : 0;                         // Possibly increase the number of N if the buffer is cleared each iteration

                    max += myUtils.randomChance(rand, 1, 5)                                 // Possibly change the max length of a line
                        ? myUtils.randomSign(rand) * rand.Next(33) : 0;
                    break;

                // Matrix-style falling pieces
                case 33:
                    N = rand.Next(666) + 333;
                    doClearBuffer = myUtils.randomChance(rand, 1, 5);
                    max = rand.Next(33) + 11;                                               // Max particle size
                    param[0] = rand.Next(4);                                                // Const size / Random size / Lines / Wide lines
                    param[1] = rand.Next(2);                                                // Rotation mode
                    param[2] = rand.Next(2);                                                // y-axis original position (in-screen, out-of-screen)
                    param[3] = rand.Next(11);                                               // y-axis acceleration / grid interval
                    param[4] = rand.Next(2) == 0 ? 0 : rand.Next(11);                       // Decrease opacity mode
                    param[5] = rand.Next(2);                                                // Random size for each frame
                    param[6] = rand.Next(3);                                                // Align to grid (if 0)
                    break;

                // Horizontal lines that are shifted sideways
                case 34:
                    doCreateAtOnce = true;
                    N = rand.Next(3) < 2 ? gl_Height : gl_Height / (rand.Next(10) + 1);
                    param[0] = rand.Next(9);                                                // Move mode
                    param[1] = rand.Next(2);                                                // Draw mode
                    param[2] = rand.Next(2);                                                // Opacity mode
                    break;

                // Random squares rotated by +/-45 degrees
                case 35:
                    N = rand.Next(666) + 333;
                    max = rand.Next(33) + 11;                                               // Max particle size
                    param[0] = rand.Next(2);                                                // Size option
                    break;

                // Random squares of pulsating size
                case 36:
                    N = rand.Next(666) + 666;
                    max = rand.Next(33) + 11;                                               // Max particle size
                    param[0] = rand.Next(2);                                                // Size option
                    break;

                // Rectangles of pulsating height
                case 37:
                    N = rand.Next(666) + 666;
                    max = rand.Next(25) + 3;                                                // Max particle size
                    param[0] = rand.Next(3);                                                // Size option (rand, const, 1)
                    param[1] = rand.Next(4);                                                // Center line option
                    param[2] = rand.Next(500) + 50;                                         // 
                    param[3] = rand.Next( 20) + 1;                                          // 
                    param[4] = rand.Next(333) + 50;                                         // 
                    dt = 0.01f;
                    break;

                // Snake-like 'roots' growing into the screen from the sides
                case 38:
                    N = rand.Next(666) + 666;
                    max = rand.Next(23) + 3;                                                // Max particle size
                    param[0] = rand.Next(3);                                                // Move mode
                    param[1] = rand.Next(11) + 1;                                           // Speed factor
                    param[2] = rand.Next(11) + 3;                                           // Probability of a side turn in move mode 2

                    if (param[0] == 2)
                    {
                        max = rand.Next(5) + 1;
                        param[1] = 33;
                    }

                    doClearBuffer = myUtils.randomChance(rand, 1, 11);
                    dimAlpha /= (rand.Next(5) + 1);
                    break;

                // High-opacity smaller particles and low-opacity larger particles flow in opposite directions
                case 39:
                    N = rand.Next(666) + 666;
                    max = rand.Next(333) + 33;                                              // Max particle size
                    dimAlpha /= (rand.Next(3) + 1);
                    param[0] = rand.Next(2);                                                // Offset for y-axis (to get some bluring effect)
                    param[1] = rand.Next(2);                                                // Squares vs Rectangles
                    break;

                // Random squares appearing at their own locations, but with a probability of getting a random offset (the larger the offset, the lesser the probability)
                case 40:
                    doClearBuffer = false;
                    N = rand.Next(666) + 666;
                    max = rand.Next(111) + 111;                                             // Max particle size
                    dimAlpha /= 5;
                    break;

                // Grid made of thin lines that are continuously moving up/down or left/right
                case 41:
                    doClearBuffer = myUtils.randomChance(rand, 4, 5);
                    doCreateAtOnce = myUtils.randomChance(rand, 4, 5);
                    max = rand.Next(111) + 7;
                    param[0] = rand.Next(3);                                                // Mode: vertical vs horizontal vs vertical + horizontal
                    param[1] = rand.Next(max / 3) + 5;                                      // line width
                    param[2] = rand.Next(4);                                                // speed: const/const vs vs const/var vs var/const vs var/var

                    switch (param[0])
                    {
                        case 0:
                            N = (gl_Width / max) + 1;
                            break;

                        case 1:
                            N = (gl_Height / max) + 1;
                            break;

                        case 2:
                            N  = (gl_Width  / max) + 1;
                            N += (gl_Height / max) + 1;
                            break;
                    }

                    paramf[0] = (float)(rand.NextDouble() + rand.Next(7));                  // dx for every particle
                    paramf[1] = (float)(rand.NextDouble() + rand.Next(7));                  // dy for every particle
                    break;

                // Horizontally and/or vertically moving squares, aligned to grid
                case 42:
                    switch (rand.Next(4))
                    {
                        case 0: N = rand.Next(111) + 111; break;
                        case 1: N = rand.Next(333) + 111; break;
                        case 2: N = rand.Next(666) + 333; break;
                        case 3: N = rand.Next(999) + 666; break;
                    }

                    max = rand.Next(75) + 7;                                                // Grid cell's size
                    param[0] = rand.Next(7) + 1;                                            // Interval between the grid cells
                    param[1] = rand.Next(6);                                                // Opacity factor (0 means const opacity)
                    param[2] = rand.Next(11);                                               // Movement mode: left, right, left+right, up, down, up+down, left+right+up+down
                    param[3] = rand.Next(9);                                                // Grid align: FALSE (0), TRUE (1-8)
                    param[4] = rand.Next(7);                                                // Acceleration (in case of 0 or 1)
                    param[5] = myUtils.randomChance(rand, 2, 3) ? 0 : rand.Next(5) + 1;     // Slight offset along non-movable axis

                    doClearBuffer = (N < 333) ? false : myUtils.randomChance(rand, 1, 5);
                    break;

                // Drawing groups of grid cells, sometimes color them using the bgr color from that location
                case 43:
                    N = rand.Next(66) + 66;
                    max = rand.Next(66) + 33;                                               // Grid cell's size
                    param[0] = rand.Next(7) + 1;                                            // Interval between the grid cells
                    param[1] = rand.Next(15) + 1;                                           // Draw the particles when (cnt < param[1])
                    param[2] = rand.Next(2);                                                // Const opacity vs random opacity
                    param[3] = rand.Next(5) + 1;                                            // Disappearing speed
                    param[4] = rand.Next(6);                                                // Single-line, in case of 0-1-2
                    param[5] = myUtils.randomChance(rand, 1, 2)                             // Zoom parameter
                        ? (int)(max * rand.NextDouble() * (rand.Next(2) + 1))
                        : max;
                    dimAlpha /= param[3];

                    doClearBuffer = false;
                    break;

                // Random cells move horizontally in 2 opposite directions
                case 44:
                    N = rand.Next(66) + 33;
                    max = rand.Next(66) + 33;                                               // Grid cell's size

                    param[0] = rand.Next(111) + 13;                                         // Max speed factor
                    param[1] = rand.Next(3);                                                // Use dWidth / dHeight
                    param[2] = rand.Next(2);                                                // Random size / const size
                    param[3] = myUtils.randomChance(rand, 1, 2) ? 0 : rand.Next(11);        // Grid interval
                    dt = 0.01f;
                    break;

                // Thin lines bounce off the sides of the screen
                case 45:
                    N = 3333;
                    max = 111;                                                              // Max length
                    param[0] = 13;                                                          // Speed factor

                    dimAlpha /= (rand.Next(5) + 1);

                    doDrawSrcImg  = myUtils.randomChance(rand, 1, 2);
                    doClearBuffer = myUtils.randomChance(rand, 1, 2);
                    doSampleOnce  = myUtils.randomChance(rand, 1, 2);
                    break;

                // Grid based cells are drawn with an offset into the texture coordinates
                case 46:
                    doClearBuffer = false;
                    doDrawSrcImg = myUtils.randomChance(rand, 1, 3);
                    N = 333 + rand.Next(666);
                    max = rand.Next(111) + 23;                                              // Cells size
                    param[0] = rand.Next(11) + 1;                                           // Grid interval
                    param[1] = rand.Next(13) + 3;                                           // Max move interval
                    param[2] = rand.Next(11) + 2;                                           // Move factor

                    dimAlpha /= (rand.Next(11) + 1);

                    // Recalc N, if too small
                    {
                        int n = (gl_Width / max) * (gl_Height / max);

                        N *= (n / N > 3) ? 2 : 1;
                    }
                    break;

                // Slowly showing grig-based image; the cells might have a border around them
                case 47:
                    doClearBuffer = false;
                    N = 333 + rand.Next(666);
                    max = rand.Next(111) + 23;                                              // Cells size
                    param[0] = rand.Next(11) + 1;                                           // Grid interval
                    param[1] = rand.Next(3);                                                // Cell border option (0 - No border, 1 - Border, 2 - Randomly offset border)
                    param[2] = rand.Next(3);                                                // Cell border color option
                    param[3] = rand.Next(2);                                                // Fill cell's background with its border color
                    param[4] = rand.Next(3);                                                // Align to grid ( > 0) or not ( == 0)
                    param[5] = rand.Next(2);                                                // Cells bluring option
                    param[6] = rand.Next(2);                                                // Draw grid line intersections
                    dimAlpha /= (9.0f + myUtils.randFloat(rand));
                    break;

                // Cells oscillating right and left in a grid based structure
                case 48:
                    N = 333 + rand.Next(1111) + rand.Next(2) * rand.Next(3333);
                    max = rand.Next(111) + 23;                                              // Cells size
                    param[0] = rand.Next(11) + 1;                                           // Grid interval
                    param[1] = rand.Next(6);                                                // Move mode
                    param[2] = rand.Next(4);                                                // Align to grid probability
                    dt = 0.01f;
                    break;

                // Fast-living small static squares coming in large numbers
                case 49:
                    N = 666 + rand.Next(1234) + rand.Next(2) * rand.Next(3333);
                    max = rand.Next(11) + 2;                                                // Cells size
                    param[0] = rand.Next(11) + 1;                                           // Grid interval
                    param[1] = rand.Next(11) + 1;                                           // Life speed factor
                    param[2] = rand.Next(2);                                                // Align to grid
                    dimAlpha /= (1.0f + myUtils.randFloat(rand) * rand.Next(5));
                    break;

                // Primitive edge searching algorithm
                case 50:
                    doClearBuffer = false;
                    N = 333 + rand.Next(666);
                    param[0] = rand.Next(4);                                                // Drawing mode
                    param[1] = rand.Next(3);                                                // Search mode
                    dimAlpha /= 33;
                    break;

                // Vertical or horizontal lines partially rotating at the same angle
                case 51:
                    doCreateAtOnce = true;
                    N = 3333;
                    max = rand.Next(666) + 66;                                              // Max line length
                    param[0] = 1;                                                           // Line generation mode
                    dt = 0.01f;
                    break;

                // At random points, draw the same particle several times with various random offset
                case 52:
                    N = 1111 + rand.Next(777);
                    max = rand.Next(22) + 11;                                               // Max size
                    param[0] = 0;
                    param[1] = 5;                                                           // Number of particles per step
                    param[2] = 66;                                                          // Particle scattering

                    dimAlpha /= rand.Next(3) + 5;
                    dt = 0.0025f;
                    break;

                // Moving grid-based spotlights
                case 53:
                    N = 2 + rand.Next(4);
                    max = rand.Next(29) + 15;                                               // Grid cell size
                    param[0] = rand.Next(11) + 1;                                           // Grid interval
                    param[1] = N;                                                           // Number of big cells
                    paramf[0] = myUtils.randFloat(rand) / 10;                               // Min cell opacity to display
                    N += 1111;
                    dt = 0.1f;
                    break;

                // Randomly moving small particles; move model taken from #53
                case 54:
                    N = rand.Next(3333) + 666;
                    max = myUtils.randomChance(rand, 4, 5)                                  // Max particle size
                        ? rand.Next(7) + 1
                        : rand.Next(50) + 33;
                    param[0] = rand.Next(2);                                                // Use delayed draw coordinates
                    param[1] = rand.Next(2);                                                // Use sudden stops

                    switch (rand.Next(3))
                    {
                        case 1: dimAlpha /= 2; break;
                        case 2: dimAlpha *= 2; break;
                    }
                    break;

                // Random particles make sudden moves vertically or horizontally
                case 55:
                    N = rand.Next(1111) + 666;
                    switch (rand.Next(4))
                    {
                        case 0: max = rand.Next( 7) + 3; break;                             // Max particle size
                        case 1: max = rand.Next( 9) + 3; break;
                        case 2: max = rand.Next(13) + 3; break;
                        case 3: max = rand.Next(50) + 3; break;
                    }
                    param[0] = rand.Next(3);                                                // Move mode
                    param[1] = rand.Next(4);                                                // Stopping mode
                    param[2] = rand.Next(400) + 101;                                        // Probability to start moving
                    param[3] = rand.Next(3);                                                // Sampling method
                    param[4] = (doClearBuffer == true)                                      // Grid interval (if > 0)
                                ? 0
                                : myUtils.randomChance(rand, 1, 2) ? 0 : rand.Next(9) + 2;
                    break;

                // Pairs of grid-aligned points moving along x-axis in opposite directions
                case 56:
                    doClearBuffer = false;
                    N = 111 + rand.Next(13);
                    max = rand.Next(33) + 10;
                    param[0] = rand.Next(9) + 1;                                            // Grid interval
                    param[1] = rand.Next(3);                                                // Align y-axis to grid or not
                    param[2] = rand.Next(2);                                                // Starting position on the x-axis (rand vs middle)
                    dimAlpha /= (1.0f + 0.1f * (rand.Next(13)));
                    dt = 0.01f;
                    break;

                // Layers of bricks
                case 57:
                    doClearBuffer = false;
                    N = 1;
                    max = rand.Next(50) + 35;
                    param[0] = rand.Next(11) + 1;                                           // Grid interval
                    param[1] = rand.Next(5);                                                // Brick mode (const size Square vs rand width Rectangle)
                    param[2] = rand.Next(2);                                                // Const vs random opacity
                    param[3] = rand.Next(2);                                                // Generate new before every new run
                    param[4] = rand.Next(2);                                                // Random offset for every brick
                    dimAlpha /= (1.0f + 0.1f * (rand.Next(60)));

                    if (myUtils.randomChance(rand, 1, 3))
                    {
                        oldRenderDelay = renderDelay;
                        renderDelay = rand.Next(11);
                    }
                    break;

                // Sorting the grid by the average cell color/luminance
                case 58:
                    N = 333;
                    doClearBuffer = false;
                    max = rand.Next(60) + 2;                                                // Cell size
                    param[0] = rand.Next(11) + 1;                                           // Grid interval
                    param[1] = rand.Next(3);                                                // Cell comparison mode
                    param[2] = rand.Next(2);                                                // Mode to determine the second cell to compare/swap with

                    dimAlpha /= 33;
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
                            $"max = {max}\n" +
                            $"doClearBuffer = {doClearBuffer}\n" +
                            $"doSampleOnce  = {doSampleOnce}\n" +
                            $"opacityFactor = {opacityFactor}\n" +
                            $"doUseRandDxy  = {doUseRandDxy}\n" +
                            $"param: [{str_params}]\n\n" +
                            $"file: {colorPicker.GetFileName()}"
            ;
            return str;
        }

        // ---------------------------------------------------------------------------------------------------------------

        // 
        protected override void setNextMode()
        {
            if (oldRenderDelay != -1)
                renderDelay = oldRenderDelay;

            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void generateNew()
        {
            a = (float)rand.NextDouble() / opacityFactor;
            cnt = 0;

            switch (mode)
            {
                case 02:
                    width = rand.Next(33) + 5;
                    height = (param[0] == 0) ? width : 1;
                    x = X = rand.Next(gl_Width);
                    y = Y = rand.Next(gl_Height);
                    dx = (float)rand.NextDouble() * myUtils.randomSign(rand) * 5;
                    dy = (float)rand.NextDouble() * myUtils.randomSign(rand) * 5;
                    break;

                case 03:
                    width = rand.Next(33) + 5;
                    height = (param[0] == 0) ? width : 1;
                    x = X = rand.Next(gl_Width);
                    y = Y = rand.Next(gl_Height);
                    dx = dy = 0;

                    if (rand.Next(2) == 0)
                        dx = (float)rand.NextDouble() * myUtils.randomSign(rand) * 5;
                    else
                        dy = (float)rand.NextDouble() * myUtils.randomSign(rand) * 5;
                    break;

                case 04:
                    width = rand.Next(33) + 5;
                    height = (param[0] == 0) ? width : 1;
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

                case 05:
                    a = doClearBuffer ? a * 1.75f : a;
                    width = rand.Next(max) + 1;
                    x = X = rand.Next(gl_Width);
                    y = Y = param[0];
                    dx = 0;
                    dy = (float)rand.NextDouble() * myUtils.randomSign(rand) * 5;
                    da = (float)rand.NextDouble() / 25;
                    break;

                case 06:
                    width = rand.Next(max) + 1;
                    x = X = rand.Next(gl_Width);
                    y = Y = rand.Next(gl_Height / 5);
                    dx = 0;
                    dy = (float)rand.NextDouble() * 5;
                    da = (float)rand.NextDouble() / 33;
                    break;

                case 07:
                    width = rand.Next(max) + 1;
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

                case 08:
                    a = (float)rand.NextDouble() / 3;
                    X = rand.Next(11) + 1;
                    break;

                case 09:
                case 10:
                    cnt = rand.Next(33) + 1;
                    a = 0.25f;
                    break;

                case 11:
                    width = height = rand.Next(max) + 1;
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
                    width  = (myUtils.randomChance(rand, 1, 33) ? rand.Next(3*max) : rand.Next(max)) + 100;
                    height = rand.Next(param[1]) + 3;

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

                    dx = (rand.Next(5) * (float)rand.NextDouble() + 0.1f) * myUtils.randomSign(rand);
                    dy = 0;

                    if (mode == 15 || mode == 17)
                    {
                        x = (dx > 0)
                            ? (0 - width - rand.Next(width))
                            : (gl_Width + rand.Next(width));

                        y = rand.Next(gl_Height);
                    }

                    if (mode == 16 || (mode == 17 && myUtils.randomChance(rand, 1, 2)))
                    {
                        myUtils.swap<float>(ref dx, ref dy);
                        myUtils.swap<int>(ref width, ref height);

                        y = (dy > 0)
                            ? (0 - height - rand.Next(height))
                            : (gl_Height + rand.Next(height));

                        x = rand.Next(gl_Width);
                    }

                    X = x;
                    Y = y;
                    break;

                case 18:

                    switch (param[0])
                    {
                        case 0:
                            width = height = max;
                            break;

                        case 1:
                            width = height = rand.Next(max * 2) + 15;
                            break;

                        case 2:
                            width = height = rand.Next(max * 3) + 10;
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
                            width = height = max;
                            break;

                        case 1:
                            width = height = rand.Next(max) + 11;
                            break;

                        case 2:
                            height = max;
                            width = height / 2;
                            break;
                    }

                    a = (float)rand.NextDouble();
                    x = X = 0 - rand.Next(5*width) - width;
                    y = Y = gl_Height - height;

                    switch (param[2])
                    {
                        case 0:
                            dx = (float)rand.NextDouble() * (rand.Next(111) + 1) + 0.01f;
                            break;

                        case 1:
                            dx = (rand.Next(33) + 5) * 1.5f;
                            break;

                        case 2:
                            dx = (rand.Next(1111) + 1) * 0.1f;
                            break;
                    }
                    break;

                case 20:
                    a = (float)rand.NextDouble();

                    switch (param[0])
                    {
                        case 0: width = max;                break;
                        case 1: width = rand.Next(max) + 1; break;
                        case 2:
                            width = rand.Next(max) + 1;
                            a = width > 150 ? 10.0f : 5.0f;
                            a /= width;
                            break;
                    }

                    x = X = rand.Next(gl_Width);
                    y = Y = rand.Next(gl_Height);
                    height = width;

                    switch (param[1])
                    {
                        case 1: y = Y = (Y - Y % height); break;
                    }

                    // Moving direction
                    switch (param[2])
                    {
                        case 0:
                            dx = (float)rand.NextDouble() * (rand.Next(33) + 1) + 0.01f;
                            break;

                        case 1:
                            dx = -(float)rand.NextDouble() * (rand.Next(33) + 1) - 0.01f;
                            break;

                        // Both ways
                        default:
                            dx = myUtils.randomSign(rand) * (float)rand.NextDouble() * (rand.Next(33) + 1) + 0.01f;
                            break;
                    }

                    break;

                case 21:
                case 22:
                    switch (param[0])
                    {
                        case 0: width = height = max; break;
                        case 1: width = height = rand.Next(max) + 1; break;
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
                        x -= x % (max + param[2]);
                        y -= y % (max + param[2]);

                        // Center smaller squares in the grid cell
                        if (width < max)
                        {
                            x += (max - width) / 2;
                            y += (max - width) / 2;

                            // Or randomly move the squares within the bounds of the cell
                            if (param[3] != 0)
                            {
                                int n = (max - width) / 2;

                                x += myUtils.randomSign(rand) * rand.Next(n);
                                y += myUtils.randomSign(rand) * rand.Next(n);
                            }
                        }

                        // Offset grid cells, so the pattern is symmetrical on the screen
                        {
                            int w = max + param[2];
                            int n = (gl_Height + param[2]) % w;

                            if (n != 0)
                            {
                                n = (gl_Height) % w;
                                y -= (max - (max + n)/2);
                            }

                            n = (gl_Width + param[2]) % w;

                            if (n != 0)
                            {
                                n = (gl_Width) % w;
                                x -= (max - (max + n)/2);
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
                    height = rand.Next(max) + 10;
                    width = height / 2;
                    width = rand.Next(max) + 10;
                    cnt = 0;
                    X = rand.Next(20) + 1;
                    break;

                case 24:
                    a = (float)rand.NextDouble();
                    width = height = rand.Next(max) + param[1];

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
                    width = height = rand.Next(max) + 10;

                    dx = (float)rand.NextDouble() * (rand.Next(111) + 1) * 0.1f + 0.001f;
                    dy = 0;
                    cnt = 0;
                    X = 2;
                    break;

                case 26:
                    a = (float)rand.NextDouble();
                    x = rand.Next(gl_Width);
                    y = rand.Next(gl_Height);
                    width = height = rand.Next(max) + 3;

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
                    width = height = max;

                    // Align to grid
                    if (mode == 29 || mode == 31)
                    {
                        x -= x % (max + param[2]);
                        y -= y % (max + param[2]);

                        // Offset grid cells, so the pattern is symmetrical on the screen
                        {
                            int w = max + param[2];
                            int n = (gl_Height + param[2]) % w;

                            if (n != 0)
                            {
                                n = (gl_Height) % w;
                                y -= (max - (max + n) / 2);
                            }

                            n = (gl_Width + param[2]) % w;

                            if (n != 0)
                            {
                                n = (gl_Width) % w;
                                x -= (max - (max + n) / 2);
                            }
                        }
                    }

                    da = -1 * (0.01f + (float)rand.NextDouble() / 10);

                    // Different cnt for different modes
                    cnt = mode == 31 ? 333: 111;
                    cnt = rand.Next(cnt) + 11;
                    break;

                case 32:
                    a = (float)rand.NextDouble();
                    x = rand.Next(gl_Width);
                    y = rand.Next(gl_Height);
                    X = Y = 0;
                    cnt = 0;

                    // Vary max parameter
                    if (param[1] == 0)
                    {
                        max += myUtils.random101(rand);

                        if (max < 20)
                            max = 20;

                        if (max > 99)
                            max = 99;
                    }

                    // Vary param[0] parameter (line width)
                    if (param[2] == 0)
                    {
                        param[0] += myUtils.random101(rand);

                        if (param[0] < 1)
                            param[0] = 1;

                        if (param[0] > 33)
                            param[0] = 33;
                    }

                    if (param[3] != 0)
                    {
                        cnt = rand.Next(param[3]) + 1;
                    }
                    break;

                case 33:
                    a = (float)rand.NextDouble();
                    da = 0;
                    x = rand.Next(gl_Width);

                    // Align to grid
                    if (param[6] == 0)
                    {
                        x -= x % (max + param[3] + 1);
                    }

                    if (param[2] == 0)
                    {
                        y = -max - 11;
                    }
                    else
                    {
                        y = rand.Next(gl_Height) - max - 11;
                    }

                    X = dx = 0;
                    dy = ((float)rand.NextDouble() + 0.01f) * (rand.Next(23) + 1);

                    switch (param[0])
                    {
                        case 0:
                            width = height = max;
                            break;

                        case 1:
                            width = height = rand.Next(max) + 2;
                            break;

                        case 2:
                            width = max;
                            height = rand.Next(2) + 1;
                            break;

                        case 3:
                            width = rand.Next(max) + 3;
                            height = rand.Next(2) + 1;
                            break;
                    }

                    // Rotation angle setup
                    if (param[1] == 1)
                    {
                        dx = myUtils.randomSign(rand) * (float)rand.NextDouble();

                        // Slowly falling pieces should not rotate very fast
                        if (dy < 1 && myUtils.randomChance(rand, 4, 5))
                            dx /= (rand.Next(11) + 10);
                    }

                    if (param[4] > 0)
                    {
                        da = -(float)rand.NextDouble() / param[4];
                    }
                    break;

                case 34:
                    a = 1.0f;
                    width = gl_Width;
                    height = gl_Height / N;
                    x = X = 0;
                    y = Y = (list.Count - 1) * height;
                    break;

                case 35:
                    a = (float)rand.NextDouble();
                    width = height = param[0] == 0 ? max : rand.Next(max) + 3;
                    x = rand.Next(gl_Width);
                    y = rand.Next(gl_Height);
                    X = rand.Next(2) == 0
                        ? +(float)(Math.PI / 4)
                        : -(float)(Math.PI / 4);
                    cnt = rand.Next(111) + 11;
                    break;

                case 36:
                    a = (float)rand.NextDouble()/2;
                    width = height = (param[0] == 0) ? (max) : (rand.Next(max) + 3);
                    x = rand.Next(gl_Width);
                    y = rand.Next(gl_Height);
                    cnt = rand.Next(777) + 123;
                    X = Y = 0;
                    dx = (float)rand.NextDouble()/5  + 1.0f;                                // Pulsating speed factor
                    dy = (float)rand.NextDouble()/10 + 0.005f;                              // Pulsating speed
                    break;

                case 37:
                    a = (float)rand.NextDouble()/2;

                    switch (param[0])
                    {
                        case 0: width = 1;                    break;
                        case 1: width = max;                  break;
                        case 2: width = (rand.Next(max) + 1); break;
                    }

                    height = 0;
                    x = rand.Next(gl_Width);
                    y = gl_Height/2;
                    cnt = rand.Next(777) + 123;
                    X = Y = 0;
                    dx = (float)rand.NextDouble() + rand.Next(9);                        // Pulsating speed factor
                    dy = (float)rand.NextDouble() + 0.01f;                               // Pulsating speed
                    break;

                case 38:
                    a = (float)rand.NextDouble()/2;
                    da = 0;
                    cnt = rand.Next(1111) + 111;

                    height = width = max;

                    if (rand.Next(2) == 0)
                    {
                        x = rand.Next(gl_Width);
                        X = x + width;
                        y = Y = rand.Next(2) == 0 ? 0 : gl_Height;
                        dx = 0;
                        dy = (y == 0 ? 1 : -1) * (float)rand.NextDouble();
                    }
                    else
                    {
                        x = X = rand.Next(2) == 0 ? 0 : gl_Width;
                        y = Y = rand.Next(gl_Height);
                        dy = 0;
                        dx = (x == 0 ? 1 : -1) * (float)rand.NextDouble();
                    }

                    // Apply speed factor
                    dx *= (rand.Next(param[1]) + 1);
                    dy *= (rand.Next(param[1]) + 1);
                    break;

                case 39:
                    a = (float)rand.NextDouble();

                    y = Y = rand.Next(gl_Height);

                    if (myUtils.randomChance(rand, 1, 3))
                    {
                        height = width = rand.Next(max * 2) + max/2;
                        x = X = -111;
                        a /= 13;
                    }
                    else
                    {
                        height = width = rand.Next(max/2) + 1;
                        x = X = gl_Width + 111;
                    }

                    // Use rectangles
                    if (param[1] == 1)
                    {
                        width = (int)(width * (1.0 + rand.NextDouble() + rand.NextDouble()));
                    }

                    dx = (x > 0) ? -(float)rand.NextDouble() : (float)rand.NextDouble();
                    dy = param[0] == 0 ? 0 : myUtils.randomSign(rand) * rand.Next(11);

                    if (x > 0)
                        dy /= 2;

                    dx *= (rand.Next(11) + 1);
                    break;

                case 40:
                    a = 0.5f + (float)rand.NextDouble()/3;
                    x = X = rand.Next(gl_Width);
                    y = Y = rand.Next(gl_Height);
                    width = height = rand.Next(max) + 3;
                    cnt = (cnt == -12345) ? rand.Next(11) + 3 : 22 + rand.Next(33);

                    if (myUtils.randomChance(rand, 1, 3))
                    {
                        int offset = 2;

                        if (myUtils.randomChance(rand, 1, 2))
                        {
                            for (int i = 0; i < 3; i++)
                            {
                                offset += rand.Next(offset);
                            }
                        }
                        else
                        {
                            for (int i = 0; i < 5; i++)
                            {
                                offset += rand.Next(offset);
                            }
                        }

                        X += myUtils.randomSign(rand) * rand.Next(offset/2);
                        Y += myUtils.randomSign(rand) * rand.Next(offset/2);
                    }
                    break;

                case 41:
                    if (list.Count < N)
                    {
                        a = 0.66f;

                        switch (param[0])
                        {
                            case 0:
                                x = max * list.Count;
                                y = 0;
                                dx = (param[2] == 0 || param[2] == 1) ? paramf[0] : (float)(rand.NextDouble() + rand.Next(7));
                                dy = 0;
                                width = param[1];
                                height = gl_Height;
                                break;

                            case 1:
                                x = 0;
                                y = max * list.Count;
                                dx = 0;
                                dy = (param[2] == 0 || param[2] == 2) ? paramf[1] : (float)(rand.NextDouble() + rand.Next(7));
                                width = gl_Width;
                                height = param[1];
                                break;

                            case 2:
                                if (list.Count < ((gl_Width / max) + 1))
                                {
                                    x = max * list.Count;
                                    y = 0;
                                    dx = (param[2] == 0 || param[2] == 1) ? paramf[0] : (float)(rand.NextDouble() + rand.Next(7));
                                    dy = 0;
                                    width = param[1];
                                    height = gl_Height;
                                }
                                else
                                {
                                    x = 0;
                                    y = max * (list.Count - (gl_Width / max) - 1);
                                    dx = 0;
                                    dy = (param[2] == 0 || param[2] == 2) ? paramf[1] : (float)(rand.NextDouble() + rand.Next(7));
                                    width = gl_Width;
                                    height = param[1];
                                }
                                break;
                        }
                    }
                    else
                    {
                        a = 0.5f + (float)rand.NextDouble();

                        // New vertical line moving to the right
                        if (dx > 0)
                        {
                            if (param[2] == 0 || param[2] == 1)
                            {
                                float min = gl_Width;

                                foreach (myObj_330 obj in list)
                                    if (obj.dx != 0 && obj.x < min)
                                        min = obj.x;

                                x = min - max;
                            }
                            else
                            {
                                // With varying speed, it is possible the last line will be already far from zero point, so put the new one just before zero
                                x = -7 - rand.Next(max);
                            }
                        }

                        // New horizontal line moving down
                        if (dy > 0)
                        {
                            if (param[2] == 0 || param[2] == 2)
                            {
                                float min = gl_Height;

                                foreach (myObj_330 obj in list)
                                    if (obj.dy != 0 && obj.y < min)
                                        min = obj.y;

                                y = min - max;
                            }
                            else
                            {
                                // With varying speed, it is possible the last line will be already far from zero point, so put the new one just before zero
                                y = -7 - rand.Next(max);
                            }
                        }
#if false
                        if (dx > 0 && rand.Next(11) == 0)
                        {
                            dx += myUtils.randomSign(rand) * (float)rand.NextDouble();

                            if (dx <= 0)
                                dx = 0.1f;
                        }

                        if (dy > 0 && rand.Next(11) == 0)
                        {
                            dy += myUtils.randomSign(rand) * (float)rand.NextDouble();

                            if (dy <= 0)
                                dy = 0.1f;
                        }
#endif
                    }
                    break;

                case 42:
                    {
                        void generateX(byte mode)
                        {
                            dx = (float)rand.NextDouble() + 0.001f;

                            y = rand.Next(gl_Height);
                            dy = 0;

                            switch (mode)
                            {
                                case 0: x = myUtils.randomChance(rand, 1, 2) ? 0 : gl_Width; break;
                                case 1: x = 0; break;
                                case 2: x = gl_Width; break;
                            }

                            x += x > 0 ? 111 : -111;
                            dx *= myUtils.signOf(-x);
                        }

                        void generateY(byte mode)
                        {
                            dy = (float)rand.NextDouble() + 0.001f;

                            x = rand.Next(gl_Width);
                            dx = 0;

                            switch (mode)
                            {
                                case 0: y = myUtils.randomChance(rand, 1, 2) ? 0 : gl_Height; break;
                                case 1: y = 0; break;
                                case 2: y = gl_Height; break;
                            }

                            y += y > 0 ? 111 : -111;
                            dy *= myUtils.signOf(-y);
                        }

                        void generateXY()
                        {
                            if (myUtils.randomChance(rand, 1, 2))
                                generateX(0);
                            else
                                generateY(0);
                        }

                        a = param[1] == 0 ? 0.5f : (float)(rand.NextDouble() / param[1]);
                        width = height = max;

                        switch (param[2])
                        {
                            case 0: generateX(1); break;
                            case 1: generateX(2); break;
                            case 2: case 3: generateX(0); break;

                            case 4: generateY(1); break;
                            case 5: generateY(2); break;
                            case 6: case 7: generateY(0); break;

                            default: generateXY(); break;
                        }

                        X = x;
                        Y = y;

                        dx *= (rand.Next(11) + 1);
                        dy *= (rand.Next(11) + 1);
                    }
                    break;

                case 43:
                    a = param[2] == 0 ? 0.66f : myUtils.randFloat(rand);
                    r = g = b = 1.0f;

                    width  = rand.Next(5 * max) + 13;
                    height = rand.Next(3 * max) + 13;

                    switch (param[4])
                    {
                        case 0: width  = 1; break;
                        case 1: height = 1; break;
                        case 2:
                            if (myUtils.randomChance(rand, 1, 2))
                                width = 1;
                            else
                                height = 1;
                            break;
                    }

                    x = rand.Next(gl_Width);
                    y = rand.Next(gl_Height);

                    X = (int)(x - x % (max + param[0]));
                    Y = (int)(y - y % (max + param[0]));

                    cnt = (cnt == -12345) ? rand.Next(11): rand.Next(66) + 33;

                    // Get additional color from the background
                    if (myUtils.randomChance(rand, 1, 3))
                        colorPicker.getColor(x + width/2, y + height/2, ref r, ref g, ref b);
                    break;

                case 44:
                    a = myUtils.randFloat(rand, min: 0.05f);

                    switch (param[2])
                    {
                        case 0:
                            width = height = max;
                            break;

                        case 1:
                            width  = rand.Next(2 * max) + 3;
                            height = rand.Next(1 * max) + 3;
                            break;
                    }

                    x = X = rand.Next(gl_Width);
                    y = Y = rand.Next(gl_Height);

                    // Align to vertical grid
                    if (param[3] != 0)
                    {
                        y -= y % (max + param[3]);
                    }

                    dx = myUtils.randFloat(rand, min: 0.1f) * (rand.Next(param[0]) + 1);
                    dy = myUtils.randFloat(rand, min: 0.1f);
                    dy = 0;
                    break;

                case 45:
                    a = myUtils.randFloat(rand, min: 0.1f);

                    width = max;
                    height = 2;

                    x = X = rand.Next(gl_Width);
                    y = Y = rand.Next(gl_Height);

                    dx = myUtils.randomSign(rand) * myUtils.randFloat(rand, min: 0.1f) * (rand.Next(param[0]) + 1);
                    dy = 0;
                    break;

                case 46:
                    a = myUtils.randFloat(rand, min: 0.1f);
                    a = 0.9f;

                    x = rand.Next(gl_Width);
                    y = rand.Next(gl_Height);

                    x -= x % (max + param[0]);
                    y -= y % (max + param[0]);

                    X = x;
                    Y = y;

                    // Width is used as a move interval
                    width = rand.Next(param[1]) + 3;

                    cnt = rand.Next(333) + 50;
                    break;

                case 47:
                    a = da = myUtils.randFloat(rand) * (myUtils.randomChance(rand, 1, 666) ? 0.25f : 0.05f);

                    x = X = rand.Next(gl_Width);
                    y = Y = rand.Next(gl_Height);

                    // Align to grid
                    if (param[4] > 0)
                    {
                        X -= x % (max + param[0]);
                        Y -= y % (max + param[0]);
                    }

                    // Border color and opacity:
                    {
                        switch (param[2])
                        {
                            case 0:
                                r = g = b = 1.0f;
                                break;

                            case 1:
                                r = myUtils.randFloat(rand);
                                g = myUtils.randFloat(rand);
                                b = myUtils.randFloat(rand);
                                da /= 2;
                                break;

                            case 2:
                                colorPicker.getColor(x, y, ref r, ref g, ref b);
                                break;

                            case 3:
                                colorPicker.getColorAverage(X, Y, max, max, ref r, ref g, ref b);
                                break;
                        }
                    }

                    cnt = rand.Next(111) + 7;
                    break;

                case 48:
                    a = myUtils.randFloat(rand) / (rand.Next(2) + 1);

                    x = rand.Next(gl_Width);
                    y = rand.Next(gl_Height);

                    // Align to grid
                    if (param[2] != 0)
                    {
                        x -= x % (max + param[0]);
                        y -= y % (max + param[0]);
                    }

                    X = x;
                    Y = y;

                    dx = rand.Next(23) + 7;
                    dy = rand.Next(23) + 7;
                    da = 1.0f + myUtils.randFloat(rand) * rand.Next(3);

                    cnt = rand.Next(111) + 7;
                    width = rand.Next(333);
                    break;

                case 49:
                    a = myUtils.randFloat(rand) / 2;

                    x = rand.Next(gl_Width);
                    y = rand.Next(gl_Height);

                    if (param[2] == 1)
                    {
                        x -= x % (max + param[0]);
                        y -= y % (max + param[0]);
                    }

                    cnt = rand.Next(param[1]) + 3;
                    break;

                case 50:
                    a = 0;
                    cnt = 2;

                    x = rand.Next(gl_Width);
                    y = rand.Next(gl_Height);

                    colorPicker.getColor(x, y, ref r, ref g, ref b);

                    switch (param[1])
                    {
                        case 0:
                            {
                                float R = 0, G = 0, B = 0, delta = 0.0001f;
                                width = height = 1;

                                while (width < 9 && a == 0)
                                {
                                    colorPicker.getColorAverage(x, y, ++width, ++height, ref R, ref G, ref B);

                                    if (Math.Abs(R - r) > delta || Math.Abs(G - g) > delta || Math.Abs(B - b) > delta)
                                        a = 1;
                                }
                            }
                            break;

                        case 1:
                            {
                                float R = 0, G = 0, B = 0, delta = 0.01f;

                                colorPicker.getColorAverage(x, y, 8, 8, ref R, ref G, ref B);

                                if (Math.Abs(R - r) > delta || Math.Abs(G - g) > delta || Math.Abs(B - b) > delta)
                                {
                                    a = 1;
                                    width = height = 8;
                                }
                            }
                            break;

                        case 2:
                            {
                                float R = 0, G = 0, B = 0, delta = 0.5f;

                                colorPicker.getColorAverage(x, y, 8, 8, ref R, ref G, ref B);

                                if (Math.Abs(R + G + B - r - g - b) > delta)
                                {
                                    a = 1;
                                    width = height = 8;
                                }
                            }
                            break;
                    }
                    break;

                case 51:
                    a = myUtils.randFloat(rand, 0.1f);
                    da = 0;

                    switch (param[0])
                    {
                        case 0:
                            width  = rand.Next(13) + 1;
                            height = rand.Next(1234) + 111;

                            a = a / width * 2.5f;
                            da = t;

                            x = rand.Next(gl_Width  + 200) - 100;
                            y = rand.Next(gl_Height + 200) - 100;

                            x -= x % 25;
                            y -= y % 25;
                            break;

                        case 1:
                            width  = rand.Next(1234) + 123;
                            height = rand.Next(13) + 1;

                            a = a / height * 2.5f;
                            da = t;

                            x = rand.Next(gl_Width  + 200) - 100;
                            y = rand.Next(gl_Height + 200) - 100;

                            x -= x % 25;
                            y -= y % 25;
                            break;
                    }

                    cnt = rand.Next(333) + 123;
                    break;

                case 52:
                    doClearBuffer = false;
                    a = myUtils.randFloat(rand, 0.1f) / 2;

                    x = X = rand.Next(gl_Width);
                    y = Y = rand.Next(gl_Height);

                    width = height = rand.Next(max) + 11;

                    cnt = rand.Next(33) + 11 + (int)t;
                    break;

                case 53:
                    a = myUtils.randFloat(rand, 0.2f) / 2;

                    x = rand.Next(gl_Width);
                    y = rand.Next(gl_Height);

                    width = 475 + rand.Next(123);

                    dx = myUtils.randFloat(rand) * myUtils.randomSign(rand) * (rand.Next(11) + 7);
                    dy = myUtils.randFloat(rand) * myUtils.randomSign(rand) * (rand.Next(11) + 7);
                    break;

                case 54:
                    a = myUtils.randFloat(rand, 0.1f);

                    x = X = rand.Next(gl_Width);
                    y = Y = rand.Next(gl_Height);

                    width = height = rand.Next(max) + 1;

                    dx = myUtils.randFloat(rand) * myUtils.randomSign(rand) * (rand.Next(11) + 7);
                    dy = myUtils.randFloat(rand) * myUtils.randomSign(rand) * (rand.Next(11) + 7);
                    break;

                case 55:
                    a = myUtils.randFloat(rand, 0.1f);

                    x = X = rand.Next(gl_Width);
                    y = Y = rand.Next(gl_Height);

                    width = height = rand.Next(max) + 1;

                    // Align to grid
                    if (param[4] != 0)
                    {
                        max = max < 7 ? 7 : max;
                        X = (int)(x - x % (max + param[4]));
                        Y = (int)(y - y % (max + param[4]));
                    }

                    dx = dy = 0;
                    cnt = 0;
                    break;

                case 56:
                    a = 0.85f;

                    x = (param[2] == 0) ? rand.Next(gl_Width) : gl_x0;
                    y = rand.Next(gl_Height);

                    X = (int)(x - x % (max + param[0]));
                    Y = (param[1] == 0) ? y : (int)(y - y % (max + param[0]));

                    x = X;
                    y = Y;

                    dx = 0.123f + myUtils.randFloat(rand) * (rand.Next(11) + 7);
                    dy = 0;

                    width = 0;
                    cnt = 0;
                    break;

                case 57:
                    a = 0.95f;
                    x = X = y = Y = gl_Width + 1;
                    width = height = max;
                    break;

                case 58:
                    {
                        a = 0.85f;

                        int step = max + param[0], offsetx, offsety;

                        offsetx = (gl_Width  % step);
                        offsety = (gl_Height % step);

                        if (p == null)
                        {
                            var prm = new p58_myObj_330();
                            p = prm;

                            if (p58_myObj_330._list1 == null)
                            {
                                prm.initLists(gl_Width, gl_Height, step, offsetx, offsety);
                            }
                        }

                        x = rand.Next(gl_Width  - offsetx);
                        y = rand.Next(gl_Height - offsety);
                        X = rand.Next(gl_Width  - offsetx);
                        Y = rand.Next(gl_Height - offsety);

                        x = (x - x % step) + offsetx/2;
                        y = (y - y % step) + offsety/2;
                        X = (X - X % step) + offsetx/2;
                        Y = (Y - Y % step) + offsety/2;

                        width = height = max;
                        cnt = rand.Next(7) + 1;
                    }
                    break;
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            bool b1 = false;

            switch (mode)
            {
                case 00:
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

                case 01:
                    x = X = rand.Next(gl_Width);
                    y = Y = rand.Next(gl_Height);

                    if (rand.Next(2) == 0)
                    {
                        width = rand.Next(max) + 1;
                        height = myUtils.randomChance(rand, 1, 11) ? 2 : 1;
                        a = 0.1f * max / width;
                        //width = gl_Width;
                    }
                    else
                    {
                        height = rand.Next(max) + 1;
                        width = myUtils.randomChance(rand, 1, 11) ? 2 : 1;
                        a = 0.1f * max / height;
                        //height = gl_Height;
                    }

                    if (doUseRandDxy)
                    {
                        x += myUtils.randomSign(rand) * rand.Next(3);
                        y += myUtils.randomSign(rand) * rand.Next(3);
                    }
                    break;

                case 02:
                case 03:
                case 04:
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

                    // Resample texture
                    if (doSampleOnce && param[1] == 1)
                    {
                        if (myUtils.randomChance(rand, 1, 321) && x > 0 && y > 0 && x < gl_Width && y < gl_Height)
                        {
                            X = x;
                            Y = y;
                        }
                    }
                    break;

                case 05:
                case 06:
                    y += dy;
                    a -= da;

                    if (y < -width || y > gl_Height + width)
                    {
                        generateNew();
                        return;
                    }
                    break;

                case 07:
                    if (a > 0.75f && da > 0)
                        da *= -1;

                    x += dx;
                    y += dy;
                    a += da;
                    break;

                case 08:
                    if (--X <= 0)
                    {
                        width  = rand.Next(max) + 1;
                        height = rand.Next(max) + 1;
                        x = rand.Next(gl_Width);
                        y = rand.Next(gl_Height);
                        X = rand.Next(66) + 1;

                        if (param[0] != 0)
                        {
                            x -= x % (max + param[0]);
                            y -= y % (max + param[0]);
                        }
                    }
                    break;

                case 09:
                    if (--cnt < 0)
                    {
                        // Shape
                        switch (param[0])
                        {
                            case 0:
                                width  = rand.Next(max) + 1;
                                height = rand.Next(max) + 1;
                                break;

                            case 1:
                                width = height = rand.Next(max) + 1;
                                break;

                            case 2:
                                width = height = max;
                                break;

                            case 3:
                                height = param[1] % 23;
                                width = 2 * max;
                                break;

                            case 4:
                                height = param[1] % 23;
                                width  = max * (rand.Next(23) + 2);
                                break;
                        }

                        // Coordinates
                        x = X = rand.Next(gl_Width);
                        y = Y = rand.Next(gl_Height);

                        // Overall speed factor
                        cnt = rand.Next(param[2]) + 1;

                        // Opacity
                        switch (param[3])
                        {
                            case 0: a = (float)rand.NextDouble() * 1;   break;
                            case 1: a = (float)rand.NextDouble() * 2;   break;
                            case 2: a = (float)Math.Sin((Y+100*t)/100); break;
                            case 3: a = 0.85f;                          break;
                        }

                        // Blurring offset
                        if (doUseRandDxy)
                        {
                            X += myUtils.randomSign(rand) * (rand.Next(2 * param[1] + 1) - param[1]);
                            Y += myUtils.randomSign(rand) * (rand.Next(2 * param[1] + 1) - param[1]);
                        }
                    }
                    break;

                case 10:
                    if (--cnt < 0)
                    {
                        // Shape
                        switch (param[0])
                        {
                            case 0:
                                width  = rand.Next(max) + 1;
                                height = rand.Next(max) + 1;
                                break;

                            case 1:
                                width = height = rand.Next(max) + 1;
                                break;

                            case 2:
                                width = height = max;
                                break;

                            case 3:
                                height = param[1] % 23;
                                width = 2*max;
                                break;
                        }

                        // Coordinates
                        x = rand.Next(gl_Width);
                        y = rand.Next(gl_Height);
                        X = rand.Next(gl_Width);
                        Y = rand.Next(gl_Height);

                        // Opacity
                        switch (param[3])
                        {
                            case 0: a = (float)rand.NextDouble() * 1;   break;
                            case 1: a = (float)rand.NextDouble() * 2;   break;
                            case 2: a = (float)Math.Sin((Y+100*t)/100); break;
                            case 3: a = 0.85f;                          break;
                        }

                        // Overall speed factor
                        cnt = rand.Next(param[1]) + 1;

                        // Vingette
                        if (param[2] != 0)
                        {
                            float dist = 0;

                            if (X < param[2] || Y < param[2])
                            {
                                dist = X > Y ? Y : X;
                            }
                            else if (X > (gl_Width - param[2]) || Y > (gl_Height - param[2]))
                            {
                                dist = (gl_Width - X) > (gl_Height - Y) ? (gl_Height - Y) : (gl_Width - X);
                            }

                            if (dist > 0)
                            {
                                a *= (dist / param[2]);
                            }
                        }
                    }
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
                        width += rand.Next(max);
                    }
                    else
                    {
                        if (rand.Next(9) == 0)
                            width++;
                        height += rand.Next(max);
                    }
                    break;

                case 13:
                    tex.setOpacity(rand.NextDouble());
                    height = 1;
                    x = rand.Next(gl_Width+max)-max;
                    y = rand.Next(gl_Height);

                    if (rand.Next(9) == 0)
                        height++;
                    width = rand.Next(max);
                    break;

                case 14:
                    tex.setOpacity(rand.NextDouble());
                    width = 1;
                    x = rand.Next(gl_Width);
                    y = rand.Next(gl_Height + max) - max;

                    if (rand.Next(9) == 0)
                        width++;
                    height = rand.Next(max);
                    break;

                case 15:
                case 16:
                case 17:
                    if ((dx > 0 && x > gl_Width ) || (dx < 0 && x < -width))
                        a = -1;

                    if ((dy > 0 && y > gl_Height) || (dy < 0 && y < -height))
                        a = -1;

                    x += dx;
                    y += dy;
                    break;

                case 18:
                    if (x < -100 || x > gl_Width + 100 || y < -100 || y > gl_Height + 100)
                        generateNew();

                    if (--cnt <= 0)
                    {
                        cnt = rand.Next(param[2]) + 1;

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

                    if (param[2] == 1)
                    {
                        //dx += dx > 0 ? 0.1f : -0.1f;
                    }

                    if ((dx > 0 && x > gl_Width) || (dx < 0 && x < 0))
                    {
                        dx *= -1;
                        y -= (height + 1 + param[1]);

                        a *= (dx > 0) ? 2.0f : 0.5f;
                    }
                    break;

                case 20:
                    x += dx;

                    switch (param[2])
                    {
                        case 0:
                        case 2:
                            if (dx > 0 && x > gl_Width)
                                x = -rand.Next(50) - 11;
                            else
                                if (dx < 0 && x < -width)
                                    dx *= -1;
                            break;

                        case 1:
                        case 3:
                            if (dx < 0 && x < -width)
                                x = gl_Width + rand.Next(50);
                            else
                                if (dx > 0 && x > gl_Width)
                                    dx *= -1;
                            break;

                        default:
                            if ((dx > 0 && x > gl_Width) || (dx < 0 && x < -width))
                                dx *= -1;
                            break;
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

                case 32:

                    if (x < 0 || x > gl_Width || y < 0 || y > gl_Height)
                    {
                        generateNew();
                    }

                    // Slow down every particle
                    if (param[3] > 0)
                    {
                        if (cnt-- > 0)
                        {
                            break;
                        }
                        else
                        {
                            cnt = rand.Next(param[3]) + 1;
                        }
                    }

                    x += X;
                    y += Y;
                    X = Y = 0;

                    if (rand.Next(2) == 0)
                    {
                        // vertical line
                        width = param[0];
                        Y = (rand.Next(max) + width + 1);
                        height = (int)(Y);

                        if (rand.Next(2) == 0)
                        {
                            // go down
                        }
                        else
                        {
                            // go up
                            y -= Y;
                            Y = 0;
                        }
                    }
                    else
                    {
                        // horizontal line
                        height = param[0];
                        X = (rand.Next(max) + height + 1);
                        width = (int)(X);

                        if (rand.Next(2) == 0)
                        {
                            // go right
                        }
                        else
                        {
                            // go left
                            x -= X;
                            X = 0;
                        }
                    }
                    break;

                case 33:
                    y += dy;
                    a += da;

                    // Rotation angle
                    if (param[1] == 1)
                    {
                        X += dx;

                        // Vary rotation speed somehow
                        if (myUtils.randomChance(rand, 1, 23))
                        {
                            dx += myUtils.randomSign(rand) * myUtils.randFloat(rand);
                        }
                    }

                    // y-axis acceleration
                    if (param[3] > 0)
                    {
                        dy += (float)rand.NextDouble() / param[3];
                    }

                    if (y > gl_Height + 111)
                        a = 0;
                    break;

                case 34:
                    switch (param[0])
                    {
                        case 0:
                            x += (float)(Math.Sin(35 * cnt / (y + 30)));
                            break;

                        case 1:
                            x += (float)(Math.Sin(35 * cnt / (y/33 + 30)));
                            break;

                        case 2:
                            x += (float)(Math.Sin(35 * cnt / (y + 30)));
                            x += (float)(Math.Sin(35 * cnt / (y / 33 + (float)rand.NextDouble())));
                            break;

                        case 3:
                            x += (float)(Math.Sin(cnt / 3 + gl_Height / 2 - y)) + 3 * (float)(Math.Cos(cnt / 33 + y * 0.01f));
                            break;

                        case 4:
                            x += (float)(Math.Sin(cnt/30) * (1.0f + y * 0.001f));
                            x += (float)(Math.Sin(cnt/30 + y * 0.1f) * 11);
                            break;

                        case 5:
                            x += (float)rand.NextDouble() * myUtils.randomSign(rand);
                            break;

                        case 6:
                            x += (float)(Math.Sin(cnt / 30 + Y / 111) * (1.0f + y * 0.001f));
                            x += (float)(Math.Sin(cnt / 123 + Y / 23) * (y * 0.0001f));
                            break;

                        case 7:
                            x += (float)(Math.Sin(cnt / 3 + Y / (cnt + 1)) * 0.25f);
                            break;

                        case 8:
                            x += (float)(Math.Sin(cnt / 30 + Y / 66 + (Y % 33) / 11) * 1.0f);
                            break;
                    }

                    cnt++;

                    if (param[2] == 0)
                    {
                        a = 0.9f;
                    }
                    else
                    {
                        if (x == X)
                        {
                            a = 1.0f;
                        }
                        else
                        {
                            a = 0.25f + 13.0f / (float)(Math.Abs(X - x));
                        }
                    }
                    break;

                case 35:
                    if (--cnt == 0)
                        a = -1.0f;
                    break;

                case 36:
                    Y += dy;
                    X = (float)Math.Sin(Y) * dx;
                    a += (X > 0) ? -0.0005f : +0.0005f;

                    x -= (int)X;
                    y -= (int)X;
                    width  += 2*(int)X;
                    height += 2*(int)X;

                    if (--cnt <= 0)
                        a -= 0.035f;
                    break;

                case 37:
                    Y += dy;
                    X = (float)Math.Sin(Y) * dx;

                    height += (int)(X * 1.75f);
                    a += (X > 0) ? -0.0005f : +0.0005f;

                    switch (param[1])
                    {
                        case 0:
                            break;

                        case 1:
                            y = gl_y0 + 50 * (float)Math.Sin(x/66);
                            break;

                        case 2:
                            y = gl_y0 + param[4] * (float)(Math.Sin(param[3]*t + x / param[2]));
                            break;

                        case 3:
                            y = gl_y0 + param[4] * (float)(Math.Sin(param[3]*t + x / (100.0 + 333 * Math.Sin(t))));
                            break;
                    }

                    if (--cnt <= 0)
                        a -= 0.035f;
                    break;

                case 38:
                    b1 = da == 0 && rand.Next(param[2]) == 0;         // Side move -- used in case 2

                    switch (param[0])
                    {
                        case 0:
                            x += dx;
                            y += dy;
                            break;

                        case 1:
                            if (dx == 0)
                            {
                                if (b1)
                                {
                                    x += myUtils.randomSign(rand) * rand.Next(2 * param[1] / 3);
                                }
                                else
                                {
                                    y += (dy > 0 ? 1 : -1) * rand.Next(param[1]);
                                }
                            }
                            else
                            {
                                if (b1)
                                {
                                    y += myUtils.randomSign(rand) * rand.Next(2 * param[1] / 3);
                                }
                                else
                                {
                                    x += (dx > 0 ? 1 : -1) * rand.Next(param[1]);
                                }
                            }
                            break;

                        case 2:
                            if (dx == 0)
                            {
                                if (b1)
                                {
                                    X = x;
                                    x += myUtils.randomSign(rand) * rand.Next(param[1]);
                                    Y = y - width;
                                    da = 1;
                                }
                                else
                                {
                                    Y = y;
                                    X = x + width;
                                    height = rand.Next(param[1]);
                                    y += (dy > 0) ? height : -height;
                                    da = 0;
                                }
                            }
                            else
                            {
                                if (b1)
                                {
                                    Y = y;
                                    y += myUtils.randomSign(rand) * rand.Next(param[1]);
                                    X = x - width;
                                    da = 1;
                                }
                                else
                                {
                                    // Here, width is actually a height, and height is a width
                                    X = x;
                                    Y = y + width;
                                    height = rand.Next(param[1]);
                                    x += (dx > 0) ? height : -height;
                                    da = 0;
                                }
                            }
                            break;
                    }

                    if (--cnt <= 0)
                        a -= 0.035f;
                    break;

                case 39:
                    x += dx;
                    //y += dy;

                    if ((dx > 0 && x > gl_Width) || (dx < 0 && x < -111))
                        a = -1;
                    break;

                case 40:
                    if (--cnt < 0)
                        a = -1;
                    break;

                case 41:
                    x += dx;
                    y += dy;

                    if ((dx > 0 && x > gl_Width) || (dx < 0 && x < 0))
                        a = -1;

                    if ((dy > 0 && y > gl_Height) || (dy < 0 && y < 0))
                        a = -1;
                    break;

                case 42:
                    x += dx;
                    y += dy;

                    // Align to grid
                    if (param[3] > 0)
                    {
                        float oldx = X, oldy = Y;

                        X = (int)(x - x % (width + param[0]));
                        Y = (int)(y - y % (width + param[0]));

                        // Add some slight random fluctuation along the non-moving axis
                        if (param[5] != 0)
                        {
                            X += dx == 0 ? myUtils.randomSign(rand) * rand.Next(param[5]) : 0;
                            Y += dy == 0 ? myUtils.randomSign(rand) * rand.Next(param[5]) : 0;
                        }

                        // Apply acceleration
                        if (param[4] < 2)
                        {
                            if (param[4] == 0)
                            {
                                if (X != oldx)
                                    dx *= 1.01f;

                                if (Y != oldy)
                                    dy *= 1.01f;
                            }

                            if (param[4] == 1)
                            {
                                if (X != oldx)
                                    dx *= 1.025f;

                                if (Y != oldy)
                                    dy *= 1.025f;
                            }
                        }
                    }
                    else
                    {
                        X = x;
                        Y = y;
                    }

                    if ((dx > 0 && x > gl_Width ) || (dx < 0 && x < -111))
                        a = -1;

                    if ((dy > 0 && y > gl_Height) || (dy < 0 && y < -111))
                        a = -1;
                    break;

                case 43:
                    if (--cnt < 0)
                        a = -1;
                    break;

                case 44:
                    x -= dx;
                    X += dx;

                    param[1] = 2;

                    // Change the particle size
                    switch (param[1])
                    {
                        case 1:
                            width += 1;
                            break;

                        case 2:
                            width += rand.Next(3) + 1;
                            dy += myUtils.randFloat(rand);

                            if (dy > 1.0f && height > 1)
                            {
                                height -= 1;
                                y += 0.5f;
                                Y = y;
                                dy = 0;
                            }
                            break;
                    }

                    if (x < 0 || X > gl_Width - width)
                        a -= 0.0025f;
                    break;

                case 45:
                    x += dx;
                    y += dy;

                    if ((dx > 0 && x >= gl_Width - width) || (dx < 0 && x <= 0))
                    {
                        dx *= -1;
                    }
                    break;

                case 46:
                    if (--cnt <= 0)
                        a = -1;

                    if (cnt % width == 0)
                    {
                        X += myUtils.random101(rand) * myUtils.randFloat(rand, 0.1f) * rand.Next(param[2]);
                        Y += myUtils.random101(rand) * myUtils.randFloat(rand, 0.1f) * rand.Next(param[2]);
                    }
                    break;

                case 47:
                    if (--cnt <= 0)
                        a = -1;
                    break;

                case 48:
                    switch (param[1])
                    {
                        case 0:
                            X += (float)(Math.Sin(t * dx + width) * da);
                            break;

                        case 1:
                            X += (float)(Math.Sin(t * dx + width) * da);
                            Y += (float)(Math.Cos(t * dx + width) * da);
                            break;

                        case 2:
                            x += (float)(Math.Sin(t * dx + width) * da);
                            break;

                        case 3:
                            x += (float)(Math.Sin(t * dx + width) * da);
                            y += (float)(Math.Cos(t * dx + width) * da);
                            break;

                        case 4:
                            X += (float)(Math.Sin(t * dx + width) * da);
                            x += (float)(Math.Sin(t * dy + width) * da);
                            break;

                        case 5:
                            X += (float)(Math.Sin(t * dx + width) * da);
                            Y += (float)(Math.Cos(t * dx + width) * da);
                            x += (float)(Math.Sin(t * dy + width) * da);
                            y += (float)(Math.Cos(t * dy + width) * da);
                            break;
                    }

                    if (--cnt < 0)
                        a -= myUtils.randFloat(rand) * 0.0037f;
                    break;

                case 49:
                    if (--cnt < 0)
                        a -= myUtils.randFloat(rand) * 0.05f;
                    break;

                case 50:
                    if (--cnt < 0)
                        a -= myUtils.randFloat(rand);
                    break;

                case 51:
                    if (--cnt < 0)
                        a -= myUtils.randFloat(rand)/5;

                    switch (param[0])
                    {
                        case 0:
                            da = t + myUtils.randomSign(rand) * t * 0.0001f * a;
                            break;

                        case 1:
                            da = t + myUtils.randomSign(rand) * t * 0.0001f * a;
                            break;
                    }
                    break;

                case 52:
                    if (--cnt < 0)
                        a = -1;
                    break;

                case 53:
                    x += dx;
                    y += dy;

                    dx += myUtils.randFloat(rand) * myUtils.randomSign(rand) * 0.5f;
                    dy += myUtils.randFloat(rand) * myUtils.randomSign(rand) * 0.5f;

                    if (x < 250)
                        dx += myUtils.randFloat(rand);

                    if (x > gl_Width - 250)
                        dx -= myUtils.randFloat(rand);

                    if (y < 250)
                        dy += myUtils.randFloat(rand);

                    if (y > gl_Height - 250)
                        dy -= myUtils.randFloat(rand);
                    break;

                case 54:
                    X = x;
                    Y = y;

                    x += dx;
                    y += dy;

                    dx += myUtils.randFloat(rand) * myUtils.randomSign(rand) * 0.5f;
                    dy += myUtils.randFloat(rand) * myUtils.randomSign(rand) * 0.5f;

                    if (x < 250)
                        dx += myUtils.randFloat(rand);

                    if (x > gl_Width - 250)
                        dx -= myUtils.randFloat(rand);

                    if (y < 250)
                        dy += myUtils.randFloat(rand);

                    if (y > gl_Height - 250)
                        dy -= myUtils.randFloat(rand);

                    // Make a sudden stop
                    if (param[1] == 1 && rand.Next(123) == 0)
                    {
                        dx /= 111;
                        dy /= 111;
                    }
                    break;

                case 55:
                    if (cnt > 0)
                    {
                        cnt--;      // Keep moving
                    }
                    else
                    {
                        if (cnt == 0)
                        {
                            width  -= 2;
                            height -= 2;
                            y += 1;
                            x += 1;
                            cnt = -1;
                        }

                        // Stop moving
                        switch (param[1])
                        {
                            case 0:
                                dx = dy = 0;
                                break;

                            case 1:
                                dx /= 2;
                                dy /= 2;
                                break;

                            case 2:
                                dx /= 1.1f;
                                dy /= 1.1f;
                                break;

                            case 3:
                                dx /= 1.05f;
                                dy /= 1.05f;
                                break;
                        }

                        // Start moving
                        if (rand.Next(param[2]) == 0)
                        {
                            cnt = rand.Next(123) + 13;
                            dx = dy = 0;

                            switch (param[0])
                            {
                                case 0:
                                    dx = myUtils.randFloat(rand) * myUtils.randomSign(rand) * rand.Next(13);
                                    break;

                                case 1:
                                    dy = myUtils.randFloat(rand) * myUtils.randomSign(rand) * rand.Next(13);
                                    break;

                                case 2:
                                    if (rand.Next(2) == 0)
                                        dx = myUtils.randFloat(rand) * myUtils.randomSign(rand) * rand.Next(13);
                                    else
                                        dy = myUtils.randFloat(rand) * myUtils.randomSign(rand) * rand.Next(13);
                                    break;
                            }

                            // Tyr to keep all the particles within the screen range
                            {
                                if ((x < 100 && dx < 0) || (x > gl_Width - 100 && dx > 0))
                                    if (myUtils.randomChance(rand, 1, 2))
                                        dx = -dx;

                                if ((y < 100 && dy < 0) || (y > gl_Height - 100 && dy > 0))
                                    if (myUtils.randomChance(rand, 1, 2))
                                        dy = -dy;
                            }

                            // Moving particle is slightly bigger than a dormant one
                            width  += 2;
                            height += 2;
                            y -= 1;
                            x -= 1;
                        }
                    }

                    x += dx;
                    y += dy;

                    // Optionally align to grid
                    if (param[4] != 0)
                    {
                        X = (int)(x - x % (max + param[4]));
                        Y = (int)(y - y % (max + param[4]));
                    }
                    break;

                case 56:
                    if (++cnt > 1)
                    {
                        x += dx;
                        y += dy;

                        dx += 0.001f;
                        width = (int)((x - X) / (max + param[0])) * (max + param[0]);

                        if (X - width < 0 && X + width > gl_Width)
                            a -= myUtils.randFloat(rand);
                    }
                    break;

                case 57:
                    x = X;
                    y = Y;

                    X += width + param[0];

                    if (param[2] == 1)
                        a = myUtils.randFloat(rand);

                    if (param[1] > 0)
                        width = max + rand.Next(3 * max);

                    if (X >= gl_Width)
                    {
                        X = 0;
                        Y += max + param[0];

                        if (Y >= gl_Height)
                        {
                            if (param[3] == 1)
                            {
                                max = rand.Next(50) + 35;
                                generateNew();
                            }

                            X = Y = 0;
                        }
                    }
                    break;

                case 58:
                    if (--cnt == 0)
                        a = -1;
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
                case 00:
                case 01:
                    tex.Draw((int)x, (int)y, width, height, (int)X, (int)Y, width, height);
                    break;

                case 02:
                case 03:
                case 04:
                    if (doSampleOnce)
                    {
                        tex.Draw((int)x - width, (int)y - height, 2 * width, 2 * height, (int)X - width, (int)X - height, 2 * width, 2 * height);
                    }
                    else
                    {
                        tex.Draw((int)x - width, (int)y - height, 2 * width, 2 * height, (int)x - width, (int)y - height, 2 * width, 2 * height);
                    }
                    break;

                case 05:
                case 06:
                case 07:
                    tex.Draw((int)x - width, (int)y - width, 2 * width, 2 * width, (int)x - width, (int)y - width, 2 * width, 2 * width);
                    break;

                case 08:
                    tex.Draw((int)x - width, (int)y - height, 2 * width, 2 * height, (int)x - width, (int)y - height, 2 * width, 2 * height);
                    break;

                case 09:
                case 10:
                    if (cnt == 0)
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
                    tex.Draw((int)x, (int)y, width, height, (int)x, (int)y, width, height);
                    break;

                case 19:
                    if (param[1] == 0)
                    {
                        tex.Draw((int)x, (int)y, width, height, (int)x, (int)y, width, height);
                    }
                    else
                    {
                        X = x - x % (width + param[1]);

                        tex.Draw((int)X, (int)y, width, height, (int)X, (int)y, width, height);
                    }
                    break;

                case 20:
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
                                tex.setAngle(Math.PI / 2);
                                break;

                            case 1:
                                tex.setAngle(Math.PI);
                                break;

                            case 2:
                                tex.setAngle(3 * Math.PI / 2);
                                break;

                            case 3:
                                tex.setAngle(rand.Next(4) * Math.PI / 2);
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
                            int w2 = (part - 2) * width / part;

                            tex.Draw((int)x + w1, (int)y, w2, height, (int)x + rx + w1, (int)y + ry, w2, height);
                        }
                        else
                        {
                            int h1 = height / part;
                            int h2 = (part - 2) * height / part;

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
                    tex.setAngle(X);
                    tex.Draw((int)x, (int)y, width, height, (int)x, (int)y, width, height);
                    break;

                case 28:
                    tex.setOpacity(a);
                    tex.setAngle(Math.Sin(dx * cnt / 250) / 17);
                    tex.Draw((int)x, (int)y, width, height, (int)x, (int)y, width, height);
                    break;

                case 29:
                case 30:
                    switch (param[0])
                    {
                        case 0:
                            tex.setOpacity(a);
                            tex.setAngle(Math.PI);
                            tex.Draw((int)x, (int)y, width, height, (int)x, (int)y, width, height);
                            break;

                        case 1:
                            tex.setOpacity(a / 2);

                            tex.setAngle(0);
                            tex.Draw((int)x, (int)y, width, height, (int)x, (int)y, width, height);

                            tex.setAngle(Math.PI);
                            tex.Draw((int)x, (int)y, width, height, (int)x, (int)y, width, height);
                            break;

                        case 2:
                            tex.setOpacity(a / 4);

                            tex.setAngle(0);
                            tex.Draw((int)x, (int)y, width, height, (int)x, (int)y, width, height);

                            tex.setAngle(Math.PI);
                            tex.Draw((int)x, (int)y, width, height, (int)x, (int)y, width, height);

                            tex.setAngle(Math.PI / 2);
                            tex.Draw((int)x, (int)y, width, height, (int)x, (int)y, width, height);

                            tex.setAngle(3 * Math.PI / 2);
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
                            tex.Draw((int)x, (int)y, width, height, (int)x, (int)y, 2 * width, 2 * height);
                            break;
                    }
                    break;

                case 31:
                    tex.setOpacity(a);
                    showCase31(param[0]);
                    break;

                case 32:
                    tex.setOpacity(a);
                    tex.Draw((int)x, (int)y, width, height, (int)x, (int)y, width, height);
                    break;

                case 33:
                    tex.setOpacity(a);
                    tex.setAngle(X);

                    if (param[5] == 0)
                    {
                        tex.Draw((int)x, (int)y, width, height, (int)x, (int)y, width, height);
                    }
                    else
                    {
                        int A = rand.Next(5) * myUtils.randomSign(rand);

                        tex.Draw((int)x - A, (int)y - A, width + 2 * A, height + 2 * A, (int)x - A, (int)y - A, width + 2 * A, height + 2 * A);
                    }
                    break;

                case 34:
                    tex.setOpacity(a);

                    switch (param[1])
                    {
                        case 0:
                            tex.Draw((int)x, (int)y, width, height, (int)X, (int)Y, width, height);
                            break;

                        case 1:
                            tex.Draw((int)(x / 10), (int)Y, (int)(width), height, (int)(X), (int)(Y), (int)(width), (int)(height + x));
                            break;
                    }
                    break;

                case 35:
                    tex.setOpacity(a);
                    tex.setAngle(X);
                    tex.Draw((int)x, (int)y, width, height, (int)x, (int)y, width, height);
                    break;

                case 36:
                    tex.setOpacity(a);
                    tex.Draw((int)x, (int)y, width, height, (int)x, (int)y, width, height);
                    break;

                case 37:
                    tex.setOpacity(a);
                    tex.Draw((int)x, (int)(y - height), width, 2 * height, (int)x, (int)(y - height), width, 2 * height);
                    break;

                case 38:
                    tex.setOpacity(a);

                    if (param[0] != 2)
                    {
                        tex.Draw((int)x, (int)y, width, height, (int)x, (int)y, width, height);
                    }
                    else
                    {
                        int _x = (int)(x > X ? X : x);
                        int _w = (int)(x > X ? x - X : X - x);
                        int _y = (int)(y > Y ? Y : y);
                        int _h = (int)(y > Y ? y - Y : Y - y);

                        tex.Draw(_x, _y, _w, _h, _x, _y, _w, _h);
                    }
                    break;

                case 39:
                    tex.setOpacity(a);
                    tex.Draw((int)x, (int)y, width, height, (int)x, (int)(y + dy), width, height);
                    break;

                case 40:
                    if (cnt == 0)
                    {
                        tex.setOpacity(a);
                        tex.Draw((int)x, (int)y, width, height, (int)X, (int)Y, width, height);
                    }
                    break;

                case 41:
                    tex.setOpacity(a);
                    //tex.setColor(r, g, b);
                    tex.Draw((int)x, (int)y, width, height, (int)x, (int)y, width, height);
                    break;

                case 42:
                    tex.setOpacity(a);
                    tex.Draw((int)X, (int)Y, width, height, (int)X, (int)Y, width, height);
                    break;

                case 43:
                    if (cnt < param[1])
                    {
                        tex.setColor(r, g, b);
                        tex.setOpacity(a);

                        for (int i = 0; i < width; i += (max + param[0]))
                            for (int j = 0; j < height; j += (max + param[0]))
                                tex.Draw((int)X + i, (int)Y + j, max, max, (int)X + i, (int)Y + j, param[5], param[5]);
                    }
                    break;

                case 44:
                    tex.Draw((int)x, (int)y, width, height, (int)x, (int)y, width, height);
                    tex.Draw((int)X, (int)Y, width, height, (int)X, (int)Y, width, height);
                    break;

                case 45:
                    if (doSampleOnce)
                    {
                        tex.Draw((int)x, (int)y, width, height, (int)X, (int)Y, width, height);
                    }
                    else
                    {
                        tex.Draw((int)x, (int)y, width, height, (int)x, (int)y, width, height);
                    }
                    break;

                case 46:
                    tex.Draw((int)x, (int)y, max, max, (int)X, (int)Y, max, max);
                    break;

                case 47:
                    if (cnt == 1)
                    {
                        int offsetx = param[5] == 1 ? myUtils.random101(rand) * rand.Next(3) : 0;
                        int offsety = param[5] == 1 ? myUtils.random101(rand) * rand.Next(3) : 0;
                        tex.Draw((int)X, (int)Y, max, max, (int)X + offsetx, (int)Y + offsety, max, max);

                        // Slightly fill the cell with its border color
                        if (param[3] == 1)
                        {
                            myPrimitive._Rectangle.SetColor(r, g, b, da / 2);
                            myPrimitive._Rectangle.Draw((int)X + 1, (int)Y + 1, max - 2, max - 2, true);
                        }

                        myPrimitive._Rectangle.SetColor(r, g, b, da);

                        switch (param[1])
                        {
                            case 1:
                                myPrimitive._Rectangle.Draw((int)X + 1, (int)Y + 1, max - 2, max - 2, false);
                                break;

                            case 2:
                                myPrimitive._Rectangle.Draw((int)X + rand.Next(3), (int)Y + rand.Next(3), max - rand.Next(6), max - rand.Next(6), false);
                                break;
                        }

                        // Draw grid lines intersections
                        if (param[6] == 1)
                        {
                            int n = 3;

                            myPrimitive._Rectangle.SetColor(0, 0, 0, 0.5f);

                            myPrimitive._Rectangle.Draw((int)X, (int)Y, n, n, true);
                            myPrimitive._Rectangle.Draw((int)X, (int)Y + max - n, n, n, true);
                            myPrimitive._Rectangle.Draw((int)X + max - n, (int)Y, n, n, true);
                            myPrimitive._Rectangle.Draw((int)X + max - n, (int)Y + max - n, n, n, true);
                        }
                    }
                    break;

                case 48:
                    tex.Draw((int)x, (int)y, max, max, (int)X, (int)Y, max, max);
                    break;

                case 49:
                    if (param[2] == 0)
                    {
                        for (int i = 1; i <= 3; i++)
                        {
                            int n = i * max / 3;
                            tex.Draw((int)x - n, (int)y - n, 2 * n, 2 * n, (int)x - n, (int)y - n, 2 * n, 2 * n);
                        }
                    }
                    else
                    {
                        tex.setOpacity(a * 2);
                        tex.Draw((int)x, (int)y, max, max, (int)x, (int)y, max, max);
                    }
                    break;

                case 50:
                    if (a > 0 && cnt == 0)
                    {
                        switch (param[0])
                        {
                            case 0:
                                tex.setOpacity(0.85f);
                                tex.Draw((int)x, (int)y, width, height, (int)x, (int)y, width, height);
                                break;

                            case 1:
                                tex.setOpacity(myUtils.randFloat(rand));
                                tex.Draw((int)x, (int)y, width, height, (int)x, (int)y, width, height);
                                break;

                            case 2:
                                myPrimitive._Rectangle.SetColor(r, g, b, myUtils.randFloat(rand));
                                myPrimitive._Rectangle.Draw((int)x, (int)y, width, height, true);
                                break;

                            case 3:
                                myPrimitive._Rectangle.SetColor(r, g, b, 0.5f);
                                myPrimitive._Rectangle.Draw((int)x, (int)y, width, height, true);

                                tex.setOpacity(0.5f);
                                tex.Draw((int)x, (int)y, width, height, (int)x, (int)y, width, height);
                                break;
                        }
                    }
                    break;

                case 51:
                    tex.setOpacity(a);
                    tex.setAngle(Math.Sin(da));
                    tex.Draw((int)x, (int)y, width, height, (int)x, (int)y, width, height);
                    break;

                case 52:
                    if (cnt == 1)
                    {
                        tex.setOpacity(a);

                        for (int i = 0; i < rand.Next(param[1]) + 3; i++)
                        {
                            int offsetx = myUtils.randomSign(rand) * rand.Next(param[2]);
                            int offsety = myUtils.randomSign(rand) * rand.Next(param[2]);

                            tex.Draw((int)x + offsetx, (int)y + offsety, width, height, (int)x, (int)y, width, height);
                        }
                    }
                    break;

                case 53:
                    if (id < param[1])
                    {
                        int d = width;
                        int step = max + param[0];
                        int offset = step / 2;
                        double dist = 0;

                        X = (int)(x - x % step);
                        Y = (int)(y - y % step);

                        d -= d % step;

                        int xmin = (int)X - d;
                        xmin = xmin < 0 ? 0 : xmin;

                        int ymin = (int)Y - d;
                        ymin = ymin < 0 ? 0 : ymin;

                        for (int i = xmin; i < X + d; i += step)
                        {
                            for (int j = ymin; j < Y + d; j += step)
                            {
                                dist = (x - i + offset) * (x - i + offset) + (y - j + offset) * (y - j + offset);
                                //dist = Math.Sqrt(dist);

                                a = (float)(max * d / dist / 4);

                                //if (a > 0.1f)
                                if (a > paramf[0])
                                {
                                    tex.setOpacity(a);
                                    tex.Draw(i, j, max, max, i, j, max, max);
                                }
                            }
                        }
                    }
                    else
                    {
                        tex.Draw((int)x, (int)y, 5, 5, (int)x, (int)y, 5, 5);
                    }
                    break;

                case 54:
                    switch (param[0])
                    {
                        case 0:
                            tex.Draw((int)x, (int)y, width, height, (int)x, (int)y, width, height);
                            break;

                        case 1:
                            tex.Draw((int)x, (int)y, width, height, (int)X, (int)Y, width, height);
                            break;
                    }
                    break;

                case 55:
                    if (param[4] != 0)
                    {
                        tex.Draw((int)X, (int)Y, max, max, (int)X, (int)Y, max, max);
                    }
                    else
                    {
                        switch (param[3])
                        {
                            case 0:
                                tex.Draw((int)x, (int)y, width, height, (int)X, (int)Y, width, height);
                                break;

                            case 1:
                            case 2:
                                tex.Draw((int)x, (int)y, width, height, (int)x, (int)y, width, height);
                                break;
                        }
                    }
                    break;

                case 56:
                    {
                        tex.Draw((int)(X - width), (int)Y, max, max, (int)(X - width), (int)Y, max, max);
                        tex.Draw((int)(X + width), (int)Y, max, max, (int)(X + width), (int)Y, max, max);
                    }
                    break;

                case 57:
                    myPrimitive._Rectangle.SetColor(0, 0, 0, 1);
                    myPrimitive._Rectangle.Draw((int)X, (int)Y, width + param[0], height + param[0], true);

                    switch (param[4])
                    {
                        case 0:
                            tex.Draw((int)X, (int)Y, width, height, (int)X, (int)Y, width, height);
                            break;

                        case 1:
                            tex.Draw((int)X, (int)Y, width, height,
                                (int)X + myUtils.randomSign(rand) * rand.Next(7),
                                (int)Y + myUtils.randomSign(rand) * rand.Next(7), width, height);
                            break;
                    }

                    myPrimitive._Rectangle.SetColor(0.66f, 0.66f, 0.66f, 0.37f);
                    myPrimitive._Rectangle.Draw((int)X + 2, (int)Y + 2, width - 4, height - 4, false);
                    break;

                case 58:

                    //tex.Draw((int)x, (int)y, width, height, prm.getX(index1), prm.getY(index1), width, height);

                    if (cnt == 1)
                    {
                        var prm = (p as p58_myObj_330);

                        void getAvg(int index, ref float val, ref int Cnt)
                        {
                            if (prm.getF(index) >= 0)
                            {
                                val = prm.getF(index);
                                Cnt++;
                            }
                            else
                            {
                                colorPicker.getColorAverage(prm.getX(index), prm.getY(index), width, height, ref r, ref g, ref b);
                                val = prm.getLuminosity(r, g, b, mode: param[1]);
                                prm.setF(index, val);
                            }
                        }

                        float avg1 = 0, avg2 = 0;
                        int trySwap = 0, step = max + param[0], index1 = 0, index2 = -1;
                        int w = gl_Width / step;
                        index1 = (int)y / step * w + (int)x / step;

                        switch (param[2])
                        {
                            // Comparing to a random cell
                            case 0:
                                index2 = (int)Y / step * w + (int)X / step;
                                break;

                            // Comparing to a cell to the left
                            case 1:
                                if (index1 % w != 0)
                                {
                                    index2 = index1 - 1;
                                    Y = y;
                                    X = x - step;
                                }
                                break;
                        }

                        if (index2 >= 0)
                        {
                            getAvg(index1, ref avg1, ref trySwap);
                            getAvg(index2, ref avg2, ref trySwap);

                            if (trySwap == 2)
                            {
                                switch (param[2])
                                {
                                    case 0:
                                        if (avg1 > avg2 == y > Y)
                                            prm.swap(index1, index2);
                                        break;

                                    case 1:
                                        if (avg1 > avg2)
                                            prm.swap(index1, index2);
                                        break;
                                }
                            }
                        }

                        if (index2 >= 0)
                        {
                            tex.Draw((int)x, (int)y, width, height, prm.getX(index1), prm.getY(index1), width, height);
                            tex.Draw((int)X, (int)Y, width, height, prm.getX(index2), prm.getY(index2), width, height);
                        }
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

            if (doClearBuffer)
            {
                glDrawBuffer(GL_FRONT_AND_BACK | GL_DEPTH_BUFFER_BIT);

                float r = (float)rand.NextDouble()/11;
                float g = (float)rand.NextDouble()/11;
                float b = (float)rand.NextDouble()/11;

                glClearColor(r, g, b, 1.0f);
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
                for (int i = 0; i < N; i++)
                    list.Add(new myObj_330());

            while (!Glfw.WindowShouldClose(window))
            {
                processInput(window);

                Glfw.SwapBuffers(window);
                Glfw.PollEvents();

                if (doClearBuffer)
                {
                    glClear(GL_COLOR_BUFFER_BIT);
                }
                else
                {
                    dimScreen(dimAlpha, false);
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
                t += dt;
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

                // Each cell is rotated around its center
                case 5:
                    if (isFirst)
                    {
                        X = myUtils.randomSign(rand) * (float)rand.NextDouble();
                        Y = 0;
                    }

                    x1 -= 1 * (int)Y/2;
                    y1 -= 1 * (int)Y/2;
                    w1 += 2 * (int)Y/2;
                    h1 += 2 * (int)Y/2;

                    tex.setOpacity(0.1f);
                    tex.setAngle(Y);

                    Y += X;
                    break;

                // Each cell displays slightly larger area than itself (offset larger than width, constant)
                case 6:
                    offset = width * param[1];
                    x2 -= 1 * offset;
                    y2 -= 1 * offset;
                    w2 += 2 * offset;
                    h2 += 2 * offset;
                    break;

                // Each cell displays slightly larger area than itself (offset larger than width, various)
                case 7:
                    if (isFirst)
                        X = (float)rand.NextDouble();

                    offset = (int)(width * (param[1] + X));
                    x2 -= 1 * offset;
                    y2 -= 1 * offset;
                    w2 += 2 * offset;
                    h2 += 2 * offset;
                    break;

                // Each cell displays slightly larger area than itself (offset lesser than width, various)
                case 8:
                    if (isFirst)
                        X = (float)rand.NextDouble();

                    offset = (int)(width * X);
                    x2 -= 1 * offset;
                    y2 -= 1 * offset;
                    w2 += 2 * offset;
                    h2 += 2 * offset;
                    break;

                // Each cell displays slightly larger area than itself (offset lesser than width, constant)
                case 9:
                    offset = (int)(width * paramf[0]);
                    x2 -= 1 * offset;
                    y2 -= 1 * offset;
                    w2 += 2 * offset;
                    h2 += 2 * offset;
                    break;

                // Each cell displays slightly less area than itself (constant offset)
                case 10:
                    offset = (int)(width * paramf[0]/2);
                    x2 += 1 * offset;
                    y2 += 1 * offset;
                    w2 -= 2 * offset;
                    h2 -= 2 * offset;
                    break;

                // Each cell displays slightly less area than itself (various offset)
                case 11:
                    if (isFirst)
                        X = (float)rand.NextDouble()/2;

                    offset = (int)(width * X);
                    x2 += 1 * offset;
                    y2 += 1 * offset;
                    w2 -= 2 * offset;
                    h2 -= 2 * offset;
                    break;

                // Cell's zoom level fluctuate
                case 12:
                    if (isFirst)
                    {
                        X = 0;
                        Y = myUtils.random101(rand) * 0.1f;
                    }
                    else
                    {
                        int sign = (Y > 0)
                            ? myUtils.randomChance(rand, 1, 23) ? -1 : +1
                            : myUtils.randomChance(rand, 1, 23) ? +1 : -1;

                        Y = sign * width * 0.005f;
                    }

                    X += Y;
                    offset = (int)X;

                    x2 -= 1 * offset;
                    y2 -= 1 * offset;
                    w2 += 2 * offset;
                    h2 += 2 * offset;
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

    // ==================== Helper Classes =========================================================================================

    public class p58_myObj_330 : myObjectParams
    {
        public static List<  int> _list1 = null;
        public static List<  int> _list2 = null;
        public static List<float> _list3 = null;

        public p58_myObj_330()
        {
        }

        public void initLists(int w, int h, int step, int offsetx, int offsety)
        {
            _list1 = new List<int>();
            _list2 = new List<int>();
            _list3 = new List<float>();

            for (int j = 0; j < h / step; j++)
            {
                for (int i = 0; i < w / step; i++)
                {
                    _list1.Add(i * step + offsetx / 2);
                    _list2.Add(j * step + offsety / 2);
                    _list3.Add(-1.0f);
                }
            }
        }

        public int getX(int index)
        {
            return _list1[index];
        }

        public int getY(int index)
        {
            return _list2[index];
        }

        public float getF(int index)
        {
            return _list3[index];
        }

        public void setF(int index, float value)
        {
            _list3[index] = value;
        }

        public void swap(int i1, int i2)
        {
            int tmpi = _list1[i1];
            _list1[i1] = _list1[i2];
            _list1[i2] = tmpi;

            tmpi = _list2[i1];
            _list2[i1] = _list2[i2];
            _list2[i2] = tmpi;

            float tmpf = _list3[i1];
            _list3[i1] = _list3[i2];
            _list3[i2] = tmpf;
        }

        public float getLuminosity(float r, float g, float b, int mode)
        {
            float res = 0.0f;

            switch (mode)
            {
                case 0:
                    res = (r + g + b) / 3.0f;
                    break;

                case 1:
                    res = 10 * r + 5 * g + 1 * b;
                    break;

                case 2:
                    res = gray(r, g, b);
                    break;
            }

            return res;
        }

        // GRAY VALUE ("brightness")
        // https://stackoverflow.com/questions/596216/formula-to-determine-perceived-brightness-of-rgb-color
        private int gray(float r, float g, float b)
        {
            // sRGB luminance(Y) values
            const float rY = 0.212655f;
            const float gY = 0.715158f;
            const float bY = 0.072187f;

            // Inverse of sRGB "gamma" function. (approx 2.2)
            float inv_gam_sRGB(float color)
            {
                if (color <= 0.04045)
                    return color / 12.92f;
                else
                    return (float)Math.Pow(((color + 0.055) / (1.055)), 2.4);
            }

            // sRGB "gamma" function (approx 2.2)
            int gam_sRGB(float v)
            {
                if (v <= 0.0031308f)
                    v *= 12.92f;
                else
                    v = (float)(1.055 * Math.Pow(v, 1.0 / 2.4) - 0.055);

                // This is correct in C++. Other languages may not require +0.5
                return (int)(v * 255 + 0.5);
            }

            return gam_sRGB(
                    rY * inv_gam_sRGB(r) +
                    gY * inv_gam_sRGB(g) +
                    bY * inv_gam_sRGB(b)
            );
        }
    }
};
