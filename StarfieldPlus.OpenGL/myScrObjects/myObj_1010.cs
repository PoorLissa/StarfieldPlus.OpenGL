using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;


/*
    - Lightnings
*/


namespace my
{
    public class myObj_1010 : myObject
    {
        // Priority
        public static int Priority => 10;
		public static System.Type Type => typeof(myObj_1010);

        private float x, y;
        private float A, R, G, B;

        private List<child> _children = null;

        private static int N = 0, shape = 0, maxCnt = 11, mode = 0;
        private static bool doFillShapes = false;

        private static myScreenGradient grad = null;

        private class child
        {
            public int cnt, maxCnt;
            public float x, y;
        }

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_1010()
        {
            if (id != uint.MaxValue)
                generateNew();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time global initialization
        protected override void initGlobal()
        {
            colorPicker = new myColorPicker(gl_Width, gl_Height);
            list = new List<myObject>();

            // Global unmutable constants
            {
                N = rand.Next(10) + 10;
                N = 333;

                shape = rand.Next(5);
            }

            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time local initialization
        private void initLocal()
        {
            doClearBuffer = myUtils.randomBool(rand);

            mode = rand.Next(2);

            renderDelay = rand.Next(3) + 1;

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            height = 600;

            string nStr(int   n) { return n.ToString("N0");    }

            string str = $"Obj = {Type}\n\n"                         +
                            $"N = {nStr(list.Count)} of {nStr(N)}\n" +
                            $"doClearBuffer = {doClearBuffer}\n"     +
                            $"mode = {mode}\n"                       +
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
            y = rand.Next(gl_Height);

            do
            {
                A = myUtils.randFloat(rand);
                R = myUtils.randFloat(rand);
                G = myUtils.randFloat(rand);
                B = myUtils.randFloat(rand);
            } while (R + G + B < 0.33f);

            //colorPicker.getColor(x, y, ref R, ref G, ref B);

            if (_children == null)
            {
                _children = new List<child>();

                int n = 100 + rand.Next(500);

                x = 0;
                float dx = 1.0f * gl_Width / (n-1);

                for (int i = 0; i < n; i++)
                {
                    var obj = new child();

                    obj.x = x + rand.Next(11) - 5;

                    switch (mode)
                    {
                        case 0:
                            {
                                obj.y = y + (rand.Next(21) - 10) * 1.0f * y / gl_Height;
                            }
                            break;

                        case 1:
                            {
                                var aaa = (float)Math.Abs(obj.x - gl_x0);
                                obj.y = y + (rand.Next((int)aaa / 23)) + aaa / 33;
                            }
                            break;
                    }

                    obj.cnt = rand.Next(maxCnt) + 3;
                    obj.maxCnt = 11 + rand.Next(33);

                    _children.Add(obj);
                    x += dx;
                }
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            for (int i = 1; i < _children.Count - 1; i++)
            {
                var obj = _children[i];

                if (--obj.cnt == 0)
                {
                    obj.cnt = rand.Next(obj.maxCnt) + 3;

                    switch (mode)
                    {
                        case 0:
                            {
                                obj.y = y + (rand.Next(21) - 10) * 1.5f * y / gl_Height;
                            }
                            break;

                        case 1:
                            {
                                var aaa = (float)Math.Abs(obj.x - gl_x0);
                                obj.y = y + (rand.Next((int)aaa / 23)) + aaa / 33;
                            }
                            break;
                    }
                }
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Show()
        {
            for (int i = 1; i < _children.Count; i++)
            {
                var obj1 = _children[i - 1];
                var obj2 = _children[i - 0];

                myPrimitive._LineInst.setInstanceCoords(obj1.x, obj1.y, obj2.x, obj2.y);

                float r = R + myUtils.randFloatSigned(rand) * 0.1f;
                float g = G + myUtils.randFloatSigned(rand) * 0.1f;
                float b = B + myUtils.randFloatSigned(rand) * 0.1f;

                myPrimitive._LineInst.setInstanceColor(r, g, b, A);
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Process(Window window)
        {
            uint cnt = 50 + (uint)rand.Next(111);
            initShapes();

            clearScreenSetup(doClearBuffer, 0.1f);

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
                        glClear(GL_COLOR_BUFFER_BIT);

                    grad.Draw();
                }

                // Render Frame
                {
                    inst.ResetBuffer();
                    myPrimitive._LineInst.ResetBuffer();

#if false
                    for (int i = 0; i != Count; i++)
                    {
                        var obj = list[i] as myObj_1010;

                        obj.Show();
                        obj.Move();
                    }
#else

                    myPrimitive._LineInst.setLineWidth(7);

                    for (int i = 0; i != Count; i++)
                    {
                        var obj = list[i] as myObj_1010;

                        float op = obj.A;
                        obj.A = 0.05f + myUtils.randFloat(rand) * 0.05f;

                        obj.Show();
                        obj.A = op;
                    }

                    myPrimitive._LineInst.Draw();
                    myPrimitive._LineInst.ResetBuffer();
                    myPrimitive._LineInst.setLineWidth(1);

                    for (int i = 0; i != Count; i++)
                    {
                        var obj = list[i] as myObj_1010;

                        obj.Show();
                        obj.Move();
                    }
#endif
                    myPrimitive._LineInst.Draw();

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

                if (--cnt == 0 && Count < N)
                {
                    list.Add(new myObj_1010());
                    cnt = 50 + (uint)rand.Next(111);
                }

                System.Threading.Thread.Sleep(renderDelay);
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void initShapes()
        {
            myPrimitive.init_ScrDimmer();
            base.initShapes(shape, N, 0);

            grad = new myScreenGradient();
            grad.SetRandomColors(rand, 0.2f);
            grad.SetOpacity(doClearBuffer ? 1 : 0.2f);

            myPrimitive.init_LineInst(N * 1000);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------
    }
};
