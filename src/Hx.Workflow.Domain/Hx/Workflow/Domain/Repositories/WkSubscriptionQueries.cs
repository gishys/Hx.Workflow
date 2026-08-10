using Hx.Workflow.Domain.Persistence;
using System;
using System.Linq.Expressions;

namespace Hx.Workflow.Domain.Repositories
{
    public static class WkSubscriptionQueries
    {
        public static Expression<Func<WkSubscription, bool>> OpenForEvent(
            string eventName,
            string eventKey,
            DateTime eventTime)
            => subscription =>
                subscription.EventName == eventName &&
                subscription.EventKey == eventKey &&
                subscription.SubscribeAsOf <= eventTime &&
                (subscription.ExternalToken == null ||
                 (subscription.ExternalTokenExpiry.HasValue &&
                  subscription.ExternalTokenExpiry <= eventTime));
    }
}
