using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddOntologyProfileFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ProfileArea",
                table: "ontologies",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ProfileName",
                table: "ontologies",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ProfileResume",
                table: "ontologies",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ProfileSource",
                table: "ontologies",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql("""
                UPDATE ontologies
                SET "ProfileName" = name,
                    "ProfileArea" = 'Modelagem conceitual e ontologias',
                    "ProfileResume" = description,
                    "ProfileSource" = 'Documentação da ontologia'
                WHERE "ProfileName" = '';
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProfileArea",
                table: "ontologies");

            migrationBuilder.DropColumn(
                name: "ProfileName",
                table: "ontologies");

            migrationBuilder.DropColumn(
                name: "ProfileResume",
                table: "ontologies");

            migrationBuilder.DropColumn(
                name: "ProfileSource",
                table: "ontologies");
        }
    }
}
