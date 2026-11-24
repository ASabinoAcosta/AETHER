namespace AetherMvp.Domain;

// Factory sencilla: centraliza la creación para no repetir new Herbivore/new Predator por todo el proyecto.
public static class OrganismFactory
{
    private static readonly string[] Names =
    [
        "Ada", "Turing", "Grace", "Kelp", "Moss", "Bug-01", "Pixel", "Byte", "Nébula", "404"
    ];

    // Crea la clase concreta según la especie solicitada.
    public static Organism Create(
        Species species,
        int id,
        int x,
        int y,
        int? energy = null,
        int age = 0,
        int? maxAge = null,
        string? name = null)
    {
        string organismName = name ?? $"{Names[(id - 1) % Names.Length]}-{id:00}";

        return species switch
        {
            Species.Herbivore => new Herbivore(id, organismName, x, y, energy ?? 12, age, maxAge ?? 55),
            Species.Predator => new Predator(id, organismName, x, y, energy ?? 20, age, maxAge ?? 65),
            _ => throw new ArgumentOutOfRangeException(nameof(species), "Especie no reconocida.")
        };
    }
}
