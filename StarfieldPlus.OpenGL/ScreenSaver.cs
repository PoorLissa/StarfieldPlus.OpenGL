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
    // - concentric circles moving inwards. The less the circle is, the less is its decreasing speed. Should look like a funnel of sorts

    public void selectObject()
    {
        int id = 1;

        switch (id)
        {
            // Stars
            case 0:
                //_obj = new my.myObj_010();
                break;

            // Randomly Roaming Squares (Snow Like)
            case 1:
                _obj = new my.myObj_010();
                break;

            // Spiraling out shapes
            case 2:
                _obj = new my.myObj_200();
                break;

            // Another spiraling shapes -- see what's the difference
            case 3:
                _obj = new my.myObj_210();
                break;

            // Small Explosions of Particles + Movement type Variations
            case 4:
                _obj = new my.myObj_300();
                break;

            case 5:
                _obj = new my.myObj_300_test();
                break;

            default:
                _obj = new my.myObj_999a();
                break;
        }
    }

    // -------------------------------------------------------------------------------------------------------------------
};
