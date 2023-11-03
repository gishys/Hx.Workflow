using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Migrations
{
    /// <inheritdoc />
    public partial class init11 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "HXWKDEFINITIONS",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "char(36)", nullable: false, comment: "主键", collation: "ascii_general_ci"),
                    VERSION = table.Column<int>(type: "int", precision: 9, nullable: false, comment: "版本号"),
                    TITLE = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true, comment: "标题")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LIMITTIME = table.Column<int>(type: "int", nullable: true, comment: "限制时间"),
                    WKDEFINITIONSTATE = table.Column<int>(type: "int", precision: 1, nullable: false, comment: "是否开启"),
                    ICON = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: true, comment: "图标路径")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    COLOR = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: true, comment: "显示颜色")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    GROUPID = table.Column<Guid>(type: "char(36)", nullable: true, comment: "属于组", collation: "ascii_general_ci"),
                    DISCRIPTION = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true, comment: "定义描述")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SORTNUMBER = table.Column<int>(type: "int", nullable: false, comment: "排序"),
                    TENANTID = table.Column<Guid>(type: "char(36)", nullable: true, comment: "租户Id", collation: "ascii_general_ci"),
                    ExtraProperties = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ConcurrencyStamp = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreationTime = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreatorId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    LastModificationTime = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    DeletionTime = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("Pk_WkDefinition", x => x.ID);
                },
                comment: "工作流定义")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "HXWKEVENTS",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    EVENTNAME = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EVENTKEY = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EVENTDATA = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EVENTTIME = table.Column<DateTime>(type: "datetime", nullable: false),
                    ISPROCESSED = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    TENANTID = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    CreationTime = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreatorId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HXWKEVENTS", x => x.ID);
                },
                comment: "流程事件")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "HXWKEXECUTIONERRORS",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    WKINSTANCEID = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    WKEXECUTIONPOINTERID = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ERRORTIME = table.Column<DateTime>(type: "datetime", nullable: false),
                    MESSAGE = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TENANTID = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HXWKEXECUTIONERRORS", x => x.ID);
                },
                comment: "执行错误")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "HXWKSTEPBODYS",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    NAME = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DISPLAYNAME = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TYPEFULLNAME = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ASSEMBLYFULLNAME = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreationTime = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreatorId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    LastModificationTime = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    DeletionTime = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HXWKSTEPBODYS", x => x.Id);
                },
                comment: "节点实体")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "HXWKINSTANCES",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    WKDIFINITIONID = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    VERSION = table.Column<int>(type: "int", nullable: false),
                    DESCRIPTION = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    REFERENCE = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    NEXTEXECUTION = table.Column<long>(type: "bigint", nullable: true),
                    STATUS = table.Column<int>(type: "int", precision: 1, nullable: false),
                    DATA = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CREATETIME = table.Column<DateTime>(type: "datetime", nullable: false),
                    COMPLETETIME = table.Column<DateTime>(type: "datetime", nullable: true),
                    TENANTID = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    ExtraProperties = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ConcurrencyStamp = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreationTime = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreatorId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    LastModificationTime = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    DeletionTime = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HXWKINSTANCES", x => x.ID);
                    table.ForeignKey(
                        name: "Pk_Instance_Definition",
                        column: x => x.WKDIFINITIONID,
                        principalTable: "HXWKDEFINITIONS",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "流程实例")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "HXWKNODES",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    WkStepBodyId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    WKDIFINITIONID = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    NAME = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DISPLAYNAME = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    STEPNODETYPE = table.Column<int>(type: "int", precision: 1, nullable: false),
                    VERSION = table.Column<int>(type: "int", nullable: false),
                    LIMITTIME = table.Column<int>(type: "int", nullable: true),
                    NODEFORMTYPE = table.Column<int>(type: "int", precision: 1, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HXWKNODES", x => x.ID);
                    table.ForeignKey(
                        name: "Pk_WkDef_WkNode",
                        column: x => x.WKDIFINITIONID,
                        principalTable: "HXWKDEFINITIONS",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "Pk_WkNode_WkStepBody",
                        column: x => x.WkStepBodyId,
                        principalTable: "HXWKSTEPBODYS",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "执行节点")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "HXWKSTEPBODYPARAMS",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    WkNodeId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    KEY = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    StepBodyParaType = table.Column<int>(type: "int", nullable: false),
                    NAME = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DISPLAYNAME = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    VALUE = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HXWKSTEPBODYPARAMS", x => x.ID);
                    table.ForeignKey(
                        name: "Pk_WkStepBody_WkParam",
                        column: x => x.WkNodeId,
                        principalTable: "HXWKSTEPBODYS",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "节点参数")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "HXWKEXECUTIONPOINTER",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    WKINSTANCEID = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    STEPID = table.Column<int>(type: "int", nullable: false),
                    ACTIVE = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    SLEEPUNTIL = table.Column<DateTime>(type: "datetime", nullable: true),
                    PERSISTENCEDATA = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    STARTTIME = table.Column<DateTime>(type: "datetime", nullable: true),
                    ENDTIME = table.Column<DateTime>(type: "datetime", nullable: true),
                    EVENTNAME = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EVENTKEY = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EVENTPUBLISHED = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    EVENTDATA = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    STEPNAME = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RETRYCOUNT = table.Column<int>(type: "int", nullable: false),
                    CHILDREN = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CONTEXTITEM = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PREDECESSORID = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    OUTCOME = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    STATUS = table.Column<int>(type: "int", precision: 2, nullable: false),
                    SCOPE = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TENANTID = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    CreationTime = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreatorId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    LastModificationTime = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    DeletionTime = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HXWKEXECUTIONPOINTER", x => x.ID);
                    table.ForeignKey(
                        name: "Pk_Instance_Pointer",
                        column: x => x.WKINSTANCEID,
                        principalTable: "HXWKINSTANCES",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "执行节点实例")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "HXAPPLICATIONFORMS",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    WKNODEID = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    PARENTID = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    CODE = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    NAME = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DISPALYNAME = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    APPLICATIONTYPE = table.Column<int>(type: "int", precision: 1, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HXAPPLICATIONFORMS", x => x.ID);
                    table.ForeignKey(
                        name: "Pk_WkNode_App",
                        column: x => x.WKNODEID,
                        principalTable: "HXWKNODES",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "流程表单")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "HXWKCONDITIONNODES",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    WKNODEID = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    LABEL = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    NEXTNODENAME = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HXWKCONDITIONNODES", x => x.ID);
                    table.ForeignKey(
                        name: "Pk_WkNode_Candition",
                        column: x => x.WKNODEID,
                        principalTable: "HXWKNODES",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "节点条件")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "HXWKNODEPARAS",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    WKNODEID = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    KEY = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    VALUE = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HXWKNODEPARAS", x => x.ID);
                    table.ForeignKey(
                        name: "Pk_WkNode_OutcomeSteps",
                        column: x => x.WKNODEID,
                        principalTable: "HXWKNODES",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "步骤参数")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "HXWKPOINTS",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    WkNodeId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    LEFT = table.Column<int>(type: "int", nullable: false),
                    RIGHT = table.Column<int>(type: "int", nullable: false),
                    TOP = table.Column<int>(type: "int", nullable: false),
                    BOTTOM = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HXWKPOINTS", x => x.ID);
                    table.ForeignKey(
                        name: "Pk_WkNode_WkPoint",
                        column: x => x.WkNodeId,
                        principalTable: "HXWKNODES",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "HXWKAUDITORS",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    WORKFLOWID = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    EXECUTIONPOINTERID = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    STATUS = table.Column<int>(type: "int", precision: 1, nullable: false),
                    AUDITTIME = table.Column<DateTime>(type: "datetime", nullable: true),
                    REMARK = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    USERID = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    USERNAME = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TENANTID = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    CreationTime = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreatorId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    LastModificationTime = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    DeleterId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    DeletionTime = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HXWKAUDITORS", x => x.Id);
                    table.ForeignKey(
                        name: "Pk_WkAuditor_ExecPointer",
                        column: x => x.EXECUTIONPOINTERID,
                        principalTable: "HXWKEXECUTIONPOINTER",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "Pk_WkAuditor_WkInstance",
                        column: x => x.WORKFLOWID,
                        principalTable: "HXWKINSTANCES",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "HXWKCANDIDATES",
                columns: table => new
                {
                    NODEID = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    CANDIDATEID = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    USERNAME = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DISPLAYUSERNAME = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DEFAULTSELECTION = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HXWKCANDIDATES", x => new { x.NODEID, x.CANDIDATEID });
                    table.ForeignKey(
                        name: "Pk_Pointer_Candidate",
                        column: x => x.CANDIDATEID,
                        principalTable: "HXWKEXECUTIONPOINTER",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "Pk_WkDef_Candidate",
                        column: x => x.NODEID,
                        principalTable: "HXWKDEFINITIONS",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "Pk_WkNode_Candidate",
                        column: x => x.NODEID,
                        principalTable: "HXWKNODES",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "HXWKEXTENSIONATTRIBUTES",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    EXECUTIONPOINTERID = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ATTRIBUTEKEY = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ATTRIBUTEVALUE = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TENANTID = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HXWKEXTENSIONATTRIBUTES", x => x.ID);
                    table.ForeignKey(
                        name: "Pk_Pointer_Attribute",
                        column: x => x.EXECUTIONPOINTERID,
                        principalTable: "HXWKEXECUTIONPOINTER",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "执行节点属性")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "HXWKSUBSCRIPTIONS",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    WORKFLOWID = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    STEPID = table.Column<int>(type: "int", nullable: false),
                    EXECUTIONPOINTERID = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    EVENTNAME = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EVENTKEY = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SUBSCRIBEASOF = table.Column<DateTime>(type: "datetime", nullable: false),
                    SUBSCRIPTIONDATA = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EXTERNALTOKEN = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EXTERNALWORKERID = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EXTERNALTOKENEXPIRY = table.Column<DateTime>(type: "datetime", nullable: true),
                    TENANTID = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HXWKSUBSCRIPTIONS", x => x.ID);
                    table.ForeignKey(
                        name: "Pk_Pointer_Subscript",
                        column: x => x.EXECUTIONPOINTERID,
                        principalTable: "HXWKEXECUTIONPOINTER",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "发布")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "HXWKCONNODECONDITIONS",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    WKCONDITIONNODEID = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    FIELD = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    OPERATOR = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    VALUE = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HXWKCONNODECONDITIONS", x => x.ID);
                    table.ForeignKey(
                        name: "Pk_Candition_ConCondition",
                        column: x => x.WKCONDITIONNODEID,
                        principalTable: "HXWKCONDITIONNODES",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "条件集合")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_HXAPPLICATIONFORMS_WKNODEID",
                table: "HXAPPLICATIONFORMS",
                column: "WKNODEID");

            migrationBuilder.CreateIndex(
                name: "IX_HXWKAUDITORS_EXECUTIONPOINTERID",
                table: "HXWKAUDITORS",
                column: "EXECUTIONPOINTERID");

            migrationBuilder.CreateIndex(
                name: "IX_HXWKAUDITORS_WORKFLOWID",
                table: "HXWKAUDITORS",
                column: "WORKFLOWID");

            migrationBuilder.CreateIndex(
                name: "IX_HXWKCANDIDATES_CANDIDATEID",
                table: "HXWKCANDIDATES",
                column: "CANDIDATEID");

            migrationBuilder.CreateIndex(
                name: "IX_HXWKCONDITIONNODES_WKNODEID",
                table: "HXWKCONDITIONNODES",
                column: "WKNODEID");

            migrationBuilder.CreateIndex(
                name: "IX_HXWKCONNODECONDITIONS_WKCONDITIONNODEID",
                table: "HXWKCONNODECONDITIONS",
                column: "WKCONDITIONNODEID");

            migrationBuilder.CreateIndex(
                name: "IX_HXWKEXECUTIONPOINTER_WKINSTANCEID",
                table: "HXWKEXECUTIONPOINTER",
                column: "WKINSTANCEID");

            migrationBuilder.CreateIndex(
                name: "IX_HXWKEXTENSIONATTRIBUTES_EXECUTIONPOINTERID",
                table: "HXWKEXTENSIONATTRIBUTES",
                column: "EXECUTIONPOINTERID");

            migrationBuilder.CreateIndex(
                name: "IX_HXWKINSTANCES_WKDIFINITIONID",
                table: "HXWKINSTANCES",
                column: "WKDIFINITIONID");

            migrationBuilder.CreateIndex(
                name: "IX_HXWKNODEPARAS_WKNODEID",
                table: "HXWKNODEPARAS",
                column: "WKNODEID");

            migrationBuilder.CreateIndex(
                name: "IX_HXWKNODES_WKDIFINITIONID",
                table: "HXWKNODES",
                column: "WKDIFINITIONID");

            migrationBuilder.CreateIndex(
                name: "IX_HXWKNODES_WkStepBodyId",
                table: "HXWKNODES",
                column: "WkStepBodyId");

            migrationBuilder.CreateIndex(
                name: "IX_HXWKPOINTS_WkNodeId",
                table: "HXWKPOINTS",
                column: "WkNodeId");

            migrationBuilder.CreateIndex(
                name: "IX_HXWKSTEPBODYPARAMS_WkNodeId",
                table: "HXWKSTEPBODYPARAMS",
                column: "WkNodeId");

            migrationBuilder.CreateIndex(
                name: "IX_HXWKSUBSCRIPTIONS_EXECUTIONPOINTERID",
                table: "HXWKSUBSCRIPTIONS",
                column: "EXECUTIONPOINTERID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HXAPPLICATIONFORMS");

            migrationBuilder.DropTable(
                name: "HXWKAUDITORS");

            migrationBuilder.DropTable(
                name: "HXWKCANDIDATES");

            migrationBuilder.DropTable(
                name: "HXWKCONNODECONDITIONS");

            migrationBuilder.DropTable(
                name: "HXWKEVENTS");

            migrationBuilder.DropTable(
                name: "HXWKEXECUTIONERRORS");

            migrationBuilder.DropTable(
                name: "HXWKEXTENSIONATTRIBUTES");

            migrationBuilder.DropTable(
                name: "HXWKNODEPARAS");

            migrationBuilder.DropTable(
                name: "HXWKPOINTS");

            migrationBuilder.DropTable(
                name: "HXWKSTEPBODYPARAMS");

            migrationBuilder.DropTable(
                name: "HXWKSUBSCRIPTIONS");

            migrationBuilder.DropTable(
                name: "HXWKCONDITIONNODES");

            migrationBuilder.DropTable(
                name: "HXWKEXECUTIONPOINTER");

            migrationBuilder.DropTable(
                name: "HXWKNODES");

            migrationBuilder.DropTable(
                name: "HXWKINSTANCES");

            migrationBuilder.DropTable(
                name: "HXWKSTEPBODYS");

            migrationBuilder.DropTable(
                name: "HXWKDEFINITIONS");
        }
    }
}
