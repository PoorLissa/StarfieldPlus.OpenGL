using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;


/*
    - Randomly Roaming Shapes (Snow Like)
*/


namespace my
{
    public class myObj_010 : myObject
    {
        private float x, y, dx, dy, Size, angle, dAngle, xOld, yOld, xOrig, yOrig;
        private float A = 0, R = 0, G = 0, B = 0;

        private static bool doFillShapes = false, doConnect = false, doUseGravityAnomaly = false;
        private static int x0, y0, minX = 0, minY = 0, maxX = 0, maxY = 0, maxSize = 0, N = 1, moveMode = 0;
        private static int shapeType = 0, rotationMode = 0, rotationSubMode = 0, connectType = 0, connectColorType = 0, gravityMode = 0;
        private static float dimAlpha = 0.25f;
        static float radius = gl_Height / 3;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_010()
        {
            if (colorPicker == null)
            {
                colorPicker = new myColorPicker(gl_Width, gl_Height);
                list = new List<myObject>();

                // In case the colorPicker points to an image, try something different
/*
                if (colorPicker.getMode() < 2)
                {
                    showMode = rand.Next(2);
                }
*/
                moveMode = rand.Next(3);

                doClearBuffer = myUtils.randomBool(rand);
                doFillShapes  = myUtils.randomBool(rand);
                doConnect     = myUtils.randomBool(rand);
                doUseGravityAnomaly = myUtils.randomBool(rand);

                // rotationMode: 0, 1 = rotation; 2 = no rotation, angle is 0; 3 = no rotation, angle is not 0
                rotationMode = rand.Next(4);
                shapeType    = rand.Next(5);
                connectType  = rand.Next(10);
                gravityMode  = rand.Next(3);
                connectColorType = rand.Next(2);

                // In case the rotation is enabled, we also may enable additional rotation option:
                if (rotationMode < 2)
                {
                    rotationSubMode = rand.Next(7);
                    rotationSubMode = rotationSubMode > 2 ? 0 : rotationSubMode + 1;     // [0, 1, 2] --> [1, 2, 3]; otherwise set to '0';
                }

                // In case the border is wider than the screen's bounds, the movement looks a bit different (no bouncing)
                int offset = 0;

                switch (rand.Next(3))
                {
                    case 0:
                        offset = 0;
                        break;

                    case 1:
                        offset = 100 + rand.Next(500);
                        break;

                    case 2:
                        offset = -rand.Next(500);
                        break;
                }

                minX = 0 - offset;
                minY = 0 - offset;
                maxX = gl_Width  + offset;
                maxY = gl_Height + offset;

                maxSize = myUtils.randomChance(rand, 1, 10) ? 33 : 11;

                x0 = gl_Width /2;
                y0 = gl_Height/2;

                if (myUtils.randomBool(rand))
                {
                    x0 = rand.Next(gl_Width);
                    y0 = rand.Next(gl_Height);
                }

                if (connectType == 0)
                {
                    N = 333;
                }
                else
                {
                    switch(rand.Next(3))
                    {
                        case 0: N = 500;   break;
                        case 1: N = 5000;  break;
                        case 2: N = 50000; break;
                    }
                }

                renderDelay = 10;
            }

            generateNew();
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            string str = $"Obj = myObj_010\n\n" +
                            $"N = {list.Count} of {N}\n" +
                            $"renderDelay = {renderDelay}\n" +
                            $"shapeType = {shapeType}\n" + 
                            $"rotationMode = {rotationMode}\n" +
                            $"rotationSubMode = {rotationSubMode}\n" +
                            $"doConnect = {doConnect}\n" +
                            $"connectType = {connectType}\n" +
                            $"doUseGravity = {doUseGravityAnomaly}\n" + 
                            $"gravityMode = {gravityMode}";
            return str;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void generateNew()
        {
            x = rand.Next(gl_Width);
            y = rand.Next(gl_Height);

            xOld = x;
            yOld = y;

            xOrig = x;
            yOrig = y;

            int maxSpeed = 2000;

            dx = 0.01f * (rand.Next(maxSpeed) + 1) * myUtils.randomSign(rand);
            dy = 0.01f * (rand.Next(maxSpeed) + 1) * myUtils.randomSign(rand);

            if (moveMode == 2)
            {
                switch (rand.Next(2))
                {
                    case 0:
                        dx = 0;
                        break;

                    case 1:
                        dy = 0;
                        break;
                }
            }

            Size = rand.Next(maxSize) + 1;

            A = (float)rand.NextDouble();

            colorPicker.getColor(x, y, ref R, ref G, ref B);

            angle = 0;
            dAngle = rotationMode < 2 ? 0.001f * rand.Next(111) * myUtils.randomSign(rand) : 0;

            // There will be no rotation, but the angle is set to non-zero
            if (rotationMode == 3)
            {
                angle = (float)rand.NextDouble() * 111;
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            x += dx;
            y += dy;
            angle += dAngle;

            if (doUseGravityAnomaly)
            {
                if (x > x0 - radius && x < x0 + radius && y > y0 - radius && y < y0 + radius)
                {
                    float dist2 = (x - x0) * (x - x0) + (y - y0) * (y - y0);

                    if (dist2 < radius * radius)
                    {
                        float ddx, ddy, sqrt = (float)Math.Sqrt(dist2);

                        switch (gravityMode)
                        {
                            case 0:
                                ddx = (x - x0) / (dist2 / radius / 1.0f);
                                ddy = (y - y0) / (dist2 / radius / 1.0f);
                                dx -= ddx;
                                dy -= ddy;
                                break;

                            case 1:
                                ddx = (x - x0) / (dist2 / radius / 10.0f);
                                ddy = (y - y0) / (dist2 / radius / 10.0f);
                                x += ddx;
                                y += ddy;
                                break;

                            case 2:
                                ddx = (x - x0) / (dist2 / 50.0f);
                                ddy = (y - y0) / (dist2 / 50.0f);
                                dx += ddx;
                                dy += ddy;
                                break;

                            case 3:
                                ddx = (x - x0) / (sqrt / 5.0f);
                                ddy = (y - y0) / (sqrt / 5.0f);
                                x -= ddx;
                                y -= ddy;
                                break;
                        }
                    }

                    return;
                }
            }

            switch (moveMode)
            {
                case 0:
                    {
                        if (x < minX || x > maxX)
                        {
                            dx *= -1;
                            xOld = x;
                            yOld = y;
                        }

                        if (y < minY || y > maxY)
                        {
                            dy *= -1;
                            xOld = x;
                            yOld = y;
                        }
                    }
                    break;

                case 1:
                case 2:
                    {
                        if (x < minX)
                        {
                            dx += myUtils.randFloat(rand, 0.1f) * 0.1f;
                            xOld = x;
                            yOld = y;
                        }

                        if (x > maxX)
                        {
                            dx -= myUtils.randFloat(rand, 0.1f) * 0.1f;
                            xOld = x;
                            yOld = y;
                        }

                        if (x < minY)
                        {
                            dy += myUtils.randFloat(rand, 0.1f) * 0.1f;
                            xOld = x;
                            yOld = y;
                        }

                        if (y > maxY)
                        {
                            dy -= myUtils.randFloat(rand, 0.1f) * 0.1f;
                            xOld = x;
                            yOld = y;
                        }
                    }
                    break;
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Show()
        {
            switch (shapeType)
            {
                case 0:
                    var rectInst = inst as myRectangleInst;

                    rectInst.setInstanceCoords(x - Size, y - Size, 2 * Size, 2 * Size);
                    rectInst.setInstanceColor(R, G, B, A);
                    rectInst.setInstanceAngle(angle);
                    break;

                case 1:
                    var triangleInst = inst as myTriangleInst;

                    triangleInst.setInstanceCoords(x, y, Size, angle);
                    triangleInst.setInstanceColor(R, G, B, A);
                    break;

                case 2:
                    var ellipseInst = inst as myEllipseInst;

                    ellipseInst.setInstanceCoords(x, y, 2 * Size, angle);
                    ellipseInst.setInstanceColor(R, G, B, A);
                    break;

                case 3:
                    var pentagonInst = inst as myPentagonInst;

                    pentagonInst.setInstanceCoords(x, y, 2 * Size, angle);
                    pentagonInst.setInstanceColor(R, G, B, A);
                    break;

                case 4:
                    var hexagonInst = inst as myHexagonInst;

                    hexagonInst.setInstanceCoords(x, y, 2 * Size, angle);
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

            myPrimitive.init_Line();

            if (doClearBuffer == false)
            {
                glDrawBuffer(GL_FRONT_AND_BACK);
            }


            while (!Glfw.WindowShouldClose(window))
            {
                processInput(window);

                // Swap fore/back framebuffers, and poll for operating system events.
                Glfw.SwapBuffers(window);
                Glfw.PollEvents();

                // Dim the screen constantly
                if (doClearBuffer == false)
                {
                    myPrimitive._Rectangle.SetAngle(0);
                    // Shift background color just a bit, to hide long lasting traces of shapes
                    myPrimitive._Rectangle.SetColor(rand.Next(5) * 0.01f, rand.Next(5) * 0.01f, rand.Next(5) * 0.01f, dimAlpha);
                    myPrimitive._Rectangle.Draw(0, 0, gl_Width, gl_Height, true);
                }
                else
                {
                    glClearColor(0, 0, 0, 1);
                    glClear(GL_COLOR_BUFFER_BIT);
                }

                inst.ResetBuffer();

                if (doConnect)
                {
                    myPrimitive._LineInst.ResetBuffer();
                }

                for (int i = 0; i < list.Count; i++)
                {
                    var obj = list[i] as myObj_010;
                    obj.Show();
                    obj.Move();

                    if (doConnect)
                    {
                        float r = 1;
                        float g = 1;
                        float b = 1;

                        switch (connectColorType)
                        {
                            case 0:
                                r = obj.R;
                                g = obj.G;
                                b = obj.B;
                                break;
                        }

                        switch (connectType)
                        {
                            // Connect every particle to every other particle out there
                            case 0:
                                for (int j = i + 1; j < list.Count; j++)
                                {
                                    var obj2 = list[j] as myObj_010;
                                    myPrimitive._LineInst.setInstanceCoords(obj.x, obj.y, obj2.x, obj2.y);
                                    myPrimitive._LineInst.setInstanceColor(r, g, b, 0.023f);
                                    //myPrimitive._LineInst.setInstanceColor(1, 1, 1, 0.023f);
                                }
                                break;

                            // Connect every particle to center (which sometimes is randomized)
                            case 1:
                            case 2:
                                myPrimitive._LineInst.setInstanceCoords(obj.x, obj.y, x0, y0);
                                myPrimitive._LineInst.setInstanceColor(r, g, b, 0.021f);
                                break;

                            // Connect every particle to random point each frame
                            case 3:
                            case 4:
                                myPrimitive._LineInst.setInstanceCoords(obj.x, obj.y, rand.Next(gl_Width), rand.Next(gl_Height));
                                myPrimitive._LineInst.setInstanceColor(r, g, b, 0.066f);
                                break;

                            // Connect each particle to its originating point
                            case 5:
                            case 6:
                                myPrimitive._LineInst.setInstanceCoords(obj.x, obj.y, obj.xOrig, obj.yOrig);
                                myPrimitive._LineInst.setInstanceColor(r, g, b, 0.066f);
                                break;

                            // Connect particle to its last reflection point - 1
                            case 7:
                                myPrimitive._LineInst.setInstanceCoords(obj.x, obj.y, obj.xOld, obj.yOld);
                                myPrimitive._LineInst.setInstanceColor(r, g, b, 0.066f);
                                break;

                            // Connect particle to its last reflection point - 2
                            case 8:
                                myPrimitive._LineInst.setInstanceCoords(obj.x, obj.y, obj.xOld, obj.yOld);
                                myPrimitive._LineInst.setInstanceColor(r, g, b, 0.1f);
                                break;

                            // Connect particle to its last reflection point - 3
                            case 9:
                                myPrimitive._LineInst.setInstanceCoords(obj.x, obj.y, obj.xOld, obj.yOld);
                                myPrimitive._LineInst.setInstanceColor(r, g, b, obj.A * 0.1f);
                                break;
                        }
                    }
                }

                if (doConnect)
                {
                    myPrimitive._LineInst.Draw();
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

                if (list.Count < N)
                {
                    list.Add(new myObj_010());
                }

                radius += (float)Math.Sin(cnt * 0.025f) * 1;
                cnt++;

                System.Threading.Thread.Sleep(renderDelay);
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void initShapes()
        {
            // Find out how many lines do we need to connect each particle with the rest of them
            int totalLines = N;
            if (connectType == 0)
            {
                totalLines = 0;
                for (int i = 0; i < N; i++)
                    totalLines += i;
            }

            myPrimitive.init_Rectangle();

            if (doConnect)
            {
                myPrimitive.init_LineInst(totalLines);
            }

            base.initShapes(shapeType, N, rotationSubMode);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------
    };
};
