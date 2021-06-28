using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MVCMoviesWithSSRS.Utility
{
    /// <summary>
    /// Type Converters that consider DBNull
    /// </summary>
    public class TypeConversion
    {
        public static string ToAString(object value)
        {
            if(value == null || object.ReferenceEquals(value, DBNull.Value))
            {
                return string.Empty;
            }
            else
            {
                return value.ToString();
            }
        }

        public static int ToInt32(object value)
        {
            return (value == DBNull.Value) ? default : Convert.ToInt32(value);
        }

        public static Int64 ToInt64(object value)
        {
            return (value == DBNull.Value) ? default : Convert.ToInt64(value);
        }

        public static DateTime ToDateTime(object value)
        {
            return (value == DBNull.Value) ? default : Convert.ToDateTime(value);
        }
    }
}
