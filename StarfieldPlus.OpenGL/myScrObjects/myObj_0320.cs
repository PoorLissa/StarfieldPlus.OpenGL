using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;


/*
    - Spiralling doodles made of squares
*/


namespace my
{
    public class myObj_0320 : myObject
    {
        // Priority
        public static int Priority => 10;
		public static System.Type Type => typeof(myObj_0320);

        private int cnt, steps, Steps;
        private float x, y, dx, dy, sizex, sizey, R, G, B, A, stepx, stepy;
        private float x1, y1, x2, y2, x3, y3, x4, y4, x4old, y4old;

        private static int N = 1000, objN = 0;
        private static int drawMode = 0, maxSteps = 0, gridMode = 0, maxSize = 0, gridSpacing = 0;
        private static float dimAlpha = 0.05f;
        private static bool doRecalcStep = true;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_0320()
        {
            generateNew();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time global initialization
        protected override void initGlobal()
        {
            colorPicker = new myColorPicker(gl_Width, gl_Height);
            list = new List<myObject>();

            {
                doClearBuffer = false;
            }

            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time initialization
        private void initLocal()
        {
            doRecalcStep = myUtils.randomBool(rand);            // Only used in drawMode 2

            maxSteps = rand.Next(51) + 5;
            gridMode = rand.Next(5);
            drawMode = rand.Next(3);

            maxSize = rand.Next(100) + 50;
            gridSpacing = rand.Next(20);

            objN = rand.Next(13) + 1;
            dimAlpha = 0.01f;
            renderDelay = rand.Next(25) + 1;

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            string fStr(float f) { return f.ToString("0.000"); }

            string str = $"Obj = {Type}\n\n"                 	+
                            $"objN = {objN}\n"                  +
                            $"drawMode = {drawMode}\n"          +
                            $"gridMode = {gridMode}\n"          +
                            $"maxSteps = {maxSteps}\n"          +
                            $"doRecalcStep = {doRecalcStep}\n"  +
                            $"dimAlpha = {fStr(dimAlpha)}\n"    +
                            $"renderDelay = {renderDelay}\n"    +
                            $"file: {colorPicker.GetFileName()}"
            ;
            return str;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void generateNew()
        {
            cnt = rand.Next(111) + 11;

            x = rand.Next(gl_Width + 123);
            y = rand.Next(gl_Height + 123);

            switch (gridMode)
            {
                // No grid, random size
                case 0:
                    sizex = sizey = rand.Next(500) + 50;
                    dx = dy = rand.Next(20) + 3;
                    break;

                // No grid, const size
                case 1:
                    sizex = sizey = maxSize;
                    dx = dy = rand.Next(13) + 3;
                    break;

                // Grid, intersected by half size
                case 2:
                    x += - x % maxSize;
                    y += - y % maxSize;

                    sizex = sizey = maxSize;
                    dx = dy = rand.Next(11) + 5;
                    break;

                // Grid, no intersections
                case 3:
                    x -= x % (maxSize * 2 + gridSpacing);
                    y -= y % (maxSize * 2 + gridSpacing);

                    sizex = sizey = maxSize;
                    dx = dy = rand.Next(11) + 5;
                    break;

                // Grid, no intersections, chance to generate larger cell
                case 4:
                    if (myUtils.randomChance(rand, 1, 33))
                    {
                        int Size = rand.Next(250) + maxSize;

                        x -= x % (Size * 2 + gridSpacing);
                        y -= y % (Size * 2 + gridSpacing);

                        sizex = sizey = Size;
                        dx = dy = rand.Next(11) + 5;
                    }
                    else
                    {
                        x -= x % (maxSize * 2 + gridSpacing);
                        y -= y % (maxSize * 2 + gridSpacing);

                        sizex = sizey = maxSize;
                        dx = dy = rand.Next(11) + 5;
                    }
                    break;
            }

            colorPicker.getColor(x, y, ref R, ref G, ref B);
            A = 0.05f;

            // Additional setup for drawMode == 1
            if (drawMode == 1)
            {
                steps = Steps = rand.Next(maxSteps) + 2;
                stepx = sizex * 2 / Steps;
                stepy = sizey * 2 / Steps;
            }

            // Additional setup for drawMode == 2
            if (drawMode == 2)
            {
                steps = Steps = rand.Next(maxSteps) + 10;
                stepx = sizex * 2 / Steps;
                stepy = sizey * 2 / Steps;

                // Draw the first square with high opacity
                A = 0.85f;
            }

            // Precalc the 4 points
            {
                x1 = x - sizex;
                y1 = y - sizey;

                x2 = x + sizex;
                y2 = y - sizey;

                x3 = x + sizex;
                y3 = y + sizey;

                x4 = x - sizex;
                y4 = y + sizey;

                x4old = x4;
                y4old = y4;
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void setNextMode()
        {
            list.Clear();
            dimScreen(0.25f);
            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            switch (drawMode)
            {
                // Draw 4 lines each step, moving end points along 1 side of the square
                case 0:
                    {
                        if (x2 - x1 > dx)
                        {
                            x1 += dx;
                            x3 -= dx;
                            y2 += dy;
                            y4 -= dy;
                        }
                        else
                        {
                            x1 = x2;
                            x3 = x4;
                            y2 = y3;
                            y4 = y1;

                            A -= 0.02f;

                            if (--cnt == 0)
                                generateNew();
                        }
                    }
                    break;

                // Draws square inside square inside square, each one is rotated a bit
                case 1:
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

                        stepx = (float)(dist / Steps);
                        stepy = (float)(dist / Steps);

                        if (stepx < 5 || dist < 100)
                        {
                            A -= 0.011f;
                        }

                        if (stepx < 2)
                        {
                            stepx = 1;
                            stepy = 1;

                            if (--cnt == 0)
                                generateNew();
                        }
                    }
                    break;

                // Might look like a [case 1], but starting with the second iteration, it is actually a spiral, not a square-inside-square
                case 2:
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

                        A = 0.5f;

                        if (doRecalcStep)
                        {
                            stepx = (float)(dist / Steps);
                            stepy = (float)(dist / Steps);
                        }

                        if (steps > 0)
                        {
                            steps--;
                        }
                        else
                        {
                            if (--cnt == 0)
                                generateNew();
                        }
                        return;
                    }
                    break;
            }

            A += 0.01f;
        }

        // ---------------------------------------------------------------------------------------------------------------

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
            switch (drawMode)
            {
                case 0:
                case 1:
                    myPrimitive._LineInst.setInstanceCoords(x1, y1, x2, y2);
                    myPrimitive._LineInst.setInstanceColor(R, G, B, A);

                    myPrimitive._LineInst.setInstanceCoords(x2, y2, x3, y3);
                    myPrimitive._LineInst.setInstanceColor(R, G, B, A);

                    myPrimitive._LineInst.setInstanceCoords(x3, y3, x4, y4);
                    myPrimitive._LineInst.setInstanceColor(R, G, B, A);

                    myPrimitive._LineInst.setInstanceCoords(x4, y4, x1, y1);
                    myPrimitive._LineInst.setInstanceColor(R, G, B, A);
                    break;

                // The last line is supposed to be drawn to the point where it intersects with the 1st one (spiral-like structure)
                case 2:
                    myPrimitive._LineInst.setInstanceCoords(x1, y1, x2, y2);
                    myPrimitive._LineInst.setInstanceColor(R, G, B, A);

                    myPrimitive._LineInst.setInstanceCoords(x2, y2, x3, y3);
                    myPrimitive._LineInst.setInstanceColor(R, G, B, A);

                    myPrimitive._LineInst.setInstanceCoords(x3, y3, x4, y4);
                    myPrimitive._LineInst.setInstanceColor(R, G, B, A);


                    myPrimitive._LineInst.setInstanceCoords(x4old, y4old, x1, y1);
                    myPrimitive._LineInst.setInstanceColor(R, G, B, A);
                    break;
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Process(Window window)
        {
            uint cnt = 0;

            initShapes();


            clearScreenSetup(doClearBuffer, 0.1f);
            glDrawBuffer(GL_FRONT_AND_BACK);


            while (!Glfw.WindowShouldClose(window))
            {
                cnt++;

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

                        if (myUtils.randomChance(rand, 1, 1234))
                        {
                            dimScreen(0.05f);
                        }
                    }
                }

                // Render Frame
                {
                    myPrimitive._LineInst.ResetBuffer();

                    for (int i = 0; i < list.Count; i++)
                    {
                        var obj = list[i] as myObj_0320;
                        obj.Show();
                        obj.Move();
                    }

                    myPrimitive._LineInst.Draw();
                }

                if (list.Count < objN)
                {
                    list.Add(new myObj_0320());
                }

                System.Threading.Thread.Sleep(renderDelay);
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void initShapes()
        {
            myPrimitive.init_ScrDimmer();
            myPrimitive.init_LineInst(N);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------
    }
};
