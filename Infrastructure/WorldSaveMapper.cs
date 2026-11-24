using AetherMvp.Domain;

namespace AetherMvp.Infrastructure;

// Traduce entre el objeto World y los datos planos que se guardan en JSON.
public static class WorldSaveMapper
{
    // Convierte el mundo actual a un objeto preparado para persistencia.
    public static WorldSaveData ToSaveData(World world)
    {
        return new WorldSaveData
        {
            Version = 1,
            Width = world.Width,
            Height = world.Height,
            Tick = world.Tick,
            Food = world.ExportFood(),
            Organisms = world.Organisms
                .Where(organism => organism.IsAlive)
                .Select(organism => new OrganismSaveData
                {
                    Id = organism.Id,
                    Name = organism.Name,
                    Species = organism.Kind,
                    X = organism.X,
                    Y = organism.Y,
                    Energy = organism.Energy,
                    Age = organism.Age,
                    MaxAge = organism.MaxAge
                })
                .ToList()
        };
    }

    // Reconstruye un mundo y valida lo básico antes de aceptar el archivo.
    public static World FromSaveData(WorldSaveData data)
    {
        if (data.Version != 1)
        {
            throw new InvalidDataException("La versión del archivo no es compatible con este MVP.");
        }

        if (data.Width < 5 || data.Width > 60 || data.Height < 5 || data.Height > 30 ||
            data.Food.Length != data.Width * data.Height)
        {
            throw new InvalidDataException("Las dimensiones o los datos del mapa no son válidos.");
        }

        World world = new(data.Width, data.Height);
        world.SetTick(data.Tick);

        for (int y = 0; y < data.Height; y++)
        {
            for (int x = 0; x < data.Width; x++)
            {
                world.SetFood(x, y, data.Food[y * data.Width + x]);
            }
        }

        foreach (OrganismSaveData saved in data.Organisms)
        {
            Organism organism = OrganismFactory.Create(
                saved.Species,
                saved.Id,
                saved.X,
                saved.Y,
                saved.Energy,
                saved.Age,
                saved.MaxAge,
                saved.Name);

            world.AddRestoredOrganism(organism);
        }

        return world;
    }
}
