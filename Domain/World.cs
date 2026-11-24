namespace AetherMvp.Domain;

// El mundo usa una matriz 2D para el alimento y una lista para los organismos.
public sealed class World
{
    private const int MaxFoodPerCell = 3;
    private const int ReproductionThreshold = 22;
    private readonly int[,] _food;
    private readonly List<Organism> _organisms = [];
    private readonly Random _random = new();
    private int _nextId = 1;

    // Construye un mundo vacío con las dimensiones indicadas.
    public World(int width = 18, int height = 9)
    {
        if (width < 5 || height < 5)
        {
            throw new ArgumentOutOfRangeException(nameof(width), "El mundo debe tener al menos 5 x 5 celdas.");
        }

        Width = width;
        Height = height;
        _food = new int[width, height];
    }

    public int Width { get; }
    public int Height { get; }
    public int Tick { get; private set; }
    public int Capacity => Width * Height;
    public IReadOnlyList<Organism> Organisms => _organisms;

    // Action es un delegado: la interfaz puede suscribirse a los acontecimientos del mundo.
    public event Action<string>? MessagePublished;
    public event Action<HuntEvent>? HuntOccurred;

    // Crea un ecosistema inicial que permite probar el MVP sin configurar todo desde cero.
    public static World CreateDemo()
    {
        World world = new();
        world.GrowFood(55);

        for (int i = 0; i < 8; i++)
        {
            world.AddOrganism(Species.Herbivore);
        }

        for (int i = 0; i < 3; i++)
        {
            world.AddOrganism(Species.Predator);
        }

        return world;
    }

    // Añade un organismo en una celda vacía; devuelve false si ya no hay espacio.
    public bool AddOrganism(Species species)
    {
        if (!TryFindEmptyPosition(out (int X, int Y) position))
        {
            return false;
        }

        Organism organism = OrganismFactory.Create(species, _nextId++, position.X, position.Y);
        _organisms.Add(organism);
        Publish($"{organism.Name} apareció en ({position.X + 1}, {position.Y + 1}).");
        return true;
    }

    // Avanza un turno completo: actúan, envejecen, pueden reproducirse y vuelve a crecer alimento.
    public void AdvanceTurn()
    {
        Tick++;

        // ToArray evita modificar la colección mientras recorremos los turnos.
        foreach (Organism organism in _organisms.ToArray())
        {
            if (!organism.IsAlive)
            {
                continue;
            }

            organism.TakeTurn(this);
            organism.GrowOlder();

            if (!organism.IsAlive)
            {
                Publish($"{organism.Name} dejó de existir. F en el chat.");
            }
        }

        _organisms.RemoveAll(organism => !organism.IsAlive);

        foreach (Organism organism in _organisms.ToArray())
        {
            TryReproduce(organism);
        }

        GrowFood(2);
    }

    // Mueve al organismo a una casilla vecina libre, si existe alguna.
    public bool MoveRandomly(Organism organism)
    {
        List<(int X, int Y)> options = GetFreeNeighbors(organism.X, organism.Y);
        if (options.Count == 0)
        {
            return false;
        }

        (int X, int Y) target = options[_random.Next(options.Count)];
        organism.MoveTo(target.X, target.Y);
        return true;
    }

    // Permite que un herbívoro consuma el alimento de la casilla donde terminó su movimiento.
    public bool TryEatFood(Herbivore herbivore)
    {
        if (_food[herbivore.X, herbivore.Y] <= 0)
        {
            return false;
        }

        _food[herbivore.X, herbivore.Y]--;
        herbivore.GainEnergy(5);
        Publish($"{herbivore.Name} encontró alimento (+5 energía).");
        return true;
    }

    // El depredador caza una presa que esté en una de las cuatro casillas vecinas.
    public bool TryHunt(Predator predator)
    {
        Organism? prey = null;
        foreach (Organism candidate in _organisms)
        {
            bool isNeighbor = Math.Abs(candidate.X - predator.X) + Math.Abs(candidate.Y - predator.Y) == 1;
            if (candidate is Herbivore && candidate.IsAlive && isNeighbor)
            {
                prey = candidate;
                break;
            }
        }

        if (prey is null)
        {
            return false;
        }

        int predatorEnergyBefore = predator.Energy;
        prey.Die();
        predator.GainEnergy(12);

        // Guardamos los datos antes de retirar la presa: después ya no estará en el mapa.
        HuntOccurred?.Invoke(new HuntEvent(
            Tick,
            predator.Code,
            predator.Name,
            predator.X,
            predator.Y,
            prey.Code,
            prey.Name,
            prey.X,
            prey.Y,
            predator.Energy - predatorEnergyBefore));
        return true;
    }

    // Devuelve el símbolo que debe dibujarse en una coordenada del mapa.
    public char GetCellSymbol(int x, int y)
    {
        Organism? organism = GetOrganismAt(x, y);
        if (organism is not null)
        {
            return organism.Symbol;
        }

        return _food[x, y] > 0 ? '*' : '·';
    }

    // Devuelve el organismo de una celda o null si está vacía.
    public Organism? GetOrganismAt(int x, int y)
    {
        return _organisms.FirstOrDefault(organism => organism.IsAlive && organism.X == x && organism.Y == y);
    }

    // Consulta la cantidad de alimento que hay en una celda.
    public int GetFoodAt(int x, int y) => _food[x, y];

    // Cuenta las unidades de alimento que quedan en todo el mundo.
    public int CountFood()
    {
        int total = 0;
        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                total += _food[x, y];
            }
        }

        return total;
    }

    // Reparte alimento al azar sin superar el límite de cada celda.
    public void GrowFood(int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            int x = _random.Next(Width);
            int y = _random.Next(Height);
            if (_food[x, y] < MaxFoodPerCell)
            {
                _food[x, y]++;
            }
        }
    }

    // Copia el estado del alimento en una lista plana para que JSON pueda guardarlo sin problemas.
    internal int[] ExportFood()
    {
        int[] data = new int[Width * Height];
        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                data[y * Width + x] = _food[x, y];
            }
        }

        return data;
    }

    // Restaura un dato de alimento al cargar una partida.
    internal void SetFood(int x, int y, int amount)
    {
        _food[x, y] = Math.Clamp(amount, 0, MaxFoodPerCell);
    }

    // Inserta un organismo ya reconstruido desde un archivo de guardado.
    internal void AddRestoredOrganism(Organism organism)
    {
        if (organism.X < 0 || organism.X >= Width || organism.Y < 0 || organism.Y >= Height ||
            GetOrganismAt(organism.X, organism.Y) is not null)
        {
            throw new InvalidDataException("El archivo contiene una posición de organismo inválida o repetida.");
        }

        _organisms.Add(organism);
        _nextId = Math.Max(_nextId, organism.Id + 1);
    }

    // Restaura el contador de turnos al cargar.
    internal void SetTick(int tick)
    {
        Tick = Math.Max(0, tick);
    }

    // Publica un mensaje para que la consola lo muestre sin acoplar la lógica del mundo a la interfaz.
    private void Publish(string message)
    {
        MessagePublished?.Invoke(message);
    }

    // Busca una casilla vacía para una aparición o un nacimiento.
    private bool TryFindEmptyPosition(out (int X, int Y) position)
    {
        for (int attempt = 0; attempt < 100; attempt++)
        {
            int x = _random.Next(Width);
            int y = _random.Next(Height);
            if (GetOrganismAt(x, y) is null)
            {
                position = (x, y);
                return true;
            }
        }

        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                if (GetOrganismAt(x, y) is null)
                {
                    position = (x, y);
                    return true;
                }
            }
        }

        position = (-1, -1);
        return false;
    }

    // Prepara las cuatro casillas vecinas y descarta las que están fuera del mapa u ocupadas.
    private List<(int X, int Y)> GetFreeNeighbors(int x, int y)
    {
        (int X, int Y)[] directions = [(0, -1), (1, 0), (0, 1), (-1, 0)];
        List<(int X, int Y)> options = [];

        foreach ((int dx, int dy) in directions)
        {
            int nextX = x + dx;
            int nextY = y + dy;
            bool inside = nextX >= 0 && nextX < Width && nextY >= 0 && nextY < Height;

            if (inside && GetOrganismAt(nextX, nextY) is null)
            {
                options.Add((nextX, nextY));
            }
        }

        return options;
    }

    // Reproduce organismos con suficiente energía y una pequeña probabilidad por turno. No son gremlins. :P
    private void TryReproduce(Organism parent)
    {
        if (parent.Energy < ReproductionThreshold || _random.Next(100) >= 8)
        {
            return;
        }

        List<(int X, int Y)> options = GetFreeNeighbors(parent.X, parent.Y);
        if (options.Count == 0)
        {
            return;
        }

        (int X, int Y) position = options[_random.Next(options.Count)];
        parent.PayForReproduction(8);
        Organism child = OrganismFactory.Create(parent.Kind, _nextId++, position.X, position.Y, energy: 10);
        _organisms.Add(child);
        Publish($"¡Nació {child.Name}! La población acaba de crecer.");
    }
}
