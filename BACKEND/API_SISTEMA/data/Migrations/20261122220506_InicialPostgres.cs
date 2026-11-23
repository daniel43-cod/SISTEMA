using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace API_SISTEMA.Data.Migrations
{
    /// <inheritdoc />
    public partial class InicialPostgres : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "public");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:citext", ",,");

            migrationBuilder.CreateTable(
                name: "caja",
                schema: "public",
                columns: table => new
                {
                    id_caja = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nombre_caja = table.Column<string>(type: "text", nullable: true),
                    descripcion = table.Column<string>(type: "text", nullable: true),
                    estado = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_caja", x => x.id_caja);
                });

            migrationBuilder.CreateTable(
                name: "categoria",
                schema: "public",
                columns: table => new
                {
                    id_categoria = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nombre_categoria = table.Column<string>(type: "text", nullable: false),
                    estado = table.Column<bool>(type: "boolean", nullable: false),
                    fecha_creacion = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    url_imagen = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_categoria", x => x.id_categoria);
                });

            migrationBuilder.CreateTable(
                name: "cliente",
                schema: "public",
                columns: table => new
                {
                    id_cliente = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nombre = table.Column<string>(type: "text", nullable: false),
                    apellido = table.Column<string>(type: "text", nullable: true),
                    nit = table.Column<string>(type: "text", nullable: true),
                    dpi = table.Column<string>(type: "text", nullable: true),
                    telefono = table.Column<string>(type: "text", nullable: true),
                    correo_electronico = table.Column<string>(type: "text", nullable: true),
                    direccion = table.Column<string>(type: "text", nullable: true),
                    fecha_Creacion = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    limite_Credito = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    estado = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cliente", x => x.id_cliente);
                });

            migrationBuilder.CreateTable(
                name: "cuenta_cliente",
                schema: "public",
                columns: table => new
                {
                    id_cuenta_cliente = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    correo_electronico = table.Column<string>(type: "text", nullable: false),
                    password_hash = table.Column<string>(type: "text", nullable: false),
                    estado = table.Column<bool>(type: "boolean", nullable: false),
                    fecha_creacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cuenta_cliente", x => x.id_cuenta_cliente);
                });

            migrationBuilder.CreateTable(
                name: "empresa",
                schema: "public",
                columns: table => new
                {
                    id_empresa = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nombre_empresa = table.Column<string>(type: "text", nullable: false),
                    nit = table.Column<string>(type: "text", nullable: true),
                    ubicacion = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    direccion = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    telefonos = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    razon_social = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    nombre_comercial = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    afiliacion_iva = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    codigo_establecimiento = table.Column<int>(type: "integer", nullable: true),
                    correo_emisor = table.Column<string>(type: "character varying(254)", maxLength: 254, nullable: true),
                    codigo_postal = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    municipio = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    departamento = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    pais = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_empresa", x => x.id_empresa);
                });

            migrationBuilder.CreateTable(
                name: "estado_compra",
                schema: "public",
                columns: table => new
                {
                    id_estado_compra = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nombre_estado_compra = table.Column<string>(type: "text", nullable: false),
                    descripcion = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_estado_compra", x => x.id_estado_compra);
                });

            migrationBuilder.CreateTable(
                name: "estado_venta",
                schema: "public",
                columns: table => new
                {
                    id_estado_venta = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    descripcion = table.Column<string>(type: "text", nullable: true),
                    estado = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_estado_venta", x => x.id_estado_venta);
                });

            migrationBuilder.CreateTable(
                name: "presentaciones",
                schema: "public",
                columns: table => new
                {
                    id_presentacion = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    descripcion = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    estado = table.Column<bool>(type: "boolean", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_presentaciones", x => x.id_presentacion);
                });

            migrationBuilder.CreateTable(
                name: "proveedores",
                schema: "public",
                columns: table => new
                {
                    id_proveedor = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nombre = table.Column<string>(type: "text", nullable: false),
                    telefono = table.Column<string>(type: "text", nullable: false),
                    correo = table.Column<string>(type: "text", nullable: true),
                    direccion = table.Column<string>(type: "text", nullable: true),
                    nit = table.Column<string>(type: "text", nullable: true),
                    fecha_creacion = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_proveedores", x => x.id_proveedor);
                });

            migrationBuilder.CreateTable(
                name: "rol",
                schema: "public",
                columns: table => new
                {
                    id_rol = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nombre = table.Column<string>(type: "text", nullable: false),
                    descripcion = table.Column<string>(type: "text", nullable: false),
                    estado = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_rol", x => x.id_rol);
                });

            migrationBuilder.CreateTable(
                name: "tabla_permisos",
                schema: "public",
                columns: table => new
                {
                    id_permiso = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nombre = table.Column<string>(type: "text", nullable: false),
                    descripcion = table.Column<string>(type: "text", nullable: false),
                    estado = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tabla_permisos", x => x.id_permiso);
                });

            migrationBuilder.CreateTable(
                name: "tipo_cliente",
                schema: "public",
                columns: table => new
                {
                    id_tipo_cliente = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    descripcion = table.Column<string>(type: "text", nullable: true),
                    estado = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tipo_cliente", x => x.id_tipo_cliente);
                });

            migrationBuilder.CreateTable(
                name: "tipo_movimiento_caja",
                schema: "public",
                columns: table => new
                {
                    id_tipo_movimiento = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nombre_movimiento = table.Column<string>(type: "text", nullable: false),
                    naturaleza = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tipo_movimiento_caja", x => x.id_tipo_movimiento);
                });

            migrationBuilder.CreateTable(
                name: "whatsapp_webhook_evento",
                schema: "public",
                columns: table => new
                {
                    huella = table.Column<string>(type: "character varying(64)", unicode: false, maxLength: 64, nullable: false),
                    cuenta = table.Column<string>(type: "character varying(30)", unicode: false, maxLength: 30, nullable: false),
                    remitente = table.Column<string>(type: "character varying(30)", unicode: false, maxLength: 30, nullable: false),
                    message_id = table.Column<string>(type: "character varying(512)", unicode: false, maxLength: 512, nullable: false),
                    estado = table.Column<string>(type: "character varying(15)", unicode: false, maxLength: 15, nullable: false),
                    timestamp_meta = table.Column<long>(type: "bigint", nullable: false),
                    codigo_meta = table.Column<int>(type: "integer", nullable: true),
                    recibido_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_whatsapp_webhook_evento", x => x.huella);
                });

            migrationBuilder.CreateTable(
                name: "zona_entrega",
                schema: "public",
                columns: table => new
                {
                    id_zona_entrega = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    minimo_compra = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    estado = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_zona_entrega", x => x.id_zona_entrega);
                    table.UniqueConstraint("UQ_zona_entrega_nombre", x => x.nombre);
                    table.CheckConstraint("CK_zona_entrega_minimo", "minimo_compra > 0");
                    table.CheckConstraint("CK_zona_entrega_nombre", "length(btrim(nombre)) > 0");
                });

            migrationBuilder.CreateTable(
                name: "marca",
                schema: "public",
                columns: table => new
                {
                    id_marca = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ruta_imagen = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    estado = table.Column<bool>(type: "boolean", nullable: false),
                    id_categoria = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_marca", x => x.id_marca);
                    table.ForeignKey(
                        name: "FK_marca_categoria_id_categoria",
                        column: x => x.id_categoria,
                        principalSchema: "public",
                        principalTable: "categoria",
                        principalColumn: "id_categoria",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "usuario",
                schema: "public",
                columns: table => new
                {
                    id_usuario = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_rol = table.Column<int>(type: "integer", nullable: false),
                    nombre = table.Column<string>(type: "text", nullable: false),
                    apellido = table.Column<string>(type: "text", nullable: false),
                    usuario = table.Column<string>(type: "citext", nullable: false),
                    correo = table.Column<string>(type: "citext", nullable: true),
                    telefono = table.Column<string>(type: "text", nullable: false),
                    password = table.Column<string>(type: "text", nullable: false),
                    estado = table.Column<bool>(type: "boolean", nullable: false),
                    fecha_Creacion = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_usuario", x => x.id_usuario);
                    table.ForeignKey(
                        name: "FK_usuario_rol_id_rol",
                        column: x => x.id_rol,
                        principalSchema: "public",
                        principalTable: "rol",
                        principalColumn: "id_rol",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "rol_permiso",
                schema: "public",
                columns: table => new
                {
                    id_rol_permiso = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_permiso = table.Column<int>(type: "integer", nullable: false),
                    id_rol = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_rol_permiso", x => x.id_rol_permiso);
                    table.ForeignKey(
                        name: "FK_rol_permiso_rol_id_rol",
                        column: x => x.id_rol,
                        principalSchema: "public",
                        principalTable: "rol",
                        principalColumn: "id_rol",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_rol_permiso_tabla_permisos_id_permiso",
                        column: x => x.id_permiso,
                        principalSchema: "public",
                        principalTable: "tabla_permisos",
                        principalColumn: "id_permiso",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "productos",
                schema: "public",
                columns: table => new
                {
                    id_producto = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    codigo_barra = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    nombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    id_marca = table.Column<int>(type: "integer", nullable: false),
                    precio_promedio_compra = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: true),
                    stock = table.Column<int>(type: "integer", nullable: true),
                    stock_minimo = table.Column<int>(type: "integer", nullable: true),
                    imagen = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    costo_unitario = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    impuesto = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    fecha_creacion = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_productos", x => x.id_producto);
                    table.CheckConstraint("CK_productos_stock_minimo", "stock_minimo >= 0");
                    table.ForeignKey(
                        name: "FK_productos_marca_id_marca",
                        column: x => x.id_marca,
                        principalSchema: "public",
                        principalTable: "marca",
                        principalColumn: "id_marca",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "auditoria_evento",
                schema: "public",
                columns: table => new
                {
                    id_auditoria = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    fecha_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    id_usuario = table.Column<int>(type: "integer", nullable: true),
                    usuario_responsable = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    identificador_intentado = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    accion = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    entidad = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    id_registro = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    resultado = table.Column<string>(type: "varchar(12)", maxLength: 12, nullable: false),
                    origen = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false, defaultValue: "API"),
                    motivo = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    trace_id = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: true),
                    direccion_ip = table.Column<string>(type: "varchar(45)", maxLength: 45, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_auditoria_evento", x => x.id_auditoria);
                    table.CheckConstraint("CK_auditoria_evento_accion", "length(btrim(accion)) > 0");
                    table.CheckConstraint("CK_auditoria_evento_origen", "origen IN ('API', 'SISTEMA', 'SQL')");
                    table.CheckConstraint("CK_auditoria_evento_registro_entidad", "id_registro IS NULL OR (entidad IS NOT NULL AND length(btrim(entidad)) > 0)");
                    table.CheckConstraint("CK_auditoria_evento_resultado", "resultado IN ('EXITOSO', 'RECHAZADO', 'FALLIDO')");
                    table.ForeignKey(
                        name: "FK_auditoria_evento_usuario",
                        column: x => x.id_usuario,
                        principalSchema: "public",
                        principalTable: "usuario",
                        principalColumn: "id_usuario");
                });

            migrationBuilder.CreateTable(
                name: "conversacion",
                schema: "public",
                columns: table => new
                {
                    id_conversacion = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_usuario = table.Column<int>(type: "integer", nullable: true),
                    id_cuenta_cliente = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_conversacion", x => x.id_conversacion);
                    table.ForeignKey(
                        name: "FK_conversacion_cuenta_cliente_id_cuenta_cliente",
                        column: x => x.id_cuenta_cliente,
                        principalSchema: "public",
                        principalTable: "cuenta_cliente",
                        principalColumn: "id_cuenta_cliente",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_conversacion_usuario_id_usuario",
                        column: x => x.id_usuario,
                        principalSchema: "public",
                        principalTable: "usuario",
                        principalColumn: "id_usuario",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "sesion_caja",
                schema: "public",
                columns: table => new
                {
                    id_sesion_caja = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_caja = table.Column<int>(type: "integer", nullable: false),
                    id_usuario_apertura = table.Column<int>(type: "integer", nullable: false),
                    id_usuario_cierre = table.Column<int>(type: "integer", nullable: true),
                    fecha_apertura = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    monto_inicial = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: true),
                    monto_esperado = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: true),
                    monto_contado = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: true),
                    diferencia = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: true),
                    observacion_apertura = table.Column<string>(type: "text", nullable: false),
                    fecha_cierre = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    observacion_cierre = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sesion_caja", x => x.id_sesion_caja);
                    table.ForeignKey(
                        name: "FK_sesion_caja_caja_id_caja",
                        column: x => x.id_caja,
                        principalSchema: "public",
                        principalTable: "caja",
                        principalColumn: "id_caja",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_sesion_caja_usuario_id_usuario_apertura",
                        column: x => x.id_usuario_apertura,
                        principalSchema: "public",
                        principalTable: "usuario",
                        principalColumn: "id_usuario",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_sesion_caja_usuario_id_usuario_cierre",
                        column: x => x.id_usuario_cierre,
                        principalSchema: "public",
                        principalTable: "usuario",
                        principalColumn: "id_usuario",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "sesiones_usuario",
                schema: "public",
                columns: table => new
                {
                    id_sesion = table.Column<Guid>(type: "uuid", nullable: false),
                    id_usuario = table.Column<int>(type: "integer", nullable: false),
                    token_hash = table.Column<byte[]>(type: "bytea", nullable: false),
                    fecha_creacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    fecha_vencimiento = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    fecha_revocacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    motivo_cierre = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sesiones_usuario", x => x.id_sesion);
                    table.CheckConstraint("CK_sesiones_usuario_motivo_cierre", "motivo_cierre IN ('VOLUNTARIO', 'EXPIRACION')");
                    table.ForeignKey(
                        name: "FK_sesiones_usuario_usuario_id_usuario",
                        column: x => x.id_usuario,
                        principalSchema: "public",
                        principalTable: "usuario",
                        principalColumn: "id_usuario",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "usuario_permiso",
                schema: "public",
                columns: table => new
                {
                    id_usuario_permiso = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_usuario = table.Column<int>(type: "integer", nullable: false),
                    id_permiso = table.Column<int>(type: "integer", nullable: false),
                    permitido = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_usuario_permiso", x => x.id_usuario_permiso);
                    table.ForeignKey(
                        name: "FK_usuario_permiso_tabla_permisos_id_permiso",
                        column: x => x.id_permiso,
                        principalSchema: "public",
                        principalTable: "tabla_permisos",
                        principalColumn: "id_permiso",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_usuario_permiso_usuario_id_usuario",
                        column: x => x.id_usuario,
                        principalSchema: "public",
                        principalTable: "usuario",
                        principalColumn: "id_usuario",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "inventario_Movimientos",
                schema: "public",
                columns: table => new
                {
                    id_movimiento = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_producto = table.Column<int>(type: "integer", nullable: false),
                    id_usuario = table.Column<int>(type: "integer", nullable: false),
                    tipo_movimiento = table.Column<string>(type: "text", nullable: false),
                    cantidad = table.Column<string>(type: "text", nullable: false),
                    motivo = table.Column<string>(type: "text", nullable: false),
                    fecha_mocimiento = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_inventario_Movimientos", x => x.id_movimiento);
                    table.ForeignKey(
                        name: "FK_inventario_Movimientos_productos_id_producto",
                        column: x => x.id_producto,
                        principalSchema: "public",
                        principalTable: "productos",
                        principalColumn: "id_producto",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_inventario_Movimientos_usuario_id_usuario",
                        column: x => x.id_usuario,
                        principalSchema: "public",
                        principalTable: "usuario",
                        principalColumn: "id_usuario",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "producto_precio",
                schema: "public",
                columns: table => new
                {
                    id_producto_precio = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_producto = table.Column<int>(type: "integer", nullable: false),
                    id_tipo_cliente = table.Column<int>(type: "integer", nullable: false),
                    precio = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    estado = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_producto_precio", x => x.id_producto_precio);
                    table.ForeignKey(
                        name: "FK_producto_precio_productos_id_producto",
                        column: x => x.id_producto,
                        principalSchema: "public",
                        principalTable: "productos",
                        principalColumn: "id_producto",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_producto_precio_tipo_cliente_id_tipo_cliente",
                        column: x => x.id_tipo_cliente,
                        principalSchema: "public",
                        principalTable: "tipo_cliente",
                        principalColumn: "id_tipo_cliente",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "producto_presentacion",
                schema: "public",
                columns: table => new
                {
                    id_producto_presentacion = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_producto = table.Column<int>(type: "integer", nullable: false),
                    unidades_equivalentes = table.Column<int>(type: "integer", nullable: false),
                    precio = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    estado = table.Column<bool>(type: "boolean", nullable: false),
                    id_presentacion = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_producto_presentacion", x => x.id_producto_presentacion);
                    table.CheckConstraint("CK_producto_presentaciones_unidades", "unidades_equivalentes > 0");
                    table.ForeignKey(
                        name: "FK_producto_presentacion_presentaciones_id_presentacion",
                        column: x => x.id_presentacion,
                        principalSchema: "public",
                        principalTable: "presentaciones",
                        principalColumn: "id_presentacion",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_producto_presentacion_productos_id_producto",
                        column: x => x.id_producto,
                        principalSchema: "public",
                        principalTable: "productos",
                        principalColumn: "id_producto",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "auditoria_evento_detalle",
                schema: "public",
                columns: table => new
                {
                    id_detalle = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_auditoria = table.Column<long>(type: "bigint", nullable: false),
                    campo = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    valor_anterior = table.Column<string>(type: "text", nullable: true),
                    valor_nuevo = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_auditoria_evento_detalle", x => x.id_detalle);
                    table.UniqueConstraint("UQ_auditoria_detalle_campo", x => new { x.id_auditoria, x.campo });
                    table.CheckConstraint("CK_auditoria_detalle_campo", "length(btrim(campo)) > 0");
                    table.ForeignKey(
                        name: "FK_auditoria_detalle_evento",
                        column: x => x.id_auditoria,
                        principalSchema: "public",
                        principalTable: "auditoria_evento",
                        principalColumn: "id_auditoria");
                });

            migrationBuilder.CreateTable(
                name: "mensaje",
                schema: "public",
                columns: table => new
                {
                    id_mensaje = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_conversacion = table.Column<int>(type: "integer", nullable: false),
                    mensaje = table.Column<string>(type: "text", nullable: false),
                    respuesta = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_mensaje", x => x.id_mensaje);
                    table.ForeignKey(
                        name: "FK_mensaje_conversacion_id_conversacion",
                        column: x => x.id_conversacion,
                        principalSchema: "public",
                        principalTable: "conversacion",
                        principalColumn: "id_conversacion",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "gastos",
                schema: "public",
                columns: table => new
                {
                    id_gastos = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_sesion_caja = table.Column<int>(type: "integer", nullable: false),
                    id_usuario = table.Column<int>(type: "integer", nullable: false),
                    descripcion = table.Column<string>(type: "text", nullable: false),
                    monto = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    fecha = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    observacion = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_gastos", x => x.id_gastos);
                    table.ForeignKey(
                        name: "FK_gastos_sesion_caja_id_sesion_caja",
                        column: x => x.id_sesion_caja,
                        principalSchema: "public",
                        principalTable: "sesion_caja",
                        principalColumn: "id_sesion_caja",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_gastos_usuario_id_usuario",
                        column: x => x.id_usuario,
                        principalSchema: "public",
                        principalTable: "usuario",
                        principalColumn: "id_usuario",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "registro_compras",
                schema: "public",
                columns: table => new
                {
                    id_compra = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_proveedor = table.Column<int>(type: "integer", nullable: false),
                    id_usuario = table.Column<int>(type: "integer", nullable: false),
                    id_sesion_caja = table.Column<int>(type: "integer", nullable: true),
                    id_estado_compra = table.Column<int>(type: "integer", nullable: false),
                    fecha_ingreso = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    total_compra = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    saldo_pendiente = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    observacion = table.Column<string>(type: "character varying(100)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_registro_compras", x => x.id_compra);
                    table.ForeignKey(
                        name: "FK_registro_compras_estado_compra_id_estado_compra",
                        column: x => x.id_estado_compra,
                        principalSchema: "public",
                        principalTable: "estado_compra",
                        principalColumn: "id_estado_compra",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_registro_compras_proveedores_id_proveedor",
                        column: x => x.id_proveedor,
                        principalSchema: "public",
                        principalTable: "proveedores",
                        principalColumn: "id_proveedor",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_registro_compras_sesion_caja_id_sesion_caja",
                        column: x => x.id_sesion_caja,
                        principalSchema: "public",
                        principalTable: "sesion_caja",
                        principalColumn: "id_sesion_caja",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_registro_compras_usuario_id_usuario",
                        column: x => x.id_usuario,
                        principalSchema: "public",
                        principalTable: "usuario",
                        principalColumn: "id_usuario",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ventas",
                schema: "public",
                columns: table => new
                {
                    id_ventas = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    subtotal = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    descuento = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    impuesto = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: false),
                    total = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    fecha_venta = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    id_cliente = table.Column<int>(type: "integer", nullable: false),
                    tipo_identificacion_receptor = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                    identificacion_receptor = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    nombre_receptor = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    id_usuario = table.Column<int>(type: "integer", nullable: false),
                    ganancia_total = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: false),
                    origen = table.Column<string>(type: "text", nullable: false),
                    id_estado_venta = table.Column<int>(type: "integer", nullable: false),
                    id_sesion_caja = table.Column<int>(type: "integer", nullable: false),
                    monto_pagado = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    saldo_pendiente = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    observacion = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ventas", x => x.id_ventas);
                    table.ForeignKey(
                        name: "FK_ventas_cliente_id_cliente",
                        column: x => x.id_cliente,
                        principalSchema: "public",
                        principalTable: "cliente",
                        principalColumn: "id_cliente",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ventas_estado_venta_id_estado_venta",
                        column: x => x.id_estado_venta,
                        principalSchema: "public",
                        principalTable: "estado_venta",
                        principalColumn: "id_estado_venta",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ventas_sesion_caja_id_sesion_caja",
                        column: x => x.id_sesion_caja,
                        principalSchema: "public",
                        principalTable: "sesion_caja",
                        principalColumn: "id_sesion_caja",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ventas_usuario_id_usuario",
                        column: x => x.id_usuario,
                        principalSchema: "public",
                        principalTable: "usuario",
                        principalColumn: "id_usuario",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "detalle_compra",
                schema: "public",
                columns: table => new
                {
                    id_detalle_compra = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_registro_compra = table.Column<int>(type: "integer", nullable: false),
                    subtotal = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    id_producto = table.Column<int>(type: "integer", nullable: false),
                    cantidad = table.Column<int>(type: "integer", nullable: false),
                    precio = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_detalle_compra", x => x.id_detalle_compra);
                    table.ForeignKey(
                        name: "FK_detalle_compra_productos_id_producto",
                        column: x => x.id_producto,
                        principalSchema: "public",
                        principalTable: "productos",
                        principalColumn: "id_producto",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_detalle_compra_registro_compras_id_registro_compra",
                        column: x => x.id_registro_compra,
                        principalSchema: "public",
                        principalTable: "registro_compras",
                        principalColumn: "id_compra",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "pagos_compra",
                schema: "public",
                columns: table => new
                {
                    id_pagos_compra = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_operacion = table.Column<Guid>(type: "uuid", nullable: true),
                    id_compra = table.Column<int>(type: "integer", nullable: false),
                    id_usuario = table.Column<int>(type: "integer", nullable: false),
                    observacion = table.Column<string>(type: "text", nullable: false),
                    fecha_pago = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    monto = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    id_sesion_caja = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pagos_compra", x => x.id_pagos_compra);
                    table.ForeignKey(
                        name: "FK_pagos_compra_registro_compras_id_compra",
                        column: x => x.id_compra,
                        principalSchema: "public",
                        principalTable: "registro_compras",
                        principalColumn: "id_compra",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_pagos_compra_sesion_caja_id_sesion_caja",
                        column: x => x.id_sesion_caja,
                        principalSchema: "public",
                        principalTable: "sesion_caja",
                        principalColumn: "id_sesion_caja",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_pagos_compra_usuario_id_usuario",
                        column: x => x.id_usuario,
                        principalSchema: "public",
                        principalTable: "usuario",
                        principalColumn: "id_usuario",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "detalle_venta",
                schema: "public",
                columns: table => new
                {
                    id_detalle_venta = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_venta = table.Column<int>(type: "integer", nullable: false),
                    id_producto = table.Column<int>(type: "integer", nullable: false),
                    cantidad = table.Column<int>(type: "integer", nullable: false),
                    precio = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    descuento = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    subtotal = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    impuesto = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: true),
                    id_producto_presentacion = table.Column<int>(type: "integer", nullable: false),
                    unidades_descontadas = table.Column<int>(type: "integer", nullable: false),
                    costo_unitario_venta = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: true),
                    ganancia = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_detalle_venta", x => x.id_detalle_venta);
                    table.ForeignKey(
                        name: "FK_detalle_venta_producto_presentacion_id_producto_presentacion",
                        column: x => x.id_producto_presentacion,
                        principalSchema: "public",
                        principalTable: "producto_presentacion",
                        principalColumn: "id_producto_presentacion",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_detalle_venta_productos_id_producto",
                        column: x => x.id_producto,
                        principalSchema: "public",
                        principalTable: "productos",
                        principalColumn: "id_producto",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_detalle_venta_ventas_id_venta",
                        column: x => x.id_venta,
                        principalSchema: "public",
                        principalTable: "ventas",
                        principalColumn: "id_ventas",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "documento_fel",
                schema: "public",
                columns: table => new
                {
                    id_documento_fel = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_venta = table.Column<int>(type: "integer", nullable: false),
                    id_usuario = table.Column<int>(type: "integer", nullable: false),
                    ambiente = table.Column<string>(type: "character varying(10)", unicode: false, maxLength: 10, nullable: false),
                    tipo_dte = table.Column<string>(type: "character varying(4)", unicode: false, maxLength: 4, nullable: false),
                    estado = table.Column<string>(type: "character varying(15)", unicode: false, maxLength: 15, nullable: false),
                    identificador = table.Column<Guid>(type: "uuid", nullable: false),
                    uuid = table.Column<string>(type: "character varying(36)", unicode: false, maxLength: 36, nullable: true),
                    serie = table.Column<string>(type: "character varying(20)", unicode: false, maxLength: 20, nullable: true),
                    numero = table.Column<string>(type: "character varying(30)", unicode: false, maxLength: 30, nullable: true),
                    fecha_creacion = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    fecha_emision = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    fecha_certificacion = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    xml_enviado = table.Column<string>(type: "text", nullable: false),
                    xml_certificado = table.Column<string>(type: "text", nullable: true),
                    respuesta = table.Column<string>(type: "text", nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_documento_fel", x => x.id_documento_fel);
                    table.CheckConstraint("CK_documento_fel_ambiente", "ambiente IN ('PRUEBAS', 'PRODUCCION')");
                    table.CheckConstraint("CK_documento_fel_certificada", "estado <> 'CERTIFICADA' OR (uuid IS NOT NULL AND serie IS NOT NULL AND numero IS NOT NULL AND xml_certificado IS NOT NULL AND fecha_certificacion IS NOT NULL)");
                    table.CheckConstraint("CK_documento_fel_estado", "estado IN ('PENDIENTE', 'EN_PROCESO', 'CERTIFICADA', 'RECHAZADA', 'POR_CONFIRMAR')");
                    table.CheckConstraint("CK_documento_fel_tipo", "tipo_dte = 'FACT'");
                    table.ForeignKey(
                        name: "FK_documento_fel_usuario_id_usuario",
                        column: x => x.id_usuario,
                        principalSchema: "public",
                        principalTable: "usuario",
                        principalColumn: "id_usuario");
                    table.ForeignKey(
                        name: "FK_documento_fel_ventas_id_venta",
                        column: x => x.id_venta,
                        principalSchema: "public",
                        principalTable: "ventas",
                        principalColumn: "id_ventas");
                });

            migrationBuilder.CreateTable(
                name: "pagos",
                schema: "public",
                columns: table => new
                {
                    id_pago = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_operacion = table.Column<Guid>(type: "uuid", nullable: true),
                    id_venta = table.Column<int>(type: "integer", nullable: false),
                    id_usuario = table.Column<int>(type: "integer", nullable: false),
                    monto = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    metodo_pago = table.Column<string>(type: "text", nullable: false),
                    observacion = table.Column<string>(type: "text", nullable: true),
                    fecha_pago = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    id_sesion_caja = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pagos", x => x.id_pago);
                    table.ForeignKey(
                        name: "FK_pagos_sesion_caja_id_sesion_caja",
                        column: x => x.id_sesion_caja,
                        principalSchema: "public",
                        principalTable: "sesion_caja",
                        principalColumn: "id_sesion_caja",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_pagos_usuario_id_usuario",
                        column: x => x.id_usuario,
                        principalSchema: "public",
                        principalTable: "usuario",
                        principalColumn: "id_usuario",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_pagos_ventas_id_venta",
                        column: x => x.id_venta,
                        principalSchema: "public",
                        principalTable: "ventas",
                        principalColumn: "id_ventas",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "pedido_catalogo",
                schema: "public",
                columns: table => new
                {
                    id_pedido = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_operacion = table.Column<Guid>(type: "uuid", nullable: false),
                    nombre_cliente = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    telefono = table.Column<string>(type: "character varying(20)", unicode: false, maxLength: 20, nullable: false),
                    direccion = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    referencias = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    observacion = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    id_zona_entrega = table.Column<int>(type: "integer", nullable: false),
                    fecha_utc = table.Column<DateTime>(type: "timestamp(3) with time zone", precision: 3, nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    total = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    estado = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)0),
                    id_venta = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pedido_catalogo", x => x.id_pedido);
                    table.UniqueConstraint("UQ_pedido_catalogo_operacion", x => x.id_operacion);
                    table.CheckConstraint("CK_pedido_catalogo_contacto", "length(btrim(nombre_cliente)) > 0 AND length(btrim(telefono)) > 0 AND length(btrim(direccion)) > 0");
                    table.CheckConstraint("CK_pedido_catalogo_conversion", "(estado = 4 AND id_venta IS NOT NULL) OR (estado <> 4 AND id_venta IS NULL)");
                    table.CheckConstraint("CK_pedido_catalogo_estado", "estado IN (0, 1, 2, 3, 4)");
                    table.CheckConstraint("CK_pedido_catalogo_total", "total > 0");
                    table.ForeignKey(
                        name: "FK_pedido_catalogo_venta",
                        column: x => x.id_venta,
                        principalSchema: "public",
                        principalTable: "ventas",
                        principalColumn: "id_ventas");
                    table.ForeignKey(
                        name: "FK_pedido_catalogo_zona",
                        column: x => x.id_zona_entrega,
                        principalSchema: "public",
                        principalTable: "zona_entrega",
                        principalColumn: "id_zona_entrega");
                });

            migrationBuilder.CreateTable(
                name: "venta_operacion",
                schema: "public",
                columns: table => new
                {
                    id_operacion = table.Column<Guid>(type: "uuid", nullable: false),
                    id_usuario = table.Column<int>(type: "integer", nullable: false),
                    huella = table.Column<string>(type: "character varying(64)", unicode: false, maxLength: 64, nullable: false),
                    id_venta = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_venta_operacion", x => x.id_operacion);
                    table.ForeignKey(
                        name: "FK_venta_operacion_ventas_id_venta",
                        column: x => x.id_venta,
                        principalSchema: "public",
                        principalTable: "ventas",
                        principalColumn: "id_ventas",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "whatsapp_envio",
                schema: "public",
                columns: table => new
                {
                    id_envio = table.Column<Guid>(type: "uuid", nullable: false),
                    id_venta = table.Column<int>(type: "integer", nullable: false),
                    id_empresa = table.Column<int>(type: "integer", nullable: false),
                    id_usuario = table.Column<int>(type: "integer", nullable: false),
                    telefono = table.Column<string>(type: "character varying(15)", unicode: false, maxLength: 15, nullable: false),
                    plantilla = table.Column<string>(type: "character varying(512)", unicode: false, maxLength: 512, nullable: false),
                    idioma = table.Column<string>(type: "character varying(10)", unicode: false, maxLength: 10, nullable: false),
                    remitente = table.Column<string>(type: "character varying(30)", unicode: false, maxLength: 30, nullable: false),
                    creado_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_whatsapp_envio", x => x.id_envio);
                    table.ForeignKey(
                        name: "FK_whatsapp_envio_empresa_id_empresa",
                        column: x => x.id_empresa,
                        principalSchema: "public",
                        principalTable: "empresa",
                        principalColumn: "id_empresa",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_whatsapp_envio_usuario_id_usuario",
                        column: x => x.id_usuario,
                        principalSchema: "public",
                        principalTable: "usuario",
                        principalColumn: "id_usuario",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_whatsapp_envio_ventas_id_venta",
                        column: x => x.id_venta,
                        principalSchema: "public",
                        principalTable: "ventas",
                        principalColumn: "id_ventas",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "documento_fel_intento",
                schema: "public",
                columns: table => new
                {
                    id_intento = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_documento_fel = table.Column<int>(type: "integer", nullable: false),
                    id_usuario = table.Column<int>(type: "integer", nullable: false),
                    identificador = table.Column<Guid>(type: "uuid", nullable: false),
                    estado = table.Column<string>(type: "varchar(15)", nullable: false),
                    fecha_creacion = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    fecha_emision = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    fecha_respuesta = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    procesamiento_inicio = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    procesamiento_vence = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    procesamiento_token = table.Column<Guid>(type: "uuid", nullable: true),
                    recuperaciones = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    ultima_recuperacion = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    codigo_http = table.Column<short>(type: "smallint", nullable: true),
                    resultado = table.Column<bool>(type: "boolean", nullable: true),
                    origen = table.Column<string>(type: "text", nullable: true),
                    descripcion = table.Column<string>(type: "text", nullable: true),
                    uuid_respuesta = table.Column<string>(type: "text", nullable: true),
                    serie_respuesta = table.Column<string>(type: "text", nullable: true),
                    numero_respuesta = table.Column<string>(type: "text", nullable: true),
                    saldo = table.Column<long>(type: "bigint", nullable: true),
                    creditos = table.Column<long>(type: "bigint", nullable: true),
                    cupo_diario = table.Column<long>(type: "bigint", nullable: true),
                    saldo_diario = table.Column<long>(type: "bigint", nullable: true),
                    alertas_infile = table.Column<bool>(type: "boolean", nullable: true),
                    alertas_sat = table.Column<bool>(type: "boolean", nullable: true),
                    cantidad_errores = table.Column<int>(type: "integer", nullable: true),
                    xml_enviado = table.Column<string>(type: "text", nullable: false),
                    xml_certificado = table.Column<string>(type: "text", nullable: true),
                    migrado = table.Column<bool>(type: "boolean", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_documento_fel_intento", x => x.id_intento);
                    table.ForeignKey(
                        name: "FK_documento_fel_intento_documento_fel_id_documento_fel",
                        column: x => x.id_documento_fel,
                        principalSchema: "public",
                        principalTable: "documento_fel",
                        principalColumn: "id_documento_fel");
                    table.ForeignKey(
                        name: "FK_documento_fel_intento_usuario_id_usuario",
                        column: x => x.id_usuario,
                        principalSchema: "public",
                        principalTable: "usuario",
                        principalColumn: "id_usuario");
                });

            migrationBuilder.CreateTable(
                name: "movimiento_caja",
                schema: "public",
                columns: table => new
                {
                    id_movimiento_caja = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_sesion_caja = table.Column<int>(type: "integer", nullable: false),
                    id_tipo_movimiento = table.Column<int>(type: "integer", nullable: false),
                    id_usuario = table.Column<int>(type: "integer", nullable: false),
                    fecha_movimiento = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    monto = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    descripcion = table.Column<string>(type: "text", nullable: true),
                    id_venta = table.Column<int>(type: "integer", nullable: true),
                    id_compra = table.Column<int>(type: "integer", nullable: true),
                    id_pago_venta = table.Column<int>(type: "integer", nullable: true),
                    id_pago_compra = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_movimiento_caja", x => x.id_movimiento_caja);
                    table.ForeignKey(
                        name: "FK_movimiento_caja_pagos_compra_id_pago_compra",
                        column: x => x.id_pago_compra,
                        principalSchema: "public",
                        principalTable: "pagos_compra",
                        principalColumn: "id_pagos_compra");
                    table.ForeignKey(
                        name: "FK_movimiento_caja_pagos_id_pago_venta",
                        column: x => x.id_pago_venta,
                        principalSchema: "public",
                        principalTable: "pagos",
                        principalColumn: "id_pago");
                    table.ForeignKey(
                        name: "FK_movimiento_caja_registro_compras_id_compra",
                        column: x => x.id_compra,
                        principalSchema: "public",
                        principalTable: "registro_compras",
                        principalColumn: "id_compra");
                    table.ForeignKey(
                        name: "FK_movimiento_caja_sesion_caja_id_sesion_caja",
                        column: x => x.id_sesion_caja,
                        principalSchema: "public",
                        principalTable: "sesion_caja",
                        principalColumn: "id_sesion_caja",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_movimiento_caja_tipo_movimiento_caja_id_tipo_movimiento",
                        column: x => x.id_tipo_movimiento,
                        principalSchema: "public",
                        principalTable: "tipo_movimiento_caja",
                        principalColumn: "id_tipo_movimiento",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_movimiento_caja_usuario_id_usuario",
                        column: x => x.id_usuario,
                        principalSchema: "public",
                        principalTable: "usuario",
                        principalColumn: "id_usuario",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_movimiento_caja_ventas_id_venta",
                        column: x => x.id_venta,
                        principalSchema: "public",
                        principalTable: "ventas",
                        principalColumn: "id_ventas");
                });

            migrationBuilder.CreateTable(
                name: "detalle_pedido_catalogo",
                schema: "public",
                columns: table => new
                {
                    id_detalle_pedido = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_pedido = table.Column<int>(type: "integer", nullable: false),
                    id_producto_presentacion = table.Column<int>(type: "integer", nullable: false),
                    nombre_producto = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    nombre_presentacion = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    cantidad = table.Column<int>(type: "integer", nullable: false),
                    precio_unitario = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    subtotal = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false, computedColumnSql: "CAST(cantidad * precio_unitario AS numeric(18,2))", stored: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_detalle_pedido_catalogo", x => x.id_detalle_pedido);
                    table.UniqueConstraint("UQ_detalle_pedido_catalogo_presentacion", x => new { x.id_pedido, x.id_producto_presentacion });
                    table.CheckConstraint("CK_detalle_pedido_catalogo_importes", "cantidad > 0 AND precio_unitario > 0");
                    table.CheckConstraint("CK_detalle_pedido_catalogo_nombres", "length(btrim(nombre_producto)) > 0 AND length(btrim(nombre_presentacion)) > 0");
                    table.ForeignKey(
                        name: "FK_detalle_pedido_catalogo_pedido",
                        column: x => x.id_pedido,
                        principalSchema: "public",
                        principalTable: "pedido_catalogo",
                        principalColumn: "id_pedido");
                    table.ForeignKey(
                        name: "FK_detalle_pedido_catalogo_presentacion",
                        column: x => x.id_producto_presentacion,
                        principalSchema: "public",
                        principalTable: "producto_presentacion",
                        principalColumn: "id_producto_presentacion");
                });

            migrationBuilder.CreateTable(
                name: "whatsapp_envio_pagina",
                schema: "public",
                columns: table => new
                {
                    id_envio = table.Column<Guid>(type: "uuid", nullable: false),
                    pagina = table.Column<int>(type: "integer", nullable: false),
                    estado = table.Column<string>(type: "character varying(20)", unicode: false, maxLength: 20, nullable: false),
                    message_id = table.Column<string>(type: "character varying(512)", unicode: false, maxLength: 512, nullable: true),
                    etapa = table.Column<string>(type: "character varying(20)", unicode: false, maxLength: 20, nullable: true),
                    codigo_meta = table.Column<int>(type: "integer", nullable: true),
                    actualizado_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_whatsapp_envio_pagina", x => new { x.id_envio, x.pagina });
                    table.ForeignKey(
                        name: "FK_whatsapp_envio_pagina_whatsapp_envio_id_envio",
                        column: x => x.id_envio,
                        principalSchema: "public",
                        principalTable: "whatsapp_envio",
                        principalColumn: "id_envio",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "documento_fel_alerta",
                schema: "public",
                columns: table => new
                {
                    id_alerta = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_intento = table.Column<long>(type: "bigint", nullable: false),
                    origen = table.Column<string>(type: "varchar(6)", nullable: false),
                    orden = table.Column<int>(type: "integer", nullable: false),
                    fuente = table.Column<string>(type: "text", nullable: true),
                    categoria = table.Column<string>(type: "text", nullable: true),
                    numeral = table.Column<string>(type: "text", nullable: true),
                    validacion = table.Column<string>(type: "text", nullable: true),
                    mensaje = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_documento_fel_alerta", x => x.id_alerta);
                    table.ForeignKey(
                        name: "FK_documento_fel_alerta_documento_fel_intento_id_intento",
                        column: x => x.id_intento,
                        principalSchema: "public",
                        principalTable: "documento_fel_intento",
                        principalColumn: "id_intento");
                });

            migrationBuilder.CreateTable(
                name: "documento_fel_error",
                schema: "public",
                columns: table => new
                {
                    id_error = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_intento = table.Column<long>(type: "bigint", nullable: false),
                    orden = table.Column<int>(type: "integer", nullable: false),
                    resultado = table.Column<bool>(type: "boolean", nullable: true),
                    fuente = table.Column<string>(type: "text", nullable: true),
                    categoria = table.Column<string>(type: "text", nullable: true),
                    numeral = table.Column<string>(type: "text", nullable: true),
                    validacion = table.Column<string>(type: "text", nullable: true),
                    mensaje = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_documento_fel_error", x => x.id_error);
                    table.ForeignKey(
                        name: "FK_documento_fel_error_documento_fel_intento_id_intento",
                        column: x => x.id_intento,
                        principalSchema: "public",
                        principalTable: "documento_fel_intento",
                        principalColumn: "id_intento");
                });

            migrationBuilder.CreateTable(
                name: "whatsapp_envio_intento",
                schema: "public",
                columns: table => new
                {
                    id_intento = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_envio = table.Column<Guid>(type: "uuid", nullable: false),
                    pagina = table.Column<int>(type: "integer", nullable: false),
                    id_usuario = table.Column<int>(type: "integer", nullable: false),
                    inicio_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    fin_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    estado = table.Column<string>(type: "character varying(20)", unicode: false, maxLength: 20, nullable: false),
                    message_id = table.Column<string>(type: "character varying(512)", unicode: false, maxLength: 512, nullable: true),
                    etapa = table.Column<string>(type: "character varying(20)", unicode: false, maxLength: 20, nullable: true),
                    codigo_meta = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_whatsapp_envio_intento", x => x.id_intento);
                    table.ForeignKey(
                        name: "FK_whatsapp_envio_intento_usuario_id_usuario",
                        column: x => x.id_usuario,
                        principalSchema: "public",
                        principalTable: "usuario",
                        principalColumn: "id_usuario",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_whatsapp_envio_intento_whatsapp_envio_pagina_id_envio_pagina",
                        columns: x => new { x.id_envio, x.pagina },
                        principalSchema: "public",
                        principalTable: "whatsapp_envio_pagina",
                        principalColumns: new[] { "id_envio", "pagina" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_auditoria_evento_accion_fecha",
                schema: "public",
                table: "auditoria_evento",
                columns: new[] { "accion", "fecha_utc" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_auditoria_evento_entidad_registro_fecha",
                schema: "public",
                table: "auditoria_evento",
                columns: new[] { "entidad", "id_registro", "fecha_utc" },
                descending: new[] { false, false, true },
                filter: "id_registro IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_auditoria_evento_fecha",
                schema: "public",
                table: "auditoria_evento",
                columns: new[] { "fecha_utc", "id_auditoria" },
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_auditoria_evento_usuario_fecha",
                schema: "public",
                table: "auditoria_evento",
                columns: new[] { "id_usuario", "fecha_utc" },
                descending: new[] { false, true },
                filter: "id_usuario IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_conversacion_id_cuenta_cliente",
                schema: "public",
                table: "conversacion",
                column: "id_cuenta_cliente");

            migrationBuilder.CreateIndex(
                name: "IX_conversacion_id_usuario",
                schema: "public",
                table: "conversacion",
                column: "id_usuario");

            migrationBuilder.CreateIndex(
                name: "IX_detalle_compra_id_producto",
                schema: "public",
                table: "detalle_compra",
                column: "id_producto");

            migrationBuilder.CreateIndex(
                name: "IX_detalle_compra_id_registro_compra",
                schema: "public",
                table: "detalle_compra",
                column: "id_registro_compra");

            migrationBuilder.CreateIndex(
                name: "IX_detalle_pedido_catalogo_id_producto_presentacion",
                schema: "public",
                table: "detalle_pedido_catalogo",
                column: "id_producto_presentacion");

            migrationBuilder.CreateIndex(
                name: "IX_detalle_venta_id_producto",
                schema: "public",
                table: "detalle_venta",
                column: "id_producto");

            migrationBuilder.CreateIndex(
                name: "IX_detalle_venta_id_producto_presentacion",
                schema: "public",
                table: "detalle_venta",
                column: "id_producto_presentacion");

            migrationBuilder.CreateIndex(
                name: "IX_detalle_venta_id_venta",
                schema: "public",
                table: "detalle_venta",
                column: "id_venta");

            migrationBuilder.CreateIndex(
                name: "IX_documento_fel_usuario",
                schema: "public",
                table: "documento_fel",
                column: "id_usuario");

            migrationBuilder.CreateIndex(
                name: "UX_documento_fel_identificador",
                schema: "public",
                table: "documento_fel",
                column: "identificador",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_documento_fel_uuid",
                schema: "public",
                table: "documento_fel",
                column: "uuid",
                unique: true,
                filter: "uuid IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "UX_documento_fel_venta_ambiente",
                schema: "public",
                table: "documento_fel",
                columns: new[] { "id_venta", "ambiente" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_documento_fel_alerta_id_intento",
                schema: "public",
                table: "documento_fel_alerta",
                column: "id_intento");

            migrationBuilder.CreateIndex(
                name: "IX_documento_fel_error_id_intento",
                schema: "public",
                table: "documento_fel_error",
                column: "id_intento");

            migrationBuilder.CreateIndex(
                name: "IX_documento_fel_intento_id_documento_fel",
                schema: "public",
                table: "documento_fel_intento",
                column: "id_documento_fel");

            migrationBuilder.CreateIndex(
                name: "IX_documento_fel_intento_id_usuario",
                schema: "public",
                table: "documento_fel_intento",
                column: "id_usuario");

            migrationBuilder.CreateIndex(
                name: "IX_documento_fel_intento_identificador",
                schema: "public",
                table: "documento_fel_intento",
                column: "identificador",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_fel_intento_procesamiento_vence",
                schema: "public",
                table: "documento_fel_intento",
                column: "procesamiento_vence",
                filter: "estado = 'EN_PROCESO'");

            migrationBuilder.CreateIndex(
                name: "IX_gastos_id_sesion_caja",
                schema: "public",
                table: "gastos",
                column: "id_sesion_caja");

            migrationBuilder.CreateIndex(
                name: "IX_gastos_id_usuario",
                schema: "public",
                table: "gastos",
                column: "id_usuario");

            migrationBuilder.CreateIndex(
                name: "IX_inventario_Movimientos_id_producto",
                schema: "public",
                table: "inventario_Movimientos",
                column: "id_producto");

            migrationBuilder.CreateIndex(
                name: "IX_inventario_Movimientos_id_usuario",
                schema: "public",
                table: "inventario_Movimientos",
                column: "id_usuario");

            migrationBuilder.CreateIndex(
                name: "IX_marca_id_categoria",
                schema: "public",
                table: "marca",
                column: "id_categoria");

            migrationBuilder.CreateIndex(
                name: "IX_mensaje_id_conversacion",
                schema: "public",
                table: "mensaje",
                column: "id_conversacion");

            migrationBuilder.CreateIndex(
                name: "IX_movimiento_caja_id_compra",
                schema: "public",
                table: "movimiento_caja",
                column: "id_compra");

            migrationBuilder.CreateIndex(
                name: "IX_movimiento_caja_id_pago_compra",
                schema: "public",
                table: "movimiento_caja",
                column: "id_pago_compra");

            migrationBuilder.CreateIndex(
                name: "IX_movimiento_caja_id_pago_venta",
                schema: "public",
                table: "movimiento_caja",
                column: "id_pago_venta");

            migrationBuilder.CreateIndex(
                name: "IX_movimiento_caja_id_sesion_caja",
                schema: "public",
                table: "movimiento_caja",
                column: "id_sesion_caja");

            migrationBuilder.CreateIndex(
                name: "IX_movimiento_caja_id_tipo_movimiento",
                schema: "public",
                table: "movimiento_caja",
                column: "id_tipo_movimiento");

            migrationBuilder.CreateIndex(
                name: "IX_movimiento_caja_id_usuario",
                schema: "public",
                table: "movimiento_caja",
                column: "id_usuario");

            migrationBuilder.CreateIndex(
                name: "IX_movimiento_caja_id_venta",
                schema: "public",
                table: "movimiento_caja",
                column: "id_venta");

            migrationBuilder.CreateIndex(
                name: "IX_pagos_id_sesion_caja",
                schema: "public",
                table: "pagos",
                column: "id_sesion_caja");

            migrationBuilder.CreateIndex(
                name: "IX_pagos_id_usuario",
                schema: "public",
                table: "pagos",
                column: "id_usuario");

            migrationBuilder.CreateIndex(
                name: "IX_pagos_id_venta",
                schema: "public",
                table: "pagos",
                column: "id_venta");

            migrationBuilder.CreateIndex(
                name: "UX_pagos_id_operacion",
                schema: "public",
                table: "pagos",
                column: "id_operacion",
                unique: true,
                filter: "id_operacion IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_pagos_compra_id_compra",
                schema: "public",
                table: "pagos_compra",
                column: "id_compra");

            migrationBuilder.CreateIndex(
                name: "IX_pagos_compra_id_sesion_caja",
                schema: "public",
                table: "pagos_compra",
                column: "id_sesion_caja");

            migrationBuilder.CreateIndex(
                name: "IX_pagos_compra_id_usuario",
                schema: "public",
                table: "pagos_compra",
                column: "id_usuario");

            migrationBuilder.CreateIndex(
                name: "UX_pagos_compra_id_operacion",
                schema: "public",
                table: "pagos_compra",
                column: "id_operacion",
                unique: true,
                filter: "id_operacion IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_pedido_catalogo_estado_fecha",
                schema: "public",
                table: "pedido_catalogo",
                columns: new[] { "estado", "fecha_utc", "id_pedido" },
                descending: new[] { false, true, true });

            migrationBuilder.CreateIndex(
                name: "IX_pedido_catalogo_id_zona_entrega",
                schema: "public",
                table: "pedido_catalogo",
                column: "id_zona_entrega");

            migrationBuilder.CreateIndex(
                name: "UX_pedido_catalogo_venta",
                schema: "public",
                table: "pedido_catalogo",
                column: "id_venta",
                unique: true,
                filter: "id_venta IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_producto_precio_id_producto",
                schema: "public",
                table: "producto_precio",
                column: "id_producto");

            migrationBuilder.CreateIndex(
                name: "IX_producto_precio_id_tipo_cliente",
                schema: "public",
                table: "producto_precio",
                column: "id_tipo_cliente");

            migrationBuilder.CreateIndex(
                name: "IX_producto_presentacion_id_presentacion",
                schema: "public",
                table: "producto_presentacion",
                column: "id_presentacion");

            migrationBuilder.CreateIndex(
                name: "IX_producto_presentacion_id_producto",
                schema: "public",
                table: "producto_presentacion",
                column: "id_producto");

            migrationBuilder.CreateIndex(
                name: "IX_productos_id_marca",
                schema: "public",
                table: "productos",
                column: "id_marca");

            migrationBuilder.CreateIndex(
                name: "IX_registro_compras_id_estado_compra",
                schema: "public",
                table: "registro_compras",
                column: "id_estado_compra");

            migrationBuilder.CreateIndex(
                name: "IX_registro_compras_id_proveedor",
                schema: "public",
                table: "registro_compras",
                column: "id_proveedor");

            migrationBuilder.CreateIndex(
                name: "IX_registro_compras_id_sesion_caja",
                schema: "public",
                table: "registro_compras",
                column: "id_sesion_caja");

            migrationBuilder.CreateIndex(
                name: "IX_registro_compras_id_usuario",
                schema: "public",
                table: "registro_compras",
                column: "id_usuario");

            migrationBuilder.CreateIndex(
                name: "IX_rol_permiso_id_permiso",
                schema: "public",
                table: "rol_permiso",
                column: "id_permiso");

            migrationBuilder.CreateIndex(
                name: "IX_rol_permiso_id_rol",
                schema: "public",
                table: "rol_permiso",
                column: "id_rol");

            migrationBuilder.CreateIndex(
                name: "IX_sesion_caja_id_caja",
                schema: "public",
                table: "sesion_caja",
                column: "id_caja");

            migrationBuilder.CreateIndex(
                name: "IX_sesion_caja_id_usuario_apertura",
                schema: "public",
                table: "sesion_caja",
                column: "id_usuario_apertura");

            migrationBuilder.CreateIndex(
                name: "IX_sesion_caja_id_usuario_cierre",
                schema: "public",
                table: "sesion_caja",
                column: "id_usuario_cierre");

            migrationBuilder.CreateIndex(
                name: "UX_sesion_caja_unica_abierta",
                schema: "public",
                table: "sesion_caja",
                column: "fecha_cierre",
                unique: true,
                filter: "fecha_cierre IS NULL")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_sesiones_usuario_fecha_vencimiento",
                schema: "public",
                table: "sesiones_usuario",
                column: "fecha_vencimiento");

            migrationBuilder.CreateIndex(
                name: "IX_sesiones_usuario_id_usuario",
                schema: "public",
                table: "sesiones_usuario",
                column: "id_usuario");

            migrationBuilder.CreateIndex(
                name: "IX_sesiones_usuario_token_hash",
                schema: "public",
                table: "sesiones_usuario",
                column: "token_hash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_usuario_id_rol",
                schema: "public",
                table: "usuario",
                column: "id_rol");

            migrationBuilder.CreateIndex(
                name: "UX_usuario_correo",
                schema: "public",
                table: "usuario",
                column: "correo",
                unique: true,
                filter: "correo IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "UX_usuario_telefono",
                schema: "public",
                table: "usuario",
                column: "telefono",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_usuario_usuario",
                schema: "public",
                table: "usuario",
                column: "usuario",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_usuario_permiso_id_permiso",
                schema: "public",
                table: "usuario_permiso",
                column: "id_permiso");

            migrationBuilder.CreateIndex(
                name: "IX_usuario_permiso_id_usuario",
                schema: "public",
                table: "usuario_permiso",
                column: "id_usuario");

            migrationBuilder.CreateIndex(
                name: "IX_venta_operacion_id_venta",
                schema: "public",
                table: "venta_operacion",
                column: "id_venta",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ventas_id_cliente",
                schema: "public",
                table: "ventas",
                column: "id_cliente");

            migrationBuilder.CreateIndex(
                name: "IX_ventas_id_estado_venta",
                schema: "public",
                table: "ventas",
                column: "id_estado_venta");

            migrationBuilder.CreateIndex(
                name: "IX_ventas_id_sesion_caja_fecha_venta",
                schema: "public",
                table: "ventas",
                columns: new[] { "id_sesion_caja", "fecha_venta" });

            migrationBuilder.CreateIndex(
                name: "IX_ventas_id_usuario",
                schema: "public",
                table: "ventas",
                column: "id_usuario");

            migrationBuilder.CreateIndex(
                name: "IX_whatsapp_envio_id_empresa",
                schema: "public",
                table: "whatsapp_envio",
                column: "id_empresa");

            migrationBuilder.CreateIndex(
                name: "IX_whatsapp_envio_id_usuario",
                schema: "public",
                table: "whatsapp_envio",
                column: "id_usuario");

            migrationBuilder.CreateIndex(
                name: "IX_whatsapp_envio_venta_empresa",
                schema: "public",
                table: "whatsapp_envio",
                columns: new[] { "id_venta", "id_empresa" });

            migrationBuilder.CreateIndex(
                name: "IX_whatsapp_envio_intento_id_envio_pagina",
                schema: "public",
                table: "whatsapp_envio_intento",
                columns: new[] { "id_envio", "pagina" });

            migrationBuilder.CreateIndex(
                name: "IX_whatsapp_envio_intento_id_usuario",
                schema: "public",
                table: "whatsapp_envio_intento",
                column: "id_usuario");

            migrationBuilder.CreateIndex(
                name: "IX_whatsapp_envio_intento_message_id",
                schema: "public",
                table: "whatsapp_envio_intento",
                column: "message_id",
                filter: "message_id IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_whatsapp_webhook_message_id",
                schema: "public",
                table: "whatsapp_webhook_evento",
                column: "message_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "auditoria_evento_detalle",
                schema: "public");

            migrationBuilder.DropTable(
                name: "detalle_compra",
                schema: "public");

            migrationBuilder.DropTable(
                name: "detalle_pedido_catalogo",
                schema: "public");

            migrationBuilder.DropTable(
                name: "detalle_venta",
                schema: "public");

            migrationBuilder.DropTable(
                name: "documento_fel_alerta",
                schema: "public");

            migrationBuilder.DropTable(
                name: "documento_fel_error",
                schema: "public");

            migrationBuilder.DropTable(
                name: "gastos",
                schema: "public");

            migrationBuilder.DropTable(
                name: "inventario_Movimientos",
                schema: "public");

            migrationBuilder.DropTable(
                name: "mensaje",
                schema: "public");

            migrationBuilder.DropTable(
                name: "movimiento_caja",
                schema: "public");

            migrationBuilder.DropTable(
                name: "producto_precio",
                schema: "public");

            migrationBuilder.DropTable(
                name: "rol_permiso",
                schema: "public");

            migrationBuilder.DropTable(
                name: "sesiones_usuario",
                schema: "public");

            migrationBuilder.DropTable(
                name: "usuario_permiso",
                schema: "public");

            migrationBuilder.DropTable(
                name: "venta_operacion",
                schema: "public");

            migrationBuilder.DropTable(
                name: "whatsapp_envio_intento",
                schema: "public");

            migrationBuilder.DropTable(
                name: "whatsapp_webhook_evento",
                schema: "public");

            migrationBuilder.DropTable(
                name: "auditoria_evento",
                schema: "public");

            migrationBuilder.DropTable(
                name: "pedido_catalogo",
                schema: "public");

            migrationBuilder.DropTable(
                name: "producto_presentacion",
                schema: "public");

            migrationBuilder.DropTable(
                name: "documento_fel_intento",
                schema: "public");

            migrationBuilder.DropTable(
                name: "conversacion",
                schema: "public");

            migrationBuilder.DropTable(
                name: "pagos_compra",
                schema: "public");

            migrationBuilder.DropTable(
                name: "pagos",
                schema: "public");

            migrationBuilder.DropTable(
                name: "tipo_movimiento_caja",
                schema: "public");

            migrationBuilder.DropTable(
                name: "tipo_cliente",
                schema: "public");

            migrationBuilder.DropTable(
                name: "tabla_permisos",
                schema: "public");

            migrationBuilder.DropTable(
                name: "whatsapp_envio_pagina",
                schema: "public");

            migrationBuilder.DropTable(
                name: "zona_entrega",
                schema: "public");

            migrationBuilder.DropTable(
                name: "presentaciones",
                schema: "public");

            migrationBuilder.DropTable(
                name: "productos",
                schema: "public");

            migrationBuilder.DropTable(
                name: "documento_fel",
                schema: "public");

            migrationBuilder.DropTable(
                name: "cuenta_cliente",
                schema: "public");

            migrationBuilder.DropTable(
                name: "registro_compras",
                schema: "public");

            migrationBuilder.DropTable(
                name: "whatsapp_envio",
                schema: "public");

            migrationBuilder.DropTable(
                name: "marca",
                schema: "public");

            migrationBuilder.DropTable(
                name: "estado_compra",
                schema: "public");

            migrationBuilder.DropTable(
                name: "proveedores",
                schema: "public");

            migrationBuilder.DropTable(
                name: "empresa",
                schema: "public");

            migrationBuilder.DropTable(
                name: "ventas",
                schema: "public");

            migrationBuilder.DropTable(
                name: "categoria",
                schema: "public");

            migrationBuilder.DropTable(
                name: "cliente",
                schema: "public");

            migrationBuilder.DropTable(
                name: "estado_venta",
                schema: "public");

            migrationBuilder.DropTable(
                name: "sesion_caja",
                schema: "public");

            migrationBuilder.DropTable(
                name: "caja",
                schema: "public");

            migrationBuilder.DropTable(
                name: "usuario",
                schema: "public");

            migrationBuilder.DropTable(
                name: "rol",
                schema: "public");
        }
    }
}
