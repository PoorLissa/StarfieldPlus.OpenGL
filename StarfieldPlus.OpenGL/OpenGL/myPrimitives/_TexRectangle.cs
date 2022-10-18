using GLFW;
using static OpenGL.GL;
using System;
using System.Drawing;
using System.Drawing.Imaging;

// todo:
// need 2 types of rotation:
//  - rotating the shape, but the texture stays
//  -- rotating the shape, the texture rotates as well

public class myTexRectangle : myPrimitive
{
    // Vbo (Vertex Buffer Object) -- Manages memory buffer on the GPU
    // Ebo (Element Buffer Object) is a buffer that stores indices that are used to decide what vertices to draw (and in what order)

    private static uint vbo = 0, ebo = 0, shaderProgram = 0;
    private static float[] vertices = null;
    private static float _angle = 0, dx = 0, dy = 0;
    private static int locationColor = 0, locationAngle = 0, locationOpacity = 0, locationScrSize = 0, locationPart = 0;

    private uint tex = 0;
    private float _opacity = 1.0f;

    // -------------------------------------------------------------------------------------------------------------------

    public myTexRectangle(string path)
    {
        if (vertices == null)
        {
            initVertices();

            dx = 1.0f / Width;
            dy = 1.0f / Height;

            CreateProgram();
            glUseProgram(shaderProgram);
            locationColor   = glGetUniformLocation(shaderProgram, "myColor");
            locationAngle   = glGetUniformLocation(shaderProgram, "myAngle");
            locationOpacity = glGetUniformLocation(shaderProgram, "myOpacity");

            locationPart    = glGetUniformLocation(shaderProgram, "myPart");
            locationScrSize = glGetUniformLocation(shaderProgram, "myScrDxDy");

            vbo = glGenBuffer();
            ebo = glGenBuffer();
            tex = glGenTexture();

            updateIndices();
        }

        myOGL.loadTexture(tex, path);
    }

    // -------------------------------------------------------------------------------------------------------------------

    public myTexRectangle(System.Drawing.Bitmap bmp)
    {
        if (vertices == null)
        {
            initVertices();

            dx = 1.0f / Width;
            dy = 1.0f / Height;

            CreateProgram();
            glUseProgram(shaderProgram);
            locationColor   = glGetUniformLocation(shaderProgram, "myColor");
            locationAngle   = glGetUniformLocation(shaderProgram, "myAngle");
            locationOpacity = glGetUniformLocation(shaderProgram, "myOpacity");

            locationPart    = glGetUniformLocation(shaderProgram, "myPart");
            locationScrSize = glGetUniformLocation(shaderProgram, "myScrDxDy");

            vbo = glGenBuffer();
            ebo = glGenBuffer();
            tex = glGenTexture();

            updateIndices();
        }

        myOGL.loadTexture(tex, bmp);
    }

    // -------------------------------------------------------------------------------------------------------------------

    // Render the texture on the screen.
    //  x, y, w, h          -- rectangle on the screen to fill with texture
    //  ptx, pty, ptw, pth  -- optional rectangle to sample pixels from
    // In case ptw is '0', the whole texture is rendered
    // In case ptw is not '0', only part of the texture is rendered
    public void Draw(int x, int y, int w, int h, int ptx = 0, int pty = 0, int ptw = 0, int pth = 0)
    {
        unsafe void __draw()
        {
            // All upcoming GL_TEXTURE_2D operations now have effect on this texture object
            glBindTexture(GL_TEXTURE_2D, tex);

            glPolygonMode(GL_FRONT_AND_BACK, GL_FILL);
            glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, ebo);
            glDrawElements(GL_TRIANGLES, 6, GL_UNSIGNED_INT, NULL);
        }

        // ---------------------------------------------------------------------------------------

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

        updateVertices();

        glUseProgram(shaderProgram);

        glUniform4f(locationPart, ptx, pty, ptw, pth);
        glUniform2f(locationScrSize, dx, dy);
        glUniform1f(locationOpacity, _opacity);

        __draw();

        return;
    }

    // -------------------------------------------------------------------------------------------------------------------

    public void setOpacity(float val)
    {
        _opacity = val;
    }

    // -------------------------------------------------------------------------------------------------------------------

    public void setOpacity(double val)
    {
        _opacity = (float)val;
    }

    // -------------------------------------------------------------------------------------------------------------------

    // Create a shader program
    private void CreateProgram()
    {
        var vertex = myOGL.CreateShaderEx(GL_VERTEX_SHADER,
            @"layout (location = 0) in vec3 pos;
              layout (location = 1) in vec3 color;
              layout (location = 2) in vec2 txCoord;
                out vec4 fragColor; out vec2 fragTxCoord;
                uniform vec4 myPart; uniform vec2 myScrDxDy; uniform float myOpacity;",

                main: @"gl_Position = vec4(pos, 1.0); fragColor = vec4(color, myOpacity);

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

            // In this case, Fragment Shader has 3 layout locations;
            // We need to provide all the three:

            // These values are changed by instanced rendering (which breaks our texture rendering)
            // To avoid this, set the values to zeroes
            glVertexAttribDivisor(0, 0);
            glVertexAttribDivisor(1, 0);
            glVertexAttribDivisor(2, 0);

            // layout (location = 0) -- position attribute
            glEnableVertexAttribArray(0);
            glVertexAttribPointer(0, 3, GL_FLOAT, false, 8 * sizeof(float), IntPtr.Zero);

            // layout(location = 1) -- color attribute
            glEnableVertexAttribArray(1);
            glVertexAttribPointer(1, 3, GL_FLOAT, false, 8 * sizeof(float), new IntPtr(3 * sizeof(float)));

            // layout (location = 2) -- texture coordinate attribute
            glEnableVertexAttribArray(2);
            glVertexAttribPointer(2, 2, GL_FLOAT, false, 8 * sizeof(float), new IntPtr(6 * sizeof(float)));
        }
    }

    // -------------------------------------------------------------------------------------------------------------------

    // Move indices data from CPU to GPU -- needs to be called only once, as we have 1 EBO, and it is not going to change;
    // The EBO must be activated prior to drawing the shape: glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, ebo);
    private static unsafe void updateIndices()
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
