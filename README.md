# Nivaes.Serializer

Serializador binario para .NET que genera código de serialización y deserialización en tiempo de compilación mediante un generador de código fuente.

## Características

- API sencilla para serializar, deserializar y clonar modelos.
- Generación de código a partir de modelos anotados con `[SerializeModel]`.
- Compatibilidad con tipos primitivos, enumeraciones, arreglos, colecciones y otros modelos serializables.
- Admite modelos con referencias anidadas y circulares, así como tipos unión.
- Permite excluir propiedades con `[SerializeIgnore]`.

## Instalación

Agrega el paquete NuGet al proyecto:

```bash
dotnet add package Nivaes.Serializer
```

El proyecto debe usar un SDK de .NET compatible con el `TargetFramework` del paquete (`net11.0`).

## Uso rápido

Anota el modelo con `[SerializeModel]`. El generador crea la implementación necesaria durante la compilación:

```csharp
using Nivaes.Serializer;

var person = new Person { Name = "Ada", Age = 36 };

ReadOnlySpan<byte> data = SerializerModel.Serialize(person);
Person? copy = SerializerModel.Deserialize<Person>(data);

[SerializeModel]
public class Person
{
	public required string Name { get; set; }
	public int Age { get; set; }
}
```

`SerializerModel.Clone(person)` crea una copia del modelo usando la serialización generada.

## Selección de propiedades

Se serializan las propiedades públicas de instancia que tienen getter y setter públicos. Las propiedades heredadas también se incluyen. Para excluir una propiedad, usa `[SerializeIgnore]`:

```csharp
[SerializeModel]
public class Account
{
	public required string Name { get; set; }

	[SerializeIgnore]
	public string? TemporaryValue { get; set; }
}
```

Los tipos de las propiedades deben ser compatibles con el generador. Los tipos genéricos y anidados anotados no se generan actualmente.


## Integration

![CI](https://github.com/Nivaes/Nivaes.Serializer/workflows/CI/badge.svg) 

![Build Release](https://github.com/Nivaes/Nivaes.Serializer/workflows/Build%20Release/badge.svg)