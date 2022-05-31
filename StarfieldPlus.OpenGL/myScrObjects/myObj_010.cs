﻿using GLFW;
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
        private float x, y, dx, dy, Size, angle, dAngle;
        private float A = 0, R = 0, G = 0, B = 0;

        private static bool doClearBuffer = false, doFillShapes = false;
        private static int minX = 0, minY = 0, maxX = 0, maxY = 0, maxSize = 0, N = 1;
        private static int shapeType = 0, rotationMode = 0, rotationSubMode = 0;
        private static float dimAlpha = 0.25f;

        private static myInstancedPrimitive inst = null;

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
                doClearBuffer = myUtils.randomBool(rand);
                doFillShapes  = myUtils.randomBool(rand);

                // rotationMode: 0, 1 = rotation; 2 = no rotation, angle is 0; 3 = no rotation, angle is not 0
                rotationMode = rand.Next(4);
                shapeType = rand.Next(5);

                // In case the rotation is enabled, we also may enable additional rotation option:
                if (rotationMode < 2)
                {
                    rotationSubMode = rand.Next(7);
                    rotationSubMode = rotationSubMode > 2 ? 0 : rotationSubMode + 1;     // [0, 1, 2] --> [1, 2, 3]; otherwise set to '0';
                }

                // In case the border is wider than the screen's bounds, the movement looks a bit different (no bouncing)
                int offset = rand.Next(2) == 0 ? 0 : 100 + rand.Next(500);

                minX = 0 - offset;
                minY = 0 - offset;
                maxX = gl_Width  + offset;
                maxY = gl_Height + offset;

                maxSize = myUtils.randomChance(rand, 0, 10) ? 33 : 11;
            }

            generateNew();
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo()
        {
/*
            string str = $"Obj = myObj_010\n\n" +
                            $"N = {N}\n" +
                            $"renderDelay = {renderDelay}\n" +
                            $"moveType = {moveType}\n" +
                            $"shapeType = {shapeType}\n" +
                            $"rotationMode = {rotationMode}\n" +
                            $"rotationSubMode = {rotationSubMode}\n" +
                            $"colorMode = {colorMode}";
*/

            string str = $"Obj = myObj_010\n\n" +
                            $"N = {N}\n" +
                            $"renderDelay = {renderDelay}" +
                            $"shapeType = {shapeType}\n" + 
                            $"rotationMode = {rotationMode}" +
                            $"rotationSubMode = {rotationSubMode}";
            return str;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void generateNew()
        {
            x = rand.Next(gl_Width);
            y = rand.Next(gl_Height);

            int maxSpeed = 2000;

            dx = 0.01f * (rand.Next(maxSpeed) + 1) * myUtils.randomSign(rand);
            dy = 0.01f * (rand.Next(maxSpeed) + 1) * myUtils.randomSign(rand);

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

            if (x < minX || x > maxX)
            {
                dx *= -1;
            }

            if (y < minY || y > maxY)
            {
                dy *= -1;
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

            N = 500;
            renderDelay = 10;


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
                myPrimitive._LineInst.ResetBuffer();

                for (int i = 0; i < list.Count; i++)
                {
                    var obj = list[i] as myObj_010;
                    obj.Show();
                    obj.Move();

                    for (int j = i+1; j < list.Count; j++)
                    {
                        var obj2 = list[j] as myObj_010;
                        myPrimitive._LineInst.setInstanceCoords(obj.x, obj.y, obj2.x, obj2.y);
                        myPrimitive._LineInst.setInstanceColor(1, 1, 1, 0.05f);
                    }
                }

                myPrimitive._LineInst.Draw();

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

                System.Threading.Thread.Sleep(renderDelay);

                cnt++;
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void initShapes()
        {
            myPrimitive.init_Rectangle();
            myPrimitive.init_LineInst(N * N);

            switch (shapeType)
            {
                case 0:
                    myPrimitive.init_RectangleInst(N);
                    myPrimitive._RectangleInst.setRotationMode(rotationSubMode);
                    inst = myPrimitive._RectangleInst;
                    break;

                case 1:
                    myPrimitive.init_TriangleInst(N);
                    myPrimitive._TriangleInst.setRotationMode(rotationSubMode);
                    inst = myPrimitive._TriangleInst;
                    break;

                case 2:
                    myPrimitive.init_EllipseInst(N);
                    myPrimitive._EllipseInst.setRotationMode(rotationSubMode);
                    inst = myPrimitive._EllipseInst;
                    break;

                case 3:
                    myPrimitive.init_PentagonInst(N);
                    myPrimitive._PentagonInst.setRotationMode(rotationSubMode);
                    inst = myPrimitive._PentagonInst;
                    break;

                case 4:
                    myPrimitive.init_HexagonInst(N);
                    myPrimitive._HexagonInst.setRotationMode(rotationSubMode);
                    inst = myPrimitive._HexagonInst;
                    break;
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------
    };
};
