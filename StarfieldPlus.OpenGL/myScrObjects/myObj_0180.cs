﻿using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;


/*
    - Generator of waves that are made of large number of particles
*/


namespace my
{
    public class myObj_0180 : myObject
    {
        // Priority
        public static int Priority => 20;
		public static System.Type Type => typeof(myObj_0180);

        private static bool doFillShapes = false, doUseDispersion = false, doUseXOffset = false, doUseRandomSpeed = false,
                            doUseIncreasingWaveSize = false, doShiftCenter = false, dXYgen_useRandSign1 = false, dXYgen_useRandSign2 = false,
                            doUseIntConversion = false, doUseStartDispersion = false, doShowParticles = true, doRandomizeCenter = false,
                            doUseTestFunc = false, doDrawTwice = true, doUsePredefinedMode = true;

        private static int x0, y0, N = 1, deadCnt = 0, waveSizeBase = 3111, WaveLifeCnt = 0, LifeCntBase = 0, xyGenMode = 0,
                           shapeType = 0, rotationMode = 0, rotationSubMode = 0, dispersionMode = 0, rateBase = 50, rateMode = 0,
                           heightRatioMode = 0, connectionMode = 0, dXYgenerationMode = -1, startDispersionRate = 0, centerRandSize = 0,
                           testFuncNo = -1;

        private static float t = 0, R, G, B, A, dimAlpha = 0.1f, Speed = 1.0f, speedBase = 1.0f, 
                                            dispersionConst = 0, heightRatio = 1.0f, xOffset = 0, dSize = 0;

        private int     lifeCnt = 0, shape = 0;
        private bool    isLive = false;
        private float   x, y, r, g, b, a, dx, dy, size = 0, size2x, angle = 0, dAngle = 0, dispersionRateX = 1.0f, dispersionRateY = 1.0f;

        // Function Generation Params
        static int argmode1, argmode2, argmode3, argmode4, argmode5, argmode6;
        static myFuncGenerator1.Targets target;
        static myFuncGenerator1.Funcs   f1, f2, f3, f4;
        static myFuncGenerator1.eqModes eqMode1, eqMode2;

        private static myScreenGradient grad = null;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_0180()
        {
            generateNew();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time global initialization
        protected override void initGlobal()
        {
            colorPicker = new myColorPicker(gl_Width, gl_Height);
            list = new List<myObject>();
            myFuncGenerator1.myExpr.rand = rand;

            dimAlpha = 0.01f + myUtils.randFloat(rand) * 0.09f;

            N = 1111 + rand.Next(2345);
            renderDelay = rand.Next(10);

            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time initialization
        private void initLocal()
        {
            x0 = gl_x0;
            y0 = gl_y0;

            dXYgenerationMode = -1;
            startDispersionRate = 5;

            doShowParticles         = true;
            doUsePredefinedMode     = false;
            doFillShapes            = myUtils.randomBool(rand);
            doUseRandomSpeed        = myUtils.randomBool(rand);
            doClearBuffer           = myUtils.randomChance(rand, 4, 5);
            doDrawTwice             = myUtils.randomChance(rand, 1, 2);
            doUseDispersion         = myUtils.randomChance(rand, 1, 3);
            doUseStartDispersion    = myUtils.randomChance(rand, 1, 3);
            doUseXOffset            = myUtils.randomChance(rand, 1, 7);
            doShiftCenter           = myUtils.randomChance(rand, 1, 6);
            doRandomizeCenter       = myUtils.randomChance(rand, 1, 5);
            doUseIntConversion      = myUtils.randomChance(rand, 1, 5);
            doUseIncreasingWaveSize = myUtils.randomChance(rand, 1, 3);
            doUseTestFunc           = myUtils.randomChance(rand, 1, 20);
            rateMode                = rand.Next(3);
            dispersionMode          = rand.Next(6);
            heightRatioMode         = rand.Next(5);
            xyGenMode               = rand.Next(2);

            // Set up x-offset
            if (doUseXOffset)
            {
                xOffset = 0.0001f * (rand.Next(101) - 50);
            }

            // Set up Dispersion: in modes 5 and 6 dispersion rate will be very close to 1
            {
                if (doUseStartDispersion)
                {
                    switch (rand.Next(3))
                    {
                        case 0: startDispersionRate += rand.Next(11); break;
                        case 1: startDispersionRate += rand.Next(22); break;
                        case 2: startDispersionRate += rand.Next(33); break;
                    }
                }

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
            centerRandSize = rand.Next(100) + 10;

            if (connectionMode > 2)
            {
                doShowParticles = myUtils.randomChance(rand, 4, 5);
            }

            // In case the rotation is enabled, we also may enable additional rotation option:
            if (rotationMode < 2)
            {
                rotationSubMode = rand.Next(7);
                rotationSubMode = rotationSubMode > 2 ? 0 : rotationSubMode + 1;     // [0, 1, 2] --> [1, 2, 3]; otherwise set to '0';
            }

            // Set up additional dx/dy generation mode:
            if (myUtils.randomChance(rand, 2, 5))
            {
                dXYgen_useRandSign1 = myUtils.randomBool(rand);
                dXYgen_useRandSign2 = myUtils.randomBool(rand);

                if (myUtils.randomBool(rand))
                {
                    // Randomly generated mode:
                    dXYgenerationMode = rand.Next(6);

                    int argModesQty = (int)myFuncGenerator1.myArgs.argsFunc(-1, 0, 0);
                    int funcsQty = (int)myFuncGenerator1.myFuncs.fFunc(myFuncGenerator1.Funcs.NONE, 0);

                    target = (myFuncGenerator1.Targets)rand.Next(2);
                    f1 = (myFuncGenerator1.Funcs)rand.Next(funcsQty);
                    f2 = (myFuncGenerator1.Funcs)rand.Next(funcsQty);
                    f3 = (myFuncGenerator1.Funcs)rand.Next(funcsQty);
                    f4 = (myFuncGenerator1.Funcs)rand.Next(funcsQty);
                    argmode1 = rand.Next(argModesQty);
                    argmode2 = rand.Next(argModesQty);
                    argmode3 = rand.Next(argModesQty);
                    argmode4 = rand.Next(argModesQty);
                    argmode5 = rand.Next(argModesQty);
                    argmode6 = rand.Next(argModesQty);
                    eqMode1 = (myFuncGenerator1.eqModes)rand.Next(5);
                    eqMode2 = (myFuncGenerator1.eqModes)rand.Next(5);
                }
                else
                {
                    // Predefined mode:
                    setPredefinedMode();
                }
            }

            // Set number of objects N:
            N = 100000 + rand.Next(250000);
            renderDelay = 1;

#if DEBUG && false
            doUseDispersion = false;
            doUseStartDispersion = false;
            doShowParticles = true;
            doUseIncreasingWaveSize = false;
            doShiftCenter = false;
            doUseXOffset = false;
            doRandomizeCenter = false;
            heightRatioMode = 1;
#endif

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            height = 900;

            string getFuncGeneratorParams()
            {
                return $"\n" + 
                       $"dXYgenerationMode = {dXYgenerationMode}\n" +
                       $"dXYgen_useRandSign = ({dXYgen_useRandSign1}, {dXYgen_useRandSign2})\n" +
                       $"target = {target}\n" +
                       $"eqMode = ({eqMode1}, {eqMode2})\n" +
                       $"f = ({f1}, {f2}, {f3}, {f4})\n" +
                       $"argmode = ({argmode1}, {argmode2}, {argmode3}, {argmode4}, {argmode5}, {argmode6})"
                ;
            }

            string getFuncGeneratorParamsShort()
            {
                string res = "";

                switch (dXYgenerationMode)
                {
                    case 0:
                    case 1:
                    case 2:
                        res = $"{(int)dXYgenerationMode}, {(int)target}, {(int)eqMode1}, {(int)f1}, {(int)f2}, {(int)argmode1}, {(int)argmode2}, {(int)argmode3}";
                        break;

                    case 3:
                        res = $"{(int)dXYgenerationMode}, {(int)target}, {(int)eqMode1}, {(int)f1}, {(int)f2}, {(int)argmode1}, {(int)argmode2}, {(int)argmode3}, {(int)eqMode2}";
                        break;

                    case 4:
                        res = $"{(int)dXYgenerationMode}, {(int)target}, {(int)eqMode1}, {(int)f1}, {(int)f2}, {(int)argmode1}, {(int)argmode2}, {(int)argmode3}, {(int)eqMode2}, {(int)f3}, {(int)f4}, {(int)argmode4}, {(int)argmode5}, {(int)argmode6}";
                        break;
                }

                return $"\n Short: {res}";
            }

            string fStr(float f) { return f.ToString("0.000"); }

            string str = $"Obj = {Type}\n\n"                                 	+
                            $"N = {N}\n"                                        +
                            $"deadCnt = {deadCnt}\n"                            +
                            $"doClearBuffer = {doClearBuffer}\n"                +
                            $"doUsePredefinedMode = {doUsePredefinedMode}\n"    +
                            $"doDrawTwice = {doDrawTwice}\n"                    +
                            $"shapeType = {shapeType}\n"                        +
                            $"rotationMode = {rotationMode}\n"                  +
                            $"rotationSubMode = {rotationSubMode}\n"            +
                            $"doUseDispersion = {doUseDispersion}\n"            +
                            $"doUseStartDispersion = {doUseStartDispersion}\n"  +
                            $"dispersionMode = {dispersionMode}\n"              +
                            $"dispersionConst = {dispersionConst}\n"            +
                            $"connectionMode = {connectionMode}\n"              +
                            $"heightRatioMode = {heightRatioMode}\n"            +
                            $"doUseXOffset = {doUseXOffset}\n"                  +
                            $"doShiftCenter = {doShiftCenter}\n"                +
                            $"doUseIntConversion = {doUseIntConversion}\n"      +
                            $"doShowParticles = {doShowParticles}\n"            +
                            $"LifeCntBase = {LifeCntBase}\n"                    +
                            $"dimAlpha = {fStr(dimAlpha)}\n"                    +
                            $"renderDelay = {renderDelay}\n"                    +
                            getFuncGeneratorParams() + "\n"                     +
                            getFuncGeneratorParamsShort()
                ;
            return str;
        }

        // ---------------------------------------------------------------------------------------------------------------

        // 
        protected override void setNextMode()
        {
            // Keep shape type and connection mode, as changing those requires also reinitializing other primitives;
            // todo: fix this sometimes later
            var oldShapeType = shapeType;
            var oldConnectionMode = connectionMode;

            initLocal();

            grad.SetOpacity(doClearBuffer ? 1 : dimAlpha);

            shapeType = oldShapeType;
            connectionMode = oldConnectionMode;

            // todo: remove this when the one above is fixed
            if (connectionMode <= 2)
            {
                doShowParticles = true;
            }
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void generateNew()
        {
            isLive = false;

            x = rand.Next(gl_Width);
            y = rand.Next(gl_Width);

            shape = shapeType;
            lifeCnt = WaveLifeCnt;

            float dist = 0;

            switch (xyGenMode)
            {
                case 0:
                    dist = (float)(Math.Sqrt((x - x0) * (x - x0) + (y - x0) * (y - x0)));

                    dx = (x - x0) * Speed / dist;
                    dy = (y - x0) * Speed / dist;
                    break;

                case 1:
                    {
                        float X = rand.Next(gl_Width);
                        float Y = rand.Next(gl_Width);

                        dist = (float)(Math.Sqrt((x - X) * (x - X) + (y - Y) * (y - Y)));

                        dx = (x - X) * Speed / dist;
                        dy = (y - Y) * Speed / dist;
                    }
                    break;
            }

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

            if (doUseStartDispersion)
            {
                x += (rand.Next(startDispersionRate) - rand.Next(startDispersionRate));
                y += (rand.Next(startDispersionRate) - rand.Next(startDispersionRate));
            }

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
            size2x = size * 2;

            switch (shape)
            {
                // Instanced squares
                case 0:
                    {
                        myPrimitive._RectangleInst.setInstanceCoords(x - size, y - size, size2x, size2x);
                        myPrimitive._RectangleInst.setInstanceColor(r, g, b, a);
                        myPrimitive._RectangleInst.setInstanceAngle(angle);

                        if (doDrawTwice)
                        {
                            size2x += 2;
                            myPrimitive._RectangleInst.setInstanceCoords(x - size - 1, y - size - 1, size2x, size2x);
                            myPrimitive._RectangleInst.setInstanceColor(r, g, b, a * 0.33f);
                            myPrimitive._RectangleInst.setInstanceAngle(angle);
                        }
                    }
                    break;

                // Instanced triangles
                case 1:
                    {
                        myPrimitive._TriangleInst.setInstanceCoords(x, y, size2x, angle);
                        myPrimitive._TriangleInst.setInstanceColor(r, g, b, a);

                        if (doDrawTwice)
                        {
                            myPrimitive._TriangleInst.setInstanceCoords(x, y, size2x + 2, angle);
                            myPrimitive._TriangleInst.setInstanceColor(r, g, b, a * 0.33f);
                        }
                    }
                    break;

                // Instanced circles
                case 2:
                    {
                        myPrimitive._EllipseInst.setInstanceCoords(x, y, size2x, angle);
                        myPrimitive._EllipseInst.setInstanceColor(r, g, b, a);

                        if (doDrawTwice)
                        {
                            myPrimitive._EllipseInst.setInstanceCoords(x, y, size2x + 2, angle);
                            myPrimitive._EllipseInst.setInstanceColor(r, g, b, a * 0.33);
                        }
                    }
                    break;

                // Instanced pentagons
                case 3:
                    {
                        myPrimitive._PentagonInst.setInstanceCoords(x, y, size2x, angle);
                        myPrimitive._PentagonInst.setInstanceColor(r, g, b, a);

                        if (doDrawTwice)
                        {
                            myPrimitive._PentagonInst.setInstanceCoords(x, y, size2x + 2, angle);
                            myPrimitive._PentagonInst.setInstanceColor(r, g, b, a * 0.33f);
                        }
                    }
                    break;

                // Instanced hexagons
                case 4:
                    {
                        myPrimitive._HexagonInst.setInstanceCoords(x, y, size2x, angle);
                        myPrimitive._HexagonInst.setInstanceColor(r, g, b, a);

                        if (doDrawTwice)
                        {
                            myPrimitive._HexagonInst.setInstanceCoords(x, y, size2x + 2, angle);
                            myPrimitive._HexagonInst.setInstanceColor(r, g, b, a * 0.33f);
                        }
                    }
                    break;
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Process(Window window)
        {
            uint cnt = 0;
            int i = 0;
            int rate = rateBase;
            int waveSize = doUseIncreasingWaveSize ? 1 : waveSizeBase;
            int dWaveSize = doUseIncreasingWaveSize ? rand.Next(17) + 1 : 0;

            float xold = x0;
            float yold = y0;

            initShapes();

            while (list.Count < N)
            {
                list.Add(new myObj_0180());
                deadCnt++;
            }

            if (doClearBuffer)
            {
                glDrawBuffer(GL_FRONT_AND_BACK | GL_DEPTH_BUFFER_BIT);
                glClearColor(0, 0, 0, 1);
            }
            else
            {
                glDrawBuffer(GL_BACK);
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
                    grad.Draw();
                }
                else
                {
                    // Dim the screen constantly;
                    //dimScreen(dimAlpha);
                    grad.Draw();
                }

                // Render Frame
                {
                    if (doShiftCenter && cnt % 100 == 0)
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

                        if (doRandomizeCenter)
                        {
                            x0 = gl_x0 + rand.Next(centerRandSize) - rand.Next(centerRandSize/2);
                            y0 = gl_y0 + rand.Next(centerRandSize) - rand.Next(centerRandSize/2);
                        }

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

                    int Count = list.Count;

                    for (i = 0; i != Count; i++)
                    {
                        var obj = list[i] as myObj_0180;

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

                    if (doShowParticles)
                    {
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
                }

                t += 0.001f;

                System.Threading.Thread.Sleep(renderDelay);
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void initShapes()
        {
            //myPrimitive.init_ScrDimmer();

            if (connectionMode > 2)
                myPrimitive.init_LineInst(N);

            base.initShapes(shapeType, N * (doDrawTwice ? 2 : 1), rotationSubMode);

            grad = new myScreenGradient();
            grad.SetRandomColors(rand, 0.2f);
            grad.SetOpacity(doClearBuffer ? 1 : dimAlpha);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        // todo: test later how much slower is using this family business instead of direct 1-level switch
        private void addDxDyModifier(ref float dx, ref float dy, float x, float y, float dist)
        {
            // Use some test function
            if (doUseTestFunc)
            {
                if (testFuncNo < 0)
                {
                    testFuncNo = rand.Next(4);
                }

                useTestFunc(ref dx, ref dy, x, y, dist, testFuncNo);
                return;
            }

            switch (dXYgenerationMode)
            {
                // FUNC(arg): [y += Sin(x+y)]; [y = Cos(x/y)];
                case 0:
                    myFuncGenerator1.myExpr.expr(ref dx, ref dy, target, eqMode1, dXYgen_useRandSign1,
                        myFuncGenerator1.myFuncs.fFunc(f1, myFuncGenerator1.myArgs.argsFunc(argmode1, dx, dy)));
                    break;

                // FUNC(arg1) op (arg2): [y += Sin(x+y) * (x-y)]; [y = Cos(x/y) + (x+y)];
                case 1:
                    myFuncGenerator1.myExpr.expr(ref dx, ref dy, target, eqMode1, dXYgen_useRandSign1,
                        myFuncGenerator1.myArgs.argsFunc(argmode3,
                                myFuncGenerator1.myFuncs.fFunc(f1, myFuncGenerator1.myArgs.argsFunc(argmode1, dx, dy)),
                                myFuncGenerator1.myArgs.argsFunc(argmode2, dx, dy)
                    ));
                    break;

                // FUNC1(arg1) op FUNC2(arg2): [y = Sin(x+y) * Cos(x-y)]
                case 2:
                    myFuncGenerator1.myExpr.expr(ref dx, ref dy, target, eqMode1, dXYgen_useRandSign1,

                        myFuncGenerator1.myArgs.argsFunc(argmode3,
                                myFuncGenerator1.myFuncs.fFunc(f1, myFuncGenerator1.myArgs.argsFunc(argmode1, dx, dy)),
                                myFuncGenerator1.myFuncs.fFunc(f2, myFuncGenerator1.myArgs.argsFunc(argmode2, dx, dy))
                    ));
                    break;

                // dx = FUNC1(arg1): dx = Sin(dx + dy);
                // dy = FUNC2(arg2): dy = Cos(dy / dx);
                case 3:
                    {
                        float arg1 = myFuncGenerator1.myArgs.argsFunc(argmode1, dx, dy);
                        float arg2 = myFuncGenerator1.myArgs.argsFunc(argmode2, dx, dy);

                        myFuncGenerator1.myExpr.expr(ref dx, eqMode1, dXYgen_useRandSign1, myFuncGenerator1.myFuncs.fFunc(f1, arg1));
                        myFuncGenerator1.myExpr.expr(ref dy, eqMode2, dXYgen_useRandSign2, myFuncGenerator1.myFuncs.fFunc(f2, arg2));
                    }
                    break;

                // dx = FUNC1(arg1) op (arg2): dx = Sin(dx + dy) / dx;
                // dy = FUNC2(arg3) op (arg4): dy = Cos(dy / dx) * dy;
                case 4:
                    {
                        float arg1 = myFuncGenerator1.myArgs.argsFunc(argmode1, dx, dy);
                        float arg2 = myFuncGenerator1.myArgs.argsFunc(argmode2, dx, dy);
                        float arg3 = myFuncGenerator1.myArgs.argsFunc(argmode3, dx, dy);
                        float arg4 = myFuncGenerator1.myArgs.argsFunc(argmode4, dx, dy);

                        myFuncGenerator1.myExpr.expr(ref dx, eqMode1, dXYgen_useRandSign1,
                            myFuncGenerator1.myArgs.argsFunc(argmode5, myFuncGenerator1.myFuncs.fFunc(f1, arg1), arg2));

                        myFuncGenerator1.myExpr.expr(ref dy, eqMode2, dXYgen_useRandSign2,
                            myFuncGenerator1.myArgs.argsFunc(argmode6, myFuncGenerator1.myFuncs.fFunc(f2, arg3), arg4));
                    }
                    break;

                // dx = FUNC1(arg1) op FUNC2(arg2): [dx = Sin(x+y) * Cos(x-y)]
                // dy = FUNC3(arg3) op FUNC4(arg4): [dy = Cos(x*y) * Sin(x/y)]
                case 5:
                    {
/*
                        dx = (float)(Math.Cos(dx) * Math.Sin(dx) * 2);
                        dy = (float)(Math.Cos(dy) * Math.Sin(dy) * 2);
                        break;

                        float ttt = dx;
                        dx = (float)Math.Cos(dy) * (float)Math.Sin(dy) * 2;
                        dy = (float)Math.Cos(ttt) * (float)Math.Sin(ttt) * 2;
                        break;

                        argmode1 = 0;
                        argmode2 = 0;
                        argmode3 = 2;
                        argmode4 = 2;
                        argmode5 = 7;
                        argmode6 = 7;

                        f1 = myFuncGenerator1.Funcs.COS;
                        f2 = myFuncGenerator1.Funcs.SIN;
                        f3 = myFuncGenerator1.Funcs.COS;
                        f4 = myFuncGenerator1.Funcs.SIN;

                        eqMode1 = myFuncGenerator1.eqModes.EQUALS;
                        eqMode2 = myFuncGenerator1.eqModes.EQUALS;

                        dXYgen_useRandSign1 = false;
                        dXYgen_useRandSign2 = false;
*/
                        float arg1 = myFuncGenerator1.myArgs.argsFunc(argmode1, dx, dy);
                        float arg2 = myFuncGenerator1.myArgs.argsFunc(argmode2, dx, dy);
                        float arg3 = myFuncGenerator1.myArgs.argsFunc(argmode3, dx, dy);
                        float arg4 = myFuncGenerator1.myArgs.argsFunc(argmode4, dx, dy);

                        myFuncGenerator1.myExpr.expr(ref dx, eqMode1, dXYgen_useRandSign1,
                            myFuncGenerator1.myArgs.argsFunc(argmode5,
                                myFuncGenerator1.myFuncs.fFunc(f1, arg1),
                                myFuncGenerator1.myFuncs.fFunc(f2, arg2)
                        ));

                        myFuncGenerator1.myExpr.expr(ref dy, eqMode2, dXYgen_useRandSign2,
                            myFuncGenerator1.myArgs.argsFunc(argmode6,
                                myFuncGenerator1.myFuncs.fFunc(f3, arg3),
                                myFuncGenerator1.myFuncs.fFunc(f4, arg4)
                        ));
                    }
                    break;

                // -------------------------------------------------------------

                case 999:
                    // INT
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

                    // =============================================================

                    // together
                    //dx = (dx > 0 ? 1 : -1) * (float)(Math.Sqrt(Math.Abs(dx)));
                    //dy = (dy > 0 ? 1 : -1) * (float)(Math.Sqrt(Math.Abs(dy)));

                    // it was working. see what it does -- probably it is covered by [case 2]
                    //myFuncGenerator0.myFunc1.func_002(ref dx, ref dy, target, targetOp, f1, argmode1, f2, argmode2, argmode3);

                    break;
            }

            if (doUseIntConversion)
            {
                if (target == myFuncGenerator1.Targets.FIRST)
                {
                    dx = (int)(dx * 10000) / 10000;
                }
                else
                {
                    dy = (int)(dy * 10000) / 10000;
                }
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        // Use test function
        private void useTestFunc(ref float dx, ref float dy, float x, float y, float dist, int n)
        {
            float olddx = dx;
            float olddy = dy;

            switch (n)
            {
                case 0:
                    dx = (float)Math.Sin(olddy);
                    dy = (float)Math.Sin(olddx);

                    for (int i = 0; i < 3; i++)
                    {
                        dx += (float)Math.Sin(dx);
                        dy += (float)Math.Sin(dy);
                    }
                    break;

                // looks like frozen droplets from the puddle!
                case 1:
                    dy /= (float)Math.Sqrt(y);
                    break;

                // looks cool with connectionMOde = 4
                case 2:
                    dx = (int)(Math.Sin(x) * 2);
                    dy = (int)(Math.Sin(y) * 2);
                    break;

                case 3:
                    dx += dx / (x);
                    dy += dy / (y);
                    break;
            }
        }

        // ---------------------------------------------------------------------------------------------------------------

        // Set predefined mode
        private void setPredefinedMode()
        {
            doUsePredefinedMode = true;

#if false
                //dx = (float)Math.Cos(dx);
                //dy = (float)Math.Sin(dy);

                //dx = (float)(Math.Cos(dx))/dx;
                //dy = (float)(Math.Sin(dy))/dy;
                //return;

                float dx1 = dx;
                float dx2 = dx;
                float dy1 = dy;
                float dy2 = dy;

                dx += (float)Math.Cos(1.0f / (dx1 + dy1));
                dy /= (float)Math.Cos((dx2 * dy2) / (dx2 + dy2));

                dx *= myUtils.randomSign(rand);
                dy *= myUtils.randomSign(rand);
                return;
#endif
            int[] arr00 = { 0, 0, 4, 1, 0, 3, 9, 0 };
            int[] arr01 = { 0, 1, 2, 2, 2, 1, 0, 0 };           // heart shaped box
            int[] arr16 = { 0, 1, 4, 0, 0, 1, 0, 0 };           // dy /= (float)Math.Sin(Math.Abs(dx));
            int[] arr02 = { 0, 1, 4, 2, 2, 1, 0, 0 };           // dy /= (float)Math.Sqrt(Math.Abs(dx));
            int[] arr13 = { 0, 1, 0, 0, 0, 9, 0, 0 };           // dy  = (float)Math.Sin(dy/dx);
            int[] arr14 = { 0, 1, 2, 0, 0, 6, 0, 0 };           // dy -= (float)Math.Sin(dy-dx);
            int[] arr15 = { 0, 1, 2, 0, 0, 6, 0, 0 };           // dy  = (float)Math.Sqrt(dx);

            int[] arr03 = { 1, 1, 0, 1, 0, 0, 2, 7 };           // dy = (float)Math.Cos(dx) * dy;
            int[] arr04 = { 1, 1, 0, 1, 0, 4, 4, 7 };           // dy = (float)Math.Cos(dx + dy) * (dy + dx);
            int[] arr05 = { 1, 1, 0, 1, 0, 7, 7, 7 };           // dy = (float)Math.Cos(dx * dy) * (dy * dx);
            int[] arr06 = { 1, 1, 0, 1, 0, 7, 9, 7 };           // dy = (float)Math.Cos(dx * dy) * (dy / dx);
            int[] arr07 = { 1, 1, 0, 1, 0, 4, 9, 7 };           // dy = (float)Math.Cos(dx + dy) * (dy / dx);
            int[] arr08 = { 1, 1, 0, 1, 0, 5, 9, 7 };           // dy = (float)Math.Cos(dx - dy) * (dy / dx);
            int[] arr09 = { 1, 1, 0, 1, 0, 8, 9, 7 };           // dy = (float)Math.Cos(dx / dy) * (dy / dx);
            int[] arr10 = { 1, 1, 0, 0, 0, 0, 2, 8 };           // dy = (float)Math.Sin(dx) / dy;
            int[] arr11 = { 1, 0, 0, 1, 0, 2, 0, 8 };           // dx = (float)Math.Cos(dy) / dx;
            int[] arr12 = { 1, 0, 0, 0, 0, 2, 0, 8 };           // dx = (float)Math.Sin(dy) / dx;

            int[] arr20 = { 2, 1, 0, 0, 0,  0, 2, 4 };          // dy  = (float)Math.Sin(dx) + (float)Math.Sin(dy);
            int[] arr21 = { 2, 0, 1, 0, 1, 23, 6, 3 };          // dx += (float)Math.Cos(Math.Abs(dy-dx));

            int[] arr30 = { 3, 0, 4, 0, 2,  6, 25,  0, 3 };     //
            int[] arr31 = { 3, 0, 3, 0, 0, 28, 22, 16, 3 };     // 

            int[] arr40 = { 4, 1, 2, 2, 0, 30, 2, 32, 4, 0, 0, 28, 31, 14 };

            var arr = arr40;

            dXYgenerationMode = arr[0];
            target  = (myFuncGenerator1.Targets)arr[1];
            eqMode1 = (myFuncGenerator1.eqModes)arr[2];
            f1 = (myFuncGenerator1.Funcs)arr[3];
            f2 = (myFuncGenerator1.Funcs)arr[4];
            argmode1 = arr[5];
            argmode2 = arr[6];
            argmode3 = arr[7];

            // More params
            if (dXYgenerationMode == 3)
            {
                eqMode2 = (myFuncGenerator1.eqModes)arr[8];
            }

            if (dXYgenerationMode == 4)
            {
                eqMode2 = (myFuncGenerator1.eqModes)arr[8];
                f3 = (myFuncGenerator1.Funcs)arr[9];
                f4 = (myFuncGenerator1.Funcs)arr[10];
                argmode4 = arr[11];
                argmode5 = arr[12];
                argmode6 = arr[13];
            }
        }
    }
};
