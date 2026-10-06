# Imágenes al crear marcas

Solo administradores. Ejecutar primero `sql/marca-url-imagen.sql` en SISTEMA.
El script no se ejecuta automáticamente; las marcas existentes quedan sin imagen.

POST /api/Marcas/con-imagen acepta multipart/form-data:
- Nombre: nombre de la marca (hasta 100 caracteres).
- IdCategoria: ID de una categoría activa.
- Imagen: archivo JPEG, PNG o WebP opcional (hasta 5 MiB).
- UrlImagen: enlace HTTPS opcional (hasta 2048 caracteres), alternativo al archivo.

POST /api/Marcas conserva JSON y acepta UrlImagen opcional.
No enviar enlace y archivo simultáneamente. Sin ambos se crea una marca sin imagen.
Nombre duplicado devuelve 409; categoría/imagen inválida devuelve 400; creación correcta 201.

Los archivos se comprueban por su contenido, se limitan a 8000 píxeles por lado y
32 megapíxeles, se redimensionan a un máximo de 1200 por lado y se guardan como WebP
sin metadatos bajo wwwroot/uploads/marcas, con nombre generado por el servidor.
Si el alta falla antes de confirmar la transacción se limpia el archivo creado.
No se descargan URLs externas. Conservar uploads entre despliegues.

La respuesta de creación y el listado incluyen urlImagen (ruta local o enlace).
La edición sigue conservando la imagen existente.
