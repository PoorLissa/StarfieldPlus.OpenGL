using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;


/*
    - try 3d
*/


namespace my
{
    public class myObj_1560 : myObject
    {
        // Priority
        public static int Priority => 10;
		public static System.Type Type => typeof(myObj_1560);

        private float x, y, z, dz;
        private float size, sizeFactor, sizeFactorZ, A, R, G, B;
        private float theta = 0, dTheta = 0;

        private static int N = 0, shape = 0;
        private static bool doFillShapes = false;
        private static float dimAlpha = 0.05f;

        private static myScreenGradient grad = null;

        private static float[] model_cube = null;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_1560()
        {
            if (id != uint.MaxValue)
                generateNew();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time global initialization
        protected override void initGlobal()
        {
            colorPicker = new myColorPicker(gl_Width, gl_Height);
            list = new List<myObject>();

            // Global unmutable constants
            {
                model_cube = new float[] {
                    -0.5f, -0.5f, +0.5f,
                    +0.5f, -0.5f, +0.5f,
                    +0.5f, +0.5f, +0.5f,
                    -0.5f, +0.5f, +0.5f,
                    -0.5f, -0.5f, -0.5f,
                    +0.5f, -0.5f, -0.5f,
                    +0.5f, +0.5f, -0.5f,
                    -0.5f, +0.5f, -0.5f
                };

                N = 33;
                shape = rand.Next(5);
                shape = 2;
            }

            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time local initialization
        private void initLocal()
        {
            doClearBuffer = true;

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            height = 600;

            string str = $"Obj = {Type}\n\n"                  +
                            myUtils.strCountOf(list.Count, N) +
                            $"file: {colorPicker.GetFileName()}"
                ;
            return str;
        }

        // ---------------------------------------------------------------------------------------------------------------

        // 
        protected override void setNextMode()
        {
            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void generateNew()
        {
            x = myUtils.randFloatSigned(rand) * 5;
            y = myUtils.randFloatSigned(rand) * 4;
            z = 3;
            dz = 0.005f + myUtils.randFloat(rand) * 0.01f;

            size = 4;
            sizeFactor = 0.1f + myUtils.randFloat(rand) * 0.5f;
            sizeFactorZ = 2.0f + myUtils.randFloat(rand) * 5.0f;

            A = 1;
            R = 0.25f;
            G = 0.85f;
            B = 0.10f;

            colorPicker.getColor(x, y, ref R, ref G, ref B);

            dTheta = 0.001f + myUtils.randFloat(rand) * 0.02f;

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            //y += (float)Math.Cos(theta) * 0.01f;

            //theta += dTheta;

            z += dz;

            if (z > 10)
            {
                A -= 0.1f;

                if (A < 0)
                    generateNew();
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Show()
        {
            // https://www.youtube.com/watch?v=qjWkNZ0SXfo

            float sin = (float)Math.Sin(theta);
            float cos = (float)Math.Cos(theta);

            float[] screenVertices = new float[16];
            int j = 0;

            for (int i = 0; i < model_cube.Length; i += 3)
            {
                // Original coordinates from the model
                float x0 = model_cube[i + 0];
                float y0 = model_cube[i + 1];
                float z0 = model_cube[i + 2];

                if (true)
                {
                    x0 *= sizeFactor;
                    y0 *= sizeFactor;
                    z0 *= sizeFactor * sizeFactorZ;
                }

                // Apply rotation
                float x1 = x0 * cos - z0 * sin;
                float y1 = y0;
                float z1 = x0 * sin + z0 * cos;

                // ?
                if (true)
                {
                    x1 += x;
                    y1 += y;
                    z1 += z;
                }

                // Translate into screen space
                float x2 = (x1 / z1) * 1.0f * gl_Height / gl_Width;
                x2 = gl_Width * (1.0f + x2) * 0.5f;

                float y2 = y1 / z1;
                y2 = gl_Height * (1.0f + y2) * 0.5f;

                screenVertices[j++] = x2;
                screenVertices[j++] = y2;

                float size2x = size * 6 / Math.Abs(z1);

                // Instanced circles
                myPrimitive._EllipseInst.setInstanceCoords(x2, y2, size2x, 0);
                myPrimitive._EllipseInst.setInstanceColor(R, G, B, A);
            }

            int[] edges = new int[]{
                0, 1, 2, 3,
                2, 3, 4, 5,
                4, 5, 6, 7,
                6, 7, 0, 1,
                0, 1, 8, 9,
                8, 9, 10, 11,
                10, 11, 2, 3,
                10, 11, 12, 13,
                12, 13, 14, 15,
                14, 15, 8, 9,
                14, 15, 6, 7,
                12, 13, 4, 5
            };

            for (int i = 0; i < edges.Length; i += 4)
            {
                float x1 = screenVertices[edges[i + 0]];
                float y1 = screenVertices[edges[i + 1]];
                float x2 = screenVertices[edges[i + 2]];
                float y2 = screenVertices[edges[i + 3]];

                myPrimitive._LineInst.setInstanceCoords(x1, y1, x2, y2);
                myPrimitive._LineInst.setInstanceColor(R, G, B, 0.25f);
            }

/*
            float x2 = x * cos - z * sin;
            float y2 = y;
            float z2 = x * sin + z * cos;

            z2 += 3;

            //x = x * cos - z * sin;
            //z = x * sin + z * cos;

            float x1 = (x2 / z2) * 1.0f * gl_Height / gl_Width;
            x1 = gl_Width * (1.0f + x1) * 0.5f;

            float y1 = y2 / z2;
            y1 = gl_Height * (1.0f + y1) * 0.5f;

            // Instanced circles
            myPrimitive._EllipseInst.setInstanceCoords(x1, y1, size2x, 0);
            myPrimitive._EllipseInst.setInstanceColor(R, G, B, A);
*/

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Process(Window window)
        {
            uint cnt = 0;
            initShapes();

            clearScreenSetup(doClearBuffer, 0.1f);

            stopwatch = new StarfieldPlus.OpenGL.myUtils.myStopwatch(true);

            while (!Glfw.WindowShouldClose(window))
            {
                int Count = list.Count;

                processInput(window);

                // Swap fore/back framebuffers, and poll for operating system events.
                Glfw.SwapBuffers(window);
                Glfw.PollEvents();

                // Dim screen
                {
                    if (doClearBuffer)
                    {
                        glClear(GL_COLOR_BUFFER_BIT);
                        grad.Draw();
                    }
                    else
                    {
                        dimScreen(dimAlpha);
                    }
                }

                // Render Frame
                {
                    inst.ResetBuffer();
                    myPrimitive._LineInst.ResetBuffer();

                    for (int i = 0; i != Count; i++)
                    {
                        var obj = list[i] as myObj_1560;

                        obj.Show();
                        obj.Move();
                    }

                    myPrimitive._LineInst.Draw();

                    if (doFillShapes)
                    {
                        // Tell the fragment shader to multiply existing instance opacity by 0.5:
                        inst.SetColorA(-0.5f);
                        inst.Draw(true);
                    }

                    // Tell the fragment shader to do nothing with the existing instance opacity:
                    inst.SetColorA(0);
                    inst.Draw(false);
                }

                if (Count < N)
                {
                    list.Add(new myObj_1560());
                }

                stopwatch.WaitAndRestart();
                cnt++;
                theta += dTheta;
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void initShapes()
        {
            myPrimitive.init_ScrDimmer();
            base.initShapes(shape, N * model_cube.Length, 0);

            myPrimitive.init_LineInst(N * 12);
            myPrimitive._LineInst.setLineWidth(3);
            myPrimitive._LineInst.setAntialized(true);

            grad = new myScreenGradient();
            grad.SetRandomColors(rand, 0.2f);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------
    }
};
