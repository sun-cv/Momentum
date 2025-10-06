using System;




namespace Momentum
{
    

    public class Stats
    {
        readonly StatsMediator mediator = new();
        public   StatsMediator Mediator => mediator;

        protected float Resolve(string stat, float baseValue)
        {
            var query = new Query(stat, baseValue);
            mediator.PerformQuery(this, query);
            return query.value;
        }

    }
}
