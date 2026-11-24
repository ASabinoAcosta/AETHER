using AetherMvp.Domain;
using AetherMvp.Infrastructure;
using AetherMvp.Presentation;

namespace AetherMvp.Application;

// Coordina menú, mundo y almacenamiento: la consola no decide cómo sobreviven los organismos.
public sealed class AetherApp
{
    private World _world;
    private readonly IStorage<WorldSaveData> _storage;
    private readonly List<string> _recentEvents = [];
    private readonly List<HuntEvent> _recentHunts = [];

    // Prepara el mundo inicial y la ruta donde se guardarán las partidas.
    public AetherApp(IStorage<WorldSaveData>? storage = null)
    {
        _world = World.CreateDemo();
        string savePath = Path.Combine(Environment.CurrentDirectory, "saves", "aether-world.json");
        _storage = storage ?? new JsonStorage<WorldSaveData>(savePath);
        SubscribeToWorldEvents();
    }

    // Mantiene el programa en el menú hasta que se elija salir.
    public void Run()
    {
        bool running = true;
        while (running)
        {
            ConsoleUi.Clear();
            ConsoleUi.ShowMenu(_world);
            ConsoleUi.WritePrompt("Selecciona una opción: ");
            string choice = Console.ReadLine()?.Trim() ?? "0";

            switch (choice)
            {
                case "1": ObserveWorld(); break;
                case "2": AdvanceOneTurn(); break;
                case "3": AdvanceManyTurns(); break;
                case "4": AddOrganism(); break;
                case "5": ShowStatistics(); break;
                case "6": SaveWorld(); break;
                case "7": LoadWorld(); break;
                case "8": ResetWorld(); break;
                case "0": running = false; break;
                case "42": ConsoleUi.WriteMessage("La respuesta es 42. La pregunta todavía no compila. :P", ConsoleColor.Magenta); ConsoleUi.WaitForEnter(); break;
                default: ConsoleUi.WriteMessage("Opción no válida. El menú no lee mentes todavía.", ConsoleColor.Red); ConsoleUi.WaitForEnter(); break;
            }
        }

        ConsoleUi.Clear();
        ConsoleUi.ShowHeader();
        ConsoleUi.WriteMessage("Laboratorio cerrado. Que el ecosistema se porte bien.", ConsoleColor.Green);
    }

    // Muestra el mapa del mundo y su leyenda.
    private void ObserveWorld()
    {
        ConsoleUi.Clear();
        ConsoleUi.ShowHeader();
        ConsoleUi.DrawWorld(_world);
        ConsoleUi.ShowOrganismRoster(_world);
        ConsoleUi.ShowHuntHistory(_recentHunts);
        ConsoleUi.ShowEvents(_recentEvents);
        ConsoleUi.WaitForEnter();
    }

    // Ejecuta exactamente un turno de la simulación.
    private void AdvanceOneTurn()
    {
        _recentEvents.Clear();
        _world.AdvanceTurn();
        ConsoleUi.Clear();
        ConsoleUi.ShowHeader();
        ConsoleUi.WriteMessage($"Turno {_world.Tick} completado.", ConsoleColor.Green);
        ConsoleUi.DrawWorld(_world);
        ConsoleUi.ShowOrganismRoster(_world);
        ConsoleUi.ShowHuntHistory(_recentHunts);
        ConsoleUi.ShowEvents(_recentEvents);
        ConsoleUi.WaitForEnter();
    }

    // Ejecuta una cantidad de turnos elegida por el usuario y muestra el progreso.
    private void AdvanceManyTurns()
    {
        int turns = AskInteger("¿Cuántos turnos simulamos? (1-500): ", 1, 500);
        _recentEvents.Clear();

        for (int i = 1; i <= turns; i++)
        {
            _world.AdvanceTurn();
            if (turns >= 15)
            {
                ConsoleUi.DrawProgress(i, turns);
            }
        }

        if (turns >= 15)
        {
            Console.WriteLine();
        }

        ConsoleUi.Clear();
        ConsoleUi.ShowHeader();
        ConsoleUi.WriteMessage($"Simulación terminada: {turns} turnos procesados.", ConsoleColor.Green);
        ConsoleUi.DrawWorld(_world);
        ConsoleUi.ShowOrganismRoster(_world);
        ConsoleUi.ShowStatistics(_world);
        ConsoleUi.ShowHuntHistory(_recentHunts);
        ConsoleUi.ShowEvents(_recentEvents);
        ConsoleUi.WaitForEnter();
    }

    // Permite introducir un herbívoro o un depredador al ecosistema.
    private void AddOrganism()
    {
        ConsoleUi.Clear();
        ConsoleUi.ShowHeader();
        Console.WriteLine("  1. Herbívoro");
        Console.WriteLine("  2. Depredador");
        ConsoleUi.WritePrompt("Tipo de organismo: ");
        string choice = Console.ReadLine()?.Trim() ?? string.Empty;

        Species? species = choice switch
        {
            "1" => Species.Herbivore,
            "2" => Species.Predator,
            _ => null
        };

        if (species is null)
        {
            ConsoleUi.WriteMessage("No reconocí esa especie.", ConsoleColor.Red);
        }
        else if (_world.AddOrganism(species.Value))
        {
            ConsoleUi.WriteMessage("Organismo introducido correctamente.", ConsoleColor.Green);
        }
        else
        {
            ConsoleUi.WriteMessage("No quedan celdas libres. Este mundo ya está bastante concurrido.", ConsoleColor.Red);
        }

        ConsoleUi.ShowEvents(_recentEvents.TakeLast(2));
        ConsoleUi.WaitForEnter();
    }

    // Calcula y presenta las estadísticas del turno actual.
    private void ShowStatistics()
    {
        ConsoleUi.Clear();
        ConsoleUi.ShowHeader();
        ConsoleUi.ShowStatistics(_world);
        ConsoleUi.ShowOrganismRoster(_world);
        ConsoleUi.ShowHuntHistory(_recentHunts);
        ConsoleUi.WaitForEnter();
    }

    // Convierte el mundo a datos simples y los guarda en un archivo JSON.
    private void SaveWorld()
    {
        try
        {
            WorldSaveData data = WorldSaveMapper.ToSaveData(_world);
            _storage.Save(data);
            ConsoleUi.WriteMessage("Mundo guardado correctamente en saves/aether-world.json.", ConsoleColor.Green);
        }
        catch (Exception exception)
        {
            ConsoleUi.WriteMessage($"No se pudo guardar: {exception.Message}", ConsoleColor.Red);
        }

        ConsoleUi.WaitForEnter();
    }

    // Carga los datos JSON y reconstruye el ecosistema.
    private void LoadWorld()
    {
        try
        {
            WorldSaveData? data = _storage.Load();
            if (data is null)
            {
                ConsoleUi.WriteMessage("Todavía no hay una partida guardada.", ConsoleColor.Yellow);
            }
            else
            {
                // Primero validamos y reconstruimos; si el JSON está dañado, el mundo actual sigue conectado.
                World restoredWorld = WorldSaveMapper.FromSaveData(data);
                UnsubscribeFromWorldEvents();
                _world = restoredWorld;
                SubscribeToWorldEvents();
                _recentEvents.Clear();
                _recentHunts.Clear();
                ConsoleUi.WriteMessage("Mundo cargado. Seguimos donde lo dejamos.", ConsoleColor.Green);
            }
        }
        catch (Exception exception)
        {
            ConsoleUi.WriteMessage($"No se pudo cargar el archivo: {exception.Message}", ConsoleColor.Red);
        }

        ConsoleUi.WaitForEnter();
    }

    // Descarta el mundo actual y crea un ecosistema nuevo.
    private void ResetWorld()
    {
        ConsoleUi.WritePrompt("¿Reiniciar el ecosistema? Se perderán los cambios no guardados (S/N): ");
        string answer = Console.ReadLine()?.Trim().ToUpperInvariant() ?? "N";
        if (answer == "S")
        {
            UnsubscribeFromWorldEvents();
            _world = World.CreateDemo();
            _recentEvents.Clear();
            _recentHunts.Clear();
            SubscribeToWorldEvents();
            ConsoleUi.WriteMessage("Ecosistema nuevo. La historia vuelve a empezar.", ConsoleColor.Green);
        }
        else
        {
            ConsoleUi.WriteMessage("Reinicio cancelado.");
        }

        ConsoleUi.WaitForEnter();
    }

    // Lee y valida un entero dentro del rango permitido.
    private static int AskInteger(string prompt, int minimum, int maximum)
    {
        int value;
        bool isValid;

        do
        {
            ConsoleUi.WritePrompt(prompt);
            string input = Console.ReadLine() ?? string.Empty;
            isValid = int.TryParse(input, out value) && value >= minimum && value <= maximum;

            if (!isValid)
            {
                ConsoleUi.WriteMessage($"Introduce un número entre {minimum} y {maximum}.", ConsoleColor.Red);
            }
        }
        while (!isValid);

        return value;
    }

    // Se suscribe al evento del mundo para recibir mensajes sin acoplar sus clases a la consola.
    private void SubscribeToWorldEvents()
    {
        _world.MessagePublished += AddRecentEvent;
        _world.HuntOccurred += AddRecentHunt;
    }

    // Desconecta los eventos antes de reemplazar el mundo para no dejar suscripciones colgadas.
    private void UnsubscribeFromWorldEvents()
    {
        _world.MessagePublished -= AddRecentEvent;
        _world.HuntOccurred -= AddRecentHunt;
    }

    // Conserva las últimas doce cacerías con la identidad de cazador y presa.
    private void AddRecentHunt(HuntEvent hunt)
    {
        _recentHunts.Add(hunt);
        if (_recentHunts.Count > 12)
        {
            _recentHunts.RemoveAt(0);
        }
    }

    // Conserva solo los ocho eventos más recientes para que el registro no se vuelva infinito.
    private void AddRecentEvent(string message)
    {
        _recentEvents.Add(message);
        if (_recentEvents.Count > 8)
        {
            _recentEvents.RemoveAt(0);
        }
    }
}
