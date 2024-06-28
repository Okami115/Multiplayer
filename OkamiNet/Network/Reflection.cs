using OkamiNet.Utils;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace OkamiNet.Network
{
    public static class Reflection
    {
        public static List<INetObj> netObjets = new List<INetObj>();

        public static void Inspect(Type type, object obj)
        {
            if (obj != null)
            {
                UtilsTools.LOG("Exist");
                foreach (FieldInfo info in type.GetFields(
                    BindingFlags.NonPublic |
                    BindingFlags.Public |
                    BindingFlags.Instance | BindingFlags.DeclaredOnly))
                {
                    UtilsTools.LOG("Can read");
                    ReadValue(info, obj);
                }

                if (type.BaseType != null)
                {
                    UtilsTools.LOG("not null");
                    Inspect(type.BaseType, obj);
                }
            }
        }

        public static void ReadValue(FieldInfo info, object obj)
        {
            if (info.FieldType.IsValueType || info.FieldType == typeof(string) || info.FieldType.IsEnum)
            {
                UtilsTools.LOG(info.Name + ": " + info.GetValue(obj));
            }
            else if (typeof(System.Collections.ICollection).IsAssignableFrom(info.FieldType))
            {
                UtilsTools.LOG("Insert Collection");
                foreach (object item in (info.GetValue(obj) as System.Collections.ICollection))
                {
                    Inspect(item.GetType(), item);
                }
            }
            else
            {
                UtilsTools.LOG("Insert");
                Inspect(info.FieldType, info.GetValue(obj));
            }
        }

        public static void UpdateNetObjts()
        {
            for (int i = 0; i < netObjets.Count; i++)
            {
                try
                {
                    UtilsTools.LOG("Obj : " + i);
                    Inspect(typeof(NetValue), netObjets[i]);
                }
                catch
                {
                    UtilsTools.LOG("¡¡ERROR!!");
                }
            }
        }
    }
}