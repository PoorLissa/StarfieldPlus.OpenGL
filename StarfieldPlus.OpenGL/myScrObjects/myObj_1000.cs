﻿using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;


/*
    - ...
*/


namespace my
{
    public class myObj_1000 : myObject
    {
        // Priority
        public static int Priority => 9999910;
		public static System.Type Type => typeof(myObj_1000);

        private float x, y, x1, y1, x2, y2, dx, dy;
        private float size, A, R, G, B, angle = 0, dAngle;

        private List<myObj_1000> _children = null;

        private static int N = 0, n = 0, NN = 0, shape = 0, cellSize = 100;
        private static bool doFillShapes = false;

        private static myScreenGradient grad = null;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_1000()
        {
            if (id != uint.MaxValue)
                generateNew();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time global initialization
        protected override void initGlobal()
        {
            colorPicker = new myColorPicker(gl_Width, gl_Height, mode: myColorPicker.colorMode.SNAPSHOT_OR_IMAGE);
            list = new List<myObject>();

            // Global unmutable constants
            {
                cellSize = 50;

                NN = 10000;

                shape = rand.Next(5);
            }

            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time local initialization
        private void initLocal()
        {
            doClearBuffer = myUtils.randomBool(rand);
            doClearBuffer = true;

            renderDelay = rand.Next(3) + 1;

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            height = 600;

            string nStr(int   n) { return n.ToString("N0");    }
            //string fStr(float f) { return f.ToString("0.000"); }

            string str = $"Obj = {Type}\n\n"                         +
                            $"N = {nStr(list.Count)} of {nStr(N)}\n" +
                            $"n = {nStr(n)}\n"                       +
                            $"cellSize = {cellSize}\n"               +
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
            if (_children == null)
            {
                _children = new List<myObj_1000>();
            }
            else
            {
                var obj = new myObj_1000();

                obj.x = x1 + rand.Next((int)size);
                obj.y = y1 + rand.Next((int)size);

                obj.size = rand.Next(3) + 1;
                obj.dAngle = myUtils.randFloatSigned(rand) * 0.01f;

                obj.dx = myUtils.randFloatSigned(rand) * 0.95f;
                obj.dy = myUtils.randFloatSigned(rand) * 0.95f;

                obj.A = (float)rand.NextDouble();
                obj.R = (float)rand.NextDouble();
                obj.G = (float)rand.NextDouble();
                obj.B = (float)rand.NextDouble();

                colorPicker.getColor(obj.x, obj.y, ref obj.R, ref obj.G, ref obj.B);

                _children.Add(obj);
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            for (int i = 0; i < _children.Count; i++)
            {
                var obj = _children[i];

                obj.x += obj.dx;
                obj.y += obj.dy;
                obj.angle += obj.dAngle;

                if (obj.x < x1 && obj.dx < 0)
                    obj.dx *= -1;
                else if (obj.x > x2 && obj.dx > 0)
                    obj.dx *= -1;

                if (obj.y < y1 && obj.dy < 0)
                    obj.dy *= -1;
                else if (obj.y > y2 && obj.dy > 0)
                    obj.dy *= -1;
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Show()
        {
            if (id < N)
            {
                //myPrimitive._Rectangle.SetColor(1, 0, 0, 0.5f);
                //myPrimitive._Rectangle.Draw(x1, y1, size, size, false);

                for (int i = 0; i < _children.Count; i++)
                {
                    _children[i].Show();
                }
            }
            else
            {
                float size2x = size * 2;

                switch (shape)
                {
                    // Instanced squares
                    case 0:
                        myPrimitive._RectangleInst.setInstanceCoords(x - size, y - size, size2x, size2x);
                        myPrimitive._RectangleInst.setInstanceColor(R, G, B, A);
                        myPrimitive._RectangleInst.setInstanceAngle(angle);
                        break;

                    // Instanced triangles
                    case 1:
                        myPrimitive._TriangleInst.setInstanceCoords(x, y, size2x, angle);
                        myPrimitive._TriangleInst.setInstanceColor(R, G, B, A);
                        break;

                    // Instanced circles
                    case 2:
                        myPrimitive._EllipseInst.setInstanceCoords(x, y, size2x, angle);
                        myPrimitive._EllipseInst.setInstanceColor(R, G, B, A);
                        break;

                    // Instanced pentagons
                    case 3:
                        myPrimitive._PentagonInst.setInstanceCoords(x, y, size2x, angle);
                        myPrimitive._PentagonInst.setInstanceColor(R, G, B, A);
                        break;

                    // Instanced hexagons
                    case 4:
                        myPrimitive._HexagonInst.setInstanceCoords(x, y, size2x, angle);
                        myPrimitive._HexagonInst.setInstanceColor(R, G, B, A);
                        break;
                }
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Process(Window window)
        {
            uint cnt = 0;
            initAll();
            initShapes();

            clearScreenSetup(doClearBuffer, 0.1f);

            while (!Glfw.WindowShouldClose(window))
            {
                int Count = list.Count;

                processInput(window);

                // Swap fore/back framebuffers, and poll for operating system events.
                Glfw.SwapBuffers(window);
                Glfw.PollEvents();

                // Dim screen
                {
                    if (doClearBuffer)
                        glClear(GL_COLOR_BUFFER_BIT);

                    grad.Draw();
                }

                // Render Frame
                {
                    inst.ResetBuffer();

                    for (int i = 0; i < Count; i++)
                    {
                        var obj = list[i] as myObj_1000;

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

                    showGrid();
                }

                if (n < NN)
                {
                    int i = rand.Next(N);

                    (list[i] as myObj_1000).generateNew();
                    n++;
                }

                cnt++;
                System.Threading.Thread.Sleep(renderDelay);
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void initShapes()
        {
            base.initShapes(shape, NN, 0);

            int nLineInst = gl_Width / cellSize + gl_Height / cellSize + 1;

            myPrimitive.init_LineInst(nLineInst);

            grad = new myScreenGradient();
            grad.SetRandomColors(rand, 0.2f);

            myPrimitive.init_Rectangle();

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void showGrid()
        {
            myPrimitive._LineInst.ResetBuffer();

            float gridA = 0.5f;
            float factor = 0.99f;

            float a = gridA;
            float r = 0.25f;
            float g = 0.25f;
            float b = 0.25f;

            for (float i = gl_x0 - cellSize / 2; i > 0; i -= cellSize)
            {
                myPrimitive._LineInst.setInstanceCoords(i, 0, i, gl_Height);
                myPrimitive._LineInst.setInstanceColor(r, g, b, a);
                a *= factor;
            }

            a = gridA;

            for (float i = gl_x0 + cellSize / 2; i < gl_Width; i += cellSize)
            {
                myPrimitive._LineInst.setInstanceCoords(i, 0, i, gl_Height);
                myPrimitive._LineInst.setInstanceColor(r, g, b, a);
                a *= factor;
            }

            a = gridA;

            for (float i = gl_y0 - cellSize / 2; i > 0; i -= cellSize)
            {
                myPrimitive._LineInst.setInstanceCoords(0, i, gl_Width, i);
                myPrimitive._LineInst.setInstanceColor(r, g, b, a);
                a *= factor;
            }

            a = gridA;

            for (float i = gl_y0 + cellSize / 2; i < gl_Height; i += cellSize)
            {
                myPrimitive._LineInst.setInstanceCoords(0, i, gl_Width, i);
                myPrimitive._LineInst.setInstanceColor(r, g, b, a);
                a *= factor;
            }

            myPrimitive._LineInst.Draw();
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void initAll()
        {
            float I = gl_x0 - cellSize / 2;
            float J = gl_y0 - cellSize / 2;

            for (; I > 0; I -= cellSize) ;
            for (; J > 0; J -= cellSize) ;

            int margin = 10;

            for (float i = I; i < gl_Width; i += cellSize)
            {
                for (float j = J; j < gl_Height; j += cellSize)
                {
                    var obj = new myObj_1000();

                    obj.x1 = i + margin;
                    obj.y1 = j + margin;
                    obj.size = cellSize - margin * 2;
                    obj.x2 = obj.x1 + obj.size;
                    obj.y2 = obj.y1 + obj.size;

                    list.Add(obj);
                }
            }

            n = 0;
            N = list.Count;

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------
    }
};