using System;

namespace Hx.Workflow.Domain
{
    /// <summary>
    /// Maintains the WorkflowCore contract that persisted engine timestamps are UTC.
    /// PostgreSQL legacy timestamp handling can materialize timestamptz values as Local,
    /// while WorkflowCore compares those values directly with UtcNow.
    /// </summary>
    internal static class WorkflowDateTime
    {
        internal static DateTime NormalizeUtc(DateTime value)
        {
            // PostgreSQL infinity values map to the DateTime boundary values. Converting
            // a Local boundary value can overflow, so preserve its ticks and only set Kind.
            if (value == DateTime.MinValue || value == DateTime.MaxValue)
            {
                return DateTime.SpecifyKind(value, DateTimeKind.Utc);
            }

            return value.Kind switch
            {
                DateTimeKind.Utc => value,
                DateTimeKind.Local => value.ToUniversalTime(),

                // WorkflowCore and timestamptz both represent absolute instants. At this
                // persistence boundary an Unspecified value therefore means UTC; treating
                // it as machine-local time would make behavior depend on the host timezone.
                DateTimeKind.Unspecified => DateTime.SpecifyKind(value, DateTimeKind.Utc),
                _ => throw new ArgumentOutOfRangeException(nameof(value), value.Kind, "Unsupported DateTimeKind.")
            };
        }

        internal static DateTime? NormalizeUtc(DateTime? value)
        {
            return value.HasValue ? NormalizeUtc(value.Value) : null;
        }
    }
}
