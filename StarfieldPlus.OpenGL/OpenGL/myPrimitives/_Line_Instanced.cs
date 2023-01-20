using GLFW;
using static OpenGL.GL;
using System;


public class myLineInst : myInstancedPrimitive
{
    private int autoDrawCnt;

    private static float[] vertices = null;

    private static uint vbo = 0, instVbo = 0, shaderProgram = 0;
    private static int locationScrSize = 0;
    private static int floatTimesN = 0;
    private static float _angle = 0;

    // -------------------------------------------------------------------------------------------------------------------

    public myLineInst(int maxInstCount)
    {
        // Number of elements in [instanceArray] that define one single instance:
        // - 4 floats for Coordinates (x1, y1, x2, y2)
        // - 4 floats for RGBA
        n = 8;

        if (vertices == null)
        {
            N = 0;

            vertices = new float[4];
            instanceArray = new float[maxInstCount * n];

            autoDrawCnt = maxInstCount * n - 8;
            floatTimesN = sizeof(float) * n;

            vertices[0] = -0.5f;
            vertices[1] = +0.0f;
            vertices[2] = +0.5f;
            vertices[3] = +0.0f;

            CreateProgram();
            glUseProgram(shaderProgram);
            locationScrSize = glGetUniformLocation(shaderProgram, "myScrSize");

            vbo = glGenBuffer();
            instVbo = glGenBuffer();
        }
    }

    // -------------------------------------------------------------------------------------------------------------------

    public void Draw()
    {
        updateInstances();
        updateVertices();

        glUseProgram(shaderProgram);
        updUniformScreenSize(locationScrSize, Width, Height);

        glPolygonMode(GL_FRONT_AND_BACK, GL_LINE);
        glDrawArraysInstanced(GL_LINES, 0, 2, N);
    }

    // -------------------------------------------------------------------------------------------------------------------

    private static void CreateProgram()
    {
        // Use gl_VertexID to be able to address the first or the second pair of coordinates
        var vertex = myOGL.CreateShaderEx(GL_VERTEX_SHADER,
            @"layout (location = 0) in vec3 pos;
              layout (location = 1) in mat2x4 mData;
                uniform ivec2 myScrSize;
                out vec4 rgbaColor;",

                    main: @"rgbaColor = mData[1];

                            int idx = gl_VertexID * 2;

                            float realSizeX = +2.0 / myScrSize.x;
                            float realSizeY = -2.0 / myScrSize.y;

                            gl_Position = vec4(realSizeX * mData[0][idx] - 1.0, realSizeY * mData[0][idx+1] + 1.0, 1.0, 1.0);"
        );

        var fragment = myOGL.CreateShaderEx(GL_FRAGMENT_SHADER,
                "in vec4 rgbaColor; out vec4 result;",
                    main: @"result = rgbaColor;"
        );

        shaderProgram = glCreateProgram();

        glAttachShader(shaderProgram, vertex);
        glAttachShader(shaderProgram, fragment);

        glLinkProgram(shaderProgram);

        glDeleteShader(vertex);
        glDeleteShader(fragment);

        return;
    }

    // -------------------------------------------------------------------------------------------------------------------

    public void setInstanceCoords(float x1, float y1, float x2, float y2)
    {
        instanceArray[instArrayPosition++] = x1;
        instanceArray[instArrayPosition++] = y1;
        instanceArray[instArrayPosition++] = x2;
        instanceArray[instArrayPosition++] = y2;
    }

    // -------------------------------------------------------------------------------------------------------------------

    public void setInstanceColor(float r, float g, float b, float a)
    {
        instanceArray[instArrayPosition++] = r;
        instanceArray[instArrayPosition++] = g;
        instanceArray[instArrayPosition++] = b;
        instanceArray[instArrayPosition++] = a;
    }

    // -------------------------------------------------------------------------------------------------------------------

    public void setInstanceColor(double r, double g, double b, double a)
    {
        instanceArray[instArrayPosition++] = (float)r;
        instanceArray[instArrayPosition++] = (float)g;
        instanceArray[instArrayPosition++] = (float)b;
        instanceArray[instArrayPosition++] = (float)a;
    }

    // -------------------------------------------------------------------------------------------------------------------

    // Single-call Draw;
    // Draws to screen automatically, when the buffer is about to be overflown
    public void autoDraw(float x1, float y1, float x2, float y2, float r, float g, float b, float a)
    {
        if (instArrayPosition > autoDrawCnt)
        {
            Draw();
            ResetBuffer();
        }

        instanceArray[instArrayPosition++] = x1;
        instanceArray[instArrayPosition++] = y1;
        instanceArray[instArrayPosition++] = x2;
        instanceArray[instArrayPosition++] = y2;

        instanceArray[instArrayPosition++] = r;
        instanceArray[instArrayPosition++] = g;
        instanceArray[instArrayPosition++] = b;
        instanceArray[instArrayPosition++] = a;

        return;
    }

    // -------------------------------------------------------------------------------------------------------------------

    // Move vertices data from CPU to GPU -- needs to be called each time we change the Rectangle's coordinates
    private static unsafe void updateVertices()
    {
        // Bind a buffer;
        // From now on, all the operations on this type of buffer will be performed on the buffer we just bound;
        glBindBuffer(GL_ARRAY_BUFFER, vbo);
        {
            fixed (float* v = &vertices[0])
                glBufferData(GL_ARRAY_BUFFER, sizeof(float) * vertices.Length, v, GL_STATIC_DRAW);

            glVertexAttribPointer(0, 2, GL_FLOAT, false, 2 * sizeof(float), NULL);
            glEnableVertexAttribArray(0);
        }
    }

    // -------------------------------------------------------------------------------------------------------------------

    // Create GPU buffer out of out instances from the array
    protected override unsafe void updateInstances()
    {
        if (instArrayPosition > 1)
        {
            N = instArrayPosition / n;

            // Copy data to GPU:
            glBindBuffer(GL_ARRAY_BUFFER, instVbo);
            {
                fixed (float* a = &instanceArray[0])
                    glBufferData(GL_ARRAY_BUFFER, sizeof(float) * instArrayPosition, a, GL_DYNAMIC_COPY);

                glEnableVertexAttribArray(1);
                glVertexAttribPointer(1, 4, GL_FLOAT, false, floatTimesN, NULL);

                glEnableVertexAttribArray(2);
                glVertexAttribPointer(2, 4, GL_FLOAT, false, floatTimesN, new IntPtr(1 * 4 * sizeof(float)));

                // Tell OpenGL this is an instanced vertex attribute
                glVertexAttribDivisor(1, 1);
                glVertexAttribDivisor(2, 1);

                glBindBuffer(GL_ARRAY_BUFFER, 0);
            }
        }

        return;
    }

    // -------------------------------------------------------------------------------------------------------------------
};
