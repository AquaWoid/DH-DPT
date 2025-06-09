using DHM.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DHM.Utilities
{
    public static class OptionSerialization
    {


        public static string GetOptionPath()
        {
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string exportPath = Path.Combine(appDataPath, "DHM");

            return exportPath;
        }

        //Initializing tableFilters.json into AppData/DHM 
        public static void InitializeOptionSaveFile()
        {
            string exportPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "DHM");

            Directory.CreateDirectory(exportPath);


            string saveFilePath = Path.Combine(exportPath, "tableFilters.json");

            JsonExport.ExportJson(OptionFileCreator.GetAvailableFilters(), saveFilePath);


        }

        public async static void LoadOptions()
        {

            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string exportPath = Path.Combine(appDataPath, "DHM");
            try
            {
                var tableFilters = await JsonParse.ParseJson<TableFilter>(Path.Join(exportPath, "tableFilters.json"));
            }
            catch (Exception ex)
            {

                if (ex is FileNotFoundException || ex is DirectoryNotFoundException)
                {

                    Directory.CreateDirectory(exportPath);

                    ObservableCollection<TableFilter> filters = OptionFileCreator.GetAvailableFilters();



                    LoadOptions();
                }

            }

        }



    }
}
