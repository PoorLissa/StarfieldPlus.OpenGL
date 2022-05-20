using GLFW;
using static OpenGL.GL;
using System.Collections.Generic;



namespace my
{
    public class myObj_300_test : myObject
    {
        private static int x0, y0, t = 25;

        // -------------------------------------------------------------------------

        public myObj_300_test()
        {
            if (colorPicker == null)
            {
                x0 = gl_Width  / 2;
                y0 = gl_Height / 2;
            }
        }

        static float[] trans = new float[] { 0, 0, 0.15f, 0, 0.3f, 0 };

        unsafe void aaa1(ref uint instanceVBO, ref uint quadVAO, ref uint quadVBO)
        {
            float[] quadVertices = new float[] {
                        // positions     // colors
                        -0.05f,  0.05f,  1.0f, 0.0f, 0.0f,
                         0.05f, -0.05f,  0.0f, 1.0f, 0.0f,
                        -0.05f, -0.05f,  0.0f, 0.0f, 1.0f,

                        -0.05f,  0.05f,  1.0f, 0.0f, 0.0f,
                         0.05f, -0.05f,  0.0f, 1.0f, 0.0f,
                         0.05f,  0.05f,  0.0f, 1.0f, 1.0f
            };

            instanceVBO = glGenBuffer();
            quadVAO = glGenVertexArray();
            quadVBO = glGenBuffer();

            glBindBuffer(GL_ARRAY_BUFFER, instanceVBO);

            fixed (float * t = &trans[0])
                glBufferData(GL_ARRAY_BUFFER, sizeof(float) * 6, t, GL_STATIC_DRAW);

            glBindVertexArray(quadVAO);
            glBindBuffer(GL_ARRAY_BUFFER, quadVBO);

            fixed (float* qv = &quadVertices[0])
                glBufferData(GL_ARRAY_BUFFER, sizeof(float) * quadVertices.Length, qv, GL_STATIC_DRAW);

            glEnableVertexAttribArray(0);
            glVertexAttribPointer(0, 2, GL_FLOAT, false, 5 * sizeof(float), (void*)0);

            glEnableVertexAttribArray(1);
            glVertexAttribPointer(1, 3, GL_FLOAT, false, 5 * sizeof(float), (void*)(2 * sizeof(float)));

            // also set instance data
            glEnableVertexAttribArray(2);
            glBindBuffer(GL_ARRAY_BUFFER, instanceVBO); // this attribute comes from a different vertex buffer
            glVertexAttribPointer(2, 2, GL_FLOAT, false, 2 * sizeof(float), NULL);
            //glBindBuffer(GL_ARRAY_BUFFER, 0);
            glVertexAttribDivisor(2, 1); // tell OpenGL this is an instanced vertex attribute.
        }

        unsafe void aaa2(ref uint instanceVBO)
        {
            glBindBuffer(GL_ARRAY_BUFFER, instanceVBO);

            fixed (float* t = &trans[0])
                glBufferData(GL_ARRAY_BUFFER, sizeof(float) * 6, t, GL_STATIC_DRAW);
        }

        private static void CreateProgram_Instanced(ref uint program)
        {
            var vertex = myOGL.CreateShader(GL_VERTEX_SHADER,
                @"#version 330 core
                    layout (location = 0) in vec2 aPos;
                    layout (location = 1) in vec3 aColor;
                    layout (location = 2) in vec2 aOffset;

                    out vec3 fColor;

                    void main()
                    {
                        fColor = aColor;
                        gl_Position = vec4(aPos + aOffset, 0.0, 1.0);
                    }"
            );

            var fragment = myOGL.CreateShaderEx(GL_FRAGMENT_SHADER,
                "out vec4 result; in vec3 fColor;",
                    main: "result = vec4(fColor, 1.0);"
            );

            program = glCreateProgram();
            glAttachShader(program, vertex);
            glAttachShader(program, fragment);

            glLinkProgram(program);

            glDeleteShader(vertex);
            glDeleteShader(fragment);

            glUseProgram(program);
        }

        // ---------------------------------------------------------------------------------------------------------------------

        protected override void Process(Window window)
        //protected void Process123(Window window)
        {
            uint cnt = 0;

            myPrimitive.init_Triangle();
            myPrimitive.init_Rectangle();
            myPrimitive.init_Ellipse();

            var rInst = new myRectangleInst(10);
            var tInst = new myTriangleInst(10);
            var eInst = new myEllipseInst(10);

            while (!Glfw.WindowShouldClose(window))
            {
                processInput(window);

                Glfw.SwapBuffers(window);
                Glfw.PollEvents();

                glClearColor(0, 0, 0, 1);
                glClear(GL_COLOR_BUFFER_BIT);

                eInst.setRotationMode(2);
                eInst.ResetBuffer();

                eInst.setInstanceCoords(666, 666, 666, 1);
                eInst.setInstanceColor(1, 0.25f, 0.25f, 0.85f);

                eInst.setInstanceCoords(666, 666, 333, 1);
                eInst.setInstanceColor(1, 0.25f, 0.25f, 0.85f);

                eInst.setInstanceCoords(666, 666, 111, 1);
                eInst.setInstanceColor(1, 0.25f, 0.25f, 0.85f);

                eInst.updateInstances();

                eInst.SetColorA(-0.5f);
                eInst.Draw(true);

                eInst.SetColorA(0);
                eInst.Draw(false);

                myPrimitive._Rectangle.SetColor(1, 1, 1, 1);
                myPrimitive._Rectangle.Draw(666, 666, 666, 666);


                System.Threading.Thread.Sleep(11);
                cnt++;
            }
        }

        //protected override void Process(Window window)
        protected void Process1(Window window)
        {
            uint cnt = 0;

            List<float> listInst = new List<float>();

            myPrimitive.init_Triangle();
            myPrimitive.init_Rectangle();
            myPrimitive.init_Pentagon();
            myPrimitive.init_Hexagon();
            myPrimitive.init_Ellipse();

            var rInst = new myRectangleInst(10);
            var tInst = new myTriangleInst(10);
            var eInst = new myEllipseInst(10);

            uint instanceVBO = 0, quadVAO = 0, quadVBO = 0;
            aaa1(ref instanceVBO, ref quadVAO, ref quadVBO);
            uint program = 0;

            bool mySpeedTest1 = false;
            bool mySpeedTest2 = false;
            bool myTestShapes = false;
            bool myInstanceTest = true;

            CreateProgram_Instanced(ref program);

            while (!Glfw.WindowShouldClose(window))
            {
                processInput(window);

                Glfw.SwapBuffers(window);
                Glfw.PollEvents();

                glClearColor(0, 0, 0, 1);
                glClear(GL_COLOR_BUFFER_BIT);

                if (myTestShapes)
                {
                    myPrimitive._Rectangle.SetAngle(cnt / 25);
                    //myPrimitive._Rectangle.SetAngle(0);
                    myPrimitive._Rectangle.SetColor(1, 0, 0, 1);
                    myPrimitive._Rectangle.Draw(666, 666, 222, 222, false);

                    myPrimitive._Rectangle.SetAngle(0);
                    myPrimitive._Rectangle.SetColor(1, 0, 0, 0.3f);
                    myPrimitive._Rectangle.Draw(1200, 666, 233, 233, true);

                    myPrimitive._Pentagon.SetColor(1, 0, 1, 0.15f);
                    myPrimitive._Pentagon.Draw(x0, y0, 333, true);

                    myPrimitive._Pentagon.SetColor(1, 0, 1, 1);
                    myPrimitive._Pentagon.Draw(x0, y0, 366, false);

                    myPrimitive._Ellipse.SetColor(1, 1, 0, 1);
                    myPrimitive._Ellipse.Draw(x0, y0, 222, 222, false);

                    myPrimitive._Rectangle.SetAngle(0);
                    myPrimitive._Rectangle.SetColor(1, 0, 0, 0.9f);
                    myPrimitive._Rectangle.Draw(1200, 666, 233, 233, false);

                    myPrimitive._Ellipse.SetColor(1, 1, 0, 0.25f);
                    myPrimitive._Ellipse.Draw(x0 + 333, y0, 222, 222, true);

                    myPrimitive._Ellipse.SetColor(1, 1, 0, 0.9f);
                    myPrimitive._Ellipse.Draw(x0 + 333, y0, 222, 222, false);
                }

                if (mySpeedTest1)
                {
#if false
                    t = 0;
                    myPrimitive._Rectangle.SetAngle(0);
                    myPrimitive._Rectangle.SetColor(1, 0, 0, 0.25f);

                    // old0: 10k = ~35fps
                    // new1: 10k = ~48fps
                    for (int i = 0; i < 30000; i++)
                    {
                        int x = rand.Next(gl_Width);
                        int y = rand.Next(gl_Height);

                        myPrimitive._Rectangle.Draw(x, y, 50, 50, true);
                    }

#else
                    // 450k = ~75fps
                    t = 0;
                    int n = 450000;
                    rInst.ResetBuffer();
                    rInst.Resize(n);

                    for (int i = 0; i < n; i++)
                    {
                        int x = rand.Next(gl_Width);
                        int y = rand.Next(gl_Height);

                        rInst.setInstanceCoords(x, y, 50, 50);
                        rInst.setInstanceColor(1, 0, 0, 0.25f);
                        rInst.setInstanceAngle(0);
                    }

                    rInst.updateInstances();
                    rInst.Draw(true);

#endif
                }

                if (myInstanceTest)
                {
#if false
                    glUseProgram(program);

                    trans[1] += (float)(System.Math.Cos(cnt/10))/500;
                    aaa2(ref instanceVBO);

                    //glBindVertexArray(quadVAO);
                    glDrawArraysInstanced(GL_TRIANGLES, 0, 6, 3);

#else
                    // --- my instancing ----------------------

                    if (cnt % 100 == 0)
                    {
                    }

                    myPrimitive._Rectangle.SetAngle(0);
                    myPrimitive._Rectangle.SetColor(1, 1, 1, 1);
                    myPrimitive._Rectangle.Draw(1000 - 100, 500 - 100, 200, 200, false);

                    eInst.ResetBuffer();
                    eInst.Resize(11);

                    eInst.setInstanceCoords(gl_Width / 2 - 150, gl_Height / 2 - 150, 300, 300);
                    eInst.setInstanceColor(1, 0, 1, 1);

                    eInst.updateInstances();
                    eInst.Draw(false);




                    tInst.ResetBuffer();
                    tInst.Resize(11);

                    tInst.setInstanceCoords(1000, 500, 100, cnt * 0.01f);
                    tInst.setInstanceColor(1, 0, 0, 1);

                    tInst.setInstanceCoords(1000, 500, 3, 0);
                    tInst.setInstanceColor(0, 1, 0, 1);

                    tInst.updateInstances();
                    tInst.Draw(false);



                    rInst.ResetBuffer();
                    rInst.Resize(11);

                    // set instance 1
                    rInst.setInstanceCoords(333, 333, 130, 130);
                    rInst.setInstanceColor(1, 0.1f, 0.1f, 1);
                    rInst.setInstanceAngle(cnt * 0.01f);

                    // set instance 2
                    rInst.setInstanceCoords(gl_Width/2 - 50, gl_Height/2 - 50, 100, 100);
                    rInst.setInstanceColor(1, 0.5f, 0.5f, 1);
                    rInst.setInstanceAngle(cnt * 0.023f);

                    // set instance 3
                    rInst.setInstanceCoords(666, 666, 111, 111);
                    rInst.setInstanceColor(1, 0.5f, 0.5f, 1);
                    rInst.setInstanceAngle(cnt * 0.23f);

                    // update and draw all the instances
                    rInst.updateInstances();
                    rInst.Draw(false);

                    myPrimitive._Rectangle.SetAngle(0);
                    myPrimitive._Rectangle.SetColor(1, 1, 1, 1);
                    //myPrimitive._Rectangle.Draw(gl_Width/2 - 75, gl_Height/2 - 75, 150, 150, false);
                    //myPrimitive._Rectangle.Draw(325, 325, 127, 127, false);
                    myPrimitive._Rectangle.Draw(323, 323, 150, 150, false);
#endif
                }

#if false
                //myPrimitive._Rectangle.SetAngle(cnt/25);
                myPrimitive._Rectangle.SetAngle(0);
                myPrimitive._Rectangle.SetColor(1, 0, 0, 1);
                myPrimitive._Rectangle.Draw(666, 666, 222, 222, false);

                myPrimitive._Rectangle.SetAngle(0);
                myPrimitive._Rectangle.SetColor(1, 0, 0, 1);
                myPrimitive._Rectangle.Draw(1200, 666, 222, 222, true);
#endif

                // todo: test which is faster
                if (mySpeedTest2)
                {
                    t = 0;
                    int n = 4000;

#if true
                    // old ellipse
                    for (int i = 0; i < n; i++)
                    {
                        int x = rand.Next(gl_Width);
                        int y = rand.Next(gl_Height);

                        myPrimitive._Ellipse.SetColor(1, 0, 0, 0.2f);
                        myPrimitive._Ellipse.Draw(x, y, 50, 50, true);

                        myPrimitive._Ellipse.SetColor(1, 0, 0, 1);
                        myPrimitive._Ellipse.Draw(x, y, 50, 50, false);
                    }
#else
                    // new ellipse
                    for (int i = 0; i < n; i++)
                    {
                        int x = rand.Next(gl_Width);
                        int y = rand.Next(gl_Height);
                    }
#endif
                }

                System.Threading.Thread.Sleep(t);
                cnt++;
            }

            return;
        }
    }
};
