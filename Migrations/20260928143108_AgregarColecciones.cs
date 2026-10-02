using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VinotecaApp.Migrations
{
    /// <inheritdoc />
    public partial class AgregarColecciones : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PrecioOferta",
                table: "Productos",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Colecciones",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Titulo = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    Texto = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    ImagenUrl = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Orden = table.Column<int>(type: "int", nullable: false),
                    Activa = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Colecciones", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ColeccionProducto",
                columns: table => new
                {
                    ColeccionesId = table.Column<int>(type: "int", nullable: false),
                    ProductosId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ColeccionProducto", x => new { x.ColeccionesId, x.ProductosId });
                    table.ForeignKey(
                        name: "FK_ColeccionProducto_Colecciones_ColeccionesId",
                        column: x => x.ColeccionesId,
                        principalTable: "Colecciones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ColeccionProducto_Productos_ProductosId",
                        column: x => x.ProductosId,
                        principalTable: "Productos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ColeccionProducto_ProductosId",
                table: "ColeccionProducto",
                column: "ProductosId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ColeccionProducto");

            migrationBuilder.DropTable(
                name: "Colecciones");

            migrationBuilder.DropColumn(
                name: "PrecioOferta",
                table: "Productos");
        }
    }
}
