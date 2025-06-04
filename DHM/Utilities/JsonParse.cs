using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using DHM.Models;
using System.IO;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;


namespace DHM.Utilities
{
    public static class JsonParse
    {
        public static async Task<ObservableCollection<T>> ParseJson<T>(string path) {


            var json = await File.ReadAllTextAsync(path, new UTF8Encoding(false));


            try
            {
                ObservableCollection<T>? factoids = JsonSerializer.Deserialize<ObservableCollection<T>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return factoids;
            }
            catch (Exception parseError)
            {
                Console.WriteLine(parseError.Message);
                var filtered = Regex.Match(parseError.Message, @"LineNumber: \d{1,}").Value;
                Console.WriteLine("Regex: " + filtered + "Error Type: " + parseError.GetType());
                return null;
            }

        }

    }
}
