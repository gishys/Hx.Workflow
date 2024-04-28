using Hx.Workflow.Domain;
using Hx.Workflow.Domain.Persistence;
using Hx.Workflow.Domain.Shared;
using Hx.Workflow.Domain.StepBodys;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
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
            builder.Entity<WkDefinition>(t =>
            {
                t.ConfigureFullAuditedAggregateRoot();
                t.ToTable(model.TablePrefix + "WKDEFINITIONS", model.Schema, tb => { tb.HasComment("工作流定义"); });
                t.HasKey(p => p.Id).HasName("Pk_WkDefinition");
                t.Property(p => p.Id).HasColumnName("ID").HasComment("主键");
                t.Property(p => p.Version).HasColumnName("VERSION").HasComment("版本号").HasPrecision(9);
                t.Property(p => p.Title).HasColumnName("TITLE").HasMaxLength(WkDefinitionConsts.MaxTitle).HasComment("标题");
                t.Property(p => p.LimitTime).HasColumnName("LIMITTIME").HasComment("限制时间");
                t.Property(p => p.WkDefinitionState).HasPrecision(1).HasColumnName("WKDEFINITIONSTATE").HasComment("是否开启");

                t.Property(p => p.Icon).HasColumnName("ICON").HasMaxLength(WkDefinitionConsts.MaxIcon).HasComment("图标路径");
                t.Property(p => p.Color).HasColumnName("COLOR").HasMaxLength(WkDefinitionConsts.MaxColor).HasComment("显示颜色");
                t.Property(p => p.GroupId).HasColumnName("GROUPID").HasComment("属于组");
                t.Property(p => p.Discription).HasColumnName("DISCRIPTION").HasMaxLength(WkDefinitionConsts.MaxDescription).HasComment("定义描述");
                t.Property(p => p.SortNumber).HasColumnName("SORTNUMBER").HasComment("排序");
                t.Property(p => p.TenantId).HasColumnName("TENANTID").HasComment("租户Id");

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
            builder.Entity<WkAuditor>(d =>
            {
                d.ToTable(model.TablePrefix + "WKAUDITORS", model.Schema, tb => { tb.HasComment("审核"); });
                d.Property(d => d.WorkflowId).HasColumnName("WORKFLOWID");
                d.Property(d => d.ExecutionPointerId).HasColumnName("EXECUTIONPOINTERID");
                d.Property(d => d.Status).HasColumnName("STATUS").HasPrecision(1);
                d.Property(d => d.AuditTime).HasColumnName("AUDITTIME").HasColumnType("datetime");
                d.Property(d => d.Remark).HasColumnName("REMARK").HasMaxLength(WkAuditorConsts.MaxRemarkLength);

                d.Property(d => d.UserId).HasColumnName("USERID");
                d.Property(d => d.UserName).HasColumnName("USERNAME").HasMaxLength(WkAuditorConsts.MaxUserNameLength);
                d.Property(d => d.TenantId).HasColumnName("TENANTID");

                d.HasOne(d => d.Workflow)
                .WithMany(d=>d.WkAuditors)
                .HasForeignKey(d => d.WorkflowId)
                .HasConstraintName("Pk_WkAuditor_WkInstance");

                d.HasOne(d => d.ExecutionPointer)
                .WithMany()
                .HasForeignKey(d => d.ExecutionPointerId)
                .HasConstraintName("Pk_WkAuditor_ExecPointer");
            });
            builder.Entity<WkCandidate>(d =>
            {
                d.ToTable(model.TablePrefix + "WKCANDIDATES", model.Schema, tb => { tb.HasComment("流程人员关系"); });
                d.HasKey(d => new { d.NodeId, d.CandidateId });
                d.Property(d => d.CandidateId).HasColumnName("CANDIDATEID");
                d.Property(d => d.NodeId).HasColumnName("NODEID");
                d.Property(d => d.UserName).HasColumnName("USERNAME").HasMaxLength(WkCandidateConsts.MaxUserNameLength);
                d.Property(d => d.DisplayUserName).HasColumnName("DISPLAYUSERNAME").HasMaxLength(WkCandidateConsts.MaxDisplayUserNameLength);
                d.Property(d => d.DefaultSelection).HasColumnName("DEFAULTSELECTION").HasDefaultValue(0);
            });
            builder.Entity<WkNode>(t =>
            {
                t.ToTable(model.TablePrefix + "WKNODES", model.Schema, tb => { tb.HasComment("执行节点"); });
                t.Property(d => d.Id).HasColumnName("ID");
                t.Property(d => d.WkDefinitionId).HasColumnName("WKDIFINITIONID");
                t.Property(d => d.Name).HasColumnName("NAME").HasMaxLength(WkNodeConsts.MaxName);
                t.Property(d => d.StepNodeType).HasColumnName("STEPNODETYPE").HasPrecision(1);
                t.Property(d => d.Version).HasColumnName("VERSION");

                t.Property(d => d.LimitTime).HasColumnName("LIMITTIME");
                t.Property(d => d.NodeFormType).HasColumnName("NODEFORMTYPE").HasPrecision(1);
                t.Property(d => d.DisplayName).HasColumnName("DISPLAYNAME").HasMaxLength(WkNodeConsts.MaxDispalyName);

                t.HasMany(d => d.ApplicationForms)
                .WithOne(d => d.WkNode)
                .HasForeignKey(d => d.WkNodeId)
                .HasConstraintName("Pk_WkNode_App")
                .OnDelete(DeleteBehavior.Cascade);

                t.HasMany(d => d.NextNodes).WithOne()
                .HasForeignKey(d => d.WkNodeId)
                .HasConstraintName("Pk_WkNode_Candition")
                .OnDelete(DeleteBehavior.Cascade);

                t.HasOne(d => d.StepBody).WithMany()
                .HasForeignKey(d => d.WkStepBodyId)
                .HasConstraintName("Pk_WkNode_WkStepBody")
                .OnDelete(DeleteBehavior.Cascade);

                t.HasMany(d => d.Position).WithOne()
                .HasForeignKey(d => d.WkNodeId)
                .HasConstraintName("Pk_WkNode_WkPoint")
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
            d.Property(d => d.NextNodeName).HasColumnName("NEXTNODENAME");

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
            builder.Entity<ApplicationForm>(t =>
            {
                t.ToTable(model.TablePrefix + "APPLICATIONFORMS", model.Schema, tb => { tb.HasComment("流程表单"); });
                t.Property(d => d.Id).HasColumnName("ID");
                t.Property(d => d.WkNodeId).HasColumnName("WKNODEID");
                t.Property(d => d.ParentId).HasColumnName("PARENTID");
                t.Property(d => d.Code).HasColumnName("CODE").HasMaxLength(ApplicationFormConsts.MaxCode);
                t.Property(d => d.Name).HasColumnName("NAME").HasMaxLength(ApplicationFormConsts.MaxName);
                t.Property(d => d.DispalyName).HasColumnName("DISPALYNAME").HasMaxLength(ApplicationFormConsts.MaxDispalyName);
                t.Property(d => d.ApplicationType).HasColumnName("APPLICATIONTYPE").HasPrecision(1);
            });
            builder.Entity<WkPoint>(t =>
            {
                t.ToTable(model.TablePrefix + "WKPOINTS", model.Schema);
                t.Property(d => d.Id).HasColumnName("ID");
                t.Property(d => d.Left).HasColumnName("LEFT");
                t.Property(d => d.Right).HasColumnName("RIGHT");
                t.Property(d => d.Top).HasColumnName("TOP");
                t.Property(d => d.Bottom).HasColumnName("BOTTOM");
            });

            builder.Entity<WkStepBody>(t =>
            {
                t.ConfigureFullAuditedAggregateRoot();
                t.ToTable(model.TablePrefix + "WKSTEPBODYS", model.Schema, tb => { tb.HasComment("节点实体"); });
                t.Property(d => d.Name).HasColumnName("NAME").HasMaxLength(WkStepBodyConsts.MaxName);
                t.Property(d => d.DisplayName).HasColumnName("DISPLAYNAME").HasMaxLength(WkStepBodyConsts.MaxDisplayName);
                t.Property(d => d.TypeFullName).HasColumnName("TYPEFULLNAME").HasMaxLength(WkStepBodyConsts.MaxTypeFullName);
                t.Property(d => d.AssemblyFullName).HasColumnName("ASSEMBLYFULLNAME").HasMaxLength(WkStepBodyConsts.MaxAssemblyFullName);

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
                t.Property(d => d.Time).HasColumnName("EVENTTIME").HasColumnType("datetime");
                t.Property(d => d.IsProcessed).HasColumnName("ISPROCESSED");
                t.Property(d => d.TenantId).HasColumnName("TENANTID");
            });
            builder.Entity<WkExecutionError>(t =>
            {
                t.ToTable(model.TablePrefix + "WKEXECUTIONERRORS", model.Schema, tb => { tb.HasComment("执行错误"); });
                t.Property(d => d.Id).HasColumnName("ID");
                t.Property(d => d.WkInstanceId).HasColumnName("WKINSTANCEID");
                t.Property(d => d.WkExecutionPointerId).HasColumnName("WKEXECUTIONPOINTERID");
                t.Property(d => d.ErrorTime).HasColumnName("ERRORTIME").HasColumnType("datetime");
                t.Property(d => d.Message).HasColumnName("MESSAGE").HasMaxLength(WkExecutionErrorConsts.MaxMessage);
                t.Property(d => d.TenantId).HasColumnName("TENANTID");
            });
            builder.Entity<WkExtensionAttribute>(d =>
            {
                d.ToTable(model.TablePrefix + "WKEXTENSIONATTRIBUTES", model.Schema, tb => { tb.HasComment("执行节点属性"); });
                d.Property(d => d.Id).HasColumnName("ID");
                d.Property(d => d.ExecutionPointerId).HasColumnName("EXECUTIONPOINTERID");
                d.Property(d => d.AttributeKey).HasColumnName("ATTRIBUTEKEY").HasMaxLength(WkExtensionAttributeConsts.AttributeKey);
                d.Property(d => d.AttributeValue).HasColumnName("ATTRIBUTEVALUE").HasMaxLength(WkExtensionAttributeConsts.AttributeValue);
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
                t.Property(d => d.SleepUntil).HasColumnName("SLEEPUNTIL").HasColumnType("datetime");

                t.Property(d => d.PersistenceData).HasColumnName("PERSISTENCEDATA").HasMaxLength(WkExecutionPointerConsts.MaxPersistenceData);
                t.Property(d => d.StartTime).HasColumnName("STARTTIME").HasColumnType("datetime");
                t.Property(d => d.EndTime).HasColumnName("ENDTIME").HasColumnType("datetime");
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

                t.HasMany(d => d.ExtensionAttributes)
                .WithOne(d => d.WkExecutionPointer)
                .HasForeignKey(d => d.ExecutionPointerId)
                .HasConstraintName("Pk_Pointer_Attribute")
                .OnDelete(DeleteBehavior.Cascade);

                t.HasMany(d => d.WkCandidates)
                .WithOne()
                .HasForeignKey(d => d.CandidateId)
                .HasConstraintName("Pk_Pointer_Candidate")
                .OnDelete(DeleteBehavior.Cascade);

                t.HasMany(d => d.WkSubscriptions)
                .WithOne()
                .HasForeignKey(d => d.ExecutionPointerId)
                .HasConstraintName("Pk_Pointer_Subscript");
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
                t.Property(d => d.CreateTime).HasColumnName("CREATETIME").HasColumnType("datetime");
                t.Property(d => d.CompleteTime).HasColumnName("COMPLETETIME").HasColumnType("datetime");

                t.Property(d => d.TenantId).HasColumnName("TENANTID");

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
                t.Property(d => d.SubscribeAsOf).HasColumnName("SUBSCRIBEASOF").HasColumnType("datetime");
                t.Property(d => d.SubscriptionData).HasColumnName("SUBSCRIPTIONDATA").HasMaxLength(WkSubscriptionConsts.SubscriptionData);
                t.Property(d => d.ExternalToken).HasColumnName("EXTERNALTOKEN").HasMaxLength(WkSubscriptionConsts.ExternalToken);
                t.Property(d => d.ExternalWorkerId).HasColumnName("EXTERNALWORKERID").HasMaxLength(WkSubscriptionConsts.ExternalWorkerId);

                t.Property(d => d.ExternalTokenExpiry).HasColumnName("EXTERNALTOKENEXPIRY").HasColumnType("datetime");
                t.Property(d => d.TenantId).HasColumnName("TENANTID");
            });
        }
    }
}