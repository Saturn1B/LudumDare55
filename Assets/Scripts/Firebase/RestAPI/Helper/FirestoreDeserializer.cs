using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

public static class FirestoreDeserializer
{
    public static T DeserializeDocument<T>(JObject document) where T : new()
    {
        JObject fields = (JObject)document["fields"];

        var instance = new T();
        DeserializeInto(instance, fields);

        return instance;
    }

    private static void DeserializeInto(object obj, JObject fields)
    {
        var type = obj.GetType();

        foreach (var fieldInfo in type.GetFields())
        {
            if (!fields.ContainsKey(fieldInfo.Name))
                continue;

            var valueToken = (JObject)fields[fieldInfo.Name];
            object value = DeserializeValue(valueToken, fieldInfo.FieldType);

            fieldInfo.SetValue(obj, value);
        }
    }

    private static object DeserializeValue(JObject token, Type targetType)
    {
        if (token["stringValue"] != null)
            return token["stringValue"].ToString();

        if (token["integerValue"] != null)
            return Convert.ToInt32(token["integerValue"].ToString());

        if (token["doubleValue"] != null)
            return Convert.ToSingle(token["doubleValue"].ToString());

        if (token["booleanValue"] != null)
            return token["booleanValue"].ToObject<bool>();

        if (token["timestampValue"] != null)
            return token["timestampValue"].ToString();

        if (token["mapValue"] != null)
        {
            if (targetType == typeof(Vector3))
            {
                var fields = (JObject)token["mapValue"]["fields"];

                return new Vector3(
                    Convert.ToSingle(fields["x"]["doubleValue"].ToString()),
                    Convert.ToSingle(fields["y"]["doubleValue"].ToString()),
                    Convert.ToSingle(fields["z"]["doubleValue"].ToString())
                );
            }

            var instance = System.Activator.CreateInstance(targetType);
            DeserializeInto(instance, (JObject)token["mapValue"]["fields"]);
            return instance;
        }

        if (token["arrayValue"] != null)
        {
            var values = token["arrayValue"]["values"] as JArray;

            if (values == null)
                return null;

            Type elementType;

            if (targetType.IsArray)
                elementType = targetType.GetElementType();
            else
                elementType = targetType.GetGenericArguments()[0];

            var list = (IList)System.Activator.CreateInstance(typeof(List<>).MakeGenericType(elementType));

            foreach (var item in values)
            {
                list.Add(DeserializeValue((JObject)item, elementType));
            }

            if (targetType.IsArray)
            {
                var array = Array.CreateInstance(elementType, list.Count);
                list.CopyTo(array, 0);
                return array;
            }

            return list;
        }

        return null;
    }
}
