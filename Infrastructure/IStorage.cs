namespace AetherMvp.Infrastructure;

// Contrato genérico: cualquier almacenamiento debe saber guardar y cargar un tipo T.
public interface IStorage<T>
{
    // Guarda un objeto del tipo que implemente este almacenamiento.
    void Save(T data);

    // Recupera el objeto guardado o devuelve el valor por defecto si no existe.
    T? Load();
}
