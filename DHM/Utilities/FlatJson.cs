using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DHM.Utilities
{
    public static class FlatJson
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


    }
}
