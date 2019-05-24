using System;

namespace Green
{
    public static class BaseLibrary
    {
        public static object Add(object[] args)
        {
            Int64 result = 0;
            foreach (var arg in args)
            {
                switch (arg)
                {
                    case Int64 intValue:
                        result += intValue;
                        break;
                    case UInt64 intValue:
                        result += (Int64)intValue;
                        break;
                    case Int32 intValue:
                        result += intValue;
                        break;
                    case UInt32 intValue:
                        result += intValue;
                        break;
                    default:
                        throw new RuntimeException($"+: bad argument {arg}");
                }
            }
            return result;
        }
    }
}
