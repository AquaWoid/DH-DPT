using DHM.Models;
using Microsoft.VisualBasic;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DHM.Utilities
{
    public static class JsonExport
    {
        public static void ExportJson<T>(ObservableCollection<T> collection, string path)
        {

            JsonSerializer serializer = new JsonSerializer();
            serializer.NullValueHandling = NullValueHandling.Ignore;

            using (StreamWriter sw = new StreamWriter(path))
            using (JsonTextWriter writer = new JsonTextWriter(sw))
            {
                writer.Formatting = Formatting.Indented;
                serializer.Serialize(writer, collection);

            }

        }


        public static void DataTableToJson(DataTable dt, string path) {


            string result = JsonConvert.SerializeObject(dt);

            JsonSerializer serializer = new JsonSerializer();
            serializer.NullValueHandling = NullValueHandling.Ignore;

            using (StreamWriter sw = new StreamWriter(Path.Join(path, "export_" + DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss") + ".json")))
            using (JsonTextWriter writer = new JsonTextWriter(sw))
            {
                writer.Formatting = Formatting.Indented;
                serializer.Serialize(writer, dt);


            }

        }
    }
}
