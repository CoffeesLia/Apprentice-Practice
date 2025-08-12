using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Stellantis.ProjectName.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class NewMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Knowledge_Squads_AssociatedSquadId",
                schema: "dbo",
                table: "Knowledge");

            migrationBuilder.RenameColumn(
                name: "AssociatedSquadId",
                schema: "dbo",
                table: "Knowledge",
                newName: "SquadId");

            migrationBuilder.RenameIndex(
                name: "IX_Knowledge_AssociatedSquadId",
                schema: "dbo",
                table: "Knowledge",
                newName: "IX_Knowledge_SquadId");

            migrationBuilder.AddColumn<int>(
                name: "KnowledgeId",
                schema: "dbo",
                table: "Squads",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AssociatedApplicationIds",
                schema: "dbo",
                table: "Knowledge",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AssociatedSquadIds",
                schema: "dbo",
                table: "Knowledge",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "KnowledgeId",
                schema: "dbo",
                table: "ApplicationData",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Squads_KnowledgeId",
                schema: "dbo",
                table: "Squads",
                column: "KnowledgeId");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationData_KnowledgeId",
                schema: "dbo",
                table: "ApplicationData",
                column: "KnowledgeId");

            migrationBuilder.AddForeignKey(
                name: "FK_ApplicationData_Knowledge_KnowledgeId",
                schema: "dbo",
                table: "ApplicationData",
                column: "KnowledgeId",
                principalSchema: "dbo",
                principalTable: "Knowledge",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Knowledge_Squads_SquadId",
                schema: "dbo",
                table: "Knowledge",
                column: "SquadId",
                principalSchema: "dbo",
                principalTable: "Squads",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Squads_Knowledge_KnowledgeId",
                schema: "dbo",
                table: "Squads",
                column: "KnowledgeId",
                principalSchema: "dbo",
                principalTable: "Knowledge",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ApplicationData_Knowledge_KnowledgeId",
                schema: "dbo",
                table: "ApplicationData");

            migrationBuilder.DropForeignKey(
                name: "FK_Knowledge_Squads_SquadId",
                schema: "dbo",
                table: "Knowledge");

            migrationBuilder.DropForeignKey(
                name: "FK_Squads_Knowledge_KnowledgeId",
                schema: "dbo",
                table: "Squads");

            migrationBuilder.DropIndex(
                name: "IX_Squads_KnowledgeId",
                schema: "dbo",
                table: "Squads");

            migrationBuilder.DropIndex(
                name: "IX_ApplicationData_KnowledgeId",
                schema: "dbo",
                table: "ApplicationData");

            migrationBuilder.DropColumn(
                name: "KnowledgeId",
                schema: "dbo",
                table: "Squads");

            migrationBuilder.DropColumn(
                name: "AssociatedApplicationIds",
                schema: "dbo",
                table: "Knowledge");

            migrationBuilder.DropColumn(
                name: "AssociatedSquadIds",
                schema: "dbo",
                table: "Knowledge");

            migrationBuilder.DropColumn(
                name: "KnowledgeId",
                schema: "dbo",
                table: "ApplicationData");

            migrationBuilder.RenameColumn(
                name: "SquadId",
                schema: "dbo",
                table: "Knowledge",
                newName: "AssociatedSquadId");

            migrationBuilder.RenameIndex(
                name: "IX_Knowledge_SquadId",
                schema: "dbo",
                table: "Knowledge",
                newName: "IX_Knowledge_AssociatedSquadId");

            migrationBuilder.AddForeignKey(
                name: "FK_Knowledge_Squads_AssociatedSquadId",
                schema: "dbo",
                table: "Knowledge",
                column: "AssociatedSquadId",
                principalSchema: "dbo",
                principalTable: "Squads",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
