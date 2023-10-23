using GLFW;
using static OpenGL.GL;
using System.Collections.Generic;


/*
    - Falling alphabet letters (Matrix style), ver2
*/


namespace my
{
    public class myObj_541 : myObject
    {
        class symbolItem
        {
            public int index;
            public float x, y;

            public symbolItem(float X, float Y)
            {
                index = rand.Next(tTex.Lengh());
                x = X - tTex.getFieldWidth(index) / 2;
                y = Y;
            }

            public void getRandomSymbol(float X)
            {
                index = rand.Next(tTex.Lengh());
                x = X - tTex.getFieldWidth(index) / 2;
            }
        };

        // ---------------------------------------------------------------------------------------------------------------

        // Priority
        public static int Priority => 10;
        public static System.Type Type => typeof(myObj_541);

        private int yDist = 0, cnt, deadIndex;
        private float x, y, dy, angle, dAngle, sizeFactor;
        private float A, R, G, B;

        private List<symbolItem> _symbols = null;

        private static int N = 0;
        private static bool doUseRGB = false;

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
            //colorPicker = new myColorPicker(gl_Width, gl_Height, mode: myColorPicker.colorMode.SNAPSHOT_OR_IMAGE);
            colorPicker = new myColorPicker(gl_Width, gl_Height, mode: myColorPicker.colorMode.SNAPSHOT);
            list = new List<myObject>();

            // Global unmutable constants
            {
                N = 7;
                N = 333;

                // If true, paint alphabet in white and then set custom color for each particle
                doUseRGB = myUtils.randomChance(rand, 1, 2);

                // todo: fix and test this
                // When true, the screen will flicker because TexText inherits myTexRectangle, which has static color components;
                // We change color of a symbol, and it also changes the color of a bgrTex
                doUseRGB = false;

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
            doClearBuffer = myUtils.randomChance(rand, 1, 2);
            doClearBuffer = true;

            maxSpeed = 3 + rand.Next(13);               // max falling speed
            posXGenMode = rand.Next(3);                 // where the particles are generated along the X-axis
            posYGenMode = rand.Next(3);                 // where the particles are generated along the Y-axis
            angleMode = rand.Next(4);                   // symbol rotation mode

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

            string str = $"Obj = {Type}\n\n"                           	  +
                            $"N = {nStr(list.Count)} of {nStr(N)}\n"      +
                            $"doClearBuffer = {doClearBuffer}\n"          +
                            $"renderDelay = {renderDelay}\n"              +
                            $"font = '{tTex.FontFamily()}'\n"             +
                            $"size = {size}\n"                            +
                            $"doUseRGB = {doUseRGB}\n"                    +
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
            if (_symbols == null)
            {
                deadIndex = -1;
                _symbols = new List<symbolItem>();
            }
            else
            {
                //_symbols.Clear();
            }

            x = rand.Next(gl_Width);
            y = -11;

            A = getOpacity(0.5f, 13);
            A = 0.1f;

            // Size factor (should only reduce the symbols, as enlarging makes them pixelated)
            {
                int maxSize = 33;

                sizeFactor = tTex.getFieldHeight() > maxSize
                    ? 1.0f * maxSize / tTex.getFieldHeight()
                    : 1.0f;
            }

            colorPicker.getColor(rand.Next(gl_Width), rand.Next(gl_Height), ref R, ref G, ref B);

            dy = myUtils.randFloat(rand, 0.1f) * (rand.Next(maxSpeed) + 1);

            yDist = (int)(sizeFactor * tTex.getFieldHeight() + 3);

            // Set up letter rotation
            {
                angle = 0;
                dAngle = myUtils.randFloat(rand) * 0.001f * myUtils.randomSign(rand);

                if (angleMode == 2)
                    dAngle *= rand.Next(5) + 1;

                if (angleMode == 3)
                    dAngle *= rand.Next(13) + 10;

            }

            _symbols.Add(new symbolItem(x, y));

            // Total number of characters generated before the object dies
            cnt = 10 + rand.Next(100);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        // todo: removing items is very expensive
        // need to work without 
        protected override void Move()
        {
            int Count = _symbols.Count;

            for (int i = 0; i < Count; i++)
            {
                symbolItem item = _symbols[i];
                item.y += dy;

                if (myUtils.randomChance(rand, 1, 111))
                {
                    item.getRandomSymbol(x);
                }

                if (i == Count - 1 && cnt > 0 && item.y - y > yDist)
                {
                    _symbols.Add(new symbolItem(x, y));
                    cnt--;
                }

                // Remove items that we can't see anymore
                if (item.y > gl_Height)
                {
                    break;
                    _symbols.RemoveAt(i);
                    Count--;
                }
            }

            if (Count == 0)
            {
                generateNew();
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Show()
        {
            int Count = _symbols.Count;

            for (int i = 0; i < Count; i++)
            {
                symbolItem item = _symbols[i];

                if (item.y > gl_Height)
                    break;

                if (doUseRGB)
                {
                    //colorPicker.getColor(item.x, item.y, ref R, ref G, ref B);
                    tTex.Draw(item.x, item.y, item.index, A, angle, sizeFactor, R, G, B);
                }
                else
                {
                    tTex.Draw(item.x, item.y, item.index, A, angle, sizeFactor);
                }
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
                        bgrTex.Draw(0, 0, gl_Width, gl_Height);
                    }
                    else
                    {
                        grad.Draw();
                    }
                }

                // Render Frame
                {
                    for (int i = 0; i != Count; i++)
                    {
                        var obj = list[i] as myObj_541;

                        obj.Show();
                        obj.Move();
                    }
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
            tTex = new TexText(size, doUseRGB, 5);

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
