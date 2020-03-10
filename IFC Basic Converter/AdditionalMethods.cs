using System;
using System.IO;
using System.Linq;

namespace IFC_Basic_Converter
{
    class AdditionalMethods
    {
        public static void writeInFile(string path)
        {
            try
            {
                //Pass the filepath and filename to the StreamWriter Constructor
                StreamWriter sw = new StreamWriter(path);

                //Write a line of text
                sw.WriteLine("Hello World!!");

                //Write a second line of text
                sw.WriteLine("From the StreamWriter class");

                //Close the file
                sw.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: " + e.Message);
            }
            finally
            {
                Console.WriteLine("Executing finally block.");
            }
        }

        public static void readFromFile(string path)
        {
            String line;
            try
            {
                //Pass the file path and file name to the StreamReader constructor
                StreamReader sr = new StreamReader(path);

                //Read the first line of text
                line = sr.ReadLine();

                //Continue to read until you reach end of file
                while (line != null)
                {
                    //write the lie to console window
                    Console.WriteLine(line);
                    //Read the next line
                    line = sr.ReadLine();
                }

                //close the file
                sr.Close();
                Console.ReadLine();
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: " + e.Message);
            }
            finally
            {
                Console.WriteLine("Executing finally block.");
            }
        }

        public static string toJsonArray(string arrayProperty)
        {
            //separate the commas
            string[] inBracketsArray = arrayProperty.Substring(1, arrayProperty.Length - 2).Split(',');
            //make it as in JSON
            return arrayProperty = '[' + string.Join(",", inBracketsArray.Select(x => $"\"{x}\"")) + ']';

        }


    }
}
