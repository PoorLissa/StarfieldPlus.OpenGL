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

// Windows 10 problems:
/*
    1. System.Threading.Thread.Sleep(renderDelay) causes irregular movement of particles
    2. When NVidia Vulkan OpenGL present method is Auto or Native, there is a problem with drawing info winform over the opengl window
*/


#pragma warning disable IDE1006


public class ScreenSaver
{
    private my.myObject _obj = null;
    private byte _mode;

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
    // - create random rectangles, but put them on the screen only when they don't intersect any existing rectangles (maybe allow placing on the inside)
    // - neural cellular automata: https://www.youtube.com/watch?v=3H79ZcBuw4M&ab_channel=EmergentGarden
    // - rand rects with the (avg) color of the underlying image; put larger pieces of real texture on a rare occasion
    // - several shapes at the same coordinates, but with different rotating angle (using alternative rotate mode)
    // - sort of a brick breaker game, without a paddle (just bouncing ball)



    public void selectObject()
    {
        my.myObj_Prioritizer.RegisterClass(my.myObj_000.Type);      // Star Field
        my.myObj_Prioritizer.RegisterClass(my.myObj_010.Type);      // Randomly Roaming Squares (Snow Like)
        my.myObj_Prioritizer.RegisterClass(my.myObj_011.Type);      // Randomly Roaming Lines (based on Randomly Roaming Squares)
        my.myObj_Prioritizer.RegisterClass(my.myObj_011a.Type);     // Particles with real trails
        my.myObj_Prioritizer.RegisterClass(my.myObj_011b.Type);     // Particles with real trails again
        my.myObj_Prioritizer.RegisterClass(my.myObj_012.Type);      // Snow-like pattern made of different layers moving in different directions
        my.myObj_Prioritizer.RegisterClass(my.myObj_020.Type);      // Linearly Moving Shapes (Soap Bubbles Alike)
        my.myObj_Prioritizer.RegisterClass(my.myObj_021.Type);      // Ever Growing Shapes located at the center of the screen + small offset
        my.myObj_Prioritizer.RegisterClass(my.myObj_030.Type);      // Rain Drops (Vertical, Top-Down)
        my.myObj_Prioritizer.RegisterClass(my.myObj_031.Type);      // Pseudo-3d-rain
        my.myObj_Prioritizer.RegisterClass(my.myObj_040.Type);      // Lines 1: Snake-like branches moving outwards from the center
        my.myObj_Prioritizer.RegisterClass(my.myObj_041.Type);      // Lines 2: Branches/snakes moving inwards/outwards with different set of rules
        my.myObj_Prioritizer.RegisterClass(my.myObj_042.Type);      // Lines 3: Patchwork / Micro Schematics
        my.myObj_Prioritizer.RegisterClass(my.myObj_043.Type);      // Various shapes growing out from a single starting point
        my.myObj_Prioritizer.RegisterClass(my.myObj_050.Type);      // Desktop pieces get swapped
        my.myObj_Prioritizer.RegisterClass(my.myObj_070.Type);      // Pieces falling off the desktop, ver1
        my.myObj_Prioritizer.RegisterClass(my.myObj_071.Type);      // Pieces falling off the desktop, ver2
        my.myObj_Prioritizer.RegisterClass(my.myObj_072.Type);      // Desktop pieces move from the offscreen position into their original positions
        my.myObj_Prioritizer.RegisterClass(my.myObj_100.Type);      // Big Bang
        my.myObj_Prioritizer.RegisterClass(my.myObj_101.Type);      // Desktop 1: - Random pieces of the screen are shown at their own slightly offset locations
        my.myObj_Prioritizer.RegisterClass(my.myObj_102.Type);      // Desktop 2: - Random shapes with a color from the underlying image (point-based or average)
        my.myObj_Prioritizer.RegisterClass(my.myObj_103.Type);      // Desktop 3: - Random shapes with a color from the underlying image (point-based or average) -- uses custom shader
        my.myObj_Prioritizer.RegisterClass(my.myObj_110.Type);      // Desktop 4: Puts random colored shapes all over the screen
        my.myObj_Prioritizer.RegisterClass(my.myObj_120.Type);      // Moving Lines (4 directions, straight lines or sin/cos curves)
        my.myObj_Prioritizer.RegisterClass(my.myObj_130.Type);      // Growing shapes -- Rain circles alike -- no buffer clearing
        my.myObj_Prioritizer.RegisterClass(my.myObj_131.Type);      // Growing shapes -- Rain circles alike
        my.myObj_Prioritizer.RegisterClass(my.myObj_132.Type);      // Splines
        my.myObj_Prioritizer.RegisterClass(my.myObj_140.Type);      // Grid with moving rectangle lenses -- test, looks strange
        my.myObj_Prioritizer.RegisterClass(my.myObj_150.Type);      // Conway's Life
        my.myObj_Prioritizer.RegisterClass(my.myObj_160.Type);      // Desktop: Ever fading away pieces
        my.myObj_Prioritizer.RegisterClass(my.myObj_170.Type);      // Desktop: Diminishing pieces
        my.myObj_Prioritizer.RegisterClass(my.myObj_180.Type);      // Single generator of particle waves
        my.myObj_Prioritizer.RegisterClass(my.myObj_181.Type);      // Multiple generators of particle waves
        my.myObj_Prioritizer.RegisterClass(my.myObj_200.Type);      // Spiraling out shapes
        my.myObj_Prioritizer.RegisterClass(my.myObj_210.Type);      // Another spiraling shapes -- see what's the difference
        my.myObj_Prioritizer.RegisterClass(my.myObj_220.Type);      // Falling lines, Matrix-Style
        my.myObj_Prioritizer.RegisterClass(my.myObj_230.Type);      // Gravity -- unfinished
        my.myObj_Prioritizer.RegisterClass(my.myObj_300.Type);      // Small Explosions of Particles + Movement type Variations
        my.myObj_Prioritizer.RegisterClass(my.myObj_310.Type);      // Moving particles, where each particle is connected with every other particle out there
        my.myObj_Prioritizer.RegisterClass(my.myObj_320.Type);      // Spiralling doodles made of squares
        my.myObj_Prioritizer.RegisterClass(my.myObj_330.Type);      // Textures, Take 1
        my.myObj_Prioritizer.RegisterClass(my.myObj_340.Type);      // Grid consisting of hexagons
        my.myObj_Prioritizer.RegisterClass(my.myObj_350.Type);      // Moving groups of small particles. Particles within the group are connected to each other
        my.myObj_Prioritizer.RegisterClass(my.myObj_360.Type);      // Moving particles; each particle is connected to 5 other random particles
        my.myObj_Prioritizer.RegisterClass(my.myObj_370.Type);      // The image is split into big number of particles that fall down
        my.myObj_Prioritizer.RegisterClass(my.myObj_380.Type);      // Rectangular shapes made of particles moving along the rectangle's outline
        my.myObj_Prioritizer.RegisterClass(my.myObj_390.Type);      // Particles move radially from the off-center position, creating a vortex-like structure
        my.myObj_Prioritizer.RegisterClass(my.myObj_400.Type);      // Circular texture stripes
        my.myObj_Prioritizer.RegisterClass(my.myObj_410.Type);      // Concentric vibrating circles around randomly moving center point
        my.myObj_Prioritizer.RegisterClass(my.myObj_420.Type);      // System, where the center attracts and repels all the particles at the same time. vary both forces
        my.myObj_Prioritizer.RegisterClass(my.myObj_430.Type);      // Shooters move across the screen, shooting at each other
        my.myObj_Prioritizer.RegisterClass(my.myObj_440.Type);      // Bouncing ball + lots of shapes rotating to always point towards it
        my.myObj_Prioritizer.RegisterClass(my.myObj_450.Type);      // Get color from image and slightly offset this color. Then put the color spots on the screen
        my.myObj_Prioritizer.RegisterClass(my.myObj_460.Type);      // Point cyclically moves on a spiral, constantly leaving a trail. Trail is made of particles that move outwards from the center OR in a point's opposite direction
        my.myObj_Prioritizer.RegisterClass(my.myObj_470.Type);      // Angled rays
        my.myObj_Prioritizer.RegisterClass(my.myObj_480.Type);      // Oscilloscope (running harmonics)
        my.myObj_Prioritizer.RegisterClass(my.myObj_490.Type);      // F (x, y)
        my.myObj_Prioritizer.RegisterClass(my.myObj_500.Type);      // Free Shader Experiments - 1
        my.myObj_Prioritizer.RegisterClass(my.myObj_501.Type);      // Free Shader Experiments - 2
        my.myObj_Prioritizer.RegisterClass(my.myObj_510.Type);      // ...
        my.myObj_Prioritizer.RegisterClass(my.myObj_520.Type);      // Static pulsating shapes
        my.myObj_Prioritizer.RegisterClass(my.myObj_530.Type);      // A ring of moving particles
        my.myObj_Prioritizer.RegisterClass(my.myObj_540.Type);      // Falling alphabet letters (Matrix style)
        my.myObj_Prioritizer.RegisterClass(my.myObj_550.Type);      // Orbits of different size + a small planet is moving along each orbit
        my.myObj_Prioritizer.RegisterClass(my.myObj_560.Type);      // Pixelating an image with average colors
        my.myObj_Prioritizer.RegisterClass(my.myObj_570.Type);      // ...
        my.myObj_Prioritizer.RegisterClass(my.myObj_580.Type);      // Gravity n-body
        my.myObj_Prioritizer.RegisterClass(my.myObj_590.Type);      // Particle moves as a result of an average of n other particles movement
        my.myObj_Prioritizer.RegisterClass(my.myObj_600.Type);      // Pendulum
        my.myObj_Prioritizer.RegisterClass(my.myObj_610.Type);      // ...
        my.myObj_Prioritizer.RegisterClass(my.myObj_620.Type);      // Rectangles with width/height that are changing constantly; while width is increasing, height is decreasing, and vice versa
        my.myObj_Prioritizer.RegisterClass(my.myObj_630.Type);      // Rotating circles made of letters and symbols

        my.myObj_Prioritizer.RegisterClass(my.myObj_999a.Type);     // Test rotating shape, unfinished yet good

#if false
        // Trails test
        my.myObj_Prioritizer.RegisterClass(my.myObj_999_test_001.Type);
#endif

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
