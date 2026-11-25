using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API_SISTEMA.Data.Migrations
{
    /// <inheritdoc />
    public partial class SurtidosPedidosCatalogo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "detalle_pedido_surtido",
                schema: "public",
                columns: table => new
                {
                    id_detalle_pedido = table.Column<int>(type: "integer", nullable: false),
                    id_producto = table.Column<int>(type: "integer", nullable: false),
                    cantidad = table.Column<int>(type: "integer", nullable: false),
                    nombre_producto = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_detalle_pedido_surtido", x => new { x.id_detalle_pedido, x.id_producto });
                    table.CheckConstraint("CK_detalle_pedido_surtido_cantidad", "cantidad > 0");
                    table.ForeignKey(
                        name: "FK_detalle_pedido_surtido_detalle_pedido_catalogo_id_detalle_p~",
                        column: x => x.id_detalle_pedido,
                        principalSchema: "public",
                        principalTable: "detalle_pedido_catalogo",
                        principalColumn: "id_detalle_pedido",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_detalle_pedido_surtido_productos_id_producto",
                        column: x => x.id_producto,
                        principalSchema: "public",
                        principalTable: "productos",
                        principalColumn: "id_producto",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_detalle_pedido_surtido_id_producto",
                schema: "public",
                table: "detalle_pedido_surtido",
                column: "id_producto");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "detalle_pedido_surtido",
                schema: "public");
        }
    }
}
