using GLFW;
using static OpenGL.GL;
using System;


/*
    https://learnopengl.com/Advanced-OpenGL/Framebuffers

    Texture attached to a framebuffer;
    ALlows rendering to an off-screen texture, instead of the actual screen

    Known issues:
    Does not really render anything until an existing texture is rendered to it.
    The workaround will be to do this before acually using the offscreen renderer:
    
        offScrRenderer.startRendering();
        {
            glDrawBuffer(GL_BACK);
            glClearColor(0, 0, 0, 1);
            glClear(GL_COLOR_BUFFER_BIT);

            var tex = new myTexRectangle(new System.Drawing.Bitmap(gl_Width, gl_Height));

            tex.setOpacity(0);
            tex.Draw(0, 0, gl_Width, gl_Height, 0, 0, gl_Width, gl_Height);

            myPrimitive._Rectangle.SetColor(0.1f, 0.1f, 0.1f, 1);
            myPrimitive._Rectangle.Draw(0, 0, gl_Width, gl_Height, true);
        }
        offScrRenderer.stopRendering();
*/


public class myTexRectangle_Renderer : myPrimitive
{
    // Vbo (Vertex Buffer Object) -- Manages memory buffer on the GPU
    // Ebo (Element Buffer Object) is a buffer that stores indices that are used to decide what vertices to draw (and in what order)
    // Fbo (Frame Buffer Object ) -- provides an additional target to render to

    private uint fbo = 0, tex = 0;
    private uint vbo = 0, ebo = 0, shaderProgram = 0;

    private float[] vertices = null;

    private const int verticesLength = 12;
    private const int sizeofFloat_x_verticesLength = sizeof(float) * verticesLength;

    // -------------------------------------------------------------------------------------------------------------------

    public myTexRectangle_Renderer()
    {
        if (vertices == null)
        {
            initVertices();

            CreateProgram();
            glUseProgram(shaderProgram);

            vbo = glGenBuffer();
            ebo = glGenBuffer();

            tex = glGenTexture();
            fbo = glGenFramebuffer();

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

    // Cal this method to make off-screen fbo buffer active;
    // All the next write framebuffer operations will affect the currently bound framebuffer
    public void startRendering()
    {
        unsafe
        {
            // By binding to the GL_FRAMEBUFFER target
            // all the next read and write framebuffer operations will affect the currently bound framebuffer
            glBindFramebuffer(GL_FRAMEBUFFER, fbo);
            glBindTexture(GL_TEXTURE_2D, tex);

            // The same call as in 'void loadTexture(uint tex, Bitmap bmp)' -- to be able to use transparency
            glTexImage2D(GL_TEXTURE_2D, 0, GL_RGBA8, Width, Height, 0, GL_BGRA, GL_UNSIGNED_BYTE, null);

            glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, GL_LINEAR);
            glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, GL_LINEAR);
            glBindTexture(GL_TEXTURE_2D, 0);

            glFramebufferTexture2D(GL_FRAMEBUFFER, GL_COLOR_ATTACHMENT0, GL_TEXTURE_2D, tex, 0);

            // if (glCheckFramebufferStatus(GL_FRAMEBUFFER) != GL_FRAMEBUFFER_COMPLETE) { ; }
        }

        return;
    }

    // -------------------------------------------------------------------------------------------------------------------

    // Call this method to unbind the off-screen buffer
    public void stopRendering()
    {
        glBindFramebuffer(GL_FRAMEBUFFER, 0);
    }

    // -------------------------------------------------------------------------------------------------------------------

    // Render the texture on the screen.
    //  x, y, w, h          -- rectangle on the screen to fill with texture
    //  ptx, pty, ptw, pth  -- optional rectangle to sample pixels from
    // - In case ptw is '0', the whole texture is rendered
    // - In case ptw is not '0', only part of the texture is rendered
    // - Using negative ptw/pth, it is possible to flip/rotate/mirror the texture
    public void Draw(int x, int y, int w, int h)
    {
        glUseProgram(shaderProgram);

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

            // All upcoming GL_TEXTURE_2D operations now have effect on this texture object
            glBindTexture(GL_TEXTURE_2D, tex);
            glPolygonMode(GL_FRONT_AND_BACK, GL_FILL);
            glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, ebo);
            glDrawElements(GL_TRIANGLES, 6, GL_UNSIGNED_INT, NULL);
        }

        return;
    }

    // -------------------------------------------------------------------------------------------------------------------

    // Create a shader program
    private void CreateProgram()
    {
        var vertex = myOGL.CreateShaderEx(GL_VERTEX_SHADER,
            @"layout (location = 0) in vec3 pos;
              layout (location = 1) in vec3 color;
              layout (location = 2) in vec2 txCoord;
                out vec4 fragColor; out vec2 fragTxCoord;",

                main: @"fragColor = vec4(color, 1.0);
                        gl_Position = vec4(pos, 1.0);
                        fragTxCoord = txCoord;"
        );

        var fragment = myOGL.CreateShaderEx(GL_FRAGMENT_SHADER,
            "out vec4 result;" +
                "in vec4 fragColor;" +
                "in vec2 fragTxCoord;" +
                "uniform sampler2D myTexture;",
                main: "result = texture(myTexture, fragTxCoord) * fragColor;"
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

    // Move indices data from CPU to GPU -- needs to be called only once, as we have 1 EBO, and it is not going to change;
    // The EBO must be activated prior to drawing the shape: glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, ebo);
    private unsafe void updateIndices()
    {
        int usage = GL_STATIC_DRAW;

        glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, ebo);
        {
            var indices = new uint[]
            {
                0, 1, 3,   // first triangle
                1, 2, 3    // second triangle
            };

            fixed (uint* i = &indices[0])
                glBufferData(GL_ELEMENT_ARRAY_BUFFER, sizeof(uint) * indices.Length, i, usage);

            // Unbind current buffer
            glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, 0);
        }
    }

    // -------------------------------------------------------------------------------------------------------------------

    // Initialize the array of vertices
    private void initVertices()
    {
        if (vertices == null)
        {
            vertices = new float[] {
                // positions          // colors           // texture coords
                +0.5f,  0.5f, 0.0f,   1.0f, 1.0f, 1.0f,   1.0f, 0.0f,   // top right
                +0.5f, -0.5f, 0.0f,   1.0f, 1.0f, 1.0f,   1.0f, 1.0f,   // bottom right
                -0.5f, -0.5f, 0.0f,   1.0f, 1.0f, 1.0f,   0.0f, 1.0f,   // bottom left
                -0.5f,  0.5f, 0.0f,   1.0f, 1.0f, 1.0f,   0.0f, 0.0f    // top left 
            };
        }
    }

    // -------------------------------------------------------------------------------------------------------------------
};
