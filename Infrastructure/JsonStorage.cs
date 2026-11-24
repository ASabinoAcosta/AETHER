using System.Text.Json;

namespace AetherMvp.Infrastructure;

// Implementación JSON del contrato de almacenamiento; T permite reutilizarla con otros datos.
public sealed class JsonStorage<T> : IStorage<T>
{
    private readonly string _filePath;
    private readonly JsonSerializerOptions _options = new() { WriteIndented = true };

    // Recibe la ruta del archivo que usará este almacenamiento.
    public JsonStorage(string filePath)
    {
        _filePath = filePath;
    }

    // Serializa los datos y crea la carpeta si todavía no existe.
    public void Save(T data)
    {
        string fullPath = Path.GetFullPath(_filePath);
        string? directory = Path.GetDirectoryName(fullPath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        string json = JsonSerializer.Serialize(data, _options);
        File.WriteAllText(fullPath, json);
    }

    // Lee el archivo y convierte el JSON nuevamente al tipo T.
    public T? Load()
    {
        string fullPath = Path.GetFullPath(_filePath);
        if (!File.Exists(fullPath))
        {
            return default;
        }

        string json = File.ReadAllText(fullPath);
        return JsonSerializer.Deserialize<T>(json, _options);
    }
}
