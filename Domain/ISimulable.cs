namespace AetherMvp.Domain;

// Interface: cualquier ser simulable debe poder jugar su turno y estar vivo (idealmente).
public interface ISimulable
{
    bool IsAlive { get; }
    void TakeTurn(World world);
}
