using System;
using System.Linq;

namespace Api.Models
{
    public static class ObjectExtensions
    {
        private static Type[] allowTypes = new Type[] { typeof(byte?), typeof(byte), typeof(short?), typeof(short), typeof(int?), typeof(int), typeof(float?), typeof(float), typeof(decimal?), typeof(decimal), typeof(double?), typeof(double), typeof(bool?), typeof(bool), typeof(DateTime), typeof(DateTime?), typeof(string) };

        public static void SetNullObjectChildren(this object obj)
        {
            var properties = obj.GetType().GetProperties().Where(p => allowTypes.Contains(p.PropertyType) == false);

            TaskArray taskArray = new TaskArray(properties.Count());

            foreach (var item in properties)
            {
                taskArray.AddAndStart(() => item.SetValue(obj, null));
            }

            taskArray.WaitAll();
        }

        public static void SetNullProperties(this object obj, params string[] propertiesName)
        {
            var properties = obj.GetType().GetProperties().Where(p => propertiesName.Contains(p.Name));

            TaskArray taskArray = new TaskArray(properties.Count());

            foreach (var item in properties)
            {
                taskArray.AddAndStart(() => item.SetValue(obj, null));
            }

            taskArray.WaitAll();
        }
    }
}