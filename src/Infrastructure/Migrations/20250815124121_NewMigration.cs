using System;
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
            migrationBuilder.EnsureSchema(
                name: "dbo");

            migrationBuilder.CreateTable(
                name: "Area",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    ManagerId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Area", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ChatMessages",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    User = table.Column<string>(type: "TEXT", nullable: false),
                    Message = table.Column<string>(type: "TEXT", nullable: false),
                    SentAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatMessages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Manager",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Email = table.Column<string>(type: "TEXT", maxLength: 70, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Manager", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Notifications",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserEmail = table.Column<string>(type: "TEXT", nullable: false),
                    Message = table.Column<string>(type: "TEXT", nullable: false),
                    SentAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Responsible",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Email = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    AreaId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Responsible", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Responsible_Area_AreaId",
                        column: x => x.AreaId,
                        principalSchema: "dbo",
                        principalTable: "Area",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ApplicationData",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    AreaId = table.Column<int>(type: "INTEGER", nullable: false),
                    ResponsibleId = table.Column<int>(type: "INTEGER", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    SquadId = table.Column<int>(type: "INTEGER", nullable: false),
                    External = table.Column<bool>(type: "INTEGER", nullable: false),
                    ProductOwner = table.Column<string>(type: "TEXT", nullable: true),
                    KnowledgeId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationData", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApplicationData_Area_AreaId",
                        column: x => x.AreaId,
                        principalSchema: "dbo",
                        principalTable: "Area",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ApplicationData_Responsible_ResponsibleId",
                        column: x => x.ResponsibleId,
                        principalSchema: "dbo",
                        principalTable: "Responsible",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DocumentData",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Url = table.Column<string>(type: "TEXT", nullable: false),
                    ApplicationId = table.Column<int>(type: "INTEGER", nullable: false),
                    ApplicationDataId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentData", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DocumentData_ApplicationData_ApplicationDataId",
                        column: x => x.ApplicationDataId,
                        principalSchema: "dbo",
                        principalTable: "ApplicationData",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DocumentData_ApplicationData_ApplicationId",
                        column: x => x.ApplicationId,
                        principalSchema: "dbo",
                        principalTable: "ApplicationData",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Feedback",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Title = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    ApplicationId = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ClosedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Status = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Feedback", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Feedback_ApplicationData_ApplicationId",
                        column: x => x.ApplicationId,
                        principalSchema: "dbo",
                        principalTable: "ApplicationData",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Incident",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Title = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    ApplicationId = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ClosedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Status = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Incident", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Incident_ApplicationData_ApplicationId",
                        column: x => x.ApplicationId,
                        principalSchema: "dbo",
                        principalTable: "ApplicationData",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Integration",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    ApplicationDataId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Integration", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Integration_ApplicationData_ApplicationDataId",
                        column: x => x.ApplicationDataId,
                        principalSchema: "dbo",
                        principalTable: "ApplicationData",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Repo",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 5000, nullable: false),
                    Url = table.Column<string>(type: "TEXT", nullable: false),
                    ApplicationId = table.Column<int>(type: "INTEGER", nullable: false),
                    ApplicationDataId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Repo", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Repo_ApplicationData_ApplicationDataId",
                        column: x => x.ApplicationDataId,
                        principalSchema: "dbo",
                        principalTable: "ApplicationData",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Repo_ApplicationData_ApplicationId",
                        column: x => x.ApplicationId,
                        principalSchema: "dbo",
                        principalTable: "ApplicationData",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ServiceData",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    ApplicationId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceData", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ServiceData_ApplicationData_ApplicationId",
                        column: x => x.ApplicationId,
                        principalSchema: "dbo",
                        principalTable: "ApplicationData",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FeedbackMembers",
                schema: "dbo",
                columns: table => new
                {
                    FeedbackId = table.Column<int>(type: "INTEGER", nullable: false),
                    MemberId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeedbackMembers", x => new { x.FeedbackId, x.MemberId });
                    table.ForeignKey(
                        name: "FK_FeedbackMembers_Feedback_FeedbackId",
                        column: x => x.FeedbackId,
                        principalSchema: "dbo",
                        principalTable: "Feedback",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "IncidentMembers",
                schema: "dbo",
                columns: table => new
                {
                    IncidentId = table.Column<int>(type: "INTEGER", nullable: false),
                    MemberId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IncidentMembers", x => new { x.IncidentId, x.MemberId });
                    table.ForeignKey(
                        name: "FK_IncidentMembers_Incident_IncidentId",
                        column: x => x.IncidentId,
                        principalSchema: "dbo",
                        principalTable: "Incident",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Knowledge",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    MemberId = table.Column<int>(type: "INTEGER", nullable: false),
                    ApplicationId = table.Column<int>(type: "INTEGER", nullable: false),
                    SquadId = table.Column<int>(type: "INTEGER", nullable: false),
                    AssociatedSquadIds = table.Column<string>(type: "TEXT", nullable: true),
                    AssociatedApplicationIds = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Knowledge", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Knowledge_ApplicationData_ApplicationId",
                        column: x => x.ApplicationId,
                        principalSchema: "dbo",
                        principalTable: "ApplicationData",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Squads",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 55, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Cost = table.Column<decimal>(type: "TEXT", nullable: true),
                    KnowledgeId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Squads", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Squads_Knowledge_KnowledgeId",
                        column: x => x.KnowledgeId,
                        principalSchema: "dbo",
                        principalTable: "Knowledge",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Members",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    Role = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Cost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Email = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    SquadId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Members", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Members_Squads_SquadId",
                        column: x => x.SquadId,
                        principalSchema: "dbo",
                        principalTable: "Squads",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationData_AreaId",
                schema: "dbo",
                table: "ApplicationData",
                column: "AreaId");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationData_KnowledgeId",
                schema: "dbo",
                table: "ApplicationData",
                column: "KnowledgeId");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationData_ResponsibleId",
                schema: "dbo",
                table: "ApplicationData",
                column: "ResponsibleId");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationData_SquadId",
                schema: "dbo",
                table: "ApplicationData",
                column: "SquadId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentData_ApplicationDataId",
                schema: "dbo",
                table: "DocumentData",
                column: "ApplicationDataId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentData_ApplicationId",
                schema: "dbo",
                table: "DocumentData",
                column: "ApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_Feedback_ApplicationId",
                schema: "dbo",
                table: "Feedback",
                column: "ApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_FeedbackMembers_MemberId",
                schema: "dbo",
                table: "FeedbackMembers",
                column: "MemberId");

            migrationBuilder.CreateIndex(
                name: "IX_Incident_ApplicationId",
                schema: "dbo",
                table: "Incident",
                column: "ApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_IncidentMembers_MemberId",
                schema: "dbo",
                table: "IncidentMembers",
                column: "MemberId");

            migrationBuilder.CreateIndex(
                name: "IX_Integration_ApplicationDataId",
                schema: "dbo",
                table: "Integration",
                column: "ApplicationDataId");

            migrationBuilder.CreateIndex(
                name: "IX_Knowledge_ApplicationId",
                schema: "dbo",
                table: "Knowledge",
                column: "ApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_Knowledge_MemberId_ApplicationId",
                schema: "dbo",
                table: "Knowledge",
                columns: new[] { "MemberId", "ApplicationId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Knowledge_SquadId",
                schema: "dbo",
                table: "Knowledge",
                column: "SquadId");

            migrationBuilder.CreateIndex(
                name: "IX_Members_SquadId",
                schema: "dbo",
                table: "Members",
                column: "SquadId");

            migrationBuilder.CreateIndex(
                name: "IX_Repo_ApplicationDataId",
                schema: "dbo",
                table: "Repo",
                column: "ApplicationDataId");

            migrationBuilder.CreateIndex(
                name: "IX_Repo_ApplicationId",
                schema: "dbo",
                table: "Repo",
                column: "ApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_Responsible_AreaId",
                schema: "dbo",
                table: "Responsible",
                column: "AreaId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceData_ApplicationId",
                schema: "dbo",
                table: "ServiceData",
                column: "ApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_Squads_KnowledgeId",
                schema: "dbo",
                table: "Squads",
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
                name: "FK_ApplicationData_Squads_SquadId",
                schema: "dbo",
                table: "ApplicationData",
                column: "SquadId",
                principalSchema: "dbo",
                principalTable: "Squads",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_FeedbackMembers_Members_MemberId",
                schema: "dbo",
                table: "FeedbackMembers",
                column: "MemberId",
                principalSchema: "dbo",
                principalTable: "Members",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_IncidentMembers_Members_MemberId",
                schema: "dbo",
                table: "IncidentMembers",
                column: "MemberId",
                principalSchema: "dbo",
                principalTable: "Members",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Knowledge_Members_MemberId",
                schema: "dbo",
                table: "Knowledge",
                column: "MemberId",
                principalSchema: "dbo",
                principalTable: "Members",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Knowledge_Squads_SquadId",
                schema: "dbo",
                table: "Knowledge",
                column: "SquadId",
                principalSchema: "dbo",
                principalTable: "Squads",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ApplicationData_Area_AreaId",
                schema: "dbo",
                table: "ApplicationData");

            migrationBuilder.DropForeignKey(
                name: "FK_Responsible_Area_AreaId",
                schema: "dbo",
                table: "Responsible");

            migrationBuilder.DropForeignKey(
                name: "FK_ApplicationData_Knowledge_KnowledgeId",
                schema: "dbo",
                table: "ApplicationData");

            migrationBuilder.DropForeignKey(
                name: "FK_Squads_Knowledge_KnowledgeId",
                schema: "dbo",
                table: "Squads");

            migrationBuilder.DropTable(
                name: "ChatMessages",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "DocumentData",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "FeedbackMembers",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "IncidentMembers",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Integration",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Manager",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Notifications",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Repo",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "ServiceData",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Feedback",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Incident",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Area",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Knowledge",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "ApplicationData",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Members",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Responsible",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Squads",
                schema: "dbo");
        }
    }
}
