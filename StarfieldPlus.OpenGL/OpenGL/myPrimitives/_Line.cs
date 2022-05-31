using GLFW;
using static OpenGL.GL;
using System;


public class myLine : myPrimitive
{
    private static uint vbo = 0, shaderProgram = 0;
    private static float[] vertices = null;
    private static int locationColor = 0, locationAngle = 0, locationCenter = 0, locationScrSize = 0;
    private static float _angle = 0;

    // -------------------------------------------------------------------------------------------------------------------

    public myLine()
    {
        if (vertices == null)
        {
            vertices = new float[4];

            CreateProgram();
            glUseProgram(shaderProgram);
            locationColor   = glGetUniformLocation(shaderProgram, "myColor");
            locationAngle   = glGetUniformLocation(shaderProgram, "myAngle");
            locationCenter  = glGetUniformLocation(shaderProgram, "myCenter");
            locationScrSize = glGetUniformLocation(shaderProgram, "myScrSize");

            vbo = glGenBuffer();
        }
    }

    // -------------------------------------------------------------------------------------------------------------------

    public void Draw(float x1, float y1, float x2, float y2, float lineWidth = 1.0f)
    {
        if (_angle == 0)
        {
            // Recalc screen coordinates into Normalized Device Coordinates (NDC)
            float fx = 2.0f * x1 / Width - 1.0f;
            float fy = 1.0f - 2.0f * y1 / Height;

            vertices[0] = fx;
            vertices[1] = fy;

            fx = 2.0f * x2 / Width - 1.0f;
            fy = 1.0f - 2.0f * y2 / Height;
            vertices[2] = fx;
            vertices[3] = fy;
        }
        else
        {
            vertices[0] = x1;
            vertices[1] = y1;

            vertices[2] = x2;
            vertices[3] = y2;
        }

        updateVertices();

        glUseProgram(shaderProgram);
        setColor(locationColor, _r, _g, _b, _a);
        glUniform1f(locationAngle, _angle);

        // Set the center of rotation
        if (_angle != 0.0f)
        {
            glUniform2f(locationCenter, (x1+x2)/2, (y1+y2)/2);
            updUniformScreenSize(locationScrSize, Width, Height);
        }

        glLineWidth(lineWidth);

        glDrawArrays(GL_LINES, 0, 2);
    }

    // -------------------------------------------------------------------------------------------------------------------

    public void SetAngle(float angle)
    {
        _angle = angle;
    }

    // -------------------------------------------------------------------------------------------------------------------

    private static void CreateProgram()
    {
        var vertex = myOGL.CreateShaderEx(GL_VERTEX_SHADER,
                "layout (location = 0) in vec2 pos;" +
                "   uniform float myAngle; uniform vec2 myCenter; uniform ivec2 myScrSize;",

                    main: @"if (myAngle == 0)
                            {
                                gl_Position = vec4(pos.x, pos.y, 0.0, 1.0);
                            }
                            else
                            {
                                float X = pos.x - myCenter.x;
                                float Y = pos.y - myCenter.y;

                                gl_Position = vec4(X * cos(myAngle) - Y * sin(myAngle), Y * cos(myAngle) + X * sin(myAngle), 0.0, 1.0);

                                gl_Position.x += myCenter.x;
                                gl_Position.y += myCenter.y;

                                gl_Position.x = 2.0f * gl_Position.x / myScrSize.x - 1.0f;
                                gl_Position.y = 1.0f - 2.0f * gl_Position.y / myScrSize.y;
                            }"
        );

        var fragment = myOGL.CreateShaderEx(GL_FRAGMENT_SHADER,
                "out vec4 result; uniform vec4 myColor;",
                    main: "result = myColor;"
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
};
