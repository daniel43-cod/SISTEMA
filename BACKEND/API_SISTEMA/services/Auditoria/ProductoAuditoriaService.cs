using API_SISTEMA.models;

namespace API_SISTEMA.services.Auditoria;

public sealed class ProductoAuditoriaService(CatalogoAuditoriaService auditoria)
{
    public static Dictionary<string, object?> Datos(Productos producto) => new()
    {
        ["nombre"] = producto.nombre,
        ["codigoBarra"] = producto.codigo_barra,
        ["idMarca"] = producto.IdMarca,
        ["stockMinimo"] = producto.stock_minimo,
        ["imagen"] = producto.imagen
    };

    public static Dictionary<string, object?> DatosPresentacion(Producto_Presentacion presentacion) => new()
    {
        ["idProducto"] = presentacion.id_producto,
        ["idPresentacion"] = presentacion.IdPresentacion,
        ["precio"] = presentacion.precio,
        ["unidadesEquivalentes"] = presentacion.unidades_equivalentes,
        ["estado"] = presentacion.estado
    };

    public Task Registrar(Productos producto, Dictionary<string, object?>? anteriores, CancellationToken ct) =>
        auditoria.Agregar(anteriores is null ? "PRODUCTO_CREADO" : "PRODUCTO_EDITADO",
            "productos", producto.id_producto, anteriores, Datos(producto), ct);

    public Task RegistrarPresentacion(Producto_Presentacion presentacion,
        Dictionary<string, object?>? anteriores, CancellationToken ct) =>
        auditoria.Agregar(anteriores is null ? "PRODUCTO_PRESENTACION_CREADA" : "PRODUCTO_PRESENTACION_EDITADA",
        "producto_presentacion", presentacion.id_producto_presentacion,
        anteriores, DatosPresentacion(presentacion), ct);
}
