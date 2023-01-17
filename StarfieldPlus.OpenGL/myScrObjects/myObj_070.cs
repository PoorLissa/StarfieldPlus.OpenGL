using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;


/*
    - Pieces drop off the desktop and fall down -- random positions and sizes of the tiles

    todo:
        look for target color across all the cell, not only in a single pixel
*/


namespace my
{
    public class myObj_070 : myObject
    {
        private float x, y, dx, dy;
        private float size, A, R, G, B, angle = 0;
        private bool alive = false, isFirstStep;

        private static int N = 0, shape = 0, maxSize = 66;
        private static bool doFillShapes = false;
        private static float dimAlpha = 0.5f;

        static myTexRectangle tex = null;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_070()
        {
            generateNew();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time global initialization
        protected override void initGlobal()
        {
            colorPicker = new myColorPicker(gl_Width, gl_Height, mode: myColorPicker.colorMode.SNAPSHOT_OR_IMAGE);
            list = new List<myObject>();

            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time local initialization
        private void initLocal()
        {
            N = 1;

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            height = 800;

            string str = $"Obj = myObj_070\n\n" +
                            $"N = {list.Count} of {N}\n" +
                            $"file: {colorPicker.GetFileName()}" +
                            $""
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
            size = rand.Next(maxSize) + 1;
            dx = rand.Next(3) - 1;
            dy = 0;
            isFirstStep = true;

            int failCnt = 0;

            do
            {
                A = (rand.Next(56) + 200) / 255.0f;
                x = rand.Next(gl_Width);
                y = rand.Next(gl_Height);
                colorPicker.getColor(x, y, ref R, ref G, ref B);

                if (++failCnt > 50)
                {
                    alive = false;
                    maxSize -= (maxSize == 5) ? 0 : 1;
                    return;
                }
            }
            while (R == 0 && G == 0 && B == 0);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            if ((int)y % 5 == 0)
            {
                x += rand.Next(3) - 1;
            }

            y += dy;

            dy += (0.01f + size / 5.0f);

            if (y > gl_Height + size)
            {
                generateNew();
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void drawToOrigin(int x, int y, int size)
        {
            System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(size, size);

            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    bmp.SetPixel(i, j, System.Drawing.Color.Red);
                }
            }

            colorPicker.zzz(bmp, x, y);
            tex.reloadImg(colorPicker.getImg());
        }

        protected override void Show()
        {
            if (isFirstStep)
            {
                isFirstStep = false;

                // Dim the tile on the src image
                drawToOrigin((int)x, (int)y, 33);

                // new a new method, update texture from desktop;
                // the method must access the buffer https://stackoverflow.com/questions/64573427/save-drawn-texture-with-opengl-in-to-a-file
                // and copy the needed piece into original colorPicker image
                // then it needs to call reload texture tex.reloadImg(colorPicker.getImg());
            }
            else
            {
            }

            return;
/*
            br.Color = Color.FromArgb(A, R, G, B);
            g.FillRectangle(br, X - Size, Y - Size, 2 * Size, 2 * Size);
            g.DrawRectangle(p, X - Size, Y - Size, 2 * Size, 2 * Size);
*/
            switch (shape)
            {
                // Instanced squares
                case 0:
                    var rectInst = inst as myRectangleInst;

                    rectInst.setInstanceCoords(x - size, y - size, 2 * size, 2 * size);
                    rectInst.setInstanceColor(R, G, B, A);
                    rectInst.setInstanceAngle(angle);
                    break;

                // Instanced triangles
                case 1:
                    var triangleInst = inst as myTriangleInst;

                    triangleInst.setInstanceCoords(x, y, 2 * size, angle);
                    triangleInst.setInstanceColor(R, G, B, A);
                    break;

                // Instanced circles
                case 2:
                    var ellipseInst = inst as myEllipseInst;

                    ellipseInst.setInstanceCoords(x, y, 2 * size, angle);
                    ellipseInst.setInstanceColor(R, G, B, A);
                    break;

                // Instanced pentagons
                case 3:
                    var pentagonInst = inst as myPentagonInst;

                    pentagonInst.setInstanceCoords(x, y, 2 * size, angle);
                    pentagonInst.setInstanceColor(R, G, B, A);
                    break;

                // Instanced hexagons
                case 4:
                    var hexagonInst = inst as myHexagonInst;

                    hexagonInst.setInstanceCoords(x, y, 2 * size, angle);
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

            // Disable VSYNC if needed
            // Glfw.SwapInterval(0);

            doClearBuffer = false;

            if (doClearBuffer)
            {
                glDrawBuffer(GL_FRONT_AND_BACK | GL_DEPTH_BUFFER_BIT);
                glClearColor(0, 0, 0, 1);
            }
            else
            {
                glDrawBuffer(GL_FRONT_AND_BACK);
            }

            tex.setOpacity(1);
            tex.Draw(0, 0, gl_Width, gl_Height);
            System.Threading.Thread.Sleep(333);

            while (!Glfw.WindowShouldClose(window))
            {
                cnt++;

                processInput(window);

                // Swap fore/back framebuffers, and poll for operating system events.
                Glfw.SwapBuffers(window);
                Glfw.PollEvents();

                if (doClearBuffer)
                {
                    glClear(GL_COLOR_BUFFER_BIT);
                }
                else
                {
                    tex.Draw(0, 0, gl_Width, gl_Height);
                }

                // Render Frame
                {
                    inst.ResetBuffer();

                    for (int i = 0; i < list.Count; i++)
                    {
                        var obj = list[i] as myObj_070;

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

                if (list.Count < N)
                {
                    list.Add(new myObj_070());
                }

                System.Threading.Thread.Sleep(renderDelay);
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void initShapes()
        {
            myPrimitive.init_Rectangle();
            base.initShapes(shape, N, 0);

            tex = new myTexRectangle(colorPicker.getImg());

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------
    }
};
