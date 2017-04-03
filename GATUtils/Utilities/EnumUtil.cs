using System;
using System.Collections.Generic;
using System.Linq;

namespace GATUtils.Utilities
{
    public class EnumUtil
    {
        public static List<string> GetStringListFromEnum(Type targetEnum)
        {
            Dictionary<string, Int32> preliminaryDictionary = _ConvertEnumToDictionary(targetEnum);

            return preliminaryDictionary.Keys.ToList();
        }

        public static List<string> GetStringListFromEnumList<T> (List<T> enumList)
        {
            List<string> convertedList = enumList.Select(enumValue => Enum.GetName(typeof (T), enumValue)).ToList();

            return new List<string>();
        }

        public static int GetEnumFromString(Type enumType, string stringName)
        {
            return (int)Enum.Parse(enumType, stringName);
        }

        public static string GetEnumStringFromInteger(Type enumType, int enumValue)
        {
            if (enumType.BaseType != typeof(Enum))
            {
                throw new InvalidCastException(string.Format("The submitted type is not an Enum ({0})", enumType));
            }

            return Enum.GetName(enumType, enumValue);
        }

        public static string GetEnumString<T>(T enumValue)
        {
            if (typeof(T).BaseType != typeof(Enum))
            {
                throw new InvalidCastException(string.Format("The submitted type is not an Enum ({0})", typeof(T)));
            }

            return Enum.GetName(typeof(T), enumValue);
        }

        public static void GetEnumFromString<T>(string enumStringValue, out T value)
        {
            List<string> enumValues = GetStringListFromEnum(typeof (T));

            int enumIndex = enumValues.IndexOf(enumStringValue);
            Array values = Enum.GetValues(typeof (T));
            value = (T)values.GetValue(enumIndex);
        }

        private static Dictionary<string, int> _ConvertEnumToDictionary(Type enumType)
        {
            if (enumType.BaseType != typeof(Enum))
            {
                throw new InvalidCastException(string.Format("The submitted type is not an Enum ({0})", enumType));
            }

            return Enum.GetValues(enumType).Cast<int>().ToDictionary(currentItem => Enum.GetName(enumType, currentItem));
        }
    }
}
