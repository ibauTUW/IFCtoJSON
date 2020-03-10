using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace IFC_Basic_Converter
{

    class IfcSplitter
    {       //class is designed to find the property values of each element in the IFC file and returns all of them in a string array, ending each with new line (ignore the backslashes since they are not recognized)
        public static string[] ifcSplit(string ifcPath)
        {
            string[] lines = File.ReadLines(ifcPath).SkipWhile(l => l != "DATA;").Skip(1).ToArray();
            List<string> propsList = new List<string>();

            for (int index = 0; !lines[index].Equals("ENDSEC;"); index++)
            {
                string line = lines[index];
                while (!line.EndsWith(";"))
                    line += lines[++index];
                string id = Regex.Match(line, @"(#\d+)").Groups[1].Value.ToUpper();     //add first id of the element
                propsList.Add(id);
                string name = Regex.Match(line, @"= ([^(]*)\(").Groups[1].Value.ToUpper();      //add the name also
                propsList.Add(name);
                string properties = line;       //add the list of properties- only between main brackets
                int start = properties.IndexOf("(") + 1;
                int end = properties.LastIndexOf(")");
                string result = properties.Substring(start, end - start);
                while (result != "")
                {
                    int propEnd = result.IndexOf(",") - 1;      //handle the cases where there is a value with brackets, last bracket in
                    if (result.Substring(0, 1) == "I")
                    {
                        if (result.Length > 3)
                        {
                            if (result.Substring(0, 3) == "IFC")
                            {
                                if (result.Length != result.LastIndexOf(")") + 1)
                                {
                                    propEnd = result.IndexOf("),");
                                    string temp = result.Substring(result.IndexOf("(") + 1, propEnd - result.IndexOf("(") - 1);
                                    propsList.Add(temp.Replace("\\", ""));
                                    result = result.Substring(propEnd + 2, result.Length - propEnd - 2);
                                }
                                else
                                {
                                    propEnd = result.Length - 1;
                                    string temp = result.Substring(result.IndexOf("(") + 1, propEnd - result.IndexOf("(") - 1);
                                    propsList.Add(temp.Replace("\\", ""));
                                    result = result.Substring(propEnd, result.Length - propEnd);

                                }
                            }
                        }
                        propEnd = 0;
                    }
                    else if (result.IndexOf("'") != -1 && result.IndexOf("'") < result.IndexOf(",") && result.IndexOf("'", result.IndexOf("'") + 1) > result.IndexOf(","))      //if no brackets with comma and not last
                    {
                        propEnd = result.IndexOf("'", result.IndexOf("'") + 1) + 1;
                        if (propEnd != result.Length)       //+1 next sign +1 skip the "," sign = +2
                        {
                            propsList.Add(result.Substring(0, propEnd).Replace("\\", ""));
                            result = result.Substring(propEnd + 1, result.Length - propEnd - 1);
                            propEnd = 0;
                        }
                    }
                    else if (result.IndexOf("(") != -1 && result.IndexOf("(") <= result.IndexOf(",") && result.IndexOf(")") > result.IndexOf(","))
                    {      //check if there is maybe more brackets inside, take only one value
                        string inBrackets = Regex.Match(result, @"\( ( [^()]+ | (?<Level>\() | (?<-Level>\)) )+ (?(Level)(?!)) \)",

                        RegexOptions.IgnorePatternWhitespace).Value;        //if starting with "(" change the form to JSON array as well (maybe should check end of string as well)
                        if (result.IndexOf(inBrackets) == 0)
                        {
                            propEnd = inBrackets.Length;
                        }
                        else
                        {
                            propEnd = result.IndexOf(inBrackets) + inBrackets.Length;
                        }
                        if (propEnd != result.Length)       //+1 next sign +1 skip the "," sign = +2
                        {
                            propsList.Add(inBrackets.Replace("\\", ""));
                            result = result.Substring(propEnd + 1, result.Length - propEnd - 1);
                        }
                    }       //handle the cases where there is a value inside quotes, last quote in 
                    else if (propEnd != -2)         //handle the last value
                    {
                        propsList.Add(result.Substring(0, propEnd + 1).Replace("\\", ""));      //+1 next sign +1 skip the "," sign = +2
                        result = result.Substring(propEnd + 2, result.Length - propEnd - 2);        //for cases which happened by chance ... that its exactly like the length
                        propEnd = 0;
                    }
                    if (propEnd == -2 || propEnd == result.Length)
                    {
                        if (result.IndexOf('(') == 0)
                        {
                            result = AdditionalMethods.toJsonArray(result);
                        }
                        propEnd = result.Length;
                        propsList.Add(result.Substring(0, propEnd).Replace("\\", ""));
                        propsList.Add(Environment.NewLine);
                        result = "";
                        //break;
                    }
                }
            }
            return (propsList.ToArray());
        }

        public static string[] findEntity(List<string> linesList, string start)      //finds the properties of the elements and makes a string array of a single element, starting with the object reference and entity name
        {
            //var lines = File.ReadAllLines(schemaPath);                              //there is already current     List<string> entitiesList = new List<string>();
            //int index;
            //Boolean firstFound = false;                                             //first found to speed up later
            List<string> current = new List<string>();

            string subtype;
            string propy;
            //List<string> linesList = lines.OfType<string>().ToList();
            string[] endArray = { "END_ENTITY;", " INVERSE", " DERIVE", " WHERE", " UNIQUE" };

            current.Add("objectRef");
            current.Add("entity");

            int index = linesList.FindIndex(x => x.Equals(start, StringComparison.OrdinalIgnoreCase));
            int index2 = linesList.FindIndex(x => x.Equals(start + ';', StringComparison.OrdinalIgnoreCase));
            if (index < 0 && index2 > 0)
                index = index2;

            while (!endArray.Any(linesList[index].Equals))
            {
                //System.Console.WriteLine(linesList[index]);
                if (linesList[index].Contains("SUBTYPE OF"))
                {
                    //System.Console.WriteLine(lines[index]);
                    subtype = Regex.Match(linesList[index], @"\(([^)]*)\)").Groups[1].Value.ToUpper();

                    //System.Console.WriteLine(subtype);
                    //propy = subtype;
                    findSubProps(linesList, "ENTITY " + subtype, current, 0);
                }
                propy = Regex.Match(linesList[index], @"\t([^:]*)\ :").Groups[1].Value;
                //System.Console.WriteLine(propy);
                if (propy != "")
                    current.Add(propy);
                index++;
            }
            /*
            for (index = 0; index < lines.Count(); index++)
            {
                if (!firstFound && lines[index].ToUpper().EndsWith(start) || lines[index].ToUpper().EndsWith(start + ";"))
                {
                    firstFound = true;
                    current.Add("objectRef");
                    current.Add("entity");
                }
                if (firstFound)
                {                                                    //findSubs(schemaPath, "ENTITY " + subtype + ";" event.);
                    if (lines[index].Contains("SUBTYPE OF"))
                    {
                        subtype = Regex.Match(lines[index], @"\(([^)]*)\)").Groups[1].Value.ToUpper();
                        findSubProps(schemaPath, "ENTITY " + subtype, current, 0);
                    }
                    propy = Regex.Match(lines[index], @"\t([^:]*)\ :").Groups[1].Value;
                    if (propy != "")
                    {
                        current.Add(propy);
                    }
                }
                string[] endArray = { "END_ENTITY", " INVERSE", " DERIVE", " WHERE", " UNIQUE" };
                if (firstFound && endArray.Any(lines[index].Equals))
                {
                    firstFound = false; break;
                }
            }*/
            return (current.ToArray());
        }

        public static void findSubProps(List<string> linesList, string start, List<string> current, int level)       //additional method after the first entity is found
        {
            //var lines = File.ReadAllLines(schemaPath);
            string subtype, propy;
            int index;

            //List<string> linesList = lines.OfType<string>().ToList();
            string[] endArray = { "END_ENTITY;", " INVERSE", " DERIVE", " WHERE", " UNIQUE" };

            index = linesList.FindIndex(x => x.Equals(start, StringComparison.OrdinalIgnoreCase));
            int index2 = linesList.FindIndex(x => x.Equals(start + ';', StringComparison.OrdinalIgnoreCase));

            //System.Console.WriteLine(index);
            //System.Console.WriteLine(index2);
            if (index < 0 && index2 > 0)
                index = index2;
            level++;
            index++;
            while (!endArray.Any(linesList[index].Equals))
            {
                if (linesList[index].Contains("SUBTYPE OF"))
                {
                    //System.Console.WriteLine(linesList[index]);
                    subtype = Regex.Match(linesList[index], @"\(([^)]*)\)").Groups[1].Value.ToUpper();
                    findSubProps(linesList, "ENTITY " + subtype, current, level);
                }
                if (linesList[index].Contains(" : "))
                {
                    propy = Regex.Match(linesList[index], @"\t([^:]*)\ :").Groups[1].Value;
                    if (propy != "")
                        current.Add(propy);
                }
                if (endArray.Any(linesList[index].Equals))
                    level--;
                index++;
            }

            /*
            for (index = 0; index < lines.Count(); index++)
            {
                if (lines[index].ToUpper().Equals(start) || lines[index].ToUpper().Equals(start + ";"))
                {
                    level++;
                }
                if (level > 0)
                {
                    if (lines[index].Contains("SUBTYPE OF"))
                    {                                               //level--;
                        subtype = Regex.Match(lines[index], @"\(([^)]*)\)").Groups[1].Value.ToUpper();
                        findSubProps(schemaPath, "ENTITY " + subtype, current, level);
                    }
                }                    //else { inside = false; }
                if (level > 0 && lines[index].Contains(" : "))
                {
                    propy = Regex.Match(lines[index], @"\t([^:]*)\ :").Groups[1].Value;
                    if (propy != "")
                    {
                        current.Add(propy);
                    }
                }
                if (level > 0)
                {
                    string[] endArray = { "END_ENTITY;", " INVERSE", " DERIVE", " WHERE", " UNIQUE" };
                    if (endArray.Any(lines[index].Equals))
                    {
                        level--;
                    }
                }
            }*/
        }

        public static void ifcConnector(string pathSchema, string pathIfc, string pathJson)         //connects the properties to the definition of the properties
        {
            Dictionary<string, Dictionary<string, string>> ifcFile = new Dictionary<string, Dictionary<string, string>>();
            Dictionary<string, string> element = new Dictionary<string, string>();
            var lines = File.ReadAllLines(pathSchema);
            List<string> linesList = lines.OfType<string>().ToList();

            string[] valuesArray = ifcSplit(pathIfc);
            string[] defsArray = findEntity(linesList, "ENTITY " + valuesArray[1]);
                /*foreach (string st in defsArray)
                    System.Console.WriteLine(st);
                foreach (string st in valuesArray)
                    System.Console.WriteLine(st);*/

            for (int index = 0, defsIndex = 0; index < valuesArray.Length; index++, defsIndex++)
            {   // handle the end of the entity
                //foreach (string st in defsArray)
                //  System.Console.WriteLine(st);
                //if (valuesArray[index+1] == "#32")


                if (valuesArray[index].Equals(Environment.NewLine))
                {
                    ifcFile.Add(element["objectRef"], element);
                    if (index != valuesArray.Length - 1)
                    {
                        element = new Dictionary<string, string>();                                 //einfache def?
                        defsArray = findEntity(linesList, "ENTITY " + valuesArray[index + 2]);
                        defsIndex = -1;
                    }

                }
                else
                {
                    //System.Console.WriteLine(defsIndex);
                    System.Console.WriteLine(defsArray[defsIndex]);
                    //System.Console.WriteLine(index);
                    System.Console.WriteLine(valuesArray[index]);
                    element.Add(defsArray[defsIndex], valuesArray[index]);
                }
            }
            MakeFile.mainPrint(ifcFile, pathJson);
        }
    }
}
