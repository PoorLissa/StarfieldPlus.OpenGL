﻿using GLFW;
using static OpenGL.GL;
using System;



public class myHexagonInst : myInstancedPrimitive
{
    private float[] vertices = null;

    private uint ebo_fill = 0, ebo_outline = 0, shaderProgram = 0, instVbo = 0, quadVbo = 0;
    private int locationColor = 0, locationScrSize = 0, locationRotateMode = 0;

    private static float sqrt3_div2 = 0;
    private static float h_div_w = 0;

    private int rotationMode;

    // -------------------------------------------------------------------------------------------------------------------

    public myHexagonInst(int maxInstCount)
    {
        if (sqrt3_div2 == 0)
            sqrt3_div2 = (float)(Math.Sqrt(3.0) / 2.0);

        if (h_div_w == 0)
            h_div_w = (float)Height / (float)Width;

        // Number of elements in [instanceArray] that define one single instance:
        // - 3 floats for Coordinates (x, y, radius of an escribed circle) + 1 float for angle
        // - 4 floats for RGBA
        n = 8;
        N = 0;

        vertices = new float[18];
        instanceArray = new float[maxInstCount * n];

        for (int i = 0; i < 18; i++)
            vertices[i] = 0.0f;

        float fr = pixelY;                      // Radius
        float frx = fr * h_div_w;               // Radius adjusted for x-coordinate

        float frx_sqrt = fr * sqrt3_div2;
        float frx_half = frx * 0.5f;

        vertices[00] = -frx;
        vertices[01] = 0;
        vertices[03] = -frx_half;
        vertices[04] = +frx_sqrt;
        vertices[06] = +frx_half;
        vertices[07] = +frx_sqrt;
        vertices[09] = +frx;
        vertices[10] = 0;
        vertices[12] = +frx_half;
        vertices[13] = -frx_sqrt;
        vertices[15] = -frx_half;
        vertices[16] = -frx_sqrt;


        CreateProgram();
        glUseProgram(shaderProgram);
        locationColor      = glGetUniformLocation(shaderProgram, "myColor");
        locationScrSize    = glGetUniformLocation(shaderProgram, "myScrSize");
        locationRotateMode = glGetUniformLocation(shaderProgram, "myRttMode");

        instVbo     = glGenBuffer();
        quadVbo     = glGenBuffer();
        ebo_fill    = glGenBuffer();
        ebo_outline = glGenBuffer();

        updateIndices();

    }

    // -------------------------------------------------------------------------------------------------------------------

    public override void Draw(bool doFill = false)
    {
        // todo: make parent method unsafe and remove this call: see if this is faster
        unsafe void __draw(bool fill)
        {
            if (fill)
            {
                glPolygonMode(GL_FRONT_AND_BACK, GL_FILL);
                glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, ebo_fill);
                glDrawElementsInstanced(GL_TRIANGLES, 12, GL_UNSIGNED_INT, NULL, N);
            }
            else
            {
                glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, ebo_outline);
                glDrawElementsInstanced(GL_LINES, 18, GL_UNSIGNED_INT, NULL, N);
            }
        }

        // ---------------------------------------------------------------------------------------

        updateInstances();
        updateVertices();

        glUseProgram(shaderProgram);

        setColor(locationColor, _r, _g, _b, _a);
        updUniformScreenSize(locationScrSize, Width, Height);
        glUniform1i(locationRotateMode, rotationMode);

        __draw(doFill);
    }

    // -------------------------------------------------------------------------------------------------------------------

    // todo: optimize this
    private void CreateProgram()
    {
        // mat2x4 mData is a [2 x 4] matrix of floats, where:
        // - first  4 floats are [x, y, w, angle];
        // - second 4 floats are [r, g, b, a]
        var vertex = myOGL.CreateShaderEx(GL_VERTEX_SHADER,
            @"layout (location = 0) in vec3 pos;
              layout (location = 1) in mat2x4 mData;
                uniform ivec2 myScrSize;
                uniform int myRttMode;
                out vec4 rgbaColor;",

                main: @"rgbaColor = mData[1];

                        if (mData[0].w == 0)
                        {
                            gl_Position = vec4(pos.x * mData[0].z, pos.y * mData[0].z, pos.z, 1.0);
                        }
                        else
                        {
                            float w_to_h = 1.0 * myScrSize.x / myScrSize.y;

                            float sin_a = sin(mData[0].w);
                            float cos_a = cos(mData[0].w);

                            float x = pos.x;
                            float y = pos.y / w_to_h;

                            // Rotate
                            float x1 = x * cos_a - y * sin_a;
                            float y1 = y * cos_a + x * sin_a;

                            // Additional pseudo-3d rotation:
                            switch (myRttMode)
                            {
                                case 1: x1 *= sin_a; break;
                                case 2: y1 *= cos_a; break;
                                case 3: x1 *= sin_a; y1 *= cos_a; break;
                            }

                            // Set width and height
                            x1 *= mData[0].z;
                            y1 *= mData[0].z * w_to_h;

                            gl_Position = vec4(x1, y1, 1.0, 1.0);
                        }

                        // Adjust for pixel density and move into final position
                        gl_Position.x += +2.0 / myScrSize.x * mData[0].x - 1.0;
                        gl_Position.y += -2.0 / myScrSize.y * mData[0].y + 1.0;"
        );

        // todo: test if "result.w *= myColor.w < 0 ? -myColor.w : 1" is faster
        var fragment = myOGL.CreateShaderEx(GL_FRAGMENT_SHADER,
            "in vec4 rgbaColor; out vec4 result; uniform vec4 myColor;",

                main: @"result = rgbaColor;

                        if (myColor.w < 0)
                            result.w *= -myColor.w;
                "
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

    public void setInstanceCoords(float x, float y, float rad, float angle)
    {
        instanceArray[instArrayPosition + 0] = x;
        instanceArray[instArrayPosition + 1] = y;
        instanceArray[instArrayPosition + 2] = rad;
        instanceArray[instArrayPosition + 3] = angle;

        instArrayPosition += 4;
    }

    // -------------------------------------------------------------------------------------------------------------------

    public void setInstanceColor(float r, float g, float b, float a)
    {
        instanceArray[instArrayPosition + 0] = r;
        instanceArray[instArrayPosition + 1] = g;
        instanceArray[instArrayPosition + 2] = b;
        instanceArray[instArrayPosition + 3] = a;

        instArrayPosition += 4;
    }

    // -------------------------------------------------------------------------------------------------------------------

    public void setInstanceColor(double r, double g, double b, double a)
    {
        instanceArray[instArrayPosition + 0] = (float)r;
        instanceArray[instArrayPosition + 1] = (float)g;
        instanceArray[instArrayPosition + 2] = (float)b;
        instanceArray[instArrayPosition + 3] = (float)a;

        instArrayPosition += 4;
    }

    // -------------------------------------------------------------------------------------------------------------------

    public void setRotationMode(int mode)
    {
        rotationMode = mode;
    }

    // -------------------------------------------------------------------------------------------------------------------

    // Move vertices data from CPU to GPU -- needs to be called each time we change the Hexagon's coordinates
    private unsafe void updateVertices()
    {
        // Bind a buffer;
        // From now on, all the operations on this type of buffer will be performed on the buffer we just bound;
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
    private unsafe void updateIndices()
    {
        int usage = GL_STATIC_DRAW;

        glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, ebo_fill);
        {
            // 4 triangles
            var indicesFill = new uint[]
            {
                0, 1, 3,
                1, 2, 3,
                0, 4, 3,
                0, 5, 4
            };

            fixed (uint* i = &indicesFill[0])
                glBufferData(GL_ELEMENT_ARRAY_BUFFER, sizeof(uint) * indicesFill.Length, i, usage);

            // Unbind current buffer
            glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, 0);
        }

        glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, ebo_outline);
        {
            // 6 lines
            var indicesOutline = new uint[]
            {
                0, 1,
                1, 2,
                2, 3,
                3, 4,
                4, 5,
                5, 0
            };

            fixed (uint* i = &indicesOutline[0])
                glBufferData(GL_ELEMENT_ARRAY_BUFFER, sizeof(uint) * indicesOutline.Length, i, usage);

            // Unbind current buffer
            glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, 0);
        }
    }

    // -------------------------------------------------------------------------------------------------------------------

    // Create GPU buffer out of out instances from the array
    protected override unsafe void updateInstances()
    {
        if (instArrayPosition > 1)
        {
            N = instArrayPosition / n;

            // Copy data to GPU:
            glBindBuffer(GL_ARRAY_BUFFER, instVbo);
            {
                fixed (float* a = &instanceArray[0])
                    glBufferData(GL_ARRAY_BUFFER, sizeof(float) * instArrayPosition, a, GL_DYNAMIC_COPY);

                glEnableVertexAttribArray(1);
                glVertexAttribPointer(1, 4, GL_FLOAT, false, n * sizeof(float), NULL);

                glEnableVertexAttribArray(2);
                glVertexAttribPointer(2, 4, GL_FLOAT, false, n * sizeof(float), new IntPtr(1 * 4 * sizeof(float)));

                // Tell OpenGL this is an instanced vertex attribute
                glVertexAttribDivisor(1, 1);
                glVertexAttribDivisor(2, 1);

                glBindBuffer(GL_ARRAY_BUFFER, 0);
            }
        }

        return;
    }

    // -------------------------------------------------------------------------------------------------------------------
};
