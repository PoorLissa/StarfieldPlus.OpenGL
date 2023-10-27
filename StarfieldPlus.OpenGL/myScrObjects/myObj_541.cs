using GLFW;
using static OpenGL.GL;
using System.Collections.Generic;
using System.Reflection;


/*
    - Falling alphabet letters (Matrix style), ver2
*/


namespace my
{
    public class myObj_541 : myObject
    {
        class symbolItem
        {
            public bool isDead;
            public int index;
            public float x, y, a;
            public float r, g, b;

            public symbolItem(float X, float Y, float A)
            {
                updateItem(X, Y, A);
            }

            public void updateItem(float X, float Y, float A)
            {
                index = rand.Next(tTex.Lengh());
                x = X - tTex.getFieldWidth(index) / 2;
                y = Y;
                a = A + myUtils.randFloatSigned(rand) * 0.15f;
                isDead = false;
            }

            public void getRandomSymbol(float X)
            {
                index = rand.Next(tTex.Lengh());
                x = X - tTex.getFieldWidth(index) / 2;
            }
        };

        // ---------------------------------------------------------------------------------------------------------------

        // Priority
        public static int Priority => 99910;
        public static System.Type Type => typeof(myObj_541);

        private int yDist = 0, cnt, deadCnt, lastIndex;
        private float x, y, dy, angle, dAngle, sizeFactor;
        private float A, R, G, B;

        private List<symbolItem> _symbols = null;

        private static int N = 0, drawMode = 0;
        private static bool doUseRGB = false, doChangeSymbols = false;

        private static int maxSpeed = 0, posXGenMode = 0, posYGenMode = 0, angleMode = 0, modX = 0, size = 20;

        private static TexText tTex = null;
        private static myScreenGradient grad = null;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_541()
        {
            if (id != uint.MaxValue)
                generateNew();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time global initialization
        protected override void initGlobal()
        {
            var mode = myColorPicker.colorMode.SNAPSHOT_OR_IMAGE;
            //mode = myColorPicker.colorMode.SNAPSHOT;

            colorPicker = new myColorPicker(gl_Width, gl_Height, mode: mode);
            list = new List<myObject>();

            // Global unmutable constants
            {
                N = 3333;

                // If true, paint alphabet in white and then set custom color for each particle
                doUseRGB = myUtils.randomChance(rand, 1, 2);

                // todo: fix and test this
                // When true, the screen will flicker because TexText inherits myTexRectangle, which has static color components;
                // We change color of a symbol, and it also changes the color of a bgrTex
                doUseRGB = true;

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
            doClearBuffer = true;
            doChangeSymbols = myUtils.randomChance(rand, 1, 3);

            maxSpeed = 3 + rand.Next(13);               // max falling speed
            posXGenMode = rand.Next(3);                 // where the particles are generated along the X-axis
            posYGenMode = rand.Next(3);                 // where the particles are generated along the Y-axis
            angleMode = rand.Next(4);                   // symbol rotation mode
            drawMode = rand.Next(5);                    // drawing mode

            modX = rand.Next(333) + 11;

            angleMode = 0;

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            height = 600;

            string nStr(int   n) { return n.ToString("N0");    }
            //string fStr(float f) { return f.ToString("0.000"); }

            int nSymbols = 0;

            for (int i = 0; i < list.Count; i++)
            {
                nSymbols += (list[i] as myObj_541)._symbols.Count;
            }

            string str = $"Obj = {Type}\n\n"                           	  +
                            $"N = {nStr(list.Count)} of {nStr(N)}\n"      +
                            $"nSymbols = {nSymbols}\n"                    +
                            $"doClearBuffer = {doClearBuffer}\n"          +
                            $"renderDelay = {renderDelay}\n"              +
                            $"font = '{tTex.FontFamily()}'\n"             +
                            $"size = {size}\n"                            +
                            $"doUseRGB = {doUseRGB}\n"                    +
                            $"drawMode = {drawMode}\n"                    +
                            $"maxSpeed = {maxSpeed}\n"                    +
                            $"xGenMode = {posXGenMode} (modX = {modX})\n" +
                            $"yGenMode = {posYGenMode}\n"                 +
                            $"angleMode = {angleMode}\n"                  +
                            $"file: {colorPicker.GetFileName()}"
                ;
            return str;
        }

        // ---------------------------------------------------------------------------------------------------------------

        // 
        protected override void setNextMode()
        {
            return;
            initLocal();
            clearScreenSetup(doClearBuffer, 0.2f);
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void generateNew()
        {
            x = rand.Next(gl_Width);
            y = -133;

            A = 0.1f;
            A = getOpacity(0.25f, 11);

            // Size factor (should only reduce the symbols, as enlarging makes them pixelated)
            {
                int maxSize = 33;

                sizeFactor = tTex.getFieldHeight() > maxSize
                    ? 1.0f * maxSize / tTex.getFieldHeight()
                    : 1.0f;

                sizeFactor = 0.5f + A;
            }

            colorPicker.getColorRand(ref R, ref G, ref B);

            dy = myUtils.randFloat(rand, 0.1f) * (rand.Next(maxSpeed) + 1);

            yDist = (int)(sizeFactor * tTex.getFieldHeight());

            // Set up letter rotation
            {
                angle = 0;
                dAngle = myUtils.randFloat(rand) * 0.001f * myUtils.randomSign(rand);

                if (angleMode == 2)
                    dAngle *= rand.Next(5) + 1;

                if (angleMode == 3)
                    dAngle *= rand.Next(13) + 10;

            }

            // Create/clear the list and insert one item into it
            {
                if (_symbols == null)
                {
                    lastIndex = 0;
                    deadCnt = 0;
                    _symbols = new List<symbolItem>();
                }
                else
                {
                    lastIndex = 0;
                    deadCnt = 0;
                    _symbols.Clear();
                }

                _symbols.Add(new symbolItem(x, y, A));
            }

            // Total number of characters generated before the object dies
            cnt = 10 + rand.Next(50);

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
                        item.getRandomSymbol(x);
                    }

                    // Insert new or reuse dead items
                    if (i == lastIndex && cnt > 0 && item.y - y > yDist)
                    {
                        cnt--;

                        if (deadCnt == 0)
                        {
                            lastIndex = Count;
                            _symbols.Add(new symbolItem(x, y, A));
                        }
                        else
                        {
                            for (int j = 0; j < Count; j++)
                            {
                                if (_symbols[j].isDead)
                                {
                                    _symbols[j].updateItem(x, y, A);

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

            float r = 0, g = 0, b = 0;

            for (int i = 0; i < Count; i++)
            {
                symbolItem item = _symbols[i];

                if (!item.isDead)
                {
                    switch (drawMode)
                    {
                        case 0:
                            tTex.Draw(item.x, item.y, item.index, sizeFactor, 1, 1, 1, A);
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

                        case 4:
                            {
                                if (rand.Next(11) == 1)
                                    colorPicker.getColor(item.x, item.y, ref item.r, ref item.g, ref item.b);

                                tTex.Draw(item.x, item.y, item.index, sizeFactor, item.r, item.g, item.b, item.a);
                            }
                            break;
                    }
                }

/*
                if (false)
                {
                    if (doUseRGB)
                    {
                        //colorPicker.getColor(item.x, item.y, ref R, ref G, ref B);
                        tTex.Draw(item.x, item.y, item.index, item.a, angle, sizeFactor, R, G, B);
                    }
                    else
                    {
                        tTex.Draw(item.x, item.y, item.index, item.a, angle, sizeFactor);
                    }
                }
*/
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Process(Window window)
        {
            uint cnt = 0;
            initShapes();

            var bgrTex = new myTexRectangle(colorPicker.getImg());

            clearScreenSetup(doClearBuffer, 0.2f);

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
                        //bgrTex.Draw(0, 0, gl_Width, gl_Height);
                        grad.Draw();
                    }
                    else
                    {
                        grad.Draw();
                    }
                }

                // Render Frame
                {
                    tTex.getTexInst().ResetBuffer();

                    for (int i = 0; i != Count; i++)
                    {
                        var obj = list[i] as myObj_541;

                        obj.Show();
                        obj.Move();
                    }

                    tTex.getTexInst().Draw();
                }

                if (Count < N)
                {
                    list.Add(new myObj_541());
                }

                cnt++;
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void initShapes()
        {
            TexText.setScrDimensions(gl_Width, gl_Height);
            tTex = new TexText(size, doUseRGB, 150000, -5);

            grad = new myScreenGradient();
            grad.SetRandomColors(rand, 0.2f, 0);

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
