using System;



namespace my
{
    public class myUtils
    {
        private static Random _rand = null;

        // -------------------------------------------------------------------------

        public static void setRand(Random r)
        {
            _rand = r;
        }

        // -------------------------------------------------------------------------

        // Randomly return +1 or -1
        public static int randomSign(Random r)
        {
            return r.Next(2) == 0 ? 1 : -1;
        }

        // -------------------------------------------------------------------------

        // Return +1 or -1, depending on the argument sign
        public static int signOf(int arg, bool reversed = false)
        {
            return reversed
                ? arg >= 0 ? -1 : +1
                : arg >= 0 ? +1 : -1;
        }

        // -------------------------------------------------------------------------

        // Return +1 or -1, depending on the argument sign
        public static float signOf(float arg, bool reversed = false)
        {
            return reversed
                ? arg >= 0 ? -1 : +1
                : arg >= 0 ? +1 : -1;
        }

        // -------------------------------------------------------------------------

        public static float randFloat(Random r, float min = 0.0f)
        {
            return (float)r.NextDouble() + min;
        }

        // -------------------------------------------------------------------------

        // Randomly return -1, 0 or +1
        public static int random101(Random r)
        {
            return r.Next(3) - 1;
        }

        // -------------------------------------------------------------------------

        // Randomly return true or false
        public static bool randomBool(Random r)
        {
            return r.Next(2) == 0;
        }

        // -------------------------------------------------------------------------

        // Randomly return a chance: [min] out of [max]
        public static bool randomChance(Random r, int min, int max)
        {
            return r.Next(max) < min;
        }

        // -------------------------------------------------------------------------

        public static float getRandomNum(float min, float max, float step, Random r)
        {
            float rrr = min + step * r.Next((int)max - (int)min);

            float a = 0.01f * (r.Next(100 - 0) + 0);

            return r.Next(2) == 0 ? 1 : -1;
        }

        // -------------------------------------------------------------------------

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

        // -------------------------------------------------------------------------

        public static void swap<Type>(ref Type a, ref Type b)
        {
            Type tmp = a;
            a = b;
            b = tmp;

            return;
        }

        // -------------------------------------------------------------------------

    };
};
