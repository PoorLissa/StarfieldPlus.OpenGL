using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;


/*
    - test for sorted O(N^2) interactions
*/


namespace my
{
    public class myObj_999_test_002 : myObject
    {
        class myObj_999_test_002_Comparer : IComparer<myObj_999_test_002>
        {
            public int Compare(myObj_999_test_002 obj1, myObj_999_test_002 obj2)
            {
                int x1 = (int)obj1.x;
                int x2 = (int)obj2.x;

                return x1 < x2
                    ? -1
                    : x1 > x2
                        ? 1
                        : 0;
            }
        };

        // ---------------------------------------------------------------------------------------------------------------

        // Priority
        public static int Priority => 9999910;
		public static System.Type Type => typeof(myObj_999_test_002);

        private float x, y, dx, dy;
        private float size, A, R, G, B, angle = 0;

        private static int N = 0, shape = 0, maxConnectionDist = 100, nTaskCount = 1, lenMode = 0;
        private static bool doFillShapes = false;
        private static float dSpeed = 0.01f;

        private static myScreenGradient grad = null;

        private static List<myObj_999_test_002> sortedList = null;

        // Comparer for a binary search on the sortedList
        private static myObj_999_test_002_Comparer cmp = new myObj_999_test_002_Comparer();

        private static object _lock = new object();

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_999_test_002()
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
            sortedList = new List<myObj_999_test_002>();

            // Global unmutable constants
            {
                N = rand.Next(100) + 100;
                N = 3333;

                nTaskCount = Environment.ProcessorCount - 1;
                nTaskCount = 4;

                shape = rand.Next(5);
            }

            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time local initialization
        private void initLocal()
        {
            doClearBuffer = true;

            lenMode = rand.Next(3);

            switch (lenMode)
            {
                case 0: maxConnectionDist = 250; break;
                case 1: maxConnectionDist = 100 + rand.Next(150); break;
                case 2: maxConnectionDist = 200; break;
            }

            maxConnectionDist = 250;
            renderDelay = 0;

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            height = 600;

            string nStr(int   n) { return n.ToString("N0");    }
            //string fStr(float f) { return f.ToString("0.000"); }

            string str = $"Obj = {Type}\n\n"                             +
                            $"N = {nStr(list.Count)} of {nStr(N)}\n"     +
                            $"lenMode = {lenMode}\n"                     +
                            $"maxConnectionDist = {maxConnectionDist}\n" +
                            $"renderDelay = {renderDelay}\n"             +
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

            dx = myUtils.randFloatSigned(rand) * (rand.Next(5) + 1);
            dy = myUtils.randFloatSigned(rand) * (rand.Next(5) + 1);

            size = 3;

            A = 0.5f + myUtils.randFloat(rand) * 0.5f;
            colorPicker.getColor(x, y, ref R, ref G, ref B);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            x += dx;
            y += dy;

            if (x < 0) dx += dSpeed; else if (x > gl_Width ) dx -= dSpeed;
            if (y < 0) dy += dSpeed; else if (y > gl_Height) dy -= dSpeed;

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Show()
        {
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

                    triangleInst.setInstanceCoords(x, y, size2x, angle);
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
            initShapes();


            clearScreenSetup(doClearBuffer, 0.1f);


            if (true)
            {
                while (list.Count < N)
                {
                    var obj = new myObj_999_test_002();

                    list.Add(obj);
                    sortedList.Add(obj);
                }
            }


            switch (nTaskCount)
            {
                // Single thread
                case 1:
                    process1(window);
                    break;

                // Multiple threads
                default:
                    process2(window);
                    break;
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        // Single thread process
        private void process1(Window window)
        {
            uint cnt = 0;

            while (!Glfw.WindowShouldClose(window))
            {
                int Count = list.Count;

                SortParticles();

                processInput(window);

                // Swap fore/back framebuffers, and poll for operating system events.
                Glfw.SwapBuffers(window);
                Glfw.PollEvents();

                // Dim screen
                {
                    glClear(GL_COLOR_BUFFER_BIT);
                    grad.Draw();
                }

                // Render Frame
                {
                    inst.ResetBuffer();
                    myPrimitive._LineInst.ResetBuffer();

                    // Draw all the connecting lines between particles;
                    // This is the most time consuming part here, and is optimized using binary searches on a sorted array
                    for (int i = 0; i != Count; i++)
                    {
                        (list[i] as myObj_999_test_002).showConnections();
                    }

                    // As we're working off a sortedList, Show and Move methods should be called from within the separate loops
                    for (int i = 0; i != Count; i++)
                    {
                        var obj = list[i] as myObj_999_test_002;
                        obj.Show();
                        obj.Move();
                    }

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

                if (Count < N)
                {
                    var obj = new myObj_999_test_002();

                    list.Add(obj);
                    sortedList.Add(obj);
                }

                cnt++;
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        // Multiple threads process
        private void process2(Window window)
        {
            uint cnt = 0;

            var taskList = new System.Threading.Tasks.Task[nTaskCount];

            // Define a delegate that executes the task:
            Func<object, int> action = (object obj) =>
            {
                int k = (int)obj;

                int beg = (k + 0) * list.Count / nTaskCount;
                int end = (k + 1) * list.Count / nTaskCount;

                for (int i = beg; i < end; i++)
                    (list[i] as myObj_999_test_002).showConnectionsThreadSafe();

                return 0;
            };

            // Make sure the list is already sorted when we're starting out threads
            SortParticles();

            for (int k = 0; k < nTaskCount; k++)
            {
                var task = System.Threading.Tasks.Task<int>.Factory.StartNew(action, k);
                taskList[k] = task;
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
                    glClear(GL_COLOR_BUFFER_BIT);
                    grad.Draw();
                }

                // Render Frame
                {
                    // Wait until all the tasks have finished
                    System.Threading.Tasks.Task.WaitAll(taskList);
                    myPrimitive._LineInst.Draw();
                    myPrimitive._LineInst.ResetBuffer();

                    inst.ResetBuffer();

                    // Move every particle
                    for (int i = 0; i != Count; i++)
                    {
                        var obj = list[i] as myObj_999_test_002;

                        obj.Move();
                        obj.Show();
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
                    var obj = new myObj_999_test_002();

                    list.Add(obj);
                    sortedList.Add(obj);
                }

                SortParticles();

                // Restart all the tasks
                for (int k = 0; k < nTaskCount; k++)
                {
                    var task = System.Threading.Tasks.Task<int>.Factory.StartNew(action, k);
                    taskList[k] = task;
                }

                cnt++;

                if (lenMode == 2 && myUtils.randomChance(rand, 1, 1234))
                {
                    maxConnectionDist = 100 + rand.Next(150);
                }
            }

        }

        // ---------------------------------------------------------------------------------------------------------------

        private void initShapes()
        {
            base.initShapes(shape, N, 0);

            myPrimitive.init_LineInst(N * N);

            grad = new myScreenGradient();
            grad.SetRandomColors(rand, 0.2f, 0);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void SortParticles()
        {
            sortedList.Sort(delegate (myObj_999_test_002 obj1, myObj_999_test_002 obj2)
            {
                return obj1.x < obj2.x
                    ? -1
                    : obj1.x > obj2.x
                        ? 1
                        : 0;
            });
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void showConnections()
        {
            int Count = list.Count;
            int maxDist2 = maxConnectionDist * maxConnectionDist;
            float maxDist2_inv = 1.0f / maxDist2, opacityFactor = 0.025f;

            int min = (int)x - maxConnectionDist;
            int max = (int)x + maxConnectionDist;

            float dx, dy, dist2, a;

            // Find current object's index within our sortedList:
            int current_index = sortedList.BinarySearch(this, cmp);

            // Traverse right, while within the maxConnectionDist distance
            for (int i = current_index + 1; i < Count; i++)
            {
                var other = sortedList[i];

                if (other.x > max)
                    break;

                dx = x - other.x;
                dy = y - other.y;

                dist2 = dx * dx + dy * dy;

                if (dist2 < maxDist2)
                {
                    a = (1.0f - dist2 * maxDist2_inv) * opacityFactor;

                    myPrimitive._LineInst.setInstanceCoords(x, y, other.x, other.y);
                    myPrimitive._LineInst.setInstanceColor(1, 1, 1, a);
                }
            }

            // Traverse left, while within the maxConnectionDist distance
            for (int i = current_index - 1; i >= 0; i--)
            {
                var other = sortedList[i];

                if (other.x < min)
                    break;

                dx = x - other.x;
                dy = y - other.y;

                dist2 = dx * dx + dy * dy;

                if (dist2 < maxDist2)
                {
                    a = (1.0f - dist2 * maxDist2_inv) * opacityFactor;

                    myPrimitive._LineInst.setInstanceCoords(x, y, other.x, other.y);
                    myPrimitive._LineInst.setInstanceColor(1, 1, 1, a);
                }
            }
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void showConnectionsThreadSafe()
        {
            var selectedList1 = new List<int>();
            var selectedList2 = new List<float>();

            int Count = list.Count;
            int maxDist2 = maxConnectionDist * maxConnectionDist;
            float maxDist2_inv = 1.0f / maxDist2, opacityFactor = 0.025f;

            int min = (int)x - maxConnectionDist;
            int max = (int)x + maxConnectionDist;

            float dx, dy, dist2, a;

            // Find current object's index within our sortedList:
            int current_index = sortedList.BinarySearch(this, cmp);

            // Traverse right, while within the maxConnectionDist distance
            for (int i = current_index + 1; i < Count; i++)
            {
                var other = sortedList[i];

                if (other.x > max)
                    break;

                dx = x - other.x;
                dy = y - other.y;

                dist2 = dx * dx + dy * dy;

                if (dist2 < maxDist2)
                {
                    a = (1.0f - dist2 * maxDist2_inv) * opacityFactor;

                    selectedList1.Add(i);
                    selectedList2.Add(a);

/*
                    lock (_lock)
                    {
                        myPrimitive._LineInst.setInstanceCoords(x, y, other.x, other.y);
                        myPrimitive._LineInst.setInstanceColor(1, 1, 1, a);
                    }*/
                }
            }

            // Traverse left, while within the maxConnectionDist distance
            for (int i = current_index - 1; i >= 0; i--)
            {
                var other = sortedList[i];

                if (other.x < min)
                    break;

                dx = x - other.x;
                dy = y - other.y;

                dist2 = dx * dx + dy * dy;

                if (dist2 < maxDist2)
                {
                    a = (1.0f - dist2 * maxDist2_inv) * opacityFactor;

                    selectedList1.Add(i);
                    selectedList2.Add(a);

/*
                    lock (_lock)
                    {
                        myPrimitive._LineInst.setInstanceCoords(x, y, other.x, other.y);
                        myPrimitive._LineInst.setInstanceColor(1, 1, 1, a);
                    }*/
                }
            }

            lock (_lock)
            {
                for (int i = 0; i < selectedList1.Count; i++)
                {
                    int index = selectedList1[i];
                    var other = sortedList[index];

                    myPrimitive._LineInst.setInstanceCoords(x, y, other.x, other.y);
                    myPrimitive._LineInst.setInstanceColor(1, 1, 1, selectedList2[i]);
                }
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

    }
};
