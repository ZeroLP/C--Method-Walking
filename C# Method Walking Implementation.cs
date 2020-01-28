using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            MethodWalk("Hangman");
            Console.ReadLine();
        }

        private static void MethodWalk(string ProcessName)
        {
            Assembly TargetAssembly = Assembly.LoadFrom(Process.GetProcessesByName(ProcessName).FirstOrDefault().MainModule.FileName); //Load the target assembly from process's MainModule location

            Console.WriteLine($"Loaded: {TargetAssembly.GetName().Name}\n");

            Dictionary<string, string> MethodSignatureDictionary = new Dictionary<string, string>(); //Declare the Method Signature Dictionary

            foreach (Type AnonType in TargetAssembly.GetTypes()) //Walk through the classes
            {
                foreach (MethodInfo AnonMethod in AnonType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)) //Walk through the methods
                {
                    string MethodName = $"{AnonMethod.ReturnType.Name} {AnonType.FullName.Replace("+", ".")}.{AnonMethod.Name}"; //declare the method name in format of "ReturnType Class.Method"

                    if (MethodSignatureDictionary.ContainsKey(MethodName)) continue; //If the same method exists in the class, ignore. (Maybe not, it can be an extension method)

                    MethodSignatureDictionary.Add(MethodName, ""); //Add the method name to the dictionary

                    foreach (ParameterInfo AnonParam in AnonMethod.GetParameters()) //Walk through the parameters
                    {
                        //If it's the last parameter, don't add the semicolon, add the parameter to MethodSignatureDictionary and break the loop
                        if (AnonParam == AnonMethod.GetParameters().Last()) { MethodSignatureDictionary[MethodName] = MethodSignatureDictionary[MethodName] + ", " + $"{AnonParam.ParameterType.Name} {AnonParam.Name}"; break; }

                        //add the parameter to MethodSignatureDictionary.
                        MethodSignatureDictionary[MethodName] = MethodSignatureDictionary[MethodName] + ", " + $"{AnonParam.ParameterType.Name} {AnonParam.Name} ";
                    }
                }

                //If it's the last class of the assembly
                if (AnonType == TargetAssembly.GetTypes().Last())
                {
                    //Walk through the MethodSignatureDictionary and retrieve methods and parameters
                    foreach (KeyValuePair<string, string> MethodSignature in MethodSignatureDictionary)
                    {
                        Console.WriteLine($"{MethodSignature.Key}({MethodSignature.Value})".Replace("(, ", "(").Replace(" ,", ","));
                    }
                }
            }

            //Method invokation example
            Type RandomType = TargetAssembly.GetType("Hangman.Modules.Menu"); //Gets the class
            MethodInfo RandomMethod = RandomType.GetMethod("InitialiseMenu"); //Gets the method
            object ClassInstance = Activator.CreateInstance(RandomType); //Create an instance to the class

            RandomMethod.Invoke(ClassInstance, null); //Execute with null parameter, since no parameters are required to be parsed for the method.
        }
    }
}
