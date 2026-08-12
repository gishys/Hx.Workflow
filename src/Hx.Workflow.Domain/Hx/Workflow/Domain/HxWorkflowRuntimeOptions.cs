namespace Hx.Workflow.Domain
{
    public sealed class HxWorkflowRuntimeOptions
    {
        public const string SectionName = "WorkflowCore";

        /// <summary>
        /// Whether this process should initialize definitions and run the WorkflowCore host.
        /// Defaults to true for backward compatibility with existing applications.
        /// </summary>
        public bool RunHost { get; set; } = true;
    }
}
