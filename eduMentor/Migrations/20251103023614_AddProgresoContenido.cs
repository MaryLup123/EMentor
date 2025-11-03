using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace eduMentor.Migrations
{
    /// <inheritdoc />
    public partial class AddProgresoContenido : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProgresoContenido",
                columns: table => new
                {
                    IdProgresoContenido = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdContenido = table.Column<int>(type: "integer", nullable: false),
                    IdEstudiante = table.Column<int>(type: "integer", nullable: false),
                    Completado = table.Column<bool>(type: "boolean", nullable: false),
                    FechaCompletado = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Peso = table.Column<double>(type: "double precision", nullable: false),
                    ModuloIdModulo = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProgresoContenido", x => x.IdProgresoContenido);
                    table.ForeignKey(
                        name: "FK_ProgresoContenido_AspNetUsers_IdEstudiante",
                        column: x => x.IdEstudiante,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProgresoContenido_Contenido_IdContenido",
                        column: x => x.IdContenido,
                        principalTable: "Contenido",
                        principalColumn: "IdContenido",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProgresoContenido_Modulo_ModuloIdModulo",
                        column: x => x.ModuloIdModulo,
                        principalTable: "Modulo",
                        principalColumn: "IdModulo");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProgresoContenido_IdContenido",
                table: "ProgresoContenido",
                column: "IdContenido");

            migrationBuilder.CreateIndex(
                name: "IX_ProgresoContenido_IdEstudiante",
                table: "ProgresoContenido",
                column: "IdEstudiante");

            migrationBuilder.CreateIndex(
                name: "IX_ProgresoContenido_ModuloIdModulo",
                table: "ProgresoContenido",
                column: "ModuloIdModulo");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProgresoContenido");
        }
    }
}
