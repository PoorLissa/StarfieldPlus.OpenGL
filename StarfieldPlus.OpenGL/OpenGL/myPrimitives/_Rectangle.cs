using GLFW;
using static OpenGL.GL;
using System;


public class myRectangle : myPrimitive
{
    private static uint vao = 0, vbo = 0, ebo = 0, program = 0;
    private static uint[] indicesOutline = null;
    private static uint[] indicesFill = null;
    private static float[] vertices = null;
    private static int locationColor = 0, locationAngle = 0, locationCenter = 0, locationScrSize = 0;
    private static float _angle;

    public myRectangle()
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
            vertices = new float[12];

            indicesOutline = new uint[]
            {
                0, 1,
                1, 2,
                2, 3,
                3, 0
            };

            indicesFill = new uint[]
            {
                0, 1, 3,   // first triangle
                1, 2, 3    // second triangle
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

            __glGenBuffers();
            glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, ebo);
        }
    }

    public void Draw(int x, int y, int w, int h, bool doFill = false)
    {
        unsafe void __draw(bool fill)
        {
            if (fill)
            {
                glPolygonMode(GL_FRONT_AND_BACK, GL_FILL);
                glDrawElements(GL_TRIANGLES, 6, GL_UNSIGNED_INT, NULL);
            }
            else
            {
                glDrawElements(GL_LINES, 8, GL_UNSIGNED_INT, NULL);
            }
        }

        // ---------------------------------------------------------------------------------------

        float fx, fy;

        if (_angle == 0)
        {
            // Recalc screen coordinates into Normalized Device Coordinates (NDC)
            fx = 2.0f * x / (Width + 1) - 1.0f;       // Shifting Width a bit to get rid of incomplete left bottom angle
            fy = 1.0f - 2.0f * y / Height;
            vertices[06] = fx;
            vertices[09] = fx;
            vertices[01] = fy;
            vertices[10] = fy;

            fx = 2.0f * (x + w) / Width - 1.0f;
            vertices[0] = fx;
            vertices[3] = fx;

            fy = 1.0f - 2.0f * (y + h) / Height;
            vertices[4] = fy;
            vertices[7] = fy;
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

        CreateVertices(doFill);

        glUseProgram(program);
        setColor(locationColor, _r, _g, _b, _a);
        setAngle(locationAngle, _angle);

        // Set the center of rotation
        if (_angle != 0.0f)
        {
            glUniform2f(locationCenter, x + w/2, y + h/2);
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

                            gl_Position.x = 2.0f * gl_Position.x / (myScrSize.x+1) - 1.0f;
                            gl_Position.y = 1.0f - 2.0f * gl_Position.y / myScrSize.y;
                        }"
        );

        var fragment = myOGL.CreateShaderEx(GL_FRAGMENT_SHADER,
            "in vec2 zzz; out vec4 result; uniform vec4 myColor;",
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
