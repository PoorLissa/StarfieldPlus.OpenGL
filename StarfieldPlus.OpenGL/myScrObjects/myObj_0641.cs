using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;


/*
    - Create random circles, but put them on the screen only when they don't intersect any existing circles
*/


namespace my
{
    public class myObj_0641 : myObject
    {
        // Priority
        public static int Priority => 23;
		public static System.Type Type => typeof(myObj_0641);

        private bool isFilled;
        private int cnt;
        private float x, y;
        private float size, A, AFill, R, G, B;

        private static int N = 0, maxSize = 1, minSize = 1, fillMode = 0, totalFailsNum = 0;
        private static float lineThickness = 1;
        private static bool doAllocateAtOnce = false;

        private static myScreenGradient grad = null;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_0641()
        {
            if (id != uint.MaxValue)
                generateNew();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time global initialization
        protected override void initGlobal()
        {
            var mode = myUtils.randomChance(rand, 1, 2)
                ? myColorPicker.colorMode.SNAPSHOT_OR_IMAGE
                : myColorPicker.colorMode.RANDOM_MODE;

            colorPicker = new myColorPicker(gl_Width, gl_Height, mode);
            list = new List<myObject>();

            // Global unmutable constants
            {
                N = 3333;

                // For larger thicknesses, we need to adjust the size, as circles will start intersecting each other
                lineThickness = 3 + rand.Next(9);
            }

            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time local initialization
        private void initLocal()
        {
            doClearBuffer = true;
            doAllocateAtOnce = myUtils.randomChance(rand, 1, 2);

            fillMode = rand.Next(7);

            renderDelay = rand.Next(3);

            minSize = 3;
            maxSize = 111 + rand.Next(123);

            totalFailsNum = 0;

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            height = 600;

            string nStr(int   n) { return n.ToString("N0");    }
            string fStr(float f) { return f.ToString("0.000"); }

            string str = $"Obj = {Type}\n\n"                           +
                            $"N = {nStr(list.Count)} of {nStr(N)}\n"   +
                            $"doAllocateAtOnce = {doAllocateAtOnce}\n" +
                            $"maxSize = {maxSize}\n"                   +
                            $"lineThickness = {fStr(lineThickness)}\n" +
                            $"fillMode = {fillMode}\n"                 +
                            $"totalFailsNum = {totalFailsNum}\n"       +
                            $"renderDelay = {renderDelay}\n"           +
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
            cnt = 666 + rand.Next(333);

            A = 0.75f;

            switch (fillMode)
            {
                case 0:
                case 1:
                    isFilled = false;
                    break;

                case 2:
                    isFilled = true;
                    AFill = A * 0.50f;
                    break;

                case 3:
                    isFilled = true;
                    AFill = A * 0.25f;
                    break;

                case 4:
                    isFilled = true;
                    AFill = A * 0.10f;
                    break;

                case 5:
                    isFilled = myUtils.randomChance(rand, 1, 2);
                    AFill = A * 0.50f;
                    break;

                case 6:
                    isFilled = myUtils.randomChance(rand, 1, 2);
                    AFill = A * myUtils.randFloat(rand) * 0.5f;
                    break;
            }

            // Must be true to continue: dist < (r1 + r2)
            // todo:
            //  use sorted list;
            //  in this case we just need to check all the particles on the interval [-(maxRadius + thisRadius), +(maxRadius + thisRadius)]
            {
                int numOfTries = 100;
                int Count = list.Count;
                bool isOk = false;

                while (isOk == false)
                {
                    isOk = true;

                    x = rand.Next(gl_Width);
                    y = rand.Next(gl_Height);

                    size = maxSize;

                    for (int i = 0; isOk && i < Count; i++)
                    {
                        var obj = list[i] as myObj_0641;

                        if (id != obj.id)
                        {
                            float dx = x - obj.x;
                            float dy = y - obj.y;
                            float distSquared = (dx * dx + dy * dy);
                            float rSquared = size + obj.size;
                                 rSquared *= rSquared;

                            if (distSquared < rSquared)
                            {
                                if (distSquared < obj.size * obj.size)
                                {
                                    isOk = false;
                                    break;
                                }
                                else
                                {
                                    float newSize = (float)Math.Sqrt(distSquared) - obj.size;

                                    // The size only is allowed to reduce
                                    size = newSize < size ? newSize : size;

                                    //size += isFilled ? 1 : -2;
                                    size -= lineThickness * 2;

                                    if (size < minSize)
                                    {
                                        isOk = false;
                                        break;
                                    }
                                }
                            }
                        }
                    }

                    // Don't spend an eternity here; try again next time
                    if (--numOfTries == 0)
                    {
                        totalFailsNum++;
                        x = y = -1111;
                        size = 0;
                        return;
                    }
                }
            }

            colorPicker.getColor(x, y, ref R, ref G, ref B);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            if (--cnt < 0)
            {
                A -= 0.005f;
                AFill -= 0.005f;

                if (A < 0)
                {
                    generateNew();
                }
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Show()
        {
            float size2x = size * 2;

            if (isFilled)
            {
                myPrimitive._Ellipse.SetColor(R, G, B, AFill);
                myPrimitive._Ellipse.Draw(x - size, y - size, size2x, size2x, true);

                myPrimitive._Ellipse.SetColor(R, G, B, A);
                myPrimitive._Ellipse.Draw(x - size, y - size, size2x, size2x, false);
            }
            else
            {
                myPrimitive._Ellipse.SetColor(R, G, B, A);
                myPrimitive._Ellipse.Draw(x - size, y - size, size2x, size2x, false);
            }
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Process(Window window)
        {
            uint cnt = 0;
            initShapes();


            clearScreenSetup(doClearBuffer, 0.1f);


            if (doAllocateAtOnce)
            {
                int n = rand.Next(N);
                while (list.Count < n)
                    list.Add(new myObj_0641());
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
                    for (int i = 0; i != Count; i++)
                    {
                        var obj = list[i] as myObj_0641;

                        obj.Show();
                        obj.Move();
                    }
                }

                //if (cnt == 1 && Count < N)
                if (Count < N)
                {
                    //cnt = 0;
                    list.Add(new myObj_0641());
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
            myPrimitive.init_Ellipse();

            myPrimitive._Ellipse.setLineThickness(lineThickness);

            grad = new myScreenGradient();
            grad.SetRandomColors(rand, 0.2f);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------
    }
};
