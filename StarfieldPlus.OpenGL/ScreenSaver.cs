﻿using System;
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

    // - concentric circles moving inwards. The lesss the circle is, the less is its decreasing speed. Should look like a funnel of sorts

    public void selectObject()
    {
        int id = 0;

        switch (id)
        {
            case 0:
                _obj = new my.myObj_200();          // Spiraling out shapes
                break;

            default:
                _obj = new my.myObj_999a();
                break;

        }
    }

    // -------------------------------------------------------------------------------------------------------------------
};
