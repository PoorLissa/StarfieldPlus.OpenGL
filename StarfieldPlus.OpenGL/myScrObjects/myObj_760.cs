using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;
using System.Drawing.Text;


/*
    - Graph test
*/


namespace my
{
    public class myObj_760 : myObject
    {
        // Priority
        public static int Priority => 10;
		public static System.Type Type => typeof(myObj_760);

        private int state;
        private uint targetId;
        private float x, y, dx, dy;
        private float size, A, R, G, B, angle = 0, dAngle;

        private List<myObj_760> neighbours = null;

        private static int N = 0, n = 0, shape = 0;
        private static bool doFillShapes = false;
        private static float dimAlpha = 0.05f;

        private static myScreenGradient grad = null;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_760()
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
                N = 333 + rand.Next(3333);
                n = 33;

                shape = rand.Next(5);
            }

            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time local initialization
        private void initLocal()
        {
            doClearBuffer = myUtils.randomBool(rand);
            doClearBuffer = true;

            renderDelay = rand.Next(11) + 3;

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
                size = 5;
            }
            else
            {
                neighbours = new List<myObj_760>();
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
                    // Wait until it decides to start moving around
                    case 0:
                        {
                            if (myUtils.randomChance(rand, 1, 123))
                            {
                                state = 1;
                                var start = list[rand.Next(list.Count - 1 - n) + n] as myObj_760;
                                x = start.x;
                                y = start.y;

                                var target = start.neighbours[rand.Next(start.neighbours.Count)];
                                targetId = target.id;

                                dx = target.x - start.x;
                                dy = target.y - start.y;

                                double dist = Math.Sqrt(dx * dx + dy * dy);

                                dx = (float)(dx / dist);
                                dy = (float)(dy / dist);
                            }
                        }
                        break;

                    case 1:
                        {
                            x += dx;
                            y += dy;

                            var target = list[(int)targetId] as myObj_760;

                            float tmpdx = x - target.x;
                            float tmpdy = y - target.y;

                            double dist = Math.Sqrt(tmpdx * tmpdx + tmpdy * tmpdy);

                            if (dist < 3)
                            {
                                x = target.x;
                                y = target.y;
                                state = 2;
                            }
                        }
                        break;

                    case 2:
                        {
                            if (myUtils.randomChance(rand, 1, 123))
                            {
                                state = 1;
                                var start = list[(int)targetId] as myObj_760;

                                var target = start.neighbours[rand.Next(start.neighbours.Count)];
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
                if (state == 0)
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
                    var rectInst = inst as myRectangleInst;

                    rectInst.setInstanceCoords(x - size, y - size, size2x, size2x);
                    rectInst.setInstanceColor(R, G, B, A);
                    rectInst.setInstanceAngle(angle);
                    break;

                // Instanced triangles
                case 1:
                    var triangleInst = inst as myTriangleInst;

                    triangleInst.setInstanceCoords(x, y, size, angle);
                    triangleInst.setInstanceColor(R, G, B, A);
                    break;

                // Instanced circles
                case 2:
                    var ellipseInst = inst as myEllipseInst;

                    ellipseInst.setInstanceCoords(x, y, size2x, angle);
                    ellipseInst.setInstanceColor(R, G, B, A);
                    break;

                // Instanced pentagons
                case 3:
                    var pentagonInst = inst as myPentagonInst;

                    pentagonInst.setInstanceCoords(x, y, size2x, angle);
                    pentagonInst.setInstanceColor(R, G, B, A);
                    break;

                // Instanced hexagons
                case 4:
                    var hexagonInst = inst as myHexagonInst;

                    hexagonInst.setInstanceCoords(x, y, size2x, angle);
                    hexagonInst.setInstanceColor(R, G, B, A);
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
                list.Add(new myObj_760());
            }


            findNeighbours(6);
            //findNeighboursRand(6);


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
                        var obj = list[i] as myObj_760;

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
            grad.SetRandomColors(rand, 0.2f, 0);

            myPrimitive.init_LineInst(N * 10);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private class findData
        {
            public int index;
            public float dist;
        };

        private void findNeighboursRand(int numConnections)
        {
            for (int i = n; i < list.Count; i++)
            {
                var obj1 = list[i] as myObj_760;

                for (int j = 0; j < numConnections; j++)
                {
                    int index = n + rand.Next(list.Count - n - 1);
                    obj1.neighbours.Add(list[index] as myObj_760);
                }
            }
        }

        private void findNeighbours(int numConnections)
        {
            var listFData = new List<findData>();

            for (int i = n; i < list.Count; i++)
            {
                var obj1 = list[i] as myObj_760;

                for (int j = n; j < list.Count; j++)
                {
                    if (i != j)
                    {
                        var obj2 = list[j] as myObj_760;

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
                    obj1.neighbours.Add(list[listFData[index].index] as myObj_760);
                }

                listFData.Clear();
            }
        }

        // ---------------------------------------------------------------------------------------------------------------
    }
};
