using GLFW;
using static OpenGL.GL;
using System;
using System.Drawing;
using System.Drawing.Imaging;



public class myTex : myPrimitive
{
    private static uint vao = 0, vbo = 0, ebo = 0, program = 0, tex = 0;
    private static uint[] indices = null;
    private static float[] vertices = null;
    private static float _angle = 0.0f;

    // -------------------------------------------------------------------------------------------------------------------

    static unsafe void __glGenBuffers()
    {
        fixed (uint* e = &ebo)
        {
            glGenBuffers(1, e);
        }
    }

    static unsafe void __draw()
    {
        glPolygonMode(GL_FRONT_AND_BACK, GL_FILL);
        glDrawElements(GL_TRIANGLES, 6, GL_UNSIGNED_INT, NULL);
    }

    // -------------------------------------------------------------------------------------------------------------------

    public myTex(string path)
    {
        if (vertices == null)
        {
            vertices = new float[] {
                // positions          // colors           // texture coords
                +0.5f,  0.5f, 0.0f,   1.0f, 0.0f, 0.0f,   1.0f, 1.0f,   // top right
                +0.5f, -0.5f, 0.0f,   0.0f, 1.0f, 0.0f,   1.0f, 0.0f,   // bottom right
                -0.5f, -0.5f, 0.0f,   0.0f, 0.0f, 1.0f,   0.0f, 0.0f,   // bottom left
                -0.5f,  0.5f, 0.0f,   1.0f, 1.0f, 0.0f,   0.0f, 1.0f    // top left 
            };

            indices = new uint[]
            {
                0, 1, 3,   // first triangle
                1, 2, 3    // second triangle
            };

            vao = glGenVertexArray();
            vbo = glGenBuffer();
            tex = myOGL.loadTexture(path);

            CreateProgram();

            glBindVertexArray(vao);
            glBindBuffer(GL_ARRAY_BUFFER, vbo);             // vertices

            __glGenBuffers();
            glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, ebo);     // indices

            // ??? wtf ???
            //          glBindTexture(GL_TEXTURE_2D, tex);              // texture
        }
    }

    public myTex(System.Drawing.Bitmap bmp)
    {
        if (vertices == null)
        {
/*
            vertices = new float[] {
                // positions          // colors           // texture coords
                +0.5f,  0.5f, 0.0f,   1.0f, 0.0f, 0.0f,   1.0f, 1.0f,   // top right
                +0.5f, -0.5f, 0.0f,   0.0f, 1.0f, 0.0f,   1.0f, 0.0f,   // bottom right
                -0.5f, -0.5f, 0.0f,   0.0f, 0.0f, 1.0f,   0.0f, 0.0f,   // bottom left
                -0.5f,  0.5f, 0.0f,   1.0f, 1.0f, 0.0f,   0.0f, 1.0f    // top left 
            };
*/

            vertices = new float[] {
                // positions          // colors           // texture coords
                +0.5f,  0.5f, 0.0f,   1.0f, 0.0f, 0.0f,   1.0f, 0.0f,   // top right
                +0.5f, -0.5f, 0.0f,   0.0f, 1.0f, 0.0f,   1.0f, 1.0f,   // bottom right
                -0.5f, -0.5f, 0.0f,   0.0f, 0.0f, 1.0f,   0.0f, 1.0f,   // bottom left
                -0.5f,  0.5f, 0.0f,   1.0f, 1.0f, 0.0f,   0.0f, 0.0f    // top left 
            };

            indices = new uint[]
            {
                0, 1, 3,   // first triangle
                1, 2, 3    // second triangle
            };

            vao = glGenVertexArray();
            vbo = glGenBuffer();
            tex = myOGL.loadTexture(bmp);

            CreateProgram();

            glBindVertexArray(vao);
            glBindBuffer(GL_ARRAY_BUFFER, vbo);             // vertices

            __glGenBuffers();
            glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, ebo);     // indices

            // ??? wtf ???
//          glBindTexture(GL_TEXTURE_2D, tex);              // texture
        }
    }

    // -------------------------------------------------------------------------------------------------------------------

    public void Draw(int x, int y, int w, int h)
    {
        float fx = 0, fy = 0;

        if (_angle == 0)
        {
            // Recalc screen coordinates into Normalized Device Coordinates (NDC)
            fx = 2.0f * x / Width - 1.0f;
            fy = 1.0f - 2.0f * y / Height;
            vertices[24] = fx;          // top left x
            vertices[25] = fy;          // top left y
            vertices[16] = fx;          // bottom left x
            vertices[01] = fy;          // top right y

            fx = 2.0f * (x + w) / Width - 1.0f;
            vertices[00] = fx;          // top right x
            vertices[08] = fx;          // bottom right x

            fy = 1.0f - 2.0f * (y + h) / Height;
            vertices[09] = fy;          // bottom right y
            vertices[17] = fy;          // bottom left y
        }
        else
        {
            // Leave coordinates as they are, and recalc them in the shader
            fx = x;
            fy = y;
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

        CreateVertices();

        glUseProgram(program);

        __draw();
    }

    // -------------------------------------------------------------------------------------------------------------------

    private static void CreateProgram()
    {
        var vertex = myOGL.CreateShader(GL_VERTEX_SHADER,
            @"#version 330 core
                layout (location = 0) in vec3 aPos;
                layout (location = 1) in vec3 aColor;
                layout (location = 2) in vec2 aTexCoord;

                out vec3 ourColor;
                out vec2 TexCoord;

                void main()
                {
                    gl_Position = vec4(aPos, 1.0);
                    ourColor = aColor;
                    TexCoord = aTexCoord;
                }"
        );

        var fragment = myOGL.CreateShader(GL_FRAGMENT_SHADER,
            @"#version 330 core
                    out vec4 FragColor;
  
                    in vec3 ourColor;
                    in vec2 TexCoord;

                    uniform sampler2D ourTexture;

                    void main()
                    {
                        FragColor = texture(ourTexture, TexCoord);
                    }"
        );

        program = glCreateProgram();
        glAttachShader(program, vertex);
        glAttachShader(program, fragment);

        glLinkProgram(program);

        glDeleteShader(vertex);
        glDeleteShader(fragment);

        glUseProgram(program);
    }

    // -------------------------------------------------------------------------------------------------------------------

    private static unsafe void CreateVertices()
    {
        fixed (float* v = &vertices[0])
        {
            glBufferData(GL_ARRAY_BUFFER, sizeof(float) * vertices.Length, v, GL_DYNAMIC_DRAW);
        }

        fixed (uint* i = &indices[0])
        {
            glBufferData(GL_ELEMENT_ARRAY_BUFFER, sizeof(uint) * indices.Length, i, GL_DYNAMIC_DRAW);
        }

        // In this case, Fragment Shader has 3 layout locations;
        // We need to provide all three:

        // position attribute
        glVertexAttribPointer(0, 3, GL_FLOAT, false, 8 * sizeof(float), IntPtr.Zero);
        glEnableVertexAttribArray(0);                                                       // < --------- this is LAYOUT LOCATION #0 in our shader program!

        // color attribute
        glVertexAttribPointer(1, 3, GL_FLOAT, false, 8 * sizeof(float), new IntPtr(3 * sizeof(float)));
        glEnableVertexAttribArray(1);

        // texture coord attribute
        glVertexAttribPointer(2, 2, GL_FLOAT, false, 8 * sizeof(float), new IntPtr(6 * sizeof(float)));
        glEnableVertexAttribArray(2);
    }

    // -------------------------------------------------------------------------------------------------------------------
};
