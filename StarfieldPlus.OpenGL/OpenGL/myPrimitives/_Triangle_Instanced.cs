using GLFW;
using static OpenGL.GL;
using System;



// https://learnopengl.com/code_viewer_gh.php?code=src/4.advanced_opengl/10.1.instancing_quads/instancing_quads.cpp



public class myTriangleInst : myPrimitive
{
    private static float[] vertices = null;
    private static float[] instanceArray = null;

    private static uint ebo_fill = 0, ebo_outline = 0, shaderProgram = 0, instVbo = 0, quadVbo = 0;
    private static float pixelX = 0, pixelY = 0;
    private static int locationColor = 0, locationAngle = 0, locationCenter = 0, locationScrSize = 0, N = 0, Count = 0;

    // Number of elements in [instanceArray] that define one single instance:
    // - 4 floats for Coordinates
    // - 4 floats for RGBA
    // - 1 float for Angle
    private static readonly int n = 9;

    // -------------------------------------------------------------------------------------------------------------------

    public myTriangleInst(int maxInstCount)
    {
        if (vertices == null)
        {
            N = 0;

            vertices = new float[9];
            instanceArray = new float[maxInstCount * n];

            for (int i = 0; i < 9; i++)
                vertices[i] = 0.0f;

            pixelX = 1.0f / Width;
            pixelY = 1.0f / Height;

            CreateProgram();
            glUseProgram(shaderProgram);
            locationColor   = glGetUniformLocation(shaderProgram, "myColor");
            locationScrSize = glGetUniformLocation(shaderProgram, "myScrSize");

            instVbo     = glGenBuffer();
            quadVbo     = glGenBuffer();
            ebo_fill    = glGenBuffer();
            ebo_outline = glGenBuffer();
        }
    }

    // -------------------------------------------------------------------------------------------------------------------

    public void Draw(bool doFill = false)
    {
        vertices[0] = +0.0f;
        vertices[1] = +0.5f;
        vertices[3] = +0.35f;
        vertices[4] = -0.35f;
        vertices[6] = -0.35f;
        vertices[7] = -0.35f;

        updateVertices();

        glUseProgram(shaderProgram);
        setColor(locationColor, _r, _g, _b, _a);

        glUniform2f(locationCenter, 0, 0);
        updUniformScreenSize(locationScrSize, Width, Height);

        // Draw only outline or fill the whole polygon with color
        glPolygonMode(GL_FRONT_AND_BACK, doFill ? GL_FILL : GL_LINE);
        glDrawArraysInstanced(GL_TRIANGLES, 0, 3, N);
    }

    // -------------------------------------------------------------------------------------------------------------------

    private static void CreateProgram()
    {
        var vertex = myOGL.CreateShaderEx(GL_VERTEX_SHADER,
            @"layout (location = 0) in vec3 pos;
              layout (location = 1) in mat2x4 mData;
              layout (location = 3) in float angle;
                uniform float myAngle; uniform vec2 myCenter; uniform ivec2 myScrSize;
                out vec4 rgbaColor;",

            main: @"rgbaColor = mData[1];
            
                    if (angle == 0)
                    {
                        float realSize = 2.0 / myScrSize.y * mData[0].z;

                        //gl_Position = vec4(pos.x * mData[0].z + mData[0].x, pos.y * mData[0].z + mData[0].y, 1.0, 1.0);

                        gl_Position = vec4(pos.x * realSize, pos.y * realSize, 1.0, 1.0);

                        // Adjust for pixel density and move into final position

                        gl_Position.x += +2.0 / myScrSize.x * (mData[0].x) - 1.0;

                        gl_Position.y += -2.0 / myScrSize.y * (mData[0].y) + 1.0;
                    }
                    else
                    {
                    }"
        );

        var fragment = myOGL.CreateShaderEx(GL_FRAGMENT_SHADER,
            "in vec4 rgbaColor; out vec4 result; uniform vec4 myColor;",

                main: @"result = rgbaColor;

                        if (myColor.w < 0)
                        {
                            result.w *= -myColor.w;
                        }"
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

    private static unsafe void updateVertices()
    {
        glBindBuffer(GL_ARRAY_BUFFER, quadVbo);
        {
            fixed (float* v = &vertices[0])
                glBufferData(GL_ARRAY_BUFFER, sizeof(float) * vertices.Length, v, GL_DYNAMIC_DRAW);

            glVertexAttribPointer(0, 3, GL_FLOAT, false, 3 * sizeof(float), NULL);
            glEnableVertexAttribArray(0);
        }
    }

    // -------------------------------------------------------------------------------------------------------------------

    public void Clear()
    {
        Count = 0;
    }

    // -------------------------------------------------------------------------------------------------------------------

    public void setInstanceCoords(float x, float y, float w, float h)
    {
        instanceArray[Count + 0] = x;
        instanceArray[Count + 1] = y;
        instanceArray[Count + 2] = w;
        instanceArray[Count + 3] = h;

        Count += 4;
    }

    // -------------------------------------------------------------------------------------------------------------------

    public void setInstanceAngle(float a)
    {
        instanceArray[Count] = a;

        Count++;
    }

    // -------------------------------------------------------------------------------------------------------------------

    public void setInstanceColor(float r, float g, float b, float a)
    {
        instanceArray[Count + 0] = r;
        instanceArray[Count + 1] = g;
        instanceArray[Count + 2] = b;
        instanceArray[Count + 3] = a;

        Count += 4;
    }

    // -------------------------------------------------------------------------------------------------------------------

    public void setInstanceColor(double r, double g, double b, double a)
    {
        int i = Count;

        instanceArray[i + 0] = (float)r;
        instanceArray[i + 1] = (float)g;
        instanceArray[i + 2] = (float)b;
        instanceArray[i + 3] = (float)a;

        Count += 4;
    }

    // -------------------------------------------------------------------------------------------------------------------

    // Create GPU buffer out of out instances from the array
    public unsafe void updateInstances()
    {
        if (Count > 1)
        {
            N = Count / n;

            // Copy data to GPU:
            glBindBuffer(GL_ARRAY_BUFFER, instVbo);
            {
                fixed (float* a = &instanceArray[0])
                    glBufferData(GL_ARRAY_BUFFER, sizeof(float) * Count, a, GL_DYNAMIC_COPY);

                glEnableVertexAttribArray(1);
                glVertexAttribPointer(1, 4, GL_FLOAT, false, n * sizeof(float), NULL);

                glEnableVertexAttribArray(2);
                glVertexAttribPointer(2, 4, GL_FLOAT, false, n * sizeof(float), new IntPtr(1 * 4 * sizeof(float)));

                glEnableVertexAttribArray(3);
                glVertexAttribPointer(3, 1, GL_FLOAT, false, n * sizeof(float), new IntPtr(1 * 8 * sizeof(float)));

                // Tell OpenGL this is an instanced vertex attribute
                glVertexAttribDivisor(1, 1);
                glVertexAttribDivisor(2, 1);
                glVertexAttribDivisor(3, 1);

                glBindBuffer(GL_ARRAY_BUFFER, 0);
            }
        }

        return;
    }

    // -------------------------------------------------------------------------------------------------------------------

    // Reallocate inner instances array, if its size is less than the new size
    public void Resize(int Size)
    {
        if (instanceArray.Length < Size * n)
        {
            instanceArray = new float[Size * n];
        }
    }

    // -------------------------------------------------------------------------------------------------------------------
};
