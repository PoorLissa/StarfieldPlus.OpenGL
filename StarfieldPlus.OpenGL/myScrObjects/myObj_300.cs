using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;


/*
    - Small Explosions of Particles + Variations

    todo:
        - make another mode, where each square will display underlying image with its own opacity and size
*/


namespace my
{
    public class myObj_300 : myObject
    {
        private class myObj_300_Particle
        {
            public bool isFirstIteration = false;
            public float x, y, r, dx, dy, dr, a, time, dt;
        };

        // -------------------------------------------------------------------------

        private static bool doClearBuffer = false, doFillShapes = false, doUseInstancing = false, doUseCenterRepel = false;
        private static int x0, y0, t = 25, N = 1, gravityRate = 0, maxParticles = 25;
        private static int shapeType = 0, moveType = 0, radiusMode = 0, fastExplosion = 0, rotationMode = 0, rotationSubMode = 0;
        private static float dimAlpha = 0.1f;

        private static float const_f1 = 0;
        private static int   const_i1 = 0;
        private static myInstancedPrimitive inst = null;

        private float x, y, R, G, B, A, lineTh;
        private int shape = 0, lifeCounter = 0, lifeMax = 0, objN = 0;

        private List<myObj_300_Particle> structsList = null;

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
                doFillShapes  = myUtils.randomBool(rand);

                doUseCenterRepel = rand.Next(11) == 0;

                // rotationMode: 0, 1 = rotation; 2 = no rotation, angle is 0; 3 = no rotation, angle is not 0
                rotationMode = rand.Next(4);
                gravityRate = rand.Next(101) + 1;
                shapeType = rand.Next(10);
                moveType = rand.Next(36);
                radiusMode = rand.Next(5);
                fastExplosion = rand.Next(11);

                // In case the rotation is enabled, we also may enable additional rotation option:
                if (rotationMode < 2)
                {
                    rotationSubMode = rand.Next(7);
                    rotationSubMode = rotationSubMode > 2 ? 0 : rotationSubMode + 1;     // [0, 1, 2] --> [1, 2, 3]; otherwise set to '0';
                }

                const_f1 = (float)rand.NextDouble() * myUtils.randomSign(rand);
                const_i1 = rand.Next(300) + 100;

                // Set number of objects N:
                N = rand.Next(666) + 100;

                if (rand.Next(3) == 0)
                {
                    N = rand.Next(11) + 1;
                }

                t = 1;

#if true
                shapeType = 0;
                shapeType = 5;  // instanced square
                shapeType = 6;  // instanced triangle
                shapeType = 7;  // instanced circle
                shapeType = 8;  // instanced pentagon
                shapeType = 9;  // instanced hexagon
                shapeType = 5 + rand.Next(5);
                //doClearBuffer = true;
                //doClearBuffer = false;
                //radiusMode = 2;
                doUseInstancing = shapeType >= 5;
                N = 333;
                //N = 30000;
#endif
            }

            structsList = new List<myObj_300_Particle>();

            for (int i = 0; i < maxParticles; i++)
            {
                structsList.Add(new myObj_300_Particle());
            }

            generateNew();
        }

        // -------------------------------------------------------------------------

        protected override void generateNew()
        {
            x = rand.Next(gl_Width);
            y = rand.Next(gl_Height);

            // Additional set up for some specific moving modes
            {
                // Let gravity-based particles sometimes be generated higher than the top of the screen
                if (moveType == 6)
                {
                    y = rand.Next(gl_Height + 333) - 333;
                }

                // For sideways/vertical moving, generate particles offscreen as well
                if (moveType == 24)
                {
                    x = rand.Next(gl_Width + 666) - 333;
                }

                if (moveType == 25)
                {
                    y = rand.Next(gl_Height + 666) - 333;
                }
            }

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

                obj.isFirstIteration = true;

                obj.x = x;
                obj.y = y;
                obj.r = 5;

                obj.r = rand.Next(6) + 2;

                obj.dx = 0.001f * (rand.Next(max) - max/2);
                obj.dy = 0.001f * (rand.Next(max) - max/2);
                obj.a = (float)rand.NextDouble() + 0.33f;

                obj.time = 0;
                obj.dt = rotationMode < 2 ? 0.001f * rand.Next(111) * myUtils.randomSign(rand) : 0;

                // There will be no rotation, but the angle is set to non-zero
                if (rotationMode == 3)
                {
                    obj.time = (float)rand.NextDouble() * 111;
                }

                obj.dr = (radiusMode == 0) ? 0.0005f * (rand.Next(100)+1) : 0;
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
                    obj.r += obj.dr;

                    if (obj.y > gl_Height && moveType != 6)
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
                            obj.dx *= 0.999f;
                            obj.dy += 0.0033f * rand.Next(gravityRate);

                            if (obj.y >= gl_Height)
                            {
                                if (rand.Next(5) == 0)
                                    obj.a = 0;

                                obj.dy *= -0.5f;
                            }
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

                        // dx = const * dy: the const is common between all the particles
                        case 22:
                            if (obj.isFirstIteration)
                            {
                                obj.isFirstIteration = false;
                                obj.dx = const_f1 * obj.dy;
                            }
                            break;

                        // dx = const * dy: each particle has its own const
                        case 23:
                            if (obj.isFirstIteration)
                            {
                                obj.isFirstIteration = false;
                                obj.dx = (float)rand.NextDouble() * obj.dy * myUtils.randomSign(rand);
                            }
                            break;

                        // Add sideways moving component
                        case 24:
                            obj.x += const_f1 * 10;
                            break;

                        // Add vertical moving component
                        case 25:
                            obj.y += const_f1 * 10;
                            break;

                        // Add harmonic motion - y-axis-1
                        case 26:
                            obj.dy = (float)System.Math.Sin(obj.x / const_i1);
                            break;

                        // Add harmonic motion - y-axis-2
                        case 27:
                            obj.dy = (float)System.Math.Sin(obj.y / const_i1);
                            break;

                        // Add harmonic motion - y-axis-3
                        case 28:
                            obj.dy = (float)System.Math.Sin(obj.x / const_i1) * (float)rand.NextDouble();
                            break;

                        // Add harmonic motion - y-axis-4
                        case 29:
                            obj.dy = (float)System.Math.Sin(obj.y / const_i1) * (float)rand.NextDouble();
                            break;

                        // Add harmonic motion - x-axis-1
                        case 30:
                            obj.dx = (float)System.Math.Sin(obj.y / const_i1);
                            break;

                        // Add harmonic motion - x-axis-2
                        case 31:
                            obj.dx = (float)System.Math.Sin(obj.x / const_i1);
                            break;

                        // Add harmonic motion - x-axis-3
                        case 32:
                            obj.dx = (float)System.Math.Sin(obj.y / const_i1) * (float)rand.NextDouble();
                            break;

                        // Add harmonic motion - x-axis-4
                        case 33:
                            obj.dx = (float)System.Math.Sin(obj.x / const_i1) * (float)rand.NextDouble();
                            break;

                        // Sideways movement in 2 streams
                        case 34:
                            obj.x += (obj.y > gl_Height/2) ? const_f1 * 10 : -const_f1 * 10;
                            break;

                        // Ever increasing rotation speed
                        case 35:
                            obj.dt += 0.005f;
                            break;

                        // -----------------------------------------------------

                        case 333:
                            break;
                    }

                    // --- Apply some generic post-effects ---

                    // Push particles out of the centeral circle;
                    // Randomly applies to any moveType
                    // todo: optimize this, it seems to be lagging when the qty of particles is quite large
                    if (doUseCenterRepel)
                    {
                        float radius = 500.0f;

                        if (obj.x > x0 - radius && obj.x < x0 + radius && obj.y > y0 - radius && obj.y < y0 + radius)
                        {
                            float distSq = (x0 - obj.x) * (x0 - obj.x) + (y0 - obj.y) * (y0 - obj.y);

                            if (distSq < radius*radius)
                            {
                                float dx = (obj.x - x0) / (distSq / 2 / radius);
                                float dy = (obj.y - y0) / (distSq / 2 / radius);

                                obj.x += dx;
                                obj.y += dy;
                            }
                        }
                    }

                    // Fast explosion, then abrupt stop and slow motion;
                    // Randomly applies to any moveType
                    if (fastExplosion == 0)
                    {
                        if (lifeCounter < lifeMax + 66)
                        {
                            obj.dx *= 1.025f;
                            obj.dy *= 1.025f;
                        }

                        if (lifeCounter == lifeMax + 66)
                        {
                            obj.dx /= 3;
                            obj.dy /= 3;
                        }

                        if (lifeCounter == lifeMax + 67)
                        {
                            obj.dx /= 3;
                            obj.dy /= 3;
                        }

                        if (lifeCounter > lifeMax + 67)
                        {
                            obj.dx *= 1.005f;
                            obj.dy *= 1.005f;
                            obj.r -= 0.01f;
                        }
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

                    // Instanced squares
                    case 5:
                        var rectInst = inst as myRectangleInst;

                        for (int i = 0; i < objN; i++)
                        {
                            var obj = structsList[i];

                            if (obj.a > 0)
                            {
                                rectInst.setInstanceCoords(obj.x - obj.r, obj.y - obj.r, 2*obj.r, 2*obj.r);
                                rectInst.setInstanceColor(R, G, B, obj.a);
                                rectInst.setInstanceAngle(obj.time);
                            }
                        }
                        break;

                    // Instanced triangles
                    case 6:
                        var triangleInst = inst as myTriangleInst;

                        for (int i = 0; i < objN; i++)
                        {
                            var obj = structsList[i];

                            if (obj.a > 0)
                            {
                                triangleInst.setInstanceCoords(obj.x - obj.r, obj.y - obj.r, 2 * obj.r, obj.time);
                                triangleInst.setInstanceColor(R, G, B, obj.a);
                            }
                        }
                        break;

                    // Instanced circles
                    case 7:
                        var ellipseInst = inst as myEllipseInst;

                        for (int i = 0; i < objN; i++)
                        {
                            var obj = structsList[i];

                            if (obj.a > 0)
                            {
                                ellipseInst.setInstanceCoords(obj.x - obj.r, obj.y - obj.r, 2 * obj.r, obj.time);
                                ellipseInst.setInstanceColor(R, G, B, obj.a);
                            }
                        }
                        break;

                    // Instanced pentagons
                    case 8:
                        var pentagonInst = inst as myPentagonInst;

                        for (int i = 0; i < objN; i++)
                        {
                            var obj = structsList[i];

                            if (obj.a > 0)
                            {
                                pentagonInst.setInstanceCoords(obj.x - obj.r, obj.y - obj.r, 2 * obj.r, obj.time);
                                pentagonInst.setInstanceColor(R, G, B, obj.a);
                            }
                        }
                        break;

                    // Instanced hexagons
                    case 9:
                        var hexagonInst = inst as myHexagonInst;

                        for (int i = 0; i < objN; i++)
                        {
                            var obj = structsList[i];

                            if (obj.a > 0)
                            {
                                hexagonInst.setInstanceCoords(obj.x, obj.y, 2*obj.r, obj.time);
                                hexagonInst.setInstanceColor(R, G, B, obj.a);
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

            switch (shapeType)
            {
                case 5:
                    myPrimitive.init_RectangleInst(N * maxParticles);
                    myPrimitive._RectangleInst.setRotationMode(rotationSubMode);
                    inst = myPrimitive._RectangleInst;
                    break;

                case 6:
                    myPrimitive.init_TriangleInst(N * maxParticles);
                    myPrimitive._TriangleInst.setRotationMode(rotationSubMode);
                    inst = myPrimitive._TriangleInst;
                    break;

                case 7:
                    myPrimitive.init_EllipseInst(N * maxParticles);
                    myPrimitive._EllipseInst.setRotationMode(rotationSubMode);
                    inst = myPrimitive._EllipseInst;
                    break;

                case 8:
                    myPrimitive.init_PentagonInst(N * maxParticles);
                    myPrimitive._PentagonInst.setRotationMode(rotationSubMode);
                    inst = myPrimitive._PentagonInst;
                    break;

                case 9:
                    myPrimitive.init_HexagonInst(N * maxParticles);
                    myPrimitive._HexagonInst.setRotationMode(rotationSubMode);
                    inst = myPrimitive._HexagonInst;
                    break;
            }

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
                    // Shift background color just a bit, to hide long lasting traces of shapes
                    myPrimitive._Rectangle.SetColor(rand.Next(5) * 0.01f, rand.Next(5) * 0.01f, rand.Next(5) * 0.01f, dimAlpha);
                    myPrimitive._Rectangle.Draw(0, 0, gl_Width, gl_Height, true);
                }
                else
                {
                    glClearColor(0, 0, 0, 1);
                    glClear(GL_COLOR_BUFFER_BIT);
                }

                if (doUseInstancing)
                {
                    inst.ResetBuffer();

                    for (int i = 0; i < list.Count; i++)
                    {
                        var obj = list[i] as myObj_300;
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

        // -------------------------------------------------------------------------

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
