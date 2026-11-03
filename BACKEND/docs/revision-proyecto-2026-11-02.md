# Revisión del proyecto — 2 de noviembre de 2026

## Alcance y límites

Revisión de estructura, arranque, dependencias, controladores, persistencia, servicios principales, documentación SQL, pruebas y conexión con el frontend vecino. Se consultaron el estado de Git y los cambios locales existentes sin revertirlos. No se modificó código funcional ni se ejecutaron scripts contra la base del negocio. Esta revisión permite orientarse en el sistema; no constituye una certificación de todos sus endpoints, del despliegue ni de la concurrencia real.

## Mapa del sistema

- Backend: API ASP.NET Core, C# y .NET 10. Controladores → servicios → SistemaDbContext → SQL Server mediante EF Core. DTOs separados de las entidades; servicios organizados principalmente por funcionalidad.
- Frontend vecino: `../Front-End.web/Front-End`, React 19, TypeScript y Vite, con PWA. Organización por funcionalidades, cliente HTTP común y navegación interna por estado. `/catalogo` abre el catálogo público; el resto entra al acceso del personal. La sesión interna se mantiene en memoria.
- Seguridad: JWT y BCrypt; administrador y vendedor para personal, CLIENTE para cuentas externas. Política de autenticación por defecto, comprobación de sesiones internas y versión de credenciales en cada petición. Permisos de usuario pueden sobrescribir los del rol.
- Datos: catálogo (categorías, marcas, productos, presentaciones y precios), inventario, clientes, proveedores, ventas, compras, pagos, gastos, caja, pedidos públicos, cuentas de clientes, conversaciones, sesiones y auditoría.
- Operación: Docker con compilación y publicación Release; Swagger en desarrollo. Vite hace proxy de `/api` y `/uploads`. Los cambios de esquema documentados usan scripts SQL explícitos, sin actualización automática al arrancar.

## Reglas que deben conservarse

- Un solo turno de caja abierto para todo el sistema; solo administradores lo abren y cierran. Iniciar sesión no abre caja. Las operaciones registran al usuario real.
- Ventas requieren caja, permiso, presentaciones activas, inventario suficiente y costo promedio positivo. Las unidades de una presentación se convierten a unidades base.
- El descuento es un importe por línea. El neto de cada línea debe superar estrictamente su costo; solo efectivo, con crédito y pago parcial o cero admitidos. El estado inactivo del cliente no bloquea la venta.
- La creación de ventas usa UUID y huella del contenido para idempotencia. Inventario, detalle, pago y movimiento se confirman transaccionalmente. Los abonos independientes de venta no tienen esa protección frente a reintentos inciertos.
- En compras, `detalle.precio` representa el total de la línea y `cantidad` las unidades base. Se actualiza el costo promedio ponderado. No reinterpretarlo como precio unitario.
- Saldo de caja: monto inicial + entradas − salidas de movimientos; sumar ventas aparte duplicaría importes.
- El pedido público no descuenta inventario al editarlo; la confirmación administrativa lo convierte en venta dentro de una transacción.
- El chat separa cuentas de clientes de usuarios internos, comprueba propiedad de conversaciones y conserva la pregunta si falla el proveedor. Las herramientas de catálogo son de lectura. Historial y rondas están acotados.
- La auditoría de sesiones usa UTC; un worker procesa vencimientos cada 30 segundos. Los reportes nuevos calculan el corte con hora de Guatemala.

## Validación realizada

Se ejecutó `dotnet build API_SISTEMA/API_SISTEMA.csproj --no-restore`: compilación correcta en Debug con advertencias.

| Proyecto de comprobaciones | Resultado |
| --- | --- |
| Ventas.Checks | 202 aprobadas, incluidas reglas de ventas, pedidos, pagos y reportes |
| OpenAIService.Checks | 12 aprobadas con HTTP simulado |
| CatalogoPublico.Checks | 12 aprobadas con base temporal |
| ConversacionListar.Checks | 18 aprobadas con base temporal |
| ProductoCrear.Checks | Correcto |
| CategoriaImagen.Checks | Correcto |
| MarcaImagen.Checks | Correcto |
| Auth.Checks | No compila: CS0200 en generador OpenAPI |
| ProductoConsulta.Checks | No compila: CS0200 en generador OpenAPI |

Las pruebas no usaron SQL Server del negocio ni llamadas reales al proveedor de IA. No se ejecutaron build, pruebas ni inspección visual del frontend. SQLite no valida los bloqueos ni los deadlocks de SQL Server.

## Hallazgos prioritarios

1. **Dependencias OpenAPI inconsistentes.** La API referencia Microsoft.OpenApi 2.4.1 y las dos suites que no compilan referencian 3.5.5. El generador produce asignaciones a `IOpenApiMediaType.Example` que el contrato resuelto considera de solo lectura. Alinear el conjunto de paquetes antes de actualizar versiones por separado. El build también reporta NU1903, gravedad alta, para 2.4.1; esta observación procede de la auditoría local de NuGet, sin verificación externa adicional.
2. **Publicación Release pendiente de validar.** ImageSharp 4.1.3 avisa de licencia ausente y de que Release la requiere. Docker publica en Release. No se ejecutó esa publicación, por lo que no se afirma que el contenedor construya correctamente. Algunos proyectos de pruebas de imágenes todavía usan 3.1.12 y ProductoCrear genera advertencias de conflicto de ensamblados.
3. **Fechas dependientes del servidor.** Ventas, compras, caja, pagos y movimientos usan `DateTime.Now`; el reporte nuevo usa explícitamente America/Guatemala. Si el servidor usa otra zona, el corte de reportes puede discrepar de las fechas guardadas. Docker no declara TZ. Definir una política común y probar el cruce de medianoche antes de desplegar en otra zona.
4. **Esquema real no verificado.** La documentación señala scripts necesarios para caja compartida, operaciones de venta, auditoría y sesiones, entre otros. No se comprobó cuáles están aplicados. El mapeo de EF no reemplaza su ejecución; verificar tablas, columnas e índices en una base de pruebas.
5. **Errores internos expuestos en rutas antiguas.** Por ejemplo, EmpresaController captura cualquier Exception y devuelve `ex.Message` con 400. Conviene extender el tratamiento seguro de errores ya presente en ventas y compras.
6. **Protección de cuentas externas por revisar.** Inicio de sesión y registro de clientes no tienen las políticas de límite de solicitudes que sí existen para el login interno. Tampoco hay política global de rate limiting en Program.cs. Revisar protección frente a intentos repetidos y uso intensivo del chat.
7. **Higiene del repositorio.** El archivo local se llama `gitignore`, sin punto. Hay numerosos archivos binarios, obj, .vs y temporales ya rastreados; añadir reglas no elimina los archivos existentes del índice. La limpieza requiere preservar los cambios de trabajo actuales.
8. **Cobertura de ejecución incompleta.** La solución solo incluye Auth.Checks y ProductoConsulta.Checks, ambos bloqueados por compilación. Las otras siete suites quedan fuera. En el frontend existen pruebas de compras, gastos, carrito público, autenticación de clientes y caché que no figuran en el comando npm test.

## Mantenimiento y estado funcional

- Program.cs concentra registros de servicios y repite algunos (Swagger, PermisoService y CatalogoPublicoService).
- SistemaDbContext concentra numerosos mapeos y repite llamadas a base.OnModelCreating; hay configuraciones separadas para auditoría y pedidos que permiten continuar una organización más clara.
- ConexionDbContext contiene una clase anidada de conexión SQL y ReporteService está vacío; revisar referencias antes de retirarlos. Los reportes implementados están en Services/Ventas/ReporteVentasIntervaloService.cs.
- La sección Configuración del frontend todavía cae en una pantalla de funcionalidad pendiente.
- Hay cambios locales en backend y frontend y archivos nuevos de reportes. La revisión corresponde al árbol de trabajo actual, no exclusivamente al último commit.
- La documentación de ventas deja certificación fiscal, anulaciones y devoluciones fuera del cambio descrito; no asumirlas implementadas por la existencia de un registro de venta.

## Orden sugerido de trabajo

Primero resolver las dependencias que bloquean pruebas y comprobar publicación Release. Después unificar fechas y validar esquema y concurrencia en SQL Server de pruebas. Continuar con errores seguros, límites de solicitudes externos y ejecución completa de suites. Dejar la reorganización estructural y la limpieza de Git para cambios separados y revisables.
