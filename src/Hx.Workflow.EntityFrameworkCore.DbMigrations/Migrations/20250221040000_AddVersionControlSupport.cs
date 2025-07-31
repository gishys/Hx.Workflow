using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hx.Workflow.EntityFrameworkCore.DbMigrations.Migrations
{
    /// <inheritdoc />
    public partial class AddVersionControlSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 删除现有的主键约束
            migrationBuilder.DropPrimaryKey(
                name: "PK_WKDEFINITION",
                table: "WKDEFINITIONS");

            // 添加复合主键
            migrationBuilder.AddPrimaryKey(
                name: "PK_WKDEFINITION",
                table: "WKDEFINITIONS",
                columns: new[] { "ID", "VERSION" });

            // 添加索引以提高查询性能
            migrationBuilder.CreateIndex(
                name: "IX_WKDEFINITIONS_ID_VERSION",
                table: "WKDEFINITIONS",
                columns: new[] { "ID", "VERSION" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WKDEFINITIONS_ID",
                table: "WKDEFINITIONS",
                column: "ID");

            // 添加版本号默认值约束
            migrationBuilder.AddCheckConstraint(
                name: "CK_WKDEFINITIONS_VERSION_POSITIVE",
                table: "WKDEFINITIONS",
                sql: "VERSION > 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // 删除检查约束
            migrationBuilder.DropCheckConstraint(
                name: "CK_WKDEFINITIONS_VERSION_POSITIVE",
                table: "WKDEFINITIONS");

            // 删除索引
            migrationBuilder.DropIndex(
                name: "IX_WKDEFINITIONS_ID",
                table: "WKDEFINITIONS");

            migrationBuilder.DropIndex(
                name: "IX_WKDEFINITIONS_ID_VERSION",
                table: "WKDEFINITIONS");

            // 删除复合主键
            migrationBuilder.DropPrimaryKey(
                name: "PK_WKDEFINITION",
                table: "WKDEFINITIONS");

            // 恢复单一主键
            migrationBuilder.AddPrimaryKey(
                name: "PK_WKDEFINITION",
                table: "WKDEFINITIONS",
                column: "ID");
        }
    }
} 