using Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace Hx.Workflow.Application.Contracts
{
    internal class WorkflowDefinitionProvider : PermissionDefinitionProvider
    {
        public override void Define(IPermissionDefinitionContext context)
        {
            var myGroup = context.AddGroup("Workflow", L("Permission:Workflow"));
            var formGroup = myGroup.AddPermission("Workflow.FormGroup", L("Permission:Workflow.FormGroup"));
            formGroup.AddChild("Workflow.FormGroup.List", L("Permission:Workflow.FormGroup.List"));
            formGroup.AddChild("Workflow.FormGroup.Create", L("Permission:Workflow.FormGroup.Create"));
            formGroup.AddChild("Workflow.FormGroup.Update", L("Permission:Workflow.FormGroup.Update"));
            formGroup.AddChild("Workflow.FormGroup.Delete", L("Permission:Workflow.FormGroup.Delete"));

            var form = myGroup.AddPermission("Workflow.Form", L("Permission:Workflow.Form"));
            form.AddChild("Workflow.Form.List", L("Permission:Workflow.Form.List"));
            form.AddChild("Workflow.Form.Details", L("Permission:Workflow.Form.Details"));
            form.AddChild("Workflow.Form.Create", L("Permission:Workflow.Form.Create"));
            form.AddChild("Workflow.Form.Update", L("Permission:Workflow.Form.Update"));
            form.AddChild("Workflow.Form.Delete", L("Permission:Workflow.Form.Delete"));

            var definitionGroup = myGroup.AddPermission("Workflow.DefinitionGroup", L("Permission:Workflow.DefinitionGroup"));
            definitionGroup.AddChild("Workflow.DefinitionGroup.List", L("Permission:Workflow.DefinitionGroup.List"));
            definitionGroup.AddChild("Workflow.DefinitionGroup.Create", L("Permission:Workflow.DefinitionGroup.Create"));
            definitionGroup.AddChild("Workflow.DefinitionGroup.Update", L("Permission:Workflow.DefinitionGroup.Update"));
            definitionGroup.AddChild("Workflow.DefinitionGroup.Delete", L("Permission:Workflow.DefinitionGroup.Delete"));

            var definition = myGroup.AddPermission("Workflow.Definition", L("Permission:Workflow.Definition"));
            definition.AddChild("Workflow.Definition.Details", L("Permission:Workflow.Definition.Details"));
            definition.AddChild("Workflow.Definition.Create", L("Permission:Workflow.Definition.Create"));
            definition.AddChild("Workflow.Definition.Update", L("Permission:Workflow.Definition.Update"));
            definition.AddChild("Workflow.Definition.Delete", L("Permission:Workflow.Definition.Delete"));

            var stepBody = myGroup.AddPermission("Workflow.StepBody", L("Permission:Workflow.StepBody"));
            stepBody.AddChild("Workflow.StepBody.Details", L("Permission:Workflow.StepBody.Details"));
            stepBody.AddChild("Workflow.StepBody.Create", L("Permission:Workflow.StepBody.Create"));
            stepBody.AddChild("Workflow.StepBody.Update", L("Permission:Workflow.StepBody.Update"));
            stepBody.AddChild("Workflow.StepBody.Delete", L("Permission:Workflow.StepBody.Delete"));
            stepBody.AddChild("Workflow.StepBody.List", L("Permission:Workflow.StepBody.List"));

            var instance = myGroup.AddPermission("Workflow.Instance", L("Permission:Workflow.Instance"));
            instance.AddChild("Workflow.Instance.List", L("Permission:Workflow.Instance.List"));
            instance.AddChild("Workflow.Instance.Create", L("Permission:Workflow.Instance.Create"));
            instance.AddChild("Workflow.Instance.Terminate", L("Permission:Workflow.Instance.Terminate"));
            instance.AddChild("Workflow.Instance.Suspend", L("Permission:Workflow.Instance.Suspend"));
            instance.AddChild("Workflow.Instance.Delete", L("Permission:Workflow.Instance.Delete"));
        }
        private static LocalizableString L(string name)
        {
            return LocalizableString.Create<WorkflowResource>(name);
        }
    }
}
