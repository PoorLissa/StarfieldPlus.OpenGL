using System;
using System.Reflection;

/*
    How to instantiate class object knowing its Type:

    1. var dic = new System.Collections.Generic.Dictionary<int, System.Type>();

    2. var item = dic[2];

    3. Type obj = (Type)System.Activator.CreateInstance(item);
*/

namespace my
{
    public class myObj_Prioritizer
    {
        private static int _totalPriority = 0;
        private static System.Collections.Generic.Dictionary<int, Type> _dic = null;

        // ---------------------------------------------------------------------------------------------------------------

        // Register class
        public static void RegisterClass(Type t)
        {
            int priority = 0;

            if (_dic == null)
            {
                _dic = new System.Collections.Generic.Dictionary<int, Type>();
                _totalPriority = 0;
            }

            priority = getProperty("Priority", t);
            _totalPriority += priority;

            if (priority > 0)
            {
                _dic.Add(_totalPriority, t);
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        // Return random object from the pool of registered classes, adjusted for each Type's priority
        public static my.myObject GetRandomObject(bool doUsePriority, bool doClearDictionary, bool doUseThisType, Type t)
        {
            Type typeToReturn;

            if (doUseThisType && t != null)
            {
                typeToReturn = t;
            }
            else
            {
                int objId = 0;
                var rand = new Random(Guid.NewGuid().GetHashCode());

                if (doUsePriority)
                {
                    // Add one, as there will be no dictionary entry with id = 0
                    objId = rand.Next(_totalPriority) + 1;

                    while (!_dic.ContainsKey(objId))
                        objId++;
                }
                else
                {
                    // No priority: get an item by its index
                    objId = rand.Next(_dic.Count);

                    foreach (var obj in _dic)
                    {
                        if (objId-- == 0)
                        {
                            objId = obj.Key;
                            break;
                        }
                    }
                }

                typeToReturn = _dic[objId];
            }

            // Clear dictionary, as we won't be needing it again in this session
            if (doClearDictionary)
            {
                _dic.Clear();
            }

            // Instantiate object of the selected type and return it
            return (my.myObject)System.Activator.CreateInstance(typeToReturn);
        }

        // ---------------------------------------------------------------------------------------------------------------

        // This function takes in a name of a static property and a type;
        // It extracts and returns the value of this property
        private static int getProperty(string name, System.Type t)
        {
            return (int)t.GetProperty(name, BindingFlags.Public | BindingFlags.Static).GetValue(null);
        }

        // ---------------------------------------------------------------------------------------------------------------
    };

};
