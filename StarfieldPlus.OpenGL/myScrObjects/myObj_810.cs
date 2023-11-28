using GLFW;
using static OpenGL.GL;
using System.Collections.Generic;


/*
    - Raster scan of an image
*/


namespace my
{
    public class myObj_810 : myObject
    {
        // Priority
        public static int Priority => 999910;
		public static System.Type Type => typeof(myObj_810);

        private float x;
        private float size, localMaxSize, A, R, G, B, dR, dG, dB;

        private static uint cnt = 0;
        private static int N = 0, Y = 0, step = 10, maxSize = 1, sizeMode = 0, drawMode = 0;
        private static int yDir = 1;
        private static float maxOpacity = 0, r = 0, g = 0, b = 0, lineWidth = 1;

        private static myScreenGradient grad = null;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_810()
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
                N = gl_Width;

                lineWidth = 0.1f + myUtils.randFloat(rand) * rand.Next(5);
            }

            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time local initialization
        private void initLocal()
        {
            doClearBuffer = myUtils.randomChance(rand, 1, 5);

            sizeMode = rand.Next(2);
            drawMode = rand.Next(6);

            maxOpacity = 0.05f + myUtils.randFloat(rand) * 0.25f;

            maxSize = (gl_y0/2 + rand.Next(gl_y0/2)) / 3;

            step = 5 + rand.Next(16);
            renderDelay = 20 - step;

            renderDelay += 1;

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            height = 600;

            string nStr(int   n) { return n.ToString("N0");    }
            string fStr(float f) { return f.ToString("0.000"); }

            string str = $"Obj = {Type}\n\n"                         +
                            $"N = {nStr(list.Count)} of {nStr(N)}\n" +
                            $"doClearBuffer = {doClearBuffer}\n"     +
                            $"step = {step}\n"                       +
                            $"sizeMode = {sizeMode}\n"               +
                            $"drawMode = {drawMode}\n"               +
                            $"maxSize = {maxSize}\n"                 +
                            $"maxOpacity = {fStr(maxOpacity)}\n"     +
                            $"lineWidth = {fStr(lineWidth)}\n"       +
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
            x = id;
            size = 1;

            A = doClearBuffer ? maxOpacity * 3 : maxOpacity;
            R = G = B = 0;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            if (cnt == 0)
            {
                colorPicker.getColor(x, Y, ref r, ref g, ref b);

                dR = (r - R) / step;
                dG = (g - G) / step;
                dB = (b - B) / step;

                switch (sizeMode)
                {
                    case 0:
                        localMaxSize = maxSize;
                        break;

                    case 1:
                        localMaxSize = maxSize + rand.Next(23);
                        break;
                }
            }

            R += dR;
            G += dG;
            B += dB;

            size = 5 + (R + G + B) * localMaxSize;

#if false
            if (R + G + B < 0.3f)
            {
                size = -1 + (R + G + B) * localMaxSize / 5;
            }
#endif
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Show()
        {
            switch (drawMode)
            {
                case 0:
                    myPrimitive._LineInst.setInstanceCoords(x, gl_y0 + size, x, gl_y0 - size);
                    myPrimitive._LineInst.setInstanceColor(R, G, B, A);
                    break;

                case 1:
                    {
                        myPrimitive._LineInst.setInstanceCoords(x, 300, x, 300 + size);
                        myPrimitive._LineInst.setInstanceColor(R, G, B, A);
                    }
                    break;

                case 2:
                    {
                        float y = gl_Height - 100;
                        myPrimitive._LineInst.setInstanceCoords(x, y, x, y - size);
                        myPrimitive._LineInst.setInstanceColor(R, G, B, A);
                    }
                    break;

                case 3:
                    {
                        myPrimitive._LineInst.setInstanceCoords(x, 0, x, gl_Height);
                        myPrimitive._LineInst.setInstanceColor(R, G, B, A);
                    }
                    break;

                case 4:
                    {
                        myPrimitive._LineInst.setInstanceCoords(x, 300, x, 300 + size);
                        myPrimitive._LineInst.setInstanceColor(R, G, B, A);

                        myPrimitive._Rectangle.SetColor(R, G, B, 0.5f);
                        myPrimitive._Rectangle.Draw(x - 1, 100 + size - 1, 2, 2);
                        //myPrimitive._Rectangle.Draw(x - 1, 300 - size/5 - 1, 2, 2);
                    }
                    break;

                case 5:
                    {
                        float y = gl_Height - 100;

                        myPrimitive._LineInst.setInstanceCoords(x, y, x, y - size);
                        myPrimitive._LineInst.setInstanceColor(R, G, B, A);

                        myPrimitive._Rectangle.SetColor(R, G, B, 0.5f);
                        myPrimitive._Rectangle.Draw(x - 1, y - size - 1, 2, 2);
                    }
                    break;
            }
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Process(Window window)
        {
            initShapes();


            clearScreenSetup(doClearBuffer, 0.1f);

            while (list.Count < N)
            {
                list.Add(new myObj_810());
            }

            if (doClearBuffer == false)
            {
                grad.SetOpacity(0.1f);
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
                    if (doClearBuffer)
                    {
                        glClear(GL_COLOR_BUFFER_BIT);
                    }

                    grad.Draw();
                }

                // Render Frame
                {
                    myPrimitive._LineInst.ResetBuffer();

                    for (int i = 0; i != Count; i++)
                    {
                        var obj = list[i] as myObj_810;

                        obj.Show();
                        obj.Move();
                    }

                    myPrimitive._LineInst.Draw();
                }

                System.Threading.Thread.Sleep(renderDelay);

                if (++cnt == step)
                {
                    cnt = 0;
                    Y += yDir;

                    if (Y == gl_Height && yDir > 0)
                    {
                        yDir *= -1;
                    }

                    if (Y == 0 && yDir < 0)
                    {
                        yDir *= -1;
                    }
                }
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void initShapes()
        {
            myPrimitive.init_Rectangle();
            myPrimitive.init_LineInst(N);

            myPrimitive._LineInst.setLineWidth(lineWidth);

            grad = new myScreenGradient();
            grad.SetRandomColors(rand, 0.2f, 0);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------
    }
};
