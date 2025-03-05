using GLFW;
using static OpenGL.GL;
using System.Collections.Generic;


/*
    - colors withing a line of image are sorted and counted
*/


namespace my
{
    public class myObj_1100 : myObject
    {
/*
        class dataItem {
            float R; float G; float B;
            int cnt;
        }
*/
        // Priority
        public static int Priority => 3;
		public static System.Type Type => typeof(myObj_1100);

        private float x, y;
        private float R, G, B;

        private static int N = 0, step = 10;
        private static bool doFillShapes = true;
        private static float dimAlpha = 0.05f;

        private static myScreenGradient grad = null;
        private static Dictionary<ulong, int> _data = null;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_1100()
        {
            if (id != uint.MaxValue)
                generateNew();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time global initialization
        protected override void initGlobal()
        {
            colorPicker = new myColorPicker(gl_Width, gl_Height, mode: myColorPicker.colorMode.SNAPSHOT_OR_IMAGE);
            list = new List<myObject>();

            _data = new Dictionary<ulong, int>();

            // Global unmutable constants
            {
                N = rand.Next(10) + 10;
                N = 1;
            }

            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time local initialization
        private void initLocal()
        {
            doClearBuffer = myUtils.randomBool(rand);
            doClearBuffer = true;

            renderDelay = rand.Next(3) + 3;

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            height = 600;

            string str = $"Obj = {Type}\n\n"                  +
                            myUtils.strCountOf(list.Count, N) +
                            $"renderDelay = {renderDelay}\n"  +
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
            x = 0;
            y = 0;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            y += 0.25f;

            if (y < 0 || y > gl_Height)
            {
                generateNew();
            }
            else
            {
                _data.Clear();

                for (int i = 0; i < gl_Width; i += step)
                {
                    colorPicker.getColorAverage_Int(i, y, step, step, ref R, ref G, ref B);

                    ulong r = (ulong)R;
                    ulong g = (ulong)G << 8;
                    ulong b = (ulong)B << 16;

                    ulong result = r | g | b;

                    if (_data.ContainsKey(result))
                    {
                        _data[result] = _data[result] + 1;
                    }
                    else
                    {
                        _data[result] = 1;
                    }
                }
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Show()
        {
            float size2x = step * 2;

            x = 0;

            int n = gl_Width / _data.Count;

            foreach (var colorData in _data)
            {
                int cnt = colorData.Value;

                ulong key = colorData.Key;

                ulong r = key & 0xFF;
                key >>= 8;
                ulong g = key & 0xFF;
                key >>= 8;
                ulong b = key & 0xFF;

                R = r / 255.0f;
                G = g / 255.0f;
                B = b / 255.0f;

                myPrimitive._RectangleInst.setInstanceCoords(x, 100, n-2, 123 + cnt * 33);
                myPrimitive._RectangleInst.setInstanceColor(R, G, B, 1);
                myPrimitive._RectangleInst.setInstanceAngle(0);

                x += n;
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Process(Window window)
        {
            uint cnt = 0;
            initShapes();

            clearScreenSetup(doClearBuffer, 0.1f);

            list.Add(new myObj_1100());

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

                    for (int i = 0; i != Count; i++)
                    {
                        var obj = list[i] as myObj_1100;

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

                cnt++;
                System.Threading.Thread.Sleep(renderDelay);
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void initShapes()
        {
            myPrimitive.init_ScrDimmer();
            base.initShapes(0, gl_Width, 0);

            grad = new myScreenGradient();
            grad.SetRandomColors(rand, 0.2f);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------
    }
};
