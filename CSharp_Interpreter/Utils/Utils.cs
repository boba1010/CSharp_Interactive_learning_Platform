namespace CSharp_Interpreter.Utils
{
    public class Utils
    {
        public static string? Input(string msg)
        {
            Console.Write(msg);
            return Console.ReadLine();
        }
    }
}
