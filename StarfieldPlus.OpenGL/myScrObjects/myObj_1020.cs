using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;


/*
    - ...
*/


namespace my
{
    public class myObj_1020 : myObject
    {
        // Priority
        public static int Priority => 999910;
		public static System.Type Type => typeof(myObj_1020);

        private float x, y, Rad;
        private float size, R, G, B;

        private List<child> _children = null;

        private static int N = 0, nChildren = 10, shape = 0, childMoveMode = 0;
        private static bool doFillShapes = false;
        private static float dimAlpha = 0.05f;

        private static myScreenGradient grad = null;

        class child
        {
            public float x, y, r, x0, y0, angle, dAngle, a;
        };

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_1020()
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

                nChildren = 100;

                childMoveMode = rand.Next(3);

                shape = rand.Next(5);
            }

            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time local initialization
        private void initLocal()
        {
            doClearBuffer = myUtils.randomBool(rand);

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
            x = rand.Next(gl_Width);
            y = rand.Next(gl_Height);

            size = rand.Next(3) + 2;
            Rad = 100 + rand.Next(200);

            R = (float)rand.NextDouble();
            G = (float)rand.NextDouble();
            B = (float)rand.NextDouble();

            //colorPicker.getColor(x, y, ref R, ref G, ref B);

            if (_children == null)
            {
                _children = new List<child>();

                for (int i = 0; i < nChildren; i++)
                {
                    var obj = new child();

                    obj.angle  = myUtils.randFloat(rand) * 123;

                    switch (childMoveMode)
                    {
                        case 0:
                        case 1:
                            obj.dAngle = myUtils.randFloat(rand) * 0.25f * 0.1f;
                            break;

                        case 2:
                            obj.dAngle = myUtils.randomSign(rand) * 0.25f * 0.1f;
                            break;
                    }

                    obj.r = 33 + rand.Next(99);
                    obj.a = myUtils.randFloat(rand);

                    obj.x = x + (float)Math.Sin(obj.angle) * Rad;
                    obj.y = y + (float)Math.Cos(obj.angle) * Rad;

                    obj.x0 = obj.x;
                    obj.y0 = obj.y;

                    _children.Add(obj);
                }
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            for (int i = 0; i < _children.Count; i++)
            {
                var obj = _children[i];

                obj.angle += obj.dAngle;

                switch (childMoveMode)
                {
                    case 0:
                        obj.x = x + (float)Math.Sin(obj.angle) * Rad;
                        obj.y = y + (float)Math.Cos(obj.angle) * Rad;
                        break;

                    case 1:
                        obj.x = obj.x0 + (float)Math.Sin(obj.angle) * obj.r;
                        obj.y = obj.y0 + (float)Math.Cos(obj.angle) * obj.r;
                        break;

                    case 2:
                        obj.x = obj.x0 + (float)Math.Sin(obj.angle) * obj.r;
                        obj.y = obj.y0 + (float)Math.Cos(obj.angle) * obj.r;
                        break;
                }
            }

            if (x < 0 || x > gl_Width || y < 0 || y > gl_Height)
            {
                generateNew();
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Show()
        {
            float size2x = size * 2;

            for (int i = 0; i < _children.Count; i++)
            {
                var obj = _children[i];

                switch (shape)
                {
                    // Instanced squares
                    case 0:
                        myPrimitive._RectangleInst.setInstanceCoords(obj.x - size, obj.y - size, size2x, size2x);
                        myPrimitive._RectangleInst.setInstanceColor(R, G, B, obj.a);
                        myPrimitive._RectangleInst.setInstanceAngle(obj.angle);
                        break;

                    // Instanced triangles
                    case 1:
                        myPrimitive._TriangleInst.setInstanceCoords(obj.x, obj.y, size2x, obj.angle);
                        myPrimitive._TriangleInst.setInstanceColor(R, G, B, obj.a);
                        break;

                    // Instanced circles
                    case 2:
                        myPrimitive._EllipseInst.setInstanceCoords(obj.x, obj.y, size2x, obj.angle);
                        myPrimitive._EllipseInst.setInstanceColor(R, G, B, obj.a);
                        break;

                    // Instanced pentagons
                    case 3:
                        myPrimitive._PentagonInst.setInstanceCoords(obj.x, obj.y, size2x, obj.angle);
                        myPrimitive._PentagonInst.setInstanceColor(R, G, B, obj.a);
                        break;

                    // Instanced hexagons
                    case 4:
                        myPrimitive._HexagonInst.setInstanceCoords(obj.x, obj.y, size2x, obj.angle);
                        myPrimitive._HexagonInst.setInstanceColor(R, G, B, obj.a);
                        break;
                }
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Process(Window window)
        {
            uint cnt = 0;
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

                    for (int i = 0; i != Count; i++)
                    {
                        var obj = list[i] as myObj_1020;

                        obj.Show();
                        obj.Move();
                    }

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

                if (Count < N)
                {
                    list.Add(new myObj_1020());
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
            base.initShapes(shape, N * nChildren, 0);

            grad = new myScreenGradient();
            grad.SetRandomColors(rand, 0.2f);
            grad.SetOpacity(doClearBuffer ? 1 : 0.2f);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------
    }
};
