﻿using GLFW;
using static OpenGL.GL;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using my;

// todo:
// need 2 types of rotation:
//  - rotating the shape, but the texture stays
//  -- rotating the shape, the texture rotates as well

public class myTexRectangle : myPrimitive
{
    // Vbo (Vertex Buffer Object) -- Manages memory buffer on the GPU
    // Ebo (Element Buffer Object) is a buffer that stores indices that are used to decide what vertices to draw (and in what order)

    protected static uint vbo = 0, ebo = 0, shaderProgram = 0;
    private static float[] vertices = null;
    private static float _angle = 0, dx = 0, dy = 0;
    private static int locationColor = 0, locationAngle = 0, locationCenter = 0, locationOpacity = 0, locationPart = 0;

    private uint tex = 0;
    private float _opacity = 1.0f;

    // -------------------------------------------------------------------------------------------------------------------

    // By using the default params, we can provide custom shader implementations
    public myTexRectangle(string path, string vertHead = "", string vertMain = "", string fragHead = "", string fragMain = "")
    {
        if (vertices == null)
        {
            initVertices();

            dx = 1.0f / Width;
            dy = 1.0f / Height;

            shaderProgram = CreateShader(vertHead, vertMain, fragHead, fragMain);
            glUseProgram(shaderProgram);

            locationColor   = glGetUniformLocation(shaderProgram, "myColor");
            locationAngle   = glGetUniformLocation(shaderProgram, "myAngle");
            locationCenter  = glGetUniformLocation(shaderProgram, "myCenter");
            locationOpacity = glGetUniformLocation(shaderProgram, "myOpacity");
            locationPart    = glGetUniformLocation(shaderProgram, "myPart");

            vbo = glGenBuffer();
            ebo = glGenBuffer();
            tex = glGenTexture();

            updateIndices();
        }

        myOGL.loadTexture(tex, path);
    }

    // -------------------------------------------------------------------------------------------------------------------

    // By using the default params, we can provide custom shader implementations
    public myTexRectangle(System.Drawing.Bitmap bmp, string vertHead = "", string vertMain = "", string fragHead = "", string fragMain = "")
    {
        if (vertices == null)
        {
            initVertices();

            dx = 1.0f / Width;
            dy = 1.0f / Height;

            shaderProgram = CreateShader(vertHead, vertMain, fragHead, fragMain);
            glUseProgram(shaderProgram);

            locationColor   = glGetUniformLocation(shaderProgram, "myColor");
            locationAngle   = glGetUniformLocation(shaderProgram, "myAngle");
            locationCenter  = glGetUniformLocation(shaderProgram, "myCenter");
            locationOpacity = glGetUniformLocation(shaderProgram, "myOpacity");
            locationPart    = glGetUniformLocation(shaderProgram, "myPart");

            vbo = glGenBuffer();
            ebo = glGenBuffer();
            tex = glGenTexture();

            updateIndices();
        }

        myOGL.loadTexture(tex, bmp);
    }

    // -------------------------------------------------------------------------------------------------------------------

    // Replace the image in the texture
    public void reloadImg(Bitmap bmp)
    {
        myOGL.loadTexture(tex, bmp);
    }

    // -------------------------------------------------------------------------------------------------------------------

    // When using the off-screen renderer, we need this call to draw the rest of the frame (drawn over the renderer);
    // todo: see what we're missing and fix it
    public void UpdateVertices__WorkaroundTmp()
    {
        updateVertices();
    }

    // -------------------------------------------------------------------------------------------------------------------

    /// <summary>
    /// Render the texture on the screen.
    ///  x, y, w, h          -- rectangle on the screen to fill with texture
    ///  ptx, pty, ptw, pth  -- optional rectangle to sample pixels from
    /// - In case ptw is '0', the whole texture is rendered
    /// - In case ptw is not '0', only part of the texture is rendered
    /// - Using negative ptw/pth, it is possible to flip/rotate/mirror the texture
    /// </summary>
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
            vertices[24] = fx;
            vertices[25] = fy;
            vertices[16] = fx;
            vertices[01] = fy;

            fx = x + w;
            vertices[00] = fx;
            vertices[08] = fx;

            fy = y + h;
            vertices[09] = fy;
            vertices[17] = fy;
        }

        updateVertices();

        glUseProgram(shaderProgram);

        glUniform4f(locationPart, ptx, pty, ptw, pth);
        glUniform1f(locationOpacity, _opacity);
        glUniform1f(locationAngle, _angle);

        // Set the center of rotation
        if (_angle != 0.0f)
        {
            glUniform2f(locationCenter, x + w / 2, y + h / 2);
        }

        unsafe
        {
            // All upcoming GL_TEXTURE_2D operations now have effect on this texture object
            glBindTexture(GL_TEXTURE_2D, tex);
            glPolygonMode(GL_FRONT_AND_BACK, GL_FILL);
            //glPolygonMode(GL_FRONT_AND_BACK, GL_LINE);
            //glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_S, GL_MIRRORED_REPEAT);
            glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, ebo);
            glDrawElements(GL_TRIANGLES, 6, GL_UNSIGNED_INT, NULL);
        }

        return;
    }

    // -------------------------------------------------------------------------------------------------------------------

    public float getOpacity()
    {
        return _opacity;
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

    public void setAngle(float angle)
    {
        _angle = angle;
    }

    // -------------------------------------------------------------------------------------------------------------------

    public void setAngle(double angle)
    {
        _angle = (float)angle;
    }

    // -------------------------------------------------------------------------------------------------------------------

    public void setColor(float r, float g, float b)
    {
        vertices[3] = vertices[11] = vertices[19] = vertices[27] = r;
        vertices[4] = vertices[12] = vertices[20] = vertices[28] = g;
        vertices[5] = vertices[13] = vertices[21] = vertices[29] = b;
    }

    // -------------------------------------------------------------------------------------------------------------------

    // Create a shader program
    private uint CreateShader(string _vertHead, string _vertMain, string _fragHead, string _fragMain)
    {
        string vertHead =
            @"layout (location = 0) in vec3 pos;
              layout (location = 1) in vec3 color;
              layout (location = 2) in vec2 txCoord;
                out vec4 fragColor; out vec2 fragTxCoord;
                uniform vec4 myPart; uniform float myOpacity; uniform float myAngle; uniform vec2 myCenter;
                vec2 sc = vec2(sin(myAngle), cos(myAngle));
            ";

        string vertMain =
            $@" fragColor = vec4(color, myOpacity);

                if (myAngle == 0)
                {{
                    gl_Position = vec4(pos, 1.0);
                }}
                else
                {{
                    vec2 p = pos.xy - myCenter;

                    gl_Position = vec4(p.x * sc.y - p.y * sc.x, p.y * sc.y + p.x * sc.x, pos.z, 1.0);
                    gl_Position.xy += myCenter.xy;

                    gl_Position.x = gl_Position.x * {2.0 / Width} - 1.0f;
                    gl_Position.y = 1.0f - gl_Position.y * {2.0 / Height};
                }}

                fragTxCoord = myPart.z == 0
                    ? txCoord
                    {"" /* This way, we are able to render just a part of a texture */ }
                    {"" /* There's a problem, however: this does not really work if the texture's size is less than the screen size */ }
                    {"" /* In this case, we can do it like that: tex.Draw(0, 0, 33, bmpHeight, 0, 0, 33 * gl_Width / bmpWidth, bmpHeight * gl_Height / bmpHeight); */ }
                    : vec2({ 1.0 / Width } * (myPart.x + txCoord.x * myPart.z), { 1.0 / Height } * (myPart.y + txCoord.y * myPart.w));
                ";

        string fragHead =
                $@"out vec4 result;
                    in vec4 fragColor;
                    in vec2 fragTxCoord;
                    uniform sampler2D myTexture;
            ";

        string fragMain =
            "result = texture(myTexture, fragTxCoord) * fragColor;";

        // Replace default implementations with user-defined ones
        if (_vertHead != string.Empty)
            vertHead = _vertHead;

        if (_vertMain != string.Empty)
            vertMain = _vertMain;

        if (_fragHead != string.Empty)
            fragHead = _fragHead;

        if (_fragMain != string.Empty)
            fragMain = _fragMain;

        return CreateProgram(vertHead, vertMain, fragHead, fragMain);
    }

    // -------------------------------------------------------------------------------------------------------------------

/*
    Ask AI (they have some clever suggestions about it):
        - Do I need to call glBufferData each time I draw a frame?
            https://stackoverflow.com/questions/32222574/is-it-better-glbuffersubdata-or-glmapbuffer
*/

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

            // layout (location = 1) -- color attribute
            glEnableVertexAttribArray(1);
            glVertexAttribPointer(1, 3, GL_FLOAT, false, 8 * sizeof(float), new IntPtr(3 * sizeof(float)));

            // layout (location = 2) -- texture coordinate attribute (txCoord)
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
