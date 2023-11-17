using GLFW;
using static OpenGL.GL;


/*
    Base class for all my drawing OpenGL instanced primitives

    https://learnopengl.com/code_viewer_gh.php?code=src/4.advanced_opengl/10.1.instancing_quads/instancing_quads.cpp
*/


public class myInstancedPrimitive : myPrimitive
{
    public enum drawMode { OWN_COLOR_OWN_OPACITY, OWN_COLOR_CUSTOM_OPACITY, CUSTOM_COLOR_CUSTOM_OPACITY };

    // ---------------------------------------------------------------------------------------

    protected static float pixelX = 0, pixelY = 0;

    protected float[] instanceArray = null;

    protected int instArrayPosition = 0, N = 0, n = 0;
    protected drawMode _drawMode;

    public myInstancedPrimitive()
    {
        instArrayPosition = 0;
        N = 0;

        // Number of elements in [instanceArray] that define one single instance
        n = 0;  

        pixelX = 1.0f / Width;
        pixelY = 1.0f / Height;

        _drawMode = drawMode.OWN_COLOR_OWN_OPACITY;
    }

    // ---------------------------------------------------------------------------------------

    protected virtual unsafe void updateInstances()
    {
    }

    // ---------------------------------------------------------------------------------------

    public virtual void Draw(bool doFill = false)
    {
    }

    // ---------------------------------------------------------------------------------------

    // Reset the position in the buffer;
    // From now on, the buffer will be filled starting from zero again
    public void ResetBuffer()
    {
        instArrayPosition = 0;

        // Reset N as well;
        // In case we don't do that,
        // ResetBuffer() followed by immediate Draw(), will still draw everything what's in the current buffer
        N = 0;
    }

    // ---------------------------------------------------------------------------------------

    // Reallocate inner instances array, if its size is less than the new size
    public void Resize(int Size)
    {
        if (instanceArray.Length < Size * n)
        {
            instanceArray = new float[Size * n];
        }
    }

    // ---------------------------------------------------------------------------------------

    public void setDrawingMode(drawMode mode)
    {
        _drawMode = mode;
    }

    // ---------------------------------------------------------------------------------------

    // Sometimes shapes are drawn incorrectly (parts of the outline are missing)
    // One way to fix this is to change the pixel density
    public void setPixelDensityOffset(int offset)
    {
        if (offset > 0)
        {
            pixelX = 1.0f / (Width  + offset);
            pixelY = 1.0f / (Height + offset);
        }
    }

    // ---------------------------------------------------------------------------------------
};
