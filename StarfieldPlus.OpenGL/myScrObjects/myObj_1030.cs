using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;


/*
    - Lightnings, take 2
*/


namespace my
{
    public class myObj_1030 : myObject
    {
        // Priority
        public static int Priority => 10;
		public static System.Type Type => typeof(myObj_1030);

        private int cnt;
        private float x, y;
        private float A, R, G, B;

        private List<child> _children = null;

        private static int N = 0, shape = 0, maxSize = 66, maxSpd = 1, drawMode = 0, moveMode = 0;
        private static bool doFillShapes = false;

        private static myScreenGradient grad = null;

        private class child
        {
            public float x, y, dy, size;
        }

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_1030()
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
                N = rand.Next(10) + 1;

                shape = rand.Next(5);

                maxSize = rand.Next(85) + 3;
                maxSpd  = rand.Next(7) + 1;
            }

            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time local initialization
        private void initLocal()
        {
            doClearBuffer = myUtils.randomBool(rand);

            renderDelay = rand.Next(3) + 1;

            drawMode = rand.Next(2);
            moveMode = rand.Next(2);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            height = 600;

            int n = 0;
            foreach (myObj_1030 obj in list)
                n += obj._children.Count;

            string str = $"Obj = {Type}\n\n"                     +
                            $"N = {myUtils.nStr(list.Count)}"    +
                            $" of {myUtils.nStr(N)}\n"           +
                            $"total children = {n}\n"            +
                            $"maxSize = {maxSize}\n"             +
                            $"maxSpd = {maxSpd}\n"               +
                            $"drawMode = {drawMode}\n"           +
                            $"moveMode = {moveMode}\n"           +
                            $"doClearBuffer = {doClearBuffer}\n" +
                            $"renderDelay = {renderDelay}\n"     +
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
            }
            while (R + G + B < 0.33f);

            if (_children == null)
            {
                _children = new List<child>();

                // 450 * 2 == 900, which is les than 1000 per object
                int n = 100 + rand.Next(350);

                x = 0;
                float dx = 1.0f * gl_Width / (n - 1);

                for (int i = 0; i < n; i++)
                {
                    var obj = new child();

                    obj.y = y;
                    obj.x = x + rand.Next(11) - 5;
                    obj.dy = (0.001f + myUtils.randFloat(rand) * 0.2f) * myUtils.randomSign(rand) * maxSpd;
                    obj.size = 10 + rand.Next(maxSize);

                    _children.Add(obj);
                    x += dx;

                    if (moveMode == 1 && i > 0 && i % 2 == 1)
                    {
                        var prev = _children[i-1];
                        prev.dy = obj.dy;
                        prev.size = obj.size;
                    }
                }
            }
            else
            {
                for (int i = 0; i < _children.Count; i++)
                {
                    var obj = _children[i];

                    obj.y = y;
                    obj.dy = (0.001f + myUtils.randFloat(rand) * 0.2f) * myUtils.randomSign(rand) * maxSpd;
                    obj.size = 10 + rand.Next(maxSize);

                    if (moveMode == 1 && i > 0 && i % 2 == 1)
                    {
                        var prev = _children[i - 1];
                        prev.dy = obj.dy;
                        prev.size = obj.size;
                    }
                }
            }

            cnt = 999 + rand.Next(999);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            if (--cnt < 0)
            {
                A -= myUtils.randFloat(rand) * 0.001f;
            }

            if (A < 0)
            {
                generateNew();
            }
            else
            {
                int Count = _children.Count - 1;

                for (int i = 1; i < Count; i++)
                {
                    var obj = _children[i];

                    obj.y += obj.dy;

                    if (obj.dy > 0)
                    {
                        if (obj.y >= y + obj.size)
                        {
                            obj.dy *= -1;
                        }
                    }
                    else
                    {
                        if (obj.y <= y - obj.size)
                        {
                            obj.dy *= -1;
                        }
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

                if (drawMode == 1 && i > 1)
                {
                    obj1 = _children[i - 2];
                    obj2 = _children[i - 0];

                    myPrimitive._LineInst.setInstanceCoords(obj1.x, obj1.y, obj2.x, obj2.y);

                    r = R + myUtils.randFloatSigned(rand) * 0.1f;
                    g = G + myUtils.randFloatSigned(rand) * 0.1f;
                    b = B + myUtils.randFloatSigned(rand) * 0.1f;

                    myPrimitive._LineInst.setInstanceColor(r, g, b, A);
                }
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
                        var obj = list[i] as myObj_1030;

                        obj.Show();
                        obj.Move();
                    }
#else

                    myPrimitive._LineInst.setLineWidth(7);

                    for (int i = 0; i != Count; i++)
                    {
                        var obj = list[i] as myObj_1030;

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
                        var obj = list[i] as myObj_1030;

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
                    list.Add(new myObj_1030());
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
