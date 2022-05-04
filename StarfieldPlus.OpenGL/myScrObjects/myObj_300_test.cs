using GLFW;
using static OpenGL.GL;
using System.Collections.Generic;



namespace my
{
    public class myObj_300_test : myObject
    {
        private static bool doClearBuffer = false, doUseRotation = false, doFillShapes = false;
        private static int x0, y0, shapeType = 0, moveType = 0, t = 25, N = 1, gravityRate = 0;
        private static float dimAlpha = 0.1f;

        private float x, y, R, G, B, A, dA, lineTh;

        private int shape = 0, lifeCounter = 0, lifeMax = 0, objN = 0;

        private List<myObj_300_Struct> structsList = null;

        // -------------------------------------------------------------------------

        public myObj_300_test()
        {
            if (colorPicker == null)
            {
                x0 = gl_Width  / 2;
                y0 = gl_Height / 2;
            }
        }

        protected override void Process(Window window)
        {
            uint cnt = 0;

            myPrimitive.init_Rectangle();

            while (!Glfw.WindowShouldClose(window))
            {
                cnt++;

                processInput(window);

                Glfw.SwapBuffers(window);
                Glfw.PollEvents();

                glClearColor(0, 0, 0, 1);
                glClear(GL_COLOR_BUFFER_BIT);

                //myPrimitive._Rectangle.SetAngle(cnt/25);
                myPrimitive._Rectangle.SetAngle(0);
                myPrimitive._Rectangle.SetColor(1, 0, 0, 1);
                myPrimitive._Rectangle.Draw(666, 666, 222, 222, false);

                myPrimitive._Rectangle.SetAngle(0);
                myPrimitive._Rectangle.SetColor(1, 0, 0, 1);
                myPrimitive._Rectangle.Draw(1200, 666, 222, 222, true);

                System.Threading.Thread.Sleep(t);
            }

            return;
        }
    }
};
