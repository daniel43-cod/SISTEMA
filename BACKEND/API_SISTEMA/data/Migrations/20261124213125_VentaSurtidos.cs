using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API_SISTEMA.Data.Migrations
{
    /// <inheritdoc />
    public partial class VentaSurtidos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_detalle_venta_productos_id_producto",
                schema: "public",
                table: "detalle_venta");

            migrationBuilder.AlterColumn<int>(
                name: "id_producto",
                schema: "public",
                table: "detalle_venta",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<int>(
                name: "id_surtido",
                schema: "public",
                table: "detalle_venta",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_detalle_venta_id_surtido",
                schema: "public",
                table: "detalle_venta",
                column: "id_surtido");

            migrationBuilder.AddCheckConstraint(
                name: "CK_detalle_venta_propietario",
                schema: "public",
                table: "detalle_venta",
                sql: "(id_producto IS NOT NULL AND id_surtido IS NULL) OR (id_producto IS NULL AND id_surtido IS NOT NULL)");

            migrationBuilder.AddForeignKey(
                name: "FK_detalle_venta_productos_id_producto",
                schema: "public",
                table: "detalle_venta",
                column: "id_producto",
                principalSchema: "public",
                principalTable: "productos",
                principalColumn: "id_producto");

            migrationBuilder.AddForeignKey(
                name: "FK_detalle_venta_surtido_id_surtido",
                schema: "public",
                table: "detalle_venta",
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
                name: "FK_detalle_venta_productos_id_producto",
                schema: "public",
                table: "detalle_venta");

            migrationBuilder.DropForeignKey(
                name: "FK_detalle_venta_surtido_id_surtido",
                schema: "public",
                table: "detalle_venta");

            migrationBuilder.DropIndex(
                name: "IX_detalle_venta_id_surtido",
                schema: "public",
                table: "detalle_venta");

            migrationBuilder.DropCheckConstraint(
                name: "CK_detalle_venta_propietario",
                schema: "public",
                table: "detalle_venta");

            migrationBuilder.DropColumn(
                name: "id_surtido",
                schema: "public",
                table: "detalle_venta");

            migrationBuilder.AlterColumn<int>(
                name: "id_producto",
                schema: "public",
                table: "detalle_venta",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_detalle_venta_productos_id_producto",
                schema: "public",
                table: "detalle_venta",
                column: "id_producto",
                principalSchema: "public",
                principalTable: "productos",
                principalColumn: "id_producto",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
