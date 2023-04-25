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


using System.Linq;
using System.Reflection;


#pragma warning disable IDE1006


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



    public void selectObject()
    {
        my.myObj_Prioritizer.RegisterClass(typeof(my.myObj_000));   // Star Field
        my.myObj_Prioritizer.RegisterClass(typeof(my.myObj_010));   // Randomly Roaming Squares (Snow Like)
        my.myObj_Prioritizer.RegisterClass(typeof(my.myObj_011));   // Randomly Roaming Lines (based on Randomly Roaming Squares)
        my.myObj_Prioritizer.RegisterClass(typeof(my.myObj_020));   // Linearly Moving Shapes (Soap Bubbles Alike)
        my.myObj_Prioritizer.RegisterClass(typeof(my.myObj_030));   // Rain Drops (Vertical, Top-Down)
        my.myObj_Prioritizer.RegisterClass(typeof(my.myObj_031));   // Pseudo-3d-rain
        my.myObj_Prioritizer.RegisterClass(typeof(my.myObj_040));   // Lines 1: Snake-like branches moving outwards from the center
        my.myObj_Prioritizer.RegisterClass(typeof(my.myObj_041));   // Lines 2: Branches/snakes moving inwards/outwards with different set of rules
        my.myObj_Prioritizer.RegisterClass(typeof(my.myObj_042));   // Lines 3: Patchwork / Micro Schematics
        my.myObj_Prioritizer.RegisterClass(typeof(my.myObj_043));   // Various shapes growing out from a single starting point
        my.myObj_Prioritizer.RegisterClass(typeof(my.myObj_050));   // Desktop pieces get swapped
        my.myObj_Prioritizer.RegisterClass(typeof(my.myObj_070));   // Pieces falling off the desktop, ver1
        my.myObj_Prioritizer.RegisterClass(typeof(my.myObj_071));   // Pieces falling off the desktop, ver2
        my.myObj_Prioritizer.RegisterClass(typeof(my.myObj_072));   // Desktop pieces move from the offscreen postion into their original positions
        my.myObj_Prioritizer.RegisterClass(typeof(my.myObj_100));   // Big Bang
        my.myObj_Prioritizer.RegisterClass(typeof(my.myObj_101));   // Desktop 1: - Random pieces of the screen are shown at their own slightly offset locations
        my.myObj_Prioritizer.RegisterClass(typeof(my.myObj_102));   // Desktop 2: - Random shapes with a color from the underlying image (point-based or average)
        my.myObj_Prioritizer.RegisterClass(typeof(my.myObj_103));   // Desktop 3: - Random shapes with a color from the underlying image (point-based or average) -- uses custom shader
        my.myObj_Prioritizer.RegisterClass(typeof(my.myObj_110));   // Desktop 4: Puts random colored shapes all over the screen
        my.myObj_Prioritizer.RegisterClass(typeof(my.myObj_120));   // Moving Lines (4 directions, straight lines or sin/cos curves)
        my.myObj_Prioritizer.RegisterClass(typeof(my.myObj_130));   // Growing shapes -- Rain circles alike -- no buffer clearing
        my.myObj_Prioritizer.RegisterClass(typeof(my.myObj_131));   // Growing shapes -- Rain circles alike
        my.myObj_Prioritizer.RegisterClass(typeof(my.myObj_132));   // Splines

        // Grid with moving rectangle lenses -- test, looks strange
        // Make it like a lense -- but with an area. The tiles colsest to the center get larger scale factor
        // Also, as an option: display a grid, where each cell is an avg color from this position;
        // ANd only where the active object is, display actual texture
        //my.myObj_Prioritizer.RegisterClass(typeof(my.myObj_140));

        my.myObj_Prioritizer.RegisterClass(typeof(my.myObj_150));   // Conway's Life
        my.myObj_Prioritizer.RegisterClass(typeof(my.myObj_160));   // Desktop: Ever fading away pieces
        my.myObj_Prioritizer.RegisterClass(typeof(my.myObj_170));   // Desktop: Diminishing pieces
        my.myObj_Prioritizer.RegisterClass(typeof(my.myObj_180));   // Single generator of particle waves
        my.myObj_Prioritizer.RegisterClass(typeof(my.myObj_181));   // Multiple generators of particle waves
        my.myObj_Prioritizer.RegisterClass(typeof(my.myObj_200));   // Spiraling out shapes
        my.myObj_Prioritizer.RegisterClass(typeof(my.myObj_210));   // Another spiraling shapes -- see what's the difference
        my.myObj_Prioritizer.RegisterClass(typeof(my.myObj_220));   // Falling lines, Matrix-Style
        my.myObj_Prioritizer.RegisterClass(typeof(my.myObj_230));   // Gravity -- unfinished
        my.myObj_Prioritizer.RegisterClass(typeof(my.myObj_300));   // Small Explosions of Particles + Movement type Variations
        my.myObj_Prioritizer.RegisterClass(typeof(my.myObj_310));   // Moving particles, where each particle is connected with every other particle out there
        my.myObj_Prioritizer.RegisterClass(typeof(my.myObj_320));   // Spiralling doodles made of squares
        my.myObj_Prioritizer.RegisterClass(typeof(my.myObj_330));   // Textures, Take 1
        my.myObj_Prioritizer.RegisterClass(typeof(my.myObj_340));   // Grid consisting of hexagons
        my.myObj_Prioritizer.RegisterClass(typeof(my.myObj_350));   // Moving groups of small particles. Particles within the group are connected to each other
        my.myObj_Prioritizer.RegisterClass(typeof(my.myObj_360));   // Moving particles; each particle is connected to 5 other random particles
        my.myObj_Prioritizer.RegisterClass(typeof(my.myObj_370));   // The image is split into big number of particles that fall down
        my.myObj_Prioritizer.RegisterClass(typeof(my.myObj_380));   // Rectangular shapes made of particles moving along the rectangle's outline
        my.myObj_Prioritizer.RegisterClass(typeof(my.myObj_390));   // Particles move radially from the off-center position, creating a vortex-like structure
        my.myObj_Prioritizer.RegisterClass(typeof(my.myObj_400));   // Circular texture stripes
        my.myObj_Prioritizer.RegisterClass(typeof(my.myObj_410));   // Concentric vibrating circles around randomly moving center point
        my.myObj_Prioritizer.RegisterClass(typeof(my.myObj_420));   // System, where the center attracts and repels all the particles at the same time. vary both forces
        my.myObj_Prioritizer.RegisterClass(typeof(my.myObj_430));   // Shooters move across the screen, shooting at each other
        my.myObj_Prioritizer.RegisterClass(typeof(my.myObj_440));   // Bouncing ball and lots of triangles rotating to point to it
        my.myObj_Prioritizer.RegisterClass(typeof(my.myObj_450));   // Get color from image and slightly offset this color. Then put the color spots on the screen
        my.myObj_Prioritizer.RegisterClass(typeof(my.myObj_460));   // Point cyclically moves on a spiral, constantly leaving a trail. Trail is made of particles that move outwards from the center OR in a point's opposite direction
        my.myObj_Prioritizer.RegisterClass(typeof(my.myObj_470));   // - ... skewed lines
        my.myObj_Prioritizer.RegisterClass(typeof(my.myObj_480));   // - ... harmonic oscillations
        my.myObj_Prioritizer.RegisterClass(typeof(my.myObj_490));   // F (x, y)
        my.myObj_Prioritizer.RegisterClass(typeof(my.myObj_500));   // Free Shader Experiments
        my.myObj_Prioritizer.RegisterClass(typeof(my.myObj_501));   // Free Shader Experiments - 2

        my.myObj_Prioritizer.RegisterClass(typeof(my.myObj_999a));  // Test rotating shape, unfinished yet good

        _obj = my.myObj_Prioritizer.GetRandomObject(usePriority: true);

        return;
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


#pragma warning restore IDE1006
