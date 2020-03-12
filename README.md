# Background
One day, a question of “**_What if you can get all the methods from an assembly and use them?_**” blinked inside my mind. So I then quickly grabbed my VS and started implementing the non brainstormed idea. It turned out, that it’s a pretty useful however yet, security concerning implementation for developers. In this post, I will be walking you through the implementation of the idea and the ways of mitigating against it.

# What Is C# Method Walking?
C# Method Walking, by the name defines itself, it is an implementation of walking every methods in the assembly and retrieves it’s signature information.  
Additionally, the concept of the implementation can be further modified by the taste of the user for extensive purposes.

# Implementation
I will be testing the implementation against my [Hangman](https://github.com/ZeroLP/Hangman) application for the demonstration purposes.  
  
First, I needed to define where to get the assembly from, which I’ve chosen to get it dynamically with a running process’s [**MainModule.FileName**](https://docs.microsoft.com/en-us/dotnet/api/system.diagnostics.processmodule.filename?view=netframework-4.8) property.

```csharp
Process.GetProcessesByName("Hangman").FirstOrDefault().MainModule.FileName
```

Next, I’ve declared an Assembly type, and assigned [**Assembly.LoadFrom**](https://docs.microsoft.com/en-us/dotnet/api/system.reflection.assembly.loadfrom?view=netframework-4.8) method to load the entire Hangman assembly into my testing environment.

```csharp
Assembly TargetAssembly = Assembly.LoadFrom(Process.GetProcessesByName(ProcessName).FirstOrDefault().MainModule.FileName);
```

  
Cool, now the assembly is loaded into our domain, next part is where the fun part begins.

From here, I have created a foreach loop to walk through the classes and methods and successfully retrieved all the methods that resides in the assembly.

```csharp
foreach (Type AnonType in TargetAssembly.GetTypes())
{
   foreach(MethodInfo AnonMethod in AnonType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static))
   {
    Console.WriteLine($"{AnonMethod.ReturnType.Name} {AnonType.FullName.Replace("+", ".")}.{AnonMethod.Name}");
   }
}
```
  
From running the above code against my Hangman app, below are the methods that was retrieved from it.

```csharp
Void Hangman.MainClass.Main
Void Hangman.Modules.Menu.InitialiseMenu
Void Hangman.Modules.Game.Start
Void Hangman.Modules.Game.RunGameLogic
Void Hangman.Modules.Game.GuessHangWordLetter
Void Hangman.Modules.Game.GuessHangWordLetterCallBack
Boolean Hangman.Modules.Game.LetterSanityCheck
Void Hangman.Modules.Game.CheckLetterOccurrence
List`1 Hangman.Modules.Game.AllIndexesOf
String Hangman.Modules.Game.GenerateHangmanText
Void Hangman.Modules.Game.FillInDuplicates
String Hangman.Modules.Game.GetCurrentGuessedLetters
Void Hangman.Modules.Game.SaveHangWordLetters
String Hangman.Modules.Game.GetRandomWord
String[] Hangman.Modules.Game.ReadWordDB
Void Hangman.Modules.LogService.Log
```
  
Now that’s some good shit right there. However, that isn’t enough for me to invoke it. Since to invoke a method, you need the parameters to be parsed(if any, or if not, null value needs to be parsed). So I gave the code some slight(HUGE ASS) twist to retrieve the method signatures. Below is the twisted code:

```csharp
foreach (Type AnonType in TargetAssembly.GetTypes())
{
   foreach (MethodInfo AnonMethod in AnonType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static
   {
      string MethodName = $"{AnonMethod.ReturnType.Name} {AnonType.FullName.Replace("+", ".")}.{AnonMethod.Name}";
      if (MethodSignatureDictionary.ContainsKey(MethodName)) continue;

      MethodSignatureDictionary.Add(MethodName, "");

      foreach (ParameterInfo AnonParam in AnonMethod.GetParameters())
      {
         if(AnonParam == AnonMethod.GetParameters().Last()) { MethodSignatureDictionary[MethodName] = MethodSignatureDictionary[MethodName] + ", " + $"{AnonParam.ParameterType.Name} {AnonParam.Name}"; break; }

         MethodSignatureDictionary[MethodName] = MethodSignatureDictionary[MethodName] + ", " + $"{AnonParam.ParameterType.Name} {AnonParam.Name} ";
       }
    }
   
    if(AnonType == TargetAssembly.GetTypes().Last())
    {
      foreach (KeyValuePair<string, string> MethodSignature in MethodSignatureDictionary) 
      {
         Console.WriteLine($"{MethodSignature.Key}({MethodSignature.Value})".Replace("(, ", "(").Replace(" ,", ","));
      }
    }
}
```
  
It’s a pretty self-explanatory code if you walk through without skimming it.(tbh, if you can’t read a code, why be on here? xD. Although that code can be simplified even more due to me writing a shit code 😛 )  
You can see the full version of the implementation in the repo.
  
Below are the method signatures that was retrieved from it.

```csharp
Void Hangman.MainClass.Main(String[] args)
Void Hangman.Modules.Menu.InitialiseMenu()
Void Hangman.Modules.Game.Start()
Void Hangman.Modules.Game.RunGameLogic()
Void Hangman.Modules.Game.GuessHangWordLetter(Char GuessLetter)
Void Hangman.Modules.Game.GuessHangWordLetterCallBack()
Boolean Hangman.Modules.Game.LetterSanityCheck(Char GuessLetter)
Void Hangman.Modules.Game.CheckLetterOccurrence()
List`1 Hangman.Modules.Game.AllIndexesOf(String str, String value)
String Hangman.Modules.Game.GenerateHangmanText()
Void Hangman.Modules.Game.FillInDuplicates(StringBuilder sb, Int32 MasterIndex)
String Hangman.Modules.Game.GetCurrentGuessedLetters()
Void Hangman.Modules.Game.SaveHangWordLetters(String HW)
String Hangman.Modules.Game.GetRandomWord()
String[] Hangman.Modules.Game.ReadWordDB()
Void Hangman.Modules.LogService.Log(String Format, LogLevel FormatColor)
```
  
Alright cool, now let’s invoke one of the methods using those signatures.  
I’ve chosen the **Hangman.Modules.Menu.InitialiseMenu()** method to demonstrate how it can be invoked. Below is the code to invoke and result of the execution:
```csharp
Type RandomType = TargetAssembly.GetType("Hangman.Modules.Menu");
MethodInfo RandomMethod = RandomType.GetMethod("InitialiseMenu");
object ClassInstance = Activator.CreateInstance(RandomType);

RandomMethod.Invoke(ClassInstance, null);
```
  
Result:
![enter image description here](https://i.imgur.com/O2nv1fA.png)
  
  
How beautiful isn’t it? It gets more greater when you can bring the concept of this implementation even more further with your knowledge and creativity.  

# Mitigation
So, we have seen the fun part from the above, however we also have seen the insecure side of this implementation as well.  
To be honest with you, there are no current ways to fully mitigate/prevent against Method Walking due to the nature of Reflection’s accessibility and the CLR itself.  
However, there are few(or more than few) ways to create a temporary barrier giving the user an hard time to get through.(Note that this is not a 100% prevention method, but only to slow down the process of the implementation.)  
  
**1. Obfuscation**  
Yes, obfuscation. Obfuscating the assembly can prevent from executing reflection against the assembly, especially with symbol renaming and string obfuscation. However, if the user is determined and has a skill enough in reverse engineering, deobfuscation is unstoppable.(To slow down the process, it’s up to you to add some anti-debugging and anti-decompile checks.)  
**2. Running with less trust**  
This, however it’s only a measurement to prevent from modules having excessive permissions. It won’t completely stop the user as well, since it can be executed with elevated privileges.(aka Admin Elevation)  
**3. Running the code from a server**  
Run the code from a server, without the user in reach of the code physically. However it’s inefficient if entire code base is ran from the server. Suggest only important codes to be executed from the server.  
**4. Checking the caller via Stacktrace**  
Checking the caller via Stacktrace is also a good option too in order to check which assembly called the reflection and the information surrounding it. This also has a down side, Stacktrace can also be detoured, so checks against that is up to you to implement it.  
  
I am sure there are numerous more ways to mitigate against method walking other than the above I’ve mentioned that I can come up with at the time of writing. So, try to research more on them if you can.


