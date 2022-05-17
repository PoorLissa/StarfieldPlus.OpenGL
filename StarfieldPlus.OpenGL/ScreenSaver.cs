using System;
using System.Windows.Forms;



public class ScreenSaver
{
    private my.myObject _obj = null;

    // -------------------------------------------------------------------------------------------------------------------

    public ScreenSaver(int Width, int Height)
    {
        my.myObject.gl_Width  = Width;
        my.myObject.gl_Height = Height;
    }

    // -------------------------------------------------------------------------------------------------------------------

    public void Show()
    {
        if (_obj != null)
        {
            _obj.Process(this);
        }

        return;
    }

    // -------------------------------------------------------------------------------------------------------------------

    // todo:
    // - concentric circles moving inwards. The lesss the circle is, the less is its decreasing speed. Should look like a funnel of sorts

    public void selectObject()
    {
        int id = 2;

        switch (id)
        {
            case 0:
                _obj = new my.myObj_200();          // Spiraling out shapes
                break;

            case 1:
                _obj = new my.myObj_210();          // Another spiraling shapes -- see what's the difference
                break;

            case 2:
                _obj = new my.myObj_300();          // Small Explosions of Particles + Variations
                break;

            case 3:
                _obj = new my.myObj_300_test();
                break;

            default:
                _obj = new my.myObj_999a();
                break;
        }
    }

    // -------------------------------------------------------------------------------------------------------------------
};
