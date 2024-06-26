﻿using GLFW;
using static OpenGL.GL;
using System.Collections.Generic;
using System.Reflection;


/*
    - Falling ASKII characters (Matrix style), ver2

    todo: fix and test this (might not be the case already)
        When true, the screen will flicker because TexText inherits myTexRectangle, which has static color components;
        We change color of a symbol, and it also changes the color of a bgrTex
*/


namespace my
{
    public class myObj_0541 : myObject
    {
        class symbolItem
        {
            public bool isDead;
            public int index;
            public float x, y, a;
            public float r, g, b;

            public symbolItem(float rowX, float rowY, float rowA, float rowSizeFactor)
            {
                updateItem(rowX, rowY, rowA, rowSizeFactor);
            }

            public void updateItem(float rowX, float rowY, float rowA, float rowSizeFactor)
            {
                index = rand.Next(tTex.Lengh());

                if (doUseRandomX)
                {
                    x = rand.Next(gl_Width);
                }
                else
                {
                    x = rowX - tTex.getFieldWidth(index) * rowSizeFactor / 2;
                }

                y = rowY;
                a = rowA + myUtils.randFloatSigned(rand) * 0.15f;
                isDead = false;
            }

            public void getRandomSymbol(float rowX, float rowSizeFactor)
            {
                index = rand.Next(tTex.Lengh());
                x = rowX - tTex.getFieldWidth(index) * rowSizeFactor / 2;
            }
        };

        // ---------------------------------------------------------------------------------------------------------------

        // Priority
        public static int Priority => 30;
        public static System.Type Type => typeof(myObj_0541);

        private int yOffset = 0, cnt, deadCnt, lastIndex;
        private float x, y, dy, sizeFactor;
        private float A, R, G, B;

        private List<symbolItem> _symbols = null;

        private static int N = 0, drawMode = 0;
        private static bool doUseCustomRGB = false, doChangeSymbols = false, doUseRandomX = false;
        private static float sR = 0, sG = 0, sB = 0;

        private static int maxSpeed = 0, angleMode = 0, modX = 0, size = 20, sizeFactorMode = 0, rowSizeMode = 0, bgrMode = 0;

        private static myTexRectangle bgrTex = null;
        private static TexText tTex = null;
        private static myScreenGradient grad = null;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_0541()
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
                N = 3333;
                //N = 1;

                doUseCustomRGB = true;                              // If true, paint the alphabet in white and then set custom color for each particle
                doClearBuffer = myUtils.randomChance(rand, 1, 2);
                bgrMode = rand.Next(2);

                // Size
                switch (rand.Next(4))
                {
                    case 0: size = rand.Next(40) + 20; break;
                    case 1: size = rand.Next(60) + 20; break;
                    case 2: size = rand.Next(80) + 20; break;
                    case 3: size = rand.Next(99) + 20; break;
                }

                renderDelay = 0;
            }

            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time local initialization
        private void initLocal()
        {
            doChangeSymbols = myUtils.randomChance(rand, 1, 3);
            doUseRandomX = myUtils.randomChance(rand, 1, 4);

            sizeFactorMode = rand.Next(6);              // How the size is changed
            rowSizeMode = rand.Next(6);                 // Mode for the number of symbols in a single row

            maxSpeed = 3 + rand.Next(13);               // max falling speed
            angleMode = rand.Next(4);                   // symbol rotation mode
            angleMode = 0;
            drawMode = rand.Next(6);                    // drawing mode

            modX = rand.Next(333) + 11;

            do
            {
                sR = myUtils.randFloat(rand);
                sG = myUtils.randFloat(rand);
                sB = myUtils.randFloat(rand);
            }
            while (sR + sG + sB < 1.0f);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            int nSymbols = 0;
            height = 600;

            for (int i = 0; i < list.Count; i++)
            {
                nSymbols += (list[i] as myObj_0541)._symbols.Count;
            }

            string str = $"Obj = {Type}\n\n"                        +
                            $"N = {myUtils.nStr(list.Count)}"       +
                            $" of {myUtils.nStr(N)}\n"              +
                            $"nSymbols = {nSymbols}\n"              +
                            $"doClearBuffer = {doClearBuffer}\n"    +
                            $"bgrMode = {bgrMode}\n"                +
                            $"renderDelay = {renderDelay}\n"        +
                            $"font = '{tTex.FontFamily()}'\n"       +
                            $"size = {size}\n"                      +
                            $"sizeFactorMode = {sizeFactorMode}\n"  +
                            $"rowSizeMode = {rowSizeMode}\n"        +
                            $"doUseCustomRGB = {doUseCustomRGB}\n"  +
                            $"drawMode = {drawMode}\n"              +
                            $"maxSpeed = {maxSpeed}\n"              +
                            $"angleMode = {angleMode}\n"            +
                            $"file: {colorPicker.GetFileName()}"
                ;
            return str;
        }

        // ---------------------------------------------------------------------------------------------------------------

        // 
        protected override void setNextMode()
        {
            initLocal();

            System.Threading.Thread.Sleep(333);
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void generateNew()
        {
            x = rand.Next(gl_Width);
            y = -133;

            A = getOpacity(0.25f, 11);

            // Size factor (should only reduce the symbols, as enlarging makes them pixelated)
            switch (sizeFactorMode)
            {
                // Const size factor:
                case 0:
                    sizeFactor = 0.5f + A;                                  // ver1
                    break;

                // Const size factor:
                case 1:
                    sizeFactor = 0.25f + A;
                    sizeFactor = sizeFactor > 1.0f ? 1.0f : sizeFactor;     // ver2
                    break;

                // Const size factor:
                case 2:
                    sizeFactor = A > 0.25f ? A : A + 0.25f;                 // ver3
                    break;

                // Size factor depends on the actual onscreen size of symbols;
                // Different fonts will produce symbols of different actual onscreen size;
                // In this mode, the size is scaled down in case the symbols are larger than allowed
                default:
                    {
                        int maxSize = 33;
                        sizeFactor = tTex.getFieldHeight() > maxSize
                            ? 1.0f * maxSize / tTex.getFieldHeight()
                            : 1.0f;
                    }
                    break;
            }

            colorPicker.getColorRand(ref R, ref G, ref B);

            dy = myUtils.randFloat(rand, 0.1f) * (rand.Next(maxSpeed) + 1);

            // Distance between 2 neighbouring symbols in a row
            yOffset = (int)(sizeFactor * tTex.getFieldHeight());

            // Set up letter rotation
/*
            {
                angle = 0;
                dAngle = myUtils.randFloat(rand) * 0.001f * myUtils.randomSign(rand);

                if (angleMode == 2)
                    dAngle *= rand.Next(5) + 1;

                if (angleMode == 3)
                    dAngle *= rand.Next(13) + 10;
            }*/

            // Create/clear the list and insert a single item into it
            {
                if (_symbols == null)
                {
                    _symbols = new List<symbolItem>();
                }
                else
                {
                    _symbols.Clear();
                }

                lastIndex = 0;
                deadCnt = 0;
                _symbols.Add(new symbolItem(x, y, A, sizeFactor));
            }

            // Total number of characters generated before the object dies
            switch (rowSizeMode)
            {
                case 0:
                case 1:
                case 2:
                    cnt = 10 + rand.Next(50);
                    break;

                case 3:
                    cnt = 10 + rand.Next(25);
                    break;

                case 4:
                    cnt = 20 + rand.Next(33);
                    break;

                case 5:
                    cnt = 30 + rand.Next(33);
                    break;
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            int Count = _symbols.Count;

            for (int i = 0; i < Count; i++)
            {
                symbolItem item = _symbols[i];

                if (!item.isDead)
                {
                    item.y += dy;

                    if (doChangeSymbols && myUtils.randomChance(rand, 1, 666))
                    {
                        item.getRandomSymbol(x, sizeFactor);
                    }

                    // Insert new or reuse dead items
                    if (i == lastIndex && cnt > 0 && item.y - y > yOffset)
                    {
                        cnt--;

                        if (deadCnt == 0)
                        {
                            lastIndex = Count;
                            _symbols.Add(new symbolItem(x, y, A, sizeFactor));
                        }
                        else
                        {
                            for (int j = 0; j < Count; j++)
                            {
                                if (_symbols[j].isDead)
                                {
                                    _symbols[j].updateItem(x, y, A, sizeFactor);

                                    lastIndex = j;
                                    deadCnt--;
                                    break;
                                }
                            }
                        }
                    }

                    // Mark as dead
                    if (item.y > gl_Height)
                    {
                        deadCnt++;
                        item.isDead = true;
                    }
                }
            }

            if (deadCnt == Count)
            {
                generateNew();
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Show()
        {
            int Count = _symbols.Count;

            bool doPickColor = myUtils.randomChance(rand, 1, 11);

            for (int i = 0; i < Count; i++)
            {
                symbolItem item = _symbols[i];

                if (!item.isDead)
                {
                    switch (drawMode)
                    {
                        case 0:
                            tTex.Draw(item.x, item.y, item.index, sizeFactor, sR, sG, sB, A);
                            break;

                        case 1:
                            tTex.Draw(item.x, item.y, item.index, sizeFactor, 1, 1, 1, item.a);
                            break;

                        case 2:
                            tTex.Draw(item.x, item.y, item.index, sizeFactor, R, G, B, A);
                            break;

                        case 3:
                            tTex.Draw(item.x, item.y, item.index, sizeFactor, R, G, B, item.a);
                            break;

                        // Separate particles will pick their bgr color with some probability
                        case 4:
                            {
                                if (rand.Next(11) == 1)
                                    colorPicker.getColor(item.x, item.y, ref item.r, ref item.g, ref item.b);

                                tTex.Draw(item.x, item.y, item.index, sizeFactor, item.r, item.g, item.b, item.a);
                            }
                            break;

                        // The whole row will pick bgr color with some probability
                        case 5:
                            {
                                if (doPickColor)
                                    colorPicker.getColor(item.x, item.y, ref item.r, ref item.g, ref item.b);

                                tTex.Draw(item.x, item.y, item.index, sizeFactor, item.r, item.g, item.b, item.a);
                            }
                            break;
                    }
                }
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Process(Window window)
        {
            uint cnt = 0;
            initShapes();

            clearScreenSetup(doClearBuffer, 0.2f);

            while (!Glfw.WindowShouldClose(window))
            {
                int Count = list.Count;

                processInput(window);

                // Swap fore/back framebuffers, and poll for operating system events.
                Glfw.SwapBuffers(window);
                Glfw.PollEvents();

                // Dim screen
                if (doClearBuffer)
                {
                    glClear(GL_COLOR_BUFFER_BIT);

                    switch (bgrMode)
                    {
                        case 0:
                            bgrTex.Draw(0, 0, gl_Width, gl_Height);
                            break;

                        case 1:
                            grad.Draw();
                            break;
                    }
                }
                else
                {
                    dimScreen(0.5f);
                }

                // Render Frame
                {
                    tTex.getTexInst().ResetBuffer();

                    for (int i = 0; i != Count; i++)
                    {
                        var obj = list[i] as myObj_0541;

                        obj.Show();
                        obj.Move();
                    }

                    tTex.getTexInst().Draw();
                }

                if (Count < N)
                {
                    list.Add(new myObj_0541());
                }

                cnt++;
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void initShapes()
        {
            myPrimitive.init_ScrDimmer();

            // Normal vs Bold font
            int fontStyle = rand.Next(2);

            TexText.setScrDimensions(gl_Width, gl_Height);
            tTex = new TexText(size, doUseCustomRGB, 150000, fontStyle, -5);

            bgrTex = new myTexRectangle(colorPicker.getImg());

            grad = new myScreenGradient();
            grad.SetRandomColors(rand, 0.2f);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        public static float getOpacity(float Base, int Chance)
        {
            // Base opacity is 0.5f at most;
            float op = myUtils.randFloat(rand) * Base;

            // Then, for some particles we increase it
            if (myUtils.randomChance(rand, 1, Chance))
                op += myUtils.randFloat(rand) * (1.0f - Base);

            return op;
        }

        // ---------------------------------------------------------------------------------------------------------------

    }
};
