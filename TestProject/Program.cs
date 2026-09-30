using DamageCalculatorV2;
using DamageCalculatorV2.Nodes;

namespace TestProject;

public class Program
{
    public static async Task Main()
    {
        var result = DmgCalc.Calculate(@"string firstName = 100;", 1, 5, 1);

        Console.WriteLine($"damage dealt: {result.DamageDealt}");
        Console.WriteLine($"damage taken: {result.DamageTaken}");

        foreach (var error in result.Errors)
            Console.WriteLine(error);

        foreach (var statement in result.Statements)
        { 
            if (statement is VariableDeclarationStatement variable)
                Console.WriteLine($"{variable.Type} {variable.Name} = {variable.Value};");

            if (statement is VariableAssignmentStatement assignment)
                Console.WriteLine($"{assignment};");
        }
    }
}