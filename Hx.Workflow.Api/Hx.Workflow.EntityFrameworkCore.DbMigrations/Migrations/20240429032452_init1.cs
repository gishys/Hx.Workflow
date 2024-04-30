using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Migrations
{
    /// <inheritdoc />
    public partial class init1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "HXWKDEFINITIONS",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false, comment: "主键"),
                    VERSION = table.Column<int>(type: "integer", precision: 9, nullable: false, comment: "版本号"),
                    TITLE = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true, comment: "标题"),
                    LIMITTIME = table.Column<int>(type: "integer", nullable: true, comment: "限制时间"),
                    WKDEFINITIONSTATE = table.Column<int>(type: "integer", precision: 1, nullable: false, comment: "是否开启"),
                    ICON = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true, comment: "图标路径"),
                    COLOR = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true, comment: "显示颜色"),
                    GROUPID = table.Column<Guid>(type: "uuid", nullable: true, comment: "属于组"),
                    DISCRIPTION = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true, comment: "定义描述"),
                    SORTNUMBER = table.Column<int>(type: "integer", nullable: false, comment: "排序"),
                    TENANTID = table.Column<Guid>(type: "uuid", nullable: true, comment: "租户Id"),
                    EXTRAPROPERTIES = table.Column<string>(type: "text", nullable: false),
                    CONCURRENCYSTAMP = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CREATIONTIME = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CREATORID = table.Column<Guid>(type: "uuid", nullable: true),
                    LASTMODIFICATIONTIME = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    LASTMODIFIERID = table.Column<Guid>(type: "uuid", nullable: true),
                    ISDELETED = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DELETERID = table.Column<Guid>(type: "uuid", nullable: true),
                    DELETIONTIME = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("Pk_WkDefinition", x => x.ID);
                },
                comment: "工作流定义");

            migrationBuilder.CreateTable(
                name: "HXWKEVENTS",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    EVENTNAME = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    EVENTKEY = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    EVENTDATA = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    EVENTTIME = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    ISPROCESSED = table.Column<bool>(type: "boolean", nullable: false),
                    TENANTID = table.Column<Guid>(type: "uuid", nullable: true),
                    CREATIONTIME = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CREATORID = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HXWKEVENTS", x => x.ID);
                },
                comment: "流程事件");

            migrationBuilder.CreateTable(
                name: "HXWKEXECUTIONERRORS",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    WKINSTANCEID = table.Column<Guid>(type: "uuid", nullable: false),
                    WKEXECUTIONPOINTERID = table.Column<Guid>(type: "uuid", nullable: false),
                    ERRORTIME = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    MESSAGE = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    TENANTID = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HXWKEXECUTIONERRORS", x => x.ID);
                },
                comment: "执行错误");

            migrationBuilder.CreateTable(
                name: "HXWKSTEPBODYS",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    NAME = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    DISPLAYNAME = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    TYPEFULLNAME = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ASSEMBLYFULLNAME = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    CREATIONTIME = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CREATORID = table.Column<Guid>(type: "uuid", nullable: true),
                    LASTMODIFICATIONTIME = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    LASTMODIFIERID = table.Column<Guid>(type: "uuid", nullable: true),
                    ISDELETED = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DELETERID = table.Column<Guid>(type: "uuid", nullable: true),
                    DELETIONTIME = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HXWKSTEPBODYS", x => x.Id);
                },
                comment: "节点实体");

            migrationBuilder.CreateTable(
                name: "HXWKINSTANCES",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    WKDIFINITIONID = table.Column<Guid>(type: "uuid", nullable: false),
                    VERSION = table.Column<int>(type: "integer", nullable: false),
                    DESCRIPTION = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    REFERENCE = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    NEXTEXECUTION = table.Column<long>(type: "bigint", nullable: true),
                    STATUS = table.Column<int>(type: "integer", precision: 1, nullable: false),
                    DATA = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CREATETIME = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    COMPLETETIME = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    TENANTID = table.Column<Guid>(type: "uuid", nullable: true),
                    EXTRAPROPERTIES = table.Column<string>(type: "text", nullable: false),
                    CONCURRENCYSTAMP = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CREATIONTIME = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CREATORID = table.Column<Guid>(type: "uuid", nullable: true),
                    LASTMODIFICATIONTIME = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    LASTMODIFIERID = table.Column<Guid>(type: "uuid", nullable: true),
                    ISDELETED = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DELETERID = table.Column<Guid>(type: "uuid", nullable: true),
                    DELETIONTIME = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
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
                comment: "流程实例");

            migrationBuilder.CreateTable(
                name: "HXWKNODES",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    WkStepBodyId = table.Column<Guid>(type: "uuid", nullable: true),
                    WKDIFINITIONID = table.Column<Guid>(type: "uuid", nullable: true),
                    NAME = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    DISPLAYNAME = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    STEPNODETYPE = table.Column<int>(type: "integer", precision: 1, nullable: false),
                    VERSION = table.Column<int>(type: "integer", nullable: false),
                    LIMITTIME = table.Column<int>(type: "integer", nullable: true),
                    NODEFORMTYPE = table.Column<int>(type: "integer", precision: 1, nullable: false)
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
                comment: "执行节点");

            migrationBuilder.CreateTable(
                name: "HXWKSTEPBODYPARAMS",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    WkNodeId = table.Column<Guid>(type: "uuid", nullable: false),
                    KEY = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    StepBodyParaType = table.Column<int>(type: "integer", nullable: false),
                    NAME = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    DISPLAYNAME = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    VALUE = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true)
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
                comment: "节点参数");

            migrationBuilder.CreateTable(
                name: "HXWKEXECUTIONPOINTER",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    WKINSTANCEID = table.Column<Guid>(type: "uuid", nullable: false),
                    STEPID = table.Column<int>(type: "integer", nullable: false),
                    ACTIVE = table.Column<bool>(type: "boolean", nullable: false),
                    SLEEPUNTIL = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    PERSISTENCEDATA = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    STARTTIME = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ENDTIME = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    EVENTNAME = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    EVENTKEY = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    EVENTPUBLISHED = table.Column<bool>(type: "boolean", nullable: false),
                    EVENTDATA = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    STEPNAME = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    RETRYCOUNT = table.Column<int>(type: "integer", nullable: false),
                    CHILDREN = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CONTEXTITEM = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    PREDECESSORID = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    OUTCOME = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    STATUS = table.Column<int>(type: "integer", precision: 2, nullable: false),
                    SCOPE = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    TENANTID = table.Column<Guid>(type: "uuid", nullable: true),
                    CREATIONTIME = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CREATORID = table.Column<Guid>(type: "uuid", nullable: true),
                    LASTMODIFICATIONTIME = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    LASTMODIFIERID = table.Column<Guid>(type: "uuid", nullable: true),
                    ISDELETED = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DELETERID = table.Column<Guid>(type: "uuid", nullable: true),
                    DELETIONTIME = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
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
                comment: "执行节点实例");

            migrationBuilder.CreateTable(
                name: "HXAPPLICATIONFORMS",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    WKNODEID = table.Column<Guid>(type: "uuid", nullable: true),
                    PARENTID = table.Column<Guid>(type: "uuid", nullable: true),
                    CODE = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    NAME = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    DISPALYNAME = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    APPLICATIONTYPE = table.Column<int>(type: "integer", precision: 1, nullable: false)
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
                comment: "流程表单");

            migrationBuilder.CreateTable(
                name: "HXWKCONDITIONNODES",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    WKNODEID = table.Column<Guid>(type: "uuid", nullable: false),
                    LABEL = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    NEXTNODENAME = table.Column<string>(type: "text", nullable: true)
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
                comment: "节点条件");

            migrationBuilder.CreateTable(
                name: "HXWKNODEPARAS",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    WKNODEID = table.Column<Guid>(type: "uuid", nullable: false),
                    KEY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    VALUE = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true)
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
                comment: "步骤参数");

            migrationBuilder.CreateTable(
                name: "HXWKPOINTS",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    WkNodeId = table.Column<Guid>(type: "uuid", nullable: false),
                    LEFT = table.Column<int>(type: "integer", nullable: false),
                    RIGHT = table.Column<int>(type: "integer", nullable: false),
                    TOP = table.Column<int>(type: "integer", nullable: false),
                    BOTTOM = table.Column<int>(type: "integer", nullable: false)
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
                });

            migrationBuilder.CreateTable(
                name: "HXWKAUDITORS",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WORKFLOWID = table.Column<Guid>(type: "uuid", nullable: false),
                    EXECUTIONPOINTERID = table.Column<Guid>(type: "uuid", nullable: false),
                    STATUS = table.Column<int>(type: "integer", precision: 1, nullable: false),
                    AUDITTIME = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    REMARK = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    USERID = table.Column<Guid>(type: "uuid", nullable: true),
                    USERNAME = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    TENANTID = table.Column<Guid>(type: "uuid", nullable: true),
                    CREATIONTIME = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CREATORID = table.Column<Guid>(type: "uuid", nullable: true),
                    LASTMODIFICATIONTIME = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    LASTMODIFIERID = table.Column<Guid>(type: "uuid", nullable: true),
                    ISDELETED = table.Column<bool>(type: "boolean", nullable: false),
                    DELETERID = table.Column<Guid>(type: "uuid", nullable: true),
                    DELETIONTIME = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
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
                },
                comment: "审核");

            migrationBuilder.CreateTable(
                name: "HXWKCANDIDATES",
                columns: table => new
                {
                    NODEID = table.Column<Guid>(type: "uuid", nullable: false),
                    CANDIDATEID = table.Column<Guid>(type: "uuid", nullable: false),
                    USERNAME = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    DISPLAYUSERNAME = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    DEFAULTSELECTION = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
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
                },
                comment: "流程人员关系");

            migrationBuilder.CreateTable(
                name: "HXWKEXTENSIONATTRIBUTES",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    EXECUTIONPOINTERID = table.Column<Guid>(type: "uuid", nullable: false),
                    ATTRIBUTEKEY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ATTRIBUTEVALUE = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    TENANTID = table.Column<Guid>(type: "uuid", nullable: true)
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
                comment: "执行节点属性");

            migrationBuilder.CreateTable(
                name: "HXWKSUBSCRIPTIONS",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    WORKFLOWID = table.Column<Guid>(type: "uuid", nullable: true),
                    STEPID = table.Column<int>(type: "integer", nullable: false),
                    EXECUTIONPOINTERID = table.Column<Guid>(type: "uuid", nullable: false),
                    EVENTNAME = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    EVENTKEY = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    SUBSCRIBEASOF = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    SUBSCRIPTIONDATA = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    EXTERNALTOKEN = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    EXTERNALWORKERID = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    EXTERNALTOKENEXPIRY = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    TENANTID = table.Column<Guid>(type: "uuid", nullable: true)
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
                comment: "发布");

            migrationBuilder.CreateTable(
                name: "HXWKCONNODECONDITIONS",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    WKCONDITIONNODEID = table.Column<Guid>(type: "uuid", nullable: false),
                    FIELD = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    OPERATOR = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    VALUE = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true)
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
                comment: "条件集合");

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
