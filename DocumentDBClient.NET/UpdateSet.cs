using MongoDB.Driver;

namespace DocumentDBClient
{
    public class UpdateSet
    {
        public string? FilterId { get; set; }
        public Filter? Filter { get; set; }
        public List<FieldValue> FieldValues { get; set; } = new List<FieldValue>();

        public UpdateSet() 
        { 
        }
        public UpdateSet(string filterId, FieldValue fieldValue)
        {
            FilterId = filterId;
            FieldValues.Add(fieldValue);
        }
        public UpdateSet(string filterId, List<FieldValue> fieldValues)
        {
            FilterId = filterId;
            FieldValues = fieldValues;
        }
        public UpdateSet(Filter filter, FieldValue fieldValue)
        {
            Filter = filter;
            FieldValues.Add(fieldValue);
        }
        public UpdateSet(Filter filter, List<FieldValue> fieldValues)
        {
            Filter = filter;
            FieldValues = fieldValues;
        }

        public UpdateDefinition<TEntity> GetUpdateDefinition<TEntity>()
        {
            var update = Builders<TEntity>.Update;
            var updates = new List<UpdateDefinition<TEntity>>();
            foreach (var updateFieldValue in FieldValues)
                updates.Add(update.Set(updateFieldValue.Field, updateFieldValue.Value));
            return update.Combine(updates);
        }
    }
    public class FieldValue
    {
        internal string Field { get; set; } = string.Empty;
        internal object Value { get; set; } = string.Empty;

        public FieldValue(string field, object value)
        {
            Field = field;
            Value = value;
        }
    }
}
