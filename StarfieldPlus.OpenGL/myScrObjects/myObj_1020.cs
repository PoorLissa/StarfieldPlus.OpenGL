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
        public static int Priority => 10;
		public static System.Type Type => typeof(myObj_1020);

        private float x, y, Rad;
        private float size, R, G, B, angle, dAngle;

        private List<child> _children = null;

        private static int N = 0, nChildren = 10, shape = 0, childMoveMode = 0, startAngle = 0, maxRad = 33;
        private static int option_i0 = 0, option_i1 = 0;
        private static bool doFillShapes = false;

        private static myScreenGradient grad = null;

        class child
        {
            public float x, y, r, x0, y0, angle, dAngle, A, R, G, B, f1, f2;
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
                N = rand.Next(100) + 100;

                nChildren = 100;

                childMoveMode = rand.Next(5);
                //childMoveMode = 4;

                shape = rand.Next(5);

                startAngle = myUtils.randomBool(rand) ? 1 : 0;

                switch (rand.Next(5))
                {
                    case 0: maxRad = rand.Next( 2); break;
                    case 1: maxRad = rand.Next(11); break;
                    case 2: maxRad = rand.Next(20); break;
                    case 3: maxRad = rand.Next(30); break;
                    case 4: maxRad = rand.Next(50); break;
                }

                switch (childMoveMode)
                {
                    case 0:
                        option_i0 = rand.Next(2);
                        option_i1 = rand.Next(3);
                        break;

                    case 1:
                        option_i0 = rand.Next(3);
                        break;

                    case 2:
                        option_i0 = rand.Next(5);
                        option_i1 = rand.Next(2);
                        break;

                    case 3:
                        option_i0 = rand.Next(33) + 1;
                        option_i1 = rand.Next(33) + 1;
                        break;

                    case 4:
                        maxRad = rand.Next(50) + 10;
                        option_i0 = rand.Next(3);           // 2 modes (0 vs 1-2)
                        option_i1 = rand.Next(9) + 3;       // number of different dAngles in the 2nd mode
                        break;
                }
            }

            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time local initialization
        private void initLocal()
        {
            doClearBuffer = myUtils.randomBool(rand);
            doFillShapes = myUtils.randomBool(rand);

            renderDelay = rand.Next(3) + 1;

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            height = 600;

            string str = $"Obj = {Type}\n\n"                        +
                            $"N = {myUtils.nStr(list.Count)}"       +
                            $" of {myUtils.nStr(N)}\n"              +
                            $"nChildren = {nChildren}\n"            +
                            $"total particles = {N * nChildren}\n"  +
                            $"childMoveMode = {childMoveMode}\n"    +
                            $"option_i0 = {option_i0}\n"            +
                            $"option_i1 = {option_i1}\n"            +
                            $"maxRad = {maxRad}\n"                  +
                            $"renderDelay = {renderDelay}\n"        +
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

            angle = myUtils.randFloat(rand) * 123;
            dAngle = myUtils.randFloatSigned(rand) * 0.2f;

            R = (float)rand.NextDouble();
            G = (float)rand.NextDouble();
            B = (float)rand.NextDouble();

            float child_dAngle = 0.01f + myUtils.randFloat(rand) * 0.025f;

            //colorPicker.getColor(x, y, ref R, ref G, ref B);

            if (_children == null)
            {
                _children = new List<child>();

                for (int i = 0; i < nChildren; i++)
                {
                    var obj = new child();

                    obj.angle = myUtils.randFloat(rand) * 123;

                    obj.A = myUtils.randFloat(rand);
                    obj.R = R + myUtils.randFloatSigned(rand) * 0.1f;
                    obj.G = G + myUtils.randFloatSigned(rand) * 0.1f;
                    obj.B = B + myUtils.randFloatSigned(rand) * 0.1f;

                    switch (childMoveMode)
                    {
                        case 0:
                            {
                                obj.angle = startAngle != 0 ? angle : obj.angle;
                                obj.dAngle = 0.025f;

                                switch (option_i0)
                                {
                                    case 0:
                                        obj.r = Rad + myUtils.randFloatSigned(rand) * maxRad;
                                        break;

                                    case 1:
                                        obj.r = Rad + (1 - obj.A) * maxRad;
                                        break;
                                }

                                switch (option_i1)
                                {
                                    case 0: obj.dAngle *= +1.0f * myUtils.randFloat(rand); break;
                                    case 1: obj.dAngle *= -1.0f * myUtils.randFloat(rand); break;
                                    case 2: obj.dAngle *= +1.0f * myUtils.randFloatSigned(rand);  break;
                                }
                            }
                            break;

                        // dAngle different per particle
                        case 1:
                            {
                                obj.dAngle = myUtils.randFloat(rand) * 0.025f;

                                switch (option_i0)
                                {
                                    case 0:
                                        obj.r = 33 + rand.Next(33);
                                        break;

                                    case 1:
                                        obj.r = 33 + rand.Next(maxRad);
                                        break;

                                    case 2:
                                        obj.r = maxRad + rand.Next(maxRad);
                                        break;
                                }
                            }
                            break;

                        // dAngle the same for all particles (sign might be different)
                        case 2:
                            {
                                switch (option_i0)
                                {
                                    case 0:
                                        obj.dAngle = 0.025f;
                                        break;

                                    case 1:
                                        obj.dAngle = 0.025f * myUtils.randomSign(rand);
                                        break;

                                    case 2:
                                        obj.dAngle = 0.0002f * (id + 1);
                                        break;

                                    case 3:
                                        obj.dAngle = 0.0002f * (id + 1) * myUtils.randomSign(rand);
                                        break;

                                    case 4:
                                        obj.dAngle = child_dAngle;
                                        break;
                                }

                                switch (option_i1)
                                {
                                    case 0:
                                        obj.r = 33 + rand.Next(99);
                                        break;

                                    case 1:
                                        obj.r = 33 + rand.Next(maxRad);
                                        break;
                                }
                            }
                            break;

                        case 3:
                            {
                                obj.dAngle = 0.025f;
                                obj.r = 33 + rand.Next(11);
                            }
                            break;

                        // Radial motion, no rotation
                        case 4:
                            {
                                switch (option_i0)
                                {
                                    case 0:
                                        obj.dAngle = 0.01f + myUtils.randFloat(rand) * 0.033f;
                                        break;

                                    case 1:
                                    case 2:
                                        obj.dAngle = 0.01f * (rand.Next(option_i1) + 1);
                                        break;
                                }

                                obj.r = obj.angle;
                                obj.f1 = (float)Math.Sin(obj.angle);    // Pre-calc sin
                                obj.f2 = (float)Math.Cos(obj.angle);    // Pre-calc cos
                            }
                            break;
                    }

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
            angle += dAngle;

            int n = _children.Count / 4;

            for (int i = 0; i < _children.Count; i++)
            {
                var obj = _children[i];

                obj.angle += obj.dAngle;

                switch (childMoveMode)
                {
                    case 0:
                        obj.x = x + (float)Math.Sin(obj.angle) * obj.r;
                        obj.y = y + (float)Math.Cos(obj.angle) * obj.r;
                        //obj.r += (float)Math.Sin(obj.angle);
                        break;

                    case 1:
                        obj.x = obj.x0 + (float)Math.Sin(obj.angle) * obj.r;
                        obj.y = obj.y0 + (float)Math.Cos(obj.angle) * obj.r;
                        break;

                    case 2:
                        obj.x = obj.x0 + (float)Math.Sin(obj.angle) * obj.r;
                        obj.y = obj.y0 + (float)Math.Cos(obj.angle) * obj.r;
                        break;

                    case 3:
                        obj.x = x + (float)Math.Sin(obj.angle) * Rad + (float)Math.Sin(obj.angle / option_i0) * obj.r;
                        obj.y = y + (float)Math.Cos(obj.angle) * Rad + (float)Math.Cos(obj.angle / option_i1) * obj.r;
                        break;

                    case 4:
                        obj.x = x + (float)(obj.f1 * (Rad + Math.Sin(obj.angle) * maxRad));
                        obj.y = y + (float)(obj.f2 * (Rad + Math.Sin(obj.angle) * maxRad));
                        break;

                    case 100:
                        obj.x = x + (float)Math.Sin(obj.angle) * (Rad + (float)(Math.Sin(obj.angle) * 33 + 11 * Math.Sin(obj.angle * 15)));
                        obj.y = y + (float)Math.Cos(obj.angle) * (Rad + (float)(Math.Cos(obj.angle) * 33 + 11 * Math.Cos(obj.angle * 15)));
                        break;

                    case 101:
                        obj.x = x + (float)(Math.Sin(obj.angle) * (Rad + 13 * Math.Sin(obj.angle * 6 + angle * 0.23)));
                        obj.y = y + (float)(Math.Cos(obj.angle) * (Rad + 11 * Math.Cos(obj.angle * 5 + angle * 0.27)));
                        //obj.x = x + (float)Math.Sin(obj.angle) * (Rad + (float)(Math.Sin(obj.angle) * 33 + 11 * Math.Sin(angle * 0.25)));
                        //obj.y = y + (float)Math.Cos(obj.angle) * (Rad + (float)(Math.Cos(obj.angle) * 33 + 13 * Math.Sin(angle * 0.25)));
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
                        myPrimitive._RectangleInst.setInstanceColor(obj.R, obj.G, obj.B, obj.A);
                        myPrimitive._RectangleInst.setInstanceAngle(obj.angle);
                        break;

                    // Instanced triangles
                    case 1:
                        myPrimitive._TriangleInst.setInstanceCoords(obj.x, obj.y, size, obj.angle);
                        myPrimitive._TriangleInst.setInstanceColor(obj.R, obj.G, obj.B, obj.A);
                        break;

                    // Instanced circles
                    case 2:
                        myPrimitive._EllipseInst.setInstanceCoords(obj.x, obj.y, size2x, obj.angle);
                        myPrimitive._EllipseInst.setInstanceColor(obj.R, obj.G, obj.B, obj.A);
                        break;

                    // Instanced pentagons
                    case 3:
                        myPrimitive._PentagonInst.setInstanceCoords(obj.x, obj.y, size2x, obj.angle);
                        myPrimitive._PentagonInst.setInstanceColor(obj.R, obj.G, obj.B, obj.A);
                        break;

                    // Instanced hexagons
                    case 4:
                        myPrimitive._HexagonInst.setInstanceCoords(obj.x, obj.y, size2x, obj.angle);
                        myPrimitive._HexagonInst.setInstanceColor(obj.R, obj.G, obj.B, obj.A);
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
