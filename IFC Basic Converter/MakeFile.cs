using System;
using System.Collections.Generic;
using System.Linq;

namespace IFC_Basic_Converter
{
    class MakeFile
    {
        public static void mainPrint(Dictionary<string, Dictionary<string, string>> ifcFile, string pathJson)
        {
            using (System.IO.StreamWriter output = new System.IO.StreamWriter(pathJson))
            {
                foreach (var elements in ifcFile)
                    output.WriteLine(jsonPrinter2(elements.Value, "", ifcFile));
            }
        }

        public static string jsonPrinter2(Dictionary<string, string> element, string elementText, Dictionary<string, Dictionary<string, string>> ifcFile)
        {
            elementText = "{" + Environment.NewLine;

            var propertyFirst = element.First();
            var propertyLast = element.Last();

            foreach (var property in element)
            {
                if (property.Value.First().Equals('['))
                    elementText += '"' + property.Key + "\" : " + property.Value;
                //else if (!property.Equals(propertyFirst) && property.Value.Length > 1 && property.Value.First().Equals('#'))
                  //  elementText += Environment.NewLine + "\"" + property.Key + "\" : " + jsonPrinter2(ifcFile[property.Value], elementText, ifcFile);
                else
                    elementText += "\"" + property.Key + "\" : \"" + property.Value + "\"";

                if (property.Equals(propertyLast))
                    elementText += Environment.NewLine;
                else
                    elementText += ",";
            }
            elementText += "}";
            if (element.Last().Equals(propertyLast))
            {
                return elementText;
            }
            else return ("");
        }
    }
}
