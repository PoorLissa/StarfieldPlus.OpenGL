using GLFW;
using static OpenGL.GL;
using System;


public class myLineInst : myInstancedPrimitive
{
    private int autoDrawCnt;

    private static float[] vertices = null;

    private static uint vbo = 0, shaderProgram = 0;
    private static int floatTimesN = 0;

    private float _lineWidth;

    // -------------------------------------------------------------------------------------------------------------------

    public myLineInst(int maxInstCount)
    {
        // Number of elements in [instanceArray] that define one single instance:
        // - 4 floats for Coordinates (x1, y1, x2, y2)
        // - 4 floats for RGBA
        n = 8;

        _lineWidth = 1.0f;

        if (vertices == null)
        {
            N = 0;

            vertices = new float[4];
            instanceArray = new float[maxInstCount * n];

            autoDrawCnt = maxInstCount * n - 8;
            floatTimesN = sizeof(float) * n;

            vertices[0] = -0.5f;
            vertices[1] = +0.0f;
            vertices[2] = +0.5f;
            vertices[3] = +0.0f;

            CreateProgram();
            glUseProgram(shaderProgram);

            vbo = glGenBuffer();
        }
    }

    // -------------------------------------------------------------------------------------------------------------------

    public void Draw()
    {
        glLineWidth(_lineWidth);

        updateInstances();

        glUseProgram(shaderProgram);

        glPolygonMode(GL_FRONT_AND_BACK, GL_LINE);
        glDrawArraysInstanced(GL_LINES, 0, 2, N);
    }

    // -------------------------------------------------------------------------------------------------------------------

    private static void CreateProgram()
    {
        // Use gl_VertexID to be able to address the first or the second pair of coordinates
        var vertex = myOGL.CreateShaderEx(GL_VERTEX_SHADER,
           $@"layout (location = 0) in vec3 pos;
              layout (location = 1) in mat2x4 mData;
                vec2 realSize = vec2({+2.0 / Width}, {-2.0 / Height});
                out vec4 rgbaColor;",

                    main: $@"rgbaColor = mData[1];
                            int idx = gl_VertexID * 2;
                            gl_Position = vec4(realSize.x * mData[0][idx] - 1.0, realSize.y * mData[0][idx+1] + 1.0, 1.0, 1.0);"
        );

        var fragment = myOGL.CreateShaderEx(GL_FRAGMENT_SHADER,
                "in vec4 rgbaColor; out vec4 result;",
                    main: @"result = rgbaColor;"
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

    // Set the instance coordinates
    public void setInstanceCoords(float x1, float y1, float x2, float y2)
    {
        instanceArray[instArrayPosition++] = x1;
        instanceArray[instArrayPosition++] = y1;
        instanceArray[instArrayPosition++] = x2;
        instanceArray[instArrayPosition++] = y2;
    }

    // -------------------------------------------------------------------------------------------------------------------

    // Set the instance color
    public void setInstanceColor(float r, float g, float b, float a)
    {
        instanceArray[instArrayPosition++] = r;
        instanceArray[instArrayPosition++] = g;
        instanceArray[instArrayPosition++] = b;
        instanceArray[instArrayPosition++] = a;
    }

    // -------------------------------------------------------------------------------------------------------------------

    // Set all the instance data in a single call
    public void setInstance(float x1, float y1, float x2, float y2, float r, float g, float b, float a)
    {
        instanceArray[instArrayPosition++] = x1;
        instanceArray[instArrayPosition++] = y1;
        instanceArray[instArrayPosition++] = x2;
        instanceArray[instArrayPosition++] = y2;

        instanceArray[instArrayPosition++] = r;
        instanceArray[instArrayPosition++] = g;
        instanceArray[instArrayPosition++] = b;
        instanceArray[instArrayPosition++] = a;
    }

    // -------------------------------------------------------------------------------------------------------------------

    // Single-call Draw;
    // Draws to screen automatically, when the buffer is about to be overflown
    public void autoDraw(float x1, float y1, float x2, float y2, float r, float g, float b, float a)
    {
        if (instArrayPosition > autoDrawCnt)
        {
            Draw();
            ResetBuffer();
        }

        instanceArray[instArrayPosition++] = x1;
        instanceArray[instArrayPosition++] = y1;
        instanceArray[instArrayPosition++] = x2;
        instanceArray[instArrayPosition++] = y2;

        instanceArray[instArrayPosition++] = r;
        instanceArray[instArrayPosition++] = g;
        instanceArray[instArrayPosition++] = b;
        instanceArray[instArrayPosition++] = a;

        return;
    }

    // -------------------------------------------------------------------------------------------------------------------

    //static bool isCreated = false;

    // Create GPU buffer out of out instances from the array
    protected override unsafe void updateInstances()
    {
#if !true
        if (instArrayPosition > 1)
        {
            N = instArrayPosition / n;

            // Copy data to GPU:
            glBindBuffer(GL_ARRAY_BUFFER, vbo);
            {
                if (isCreated == false)
                {
                    // Create the buffer only once
                    glBufferData(GL_ARRAY_BUFFER, sizeof(float) * instanceArray.Length, NULL, GL_DYNAMIC_COPY);
                    isCreated = true;
                }
                else
                {
                    // ??? do we need it? is this orphaning?
                    // https://stackoverflow.com/questions/24512468/when-to-clear-a-vertex-buffer-object#:~:text=To%20directly%20answer%20your%20question,update%20whatever%20contents%20you%20want.
                    glBufferData(GL_ARRAY_BUFFER, sizeof(float) * instanceArray.Length, NULL, GL_DYNAMIC_COPY);

                    // When created, update the buffer
                    fixed (float* a = &instanceArray[0])
                        glBufferSubData(GL_ARRAY_BUFFER, 0, sizeof(float) * instArrayPosition, a);
                }

                glEnableVertexAttribArray(1);
                glVertexAttribPointer(1, 4, GL_FLOAT, false, floatTimesN, NULL);

                // ??? why use new intptr?
                glEnableVertexAttribArray(2);
                glVertexAttribPointer(2, 4, GL_FLOAT, false, floatTimesN, new IntPtr(1 * 4 * sizeof(float)));

                // Tell OpenGL this is an instanced vertex attribute
                glVertexAttribDivisor(1, 1);
                glVertexAttribDivisor(2, 1);

                glBindBuffer(GL_ARRAY_BUFFER, 0);
            }
        }
#endif

#if true
        if (instArrayPosition > 1)
        {
            N = instArrayPosition / n;

            // Copy data to GPU:
            glBindBuffer(GL_ARRAY_BUFFER, vbo);
            {
                // todo: can we use it?
                // https://www.khronos.org/opengl/wiki/Buffer_Object_Streaming
                //glBufferData(GL_ARRAY_BUFFER, sizeof(float) * instArrayPosition, NULL, GL_DYNAMIC_COPY);
                //glMapBufferRange(GL_ARRAY_BUFFER, 0, sizeof(float) * instArrayPosition, GL_MAP_INVALIDATE_BUFFER_BIT);

                // also: https://www.khronos.org/opengl/wiki/Buffer_Object#Data_Specification

                // https://onrendering.blogspot.com/2011/10/buffer-object-streaming-in-opengl.html

                // https://gamedev.stackexchange.com/questions/87074/for-vertex-buffer-steaming-multiple-glbuffersubdata-vs-orphaning

                // https://zachbethel.wordpress.com/2013/03/20/buffer-streamin-opengl/

                fixed (float* a = &instanceArray[0])
                    glBufferData(GL_ARRAY_BUFFER, sizeof(float) * instArrayPosition, a, GL_DYNAMIC_COPY);

                glEnableVertexAttribArray(1);
                glVertexAttribPointer(1, 4, GL_FLOAT, false, floatTimesN, NULL);

                glEnableVertexAttribArray(2);
                glVertexAttribPointer(2, 4, GL_FLOAT, false, floatTimesN, new IntPtr(1 * 4 * sizeof(float)));

                // Tell OpenGL this is an instanced vertex attribute
                glVertexAttribDivisor(1, 1);
                glVertexAttribDivisor(2, 1);

                glBindBuffer(GL_ARRAY_BUFFER, 0);
            }
        }
#endif


#if !true

/*
        glBindBuffer(GL_ARRAY_BUFFER, verticesBufferId);
        void* data = glMapBuffer(GL_ARRAY_BUFFER, ... );
        // copy vertex data from instance 
        ::memcpy(data, vertices, vertexSize);
        glUnmapBuffer(... );
*/

        if (instArrayPosition > 1)
        {
            N = instArrayPosition / n;

            // Copy data to GPU:
            glBindBuffer(GL_ARRAY_BUFFER, vbo);
            {
                System.IntPtr data = glMapBuffer(GL_ARRAY_BUFFER, GL_READ_WRITE);

                for (int i = 0; i < instArrayPosition; i++)
                {
                    //data[i] = instArrayPosition[i];
                }

                glEnableVertexAttribArray(1);
                glVertexAttribPointer(1, 4, GL_FLOAT, false, floatTimesN, NULL);

                glEnableVertexAttribArray(2);
                glVertexAttribPointer(2, 4, GL_FLOAT, false, floatTimesN, new IntPtr(1 * 4 * sizeof(float)));

                // Tell OpenGL this is an instanced vertex attribute
                glVertexAttribDivisor(1, 1);
                glVertexAttribDivisor(2, 1);

                glBindBuffer(GL_ARRAY_BUFFER, 0);
            }
        }
#endif
        return;
    }

    // -------------------------------------------------------------------------------------------------------------------

    public void setLineWidth(float width)
    {
        _lineWidth = width;
    }

    // -------------------------------------------------------------------------------------------------------------------

    // Make lines antialized;
    // Be careful, this decreases performance greatly!
    // 
    // Also, there might be a less expensive way of doing this (see the link)
    // https://vitaliburkov.wordpress.com/2016/09/17/simple-and-fast-high-quality-antialiased-lines-with-opengl/
    public void setAntialized(bool antialized)
    {
        if (antialized)
        {
            glEnable(GL_LINE_SMOOTH);
            glEnable(GL_POLYGON_SMOOTH);
            glHint(GL_LINE_SMOOTH_HINT, GL_NICEST);
            glHint(GL_POLYGON_SMOOTH_HINT, GL_NICEST);
        }
        else
        {
            glDisable(GL_LINE_SMOOTH);
            glDisable(GL_POLYGON_SMOOTH);
            glHint(GL_LINE_SMOOTH_HINT, GL_FASTEST);
            glHint(GL_POLYGON_SMOOTH_HINT, GL_FASTEST);
        }

        //glDepthMask(!antialized);
    }

    // -------------------------------------------------------------------------------------------------------------------
};
