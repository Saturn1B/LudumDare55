using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public static class FirestoreSerializer
{
    // ROOT SERIALIZATION (for documents)
    public static Dictionary<string, object> SerializeRoot(object obj)
    {
        return SerializeObject(obj);
    }

    public static object SerializeField(object obj)
    {
        return SerializeValue(obj);
    }

    // =====================================

    private static object SerializeValue(object obj)
    {
        if (obj == null)
            return new Dictionary<string, object> { { "nullValue", null } };

        Type type = obj.GetType();

        if (type == typeof(string))
            return new Dictionary<string, object> { { "stringValue", obj } };

        if (type == typeof(int))
            return new Dictionary<string, object> { { "integerValue", obj.ToString() } };

        if (type == typeof(float) || type == typeof(double))
            return new Dictionary<string, object> { { "doubleValue", Convert.ToDouble(obj) } };

        if (type == typeof(bool))
            return new Dictionary<string, object> { { "booleanValue", obj } };

        if (type == typeof(DateTime))
            return new Dictionary<string, object>
            {
                { "timestampValue", ((DateTime)obj).ToString("o") }
            };

        if (type == typeof(Vector3))
        {
            Vector3 v = (Vector3)obj;

            return new Dictionary<string, object>
            {
                {
                    "mapValue",
                    new Dictionary<string, object>
                    {
                        {
                            "fields",
                            new Dictionary<string, object>
                            {
                                { "x", new Dictionary<string, object>{{"doubleValue", v.x }}},
                                { "y", new Dictionary<string, object>{{"doubleValue", v.y }}},
                                { "z", new Dictionary<string, object>{{"doubleValue", v.z }}}
                            }
                        }
                    }
                }
            };
        }

        if (typeof(IEnumerable).IsAssignableFrom(type) && type != typeof(string))
        {
            var values = new List<object>();

            foreach (var item in (IEnumerable)obj)
                values.Add(SerializeValue(item));

            return new Dictionary<string, object>
            {
                {
                    "arrayValue",
                    new Dictionary<string, object>
                    {
                        { "values", values }
                    }
                }
            };
        }

        // CLASS / STRUCT
        return new Dictionary<string, object>
        {
            {
                "mapValue",
                new Dictionary<string, object>
                {
                    { "fields", SerializeObject(obj) }
                }
            }
        };
    }

    private static Dictionary<string, object> SerializeObject(object obj)
    {
        var fields = new Dictionary<string, object>();

        var fieldInfos = obj.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);

        foreach (var field in fieldInfos)
        {
            object value = field.GetValue(obj);
            fields[field.Name] = SerializeValue(value);
        }

        return fields;
    }
}
