using Hx.Workflow.EntityFrameworkCore.DbMigrations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using System;

#nullable disable

namespace Migrations
{
    [DbContext(typeof(WkDbMigrationsContext))]
    [Migration("20260810000000_AddActivitySubmissionOutbox")]
    public partial class AddActivitySubmissionOutbox : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "HXWKACTIVITYSUBMISSIONS",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    WORKFLOWID = table.Column<Guid>(type: "uuid", nullable: false),
                    ACTIVITYNAME = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    PAYLOAD = table.Column<string>(type: "text", nullable: false),
                    REQUESTHASH = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    STATUS = table.Column<int>(type: "integer", nullable: false),
                    ERROR = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CREATIONTIME = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LASTMODIFICATIONTIME = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LOCKEDUNTIL = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ATTEMPTCOUNT = table.Column<int>(type: "integer", nullable: false),
                    TENANTID = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WKACTIVITYSUBMISSIONS", x => x.ID);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WKACTIVITYSUBMISSIONS_PROCESSABLE",
                table: "HXWKACTIVITYSUBMISSIONS",
                columns: new[] { "STATUS", "LOCKEDUNTIL" });

            migrationBuilder.CreateIndex(
                name: "UX_WKACTIVITYSUBMISSIONS_WORKFLOW_ACTIVITY",
                table: "HXWKACTIVITYSUBMISSIONS",
                columns: new[] { "WORKFLOWID", "ACTIVITYNAME" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "HXWKACTIVITYSUBMISSIONS");
        }
    }
}
