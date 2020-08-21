using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using RedactedParser;

namespace ConsoleApp1
{
    class Program
    {
        static Dictionary<string, RedactedClass> table = new Dictionary<string, RedactedClass>();
        static Dictionary<int, List<Exception>> errorTable = new Dictionary<int, List<Exception>>();
        public static void Main(string[] args)
        {
            int numOfCoulmns = 38; //Theres 38 columns in the csv
            int errors = 0;
            StreamReader sr = new StreamReader("localfile.csv");
            sr.ReadLine(); //Skip first line
            string line;
            int lineNum = 0;

            while ((line = sr.ReadLine()) != null)
            {
                lineNum++;
                string[] productInfo = Regex.Split(line, "[\"],[\"]");//Split on commas between double quotes
                for (int i = 0; i < productInfo.Length; i++)
                {
                    productInfo[i] = productInfo[i].Replace("\"", "").Trim();//Remove double quotes/trailing/leading spaces
                }
                RedactedClass product = new RedactedClass();
                table.Add(productInfo[2], product);//Ensure unique identifiers are unique/store objects
                int counter = 0;
                foreach (PropertyInfo property in product.GetType().GetProperties()) //Get properties of object's type
                {
                    var value = productInfo[counter]; //Split strings index corresponds to property index
                    Type t = property.PropertyType; //Get the current properties type

                    if (t != typeof(string) && value != "")//If the property isn't a string, and the value isn't "", check if it's nullable/convert to underlying type
                    {
                        try
                        {
                            if (t.IsGenericType && t.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
                            {
                                if (value == null)
                                {
                                    property.SetValue(product, null);

                                }
                                else
                                    t = Nullable.GetUnderlyingType(t);
                            }

                            var newValue = Convert.ChangeType(value, t);
                            property.SetValue(product, newValue);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("{0} Exception caught.", e);
                            errors++;
                            List<Exception> eList = new List<Exception>();
                            eList.Add(e);
                            if (errorTable.ContainsKey(lineNum))
                                errorTable[lineNum].Add(e);
                            else
                                errorTable.Add(lineNum, eList);
                        }
                    }
                    else if (t == typeof(string))
                    {
                        property.SetValue(product, value);
                    }
                    else//This probably isnt needed
                    {
                        property.SetValue(product, null);
                    }
                    
                    if (counter < numOfCoulmns)
                        counter++;
                    else
                        counter = 0;
                }
            }
            Console.WriteLine($"Total Errors: {errors}");
            sr.Close();
        }

    }
}
