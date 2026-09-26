using DamageCalculatorV2;

namespace TestProject;

public class Program
{
    public static async Task Main()
    {
        DmgCalc dmgCalc = new();
        var result = dmgCalc.Calculate("int i = 5; i = 10;", 1, 5, false, 1);

        Console.WriteLine($"damage dealt: {result.DamageDealt}");
        Console.WriteLine($"damage taken: {result.DamageTaken}");

        foreach (var error in result.Errors)
            Console.WriteLine(error);

        foreach (var variable in result.Statements)
            Console.WriteLine($"{variable.Type.ToString().ToLower()} {variable.Name} = {variable.Value};");
    }
}