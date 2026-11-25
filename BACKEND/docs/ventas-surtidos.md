# Venta de surtidos

En Nueva venta se buscan los surtidos por nombre y presentación, junto con los productos
individuales. Al elegir una presentación, se abre un formulario con una cantidad entera
para cada producto permitido. Los campos vacíos equivalen a cero y no se envían como componentes.
No se permite agregar hasta completar exactamente la presentación: dos bandejas de seis
requieren doce unidades. La composición puede editarse después desde la misma venta.

## Registro

`GET /api/Venta/surtidos?pagina=1&tamanoPagina=100` permite consultar surtidos activos
con sus presentaciones, componentes y existencias. El campo `bloqueo` indica si el surtido
completo tiene costos incompatibles o marcas/categorías inactivas. No revela costos al vendedor.

`POST /api/Venta/crear` mantiene la idempotencia existente y admite líneas como:

```json
{
  "id_surtido": 1,
  "id_producto_presentacion": 10,
  "cantidad": 1,
  "descuento": 0,
  "componentes": [
    { "id_producto": 20, "cantidad": 3 },
    { "id_producto": 21, "cantidad": 3 }
  ]
}
```

Los identificadores son ilustrativos. Una línea identifica exclusivamente `id_producto`
o `id_surtido`. El servidor no acepta precios, costos ni ganancias proporcionados por el cliente.

La validación compara las partes enteras del costo promedio de **todos** los productos
permitidos, incluidos los no utilizados. Un cambio decimal dentro de la misma parte entera
no bloquea por compatibilidad. El importe neto se compara con el costo real de la composición;
puede igualarlo, pero nunca ser menor. Las reglas anteriores de productos individuales se conservan.

Se acumula el consumo por producto entre todas las líneas, tanto individuales como surtidas.
Las existencias solo se modifican después de validar todas las líneas y dentro de la transacción
de caja. Se guardan cantidad y costo promedio original de cada componente en `detalle_venta_surtido`.
La modificación de ventas restaura las cantidades históricas antes de validar la nueva composición.
Los detalles y comprobantes muestran el nombre del surtido; el detalle incluye su composición.

## Migración

`VentaSurtidos` permite `id_producto` nulo en `detalle_venta`, agrega `id_surtido` y exige
exactamente un propietario. Conserva las ventas individuales existentes. Para aplicarla,
detener la API y ejecutar desde BACKEND:

```powershell
dotnet ef database update --project API_SISTEMA --startup-project API_SISTEMA --context SistemaDbContext
```

Después, reiniciar la API. No se aplicó a la base del negocio durante esta implementación.

## Verificación

- `dotnet run --project tests/SurtidoVenta.Checks`: reglas y persistencia en SQLite temporal.
- `dotnet run --project tests/Ventas.Checks`: regresiones de ventas individuales y pedidos.
- `tests/Postgres.Checks`: migraciones, catálogo, restricción de propietario y concurrencia real.
  Las pruebas con conexión solo permiten la base temporal `sistema_checks` en `127.0.0.1:55438`.
- Frontend: `tests/saleSurtidos.test.ts` comprueba cantidades, composición, inventario compartido,
  descuentos, búsqueda y contratos de API; está incluido en el comando habitual de pruebas.

Estas pruebas cubren casos válidos, límites y rechazos identificados; no sustituyen la revisión
visual en los dispositivos donde se usará el formulario.
