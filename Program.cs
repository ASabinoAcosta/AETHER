using AetherMvp.Application;

// Punto de entrada: desde aquí arranca el laboratorio.
internal static class Program
{
    private static void Main()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        Console.InputEncoding = System.Text.Encoding.UTF8;
        new AetherApp().Run();
    }
}
