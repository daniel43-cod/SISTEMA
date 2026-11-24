using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API_SISTEMA.Data.Migrations
{
    /// <inheritdoc />
    public partial class ImagenSurtido : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "imagen",
                schema: "public",
                table: "surtido",
                type: "character varying(2048)",
                maxLength: 2048,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "imagen",
                schema: "public",
                table: "surtido");
        }
    }
}
