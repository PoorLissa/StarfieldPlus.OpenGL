using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;


/*
    - Generator of waves that are made of particles
*/


namespace my
{
    public class myObj_180 : myObject
    {
        private delegate float family1Delegate(float f1, float f2);

        // ---------------------------------------------------------------------------------------------------------------

        private static bool doClearBuffer = false, doFillShapes = false, doUseDispersion = false, doUseXOffset = false, doUseRandomSpeed = false,
                            doUseIncreasingWaveSize = false, doShiftCenter = false, dXYgen_useRandSign = false;
        private static int x0, y0, N = 1, deadCnt = 0, waveSizeBase = 3111, WaveLifeCnt = 0, LifeCntBase = 0;
        private static int shapeType = 0, rotationMode = 0, rotationSubMode = 0, dispersionMode = 0, delegateNo = -1, rateBase = 50, rateMode = 0;
        private static int heightRatioMode = 0, connectionMode = 0, dXYgenerationMode = -1;
        private static float t = 0, R, G, B, A, dimAlpha = 0.1f, Speed = 1.0f, speedBase = 1.0f, 
                                            dispersionConst = 0, heightRatio = 1.0f, xOffset = 0, dSize = 0;

        private static myInstancedPrimitive inst = null;

        private float x, y, r, g, b, a;
        private int lifeCnt = 0;
        private int shape = 0;

        private bool isLive = false;
        private float dx, dy, size = 0, angle = 0, dAngle = 0, dispersionRateX = 1.0f, dispersionRateY = 1.0f, acceleration = 1.0f;

        // Function Generation
        static int argmode1, argmode2, argmode3;
        static myFuncGenerator1.Targets target;
        static myFuncGenerator1.Funcs   f1, f2;
        static myFuncGenerator1.eqModes eqMode;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_180()
        {
            if (colorPicker == null)
            {
                init();
            }

            generateNew();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time initialization
        private void init()
        {
            x0 = gl_Width / 2;
            y0 = gl_Height / 2;

            colorPicker = new myColorPicker(gl_Width, gl_Height);

            list = new List<myObject>();

            doClearBuffer = myUtils.randomChance(rand, 4, 5);
            doFillShapes = myUtils.randomBool(rand);
            doUseDispersion = myUtils.randomChance(rand, 2, 3);
            doUseXOffset = myUtils.randomChance(rand, 1, 7);
            doShiftCenter = myUtils.randomChance(rand, 1, 6);
            doUseRandomSpeed = myUtils.randomBool(rand);
            doUseIncreasingWaveSize = myUtils.randomChance(rand, 1, 3);
            rateMode = rand.Next(3);
            dispersionMode = rand.Next(6);
            heightRatioMode = rand.Next(5);

            // Set up x-offset
            if (doUseXOffset)
            {
                xOffset = 0.0001f * (rand.Next(101) - 50);
            }

            // Set up Dispersion: in modes 5 and 6 dispersion rate will be very close to 1
            {
                if (dispersionMode == 5)
                {
                    dispersionConst = (myUtils.randomBool(rand) ? 0.9f : 0.95f) + 0.0001f * rand.Next(1000);
                }
                else if (dispersionMode == 6)
                {
                    dispersionConst = (myUtils.randomBool(rand) ? 0.95f : 0.975f) + 0.0001f * rand.Next(500);
                }
                else
                {
                    if (myUtils.randomBool(rand))
                    {
                        dispersionConst = (float)rand.NextDouble();
                    }
                    else
                    {
                        dispersionConst = 1.0f + rand.Next(5) + (float)rand.NextDouble();
                    }
                }
            }

            // Set up Height Ratio:
            if (heightRatioMode == 3)
            {
                heightRatio = (float)rand.NextDouble();
            }

            // rotationMode: 0, 1 = rotation; 2 = no rotation, angle is 0; 3 = no rotation, angle is not 0
            rotationMode = rand.Next(4);
            shapeType = rand.Next(5);
            LifeCntBase = rand.Next(333) + 111;
            connectionMode = rand.Next(5);
            speedBase = 1.0f + 0.001f * rand.Next(2000);
            dSize = (float)rand.NextDouble() / 100;

            // In case the rotation is enabled, we also may enable additional rotation option:
            if (rotationMode < 2)
            {
                rotationSubMode = rand.Next(7);
                rotationSubMode = rotationSubMode > 2 ? 0 : rotationSubMode + 1;     // [0, 1, 2] --> [1, 2, 3]; otherwise set to '0';
            }

            // todo: remove true later
            // Set up additional dx/dy generation mode:
            if (true || myUtils.randomChance(rand, 1, 3))
            {
                dXYgenerationMode = rand.Next(2);
                dXYgenerationMode = 1;
                dXYgen_useRandSign = myUtils.randomBool(rand);

                if (dXYgenerationMode == 0 || dXYgenerationMode == 1)
                {
                    myFuncGenerator1.myExpr.rand = rand;

                    int argModesQty = (int)myFuncGenerator1.myArgs.argsFunc(0, 0, -1);

                    target = (myFuncGenerator1.Targets)rand.Next(2);
                    f1 = (myFuncGenerator1.Funcs)rand.Next(myFuncGenerator1.myFuncs.iFunc(myFuncGenerator1.Funcs.NONE, 0));
                    f2 = (myFuncGenerator1.Funcs)rand.Next(myFuncGenerator1.myFuncs.iFunc(myFuncGenerator1.Funcs.NONE, 0));
                    argmode1 = rand.Next(argModesQty);
                    argmode1 = rand.Next(argModesQty);
                    argmode1 = rand.Next(argModesQty);
                    eqMode = (myFuncGenerator1.eqModes)rand.Next(5);
                }
            }

            // Set number of objects N:
            N = 100000;
            renderDelay = 1;

#if true
            doUseDispersion = false;
            doUseIncreasingWaveSize = false;
            doShiftCenter = false;
            doUseXOffset = false;
            heightRatioMode = 1;
#endif
            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo()
        {
            string getFuncGeneratorParams()
            {
                return $"\n" + 
                       $"dXYgenerationMode = {dXYgenerationMode}\n" +
                       $"dXYgen_useRandSign = {dXYgen_useRandSign}\n" +
                       $"target = {target}\n" +
                       $"eqMode = {eqMode}\n" + 
                       $"f1 = {f1}\n" +
                       $"f2 = {f2}\n" +
                       $"argmode1 = {argmode1}\n" +
                       $"argmode2 = {argmode2}\n" +
                       $"argmode3 = {argmode3}"
                ;
            }

            string str = $"Obj = myObj_180\n\n" +
                            $"N = {N}\n" +
                            $"deadCnt = {deadCnt}\n" + 
                            $"renderDelay = {renderDelay}\n" +
                            $"shapeType = {shapeType}\n" +
                            $"rotationMode = {rotationMode}\n" +
                            $"rotationSubMode = {rotationSubMode}\n" +
                            $"doUseDispersion = {doUseDispersion}\n" +
                            $"dispersionMode = {dispersionMode}\n" +
                            $"dispersionConst = {dispersionConst}\n" +
                            $"connectionMode = {connectionMode}\n" +
                            $"heightRatioMode = {heightRatioMode}\n" +
                            $"doUseXOffset = {doUseXOffset}\n" +
                            $"doShiftCenter = {doShiftCenter}\n" +
                            $"LifeCntBase = {LifeCntBase}\n" +
                            getFuncGeneratorParams()
                ;
            return str;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void generateNew()
        {
            isLive = false;

            x = rand.Next(gl_Width);
            y = rand.Next(gl_Width);

            shape = shapeType;
            lifeCnt = WaveLifeCnt;

            float dist = (float)(Math.Sqrt((x - x0) * (x - x0) + (y - x0) * (y - x0)));

            dx = (x - x0) * Speed / dist;
            dy = (y - x0) * Speed / dist;

            if (heightRatioMode > 2)
            {
                dy *= (heightRatioMode == 3) ? heightRatio : (float)rand.NextDouble();
            }

            // Additionally modify dx/dy in order to get various non-elliptical shapes
            if (dXYgenerationMode > -1)
            {
                addDxDyModifier(ref dx, ref dy, x, y, dist);
            }

            x = x0;
            y = y0;

            r = R + 0.01f * (rand.Next(11) - 5);
            g = G + 0.01f * (rand.Next(11) - 5);
            b = B + 0.01f * (rand.Next(11) - 5);
            a = A + 0.01f * (rand.Next(11) - 5);

            angle = 0;
            dAngle = rotationMode < 2 ? 0.001f * rand.Next(111) * myUtils.randomSign(rand) : 0;

            if (doUseDispersion)
            {
                switch (dispersionMode)
                {
                    case 0:
                        dispersionRateX = dispersionRateY = (float)rand.NextDouble();
                        break;

                    case 1:
                        dispersionRateX = (float)rand.NextDouble();
                        dispersionRateY = (float)rand.NextDouble();
                        break;

                    case 2:
                        dispersionRateX = dispersionRateY = dispersionConst * (float)rand.NextDouble();
                        break;

                    case 3:
                        dispersionRateX = dispersionConst * (float)rand.NextDouble();
                        dispersionRateY = dispersionConst * (float)rand.NextDouble();
                        break;

                    case 4:
                    case 5:
                    case 6:
                        dispersionRateX = dispersionConst;
                        dispersionRateY = dispersionConst;
                        break;
                }
            }

            size = 1;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            if (isLive)
            {
                x += dx;
                y += dy;
                angle += dAngle;

                // gravity
                //dy += (float)Math.Sin(t)/10;

                if (--lifeCnt < 0)
                {
                    a -= 0.0025f;

                    if (a < 0)
                    {
                        x = -1111;
                        y = -1111;
                    }
                }

                size += 0.0025f;
                size += dSize;

                if (doUseXOffset)
                {
                    dx += xOffset;
                }

                if (doUseDispersion && rand.Next(100) == 0)
                {
                    dx *= dispersionRateX;
                    dy *= dispersionRateY;
                }

                if (x < 0 || x > gl_Width || y < 0 || y > gl_Height)
                {
                    isLive = false;
                    deadCnt++;
                    x = -1111;
                    y = -1111;
                }
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Show()
        {
            switch (shape)
            {
                // Instanced squares
                case 0:
                    var rectInst = inst as myRectangleInst;

                    rectInst.setInstanceCoords(x - size, y - size, 2*size, 2*size);
                    rectInst.setInstanceColor(r, g, b, a);
                    rectInst.setInstanceAngle(angle);
                    break;

                // Instanced triangles
                case 1:
                    var triangleInst = inst as myTriangleInst;
                    
                    triangleInst.setInstanceCoords(x, y, 2*size, angle);
                    triangleInst.setInstanceColor(r, g, b, a);
                    break;

                // Instanced circles
                case 2:
                    var ellipseInst = inst as myEllipseInst;

                    ellipseInst.setInstanceCoords(x, y, 2*size, angle);
                    ellipseInst.setInstanceColor(r, g, b, a);
                    break;

                // Instanced pentagons
                case 3:
                    var pentagonInst = inst as myPentagonInst;

                    pentagonInst.setInstanceCoords(x, y, 2*size, angle);
                    pentagonInst.setInstanceColor(r, g, b, a);
                    break;

                // Instanced hexagons
                case 4:
                    var hexagonInst = inst as myHexagonInst;

                    hexagonInst.setInstanceCoords(x, y, 2*size, angle);
                    hexagonInst.setInstanceColor(r, g, b, a);
                    break;
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Process(Window window)
        {
            uint cnt = 0;
            int rate = rateBase;
            int waveSize = doUseIncreasingWaveSize ? 1 : waveSizeBase;
            int dWaveSize = doUseIncreasingWaveSize ? rand.Next(17) + 1 : 0;

            float xold = x0;
            float yold = y0;

            initShapes();

            while (list.Count < N)
            {
                list.Add(new myObj_180());
                deadCnt++;
            }

            if (doClearBuffer)
            {
                glDrawBuffer(GL_FRONT_AND_BACK | GL_DEPTH_BUFFER_BIT);
                glClearColor(0, 0, 0, 1);
            }

            while (!Glfw.WindowShouldClose(window))
            {
                cnt++;

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
                    // Dim the screen constantly;
                    // Shift background color just a bit, to hide long lasting traces of shapes
                    float r = (float)Math.Sin(cnt * 0.001f) * 0.03f;
                    float g = (float)Math.Cos(cnt * 0.002f) * 0.03f;
                    float b = (float)Math.Sin(cnt * 0.003f) * 0.03f;
                    myPrimitive._Rectangle.SetColor(r, g, b, dimAlpha);
                    myPrimitive._Rectangle.SetAngle(0);
                    myPrimitive._Rectangle.Draw(0, 0, gl_Width, gl_Height, true);
                }

                // Render Frame
                {
                    if (doShiftCenter)
                    {
                        x0 += (rand.Next(3) - 1);
                        y0 += (rand.Next(3) - 1);
                    }

                    inst.ResetBuffer();

                    if (connectionMode > 2)
                        myPrimitive._LineInst.ResetBuffer();

                    int newWaveCnt = (cnt % rate == 0) && deadCnt >= waveSize ? waveSize : 0;

                    if (newWaveCnt > 0)
                    {
                        WaveLifeCnt = LifeCntBase + rand.Next(333);
                        Speed = doUseRandomSpeed ? 0.75f + 0.01f * rand.Next(500) : speedBase;
                        colorPicker.getColorRand(ref R, ref G, ref B, ref A);

                        if (waveSize < waveSizeBase)
                            waveSize += dWaveSize;

                        // Vary rate:
                        //  rateMode = 0: const pace
                        //  rateMode = 1: sin of time
                        //  rateMode = 2: random pick
                        switch (rateMode)
                        {
                            case 1:
                                rate = (int)(Math.Abs(Math.Sin(t)) * rateBase) + 5;
                                break;

                            case 2:
                                rate = rand.Next(rateBase * 2) + 5;
                                break;
                        }
                    }

                    for (int i = 0; i < list.Count; i++)
                    {
                        var obj = list[i] as myObj_180;

                        if (obj.isLive)
                        {
                            obj.Show();
                            obj.Move();

                            if (obj.isLive)
                            {
                                if (connectionMode == 3)
                                {
                                    myPrimitive._LineInst.setInstanceCoords(obj.x, obj.y, x0, y0);
                                    myPrimitive._LineInst.setInstanceColor(1, 1, 1, 0.02f);
                                }

                                if (connectionMode == 4)
                                {
                                    myPrimitive._LineInst.setInstanceCoords(obj.x, obj.y, xold, yold);
                                    myPrimitive._LineInst.setInstanceColor(1, 1, 1, 0.05f);

                                    xold = obj.x;
                                    yold = obj.y;
                                }
                            }
                        }
                        else
                        {
                            if (newWaveCnt > 0)
                            {
                                obj.generateNew();
                                obj.isLive = true;
                                deadCnt--;
                                newWaveCnt--;
                            }
                        }
                    }

                    if (connectionMode > 2)
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
                }

                t += 0.001f;

                System.Threading.Thread.Sleep(renderDelay);
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void initShapes()
        {
            myPrimitive.init_Rectangle();

            if (connectionMode > 2)
                myPrimitive.init_LineInst(N);

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

        // todo: test later how much slower is using this family business instead of direct 1-level switch
        private void addDxDyModifier(ref float dx, ref float dy, float x, float y, float dist)
        {
            float tmp = 0;

            if (false)
            {
                float dx2 = dx;
                float dy2 = dy;

                dy /= (float)Math.Sin(dx + dy);

                tmp = myFuncGenerator1.myFuncs.fFunc(myFuncGenerator1.Funcs.SIN, myFuncGenerator1.myArgs.argsFunc(dx2, dy2, 2));
                myFuncGenerator1.myExpr.expr(ref dx2, ref dy2, myFuncGenerator1.Targets.SECOND, myFuncGenerator1.eqModes.DIV, dXYgen_useRandSign, tmp);

                if (dx2 != dx || dy2 != dy)
                {
                    ;
                }

                return;

                dx -= (float)Math.Sqrt(dx / dy);
                dx *= myUtils.randomSign(rand);
            }

            // Set predefined mode
            if (false)
            {
                int[] arr0 = { 0, 4, 1, 0, 3, 9, 0 };
                int[] arr1 = { 1, 2, 2, 2, 1, 0, 0 };           // heart shaped box -- dXYgenerationMode 0
                int[] arr2 = { 1, 4, 2, 2, 1, 0, 0 };           // 

                var arr = arr2;

                target = (myFuncGenerator1.Targets)arr[0];
                eqMode = (myFuncGenerator1.eqModes)arr[1];
                f1 = (myFuncGenerator1.Funcs)arr[2];
                f1 = (myFuncGenerator1.Funcs)arr[3];
                argmode1 = arr[4];
                argmode2 = arr[5];
                argmode3 = arr[6];
            }

            switch (dXYgenerationMode)
            {
                // y += Sin(x+y); y = Cos(x/y);
                case 0:
                    myFuncGenerator1.myExpr.expr(ref dx, ref dy, target, eqMode, dXYgen_useRandSign,
                        myFuncGenerator1.myFuncs.fFunc(f1, myFuncGenerator1.myArgs.argsFunc(dx, dy, argmode1)));
                    break;

                // y += Sin(x+y) * (x-y); y = Cos(x/y) + (x+y);
                case 1:
                    myFuncGenerator1.myExpr.expr(ref dx, ref dy, target, eqMode, dXYgen_useRandSign,
                        myFuncGenerator1.myArgs.argsFunc(myFuncGenerator1.myFuncs.fFunc(f1, myFuncGenerator1.myArgs.argsFunc(dx, dy, argmode1)),
                                                         myFuncGenerator1.myArgs.argsFunc(dx, dy, argmode2),
                                                         argmode3
                    ));
                    break;

                // -------------------------------------------------------------

                case 721:
                    dx = (float)(Math.Cos(dx) * Math.Sin(dx) * 2);
                    dy = (float)(Math.Cos(dy) * Math.Sin(dy) * 2);
                    break;

                case 722:
                    tmp = dx;
                    dx = (float)Math.Cos(dy) * (float)Math.Sin(dy) * 2;
                    dy = (float)Math.Cos(tmp) * (float)Math.Sin(tmp) * 2;
                    break;

                // -------------------------------------------------------------

                case 999:

                    //dy = (float)Math.Sin(dx) + (float)Math.Sin(dy);
                    //dy = (float)Math.Sin(dx) + (float)Math.Cos(dy);

                    //dy = (float)(Math.Cos(dx+dy) + Math.Sin(dx+dy));
                    //dy = (float)Math.Cos(dx) + (float)Math.Sin(dy);

                    //dy = (float)Math.Cos(dx)*dy;

                    //dy = (float)Math.Cos(dx + dy) * (dy + dx);

                    //dy = (float)Math.Cos(dx * dy) * (dy * dx);

                    //dy = (float)Math.Cos(dx * dy) * (dy / dx);

                    //dy = (float)Math.Cos(dx + dy) * (dy / dx);

                    //dy = (float)Math.Cos(dx - dy) * (dy / dx);

                    //dy = (float)Math.Cos(dx / dy) * (dy / dx);

                    //dy = (float)Math.Cos(dx / dy) * (dy + dx);    // khm

                    //dy = (float)Math.Sin(dx + dy) + (float)Math.Cos(dy+dx);

                    //dy = (float)(Math.Sin(dx)) / dy;

                    //dy = (float)(Math.Cos(dx)) / dy;

                    //dx = (float)(Math.Cos(dy)) / dx;

                    //dx = (float)(Math.Sin(dy)) / dx;

                    //dy = (int)(Math.Cos(dy) * dy * 2);

                    //dy += (int)(Math.Sin(dx) * Math.Cos(dy) * 2);

                    //dy *= (int)(Math.Sin(dx) * Math.Cos(dy) * 2);

                    //dy /= (int)(Math.Sin(dx) * 2);

                    // together
                    //dx += (int)(Math.Sin(dy) * Math.Cos(dx) * 2);
                    //dy += (int)(Math.Sin(dx) * Math.Cos(dy) * 2);

                    // together
                    //dx = (int)(Math.Sin(dx) * dy * 3);
                    //dy = (int)(Math.Cos(dy) * dx * 3);

                    // together
                    //dx = (float)(Math.Cos(dx))/dx;
                    //dy = (float)(Math.Sin(dy))/dy;

                    // together
                    //dx = (float)(Math.Sin(dx))/dy;
                    //dy = (float)(Math.Cos(dy))/dx;

                    // together
                    //dx = (float)(Math.Sin(dy))/dx;
                    //dy = (float)(Math.Cos(dx))/dy;


                    // =============================================================

                    // together
                    //dx = (dx > 0 ? 1 : -1) * (float)(Math.Sqrt(Math.Abs(dx)));
                    //dy = (dy > 0 ? 1 : -1) * (float)(Math.Sqrt(Math.Abs(dy)));

                    //myTest.myFunc1.func_001(ref dx, ref dy, 2, mode: 1, myTest.BasicFunctions.SIN);

                    //dy -= (float)Math.Sin(dx/dy);
                    //dy = (float)Math.Sin(dy/dx);

                    //myTest.myFunc1.func_001(ref dx, ref dy, 2, argMode: 6, myTest.BasicFunctions.SIN, myTest.Operations.MINUS);

                    //myTest.myFunc1.func_001(ref dx, ref dy, myTest.Targets.SECOND, 8, myTest.BasicFunctions.SQRT, myTest.Operations.MINUS);

                    //myTest.myFunc1.func_001(ref dx, ref dy, myTest.Targets.SECOND, 0, myTest.BasicFunctions.SQRT, myTest.Operations.EQUALS);

                    //dy = (float)Math.Sin(dx+dy) + (float)Math.Cos(dy+dx);

                    // it was working. see what it does
                    //myFuncGenerator0.myFunc1.func_002(ref dx, ref dy, target, targetOp, f1, argmode1, f2, argmode2, argmode3);

                    // looks like frozen droplets from the puddle !
                    //dy /= (float)Math.Sqrt(y);
                    break;
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------
    }
};
