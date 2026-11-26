using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API_SISTEMA.Data.Migrations
{
    /// <inheritdoc />
    public partial class IdempotenciaComprasGastos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "huella_operacion",
                schema: "public",
                table: "registro_compras",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "id_operacion",
                schema: "public",
                table: "registro_compras",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "huella_operacion",
                schema: "public",
                table: "gastos",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "id_operacion",
                schema: "public",
                table: "gastos",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "UX_registro_compra_id_operacion",
                schema: "public",
                table: "registro_compras",
                column: "id_operacion",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_gastos_id_operacion",
                schema: "public",
                table: "gastos",
                column: "id_operacion",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UX_registro_compra_id_operacion",
                schema: "public",
                table: "registro_compras");

            migrationBuilder.DropIndex(
                name: "UX_gastos_id_operacion",
                schema: "public",
                table: "gastos");

            migrationBuilder.DropColumn(
                name: "huella_operacion",
                schema: "public",
                table: "registro_compras");

            migrationBuilder.DropColumn(
                name: "id_operacion",
                schema: "public",
                table: "registro_compras");

            migrationBuilder.DropColumn(
                name: "huella_operacion",
                schema: "public",
                table: "gastos");

            migrationBuilder.DropColumn(
                name: "id_operacion",
                schema: "public",
                table: "gastos");
        }
    }
}
