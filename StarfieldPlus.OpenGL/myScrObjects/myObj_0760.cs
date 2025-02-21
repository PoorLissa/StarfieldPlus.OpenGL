using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;
using System.Drawing.Text;
using System.Reflection;


/*
    - Points randomly travelling over a graph
*/


namespace my
{
    public class myObj_0760 : myObject
    {
        private class findData {
            public int index;
            public float dist;
        };

        // ---------------------------------------------------------------------------------------------------------------

        // Priority
        public static int Priority => 23;
		public static System.Type Type => typeof(myObj_0760);

        private int state;
        private uint targetId, startId;
        private float x, y, dx, dy;
        private float size, A, R, G, B, angle = 0, dAngle;

        private List<myObj_0760> neighbours = null;
        private myParticleTrail trail = null;

        private static int N = 0, n = 0, shape = 0, nTrail = 0;
        private static bool doFillShapes = false;
        private static float dimAlpha = 0.05f;

        private static myScreenGradient grad = null;
        private static myFreeShader shader = null;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_0760()
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
                n = 333;
                N = 333 + rand.Next(3333) + n;

                shape = rand.Next(5);

                nTrail = 123 + rand.Next(333);
            }

            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time local initialization
        private void initLocal()
        {
            doClearBuffer = myUtils.randomBool(rand);
            doFillShapes  = myUtils.randomBool(rand);
            doClearBuffer = true;

            renderDelay = rand.Next(3) + 3;

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
                            $"doClearBuffer = {doClearBuffer}\n"     +
                            $"n = {n}\n"                             +
                            $"nTrail = {nTrail}\n"                   +
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

            dx = myUtils.randFloatSigned(rand);
            dy = myUtils.randFloatSigned(rand);

            dx = dy = 0;

            size = 3;

            dAngle = myUtils.randFloatSigned(rand, 0.1f) * 0.05f;

            A = 0.5f;
            colorPicker.getColor(x, y, ref R, ref G, ref B);

            if (id < n)
            {
                state = 0;
                R = 1;
                G = 0.5f;
                B = 0.5f;
                A = 0.9f;
                size = 10;

                // Initialize Trail
                if (trail == null)
                {
                    trail = new myParticleTrail(nTrail, x, y);
                }
                else
                {
                    trail.reset(x, y);
                }

                trail.updateDa(1);
            }
            else
            {
                neighbours = new List<myObj_0760>();
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            if (id < n)
            {
                switch (state)
                {
                    // Wait until the point decides to start moving around for the first time
                    case 0:
                        {
                            if (myUtils.randomChance(rand, 1, 3333))
                            {
                                state = 1;
                                var start = list[rand.Next(list.Count - 1 - n) + n] as myObj_0760;
                                startId = start.id;
                                x = start.x;
                                y = start.y;

                                var target = start.neighbours[rand.Next(start.neighbours.Count)];
                                targetId = target.id;

                                dx = target.x - start.x;
                                dy = target.y - start.y;

                                double dist = Math.Sqrt(dx * dx + dy * dy);

                                dx = (float)(dx / dist);
                                dy = (float)(dy / dist);

                                trail.reset(x, y);
                            }
                        }
                        break;

                    // Move from point to point
                    case 1:
                        {
                            trail.update(x, y);

                            var target = list[(int)targetId] as myObj_0760;

                            if (Math.Abs(dx) >= Math.Abs(x - target.x) || Math.Abs(dy) >= Math.Abs(y - target.y))
                            {
                                x = target.x;
                                y = target.y;
                                state = 2;
                            }
                            else
                            {
                                x += dx;
                                y += dy;
                            }
                        }
                        break;

                    // Wait at the target point and eventually find the next target
                    case 2:
                        {
                            if (myUtils.randomChance(rand, 1, 123))
                            {
                                state = 1;
                                var start = list[(int)targetId] as myObj_0760;

                                int next = -1;

                                if (start.neighbours.Count > 1)
                                {
                                    // Make sure we don't go back to where we just came from
                                    do
                                        next = rand.Next(start.neighbours.Count);
                                    while (start.neighbours[next].id == startId);
                                }
                                else
                                {
                                    next = 0;
                                }

                                startId = start.id;

                                var target = start.neighbours[next];
                                targetId = target.id;

                                dx = target.x - start.x;
                                dy = target.y - start.y;

                                double dist = Math.Sqrt(dx * dx + dy * dy);

                                dx = (float)(dx / dist);
                                dy = (float)(dy / dist);
                            }
                        }
                        break;
                }
            }
            else
            {
                angle += dAngle;
            }

            return;

            x += dx;
            y += dy;

            dx += myUtils.randFloatSigned(rand) * 0.01f;
            dy += myUtils.randFloatSigned(rand) * 0.01f;

            return;

            if (myUtils.randomChance(rand, 1, 33))
            {
                x += myUtils.randFloatSigned(rand) * 2.5f;
                y += myUtils.randFloatSigned(rand) * 2.5f;
            }

            return;

            x += dx;
            y += dy;

            if (x < 0 || x > gl_Width || y < 0 || y > gl_Height)
            {
                generateNew();
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Show()
        {
            if (id < n)
            {
                if (state != 0)
                {
                    trail.Show(R, G, B, A > 1 ? 1 : A);

                    shader.SetColor(R, G, B, A);
                    shader.Draw(x, y, size, size, 10);
                }

                return;
            }
            else
            {
                for (int i = 0; i < neighbours.Count; i++)
                {
                    var other = neighbours[i];

                    myPrimitive._LineInst.setInstanceCoords(x, y, other.x, other.y);
                    myPrimitive._LineInst.setInstanceColor(1, 1, 1, 0.1f);
                }
            }

            float size2x = size * 2;

            switch (shape)
            {
                // Instanced squares
                case 0:
                    myPrimitive._RectangleInst.setInstanceCoords(x - size, y - size, size2x, size2x);
                    myPrimitive._RectangleInst.setInstanceColor(R, G, B, A);
                    myPrimitive._RectangleInst.setInstanceAngle(angle);
                    break;

                // Instanced triangles
                case 1:
                    myPrimitive._TriangleInst.setInstanceCoords(x, y, size, angle);
                    myPrimitive._TriangleInst.setInstanceColor(R, G, B, A);
                    break;

                // Instanced circles
                case 2:
                    myPrimitive._EllipseInst.setInstanceCoords(x, y, size2x, angle);
                    myPrimitive._EllipseInst.setInstanceColor(R, G, B, A);
                    break;

                // Instanced pentagons
                case 3:
                    myPrimitive._PentagonInst.setInstanceCoords(x, y, size2x, angle);
                    myPrimitive._PentagonInst.setInstanceColor(R, G, B, A);
                    break;

                // Instanced hexagons
                case 4:
                    myPrimitive._HexagonInst.setInstanceCoords(x, y, size2x, angle);
                    myPrimitive._HexagonInst.setInstanceColor(R, G, B, A);
                    break;
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
                list.Add(new myObj_0760());
            }


            findNeighbours(6);


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
                        grad.Draw();
                    }
                    else
                    {
                        dimScreen(dimAlpha);
                    }
                }

                // Render Frame
                {
                    inst.ResetBuffer();
                    myLineInst._LineInst.ResetBuffer();

                    for (int i = 0; i != Count; i++)
                    {
                        var obj = list[i] as myObj_0760;

                        obj.Show();
                        obj.Move();
                    }

                    myLineInst._LineInst.Draw();

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

                cnt++;
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

            myPrimitive.init_LineInst(N * 10 + N * nTrail);
            myPrimitive._LineInst.setAntialized(true);

            getShader();

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void getShader()
        {
            string header = "", main = "";

            my.myShaderHelpers.Shapes.getShader_000_circle(ref rand, ref header, ref main);

            shader = new myFreeShader(header, main);
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void findNeighbours(int numConnections)
        {
            var listFData = new List<findData>();

            for (int i = n; i < list.Count; i++)
            {
                var obj1 = list[i] as myObj_0760;

                for (int j = n; j < list.Count; j++)
                {
                    if (i != j)
                    {
                        var obj2 = list[j] as myObj_0760;

                        float dx = obj1.x - obj2.x;
                        float dy = obj1.y - obj2.y;

                        float distSquared = dx * dx + dy * dy;

                        var item = new findData();
                        item.index = j;
                        item.dist = distSquared;

                        listFData.Add(item);
                    }
                }

                listFData.Sort(delegate (findData x, findData y)
                {
                    if (x.dist > y.dist)
                        return 1;

                    if (x.dist < y.dist)
                        return -1;

                    return 0;
                });

                for (int index = 0; index < numConnections; index++)
                {
                    obj1.neighbours.Add(list[listFData[index].index] as myObj_0760);
                }

                if (myUtils.randomChance(rand, 1, 111))
                {
                    int idx = rand.Next(listFData.Count);
                    obj1.neighbours.Add(list[listFData[idx].index] as myObj_0760);
                }

                listFData.Clear();
            }
        }

        // ---------------------------------------------------------------------------------------------------------------
    }
};
