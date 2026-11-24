using AetherMvp.Domain;

namespace AetherMvp.Presentation;

// La presentación vive aquí para que el ecosistema no tenga que saber cómo se dibuja la consola.
public static class ConsoleUi
{
    // Limpia la pantalla; si la consola no lo permite, seguimos sin romper el programa.
    public static void Clear()
    {
        try
        {
            Console.Clear();
        }
        catch (IOException)
        {
            // Puede pasar al ejecutar desde una terminal que no permite limpiar la pantalla.
        }
    }

    // Dibuja el encabezado principal del laboratorio.
    public static void ShowHeader()
    {
        WriteColor("  ╔══════════════════════════════════════════════════════════════╗\n", ConsoleColor.Cyan);
        WriteColor("  ║   █████╗ ███████╗████████╗██╗  ██╗███████╗██████╗            ║\n", ConsoleColor.Cyan);
        WriteColor("  ║  ██╔══██╗██╔════╝╚══██╔══╝██║  ██║██╔════╝██╔══██╗           ║\n", ConsoleColor.Cyan);
        WriteColor("  ║  ███████║█████╗     ██║   ███████║█████╗  ██████╔╝           ║\n", ConsoleColor.Cyan);
        WriteColor("  ║  ██╔══██║██╔══╝     ██║   ██╔══██║██╔══╝  ██╔══██╗           ║\n", ConsoleColor.Cyan);
        WriteColor("  ║  ██║  ██║███████╗   ██║   ██║  ██║███████╗██║  ██║           ║\n", ConsoleColor.Cyan);
        WriteColor("  ║  ╚═╝  ╚═╝╚══════╝   ╚═╝   ╚═╝  ╚═╝╚══════╝╚═╝  ╚═╝           ║\n", ConsoleColor.Cyan);
        WriteColor("  ║                 ARTIFICIAL ECOSYSTEM LAB                    ║\n", ConsoleColor.Magenta);
        WriteColor("  ╚══════════════════════════════════════════════════════════════╝\n", ConsoleColor.Cyan);
        Console.WriteLine("  Un pequeño mundo. Muchas formas de arruinar el equilibrio. :)");
        Console.WriteLine();
    }

    // Presenta el menú junto con un resumen del estado actual del mundo.
    public static void ShowMenu(World world)
    {
        ShowHeader();
        WorldStats stats = WorldStats.Create(world);

        WriteColor($"  TURNO {stats.Tick:0000}", ConsoleColor.Yellow);
        Console.Write("   │   Población: ");
        WriteColor(stats.Population.ToString(), ConsoleColor.Green);
        Console.Write("   │   Alimento: ");
        WriteColor(stats.Food.ToString(), ConsoleColor.Yellow);
        Console.WriteLine("\n  ──────────────────────────────────────────────────────────────");
        Console.WriteLine("   [1] Observar ecosistema");
        Console.WriteLine("   [2] Avanzar un turno");
        Console.WriteLine("   [3] Simular varios turnos");
        Console.WriteLine("   [4] Introducir un organismo");
        Console.WriteLine("   [5] Panel de estadísticas");
        Console.WriteLine("   [6] Guardar mundo (JSON)");
        Console.WriteLine("   [7] Cargar mundo (JSON)");
        Console.WriteLine("   [8] Reiniciar laboratorio");
        Console.WriteLine("   [0] Salir");
        Console.WriteLine("  ──────────────────────────────────────────────────────────────");
    }

    // Renderiza la matriz como un mapa y diferencia visualmente especies, alimento y espacios vacíos.
    public static void DrawWorld(World world)
    {
        Console.WriteLine();
        WriteColor("  MAPA DEL ECOSISTEMA", ConsoleColor.Cyan);
        Console.WriteLine($"   {world.Width} x {world.Height} celdas  |  Turno {world.Tick}");

        // Cada organismo ocupa su código (H01/P02), no solo una letra imposible de rastrear.
        int cellWidth = Math.Max(3, world.Organisms.Select(organism => organism.Code.Length).DefaultIfEmpty(3).Max());
        int mapWidth = world.Width * cellWidth;

        Console.Write("  ┌");
        Console.Write(new string('─', mapWidth));
        Console.WriteLine("┐");

        for (int y = 0; y < world.Height; y++)
        {
            Console.Write("  │");
            for (int x = 0; x < world.Width; x++)
            {
                Organism? organism = world.GetOrganismAt(x, y);
                string token = organism?.Code ?? (world.GetFoodAt(x, y) > 0 ? "*" : "·");
                string cell = token.PadRight(cellWidth);
                if (organism is Herbivore)
                {
                    WriteColor(cell, ConsoleColor.Green);
                }
                else if (organism is Predator)
                {
                    WriteColor(cell, ConsoleColor.Red);
                }
                else if (world.GetFoodAt(x, y) > 0)
                {
                    WriteColor(cell, ConsoleColor.Yellow);
                }
                else
                {
                    WriteColor(cell, ConsoleColor.DarkGray);
                }
            }

            Console.WriteLine("│");
        }

        Console.Write("  └");
        Console.Write(new string('─', mapWidth));
        Console.WriteLine("┘");
        Console.WriteLine("   ");
        WriteColor("   Hxx", ConsoleColor.Green);
        Console.Write(" Herbívoro   ");
        WriteColor("Pxx", ConsoleColor.Red);
        Console.Write(" Depredador   ");
        WriteColor("*", ConsoleColor.Yellow);
        Console.WriteLine(" Alimento   · Vacío");
    }

    // Relaciona cada código del mapa con su nombre y sus datos actuales.
    public static void ShowOrganismRoster(World world)
    {
        Console.WriteLine();
        WriteColor("  ═══ IDENTIFICACIÓN DE ORGANISMOS ═══", ConsoleColor.Cyan);
        if (world.Organisms.Count == 0)
        {
            Console.WriteLine("\n  No quedan organismos vivos. El mapa está en modo fantasma.");
            return;
        }

        foreach (Organism organism in world.Organisms.OrderBy(item => item.Id))
        {
            WriteColor($"\n  {organism.Code,-4}", organism is Herbivore ? ConsoleColor.Green : ConsoleColor.Red);
            Console.Write($" {organism.Name,-13} {SpeciesLabel(organism.Kind),-12}");
            Console.Write($" posición ({organism.X + 1:00}, {organism.Y + 1:00})");
            Console.Write($"  energía {organism.Energy,3}  edad {organism.Age,2}/{organism.MaxAge}");
        }

        Console.WriteLine();
    }

    // Explica las cacerías con los códigos que aparecen en el mapa.
    public static void ShowHuntHistory(IEnumerable<HuntEvent> hunts)
    {
        HuntEvent[] recent = hunts.ToArray();
        Console.WriteLine();
        WriteColor("  ═══ REGISTRO DE CAZA · QUIÉN SE COMIÓ A QUIÉN ═══", ConsoleColor.Red);
        if (recent.Length == 0)
        {
            Console.WriteLine("\n  Todavía no hay cacerías registradas. Los herbívoros siguen respirando.");
            return;
        }

        foreach (HuntEvent hunt in recent.Reverse())
        {
            Console.Write($"\n  T{hunt.Tick:0000}  ");
            WriteColor(hunt.PredatorCode, ConsoleColor.Red);
            Console.Write($" ({hunt.PredatorName}) se comió a ");
            WriteColor(hunt.PreyCode, ConsoleColor.Green);
            Console.WriteLine($" ({hunt.PreyName}).");
            Console.Write($"      Presa: ({hunt.PreyX + 1:00}, {hunt.PreyY + 1:00})");
            Console.Write($" · Cazador: ({hunt.PredatorX + 1:00}, {hunt.PredatorY + 1:00})");
            Console.Write($" · +{hunt.EnergyGained} energía");
        }

        Console.WriteLine();
    }

    private static string SpeciesLabel(Species species) => species switch
    {
        Species.Herbivore => "Herbívoro",
        Species.Predator => "Depredador",
        _ => "Desconocido"
    };

    // Muestra las cifras principales y barras para leerlas de un vistazo.
    public static void ShowStatistics(World world)
    {
        WorldStats stats = WorldStats.Create(world);
        Console.WriteLine();
        WriteColor("  ═══ INFORME DEL ECOSISTEMA ═══", ConsoleColor.Cyan);
        Console.WriteLine($"\n  Turno actual:       {stats.Tick}");
        Console.WriteLine($"  Población total:    {stats.Population}");
        Console.WriteLine($"  Herbívoros:         {stats.Herbivores}");
        Console.WriteLine($"  Depredadores:       {stats.Predators}");
        Console.WriteLine($"  Energía promedio:   {stats.AverageEnergy:F1}");
        Console.WriteLine($"  Edad promedio:      {stats.AverageAge:F1}");
        Console.WriteLine($"  Unidades de comida: {stats.Food}");
        Console.WriteLine();
        DrawBar("Población", stats.Population, world.Capacity, ConsoleColor.Green);
        DrawBar("Herbívoros", stats.Herbivores, Math.Max(1, world.Capacity), ConsoleColor.Green);
        DrawBar("Depredadores", stats.Predators, Math.Max(1, world.Capacity), ConsoleColor.Red);
        DrawBar("Alimento", stats.Food, world.Capacity * 3, ConsoleColor.Yellow);
    }

    // Dibuja una barra proporcional sin necesitar una biblioteca gráfica.
    public static void DrawBar(string label, int value, int max, ConsoleColor color)
    {
        const int width = 24;
        double ratio = max <= 0 ? 0 : Math.Clamp((double)value / max, 0, 1);
        int filled = (int)Math.Round(ratio * width);
        Console.Write($"  {label,-14} [");
        WriteColor(new string('█', filled), color);
        Console.Write(new string('░', width - filled));
        Console.WriteLine($"] {value}");
    }

    // Actualiza una barra mientras se procesan varios turnos.
    public static void DrawProgress(int current, int total)
    {
        const int width = 30;
        double ratio = total == 0 ? 1 : (double)current / total;
        int filled = (int)(ratio * width);
        Console.Write($"\r  Simulando [");
        WriteColor(new string('█', filled), ConsoleColor.Cyan);
        Console.Write(new string('░', width - filled));
        Console.Write($"] {ratio:P0}");
    }

    // Imprime los últimos acontecimientos del mundo.
    public static void ShowEvents(IEnumerable<string> events)
    {
        string[] recent = events.ToArray();
        if (recent.Length == 0)
        {
            return;
        }

        Console.WriteLine("\n  ── REGISTRO DE EVENTOS ──");
        foreach (string item in recent)
        {
            WriteColor("  › ", ConsoleColor.DarkCyan);
            Console.WriteLine(item);
        }
    }

    // Escribe un mensaje de resultado con color.
    public static void WriteMessage(string message, ConsoleColor color = ConsoleColor.Gray)
    {
        WriteColor($"  {message}", color);
        Console.WriteLine();
    }

    // Escribe una pregunta de entrada.
    public static void WritePrompt(string message)
    {
        WriteColor($"\n  {message}", ConsoleColor.Yellow);
    }

    // Espera a que la persona pulse Enter antes de volver al menú.
    public static void WaitForEnter()
    {
        Console.Write("\n  Pulsa Enter para volver al menú...");
        Console.ReadLine();
    }

    // Cambia temporalmente el color y lo restaura al terminar.
    private static void WriteColor(string text, ConsoleColor color)
    {
        ConsoleColor previous = Console.ForegroundColor;
        Console.ForegroundColor = color;
        Console.Write(text);
        Console.ForegroundColor = previous;
    }
}
