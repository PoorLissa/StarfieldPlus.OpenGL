using GLFW;
using static OpenGL.GL;
using System;


/*
    This class lets the user to create full-screen custom shaders.

    Example:

            string fragHeader = @"

                float concentric(vec2 m, float repeat, float t) {
                    float r = length(m);
                    float v = sin((1.0 - r) * (1.0 - r) * repeat + t) * 0.5 + 0.5;
                    return v;
                }

                float spiral(vec2 m, float repeat, float dir, float t) {
	                float r = length(m);
	                float a = atan(m.y, m.x);
	                float v = sin(repeat * (sqrt(r) + (1.0 / repeat) * dir * a - t)) * 0.5 + 0.5;
	                return v;
                }
            ";

            string fragShader = $@"

                float aspect = iResolution.x / iResolution.y;
    
                vec2 uv = (gl_FragCoord.xy / iResolution.xy * 2.0 - 1.0) * vec2(1.0, 1.0 / aspect);
                float r = length(uv);

                float c0 = 1.0 - sin(r * r) * 0.85;

                float c1 = concentric(uv, 50.0, uTime * 0.1) * 0.5 + 0.5;
                float c2 = spiral(uv, 90.0, +1.0, uTime * 0.11) * 0.9 + 0.1;
                float c3 = spiral(uv, 30.0, -1.0, uTime * 0.09) * 0.8 + 0.2;

                vec3 col = vec3(c0 * c1 * c2 * c3) * vec3(0.5, 0.1, 0.25);

                result = vec4(col, 1.0);
            ";

            ...

            myFreeShader shader = new myFreeShader(fHeader: fragHeader, fMain: fragShader);

            ...

            shader.Draw();
*/


public class myFreeShader_FullScreen : myPrimitive
{
    private long tBegin;
    private uint vbo = 0, ebo = 0, shaderProgram = 0;
    private float[] vertices = null;

    // Uniform ids:
    private static int u_Time;

    private static int verticesLength = 12;
    private static int sizeofFloat_x_verticesLength = sizeof(float) * verticesLength;

    // -------------------------------------------------------------------------------------------------------------------

    public myFreeShader_FullScreen(string fHeader = "", string fMain = "")
    {
        if (vertices == null)
        {
            vertices = new float[verticesLength];

            CreateProgram(fHeader, fMain);
            glUseProgram(shaderProgram);

            // Uniforms
            u_Time = glGetUniformLocation(shaderProgram, "uTime");

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

            // Start the timer
            tBegin = DateTime.Now.Ticks;
        }
    }

    // -------------------------------------------------------------------------------------------------------------------

    // Draw;
    // As this is only a full-screen shader, no need to recalculate any coordinates
    public void Draw()
    {
        glUseProgram(shaderProgram);

        // Update uniforms:
        glUniform1f(u_Time, (float)(TimeSpan.FromTicks(DateTime.Now.Ticks - tBegin).TotalSeconds));

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
            {
                glPolygonMode(GL_FRONT_AND_BACK, GL_FILL);
                glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, ebo);
                glDrawElements(GL_TRIANGLES, 6, GL_UNSIGNED_INT, NULL);
            }
        }
    }

    // -------------------------------------------------------------------------------------------------------------------

    // Create a shader program:
    // - Vertex shader uses the default implementation
    // - Fragment shader code must be supplied by the user
    private void CreateProgram(string fHeader, string fMain)
    {
        // Vertex Shader Program
        string vHeader = "layout (location=0) in vec3 pos;";
        string vMain   = "gl_Position = vec4(pos, 1.0);";

        // Fragment Shader Program
        {
            if (string.IsNullOrEmpty(fHeader))
            {
                // Default implementation
                fHeader = "out vec4 result;";
            }
            else
            {
                // Extend the header with some pre-defined variables:
                fHeader = $"out vec4 result;" +
                          $"uniform float uTime;" +
                          $"vec2 iResolution = vec2({Width}, {Height});" +
                          $"{fHeader}";
            }

            if (string.IsNullOrEmpty(fMain))
            {
                // Default implementation
                fMain = $@"result = vec4(gl_FragCoord.x / {Width}, gl_FragCoord.y / {Height}, sin(uTime * 0.33), 1);";
            }
        }

        var vertex = myOGL.CreateShaderEx(GL_VERTEX_SHADER,
            header : vHeader,
                main : vMain
        );

        var fragment = myOGL.CreateShaderEx(GL_FRAGMENT_SHADER,
            header : fHeader,
                main : fMain
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

    // Move indices data from CPU to GPU -- needs to be called only once
    // The EBO must be activated prior to drawing the shape: glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, ebo);
    private unsafe void updateIndices()
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
