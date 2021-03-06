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
    // - number of rotating lines. the length of each line is changing over time
    // - lots of triangles, where each vertice is moving like a bouncing ball
    // - rectangles, where lenght/height are changing constantly; while lenght is increasing, height is decreasing
    // - create random rectangles, but put them on the screen only when they don't intersect any existing rectangles (maybe allow placing on the inside)
    // - 

    private enum ids { myObj_000, myObj_010, myObj_011, myObj_030, myObj_102, myObj_180, myObj_200, myObj_210, myObj_220, myObj_300, myObj_310, myObj_320, myObj_300_test, myObj_999a };

    public void selectObject()
    {
        ids id = (ids)2;

        id = ids.myObj_310;
        id = ids.myObj_011;
        id = ids.myObj_102;
        id = ids.myObj_030;

        switch (id)
        {
            // Stars: todo later
            case ids.myObj_000:
                //_obj = new my.myObj_000();
                break;

            // Randomly Roaming Squares (Snow Like)
            case ids.myObj_010:
                _obj = new my.myObj_010();
                break;

            // Randomly Roaming Lines (based on Randomly Roaming Squares)
            case ids.myObj_011:
                _obj = new my.myObj_011();
                break;

            // Rain Drops(Vertical, Top-Down)
            case ids.myObj_030:
                _obj = new my.myObj_030();
                break;

            // Desktop 2
            case ids.myObj_102:
                _obj = new my.myObj_102();
                break;

            // Generator of waves that are made of particles
            case ids.myObj_180:
                _obj = new my.myObj_180();
                break;

            // Spiraling out shapes
            case ids.myObj_200:
                _obj = new my.myObj_200();
                break;

            // Another spiraling shapes -- see what's the difference
            case ids.myObj_210:
                _obj = new my.myObj_210();
                break;

            // Falling lines, Matrix-Style
            case ids.myObj_220:
                _obj = new my.myObj_220();
                break;

            // Small Explosions of Particles + Movement type Variations
            case ids.myObj_300:
                _obj = new my.myObj_300();
                break;

            // Triangulation
            case ids.myObj_310:
                _obj = new my.myObj_310();
                break;

            case ids.myObj_320:
                _obj = new my.myObj_320();
                break;

            case ids.myObj_300_test:
                _obj = new my.myObj_300_test();
                break;

            default:
                _obj = new my.myObj_999a();
                break;
        }
    }

    // -------------------------------------------------------------------------------------------------------------------
};
