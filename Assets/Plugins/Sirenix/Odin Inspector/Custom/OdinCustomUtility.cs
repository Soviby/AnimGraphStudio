using System;
using System.Reflection;
using UnityEngine;

namespace Sirenix.OdinInspector.Custom
{
    public static class OdinCustomUtility
    {
        public static dynamic GetEnumMeta(Type enumType, string enumName)
        {
            var attributes = Attribute.GetCustomAttributes(enumType.GetMember(enumName)[0]);
            if (attributes == null)
                return null;
            return attributes != null ? attributes[0] : null;
        }

        public static string GetEnumMetaName(Type enumType, string enumName)
        {
            object meta = GetEnumMeta(enumType, enumName);
            if (meta == null)
                return "";    
            Type type = meta.GetType();
            var field = type.GetField("name");
            if (field == null)
                return "";        
            string name = (string)field.GetValue(meta);
            return name ?? "";
        }

        public static string GetEnumTextName(Type enumType, string enumName)
        {
            var metaName = GetEnumMetaName(enumType, enumName);
            if (string.IsNullOrEmpty(metaName))
                return enumName;
            return metaName;
        }
    }
}