namespace AetherMvp.Domain;

// Clase base: compartimos lo que todos los organismos tienen en común.
public abstract class Organism : ISimulable
{
    // Inicializa los datos comunes de cualquier organismo.
    protected Organism(int id, string name, int x, int y, int energy, int age, int maxAge)
    {
        Id = id;
        Name = name;
        X = x;
        Y = y;
        Energy = energy;
        Age = age;
        MaxAge = maxAge;
    }

    public int Id { get; }
    public string Name { get; }
    // Este código corto conecta lo que ves en el mapa con el registro de eventos.
    public string Code => $"{Symbol}{Id:00}";
    public int X { get; private set; }
    public int Y { get; private set; }
    public int Energy { get; private set; }
    public int Age { get; private set; }
    public int MaxAge { get; }
    public bool IsAlive => Energy > 0 && Age < MaxAge;

    public abstract Species Kind { get; }
    public abstract char Symbol { get; }

    // Cada especie decide qué hacer durante su turno: aquí aparece el polimorfismo.
    public abstract void TakeTurn(World world);

    // Actualiza la posición sin permitir que otras clases manipulen las coordenadas directamente.
    internal void MoveTo(int x, int y)
    {
        X = x;
        Y = y;
    }

    // Suma energía sin pasar el límite práctico de 100. La inflación energética está controlada. :)
    public void GainEnergy(int amount)
    {
        Energy = Math.Min(100, Energy + Math.Max(0, amount));
    }

    // Descuenta energía y evita que termine en números negativos.
    protected void SpendEnergy(int amount)
    {
        Energy = Math.Max(0, Energy - Math.Max(0, amount));
    }

    // Envejece al organismo un turno.
    internal void GrowOlder()
    {
        Age++;
    }

    // Marca al organismo como eliminado del ecosistema.
    internal void Die()
    {
        Energy = 0;
    }

    // Paga el coste de reproducirse; tener descendencia no sale gratis.
    internal void PayForReproduction(int amount)
    {
        SpendEnergy(amount);
    }
}
