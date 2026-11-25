# Surtidos en los catálogos público e interno

Al entrar a una marca, ambos catálogos muestran los surtidos activos de esa marca junto
a los productos individuales. Cada surtido incluye su imagen, presentaciones y productos
permitidos. Se ingresa la cantidad de presentaciones y las unidades enteras de cada producto.
La selección exige completar exactamente las unidades equivalentes multiplicadas por la cantidad.

Hay un botón por tarjeta: «Agregar al carrito» para el público y «Agregar a la venta» para
el usuario interno. Escribir cantidades no agrega automáticamente artículos. El catálogo
interno conserva el descuento en Nueva venta y avisa cuando una presentación ya está agregada.
Enter/flecha abajo avanza entre cantidades; flecha arriba o Shift+Enter retrocede.

## Carrito y pedidos públicos

El carrito público sigue separado del borrador de venta interno. Guarda los componentes
y cantidades en almacenamiento de la pestaña y los envía con el pedido. Para cambiar la
cantidad de un surtido se completa otra vez la composición desde su tarjeta; el carrito
permite eliminarlo sin alterar silenciosamente su composición.

Los pedidos no reservan inventario. Al registrarlos y al convertirlos en venta se validan
otra vez componentes permitidos, estados, costos compatibles, precio y existencias acumuladas.
La composición se conserva en `detalle_pedido_surtido`, con el nombre de cada producto
al solicitarlo. Los reintentos comparan también esa composición para no reutilizar una
clave de operación con cantidades distintas.

El catálogo público no expone costos promedio ni existencias exactas. Su endpoint es
`GET /api/CatalogoPublico/marcas/{idMarca}/surtidos?pagina=1&tamanoPagina=24`.

## Migración y pruebas

La migración `SurtidosPedidosCatalogo` crea únicamente la tabla de composición del pedido;
conserva los pedidos existentes. No se aplicó a la base del negocio.

Detener la API, ejecutar desde BACKEND y luego reiniciarla:

```powershell
dotnet ef database update --project API_SISTEMA --startup-project API_SISTEMA --context SistemaDbContext
```

Se amplió `tests/SurtidoVenta.Checks` con publicación por marca, privacidad de costos,
registro e idempotencia de pedidos, edición, conversión a venta y validaciones de cantidades.
`tests/Postgres.Checks` comprueba también la migración y ese recorrido en PostgreSQL temporal.
`tests/catalogSurtidos.test.ts` cubre selección, cantidades, disponibilidad, almacenamiento
del carrito y el contrato público. Está incluido en las pruebas habituales del frontend.
