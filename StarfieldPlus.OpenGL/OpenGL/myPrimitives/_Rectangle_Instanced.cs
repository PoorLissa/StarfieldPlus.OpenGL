using GLFW;
using static OpenGL.GL;
using System;


// todo:
//  - need to create vertices only once -- do I?


public class myRectangleInst : myInstancedPrimitive
{
    private static float[] vertices = null;

    private static uint ebo_fill = 0, ebo_outline = 0, instVbo = 0, quadVbo = 0;

    private static uint[] shaderProg = null;
    private static int[] locationColor = null, locationScrSize = null, locationRotateMode = null;

    private int rotationMode;

    // -------------------------------------------------------------------------------------------------------------------

    public myRectangleInst(int maxInstCount)
    {
        // Number of elements in [instanceArray] that define one single instance:
        // - 4 floats for Coordinates
        // - 4 floats for RGBA
        // - 1 float for Angle
        n = 9;

        if (vertices == null)
        {
            N = 0;

            vertices = new float[12];
            instanceArray = new float[maxInstCount * n];

            shaderProg = new uint[3];
            locationColor = new int[3];
            locationScrSize = new int[3];
            locationRotateMode = new int[3];

            CreateProgram();

            instVbo     = glGenBuffer();
            quadVbo     = glGenBuffer();
            ebo_fill    = glGenBuffer();
            ebo_outline = glGenBuffer();

            updateIndices();
        }

        rotationMode = 0;
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
                glDrawElementsInstanced(GL_TRIANGLES, 6, GL_UNSIGNED_INT, NULL, N);
            }
            else
            {
                glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, ebo_outline);
                glDrawElementsInstanced(GL_LINES, 8, GL_UNSIGNED_INT, NULL, N);
            }
        }

        // ---------------------------------------------------------------------------------------

        // Our initial square is located at the center of coordinates: [x = -pixel/2, y = pixel/2, w = 1*pixel, h = 1*pixel];
        // It will be scaled and moved into position by the shader

        vertices[00] = +pixelX;
        vertices[01] = +pixelY;
        vertices[03] = +pixelX;
        vertices[04] = -pixelY;
        vertices[06] = -pixelX;
        vertices[07] = -pixelY;
        vertices[09] = -pixelX;
        vertices[10] = +pixelY;

        // todo: check later, if it is possible to do this only once
        updateInstances();
        updateVertices();

        switch (_drawMode)
        {
            case drawMode.OWN_COLOR_OWN_OPACITY:
                glUseProgram(shaderProg[0]);

                // Update uniforms:
                glUniform2i(locationScrSize[0], Width, Height);
                glUniform1i(locationRotateMode[0], rotationMode);
                break;

            case drawMode.OWN_COLOR_CUSTOM_OPACITY:
                glUseProgram(shaderProg[1]);

                // Update uniforms:
                glUniform1f(locationColor[1], _a);
                glUniform2i(locationScrSize[1], Width, Height);
                glUniform1i(locationRotateMode[1], rotationMode);
                break;

            case drawMode.CUSTOM_COLOR_CUSTOM_OPACITY:
                glUseProgram(shaderProg[2]);

                // Update uniforms:
                glUniform4f(locationColor[2], _r, _g, _b, _a);
                glUniform2i(locationScrSize[2], Width, Height);
                glUniform1i(locationRotateMode[2], rotationMode);
                break;
        }

        __draw(doFill);
    }

    // -------------------------------------------------------------------------------------------------------------------

    private static void CreateProgram()
    {
        // mat2x4 mData is a [2 x 4] matrix of floats, where:
        // - first  4 floats are [x, y, w, h];
        // - second 4 floats are [r, g, b, a]
        var vertex = myOGL.CreateShaderEx(GL_VERTEX_SHADER,
            @"layout (location = 0) in vec3 pos;
              layout (location = 1) in mat2x4 mData;
              layout (location = 3) in float angle;
                uniform ivec2 myScrSize;
                uniform int myRttMode;
                out vec4 rgbaColor;",

                main: @"rgbaColor = mData[1];

                        if (angle == 0)
                        {
                            gl_Position = vec4(pos.x * mData[0].z, pos.y * mData[0].w, 1.0, 1.0);
                        }
                        else
                        {
                            float w_to_h = 1.0 * myScrSize.x / myScrSize.y;

                            float x = pos.x;
                            float y = pos.y / w_to_h;

                            float sin_a = sin(angle);
                            float cos_a = cos(angle);

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
                            y1 *= mData[0].w * w_to_h;

                            gl_Position = vec4(x1, y1, 1.0, 1.0);
                        }

                        // Adjust for pixel density and move into final position
                        gl_Position.x += +2.0 / myScrSize.x * (mData[0].x + mData[0].z/2) - 1.0;
                        gl_Position.y += -2.0 / myScrSize.y * (mData[0].y + mData[0].w/2) + 1.0;"
        );

        shaderProg[0] = glCreateProgram();
        {
            // Default fragment shader:
            // Paints each instance with its own color and its own opacity
            var fragment = myOGL.CreateShaderEx(GL_FRAGMENT_SHADER,
                "in vec4 rgbaColor; out vec4 result;",

                    main: @"result = rgbaColor;"
            );

            glAttachShader(shaderProg[0], vertex);
            glAttachShader(shaderProg[0], fragment);

            glLinkProgram(shaderProg[0]);

            glDeleteShader(vertex);
            glDeleteShader(fragment);

            glUseProgram(shaderProg[0]);
            locationScrSize[0] = glGetUniformLocation(shaderProg[0], "myScrSize");
            locationRotateMode[0] = glGetUniformLocation(shaderProg[0], "myRttMode");
        }

        shaderProg[1] = glCreateProgram();
        {
            // This shader paints each instance with its own color, but with a custom opacity:
            // - In case the opacity is negative, we multiply our instance's opacity by this value (with neg.sign);
            // - In case the opacity is positive, we use this value;
            // - In case the opacity is 0, we use instance's own value;
            // This is done to be able to draw all the instances the second time with changed opacity
            // For example: we want to draw set of filled-in rectangles with lower opacity, and then we want to give them borders with higher opacity
            var fragment = myOGL.CreateShaderEx(GL_FRAGMENT_SHADER,
                "in vec4 rgbaColor; out vec4 result; uniform float myColor;",

                    main: @"result = rgbaColor;
                        if (myColor < 0)
                            result.w *= -myColor;

                        if (myColor > 0)
                            result.w = myColor;"
            );

            glAttachShader(shaderProg[1], vertex);
            glAttachShader(shaderProg[1], fragment);

            glLinkProgram(shaderProg[1]);

            glDeleteShader(vertex);
            glDeleteShader(fragment);

            glUseProgram(shaderProg[1]);
            locationColor[1] = glGetUniformLocation(shaderProg[1], "myColor");
            locationScrSize[1] = glGetUniformLocation(shaderProg[1], "myScrSize");
            locationRotateMode[1] = glGetUniformLocation(shaderProg[1], "myRttMode");
        }

        shaderProg[2] = glCreateProgram();
        {
            // This shader paints each instance with the same custom color and opacity:
            // This is done to be able to draw all the instances the second time with a custom border
            var fragment = myOGL.CreateShaderEx(GL_FRAGMENT_SHADER,
                "in vec4 rgbaColor; out vec4 result; uniform vec4 myColor;",

                    main: @"result = myColor;"
            );

            glAttachShader(shaderProg[2], vertex);
            glAttachShader(shaderProg[2], fragment);

            glLinkProgram(shaderProg[2]);

            glDeleteShader(vertex);
            glDeleteShader(fragment);

            glUseProgram(shaderProg[2]);
            locationColor[2] = glGetUniformLocation(shaderProg[1], "myColor");
            locationScrSize[2] = glGetUniformLocation(shaderProg[1], "myScrSize");
            locationRotateMode[2] = glGetUniformLocation(shaderProg[1], "myRttMode");
        }

        return;
    }

    // -------------------------------------------------------------------------------------------------------------------

    // Move vertices data from CPU to GPU -- needs to be called each time we change the Rectangle's coordinates
    // -- not anymore. need to call this once now, i think
    private unsafe void updateVertices()
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
    }

    // -------------------------------------------------------------------------------------------------------------------

    public void setInstanceCoords(float x, float y, float w, float h)
    {
        instanceArray[instArrayPosition + 0] = x;
        instanceArray[instArrayPosition + 1] = y;
        instanceArray[instArrayPosition + 2] = w;
        instanceArray[instArrayPosition + 3] = h;

        instArrayPosition += 4;
    }

    // -------------------------------------------------------------------------------------------------------------------

    public void setInstanceAngle(float a)
    {
        instanceArray[instArrayPosition] = a;

        instArrayPosition++;
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

                glEnableVertexAttribArray(3);
                glVertexAttribPointer(3, 1, GL_FLOAT, false, n * sizeof(float), new IntPtr(1 * 8 * sizeof(float)));

                // Tell OpenGL this is an instanced vertex attribute
                glVertexAttribDivisor(1, 1);
                glVertexAttribDivisor(2, 1);
                glVertexAttribDivisor(3, 1);

                glBindBuffer(GL_ARRAY_BUFFER, 0);
            }
        }

        return;
    }

    // -------------------------------------------------------------------------------------------------------------------
};
