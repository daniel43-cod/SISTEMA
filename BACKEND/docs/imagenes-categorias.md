# Imágenes de categorías

- POST /api/Categoria: JSON con nombre y urlImagen opcional (HTTPS).
- POST /api/Categoria/con-imagen: multipart/form-data con Nombre e Imagen. También acepta UrlImagen, pero no simultáneamente con un archivo.
- Ambos endpoints requieren ADMINISTRADOR.
- La respuesta incluye urlImagen; la base guarda solo la referencia.
- Archivos: JPEG/PNG/WebP, máximo 5 MiB, 8000 píxeles por lado y 32 megapíxeles. Se decodifican, orientan y convierten a WebP, hasta 1200 píxeles, sin metadatos personales.
- Carpeta: wwwroot/uploads/categorias. Debe persistir entre despliegues y tener copia de seguridad. En producción enrutar /uploads/categorias al backend junto con /api.
- Los enlaces externos no se descargan en la API.
- La cámara usa el selector del dispositivo: su disponibilidad depende del navegador. Fotos HEIC necesitan convertirse a JPEG/PNG/WebP.
- SQL opcional: docs/sql/categoria-url-imagen.sql. No se ejecutó automáticamente.
- Si falla la base antes de confirmar se limpia el archivo. Si falla la confirmación de la transacción, se conserva porque el resultado puede ser incierto. Una interrupción del proceso también puede dejar archivos sin referencia.
- Procesamiento: SixLabors.ImageSharp 3.1.12, sujeto a Six Labors Split License: https://github.com/SixLabors/ImageSharp/blob/v3.1.12/LICENSE

Verificaciones:
dotnet run --project tests/CategoriaImagen.Checks
npm test (en el frontend)

No se probaron inserciones en SQL ni la cámara de un teléfono real.
