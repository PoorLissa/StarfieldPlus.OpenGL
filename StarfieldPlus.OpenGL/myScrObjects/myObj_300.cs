using GLFW;
using static OpenGL.GL;
using System.Collections.Generic;


/*
    - Small Explosions of Particles + Variations
*/


namespace my
{
    internal class myObj_300_Struct
    {
        public float x, y, r, dx, dy, a, time, dt;
    };

    // ===================================================================================================================

    public class myObj_300 : myObject
    {
        private static bool doClearBuffer = false, doUseRotation = false, doFillShapes = false, doUseInstancing = false;
        private static int x0, y0, shapeType = 0, moveType = 0, t = 25, N = 1, gravityRate = 0, maxParticles = 25;
        private static float dimAlpha = 0.1f;

        private float x, y, R, G, B, A, dA, lineTh;

        private int shape = 0, lifeCounter = 0, lifeMax = 0, objN = 0;

        private List<myObj_300_Struct> structsList = null;

        static myRectangleInst rInst = null;

        // -------------------------------------------------------------------------

        public myObj_300()
        {
            if (colorPicker == null)
            {
                x0 = gl_Width  / 2;
                y0 = gl_Height / 2;

                colorPicker = new myColorPicker(gl_Width, gl_Height);
                list = new List<myObject>();

                doClearBuffer = myUtils.randomBool(rand);
                doUseRotation = myUtils.randomBool(rand);
                doFillShapes  = myUtils.randomBool(rand);

                gravityRate = rand.Next(101) + 1;
                shapeType = rand.Next(5);
                moveType = rand.Next(22);

                // Set number of objects N:
                N = rand.Next(666) + 100;

                if (rand.Next(3) == 0)
                {
                    N = rand.Next(11) + 1;
                }

                t = 1;

#if true
                shapeType = 0;
                shapeType = 5;
                //doClearBuffer = true;
                //doClearBuffer = false;
                doUseInstancing = shapeType == 5;
                N = 333;
#endif
            }

            structsList = new List<myObj_300_Struct>();

            for (int i = 0; i < maxParticles; i++)
            {
                structsList.Add(new myObj_300_Struct());
            }

            generateNew();
        }

        // -------------------------------------------------------------------------

        protected override void generateNew()
        {
            x = rand.Next(gl_Width);
            y = rand.Next(gl_Height);

            colorPicker.getColor(x, y, ref R, ref G, ref B);

            A = (float)rand.NextDouble() + 0.1f;

            shape = shapeType;
            lineTh = rand.Next(2) + 1;

            lifeCounter = 0;
            lifeMax = rand.Next(333) + 33;

            objN = rand.Next(maxParticles - 4) + 5;

            int max = rand.Next(5000) + 1000;

            for (int i = 0; i < objN; i++)
            {
                var obj = structsList[i];

                obj.x = x;
                obj.y = y;
                obj.r = 5;

                obj.r = rand.Next(6) + 2;

                obj.dx = 0.001f * (rand.Next(max) - max/2);
                obj.dy = 0.001f * (rand.Next(max) - max/2);
                obj.a = (float)rand.NextDouble() + 0.33f;

                obj.time = 0;
                obj.dt = doUseRotation ? 0.001f * rand.Next(111) : 0;
            }

            return;
        }

        // -------------------------------------------------------------------------

        protected override void Move()
        {
            lifeCounter++;

            if (lifeCounter > lifeMax)
            {
                int liveCnt = 0;

                for (int i = 0; i < objN; i++)
                {
                    var obj = structsList[i];

                    obj.x += obj.dx;
                    obj.y += obj.dy;
                    obj.a -= 0.005f;

                    obj.time += obj.dt;

                    if (obj.y > gl_Height)
                        obj.a = 0;

                    liveCnt += obj.a > 0 ? 1 : 0;

                    switch (moveType)
                    {
                        // Const speed
                        case 0:
                        case 1:
                            break;

                        // Acceleration -- generic
                        case 2:
                        case 3:
                            obj.dx *= 1.005f;
                            obj.dy *= 1.005f;
                            break;

                        // Acceleration -- Radius related (the smaller -- the faster)
                        case 4:
                        case 5:
                            obj.dx *= (1.05f - obj.r / 120) * 0.99f;
                            obj.dy *= (1.05f - obj.r / 120) * 0.99f;
                            break;

                        // Gravity
                        case 6:
                            obj.dy += 0.01f * rand.Next(gravityRate);
                            break;

                        // Deceleration -- generic
                        case 7:
                        case 8:
                            obj.dx *= 0.99f;
                            obj.dy *= 0.99f;
                            break;

                        // Deceleration -- Radius related (the smaller -- the faster)
                        case 9:
                        case 10:
                            obj.dx *= (1.0f - obj.r / 200);
                            obj.dy *= (1.0f - obj.r / 200);
                            break;

                        // Deceleration -- Radius related (the smaller -- the slower)
                        case 11:
                        case 12:
                            obj.dx *= (1.0f - 0.1f / obj.r);
                            obj.dy *= (1.0f - 0.1f / obj.r);
                            break;

                        // Move Sideways -- generic
                        case 13:
                            obj.dy = 0;
                            break;

                        // Move Sideways -- X-acceleration
                        case 14:
                            obj.dy = 0;
                            obj.dx *= 1.005f;
                            break;

                        // Move Sideways -- Radius related (the smaller -- the faster)
                        case 15:
                            obj.dy *= 0.1f;
                            obj.dx *= (1.05f - obj.r / 120);
                            break;

                        // Move Sideways -- Y-strong-deceleration
                        case 16:
                            obj.dy *= 0.95f;
                            break;

                        // Move up or down -- generic
                        case 17:
                            obj.dx = 0;
                            break;

                        // Move up or down -- larger shapes go up, smaller shapes go down
                        case 18:
                            obj.dx = 0;
                            obj.dy += (0.05f - obj.r / 100);
                            break;

                        // Move up or down -- larger shapes go up, smaller shapes go down + some X-axis dispersion
                        case 19:
                        case 20:
                            obj.dx *= 0.75f;
                            obj.dy += (0.05f - obj.r / 100);
                            break;

                        // Move in a cross fashion
                        case 21:
                            if (System.Math.Abs(obj.dx) >= System.Math.Abs(obj.dy))
                                obj.dy = 0;
                            else
                                obj.dx = 0;
                            break;
                    }
                }

                if (liveCnt == 0)
                {
                    generateNew();
                }
            }

            return;
        }

        // -------------------------------------------------------------------------

        protected override void Show()
        {
            if (lifeCounter > lifeMax)
            {
                switch (shape)
                {
                    case 0:
                        glLineWidth(lineTh);
                        myPrimitive._Hexagon.SetColor(R, G, B, 1.0f);

                        for (int i = 0; i < objN; i++)
                        {
                            var obj = structsList[i];

                            if (obj.a > 0)
                            {
                                myPrimitive._Hexagon.SetAngle(obj.time);

                                if (doFillShapes)
                                {
                                    myPrimitive._Hexagon.SetColorA(obj.a/2);
                                    myPrimitive._Hexagon.Draw(obj.x, obj.y, obj.r, true);
                                }

                                myPrimitive._Hexagon.SetColorA(obj.a);
                                myPrimitive._Hexagon.Draw(obj.x, obj.y, obj.r, false);
                            }
                        }
                        break;

                    case 1:
                        glLineWidth(lineTh);
                        myPrimitive._Triangle.SetColor(R, G, B, 1.0f);

                        for (int i = 0; i < objN; i++)
                        {
                            var obj = structsList[i];

                            if (obj.a > 0)
                            {
                                myPrimitive._Triangle.SetAngle(obj.time);

                                if (doFillShapes)
                                {
                                    myPrimitive._Triangle.SetColorA(obj.a/2);
                                    myPrimitive._Triangle.Draw(obj.x, obj.y - obj.r, obj.x - 5 * obj.r / 6, obj.y + obj.r / 2, obj.x + 5 * obj.r / 6, obj.y + obj.r / 2, true);
                                }

                                myPrimitive._Triangle.SetColorA(obj.a);
                                myPrimitive._Triangle.Draw(obj.x, obj.y - obj.r, obj.x - 5 * obj.r / 6, obj.y + obj.r / 2, obj.x + 5 * obj.r / 6, obj.y + obj.r / 2, false);
                            }
                        }
                        break;

                    case 2:
                        glLineWidth(lineTh);
                        myPrimitive._Pentagon.SetColor(R, G, B, 1.0f);

                        for (int i = 0; i < objN; i++)
                        {
                            var obj = structsList[i];

                            if (obj.a > 0)
                            {
                                myPrimitive._Pentagon.SetAngle(obj.time);

                                if (doFillShapes)
                                {
                                    myPrimitive._Pentagon.SetColorA(obj.a/2);
                                    myPrimitive._Pentagon.Draw(obj.x, obj.y, obj.r, true);
                                }

                                myPrimitive._Pentagon.SetColorA(obj.a);
                                myPrimitive._Pentagon.Draw(obj.x, obj.y, obj.r, false);
                            }
                        }
                        break;

                    case 3:
                        glLineWidth(lineTh);
                        myPrimitive._Rectangle.SetColor(R, G, B, 1.0f);

                        for (int i = 0; i < objN; i++)
                        {
                            var obj = structsList[i];

                            if (obj.a > 0)
                            {
                                myPrimitive._Rectangle.SetAngle(obj.time);

                                if (doFillShapes)
                                {
                                    myPrimitive._Rectangle.SetColorA(obj.a/2);
                                    myPrimitive._Rectangle.Draw(obj.x - obj.r, obj.y - obj.r, 2 * obj.r, 2 * obj.r, true);
                                }

                                myPrimitive._Rectangle.SetColorA(obj.a);
                                myPrimitive._Rectangle.Draw(obj.x - obj.r, obj.y - obj.r, 2 * obj.r, 2 * obj.r, false);
                            }
                        }
                        break;

                    case 4:
                        glLineWidth(1);
                        myPrimitive._Ellipse.SetColor(R, G, B, 1.0f);

                        for (int i = 0; i < objN; i++)
                        {
                            var obj = structsList[i];

                            if (obj.a > 0)
                            {
                                if (doFillShapes)
                                {
                                    myPrimitive._Ellipse.SetColorA(obj.a/2);
                                    myPrimitive._Ellipse.Draw(obj.x - obj.r, obj.y - obj.r, 2 * obj.r, 2 * obj.r, true);
                                }

                                myPrimitive._Ellipse.SetColorA(obj.a);
                                myPrimitive._Ellipse.Draw(obj.x - obj.r, obj.y - obj.r, 2 * obj.r, 2 * obj.r, false);
                            }
                        }
                        break;

                    case 5:

                        for (int i = 0; i < objN; i++)
                        {
                            var obj = structsList[i];

                            if (obj.a > 0)
                            {
                                rInst.setCoords(obj.x - obj.r, obj.y - obj.r, 2*obj.r, 2*obj.r);
                                rInst.setColor(R, G, B, obj.a);
                            }
                        }
                        break;
                }
            }

            return;
        }

        // -------------------------------------------------------------------------

        protected override void Process(Window window)
        {
            uint cnt = 0;

            myPrimitive.init_Triangle();
            myPrimitive.init_Rectangle();
            myPrimitive.init_Pentagon();
            myPrimitive.init_Hexagon();
            myPrimitive.init_Ellipse();

            rInst = new myRectangleInst(N * maxParticles);

            while (list.Count < N)
            {
                list.Add(new myObj_300());
            }

            if (doClearBuffer == false)
            {
                glDrawBuffer(GL_FRONT_AND_BACK);
            }

            // https://stackoverflow.com/questions/25548179/opengl-alpha-blending-suddenly-stops

            while (!Glfw.WindowShouldClose(window))
            {
                cnt++;

                processInput(window);

                // Swap fore/back framebuffers, and poll for operating system events.
                Glfw.SwapBuffers(window);
                Glfw.PollEvents();

                // Dim the screen constantly
                if (doClearBuffer == false)
                {
                    myPrimitive._Rectangle.SetAngle(0);
                    myPrimitive._Rectangle.SetColor(0, 0, 0, dimAlpha);
                    myPrimitive._Rectangle.Draw(0, 0, gl_Width, gl_Height, true);
                }
                else
                {
                    glClearColor(0, 0, 0, 1);
                    glClear(GL_COLOR_BUFFER_BIT);
                }

                if (doUseInstancing)
                {
                    rInst.Clear();

                    for (int i = 0; i < list.Count; i++)
                    {
                        var obj = list[i] as myObj_300;
                        obj.Show();
                        obj.Move();
                    }

                    rInst.updateInstances();

                    if (doFillShapes)
                    {
                        rInst.SetColorA(-0.5f);
                        rInst.Draw(true);
                    }

                    rInst.SetColorA(0);
                    rInst.Draw(false);
                }
                else
                {
                    for (int i = 0; i < list.Count; i++)
                    {
                        var obj = list[i] as myObj_300;
                        obj.Show();
                        obj.Move();
                    }
                }

                System.Threading.Thread.Sleep(t);
            }

            return;
        }

        unsafe void readPixel(int x, int y)
        {
            float[] pixel = new float[4];

            fixed (float * ppp = &pixel[0])
            {
                glReadPixels(x, y, 1, 1, GL_RGBA, GL_FLOAT, ppp);
            }

            ;
        }
    }
};
