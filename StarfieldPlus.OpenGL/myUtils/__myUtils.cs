using System;



namespace my
{
    public class myUtils
    {
        // -------------------------------------------------------------------------

        // Randomly return +1 or -1
        public static int getRandomSign(Random r)
        {
            return r.Next(2) == 0 ? 1 : -1;
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
