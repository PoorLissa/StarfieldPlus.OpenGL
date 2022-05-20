using GLFW;
using static OpenGL.GL;
using System;



public class myHexagon : myPrimitive
{
    // Vbo (Vertex Buffer Object) -- Manages memory buffer on the GPU
    // Ebo (Element Buffer Object) is a buffer that stores indices that are used to decide what vertices to draw (and in what order)

    private static uint vbo = 0, ebo_fill = 0, ebo_outline = 0, shaderProgram = 0;
    private static float[] vertices = null;
    private static int locationColor = 0, locationAngle = 0, locationCenter = 0, locationScrSize = 0;
    private static float sqrt3_div2 = 0;
    private static float h_div_w = 0;
    private static float _angle;

    // -------------------------------------------------------------------------------------------------------------------

    public myHexagon()
    {
        if (vertices == null)
        {
            sqrt3_div2 = (float)(Math.Sqrt(3.0) / 2.0);
            h_div_w    = (float)Height / (float)Width;

            vertices = new float[18];

            for (int i = 0; i < 18; i++)
                vertices[i] = 0.0f;

            CreateProgram();
            glUseProgram(shaderProgram);
            locationColor   = glGetUniformLocation(shaderProgram, "myColor");
            locationAngle   = glGetUniformLocation(shaderProgram, "myAngle");
            locationCenter  = glGetUniformLocation(shaderProgram, "myCenter");
            locationScrSize = glGetUniformLocation(shaderProgram, "myScrSize");

            vbo         = glGenBuffer();
            ebo_fill    = glGenBuffer();
            ebo_outline = glGenBuffer();

            updateIndices();
        }
    }

    // -------------------------------------------------------------------------------------------------------------------

    public void Draw(float x, float y, float r, bool doFill = false)
    {
        unsafe void __draw(bool fill)
        {
            if (fill)
            {
                glPolygonMode(GL_FRONT_AND_BACK, GL_FILL);
                glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, ebo_fill);
                glDrawElements(GL_TRIANGLES, 12, GL_UNSIGNED_INT, NULL);
            }
            else
            {
                glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, ebo_outline);
                glDrawElements(GL_LINES, 18, GL_UNSIGNED_INT, NULL);
            }
        }

        // ---------------------------------------------------------------------------------------

        float fx, fy;

        if (_angle == 0)
        {
            // Recalc screen coordinates into Normalized Device Coordinates (NDC)
            fx = 2.0f * x / Width - 1.0f;
            fy = 1.0f - 2.0f * y / Height;

            float fr = 2.0f * r / Height;           // Radius
            float frx = fr * h_div_w;               // Radius adjusted for x-coordinate

            float frx_sqrt = fr * sqrt3_div2;
            float frx_half = frx * 0.5f;

            vertices[00] = fx - frx;
            vertices[01] = fy;
            vertices[03] = fx - frx_half;
            vertices[04] = fy + frx_sqrt;
            vertices[06] = fx + frx_half;
            vertices[07] = fy + frx_sqrt;
            vertices[09] = fx + frx;
            vertices[10] = fy;
            vertices[12] = fx + frx_half;
            vertices[13] = fy - frx_sqrt;
            vertices[15] = fx - frx_half;
            vertices[16] = fy - frx_sqrt;
        }
        else
        {
            // Leave coordinates as they are, and recalc them in the shader
            fx = x;
            fy = y;

            float fr = r;                           // Radius
            float frx = fr;                         // Radius adjusted for x-coordinate

            float frx_sqrt = fr * sqrt3_div2;
            float frx_half = frx * 0.5f;

            vertices[00] = fx - frx;
            vertices[01] = fy;
            vertices[03] = fx - frx_half;
            vertices[04] = fy + frx_sqrt;
            vertices[06] = fx + frx_half;
            vertices[07] = fy + frx_sqrt;
            vertices[09] = fx + frx;
            vertices[10] = fy;
            vertices[12] = fx + frx_half;
            vertices[13] = fy - frx_sqrt;
            vertices[15] = fx - frx_half;
            vertices[16] = fy - frx_sqrt;
        }

        updateVertices();

        glUseProgram(shaderProgram);
        setColor(locationColor, _r, _g, _b, _a);
        glUniform1f(locationAngle, _angle);

        // Set the center of rotation
        if (_angle != 0.0f)
        {
            glUniform2f(locationCenter, x, y);
            updUniformScreenSize(locationScrSize, Width, Height);
        }

        __draw(doFill);
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
                            gl_Position.y += myCenter.y;

                            gl_Position.x = 2.0f * gl_Position.x / (myScrSize.x + 1) - 1.0f;
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
            // Copy user-defined data into the currently bound buffer:
            fixed (float* v = &vertices[0])
                glBufferData(GL_ARRAY_BUFFER, sizeof(float) * vertices.Length, v, GL_DYNAMIC_DRAW);
        }

        glVertexAttribPointer(0, 3, GL_FLOAT, false, 3 * sizeof(float), NULL);
        glEnableVertexAttribArray(0);
    }

    // -------------------------------------------------------------------------------------------------------------------

    // Move indices data from CPU to GPU -- needs to be called only once, as we have 2 different EBOs, and they are not going to change;
    // The EBO must be activated prior to drawing the shape: glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, doFill ? ebo1 : ebo2);
    private static unsafe void updateIndices()
    {
        int usage = GL_STATIC_DRAW;

        glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, ebo_fill);
        {
            // 4 triangles
            var indicesFill = new uint[]
            {
                0, 1, 3,
                1, 2, 3,
                0, 4, 3,
                0, 5, 4
            };

            fixed (uint* i = &indicesFill[0])
                glBufferData(GL_ELEMENT_ARRAY_BUFFER, sizeof(uint) * indicesFill.Length, i, usage);

            // Unbind current buffer
            glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, 0);
        }

        glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, ebo_outline);
        {
            // 6 lines
            var indicesOutline = new uint[]
            {
                0, 1,
                1, 2,
                2, 3,
                3, 4,
                4, 5,
                5, 0
            };

            fixed (uint* i = &indicesOutline[0])
                glBufferData(GL_ELEMENT_ARRAY_BUFFER, sizeof(uint) * indicesOutline.Length, i, usage);

            // Unbind current buffer
            glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, 0);
        }
    }

    // -------------------------------------------------------------------------------------------------------------------
};
