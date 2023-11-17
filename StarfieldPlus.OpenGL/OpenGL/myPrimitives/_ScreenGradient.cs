using System;
using my;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TaskbarClock;
using static OpenGL.GL;

/*
    This class only serves one purpose: drawing a full screen color gradient
*/

public class myScreenGradient : myPrimitive
{
    private long tBegin;

    private static uint vbo = 0, ebo = 0, shaderProgram = 0;
    private static float[] vertices = null;

    private float _r1, _g1, _b1, _a1;
    private float _opacity;

    // Uniform ids:
    private static int loc_myColor1 = 0, loc_myColor2 = 0, loc_uTime = 0, loc_myOpacity = 0;

    private static int verticesLength = 12;
    private static int sizeofFloat_x_verticesLength = sizeof(float) * verticesLength;
    private static int _mode = 0;

    // -------------------------------------------------------------------------------------------------------------------

    public myScreenGradient(int mode = -1)
    {
        // Start the timer
        tBegin = DateTime.Now.Ticks;

        _mode = mode < 0
            ? new System.Random().Next(2)
            : mode;

        // Initial opacity is 1
        _opacity = 1.0f;

        if (vertices == null)
        {
            vertices = new float[verticesLength];

            shaderProgram = CreateShader();
            glUseProgram(shaderProgram);

            // Uniforms
            loc_uTime     = glGetUniformLocation(shaderProgram, "uTime");
            loc_myOpacity = glGetUniformLocation(shaderProgram, "myOpacity");
            loc_myColor1  = glGetUniformLocation(shaderProgram, "myColor1");
            loc_myColor2  = glGetUniformLocation(shaderProgram, "myColor2");

            vbo = glGenBuffer();
            ebo = glGenBuffer();

            updateIndices();

            // Need to do this only once, as it will always be drawn as (0, 0, Width, Height)
            {
                vertices[06] = -1.0f;
                vertices[09] = -1.0f;
                vertices[01] = +1.0f;
                vertices[10] = +1.0f;

                vertices[00] = +1.0f;
                vertices[03] = +1.0f;

                vertices[04] = -1.0f;
                vertices[07] = -1.0f;
            }
        }
    }

    // -------------------------------------------------------------------------------------------------------------------

    public void SetColor2(float r, float g, float b, float a)
    {
        _r1 = r;
        _g1 = g;
        _b1 = b;
        _a1 = a;
    }

    // -------------------------------------------------------------------------------------------------------------------

    public void SetOpacity(float a)
    {
        _opacity = a;
    }

    // -------------------------------------------------------------------------------------------------------------------

    // Set random RGB gradient colors
    public void SetRandomColors(System.Random r, float factor, int mode)
    {
        void rndColor(ref float R, ref float G, ref float B) {
            R = myUtils.randFloat(r) * factor;
            G = myUtils.randFloat(r) * factor;
            B = myUtils.randFloat(r) * factor;
        }

        switch (mode)
        {
            case 0:
                rndColor(ref _r, ref _g, ref _b);
                rndColor(ref _r1, ref _g1, ref _b1);
                break;

            case 1:
                rndColor(ref _r, ref _g, ref _b);
                break;

            case 2:
                rndColor(ref _r1, ref _g1, ref _b1);
                break;

            case 3:
                if (myUtils.randomChance(r, 1, 2))
                    rndColor(ref _r, ref _g, ref _b);
                else
                    rndColor(ref _r1, ref _g1, ref _b1);
                break;
        }
    }

    // -------------------------------------------------------------------------------------------------------------------

    public void Draw()
    {
        glUseProgram(shaderProgram);

        // Update uniforms:
        {
            glUniform4f(loc_myColor1, _r, _g, _b, _a);
            glUniform4f(loc_myColor2, _r1, _g1, _b1, _a1);
            glUniform1f(loc_uTime, (float)(TimeSpan.FromTicks(DateTime.Now.Ticks - tBegin).TotalSeconds));
            glUniform1f(loc_myOpacity, _opacity);
        }

        // Move vertices data from CPU to GPU
        unsafe
        {
            // Bind a buffer;
            // From now on, all the operations on this type of buffer will be performed on the buffer we just bound;
            glBindBuffer(GL_ARRAY_BUFFER, vbo);
            {
                // Copy user-defined data into the currently bound buffer:
                fixed (float* v = &vertices[0])
                    glBufferData(GL_ARRAY_BUFFER, sizeofFloat_x_verticesLength, v, GL_STREAM_DRAW);
            }

            glVertexAttribPointer(0, 3, GL_FLOAT, false, sizeofFloat_x_3, null);
            glEnableVertexAttribArray(0);

            // Draw
            {
                glPolygonMode(GL_FRONT_AND_BACK, GL_FILL);
                glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, ebo);
                glDrawElements(GL_TRIANGLES, 6, GL_UNSIGNED_INT, null);
            }
        }
    }

    // -------------------------------------------------------------------------------------------------------------------

    // Create a shader program
    private static uint CreateShader()
    {
        string vertHead =
            "layout (location = 0) in vec3 pos;";

        string vertMain =
            "gl_Position = vec4(pos, 1.0);";

        // Gradient appears to be banded (distinct steps of color can be seen)
        // To avoid that, use dithering: https://shader-tutorial.dev/advanced/color-banding-dithering/

        string fragHead =
            $@" out vec4 result;
                uniform vec4 myColor1; uniform vec4 myColor2; uniform float uTime; uniform float myOpacity;
                float hInv = { 1.0 / Height };
                float wInv = { 1.0 / Width  };
                {myShaderHelpers.Generic.noiseFunc12_v1}
                {myShaderHelpers.Generic.randFunc}
            ";

        string fragMain =
            getMainFunc();

        return CreateProgram(vertHead, vertMain, fragHead, fragMain);
    }

    // -------------------------------------------------------------------------------------------------------------------

    // Move indices data from CPU to GPU -- needs to be called only once
    // The EBO must be activated prior to drawing the shape: glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, ebo);
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
        }
        glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, 0);
    }

    // -------------------------------------------------------------------------------------------------------------------

    private static string getMainFunc()
    {
        string res = null;

        _mode = 0;

        switch (_mode)
        {
            // Simple vertical gradient
            case 0:
                {
                    res =
                        $@"vec2 st = vec2(0, gl_FragCoord.y * hInv);

                        float mixValue = distance(st, vec2(0, 1));
                        float randValue = rand(gl_FragCoord.x + gl_FragCoord.y);

                        //float noise = 0.9 + noise12_v1(gl_FragCoord.xy * randValue * sin(uTime)) * 0.1;
                        float noise = 0.9 + noise12_v1(gl_FragCoord.xy * sin(uTime)) * 0.1;
                        mixValue *= noise;

                        vec3 color = mix(myColor1.xyz, myColor2.xyz, mixValue);
                        result = vec4(color, myOpacity);
                    ";
                }
                break;

            // Randomized gradient
            case 1:
                {
                    var r = new System.Random();

                    res =
                        $@"vec2 st = vec2(gl_FragCoord.x * wInv, gl_FragCoord.y * hInv);

                        float mixValue = distance(st, vec2({myUtils.randFloat(r)}, {myUtils.randFloat(r)}));
                        float randValue = rand(gl_FragCoord.x + gl_FragCoord.y);

                        //float noise = 0.9 + noise12_v1(gl_FragCoord.xy * randValue) * 0.1;
                        float noise = 0.9 + noise12_v1(gl_FragCoord.xy * sin(uTime)) * 0.1;
                        mixValue *= noise;

                        vec3 color = mix(myColor1.xyz, myColor2.xyz, mixValue);
                        result = vec4(color, myOpacity);
                    ";
                }
                break;

            default:
                System.Diagnostics.Debug.Assert(false, "Unexpected scr.gradient mode.");
                break;
        }

        return res;
    }

    // -------------------------------------------------------------------------------------------------------------------
};
