using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;


/*
    - Bouncing ball + lots of shapes rotating to always point towards it
*/


namespace my
{
    // This class is used as a bouncing ball
    public partial class myObj_440_Ball : myObj_440 { };

    public class myObj_440 : myObject
    {
        // Priority
        public static int Priority => 10;

        private int cnt;
        protected float x, y, dx, dy, tmpx, tmpy;
        protected float size, dSize, a, da, A, R, G, B, angle = 0;

        protected static int N = 0, shape = 0, ballMoveMode = 0, moveMode = 0, insertMode = 0;
        private static bool doFillShapes = false;
        private static float dimAlpha = 0.05f, lineTh = 1.0f;

        protected static float centerX = 0, centerY = 0;

        protected static double _1pi2 = 1.0 * Math.PI/2;
        protected static double _3pi2 = 3.0 * Math.PI/2;

        protected static myFreeShader shader = null;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_440()
        {
            generateNew();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time global initialization
        protected override void initGlobal()
        {
            colorPicker = new myColorPicker(gl_Width, gl_Height);
            list = new List<myObject>();

            // Global unmutable constants
            {
                N = 1 + rand.Next(9999) + rand.Next(9999) + rand.Next(6666) + rand.Next(3333) + 1234;

                switch (rand.Next(13))
                {
                    case 0:  shape = 0; break;      // Square
                    case 1:  shape = 3; break;      // Pentagon
                    case 2:  shape = 4; break;      // Hexagon
                    case 3:  shape = 5; break;      // Line
                    default: shape = 1; break;      // Triangle (default)
                }
            }

            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time local initialization
        private void initLocal()
        {
            doClearBuffer = myUtils.randomChance(rand, 1, 2);
            doFillShapes  = myUtils.randomChance(rand, 1, 2);

            moveMode = rand.Next(3);
            ballMoveMode = rand.Next(5);
            insertMode = rand.Next(2);
            renderDelay  = rand.Next(11) + 5;

            dimAlpha = 0.25f;

            lineTh = myUtils.randFloat(rand, 0.1f) * (rand.Next(5) + 1);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            height = 600;

            string nStr(int   n) { return n.ToString("N0");    }
            string fStr(float f) { return f.ToString("0.000"); }

            float ballSize = (list[0] as myObj_440_Ball).size;

            string str = $"Obj = myObj_440\n\n"                      +
                            $"N = {nStr(list.Count)} of {nStr(N)}\n" +
                            $"doClearBuffer = {doClearBuffer}\n"     +
                            $"shape = {shape}\n"                     +
                            $"moveMode = {moveMode}\n"               +
                            $"insertMode = {insertMode}\n"           +
                            $"ballMoveMode = {ballMoveMode}\n"       +
                            $"ball.size = {fStr(ballSize)}\n"        +
                            $"lineTh = {fStr(lineTh)}\n"             +
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
            cnt = 0;

            x = rand.Next(gl_Width);
            y = rand.Next(gl_Height);

            dx = myUtils.randomSign(rand) * myUtils.randFloat(rand);
            dy = myUtils.randomSign(rand) * myUtils.randFloat(rand);

            size = rand.Next(33) + 3;
            dSize = myUtils.randFloat(rand) + 0.0001f;

            A = myUtils.randFloat(rand, 0.33f);                     // Target opacity
            a = 0;                                                  // Current opacity
            da = myUtils.randFloat(rand, 0.01f) * 0.01f;            // Opacity increment

            colorPicker.getColor(x, y, ref R, ref G, ref B);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            if (cnt == 0)
            {
                if (a < A)
                    a += da;

                tmpx = x - centerX;
                tmpy = y - centerY;

                if (tmpx > 0)
                {
                    angle = (float)(_1pi2 - Math.Atan(tmpy / tmpx));
                }
                else
                {
                    angle = (float)(_3pi2 - Math.Atan(tmpy / tmpx));
                }

                float distSquared = tmpx * tmpx + tmpy * tmpy;

                // Depending on cnt, we'll calculate the angle more or less frequently
                if (distSquared < 110000)
                {
                    cnt = 0;
                }
                else if (distSquared < 202500)
                {
                    cnt = rand.Next(2);
                }
                else
                {
                    cnt = rand.Next(3);
                }

                // Define different behaviour modes
                switch (moveMode)
                {
                    // Static
                    case 0:
                        break;

                    case 1:
                        if ((size += dSize) > 123)
                        {
                            a -= 3 * da;

                            if (a <= 0)
                            {
                                generateNew();
                            }
                        }
                        break;

                    case 2:
                        x += dx;
                        y += dy;

                        if (x < -size || x > gl_Width + size || y < -size || y > gl_Height + size)
                        {
                            generateNew();
                        }
                        break;
                }
            }
            else
            {
                cnt--;
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Show()
        {
            float size2x = size * 2;

            switch (shape)
            {
                // Instanced squares
                case 0:
                    var rectInst = inst as myRectangleInst;

                    rectInst.setInstanceCoords(x - size, y - size, size2x, size2x);
                    rectInst.setInstanceColor(R, G, B, a);
                    rectInst.setInstanceAngle(angle);
                    break;

                // Instanced triangles
                case 1:
                    var triangleInst = inst as myTriangleInst;

                    triangleInst.setInstanceCoords(x, y, size2x, angle);
                    triangleInst.setInstanceColor(R, G, B, a);
                    break;

                // Instanced circles
                case 2:
                    var ellipseInst = inst as myEllipseInst;

                    ellipseInst.setInstanceCoords(x, y, size2x, angle);
                    ellipseInst.setInstanceColor(R, G, B, a);
                    break;

                // Instanced pentagons
                case 3:
                    var pentagonInst = inst as myPentagonInst;

                    pentagonInst.setInstanceCoords(x, y, size2x, angle);
                    pentagonInst.setInstanceColor(R, G, B, a);
                    break;

                // Instanced hexagons
                case 4:
                    var hexagonInst = inst as myHexagonInst;

                    hexagonInst.setInstanceCoords(x, y, size2x, angle);
                    hexagonInst.setInstanceColor(R, G, B, a);
                    break;

                case 5:
                    myPrimitive._Line.SetColor(R, G, B, a);
                    myPrimitive._Line.SetAngle((float)Math.PI - angle);
                    myPrimitive._Line.Draw(x, y-2*size, x, y+2*size, lineTh);
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

            // Add moving ball
            list.Add(new myObj_440_Ball());

#if false
            while (list.Count < N)
                list.Add(new myObj_440());
#endif

            glLineWidth(lineTh);

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
                    if (shape != 5)
                        inst.ResetBuffer();

                    int Count = list.Count;

                    for (int i = 0; i != Count; i++)
                    {
                        var obj = list[i] as myObj_440;

                        obj.Show();
                        obj.Move();
                    }

                    if (shape != 5)
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

                if (insertMode == 0)
                {
                    if (list.Count < N)
                        list.Add(new myObj_440());
                }
                else
                {
                    if (list.Count < N && cnt % 100 == 0)
                        list.Add(new myObj_440());
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
            myPrimitive.init_Line();

            if (shape != 5)
            {
                base.initShapes(shape, N, 0);
            }

            getShader();

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void getShader()
        {
            string header = "";
            string main = "";

            my.myShaderHelpers.Shapes.getShader_000(ref rand, ref header, ref main);
            shader = new myFreeShader(header, main);
        }

        // ---------------------------------------------------------------------------------------------------------------
    }


    // =============================================================================================================================
    // =============================================================================================================================


    public partial class myObj_440_Ball : myObj_440
    {
        private int radx, rady, n;
        private float t, dt;
        private float[] arr = null;

        public myObj_440_Ball()
        {
            // Set up the average moving mode
            {
                n = rand.Next(2) + 2;
                arr = new float[n * 4];

                for (int i = 0; i < n * 4; i += 4)
                {
                    arr[i + 0] = rand.Next(gl_Width);                                   // x
                    arr[i + 1] = rand.Next(gl_Height);                                  // y
                    arr[i + 2] = myUtils.randFloat(rand, 0.1f) * (rand.Next(5) + 5);    // dx
                    arr[i + 3] = myUtils.randFloat(rand, 0.1f) * (rand.Next(5) + 5);    // dy
                }
            }
        }

        protected override void generateNew()
        {
            t = myUtils.randFloat(rand);
            dt = (0.005f + myUtils.randFloat(rand) * 0.025f) * myUtils.randomSign(rand);
            radx = 666 + rand.Next(1234);
            rady = 666 + rand.Next(1234);

            x = rand.Next(gl_Width);
            y = rand.Next(gl_Height);

            dx = myUtils.randomSign(rand) * myUtils.randFloat(rand, 0.25f) * (rand.Next(15) + 3);
            dy = myUtils.randomSign(rand) * myUtils.randFloat(rand, 0.25f) * (rand.Next(15) + 3);

            if (ballMoveMode == 4)
            {
                ballSpecialMove();
            }

            switch (rand.Next(4))
            {
                case 0: size = rand.Next(003) + 3; break;
                case 1: size = rand.Next(033) + 3; break;
                case 2: size = rand.Next(111) + 3; break;
                case 3: size = rand.Next(333) + 3; break;
            }

            a = 0.5f;
            colorPicker.getColor(x, y, ref R, ref G, ref B);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            switch (ballMoveMode)
            {
                // Bouncing off the walls (soft)
                case 0:
                    {
                        x += dx;
                        y += dy;

                        if (x < 0)
                            dx += 0.1f;

                        if (y < 0)
                            dy += 0.1f;

                        if (x > gl_Width)
                            dx -= 0.1f;

                        if (y > gl_Height)
                            dy -= 0.1f;
                    }
                    break;

                // Bouncing off the walls (hard)
                case 1:
                    {
                        x += dx;
                        y += dy;

                        if (x < 0 - size || x > gl_Width - size)
                            dx *= -1;

                        if (y < 0 - size || y > gl_Height - size)
                            dy *= -1;
                    }
                    break;

                // Elliptic 1
                case 2:
                case 3:
                    {
                        x = gl_x0 + (float)Math.Sin(t) * radx;
                        y = gl_y0 + (float)Math.Cos(t) * rady;
                        t += (ballMoveMode == 2) ? dt : -dt;

                        radx += (int)(3 * Math.Sin(2*t));
                        rady += (int)(3 * Math.Cos(3*t));
                    }
                    break;

                // Average motion mode
                case 4:
                    ballSpecialMove();
                    break;
            }

            centerX = x;
            centerY = y;

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Show()
        {
            shader.SetColor(R, G, B, a);
            shader.Draw(x, y, size, size, 25);
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void ballSpecialMove()
        {
            float X = 0;
            float Y = 0;

            for (int i = 0; i < n; i++)
            {
                int  ix = i * 4 + 0;
                int  iy = i * 4 + 1;
                int idx = i * 4 + 2;
                int idy = i * 4 + 3;

                arr[ix] += arr[idx];
                arr[iy] += arr[idy];

                X += arr[ix];
                Y += arr[iy];

                // x < 0
                if (arr[ix] < 0)
                    arr[idx] += 0.1f;

                // x > gl_Width
                if (arr[ix] > gl_Width)
                    arr[idx] -= 0.1f;

                // y < 0
                if (arr[iy] < 0)
                    arr[idy] += 0.1f;

                // y > gl_Height
                if (arr[iy] > gl_Height)
                    arr[idy] -= 0.1f;
            }

            // Get average
            x = X / n;
            y = Y / n;
        }

        // ---------------------------------------------------------------------------------------------------------------
    };
};
