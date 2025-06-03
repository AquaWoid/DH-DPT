using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using DHM.Models;

public static class DataTableHelper
{
    // Option 1: Flatten nested properties with dot notation
    public static DataTable CreateFlattenedDataTable<Factoid>(ObservableCollection<Factoid> factoids, int maxDepth = 2)
    {
        var flatProperties = GetFlattenedProperties(typeof(Factoid), maxDepth);
        DataTable dataTable = new DataTable();
        dataTable.TableName = typeof(Factoid).FullName;


        List<FlatProperty> filteredProperties = new List<FlatProperty>();

        List<string> filters = new List<string>() {"id", "name", "created_by"};


        foreach (FlatProperty prop in filteredProperties)
        {

            if (filters.Any(p => p.Contains(prop.Name)))
            {
                filteredProperties.Add(prop);
            }
        }

        // Add columns for filtered properties
        foreach (FlatProperty prop in filteredProperties)
        {

            Type columnType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
            dataTable.Columns.Add(new DataColumn(prop.Name, columnType));
        }

        // Add rows
        foreach (Factoid item in factoids)
        {
            object[] values = new object[filteredProperties.Count];
            for (int i = 0; i < filteredProperties.Count; i++)
            {
                values[i] = GetNestedPropertyValue(factoids, filteredProperties[i].PropertyPath) ?? DBNull.Value;
            }
            dataTable.Rows.Add(values);
        }

        return dataTable;
    }



    // Helper classes and methods for flattened approach
    private class FlatProperty
    {
        public string Name { get; set; }
        public Type PropertyType { get; set; }
        public string[] PropertyPath { get; set; }
    }

    private static List<FlatProperty> GetFlattenedProperties(Type type, int maxDepth, string prefix = "", int currentDepth = 0)
    {
        var result = new List<FlatProperty>();

        if (currentDepth >= maxDepth)
            return result;

        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var prop in properties)
        {
            string propName = string.IsNullOrEmpty(prefix) ? prop.Name : $"{prefix}.{prop.Name}";

            if (IsSimpleType(prop.PropertyType))
            {
                result.Add(new FlatProperty
                {
                    Name = propName,
                    PropertyType = prop.PropertyType,
                    PropertyPath = propName.Split('.')
                });
            }
            else if (currentDepth < maxDepth - 1)
            {
                // Recursively get properties from complex types
                var nestedProps = GetFlattenedProperties(prop.PropertyType, maxDepth, propName, currentDepth + 1);
                result.AddRange(nestedProps);
            }
        }

        return result;
    }

    private static object GetNestedPropertyValue(object obj, string[] propertyPath)
    {
        object current = obj;

        foreach (string propName in propertyPath)
        {
            if (current == null) return null;

            PropertyInfo prop = current.GetType().GetProperty(propName);
            if (prop == null) return null;

            current = prop.GetValue(current);
        }

        return current;
    }

    private static bool IsSimpleType(Type type)
    {
        // Get the underlying type if it's nullable
        Type underlyingType = Nullable.GetUnderlyingType(type) ?? type;

        return underlyingType.IsPrimitive ||
               underlyingType.IsEnum ||
               underlyingType == typeof(string) ||
               underlyingType == typeof(decimal) ||
               underlyingType == typeof(DateTime) ||
               underlyingType == typeof(DateTimeOffset) ||
               underlyingType == typeof(TimeSpan) ||
               underlyingType == typeof(Guid);
    }
}
