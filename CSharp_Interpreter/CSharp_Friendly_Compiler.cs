using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Reflection;
using System.Runtime.Loader;

namespace CSharp_Friendly_Compiler;

public static class CSharp_Friendly_Compiler
{
    public static void CompileAndRun(string code)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(code);

        List<MetadataReference> references =
        [
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Console).Assembly.Location),
            MetadataReference.CreateFromFile(Assembly.Load("System.Runtime").Location)
        ];

        var compilation = CSharpCompilation.Create("Temp", [syntaxTree], references, new CSharpCompilationOptions(OutputKind.ConsoleApplication));

        var alc = new AssemblyLoadContext("UserCodeSandbox", isCollectible: true);

        try
        {
            using var ms = new MemoryStream();
            var result = compilation.Emit(ms);

            if (!result.Success)
            {
                foreach (var error in result.Diagnostics)
                    Console.WriteLine(error.GetMessage());
                return;
            }

            ms.Seek(0, SeekOrigin.Begin);

            var assembly = alc.LoadFromStream(ms);

            var type = assembly.GetType("Program");

            var method = type?.GetMethod("Main", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);

            // Invoke execution
            method?.Invoke(null, null);
        }
        finally
        {
            alc.Unload();

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            var memory = GC.GetTotalMemory(true);
            Console.WriteLine("Total memory freed: " + memory / 1024 / 1024);
        }
    }
}