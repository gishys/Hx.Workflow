using Npgsql;
using NpgsqlTypes;
using System;
using System.Data;
using System.Data.Common;

namespace Hx.Workflow.EntityFrameworkCore
{
    internal static class WkActivitySubmissionParameterFactory
    {
        public static DbParameter CreateTenantParameter(Guid? tenantId)
        {
            var specification = GetTenantParameterSpecification(tenantId);
            return new NpgsqlParameter("tenantId", NpgsqlDbType.Uuid)
            {
                Value = specification.Value
            };
        }

        internal static (DbType DbType, object Value) GetTenantParameterSpecification(Guid? tenantId)
            => (DbType.Guid, tenantId.HasValue ? tenantId.Value : DBNull.Value);
    }
}
