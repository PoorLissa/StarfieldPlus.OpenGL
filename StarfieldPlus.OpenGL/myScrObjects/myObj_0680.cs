using GLFW;
using static OpenGL.GL;
using System.Collections.Generic;


/*
    - Scrolling wall of pseudo text
*/


namespace my
{
    public class myObj_0680 : myObject
    {
        class symbolItem
        {
            public int index;
            public float x, a;
            //public float r, g, b;

            public symbolItem(float rowX, float rowY, float rowA, float rowSizeFactor)
            {
                index = rand.Next(tTex.Lengh());
                a = rowA + myUtils.randFloatSigned(rand) * 0.15f;
                x = rowX;
            }
        };

        // ---------------------------------------------------------------------------------------------------------------

        // Priority
        public static int Priority => 10;
		public static System.Type Type => typeof(myObj_0680);

        private bool isDead = true;
        private int yOffset = 0;
        private float x, y, dy, sizeFactor;
        private float A, R, G, B;

        private List<symbolItem> itemList = null;

        private static int N = 0, size = 20, startY = -100;

        private static TexText tTex = null;
        private static myScreenGradient grad = null;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_0680()
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
                N = 1;

                size = 20;

                switch (rand.Next(3))
                {
                    case 0: size += rand.Next(10); break;
                    case 1: size += rand.Next(30); break;
                    case 2: size += rand.Next(50); break;
                }
            }

            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time local initialization
        private void initLocal()
        {
            doClearBuffer = true;

            renderDelay = 0;

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            height = 600;

            string str = $"Obj = {Type}\n\n"                    +
                            $"N = {myUtils.nStr(list.Count)}"   +
                            $" of {myUtils.nStr(N)}\n"          +
                            $"size = {size}\n"                  +
                            $"renderDelay = {renderDelay}\n"    +
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
            if (itemList == null)
            {
                itemList = new List<symbolItem>();
            }
            else
            {
                itemList.Clear();
            }

            x = gl_x0 / 2;
            y = startY;

            dy = 1.0f;

            //size = rand.Next(11) + 3;
            sizeFactor = 1.0f;

            // Distance between 2 neighbouring rows
            yOffset = (int)(sizeFactor * tTex.getFieldHeight());

            A = 1;
            R = (float)rand.NextDouble();
            G = (float)rand.NextDouble();
            B = (float)rand.NextDouble();

            colorPicker.getColor(x, y, ref R, ref G, ref B);

            // Generate a line of text
            {
                int maxWidth = gl_x0 + gl_x0 / 2;
                float lastX = x;

                int wordLen = 1 + rand.Next(13);

                for (int i = 0; i < 300; i++)
                {
                    var item = new symbolItem(lastX, y, A, sizeFactor);

                    itemList.Add(item);
                    lastX = item.x + tTex.getFieldWidth(item.index);

                    if (lastX >= maxWidth)
                        break;

                    if (--wordLen == 0)
                    {
                        wordLen = 1 + rand.Next(13);
                        lastX += tTex.getFieldWidth(0);
                    }
                }
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            y += dy;

            if (dy > 0 && y > gl_Height)
            {
                isDead = true;
            }
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Show()
        {
            if (isDead == false)
            {
                int Count = itemList.Count;

                for (int i = 0; i < Count; i++)
                {
                    symbolItem item = itemList[i];

                    tTex.Draw(item.x, y, item.index, sizeFactor, R, G, B, A);
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


            {
                while (list.Count < N)
                {
                    list.Add(new myObj_0680());
                }

                (list[0] as myObj_0680).isDead = false;
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
                    tTex.getTexInst().ResetBuffer();

                    float minY = gl_Height;
                    int idx = 0;

                    for (int i = 0; i != Count; i++)
                    {
                        var obj = list[i] as myObj_0680;

                        obj.Show();
                        obj.Move();

                        if (obj.y < minY)
                        {
                            minY = obj.y;
                            idx = i;
                        }
                    }

                    // Generate next row
                    {
                        var topmost = list[idx] as myObj_0680;

                        if (topmost.y - startY > topmost.yOffset)
                        {
                            for (int i = 0; i < N; i++)
                            {
                                var obj = list[i] as myObj_0680;

                                if (obj.isDead)
                                {
                                    obj.isDead = false;
                                    obj.generateNew();
                                    break;
                                }
                            }
                        }
                    }

                    tTex.getTexInst().Draw();
                }

                cnt++;
                System.Threading.Thread.Sleep(renderDelay);
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void initShapes()
        {
            int fontStyle = 0;

            TexText.setScrDimensions(gl_Width, gl_Height);
            tTex = new TexText(size, true, 100000, fontStyle, -5);

            grad = new myScreenGradient();
            grad.SetRandomColors(rand, 0.2f);

            N = gl_Height / tTex.getFieldHeight() + 3;

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------
    }
};
