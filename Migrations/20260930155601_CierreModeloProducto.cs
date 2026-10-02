using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VinotecaApp.Migrations
{
    /// <inheritdoc />
    public partial class CierreModeloProducto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "Precio",
                table: "Productos",
                type: "int",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,2)");

            migrationBuilder.AddColumn<int>(
                name: "Cosecha",
                table: "Productos",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Productos_Nombre_CategoriaId_BodegaId_Cosecha",
                table: "Productos",
                columns: new[] { "Nombre", "CategoriaId", "BodegaId", "Cosecha" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Categorias_Nombre_CategoriaPadreId",
                table: "Categorias",
                columns: new[] { "Nombre", "CategoriaPadreId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Productos_Nombre_CategoriaId_BodegaId_Cosecha",
                table: "Productos");

            migrationBuilder.DropIndex(
                name: "IX_Categorias_Nombre_CategoriaPadreId",
                table: "Categorias");

            migrationBuilder.DropColumn(
                name: "Cosecha",
                table: "Productos");

            migrationBuilder.AlterColumn<decimal>(
                name: "Precio",
                table: "Productos",
                type: "decimal(10,2)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");
        }
    }
}
