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


            string docsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string exportPath = Path.Join(docsPath, "DHM Exports");
            Directory.CreateDirectory(exportPath);

            File.WriteAllText(Path.Join(exportPath, "export_" + DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss") + ".csv"), sb.ToString());

            File.WriteAllText("C:\\Users\\luwa0\\Desktop\\Code\\test.csv", sb.ToString());

        }

    }
}
