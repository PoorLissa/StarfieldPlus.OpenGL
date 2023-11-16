using GLFW;
using static OpenGL.GL;
using System;


public class myPentagon : myPrimitive
{
    private static float[] vertices = null;

    private static uint vbo = 0, ebo_fill = 0, ebo_outline = 0, shaderProgram = 0;
    private static int locationColor = 0, locationAngle = 0, locationCenter = 0, locationScrSize = 0;
    private static float _angle;

    private static float h_div_w = 0;
    private static float sin18 = (float)(Math.Sin(18.0 * Math.PI / 180.0));
    private static float cos18 = (float)(Math.Cos(18.0 * Math.PI / 180.0));
    private static float sin54 = (float)(Math.Sin(54.0 * Math.PI / 180.0));
    private static float cos54 = (float)(Math.Cos(54.0 * Math.PI / 180.0));

    // -------------------------------------------------------------------------------------------------------------------

    public myPentagon()
    {
        if (vertices == null)
        {
            h_div_w = (float)Height / (float)Width;

            vertices = new float[15];

            for (int i = 0; i < 15; i++)
                vertices[i] = 0.0f;

            shaderProgram = CreateShader();
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
        float fx = x, fy = y;

        if (_angle == 0)
        {
            // Recalc screen coordinates into Normalized Device Coordinates (NDC)
            fx = 2.0f * x / Width - 1.0f;
            fy = 1.0f - 2.0f * y / Height;

            float fr = 2.0f * r / Height;           // Radius
            float frx = fr * h_div_w;               // Radius adjusted for x-coordinate

            vertices[00] = fx;
            vertices[01] = fy + fr;

            vertices[03] = fx + frx * cos18;
            vertices[04] = fy + fr  * sin18;

            vertices[06] = fx + frx * cos54;
            vertices[07] = fy - fr  * sin54;

            vertices[09] = fx - frx * cos54;
            vertices[10] = fy - fr  * sin54;

            vertices[12] = fx - frx * cos18;
            vertices[13] = fy + fr  * sin18;
        }
        else
        {
            // Leave coordinates as they are, and recalc them in the shader
            vertices[00] = fx;
            vertices[01] = fy + r;

            vertices[03] = fx + r * cos18;
            vertices[04] = fy + r * sin18;

            vertices[06] = fx + r * cos54;
            vertices[07] = fy - r * sin54;

            vertices[09] = fx - r * cos54;
            vertices[10] = fy - r * sin54;

            vertices[12] = fx - r * cos18;
            vertices[13] = fy + r * sin18;
        }

        updateVertices(doFill);

        glUseProgram(shaderProgram);
        setColor(locationColor, _r, _g, _b, _a);
        glUniform1f(locationAngle, _angle);

        // Set the center of rotation
        if (_angle != 0.0f)
        {
            glUniform2f(locationCenter, x, y);
            updUniformScreenSize(locationScrSize, Width, Height);
        }

        unsafe
        {
            if (doFill)
            {
                glPolygonMode(GL_FRONT_AND_BACK, GL_FILL);
                glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, ebo_fill);
                glDrawElements(GL_TRIANGLES, 12, GL_UNSIGNED_INT, NULL);
            }
            else
            {
                glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, ebo_outline);
                glDrawElements(GL_LINES, 15, GL_UNSIGNED_INT, NULL);
            }
        }
    }

    // -------------------------------------------------------------------------------------------------------------------

    public void SetAngle(float angle)
    {
        _angle = angle;
    }

    // -------------------------------------------------------------------------------------------------------------------

    private static uint CreateShader()
    {
        string vertHead =
            @"layout (location = 0) in vec3 pos;
                uniform float myAngle; uniform vec2 myCenter;
                vec2 sc = vec2(sin(myAngle), cos(myAngle));
            ";

        string vertMain =
            $@" if (myAngle == 0)
                {{
                    gl_Position = vec4(pos, 1.0);
                }}
                else
                {{
                    vec2 POS = vec2(pos.x - myCenter.x, pos.y - myCenter.y);

                    gl_Position = vec4(POS.x * sc.y - POS.y * sc.x, POS.y * sc.y + POS.x * sc.x, pos.z, 1.0);

                    gl_Position.x += myCenter.x;
                    gl_Position.y += myCenter.y;

                    gl_Position.x = gl_Position.x * {2.0 / (Width + 1)} - 1.0f;
                    gl_Position.y = 1.0f - gl_Position.y * {2.0 / Height};
                }}
            ";

        string fragHead =
            "out vec4 result; uniform vec4 myColor;";

        string fragMain =
            "result = myColor;";

        return CreateProgram(vertHead, vertMain, fragHead, fragMain);
    }

    // -------------------------------------------------------------------------------------------------------------------

    // Move indices data from CPU to GPU -- needs to be called only once, as we have 2 different EBOs, and they are not going to change;
    // The EBO must be activated prior to drawing the shape: glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, doFill ? ebo1 : ebo2);
    private static unsafe void updateIndices()
    {
        int usage = GL_STATIC_DRAW;

        glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, ebo_fill);
        {
            // 3 triangles
            var indicesFill = new uint[]
            {
                0, 1, 4,
                1, 3, 4,
                1, 2, 3
            };

            fixed (uint* i = &indicesFill[0])
                glBufferData(GL_ELEMENT_ARRAY_BUFFER, sizeof(uint) * indicesFill.Length, i, usage);

            // Unbind current buffer
            glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, 0);
        }

        glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, ebo_outline);
        {
            // 5 lines
            var indicesOutline = new uint[]
            {
                0, 1,
                1, 2,
                2, 3,
                3, 4,
                4, 0
            };

            fixed (uint* i = &indicesOutline[0])
                glBufferData(GL_ELEMENT_ARRAY_BUFFER, sizeof(uint) * indicesOutline.Length, i, usage);

            // Unbind current buffer
            glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, 0);
        }
    }

    // -------------------------------------------------------------------------------------------------------------------

    private static unsafe void updateVertices(bool doFill)
    {
        glBindBuffer(GL_ARRAY_BUFFER, vbo);
        {
            fixed (float* v = &vertices[0])
                glBufferData(GL_ARRAY_BUFFER, sizeof(float) * vertices.Length, v, GL_DYNAMIC_DRAW);

            glVertexAttribPointer(0, 3, GL_FLOAT, false, 3 * sizeof(float), NULL);
            glEnableVertexAttribArray(0);
        }
    }

    // -------------------------------------------------------------------------------------------------------------------
};
