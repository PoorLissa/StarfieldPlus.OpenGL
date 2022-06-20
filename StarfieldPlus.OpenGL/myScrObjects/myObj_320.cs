using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;


/*

*/


namespace my
{
    public class myObj_320 : myObject
    {
        // ---------------------------------------------------------------------------------------------------------------

        private static int x0, y0, N = 1000, objN = 0;
        private static int shapeType = 0, drawMode = 0, maxSteps = 0, gridMode = 0;

        private static myInstancedPrimitive inst = null;

        private float x, y, dx, dy, sizex, sizey, R, G, B, A;
        private int cnt = 0, stepx = 0, stepy = 0, steps = 0;

        private float x1, y1, x2, y2, x3, y3, x4, y4, x4old, y4old;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_320()
        {
            if (colorPicker == null)
            {
                colorPicker = new myColorPicker(gl_Width, gl_Height);
                list = new List<myObject>();

                objN = 10;

                init();
            }

            generateNew();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time initialization
        private void init()
        {
            x0 = gl_Width  / 2;
            y0 = gl_Height / 2;

            maxSteps = rand.Next(51) + 5;
            gridMode = rand.Next(3);
            drawMode = rand.Next(3);

drawMode = 1;
gridMode = 0;
objN = 1;

            if (gridMode > 0)
            {
                objN = 100;
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            string str = $"Obj = myObj_320\n\n" +
                         $"gridMode = {gridMode}"
                ;

            return str;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void generateNew()
        {
            cnt = rand.Next(111) + 11;

            x = rand.Next(gl_Width);
            y = rand.Next(gl_Height);

            colorPicker.getColor(x, y, ref R, ref G, ref B);

            //x = x0;
            //y = y0;

            sizex = sizey = rand.Next(2222) + 666;
            steps = rand.Next(maxSteps) + 2;

            switch (gridMode)
            {
                // No grid
                case 0:
                    break;

                // Grid, no large objects
                case 1:
                    x += 75 - x % 205;
                    y += 50 - y % 205;
                    sizex = sizey = 100;
                    break;

                // Grid, large objects are rare
                case 2:
                    x += 75 - x % 205;
                    y += 50 - y % 205;

                    if (myUtils.randomChance(rand, 499, 500))
                        sizex = sizey = 100;
                    break;
            }

            stepx = (int)sizex * 2 / steps;
            stepy = (int)sizey * 2 / steps;

            x1 = x - sizex;
            y1 = y - sizey;

            x2 = x + sizex;
            y2 = y - sizey;

            x3 = x + sizex;
            y3 = y + sizey;

            x4old = x4 = x - sizex;
            y4old = y4 = y + sizey;

            A = gridMode > 0 ? 0 : 0.05f;

            dx = dy = rand.Next(10) + 3;

x1 = 333;
y1 = 333;

x2 = 1000;
y2 = 444;

x3 = 1200;
y3 = 777;

x4old = x4 = 222;
y4old = y4 = 666;

x1 = rand.Next(gl_Width);
y1 = rand.Next(gl_Height);

x2 = rand.Next(gl_Width); ;
y2 = rand.Next(gl_Height);

x3 = rand.Next(gl_Width); ;
y3 = rand.Next(gl_Height);

x4old = x4 = rand.Next(gl_Width); ;
y4old = y4 = rand.Next(gl_Height);

A = 0.5f;

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void setNextMode()
        {
            init();

gridMode = 0;
objN = 1;

            list.Clear();

            var obj = new myObj_320();

            obj.x = x0;
            obj.y = y0;

            obj.sizex = obj.sizey = 500;

            list.Add(obj);
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            if (drawMode == 0)
            {
                if (x1 < x2)
                {
                    x1 += dx;
                    x3 -= dx;
                    y2 += dy;
                    y4 -= dy;
                }
                else
                {
                    if (--cnt == 0)
                        generateNew();
                }
            }

            if (drawMode == 1)
            {
                float X1 = 0, Y1 = 0, X2 = 0, Y2 = 0, X3 = 0, Y3 = 0, X4 = 0, Y4 = 0;

                double dist = recalc(ref X1, ref Y1, x1, y1, x2, y2);
                recalc(ref X2, ref Y2, x2, y2, x3, y3);
                recalc(ref X3, ref Y3, x3, y3, x4, y4);
                recalc(ref X4, ref Y4, x4, y4, x1, y1);

                x1 += X1;
                y1 += Y1;
                x2 += X2;
                y2 += Y2;
                x3 += X3;
                y3 += Y3;
                x4 += X4;
                y4 += Y4;

                stepx = (int)dist / steps;
                stepy = (int)dist / steps;

                if (stepx < 5 || dist < 100)
                {
                    A -= 0.011f;
                }

                if (stepx < 2)
                {
                    stepx = 1;
                    stepy = 1;
/*
                    if (--cnt == 0)
                        generateNew();
*/
                }
            }

            if (drawMode == 2)
            {
                float X1 = 0, Y1 = 0, X2 = 0, Y2 = 0, X3 = 0, Y3 = 0, X4 = 0, Y4 = 0;

                double dist = recalc(ref X1, ref Y1, x1, y1, x2, y2);
                recalc(ref X2, ref Y2, x2, y2, x3, y3);
                recalc(ref X3, ref Y3, x3, y3, x4, y4);

                x1 += X1;
                y1 += Y1;

                recalc(ref X4, ref Y4, x4, y4, x1, y1);

                x4old = x4;
                y4old = y4;

                x2 += X2;
                y2 += Y2;
                x3 += X3;
                y3 += Y3;
                x4 += X4;
                y4 += Y4;

                A = 0.85f;
                return;
            }

            A += 0.01f;
        }

        double recalc(ref float x, ref float y, float x1, float y1, float x2, float y2)
        {
            double dist = Math.Sqrt((x2-x1)*(x2-x1) + (y2-y1)*(y2-y1));

            x = (float)((x2 - x1) * stepx / dist);
            y = (float)((y2 - y1) * stepy / dist);

            return dist;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Show()
        {
            myPrimitive._LineInst.setInstanceCoords(x1, y1, x2, y2);
            myPrimitive._LineInst.setInstanceColor(R, G, B, A);

            myPrimitive._LineInst.setInstanceCoords(x2, y2, x3, y3);
            myPrimitive._LineInst.setInstanceColor(R, G, B, A);

            myPrimitive._LineInst.setInstanceCoords(x3, y3, x4, y4);
            myPrimitive._LineInst.setInstanceColor(R, G, B, A);

            if (drawMode < 2)
                myPrimitive._LineInst.setInstanceCoords(x4, y4, x1, y1);

            if (drawMode == 2)
                myPrimitive._LineInst.setInstanceCoords(x4old, y4old, x1, y1);

            myPrimitive._LineInst.setInstanceColor(R, G, B, A);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Process(Window window)
        {
            uint cnt = 0;

            initShapes();

            while (list.Count < objN)
            {
                list.Add(new myObj_320());
            }

            glDrawBuffer(GL_FRONT_AND_BACK);
            glClearColor(0, 0, 0, 1);

            while (!Glfw.WindowShouldClose(window))
            {
                cnt++;

                processInput(window);

                // Swap fore/back framebuffers, and poll for operating system events.
                Glfw.SwapBuffers(window);
                Glfw.PollEvents();

                //glClear(GL_COLOR_BUFFER_BIT);

                myPrimitive._Rectangle.SetColor(0, 0, 0, 0.01f);
                myPrimitive._Rectangle.SetAngle(0);
                myPrimitive._Rectangle.Draw(0, 0, gl_Width, gl_Height, true);

                // Render Frame
                {
                    inst.ResetBuffer();
                    myPrimitive._LineInst.ResetBuffer();

                    for (int i = 0; i < list.Count; i++)
                    {
                        var obj = list[i] as myObj_320;
                        obj.Show();
                        obj.Move();
                    }

                    myPrimitive._LineInst.Draw();

                    inst.SetColorA(0);
                    inst.Draw(false);
                }

                System.Threading.Thread.Sleep(renderDelay);
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void initShapes()
        {
            myPrimitive.init_Rectangle();
            myPrimitive.init_LineInst(N);

            switch (shapeType)
            {
                case 0:
                    myPrimitive.init_RectangleInst(N);
                    myPrimitive._RectangleInst.setRotationMode(0);
                    inst = myPrimitive._RectangleInst;
                    break;
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------
    }
};
