namespace AetherMvp.Domain;

// Un resumen separado de la simulación: calcular estadísticas no debería alterar el mundo.
public sealed class WorldStats
{
    // Guarda los indicadores ya calculados para mostrarlos sin volver a recorrer el mundo.
    private WorldStats(int tick, int population, int herbivores, int predators, int food, double averageEnergy, double averageAge)
    {
        Tick = tick;
        Population = population;
        Herbivores = herbivores;
        Predators = predators;
        Food = food;
        AverageEnergy = averageEnergy;
        AverageAge = averageAge;
    }

    public int Tick { get; }
    public int Population { get; }
    public int Herbivores { get; }
    public int Predators { get; }
    public int Food { get; }
    public double AverageEnergy { get; }
    public double AverageAge { get; }

    // Calcula los indicadores actuales a partir de los organismos que siguen vivos.
    public static WorldStats Create(World world)
    {
        List<Organism> alive = world.Organisms.Where(organism => organism.IsAlive).ToList();
        double averageEnergy = alive.Count == 0 ? 0 : alive.Average(organism => organism.Energy);
        double averageAge = alive.Count == 0 ? 0 : alive.Average(organism => organism.Age);

        return new WorldStats(
            world.Tick,
            alive.Count,
            CountAlive<Herbivore>(alive),
            CountAlive<Predator>(alive),
            world.CountFood(),
            averageEnergy,
            averageAge);
    }

    // Método genérico: cuenta cualquier subtipo de Organism que le indiquemos.
    private static int CountAlive<T>(IEnumerable<Organism> organisms) where T : Organism
    {
        int total = 0;
        foreach (Organism organism in organisms)
        {
            if (organism is T && organism.IsAlive)
            {
                total++;
            }
        }

        return total;
    }
}
