using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VinotecaApp.Migrations
{
    /// <inheritdoc />
    public partial class AgregarBodegaAProducto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Descripcion",
                table: "Productos");

            migrationBuilder.AddColumn<int>(
                name: "BodegaId",
                table: "Productos",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Bodega",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bodega", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Productos_BodegaId",
                table: "Productos",
                column: "BodegaId");

            migrationBuilder.AddForeignKey(
                name: "FK_Productos_Bodega_BodegaId",
                table: "Productos",
                column: "BodegaId",
                principalTable: "Bodega",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Productos_Bodega_BodegaId",
                table: "Productos");

            migrationBuilder.DropTable(
                name: "Bodega");

            migrationBuilder.DropIndex(
                name: "IX_Productos_BodegaId",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "BodegaId",
                table: "Productos");

            migrationBuilder.AddColumn<string>(
                name: "Descripcion",
                table: "Productos",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);
        }
    }
}
