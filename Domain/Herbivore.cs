namespace AetherMvp.Domain;

// Hereda los datos comunes, pero tiene su propia estrategia de supervivencia.
public sealed class Herbivore : Organism
{
    // Crea un herbívoro con sus valores iniciales.
    public Herbivore(int id, string name, int x, int y, int energy = 12, int age = 0, int maxAge = 55)
        : base(id, name, x, y, energy, age, maxAge)
    {
    }

    public override Species Kind => Species.Herbivore;
    public override char Symbol => 'H';

    // Se mueve, intenta comer y gasta energía. La vida es dura incluso en píxeles.
    public override void TakeTurn(World world)
    {
        world.MoveRandomly(this);
        world.TryEatFood(this);
        SpendEnergy(1);
    }
}
