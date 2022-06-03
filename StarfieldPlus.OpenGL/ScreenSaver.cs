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
    // - 

    private enum ids { myObj_000, myObj_010, myObj_200, myObj_210, myObj_220, myObj_300, myObj_300_test, myObj_999a };

    public void selectObject()
    {
        ids id = (ids)5;

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


// Гофман Э. - Щелкунчик и мышиный король
// Погорельский А. - Черная курица или подземные жители
// Олеша Ю. - Три толстяка
// Велтистов Е. «Приключения Электроника», «Гум-Гам», «Миллион и один день каникул»
// Гайдар А. «Голубая чашка», «Чук и Гек»
// Грэм Кеннет «Ветер в ивах»
// Тим Талер, или Проданный смех
// Льюис Клайв «Хроники Нарнии»
// Владислав Крапивин. Оруженосец Кашка
// Папа, мама, бабушка, восемь детей и грузовик
// Астрид Линдгрен. Эмиль из Леннеберги.
// Сказки народов мира

