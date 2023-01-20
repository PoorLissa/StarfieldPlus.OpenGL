using static OpenGL.GL;



public class myScrDimmer : myPrimitive
{
    // Vbo (Vertex Buffer Object) -- Manages memory buffer on the GPU
    // Ebo (Element Buffer Object) is a buffer that stores indices that are used to decide what vertices to draw (and in what order)

    private static uint vbo = 0, ebo = 0, shaderProgram = 0;
    private static float[] vertices = null;
    private static int locationColor = 0;

    private const int verticesLength = 12;
    private const int sizeofFloat_x_verticesLength = sizeof(float) * verticesLength;
    private const int sizeofFloat_x_3 = sizeof(float) * 3;

    // -------------------------------------------------------------------------------------------------------------------

    public myScrDimmer()
    {
        if (vertices == null)
        {
            vertices = new float[verticesLength];

            CreateProgram();
            glUseProgram(shaderProgram);
            locationColor = glGetUniformLocation(shaderProgram, "myColor");

            vbo = glGenBuffer();
            ebo = glGenBuffer();

            updateIndices();

            // Need to do this only once, as it will always be drawn as (0, 0, Width, Height)
            {
                vertices[06] = -1.0f;
                vertices[09] = -1.0f;
                vertices[01] = +1.0f;
                vertices[10] = +1.0f;

                vertices[00] = +1.0f;
                vertices[03] = +1.0f;

                vertices[04] = -1.0f;
                vertices[07] = -1.0f;
            }
        }
    }

    // -------------------------------------------------------------------------------------------------------------------

    public void Draw()
    {
        glUseProgram(shaderProgram);
        glUniform4f(locationColor, _r, _g, _b, _a);

        // Move vertices data from CPU to GPU -- needs to be called each time we change the Rectangle's coordinates
        unsafe
        {
            // Bind a buffer;
            // From now on, all the operations on this type of buffer will be performed on the buffer we just bound;
            glBindBuffer(GL_ARRAY_BUFFER, vbo);
            {
                // Copy user-defined data into the currently bound buffer:
                fixed (float* v = &vertices[0])
                    glBufferData(GL_ARRAY_BUFFER, sizeofFloat_x_verticesLength, v, GL_STREAM_DRAW);
            }

            glVertexAttribPointer(0, 3, GL_FLOAT, false, sizeofFloat_x_3, NULL);
            glEnableVertexAttribArray(0);

            // Draw
            glPolygonMode(GL_FRONT_AND_BACK, GL_FILL);
            glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, ebo);
            glDrawElements(GL_TRIANGLES, 6, GL_UNSIGNED_INT, NULL);
        }
    }

    // -------------------------------------------------------------------------------------------------------------------

    // Create a shader program
    private static void CreateProgram()
    {
        var vertex = myOGL.CreateShaderEx(GL_VERTEX_SHADER,
            @"layout (location = 0) in vec3 pos;",
                main: @"gl_Position = vec4(pos, 1.0);"
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

    // Move indices data from CPU to GPU -- needs to be called only once, as we have 2 different EBOs, and they are not going to change;
    // The EBO must be activated prior to drawing the shape: glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, doFill ? ebo1 : ebo2);
    private static unsafe void updateIndices()
    {
        int usage = GL_STATIC_DRAW;

        glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, ebo);
        {
            var indicesFill = new uint[]
            {
                0, 1, 3,   // first triangle
                1, 2, 3    // second triangle
            };

            fixed (uint* i = &indicesFill[0])
                glBufferData(GL_ELEMENT_ARRAY_BUFFER, sizeof(uint) * indicesFill.Length, i, usage);

            // Unbind current buffer
            glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, 0);
        }
    }

    // -------------------------------------------------------------------------------------------------------------------
};
