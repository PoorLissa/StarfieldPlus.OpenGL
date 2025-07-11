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


using my;

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
    // - gravity, where the color of a pixel is its mass
    // - sort all the screen pixels
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
    // - try bezier curves:             https://en.wikipedia.org/wiki/B%C3%A9zier_curve
    // - try neural cellular automata:  https://www.youtube.com/watch?v=3H79ZcBuw4M&ab_channel=EmergentGarden
    // - try Delaunay triangulation:    https://en.wikipedia.org/wiki/Delaunay_triangulation
    // - try all of these:              https://www.youtube.com/watch?v=R5jIoLnL_nE&ab_channel=JosuRelax
    // - rand rects with the (avg) color of the underlying image; put larger pieces of real texture on a rare occasion
    // - several shapes at the same coordinates, but with different rotating angle (using alternative rotate mode)
    // - sort of a brick breaker game, without a paddle (just bouncing ball)
    // - snake that does not cross itself (use lists of vertical and horizontal lines the snake consists of)
    // - a whole screen graph, where all the connections are not straight lines, but lightning-like sectional curves that change over time
    // - like 720, but every line is crossed by lots of short lines of the same color at 90 degrees
    // - Hexagon grid with a pseudo 3d effect
    // - Beam of particles directed at a black hole (or just massive) object, and is able to escape its gravitation, but is scattered around
    // - A wave of hexagons that moves across the screen, and where the new hex appears, it stars growing until all the screen is covered in solid color

    // https://en.wikipedia.org/wiki/Langton%27s_ant

    // -------------------------------------------------------------------------------------------------------------------

    public void selectObject()
    {
        // Objects to register
        System.Type[] types = new System.Type[]
        {
            my.myObj_0000.Type,     // Star Field
            my.myObj_0010.Type,     // Randomly Roaming Squares (Snow Like)
            my.myObj_0011.Type,     // Randomly Roaming Lines (based on Randomly Roaming Squares)
            my.myObj_0012.Type,     // Particles with real trails
            my.myObj_0013.Type,     // Particles with real trails again
            my.myObj_0014.Type,     // Particles with real trails again again
            my.myObj_0015.Type,     // Snow-like pattern made of different layers moving in different directions
            my.myObj_0020.Type,     // Linearly Moving Shapes (Soap Bubbles Alike)
            my.myObj_0021.Type,     // Ever Growing Shapes located at the center of the screen + small offset
            my.myObj_0030.Type,     // Rain Drops (Vertical, Top-Down)
            my.myObj_0031.Type,     // Pseudo-3d-rain
            my.myObj_0040.Type,     // Lines 1: Snake-like branches moving outwards from the center
            my.myObj_0041.Type,     // Lines 2: Branches/snakes moving inwards/outwards with different set of rules
            my.myObj_0042.Type,     // Lines 3: Patchwork / Micro Schematics
            my.myObj_0043.Type,     // Various shapes growing out from a single starting point
            my.myObj_0050.Type,     // Desktop pieces get swapped
            my.myObj_0070.Type,     // Pieces falling off the desktop, ver1
            my.myObj_0071.Type,     // Pieces falling off the desktop, ver2
            my.myObj_0072.Type,     // Desktop pieces move from the offscreen position into their original positions
            my.myObj_0100.Type,     // Big Bang
            my.myObj_0101.Type,     // Desktop 1: - Random pieces of the screen are shown at their own slightly offset locations
            my.myObj_0102.Type,     // Desktop 2: - Random shapes with a color from the underlying image (point-based or average)
            my.myObj_0103.Type,     // Desktop 3: - Random shapes with a color from the underlying image (point-based or average) -- uses custom shader
            my.myObj_0110.Type,     // Desktop 4: - Puts random colored shapes all over the screen
            my.myObj_0120.Type,     // Moving Lines (4 directions, straight lines or sin/cos curves)
            my.myObj_0130.Type,     // Growing shapes -- Rain circles alike -- no buffer clearing
            my.myObj_0131.Type,     // Growing shapes -- Rain circles alike
            my.myObj_0132.Type,     // Splines
            my.myObj_0140.Type,     // A random image viewer which displays 2 different images at different opacity
            my.myObj_0150.Type,     // Conway's Life
            my.myObj_0160.Type,     // Desktop: Ever fading away pieces
            my.myObj_0170.Type,     // Desktop: Diminishing pieces
            my.myObj_0180.Type,     // Single generator of particle waves
            my.myObj_0181.Type,     // Multiple generators of particle waves
            my.myObj_0200.Type,     // Spiraling out shapes
            my.myObj_0210.Type,     // Another spiraling shapes -- see what's the difference
            my.myObj_0220.Type,     // Falling lines, Matrix-Style
            my.myObj_0230.Type,     // Gravity 1 -- unfinished
            my.myObj_0231.Type,     // Gravity 2 -- unfinished
            my.myObj_0300.Type,     // Small Explosions of Particles + Movement type Variations
            my.myObj_0310.Type,     // Moving particles, where each particle is connected with every other particle out there
            my.myObj_0320.Type,     // Spiralling doodles made of squares
            my.myObj_0330.Type,     // Textures, Take 1
            my.myObj_0340.Type,     // Grid of hexagons
            my.myObj_0341.Type,     // Grid of rhombuses
            my.myObj_0350.Type,     // Moving groups of small particles. Particles within the group are connected to each other
            my.myObj_0360.Type,     // Moving particles; each particle is connected to 5 other random particles
            my.myObj_0370.Type,     // The image is split into big number of particles that fall down
            my.myObj_0380.Type,     // Rectangular shapes made of particles moving along the rectangle's outline
            my.myObj_0390.Type,     // Particles move radially from the off-center position, creating a vortex-like structure
            my.myObj_0400.Type,     // Circular texture stripes
            my.myObj_0410.Type,     // Concentric vibrating circles around randomly moving center point
            my.myObj_0420.Type,     // System, where the center attracts and repels all the particles at the same time. vary both forces
            my.myObj_0430.Type,     // Shooters move across the screen, shooting at each other
            my.myObj_0440.Type,     // Bouncing ball + lots of shapes rotating to always point towards it
            my.myObj_0450.Type,     // Get color from image and slightly offset this color. Then put the color spots on the screen
            my.myObj_0460.Type,     // Point cyclically moves on a spiral, constantly leaving a trail. Trail is made of particles that move outwards from the center OR in a point's opposite direction
            my.myObj_0470.Type,     // Angled rays
            my.myObj_0480.Type,     // Oscilloscope (running harmonics)
            my.myObj_0490.Type,     // F (x, y)
            my.myObj_0500.Type,     // Free Shader Experiments - 1
            my.myObj_0501.Type,     // Free Shader Experiments - 2
            my.myObj_0510.Type,     // Moving Shooters vs static Targets
            my.myObj_0520.Type,     // Static pulsating shapes
            my.myObj_0530.Type,     // A ring of moving particles
            my.myObj_0540.Type,     // Falling alphabet letters (Matrix style), ver1
            my.myObj_0541.Type,     // Falling alphabet letters (Matrix style), ver2
            my.myObj_0550.Type,     // Orbits of different size + a small planet is moving along each orbit
            my.myObj_0560.Type,     // Pixelating an image with average colors
            my.myObj_0570.Type,     // Some spots using the color of an image. Linear connections between these shapes
            my.myObj_0580.Type,     // Gravity n-body
            my.myObj_0590.Type,     // Particle moves as a result of an average of n other particles' motions
            my.myObj_0600.Type,     // Pendulum
            my.myObj_0610.Type,     // Snake-like patterns, stupid implementation
            my.myObj_0620.Type,     // Rectangles with width/height that are changing constantly; while width is increasing, height is decreasing, and vice versa
            my.myObj_0630.Type,     // Rotating circles made of letters and symbols
            my.myObj_0640.Type,     // Create random rectangles, but put them on the screen only when they don't intersect any existing rectangles
            my.myObj_0641.Type,     // Create random circles, but put them on the screen only when they don't intersect any existing circles
            my.myObj_0650.Type,     // Drawing characters using the color sampled from an image
            my.myObj_0660.Type,     // Particle waves originating from the center. Particles in every wave are interconnected
            my.myObj_0670.Type,     // Nested rectangles. Lots of smaller particles are bouncing off the rectangles' edges
            my.myObj_0680.Type,     // Scrolling wall of pseudo text
            my.myObj_0690.Type,     // Circularly moving particles with discrete curvature
            my.myObj_0691.Type,     // Pseudo 3d based off myObj_690
            my.myObj_0700.Type,     // Straight lines that reflect backwards
            my.myObj_0710.Type,     // Static growing shapes of the color of the underlying image
            my.myObj_0720.Type,     // Drop a random point, get its underlying color, then draw a horizontal or vertical line through this point
            my.myObj_0730.Type,     // Lots of triangles, where each vertice is moving like a bouncing ball
            my.myObj_0740.Type,     // Periodic vertical or horizontal waves of particles
            my.myObj_0750.Type,     // Particles generated with the same movement direction, bouncing off the walls
            my.myObj_0760.Type,     // Points randomly travelling over a graph
            my.myObj_0770.Type,     // Bubbles through mutually repellent particles
            my.myObj_0780.Type,     // Instanced shapes in a large quantity revealing an underlying image
            my.myObj_0790.Type,     // Two-point swaps
            my.myObj_0800.Type,     // Rows of Triangles
            my.myObj_0810.Type,     // Raster scan of an image
            my.myObj_0820.Type,     // Spirally rotating squares
            my.myObj_0830.Type,     // Thin texture lines moving top to bottom of the screen
            my.myObj_0840.Type,     // Trains moving across the screen
            my.myObj_0850.Type,     // 3 rotating points per particle, making a rotating triangle
            my.myObj_0860.Type,     // Full screen shader
            my.myObj_0870.Type,     // Thin and long vertical or horizontal rectangles with low opacity and random color
            my.myObj_0880.Type,     // Perspective made of sine-cosine graphs
            my.myObj_0890.Type,     // Desktop pieces falling down in a matrix-style
            my.myObj_0900.Type,     // Waveforms moving sideways, v1
            my.myObj_0901.Type,     // Waveforms moving sideways, v2
            my.myObj_0910.Type,     // Radially moving multiple particles, with an underlying image
            my.myObj_0920.Type,     // Radially moving multiple particles with a color shift
            my.myObj_0930.Type,     // Stack of circle shapes moving with a delay
            my.myObj_0940.Type,     // Circular shapes made of instanced lines
            my.myObj_0950.Type,     // Very narrow window from a texture stretched to full-screen
            my.myObj_0960.Type,     // Pulsing grids
            my.myObj_0970.Type,     // Static rectangles drawn inside each other with a color shift
            my.myObj_0980.Type,     // Edge finding random algorythm
            my.myObj_0990.Type,     // Square/Rectangle Tunnel
            my.myObj_0992.Type,     // Slowly growing shapes, originating in or near to the center
            my.myObj_0993.Type,     // Slowly growing shapes (similar to myObj_0992), originating from multiple generators
            my.myObj_1000.Type,     // Bouncing particles inside grid cells
            my.myObj_1010.Type,     // Lightnings
            my.myObj_1020.Type,     // Multiple particles moving along circular trajectories
            my.myObj_1021.Type,     // The same as 1020, but on a offscreen texture, thus no traces are left
            my.myObj_1030.Type,     // Lightnings, take 2
            my.myObj_1040.Type,     // Tiled image transitioning to another image over time
            my.myObj_1050.Type,     // Roaming lines, no buffer clearing
            my.myObj_1060.Type,     // Draw texture's pixels only if their color is close to a target color
            my.myObj_1070.Type,     // Simplified gravity -- lots of light objects vs a few massive ones
            my.myObj_1080.Type,     // Pseudo 3d: rotating 'tube'
            my.myObj_1090.Type,     // Like Starfield, but instead of flying dots we have flying lines (made of 2 dots with the same angle, but slightly different speed)
            my.myObj_1100.Type,     // ...
            my.myObj_1110.Type,     // Progress bars
            my.myObj_1120.Type,     // Black hole
            my.myObj_1130.Type,     // Filling the screen with lines of blocks with constantly diminishing size
            my.myObj_1140.Type,     // Spiraling particles with long tails
            my.myObj_1150.Type,     // ...
            my.myObj_1160.Type,     // Partial hexagon grid revealed in the vicinity of a moving particle
            my.myObj_1170.Type,     // Scrolling texture images
            my.myObj_1180.Type,     // ...
            my.myObj_1190.Type,     // Generators expand and spawn particles along their circumference
            my.myObj_1200.Type,     // Pseudo 3d 'tooth like' pyramids
            my.myObj_1210.Type,     // Rotating lines aligned to grid
            my.myObj_1220.Type,     // Centers of rotation attached to a grid
            my.myObj_1230.Type,     // ...
            my.myObj_1240.Type,     // Several generators that produce concentric circles of small opacity
            my.myObj_1250.Type,     // ...
            my.myObj_1260.Type,     // ...
            my.myObj_1270.Type,     // Straight lines symmetrically originating from starting points
            my.myObj_1280.Type,     // Doom style prysms
            my.myObj_1290.Type,     // Prysms
            my.myObj_1300.Type,     // ...
            my.myObj_1310.Type,     // ...
            my.myObj_1320.Type,     // Falling squares followed by a trail of smaller squares of darker color
            my.myObj_1330.Type,     // ...
            my.myObj_1340.Type,     // ...
            my.myObj_1350.Type,     // ...
            my.myObj_1360.Type,     // Semispheres growing into the screen space from the screen borders
            my.myObj_1370.Type,     // ...
            my.myObj_1380.Type,     // ...
            my.myObj_1390.Type,     // ...
            my.myObj_1400.Type,     // ...
            my.myObj_1410.Type,     // ...
            my.myObj_1420.Type,     // Depth focus test 1
            my.myObj_1430.Type,     // Depth focus test 2
            my.myObj_1440.Type,     // Depth focus test 3
            my.myObj_1450.Type,     // Depth focus test: falling 'snow'
            my.myObj_1460.Type,     // Depth focus test: falling debris
            my.myObj_1470.Type,     // Procedural animation
            my.myObj_1480.Type,     // Full screen texture shader
            my.myObj_1490.Type,     // ...

            my.myObj_9999_test_002c.Type,
            my.myObj_9998.Type,     // Test rotating shape, unfinished yet good
        };

        foreach (var t in types)
        {
            my.myObj_Prioritizer.RegisterClass(t);
        }

        // Randomly pick one single object
        _obj = my.myObj_Prioritizer.GetRandomObject(doUsePriority   : true,
                                                    doUseCustomType : true, my.myObj_1490.Type);
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
