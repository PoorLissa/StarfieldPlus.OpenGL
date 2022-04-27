using System;
using System.Drawing;

#if false

namespace my
{
    public class distributionTester : myObject
    {
        //private int dx, dy, a, oldx, oldy, iterCounter;
        //private bool isStatic = false;

        //private static int moveMode = 0, maxA = 33, maxSize = 0, spd = 0, divider = 0, t = 0;
        //private static float moveConst = 0;
        //private static bool showStatics = false;

        public distributionTester()
        {
            if (colorPicker == null)
            {
                p = new Pen(Color.White);
                br = new SolidBrush(Color.White);
                f = new Font("Segoe UI", 8, FontStyle.Regular, GraphicsUnit.Point);
                colorPicker = new myColorPicker(Width, Height);
            }
        }

        // -------------------------------------------------------------------------

        protected override void Process()
        {
            int cnt = 0, x = 0;

            g.FillRectangle(Brushes.Black, 0, 0, Width, Height);

            var list = new System.Collections.Generic.List<int>();

            Count = 1000;

            while (list.Count < Count)
            {
                list.Add(0);
            }

            g.DrawLine(Pens.Green, 1000 - 2, 1000, 1000 - 2, 10);
            g.DrawLine(Pens.Green, 1000 + Count + 2, 1000, 1000 + Count + 2, 10);

            while (isAlive)
            {
                //x = rand.Next(Count/2);
                //x = rand.Next(Count/2) + rand.Next(Count / 2);                        // Triangle
                //x = rand.Next(Count/3) + rand.Next(2*Count/3);
                //x = rand.Next(Count/40) * rand.Next(Count/40);

                //x = rand.Next(Count/3) + rand.Next(Count/3) + rand.Next(Count/3);     // Gaussian

                int n = 3;
                x = 0;

                for (int i = 0; i < n; i++)
                {
                    x += rand.Next(Count/n);

                    if (rand.Next(2) == 0)
                    {
                        x -= 2*rand.Next(x)/3;
                    }
                }



                list[x]++;
                int y = list[x];

                g.DrawLine(Pens.Red, 1000 + x, 1000, 1000 + x, 1000 - y);

                if (cnt % 100 == 0)
                {
                    form.Invalidate();
                    System.Threading.Thread.Sleep(1);
                }

                cnt++;
            }

            return;
        }
    };
};

#endif
