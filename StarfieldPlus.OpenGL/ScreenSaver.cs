//
// https://learnopengl.com/
//
// https://thebookofshaders.com/
//
// If you want to read a rectangular area form the framebuffer, then you can use GL.ReadPixels;
// For instance: https://stackoverflow.com/questions/64573427/save-drawn-texture-with-opengl-in-to-a-file
//

// Some cool links:
/*
    https://www.shadertoy.com/view/3tXXRn
*/

public class ScreenSaver
{
    private my.myObject _obj = null;
    private byte _mode;

    private enum ids
    {
        myObj_000, myObj_010, myObj_011, myObj_020, myObj_030, myObj_031, myObj_040, myObj_041, myObj_042, myObj_043, myObj_050, myObj_070, myObj_071, myObj_100,
        myObj_101, myObj_102, myObj_103, myObj_110, myObj_120, myObj_130, myObj_131, myObj_132, myObj_150, myObj_160, myObj_170, myObj_180, myObj_181,
        myObj_200, myObj_210, myObj_220, myObj_230,
        myObj_300, myObj_310, myObj_320, myObj_330, myObj_340, myObj_350, myObj_360, myObj_370, myObj_380, myObj_390,
        myObj_400, myObj_410, myObj_420, myObj_430, myObj_440, myObj_450, myObj_460, myObj_470, myObj_480, myObj_490,
        myObj_500,
        myObj_999a,
        myObj_last
    };

    // -------------------------------------------------------------------------------------------------------------------

    public ScreenSaver()
    {
        my.myObject.gl_Width  = 0;
        my.myObject.gl_Height = 0;

        myOGL.getDesktopResolution(ref my.myObject.gl_Width, ref my.myObject.gl_Height);

#if DEBUG
        bool isWindowed = false;
        _mode = 1;

        if (isWindowed)
        {
            _mode = 2;
            my.myObject.gl_Width  = 1920;
            my.myObject.gl_Height = 1200;
        }
#else
        _mode = 1;
#endif
    }

    // -------------------------------------------------------------------------------------------------------------------

    // todo:
    //  - make every object report its number of submodes; then use this number in a ramdom picking of the active object -- to level out every mode's probability

    // todo from the old StarfieldPlus:
    // - divide the screen in squares and swap them randomly
    // - gravity
    // - gravity towards center, but the particles also bounce off the borders of the screen
    // - sort all the screen pixels
    // - gravity, where the color of a pixel is its mass
    // - posterization (color % int)
    // - divide in squares and each square gets its own blur factor
    // - sperm floating towards the center
    // - cover everything in spiralling traingles
    // - try bezier curves: https://en.wikipedia.org/wiki/B%C3%A9zier_curve
    // - something like myObj_101, but the pieces are moved via sine/cosine function (up-down or elliptically)
    // - randomly generate points. Every point grows its own square (with increasing or decreasing opacity).
    // Grown squares stay a while then fade away.
    // Example: myobj040 + moveType = 1 + shape = 0 + Show == g.FillRectangle(br, X, Y, Size, Size);
    // - bouncing ball, but its trajctory is not straight line, but curved like in obj_040
    // - moving ponts generator, where the moment of generation depends on sin(time)
    // - battle ships
    // - grid over an image. grid pulses, increasing and decreasing its cells size. each cell is displaying average img color
    // - mandlebrot (can i calculate the color of pixels in the shader?..)

    // todo:
    // - number of rotating lines. the length of each line is changing over time
    // - lots of triangles, where each vertice is moving like a bouncing ball
    // - rectangles, where lenght/height are changing constantly; while lenght is increasing, height is decreasing
    // - create random rectangles, but put them on the screen only when they don't intersect any existing rectangles (maybe allow placing on the inside)
    // - neural cellular automata: https://www.youtube.com/watch?v=3H79ZcBuw4M&ab_channel=EmergentGarden
    // - rand rects with the (avg) color of the underlying image; put larger pieces of real texture on a rare occasion
    // - several shapes at the same coordinates, but with different rotating angle (using alternative rotate mode)
    // - sort of a brick breaker game, without a paddle (just bouncing ball)
    // - 

    enum testEnum { one, two, three };

    public void selectObject()
    {
/*
        // How can I access my objects using this enum?..

        var values = (testEnum[])System.Enum.GetValues(typeof(testEnum));

        foreach (var val in values)
        {
            ;
        }
*/


        ids id = (ids)(new System.Random()).Next((int)ids.myObj_last);

#if DEBUG
        id = ids.myObj_500;
        id = ids.myObj_103;
        id = ids.myObj_500;
        id = ids.myObj_071;
#endif

        switch (id)
        {
            // Star Field: ----------- kind of working, but needs finishing the migration -----------
            case ids.myObj_000:
                _obj = new my.myObj_000();
                break;

            // Randomly Roaming Squares (Snow Like)
            case ids.myObj_010:
                _obj = new my.myObj_010();
                break;

            // Randomly Roaming Lines (based on Randomly Roaming Squares)
            case ids.myObj_011:
                _obj = new my.myObj_011();
                break;

            // Linearly Moving Shapes (Soap Bubbles Alike)
            case ids.myObj_020:
                _obj = new my.myObj_020();
                break;

            // Rain Drops (Vertical, Top-Down)
            case ids.myObj_030:
                _obj = new my.myObj_030();
                break;

            // Pseudo-3d-rain
            case ids.myObj_031:
                _obj = new my.myObj_031();
                break;

            // Lines 1: Snake-like branches moving outwards from the center
            case ids.myObj_040:
                _obj = new my.myObj_040();
                break;

            // Lines 2: Branches/snakes moving inwards/outwards with different set of rules
            case ids.myObj_041:
                _obj = new my.myObj_041();
                break;

            // Lines 3: Patchwork / Micro Schematics
            case ids.myObj_042:
                _obj = new my.myObj_042();
                break;

            // Various shapes growing out from a single starting point
            case ids.myObj_043:
                _obj = new my.myObj_043();
                break;

            // Desktop pieces get swapped
            case ids.myObj_050:
                _obj = new my.myObj_050();
                break;

            // Pieces falling off the desktop, ver1
            case ids.myObj_070:
                _obj = new my.myObj_070();
                break;

            // Pieces falling off the desktop, ver2
            case ids.myObj_071:
                _obj = new my.myObj_071();
                break;

            // Big Bang
            case ids.myObj_100:
                _obj = new my.myObj_100();
                break;

            // Desktop 1: - Random pieces of the screen are shown at their own slightly offset locations
            case ids.myObj_101:
                _obj = new my.myObj_101();
                break;

            // Desktop 2: - Random shapes with a color from the underlying image (point-based or average)
            case ids.myObj_102:
                _obj = new my.myObj_102();
                break;

            // Desktop 3: - Random shapes with a color from the underlying image (point-based or average) -- uses custom shader
            case ids.myObj_103:
                _obj = new my.myObj_103();
                break;

            // Desktop 4: Puts random colored shapes all over the screen
            case ids.myObj_110:
                _obj = new my.myObj_110();
                break;

            // Moving Lines (4 directions, straight lines or sin/cos curves)
            case ids.myObj_120:
                _obj = new my.myObj_120();
                break;

            // Growing shapes -- Rain circles alike -- no buffer clearing
            case ids.myObj_130:
                _obj = new my.myObj_130();
                break;

            // Growing shapes -- Rain circles alike
            case ids.myObj_131:
                _obj = new my.myObj_131();
                break;

            // Splines
            case ids.myObj_132:
                _obj = new my.myObj_132();
                break;

            // Grid with moving rectangle lenses -- test, looks strange
/*
            case 20:
                _obj = new my.myObj_140();
                break;*/

            // Conway's Life
            case ids.myObj_150:
                _obj = new my.myObj_150();
                break;

            // Desktop: Ever fading away pieces
            case ids.myObj_160:
                _obj = new my.myObj_160();
                break;

            // Desktop: Diminishing pieces
            case ids.myObj_170:
                _obj = new my.myObj_170();
                break;

            // Single generator of particle waves
            case ids.myObj_180:
                _obj = new my.myObj_180();
                break;

            // Multiple generators of particle waves
            case ids.myObj_181:
                _obj = new my.myObj_181();
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

            // Gravity -- unfinished
            case ids.myObj_230:
                _obj = new my.myObj_230();
                break;

            // Small Explosions of Particles + Movement type Variations
            case ids.myObj_300:
                _obj = new my.myObj_300();
                break;

            // Moving particles, where each particle is connected with every other particle out there
            case ids.myObj_310:
                _obj = new my.myObj_310();
                break;

            // Spiralling doodles made of squares
            case ids.myObj_320:
                _obj = new my.myObj_320();
                break;

            // Textures, Take 1
            case ids.myObj_330:
                _obj = new my.myObj_330();
                break;

            // Grid consisting of hexagons
            case ids.myObj_340:
                _obj = new my.myObj_340();
                break;

            // Moving groups of small particles. Particles within the group are connected to each other.
            case ids.myObj_350:
                _obj = new my.myObj_350();
                break;

            // Moving particles; each particle is connected to 5 other random particles
            case ids.myObj_360:
                _obj = new my.myObj_360();
                break;

            // The image is split into big number of particles that fall down
            case ids.myObj_370:
                _obj = new my.myObj_370();
                break;

            // Rectangular shapes made of particles moving along the rectangle's outline
            case ids.myObj_380:
                _obj = new my.myObj_380();
                break;

            // Particles move radially from the off-center position, creating a vortex-like structure
            case ids.myObj_390:
                _obj = new my.myObj_390();
                break;

            // Circular texture stripes
            case ids.myObj_400:
                _obj = new my.myObj_400();
                break;

            // Concentric vibrating circles around randomly moving center point
            case ids.myObj_410:
                _obj = new my.myObj_410();
                break;

            // - system, where the center attracts and repels all the particles at the same time. vary both forces
            case ids.myObj_420:
                _obj = new my.myObj_420();
                break;

            // Shooters move across the screen, shooting at each other
            case ids.myObj_430:
                _obj = new my.myObj_430();
                break;

            // Bouncing ball and lots of triangles rotating to point to it
            case ids.myObj_440:
                _obj = new my.myObj_440();
                break;

            // Get color from image and slightly offset this color. Then put the color spots on the screen
            case ids.myObj_450:
                _obj = new my.myObj_450();
                break;

            // - point cyclically moves on a spiral, constantly leaving a trail. Trail is made of particles that move outwards from the center OR in a point's opposite direction
            case ids.myObj_460:
                _obj = new my.myObj_460();
                break;

            // - ... skewed lines
            case ids.myObj_470:
                _obj = new my.myObj_470();
                break;

            // - ... harmonic oscillations
            case ids.myObj_480:
                _obj = new my.myObj_480();
                break;

            // - F (x, y)
            case ids.myObj_490:
                _obj = new my.myObj_490();
                break;

            // - Free Shader Experiments
            case ids.myObj_500:
                _obj = new my.myObj_500();
                break;

            // Test rotating shape, unfinished yet good
            case ids.myObj_999a:
                _obj = new my.myObj_999a();
                break;
        }
    }

    // -------------------------------------------------------------------------------------------------------------------

    public byte GetMode()
    {
        return _mode;
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
};
