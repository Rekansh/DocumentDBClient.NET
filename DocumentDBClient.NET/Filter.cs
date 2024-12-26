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
    }
}
