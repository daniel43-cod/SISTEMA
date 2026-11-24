using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API_SISTEMA.Data.Migrations
{
    /// <inheritdoc />
    public partial class MarcaSurtido : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "id_marca",
                schema: "public",
                table: "surtido",
                type: "integer",
                nullable: false);

            migrationBuilder.CreateIndex(
                name: "IX_surtido_id_marca",
                schema: "public",
                table: "surtido",
                column: "id_marca");

            migrationBuilder.AddForeignKey(
                name: "FK_surtido_marca_id_marca",
                schema: "public",
                table: "surtido",
                column: "id_marca",
                principalSchema: "public",
                principalTable: "marca",
                principalColumn: "id_marca",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_surtido_marca_id_marca",
                schema: "public",
                table: "surtido");

            migrationBuilder.DropIndex(
                name: "IX_surtido_id_marca",
                schema: "public",
                table: "surtido");

            migrationBuilder.DropColumn(
                name: "id_marca",
                schema: "public",
                table: "surtido");
        }
    }
}
