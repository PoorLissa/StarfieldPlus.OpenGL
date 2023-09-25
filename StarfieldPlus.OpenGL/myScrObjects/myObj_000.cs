using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;


/*
    - Starfield
*/


namespace my
{
    public class myObj_000 : myObject
    {
        // Priority
        public static int Priority => 10;
		public static System.Type Type => typeof(myObj_000);

        protected float size, dSize, A, R, G, B, angle, dAngle;
        protected float x, y, dx, dy, acceleration = 1.0f;
        protected int cnt = 0, max = 0;

        protected static int drawMode = 0, colorMode = 0, angleMode = 0, accelerationMode = 0;
        protected static int N = 0, staticStarsN = 0, cometsN = 0, lightsN = 0, shape = 0;
        protected static bool doFillShapes = true, doCreateAllAtOnce = true, doConnectStatics = true;
        protected static float connectOpacity = 0;

        protected static myHexagonInst staticStarBgr = null;

        static string ssstmp = "";

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_000()
        {
            generateNew();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time global initialization
        protected override void initGlobal()
        {
            colorPicker = new myColorPicker(gl_Width, gl_Height);
            list = new List<myObject>();

            // immutable constants
            {
                doClearBuffer = true;

                shape = rand.Next(5);

                N = rand.Next(333) + 100;                       // Fast moving stars
                cometsN = 3;                                    // Comets
                lightsN = 3;                                    // Vague roaming lights
                staticStarsN = rand.Next(333) + 333;            // Very slow moving stars which make up constellations
                N += staticStarsN;
                N += cometsN;
                N += lightsN;
            }

            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time local initialization
        private void initLocal()
        {
            doFillShapes      = myUtils.randomChance(rand, 1, 2);
            doCreateAllAtOnce = myUtils.randomChance(rand, 1, 9);
            doConnectStatics  = myUtils.randomChance(rand, 1, 2);

            colorMode = rand.Next(4);
            accelerationMode = rand.Next(2);

            connectOpacity = 0.03f + myUtils.randFloat(rand, 0.1f) * 0.05f;

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            height = 700;

            string str = $"Obj = myObj_000 (Starfield)\n\n"            +
                            $"Total N = {list.Count} of ({N})\n"       +
                            $"moving stars N = {N - staticStarsN}\n"   +
                            $"static stars N = {staticStarsN}\n"       +
                            $"comets N = {cometsN}\n"                  +
                            $"doClearBuffer = {doClearBuffer}\n"       +
                            $"doFillShapes = {doFillShapes}\n"         +
                            $"doConnectStatics = {doConnectStatics}\n" +
                            $"shape = {shape}\n"                       +
                            $"colorMode = {colorMode}\n"               +
                            $"angleMode = {angleMode}\n"               +
                            $"accelerationMode = {accelerationMode}\n" +
                            $"connectOpacity = {connectOpacity}\n"     +
                            $"renderDelay = {renderDelay}\n"           +
                            $"file: {colorPicker.GetFileName()}"

                            + "\n\n" + ssstmp;
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
            A = myUtils.randFloat(rand);

            switch (colorMode)
            {
                // Random color
                case 0:
                    R = myUtils.randFloat(rand);
                    G = myUtils.randFloat(rand);
                    B = myUtils.randFloat(rand);
                    break;

                // Random color from color picker
                case 1:
                    colorPicker.getColorRand(ref R, ref G, ref B);
                    break;

                // Limited color scheme
                case 2:
                    switch (rand.Next(50))
                    {
                        // Red
                        case 0:
                            R = 1;
                            G = 0;
                            B = 0;
                            break;

                        // Yellow
                        case 1:
                            R = 1;
                            G = 1;
                            B = 0;
                            break;

                        // Blue
                        case 2:
                            R = 0;
                            G = 0;
                            B = 1;
                            break;

                        // Orange
                        case 3:
                            R = 1;
                            G = 0.647f;
                            B = 0;
                            break;

                        // Aqua
                        case 4:
                            R = 0;
                            G = 1;
                            B = 1;
                            break;

                        // Violet
                        case 5:
                            R = 0.933f;
                            G = 0.510f;
                            B = 0.933f;
                            break;

                        // White
                        default:
                            R = 1.0f - myUtils.randFloat(rand) * 0.1f;
                            G = 1.0f - myUtils.randFloat(rand) * 0.1f;
                            B = 1.0f - myUtils.randFloat(rand) * 0.1f;
                            break;
                    }
                    break;

                // White
                case 3:
                    R = 1.0f - myUtils.randFloat(rand) * 0.2f;
                    G = 1.0f - myUtils.randFloat(rand) * 0.2f;
                    B = 1.0f - myUtils.randFloat(rand) * 0.2f;
                    break;
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
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

                    rectInst.setInstanceCoords(x - size/2, y - size/2, size, size);
                    rectInst.setInstanceColor(R, G, B, A);
                    rectInst.setInstanceAngle(angle);
                    break;

                // Instanced triangles
                case 1:
                    var triangleInst = inst as myTriangleInst;

                    triangleInst.setInstanceCoords(x, y, size/2, angle);
                    triangleInst.setInstanceColor(R, G, B, A);
                    break;

                // Instanced circles
                case 2:
                    var ellipseInst = inst as myEllipseInst;

                    ellipseInst.setInstanceCoords(x, y, size, angle);
                    ellipseInst.setInstanceColor(R, G, B, A);
                    break;

                // Instanced pentagons
                case 3:
                    var pentagonInst = inst as myPentagonInst;

                    pentagonInst.setInstanceCoords(x, y, size, angle);
                    pentagonInst.setInstanceColor(R, G, B, A);
                    break;

                // Instanced hexagons
                case 4:
                    var hexagonInst = inst as myHexagonInst;

                    hexagonInst.setInstanceCoords(x, y, size, angle);
                    hexagonInst.setInstanceColor(R, G, B, A);
                    break;
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Process(Window window)
        {
            uint cnt = 0;
            initShapes();

            // Build the background Galaxy
            var bgrTex = new myTexRectangle(buildGalaxy());

            if (doClearBuffer)
            {
                glDrawBuffer(GL_FRONT_AND_BACK | GL_DEPTH_BUFFER_BIT);
                glClearColor(0, 0, 0, 1);
            }

            // Create all the stars and other objects
            {
                // Add static stars (constellations)
                for (int i = 0; i < staticStarsN; i++)
                {
                    list.Add(new myObj_000_StaticStar());
                }

                // Add comets
                for (int i = 0; i < cometsN; i++)
                {
                    list.Add(new myObj_000_Comet());
                }

                // Add vague roaming lights
                for (int i = 0; i < cometsN; i++)
                {
                    list.Add(new myObj_000_VagueLight());
                }

                if (doCreateAllAtOnce)
                {
                    while (list.Count < N)
                        list.Add(new myObj_000_Star());
                }
            }

            // Gradually display the background Galaxy
            if (true)
            {
                bgrTex.setOpacity(0);

                while (!Glfw.WindowShouldClose(window))
                {
                    processInput(window);
                    Glfw.SwapBuffers(window);
                    Glfw.PollEvents();

                    float opacity = bgrTex.getOpacity() + 0.0025f;

                    if (opacity >= 1)
                        break;

                    bgrTex.setOpacity(opacity);
                    bgrTex.Draw(0, 0, gl_Width, gl_Height);
                }

                bgrTex.setOpacity(1);
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

                // Render Frame
                {
                    bgrTex.Draw(0, 0, gl_Width, gl_Height);

                    inst.ResetBuffer();
                    staticStarBgr.ResetBuffer();

                    if (doConnectStatics)
                    {
                        myPrimitive._LineInst.ResetBuffer();
                    }

                    int Count = list.Count;

                    for (int i = 0; i != Count; i++)
                    {
                        var obj = list[i] as myObj_000;

                        obj.Show();
                        obj.Move();
                    }

                    if (doConnectStatics)
                    {
                        myPrimitive._LineInst.Draw();
                    }

                    staticStarBgr.Draw(true);

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
                    list.Add(new myObj_000_Star());
                }

                System.Threading.Thread.Sleep(renderDelay);
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void initShapes()
        {
            staticStarBgr = new myHexagonInst(staticStarsN + cometsN * 3 + lightsN);

            myPrimitive.init_Triangle();

            myPrimitive.init_LineInst(staticStarsN * staticStarsN);

            base.initShapes(shape, N * 3, 0);
        }

        // ---------------------------------------------------------------------------------------------------------------

        // Build random Galaxy background
        private void setUpBackground(Graphics g, int w, int h)
        {
            void localGetRGB(int maxValue, int maxAggregate, ref int R, ref int G, ref int B)
            {
                do
                {
                    R = rand.Next(maxValue);
                    G = rand.Next(maxValue);
                    B = rand.Next(maxValue);
                }
                while (R + G + B > maxAggregate);
            }

            // ---------------------------------------------------------------------------

            switch (rand.Next(5))
            {
                // Black background
                case 0:
                    {
                        ssstmp = "mode0";
                        g.FillRectangle(Brushes.Black, 0, 0, w, h);
                    }
                    break;

                // Linear 2-color gradient in a random direction
                case 1:
                case 2:
                    {
                        ssstmp = "mode1";
                        RectangleF rect = new RectangleF(0, 0, w, h);
                        g.InterpolationMode = InterpolationMode.HighQualityBicubic;

                        Color color1 = myUtils.randomBool(rand) ? Color.Black : Color.FromArgb(255, rand.Next(23), rand.Next(23), rand.Next(23));
                        Color color2 =  (color1 != Color.Black) ? Color.Black : Color.FromArgb(255, rand.Next(23), rand.Next(23), rand.Next(23));

                        LinearGradientBrush grad = new LinearGradientBrush(rect, color1, color2, (LinearGradientMode)rand.Next(4));

                        g.FillRectangle(grad, rect);
                        grad.Dispose();
                    }
                    break;

                // Linear multi-color gradient
                case 3:
                case 4:
                    {
                        Color startColor = Color.Black;                                 // задаем начальный цвет для градиента
                        g.InterpolationMode = InterpolationMode.HighQualityBicubic;     // задаем режим интерполяции для градиента

                        // создаем градиент с начальным и конечным цветами
                        LinearGradientBrush grad = new LinearGradientBrush(new Point(0, 0), new Point(0, h), startColor, Color.Black);

                        // todo: limit max value for each r+g+b triplet. The value over 60 seems to be too much already.

                        int r2 = 0, g2 = 0, b2 = 0;
                        int r3 = 0, g3 = 0, b3 = 0;
                        int r4 = 0, g4 = 0, b4 = 0;

                        localGetRGB(10, 13, ref r2, ref g2, ref b2);
                        localGetRGB(15, 17, ref r3, ref g3, ref b3);
                        localGetRGB(10, 13, ref r4, ref g4, ref b4);

                        ssstmp = $"{r2}-{g2}-{b2} == {r2+g2+b2}\n{r3}-{g3}-{b3} == {r3+g3+b3}\n{r4}-{g4}-{b4} == {r4+g4+b4}";

                        // добавляем в градиент несколько цветов туманностей
                        grad.InterpolationColors = new ColorBlend()
                        {
                            // Colors
                            Colors = new Color[]
                            {
                                Color.FromArgb(255, rand.Next(05), rand.Next(05), rand.Next(05)),
                                Color.FromArgb(255, r2, g2, b2),
                                Color.FromArgb(255, r3, g3, b3),
                                Color.FromArgb(255, r4, g4, b4),
                                Color.FromArgb(255, rand.Next(05), rand.Next(05), rand.Next(05))
                            },

                            // Color positions
                            Positions = new float[]
                            {
                                0.0f,
                                0.2f + myUtils.randomSign(rand) * myUtils.randFloat(rand) * 0.10f,
                                0.5f + myUtils.randomSign(rand) * myUtils.randFloat(rand) * 0.15f,
                                0.8f + myUtils.randomSign(rand) * myUtils.randFloat(rand) * 0.10f,
                                1.0f 
                            }
                        };

                        // заполняем битмап градиентом
                        g.FillRectangle(grad, new Rectangle(0, 0, w, h));

                        // освобождаем ресурсы градиента
                        grad.Dispose();
                    }
                    break;
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        // Build random Galaxy
        private Bitmap buildGalaxy()
        {
            Bitmap bmp = null;

            int w = gl_Width;
            int h = gl_Height;

            try
            {
                int x1 = 0, y1 = 0, x2 = 0, y2 = 0;

                bmp = new Bitmap(w, h);

                using (var gr = Graphics.FromImage(bmp))
                using (var br = new SolidBrush(Color.Red))
                {
                    // Add background
                    setUpBackground(gr, w, h);

                    // Add low opacity colored spots
                    for (int i = 0; i < 10; i++)
                    {
                        int opacity = rand.Next(7) + 1;

                        x1 = rand.Next(w);
                        y1 = rand.Next(h);

                        x2 = rand.Next(333) + 100 * opacity;
                        y2 = rand.Next(333) + 100 * opacity;

                        br.Color = Color.FromArgb(1, rand.Next(256), rand.Next(256), rand.Next(256));

                        while (opacity != 0)
                        {
                            gr.FillRectangle(br, x1, y1, x2, y2);

                            x1 += rand.Next(25) + 25;
                            y1 += rand.Next(25) + 25;
                            x2 -= rand.Next(50) + 50;
                            y2 -= rand.Next(50) + 50;
                            opacity--;
                        }
                    }

                    // Add nebulae
                    for (int k = 0; k < 111; k++)
                    {
                        for (int i = 0; i < 3; i++)
                        {
                            x1 = rand.Next(w);
                            y1 = rand.Next(h);

                            int masterOpacity = rand.Next(100) + 100;
                            int radius1 = rand.Next(250) + 100;
                            int radius2 = rand.Next(250) + 100;
                            colorPicker.getColor(br, x1, y1, masterOpacity);

                            gr.FillRectangle(br, x1, y1, 1, 1);

                            for (int j = 0; j < 100; j++)
                            {
                                x2 = x1 + rand.Next(radius1) - radius1 / 2;
                                y2 = y1 + rand.Next(radius2) - radius2 / 2;

                                int maxSlaveOpacity = 50 + rand.Next(50);
                                int slaveOpacity = rand.Next(maxSlaveOpacity);

                                br.Color = Color.FromArgb(slaveOpacity, br.Color.R, br.Color.G, br.Color.B);
                                gr.FillRectangle(br, x2, y2, 1, 1);

                                int r = rand.Next(2) + 3;

                                br.Color = Color.FromArgb(3, br.Color.R, br.Color.G, br.Color.B);
                                gr.FillRectangle(br, x2 - r, y2 - r, 2 * r, 2 * r);
                            }
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                throw ex;
            }

            return bmp;
        }

        // ---------------------------------------------------------------------------------------------------------------
    }


    // ===================================================================================================================
    // ===================================================================================================================


    // Moving Star Class
    class myObj_000_Star : myObj_000
    {
        protected override void generateNew()
        {
            base.generateNew();

            float speed = myUtils.randFloat(rand) + rand.Next(10);

            max = rand.Next(75) + 20;
            cnt = 0;
            acceleration = 1.005f + (rand.Next(100) * 0.0005f);

            x = rand.Next(gl_Width);
            y = rand.Next(gl_Width);

            double dist = Math.Sqrt((x - gl_x0) * (x- gl_x0) + (y - gl_x0) * (y - gl_x0));
            double sp_dist = speed / dist;

            dx = (float)((x - gl_x0) * sp_dist);
            dy = (float)((y - gl_x0) * sp_dist);

            y = (y - (gl_Width - gl_Height) / 2);

            size = 0;
            dSize = myUtils.randFloat(rand) * 0.1f;

            if (myUtils.randomChance(rand, 1, 1000))
            {
                dSize = myUtils.randFloat(rand) * 0.5f;
            }

            angle = 0;

            switch (angleMode)
            {
                case 0:
                    dAngle = myUtils.randFloat(rand, 0.01f) * 0.01f * myUtils.randomSign(rand);
                    break;

                case 1:
                    dAngle = myUtils.randFloat(rand, 0.25f) * 0.50f * myUtils.randomSign(rand);
                    break;

                case 2:
                    dAngle = myUtils.randFloat(rand, 0.33f) * 2.00f * myUtils.randomSign(rand);
                    break;
            }
        }

        protected override void Show()
        {
            base.Show();
        }

        protected override void Move()
        {
            x += dx;
            y += dy;

            // todo: 
            size += dSize;
            angle += dAngle;

            acceleration *= (1.0f + (size * 0.0001f));

            if (cnt++ > max)
            {
                cnt = 0;

                // Accelerate acceleration rate
                if (accelerationMode == 1)
                {
                    acceleration *= (1.0f + (size * 0.001f));
                }
            }

            // Accelerate our moving stars
            {
                dx *= acceleration;
                dy *= acceleration;
            }

            if (x < 0 || x > gl_Width || y < 0 || y > gl_Height)
            {
                generateNew();
            }

            return;
        }
    };


    // ===================================================================================================================
    // ===================================================================================================================


    // Static (Constellation) Star Class
    class myObj_000_StaticStar : myObj_000
    {
        private int lifeCounter = 0;
        protected int alpha = 0, bgrAlpha = 0;
        private static int factor = 1, maxDist = 15000;
        private static bool doMove = true;

        private List<myObj_000_StaticStar> neighbours = new List<myObj_000_StaticStar>();

        protected override void generateNew()
        {
            dx = dy = 0;

            base.generateNew();

            lifeCounter = (rand.Next(500) + 500) * factor;

            x = rand.Next(gl_Width);
            y = rand.Next(gl_Height);
            alpha = rand.Next(50) + 175;

            bgrAlpha = 1;

            if (rand.Next(11) == 0)
                bgrAlpha = 2;

            max = (rand.Next(200) + 100) * factor;
            cnt = 0;
            size = 0;

            dAngle = myUtils.randFloat(rand) * 0.001f * myUtils.randomSign(rand);

            A = myUtils.randFloat(rand, 0.2f) * 0.33f;

            // Make our static stars not so static
            if (doMove)
            {
                // Linear speed outwards:
                double dist = Math.Sqrt((x - gl_x0) * (x - gl_x0) + (y - gl_y0) * (y - gl_y0));
                double sp_dist = 0.1f / dist;

                dx = (float)((x - gl_x0) * sp_dist);
                dy = (float)((y - gl_y0) * sp_dist);
            }

            findNeighbours();
        }

        protected override void Show()
        {
            // Background glow
            {
                int bgrSize = rand.Next(3) + 3;
                float bgrA = 0.2f / bgrSize;

                float r = R + myUtils.randomSign(rand) * myUtils.randFloat(rand) * 0.1f;
                float g = G + myUtils.randomSign(rand) * myUtils.randFloat(rand) * 0.1f;
                float b = B + myUtils.randomSign(rand) * myUtils.randFloat(rand) * 0.1f;

                staticStarBgr.setInstanceCoords(x, y, bgrSize * size, 0);
                staticStarBgr.setInstanceColor(r, g, b, bgrA);
            }

            switch (shape)
            {
                // Instanced squares
                case 0:
                    var rectInst = inst as myRectangleInst;

                    rectInst.setInstanceCoords(x - size * 0.5f, y - size * 0.5f, size, size);
                    rectInst.setInstanceColor(R, G, B, A * 0.5f);
                    rectInst.setInstanceAngle(angle);

                    rectInst.setInstanceCoords(x - size * 0.5f, y - size * 0.5f, size, size);
                    rectInst.setInstanceColor(R, G, B, A * 0.5f);
                    rectInst.setInstanceAngle(angle + (float)Math.PI * 0.25f);
                    break;

                // Instanced triangles
                case 1:
                    var triangleInst = inst as myTriangleInst;

                    triangleInst.setInstanceCoords(x, y, size, angle);
                    triangleInst.setInstanceColor(R, G, B, A * 0.5f);

                    triangleInst.setInstanceCoords(x, y, size, angle + (float)Math.PI);
                    triangleInst.setInstanceColor(R, G, B, A * 0.5f);
                    break;

                // Instanced circles
                case 2:
                    var ellipseInst = inst as myEllipseInst;

                    ellipseInst.setInstanceCoords(x, y, size, angle);
                    ellipseInst.setInstanceColor(R, G, B, A);
                    break;

                // Instanced pentagons
                case 3:
                    var pentagonInst = inst as myPentagonInst;

                    pentagonInst.setInstanceCoords(x, y, size, angle);
                    pentagonInst.setInstanceColor(R, G, B, A);

                    pentagonInst.setInstanceCoords(x, y, size, angle + (float)Math.PI);
                    pentagonInst.setInstanceColor(R, G, B, A * 0.75f);
                    break;

                // Instanced hexagons
                case 4:
                    var hexagonInst = inst as myHexagonInst;

                    hexagonInst.setInstanceCoords(x, y, size, angle);
                    hexagonInst.setInstanceColor(R, G, B, A);
                    break;
            }

            // Also, each star could have its own maxDist
            // This way, constellations would look a bit different

            if (doConnectStatics == true)
            {
                for (int i = 0; i < neighbours.Count; i++)
                {
                    float distSquared = (x - neighbours[i].x) * (x - neighbours[i].x) + (y - neighbours[i].y) * (y - neighbours[i].y);

                    if (distSquared > maxDist)
                    {
                        neighbours.RemoveAt(i);
                    }
                    else
                    {
                        myPrimitive._LineInst.setInstanceCoords(x, y, neighbours[i].x, neighbours[i].y);
                        myPrimitive._LineInst.setInstanceColor(1, 1, 1, connectOpacity * distSquared / maxDist);
                    }
                }
            }

            return;
        }

        protected override void Move()
        {
            x += dx;
            y += dy;
            angle += dAngle;

            if (lifeCounter-- == 0)
            {
                factor = 3;
                generateNew();
            }
            else
            {
                if (cnt++ > max)
                {
                    cnt = 0;
                    size = rand.Next(5) + rand.Next(2) + 1;
                    A += myUtils.randomSign(rand) * myUtils.randFloat(rand) * 0.1f;

                    A = A < 0.066f ? 0.066f : A;

                    if (myUtils.randomChance(rand, 1, 5))
                    {
                        findNeighbours();
                    }
                }
            }
        }

        private void findNeighbours()
        {
            neighbours.Clear();

            int Count = list.Count;

            for (int i = 0; i != Count; i++)
            {
                if (list[i] is myObj_000_StaticStar)
                {
                    myObj_000_StaticStar obj = list[i] as myObj_000_StaticStar;

                    if (obj != this)
                    {
                        float distSquared = (x - obj.x) * (x - obj.x) + (y - obj.y) * (y - obj.y);

                        if (distSquared < maxDist)
                        {
                            neighbours.Add(obj);
                        }
                    }
                }
            }
        }
    };


    // ===================================================================================================================
    // ===================================================================================================================


    // Falling Star (Comet) Class
    class myObj_000_Comet : myObj_000
    {
        private float X, Y, tailR, tailG, tailB;
        private int lifeCounter = 0;

        protected override void generateNew()
        {
            lifeCounter = rand.Next(2000) + 999;
            //lifeCounter = rand.Next(100) + 66;

            R = 1.0f - myUtils.randFloat(rand) * 0.23f;
            G = 0.0f + myUtils.randFloat(rand) * 0.23f;
            B = 0.0f + myUtils.randFloat(rand) * 0.23f;
            A = 1.0f - myUtils.randFloat(rand) * 0.10f;

            tailR = 1.0f - myUtils.randFloat(rand) * 0.33f;
            tailG = 0.0f + myUtils.randFloat(rand) * 0.33f;
            tailB = 0.0f + myUtils.randFloat(rand) * 0.33f;

            int x0 = rand.Next(gl_Width);
            int y0 = rand.Next(gl_Height);
            int x1 = rand.Next(gl_Width);
            int y1 = rand.Next(gl_Height);

            float a = (float)(y1 - y0) / (float)(x1 - x0);
            float b = y1 - a * x1;

            float speed = rand.Next(200) + 200.0f + myUtils.randFloat(rand);
            //speed *= 0.01f;

            // Set comet size
            switch (rand.Next(11))
            {
                case 0:
                    size = rand.Next(20) + 5;
                    break;

                case 1:
                    size = rand.Next(3) + 1;
                    speed /= (rand.Next(25) + 25);
                    A /= 3;
                    break;

                default:
                    size = rand.Next(7) + 3;
                    break;
            }

            double dist = Math.Sqrt((x1 - x0) * (x1 - x0) + (y1 - y0) * (y1 - y0));
            double sp_dist = speed / dist;

            dx = (float)((x1 - x0) * sp_dist);
            dy = (float)((y1 - y0) * sp_dist);

            if (dx > 0)
            {
                x = X = 0;
                y = Y = (int)b;
            }
            else
            {
                X = x = gl_Width;
                y = a * x + b;
                Y = (int)y;
            }
        }

        protected override void Show()
        {
            if (lifeCounter < 0)
            {
                // Background glow
                {
                    int bgrSize = (int)size + rand.Next(13) + 33;
                    float bgrA = 0.05f;

                    float r = 1.00f - myUtils.randFloat(rand) * 0.1f;
                    float g = 0.26f + myUtils.randFloat(rand) * 0.1f;
                    float b = 0.05f + myUtils.randFloat(rand) * 0.1f;

                    staticStarBgr.setInstanceCoords(x, y, bgrSize * size, myUtils.randFloat(rand));
                    staticStarBgr.setInstanceColor(r, g, b, bgrA);

                    bgrSize = (int)size + rand.Next(11) + 5;
                    bgrA = 0.025f;

                    r = R + myUtils.randomSign(rand) * myUtils.randFloat(rand) * 0.1f;
                    g = G + myUtils.randomSign(rand) * myUtils.randFloat(rand) * 0.1f;
                    b = B + myUtils.randomSign(rand) * myUtils.randFloat(rand) * 0.1f;

                    staticStarBgr.setInstanceCoords(x, y, bgrSize * size, 0);
                    staticStarBgr.setInstanceColor(r, g, b, bgrA);

                    bgrSize = (int)size + rand.Next(3) + 3;
                    bgrA = 0.05f;

                    r = R + myUtils.randomSign(rand) * myUtils.randFloat(rand) * 0.1f;
                    g = G + myUtils.randomSign(rand) * myUtils.randFloat(rand) * 0.1f;
                    b = B + myUtils.randomSign(rand) * myUtils.randFloat(rand) * 0.1f;

                    staticStarBgr.setInstanceCoords(x, y, bgrSize * size, 0);
                    staticStarBgr.setInstanceColor(r, g, b, bgrA);
                }

                // Tail: Draw some triangles
                {
                    int x1 = (int)x;
                    int y1 = (int)y;
                    int x2 = (int)x;
                    int y2 = (int)y;

                    if (Math.Abs(dx) > Math.Abs(dy))
                    {
                        y1 -= (int)size;
                        y2 += (int)size;
                    }
                    else
                    {
                        x1 -= (int)size;
                        x2 += (int)size;
                    }

                    float a1 = 0.05f + myUtils.randFloat(rand) * 0.1f;
                    float a2 = 0.05f + myUtils.randFloat(rand) * 0.2f;

                    // Length of the tail depends on the speed. For slow speeds tail will be very short

                    myPrimitive._Triangle.SetColor(1, 1, 1, a1);
                    myPrimitive._Triangle.Draw(x1, y1, x2, y2, x - dx * 7, y - dy * 7, myUtils.randomChance(rand, 2, 3));

                    myPrimitive._Triangle.SetColor(tailR, tailG, tailB, a2);
                    myPrimitive._Triangle.Draw(x1, y1, x2, y2, x - dx * 4, y - dy * 4, myUtils.randomChance(rand, 2, 3));
                }

                base.Show();
            }
        }

        protected override void Move()
        {
            // Wait for the counter to reach zero. Then start moving the comet
            if (lifeCounter-- < 0)
            {
                x += dx;
                y += dy;

                X = (int)x;
                Y = (int)y;

                if ((dx > 0 && X > gl_Width + 999) || (dx < 0 && X < -999) || (dy > 0 && Y > gl_Height + 999) || (dy < 0 && Y < -999))
                {
                    generateNew();
                }
            }
        }
    };


    // ===================================================================================================================
    // ===================================================================================================================


    // Vague Roaming Light Class
    class myObj_000_VagueLight : myObj_000
    {
        protected override void generateNew()
        {
            x = rand.Next(gl_Width);
            y = rand.Next(gl_Height);

            size = rand.Next(666) + 234;
            angle = myUtils.randFloat(rand);
            dAngle = myUtils.randomSign(rand) * myUtils.randFloat(rand, 0.1f) * 0.005f;

            dx = myUtils.randomSign(rand) * myUtils.randFloat(rand, 0.1f);
            dy = myUtils.randomSign(rand) * myUtils.randFloat(rand, 0.1f);

            R = 1.0f - myUtils.randFloat(rand) * 0.5f;
            G = 1.0f - myUtils.randFloat(rand) * 0.5f;
            B = 1.0f - myUtils.randFloat(rand) * 0.5f;
            A = myUtils.randFloat(rand) * 0.023f + 0.001f;
        }

        protected override void Show()
        {
            staticStarBgr.setInstanceCoords(x, y, size, angle);
            staticStarBgr.setInstanceColor(R, G, B, A);
        }

        protected override void Move()
        {
            x += dx;
            y += dy;
            angle += dAngle;

            if (x < 0)
                dx += 0.01f;

            if (y < 0)
                dy += 0.01f;

            if (x > gl_Width)
                dx -= 0.01f;

            if (y > gl_Height)
                dy -= 0.01f;
        }
    };


    // ===================================================================================================================
    // ===================================================================================================================
};
