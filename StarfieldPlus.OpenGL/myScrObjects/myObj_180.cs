using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;


/*
    - moving points generator
*/


namespace my
{
    public class myObj_180 : myObject
    {
        private delegate float family1Delegate(float f1, float f2);

        // ---------------------------------------------------------------------------------------------------------------

        private static bool doClearBuffer = false, doFillShapes = false, doUseDispersion = false, doUseXOffset = false, doUseRandomSpeed = false,
                            doUseIncreasingWaveSize = false, doShiftCenter = false, dXYgenerationMode_useRandSign = false;
        private static int x0, y0, N = 1, deadCnt = 0, waveSizeBase = 1111, WaveLifeCnt = 0, LifeCntBase = 0;
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

        private static family1Delegate f1dFunc = null;

        // Function Generation
        static myTest.Targets target;
        static myTest.BasicFunctions f1, f2;
        static uint argmode1, argmode2, argmode3;
        static myTest.Operations targetOp;

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
            x0 = gl_Width  / 2;
            y0 = gl_Height / 2;

            colorPicker = new myColorPicker(gl_Width, gl_Height);
            
            list = new List<myObject>();

            doClearBuffer   = myUtils.randomChance(rand, 4, 5);
            doFillShapes    = myUtils.randomBool(rand);
            doUseDispersion = myUtils.randomChance(rand, 2, 3);
            doUseXOffset    = myUtils.randomChance(rand, 1, 7);
            doShiftCenter   = myUtils.randomChance(rand, 1, 6);
            doUseRandomSpeed = myUtils.randomBool(rand);
            doUseIncreasingWaveSize = myUtils.randomChance(rand, 1, 3);
            rateMode = rand.Next(3);
            dispersionMode  = rand.Next(6);
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
            rotationMode   = rand.Next(4);
            shapeType      = rand.Next(5);
            LifeCntBase    = rand.Next(333) + 111;
            connectionMode = rand.Next(5);
            speedBase      = 1.0f + 0.001f * rand.Next(2000);
            dSize          = (float)rand.NextDouble()/100;

            // In case the rotation is enabled, we also may enable additional rotation option:
            if (rotationMode < 2)
            {
                rotationSubMode = rand.Next(7);
                rotationSubMode = rotationSubMode > 2 ? 0 : rotationSubMode + 1;     // [0, 1, 2] --> [1, 2, 3]; otherwise set to '0';
            }

            // Set up additional dx/dy generation mode:
            if (myUtils.randomChance(rand, 1, 3))
            {
                dXYgenerationMode = rand.Next(20);
                dXYgenerationMode_useRandSign = myUtils.randomBool(rand);

                delegateNo = 999;
                delegateNo = rand.Next(30);

                switch (delegateNo)
                {
                    case 0:
                        f1dFunc = new family1Delegate((float x, float y) => { return x; });
                        break;

                    case 1:
                        f1dFunc = new family1Delegate((float x, float y) => { return y; });
                        break;

                    case 2:
                        f1dFunc = new family1Delegate((float x, float y) => { return x + y; });
                        break;

                    case 3:
                        f1dFunc = new family1Delegate((float x, float y) => { return x * y; });
                        break;

                    case 4:
                        f1dFunc = new family1Delegate((float x, float y) => { return x / y; });
                        break;

                    case 5:
                        f1dFunc = new family1Delegate((float x, float y) => { return (x + y) / (x * y); });
                        break;

                    case 6:
                        f1dFunc = new family1Delegate((float x, float y) => { return (x * y) / (x + y); });
                        break;

                    case 7:
                        f1dFunc = new family1Delegate((float x, float y) => { return (x + y) * (x - y); });
                        break;

                    case 8:
                        f1dFunc = new family1Delegate((float x, float y) => { return x * x * y * y; });
                        break;

                    case 9:
                        f1dFunc = new family1Delegate((float x, float y) => { return 1 / (x + y); });
                        break;

                    case 10:
                        f1dFunc = new family1Delegate((float x, float y) => { return 1 / (x - y); });
                        break;

                    case 11:
                        f1dFunc = new family1Delegate((float x, float y) => { return y / (x + y); });
                        break;

                    case 12:
                        f1dFunc = new family1Delegate((float x, float y) => { return y / (x - y); });
                        break;

                    case 13:
                        f1dFunc = new family1Delegate((float x, float y) => { return x / y + y / x; });
                        break;

                    case 14:
                        f1dFunc = new family1Delegate((float x, float y) => { return x > y ? x / y : y / x; });
                        break;

                    case 15:
                        f1dFunc = new family1Delegate((float x, float y) => { return (x + y) * t; });
                        break;

                    case 16:
                        f1dFunc = new family1Delegate((float x, float y) => { return (x * y) > (x / y) ? x : y; });
                        break;

                    case 17:
                        f1dFunc = new family1Delegate((float x, float y) => { return (x * y) > (x / y) ? y : x; });
                        break;

                    case 18:
                        f1dFunc = new family1Delegate((float x, float y) => { return (x * y * x) * (y * x * y); });
                        break;

                    case 19:
                        f1dFunc = new family1Delegate((float x, float y) => { return (x * x) > (y * y) ? y/x : x/y; });
                        break;

                    case 20:
                        f1dFunc = new family1Delegate((float x, float y) => { return (x * x) > (y * y) ? x/y : y/x; });
                        break;

                    case 21:
                        f1dFunc = new family1Delegate((float x, float y) => { return (x * x) > (y * y) ? x : y; });
                        break;

                    case 22:
                        f1dFunc = new family1Delegate((float x, float y) => { return (x * x) > (y * y) ? y : x; });
                        break;

                    case 23:
                        f1dFunc = new family1Delegate((float x, float y) => { return (x * x) > 0.5f ? 1 : -1; });
                        break;

                    case 24:
                        f1dFunc = new family1Delegate((float x, float y) => { return (x * x) > 0.5f ? x+y : x-y; });
                        break;

                    case 25:
                        f1dFunc = new family1Delegate((float x, float y) => { return (x * y) > 0.5f ? (float)Math.Sin(x) : (float)Math.Sin(y); });
                        break;

                    case 26:
                        f1dFunc = new family1Delegate((float x, float y) => { return (x * y) > 1 ? (float)Math.Sin(x) : (float)Math.Sin(y); });
                        break;

                    case 27:
                        f1dFunc = new family1Delegate((float x, float y) => { return (x + y) > 1 ? (float)Math.Sin(x) : (float)Math.Cos(y); });
                        break;

                    case 28:
                        f1dFunc = new family1Delegate((float x, float y) => { return (float)Math.Sin(x + y); });
                        break;

                    case 29:
                        f1dFunc = new family1Delegate((float x, float y) => { return (float)Math.Sin(1 / (x + y)); });
                        break;

                    case 999:
                        f1dFunc = new family1Delegate((float x, float y) => 
                        {
                            return (x - 1)*(x + 1) - (y - 1) * (y + 1);
                        });
                        break;
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
            dXYgenerationMode = rand.Next(20);
            dXYgenerationMode = 999;
#endif

            if (dXYgenerationMode == 999)
            {
                target = (myTest.Targets)rand.Next(2);
                f1 = (myTest.BasicFunctions)(rand.Next(3) + 1);
                f2 = (myTest.BasicFunctions)(rand.Next(3) + 1);
                argmode1 = (uint)rand.Next(10);
                argmode2 = (uint)rand.Next(10);
                argmode3 = (uint)rand.Next(10);
                targetOp = (myTest.Operations)rand.Next(5);
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo()
        {
            string getFuncGeneratorParams()
            {
                return $"target = {target}\n" +
                       $"targetOp = {targetOp}\n" + 
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
                            $"dXYgenerationMode = {dXYgenerationMode}\n" +
                            $"dXYgenerationMode_useRandSign = {dXYgenerationMode_useRandSign}\n" +
                            $"delegateNo = {delegateNo}\n" +
                            $"heightRatioMode = {heightRatioMode}\n" +
                            $"doUseXOffset = {doUseXOffset}\n" +
                            $"doShiftCenter = {doShiftCenter}\n" +
                            $"LifeCntBase = {LifeCntBase}\n" +
                            "\nFuncGeneratorParams:\n\n"
                            + getFuncGeneratorParams()
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

        private enum Operations { EQUALS, PLUS, MINUS, MULT, DIV, SIN, COS };

        private void family1(ref float f1, Operations op1, ref float f2, Operations op2, bool randSign)
        {
            // f1 +-*/= Sin(f2);
            // f1 +-*/= Cos(f2);

            double res = 0;

            switch (op2)
            {
                case Operations.SIN: res = Math.Sin(f2); break;
                case Operations.COS: res = Math.Cos(f2); break;
            }

            switch (op1)
            {
                case Operations.EQUALS: f1  = (float)res; break;
                case Operations.PLUS:   f1 += (float)res; break;
                case Operations.MINUS:  f1 -= (float)res; break;
                case Operations.MULT:   f1 *= (float)res; break;
                case Operations.DIV:    f1 /= (float)res; break;
            }

            if (randSign)
            {
                f1 *= myUtils.randomSign(rand);
            }

            return;
        }

        // Must be used with a block of 20 switch cases;
        // startIndex is the index of the first case used;
        // Applies 4 different [family1] calls with 5 different operations;
        // Must receive a function delegate to perform an operation on the incoming dx and dy values;
        private void useFamily1(ref float dx, ref float dy, family1Delegate func, int startIndex, bool randSign)
        {
            float val = func(dx, dy);

            Operations op = Operations.EQUALS;

            switch ((dXYgenerationMode - startIndex) / 4)
            {
                case 1: op = Operations.PLUS;  break;
                case 2: op = Operations.MINUS; break;
                case 3: op = Operations.MULT;  break;
                case 4: op = Operations.DIV;   break;
            }

            switch (dXYgenerationMode % 4)
            {
                case 0:
                    family1(ref dy, op, ref val, Operations.SIN, randSign);
                    break;

                case 1:
                    family1(ref dy, op, ref val, Operations.COS, randSign);
                    break;

                case 2:
                    family1(ref dx, op, ref val, Operations.SIN, randSign);
                    break;

                case 3:
                    family1(ref dx, op, ref val, Operations.COS, randSign);
                    break;
            }
        }

        // todo: test later how much slower is using this family business instead of direct 1-level switch
        private void addDxDyModifier(ref float dx, ref float dy, float x, float y, float dist)
        {
            float tmp = 0;

            switch (dXYgenerationMode)
            {
                case 0: case 1: case 2: case 3:
                case 4: case 5: case 6: case 7:
                case 8: case 9: case 10: case 11:
                case 12: case 13: case 14: case 15:
                case 16: case 17: case 18: case 19:
                    useFamily1(ref dx, ref dy, f1dFunc, 0, randSign: dXYgenerationMode_useRandSign);
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

                    // heart shaped box
                    //dy -= (float)(Math.Sqrt(Math.Abs(dx)));

                    //dy /= (float)(Math.Sqrt(Math.Abs(dx)));

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

#if false
                    dy += (float)Math.Sin(dx) + (float)Math.Cos(dy);
#else

                    if (false)
                    {
                        myTest.myFunc1.func_002(ref dx, ref dy, target, targetOp, f1, argmode1, f2, argmode2, argmode3);
                    }

                    dy /= (float)Math.Sin(Math.Abs(dx));// + (float)Math.Sqrt(dy);

/*
target = FIRST
op = DIV
f1 = COS
f2 = SIN
argmode1 = 3
argmode2 = 9
argmode3 = 0
*/

/*
dy += (float)Math.Cos((Math.Abs(dy))) / (float)Math.Cos(dy*dx);* 
target = SECOND
targetOp = PLUS
f1 = COS
f2 = COS
argmode1 = 7
argmode2 = 3
argmode3 = 9
*/

/*
    -- save this one
target = SECOND
targetOp = DIV
f1 = SIN
f2 = SQRT
argmode1 = 1
argmode2 = 1
argmode3 = 0
*/

#endif
                    // =============================================================
                    break;
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------
    }
};


namespace myTest
{
    public enum Targets { FIRST, SECOND, THIRD };
    public enum Operations { EQUALS, PLUS, MINUS, MULT, DIV };
    public enum BasicFunctions { NONE, SIN, COS, SQRT };

    public class myFunc1
    {
        public static void func_001(ref float f1, ref float f2, Targets target, uint argMode, BasicFunctions func, Operations op)
        {
            float arg = nArgsFunc(argMode, f1, f2);

            if (target == Targets.FIRST)
            {
                applyBasicFunc(ref f1, arg, func, op);
            }
            else
            {
                applyBasicFunc(ref f2, arg, func, op);
            }
        }

        public static void func_002(ref float f1, ref float f2, Targets target, Operations targetOp,
                                        BasicFunctions func1, uint argMode1,
                                        BasicFunctions func2, uint argMode2,
                                        uint argMode3)
        {
            float res1 = f1, res2 = f1;

            func_001(ref res1, ref f2, Targets.FIRST, argMode1, func1, Operations.EQUALS);
            func_001(ref res2, ref f2, Targets.FIRST, argMode2, func2, Operations.EQUALS);

            float res = nArgsFunc(argMode3, res1, res2);

            if (target == Targets.FIRST)
            {
                applyBasicFunc(ref f1, res, BasicFunctions.NONE, targetOp);
            }
            else
            {
                applyBasicFunc(ref f2, res, BasicFunctions.NONE, targetOp);
            }
        }

        private static float nArgsFunc(uint mode, float f1, float f2 = 0, float f3 = 0, float f4 = 0, float f5 = 0)
        {
            switch (mode)
            {
                case 0: return f1;
                case 1: return Math.Abs(f1);
                case 2: return f2;
                case 3: return Math.Abs(f2);
                case 4: return f1 + f2;
                case 5: return f1 - f2;
                case 6: return f2 - f1;
                case 7: return f1 * f2;
                case 8: return f1 / f2;
                case 9: return f2 / f1;
            }

            return 0;
        }

        private static void applyBasicFunc(ref float f1, float f2, BasicFunctions func, Operations operation)
        {
            double res = 0;

            switch (func)
            {
                case BasicFunctions.SIN  : res = Math.Sin(f2);  break;
                case BasicFunctions.COS  : res = Math.Cos(f2);  break;
                case BasicFunctions.SQRT : res = Math.Sqrt(f2); break;
                                 default : res = f2;            break;
            }

            switch (operation)
            {
                case Operations.EQUALS  : f1  = (float)res; break;
                case Operations.PLUS    : f1 += (float)res; break;
                case Operations.MINUS   : f1 -= (float)res; break;
                case Operations.MULT    : f1 *= (float)res; break;
                case Operations.DIV     : f1 /= (float)res; break;
            }
        }
    };
};
