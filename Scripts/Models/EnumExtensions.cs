using Godot;
using System;
using System.ComponentModel;
using System.Linq;

namespace Orcinus.Scripts.Models
{
    public static class EnumExtensions
    {
        public static T[] GetEnumValues<T>() where T : struct, IConvertible, IComparable, IFormattable
        {
            return Enum.GetValues(typeof(T)).Cast<T>().ToArray();
        }

        public static T GetMaxEnum<T>() where T : struct, IConvertible, IComparable, IFormattable
        {
            return Enum.GetValues(typeof(T)).Cast<T>().Max();
        }

        public static int ToInt<T>(T enumeration) where T : struct, IConvertible, IComparable, IFormattable
        {
            return Convert.ToInt32(enumeration);
        }

        public static T GetRandomValue<T>(RandomNumberGenerator rng) where T : struct, IConvertible, IComparable, IFormattable
        {
            var enums = Enum.GetValues(typeof(T)).Cast<T>().ToArray();
            return enums[rng.RandiRange(0, enums.Length - 1)];
        }

        public static T GetAttribute<T>(this Enum enumeration) where T : Attribute
        {
            object[] attr = enumeration.GetType().GetField(enumeration.ToString())
                .GetCustomAttributes(typeof(T), false);

            var description = attr.Length > 0 ? ((T)attr[0]) : null;
            return description;
        }

        public static string GetDescription(this Enum enumeration)
        {
            return enumeration.GetAttribute<DescriptionAttribute>().Description;
        }
    }
}