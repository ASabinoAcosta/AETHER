# AETHER — Artificial Ecosystem Lab (MVP)

Un pequeño simulador de ecosistemas artificiales hecho en **C# con .NET 10**. La entrega prioriza un producto funcional, una consola visual y una estructura que permita seguir ampliándolo.

## Qué incluye esta primera versión

- Mundo 2D representado mediante una matriz.
- Herbívoros que se desplazan, consumen alimento y gastan energía.
- Depredadores que cazan herbívoros cercanos.
- Envejecimiento, muerte y reproducción probabilística.
- Turnos individuales o simulación por lotes (hasta 500 turnos).
- Panel de estadísticas y barras de progreso.
- Introducción manual de organismos.
- Guardado y carga del mundo mediante JSON.
- Registro de los últimos acontecimientos.
- Códigos individuales en el mapa (H01, P02...) y ficha de cada organismo vivo.
- Historial de las últimas 12 cacerías: depredador, presa, nombres, coordenadas y energía obtenida.
- Menú de consola con colores, bordes, símbolos y mapa.

## Requisitos

- SDK de **.NET 10**.
- No requiere paquetes NuGet externos.
- Terminal que admita caracteres Unicode para ver correctamente el mapa y los paneles.

## Ejecutar

Desde la carpeta `AETHER_MVP`:

```bash
dotnet run
```

Para compilar sin ejecutar:

```bash
dotnet build
```

El archivo de guardado se crea en `saves/aether-world.json`, relativo al directorio desde el que se ejecuta el programa.

## Controles

| Opción | Acción |
|---|---|
| 1 | Observar el ecosistema |
| 2 | Avanzar un turno |
| 3 | Simular entre 1 y 500 turnos |
| 4 | Introducir un herbívoro o depredador |
| 5 | Consultar estadísticas |
| 6 | Guardar el mundo en JSON |
| 7 | Cargar el mundo guardado |
| 8 | Reiniciar el ecosistema |
| 0 | Salir |

## Estructura

```text
AETHER_MVP/
├── Application/
│   └── AetherApp.cs          # Menú y coordinación del programa
├── Domain/
│   ├── Herbivore.cs          # Comportamiento del herbívoro
│   ├── HuntEvent.cs          # Datos de una cacería individual
│   ├── ISimulable.cs         # Contrato de los objetos simulables
│   ├── Organism.cs           # Clase base de los organismos
│   ├── OrganismFactory.cs    # Creación centralizada de organismos
│   ├── Predator.cs           # Comportamiento del depredador
│   ├── Species.cs            # Enumeración de especies
│   ├── World.cs              # Reglas y estado del ecosistema
│   ├── WorldSaveData.cs      # Modelo de datos para guardar
│   └── WorldStats.cs         # Cálculo de estadísticas
├── Infrastructure/
│   ├── IStorage.cs           # Contrato de almacenamiento genérico
│   ├── JsonStorage.cs        # Persistencia JSON
│   └── WorldSaveMapper.cs    # Conversión entre mundo y datos guardados
├── Presentation/
│   └── ConsoleUi.cs          # Interfaz de consola
├── Program.cs
└── AETHER_MVP.csproj
```

## Conceptos de C# que se pueden señalar en la entrega

| Tema | Dónde aparece |
|---|---|
| Variables, operadores y condicionales | Reglas de movimiento, energía, alimento y supervivencia |
| `for`, `foreach`, `while`, `do-while`, `break` y `continue` | Recorridos, validación de entradas y búsqueda de presas |
| Funciones y métodos | Operaciones separadas en `World`, `ConsoleUi` y `AetherApp` |
| Arreglos bidimensionales | `int[,] _food` en `World` |
| Listas y arreglos | Colección de organismos y posiciones vecinas |
| Clases, encapsulación y clases abstractas | `Organism`, `World` y sus propiedades controladas |
| Constructores | Creación de mundos y organismos |
| Herencia | `Herbivore` y `Predator` heredan de `Organism` |
| Polimorfismo | `TakeTurn()` se ejecuta según el tipo concreto |
| Interfaces | `ISimulable` e `IStorage<T>` |
| `enum` | `Species` |
| Fábrica de objetos | `OrganismFactory` centraliza la creación |
| Delegados y eventos | `Action<string>`, `Action<HuntEvent>`, `MessagePublished` y `HuntOccurred` |
| Genéricos | `IStorage<T>`, `JsonStorage<T>` y `CountAlive<T>` |
| JSON | `JsonSerializer` en `JsonStorage<T>` |
| MVC a nivel de consola y principios SOLID | `World` actúa como modelo, `ConsoleUi` como vista y `AetherApp` coordina; se separan responsabilidades y se depende de `IStorage<T>` |

## Límites conocidos del MVP

- El movimiento es aleatorio entre las cuatro casillas vecinas libres.
- Los depredadores cazan si tienen un herbívoro en una casilla adyacente.
- El alimento aparece de forma aleatoria; no hay todavía estaciones ni biomas.
- La reproducción tiene una probabilidad fija y un coste energético.
- No hay gráficos, genética, inteligencia artificial ni simulación paralela.
- El historial de caza conserva las últimas 12 cacerías de la sesión actual; no se incluye en el archivo JSON de guardado.
- La simulación no pretende ser un modelo biológico real: sus reglas son deliberadamente sencillas para demostrar programación orientada a objetos y control de estado.

## Próximas ampliaciones posibles

1. Añadir tipos de terreno y recursos diferentes.
2. Dar atributos configurables a cada especie.
3. Incorporar reproducción con herencia de características.
4. Añadir historial de experimentos y comparación entre simulaciones.
5. Construir una interfaz gráfica sin cambiar el núcleo del dominio.
