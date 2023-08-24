using GLFW;
using static OpenGL.GL;
using System;



#if true

// New test implementation with compilation-time hardcoded screen size

public class myRectangle : myPrimitive
{
    // Vbo (Vertex Buffer Object) -- Manages memory buffer on the GPU
    // Ebo (Element Buffer Object) is a buffer that stores indices that are used to decide what vertices to draw (and in what order)

    private static uint vbo = 0, ebo_fill = 0, ebo_outline = 0, shaderProgram = 0;
    private static float[] vertices = null;
    private static float _angle;
    private static int myColor = 0, myAngle = 0, myCenter = 0;

    private static float w1 = -1, w2 = -1, invW = -1, invH = -1;

    private static int verticesLength = 12;
    private static int sizeofFloat_x_verticesLength = sizeof(float) * verticesLength;

    // -------------------------------------------------------------------------------------------------------------------

    public myRectangle()
    {
        if (vertices == null)
        {
            w1 = 2.0f / (Width - 1);
            w2 = 2.0f / (Width + 1);

            invW = 2.0f / Width;
            invH = 2.0f / Height;

            vertices = new float[verticesLength];

            CreateProgram();
            glUseProgram(shaderProgram);

            // Uniforms
            {
                myColor  = glGetUniformLocation(shaderProgram, "myColor");
                myAngle  = glGetUniformLocation(shaderProgram, "myAngle");
                myCenter = glGetUniformLocation(shaderProgram, "myCenter");

                if (myColor < 0 || myAngle < 0 || myCenter < 0)
                {
                    throw new System.Exception("Failed to initialize uniform(s)");
                }
            }

            vbo         = glGenBuffer();
            ebo_fill    = glGenBuffer();
            ebo_outline = glGenBuffer();

            updateIndices();
        }
    }

    // -------------------------------------------------------------------------------------------------------------------

    public void Draw(int x, int y, int w, int h, bool doFill = false)
    {
        if (_angle == 0)
        {
            // Recalc screen coordinates into Normalized Device Coordinates (NDC)

            // https://stackoverflow.com/questions/70146951/opengl-how-to-fix-missing-corner-pixel-in-rect-lines-or-line-loop

            //float fx = 2.0f * x / Width - 1.0f;

            // Shift Width a bit to get rid of incomplete left bottom angle
            //float fx = 2.0f * x / (Width + 1) - 1.0f;
            //float fx = 2.0f * x / (Width) - 1.0f;
            //float fx = (x < Width / 2) ? (w1 * x - 1.0f) : (w2 * x - 1.0f);
            //float fx = 2.0f * x / (Width) - 1.0f;

            float fx = -1.0f + x * invW;        // 2.0 * x / Width - 1.0;
            float fy = +1.0f - y * invH;        // 1.0 + 2.0 * y / Height;

            vertices[06] = fx;
            vertices[09] = fx;
            vertices[01] = fy;
            vertices[10] = fy;

            fx = -1.0f + (x + w) * invW;
            fy = +1.0f - (y + h) * invH;

            vertices[0] = fx;
            vertices[3] = fx;
            vertices[4] = fy;
            vertices[7] = fy;
        }
        else
        {
            // Leave coordinates as they are, and recalc them in the shader
            float fx = x;
            float fy = y;

            vertices[06] = fx;
            vertices[09] = fx;
            vertices[01] = fy;
            vertices[10] = fy;

            fx = x + w;
            fy = y + h;

            vertices[0] = fx;
            vertices[3] = fx;
            vertices[4] = fy;
            vertices[7] = fy;
        }


        updateVertices();

        glUseProgram(shaderProgram);

        glUniform4f(myColor, _r, _g, _b, _a);
        glUniform1f(myAngle, _angle);

        // Set the center of rotation
        if (_angle != 0.0f)
        {
            glUniform2f(myCenter, x + w / 2, y + h / 2);
        }

        // Draw
        unsafe
        {
            if (doFill)
            {
                glPolygonMode(GL_FRONT_AND_BACK, GL_FILL);
                glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, ebo_fill);

                // Renders the triangles using an index buffer EBO
                glDrawElements(GL_TRIANGLES, 6, GL_UNSIGNED_INT, NULL);
            }
            else
            {
                glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, ebo_outline);
                glDrawElements(GL_LINES, 8, GL_UNSIGNED_INT, NULL);
            }
        }

        return;
    }

    // -------------------------------------------------------------------------------------------------------------------

    // Create a shader program
    private static void CreateProgram()
    {
        var vertex = myOGL.CreateShaderEx(GL_VERTEX_SHADER,

            header: $@"layout (location = 0) in vec3 pos;
                        uniform float myAngle; uniform vec2 myCenter;
                        float invW = {2.0 / Width}; float invH = {2.0 / Height};",

                main: @"if (myAngle == 0)
                        {
                            gl_Position = vec4(pos, 1.0);
                        }
                        else
                        {
                            vec2 sc = vec2(sin(myAngle), cos(myAngle));
                            vec2 pt = pos.xy - myCenter.xy;

                            gl_Position = vec4(pt.x * sc.y - pt.y * sc.x, pt.y * sc.y + pt.x * sc.x, pos.z, 1.0);
                            gl_Position.xy += myCenter.xy;

                            gl_Position.x = +gl_Position.x * invW - 1.0;
                            gl_Position.y = -gl_Position.y * invH + 1.0;
                        }"
        );

        var fragment = myOGL.CreateShaderEx(GL_FRAGMENT_SHADER,

            header: $@"out vec4 result; uniform vec4 myColor;",

                main: $@"result = myColor;"
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
                glBufferData(GL_ARRAY_BUFFER, sizeofFloat_x_verticesLength, v, GL_DYNAMIC_DRAW);
        }

        glVertexAttribPointer(0, 3, GL_FLOAT, false, sizeofFloat_x_3, NULL);
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

        glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, ebo_outline);
        {
            var indicesOutline = new uint[]
            {
                0, 1,
                1, 2,
                2, 3,
                3, 0
            };

            fixed (uint* i = &indicesOutline[0])
                glBufferData(GL_ELEMENT_ARRAY_BUFFER, sizeof(uint) * indicesOutline.Length, i, usage);

            // Unbind current buffer
            glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, 0);
        }
    }

    // -------------------------------------------------------------------------------------------------------------------

    public void SetAngle(float angle)
    {
        _angle = angle;
    }

    // -------------------------------------------------------------------------------------------------------------------

    public void Draw(float x, float y, float w, float h, bool doFill = false)
    {
        Draw((int)x, (int)y, (int)w, (int)h, doFill);
    }

    // -------------------------------------------------------------------------------------------------------------------

    // Sometimes shapes are drawn incorrectly (parts of the outline are missing)
    // One way to fix this is to change the pixel density
    public void setPixelDensityOffset(int offset)
    {
        if (offset > 0)
        {
            invW = 2.0f / (Width  + offset);
            invH = 2.0f / (Height + offset);
        }
    }

    // -------------------------------------------------------------------------------------------------------------------

};

#else

public class myRectangle : myPrimitive
{
    // Vbo (Vertex Buffer Object) -- Manages memory buffer on the GPU
    // Ebo (Element Buffer Object) is a buffer that stores indices that are used to decide what vertices to draw (and in what order)

    private static uint vbo = 0, ebo_fill = 0, ebo_outline = 0, shaderProgram = 0;
    private static float[] vertices = null;
    private static float _angle;
    private static int locationColor = 0, locationAngle = 0, locationCenter = 0, locationScrSize = 0;

    private static float w1 = -1, w2 = -1;

    // -------------------------------------------------------------------------------------------------------------------

    public myRectangle()
    {
        if (vertices == null)
        {
            w1 = 2.0f / (Width - 1);
            w2 = 2.0f / (Width + 1);

            vertices = new float[12];

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

    public void Draw(int x, int y, int w, int h, bool doFill = false)
    {
        unsafe void __draw(bool fill)
        {
            if (fill)
            {
                glPolygonMode(GL_FRONT_AND_BACK, GL_FILL);
                glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, ebo_fill);
                glDrawElements(GL_TRIANGLES, 6, GL_UNSIGNED_INT, NULL);     // Renders the triangles using an index buffer EBO
            }
            else
            {
                glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, ebo_outline);
                glDrawElements(GL_LINES, 8, GL_UNSIGNED_INT, NULL);
            }
        }

        // ---------------------------------------------------------------------------------------

        if (_angle == 0)
        {
            // Recalc screen coordinates into Normalized Device Coordinates (NDC)

            //float fx = 2.0f * x / Width - 1.0f;

            // Shift Width a bit to get rid of incomplete left bottom angle
            //float fx = 2.0f * x / (Width + 1) - 1.0f;
            float fx = 2.0f * x / (Width) - 1.0f;

            //float fx = (x < Width / 2) ? (w1 * x - 1.0f) : (w2 * x - 1.0f);
            //float fx = 2.0f * x / (Width) - 1.0f;

            // https://stackoverflow.com/questions/70146951/opengl-how-to-fix-missing-corner-pixel-in-rect-lines-or-line-loop

            float fy = 1.0f - 2.0f * y / Height;
            vertices[06] = fx;
            vertices[09] = fx;
            vertices[01] = fy;
            vertices[10] = fy;

            fx = 2.0f * (x + w) / Width - 1.0f;
            vertices[0] = fx;
            vertices[3] = fx;

            fy = 1.0f - 2.0f * (y + h) / Height;
            vertices[4] = fy;
            vertices[7] = fy;
        }
        else
        {
            // Leave coordinates as they are, and recalc them in the shader
            float fx = x;
            float fy = y;
            vertices[06] = fx;
            vertices[09] = fx;
            vertices[01] = fy;
            vertices[10] = fy;

            fx = x + w;
            vertices[0] = fx;
            vertices[3] = fx;

            fy = y + h;
            vertices[4] = fy;
            vertices[7] = fy;
        }


        updateVertices();

        glUseProgram(shaderProgram);

        setColor(locationColor, _r, _g, _b, _a);
        glUniform1f(locationAngle, _angle);

        // Set the center of rotation
        if (_angle != 0.0f)
        {
            glUniform2f(locationCenter, x + w / 2, y + h / 2);
            updUniformScreenSize(locationScrSize, Width, Height);
        }

        __draw(doFill);
    }

    // -------------------------------------------------------------------------------------------------------------------

    // Create a shader program
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

                            gl_Position.x = 2.0f * gl_Position.x / (myScrSize.x+1) - 1.0f;
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

        glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, ebo_outline);
        {
            var indicesOutline = new uint[]
            {
                0, 1,
                1, 2,
                2, 3,
                3, 0
            };

            fixed (uint* i = &indicesOutline[0])
                glBufferData(GL_ELEMENT_ARRAY_BUFFER, sizeof(uint) * indicesOutline.Length, i, usage);

            // Unbind current buffer
            glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, 0);
        }
    }

    // -------------------------------------------------------------------------------------------------------------------

    public void SetAngle(float angle)
    {
        _angle = angle;
    }

    // -------------------------------------------------------------------------------------------------------------------

    public void Draw(float x, float y, float w, float h, bool doFill = false)
    {
        Draw((int)x, (int)y, (int)w, (int)h, doFill);
    }

    // -------------------------------------------------------------------------------------------------------------------
};

#endif
