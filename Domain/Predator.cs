namespace AetherMvp.Domain;

// Otro tipo de organismo: misma base, comportamiento diferente.
public sealed class Predator : Organism
{
    // Crea un depredador con sus valores iniciales.
    public Predator(int id, string name, int x, int y, int energy = 20, int age = 0, int maxAge = 65)
        : base(id, name, x, y, energy, age, maxAge)
    {
    }

    public override Species Kind => Species.Predator;
    public override char Symbol => 'P';

    // Primero busca una presa cercana; si no encuentra, se mueve para buscarla.
    public override void TakeTurn(World world)
    {
        if (!world.TryHunt(this))
        {
            world.MoveRandomly(this);
        }

        SpendEnergy(2);
    }
}
