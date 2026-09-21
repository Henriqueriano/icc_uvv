using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Data.Migrations
{
    /// <inheritdoc />
    public partial class RenameOntologyAuthorsToPortfolios : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE ontology_author_images SET image_url = 'https://www.google.com/search?q=' || replace(author_name, ' ', '+') WHERE image_url IS NULL OR image_url = '';");
            migrationBuilder.RenameTable(name: "ontology_author_images", newName: "ontology_author_portfolios");
            migrationBuilder.RenameIndex(name: "IX_ontology_author_images_ontology_id_author_name", table: "ontology_author_portfolios", newName: "IX_ontology_author_portfolios_ontology_id_author_name");
            migrationBuilder.RenameColumn(name: "image_url", table: "ontology_author_portfolios", newName: "portfolio_url");
            migrationBuilder.AlterColumn<string>(
                name: "portfolio_url",
                table: "ontology_author_portfolios",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(2000)",
                oldMaxLength: 2000,
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "portfolio_url",
                table: "ontology_author_portfolios",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(2000)",
                oldMaxLength: 2000);
            migrationBuilder.RenameColumn(name: "portfolio_url", table: "ontology_author_portfolios", newName: "image_url");
            migrationBuilder.RenameIndex(name: "IX_ontology_author_portfolios_ontology_id_author_name", table: "ontology_author_portfolios", newName: "IX_ontology_author_images_ontology_id_author_name");
            migrationBuilder.RenameTable(name: "ontology_author_portfolios", newName: "ontology_author_images");
        }
    }
}
