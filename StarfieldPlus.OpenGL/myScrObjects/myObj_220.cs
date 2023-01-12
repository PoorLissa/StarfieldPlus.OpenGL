using GLFW;
using static OpenGL.GL;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections.Generic;


/*
    - Falling lines, Matrix-Style
*/


namespace my
{
    public class myObj_220 : myObject
    {
        private int lifeCounter = 0, type = 0;
        private float x, y, Rad, rad, time1 = 0, dt1 = 0, time2 = 0, dt2 = 0, R, G, B, A;

        private static int N = 1;
        private static float baseDt = 1.0f, dimAlpha = 0.025f;
        private static bool doOscillateDimRate = false;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_220()
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

            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time local initialization
        private void initLocal()
        {
            N = 3333;
            renderDelay = 10;

            doOscillateDimRate = myUtils.randomBool(rand);
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            string str = $"Obj = myObj_220\n\n" +
                            $"N = {list.Count} of {N}\n" +
                            $"renderDelay = {renderDelay}\n" +
                            $"doOscillateDimRate = {doOscillateDimRate}\n" +
                            $"file: {colorPicker.GetFileName()}"
                ;
            return str;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void generateNew()
        {
            x = rand.Next(gl_Width);
            y = rand.Next(gl_Height + 333) - 333;

            colorPicker.getColor(x, y, ref R, ref G, ref B);
            A = myUtils.randFloat(rand, 0.1f);

            lifeCounter = 100 + rand.Next(200);

            type = rand.Next(3);

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

            dt1 += 0.00001f * rand.Next(10);

            rad = (int)(Rad * Math.Sin(time2));

            x += rand.Next(3) - 1;
            y += rand.Next((int)Rad/2);

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
                    myPrimitive._Line.SetAngle((float)rand.NextDouble() * rand.Next(123));
                    myPrimitive._Line.Draw(x - rand.Next(33), y, x + rand.Next(33), y);
                    break;

                case 1:
                    myPrimitive._Line.SetAngle(time1);
                    myPrimitive._Line.Draw(x - rad, y, x + rad, y);
                    break;

                case 2:
                    myPrimitive._Line.SetAngle(time1);
                    myPrimitive._Line.Draw(x, y, x + rad, y);
                    break;
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Process(Window window)
        {
            uint cnt = 0;

            myPrimitive.init_Rectangle();
            myPrimitive.init_Line();

            doClearBuffer = false;

            if (doClearBuffer == false)
            {
                dimScreenRGB_Set(myUtils.randFloat(rand) / 11, myUtils.randFloat(rand) / 11, myUtils.randFloat(rand) / 11);
                glDrawBuffer(GL_FRONT_AND_BACK);
            }
    
            while (!Glfw.WindowShouldClose(window))
            {
                cnt++;

                processInput(window);

                // Swap fore/back framebuffers, and poll for operating system events.
                Glfw.SwapBuffers(window);
                Glfw.PollEvents();

                foreach (myObj_220 obj in list)
                {
                    obj.Show();
                    obj.Move();
                }

                System.Threading.Thread.Sleep(renderDelay);

                if (list.Count < N)
                {
                    list.Add(new myObj_220());
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
    }
};
