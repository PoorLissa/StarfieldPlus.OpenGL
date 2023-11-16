using GLFW;
using static OpenGL.GL;
using System;
using System.Diagnostics;

public class myTriangle : myPrimitive
{
    private static uint vbo = 0, shaderProgram = 0;
    private static float[] vertices = null;
    private static int locationColor = 0, locationAngle = 0, locationCenter = 0, locationScrSize = 0;
    private static float _angle = 0;

    // -------------------------------------------------------------------------------------------------------------------

    public myTriangle()
    {
        if (vertices == null)
        {
            vertices = new float[9];

            for (int i = 0; i < 9; i++)
                vertices[i] = 0.0f;

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

    public void Draw(float x1, float y1, float x2, float y2, float x3, float y3, bool doFill = false)
    {
        float recalcX(float x) { return 2.0f * x / Width - 1.0f;  }
        float recalcY(float y) { return 1.0f - 2.0f * y / Height; }

        // ---------------------------------------------------------------------------------------

        if (_angle == 0)
        {
            // Recalc screen coordinates into Normalized Device Coordinates (NDC)
            vertices[0] = recalcX(x1);
            vertices[1] = recalcY(y1);
            vertices[3] = recalcX(x2);
            vertices[4] = recalcY(y2);
            vertices[6] = recalcX(x3);
            vertices[7] = recalcY(y3);
        }
        else
        {
            // Leave coordinates as they are, and recalc them in the shader
            vertices[0] = x1;
            vertices[1] = y1;
            vertices[3] = x2;
            vertices[4] = y2;
            vertices[6] = x3;
            vertices[7] = y3;
        }

        updateVertices();

        glUseProgram(shaderProgram);
        setColor(locationColor, _r, _g, _b, _a);
        glUniform1f(locationAngle, _angle);

        // Set the center of rotation
        if (_angle != 0.0f)
        {
            glUniform2f(locationCenter, (x1+x2+x3)/3, (y1+y2+y3)/3);
            updUniformScreenSize(locationScrSize, Width, Height);
        }

        // Draw only outline or fill the whole polygon with color
        glPolygonMode(GL_FRONT_AND_BACK, doFill ? GL_FILL : GL_LINE);
        glDrawArrays(GL_TRIANGLES, 0, 3);
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
            @"layout (location = 0) in vec3 pos;
                uniform float myAngle; uniform vec2 myCenter; uniform ivec2 myScrSize;",

                main: @"if (myAngle == 0)
                        {
                            gl_Position = vec4(pos, 1.0);
                        }
                        else
                        {
                            float X = pos.x - myCenter.x;
                            float Y = pos.y - myCenter.y;

                            gl_Position = vec4(X * cos(myAngle) - Y * sin(myAngle), Y * cos(myAngle) + X * sin(myAngle), pos.z, 1.0);

                            gl_Position.x += myCenter.x;
                            gl_Position.y += myCenter.y1;

                            gl_Position.x = 2.0f * gl_Position.x / myScrSize.x - 1.0f;
                            gl_Position.y = 1.0f - 2.0f * gl_Position.y / myScrSize.y;
                        }"
        );

        var fragment = myOGL.CreateShaderEx(GL_FRAGMENT_SHADER,
                "out vec4 result; uniform vec4 myColor;",
                    main: "result = myColor;"
        );

        int[] status = glGetShaderiv(vertex, GL_COMPILE_STATUS, 1);

        if (status[0] == 0)
        {
            string error = glGetShaderInfoLog(vertex);
            Debug.Assert(false, error);
        }

        shaderProgram = glCreateProgram();

        glAttachShader(shaderProgram, vertex);
        glAttachShader(shaderProgram, fragment);

        glLinkProgram(shaderProgram);

        glDetachShader(shaderProgram, vertex);
        glDetachShader(shaderProgram, fragment);

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
            // todo: see which one is better or maybe we need a choise here
            fixed (float* v = &vertices[0])
                glBufferData(GL_ARRAY_BUFFER, sizeof(float) * vertices.Length, v, GL_DYNAMIC_DRAW);

            glVertexAttribPointer(0, 3, GL_FLOAT, false, 3 * sizeof(float), NULL);
            glEnableVertexAttribArray(0);
        }
    }

    // -------------------------------------------------------------------------------------------------------------------
};
