using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;


/*
    - Moving particles interconnected with each other
*/


namespace my
{
    public class myObj_310 : myObject
    {
        // ---------------------------------------------------------------------------------------------------------------

        private static int N = 0;
        private static int shapeType = 0, mode = 0, colorMode = 0;
        private static float dimAlpha = 0.05f, t = 0, dt = 0;

        private static short[] prm_i = new short[3];
        private static int max = 0, xRad = 666, yRad = 666, lineMode = 0, lineStyle = 0, slowFactor = 1, axisMode = 0;
        private static bool moveStep = false;
        private static bool doCreateAtOnce = false, isAggregateOpacity = false, isVerticalLine = false, isFastMoving = false, isRandomSize = false;

        private float x, y, dx, dy, size, r, g, b, a;
        private int offset = 0;
        private int shape = 0;

        private static float X = 0, Y = 0;

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
            xRad += myUtils.randomChance(rand, 1, 2) ? 0 : rand.Next(1234);

            X = gl_x0 + (float)Math.Sin(t) * xRad;
            Y = gl_y0 + (float)Math.Cos(t) * yRad;

            N = rand.Next(500) + 25;

            doClearBuffer = myUtils.randomChance(rand, 4, 5);
            doCreateAtOnce = myUtils.randomChance(rand, 1, 2);
            colorMode = rand.Next(4);
            lineMode = rand.Next(6);                                                // Interconnection lines drawing mode (affects distance and opacity factor calculation)
            lineStyle = rand.Next(9);                                               // Drawing style for interconnection lines (parallel vs crossed)
            slowFactor = rand.Next(5) + 1;                                          // Slowness factor for dx and/or dy
            axisMode = rand.Next(7);                                                // In a number of modes, will cause only vertical and/or horizontal movement of particles
            max = rand.Next(11) + 3;                                                // Particle size
            mode = rand.Next(12);
            mode = 8;

            isAggregateOpacity = myUtils.randomChance(rand, 1, 2);                  // Const opacity vs a sum of all particle's connecting line opacities
            isVerticalLine = myUtils.randomChance(rand, 1, 15);                     // Draw vertical lines
            isFastMoving = myUtils.randomChance(rand, 1, 2);                        // For large N and when slowFactor > 3, chance to have fast moving particles
            isRandomSize = myUtils.randomChance(rand, 1, 3);                        // Use particles of different size

            // Reset parameter values
            {
                for (int i = 0; i < prm_i.Length; i++)
                    prm_i[i] = 0;
            }

            dimAlpha /= (0.1f + 0.1f * rand.Next(20));
            dt = 0.025f;

            switch (mode)
            {
                case 08:
                    axisMode = 7;
                    doCreateAtOnce = false;
                    break;

                case 10:
                    prm_i[0] = (short)rand.Next(2);                                 // Generate particles in-screen or off-screen
                    break;

                case 11:
                    prm_i[0] = (short)(rand.Next(4) + 1);                           // Probability used in this mode; must be 1-2 or 8-9
                    prm_i[0] += (short)((prm_i[0] > 2) ? 5 : 0);
                    break;
            }

#if false
            N = 4;
            max = 20;
            lineMode = 0;
            lineStyle = 8;
            doClearBuffer = true;
#endif
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            height += 100;

            string str_params = "";
            for (int i = 0; i < prm_i.Length; i++)
            {
                str_params += i == 0 ? $"{prm_i[i]}" : $", {prm_i[i]}";
            }

            string str = $"Obj = myObj_310\n\n" +
                            $"N = {list.Count} of {N}\n" +
                            $"mode = {mode}\n" +
                            $"max = {max}\n" +
                            $"dimAlpha = {dimAlpha}\n" +
                            $"doClearBuffer = {doClearBuffer}\n" +
                            $"isAggregateOpacity = {isAggregateOpacity}\n" +
                            $"isVerticalLine = {isVerticalLine}\n" +
                            $"isFastMoving = {isFastMoving}\n" +
                            $"isRandomSize = {isRandomSize}\n" +
                            $"colorMode = {colorMode}\n" +
                            $"lineMode = {lineMode}\n" +
                            $"lineStyle = {lineStyle}\n" +
                            $"slowFactor = {slowFactor}\n" +
                            $"axisMode = {axisMode}\n" +
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

                case 05:
                    dx = (0.5f + 0.5f * rand.Next(13)) * myUtils.randomSign(rand);
                    dy = (0.5f + 0.5f * rand.Next(13)) * myUtils.randomSign(rand);
                    break;

                case 06:
                case 07:
                    dx = (0.5f + 0.5f * rand.Next(17)) * myUtils.randomSign(rand);
                    dy = (0.5f + 0.5f * rand.Next(17)) * myUtils.randomSign(rand);
                    break;

                case 08:
                    {
                        x = gl_x0;
                        y = gl_y0;

                        float dX = X - x;
                        float dY = Y - y;

                        float dist2 = (float)Math.Sqrt(dX * dX + dY * dY);
                        float sp_dist = (50 + rand.Next(100)) / dist2 / 10;

                        dx = dX * sp_dist;
                        dy = dY * sp_dist;
                    }
                    break;

                case 09:
                    dx = (0.5f + 0.5f * rand.Next(17)) * myUtils.randomSign(rand);
                    dy = (0.5f + 0.5f * rand.Next(17)) * myUtils.randomSign(rand);

                    offset = 100 + rand.Next(50 + N);

                    // Is modified into vertical/horizontal pattern a bit below
                    break;

                case 10:
                    offset = 100 + rand.Next(50 + N);

                    dx = (0.5f + 0.5f * rand.Next(99)) * myUtils.randomSign(rand);
                    dy = (0.5f + 0.5f * rand.Next(15)) * myUtils.randomSign(rand);

                    if (prm_i[0] == 1)
                    {
                        // generate particles off-screen -- fox x axis only
                        x = (dx > 0) ? -50 : gl_Width + 50;
                    }
                    break;

                case 11:
                    offset = 100 + rand.Next(50 + N);

                    dx = (0.5f + 0.5f * rand.Next(17)) * myUtils.randomSign(rand);
                    dy = (0.5f + 0.5f * rand.Next(17)) * myUtils.randomSign(rand);

                    // This translates to (1 out of 10), (2 out of 10), (8 out of 10) or (9 out of 10)
                    if (myUtils.randomChance(rand, prm_i[0], 10))
                    {
                        dx = 0;
                        dy *= 5;
                    }
                    else
                    {
                        dy = 0;
                        dx /= 5;
                    }
                    break;
            }

            // Transform movement into vertical and/or horizontal
            {
                if (mode < 9)
                {
                    switch (axisMode)
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

                        default:
                            break;
                    }
                }

                if (mode == 9)
                {
                    switch (axisMode)
                    {
                        case 0:
                        case 1:
                            dx = 0;
                            break;

                        case 2:
                        case 3:
                            dy = 0;
                            break;

                        case 4:
                            if (myUtils.randomChance(rand, 1, 7))
                                dx = 0;
                            else
                                dy = 0;
                            break;

                        case 5:
                            if (myUtils.randomChance(rand, 1, 7))
                                dy = 0;
                            else
                                dx = 0;
                            break;

                        default:
                            if (myUtils.randomChance(rand, 1, 2))
                                dx = 0;
                            else
                                dy = 0;
                            break;
                    }
                }
            }

            // Apply slowness factor
            if (N > 50 && isFastMoving && rand.Next(111) == 0)
            {
                dx *= 1.1f;
                dy *= 1.1f;
            }
            else
            {
                dx /= slowFactor;
                dy /= slowFactor;
            }

            // Get color
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

                case 3:
                    r = myUtils.randFloat(rand, 0.1f);
                    g = myUtils.randFloat(rand, 0.1f);
                    b = myUtils.randFloat(rand, 0.1f);
                    break;
            }

            a = 0.85f;
            size = isRandomSize ? rand.Next(max) + 3 : max;

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
            // Movement is calculated in 2 steps;
            //  - step 1 (moveStep == false) calculates all the dx/dy for each particle
            //  - step 2 (moveStep == true ) just recalcs all xs and ys
            if (moveStep == true)
            {
                x += dx;
                y += dy;

                if (id == 0)
                {
                    X = gl_x0 + (float)Math.Sin(t * 2.5f) * xRad;
                    Y = gl_y0 + (float)Math.Cos(t * 2.5f) * yRad;
                }
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

                    case 05:
                        if (x < -50)
                            x = gl_Width + 50;
                        else if (x > gl_Width + 50)
                            x = -50;

                        if (y < -50)
                            y = gl_Height + 50;
                        else if (y > gl_Height + 50)
                            y = -50;
                        break;

                    case 06:
                        if (x < -50 || x > gl_Width + 50 || y < -50 || y > gl_Height + 50)
                        {
                            x = gl_x0;
                            y = gl_y0;

                            if (myUtils.randomChance(rand, 1, 2))
                            {
                                myUtils.swap<float>(ref dx, ref dy);
                            }
                        }
                        break;

                    case 07:
                    case 08:
                        if (x < -50 || x > gl_Width + 50 || y < -50 || y > gl_Height + 50)
                        {
                            if (a > 1)
                                a = 1;

                            a -= 0.05f;

                            if (a <= 0)
                                generateNew();
                        }
                        break;

                    case 09:
                        if ((x < -offset && dx < 0) || (x > gl_Width + offset && dx > 0))
                            dx *= -1;

                        if ((y < -offset && dy < 0) || (y > gl_Height + offset && dy > 0))
                            dy *= -1;
                        break;

                    case 10:
                    case 11:
                        if ((x < -offset && dx < 0) || (x > gl_Width + offset && dx > 0))
                            generateNew();

                        if ((y < -offset && dy < 0) || (y > gl_Height + offset && dy > 0))
                            generateNew();
                        break;
                }
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Show()
        {
            if (false)
            {
                myPrimitive._Rectangle.SetColor(1, 1, 1, 1);
                myPrimitive._Rectangle.Draw(X - 10, Y - 10, 20, 20, true);
            }

            if (isAggregateOpacity)
                a = 0;

            // Render connecting lines
            for (int i = 0; i < list.Count; i++)
            {
                var obj = list[i] as myObj_310;

                float lineOpacity = 0.1f;

                if (obj != this)
                {
                    float xx = obj.x - x;
                    float yy = obj.y - y;
                    float dist2 = 0.0001f;

                    switch (lineMode)
                    {
                        case 0:
                            // Const opacity (adjusted for N)
                            {
                                if (N > 500)
                                    lineOpacity -= N * 0.000175f;
                                else if (N > 450)
                                    lineOpacity -= N * 0.000185f;
                                else if (N > 333)
                                    lineOpacity -= N * 0.00020f;
                                else
                                    lineOpacity -= N * 0.00025f;
                            }
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

                        case 5:
                            {
                                lineOpacity = 0;
                                int zzz = 234;

                                if (xx > -zzz && xx < zzz && yy > -zzz && yy < zzz)
                                {
                                    dist2 += (float)Math.Sqrt(xx * xx + yy * yy);
                                    lineOpacity = (float)(zzz / dist2 / 3);
                                }
                            }
                            break;
                    }

                    if (doClearBuffer == false)
                    {
                        lineOpacity /= (N < 300) ? 3 : 7;
                    }

                    if (lineOpacity > 0)
                    {
                        switch (lineStyle)
                        {
                            case 0:
                                myPrimitive._LineInst.setInstanceCoords(obj.x, obj.y, x, y);
                                break;

                            // Parallel
                            case 1:
                            case 2:
                            case 3:
                                if (obj.id < id)
                                    myPrimitive._LineInst.setInstanceCoords(obj.x + lineStyle, obj.y, x + lineStyle, y);
                                else
                                    myPrimitive._LineInst.setInstanceCoords(obj.x - lineStyle, obj.y, x - lineStyle, y);
                                break;

                            // Crossed
                            case 4:
                            case 5:
                            case 6:
                                myPrimitive._LineInst.setInstanceCoords(obj.x + lineStyle - 3, obj.y, x - lineStyle + 3, y);
                                break;

                            // Parallel, the TL and BR angles are connected
                            case 7:
                                if (obj.id < id)
                                    myPrimitive._LineInst.setInstanceCoords(obj.x - size + 1, obj.y - size + 1, x - size + 1, y - size + 1);
                                else
                                    myPrimitive._LineInst.setInstanceCoords(obj.x + size - 1, obj.y + size - 1, x + size - 1, y + size - 1);
                                break;

                            // Parallel, the TL and BR angles are connected
                            case 8:
                                myPrimitive._LineInst.setInstanceCoords(obj.x - size + 1, obj.y - size + 1, x + size - 1, y + size - 1);
                                break;
                        }

                        myPrimitive._LineInst.setInstanceColor(r, g, b, lineOpacity);
                    }

                    if (isAggregateOpacity)
                        a += lineOpacity/3;
                }
            }

            // Draw vertical lines
            if (isVerticalLine && N < 50)
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

            if (doCreateAtOnce)
                while (list.Count < N)
                    list.Add(new myObj_310());

/*
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
*/
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

                if (!doCreateAtOnce)
                    if (list.Count < N)
                        list.Add(new myObj_310());

                System.Threading.Thread.Sleep(renderDelay);
                t += dt;
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void initShapes()
        {
            myPrimitive.init_Rectangle();
            myPrimitive.init_LineInst(N * (N-1) + N * 2);

            base.initShapes(shapeType, 2*N, 0);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void Triangulate()
        {
/*
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
*/
        }

        // ---------------------------------------------------------------------------------------------------------------
    }
};
