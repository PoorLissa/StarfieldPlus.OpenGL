using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;


/*
    - Falling lines, Matrix-Style
*/


namespace my
{
    public class myObj_0220 : myObject
    {
        // Priority
        public static int Priority => 10;
		public static System.Type Type => typeof(myObj_0220);

        private int lifeCounter = 0, type = 0;
        private float x, y, Rad, rad;
        private float time1 = 0, dt1 = 0, dt1Factor = 0, time2 = 0, dt2 = 0;
        private float R, G, B, A;

        private static int N = 1;
        private static float baseDt = 1.0f, dimAlpha = 0.025f, lineTh = 1.0f;
        private static bool doOscillateDimRate = false;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_0220()
        {
            dt1 = baseDt * (rand.Next(1000) + 1);                   // Rotation
            dt2 = baseDt * 0.1f * (rand.Next(100) + 1);             // Radius

            dt1 *= myUtils.randomSign(rand);
            dt2 *= myUtils.randomSign(rand);

            Rad = rand.Next(50) + 10;

            generateNew();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time global initialization
        protected override void initGlobal()
        {
            colorPicker = new myColorPicker(gl_Width, gl_Height);
            list = new List<myObject>();

            doClearBuffer = false;

            N = 1111 + rand.Next(2345);
            renderDelay = rand.Next(10);

            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time local initialization
        private void initLocal()
        {
            doOscillateDimRate = myUtils.randomBool(rand);

            lineTh = myUtils.randFloat(rand, 0.1f) * (rand.Next(5) + 1);
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            string str = $"Obj = {Type}\n\n"                             	+
                            $"N = {list.Count} of {N}\n"                    +
                            $"doClearBuffer = {doClearBuffer}\n"            +
                            $"doOscillateDimRate = {doOscillateDimRate}\n"  +
                            $"lineTh = {lineTh}\n"                          +
                            $"renderDelay = {renderDelay}\n"                +
                            $"file: {colorPicker.GetFileName()}"
                ;
            return str;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void setNextMode()
        {
            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void generateNew()
        {
            x = rand.Next(gl_Width);
            y = rand.Next(gl_Height + 333) - 333;

            colorPicker.getColor(x, y, ref R, ref G, ref B);
            A = myUtils.randFloat(rand, 0.1f);

            lifeCounter = rand.Next(200) + 100;
            type = rand.Next(4);

            switch (type)
            {
                case 3:
                    dt1 = myUtils.randomSign(rand) * myUtils.randFloat(rand) * 0.1f;
                    dt1Factor = myUtils.randFloat(rand) * rand.Next(30);
                    break;
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            lifeCounter--;

            if (lifeCounter < 0)
            {
                A -= 0.1f;

                if (A <= 0)
                {
                    generateNew();
                }
            }

            time1 += dt1;
            time2 += dt2;

            switch (type)
            {
                case 0:
                case 1:
                case 2:
                    dt1 += 0.00001f * rand.Next(10);
                    break;
            }

            rad = (int)(Rad * Math.Sin(time2));

            x += rand.Next(3) - 1;
            y += rand.Next((int)Rad/2);

/*
            // Anomaly
            if (y > 500 && x > gl_x0 - 333 && x < gl_x0)
            {
                x -= 12;
            }

            if (y > 500 && x < gl_x0 + 333 && x > gl_x0)
            {
                x += 12;
            }
*/

            if (y > gl_Height + 333)
            {
                generateNew();
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Show()
        {
            myPrimitive._Line.SetColor(R, G, B, A);

            switch (type)
            {
                case 0:
                    //for (int i = 0; i < 33; i++)
                    {
                        myPrimitive._Line.SetAngle((float)rand.NextDouble() * rand.Next(123));
                        myPrimitive._Line.Draw(x - rand.Next(33), y, x + rand.Next(33), y, lineTh);
                    }
                    break;

                case 1:
                    myPrimitive._Line.SetAngle(time1);
                    myPrimitive._Line.Draw(x - rad, y, x + rad, y, lineTh);
                    break;

                case 2:
                    myPrimitive._Line.SetAngle(time1);
                    myPrimitive._Line.Draw(x, y, x + rad, y, lineTh);
                    break;

                case 3:
                    myPrimitive._Line.SetAngle((float)Math.Sin(time1 * dt1Factor));
                    myPrimitive._Line.Draw(x - rad, y, x + rad, y, lineTh);
                    break;
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Process(Window window)
        {
            uint cnt = 0;

            initShapes();

            if (doClearBuffer == false)
            {
                dimScreenRGB_SetRandom(0.1f, ligtmMode: myUtils.randomChance(rand, 1, 11));
                glDrawBuffer(GL_FRONT_AND_BACK);
            }

            while (!Glfw.WindowShouldClose(window))
            {
                cnt++;

                processInput(window);

                // Swap fore/back framebuffers, and poll for operating system events.
                Glfw.SwapBuffers(window);
                Glfw.PollEvents();

                foreach (myObj_0220 obj in list)
                {
                    obj.Show();
                    obj.Move();
                }

                System.Threading.Thread.Sleep(renderDelay);

                if (list.Count < N)
                {
                    list.Add(new myObj_0220());
                }

                // Dim the screen constantly
                if (doClearBuffer == false)
                {
                    if (cnt % 250 == 0)
                    {
                        dimScreenRGB_Adjust(0.05f);
                    }

                    if (doOscillateDimRate)
                    {
                        dimScreen(dimAlpha + (float)Math.Sin(0.001f * cnt) / 100, false);
                    }
                    else
                    {
                        dimScreen(dimAlpha, false);
                    }
                }
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
    }
};
