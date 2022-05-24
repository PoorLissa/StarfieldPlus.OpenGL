using System;



namespace my
{
    public class myUtils
    {
        // -------------------------------------------------------------------------

        // Randomly return +1 or -1
        public static int randomSign(Random r)
        {
            return r.Next(2) == 0 ? 1 : -1;
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
            return r.Next(max) <= min;
        }

        // -------------------------------------------------------------------------

        public static float getRandomNum(float min, float max, float step, Random r)
        {
            float rrr = min + step * r.Next((int)max - (int)min);

            float a = 0.01f * (r.Next(100 - 0) + 0);

            return r.Next(2) == 0 ? 1 : -1;
        }

        // -------------------------------------------------------------------------

    };
};
