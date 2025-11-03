using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace eduMentor.Migrations
{
    /// <inheritdoc />
    public partial class ForceFullSync : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FechaCompletado",
                table: "ProgresoModulo");

            migrationBuilder.RenameColumn(
                name: "Contenido",
                table: "Modulo",
                newName: "Descripcion");

            migrationBuilder.AddColumn<double>(
                name: "PorcentajeAvance",
                table: "ProgresoModulo",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<int>(
                name: "TareasCompletadas",
                table: "ProgresoModulo",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TotalTareas",
                table: "ProgresoModulo",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "UltimaActualizacion",
                table: "ProgresoModulo",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateTable(
                name: "Contenido",
                columns: table => new
                {
                    IdContenido = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdModulo = table.Column<int>(type: "integer", nullable: false),
                    Titulo = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Tipo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    JsonContenido = table.Column<string>(type: "jsonb", nullable: false),
                    EsTarea = table.Column<bool>(type: "boolean", nullable: false),
                    Peso = table.Column<double>(type: "double precision", nullable: false),
                    Orden = table.Column<int>(type: "integer", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Contenido", x => x.IdContenido);
                    table.ForeignKey(
                        name: "FK_Contenido_Modulo_IdModulo",
                        column: x => x.IdModulo,
                        principalTable: "Modulo",
                        principalColumn: "IdModulo",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Contenido_IdModulo",
                table: "Contenido",
                column: "IdModulo");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Contenido");

            migrationBuilder.DropColumn(
                name: "PorcentajeAvance",
                table: "ProgresoModulo");

            migrationBuilder.DropColumn(
                name: "TareasCompletadas",
                table: "ProgresoModulo");

            migrationBuilder.DropColumn(
                name: "TotalTareas",
                table: "ProgresoModulo");

            migrationBuilder.DropColumn(
                name: "UltimaActualizacion",
                table: "ProgresoModulo");

            migrationBuilder.RenameColumn(
                name: "Descripcion",
                table: "Modulo",
                newName: "Contenido");

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaCompletado",
                table: "ProgresoModulo",
                type: "timestamp with time zone",
                nullable: true);
        }
    }
}
