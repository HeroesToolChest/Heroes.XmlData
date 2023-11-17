using System.ComponentModel;
using System.Reflection;

namespace Heroes.XmlData.Extensions;

internal static class EnumExtensions
{
    public static string GetDescription<T>(this T enumerationValue)
        where T : Enum
    {
        Type type = enumerationValue.GetType();
        if (!type.GetTypeInfo().IsEnum)
        {
            throw new ArgumentException("Must be of Enum type", nameof(enumerationValue));
        }

        // Tries to find a DescriptionAttribute for a potential friendly name for the enum
        string enumString = enumerationValue.ToString() ?? throw new ArgumentException("Cannot be null", nameof(enumerationValue));

        MemberInfo[] memberInfo = type.GetMember(enumString);
        if (memberInfo != null && memberInfo.Length > 0)
        {
            object[] attributes = memberInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);

            if (attributes != null && attributes.Length > 0)
            {
                return ((DescriptionAttribute)attributes[0]).Description;
            }
        }

        return enumString;
    }
}
