using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VinotecaApp.Migrations
{
    /// <inheritdoc />
    public partial class AgregarBodegas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Productos_Bodega_BodegaId",
                table: "Productos");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Bodega",
                table: "Bodega");

            migrationBuilder.RenameTable(
                name: "Bodega",
                newName: "Bodegas");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Bodegas",
                table: "Bodegas",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Productos_Bodegas_BodegaId",
                table: "Productos",
                column: "BodegaId",
                principalTable: "Bodegas",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Productos_Bodegas_BodegaId",
                table: "Productos");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Bodegas",
                table: "Bodegas");

            migrationBuilder.RenameTable(
                name: "Bodegas",
                newName: "Bodega");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Bodega",
                table: "Bodega",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Productos_Bodega_BodegaId",
                table: "Productos",
                column: "BodegaId",
                principalTable: "Bodega",
                principalColumn: "Id");
        }
    }
}
