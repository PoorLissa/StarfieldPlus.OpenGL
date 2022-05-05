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

        protected override void Process(Window window)
        {
            uint cnt = 0;

            myPrimitive.init_Rectangle();

            uint instanceVBO = 0, quadVAO = 0, quadVBO = 0;

            aaa1(ref instanceVBO, ref quadVAO, ref quadVBO);

            uint program = 0;

            CreateProgram_Instanced(ref program);

            while (!Glfw.WindowShouldClose(window))
            {
                cnt++;

                processInput(window);

                Glfw.SwapBuffers(window);
                Glfw.PollEvents();

                glClearColor(0, 0, 0, 1);
                glClear(GL_COLOR_BUFFER_BIT);

                glUseProgram(program);

                trans[1] += (float)(System.Math.Cos(cnt/10))/500;
                aaa2(ref instanceVBO);

                //glBindVertexArray(quadVAO);
                glDrawArraysInstanced(GL_TRIANGLES, 0, 6, 3);


#if false
                //myPrimitive._Rectangle.SetAngle(cnt/25);
                myPrimitive._Rectangle.SetAngle(0);
                myPrimitive._Rectangle.SetColor(1, 0, 0, 1);
                myPrimitive._Rectangle.Draw(666, 666, 222, 222, false);

                myPrimitive._Rectangle.SetAngle(0);
                myPrimitive._Rectangle.SetColor(1, 0, 0, 1);
                myPrimitive._Rectangle.Draw(1200, 666, 222, 222, true);
#endif

                System.Threading.Thread.Sleep(t);
            }

            return;
        }
    }
};
