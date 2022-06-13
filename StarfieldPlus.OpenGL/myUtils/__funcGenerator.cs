using System;



namespace myFuncGenerator
{
    public enum Targets { FIRST, SECOND };
    public enum Operations { EQUALS, PLUS, MINUS, MULT, DIV };
    public enum BasicFunctions { SIN, COS, SQRT, NONE };

    // ---------------------------------------------------------------------------------------------------------------

    public class myFunc1
    {
        public static Random rand = null;

        // ---------------------------------------------------------------------------------------------------------------

        // Gets 2 arguments;
        // Applies nArgsFunc to the arguments;
        // Uses the result as a parameter to a single main function;
        // Sometimes reverses the sign of the result, which leads to a symmetric resulting shape;
        // y [+-*/]= FUNC(args(x, y)) => y += Sin(x + y); y = Cos(x/y);
        public static void func_001(ref float f1, ref float f2, Targets target, Operations targetOp, uint argMode, BasicFunctions func, bool randomSign)
        {
            float arg = nArgsFunc(argMode, f1, f2);

            if (target == Targets.FIRST)
            {
                applyBasicFunc(ref f1, arg, func, targetOp);

                if (randomSign)
                {
                    f1 *= my.myUtils.randomSign(rand);
                }
            }
            else
            {
                applyBasicFunc(ref f2, arg, func, targetOp);

                if (randomSign)
                {
                    f2 *= my.myUtils.randomSign(rand);
                }
            }
        }

        // ---------------------------------------------------------------------------------------------------------------

        public static void func_001_1(ref float f1, ref float f2, Targets target, Operations targetOp, uint argMode1, BasicFunctions func, uint argMode2, uint argMode3, bool randomSign)
        {
            float res1 = f1, res2 = f1;

            func_001(ref res1, ref f2, Targets.FIRST, Operations.EQUALS, argMode1, func, false);
            func_001(ref res2, ref f2, Targets.FIRST, Operations.EQUALS, argMode2, BasicFunctions.NONE, false);

            float res = nArgsFunc(argMode3, res1, res2);

            if (target == Targets.FIRST)
            {
                applyBasicFunc(ref f1, res, BasicFunctions.NONE, targetOp);
            }
            else
            {
                applyBasicFunc(ref f2, res, BasicFunctions.NONE, targetOp);
            }
        }

        // ---------------------------------------------------------------------------------------------------------------

        public static void func_002(ref float f1, ref float f2, Targets target, Operations targetOp,
                                        BasicFunctions func1, uint argMode1,
                                        BasicFunctions func2, uint argMode2,
                                        uint argMode3)
        {
            float res1 = f1, res2 = f1;

            func_001(ref res1, ref f2, Targets.FIRST, Operations.EQUALS, argMode1, func1, false);
            func_001(ref res2, ref f2, Targets.FIRST, Operations.EQUALS, argMode2, func2, false);

            float res = nArgsFunc(argMode3, res1, res2);

            if (target == Targets.FIRST)
            {
                applyBasicFunc(ref f1, res, BasicFunctions.NONE, targetOp);
            }
            else
            {
                applyBasicFunc(ref f2, res, BasicFunctions.NONE, targetOp);
            }
        }

        // ---------------------------------------------------------------------------------------------------------------

        // Get number of modes in nArgsFunc
        public static int nArgsFuncNum()
        {
            return (int)nArgsFunc(9999999, 0, 0);
        }

        private static float nArgsFunc(uint mode, float f1, float f2)
        {
            switch (mode)
            {
                case 0:  return f1;
                case 1:  return Math.Abs(f1);
                case 2:  return f2;
                case 3:  return Math.Abs(f2);
                case 4:  return f1 + f2;
                case 5:  return f1 - f2;
                case 6:  return f2 - f1;
                case 7:  return f1 * f2;
                case 8:  return f1 / f2;
                case 9:  return f2 / f1;
                case 10: return (f1 + f2) / (f1 * f2);
                case 11: return (f1 * f2) / (f1 + f2);
                case 12: return (f1 + f2) * (f1 - f2);
                case 13: return f1 * f1 * f2 * f2;
                case 14: return 1.0f / (f1 + f2);
                case 15: return 1.0f / (f1 - f2);
                case 16: return f2 / (f1 + f2);
                case 17: return f2 / (f1 - f2);
                case 18: return f1 / f2 + f2 / f1;
                case 19: return f1 > f2 ? f1 / f2 : f2 / f1;
                case 20: return (f1 + f2) * 0.0001f;    // should be 't', but couldn't figure out how to pass it in here yet
                case 21: return (f1 * f2) > (f1 / f2) ? f1 : f2;
                case 22: return (f1 * f2) > (f1 / f2) ? f2 : f1;
                case 23: return (f1 * f2 * f1) * (f2 * f1 * f2);
                case 24: return (f1 * f1) > (f2 * f2) ? f2 / f1 : f1 / f2;
                case 25: return (f1 * f1) > (f2 * f2) ? f1 / f2 : f2 / f1;
                case 26: return (f1 * f1) > (f2 * f2) ? f1 : f2;
                case 27: return (f1 * f1) > (f2 * f2) ? f2 : f1;
                case 28: return (f1 * f1) > 0.5f ? 1.0f : -1.0f;
                case 29: return (f1 * f1) > 0.5f ? f1 + f2 : f1 - f2;
                case 30: return (f1 * f2) > 0.5f ? (float)Math.Sin(f1) : (float)Math.Sin(f2);
                case 31: return (f1 * f2) > 1.0f ? (float)Math.Sin(f1) : (float)Math.Sin(f2);
                case 32: return (f1 + f2) > 1.0f ? (float)Math.Sin(f1) : (float)Math.Cos(f2);
                case 33: return (float)Math.Sin(f1 + f2);
                case 34: return (float)Math.Sin(1.0f / (f1 + f2));

                // ========================================================

                case 999: return (f1 - 1.0f) * (f1 + 1.0f) - (f2 - 1.0f) * (f2 + 1.0f);
            }

            // Return the number of distinct modes
            return 35;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private static void applyBasicFunc(ref float f1, float f2, BasicFunctions func, Operations operation)
        {
            double res = 0;

            switch (func)
            {
                case BasicFunctions.SIN  : res = Math.Sin(f2);  break;
                case BasicFunctions.COS  : res = Math.Cos(f2);  break;
                case BasicFunctions.SQRT : res = Math.Sqrt(f2); break;
                                 default : res = f2;            break;
            }

            switch (operation)
            {
                case Operations.EQUALS  : f1  = (float)res; break;
                case Operations.PLUS    : f1 += (float)res; break;
                case Operations.MINUS   : f1 -= (float)res; break;
                case Operations.MULT    : f1 *= (float)res; break;
                case Operations.DIV     : f1 /= (float)res; break;
            }
        }

        // ---------------------------------------------------------------------------------------------------------------
    };
};

/*
    target = FIRST
    op = DIV
    f1 = COS
    f2 = SIN
    argmode1 = 3
    argmode2 = 9
    argmode3 = 0

    dy += (float)Math.Cos((Math.Abs(dy))) / (float)Math.Cos(dy*dx);* 
        target = SECOND
        targetOp = PLUS
        f1 = COS
        f2 = COS
        argmode1 = 7
        argmode2 = 3
        argmode3 = 9

    -- save this one
    target = SECOND
    targetOp = DIV
    f1 = SIN
    f2 = SQRT
    argmode1 = 1
    argmode2 = 1
    argmode3 = 0

    // heart shaped box
    //dy -= (float)(Math.Sqrt(Math.Abs(dx)));
        dXYgenerationMode = 0
        target = SECOND
        targetOp = MINUS
        f1 = SQRT
        argmode1 = 1

    // dy /= (float)(Math.Sqrt(Math.Abs(dx)));
    dXYgenerationMode = 0
    target = SECOND
    targetOp = DIV
    f1 = SQRT
    argmode1 = 1
*/
