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
    // - divide in squares and each square gets its own blur factor
    // - sperm floating towards the center
    // - cover everything in spiralling traingles
    // - something like myObj_101, but the pieces are moved via sine/cosine function (up-down or elliptically)
    // - randomly generate points. Every point grows its own square (with increasing or decreasing opacity). Grown squares stay a while then fade away.
    //      Example: myobj040 + moveType = 1 + shape = 0 + Show == g.FillRectangle(br, X, Y, Size, Size);
    // - bouncing ball, but its trajctory is not straight line, but curved like in obj_040
    // - moving ponts generator, where the moment of generation depends on sin(time)
    // - battle ships
    // - grid over an image. grid pulses, increasing and decreasing its cells size. each cell is displaying average img color

    // todo:
    // - try bezier curves: https://en.wikipedia.org/wiki/B%C3%A9zier_curve
    // - try neural cellular automata: https://www.youtube.com/watch?v=3H79ZcBuw4M&ab_channel=EmergentGarden
    // - number of rotating lines. the length of each line is changing over time
    // - rand rects with the (avg) color of the underlying image; put larger pieces of real texture on a rare occasion
    // - several shapes at the same coordinates, but with different rotating angle (using alternative rotate mode)
    // - sort of a brick breaker game, without a paddle (just bouncing ball)
    // - snake that does not cross itself (use lists of vertical and horizontal lines the snake consists of)
    // - a whole screen graph, where all the connections are not straight lines, but lightning-like sectional curves that change over time
    // - get a picture, go through all its pixels and map them <R+G+B to Count>, so we know percentages of all the colors in the image; then pick a color that is 20-40%, and only display those pixels
    // - make a grid like in conway's life, and then get an image and put grid cells using image's color; when the image is done, select another image and so on

    // -------------------------------------------------------------------------------------------------------------------

    public void selectObject()
    {
        void register(System.Type t) {
            my.myObj_Prioritizer.RegisterClass(t);
        }

        register(my.myObj_0000.Type);       // Star Field
        register(my.myObj_0010.Type);       // Randomly Roaming Squares (Snow Like)
        register(my.myObj_0011.Type);       // Randomly Roaming Lines (based on Randomly Roaming Squares)
        register(my.myObj_0012.Type);       // Particles with real trails
        register(my.myObj_0013.Type);       // Particles with real trails again
        register(my.myObj_0014.Type);       // Particles with real trails again again
        register(my.myObj_0015.Type);       // Snow-like pattern made of different layers moving in different directions
        register(my.myObj_0020.Type);       // Linearly Moving Shapes (Soap Bubbles Alike)
        register(my.myObj_0021.Type);       // Ever Growing Shapes located at the center of the screen + small offset
        register(my.myObj_0030.Type);       // Rain Drops (Vertical, Top-Down)
        register(my.myObj_0031.Type);       // Pseudo-3d-rain
        register(my.myObj_0040.Type);       // Lines 1: Snake-like branches moving outwards from the center
        register(my.myObj_0041.Type);       // Lines 2: Branches/snakes moving inwards/outwards with different set of rules
        register(my.myObj_0042.Type);       // Lines 3: Patchwork / Micro Schematics
        register(my.myObj_0043.Type);       // Various shapes growing out from a single starting point
        register(my.myObj_0050.Type);       // Desktop pieces get swapped
        register(my.myObj_0070.Type);       // Pieces falling off the desktop, ver1
        register(my.myObj_0071.Type);       // Pieces falling off the desktop, ver2
        register(my.myObj_0072.Type);       // Desktop pieces move from the offscreen position into their original positions
        register(my.myObj_0100.Type);       // Big Bang
        register(my.myObj_0101.Type);       // Desktop 1: - Random pieces of the screen are shown at their own slightly offset locations
        register(my.myObj_0102.Type);       // Desktop 2: - Random shapes with a color from the underlying image (point-based or average)
        register(my.myObj_0103.Type);       // Desktop 3: - Random shapes with a color from the underlying image (point-based or average) -- uses custom shader
        register(my.myObj_0110.Type);       // Desktop 4: - Puts random colored shapes all over the screen
        register(my.myObj_0120.Type);       // Moving Lines (4 directions, straight lines or sin/cos curves)
        register(my.myObj_0130.Type);       // Growing shapes -- Rain circles alike -- no buffer clearing
        register(my.myObj_0131.Type);       // Growing shapes -- Rain circles alike
        register(my.myObj_0132.Type);       // Splines
        register(my.myObj_0140.Type);       // Grid with moving rectangle lenses -- TBD -- looks strange
        register(my.myObj_0150.Type);       // Conway's Life
        register(my.myObj_0160.Type);       // Desktop: Ever fading away pieces
        register(my.myObj_0170.Type);       // Desktop: Diminishing pieces
        register(my.myObj_0180.Type);       // Single generator of particle waves
        register(my.myObj_0181.Type);       // Multiple generators of particle waves
        register(my.myObj_0200.Type);       // Spiraling out shapes
        register(my.myObj_0210.Type);       // Another spiraling shapes -- see what's the difference
        register(my.myObj_0220.Type);       // Falling lines, Matrix-Style
        register(my.myObj_0230.Type);       // Gravity -- unfinished
        register(my.myObj_0300.Type);       // Small Explosions of Particles + Movement type Variations
        register(my.myObj_0310.Type);       // Moving particles, where each particle is connected with every other particle out there
        register(my.myObj_0320.Type);       // Spiralling doodles made of squares
        register(my.myObj_0330.Type);       // Textures, Take 1
        register(my.myObj_0340.Type);       // Grid of hexagons
        register(my.myObj_0350.Type);       // Moving groups of small particles. Particles within the group are connected to each other
        register(my.myObj_0360.Type);       // Moving particles; each particle is connected to 5 other random particles
        register(my.myObj_0370.Type);       // The image is split into big number of particles that fall down
        register(my.myObj_0380.Type);       // Rectangular shapes made of particles moving along the rectangle's outline
        register(my.myObj_0390.Type);       // Particles move radially from the off-center position, creating a vortex-like structure
        register(my.myObj_0400.Type);       // Circular texture stripes
        register(my.myObj_0410.Type);       // Concentric vibrating circles around randomly moving center point
        register(my.myObj_0420.Type);       // System, where the center attracts and repels all the particles at the same time. vary both forces
        register(my.myObj_0430.Type);       // Shooters move across the screen, shooting at each other
        register(my.myObj_0440.Type);       // Bouncing ball + lots of shapes rotating to always point towards it
        register(my.myObj_0450.Type);       // Get color from image and slightly offset this color. Then put the color spots on the screen
        register(my.myObj_0460.Type);       // Point cyclically moves on a spiral, constantly leaving a trail. Trail is made of particles that move outwards from the center OR in a point's opposite direction
        register(my.myObj_0470.Type);       // Angled rays
        register(my.myObj_0480.Type);       // Oscilloscope (running harmonics)
        register(my.myObj_0490.Type);       // F (x, y)
        register(my.myObj_0500.Type);       // Free Shader Experiments - 1
        register(my.myObj_0501.Type);       // Free Shader Experiments - 2
        register(my.myObj_0510.Type);       // Moving Shooters vs static Targets
        register(my.myObj_0520.Type);       // Static pulsating shapes
        register(my.myObj_0530.Type);       // A ring of moving particles
        register(my.myObj_0540.Type);       // Falling alphabet letters (Matrix style), ver1
        register(my.myObj_0541.Type);       // Falling alphabet letters (Matrix style), ver2
        register(my.myObj_0550.Type);       // Orbits of different size + a small planet is moving along each orbit
        register(my.myObj_0560.Type);       // Pixelating an image with average colors
        register(my.myObj_0570.Type);       // ...
        register(my.myObj_0580.Type);       // Gravity n-body
        register(my.myObj_0590.Type);       // Particle moves as a result of an average of n other particles movement
        register(my.myObj_0600.Type);       // Pendulum
        register(my.myObj_0610.Type);       // ...
        register(my.myObj_0620.Type);       // Rectangles with width/height that are changing constantly; while width is increasing, height is decreasing, and vice versa
        register(my.myObj_0630.Type);       // Rotating circles made of letters and symbols
        register(my.myObj_0640.Type);       // Create random rectangles, but put them on the screen only when they don't intersect any existing rectangles
        register(my.myObj_0641.Type);       // Create random circles, but put them on the screen only when they don't intersect any existing circles
        register(my.myObj_0650.Type);       // Drawing symbols using the color sampled from an image
        register(my.myObj_0660.Type);       // ...
        register(my.myObj_0670.Type);       // ...
        register(my.myObj_0680.Type);       // Scrolling wall of "text"
        register(my.myObj_0690.Type);       // Circularly moving particles with discrete curvature
        register(my.myObj_0691.Type);       // Pseudo 3d based off myObj_690
        register(my.myObj_0700.Type);       // Straight lines that reflect backwards
        register(my.myObj_0710.Type);       // Static growing shapes of the color of the underlying image
        register(my.myObj_0720.Type);       // Drop a random point, get its underlying color, then draw a horizontal or vertical line through this point
        register(my.myObj_0730.Type);       // Lots of triangles, where each vertice is moving like a bouncing ball
        register(my.myObj_0740.Type);       // Periodic vertical or horizontal waves of particles
        register(my.myObj_0750.Type);       // Particles generated with the same movement direction, bouncing off the walls
        register(my.myObj_0760.Type);       // Points randomly travelling over a graph
        register(my.myObj_0770.Type);       // Bubbles through mutually repellent particles
        register(my.myObj_0780.Type);       // Instanced shapes in a large quantity revealing an underlying image
        register(my.myObj_0790.Type);       // Two-point swaps
        register(my.myObj_0800.Type);       // Rows of Triangles
        register(my.myObj_0810.Type);       // Raster scan of an image
        register(my.myObj_0820.Type);       // ...
        register(my.myObj_0830.Type);       // Thin texture lines moving top to bottom of the screen
        register(my.myObj_0840.Type);       // Trains moving across the screen
        register(my.myObj_0850.Type);       // 3 rotating points per particle, making a rotating triangle
        register(my.myObj_0860.Type);       // ...
        register(my.myObj_0870.Type);       // Thin and long vertical or horizontal rectangles with low opacity and random color
        register(my.myObj_0880.Type);       // Perspective made of sine-cosine graphs
        register(my.myObj_0890.Type);       // Desktop pieces falling down in a matrix-style
        register(my.myObj_0900.Type);       // Waveforms moving sideways, v1
        register(my.myObj_0901.Type);       // Waveforms moving sideways, v2
        register(my.myObj_0910.Type);       // ...
        register(my.myObj_0920.Type);       // ...
        register(my.myObj_0930.Type);       // Stack of circle shapes moving with a delay
        register(my.myObj_0940.Type);       // Circular shapes made of instanced lines
        register(my.myObj_0950.Type);       // Very narrow window from a texture stretched to full-screen
        register(my.myObj_0960.Type);       // Pulsing grids
        register(my.myObj_0970.Type);       // ...
        register(my.myObj_0980.Type);       // Edge finding random algorythm
        register(my.myObj_0990.Type);       // Square/Rectangle Tunnel
        register(my.myObj_0992.Type);       // ...
        register(my.myObj_1000.Type);       // Bouncing particles inside grid cells
        register(my.myObj_1010.Type);       // Lightnings

        // несколько партиклов, которые в ограниченной терротории рандомно двигаются туда и сюда.
        // Они соединяются один раз при создании по цепочке, пока не соединятся по кругу.
        // Дальше они движутся, а соединения рисуются все время одни и те же

        register(my.myObj_9998.Type);      // Test rotating shape, unfinished yet good
        registerTests(register);            // Register additional testing objects

        _obj = my.myObj_Prioritizer.GetRandomObject(usePriority: true);

        return;
    }

    // -------------------------------------------------------------------------------------------------------------------

    private void registerTests(System.Action<System.Type> register)
    {
        //register(my.myObj_9999_test_001.Type);        // Trails test
        //register(my.myObj_9999_test_002.Type);
        //register(my.myObj_9999_test_002a.Type);
        //register(my.myObj_9999_test_002b.Type);
        //register(my.myObj_9999_test_003.Type);        // TextTex test

        register(my.myObj_9999_test_002c.Type);
        //register(my.myObj_999_test_004.Type);         // Instanced lines test
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
