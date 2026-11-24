namespace AetherMvp.Domain;

// Guarda la identidad y ubicación de ambos organismos antes de que la presa desaparezca.
public sealed record HuntEvent(
    int Tick,
    string PredatorCode,
    string PredatorName,
    int PredatorX,
    int PredatorY,
    string PreyCode,
    string PreyName,
    int PreyX,
    int PreyY,
    int EnergyGained);
