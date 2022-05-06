using GLFW;
using static OpenGL.GL;
using System;


// https://learnopengl.com/code_viewer_gh.php?code=src/4.advanced_opengl/10.1.instancing_quads/instancing_quads.cpp

// todo:
//  - hexagons don't draw when instancing is enabled. fix hexagons and other shapes that are broken by instancing
//  - need rotation

public class myRectangleInst : myPrimitive
{
    private static uint ebo_fill = 0, ebo_outline = 0, shaderProgram = 0, instVbo = 0, quadVbo = 0;
    private static float[] vertices = null;
    private static float _angle;
    private static int locationColor = 0, locationAngle = 0, locationCenter = 0, locationScrSize = 0, N = 0, Count = 0;

    private static float[] instanceArray = null;

    // -------------------------------------------------------------------------------------------------------------------

    public myRectangleInst(int maxInstCount)
    {
        if (vertices == null)
        {
            N = 0;

            vertices = new float[12];
            instanceArray = new float[maxInstCount * 8];

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

    public void Draw(bool doFill = false)
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
            // Our initial rectangle is [x = -1, y = 1, w = 1, h = 1]
            // It will be scaled and moved into position in the shader
            float fx = -1.0f;
            float fy = +1.0f;

            vertices[06] = fx;
            vertices[09] = fx;
            vertices[01] = fy;
            vertices[10] = fy;

            fx = 2.0f / Width - 1.0f;
            vertices[0] = fx;
            vertices[3] = fx;

            fy = 1.0f - 2.0f / Height;
            vertices[4] = fy;
            vertices[7] = fy;
        }
        else
        {
        }


        updateVertices();

        glUseProgram(shaderProgram);

        setColor(locationColor, _r, _g, _b, _a);
        //setAngle(locationAngle, _angle);

        // Set the center of rotation
        if (_angle != 0.0f)
        {
            //glUniform2f(locationCenter, x + w / 2, y + h / 2);
            updUniformScreenSize(locationScrSize, Width, Height);
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
                uniform float myAngle; uniform vec2 myCenter; uniform ivec2 myScrSize;
                out vec4 rgbaColor;",

                main: @"if (myAngle == 0)
                        {
                            gl_Position = vec4(pos.x * mData[0].z + mData[0].x, pos.y * mData[0].w + mData[0].y, pos.z, 1.0);

                            gl_Position.x -= (1 - mData[0].z);
                            gl_Position.y += (1 - mData[0].w);

                            rgbaColor = mData[1];
                        }"
        );

        // In case opacity in myColor vec is negative, we know that we should just multiply our instance's opacity by this value (with neg.sign)
        // This is done to be able to draw all the instances the second time with changed opacity
        // For example: we want to draw set of filled-in rectangles with lower opacity, and then we want to give them borders with higher opacity
        var fragment = myOGL.CreateShaderEx(GL_FRAGMENT_SHADER,
            "in vec4 rgbaColor; out vec4 result; uniform vec4 myColor;",

                main: @"result = rgbaColor;

                        if (myColor.w < 0)
                        {
                            result.w *= -myColor.w;
                        }"
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
    // -- not anymore. need to call this once now, i think
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
    }

    // -------------------------------------------------------------------------------------------------------------------

    public void Clear()
    {
        Count = 0;
    }

    // -------------------------------------------------------------------------------------------------------------------

    public void setCoords(float x, float y, float w, float h)
    {
        instanceArray[Count + 0] = +2.0f * x / Width;
        instanceArray[Count + 1] = -2.0f * y / Height;
        instanceArray[Count + 2] = w;
        instanceArray[Count + 3] = h;

        Count += 4;
    }

    // -------------------------------------------------------------------------------------------------------------------

    public void setColor(float r, float g, float b, float a)
    {
        instanceArray[Count + 0] = r;
        instanceArray[Count + 1] = g;
        instanceArray[Count + 2] = b;
        instanceArray[Count + 3] = a;

        Count += 4;
    }

    // -------------------------------------------------------------------------------------------------------------------

    public void setColor(double r, double g, double b, double a)
    {
        int i = Count;

        instanceArray[i + 0] = (float)r;
        instanceArray[i + 1] = (float)g;
        instanceArray[i + 2] = (float)b;
        instanceArray[i + 3] = (float)a;

        Count += 4;
    }

    // -------------------------------------------------------------------------------------------------------------------

    // Create GPU buffer out of out instances from the array
    public unsafe void updateInstances()
    {
        if (Count > 1)
        {
            int n = 8;

            N = Count / n;

            // Copy data to GPU:
            glBindBuffer(GL_ARRAY_BUFFER, instVbo);
            {
                fixed (float* a = &instanceArray[0])
                    glBufferData(GL_ARRAY_BUFFER, sizeof(float) * Count, a, GL_STATIC_DRAW);    // todo: test static vs dynamic FPS here

                glEnableVertexAttribArray(1);
                glVertexAttribPointer(1, 4, GL_FLOAT, false, 8 * sizeof(float), NULL);

                glEnableVertexAttribArray(2);
                glVertexAttribPointer(2, 4, GL_FLOAT, false, 8 * sizeof(float), new IntPtr(1 * 4 * sizeof(float)));

                // Tell OpenGL this is an instanced vertex attribute
                glVertexAttribDivisor(1, 1);
                glVertexAttribDivisor(2, 1);

                glBindBuffer(GL_ARRAY_BUFFER, 0);
            }
        }

        return;
    }

    // -------------------------------------------------------------------------------------------------------------------

    // Reallocate inner instances array, if its size is less than the new size
    public void Resize(int Size)
    {
        if (instanceArray.Length < Size * 8)
        {
            instanceArray = new float[Size * 8];
        }
    }

    // -------------------------------------------------------------------------------------------------------------------
};
