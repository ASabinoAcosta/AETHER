namespace AetherMvp.Domain;

// DTO: representa el estado que necesitamos guardar, no la lógica del ecosistema.
public sealed class WorldSaveData
{
    public int Version { get; set; } = 1;
    public int Width { get; set; }
    public int Height { get; set; }
    public int Tick { get; set; }
    public int[] Food { get; set; } = [];
    public List<OrganismSaveData> Organisms { get; set; } = [];
}

// Datos simples para reconstruir cada organismo al cargar el JSON.
public sealed class OrganismSaveData
{
    public int Id { get; set; }
    public string Name { get; set; } = "Organismo";
    public Species Species { get; set; }
    public int X { get; set; }
    public int Y { get; set; }
    public int Energy { get; set; }
    public int Age { get; set; }
    public int MaxAge { get; set; }
}
