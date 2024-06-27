using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using static OpenGL.GL;



namespace my
{
    public class myUtils
    {
        private static Random _rand = null;

        // ---------------------------------------------------------------------------------------------------------------

        public static void setRand(Random r)
        {
            _rand = r;
        }

        // ---------------------------------------------------------------------------------------------------------------

        // Randomly return +1 or -1
        public static int randomSign(Random r)
        {
            return r.Next(2) == 0 ? 1 : -1;
        }

        // ---------------------------------------------------------------------------------------------------------------

        // Return +1 or -1, depending on the argument sign
        public static int signOf(int arg, bool reversed = false)
        {
            return reversed
                ? arg >= 0 ? -1 : +1
                : arg >= 0 ? +1 : -1;
        }

        // ---------------------------------------------------------------------------------------------------------------

        // Return +1 or -1, depending on the argument sign
        public static float signOf(float arg, bool reversed = false)
        {
            return reversed
                ? arg >= 0 ? -1 : +1
                : arg >= 0 ? +1 : -1;
        }

        // ---------------------------------------------------------------------------------------------------------------

        public static float randFloat(Random r, float min = 0.0f)
        {
            return (float)r.NextDouble() + min;
        }

        // ---------------------------------------------------------------------------------------------------------------

        // Returns a double number from the range [-1, +1]
        public static float randFloatSigned(Random r, float min = 0.0f)
        {
            //return (float)(2 * r.NextDouble() - 1 + min);     // How to ajdust for min here?

            return (float)(r.NextDouble() + min) * (r.Next(2) == 0 ? 1 : -1);
        }

        // ---------------------------------------------------------------------------------------------------------------

        // Randomly return -1, 0 or +1
        public static int random101(Random r)
        {
            return r.Next(3) - 1;
        }

        // ---------------------------------------------------------------------------------------------------------------

        // Randomly return true or false
        public static bool randomBool(Random r)
        {
            return r.Next(2) == 0;
        }

        // ---------------------------------------------------------------------------------------------------------------

        // Randomly return a chance: [min] out of [max]
        public static bool randomChance(Random r, int min, int max)
        {
            return r.Next(max) < min;
        }

        // ---------------------------------------------------------------------------------------------------------------

        public static float getRandomNum(float min, float max, float step, Random r)
        {
            float rrr = min + step * r.Next((int)max - (int)min);

            float a = 0.01f * (r.Next(100 - 0) + 0);

            return r.Next(2) == 0 ? 1 : -1;
        }

        // ---------------------------------------------------------------------------------------------------------------

        public static void getRandomColor(Random r, ref float R, ref float G, ref float B, float min)
        {
            do
            {
                R = (float)r.NextDouble();
                G = (float)r.NextDouble();
                B = (float)r.NextDouble();
            }
            while (R + G + B < min);
        }

        // ---------------------------------------------------------------------------------------------------------------

        public static void swap<Type>(ref Type a, ref Type b)
        {
            Type tmp = a;
            a = b;
            b = tmp;

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        // Get gradiend background
        public static Bitmap getGradientBgr(ref Random rand, int width, int height)
        {
            Bitmap bmp = null;

            bmp = new Bitmap(width, height);

            using (var gr = Graphics.FromImage(bmp))
            {
                RectangleF rect = new RectangleF(0, 0, width, height);
                gr.InterpolationMode = InterpolationMode.HighQualityBicubic;
                gr.SmoothingMode = SmoothingMode.AntiAlias;

                Color color1 = Color.DarkMagenta;
                Color color2 = Color.Yellow;

                if (myUtils.randomChance(rand, 1, 2))
                {
                    color1 = Color.FromArgb(255, rand.Next(33), rand.Next(33), rand.Next(33));
                    color2 = Color.FromArgb(255, rand.Next(66), rand.Next(66), rand.Next(66));
                }
                else
                {
                    color1 = Color.FromArgb(255, rand.Next(66), rand.Next(66), rand.Next(66));
                    color2 = Color.FromArgb(255, rand.Next(33), rand.Next(33), rand.Next(33));
                }

                color1 = Color.DarkMagenta;
                color2 = Color.Yellow;

                color1 = Color.FromArgb(255, 39, 0, 39);
                color2 = Color.FromArgb(255, 55, 55, 0);

                LinearGradientBrush grad = new LinearGradientBrush(rect, color1, color2, LinearGradientMode.Vertical);

                gr.FillRectangle(grad, rect);
                grad.Dispose();
            }

            return bmp;
        }

        // ---------------------------------------------------------------------------------------------------------------

        // Read a pixel from the current frame buffer
        public static unsafe void readPixel(int x, int y)
        {
            float[] pixel = new float[4];

            fixed (float* ppp = &pixel[0])
            {
                glReadPixels(x, y, 1, 1, GL_RGBA, GL_FLOAT, ppp);
            }
        }

        // ---------------------------------------------------------------------------------------------------------------

        // Change global antializing mode
        public static void SetAntializingMode(bool isSet)
        {
            if (isSet)
            {
                glEnable(GL_LINE_SMOOTH);
                glEnable(GL_POLYGON_SMOOTH);
                glHint(GL_LINE_SMOOTH_HINT, GL_NICEST);
                glHint(GL_POLYGON_SMOOTH_HINT, GL_NICEST);
            }
        }

        // ---------------------------------------------------------------------------------------------------------------

        public static string nStr(int n)
        {
            return n.ToString("N0");
        }

        // ---------------------------------------------------------------------------------------------------------------

        public static string fStr(float f)
        {
            return f.ToString("0.000");
        }

        // ---------------------------------------------------------------------------------------------------------------

        public static string strCountOf(int cnt, int N)
        {
            return $"N = {cnt} of {N}\n";
        }

        // ---------------------------------------------------------------------------------------------------------------
    };
};
