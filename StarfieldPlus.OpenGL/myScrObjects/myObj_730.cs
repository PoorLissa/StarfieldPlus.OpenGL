using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;


/*
    - 
*/


namespace my
{
    public class myObj_730 : myObject
    {
        // Priority
        public static int Priority => 999910;
		public static System.Type Type => typeof(myObj_730);

        private float x, y, dx, dy;
        private float A, R, G, B;

        private static int N = 0, i = 0;
        private static float dimAlpha = 0.05f;

        private static myScreenGradient grad = null;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_730()
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

            // Global unmutable constants
            {
                N = rand.Next(10) + 10;
                N = 1111;
            }

            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time local initialization
        private void initLocal()
        {
            doClearBuffer = true;

            renderDelay = 1;

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

            A = 0.1f;
            colorPicker.getColor(x, y, ref R, ref G, ref B);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            x += dx;
            y += dy;

            if (x < 0 && dx < 0)
                dx *= -1;

            if (y < 0 && dy < 0)
                dy *= -1;

            if (x > gl_Width && dx > 0)
                dx *= -1;

            if (y > gl_Height && dy > 0)
                dy *= -1;

            colorPicker.getColor(x, y, ref R, ref G, ref B);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Show()
        {
            var obj1 = list[i+0] as myObj_730;
            var obj2 = list[i+1] as myObj_730;
            var obj3 = list[i+2] as myObj_730;

            myPrimitive._LineInst.setInstanceCoords(obj1.x, obj1.y, obj2.x, obj2.y);
            myPrimitive._LineInst.setInstanceColor((obj1.R + obj2.R)/2, (obj1.G + obj2.G)/2, (obj1.B + obj2.B)/2, A);

            myPrimitive._LineInst.setInstanceCoords(obj2.x, obj2.y, obj3.x, obj3.y);
            myPrimitive._LineInst.setInstanceColor((obj2.R + obj3.R)/2, (obj2.G + obj3.G)/2, (obj2.B + obj3.B)/2, A);

            myPrimitive._LineInst.setInstanceCoords(obj3.x, obj3.y, obj1.x, obj1.y);
            myPrimitive._LineInst.setInstanceColor((obj1.R + obj3.R)/2, (obj1.G + obj3.G)/2, (obj1.B + obj3.B)/2, A);

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
                    myPrimitive._LineInst.ResetBuffer();

                    for (i = 0; i != Count; i++)
                    {
                        var obj = list[i] as myObj_730;
                        obj.Move();
                    }

                    for (i = 0; i != Count; i += 3)
                    {
                        var obj = list[i] as myObj_730;
                        obj.Show();
                    }

                    myPrimitive._LineInst.Draw();
                }

                if (Count < N)
                {
                    list.Add(new myObj_730());
                    list.Add(new myObj_730());
                    list.Add(new myObj_730());
                }

                cnt++;
                System.Threading.Thread.Sleep(renderDelay);
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void initShapes()
        {
            myPrimitive.init_LineInst(N * 3);

            grad = new myScreenGradient();
            grad.SetRandomColors(rand, 0.2f, 0);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------
    }
};
