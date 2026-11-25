# Configuración de surtidos: backend

Los endpoints `/api/surtidos` requieren sesión interna y rol ADMINISTRADOR.
El frontend incluye «Crear surtido» junto a «Crear producto». Abre un formulario con
nombre, marca, buscador de productos, selección múltiple y presentaciones. La compatibilidad
de costos se comprueba al guardar; un rechazo conserva los datos para corregirlos.
El flujo de venta y sus validaciones se describen en [ventas-surtidos.md](ventas-surtidos.md).

Los surtidos se muestran como tarjetas dentro de su marca, en Productos. Incluyen
imagen y presentaciones con sus precios. La imagen opcional se guarda en `surtido.imagen`;
se admite enlace HTTPS o archivo mediante `POST /api/surtidos/con-imagen` (multipart).
Los archivos reutilizan el procesamiento de imágenes y la carpeta persistente de productos.
`GET /api/surtidos?idMarca=1` filtra exclusivamente por la marca del surtido.
La migración `ImagenSurtido` agrega la columna nullable y conserva los surtidos existentes.

## Crear

`POST /api/surtidos`, Content-Type `application/json`:

```json
{
  "nombre": "Super cola 3.3 surtido",
  "idMarca": 1,
  "idsProductos": [10, 11, 12],
  "presentaciones": [
    { "idPresentacion": 2, "unidadesEquivalentes": 6, "precio": 16.20 }
  ]
}
```

Los identificadores son ejemplos: deben corresponder a registros existentes.
La marca del surtido debe estar activa y pertenecer a una categoría activa.
Los componentes pueden pertenecer a distintas marcas, también activas.
Se reutilizan las presentaciones existentes, guardando sus unidades y precios en
`producto_presentacion` con `id_surtido`, dejando `id_producto` vacío.

Todos los productos necesitan costo promedio positivo y la misma parte entera del costo.
El precio mínimo de cada presentación es **unidades × mayor costo promedio**, elevado
al centavo suficiente. Ejemplo: 6 × Q2.70 = Q16.20; 6 × Q2.700001 requiere Q16.21.
Se acepta igualdad o mayor precio, con hasta dos decimales y el límite de importes de ventas.
No se permite repetir productos ni presentaciones dentro del mismo surtido.
Configurar no crea existencias ni descuenta inventario.

La creación de surtido, componentes, presentaciones y auditoría es una única transacción.
Responde `201` con el detalle y su ubicación. Validaciones: `400`; registro inexistente:
`404`; conflicto de concurrencia: `409`.

## Consultar

- `GET /api/surtidos?texto=cola&pagina=1&tamano=20`: listado paginado (máximo 100).
- `GET /api/surtidos/1`: componentes, costos actuales y presentaciones.

## Editar

`PUT /api/surtidos/1` admite el mismo cuerpo de creación, más `estado` (booleano) y
`quitarImagen` (booleano). Omitir imagen conserva la actual; un enlace HTTPS la reemplaza;
`quitarImagen: true` la retira. Para subir archivo se usa `PUT /api/surtidos/1/con-imagen`
con multipart y los mismos campos indexados de creación.

En Productos, «Editar» abre los datos precargados. Permite cambiar nombre, marca,
estado, productos permitidos, presentaciones, unidades, precios y la imagen.
El guardado vuelve a validar la compatibilidad de costos y el precio mínimo.
Las presentaciones existentes conservan su identificador; las retiradas se desactivan.
Los cambios de composición no eliminan detalles históricos de venta.
La actualización y auditoría se confirman juntas, sin modificar inventario.

El detalle devuelve `precioMinimoActual` por presentación. Si los costos actuales no son
compatibles, ese mínimo es `null`; consultar nunca modifica el estado configurado.
La venta revalida costos y composición, existencias y precio neto después de descuentos.
La configuración no sustituye esas validaciones.

## Migraciones

Se prepararon `AgregarSurtidos` y `MarcaSurtido`. Para aplicarlas:

```powershell
dotnet ef database update --project API_SISTEMA --startup-project API_SISTEMA --context SistemaDbContext
```

`MarcaSurtido` incorpora `id_marca` obligatorio y su clave foránea. No asigna una marca
inventada a surtidos existentes. Si se insertaron surtidos manualmente antes de esta etapa,
hay que definir su marca antes de aplicar esa migración; con la tabla vacía puede aplicarse directamente.

## Comprobaciones

```powershell
dotnet run --project tests/SurtidoCrear.Checks
dotnet run --project tests/SurtidoReglas.Checks
```

La primera usa SQLite temporal para comprobar persistencia, validaciones y reversión
si falla la auditoría; la segunda comprueba reglas de costos e importes sin base de datos.
