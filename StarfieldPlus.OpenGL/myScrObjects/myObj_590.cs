using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;


/*
    - Particle's move as an average of n other particles' move
*/


namespace my
{
    public class myObj_590 : myObject
    {
        // Data item class
        private class data {
            public float x, y, z, dx, dy, dz;
        };

        // ---------------------------------------------------------------------------------------------------------------

        // Priority
        public static int Priority => 10;

        private float X, Y, Z;
        private float A, R, G, B;

        private static int N = 0, n = 0, moveMode = 0;
        private static bool doFillShapes = false;
        private static float dimAlpha = 0.05f;

        private static myFreeShader shader = null;

        private List<data> dataList = null;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_590()
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
                N = rand.Next(10) + 2;
                n = rand.Next(10) + 2;
            }

            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time local initialization
        private void initLocal()
        {
            doClearBuffer = true;

            moveMode = rand.Next(2);
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            height = 600;

            string nStr(int   n) { return n.ToString("N0");    }
            //string fStr(float f) { return f.ToString("0.000"); }

            string str = $"Obj = myObj_590\n\n"                      +
                            $"N = {nStr(list.Count)} of {nStr(N)}\n" +
                            $"n = {n}\n"                             +
                            $"moveMode = {moveMode}\n"               +
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
            X = Y = Z = 0;

            if (dataList == null)
            {
                dataList = new List<data>();
            }

            for (int i = 0; i < n; i++)
            {
                var d = new data();

                d.x = rand.Next(gl_Width);
                d.y = rand.Next(gl_Height);
                d.z = rand.Next(1000);

                d.dx = myUtils.randFloat(rand) * myUtils.randomSign(rand) * (rand.Next(5) + 11);
                d.dy = myUtils.randFloat(rand) * myUtils.randomSign(rand) * (rand.Next(5) + 11);
                d.dz = myUtils.randFloat(rand) * myUtils.randomSign(rand) * (rand.Next(5) + 11);

                dataList.Add(d);
            }

            A = 1;
            colorPicker.getColorRand(ref R, ref G, ref B);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            for (int i = 0; i < n; i++)
            {
                var d = dataList[i];

                d.x += d.dx;
                d.y += d.dy;
                d.z += d.dz;

                switch (moveMode)
                {
                    case 0:
                        {
                            if (d.x < 0 && d.dx < 0)
                                d.dx *= -1;

                            if (d.y < 0 && d.dy < 0)
                                d.dy *= -1;

                            if (d.x > gl_Width && d.dx > 0)
                                d.dx *= -1;

                            if (d.y > gl_Height && d.dy > 0)
                                d.dy *= -1;

                            if (d.z > 1000 && d.dz > 0)
                                d.dz *= -1;

                            if (d.z < 0 && d.dz < 0)
                                d.dz *= -1;
                        }
                        break;

                    case 1:
                        {
                            float spd = 0.5f;

                            if (d.x < 0)
                                d.dx += spd;

                            if (d.y < 0)
                                d.dy += spd;

                            if (d.z < 0)
                                d.dz += spd;

                            if (d.x > gl_Width)
                                d.dx -= spd;

                            if (d.y > gl_Height)
                                d.dy -= spd;

                            if (d.z > 1000)
                                d.dz -= spd;
                        }
                        break;
                }
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Show()
        {
            X = Y = Z = 0;

            for (int i = 0; i < n; i++)
            {
                var d = dataList[i];

                var ellipseInst = inst as myEllipseInst;

                ellipseInst.setInstanceCoords(d.x, d.y, 10, 0);
                ellipseInst.setInstanceColor(R, G, B, 0.5f);

                X += d.x;
                Y += d.y;
                Z += d.z;
            }

            X /= n;
            Y /= n;
            Z /= n;

            //float size = 33 + (Z - 500) / 10;
            float size = 33;

            shader.SetColor(R, G, B, A);
            shader.Draw(X, Y, 5, 5, 10);
            shader.Draw(X, Y, size, size, 10);

            if (id > 0)
            {
                var prev = list[(int)id - 1] as myObj_590;

                myPrimitive._LineInst.setInstanceCoords(X, Y, prev.X, prev.Y);
                myPrimitive._LineInst.setInstanceColor(1, 1, 1, 0.1f);

                if (id == N-1)
                {
                    var first = list[0] as myObj_590;

                    myPrimitive._LineInst.setInstanceCoords(X, Y, first.X, first.Y);
                    myPrimitive._LineInst.setInstanceColor(1, 1, 1, 0.1f);
                }
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Process(Window window)
        {
            myTexRectangle tex = new myTexRectangle(myUtils.getGradientBgr(ref rand, gl_Width, gl_Height));

            uint cnt = 0;
            initShapes();

            if (doClearBuffer)
            {
                glDrawBuffer(GL_FRONT_AND_BACK | GL_DEPTH_BUFFER_BIT);
                glClearColor(0, 0, 0, 1);
            }
            else
            {
                dimScreenRGB_SetRandom(0.1f);
                glDrawBuffer(GL_FRONT_AND_BACK);
                //glDrawBuffer(GL_DEPTH_BUFFER_BIT);
            }

            while (!Glfw.WindowShouldClose(window))
            {
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
                    else
                    {
                        dimScreen(dimAlpha);
                    }

                    tex.setOpacity(0.25f);
                    tex.Draw(0, 0, gl_Width, gl_Height);
                }

                // Render Frame
                {
                    inst.ResetBuffer();
                    myPrimitive._LineInst.ResetBuffer();

                    for (int i = 0; i != list.Count; i++)
                    {
                        var obj = list[i] as myObj_590;

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

                    myPrimitive._LineInst.Draw();
                }

                if (list.Count < N)
                {
                    list.Add(new myObj_590());
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
            myPrimitive.init_LineInst(N);
            base.initShapes(2, N * n, 0);

            string h = "", m = "";
            my.myShaderHelpers.Shapes.getShader_000(ref rand, ref h, ref m);
            shader = new myFreeShader(h, m);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------
    }
};
