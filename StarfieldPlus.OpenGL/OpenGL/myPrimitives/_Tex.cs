using GLFW;
using static OpenGL.GL;
using System;
using System.Drawing;
using System.Drawing.Imaging;



public class myTex : myPrimitive
{
    private static uint vao = 0, vbo = 0, ebo = 0, program = 0;
    private static uint[] indices = null;
    private static float[] vertices = null;
    private static float dx = 0, dy = 0;
    private static int locationPart = 0, locationScrSize = 0;

    private uint  _tex = 0;
    private float _angle = 0.0f;

    // -------------------------------------------------------------------------------------------------------------------

    unsafe void __glGenBuffers_EBO()
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

    private void initArrays()
    {
        if (vertices == null)
        {
            vertices = new float[] {
                // positions          // colors           // texture coords
                +0.5f,  0.5f, 0.0f,   1.0f, 0.0f, 0.0f,   1.0f, 0.0f,   // top right
                +0.5f, -0.5f, 0.0f,   0.0f, 1.0f, 0.0f,   1.0f, 1.0f,   // bottom right
                -0.5f, -0.5f, 0.0f,   0.0f, 0.0f, 1.0f,   0.0f, 1.0f,   // bottom left
                -0.5f,  0.5f, 0.0f,   1.0f, 1.0f, 0.0f,   0.0f, 0.0f    // top left 
            };

            vao = glGenVertexArray();   // Generate VAO name
            vbo = glGenBuffer();        // Generate VBO name
            __glGenBuffers_EBO();       // Generate EBO name

            dx = 1.0f / Width;
            dy = 1.0f / Height;
        }

        if (indices == null)
        {
            indices = new uint[]
            {
                0, 1, 3,   // first triangle
                1, 2, 3    // second triangle
            };
        }

        _tex = glGenTexture();
    }

    private void initTheRest()
    {
        CreateProgram();

        locationPart = glGetUniformLocation(program, "myPart");
        locationScrSize = glGetUniformLocation(program, "myScrDxDy");

        glBindVertexArray(vao);
        glBindBuffer(GL_ARRAY_BUFFER, vbo);             // vertices

        glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, ebo);     // indices
    }

    // -------------------------------------------------------------------------------------------------------------------

    public myTex(string path)
    {
        initArrays();
        myOGL.loadTexture(_tex, path);
        initTheRest();
    }

    public myTex(System.Drawing.Bitmap bmp)
    {
        initArrays();
        myOGL.loadTexture(_tex, bmp);
        initTheRest();
    }

    // -------------------------------------------------------------------------------------------------------------------

    // Render the texture on the screen.
    //  x, y, w, h          -- rectangle on the screen to fill with texture
    //  ptx, pty, ptw, pth  -- optional rectangle to sample pixels from
    // In case ptw is '0', the whole texture is rendered
    // In case ptw is not '0', only part of the texture is rendered
    public void Draw(int x, int y, int w, int h, int ptx = 0, int pty = 0, int ptw = 0, int pth = 0)
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

        // All upcoming GL_TEXTURE_2D operations now have effect on this texture object
        glBindTexture(GL_TEXTURE_2D, _tex);

        glUseProgram(program);

        glUniform4f(locationPart, ptx, pty, ptw, pth);
        glUniform2f(locationScrSize, dx, dy);

        __draw();
    }

    // -------------------------------------------------------------------------------------------------------------------

    private void CreateProgram()
    {
        var vertex = myOGL.CreateShaderEx(GL_VERTEX_SHADER,
            @"layout (location = 0) in vec3 pos; layout (location = 1) in vec3 color; layout (location = 2) in vec2 txCoord;
              out vec3 fragColor; out vec2 fragTxCoord;
              uniform vec4 myPart; uniform vec2 myScrDxDy;",
                main: @"gl_Position = vec4(pos, 1.0); fragColor = color;

                        if (myPart.z == 0)
                        {
                            fragTxCoord = txCoord;
                        }
                        else
                        {
                            // This way, we are able to render just a part of a texture
                            fragTxCoord = vec2(myScrDxDy.x * (myPart.x + txCoord.x * myPart.z), myScrDxDy.y * (myPart.y + txCoord.y * myPart.w));
                        }"
        );

        // fragColor is not used now, but we could use it like that, for example:
        // result = texture(ourTexture, texCoord) * vec4(fragColor, 1.0) -- for some color effect
        // OR
        // result = texture(myTexture, fragTxCoord) * vec4(1, 1, 1, 0.9) -- for a transparency effect;
        var fragment = myOGL.CreateShaderEx(GL_FRAGMENT_SHADER,
            "out vec4 result; in vec3 fragColor; in vec2 fragTxCoord; uniform sampler2D myTexture;",
                main: "result = texture(myTexture, fragTxCoord);"
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
        // We need to provide all the three:

        // layout (location = 0) -- position attribute
        glVertexAttribPointer(0, 3, GL_FLOAT, false, 8 * sizeof(float), IntPtr.Zero);
        glEnableVertexAttribArray(0);

        // layout(location = 1) -- color attribute
        glVertexAttribPointer(1, 3, GL_FLOAT, false, 8 * sizeof(float), new IntPtr(3 * sizeof(float)));
        glEnableVertexAttribArray(1);

        // layout (location = 2) -- texture coordinate attribute
        glVertexAttribPointer(2, 2, GL_FLOAT, false, 8 * sizeof(float), new IntPtr(6 * sizeof(float)));
        glEnableVertexAttribArray(2);
    }

    // -------------------------------------------------------------------------------------------------------------------
};
