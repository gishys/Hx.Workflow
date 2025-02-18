using Hx.Workflow.Domain;
using Hx.Workflow.Domain.Persistence;
using Hx.Workflow.Domain.Shared;
using Hx.Workflow.Domain.StepBodys;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using SharpYaml.Tokens;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace Hx.Workflow.EntityFrameworkCore
{
    public class HxWorkflowModelBuilderConfigrationOptions : AbpModelBuilderConfigurationOptions
    {
        public HxWorkflowModelBuilderConfigrationOptions(
            [NotNull] string tablePrefix,
            [CanBeNull] string schema)
            : base(tablePrefix, schema)
        { }
    }
    public static class WorkflowModelBuilderConfigrationExtensions
    {
        public static void HxWorkflowConfigration(this ModelBuilder builder)
        {
            var model = new HxWorkflowModelBuilderConfigrationOptions(
                WkDbProperties.DbTablePrefix,
                WkDbProperties.DbSchema);
            builder.Entity<WkDefinitionGroup>(t =>
            {
                t.ConfigureFullAuditedAggregateRoot();
                t.ToTable(model.TablePrefix + "WKDEFINITION_GROUPS", model.Schema, tb => { tb.HasComment("模板组"); });
                t.HasKey(p => p.Id).HasName("PK_WKDEFINITION_GROUP");
                t.Property(p => p.Id).HasColumnName("ID").HasComment("主键");
                t.Property(t => t.Title).IsRequired().HasMaxLength(255).HasColumnName("TITLE").HasComment("标题");
                t.Property(t => t.Code).IsRequired().HasMaxLength(119).HasColumnName("CODE").HasComment("路径枚举");
                t.Property(t => t.ParentId).IsRequired(false).HasColumnName("PARENT_ID").HasComment("父Id");
                t.Property(t => t.Order).IsRequired().HasColumnName("ORDER").HasComment("序号");
                t.Property(p => p.TenantId).HasColumnName("TENANTID").HasComment("租户Id");
                t.Property(t => t.Description).IsRequired(false).HasMaxLength(500).HasColumnName("DESCRIPTION").HasComment("描述");

                t.HasMany(t => t.Definitions)
                       .WithOne()
                       .HasForeignKey(d => d.GroupId)
                       .HasConstraintName("QI_GROUPS_WKDEFINITION_ID")
                       .OnDelete(DeleteBehavior.Cascade);

                t.HasMany(t => t.Children)
                       .WithOne()
                       .HasForeignKey(d => d.ParentId)
                       .HasConstraintName("QI_GROUPS_PARENT_ID")
                       .OnDelete(DeleteBehavior.Cascade);

                t.Property(p => p.CreationTime).HasColumnName("CREATIONTIME").HasColumnType("timestamp with time zone");
                t.Property(p => p.CreatorId).HasColumnName("CREATORID");
                t.Property(p => p.LastModificationTime).HasColumnName("LASTMODIFICATIONTIME").HasColumnType("timestamp with time zone");
                t.Property(p => p.LastModifierId).HasColumnName("LASTMODIFIERID");
                t.Property(p => p.IsDeleted).HasColumnName("ISDELETED");
                t.Property(p => p.DeleterId).HasColumnName("DELETERID");
                t.Property(p => p.DeletionTime).HasColumnName("DELETIONTIME").HasColumnType("timestamp with time zone");
            });
            builder.Entity<WkDefinition>(t =>
            {
                t.ConfigureFullAuditedAggregateRoot();
                t.ToTable(model.TablePrefix + "WKDEFINITIONS", model.Schema, tb => { tb.HasComment("工作流定义"); });
                t.HasKey(p => p.Id).HasName("PK_WKDEFINITION");
                t.Property(p => p.Id).HasColumnName("ID").HasComment("主键");
                t.Property(p => p.Version).HasColumnName("VERSION").HasComment("版本号").HasPrecision(9);
                t.Property(p => p.Title).HasColumnName("TITLE").HasMaxLength(WkDefinitionConsts.MaxTitle).HasComment("标题");
                t.Property(p => p.LimitTime).HasColumnName("LIMITTIME").HasComment("限制时间");
                t.Property(p => p.GroupId).HasColumnName("GROUPID").HasComment("属于组");
                t.Property(p => p.Description).HasColumnName("DESCRIPTION").HasMaxLength(WkDefinitionConsts.MaxDescription).HasComment("定义描述");
                t.Property(p => p.SortNumber).HasColumnName("SORTNUMBER").HasComment("排序");
                t.Property(p => p.TenantId).HasColumnName("TENANTID").HasComment("租户Id");

                t.Property(p => p.BusinessType).HasColumnName("BUSINESSTYPE").HasMaxLength(WkDefinitionConsts.MaxBusinessType).HasComment("业务类型");
                t.Property(p => p.ProcessType).HasColumnName("PROCESSTYPE").HasMaxLength(WkDefinitionConsts.MaxProcessType).HasComment("流程类型");

                t.Property(p => p.ExtraProperties).HasColumnName("EXTRAPROPERTIES");
                t.Property(p => p.ConcurrencyStamp).HasColumnName("CONCURRENCYSTAMP");
                t.Property(p => p.CreationTime).HasColumnName("CREATIONTIME").HasColumnType("timestamp with time zone");
                t.Property(p => p.CreatorId).HasColumnName("CREATORID");
                t.Property(p => p.LastModificationTime).HasColumnName("LASTMODIFICATIONTIME").HasColumnType("timestamp with time zone");
                t.Property(p => p.LastModifierId).HasColumnName("LASTMODIFIERID");
                t.Property(p => p.IsDeleted).HasColumnName("ISDELETED");
                t.Property(p => p.DeleterId).HasColumnName("DELETERID");
                t.Property(p => p.DeletionTime).HasColumnName("DELETIONTIME").HasColumnType("timestamp with time zone");
                t.Property(p => p.IsEnabled).HasColumnName("ISENABLED").HasDefaultValue(true);

                t.HasMany(d => d.Nodes)
                .WithOne(d => d.WkDefinition)
                .HasForeignKey(d => d.WkDefinitionId)
                .HasConstraintName("Pk_WkDef_WkNode")
                .OnDelete(DeleteBehavior.Cascade);

                t.HasMany(d => d.WkCandidates)
                .WithOne()
                .HasForeignKey(d => d.NodeId)
                .HasConstraintName("Pk_WkDef_Candidate")
                .OnDelete(DeleteBehavior.Cascade);
            });
            builder.Entity<DefinitionCandidate>(d =>
            {
                d.ToTable(model.TablePrefix + "DEFINITION_CANDIDATES", model.Schema, tb => { tb.HasComment("流程模板候选人"); });
                d.HasKey(d => new { d.NodeId, d.CandidateId });
                d.Property(d => d.CandidateId).HasColumnName("CANDIDATEID");
                d.Property(d => d.NodeId).HasColumnName("NODEID");
                d.Property(d => d.UserName).HasColumnName("USERNAME").HasMaxLength(WkCandidateConsts.MaxUserNameLength);
                d.Property(d => d.DisplayUserName).HasColumnName("DISPLAYUSERNAME").HasMaxLength(WkCandidateConsts.MaxDisplayUserNameLength);
                d.Property(d => d.ExecutorType).HasColumnName("EXECUTORTYPE");
                d.Property(d => d.DefaultSelection).HasColumnName("DEFAULTSELECTION").HasDefaultValue(0);
            });
            builder.Entity<WkAuditor>(d =>
            {
                d.ToTable(model.TablePrefix + "WKAUDITORS", model.Schema, tb => { tb.HasComment("审核"); });
                d.Property(d => d.WorkflowId).HasColumnName("WORKFLOWID");
                d.Property(d => d.ExecutionPointerId).HasColumnName("EXECUTIONPOINTERID");
                d.Property(d => d.Status).HasColumnName("STATUS").HasPrecision(1);
                d.Property(d => d.AuditTime).HasColumnName("AUDITTIME").HasColumnType("timestamp with time zone");
                d.Property(d => d.Remark).HasColumnName("REMARK").HasMaxLength(WkAuditorConsts.MaxRemarkLength);

                d.Property(d => d.UserId).HasColumnName("USERID");
                d.Property(d => d.UserName).HasColumnName("USERNAME").HasMaxLength(WkAuditorConsts.MaxUserNameLength);
                d.Property(d => d.TenantId).HasColumnName("TENANTID");

                d.Property(p => p.CreationTime).HasColumnName("CREATIONTIME").HasColumnType("timestamp with time zone");
                d.Property(p => p.CreatorId).HasColumnName("CREATORID");
                d.Property(p => p.LastModificationTime).HasColumnName("LASTMODIFICATIONTIME").HasColumnType("timestamp with time zone");
                d.Property(p => p.LastModifierId).HasColumnName("LASTMODIFIERID");
                d.Property(p => p.IsDeleted).HasColumnName("ISDELETED");
                d.Property(p => p.DeleterId).HasColumnName("DELETERID");
                d.Property(p => p.DeletionTime).HasColumnName("DELETIONTIME").HasColumnType("timestamp with time zone");

                d.HasOne(d => d.Workflow)
                .WithMany(d => d.WkAuditors)
                .HasForeignKey(d => d.WorkflowId)
                .HasConstraintName("Pk_WkAuditor_WkInstance");

                d.HasOne(d => d.ExecutionPointer)
                .WithMany()
                .HasForeignKey(d => d.ExecutionPointerId)
                .HasConstraintName("Pk_WkAuditor_ExecPointer");
            });
            builder.Entity<WkNode>(t =>
            {
                t.ConfigureExtraProperties();
                t.ToTable(model.TablePrefix + "WKNODES", model.Schema, tb => { tb.HasComment("执行节点"); });
                t.Property(d => d.Id).HasColumnName("ID");
                t.Property(d => d.WkDefinitionId).HasColumnName("WKDIFINITIONID");
                t.Property(d => d.Name).HasColumnName("NAME").HasMaxLength(WkNodeConsts.MaxName);
                t.Property(d => d.StepNodeType).HasColumnName("STEPNODETYPE").HasPrecision(1);
                t.Property(d => d.Version).HasColumnName("VERSION");

                t.Property(d => d.LimitTime).HasColumnName("LIMITTIME");
                t.Property(d => d.DisplayName).HasColumnName("DISPLAYNAME").HasMaxLength(WkNodeConsts.MaxDisplayName);
                t.Property(p => p.SortNumber).HasColumnName("SORTNUMBER").HasComment("排序");

                t.Property(p => p.ExtraProperties).HasColumnName("EXTRAPROPERTIES");

                t.HasMany(d => d.NextNodes).WithOne()
                .HasForeignKey(d => d.WkNodeId)
                .HasConstraintName("Pk_WkNode_Candition")
                .OnDelete(DeleteBehavior.Cascade);

                t.HasOne(d => d.StepBody).WithMany()
                .HasForeignKey(d => d.WkStepBodyId)
                .HasConstraintName("Pk_WkNode_WkStepBody")
                .OnDelete(DeleteBehavior.Cascade);

                t.HasMany(d => d.WkCandidates)
                .WithOne()
                .HasForeignKey(d => d.NodeId)
                .HasConstraintName("Pk_WkNode_Candidate")
                .OnDelete(DeleteBehavior.Cascade);

                t.HasMany(d => d.OutcomeSteps)
                .WithOne()
                .HasForeignKey(d => d.WkNodeId)
                .HasConstraintName("Pk_WkNode_OutcomeSteps")
                .OnDelete(DeleteBehavior.Cascade);

                t.OwnsMany(p => p.Params, param =>
                {
                    param.ToJson();
                });
                t.OwnsMany(p => p.Materials, param =>
                {
                    param.ToJson();
                    param.OwnsMany(d => d.Children, cparam =>
                    {
                        cparam.ToJson();
                    });
                });
            });
            builder.Entity<WkNode_ApplicationForms>(d =>
            {
                d.ToTable(model.TablePrefix + "_NODES_APPLICATION_FORMS", model.Schema, tb => { tb.HasComment("节点表单关联表"); });
                d.HasKey(d => new { d.NodeId, d.ApplicationId });
                d.Property(d => d.ApplicationId).HasColumnName("APPLICATION_ID");
                d.Property(d => d.NodeId).HasColumnName("NODE_ID");
                d.Property(d => d.SequenceNumber).HasColumnName("SEQUENCENUMBER");
                d.HasOne<WkNode>().WithMany(d => d.ApplicationForms).HasForeignKey(d => d.NodeId).HasConstraintName("NODE_FKEY");
                d.HasOne(d => d.ApplicationForm).WithMany().HasForeignKey(d => d.ApplicationId).HasConstraintName("APLLICATION_FKEY");
            });
            builder.Entity<WkNodeCandidate>(d =>
            {
                d.ToTable(model.TablePrefix + "NODE_CANDIDATES", model.Schema, tb => { tb.HasComment("流程模板候选人"); });
                d.HasKey(d => new { d.NodeId, d.CandidateId });
                d.Property(d => d.CandidateId).HasColumnName("CANDIDATEID");
                d.Property(d => d.NodeId).HasColumnName("NODEID");
                d.Property(d => d.UserName).HasColumnName("USERNAME").HasMaxLength(WkCandidateConsts.MaxUserNameLength);
                d.Property(d => d.DisplayUserName).HasColumnName("DISPLAYUSERNAME").HasMaxLength(WkCandidateConsts.MaxDisplayUserNameLength);
                d.Property(d => d.ExecutorType).HasColumnName("EXECUTORTYPE");
                d.Property(d => d.DefaultSelection).HasColumnName("DEFAULTSELECTION").HasDefaultValue(0);
            });
            builder.Entity<WkNodePara>(d =>
            {
                d.ToTable(model.TablePrefix + "WKNODEPARAS", model.Schema, tb => { tb.HasComment("步骤参数"); });
                d.Property(d => d.Id).HasColumnName("ID");
                d.Property(d => d.WkNodeId).HasColumnName("WKNODEID");
                d.Property(d => d.Key).HasColumnName("KEY").HasMaxLength(WkNodeParaConsts.MaxKey);
                d.Property(d => d.Value).HasColumnName("VALUE").HasMaxLength(WkNodeParaConsts.MaxValue);
            });
            builder.Entity<WkConditionNode>(d =>
            {
                d.ToTable(model.TablePrefix + "WKCONDITIONNODES", model.Schema, tb => { tb.HasComment("节点条件"); });
                d.Property(d => d.Id).HasColumnName("ID");
                d.Property(d => d.Label).HasColumnName("LABEL").HasMaxLength(WkConditionNodeConsts.MaxLabel);
                d.Property(d => d.WkNodeId).HasColumnName("WKNODEID");
                d.Property(d => d.NextNodeName).HasColumnName("NEXTNODENAME").HasMaxLength(WkConditionNodeConsts.MaxNodeName);
                d.Property(d => d.NodeType).HasColumnName("NODETYPE").HasMaxLength(WkConditionNodeConsts.MaxNodeType);

                d.HasMany(d => d.WkConNodeConditions).WithOne()
                .HasForeignKey(d => d.WkConditionNodeId)
                .HasConstraintName("Pk_Candition_ConCondition")
                .OnDelete(DeleteBehavior.Cascade);
            });
            builder.Entity<WkConNodeCondition>(d =>
            {
                d.ToTable(model.TablePrefix + "WKCONNODECONDITIONS", model.Schema, tb => { tb.HasComment("条件集合"); });
                d.Property(d => d.Id).HasColumnName("ID");
                d.Property(d => d.Field).HasColumnName("FIELD").HasMaxLength(WkConNodeConditionConsts.MaxField);
                d.Property(d => d.Operator).HasColumnName("OPERATOR").HasMaxLength(WkConNodeConditionConsts.MaxOperator);
                d.Property(d => d.Value).HasColumnName("VALUE").HasMaxLength(WkConNodeConditionConsts.MaxValue);
                d.Property(d => d.WkConditionNodeId).HasColumnName("WKCONDITIONNODEID");
            });
            builder.Entity<ApplicationFormGroup>(t =>
            {
                t.ConfigureFullAuditedAggregateRoot();
                t.ToTable(model.TablePrefix + "APPLICATIONFORM_GROUPS", model.Schema, tb => { tb.HasComment("表单组"); });
                t.HasKey(p => p.Id).HasName("PK_APPLICATIONFORM_GROUP");
                t.Property(p => p.Id).HasColumnName("ID").HasComment("主键");
                t.Property(t => t.Title).IsRequired().HasMaxLength(255).HasColumnName("TITLE").HasComment("标题");
                t.Property(t => t.Code).IsRequired().HasMaxLength(119).HasColumnName("CODE").HasComment("路径枚举");
                t.Property(t => t.ParentId).IsRequired(false).HasColumnName("PARENT_ID").HasComment("父Id");
                t.Property(t => t.Order).IsRequired().HasColumnName("ORDER").HasComment("序号");
                t.Property(p => p.TenantId).HasColumnName("TENANTID").HasComment("租户Id");
                t.Property(t => t.Description).IsRequired(false).HasMaxLength(500).HasColumnName("DESCRIPTION").HasComment("描述");

                t.HasMany(t => t.Items)
                       .WithOne()
                       .HasForeignKey(d => d.GroupId)
                       .HasConstraintName("AF_GROUPS_APPLICATIONFORM_ID")
                       .OnDelete(DeleteBehavior.Cascade);

                t.HasMany(t => t.Children)
                       .WithOne()
                       .HasForeignKey(d => d.ParentId)
                       .HasConstraintName("AF_GROUPS_PARENT_ID")
                       .OnDelete(DeleteBehavior.Cascade);

                t.Property(p => p.CreationTime).HasColumnName("CREATIONTIME").HasColumnType("timestamp with time zone");
                t.Property(p => p.CreatorId).HasColumnName("CREATORID");
                t.Property(p => p.LastModificationTime).HasColumnName("LASTMODIFICATIONTIME").HasColumnType("timestamp with time zone");
                t.Property(p => p.LastModifierId).HasColumnName("LASTMODIFIERID");
                t.Property(p => p.IsDeleted).HasColumnName("ISDELETED");
                t.Property(p => p.DeleterId).HasColumnName("DELETERID");
                t.Property(p => p.DeletionTime).HasColumnName("DELETIONTIME").HasColumnType("timestamp with time zone");
            });
            builder.Entity<ApplicationForm>(t =>
            {
                t.ConfigureExtraProperties();
                t.ToTable(model.TablePrefix + "APPLICATIONFORMS", model.Schema, tb => { tb.HasComment("流程表单"); });
                t.Property(d => d.Id).HasColumnName("ID");
                t.Property(d => d.Data).HasColumnName("DATA");
                t.Property(d => d.ApplicationComponentType).HasColumnName("APPLICATIONCOMPONENTTYPE").HasPrecision(1);
                t.Property(p => p.ExtraProperties).HasColumnName("EXTRAPROPERTIES");
                t.Property(d => d.Name).HasColumnName("NAME").HasMaxLength(ApplicationFormConsts.MaxName);
                t.Property(d => d.Title).HasColumnName("TITLE").HasMaxLength(ApplicationFormConsts.MaxTitle);
                t.Property(d => d.ApplicationType).HasColumnName("APPLICATIONTYPE").HasPrecision(1);

                t.OwnsMany(p => p.Params, param =>
                {
                    param.ToJson();
                });
            });

            builder.Entity<WkStepBody>(t =>
            {
                t.ConfigureFullAuditedAggregateRoot();
                t.ConfigureExtraProperties();
                t.ToTable(model.TablePrefix + "WKSTEPBODYS", model.Schema, tb => { tb.HasComment("节点实体"); });
                t.Property(d => d.Name).HasColumnName("NAME").HasMaxLength(WkStepBodyConsts.MaxName);
                t.Property(d => d.Data).HasColumnName("DATA");
                t.Property(d => d.DisplayName).HasColumnName("DISPLAYNAME").HasMaxLength(WkStepBodyConsts.MaxDisplayName);
                t.Property(d => d.TypeFullName).HasColumnName("TYPEFULLNAME").HasMaxLength(WkStepBodyConsts.MaxTypeFullName);
                t.Property(d => d.AssemblyFullName).HasColumnName("ASSEMBLYFULLNAME").HasMaxLength(WkStepBodyConsts.MaxAssemblyFullName);
                t.Property(p => p.ExtraProperties).HasColumnName("EXTRAPROPERTIES");

                t.Property(p => p.CreationTime).HasColumnName("CREATIONTIME").HasColumnType("timestamp with time zone");
                t.Property(p => p.CreatorId).HasColumnName("CREATORID");
                t.Property(p => p.LastModificationTime).HasColumnName("LASTMODIFICATIONTIME").HasColumnType("timestamp with time zone");
                t.Property(p => p.LastModifierId).HasColumnName("LASTMODIFIERID");
                t.Property(p => p.IsDeleted).HasColumnName("ISDELETED");
                t.Property(p => p.DeleterId).HasColumnName("DELETERID");
                t.Property(p => p.DeletionTime).HasColumnName("DELETIONTIME").HasColumnType("timestamp with time zone");

                t.HasMany(d => d.Inputs)
                .WithOne()
                .HasForeignKey(d => d.WkNodeId)
                .HasConstraintName("Pk_WkStepBody_WkParam")
                .OnDelete(DeleteBehavior.Cascade);
            });
            builder.Entity<WkStepBodyParam>(t =>
            {
                t.ToTable(model.TablePrefix + "WKSTEPBODYPARAMS", model.Schema, tb => { tb.HasComment("节点参数"); });
                t.Property(d => d.Id).HasColumnName("ID");
                t.Property(d => d.Name).HasColumnName("NAME").HasMaxLength(WkParamConsts.MaxName);
                t.Property(d => d.DisplayName).HasColumnName("DISPLAYNAME").HasMaxLength(WkParamConsts.MaxDisplay);
                t.Property(d => d.Key).HasColumnName("KEY").HasMaxLength(WkParamConsts.MaxKey);
                t.Property(d => d.Value).HasColumnName("VALUE").HasMaxLength(WkParamConsts.MaxValue);
            });
            builder.Entity<WkEvent>(t =>
            {
                t.ToTable(model.TablePrefix + "WKEVENTS", model.Schema, tb => { tb.HasComment("流程事件"); });
                t.Property(d => d.Id).HasColumnName("ID");
                t.Property(d => d.Name).HasColumnName("EVENTNAME").HasMaxLength(WkEventConsts.MaxName);
                t.Property(d => d.Key).HasColumnName("EVENTKEY").HasMaxLength(WkEventConsts.MaxKey);
                t.Property(d => d.Data).HasColumnName("EVENTDATA").HasMaxLength(WkEventConsts.MaxData);
                t.Property(d => d.Time).HasColumnName("EVENTTIME").HasColumnType("timestamp with time zone");
                t.Property(d => d.IsProcessed).HasColumnName("ISPROCESSED");
                t.Property(d => d.TenantId).HasColumnName("TENANTID");

                t.Property(p => p.CreationTime).HasColumnName("CREATIONTIME").HasColumnType("timestamp with time zone");
                t.Property(p => p.CreatorId).HasColumnName("CREATORID");
            });
            builder.Entity<WkExecutionError>(t =>
            {
                t.ToTable(model.TablePrefix + "WKEXECUTIONERRORS", model.Schema, tb => { tb.HasComment("执行错误"); });
                t.Property(d => d.Id).HasColumnName("ID");
                t.Property(d => d.WkInstanceId).HasColumnName("WKINSTANCEID");
                t.Property(d => d.WkExecutionPointerId).HasColumnName("WKEXECUTIONPOINTERID");
                t.Property(d => d.ErrorTime).HasColumnName("ERRORTIME").HasColumnType("timestamp with time zone");
                t.Property(d => d.Message).HasColumnName("MESSAGE").HasMaxLength(WkExecutionErrorConsts.MaxMessage);
                t.Property(d => d.TenantId).HasColumnName("TENANTID");
            });
            builder.Entity<WkExtensionAttribute>(d =>
            {
                d.ToTable(model.TablePrefix + "WKEXTENSIONATTRIBUTES", model.Schema, tb => { tb.HasComment("执行节点属性"); });
                d.Property(d => d.Id).HasColumnName("ID");
                d.Property(d => d.ExecutionPointerId).HasColumnName("EXECUTIONPOINTERID");
                d.Property(d => d.AttributeKey).HasColumnName("ATTRIBUTEKEY").HasMaxLength(WkExtensionAttributeConsts.AttributeKey);
                d.Property(d => d.AttributeValue).HasColumnName("ATTRIBUTEVALUE");
                d.Property(d => d.TenantId).HasColumnName("TENANTID");
            });
            builder.Entity<WkExecutionPointer>(t =>
            {
                t.ConfigureByConvention();
                t.ToTable(model.TablePrefix + "WKEXECUTIONPOINTER", model.Schema, tb => { tb.HasComment("执行节点实例"); });
                t.Property(d => d.Id).HasColumnName("ID");
                t.Property(d => d.WkInstanceId).HasColumnName("WKINSTANCEID");
                t.Property(d => d.StepId).HasColumnName("STEPID");
                t.Property(d => d.Active).HasColumnName("ACTIVE");
                t.Property(d => d.SleepUntil).HasColumnName("SLEEPUNTIL").HasColumnType("timestamp with time zone");

                t.Property(d => d.PersistenceData).HasColumnName("PERSISTENCEDATA").HasMaxLength(WkExecutionPointerConsts.MaxPersistenceData);
                t.Property(d => d.StartTime).HasColumnName("STARTTIME").HasColumnType("timestamp with time zone");
                t.Property(d => d.EndTime).HasColumnName("ENDTIME").HasColumnType("timestamp with time zone");
                t.Property(d => d.EventName).HasColumnName("EVENTNAME").HasMaxLength(WkExecutionPointerConsts.MaxEventName);
                t.Property(d => d.EventKey).HasColumnName("EVENTKEY").HasMaxLength(WkExecutionPointerConsts.MaxEventKey);

                t.Property(d => d.EventPublished).HasColumnName("EVENTPUBLISHED");
                t.Property(d => d.EventData).HasColumnName("EVENTDATA").HasMaxLength(WkExecutionPointerConsts.MaxEventData);
                t.Property(d => d.StepName).HasColumnName("STEPNAME").HasMaxLength(WkExecutionPointerConsts.MaxStepName);
                t.Property(d => d.RetryCount).HasColumnName("RETRYCOUNT");
                t.Property(d => d.Children).HasColumnName("CHILDREN").HasMaxLength(WkExecutionPointerConsts.MaxChildren);

                t.Property(d => d.ContextItem).HasColumnName("CONTEXTITEM").HasMaxLength(WkExecutionPointerConsts.MaxContextItem);
                t.Property(d => d.PredecessorId).HasColumnName("PREDECESSORID").HasMaxLength(WkExecutionPointerConsts.MaxPredecessorId);
                t.Property(d => d.Outcome).HasColumnName("OUTCOME").HasMaxLength(WkExecutionPointerConsts.MaxOutcome);
                t.Property(d => d.Status).HasColumnName("STATUS").HasPrecision(2);

                t.Property(d => d.Scope).HasColumnName("SCOPE").HasMaxLength(WkExecutionPointerConsts.MaxScope);
                t.Property(d => d.TenantId).HasColumnName("TENANTID");
                t.Property(d => d.Recipient).HasColumnName("RECIPIENT").HasMaxLength(WkExecutionPointerConsts.RecipientMaxLength);
                t.Property(d => d.RecipientId).HasColumnName("RECIPIENTID");
                t.Property(d => d.Submitter).HasColumnName("SUBMITTER").HasMaxLength(WkExecutionPointerConsts.SubmitterMaxLength);
                t.Property(d => d.SubmitterId).HasColumnName("SUBMITTERID");
                t.Property(p => p.IsInitMaterials).HasColumnName("ISINITMATERIALS");
                t.Property(p => p.CommitmentDeadline).HasColumnName("COMMITMENTDEADLINE").HasColumnType("timestamp with time zone");

                t.Property(p => p.CreationTime).HasColumnName("CREATIONTIME").HasColumnType("timestamp with time zone");
                t.Property(p => p.CreatorId).HasColumnName("CREATORID");
                t.Property(p => p.LastModificationTime).HasColumnName("LASTMODIFICATIONTIME").HasColumnType("timestamp with time zone");
                t.Property(p => p.LastModifierId).HasColumnName("LASTMODIFIERID");
                t.Property(p => p.IsDeleted).HasColumnName("ISDELETED");
                t.Property(p => p.DeleterId).HasColumnName("DELETERID");
                t.Property(p => p.DeletionTime).HasColumnName("DELETIONTIME").HasColumnType("timestamp with time zone");

                t.OwnsMany(p => p.Materials, param =>
                {
                    param.ToJson();
                    param.OwnsMany(d => d.Children, cparam =>
                    {
                        cparam.ToJson();
                    });
                });

                t.HasMany(d => d.ExtensionAttributes)
                .WithOne(d => d.WkExecutionPointer)
                .HasForeignKey(d => d.ExecutionPointerId)
                .HasConstraintName("Pk_Pointer_Attribute")
                .OnDelete(DeleteBehavior.Cascade);

                t.HasMany(d => d.WkCandidates)
                .WithOne()
                .HasForeignKey(d => d.NodeId)
                .HasConstraintName("Pk_Pointer_Candidate")
                .OnDelete(DeleteBehavior.Cascade);

                t.HasMany(d => d.WkSubscriptions)
                .WithOne()
                .HasForeignKey(d => d.ExecutionPointerId)
                .HasConstraintName("Pk_Pointer_Subscript");
            });
            builder.Entity<ExePointerCandidate>(d =>
            {
                d.ToTable(model.TablePrefix + "POINTER_CANDIDATES", model.Schema, tb => { tb.HasComment("流程模板候选人"); });
                d.HasKey(d => new { d.NodeId, d.CandidateId });
                d.Property(d => d.CandidateId).HasColumnName("CANDIDATEID");
                d.Property(d => d.NodeId).HasColumnName("NODEID");
                d.Property(d => d.UserName).HasColumnName("USERNAME").HasMaxLength(WkCandidateConsts.MaxUserNameLength);
                d.Property(d => d.DisplayUserName).HasColumnName("DISPLAYUSERNAME").HasMaxLength(WkCandidateConsts.MaxDisplayUserNameLength);
                d.Property(d => d.DefaultSelection).HasColumnName("DEFAULTSELECTION").HasDefaultValue(0);
                d.Property(d => d.ParentState).HasColumnName("PARENTSTATE");
                d.Property(d => d.ExeOperateType).HasColumnName("EXEOPERATETYPE");
                d.Property(d => d.ExecutorType).HasColumnName("EXECUTORTYPE");
                d.Property(d => d.Follow).HasColumnName("FOLLOW");
            });
            builder.Entity<WkInstance>(t =>
            {
                t.ConfigureFullAuditedAggregateRoot();
                t.ToTable(model.TablePrefix + "WKINSTANCES", model.Schema, tb => { tb.HasComment("流程实例"); });
                t.Property(d => d.Id).HasColumnName("ID");
                t.Property(d => d.WkDifinitionId).HasColumnName("WKDIFINITIONID");
                t.Property(d => d.Version).HasColumnName("VERSION");
                t.Property(d => d.Description).HasColumnName("DESCRIPTION").HasMaxLength(WkInstanceConsts.Description);
                t.Property(d => d.Reference).HasColumnName("REFERENCE").HasMaxLength(WkInstanceConsts.Reference);

                t.Property(d => d.NextExecution).HasColumnName("NEXTEXECUTION");
                t.Property(d => d.Status).HasColumnName("STATUS").HasPrecision(1);
                t.Property(d => d.Data).HasColumnName("DATA").HasMaxLength(WkInstanceConsts.Data);
                t.Property(d => d.CreateTime).HasColumnName("CREATETIME").HasColumnType("timestamp with time zone");
                t.Property(d => d.CompleteTime).HasColumnName("COMPLETETIME").HasColumnType("timestamp with time zone");

                t.Property(d => d.TenantId).HasColumnName("TENANTID");

                t.Property(p => p.ExtraProperties).HasColumnName("EXTRAPROPERTIES");
                t.Property(p => p.ConcurrencyStamp).HasColumnName("CONCURRENCYSTAMP");
                t.Property(p => p.CreationTime).HasColumnName("CREATIONTIME").HasColumnType("timestamp with time zone");
                t.Property(p => p.CreatorId).HasColumnName("CREATORID");
                t.Property(p => p.LastModificationTime).HasColumnName("LASTMODIFICATIONTIME").HasColumnType("timestamp with time zone");
                t.Property(p => p.LastModifierId).HasColumnName("LASTMODIFIERID");
                t.Property(p => p.IsDeleted).HasColumnName("ISDELETED");
                t.Property(p => p.DeleterId).HasColumnName("DELETERID");
                t.Property(p => p.DeletionTime).HasColumnName("DELETIONTIME").HasColumnType("timestamp with time zone");

                t.HasOne(d => d.WkDefinition)
                .WithMany()
                .HasForeignKey(d => d.WkDifinitionId)
                .HasConstraintName("Pk_Instance_Definition")
                .OnDelete(DeleteBehavior.Cascade);

                t.HasMany(d => d.ExecutionPointers)
                .WithOne(d => d.WkInstance)
                .HasForeignKey(d => d.WkInstanceId)
                .HasConstraintName("Pk_Instance_Pointer");
            });
            builder.Entity<WkSubscription>(t =>
            {
                t.ToTable(model.TablePrefix + "WKSUBSCRIPTIONS", model.Schema, tb => { tb.HasComment("发布"); });

                t.Property(d => d.Id).HasColumnName("ID");
                t.Property(d => d.WorkflowId).HasColumnName("WORKFLOWID");
                t.Property(d => d.StepId).HasColumnName("STEPID");
                t.Property(d => d.ExecutionPointerId).HasColumnName("EXECUTIONPOINTERID");
                t.Property(d => d.EventName).HasColumnName("EVENTNAME").HasMaxLength(WkSubscriptionConsts.EventName);

                t.Property(d => d.EventKey).HasColumnName("EVENTKEY").HasMaxLength(WkSubscriptionConsts.EventKey);
                t.Property(d => d.SubscribeAsOf).HasColumnName("SUBSCRIBEASOF").HasColumnType("timestamp with time zone");
                t.Property(d => d.SubscriptionData).HasColumnName("SUBSCRIPTIONDATA").HasMaxLength(WkSubscriptionConsts.SubscriptionData);
                t.Property(d => d.ExternalToken).HasColumnName("EXTERNALTOKEN").HasMaxLength(WkSubscriptionConsts.ExternalToken);
                t.Property(d => d.ExternalWorkerId).HasColumnName("EXTERNALWORKERID").HasMaxLength(WkSubscriptionConsts.ExternalWorkerId);

                t.Property(d => d.ExternalTokenExpiry).HasColumnName("EXTERNALTOKENEXPIRY").HasColumnType("timestamp with time zone");
                t.Property(d => d.TenantId).HasColumnName("TENANTID");
            });
        }
    }
}