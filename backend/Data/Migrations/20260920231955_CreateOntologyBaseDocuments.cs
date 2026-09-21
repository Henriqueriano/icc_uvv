using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Data.Migrations
{
    /// <inheritdoc />
    public partial class CreateOntologyBaseDocuments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ontology_base_documents",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    ontology_id = table.Column<Guid>(type: "uuid", nullable: false),
                    link = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    description = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ontology_base_documents", x => x.id);
                    table.ForeignKey(
                        name: "FK_ontology_base_documents_ontologies_ontology_id",
                        column: x => x.ontology_id,
                        principalTable: "ontologies",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ontology_base_documents_ontology_id_link",
                table: "ontology_base_documents",
                columns: new[] { "ontology_id", "link" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ontology_base_documents");
        }
    }
}
