using System;

namespace IFC_Basic_Converter
{
    static class Program // Program is designed to extract information from IFC files, rearrange it properly using exp file and write information in json file. 
    {
        [STAThread]
        static void Main()
        {
            IfcSplitter.ifcConnector(@"C:\Users\vpetrina\Documents\Projekt1\IFC4.exp",     //location and name of exp file
                @"C:\Users\vpetrina\Documents\Projekt1\Z3-Rohbau-ifc4.ifc",          //location and name of ifc file .ifc
                @"C:\Users\vpetrina\Documents\Projekt1\Z3_nicht_nested.json");                //location and name of newly created json file 
        }
    }
}
