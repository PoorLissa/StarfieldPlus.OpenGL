﻿using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;
using System.Drawing;


/*
    - Starfield
*/


namespace my
{
    public class myObj_000 : myObject
    {
        protected float size, dSize, A, R, G, B, angle, dAngle;
        protected float x, y, dx, dy, acceleration = 1.0f;
        protected int cnt = 0, max = 0;

        protected static int drawMode = 0, colorMode = 0, angleMode = 0;
        protected static int N = 0, staticStarsN = 0, cometsN = 0, shape = 0;
        private static bool doFillShapes = true, doCreateAllAtOnce = true;

        protected static myHexagonInst staticStarBgr = null;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_000()
        {
            generateNew();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time global initialization
        protected override void initGlobal()
        {
            colorPicker = new myColorPicker(gl_Width, gl_Height);
            list = new List<myObject>();

            // immutable constants
            {
                doClearBuffer = true;

                shape = rand.Next(5);

                N = rand.Next(333) + 100;                       // Fast moving stars
                cometsN = 3;                                    // Comets
                staticStarsN = rand.Next(333) + 333;            // Very slow moving stars which make up constellations
                N += staticStarsN;
                N += cometsN;
            }

            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time local initialization
        private void initLocal()
        {
            doFillShapes      = myUtils.randomChance(rand, 1, 2);
            doCreateAllAtOnce = myUtils.randomChance(rand, 1, 9);

            colorMode = rand.Next(3);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            height = 700;

            string str = $"Obj = myObj_000 (Starfield)\n\n"          +
                            $"Total N = {list.Count} of ({N})\n"     +
                            $"moving stars N = {N - staticStarsN}\n" +
                            $"static stars N = {staticStarsN}\n"     +
                            $"comets N = {cometsN}\n"                +
                            $"doClearBuffer = {doClearBuffer}\n"     +
                            $"doFillShapes = {doFillShapes}\n"       +
                            $"shape = {shape}\n"                     +
                            $"colorMode = {colorMode}\n"             +
                            $"angleMode = {angleMode}\n"             +
                            $"renderDelay = {renderDelay}\n"         +
                            $"file: {colorPicker.GetFileName()}"
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
            A = myUtils.randFloat(rand);

            switch (colorMode)
            {
                // Random color
                case 0:
                    R = myUtils.randFloat(rand);
                    G = myUtils.randFloat(rand);
                    B = myUtils.randFloat(rand);
                    break;

                // Random color from color picker
                case 1:
                    colorPicker.getColorRand(ref R, ref G, ref B);
                    break;

                // Limited color scheme
                case 2:
                    switch (rand.Next(50))
                    {
                        // Red
                        case 0:
                            R = 1;
                            G = 0;
                            B = 0;
                            break;

                        // Yellow
                        case 1:
                            R = 1;
                            G = 1;
                            B = 0;
                            break;

                        // Blue
                        case 2:
                            R = 0;
                            G = 0;
                            B = 1;
                            break;

                        // Orange
                        case 3:
                            R = 1;
                            G = 0.647f;
                            B = 0;
                            break;

                        // Aqua
                        case 4:
                            R = 0;
                            G = 1;
                            B = 1;
                            break;

                        // Violet
                        case 5:
                            R = 0.933f;
                            G = 0.510f;
                            B = 0.933f;
                            break;

                        // White
                        default:
                            R = 1.0f - myUtils.randFloat(rand) * 0.1f;
                            G = 1.0f - myUtils.randFloat(rand) * 0.1f;
                            B = 1.0f - myUtils.randFloat(rand) * 0.1f;
                            break;
                    }
                    break;
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Show()
        {
            switch (shape)
            {
                // Instanced squares
                case 0:
                    var rectInst = inst as myRectangleInst;

                    rectInst.setInstanceCoords(x - size/2, y - size/2, size, size);
                    rectInst.setInstanceColor(R, G, B, A);
                    rectInst.setInstanceAngle(angle);
                    break;

                // Instanced triangles
                case 1:
                    var triangleInst = inst as myTriangleInst;

                    triangleInst.setInstanceCoords(x, y, size, angle);
                    triangleInst.setInstanceColor(R, G, B, A);
                    break;

                // Instanced circles
                case 2:
                    var ellipseInst = inst as myEllipseInst;

                    ellipseInst.setInstanceCoords(x, y, 2 * size, angle);
                    ellipseInst.setInstanceColor(R, G, B, A);
                    break;

                // Instanced pentagons
                case 3:
                    var pentagonInst = inst as myPentagonInst;

                    pentagonInst.setInstanceCoords(x, y, 2 * size, angle);
                    pentagonInst.setInstanceColor(R, G, B, A);
                    break;

                // Instanced hexagons
                case 4:
                    var hexagonInst = inst as myHexagonInst;

                    hexagonInst.setInstanceCoords(x, y, 2 * size, angle);
                    hexagonInst.setInstanceColor(R, G, B, A);
                    break;
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Process(Window window)
        {
            uint cnt = 0;
            initShapes();

            // Build the background Galaxy
            var bgrTex = new myTexRectangle(buildGalaxy());

            if (doClearBuffer)
            {
                glDrawBuffer(GL_FRONT_AND_BACK | GL_DEPTH_BUFFER_BIT);
                glClearColor(0, 0, 0, 1);
            }

            // Add static stars (constellations)
            for (int i = 0; i < staticStarsN; i++)
            {
                list.Add(new myObj_000_StaticStar());
            }

            // Add comets
            for (int i = 0; i < cometsN; i++)
            {
                list.Add(new myObj_000_Comet());
            }

            if (doCreateAllAtOnce)
            {
                while (list.Count < N)
                    list.Add(new myObj_000_Star());
            }


            // Gradually display the background Galaxy
            if (true)
            {
                bgrTex.setOpacity(0);

                while (!Glfw.WindowShouldClose(window))
                {
                    processInput(window);
                    Glfw.SwapBuffers(window);
                    Glfw.PollEvents();

                    float opacity = bgrTex.getOpacity() + 0.0025f;

                    if (opacity >= 1)
                        break;

                    bgrTex.setOpacity(opacity);
                    bgrTex.Draw(0, 0, gl_Width, gl_Height);
                }

                bgrTex.setOpacity(1);
            }


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

                // Render Frame
                {
                    bgrTex.Draw(0, 0, gl_Width, gl_Height);

                    inst.ResetBuffer();
                    staticStarBgr.ResetBuffer();

                    int Count = list.Count;

                    for (int i = 0; i != Count; i++)
                    {
                        var obj = list[i] as myObj_000;

                        obj.Show();
                        obj.Move();
                    }

                    staticStarBgr.Draw(true);

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

                if (list.Count < N)
                {
                    list.Add(new myObj_000_Star());
                }

                System.Threading.Thread.Sleep(renderDelay);
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void initShapes()
        {
            staticStarBgr = new myHexagonInst(staticStarsN + cometsN);

            base.initShapes(shape, N * 3, 0);
        }

        // ---------------------------------------------------------------------------------------------------------------

        // Build random Galaxy background
        private Bitmap buildGalaxy()
        {
            Bitmap bmp = new Bitmap(gl_Width, gl_Height);
            int x1 = 0, y1 = 0, x2 = 0, y2 = 0;

            using (var gr = Graphics.FromImage(bmp))
            using (var br = new SolidBrush(Color.Red))
            {
                // Black background
                gr.FillRectangle(Brushes.Black, 0, 0, gl_Width, gl_Height);

                // Add low opacity colored spots
                for (int i = 0; i < 10; i++)
                {
                    int opacity = rand.Next(7) + 1;

                    x1 = rand.Next(gl_Width);
                    y1 = rand.Next(gl_Height);

                    x2 = rand.Next(333) + 100 * opacity;
                    y2 = rand.Next(333) + 100 * opacity;

                    br.Color = Color.FromArgb(1, rand.Next(256), rand.Next(256), rand.Next(256));

                    while (opacity != 0)
                    {
                        gr.FillRectangle(br, x1, y1, x2, y2);

                        x1 += rand.Next(25) + 25;
                        y1 += rand.Next(25) + 25;
                        x2 -= rand.Next(50) + 50;
                        y2 -= rand.Next(50) + 50;
                        opacity--;
                    }
                }

                // Add nebulae
                for (int k = 0; k < 111; k++)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        x1 = rand.Next(gl_Width);
                        y1 = rand.Next(gl_Height);

                        int masterOpacity = rand.Next(100) + 100;
                        int radius1 = rand.Next(250) + 100;
                        int radius2 = rand.Next(250) + 100;
                        colorPicker.getColor(br, x1, y1, masterOpacity);

                        gr.FillRectangle(br, x1, y1, 1, 1);

                        for (int j = 0; j < 100; j++)
                        {
                            x2 = x1 + rand.Next(radius1) - radius1 / 2;
                            y2 = y1 + rand.Next(radius2) - radius2 / 2;

                            int maxSlaveOpacity = 50 + rand.Next(50);
                            int slaveOpacity = rand.Next(maxSlaveOpacity);

                            br.Color = Color.FromArgb(slaveOpacity, br.Color.R, br.Color.G, br.Color.B);
                            gr.FillRectangle(br, x2, y2, 1, 1);

                            int r = rand.Next(2) + 3;

                            br.Color = Color.FromArgb(3, br.Color.R, br.Color.G, br.Color.B);
                            gr.FillRectangle(br, x2 - r, y2 - r, 2 * r, 2 * r);
                        }
                    }
                }
            }

            return bmp;
        }

        // ---------------------------------------------------------------------------------------------------------------
    }


    // ===================================================================================================================
    // ===================================================================================================================


    // Moving Star Class
    class myObj_000_Star : myObj_000
    {
        protected override void generateNew()
        {
            base.generateNew();

            float speed = myUtils.randFloat(rand) + rand.Next(10);

            max = rand.Next(75) + 20;
            cnt = 0;
            acceleration = 1.005f + (rand.Next(100) * 0.0005f);

            x = rand.Next(gl_Width);
            y = rand.Next(gl_Width);

            double dist = Math.Sqrt((x - gl_x0) * (x- gl_x0) + (y - gl_x0) * (y - gl_x0));
            double sp_dist = speed / dist;

            dx = (float)((x - gl_x0) * sp_dist);
            dy = (float)((y - gl_x0) * sp_dist);

            y = (y - (gl_Width - gl_Height) / 2);

            size = 0;
            dSize = myUtils.randFloat(rand) * 0.1f;

            angle = 0;

            switch (angleMode)
            {
                case 0:
                    dAngle = myUtils.randFloat(rand, 0.01f) * 0.01f * myUtils.randomSign(rand);
                    break;

                case 1:
                    dAngle = myUtils.randFloat(rand, 0.25f) * 0.50f * myUtils.randomSign(rand);
                    break;

                case 2:
                    dAngle = myUtils.randFloat(rand, 0.33f) * 2.00f * myUtils.randomSign(rand);
                    break;
            }
        }

        protected override void Show()
        {
            base.Show();
        }

        protected override void Move()
        {
            x += dx;
            y += dy;

            // todo: 
            size += dSize;
            angle += dAngle;

            acceleration *= (1.0f + (size * 0.0001f));

            if (cnt++ > max)
            {
                cnt = 0;

                // Accelerate acceleration rate
                //acceleration *= (1.0f + (size * 0.001f));
            }

            // Accelerate our moving stars
            dx *= acceleration;
            dy *= acceleration;

            if (x < 0 || x > gl_Width || y < 0 || y > gl_Height)
            {
                generateNew();
            }

            return;
        }
    };


    // ===================================================================================================================
    // ===================================================================================================================


    // Static (Constellation) Star Class
    class myObj_000_StaticStar : myObj_000
    {
        private int lifeCounter = 0;
        protected int alpha = 0, bgrAlpha = 0;
        private static int factor = 1;
        private static bool doMove = true;

        protected override void generateNew()
        {
            base.generateNew();

            lifeCounter = (rand.Next(500) + 500) * factor;

            x = rand.Next(gl_Width);
            y = rand.Next(gl_Height);
            alpha = rand.Next(50) + 175;

            bgrAlpha = 1;

            if (rand.Next(11) == 0)
                bgrAlpha = 2;

            max = (rand.Next(200) + 100) * factor;
            cnt = 0;
            size = 0;

            dAngle = myUtils.randFloat(rand) * 0.001f * myUtils.randomSign(rand);

            A *= 0.5f;

            // Make our static stars not so static
            if (doMove)
            {
                // Linear speed outwards:
                double dist = Math.Sqrt((x - gl_x0) * (x - gl_x0) + (y - gl_y0) * (y - gl_y0));
                double sp_dist = 0.1f / dist;

                dx = (float)((x - gl_x0) * sp_dist);
                dy = (float)((y - gl_y0) * sp_dist);
            }
        }

        protected override void Show()
        {
            // Background glow
            {
                int bgrSize = rand.Next(3) + 3;
                float bgrA = 0.2f / bgrSize;

                float r = R + myUtils.randomSign(rand) * myUtils.randFloat(rand) * 0.1f;
                float g = G + myUtils.randomSign(rand) * myUtils.randFloat(rand) * 0.1f;
                float b = B + myUtils.randomSign(rand) * myUtils.randFloat(rand) * 0.1f;

                staticStarBgr.setInstanceCoords(x, y, bgrSize * size, 0);
                staticStarBgr.setInstanceColor(r, g, b, bgrA);
            }

            switch (shape)
            {
                // Instanced squares
                case 0:
                    var rectInst = inst as myRectangleInst;

                    rectInst.setInstanceCoords(x - size / 2, y - size / 2, size, size);
                    rectInst.setInstanceColor(R, G, B, A/2);
                    rectInst.setInstanceAngle(angle);

                    rectInst.setInstanceCoords(x - size / 2, y - size / 2, size, size);
                    rectInst.setInstanceColor(R, G, B, A/2);
                    rectInst.setInstanceAngle(angle + (float)Math.PI * 0.25f);
                    break;

                // Instanced triangles
                case 1:
                    var triangleInst = inst as myTriangleInst;

                    triangleInst.setInstanceCoords(x, y, size, angle);
                    triangleInst.setInstanceColor(R, G, B, A/2);

                    triangleInst.setInstanceCoords(x, y, size, angle + (float)Math.PI);
                    triangleInst.setInstanceColor(R, G, B, A/2);
                    break;

                // Instanced circles
                case 2:
                    var ellipseInst = inst as myEllipseInst;

                    ellipseInst.setInstanceCoords(x, y, 2 * size, angle);
                    ellipseInst.setInstanceColor(R, G, B, A);
                    break;

                // Instanced pentagons
                case 3:
                    var pentagonInst = inst as myPentagonInst;

                    pentagonInst.setInstanceCoords(x, y, 2 * size, angle);
                    pentagonInst.setInstanceColor(R, G, B, A);

                    pentagonInst.setInstanceCoords(x, y, 2 * size, angle + (float)Math.PI);
                    pentagonInst.setInstanceColor(R, G, B, A * 0.75f);
                    break;

                // Instanced hexagons
                case 4:
                    var hexagonInst = inst as myHexagonInst;

                    hexagonInst.setInstanceCoords(x, y, 2 * size, angle);
                    hexagonInst.setInstanceColor(R, G, B, A);
                    break;
            }

            return;
        }

        protected override void Move()
        {
            angle += dAngle;

            if (doMove)
            {
                x += dx;
                y += dy;
            }

            if (lifeCounter-- == 0)
            {
                factor = 3;
                generateNew();
            }
            else
            {
                if (cnt++ > max)
                {
                    cnt = 0;
                    size = rand.Next(5) + 1;
                }
            }
        }
    };


    // ===================================================================================================================
    // ===================================================================================================================


    class myObj_000_Comet : myObj_000
    {
        private float X, Y;
        private int lifeCounter = 0, xOld = 0, yOld = 0;

        protected override void generateNew()
        {
            lifeCounter = rand.Next(1000) + 666;
            //lifeCounter = rand.Next(100) + 66;

            int x0 = rand.Next(gl_Width);
            int y0 = rand.Next(gl_Height);
            int x1 = rand.Next(gl_Width);
            int y1 = rand.Next(gl_Height);

            float a = (float)(y1 - y0) / (float)(x1 - x0);
            float b = y1 - a * x1;

            int speed = rand.Next(200) + 200;

            double dist = Math.Sqrt((x1 - x0) * (x1 - x0) + (y1 - y0) * (y1 - y0));
            double sp_dist = speed / dist;

            dx = (float)((x1 - x0) * sp_dist);
            dy = (float)((y1 - y0) * sp_dist);

            if (dx > 0)
            {
                x = X = xOld = 0;
                y = Y = yOld = (int)b;
            }
            else
            {
                X = x = xOld = gl_Width;
                y = a * x + b;
                Y = yOld = (int)y;
            }

            size = rand.Next(5) + 1;
            size = rand.Next(7) + 3;

            R = 1.0f - myUtils.randFloat(rand) * 0.23f;
            G = 0.0f + myUtils.randFloat(rand) * 0.23f;
            B = 0.0f + myUtils.randFloat(rand) * 0.23f;
            A = 1.0f - myUtils.randFloat(rand) * 0.10f;
        }

        protected override void Show()
        {
            if (lifeCounter < 0)
            {
                // Background glow
                {
                    int bgrSize = (int)size + rand.Next(3) + 3;
                    float bgrA = 0.25f;

                    float r = R + myUtils.randomSign(rand) * myUtils.randFloat(rand) * 0.1f;
                    float g = G + myUtils.randomSign(rand) * myUtils.randFloat(rand) * 0.1f;
                    float b = B + myUtils.randomSign(rand) * myUtils.randFloat(rand) * 0.1f;

                    staticStarBgr.setInstanceCoords(x, y, bgrSize * size, 0);
                    staticStarBgr.setInstanceColor(r, g, b, bgrA);
                }

                base.Show();
            }
        }

        protected override void Move()
        {
            // Wait for the counter to reach zero. Then start moving the comet
            if (lifeCounter-- < 0)
            {
                xOld = (int)X;
                yOld = (int)Y;

                x += dx;
                y += dy;

                X = (int)x;
                Y = (int)y;

                if ((dx > 0 && X > gl_Width) || (dx < 0 && X < 0))
                {
                    generateNew();
                }
            }
        }
    };


    // ===================================================================================================================
    // ===================================================================================================================
};
