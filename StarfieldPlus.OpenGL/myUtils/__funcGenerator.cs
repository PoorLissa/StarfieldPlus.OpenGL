using System;



namespace myFuncGenerator0
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




namespace myFuncGenerator1
{
    public enum Funcs { SIN, COS, SQRT, EXP, NONE };
    public enum eqModes { EQUALS, PLUS, MINUS, MULT, DIV };
    public enum Targets { FIRST, SECOND };

    // ---------------------------------------------------------------------------------------------------------------

    public class myFuncs
    {
        public static int iFunc(Funcs f, float arg)
        {
            switch (f)
            {
                case Funcs.SIN : return (int)Math.Sin(arg);
                case Funcs.COS : return (int)Math.Cos(arg);
                case Funcs.SQRT: return (int)Math.Sqrt(arg);
                case Funcs.EXP : return (int)Math.Exp(arg);         // not used yet
            }

            return 3;
        }

        public static float fFunc(Funcs f, float arg)
        {
            switch (f)
            {
                case Funcs.SIN : return (float)Math.Sin(arg);
                case Funcs.COS : return (float)Math.Cos(arg);
                case Funcs.SQRT: return (float)Math.Sqrt(arg);
                case Funcs.EXP : return (float)Math.Exp(arg);       // not used yet
            }

            return 3;
        }

    };

    // ---------------------------------------------------------------------------------------------------------------

    public class myArgs
    {
        // Calculates some predefined function of 2 arguments;
        // With [argMode == -1], returns the number of implemented functions
        public static float argsFunc(int argMode, float x, float y)
        {
            switch (argMode)
            {
                case 0 : return x;
                case 1 : return Math.Abs(x);
                case 2 : return y;
                case 3 : return Math.Abs(y);
                case 4 : return x + y;
                case 5 : return x - y;
                case 6 : return y - x;
                case 7 : return x * y;
                case 8 : return x / y;
                case 9 : return y / x;
                case 10: return (x + y) / (x * y);
                case 11: return (x * y) / (x + y);
                case 12: return (x + y) * (x - y);
                case 13: return x * x * y * y;
                case 14: return 1.0f / (x + y);
                case 15: return 1.0f / (x - y);
                case 16: return y / (x + y);
                case 17: return y / (x - y);
                case 18: return x / y + y / x;
                case 19: return x > y ? x / y : y / x;
                case 20: return (x + y) * 0.0001f;    // should be 't', but couldn't figure out how to pass it in here yet
                case 21: return (x * y) > (x / y) ? x : y;
                case 22: return (x * y) > (x / y) ? y : x;
                case 23: return (x * y * x) * (y * x * y);
                case 24: return (x * x) > (y * y) ? y / x : x / y;
                case 25: return (x * x) > (y * y) ? x / y : y / x;
                case 26: return (x * x) > (y * y) ? x : y;
                case 27: return (x * x) > (y * y) ? y : x;
                case 28: return (x * x) > 0.5f ? 1.0f : -1.0f;
                case 29: return (x * x) > 0.5f ? x + y : x - y;
                case 30: return (x * y) > 0.5f ? (float)Math.Sin(x) : (float)Math.Sin(y);
                case 31: return (x * y) > 1.0f ? (float)Math.Sin(x) : (float)Math.Sin(y);
                case 32: return (x + y) > 1.0f ? (float)Math.Sin(x) : (float)Math.Cos(y);
                case 33: return (float)Math.Sin(x + y);
                case 34: return (float)Math.Sin(1.0f / (x + y));

                // ========================================================

                case 999: return (x - 1.0f) * (x + 1.0f) - (y - 1.0f) * (y + 1.0f);
            }

            return 35;
        }
    };

    // ---------------------------------------------------------------------------------------------------------------

    public class myExpr
    {
        public static Random rand = null;

        public static void expr(ref float val, eqModes eqMode, bool randSign, float arg)
        {
            switch (eqMode)
            {
                case eqModes.EQUALS : val  = arg; break;
                case eqModes.PLUS   : val += arg; break;
                case eqModes.MINUS  : val -= arg; break;
                case eqModes.MULT   : val *= arg; break;
                case eqModes.DIV    : val /= arg; break;
            }

            if (randSign)
            {
                val *= my.myUtils.randomSign(rand);
            }
        }

        public static void expr(ref float f1, ref float f2, Targets t, eqModes eqMode, bool randSign, float arg)
        {
            switch (t)
            {
                case Targets.FIRST:
                    expr(ref f1, eqMode, randSign, arg);
                    break;

                case Targets.SECOND:
                    expr(ref f2, eqMode, randSign, arg);
                    break;
            }
        }
    };

    // ---------------------------------------------------------------------------------------------------------------
};
