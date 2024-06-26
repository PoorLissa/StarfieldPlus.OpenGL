﻿using GLFW;
using static OpenGL.GL;
using System.Collections.Generic;
using System;


/*
    - Raster scan of an image
*/


namespace my
{
    public class myObj_0810 : myObject
    {
        // Priority
        public static int Priority => 10;
		public static System.Type Type => typeof(myObj_0810);

        private float x;
        private float size, localMaxSize, A, R, G, B, dR, dG, dB;
        private float topY, topR, topG, topB;

        private static uint cnt = 0;
        private static int N = 0, Y = 0, step = 10, maxSize = 1, sizeMode = 0, drawMode = 0;
        private static int yDir = 1;
        private static float maxOpacity = 0, r = 0, g = 0, b = 0, lineWidth = 1, fallingSpeed = 0, slowFactor = 1.0f;

        private static myScreenGradient grad = null;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_0810()
        {
            if (id != uint.MaxValue)
                generateNew();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time global initialization
        protected override void initGlobal()
        {
            colorPicker = new myColorPicker(gl_Width, gl_Height, mode: myColorPicker.colorMode.SNAPSHOT_OR_IMAGE);
            list = new List<myObject>();

            // Global unmutable constants
            {
                N = gl_Width;

                lineWidth = 0.1f + myUtils.randFloat(rand) * rand.Next(5);
            }

            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time local initialization
        private void initLocal()
        {
            doClearBuffer = myUtils.randomChance(rand, 1, 5);
            doClearBuffer = true;

            sizeMode = rand.Next(2);
            drawMode = rand.Next(12);

            maxOpacity = 0.05f + myUtils.randFloat(rand) * 0.25f;

            maxSize = (gl_y0/2 + rand.Next(gl_y0/2)) / 3;

            fallingSpeed = 0.1f + myUtils.randFloat(rand) * 0.6f;

            slowFactor = 1.0f;
            slowFactor = 0.75f;
            slowFactor = 0.1f;
            slowFactor = 0.1f + myUtils.randFloat(rand) * 0.9f;

            step = 5 + rand.Next(16);
            renderDelay = 20 - step;

            renderDelay += 1;

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            height = 600;

            string nStr(int   n) { return n.ToString("N0");    }
            string fStr(float f) { return f.ToString("0.000"); }

            string str = $"Obj = {Type}\n\n"                         +
                            $"N = {nStr(list.Count)} of {nStr(N)}\n" +
                            $"doClearBuffer = {doClearBuffer}\n"     +
                            $"step = {step}\n"                       +
                            $"slowFactor = {fStr(slowFactor)}\n"     +
                            $"sizeMode = {sizeMode}\n"               +
                            $"drawMode = {drawMode}\n"               +
                            $"maxSize = {maxSize}\n"                 +
                            $"maxOpacity = {fStr(maxOpacity)}\n"     +
                            $"lineWidth = {fStr(lineWidth)}\n"       +
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
            x = id;
            size = 1;

            A = doClearBuffer ? maxOpacity * 3 : maxOpacity;
            R = G = B = 0;

            topY = 0;
            topR = 0;
            topG = 0;
            topB = 0;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            if (cnt == 0)
            {
                colorPicker.getColor(x, Y, ref r, ref g, ref b);

                dR = (r - R) * slowFactor / step;
                dG = (g - G) * slowFactor / step;
                dB = (b - B) * slowFactor / step;

                switch (sizeMode)
                {
                    case 0:
                        localMaxSize = maxSize;
                        break;

                    case 1:
                        localMaxSize = maxSize + rand.Next(7);
                        break;
                }
            }

            R += dR;
            G += dG;
            B += dB;

            // Size is recalculated each frame, because it directly depends on R, G, B
            size = 5 + (R + G + B) * localMaxSize;

            // Remember the highest point and its color (will be used to draw separate hovering dots)
            if (size > topY)
            {
                topY = size + 33;
                topR = r;
                topG = g;
                topB = b;
            }
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Show()
        {
            switch (drawMode)
            {
                // Full height
                case 000:
                    {
                        myPrimitive._LineInst.setInstanceCoords(x, 0, x, gl_Height);
                        myPrimitive._LineInst.setInstanceColor(R, G, B, A);
                    }
                    break;

                // Partial constant height
                case 001:
                    {
                        myPrimitive._LineInst.setInstanceCoords(x, gl_y0 - maxSize, x, gl_y0 + maxSize);
                        myPrimitive._LineInst.setInstanceColor(R, G, B, A);
                    }
                    break;

                // Size height on both sides of central line
                case 002:
                    myPrimitive._LineInst.setInstanceCoords(x, gl_y0 + size, x, gl_y0 - size);
                    myPrimitive._LineInst.setInstanceColor(R, G, B, A);
                    break;

                // Size height, top-down
                case 003:
                    {
                        myPrimitive._LineInst.setInstanceCoords(x, 100, x, 100 + size);
                        myPrimitive._LineInst.setInstanceColor(R, G, B, A);
                    }
                    break;

                // Size height, down-top
                case 004:
                    {
                        float y = gl_Height - 100;
                        myPrimitive._LineInst.setInstanceCoords(x, y, x, y - size);
                        myPrimitive._LineInst.setInstanceColor(R, G, B, A);
                    }
                    break;

                // Size height, top-down + dots at a 'size' height
                case 005:
                    {
                        myPrimitive._LineInst.setInstanceCoords(x, 100, x, 100 + size);
                        myPrimitive._LineInst.setInstanceColor(R, G, B, A);

                        myPrimitive._RectangleInst.setInstanceCoords(x - 1, 100 + size - 1, 2, 2);
                        myPrimitive._RectangleInst.setInstanceColor(R, G, B, 0.5f);
                        myPrimitive._RectangleInst.setInstanceAngle(0);
                    }
                    break;

                // Size height, down-top + dots at a 'size' height
                case 006:
                    {
                        float y = gl_Height - 100;

                        myPrimitive._LineInst.setInstanceCoords(x, y, x, y - size);
                        myPrimitive._LineInst.setInstanceColor(R, G, B, A);

                        myPrimitive._RectangleInst.setInstanceCoords(x - 1, y - size - 1, 2, 2);
                        myPrimitive._RectangleInst.setInstanceColor(R, G, B, 0.5f);
                        myPrimitive._RectangleInst.setInstanceAngle(0);
                    }
                    break;

                // Size height, top-down + dots at a 'maxY' height
                case 007:
                    {
                        myPrimitive._LineInst.setInstanceCoords(x, 100, x, 100 + size);
                        myPrimitive._LineInst.setInstanceColor(R, G, B, A);

                        myPrimitive._RectangleInst.setInstanceCoords(x - 1, 100 + topY - 1, 2, 2);
                        myPrimitive._RectangleInst.setInstanceColor(topR, topG, topB, 0.5f);
                        myPrimitive._RectangleInst.setInstanceAngle(0);
                    }
                    break;

                // Size height, down-top + dots at a 'maxY' height
                case 008:
                    {
                        float y = gl_Height - 100;

                        myPrimitive._LineInst.setInstanceCoords(x, y, x, y - size);
                        myPrimitive._LineInst.setInstanceColor(R, G, B, A);

                        myPrimitive._RectangleInst.setInstanceCoords(x - 1, y - topY - 1, 2, 2);
                        myPrimitive._RectangleInst.setInstanceColor(topR, topG, topB, 0.5f);
                        myPrimitive._RectangleInst.setInstanceAngle(0);
                    }
                    break;

                // Size height, top-down + falling dots at a 'maxY' height
                case 009:
                    {
                        myPrimitive._LineInst.setInstanceCoords(x, 100, x, 100 + size);
                        myPrimitive._LineInst.setInstanceColor(R, G, B, A);

                        myPrimitive._RectangleInst.setInstanceCoords(x - 3, 100 + topY - 2, 7, 2);
                        myPrimitive._RectangleInst.setInstanceColor(topR, topG, topB, 0.5f);
                        myPrimitive._RectangleInst.setInstanceAngle(0);

                        topY -= fallingSpeed;
                    }
                    break;

                // Size height, down-top + falling dots at a 'maxY' height
                case 010:
                    {
                        float y = gl_Height - 100;

                        myPrimitive._LineInst.setInstanceCoords(x, y, x, y - size);
                        myPrimitive._LineInst.setInstanceColor(R, G, B, A);

                        myPrimitive._RectangleInst.setInstanceCoords(x - 1, y - topY - 1, 3, 3);
                        myPrimitive._RectangleInst.setInstanceColor(topR, topG, topB, 0.5f);
                        myPrimitive._RectangleInst.setInstanceAngle(0);

                        topY -= fallingSpeed;
                    }
                    break;

                // Sine of (R + G + B)
                case 011:
                    {
                        float val = (float)Math.Sin(R + G + B) * 333;

                        //myPrimitive._LineInst.setInstanceCoords(x, gl_y0 + val, x, gl_y0 - size/2 - val);
                        myPrimitive._LineInst.setInstanceCoords(x, gl_y0, x, gl_y0 + val);
                        myPrimitive._LineInst.setInstanceColor(R, G, B, A);
                    }
                    break;
            }
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Process(Window window)
        {
            initShapes();


            clearScreenSetup(doClearBuffer, 0.1f);

            while (list.Count < N)
            {
                list.Add(new myObj_0810());
            }

            if (doClearBuffer == false)
            {
                grad.SetOpacity(0.1f);
            }


            while (!Glfw.WindowShouldClose(window))
            {
                int Count = list.Count;

                processInput(window);

                // Swap fore/back framebuffers, and poll for operating system events.
                Glfw.SwapBuffers(window);
                Glfw.PollEvents();

                // Dim screen
                {
                    if (doClearBuffer)
                    {
                        glClear(GL_COLOR_BUFFER_BIT);
                    }

                    grad.Draw();
                }

                // Render Frame
                {
                    inst.ResetBuffer();
                    myPrimitive._LineInst.ResetBuffer();

                    for (int i = 0; i != Count; i++)
                    {
                        var obj = list[i] as myObj_0810;

                        obj.Show();
                        obj.Move();
                    }

                    myPrimitive._LineInst.Draw();
                    inst.Draw(false);
                }

                System.Threading.Thread.Sleep(renderDelay);

                // Move Y one step further
                if (++cnt == step)
                {
                    cnt = 0;
                    Y += yDir;

                    if (Y == gl_Height && yDir > 0)
                    {
                        yDir *= -1;
                    }

                    if (Y == 0 && yDir < 0)
                    {
                        yDir *= -1;
                    }
                }
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void initShapes()
        {
            base.initShapes(0, N, 0);

            myPrimitive.init_LineInst(N);

            myPrimitive._LineInst.setLineWidth(lineWidth);

            grad = new myScreenGradient();
            grad.SetRandomColors(rand, 0.2f);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------
    }
};
