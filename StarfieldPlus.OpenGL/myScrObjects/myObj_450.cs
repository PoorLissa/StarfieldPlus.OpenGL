using GLFW;
using static OpenGL.GL;
using System.Collections.Generic;


/*
    - Get color from image and slightly offset this color;
    - Then put color spots on the screen at the same coordinates

    - As an option, set color as (1 - Color)
    - As an option, set 1 or 2 colors as averages
*/


namespace my
{
    public class myObj_450 : myObject
    {
        // Priority
        public static int Priority => 10;
		public static System.Type Type => typeof(myObj_450);

        private float x, y, w, h;
        private float A, R, G, B, angle = 0;

        private static int N = 0, shape = 0, maxSize = 1, opacityMode = 0, angleMode = 0, offsetMode = 0, mode = 0;
        private static bool doFillShapes = false;
        private static float dimAlpha = 0.05f, maxOffset = 1, lineWidth = 1.0f;

        private static float rAvg = 0, gAvg = 0, bAvg = 0;
        private static int avgCnt = 0, mode2mode = 0;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_450()
        {
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
                doClearBuffer = false;
                doFillShapes = true;

                N = rand.Next(10) + 10;
                shape = rand.Next(6);

                dimAlpha = 0.001f;
            }

            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time local initialization
        private void initLocal()
        {
            // Max color offset
            switch (rand.Next(3))
            {
                case 0:
                    maxOffset = myUtils.randFloat(rand) * 0.05f;
                    break;

                case 1:
                    maxOffset = myUtils.randFloat(rand) * 0.10f;
                    break;

                case 2:
                    maxOffset = myUtils.randFloat(rand) * 0.20f;
                    break;

                case 3:
                    maxOffset = myUtils.randFloat(rand) * 0.30f;
                    break;
            }

            mode = 0;

            switch (rand.Next(8))
            {
                case 0: mode = 1; break;
                case 1: mode = 2; break;
                case 2: mode = 2; break;
            }

            offsetMode = rand.Next(13);
            maxSize = rand.Next(35) + 15;
            opacityMode = rand.Next(9);
            angleMode = rand.Next(4);
            mode2mode = rand.Next(6);

            renderDelay = rand.Next(11) + 3;

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            height = 600;

            string nStr(int   n) { return n.ToString("N0");    }
            string fStr(float f) { return f.ToString("0.000"); }

            string str = $"Obj = myObj_450\n\n"                      +
                            $"N = {nStr(list.Count)} of {nStr(N)}\n" +
                            $"mode = {mode}\n"                       +
                            $"mode2mode = {mode2mode}\n"             +
                            $"shape = {shape}\n"                     +
                            $"maxSize = {maxSize}\n"                 +
                            $"offsetMode = {offsetMode}\n"           +
                            $"opacityMode = {opacityMode}\n"         +
                            $"angleMode = {angleMode}\n"             +
                            $"dimAlpha = {fStr(dimAlpha)}\n"         +
                            $"maxOffset = {fStr(maxOffset)}\n"       +
                            $"lineWidth = {fStr(lineWidth)}\n"       +
                            $"renderDelay = {renderDelay}\n"         +
                            $"file: {colorPicker.GetFileName()}"
                ;
            return str;
        }

        // ---------------------------------------------------------------------------------------------------------------

        // Press 'Space' to change mode
        protected override void setNextMode()
        {
            initLocal();

            setLineWidth();
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void generateNew()
        {
            x = rand.Next(gl_Width);
            y = rand.Next(gl_Height);

            w = rand.Next(maxSize) + 3;
            h = rand.Next(maxSize) + 3;

            colorPicker.getColor(x, y, ref R, ref G, ref B);

            // Set opacity
            switch (opacityMode)
            {
                case 0:
                case 1:
                    A = myUtils.randFloat(rand);
                    break;

                case 2:
                case 3:
                    A = myUtils.randFloat(rand) * 0.5f;
                    break;

                case 4:
                case 5:
                    A = myUtils.randFloat(rand) * 0.25f;
                    break;

                case 6:
                    A = 0.5f;
                    break;

                case 7:
                    A = 0.85f;
                    break;

                case 8:
                    A = 0.95f;
                    break;
            }

            // Offset colors
            if (mode == 0)
            {
                // Offset picked color
                switch (offsetMode < 7 ? offsetMode : rand.Next(7))
                {
                    case 0:
                        R += myUtils.randomSign(rand) * myUtils.randFloat(rand) * maxOffset;
                        break;

                    case 1:
                        G += myUtils.randomSign(rand) * myUtils.randFloat(rand) * maxOffset;
                        break;

                    case 2:
                        B += myUtils.randomSign(rand) * myUtils.randFloat(rand) * maxOffset;
                        break;

                    case 3:
                        R += myUtils.randomSign(rand) * myUtils.randFloat(rand) * maxOffset;
                        G += myUtils.randomSign(rand) * myUtils.randFloat(rand) * maxOffset;
                        break;

                    case 4:
                        R += myUtils.randomSign(rand) * myUtils.randFloat(rand) * maxOffset;
                        B += myUtils.randomSign(rand) * myUtils.randFloat(rand) * maxOffset;
                        break;

                    case 5:
                        G += myUtils.randomSign(rand) * myUtils.randFloat(rand) * maxOffset;
                        B += myUtils.randomSign(rand) * myUtils.randFloat(rand) * maxOffset;
                        break;

                    case 6:
                        R += myUtils.randomSign(rand) * myUtils.randFloat(rand) * maxOffset;
                        G += myUtils.randomSign(rand) * myUtils.randFloat(rand) * maxOffset;
                        B += myUtils.randomSign(rand) * myUtils.randFloat(rand) * maxOffset;
                        break;
                }
            }

            // Inverted colors
            if (mode == 1)
            {
                R = 1 - R;
                G = 1 - G;
                B = 1 - B;
            }

            // 1 or 2 colors stay the same, the other color components are averaged
            if (mode == 2)
            {
                if (avgCnt < 10000)
                {
                    rAvg += R;
                    gAvg += G;
                    bAvg += B;
                    avgCnt++;
                }

                switch (mode2mode)
                {
                    case 0:
                        R = rAvg / avgCnt;
                        break;

                    case 1:
                        G = gAvg / avgCnt;
                        break;

                    case 2:
                        B = bAvg / avgCnt;
                        break;

                    case 3:
                        G = gAvg / avgCnt;
                        B = bAvg / avgCnt;
                        break;

                    case 4:
                        R = rAvg / avgCnt;
                        B = bAvg / avgCnt;
                        break;

                    case 5:
                        R = rAvg / avgCnt;
                        G = gAvg / avgCnt;
                        break;
                }
            }

            // Set angle
            switch (angleMode)
            {
                case 0:
                case 1:
                    angle = 0;
                    break;

                case 2:
                    angle = myUtils.randomSign(rand) * myUtils.randFloat(rand) * 0.05f;
                    break;

                case 3:
                    angle = myUtils.randFloat(rand);
                    break;
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            generateNew();

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Show()
        {
            float size2x = w * 2;

            switch (shape)
            {
                // Instanced squares
                case 0:
                    var rectInst = inst as myRectangleInst;

                    rectInst.setInstanceCoords(x - w, y - w, size2x, size2x);
                    rectInst.setInstanceColor(R, G, B, A);
                    rectInst.setInstanceAngle(angle);
                    break;

                // Instanced triangles
                case 1:
                    var triangleInst = inst as myTriangleInst;

                    triangleInst.setInstanceCoords(x, y, size2x, angle);
                    triangleInst.setInstanceColor(R, G, B, A);
                    break;

                // Instanced circles
                case 2:
                    var ellipseInst = inst as myEllipseInst;

                    ellipseInst.setInstanceCoords(x, y, size2x, angle);
                    ellipseInst.setInstanceColor(R, G, B, A);
                    break;

                // Instanced pentagons
                case 3:
                    var pentagonInst = inst as myPentagonInst;

                    pentagonInst.setInstanceCoords(x, y, size2x, angle);
                    pentagonInst.setInstanceColor(R, G, B, A);
                    break;

                // Instanced hexagons
                case 4:
                    var hexagonInst = inst as myHexagonInst;

                    hexagonInst.setInstanceCoords(x, y, size2x, angle);
                    hexagonInst.setInstanceColor(R, G, B, A);
                    break;

                // Instanced rectangles
                case 5:
                    var rectInst2 = inst as myRectangleInst;

                    rectInst2.setInstanceCoords(x - w, y - h, 2 * w, 2 * h);
                    rectInst2.setInstanceColor(R, G, B, A);
                    rectInst2.setInstanceAngle(angle);
                    break;
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Process(Window window)
        {
            uint cnt = 0;
            initShapes();

            // Disable VSYNC if needed
            // Glfw.SwapInterval(0);

            if (doClearBuffer)
            {
                glDrawBuffer(GL_FRONT_AND_BACK | GL_DEPTH_BUFFER_BIT);
                glClearColor(0, 0, 0, 1);
            }
            else
            {
                dimScreenRGB_SetRandom(0.1f);
                glDrawBuffer(GL_FRONT_AND_BACK);
            }

            setLineWidth();

            while (!Glfw.WindowShouldClose(window))
            {
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
                    }
                }

                // Render Frame
                {
                    inst.ResetBuffer();

                    int Count = list.Count;

                    for (int i = 0; i != Count; i++)
                    {
                        var obj = list[i] as myObj_450;

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

                if (list.Count < N)
                {
                    list.Add(new myObj_450());
                }

                cnt++;
                System.Threading.Thread.Sleep(renderDelay);
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void initShapes()
        {
            myPrimitive.init_ScrDimmer();

            base.initShapes(shape < 5 ? shape : 0, N, 0);

            if (shape == 0 || shape == 5)
            {
                // Change pixel density, so the rectangle's outline is displayed correctly
                inst.setPixelDensityOffset(1);
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        // Randomly change shape's border width
        private void setLineWidth()
        {
            lineWidth = 0.5f + myUtils.randFloat(rand) * rand.Next(4);

            glLineWidth(lineWidth);
        }

        // ---------------------------------------------------------------------------------------------------------------

    }
};
