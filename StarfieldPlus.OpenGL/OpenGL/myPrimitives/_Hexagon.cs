using GLFW;
using static OpenGL.GL;
using System;


public class myHexagon : myPrimitive
{
    private static uint vao = 0, vbo = 0, ebo = 0, program = 0;
    private static uint[] indicesOutline = null;
    private static uint[] indicesFill = null;
    private static float[] vertices = null;
    private static int locationColor = 0, locationAngle = 0, locationCenter = 0, locationScrSize = 0;
    private static float sqrt3_div2 = 0;
    private static float h_div_w = 0;
    private static float _angle;

    private static bool bbb = false;

    public myHexagon()
    {
        unsafe void __glGenBuffers()
        {
            fixed (uint* e = &ebo)
            {
                glGenBuffers(1, e);
            }
        }

        // ---------------------------------------------------------------------------------------

        if (vertices == null)
        {
            sqrt3_div2 = (float)(Math.Sqrt(3.0) / 2.0);
            h_div_w = (float)Height / (float)Width;

            vertices = new float[18];

            for (int i = 0; i < 18; i++)
                vertices[i] = 0.0f;

            indicesOutline = new uint[]
            {
                0, 1,
                1, 2,
                2, 3,
                3, 4,
                4, 5,
                5, 0
            };

            // 4 triangles
            indicesFill = new uint[]
            {
                0, 1, 3,
                1, 2, 3,
                0, 4, 3,
                0, 5, 4
            };

            vao = glGenVertexArray();
            vbo = glGenBuffer();

            CreateProgram();
            locationColor   = glGetUniformLocation(program, "myColor");
            locationAngle   = glGetUniformLocation(program, "myAngle");
            locationCenter  = glGetUniformLocation(program, "myCenter");
            locationScrSize = glGetUniformLocation(program, "myScrSize");

            glBindVertexArray(vao);
            glBindBuffer(GL_ARRAY_BUFFER, vbo);

            // Ebo is used with glDrawElements (with the array of indices).
            // glDrawArrays does not need this
            __glGenBuffers();
            glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, ebo);
        }
    }

    public void Draw(float x, float y, float r, bool doFill = false)
    {
        Draw((int)x, (int)y, (int)r, doFill);
    }

    public void Draw(int x, int y, int r, bool doFill = false)
    {
        unsafe void __draw(bool fill)
        {
            if (fill)
            {
                glPolygonMode(GL_FRONT_AND_BACK, GL_FILL);
                glDrawElements(GL_TRIANGLES, 12, GL_UNSIGNED_INT, NULL);
            }
            else
            {
                glDrawElements(GL_LINES, 18, GL_UNSIGNED_INT, NULL);
            }
        }

        // ---------------------------------------------------------------------------------------

        float fx, fy;

        if (_angle == 0)
        {
            // Recalc screen coordinates into Normalized Device Coordinates (NDC)
            fx = 2.0f * x / Width - 1.0f;
            fy = 1.0f - 2.0f * y / Height;

            float fr = 2.0f * r / Height;           // Radius
            float frx = fr * h_div_w;               // Radius adjusted for x-coordinate

            float frx_sqrt = fr * sqrt3_div2;
            float frx_half = frx * 0.5f;

            vertices[00] = fx - frx;
            vertices[01] = fy;
            vertices[03] = fx - frx_half;
            vertices[04] = fy + frx_sqrt;
            vertices[06] = fx + frx_half;
            vertices[07] = fy + frx_sqrt;
            vertices[09] = fx + frx;
            vertices[10] = fy;
            vertices[12] = fx + frx_half;
            vertices[13] = fy - frx_sqrt;
            vertices[15] = fx - frx_half;
            vertices[16] = fy - frx_sqrt;
        }
        else
        {
            // Leave coordinates as they are, and recalc them in the shader
            fx = x;
            fy = y;

            float fr = r;                           // Radius
            float frx = fr;                         // Radius adjusted for x-coordinate

            float frx_sqrt = fr * sqrt3_div2;
            float frx_half = frx * 0.5f;

            vertices[00] = fx - frx;
            vertices[01] = fy;
            vertices[03] = fx - frx_half;
            vertices[04] = fy + frx_sqrt;
            vertices[06] = fx + frx_half;
            vertices[07] = fy + frx_sqrt;
            vertices[09] = fx + frx;
            vertices[10] = fy;
            vertices[12] = fx + frx_half;
            vertices[13] = fy - frx_sqrt;
            vertices[15] = fx - frx_half;
            vertices[16] = fy - frx_sqrt;
        }

        CreateVertices(doFill);

        glUseProgram(program);
        setColor(locationColor, _r, _g, _b, _a);
        setAngle(locationAngle, _angle);

        // Set the center of rotation
        if (_angle != 0.0f)
        {
            glUniform2f(locationCenter, x, y);
            updUniformScreenSize(locationScrSize, Width, Height);
        }

        __draw(doFill);
    }

    public void SetAngle(float angle)
    {
        _angle = angle;
    }

    private static void setAngle(int location, float angle)
    {
        glUniform1f(location, angle);
    }

    private static void CreateProgram()
    {
        var vertex = myOGL.CreateShaderEx(GL_VERTEX_SHADER,
            "layout (location = 0) in vec3 pos; uniform float myAngle; uniform vec2 myCenter; uniform ivec2 myScrSize;",
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

                            gl_Position.x = 2.0f * gl_Position.x / (myScrSize.x + 1) - 1.0f;
                            gl_Position.y = 1.0f - 2.0f * gl_Position.y / myScrSize.y;
                        }"
        );

        var fragment = myOGL.CreateShaderEx(GL_FRAGMENT_SHADER,
            "out vec4 result; uniform vec4 myColor;",
                main: "result = myColor;"
        );

        program = glCreateProgram();
        glAttachShader(program, vertex);
        glAttachShader(program, fragment);

        glLinkProgram(program);

        glDeleteShader(vertex);
        glDeleteShader(fragment);

        glUseProgram(program);
    }

    private static unsafe void CreateVertices(bool doFill)
    {
        fixed (float* v = &vertices[0])
        {
            glBufferData(GL_ARRAY_BUFFER, sizeof(float) * vertices.Length, v, GL_DYNAMIC_DRAW);
        }

        if (doFill)
        {
            fixed (uint* i = &indicesFill[0])
            {
                glBufferData(GL_ELEMENT_ARRAY_BUFFER, sizeof(uint) * indicesFill.Length, i, GL_DYNAMIC_DRAW);
            }
        }
        else
        {
            fixed (uint* i = &indicesOutline[0])
            {
                glBufferData(GL_ELEMENT_ARRAY_BUFFER, sizeof(uint) * indicesOutline.Length, i, GL_DYNAMIC_DRAW);
            }
        }

        glVertexAttribPointer(0, 3, GL_FLOAT, false, 3 * sizeof(float), NULL);
        glEnableVertexAttribArray(0);
    }
};
