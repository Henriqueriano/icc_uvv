using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Data.Migrations
{
    /// <inheritdoc />
    public partial class CreateOntologies : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ontologies",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    iri = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    description = table.Column<string>(type: "text", nullable: false),
                    documentation = table.Column<string>(type: "text", nullable: false),
                    source_document = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ontologies", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "ontology_author_images",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    ontology_id = table.Column<Guid>(type: "uuid", nullable: false),
                    author_name = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    image_url = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ontology_author_images", x => x.id);
                    table.ForeignKey(
                        name: "FK_ontology_author_images_ontologies_ontology_id",
                        column: x => x.ontology_id,
                        principalTable: "ontologies",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ontologies_iri",
                table: "ontologies",
                column: "iri",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ontology_author_images_ontology_id_author_name",
                table: "ontology_author_images",
                columns: new[] { "ontology_id", "author_name" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ontology_author_images");

            migrationBuilder.DropTable(
                name: "ontologies");
        }
    }
}
