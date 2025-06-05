using static OpenGL.GL;
using System;
using my;


/*
    Custom texture shader class with additional uniform uTime variable
    Requires calling 
        myTexRectangle_001.getShader_000(ref fHeader, ref fMain)
    before instantiating
*/


class myTexRectangle_001 : myTexRectangle
{
    private int loc_uTime = -123;
    private long tBegin;
    public static string Mode, ColorMode;
    public static int gl_Width, gl_Height;

    // ---------------------------------------------------------------------------------------------------------------

    public myTexRectangle_001(string path, string fragHead = "", string fragMain = "")
        : base (path, "", "", fragHead, fragMain)
    {
        init();
    }

    // ---------------------------------------------------------------------------------------------------------------

    public myTexRectangle_001(System.Drawing.Bitmap bmp, string fragHead = "", string fragMain = "")
        : base(bmp, "", "", fragHead, fragMain)
    {
        init();
    }

    // ---------------------------------------------------------------------------------------------------------------

    private void init()
    {
        if (loc_uTime < 0)
        {
            tBegin = DateTime.Now.Ticks;
            loc_uTime = glGetUniformLocation(shaderProgram, "uTime");
        }
    }

    // ---------------------------------------------------------------------------------------------------------------

    public new void Draw(int x, int y, int w, int h, int ptx = 0, int pty = 0, int ptw = 0, int pth = 0)
    {
        glUseProgram(shaderProgram);
        glUniform1f(loc_uTime, (float)(TimeSpan.FromTicks(DateTime.Now.Ticks - tBegin).TotalSeconds));

        base.Draw(x, y, w, h, ptx, pty, ptw, pth);
    }

    // ---------------------------------------------------------------------------------------------------------------

    private static string getStdHeader()
    {
        return $@"out vec4 result;
                    in vec4 fragColor;
                    in vec2 fragTxCoord;
                    uniform float uTime;
                    uniform sampler2D myTexture;
                    {myShaderHelpers.Generic.noiseFunc12_v1}
                    vec2 iResolution = vec2({gl_Width}, {gl_Height});";
    }

    // ---------------------------------------------------------------------------------------------------------------

    // Simple sine wave
    public static void getShader_000(ref string fragHeader, ref string fragMain)
    {
        bool isReady = false;
        var rand = new Random((int)DateTime.Now.Ticks);
        int mode = rand.Next(7);
        int colorMode = rand.Next(2);

        Mode = $"000:{mode}";
        ColorMode = $"000:{colorMode}";

        fragHeader = getStdHeader();
        fragMain = @"
                    float X = fragTxCoord.x;
                    float Y = fragTxCoord.y;
                    vec2 uv = (gl_FragCoord.xy - iResolution.xy * 0.5) / iResolution.y;";

        switch (mode)
        {
            case 0:
                fragMain +=
                    $@"
                        float x = sin(Y * 33 + uTime) * 0.01;
                        float y = 0;
                        vec2 offset = vec2(x, y) * 0.3;
                        result = texture(myTexture, fragTxCoord + offset) * fragColor;";
                break;

            case 1:
                fragMain +=
                    $@"
                        float x = sin(Y * 33 + uTime) * 0.01;
                        float y = cos(X * 33 + uTime) * 0.01;
                        vec2 offset = vec2(x, y) * 0.3;
                        result = texture(myTexture, fragTxCoord + offset) * fragColor;";
                break;

            case 2:
                fragMain +=
                    $@"
                        float x = sin(Y * {11 + rand.Next(111)} + uTime) * 0.01;
                        float y = cos(X * {11 + rand.Next(333)} + uTime) * 0.01;
                        vec2 offset = vec2(x, y) * 0.3;
                        result = texture(myTexture, fragTxCoord + offset) * fragColor;";
                break;

            case 3:
                fragMain +=
                    $@"
                        float x = sin(X * {11 + rand.Next(111)} + uTime) * 0.01;
                        float y = 0;
                        vec2 offset = vec2(x, y) * 0.3;
                        result = texture(myTexture, fragTxCoord + offset) * fragColor;";
                break;

            case 4:
                fragMain +=
                    $@"
                        float x = 0;
                        float y = sin(Y * {11 + rand.Next(111)} + uTime) * 0.01;
                        vec2 offset = vec2(x, y) * 0.3;
                        result = texture(myTexture, fragTxCoord + offset) * fragColor;";
                break;

            case 5:
                fragMain +=
                    $@"
                        float x = sin(X * {11 + rand.Next(111)} + uTime) * 0.01;
                        float y = sin(Y * {11 + rand.Next(111)} + uTime) * 0.01;
                        vec2 offset = vec2(x, y) * 0.3;
                        result = texture(myTexture, fragTxCoord + offset) * fragColor;";
                break;

            // Only RGB offsets
            case 6:
                isReady = true;
                ColorMode = $"000: n/a";
                fragMain +=
                    $@"
                        float x1 = sin(X * {11 + rand.Next(111)} + uTime) * 0.01;
                        float y1 = sin(Y * {11 + rand.Next(111)} + uTime) * 0.01;

                        float x2 = sin(X * {11 + rand.Next(111)} + uTime) * 0.01;
                        float y2 = sin(Y * {11 + rand.Next(111)} + uTime) * 0.01;

                        float x3 = sin(X * {11 + rand.Next(111)} + uTime) * 0.01;
                        float y3 = sin(Y * {11 + rand.Next(111)} + uTime) * 0.01;

                        vec2 off1 = vec2(x1, y1) * {myUtils.randFloatClamped(rand, 0.1f)};
                        vec2 off2 = vec2(x2, y2) * {myUtils.randFloatClamped(rand, 0.1f)};
                        vec2 off3 = vec2(x3, y3) * {myUtils.randFloatClamped(rand, 0.1f)};

                        float final = 0.85 + noise12_v1(gl_FragCoord.xy * uv * uTime * 0.01) * 0.15;

                        float r = texture(myTexture, fragTxCoord + off1).r * final;
                        float g = texture(myTexture, fragTxCoord + off2).g * final;
                        float b = texture(myTexture, fragTxCoord + off3).b * final;

                        result = vec4(r, g, b, 1);";
                break;
        }

        if (!isReady)
        {
            switch (colorMode)
            {
                case 0:
                    fragMain += "result = texture(myTexture, fragTxCoord + offset) * fragColor;";
                    break;

                case 1:
                    fragMain +=
                        $@" vec2 off1 = vec2(x, y) * 0.1;
                        vec2 off2 = vec2(x, y) * 0.3;
                        vec2 off3 = vec2(x, y) * 0.5;

                        float r = texture(myTexture, fragTxCoord + off1).r;
                        float g = texture(myTexture, fragTxCoord + off2).g;
                        float b = texture(myTexture, fragTxCoord + off3).b;

                        result = vec4(r, g, b, 1);";
                    break;
            }
        }

        return;
    }

    // ---------------------------------------------------------------------------------------------------------------
}
