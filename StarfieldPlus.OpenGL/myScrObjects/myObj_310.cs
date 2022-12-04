﻿using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;


/*
    - Triangulation (experimental unfinished)
*/


namespace my
{
    public class myObj_310 : myObject
    {
        // ---------------------------------------------------------------------------------------------------------------

        private static int N = 0;
        private static int shapeType = 0, mode = 0, colorMode = 0;
        private static float dimAlpha = 0.05f, t = 0, dt = 0;

        static int[] prm_i = new int[5];
        static int max = 0;
        static bool moveStep = false;

        private float x, y, dx, dy, size, r, g, b, a;
        private int shape = 0;

        private myObj_310 left = null, right = null;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_310()
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
            N = rand.Next(500) + 25;

            doClearBuffer = myUtils.randomChance(rand, 1, 2);
            colorMode = rand.Next(3);
            max = rand.Next(3) + 3;
            mode = rand.Next(5);

            // Reset parameter values
            {
                for (int i = 0; i < prm_i.Length; i++)
                    prm_i[i] = 0;
            }

            prm_i[0] = rand.Next(5);                                                // Interconnection lines drawing mode
            prm_i[1] = rand.Next(15);                                               // Draw vertical lines (in case of 1)
            prm_i[2] = rand.Next(5) + 1;                                            // Slowness factor for dx/dy
            prm_i[3] = rand.Next(7);                                                // Drawing style for interconnection lines (parallel vs crossed)
            prm_i[4] = rand.Next(7);                                                // In modes 0-2, dx or dy will be zero

            switch (mode)
            {
                case 00:
                case 01:
                case 02:
                case 03:
                case 04:
                    dimAlpha /= (0.1f  + 0.1f * rand.Next(20));
                    dt = 0.025f;
                    break;
            }

            N += 4;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            string str_params = "";

            for (int i = 0; i < prm_i.Length; i++)
            {
                str_params += i == 0 ? $"{prm_i[i]}" : $", {prm_i[i]}";
            }

            string str = $"Obj = myObj_310\n\n" +
                            $"N = {N}\n" +
                            $"mode = {mode}\n" +
                            $"dimAlpha = {dimAlpha}\n" +
                            $"doClearBuffer = {doClearBuffer}\n" +
                            $"colorMode = {colorMode}\n" +
                            $"param: [{str_params}]\n\n" +
                            $"file: {colorPicker.GetFileName()}" + 
                            $""
            ;
            return str;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void generateNew()
        {
            x = rand.Next(gl_Width);
            y = rand.Next(gl_Height);

            switch (mode)
            {
                case 00:
                case 01:
                    dx = (rand.Next(111) + 11) * 0.1f * myUtils.randomSign(rand);
                    dy = (rand.Next(111) + 11) * 0.1f * myUtils.randomSign(rand);
                    break;

                case 02:
                case 03:
                case 04:
                    dx = myUtils.randFloat(rand, 0.1f) * (rand.Next(50) + 1) * myUtils.randomSign(rand);
                    dy = myUtils.randFloat(rand, 0.1f) * (rand.Next(50) + 1) * myUtils.randomSign(rand);
                    break;
            }

            switch (colorMode)
            {
                case 0:
                    r = g = b = 1.0f;
                    break;

                case 1:
                    r = 1.0f - myUtils.randFloat(rand) / 10;
                    g = 1.0f - myUtils.randFloat(rand) / 10;
                    b = 1.0f - myUtils.randFloat(rand) / 10;
                    break;

                case 2:
                    colorPicker.getColor(x, y, ref r, ref g, ref b);
                    break;
            }

            switch (prm_i[4])
            {
                case 0:
                    dx = 0.0f;
                    break;

                case 1:
                    dy = 0.0f;
                    break;

                case 2:
                    if (myUtils.randomChance(rand, 1, 2))
                        dx = 0;
                    else
                        dy = 0;
                    break;
            }

            dx /= prm_i[2];
            dy /= prm_i[2];

            a = 0.85f;
            size = max;

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void setNextMode()
        {
            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            if (moveStep == true)
            {
                x += dx;
                y += dy;
            }
            else
            {
                switch (mode)
                {
                    case 00:
                        if (x < 0 || x > gl_Width)
                            dx *= -1;

                        if (y < 0 || y > gl_Height)
                            dy *= -1;
                        break;

                    case 01:
                        if (x < -6666 || x > gl_Width + 6666)
                            dx *= -1;

                        if (y < -6666 || y > gl_Height + 6666)
                            dy *= -1;
                        break;

                    case 02:
                        {
                            float factor = 0.25f;
                            int offset = 111;

                            if (x < offset)
                                dx += myUtils.randFloat(rand) * factor;

                            if (x > gl_Width - offset)
                                dx -= myUtils.randFloat(rand) * factor;

                            if (y < offset)
                                dy += myUtils.randFloat(rand) * factor;

                            if (y > gl_Height - offset)
                                dy -= myUtils.randFloat(rand) * factor;
                        }
                        break;

                    case 03:
                        {
                            int chance = N * 2;

                            if (x < 0 && dx < 0 && myUtils.randomChance(rand, 1, chance))
                                dx *= -1;

                            if (x > gl_Width && dx > 0 && myUtils.randomChance(rand, 1, chance))
                                dx *= -1;

                            if (y < 0 && dy < 0 && myUtils.randomChance(rand, 1, chance))
                                dy *= -1;

                            if (y > gl_Height && dy > 0 && myUtils.randomChance(rand, 1, chance))
                                dy *= -1;
                        }
                        break;

                    case 04:
                        {
                            int offset = N * 3;

                            dx += myUtils.randomSign(rand) * 0.01f * rand.Next(50);
                            dy += myUtils.randomSign(rand) * 0.01f * rand.Next(50);

                            if (x < -offset || x > gl_Width + offset)
                                dx *= -1;

                            if (y < -offset || y > gl_Height + offset)
                                dy *= -1;
                        }
                        break;
                }
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Show()
        {
            float lineOpacity = 0.1f;

            // Render connecting lines
            for (int i = 0; i < list.Count; i++)
            {
                var obj = list[i] as myObj_310;

                if (obj != this)
                {
                    float xx = obj.x - x;
                    float yy = obj.y - y;
                    float dist2 = 0.0001f;

                    switch (prm_i[0])
                    {
                        case 0:
                            // Const opacity
                            break;

                        case 1:
                            dist2 += xx * xx + yy * yy;
                            lineOpacity = (float)(10000.0 / dist2);
                            break;

                        case 2:
                            dist2 += xx * xx + yy * yy;
                            lineOpacity = (float)(20000.0 / dist2);
                            break;

                        case 3:
                            dist2 += xx * xx + yy * yy;
                            lineOpacity = (float)(20000.0 / dist2) + 0.05f;
                            break;

                        case 4:
                            dist2 += (float)Math.Sqrt(xx * xx + yy * yy);

                            if (N > 300)
                                lineOpacity = (float)((gl_Height * 0.01f) / dist2);
                            else if (N > 100)
                                lineOpacity = (float)((gl_Height * 0.02f) / dist2);
                            else if (N > 50)
                                lineOpacity = (float)((gl_Height * 0.04f) / dist2);
                            else
                                lineOpacity = (float)((gl_Height * 0.05f) / dist2);
                            break;
                    }

                    if (doClearBuffer == false)
                    {
                        lineOpacity /= (N < 300) ? 3 : 7;
                    }

                    switch (prm_i[3])
                    {
                        case 0:
                            myPrimitive._LineInst.setInstanceCoords(obj.x, obj.y, x, y);
                            break;

                        // Parallel
                        case 1:
                        case 2:
                        case 3:
                            if (obj.id < id)
                                myPrimitive._LineInst.setInstanceCoords(obj.x + prm_i[3], obj.y, x + prm_i[3], y);
                            else
                                myPrimitive._LineInst.setInstanceCoords(obj.x - prm_i[3], obj.y, x - prm_i[3], y);
                            break;

                        // Crossed
                        case 4:
                        case 5:
                        case 6:
                            myPrimitive._LineInst.setInstanceCoords(obj.x + prm_i[3] - 3, obj.y, x - prm_i[3] + 3, y);
                            break;
                    }

                    myPrimitive._LineInst.setInstanceColor(r, g, b, lineOpacity);
                }
            }

            // Draw vertical lines
            if (prm_i[1] == 1 && N < 50)
            {
                myPrimitive._LineInst.setInstanceCoords(x, 0, x, gl_Height);
                myPrimitive._LineInst.setInstanceColor(1, 1, 1, 0.13f);

                myPrimitive._LineInst.setInstanceCoords(0, y, gl_Width, y);
                myPrimitive._LineInst.setInstanceColor(1, 1, 1, 0.13f);
            }

            switch (shape)
            {
                // Instanced squares
                case 0:
                    var rectInst = inst as myRectangleInst;

                    for (int i = 0; i < 2; i++)
                    {
                        int val1 = (int)(size - 2 * i);
                        int val2 = val1 * 2;

                        rectInst.setInstanceCoords(x - val1, y - val1, val2, val2);
                        rectInst.setInstanceColor(r, g, b, i == 0 ? a/2 : a);
                        rectInst.setInstanceAngle(i == 0 ? t : 0);
                    }
                    break;
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Process(Window window)
        {
            initShapes();

            //Glfw.SwapInterval(0);

            while (list.Count < N - 4)
            {
                list.Add(new myObj_310());
            }

            list.Add(new myObj_310());
            (list[list.Count - 1] as myObj_310).x = 0;
            (list[list.Count - 1] as myObj_310).y = 0;

            list.Add(new myObj_310());
            (list[list.Count - 1] as myObj_310).x = 0;
            (list[list.Count - 1] as myObj_310).y = gl_Height;

            list.Add(new myObj_310());
            (list[list.Count - 1] as myObj_310).x = gl_Width;
            (list[list.Count - 1] as myObj_310).y = 0;

            list.Add(new myObj_310());
            (list[list.Count - 1] as myObj_310).x = gl_Width;
            (list[list.Count - 1] as myObj_310).y = gl_Height;

            if (doClearBuffer)
            {
                glDrawBuffer(GL_FRONT_AND_BACK | GL_DEPTH_BUFFER_BIT);

                float r = (float)rand.NextDouble() / 13;
                float g = (float)rand.NextDouble() / 13;
                float b = (float)rand.NextDouble() / 13;

                glClearColor(r, g, b, 1.0f);
            }
            else
            {
                glDrawBuffer(GL_FRONT_AND_BACK);
            }

            while (!Glfw.WindowShouldClose(window))
            {
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
                    dimScreen(dimAlpha, false, false);
                }

                // Render Frame
                {
                    inst.ResetBuffer();
                    myPrimitive._LineInst.ResetBuffer();

                    moveStep = true;

                    for (int i = 0; i < list.Count; i++)
                    {
                        var obj = list[i] as myObj_310;
                        obj.Move();
                    }

                    moveStep = false;

                    for (int i = 0; i < list.Count; i++)
                    {
                        var obj = list[i] as myObj_310;
                        obj.Show();
                        obj.Move();
                    }

                    myPrimitive._LineInst.Draw();

                    inst.SetColorA(0);
                    inst.Draw(false);
                }

                System.Threading.Thread.Sleep(renderDelay);
                t += dt;
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void initShapes()
        {
            myPrimitive.init_Rectangle();
            myPrimitive.init_LineInst(N * N + N * 2);

            base.initShapes(shapeType, 2*N, 0);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void Triangulate()
        {
            for (int i = 0; i < list.Count; i++)
            {
                var obj = list[i] as myObj_310;

                if (obj.left != null || obj.right != null)
                {
                    for (int j = 0; j < list.Count; j++)
                    {
                        if (i != j)
                        {
                            var other = list[i] as myObj_310;
                            float dist = (obj.x - other.x) * (obj.x - other.x) + (obj.y - other.y) * (obj.y - other.y);
                        }
                    }
                }
            }
        }

        // ---------------------------------------------------------------------------------------------------------------
    }
};
