using GLFW;
using static OpenGL.GL;
using System;


/*
    This class lets the user to create custom non-full-screen shaders.

    Example:

            string fragHeader = @"
            ";

            string fragShader = $@"
            ";

            ...

            myFreeShader shader = new myFreeShader(fHeader: fragHeader, fMain: fragShader);

            ...

            shader.Draw();
*/


public class myFreeShader : myPrimitive
{
    private long tBegin;
    private uint vbo = 0, ebo = 0, shaderProgram = 0;
    private float _angle;
    private float[] vertices = null;

    // Uniform ids:
    private static int u_Time, myCenter;

    private static float invW = -1, invH = -1;

    private static int verticesLength = 12;
    private static int sizeofFloat_x_verticesLength = sizeof(float) * verticesLength;

    // -------------------------------------------------------------------------------------------------------------------

    public myFreeShader(string fHeader = "", string fMain = "")
    {
        if (vertices == null)
        {
            invW = 2.0f / Width;
            invH = 2.0f / Height;

            vertices = new float[verticesLength];

            CreateProgram(fHeader, fMain);
            glUseProgram(shaderProgram);

            // Uniforms
            u_Time   = glGetUniformLocation(shaderProgram, "uTime");
            myCenter = glGetUniformLocation(shaderProgram, "myCenter");

            vbo = glGenBuffer();
            ebo = glGenBuffer();

            updateIndices();

            // Start the timer
            tBegin = DateTime.Now.Ticks;
        }
    }

    // -------------------------------------------------------------------------------------------------------------------

    // Draw;
    public void Draw(int x, int y, int w, int h)
    {
        if (_angle == 0)
        {
            // Recalc screen coordinates into Normalized Device Coordinates (NDC)

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

        // Update uniforms:
        glUniform1f(u_Time, (float)(TimeSpan.FromTicks(DateTime.Now.Ticks - tBegin).TotalSeconds));
        glUniform2f(myCenter, x + w / 2, y + w / 2);

        // Draw
        unsafe
        {
            glPolygonMode(GL_FRONT_AND_BACK, GL_FILL);
            glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, ebo);
            glDrawElements(GL_TRIANGLES, 6, GL_UNSIGNED_INT, NULL);
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
        string vMain   = "gl_Position = vec4(pos, 1.0); gl_Position.x -= 0.01;";

        // Fragment Shader Program
        {
            if (string.IsNullOrEmpty(fHeader))
            {
                // Default implementation
                fHeader = $"out vec4 result;" +
                          $"uniform float uTime;" +
                          $"uniform vec2 myCenter;"
                ;
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
                //fMain = $@"result = vec4(gl_FragCoord.x / {Width}, gl_FragCoord.y / {Height}, sin(uTime * 0.33), 1);";


                fMain = $@"

                    vec2 iResolution = vec2({Width}, {Height});

                    //vec2 uv = (gl_FragCoord.xy - iResolution.xy * 0.5) / iResolution.y;

                    vec2 zzz = vec2((gl_FragCoord.x - (2.0 * myCenter.x * {Width} - 1.0)),
                                   ((gl_FragCoord.y - (1.0 - 2.0 * myCenter.y * {Height}))));

                    if (length(zzz) < 0.75)
                        result = vec4(1, sin(uTime), 1, 1);

                ";

                //result = vec4(gl_FragCoord.x / {Width}, gl_FragCoord.y / {Height}, sin(uTime * 0.33), 1);
            }
        }

        fHeader = $@"out vec4 result;
                    uniform float uTime;
                    uniform vec2 myCenter;
                    vec2 iResolution = vec2({Width}, {Height});
        ";

        fMain = $@"
                    vec2 uv = (gl_FragCoord.xy - iResolution.xy * 0.5) / iResolution.y;

                    //if (gl_FragCoord.x < 112)
                    //if (uv.x < -0.75)
                    //if (uv.x < 0)
                    if (gl_FragCoord.x < 115)
                        result = vec4(1, sin(uTime), 1, 1);
                ";

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

    // Move vertices data from CPU to GPU -- needs to be called each time we change the Rectangle's coordinates
    private unsafe void updateVertices()
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
