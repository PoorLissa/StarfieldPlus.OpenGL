using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;

using System.Drawing;
using System.Drawing.Drawing2D;


/*
    - Falling alphabet letters (Matrix style)
*/


namespace my
{
    public class myObj_540 : myObject
    {
        public class generator
        {
            public int x, y, cnt;
            public float spd, opacity;

            public void getNew()
            {
                x = rand.Next(gl_Width);
                y = rand.Next(gl_Height/2) - 333;
                y = -111;

                cnt = rand.Next(33) + 11;

                spd = myUtils.randomChance(rand, 1, 2)
                    ? myUtils.randFloat(rand, 0.1f) * (rand.Next(maxSpeed) + 1)
                    : -1;

                opacity = myUtils.randomChance(rand, 1, 2)
                    ? myUtils.randFloat(rand) * 0.9f
                    : -1;
            }
        };

        // Priority
        public static int Priority => 10;

        private float x, y, dy, angle, dAngle;
        private float A, R, G, B;
        private int   index;

        private static int N = 0, nGenerators = 0;
        private static float dimAlpha = 0.05f;

        private static int texWidth = 0, texHeight = 0, maxSpeed = 0, posXGenMode = 0, posYGenMode = 0, angleMode = 0, modX = 0, size = 33;

        private static myTexRectangle tex = null;
        private static string str, fontFamily = "Tahoma";

        private static List<generator> Generators = null;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_540()
        {
            if (id != uint.MaxValue)
                generateNew();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time global initialization
        protected override void initGlobal()
        {
            colorPicker = new myColorPicker(gl_Width, gl_Height, mode: myColorPicker.colorMode.SNAPSHOT);
            list = new List<myObject>();
            Generators = new List<generator>();

            // Global unmutable constants
            {
                N = 10000;

                size = rand.Next(60) + 20;

                getFont(ref fontFamily);
            }

            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time local initialization
        private void initLocal()
        {
            doClearBuffer = myUtils.randomChance(rand, 4, 5);

            maxSpeed = 3 + rand.Next(13);               // max falling speed
            posXGenMode = rand.Next(3);                 // where the particles are generated along the X-axis
            posYGenMode = rand.Next(3);                 // where the particles are generated along the Y-axis
            angleMode = rand.Next(3);                   // letter rotation mode

            renderDelay = rand.Next(11) + 1;

            modX = rand.Next(333) + 11;

            // Set up Generators:
            {
                Generators.Clear();

                nGenerators = myUtils.randomChance(rand, 1, 2)
                    ? 0
                    : rand.Next(111) + 33;

                for (int i = 0; i < nGenerators; i++)
                {
                    var gen = new generator();
                    gen.getNew();
                    Generators.Add(gen);
                }
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            height = 600;

            string nStr(int   n) { return n.ToString("N0");    }
            //string fStr(float f) { return f.ToString("0.000"); }

            string str = $"Obj = myObj_540\n\n"                           +
                            $"N = {nStr(list.Count)} of {nStr(N)}\n"      +
                            $"doClearBuffer = {doClearBuffer}\n"          +
                            $"renderDelay = {renderDelay}\n"              +
                            $"generators = {nGenerators}\n"               +
                            $"font = '{fontFamily}'\n"                    +
                            $"size = {size}\n"                            +
                            $"maxSpeed = {maxSpeed}\n"                    +
                            $"xGenMode = {posXGenMode} (modX = {modX})\n" +
                            $"yGenMode = {posYGenMode}\n"                 +
                            $"angleMode = {angleMode}\n"                  +
                            $"file: {colorPicker.GetFileName()}"
                ;
            return str;
        }

        // ---------------------------------------------------------------------------------------------------------------

        // 
        protected override void setNextMode()
        {
            initLocal();

            clearScreenSetup(doClearBuffer, 0.2f);
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void generateNew()
        {
            A = myUtils.randFloat(rand) * 0.9f;

            colorPicker.getColor(rand.Next(gl_Width), rand.Next(gl_Height), ref R, ref G, ref B);

            dy = myUtils.randFloat(rand, 0.1f) * (rand.Next(maxSpeed) + 1);

            if (nGenerators > 0)
            {
                generateNew_2();

                if (dy < 0)
                    dy = myUtils.randFloat(rand, 0.1f) * (rand.Next(maxSpeed) + 1);
            }
            else
            {
                generateNew_1();
            }

            // Set up letter rotation
            {
                angle = 0;
                dAngle = myUtils.randFloat(rand) * 0.001f * myUtils.randomSign(rand);

                if (angleMode == 2)
                    dAngle *= rand.Next(5) + 1;
            }

            index = rand.Next(str.Length);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        // No Generators
        private void generateNew_1()
        {
            x = rand.Next(gl_Width);

            // Set position X
            switch (posXGenMode)
            {
                case 0:
                    break;

                case 1:
                    x -= x % modX;
                    break;

                case 2:
                    {
                        int opacityFactor = (int)(A * 10);

                        if (opacityFactor > 0)
                        {
                            x -= x % (opacityFactor * modX / 10);
                        }
                    }
                    break;
            }

            // Set position Y
            switch (posYGenMode)
            {
                case 0:
                    y = rand.Next(gl_Height / 2);
                    break;

                case 1:
                    y = rand.Next(gl_Height / 2) - 333;
                    break;

                case 2:
                    y = -1 * rand.Next(333) - 100;
                    break;
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        // Use Generators
        private void generateNew_2()
        {
            int n = rand.Next(nGenerators);

            x = Generators[n].x;
            y = Generators[n].y;

            if (Generators[n].spd > 0)
                dy = Generators[n].spd;

            if (Generators[n].opacity > 0)
                A = Generators[n].opacity;

            Generators[n].cnt--;

            if (Generators[n].cnt < 0)
            {
                Generators[n].getNew();
            }
        }

        // ---------------------------------------------------------------------------------------------------------------


        protected override void Move()
        {
            y += dy;

            switch (angleMode)
            {
                case 0:
                    break;

                case 1:
                    angle += dAngle;
                    break;

                case 2:
                    {
                        if ((angle > 0.2f && dAngle > 0) || (angle < -0.2f && dAngle < 0))
                            dAngle *= -1;
                    }
                    break;
            }

            if (y > gl_Height)
            {
                generateNew();
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Show()
        {
            int offset = (size) * index;

            tex.setOpacity(A);
            tex.setAngle(angle);

            //tex.setColor(R, G, B);

            tex.Draw((int)x, (int)y, size, texHeight, offset * gl_Width / texWidth, 0, size * gl_Width / texWidth, texHeight * gl_Height / texHeight);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Process(Window window)
        {
            getAlphabet(ref str);

            try
            {
                tex = getFontTexture(fontFamily, size, ref texWidth, ref texHeight, str);
            }
            catch (System.Exception ex)
            {
                // Some fonts will cause this exception (for example, "Vivaldi")
                tex = getFontTexture("Tahoma", size, ref texWidth, ref texHeight, str);
            }

            uint cnt = 0;
            initShapes();

            dimAlpha = 0.25f;

            if (doClearBuffer)
            {
                clearScreenSetup(doClearBuffer, 0.2f);
            }
            else
            {
                dimScreenRGB_SetRandom(0.1f);
                glDrawBuffer(GL_FRONT_AND_BACK);
                glDrawBuffer(GL_BACK);
            }

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
                    int Count = list.Count;

                    for (int i = 0; i != Count; i++)
                    {
                        var obj = list[i] as myObj_540;

                        obj.Show();
                        obj.Move();
                    }
                }

                if (list.Count < N)
                {
                    list.Add(new myObj_540());
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

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        // Create a texture with symbols on it
        private myTexRectangle getFontTexture(string fontName, int fontSize, ref int Width, ref int Height, string str)
        {
            Bitmap bmp = null;

            //int www = maxCharWidth(str, fontSize, fontName);
            //extraSpace = www - fontSize;

            Width  = (fontSize) * str.Length;
            Height = fontSize * 2;

            int aaa = 0;

            try
            {
                bmp = new Bitmap(Width, Height);

                aaa = 1;

                RectangleF rectf = new RectangleF(0, 0, fontSize, Height);

                aaa = 2;

                using (var gr = Graphics.FromImage(bmp))
                {
                    aaa = 3;

                    using (var br = new SolidBrush(Color.FromArgb(255, rand.Next(255), rand.Next(255), rand.Next(255))))
                    {
                        aaa = 4;

                        using (var font = new Font(fontName, fontSize, FontStyle.Regular, GraphicsUnit.Pixel))
                        {
                            aaa = 5;

                            gr.SmoothingMode = SmoothingMode.AntiAlias;
                            gr.InterpolationMode = InterpolationMode.HighQualityBicubic;
                            gr.PixelOffsetMode = PixelOffsetMode.HighQuality;

                            for (int i = 0; i < str.Length; i++)
                            {
                                gr.DrawString(str[i].ToString(), font, br, rectf);
                                rectf.X += fontSize;
                            }

                            aaa = 6;
                        }
                    }
                }

            }
            catch (System.Exception ex)
            {
                throw ex;
            }

            return new myTexRectangle(bmp);
        }

        // ---------------------------------------------------------------------------------------------------------------

        // Return random set of characters made of a random sum of defined sets
        private void getAlphabet(ref string str)
        {
            const int N = 6;
            str = "";

            string[] arr = new string[N] {
                "абвгдеёжзийклмнопрстуфхцчшщъыьэюя",
                "АБВГДЕЁЖЗИЙКЛМНОПРСТУФХЦЧШЩЪЫЬЭЮЯ",
                "abcdefghijklmnopqrstuvwxyz",
                "ABCDEFGHIJKLMNOPQRSTUVWXYZ",
                "0123456789",
                "!@#$%^&*()[]{}<>-+=/?~;:"
            };

            // Get random number from [1 .. (2^N)-1]
            uint n = (uint)rand.Next((int)Math.Pow(2, N) - 1) + 1;

            for (int i = 0; i < N; i++)
            {
                if (((uint)(1 << i) & n) != 0)
                {
                    str += arr[i];
                }
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        // Return random font family name
        private void getFont(ref string font)
        {
            switch (rand.Next(7))
            {
                case 0: font = "Tahoma";   break;
                case 1: font = "Arial";    break;
                case 2: font = "Consolas"; break;
                case 3: font = "Calibri";  break;

                default:
                    using (System.Drawing.Text.InstalledFontCollection col = new System.Drawing.Text.InstalledFontCollection())
                    {
                        int n = rand.Next(col.Families.Length);
                        font = col.Families[n].Name;
                    }
                    break;
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private int maxCharWidth(string text, int size, string fontName)
        {
            int res = 0;

            var bmp = new Bitmap(100, 100);

            using (var gr = Graphics.FromImage(bmp))
            using (var br = new SolidBrush(Color.FromArgb(255, rand.Next(255), rand.Next(255), rand.Next(255))))
            using (var font = new Font(fontName, size, FontStyle.Regular, GraphicsUnit.Pixel))
            {
                gr.SmoothingMode = SmoothingMode.AntiAlias;
                gr.InterpolationMode = InterpolationMode.HighQualityBicubic;
                gr.PixelOffsetMode = PixelOffsetMode.HighQuality;

                for (int i = 0; i < str.Length; i++)
                {
                    var n = gr.MeasureString(str[i].ToString(), font);

                    if (res < n.Width)
                        res = (int)n.Width;
                }
            }

            return res;
        }

        // ---------------------------------------------------------------------------------------------------------------

    }
};
