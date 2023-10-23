﻿using GLFW;
using static OpenGL.GL;
using System;


/*
    - Instanced Rectangle Texture Renderer
*/


public class myTexRectangleInst : myInstancedPrimitive
{
    private static float[] vertices = null;

    private static uint ebo = 0, instVbo = 0, quadVbo = 0;

    private static uint[] shaderProg = null;
    private static int[] locationColor = null, locationScrSize = null;

    private int rotationMode;

    private uint tex = 0;

    // -------------------------------------------------------------------------------------------------------------------

    public myTexRectangleInst(System.Drawing.Bitmap bmp, int maxInstCount)
    {
        // Number of elements in [instanceArray] that define one single instance:
        // - 4 floats for Onscreen Coordinates
        // - 4 floats for Texture Coordinates
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

            CreateProgram();

            instVbo = glGenBuffer();
            quadVbo = glGenBuffer();
            ebo     = glGenBuffer();

            updateIndices();
        }

        rotationMode = 0;

        myOGL.loadTexture(tex, bmp);
    }

    // -------------------------------------------------------------------------------------------------------------------

    public override void Draw(bool doFill = false)
    {
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
                break;

            case drawMode.OWN_COLOR_CUSTOM_OPACITY:
                glUseProgram(shaderProg[1]);

                // Update uniforms:
                glUniform1f(locationColor[1], _a);
                glUniform2i(locationScrSize[1], Width, Height);
                break;

            case drawMode.CUSTOM_COLOR_CUSTOM_OPACITY:
                glUseProgram(shaderProg[2]);

                // Update uniforms:
                glUniform4f(locationColor[2], _r, _g, _b, _a);
                glUniform2i(locationScrSize[2], Width, Height);
                break;
        }

        unsafe
        {
            // All upcoming GL_TEXTURE_2D operations now have effect on this texture object
            glBindTexture(GL_TEXTURE_2D, tex);
            glPolygonMode(GL_FRONT_AND_BACK, GL_FILL);
            glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, ebo);
            glDrawElementsInstanced(GL_TRIANGLES, 6, GL_UNSIGNED_INT, NULL, N);
        }
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
              // 2 for 2nd mat row
              layout (location = 3) in float angle;
                uniform ivec2 myScrSize;
                out vec4 rgbaColor;
                out vec2 fragTxCoord;
            ",

                main: @"rgbaColor = mData[1];

                        if (angle == 0)
                        {
                            // Rectangle onscreen coordinates
                            gl_Position = vec4(pos.x * mData[0].z, pos.y * mData[0].w, 1.0, 1.0);
                        }

                        // Adjust for pixel density and move into final position
                        gl_Position.x += +2.0 / myScrSize.x * (mData[0].x + mData[0].z/2) - 1.0;
                        gl_Position.y += -2.0 / myScrSize.y * (mData[0].y + mData[0].w/2) + 1.0;

                        fragTxCoord = vec2(gl_Position.x/2, -gl_Position.y/2);
                "
        );

        // Default fragment shader: Paints each instance with its own color and its own opacity
        shaderProg[0] = glCreateProgram();
        {
            var fragment = myOGL.CreateShaderEx(GL_FRAGMENT_SHADER,
                
                header:
                    $@"out vec4 result;
                        in vec4 rgbaColor;
                        in vec2 fragTxCoord;
                        uniform sampler2D myTexture;
                    ",

                main:
                    $@"result = texture(myTexture, fragTxCoord) * rgbaColor * vec4(1, 0, 1, 1);"
            );

            glAttachShader(shaderProg[0], vertex);
            glAttachShader(shaderProg[0], fragment);

            glLinkProgram(shaderProg[0]);

            glDeleteShader(vertex);
            glDeleteShader(fragment);

            glUseProgram(shaderProg[0]);
            locationScrSize[0] = glGetUniformLocation(shaderProg[0], "myScrSize");
        }

        shaderProg[1] = glCreateProgram();
        {
            // later
        }

        shaderProg[2] = glCreateProgram();
        {
            // later
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

        glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, ebo);
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
    }

    // -------------------------------------------------------------------------------------------------------------------

    public void setInstanceCoords(float x, float y, float w, float h, float tx, float ty, float tw, float th)
    {
        instanceArray[instArrayPosition + 0] = x;
        instanceArray[instArrayPosition + 1] = y;
        instanceArray[instArrayPosition + 2] = w;
        instanceArray[instArrayPosition + 3] = h;
/*
        instanceArray[instArrayPosition + 4] = tx;
        instanceArray[instArrayPosition + 5] = ty;
        instanceArray[instArrayPosition + 6] = tw;
        instanceArray[instArrayPosition + 7] = th;*/

        instArrayPosition += 4;
    }

    // -------------------------------------------------------------------------------------------------------------------

    public void setInstanceAngle(float angle)
    {
        instanceArray[instArrayPosition] = angle;

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
                fixed (float* arrayData = &instanceArray[0])
                    glBufferData(GL_ARRAY_BUFFER, sizeof(float) * instArrayPosition, arrayData, GL_DYNAMIC_COPY);

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
