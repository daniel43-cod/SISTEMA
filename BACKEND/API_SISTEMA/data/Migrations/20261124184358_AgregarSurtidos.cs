using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace API_SISTEMA.Data.Migrations
{
    /// <inheritdoc />
    public partial class AgregarSurtidos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_producto_presentacion_productos_id_producto",
                schema: "public",
                table: "producto_presentacion");

            migrationBuilder.AlterColumn<int>(
                name: "id_producto",
                schema: "public",
                table: "producto_presentacion",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<int>(
                name: "id_surtido",
                schema: "public",
                table: "producto_presentacion",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "detalle_venta_surtido",
                schema: "public",
                columns: table => new
                {
                    id_detalle_venta = table.Column<int>(type: "integer", nullable: false),
                    id_producto = table.Column<int>(type: "integer", nullable: false),
                    cantidad = table.Column<int>(type: "integer", nullable: false),
                    costo_unitario_venta = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_detalle_venta_surtido", x => new { x.id_detalle_venta, x.id_producto });
                    table.CheckConstraint("CK_detalle_venta_surtido_cantidad", "cantidad > 0");
                    table.CheckConstraint("CK_detalle_venta_surtido_costo", "costo_unitario_venta > 0");
                    table.ForeignKey(
                        name: "FK_detalle_venta_surtido_detalle_venta_id_detalle_venta",
                        column: x => x.id_detalle_venta,
                        principalSchema: "public",
                        principalTable: "detalle_venta",
                        principalColumn: "id_detalle_venta",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_detalle_venta_surtido_productos_id_producto",
                        column: x => x.id_producto,
                        principalSchema: "public",
                        principalTable: "productos",
                        principalColumn: "id_producto",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "surtido",
                schema: "public",
                columns: table => new
                {
                    id_surtido = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    estado = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_surtido", x => x.id_surtido);
                    table.CheckConstraint("CK_surtido_nombre", "length(btrim(nombre)) > 0");
                });

            migrationBuilder.CreateTable(
                name: "surtido_producto",
                schema: "public",
                columns: table => new
                {
                    id_surtido = table.Column<int>(type: "integer", nullable: false),
                    id_producto = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_surtido_producto", x => new { x.id_surtido, x.id_producto });
                    table.ForeignKey(
                        name: "FK_surtido_producto_productos_id_producto",
                        column: x => x.id_producto,
                        principalSchema: "public",
                        principalTable: "productos",
                        principalColumn: "id_producto",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_surtido_producto_surtido_id_surtido",
                        column: x => x.id_surtido,
                        principalSchema: "public",
                        principalTable: "surtido",
                        principalColumn: "id_surtido",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_producto_presentacion_id_surtido",
                schema: "public",
                table: "producto_presentacion",
                column: "id_surtido");

            migrationBuilder.AddCheckConstraint(
                name: "CK_producto_presentacion_propietario",
                schema: "public",
                table: "producto_presentacion",
                sql: "(id_producto IS NOT NULL AND id_surtido IS NULL) OR (id_producto IS NULL AND id_surtido IS NOT NULL)");

            migrationBuilder.CreateIndex(
                name: "IX_detalle_venta_surtido_id_producto",
                schema: "public",
                table: "detalle_venta_surtido",
                column: "id_producto");

            migrationBuilder.CreateIndex(
                name: "IX_surtido_producto_id_producto",
                schema: "public",
                table: "surtido_producto",
                column: "id_producto");

            migrationBuilder.AddForeignKey(
                name: "FK_producto_presentacion_productos_id_producto",
                schema: "public",
                table: "producto_presentacion",
                column: "id_producto",
                principalSchema: "public",
                principalTable: "productos",
                principalColumn: "id_producto");

            migrationBuilder.AddForeignKey(
                name: "FK_producto_presentacion_surtido_id_surtido",
                schema: "public",
                table: "producto_presentacion",
                column: "id_surtido",
                principalSchema: "public",
                principalTable: "surtido",
                principalColumn: "id_surtido",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_producto_presentacion_productos_id_producto",
                schema: "public",
                table: "producto_presentacion");

            migrationBuilder.DropForeignKey(
                name: "FK_producto_presentacion_surtido_id_surtido",
                schema: "public",
                table: "producto_presentacion");

            migrationBuilder.DropTable(
                name: "detalle_venta_surtido",
                schema: "public");

            migrationBuilder.DropTable(
                name: "surtido_producto",
                schema: "public");

            migrationBuilder.DropTable(
                name: "surtido",
                schema: "public");

            migrationBuilder.DropIndex(
                name: "IX_producto_presentacion_id_surtido",
                schema: "public",
                table: "producto_presentacion");

            migrationBuilder.DropCheckConstraint(
                name: "CK_producto_presentacion_propietario",
                schema: "public",
                table: "producto_presentacion");

            migrationBuilder.DropColumn(
                name: "id_surtido",
                schema: "public",
                table: "producto_presentacion");

            migrationBuilder.AlterColumn<int>(
                name: "id_producto",
                schema: "public",
                table: "producto_presentacion",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_producto_presentacion_productos_id_producto",
                schema: "public",
                table: "producto_presentacion",
                column: "id_producto",
                principalSchema: "public",
                principalTable: "productos",
                principalColumn: "id_producto",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
