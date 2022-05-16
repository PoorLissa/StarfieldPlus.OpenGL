using GLFW;
using static OpenGL.GL;
using System;


// https://learnopengl.com/code_viewer_gh.php?code=src/4.advanced_opengl/10.1.instancing_quads/instancing_quads.cpp

// todo:
//  - hexagons don't draw when instancing is enabled. fix hexagons and other shapes that are broken by instancing

public class myRectangleInst : myPrimitive
{
    private static uint ebo_fill = 0, ebo_outline = 0, shaderProgram = 0, instVbo = 0, quadVbo = 0;
    private static float[] vertices = null;
    private static float _angle;
    private static int locationColor = 0, locationAngle = 0, locationCenter = 0, locationScrSize = 0, N = 0, Count = 0;

    private static float[] instanceArray = null;

    // Number of elements in [instanceArray] that define one single instance
    private static readonly int n = 9;

    // -------------------------------------------------------------------------------------------------------------------

    public myRectangleInst(int maxInstCount)
    {
        if (vertices == null)
        {
            N = 0;

            vertices = new float[12];
            instanceArray = new float[maxInstCount * n];

            CreateProgram();
            glUseProgram(shaderProgram);
            locationColor   = glGetUniformLocation(shaderProgram, "myColor");
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

        // Our initial square is located at the center of coordinates: [x = -pixel/2, y = pixel/2, w = 1*pixel, h = 1*pixel];
        // It will be scaled and moved into position by the shader

        float pixelX = 1.0f / Width;
        float pixelY = 1.0f / Height;

        float fx = -pixelX;
        float fy = +pixelY;

        vertices[06] = fx;
        vertices[09] = fx;
        vertices[01] = fy;
        vertices[10] = fy;

        fx = +pixelX;
        vertices[0] = fx;
        vertices[3] = fx;

        fy = -pixelY;
        vertices[4] = fy;
        vertices[7] = fy;

        updateVertices();

        glUseProgram(shaderProgram);

        setColor(locationColor, _r, _g, _b, _a);
        glUniform1f(locationAngle, _angle);
        updUniformScreenSize(locationScrSize, Width, Height);

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
                uniform vec2 myCenter; uniform ivec2 myScrSize;
                out vec4 rgbaColor;",
#if false
                // working fine, no rotation
                main: @"if (myAngle == 0)
                        {
                            gl_Position = vec4(pos.x * mData[0].z + mData[0].x, pos.y * mData[0].w + mData[0].y, pos.z, 1.0);

                            gl_Position.x -= (1 - mData[0].z);
                            gl_Position.y += (1 - mData[0].w);

                            // this one is the same as the one above
                            //gl_Position = vec4(mData[0].z * (pos.x + 1) + mData[0].x - 1, mData[0].w * (pos.y - 1) + mData[0].y + 1, pos.z, 1.0);

                            rgbaColor = mData[1];
                        }"

#else

                // todo: try optimizing the shader
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

                            // Rotate
                            float x1 = x * cos(angle) - y * sin(angle);
                            float y1 = y * cos(angle) + x * sin(angle);

                            // Set width and height
                            float x2 = x1 * mData[0].z;
                            float y2 = y1 * mData[0].w * w_to_h;

                            gl_Position = vec4(x2, y2, 1.0, 1.0);
                        }

                        // Adjust for pixel density and move into final position
                        gl_Position.x += +2.0 / myScrSize.x * (mData[0].x + mData[0].z/2) - 1.0;
                        gl_Position.y += -2.0 / myScrSize.y * (mData[0].y + mData[0].w/2) + 1.0;"
#endif
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

    public void setInstanceCoords(float x, float y, float w, float h)
    {
        instanceArray[Count + 0] = x;
        instanceArray[Count + 1] = y;
        instanceArray[Count + 2] = w;
        instanceArray[Count + 3] = h;

        Count += 4;
    }

    // -------------------------------------------------------------------------------------------------------------------

    public void setInstanceAngle(float a)
    {
        instanceArray[Count] = a;

        Count++;
    }

    // -------------------------------------------------------------------------------------------------------------------

    public void setInstanceColor(float r, float g, float b, float a)
    {
        instanceArray[Count + 0] = r;
        instanceArray[Count + 1] = g;
        instanceArray[Count + 2] = b;
        instanceArray[Count + 3] = a;

        Count += 4;
    }

    // -------------------------------------------------------------------------------------------------------------------

    public void setInstanceColor(double r, double g, double b, double a)
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
            N = Count / n;

            // Copy data to GPU:
            glBindBuffer(GL_ARRAY_BUFFER, instVbo);
            {
                fixed (float* a = &instanceArray[0])
                    glBufferData(GL_ARRAY_BUFFER, sizeof(float) * Count, a, GL_STATIC_DRAW);    // todo: test static vs dynamic FPS here

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

    // Reallocate inner instances array, if its size is less than the new size
    public void Resize(int Size)
    {
        if (instanceArray.Length < Size * n)
        {
            instanceArray = new float[Size * n];
        }
    }

    // -------------------------------------------------------------------------------------------------------------------
};
