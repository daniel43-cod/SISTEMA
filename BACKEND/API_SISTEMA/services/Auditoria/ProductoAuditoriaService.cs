using API_SISTEMA.Models;

namespace API_SISTEMA.Services.Auditoria;

public sealed class ProductoAuditoriaService(CatalogoAuditoriaService auditoria)
{
    public static Dictionary<string, object?> Datos(API_SISTEMA.Models.Productos producto) => new()
    {
        ["nombre"] = producto.Nombre,
        ["codigoBarra"] = producto.CodigoBarra,
        ["idMarca"] = producto.IdMarca,
        ["stockMinimo"] = producto.StockMinimo,
        ["imagen"] = producto.Imagen
    };

    public static Dictionary<string, object?> DatosPresentacion(ProductoPresentacion presentacion) => new()
    {
        ["idProducto"] = presentacion.id_producto,
        ["idPresentacion"] = presentacion.IdPresentacion,
        ["precio"] = presentacion.precio,
        ["unidadesEquivalentes"] = presentacion.unidades_equivalentes,
        ["estado"] = presentacion.estado
    };

    public Task Registrar(API_SISTEMA.Models.Productos producto, Dictionary<string, object?>? anteriores, CancellationToken ct) =>
        auditoria.Agregar(anteriores is null ? "PRODUCTO_CREADO" : "PRODUCTO_EDITADO",
            "productos", producto.IdProducto, anteriores, Datos(producto), ct);

    public Task RegistrarPresentacion(ProductoPresentacion presentacion,
        Dictionary<string, object?>? anteriores, CancellationToken ct) =>
        auditoria.Agregar(anteriores is null ? "PRODUCTO_PRESENTACION_CREADA" : "PRODUCTO_PRESENTACION_EDITADA",
        "producto_presentacion", presentacion.id_producto_presentacion,
        anteriores, DatosPresentacion(presentacion), ct);
}
