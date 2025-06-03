using static OpenGL.GL;
using System;


/*
    Custom texture shader class with additional uniform uTime variable
*/


class myTexRectangle_001 : myTexRectangle
{
    private int loc_uTime = -123;
    private long tBegin;

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

    // Simple sine wave
    public static void getShader_000(ref string fragHeader, ref string fragMain)
    {
        fragHeader =
                $@"out vec4 result;
                        in vec4 fragColor;
                        in vec2 fragTxCoord;
                        uniform float uTime;
                        uniform sampler2D myTexture;
                ";

        fragMain =
            $@" float x = sin(fragTxCoord.y * 33 + uTime) * 0.01;
                    float y = cos(fragTxCoord.x * 33 + uTime) * 0.01;
                    y = 0;
                    vec2 offset = vec2(x, y) * 0.3;
                    result = texture(myTexture, fragTxCoord + offset) * fragColor;";
    }

    // ---------------------------------------------------------------------------------------------------------------
}
