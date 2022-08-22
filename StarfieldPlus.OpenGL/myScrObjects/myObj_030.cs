using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;


/*
    - Rain Drops (Vertical, Top-Down)
*/


namespace my
{
    public class myObj_030 : myObject
    {
        private float x, y, dx, dy, Size, angle, dAngle, xOld, yOld, dA;
        private float A = 0, R = 0, G = 0, B = 0;
        private int lifeCounter = -1;
        private bool isSlow = false, isFalling = true;

        private static float dimAlpha = 0;
        private static bool doClearBuffer = true, doFillShapes = false;
        private static int N = 1, shapeType = 0, rotationMode = 0, rotationSubMode = 0, connectionMode = 0, moveMode = 0;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_030()
        {
            if (colorPicker == null)
            {
                colorPicker = new myColorPicker(gl_Width, gl_Height);
                list = new List<myObject>();

                init();
            }

            generateNew();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time initialization
        private void init()
        {
            N = 500;
            dimAlpha = 0.1f;
            renderDelay = 10;

            doFillShapes  = myUtils.randomBool(rand);
            doClearBuffer = myUtils.randomBool(rand);

            moveMode = rand.Next(7);
            shapeType = rand.Next(5);
            connectionMode = rand.Next(2);

            // rotationMode: 0, 1 = rotation; 2 = no rotation, angle is 0; 3 = no rotation, angle is not 0
            rotationMode = rand.Next(4);

            // In case the rotation is enabled, we also may enable additional rotation option:
            if (rotationMode < 2)
            {
                rotationSubMode = rand.Next(7);
                rotationSubMode = rotationSubMode > 2 ? 0 : rotationSubMode + 1;     // [0, 1, 2] --> [1, 2, 3]; otherwise set to '0';
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            return $"Obj = myObj_030\n\n" +
                            $"N = {N}\n" +
                            $"moveMode = {moveMode}\n"
                            ;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void generateNew()
        {
            if (moveMode == 0)
            {
                isSlow = true;

                Size = rand.Next(7) + 1;
                angle = 0;
                dAngle = rotationMode < 2 ? 0.001f * rand.Next(111) * myUtils.randomSign(rand) : 0;

                xOld = x = rand.Next(gl_Width);
                yOld = y = rand.Next(gl_Height);

                lifeCounter = 0;
                dx = 0;
                dy = 0;
                A = 0;
                dA = 0.001f * (rand.Next(11) + 1);
                colorPicker.getColorRand(ref R, ref G, ref B);
            }
            else
            {
                int rnd = rand.Next(1000);
                int maxSize = 13;

                isSlow = false;
                isFalling = true;

                if (rnd < 100)
                    maxSize = 27;

                if (rnd > 750)
                {
                    isSlow = true;
                    maxSize = 2;
                    lifeCounter = rand.Next(5) + 1;
                }

                Size = rand.Next(maxSize) + 1;
                angle = 0;
                dAngle = rotationMode < 2 ? 0.001f * rand.Next(111) * myUtils.randomSign(rand) : 0;

                lifeCounter = (lifeCounter == -1) ? rand.Next(100) : rand.Next(100) + 100;

                xOld = x = rand.Next(gl_Width);
                yOld = y = isSlow ? rand.Next(gl_Height) : -rand.Next((int)Size);

                dx = 0;
                dy = 0;

                A = (float)rand.NextDouble();
                colorPicker.getColorRand(ref R, ref G, ref B);
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            if (moveMode == 0)
            {
                A += dA;
                if (A < 0.85f)
                    return;
            }

            if (isSlow)
            {
                if (y % 5 == 0)
                {
                    x += rand.Next(3) - 1;
                }

                y += dy;
                dy += (0.01f + Size / 20.0f);

                if (y > gl_Height)
                {
                    generateNew();
                }
            }
            else
            {
                if (lifeCounter == 0)
                {
                    angle += dAngle;

                    xOld = x;
                    yOld = y;

                    x += dx;
                    y += dy;

                    dy += (0.5f + Size / 33.0f);

                    if (y >= gl_Height)
                    {
                        isFalling = false;

                        dy *= -0.025f * (rand.Next(11) + 1);
                        dx += 0.5f * (rand.Next(10)) * (rand.Next(3) - 1);

                        if (dy > -1)
                        {
                            generateNew();
                        }
                    }
                }
                else
                {
                    if (--lifeCounter == 0)
                    {
                        dy = -0.1f * (rand.Next(20) + 10);
                    }
                }
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
                    rectInst.setInstanceColor(R, G, B, A/2);
                    rectInst.setInstanceAngle(angle);

                    rectInst.setInstanceCoords(x - Size/2, y - Size/2, Size, Size);
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
            uint cnt = 0, backRainCnt = 0;
            initShapes();

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

                // Render background rain
                {
                    myPrimitive._LineInst.ResetBuffer();

                    if (backRainCnt < 300)
                    {
                        backRainCnt++;
                    }

                    for (int i = 0; i < backRainCnt/3; i++)
                    {
                        int x1 = rand.Next(gl_Width);
                        int x2 = x1 + rand.Next(201) - 100;

                        myPrimitive._LineInst.setInstanceCoords(x1, 0, x2, gl_Height);
                        myPrimitive._LineInst.setInstanceColor(1, 1, 1, doClearBuffer ? 0.085f : 0.033f);
                    }

                    myPrimitive._LineInst.Draw();
                }

                inst.ResetBuffer();

                if (connectionMode > 0)
                    myPrimitive._LineInst.ResetBuffer();

                for (int i = 0; i < list.Count; i++)
                {
                    var obj = list[i] as myObj_030;

                    if (connectionMode > 0 && !obj.isSlow && obj.isFalling)
                    {
                        if (doClearBuffer == false)
                        {
                            myPrimitive._LineInst.setInstanceCoords(obj.x, obj.y, obj.xOld, obj.yOld);
                            myPrimitive._LineInst.setInstanceColor(obj.R, obj.G, obj.B, 0.25f);
                        }
                        else
                        {
                            if (obj.dy > 0)
                            {
                                myPrimitive._LineInst.setInstanceCoords(obj.x, obj.y, obj.x, 0);
                                myPrimitive._LineInst.setInstanceColor(1, 1, 1, 0.05f);

                                myPrimitive._LineInst.setInstanceCoords(obj.x, obj.y, obj.x, obj.y / 2);
                                myPrimitive._LineInst.setInstanceColor(1, 1, 1, 0.05f);

                                myPrimitive._LineInst.setInstanceCoords(obj.x, obj.y, obj.x, obj.yOld);
                                myPrimitive._LineInst.setInstanceColor(1, 1, 1, 0.05f);
                            }
                        }
                    }

                    obj.Show();
                    obj.Move();
                }

                if (connectionMode > 0)
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
                    list.Add(new myObj_030());
                }

                cnt++;

                System.Threading.Thread.Sleep(renderDelay);
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void initShapes()
        {
            int lineN = N * 3, shapeN = N * 2;

            myPrimitive.init_Rectangle();

            myPrimitive.init_LineInst(lineN);

            base.initShapes(shapeType, shapeN, rotationSubMode);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------
    };
};
