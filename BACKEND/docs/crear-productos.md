# Crear productos

POST /api/Productos/crear (JSON, administrador):

{
  "codigo_barra": "001234",
  "nombre": "Coca-Cola 2.5 L",
  "idMarca": 1,
  "stock_minimo": 12,
  "urlImagen": "https://ejemplo.com/imagen.jpg",
  "presentaciones": [
    { "id_presentacion": 1, "unidades_equivalentes": 1, "precio": 15.50 },
    { "id_presentacion": 2, "unidades_equivalentes": 6, "precio": 90 }
  ]
}

Archivo/foto: POST /api/Productos/crear/con-imagen, multipart/form-data:
codigo_barra, nombre, IdMarca, stock_minimo, Imagen,
presentaciones[0].id_presentacion, presentaciones[0].unidades_equivalentes,
presentaciones[0].precio, etc. No enviar URL y archivo simultáneamente.

Cantidades y stock mínimo: enteros, sin letras ni fracciones. Precios: mayores que cero,
hasta dos decimales. En JSON usar números (punto decimal). Los errores de enlace de
tipos se devuelven como HTTP 400 por ApiController.
Nombre hasta 200 caracteres, código hasta 100 (se conservan ceros iniciales),
de 1 a 100 presentaciones sin IDs repetidos. Marca, categoría y presentaciones activas.

La API asigna fecha, stock cero y costo desconocido (null). La categoría se obtiene de Marca; Productos no almacena id_categoria.
La operación devuelve 201, validaciones 400, duplicado de código 409 y fallos internos 500.
JSON con propiedades desconocidas se rechaza: el contrato antiguo con id_categoria,
impuesto o descripcion debe actualizarse.

Imágenes: JPEG/PNG/WebP hasta 5 MiB, convertidas a WebP en wwwroot/uploads/productos.
No se descargan enlaces externos. El endpoint anterior de reemplazo reutiliza esa validación.
Para URLs de imagen, ejecutar `docs/sql/producto-imagen.sql` en la base `SISTEMA`.
El modelo admite 2048 caracteres; cambiar el modelo no amplía automáticamente una columna existente.

Debe existir id_marca y su FK en SQL. Verificar que las columnas de nombre/código/imagen
admitan 200/100/2048 caracteres y precio sea decimal(18,2); no se modificó SQL.
Publicar /uploads/productos desde el backend y conservar archivos entre despliegues.

Pruebas: dotnet run --project tests/ProductoCrear.Checks
No se probaron altas en SQL. El cálculo de costo promedio y las alertas de stock son tareas separadas.

