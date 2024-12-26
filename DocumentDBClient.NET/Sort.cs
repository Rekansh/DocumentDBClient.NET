using MongoDB.Driver;

namespace DocumentDBClient
{
    public enum SortDirection
    {
        Ascending = 1,
        Descending = 2
    }

    public class Sort
    {
        public string Name { get; set; } = string.Empty;
        public SortDirection Direction { get; set; } = SortDirection.Ascending;

        public Sort(string name, SortDirection direction)
        {
            Name = name;
            Direction = direction;
        }

        internal static SortDefinition<TEntity> GetSortDefinitions<TEntity>(List<Sort> sorts)
        {
            if (sorts.Any())
            {
                var sortBuilders = Builders<TEntity>.Sort;
                var sortDefinitions = sorts.Select(x =>
                {
                    SortDefinition<TEntity> sortDefination;
                    if (x.Direction == SortDirection.Descending)
                        sortDefination = sortBuilders.Descending(x.Name);
                    else
                        sortDefination = sortBuilders.Ascending(x.Name);
                    return sortDefination;
                });
                return sortBuilders.Combine(sortDefinitions);
            }
            return null;
        }
    }

}
