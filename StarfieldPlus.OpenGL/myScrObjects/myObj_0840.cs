using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Net;


/*
    - Trains moving across the screen
*/


namespace my
{
    public class myObj_0840 : myObject
    {
        // Priority
        public static int Priority => 10;
		public static System.Type Type => typeof(myObj_0840);

        private uint parentId;
        private int direction;
        private float x, y, dx, dy;
        private float size, A, R, G, B;
        private bool isAlive;

        private List<myObj_0840> train = null;

        private static int N = 0, n = 0, dirMode = 0, colorMode = 0, sizeMode = 0, cellSize = 1, cellGap = 1, deadCnt = 0, yOffset = 0;

        private static myScreenGradient grad = null;
        private static Dictionary<int, Tuple<float, float, float>> _colorMap;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_0840()
        {
            isAlive = false;
            parentId = uint.MaxValue;

            if (id != uint.MaxValue)
                generateNew();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time global initialization
        protected override void initGlobal()
        {
            colorPicker = new myColorPicker(gl_Width, gl_Height);
            list = new List<myObject>();
            _colorMap = new Dictionary<int, Tuple<float, float, float>>();

            // Global unmutable constants
            {
                cellSize = 10 + rand.Next(50);
                cellGap = 3;

                N = 333 + rand.Next(1111);
                n = gl_Height / (cellSize + cellGap);

                yOffset = (gl_Height - cellSize * n - cellGap * (n-1)) / 2;

                deadCnt = N - n;

                // Initialize color map
                {
                    int colorNum = 2 + rand.Next(5);

                    float r, g, b;

                    for (int i = 0; i < colorNum; i++)
                    {
                        do
                        {
                            r = myUtils.randFloat(rand);
                            g = myUtils.randFloat(rand);
                            b = myUtils.randFloat(rand);
                        }
                        while (r + g + b < 0.33f);

                        _colorMap[i] = new Tuple<float, float, float>(r, g, b);
                    }
                }
            }

            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time local initialization
        private void initLocal()
        {
            doClearBuffer = myUtils.randomBool(rand);
            doClearBuffer = true;

            dirMode = rand.Next(4);
            colorMode = rand.Next(3);
            sizeMode = rand.Next(3);

            renderDelay = rand.Next(2);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            height = 600;

            string nStr(int   n) { return n.ToString("N0");    }
            //string fStr(float f) { return f.ToString("0.000"); }

            string str = $"Obj = {Type}\n\n"                         +
                            $"N = {nStr(list.Count)} of {nStr(N)}\n" +
                            $"deadCnt = {deadCnt}\n"                 +
                            $"dirMode = {dirMode}\n"                 +
                            $"sizeMode = {sizeMode}\n"               +
                            $"colorMode = {colorMode}\n"             +
                            $"cellSize = {cellSize}\n"               +
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
            if (id < n)
            {
                if (train == null)
                {
                    train = new List<myObj_0840>();

                    x = 0;
                    y = id * (cellSize + cellGap * 2) - yOffset;
                    dy = size = 0;
                    dx = 0.25f + myUtils.randFloat(rand) * 7;

                    switch (dirMode)
                    {
                        case 0: direction = +1; break;
                        case 1: direction = -1; break;
                        case 2:
                        case 3:
                            direction = myUtils.randomSign(rand);
                            break;
                    }    
                }
            }
            else
            {
                x = -size;
                y = rand.Next(gl_Height);
                y -= y % (cellSize + cellGap);

                dx = 0.5f + myUtils.randFloat(rand) * 3;
                dy = 0;

                size = 100;

                A = 0.85f;
                colorPicker.getColorRand(ref R, ref G, ref B);
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            if (id < n)
            {
                if (train.Count == 0 ||
                    (direction > 0 && train[train.Count - 1].x > 100) ||
                    (direction < 0 && train[train.Count - 1].x < gl_Width - 100))
                {
                    if (myUtils.randomChance(rand, 1, 333))
                    {
                        int num = 3 + rand.Next(11);

                        if (num < deadCnt)
                        {
                            int idx = 0;
                            getColor();
                            size = 50 + rand.Next(100);

                            float aggregateLength = 0;

                            for (int i = n; idx != num && i < list.Count; i++)
                            {
                                var obj = list[i] as myObj_0840;

                                if (obj.isAlive == false)
                                {
                                    idx++;

                                    obj.isAlive = true;
                                    deadCnt--;
                                    obj.parentId = id;

                                    switch (sizeMode)
                                    {
                                        case 0:
                                        case 1:
                                            obj.size = size;
                                            break;

                                        case 2:
                                            obj.size = 50 + rand.Next(100);
                                            break;
                                    }

                                    if (direction > 0)
                                    {
                                        aggregateLength += obj.size + cellGap * 2;
                                        obj.x = -aggregateLength;
                                        obj.dx = +dx;
                                    }
                                    else
                                    {
                                        obj.x = gl_Width + aggregateLength;
                                        aggregateLength += obj.size + cellGap * 2;
                                        obj.dx = -dx;
                                    }

                                    obj.y = y;

                                    obj.R = R;
                                    obj.G = G;
                                    obj.B = B;

                                    train.Add(obj);
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                if (isAlive)
                {
                    x += dx;
                    y += dy;

                    if ((x > gl_Width && dx > 0) || (x < -size && dx < 0))
                    {
                        var parent = list[(int)parentId] as myObj_0840;
                        parent.train.RemoveAt(0);

                        isAlive = false;
                        parentId = uint.MaxValue;

                        deadCnt++;
                    }
                }
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Show()
        {
            if (id >= n && isAlive)
            {
                myPrimitive._Rectangle.SetColor(R, B, G, A);
                myPrimitive._Rectangle.Draw(x, y, size, cellSize, false);

                myPrimitive._Rectangle.SetColor(R, B, G, A / 2);
                myPrimitive._Rectangle.Draw(x + 2, y + 3, size - 5, cellSize - 5, true);
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Process(Window window)
        {
            uint cnt = 0;
            initShapes();

            clearScreenSetup(doClearBuffer, 0.1f);

            while (list.Count < N)
            {
                list.Add(new myObj_0840());
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
                    grad.Draw();
                }

                // Render Frame
                {
                    for (int i = 0; i != Count; i++)
                    {
                        var obj = list[i] as myObj_0840;

                        obj.Show();
                        obj.Move();
                    }
                }

                cnt++;
                System.Threading.Thread.Sleep(renderDelay);
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void initShapes()
        {
            myPrimitive.init_Rectangle();

            grad = new myScreenGradient();
            grad.SetRandomColors(rand, 0.2f);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void getColor()
        {
            switch (colorMode)
            {
                case 0:
                case 1:
                    colorPicker.getColorRand(ref R, ref G, ref B);
                    break;

                case 2:
                    {
                        var t = _colorMap[rand.Next(_colorMap.Count)];

                        R = t.Item1;
                        G = t.Item2;
                        B = t.Item3;
                    }
                    break;
            }
        }

        // ---------------------------------------------------------------------------------------------------------------
    }
};
