using DHM.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DHM.Utilities
{
    public static class CsvExport
    {

        public static Dictionary<string, object> FlattenJToken(JToken token, string prefix = "")
        {
            var result = new Dictionary<string, object>();

            if (token.Type == JTokenType.Object)
            {
                foreach (var prop in token.Children<JProperty>())
                {
                    string propName = string.IsNullOrEmpty(prefix) ? prop.Name : $"{prefix}.{prop.Name}";
                    foreach (var inner in FlattenJToken(prop.Value, propName))
                        result[inner.Key] = inner.Value;
                }
            }
            else if (token.Type == JTokenType.Array)
            {
                int index = 0;
                foreach (var item in token.Children())
                {
                    string arrayPrefix = $"{prefix}[{index}]";
                    foreach (var inner in FlattenJToken(item, arrayPrefix))
                        result[inner.Key] = inner.Value;
                    index++;
                }
            }
            else
            {
                result[prefix] = ((JValue)token).Value;
            }

            return result;
        }



        public static void ExportCsv()
        {


            var json = File.ReadAllText("C:\\Users\\luwa0\\Documents\\Code Projects\\jsonParse\\jsonParse\\Models\\factoidfull.json");



            DataTable dt = new DataTable();


            /*
             //Dynamic Export
             
  
            JToken root = JToken.Parse(json);
            JArray items = root.Type == JTokenType.Array ? (JArray)root : new JArray(root);

            DataTable dt = new DataTable();

            foreach (JToken item in items)
            {
                var flat = FlattenJToken(item);

                // Add columns if not already present
                foreach (var key in flat.Keys)
                {
                    if (!dt.Columns.Contains(key))
                        dt.Columns.Add(key, flat[key]?.GetType() ?? typeof(object));
                }

                // Create row with values in column order
                var row = dt.NewRow();
                foreach (var kvp in flat)
                    row[kvp.Key] = kvp.Value ?? DBNull.Value;

                dt.Rows.Add(row);


            }           
             
             
             */



            DataTableToCsv(dt);


        }

        //Joins the datatable contents into a limited string and writes to a csv file
        public static void DataTableToCsv(DataTable dt) {


            StringBuilder sb = new StringBuilder();

            IEnumerable<string> columnNames = dt.Columns.Cast<DataColumn>().Select(column => column.ColumnName);
            sb.AppendLine(string.Join(",", columnNames));

            foreach (DataRow row in dt.Rows)
            {
                IEnumerable<string> fields = row.ItemArray.Select(field => field.ToString());
                sb.AppendLine(string.Join(",", fields));
            }

            File.WriteAllText("C:\\Users\\luwa0\\Desktop\\Code\\test.csv", sb.ToString());

        }


        public static DataTable CreateDataTable<Factoid>(ObservableCollection<Factoid> factoids)
        {
            Type type = typeof(Factoid);
            var properties = type.GetProperties();

            DataTable dataTable = new DataTable();
            dataTable.TableName = typeof(Factoid).FullName;



            foreach (PropertyInfo info in properties)
            {
                    
                    dataTable.Columns.Add(new DataColumn(info.Name, Nullable.GetUnderlyingType(info.PropertyType) ?? info.PropertyType));


            }


            foreach (Factoid factoid in factoids)
            {
                object[] values = new object[properties.Length];
                for (int i = 0; i < properties.Length; i++)
                {

                        values[i] = properties[i].GetValue(factoid);


                }

                dataTable.Rows.Add(values);
            }




            return dataTable;
        }



    }
}
