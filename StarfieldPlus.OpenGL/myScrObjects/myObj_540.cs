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
        // Priority
        public static int Priority => 10;

        private float x, y, dy, angle, dAngle;
        private float A, R, G, B;
        private int   size, index;

        private static int N = 0;
        private static float dimAlpha = 0.05f;

        private static myTexRectangle tex = null;
        private static int texWidth = 0, texHeight = 0;

        private static string str;

        private static int maxSpeed = 0, posXGenMode = 0, posYGenMode = 0, angleMode = 0, modX = 0;

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

            // Global unmutable constants
            {
                N = 10000;
            }

            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time local initialization
        private void initLocal()
        {
            doClearBuffer = myUtils.randomBool(rand);
            doClearBuffer = true;

            maxSpeed = 3 + rand.Next(13);               // max falling speed
            posXGenMode = rand.Next(3);                 // where the particles are generated along the X-axis
            posYGenMode = rand.Next(3);                 // where the particles are generated along the Y-axis
            angleMode = rand.Next(3);                   // letter rotation mode

            renderDelay = rand.Next(11) + 1;

            modX = rand.Next(333) + 11;

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

            dy = myUtils.randFloat(rand, 0.1f) * (rand.Next(maxSpeed) + 1);

            // Set up letter rotation
            {
                angle = 0;
                dAngle = myUtils.randFloat(rand) * 0.001f * myUtils.randomSign(rand);

                if (angleMode == 2)
                    dAngle *= rand.Next(5) + 1;
            }

            size = 33;
            index = rand.Next(str.Length);

            return;
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
            int offset = size * index;

            tex.setOpacity(A);
            tex.setAngle(angle);

            //tex.setColor(R, G, B);

            tex.Draw((int)x, (int)y, size, texHeight, offset * gl_Width / texWidth, 0, size * gl_Width / texWidth, texHeight * gl_Height / texHeight);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Process(Window window)
        {
            str = "abcdefghijklmnopqrstuvwxyz";

            tex = getFontTexture("Tahoma", 33, ref texWidth, ref texHeight, str);

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
                //glDrawBuffer(GL_DEPTH_BUFFER_BIT);
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
            Width  = fontSize * str.Length;
            Height = fontSize * 2;

            var bmp = new Bitmap(Width, Height);

            RectangleF rectf = new RectangleF(0, 0, fontSize, Height);

            using (var gr = Graphics.FromImage(bmp))
            using (var br = new SolidBrush(Color.FromArgb(255, rand.Next(255), rand.Next(255), rand.Next(255))))
            using (var font = new Font(fontName, fontSize, FontStyle.Regular, GraphicsUnit.Pixel))
            {
                gr.SmoothingMode = SmoothingMode.AntiAlias;
                gr.InterpolationMode = InterpolationMode.HighQualityBicubic;
                gr.PixelOffsetMode = PixelOffsetMode.HighQuality;

                for (int i = 0; i < str.Length; i++)
                {
                    gr.DrawString(str[i].ToString(), font, br, rectf);
                    rectf.X += fontSize;
                }
            }

            return new myTexRectangle(bmp);
        }

        // ---------------------------------------------------------------------------------------------------------------

    }
};
