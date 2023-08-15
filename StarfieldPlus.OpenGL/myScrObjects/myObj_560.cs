﻿using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;


/*
    - Pixelating an image with average colors
*/


namespace my
{
    public class myObj_560 : myObject
    {
        // Priority
        public static int Priority => 10;

        private float x, y;
        private float A, R, G, B;

        private static int N = 0, size = 0, offset = 0, offset2x = 0, genMode = 0, getColorMode = 0, mode1Divider = 0;
        private static float dimAlpha = 0.05f, maxA = 0;
        private static bool doFill = true;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_560()
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
                N = rand.Next(33) + 100;
            }

            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time local initialization
        private void initLocal()
        {
            doClearBuffer = false;

            doFill = myUtils.randomChance(rand, 5, 7);

            genMode = rand.Next(3);
            getColorMode = rand.Next(2);

            size = rand.Next(30) + 2;
            offset = rand.Next(5) - 2;
            mode1Divider = rand.Next(size - 2) + 2;
            maxA = 0.1f;

            switch (rand.Next(5))
            {
                case 2: maxA += myUtils.randFloat(rand) * 0.2f; break;
                case 3: maxA += myUtils.randFloat(rand) * 0.4f; break;
                case 4: maxA += myUtils.randFloat(rand) * 0.6f; break;
            }

            offset2x = offset * 2;

            renderDelay = rand.Next(11) + 3;

            dimAlpha = 0.001f + myUtils.randFloat(rand) * 0.002f;

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            height = 600;

            string nStr(int   n) { return n.ToString("N0");    }
            string fStr(float f) { return f.ToString("0.000"); }

            string str = $"Obj = myObj_560\n\n"                         +
                            $"N = {nStr(list.Count)} of {nStr(N)}\n"    +
                            $"size = {size}\n"                          +
                            $"maxA = {fStr(maxA)}\n"                    +
                            $"genMode = {genMode}\n"                    +
                            $"getColorMode = {getColorMode}\n"          +
                            $"offset = {offset}\n"                      +
                            $"mode1Divider = {mode1Divider}\n"          +
                            $"renderDelay = {renderDelay}\n"            +
                            $"dimAlpha = {fStr(dimAlpha)}\n"            +
                            $"file: {colorPicker.GetFileName()}"
                ;
            return str;
        }

        // ---------------------------------------------------------------------------------------------------------------

        // 
        protected override void setNextMode()
        {
            System.Threading.Thread.Sleep(123);

            initLocal();

            clearScreenSetup(doClearBuffer, 0.15f, true);
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void generateNew()
        {
            x = rand.Next(gl_Width);
            y = rand.Next(gl_Height);

            if (getColorMode == 0)
            {
                colorPicker.getColorAverage(x, y, size, size, ref R, ref G, ref B);
            }

            switch (genMode)
            {
                case 0:
                    break;

                case 1:
                    y -= y % size;
                    x -= x % size + y % mode1Divider;
                    break;

                case 2:
                    y -= y % size;
                    x -= x % size + (int)(Math.Sin(y/100) * 5);
                    break;
            }

            if (getColorMode == 1)
            {
                colorPicker.getColorAverage(x, y, size, size, ref R, ref G, ref B);
            }

            A = maxA;

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            generateNew();
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Show()
        {
            myPrimitive._Rectangle.SetColor(R, G, B, A);
            myPrimitive._Rectangle.Draw(x - offset, y - offset, size + offset2x, size + offset2x, doFill);
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Process(Window window)
        {
            uint cnt = 0;
            initShapes();

            clearScreenSetup(doClearBuffer, 0.15f, true);

            while (!Glfw.WindowShouldClose(window))
            {
                int Count = list.Count;

                processInput(window);

                // Swap fore/back framebuffers, and poll for operating system events
                Glfw.SwapBuffers(window);
                Glfw.PollEvents();

                // Dim screen
                dimScreen(dimAlpha);

                // Render Frame
                {
                    for (int i = 0; i != Count; i++)
                    {
                        var obj = list[i] as myObj_560;

                        obj.Show();
                        obj.Move();
                    }
                }

                if (Count < N)
                {
                    list.Add(new myObj_560());
                }

                cnt++;
                System.Threading.Thread.Sleep(renderDelay);
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void initShapes()
        {
            myPrimitive.init_ScrDimmer();
            myPrimitive.init_Rectangle();

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------
    }
};
