using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;


/*
    -- Textures, Take 1
    https://www.youtube.com/watch?v=sA4p3wuDLo8&ab_channel=FranklyGaming
*/


namespace my
{
    public class myObj_330 : myObject
    {
        public float x, y, X, Y, dx, dy, a, da;
        public int width, height;

        private static int N = 1, max1 = 1, max2 = 1, opacityFactor = 1;
        private static int mode = 0;

        static bool doClearBuffer = false, doCreateAtOnce = true, doSampleOnce = false, doUseRandDxy = false, doDrawSrcImg = false;
        static float dimAlpha = 0.05f;

        static myTexRectangle tex = null;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_330()
        {
            if (colorPicker == null)
            {
                colorPicker = new myColorPicker(gl_Width, gl_Height, mode: myColorPicker.colorMode.SNAPSHOT_OR_IMAGE);
                list = new List<myObject>();

                init();
            }

            generateNew();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time initialization
        private void init()
        {
            gl_x0 = gl_Width  / 2;
            gl_y0 = gl_Height / 2;

            mode = rand.Next(4);
            opacityFactor = rand.Next(3) + 1 + (myUtils.randomChance(rand, 1, 7) ? rand.Next(3) : 0);

            doCreateAtOnce = myUtils.randomBool(rand);
            doClearBuffer  = myUtils.randomBool(rand);
            doUseRandDxy   = myUtils.randomBool(rand);
            doSampleOnce   = false;
            doDrawSrcImg   = false;

            //mode = 12;

            switch (mode)
            {
                case 0:
                    doClearBuffer = false;
                    dimAlpha /= 10;
                    N = 10;
                    break;

                case 1:
                    doClearBuffer = false;
                    dimAlpha /= 10;
                    max1 = 333 + rand.Next(666);
                    N = 10;
                    break;

                case 2:
                case 3:
                case 4:
                    doSampleOnce = doClearBuffer ? myUtils.randomBool(rand) : false;
                    dimAlpha /= 3;
                    N = 999 + rand.Next(666);
                    break;

                case 5:
                case 6:
                    dimAlpha /= 3;
                    max1 = rand.Next(45) + 3;
                    N = 999 + rand.Next(666);
                    break;

                case 7:
                    dimAlpha /= 3;
                    doClearBuffer = true;
                    N = 111 + rand.Next(666);
                    max1 = N > 450 ? 99 : 125;
                    break;

                case 8:
                    doClearBuffer = true;
                    N = 999 + rand.Next(111);
                    max1 = 50;
                    break;

                case 9:
                    if (rand.Next(3) == 0)
                    {
                        dimAlpha /= (rand.Next(11) + 1);
                    }
                    else
                    {
                        dimAlpha /= 5;
                    }

                    doClearBuffer = false;
                    doUseRandDxy = myUtils.randomChance(rand, 2, 3);
                    N = 999 + rand.Next(111);
                    max1 = rand.Next(50) + 25;

                    // Bluring effect strength
                    if (rand.Next(3) == 0)
                    {
                        max2 = rand.Next(23) + 1;
                    }
                    else
                    {
                        max2 = rand.Next(5) + 1;
                    }
                    break;

                case 10:
                    if (rand.Next(3) == 0)
                    {
                        dimAlpha /= (rand.Next(11) + 1);
                    }
                    else
                    {
                        dimAlpha /= 5;
                    }

                    doClearBuffer = false;
                    N = (999 + rand.Next(111)) / (rand.Next(11) + 1);
                    max1 = rand.Next(50) + 25;
                    break;

                case 11:
                    doClearBuffer = false;
                    N = (999 + rand.Next(111)) / (rand.Next(11) + 1);
                    max1 = rand.Next(150) + 25;
                    break;

                case 12:
                    N = 2000 + rand.Next(3333);
                    max1 = rand.Next(333) + 125;
                    break;
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            string str = $"Obj = myObj_330\n\n" +
                            $"N = {N} ({list.Count})\n" +
                            $"mode = {mode}\n" +
                            $"doClearBuffer = {doClearBuffer}\n" +
                            $"doSampleOnce  = {doSampleOnce}\n" +
                            $"opacityFactor = {opacityFactor}\n" +
                            $"doUseRandDxy  = {doUseRandDxy}\n"
            ;
            return str;
        }

        // ---------------------------------------------------------------------------------------------------------------

        // 
        protected override void setNextMode()
        {
            init();
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void generateNew()
        {
            a = (float)rand.NextDouble() / opacityFactor;

            switch (mode)
            {
                case 2:
                    width = rand.Next(33) + 5;
                    x = X = rand.Next(gl_Width);
                    y = Y = rand.Next(gl_Height);
                    dx = (float)rand.NextDouble() * myUtils.randomSign(rand) * 5;
                    dy = (float)rand.NextDouble() * myUtils.randomSign(rand) * 5;
                    break;

                case 3:
                    width = rand.Next(33) + 5;
                    x = X = rand.Next(gl_Width);
                    y = Y = rand.Next(gl_Height);
                    dx = dy = 0;

                    if (rand.Next(2) == 0)
                        dx = (float)rand.NextDouble() * myUtils.randomSign(rand) * 5;
                    else
                        dy = (float)rand.NextDouble() * myUtils.randomSign(rand) * 5;
                    break;

                case 4:
                    width = rand.Next(33) + 5;
                    x = X = rand.Next(gl_Width);
                    y = Y = rand.Next(gl_Height);
                    dx = (float)rand.NextDouble() * myUtils.randomSign(rand) * 5;
                    dy = (float)rand.NextDouble() * myUtils.randomSign(rand) * 5;

                    if (rand.Next(2) == 0)
                    {
                        dx = dy = 0;

                        if (rand.Next(2) == 0)
                            dx = (float)rand.NextDouble() * myUtils.randomSign(rand) * 5;
                        else
                            dy = (float)rand.NextDouble() * myUtils.randomSign(rand) * 5;
                    }
                    break;

                case 5:
                    width = rand.Next(max1) + 1;
                    x = X = rand.Next(gl_Width);
                    y = Y = gl_y0;
                    dx = 0;
                    dy = (float)rand.NextDouble() * myUtils.randomSign(rand) * 5;
                    da = (float)rand.NextDouble() / 25;
                    break;

                case 6:
                    width = rand.Next(max1) + 1;
                    x = X = rand.Next(gl_Width);
                    y = Y = rand.Next(gl_Height / 5);
                    dx = 0;
                    dy = (float)rand.NextDouble() * 5;
                    da = (float)rand.NextDouble() / 33;
                    break;

                case 7:
                    width = rand.Next(max1) + 1;
                    x = X = rand.Next(gl_Width);
                    y = Y = rand.Next(gl_Height);
                    dx = 0;
                    dy = 0;
                    da = (float)rand.NextDouble() / (rand.Next(N/10) + 1);

                    if (doUseRandDxy)
                    {
                        dx = myUtils.randomSign(rand) * (float)rand.NextDouble() / 33;
                        dy = myUtils.randomSign(rand) * (float)rand.NextDouble() / 33;
                    }

                    if (da < 0.0005f)
                        da = 0.0005f;

                    if (da > 0.01f)
                        da = 0.01f;

                    a = 0;
                    break;

                case 8:
                    a = (float)rand.NextDouble() / 3;
                    X = rand.Next(11) + 1;
                    break;

                case 9:
                case 10:
                    break;

                case 11:
                    width = height = rand.Next(max1) + 1;
                    x = X = rand.Next(gl_Width);
                    y = Y = rand.Next(gl_Height);
                    dx = myUtils.randomSign(rand) * (float)rand.NextDouble() * 5;
                    dy = myUtils.randomSign(rand) * (float)rand.NextDouble() * 5;
                    da = (float)rand.NextDouble() / 33;
                    break;

                case 12:
                    break;
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            switch (mode)
            {
                case 0:
                    x = X = rand.Next(gl_Width);
                    y = Y = rand.Next(gl_Height);
                    width = rand.Next(133) + 1;
                    height = rand.Next(133) + 1;
                    a = 133.0f / width / height;

                    if (doUseRandDxy)
                    {
                        x += myUtils.randomSign(rand) * rand.Next(3);
                        y += myUtils.randomSign(rand) * rand.Next(3);
                    }
                    break;

                case 1:
                    x = X = rand.Next(gl_Width);
                    y = Y = rand.Next(gl_Height);

                    if (rand.Next(2) == 0)
                    {
                        width = rand.Next(max1) + 1;
                        height = myUtils.randomChance(rand, 1, 11) ? 2 : 1;
                        a = 0.1f * max1 / width;
                        //width = gl_Width;
                    }
                    else
                    {
                        height = rand.Next(max1) + 1;
                        width = myUtils.randomChance(rand, 1, 11) ? 2 : 1;
                        a = 0.1f * max1 / height;
                        //height = gl_Height;
                    }

                    if (doUseRandDxy)
                    {
                        x += myUtils.randomSign(rand) * rand.Next(3);
                        y += myUtils.randomSign(rand) * rand.Next(3);
                    }
                    break;

                case 2:
                case 3:
                case 4:
                    if (doUseRandDxy && rand.Next(10) == 0)
                    {
                        dx += myUtils.randomSign(rand) * (float)rand.NextDouble();
                        dy += myUtils.randomSign(rand) * (float)rand.NextDouble();
                    }

                    x += dx;
                    y += dy;

                    if (x < 0 || x > gl_Width)
                        dx *= -1;

                    if (y < 0 || y > gl_Height)
                        dy *= -1;
                    break;

                case 5:
                case 6:
                    y += dy;
                    a -= da;

                    if (y < -width || y > gl_Height + width)
                    {
                        generateNew();
                        return;
                    }
                    break;

                case 7:
                    if (a > 0.75f && da > 0)
                        da *= -1;

                    x += dx;
                    y += dy;
                    a += da;
                    break;

                case 8:
                    if (--X <= 0)
                    {
                        width = rand.Next(max1) + 1;
                        height = rand.Next(max1) + 1;
                        x = rand.Next(gl_Width);
                        y = rand.Next(gl_Height);
                        X = rand.Next(66) + 1;
                    }
                    break;

                case 9:
                    width = rand.Next(max1) + 1;
                    height = rand.Next(max1) + 1;
                    x = X = rand.Next(gl_Width);
                    y = Y = rand.Next(gl_Height);

                    if (doUseRandDxy)
                    {
                        X += myUtils.randomSign(rand) * (rand.Next(2*max2+1) - max2);
                        Y += myUtils.randomSign(rand) * (rand.Next(2*max2+1) - max2);
                    }
                    break;

                case 10:
                    width = rand.Next(max1) + 1;
                    height = rand.Next(max1) + 1;
                    x = rand.Next(gl_Width);
                    y = rand.Next(gl_Height);
                    X = rand.Next(gl_Width);
                    Y = rand.Next(gl_Height);
                    break;

                case 11:
                    x += dx;
                    y += dy;
                    a -= da;
                    width -= 1;
                    height -= 1;

                    if (true)
                    {
                        dx += (float)rand.NextDouble() * myUtils.randomSign(rand);
                        dy += (float)rand.NextDouble() * myUtils.randomSign(rand);
                    }

                    if (x < 0 || x > gl_Width || y < 0 || y > gl_Height || width < 0 || height < 0)
                    {
                        generateNew();
                        return;
                    }
                    break;

                case 12:
                    tex.setOpacity(rand.NextDouble());
                    width = height = 1;
                    x = rand.Next(gl_Width);
                    y = rand.Next(gl_Height);

                    if (rand.Next(2) == 0)
                    {
                        if (rand.Next(9) == 0)
                            height++;
                        width += rand.Next(max1);
                    }
                    else
                    {
                        if (rand.Next(9) == 0)
                            width++;
                        height += rand.Next(max1);
                    }
                    break;
            }

            if (a <= 0)
            {
                generateNew();
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Show()
        {
            tex.setOpacity(a);

            switch (mode)
            {
                case 0:
                case 1:
                    tex.Draw((int)x, (int)y, width, height, (int)X, (int)Y, width, height);
                    break;

                case 2:
                case 3:
                case 4:
                    if (doSampleOnce)
                    {
                        tex.Draw((int)x - width, (int)y - width, 2 * width, 2 * width, (int)X - width, (int)X - width, 2 * width, 2 * width);
                    }
                    else
                    {
                        tex.Draw((int)x - width, (int)y - width, 2 * width, 2 * width, (int)x - width, (int)y - width, 2 * width, 2 * width);
                    }
                    break;

                case 5:
                case 6:
                case 7:
                    tex.Draw((int)x - width, (int)y - width, 2 * width, 2 * width, (int)x - width, (int)y - width, 2 * width, 2 * width);
                    break;

                case 8:
                    tex.Draw((int)x - width, (int)y - height, 2 * width, 2 * height, (int)x - width, (int)y - height, 2 * width, 2 * height);
                    break;

                case 9:
                case 10:
                    tex.Draw((int)X - width, (int)Y - height, 2 * width, 2 * height, (int)x - width, (int)y - height, 2 * width, 2 * height);
                    break;

                case 11:
                case 12:
                    tex.Draw((int)x, (int)y, width, height, (int)x, (int)y, width, height);
                    break;
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Process(Window window)
        {
            int maxCnt = 333;
            int cnt = rand.Next(maxCnt) + 11;
            int mode = rand.Next(2);

            initShapes();

            if (doClearBuffer)
            {
                glDrawBuffer(GL_FRONT_AND_BACK | GL_DEPTH_BUFFER_BIT);
                glClearColor(0, 0, 0, 1);
            }
            else
            {
                glDrawBuffer(GL_FRONT_AND_BACK);
            }

            if (doDrawSrcImg)
            {
                tex.Draw(0, 0, gl_Width, gl_Height);
            }

            if (doCreateAtOnce)
            {
                for (int i = 0; i < N; i++)
                {
                    list.Add(new myObj_330());
                }
            }

            while (!Glfw.WindowShouldClose(window))
            {
                processInput(window);

                Glfw.SwapBuffers(window);
                Glfw.PollEvents();

                if (doClearBuffer)
                {
                    glClear(GL_COLOR_BUFFER_BIT);
                }
                else
                {
                    dimScreen(dimAlpha, false);

                    if (false)
                    {
                        if (cnt > 0)
                            cnt--;
                        else
                        {
                            cnt = rand.Next(maxCnt) + 11;
                            glClear(GL_COLOR_BUFFER_BIT);
                        }
                    }
                }

                // Render Frame
                {
                    for (int i = 0; i < list.Count; i++)
                    {
                        var obj = list[i] as myObj_330;

                        obj.Move();
                        obj.Show();
                    }
                }

                if (list.Count < N)
                {
                    list.Add(new myObj_330());
                }

                System.Threading.Thread.Sleep(renderDelay);
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void initShapes()
        {
            myPrimitive.init_Rectangle();
            tex = new myTexRectangle(colorPicker.getImg());
        }

        // ---------------------------------------------------------------------------------------------------------------

        // Dim the screen constantly
        private void dimScreen(float dimAlpha, bool useStrongerDimFactor = false)
        {
            int rnd = rand.Next(101), dimFactor = 1;

            if (useStrongerDimFactor && rnd < 11)
            {
                dimFactor = (rnd == 0) ? 5 : 2;
            }

            myPrimitive._Rectangle.SetAngle(0);

            // Shift background color just a bit, to hide long lasting traces of shapes
            myPrimitive._Rectangle.SetColor(rand.Next(5) * 0.01f, rand.Next(5) * 0.01f, rand.Next(5) * 0.01f, dimAlpha * dimFactor);
            myPrimitive._Rectangle.Draw(0, 0, gl_Width, gl_Height, true);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------
    }
};
