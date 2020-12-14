using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;

namespace WangSql
{
    public class ResultMap
    {
        public object Deserializer<T>(IDataReader reader)
        {
            var type = TypeMap.GetStandardType(typeof(T));
            switch (type)
            {
                case StandardType.Dictionary:
                    return DeserializerDictionary<T>(reader);
                case StandardType.Simple:
                    return (T)DeserializerSimple(reader);
                case StandardType.Class:
                    return DeserializerClass<T>(reader);
            }

            return null;
        }







        

        private T DeserializerClass<T>(IDataReader reader)
        {
            var dict = DeserializerDictionary<Dictionary<string, object>>(reader);

            var entity = Activator.CreateInstance<T>();
            entity.GetType().GetProperties(BindingFlags.GetProperty | BindingFlags.Instance | BindingFlags.Public)
                .ToList().ForEach(op =>
                {
                    if (dict.Any(x => x.Key.ToUpper().Equals(op.Name.ToUpper())))
                    {
                        op.SetValue(
                            entity,
                            TypeMap.ConvertToType(dict.First(x => x.Key.ToUpper().Equals(op.Name.ToUpper())).Value, op.PropertyType),
                            null
                            );
                    }
                });

            return entity;
        }

        private T DeserializerDictionary<T>(IDataReader reader)
        {
            var dict = Activator.CreateInstance<T>() as IDictionary;
            for (var i = 0; i < reader.FieldCount; i++)
            {
                var k = reader.GetName(i);
                var v = GetReaderValue(reader, i);
                if (!dict.Contains(k))
                    dict.Add(k, v);
            }

            var t = (T)dict;
            return t;
        }

        private object DeserializerSimple(IDataReader reader)
        {
            var obj = GetReaderValue(reader, 0);
            return obj;
        }

        private object GetReaderValue(IDataReader reader, int index)
        {
            if (reader.FieldCount <= index)
            {
                return TypeMap.ConvertToType(null, typeof(object));
            }

            var type = reader.GetFieldType(index);
            var obj = reader.GetValue(index);
            return TypeMap.ConvertToType(obj, type);
        }
    }
}