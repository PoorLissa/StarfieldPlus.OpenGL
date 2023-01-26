using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;


/*
    - Pieces drop off the desktop and fall down -- random positions and sizes of the tiles

    todo:
        - find a way to dim the existing texture or create another paintable tex and draw it over the picture;
        - look for target color across all the cell, not only in a single pixel;
*/


namespace my
{
    public class myObj_070 : myObject
    {
        private float x, y, dx, dy;
        private float size, A, R, G, B, angle = 0, dAngle = 0;
        private bool alive = false, isFirstStep;

        private static int N = 0, shape = 0, maxSize = 66;
        private static bool doFillShapes = false;
        private static float dimAlpha = 0.5f;

        static myTexRectangle tex = null;

        myTexRectangle_Renderer rnd = null;

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

            N = 333;
            shape = 4;

            doFillShapes = true;
            doClearBuffer = true;

            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time local initialization
        private void initLocal()
        {
            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            height = 800;

            string str = $"Obj = myObj_070 -- unfinished\n\n"   +
                            $"N = {list.Count} of {N}\n"        +
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

            size = rand.Next(666) + 33;
            angle = myUtils.randFloat(rand) * 1234;
            dAngle = myUtils.randFloat(rand, 0.1f) * 0.01f;

            //x = gl_x0;
            //y = gl_y0;

            R = 1;
            G = 1;
            B = 1;

            colorPicker.getColor(x, y, ref R, ref G, ref B);

            A = myUtils.randFloat(rand, 0.1f) * 0.1f;

            dx = dy = 0;

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            angle += dAngle;

            return;

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

        protected override void Show()
        {
            if (isFirstStep)
            {
                isFirstStep = false;
/*
                // Dim the tile on the src image
                myPrimitive._Rectangle.SetAngle(0);
                myPrimitive._Rectangle.SetColor(myUtils.randFloat(rand), myUtils.randFloat(rand), myUtils.randFloat(rand), 0.75f);
                myPrimitive._Rectangle.Draw(x, y, size, size, true);

                var bmp = myOGL.copyScreenBuffer((int)x, (int)(y + gl_Height - size), (int)size, (int)size);

                colorPicker.updateSrcImg(bmp, (int)x, (int)y);
                tex.reloadImg(colorPicker.getImg());
*/
            }
            else
            {
            }

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
                    tex.Draw(0, 0, gl_Width, gl_Height);
                }
                else
                {
                    tex.Draw(0, 0, gl_Width, gl_Height);
                }

                if (true)
                {
                    rnd.startRendering();
                    {
                        glClearColor(0, 0, 0, 1);
                        glClear(GL_DEPTH_BUFFER_BIT);

                        for (int i = 0; i < 3; i++)
                        {
                            int x = rand.Next(gl_Width);
                            int y = rand.Next(gl_Height);
                            int w = rand.Next(50) + 3;

                            float r = myUtils.randFloat(rand) * 0.1f;
                            float g = myUtils.randFloat(rand) * 0.1f;
                            float b = myUtils.randFloat(rand) * 0.1f;

                            myPrimitive._Rectangle.SetColor(r, g, b, 0.25f);
                            myPrimitive._Rectangle.Draw(x, y, w, w, true);

                            myPrimitive._Rectangle.SetColor(r, g, b, 0.5f);
                            myPrimitive._Rectangle.Draw(x, y, w, w, false);
                        }
                    }
                    rnd.stopRendering();
                    rnd.Draw(0, 0, gl_Width, gl_Height);
                }

                // Render Frame
                if (true)
                {
                    inst.ResetBuffer();

                    for (int i = 0; i != list.Count; i++)
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

myPrimitive._HexagonInst.setRotationMode(1);

            rnd = new myTexRectangle_Renderer();

            tex = new myTexRectangle(colorPicker.getImg());

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------
    }
};
