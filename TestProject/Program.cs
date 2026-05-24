using DamageCalculatorV2;
using System.Security.Cryptography;

public class Program
{
    public static void Main()
    {
        //DmgCalc dmgCalc = new DmgCalc();
        //var result = dmgCalc.Main("inti = 5;", 1, 5, false, 1);

        //Console.WriteLine($"Self dmg: {result.SelfDamage} | Errors: {result.Errors} | Enemies Number: {result.RemainingEnemies.Count}");

        //Console.WriteLine(Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)));

        var key = Environment.GetEnvironmentVariable("JWT_KEY");
        Console.WriteLine(key);
    }
}