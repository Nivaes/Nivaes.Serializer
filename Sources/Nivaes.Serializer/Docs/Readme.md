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

## Serialización dinámica

Para serializar un modelo cuyo tipo se conoce en tiempo de ejecución, regístralo una vez antes de usar las sobrecargas dinámicas. El generador proporciona el registro y el identificador del modelo:

```csharp
uint id = SerializerModel.GetIdClass<Person>();
SerializerModel.Register<Person>();

object value = new Person { Name = "Ada", Age = 36 };
ReadOnlySpan<byte> data = SerializerModel.Serialize(id, value);
Person? copy = (Person?)SerializerModel.Deserialize(id, data);
```

También puedes despachar por tipo en tiempo de ejecución usando `SerializerModel.Serialize(typeof(Person), value)` y `SerializerModel.Deserialize(typeof(Person), data)`. Para esta opción, el modelo también debe estar registrado. `GetIdClass<T>()` produce el identificador asociado al tipo en la compilación; úsalo para obtenerlo en lugar de definirlo manualmente.

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

