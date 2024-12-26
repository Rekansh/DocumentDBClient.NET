using MongoDB.Driver;
using System.ComponentModel.DataAnnotations;

namespace DocumentDBClient
{
    public class Filter
    {
        internal Condition Condition { get; set; }

        internal GroupOperator GroupOperator { get; set; } = GroupOperator.NONE;

        internal List<Condition> GroupConditions = new List<Condition>();

        public Filter(Condition condition)
        {
            Condition = condition;
        }

        public Filter(GroupOperator _operator, List<Condition> conditions)
        {
            GroupOperator = _operator;
            GroupConditions = conditions;
        }

        #region Methods
        internal FindOptions<TEntity, TEntity> GetOptions<TEntity>(Sort sort, int pageNo = 0, int pageSize = 0)
        {
            var findOptions = new FindOptions<TEntity, TEntity>();
            if (sort != null && MyConvert.ToString(sort.Name) != string.Empty)
            {
                var sortBuilders = Builders<TEntity>.Sort.Ascending(sort.Name);
                if (sort.Direction == SortDirection.Descending)
                    sortBuilders = Builders<TEntity>.Sort.Descending(sort.Name);
                findOptions = new FindOptions<TEntity, TEntity>()
                {
                    Sort = sortBuilders
                };
            }
            if (pageNo > 0 && pageSize > 0)
            {
                int skipRecords = (pageNo - 1) * pageSize;
                findOptions.Skip = skipRecords;
                findOptions.Limit = pageSize;
            }
            return findOptions;
        }

        internal string ToString()
        {
            if ((GroupConditions.Count > 0 && GroupOperator != GroupOperator.NONE) && Condition != null)
                throw new Exception("Parameter should not have single condition and group condition both.");

            if (!((GroupConditions.Count > 0 && GroupOperator != GroupOperator.NONE) || Condition != null))
                throw new Exception("Parameter should have atleast single condition or group condition.");

            return getFilterProcessStart();
        }

        private string getFilterProcessStart()
        {
            string filterString = string.Empty;
            if (Condition != null)
                filterString = getFilterStringBySingleCondition(Condition);
            else if (GroupConditions.Count > 0 && GroupOperator != GroupOperator.NONE)
                filterString = getFilterStringByGroupCondition(GroupConditions, GroupOperator);
            return filterString;
        }

        private string getOperationString(GroupOperator groupOperator)
        {
            switch (groupOperator)
            {
                case GroupOperator.AND:
                    return "$and";
                case GroupOperator.OR:
                    return "$or";
                case GroupOperator.NOT:
                    return "$not";
                case GroupOperator.NOR:
                    return "$nor";
            }
            return string.Empty;
        }

        private string getFilterStringByGroupCondition(List<Condition> groupConditions, GroupOperator groupOperator)
        {
            string filterString = "{ " + getOperationString(groupOperator) + " : [ ";

            foreach (var groupCondition in groupConditions)
                filterString += getFilterStringBySingleCondition(groupCondition) + ",";

            filterString = filterString.TrimEnd(',') + " ] }";

            return filterString;
        }

        private string getFilterStringBySingleCondition(Condition condition)
        {
            string filterString = "";

            if ((condition.GroupConditions.Count > 0 && condition.GroupOperator != GroupOperator.NONE) && MyConvert.ToString(condition.Parameter) != String.Empty)
                throw new Exception("Condition should not have single condition and group condition both.");

            if (!((condition.GroupConditions.Count > 0 && condition.GroupOperator != GroupOperator.NONE) || MyConvert.ToString(condition.Parameter) != String.Empty))
                throw new Exception("Condition should have atleast single condition or group condition.");

            if (condition.GroupConditions.Count > 0 && condition.GroupOperator != GroupOperator.NONE)
            {
                filterString += getFilterStringByGroupCondition(condition.GroupConditions, condition.GroupOperator);
            }
            else
            {
                if (condition.Parameter != string.Empty)
                {
                    if (condition.Compare == CompareOperator.Equal)
                        filterString += "{ \"" + condition.Parameter + "\" : { $eq: " + condition.DocumentValue + " } } ";
                    else if (condition.Compare == CompareOperator.NotEqual)
                        filterString += "{ \"" + condition.Parameter + "\" : { $ne: " + condition.DocumentValue + " } } ";
                    else if (condition.Compare == CompareOperator.GreaterThan)
                        filterString += "{ \"" + condition.Parameter + "\" : { $gt: " + condition.DocumentValue + " } } ";
                    else if (condition.Compare == CompareOperator.GreaterThanEqual)
                        filterString += "{ \"" + condition.Parameter + "\" : { $gte: " + condition.DocumentValue + " } } ";
                    else if (condition.Compare == CompareOperator.LessThan)
                        filterString += "{ \"" + condition.Parameter + "\" : { $lt: " + condition.DocumentValue + " } } ";
                    else if (condition.Compare == CompareOperator.LessThanEqual)
                        filterString += "{ \"" + condition.Parameter + "\" : { $lte: " + condition.DocumentValue + " } } ";
                    else if (condition.Compare == CompareOperator.Contains)
                        filterString += "{ \"" + condition.Parameter + "\" : { $regex: /.*" + condition.Value + ".*/, $options: 'im' } } ";
                    else if (condition.Compare == CompareOperator.BeginWith)
                        filterString += "{ \"" + condition.Parameter + "\" : { $regex: /^" + condition.Value + "/, $options: 'im' } } ";
                    else if (condition.Compare == CompareOperator.EndWith)
                        filterString += "{ \"" + condition.Parameter + "\" : { $regex: /" + condition.Value + "$/, $options: 'im' } } ";
                    else if (condition.Compare == CompareOperator.In)
                        filterString += "{ \"" + condition.Parameter + "\" : { $in: " + condition.Value + " } } ";
                    else if (condition.Compare == CompareOperator.NotIn)
                        filterString += "{ \"" + condition.Parameter + "\" : { $nin: " + condition.Value + " } } ";
                }
            }
            return filterString;
        }

        #endregion
    }

    public class Condition
    {
        internal string Parameter { get; set; } = string.Empty;

        internal FieldType Type { get; set; } = FieldType.String;

        internal CompareOperator Compare { get; set; } = CompareOperator.Equal;

        internal object Value { get; set; } = string.Empty;

        internal string DocumentValue
        {
            get
            {
                if (Type == FieldType.String)
                    return "\"" + MyConvert.ToString(Value) + "\"";
                else if (Type == FieldType.Number)
                    return MyConvert.ToString(Value);
                else if (Type == FieldType.DateTime)
                    return MyConvert.ToString(Value);
                else
                    return string.Empty;
            }
        }

        internal GroupOperator GroupOperator { get; set; } = GroupOperator.AND;

        internal List<Condition> GroupConditions = new List<Condition>();

        public Condition(string parameter, FieldType type = FieldType.String, CompareOperator compare = CompareOperator.Equal, object value = null)
        {
            Parameter = parameter;
            Type = type;
            Compare = compare;
            Value = value;
        }

        public Condition(string parameter, CompareOperator compare = CompareOperator.Equal, object value = null)
        {
            Parameter = parameter;
            Compare = compare;
            Value = value;
        }

        public Condition(string parameter, object value = null)
        {
            Parameter = parameter;
            Value = value;
        }

        public Condition(GroupOperator groupOperator, List<Condition> groupConditions)
        {
            GroupOperator = groupOperator;
            GroupConditions = groupConditions;
        }
    }

    public enum CompareOperator
    {
        [Display(Name = "==")]
        Equal = 1,

        [Display(Name = "!=")]
        NotEqual = 2,

        [Display(Name = ">")]
        GreaterThan = 3,

        [Display(Name = ">=")]
        GreaterThanEqual = 4,

        [Display(Name = "<")]
        LessThan = 5,

        [Display(Name = "<=")]
        LessThanEqual = 6,

        [Display(Name = "Contains")]
        Contains = 7,

        [Display(Name = "BeginWith")]
        BeginWith = 8,

        [Display(Name = "EndWith")]
        EndWith = 9,

        [Display(Name = "In")]
        In = 10,

        [Display(Name = "NotIn")]
        NotIn = 11

    }

    public enum GroupOperator
    {
        NONE = 0,

        [Display(Name = "||")]
        OR = 1,

        [Display(Name = "&&")]
        AND = 2,

        [Display(Name = "!=")]
        NOT = 3,

        [Display(Name = "!||")]
        NOR = 4
    }

    public enum FieldType
    {
        String = 1,
        Number = 2,
        DateTime = 3
    }
}
