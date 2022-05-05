using GLFW;
using static OpenGL.GL;
using System;


// https://learnopengl.com/code_viewer_gh.php?code=src/4.advanced_opengl/10.1.instancing_quads/instancing_quads.cpp


public class myRectangleInst : myPrimitive
{
    private static uint ebo_fill = 0, ebo_outline = 0, shaderProgram = 0, instVbo = 0, quadVbo = 0;
    private static float[] vertices = null;
    private static float _angle;
    private static int locationColor = 0, locationAngle = 0, locationCenter = 0, locationScrSize = 0, N = 4;

    // -------------------------------------------------------------------------------------------------------------------

    public myRectangleInst()
    {
        if (vertices == null)
        {
            vertices = new float[12];

            CreateProgram();
            glUseProgram(shaderProgram);
            locationColor   = glGetUniformLocation(shaderProgram, "myColor");
            locationAngle   = glGetUniformLocation(shaderProgram, "myAngle");
            locationCenter  = glGetUniformLocation(shaderProgram, "myCenter");
            locationScrSize = glGetUniformLocation(shaderProgram, "myScrSize");

            instVbo     = glGenBuffer();
            quadVbo     = glGenBuffer();
            ebo_fill    = glGenBuffer();
            ebo_outline = glGenBuffer();

            updateIndices();
        }
    }

    // -------------------------------------------------------------------------------------------------------------------

    public void Draw(int x, int y, int w, int h, bool doFill = false)
    {
        unsafe void __draw(bool fill)
        {
            // do we need this?..
            //glBindBuffer(GL_ARRAY_BUFFER, instVbo);
            //glBindBuffer(GL_ARRAY_BUFFER, quadVbo);

            //glBindBuffer(GL_ARRAY_BUFFER, quadVbo);

            if (fill)
            {
                glPolygonMode(GL_FRONT_AND_BACK, GL_FILL);
                glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, ebo_fill);
                glDrawElementsInstanced(GL_TRIANGLES, 6, GL_UNSIGNED_INT, NULL, N);
            }
            else
            {
                glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, ebo_outline);
                glDrawElementsInstanced(GL_LINES, 8, GL_UNSIGNED_INT, NULL, N);
            }
        }

        // ---------------------------------------------------------------------------------------

        if (_angle == 0)
        {
            // Recalc screen coordinates into Normalized Device Coordinates (NDC)
            float fx = 2.0f * x / (Width + 1) - 1.0f;       // Shifting Width a bit to get rid of incomplete left bottom angle
            float fy = 1.0f - 2.0f * y / Height;
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
            float fx = x;
            float fy = y;
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

        setColor(locationColor, _r, _g, _b, _a);
        setAngle(locationAngle, _angle);

        // Set the center of rotation
        if (_angle != 0.0f)
        {
            glUniform2f(locationCenter, x + w / 2, y + h / 2);
            updUniformScreenSize(locationScrSize, Width, Height);
        }

        __draw(doFill);
    }

    // -------------------------------------------------------------------------------------------------------------------

    private static void CreateProgram()
    {
        var vertex = myOGL.CreateShaderEx(GL_VERTEX_SHADER,
            @"layout (location = 0) in vec3 pos;
              layout (location = 1) in vec2 aOffset;
                uniform float myAngle; uniform vec2 myCenter; uniform ivec2 myScrSize;",

                main: @"if (myAngle == 0)
                        {
                            gl_Position = vec4(pos.x + aOffset.x, pos.y + aOffset.y, pos.z, 1.0);
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
            "out vec4 result; uniform vec4 myColor;",
                main: "result = myColor;"
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
        glBindBuffer(GL_ARRAY_BUFFER, quadVbo);
        {
            fixed (float* v = &vertices[0])
                glBufferData(GL_ARRAY_BUFFER, sizeof(float) * vertices.Length, v, GL_DYNAMIC_DRAW);

            glVertexAttribPointer(0, 3, GL_FLOAT, false, 3 * sizeof(float), NULL);
            glEnableVertexAttribArray(0);
        }
    }

    // -------------------------------------------------------------------------------------------------------------------

    // Move indices data from CPU to GPU -- needs to be called only once, as we have 2 different EBOs, and they are not going to change;
    // The EBO must be activated prior to drawing the shape: glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, doFill ? ebo1 : ebo2);
    private static unsafe void updateIndices()
    {
        int usage = GL_STATIC_DRAW;

        glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, ebo_fill);
        {
            var indicesFill = new uint[]
            {
                0, 1, 3,   // first triangle
                1, 2, 3    // second triangle
            };

            fixed (uint* i = &indicesFill[0])
                glBufferData(GL_ELEMENT_ARRAY_BUFFER, sizeof(uint) * indicesFill.Length, i, usage);

            glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, 0);
        }

        glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, ebo_outline);
        {
            var indicesOutline = new uint[]
            {
                0, 1,
                1, 2,
                2, 3,
                3, 0
            };

            fixed (uint* i = &indicesOutline[0])
                glBufferData(GL_ELEMENT_ARRAY_BUFFER, sizeof(uint) * indicesOutline.Length, i, usage);

            glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, 0);
        }

        glBindBuffer(GL_ARRAY_BUFFER, instVbo);
        {
            float[] trans = new float[] { 0.1f, 0, 0.3f, 0.1f, 0.5f, 0.2f, 0.5f, -0.2f };

            fixed (float* tt = &trans[0])
                glBufferData(GL_ARRAY_BUFFER, sizeof(float) * trans.Length, tt, GL_STATIC_DRAW);

            glEnableVertexAttribArray(1);
            glVertexAttribPointer(1, 2, GL_FLOAT, false, 2 * sizeof(float), NULL);

            // Tell OpenGL this is an instanced vertex attribute
            glVertexAttribDivisor(1, 1);

            glBindBuffer(GL_ARRAY_BUFFER, 0);
        }
    }

    // -------------------------------------------------------------------------------------------------------------------

    public unsafe void upd(System.Collections.Generic.List<float> list)
    {
        float[] arr = new float[list.Count];

        for (int i = 0; i < list.Count; i++)
        {
            arr[i] = list[i];
        }

        N = list.Count / 2;

        glBindBuffer(GL_ARRAY_BUFFER, instVbo);
        {
            fixed (float* a = &arr[0])
                glBufferData(GL_ARRAY_BUFFER, sizeof(float) * arr.Length, a, GL_STATIC_DRAW);

            glEnableVertexAttribArray(1);
            glVertexAttribPointer(1, 2, GL_FLOAT, false, 2 * sizeof(float), NULL);

            // Tell OpenGL this is an instanced vertex attribute
            glVertexAttribDivisor(1, 1);

            glBindBuffer(GL_ARRAY_BUFFER, 0);
        }
    }

    public void SetAngle(float angle)
    {
        _angle = angle;
    }

    // -------------------------------------------------------------------------------------------------------------------

    private static void setAngle(int location, float angle)
    {
        glUniform1f(location, angle);
    }

    // -------------------------------------------------------------------------------------------------------------------

    public void Draw(float x, float y, float w, float h, bool doFill = false)
    {
        Draw((int)x, (int)y, (int)w, (int)h, doFill);
    }

    // -------------------------------------------------------------------------------------------------------------------
};
