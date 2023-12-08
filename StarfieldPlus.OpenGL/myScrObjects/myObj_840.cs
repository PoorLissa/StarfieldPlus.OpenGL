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
    public class myObj_840 : myObject
    {
        // Priority
        public static int Priority => 999910;
		public static System.Type Type => typeof(myObj_840);

        private uint parentId;
        private float x, y, dx, dy;
        private float size, A, R, G, B;
        private bool isAlive;

        private List<myObj_840> train = null;

        private static int N = 0, n = 0, cellSize = 1, cellGap = 1;

        private static myScreenGradient grad = null;
        private static Dictionary<int, int> trainMap = null;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_840()
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
            trainMap = new Dictionary<int, int>();

            // Global unmutable constants
            {
                cellSize = 50;
                cellGap = 3;

                N = 100;
                n = gl_Height / (cellSize + cellGap);

                for (int i = 0; i < gl_Height / (cellSize + cellGap); i += cellSize + cellGap)
                {
                    trainMap[i] = 0;
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

            renderDelay = rand.Next(3) + 1;

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
                    train = new List<myObj_840>();

                    x = 0;
                    y = id * (cellSize + cellGap * 2);
                    dy = size = 0;
                    dx = 0.25f + myUtils.randFloat(rand) * 7;
                }
            }
            else
            {
                x = -size;
                y = rand.Next(gl_Height);
                y -= y % (cellSize + cellGap);

                dx = 0.25f + myUtils.randFloat(rand) * 3;
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
                if (train.Count == 0 || train[train.Count - 1].x > 100)
                {
                    if (myUtils.randomChance(rand, 1, 333))
                    {
                        int NUM = 3 + rand.Next(11);
                        int num = 0;

                        // Check if there is enough dead objects to create a new train of NUM
                        for (int i = n; i < list.Count; i++)
                        {
                            var obj = list[i] as myObj_840;

                            if (obj.isAlive == false)
                            {
                                if (++num == NUM)
                                    break;
                            }
                        }

                        if (num == NUM)
                        {
                            num = 0;

                            colorPicker.getColorRand(ref R, ref G, ref B);

                            for (int i = n; num != NUM && i < list.Count; i++)
                            {
                                var obj = list[i] as myObj_840;

                                if (obj.isAlive == false)
                                {
                                    num++;

                                    obj.isAlive = true;
                                    obj.parentId = id;

                                    obj.size = 100;

                                    obj.x = num * (-obj.size - cellGap * 2);
                                    obj.y = y;

                                    obj.dx = dx;

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

                    if (x > gl_Width && dx > 0)
                    {
                        var parent = list[(int)parentId] as myObj_840;
                        parent.train.RemoveAt(0);

                        isAlive = false;
                        parentId = uint.MaxValue;
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
                list.Add(new myObj_840());
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
                        var obj = list[i] as myObj_840;

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
    }
};
