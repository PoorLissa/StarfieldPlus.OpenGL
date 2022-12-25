using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;


/*
    1. moving vertical and horizontal lines
    2. moving vertical and horizontal sin/cos functions
*/


namespace my
{
    public class myObj_120 : myObject
    {
        private int length, dir;
        private float x, y, dx, dy, size, freqFactor, A, R, G, B;

        private static int N = 0, mode = 0, dirMode = 0, freqMode = 0, dtMode = 0, minLength = 100;
        private static float dimAlpha = 0, t = 0, dt = 0.01f;

        private static int di = 1;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_120()
        {
            generateNew();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time global initialization
        protected override void initGlobal()
        {
            colorPicker = new myColorPicker(gl_Width, gl_Height);
            list = new List<myObject>();

            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time local initialization
        private void initLocal()
        {
            N = 33;

            switch (rand.Next(3))
            {
                case 0: N += rand.Next(0333); break;
                case 1: N += rand.Next(1333); break;
                case 2: N += rand.Next(2333); break;
                case 3: N += rand.Next(3333); break;
            }

            di = rand.Next(100) + 10;

            doClearBuffer = myUtils.randomChance(rand, 1, 2);
            dimAlpha = 0.05f;

            mode = rand.Next(2);
            dirMode = rand.Next(16);
            freqMode = rand.Next(50);
            dtMode = rand.Next(3);
            dtMode = rand.Next(15);

mode = 2;
dirMode = 0;

            if (mode == 1)
            {
                minLength = 333 + rand.Next(333);
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            height = 800;

            string str = $"Obj = myObj_120\n\n" +
                            $"N = {list.Count} of {N}\n" +
                            $"doClearBuffer = {doClearBuffer}\n" +
                            $"mode = {mode}\n" +
                            $"dirMode = {dirMode}\n" +
                            $"freqMode = {freqMode}\n" +
                            $"dtMode = {dtMode}\n" +
                            $"di = {di}\n" +
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
            x = rand.Next(gl_Width);
            y = rand.Next(gl_Height);

            length = rand.Next(gl_Width + gl_Height) + minLength;
            size = rand.Next(111) + 3;

            switch (dirMode)
            {
                case 0: case 1: case 2: case 3:
                    dir = dirMode;
                    break;

                case 4: case 5: case 6: case 7:
                    dir = rand.Next(2);
                    break;

                case 8: case 9: case 10: case 11:
                    dir = rand.Next(2) + 2;
                    break;

                case 12: case 13: case 14: case 15:
                    dir = rand.Next(4);
                    break;
            }

            colorPicker.getColorRand(ref R, ref G, ref B);
            A = myUtils.randFloat(rand);

            switch (N / 500)
            {
                case 0:
                    break;

                case 1: if (myUtils.randomChance(rand, 1, 2))
                    A *= mode == 0 ? 0.5f : 0.35f;
                    break;

                case 2: if (myUtils.randomChance(rand, 2, 3))
                    A *= mode == 0 ? 0.5f : 0.35f;
                    break;

                case 3: if (myUtils.randomChance(rand, 3, 4))
                    A *= mode == 0 ? 0.5f : 0.35f;
                    break;

                case 4: if (myUtils.randomChance(rand, 4, 5))
                    A *= mode == 0 ? 0.5f : 0.35f;
                    break;

                case 5: if (myUtils.randomChance(rand, 5, 6))
                    A *= mode == 0 ? 0.5f : 0.35f;
                    break;

                case 6: if (myUtils.randomChance(rand, 6, 7))
                    A *= mode == 0 ? 0.5f : 0.35f;
                    break;

                default:
                    A *= 0.25f;
                    break;
            }

            switch (dir)
            {
                // Left to right
                case 0:
                    dx = 1;
                    dy = 0;
                    x = -33 - length - rand.Next(gl_Width);
                    break;

                // Right to left
                case 1:
                    dx = -1;
                    dy = 0;
                    x = gl_Width + 33 + length + rand.Next(gl_Width);
                    break;

                // Top to botttom
                case 2:
                    dx = 0;
                    dy = 1;
                    y = -33 - length - rand.Next(gl_Height);
                    break;

                // Bottom to top
                case 3:
                    dx = 0;
                    dy = -1;
                    y = gl_Height + 33 + length + rand.Next(gl_Height);
                    break;
            }

            dx *= (rand.Next(20) + 1) * myUtils.randFloat(rand, 0.1f);
            dy *= (rand.Next(20) + 1) * myUtils.randFloat(rand, 0.1f);

            switch (freqMode)
            {
                case 0: case 1: case 2: case 3: case 4: case 5: case 6: case 7: case 8: case 9:
                    freqFactor = di / (freqMode + 1);
                    dx /= 5;
                    dy /= 5;
                    break;

                case 10: case 11: case 12: case 13: case 14: case 15: case 16: case 17: case 18: case 19:
                    freqFactor = rand.Next(333) + 1;
                    break;

                case 20: case 21: case 22: case 23: case 24: case 25: case 26: case 27: case 28: case 29:
                    freqFactor = di + rand.Next(150);
                    break;

                case 30: case 31: case 32: case 33: case 34: case 35: case 36: case 37: case 38: case 39:
                    freqFactor = (freqMode - 30 + 1) * 50;
                    break;

                case 40: case 41: case 42: case 43: case 44:
                    freqFactor = (rand.Next(10) + 1) * 25;
                    break;

                case 45: case 46: case 47: case 48: case 49:
                    freqFactor = (rand.Next(20) + 1) * 25;
                    break;
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            x += dx;
            y += dy;

            switch (dtMode)
            {
                case 0:
                    break;

                case 1:
                    size += (int)(Math.Sin(t/2) * 1.05f);
                    break;

                case 2:
                    size += (int)(Math.Sin(t/2 + id) * 1.05f);
                    break;

                case 3:
                    size += (int)(Math.Sin(t/2 * id) * 1.05f);
                    break;

                case 4:
                    size += (int)(Math.Sin(t) * 1.005f);
                    break;

                case 5:
                    size += (int)(Math.Sin(t + id) * 1.005f);
                    break;

                case 6:
                    size += (int)(Math.Sin(t * id) * 1.005f);
                    break;

                case 7:
                    size += (float)(Math.Sin(t) * 1.05f);
                    break;

                case 8:
                    size += (float)(Math.Sin(t + id) * 1.05f);
                    break;

                case 9:
                    size += (float)(Math.Sin(t * id) * 1.05f);
                    break;

                case 10:
                    size += (float)(Math.Sin(t) * 1.005f);
                    break;

                case 11:
                    size += (float)(Math.Sin(t + id) * 1.005f);
                    break;

                case 12:
                    size += (float)(Math.Sin(t * id) * 1.005f);
                    break;

                case 13:
                    size += (float)(Math.Sin(t) * id * 1.005f);
                    break;

                case 14:
                    size += (float)(Math.Sin(t) * id * 0.001f);
                    break;
            }

            if ((dir == 0 && x > gl_Width) || (dir == 1 && x < 0) || (dir == 2 && y > gl_Height) || (dir == 3 && y < 0))
            {
                generateNew();
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Show()
        {
            float x2 = 0, y2 = 0;

            switch (dir)
            {
                case 0:
                    x2 = x + length;
                    y2 = y;
                    break;

                case 1:
                    x2 = x - length;
                    y2 = y;
                    break;

                case 2:
                    x2 = x;
                    y2 = y + length;
                    break;

                case 3:
                    x2 = x;
                    y2 = y - length;
                    break;
            }

            switch (mode)
            {
                // Straight lines
                case 0:
                    myPrimitive._LineInst.setInstanceCoords(x, y, x2, y2);
                    myPrimitive._LineInst.setInstanceColor(R, G, B, A);
                    break;

                // Sin function
                case 1:
                    {
                        myPrimitive._LineInst.ResetBuffer();

                        float oldx = 0, oldy = 0, newx = 0, newy = 0;

                        if (dir == 0)
                        {
                            oldx = x;
                            oldy = y + (float)Math.Sin(x / freqFactor) * size;

                            for (int i = (int)x; i < x2; i += di)
                            {
                                newx = i;
                                newy = y + (float)Math.Sin(newx / freqFactor) * size;

                                myPrimitive._LineInst.setInstanceCoords(oldx, oldy, newx, newy);
                                myPrimitive._LineInst.setInstanceColor(R, G, B, A);

                                oldx = newx;
                                oldy = newy;
                            }
                        }

                        if (dir == 1)
                        {
                            oldx = x2;
                            oldy = y2 + (float)Math.Sin(x2 / freqFactor) * size;

                            for (int i = (int)x2; i < x; i += di)
                            {
                                newx = i;
                                newy = y2 + (float)Math.Sin(newx / freqFactor) * size;

                                myPrimitive._LineInst.setInstanceCoords(oldx, oldy, newx, newy);
                                myPrimitive._LineInst.setInstanceColor(R, G, B, A);

                                oldx = newx;
                                oldy = newy;
                            }
                        }

                        if (dir == 2)
                        {
                            oldy = y;
                            oldx = x + (float)Math.Sin(y / freqFactor) * size;

                            for (int i = (int)y; i < y2; i += di)
                            {
                                newy = i;
                                newx = x + (float)Math.Sin(newy / freqFactor) * size;

                                myPrimitive._LineInst.setInstanceCoords(oldx, oldy, newx, newy);
                                myPrimitive._LineInst.setInstanceColor(R, G, B, A);

                                oldx = newx;
                                oldy = newy;
                            }
                        }

                        if (dir == 3)
                        {
                            oldy = y2;
                            oldx = x2 + (float)Math.Sin(y2 / freqFactor) * size;

                            for (int i = (int)y2; i < y; i += di)
                            {
                                newy = i;
                                newx = x2 + (float)Math.Sin(newy / freqFactor) * size;

                                myPrimitive._LineInst.setInstanceCoords(oldx, oldy, newx, newy);
                                myPrimitive._LineInst.setInstanceColor(R, G, B, A);

                                oldx = newx;
                                oldy = newy;
                            }
                        }

                        myPrimitive._LineInst.Draw();
                    }
                    break;

                case 2:
                    {
                        myPrimitive._LineInst.ResetBuffer();

                        float oldx = 0, oldy = 0, newx = 0, newy = 0;

                        if (dir == 0)
                        {
                            oldx = x;
                            oldy = y + (float)Math.Sin(x / freqFactor) * size;

                            for (int i = (int)x; i < x2; i += di)
                            {
                                newx = i;
                                newy = y + (float)Math.Sin(newx / freqFactor) * size;

                                myPrimitive._LineInst.setInstanceCoords(oldx, oldy, newx, newy);
                                myPrimitive._LineInst.setInstanceColor(R, G, B, A);

                                oldx = newx;
                                oldy = newy;
                            }
                        }

                        myPrimitive._LineInst.Draw();
                    }
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
                    dimScreen(dimAlpha, doShiftColor: true);
                }

                // Render Frame
                {
                    myPrimitive._LineInst.ResetBuffer();

                    for (int i = 0; i < list.Count; i++)
                    {
                        var obj = list[i] as myObj_120;

                        obj.Show();
                        obj.Move();
                    }

                    myPrimitive._LineInst.Draw();
                }

                if (list.Count < N)
                {
                    list.Add(new myObj_120());
                }

                System.Threading.Thread.Sleep(renderDelay);

                t += dt;
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void initShapes()
        {
            int lineInstQty = (gl_Width + gl_Height + minLength) / di + 3;

            myPrimitive.init_LineInst(N > lineInstQty ? N : lineInstQty);
            myPrimitive.init_Rectangle();

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------
    }
};
