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
    private static int u_Time, myCenter, myColor;

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
            myColor  = glGetUniformLocation(shaderProgram, "myColor");

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
        // Leave coordinates as they are, and recalc them in the shader
        {
            vertices[06] = x;
            vertices[09] = x;
            vertices[01] = y;
            vertices[10] = y;

            x += w;
            y += h;

            vertices[0] = x;
            vertices[3] = x;
            vertices[4] = y;
            vertices[7] = y;
        }

        updateVertices();

        glUseProgram(shaderProgram);

        // Update uniforms:
        glUniform4f(myColor, _r, _g, _b, _a);
        glUniform1f(u_Time, (float)(TimeSpan.FromTicks(DateTime.Now.Ticks - tBegin).TotalSeconds));
        glUniform2f(myCenter, x - w / 2, y - w / 2);

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
        string vHeader, vMain;

        // Vertex Shader Program
        {
            vHeader = "layout (location=0) in vec3 pos; uniform vec2 myCenter; out vec2 C;";

            // Recalc coordinates in the shader
            vMain = $@"
                gl_Position.x = -1.0 + pos.x * {2.0 / Width };
                gl_Position.y = +1.0 - pos.y * {2.0 / Height};

                C.x = (-1.0 + myCenter.x * {2.0 / Width });
                C.y = (+1.0 - myCenter.y * {2.0 / Height});
            ";
        }

        // Fragment Shader Program
        {
            if (string.IsNullOrEmpty(fHeader))
            {
                // Default implementation
                fHeader = $"out vec4 result;" +
                          $"uniform float uTime;"
                ;
            }
            else
            {
                // Extend the header with some pre-defined variables:
                fHeader = $@"

                    out vec4 result;
                    in vec2 C;
                    uniform float uTime;
                    uniform vec4 myColor;
                    vec2 iResolution = vec2({Width}, {Height});

                    vec2 aspect = vec2(1.0, {1.0 * Height / Width});

                    float circle(vec2 uv, float rad) {{ return smoothstep(rad, rad - 0.005, length(uv)); }}
                ";
            }

            if (string.IsNullOrEmpty(fMain))
            {
                // Default implementation
                fMain = $@"
                ";
            }
            else
            {
                fMain = $@"
           
                    vec2 uv = (gl_FragCoord.xy / iResolution.xy * 2.0 - 1.0);

                    uv -= C;
                    uv *= aspect;

                    float rad = (0.01 + gl_FragCoord.y * 0.00003) - sin(uTime) * 0.003;
                    float c = circle(uv, rad);

                    if (false)
                    {{
                        if (length(uv) <= rad)
                            result = vec4(myColor.xyz, myColor.w);
                    }}
                    else
                    {{
                        if (length(uv) <= c)
                            //result = vec4(myColor.xyz * c, myColor.w);
                            result = vec4(vec3(0.5) * c, myColor.w * c);
                        else
                            result = vec4(vec3(0.3, 0.2, 0.1), 0.0);
                    }}
                ";
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
