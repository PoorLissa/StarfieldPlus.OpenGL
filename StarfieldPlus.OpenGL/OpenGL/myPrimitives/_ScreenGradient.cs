using my;
using static OpenGL.GL;

/*
    This class only serves one purpose: drawing a full screen color gradient
*/

public class myScreenGradient : myPrimitive
{
    private static uint vbo = 0, ebo = 0, shaderProgram = 0;
    private static float[] vertices = null;

    private float _r1, _g1, _b1, _a1;

    // Uniform ids:
    private static int _myColor1 = 0, _myColor2 = 0;

    private static int verticesLength = 12;
    private static int sizeofFloat_x_verticesLength = sizeof(float) * verticesLength;
    private static int _mode = 0;

    // -------------------------------------------------------------------------------------------------------------------

    public myScreenGradient(int mode = -1)
    {
        _mode = mode < 0
            ? new System.Random().Next(2)
            : mode;

        if (vertices == null)
        {
            vertices = new float[verticesLength];

            CreateProgram();
            glUseProgram(shaderProgram);

            // Uniforms
            _myColor1 = glGetUniformLocation(shaderProgram, "myColor1");
            _myColor2 = glGetUniformLocation(shaderProgram, "myColor2");

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
            glUniform4f(_myColor1, _r, _g, _b, _a);
            glUniform4f(_myColor2, _r1, _g1, _b1, _a1);
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

            glVertexAttribPointer(0, 3, GL_FLOAT, false, sizeofFloat_x_3, NULL);
            glEnableVertexAttribArray(0);


            // Draw
            {
                glPolygonMode(GL_FRONT_AND_BACK, GL_FILL);
                glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, ebo);
                glDrawElements(GL_TRIANGLES, 6, GL_UNSIGNED_INT, NULL);
            }
        }
    }

    // -------------------------------------------------------------------------------------------------------------------

    // Create a shader program
    private static void CreateProgram()
    {
        var vertex = myOGL.CreateShaderEx(GL_VERTEX_SHADER,

            header: @"layout (location = 0) in vec3 pos;",

                main: "gl_Position = vec4(pos, 1.0);"
        );

        // Gradient appears to be banded (distinct steps of color can be seen)
        // To avoid that, use dithering: https://shader-tutorial.dev/advanced/color-banding-dithering/
        var fragment = myOGL.CreateShaderEx(GL_FRAGMENT_SHADER,

            header: $@"out vec4 result; uniform vec4 myColor1; uniform vec4 myColor2;
                                float hInv = 1.0 / {Height}; float wInv = 1.0 / {Width};
                {myShaderHelpers.Generic.noiseFunc12_v1}
                {myShaderHelpers.Generic.randFunc}",

                main: getMainFunc()
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

            // Unbind current buffer
            glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, 0);
        }
    }

    // -------------------------------------------------------------------------------------------------------------------

    private static string getMainFunc()
    {
        string res = null;

        switch (_mode)
        {
            // Simple vertical gradient
            case 0:
                {
                    res = $@"vec2 st = vec2(0, gl_FragCoord.y * hInv);

                    float mixValue = distance(st, vec2(0, 1));
                    float randValue = rand(gl_FragCoord.x + gl_FragCoord.y);

                    float noise = 0.9 + noise12_v1(gl_FragCoord.xy * randValue) * 0.1;
                    mixValue *= noise;

                    vec3 color = mix(myColor1.xyz, myColor2.xyz, mixValue);
                    result = vec4(color, 1);";
                }
                break;

            // Randomized gradient
            case 1:
                {
                    var r = new System.Random();

                    res = $@"vec2 st = vec2(gl_FragCoord.x * wInv, gl_FragCoord.y * hInv);

                    float mixValue = distance(st, vec2({myUtils.randFloat(r)}, {myUtils.randFloat(r)}));
                    float randValue = rand(gl_FragCoord.x + gl_FragCoord.y);

                    float noise = 0.9 + noise12_v1(gl_FragCoord.xy * randValue) * 0.1;
                    mixValue *= noise;

                    vec3 color = mix(myColor1.xyz, myColor2.xyz, mixValue);
                    result = vec4(color, 1);";
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
