using System.Text;

namespace Rolfin.QueryBuilder;

public class QueryBuilder
{

    string _columns = string.Empty;
    Select _select;

    protected IList<Where>? _wheres = null;
    protected IList<Join>? _joins = null;


    public QueryBuilder() { _select = new Select(this); }

    public Select Select(string columns)
    {
        Guards.NullOrWhiteSpace(columns);

        _columns = columns;
        return _select;
    }
   


    public Where Where(string columnName)
    {
        Guards.NullOrWhiteSpace(columnName);
        _wheres ??= new List<Where>();

        var where = new Where(columnName, _select._tableAlias);
        _wheres.Add(where);

        return where;
    }

    public Join Join(string tableName, string alias, JoinType? joinType = null)
    {
        Guards.NullOrWhiteSpace(tableName);
        _joins ??= new List<Join>();

        var join = new Join(tableName, alias, joinType);
        _joins.Add(join);

        return join;
    }



    public void OrderBy(string column)
    {

    }



    public override string ToString()
    {
        var stringBuilder = new StringBuilder("SELECT ");
        stringBuilder.Append(_columns);
        stringBuilder.Append(Tokens.FROM);
        stringBuilder.Append($"{_select} ");
        stringBuilder.AppendLine();

        if( _joins is not null || _joins?.Count > 0)
        {
            foreach(var join in _joins)
            {
                stringBuilder.Append(Tokens.JOIN);
                stringBuilder.Append(join.ToString() );
                stringBuilder.AppendLine();
            }
        }

        stringBuilder.Append(Tokens.WHERE);

        if (_wheres is not null || _wheres?.Count > 0)
        {
            foreach (var where in _wheres)
            {
                stringBuilder.Append(where.ToString());
                stringBuilder.AppendLine();
            }
        }

        

        return stringBuilder.ToString();
    }

}

public class Select
{
    internal string _tableAlias = string.Empty;
    string _tableName = string.Empty;
    bool isDistinct = false;
    QueryBuilder _builder;

    internal Select(QueryBuilder qb)
    {
        _builder = qb;
    }

    public Select Distinct()
    {
        isDistinct = true;
        return this;
    }

    public QueryBuilder From(string tableName, string alias = "")
    {
        Guards.NullOrWhiteSpace(tableName);

        _tableName = tableName;
        _tableAlias = alias;

        return _builder;
    }


    public override string ToString()
        => string.IsNullOrWhiteSpace(_tableName) is true ? $"{_tableName}" : $"{_tableName} {_tableAlias}";
}

public class QueryBuilder<T> : QueryBuilder  where T : class
{
    T? _obj;

    public QueryBuilder(T obj) : base() 
    { 
        _obj = obj;
    }

    public Where WhereIf(Func<T, bool> condition, string queryCondition)
    {
        if (condition == null) throw new ArgumentNullException("");
        if (condition(_obj!) is false) return new();
        Guards.NullOrWhiteSpace(queryCondition);

        return base.Where(queryCondition);
    }
    public Join JoinIf(Func<T, bool> condition, string tableName, string alias = "", JoinType? joinType = null)
    {
        if(condition == null) throw new ArgumentNullException("field");
        if (condition(_obj!) is false) return new();
        return base.Join(tableName, alias, joinType);
    }
}


public class Join
{
    internal string _tableName = string.Empty;
    internal string _alias = string.Empty;
    internal JoinType? _joinType = null;
    internal string _columnName = string.Empty;

    internal On _on = new();

    internal Join() { }
    internal Join(string tableName, string alias, JoinType? joinType = null)
    {
        Guards.NullOrWhiteSpace(tableName);
        Guards.NullOrWhiteSpace(alias);

        _tableName = tableName;
        _joinType = joinType;
        _alias = alias;
    }


    public On On(string columnName)
    {
        Guards.NullOrWhiteSpace(columnName);
        _columnName = columnName;
        return _on;
    }


    public override string ToString()
    {
        _on.Build();
        return $"{_tableName} {_alias} {Tokens.ON} {_alias}.{_columnName} {_on._condition}";
    }
}

public class On
{
    internal string _condition = string.Empty;
    internal QueryBuilder? _queryBuilder = null;

    public void Eql(int value) { _condition = $"== {value}"; }
    public void Eql(string value) { _condition = $"== {value}"; }
    public void NotEql(string value) { _condition = $"<> {value}"; }
    public void More(string value) { _condition = $"> {value}"; }
    public void Less(string value) { _condition = $"< {value}"; }
    public void MoreOrEql(string value) { _condition = $">= {value}"; }
    public void LessOrEql(string value) { _condition = $"<= {value}"; }
    public void Like(string value) { _condition = $"like '%{value}%'"; } // more specific ex" '%value' or 'value%'

    //Above methods can be used with inner query

    public QueryBuilder In()
    {
        _queryBuilder ??= new QueryBuilder();
        return _queryBuilder;
    }
    public void In<T>(List<T> collection) => new QueryBuilder();
    public void In(string list) => new QueryBuilder();

    public void Build()
    {
        if (_queryBuilder is not null)
            _condition = $"({_queryBuilder})";
    }
}

public class Where
{ 
    string _columnName = string.Empty;
    string _tableAlias = string.Empty;

    string _condition = string.Empty;

    internal Where(string columnName, string tableAlias)
    {
        _tableAlias = tableAlias;
        _columnName = columnName;
    }

    internal Where() { }

    public Where And(string condition) => this;
    public Where Or(string condition) => this;
    public Where Or(params string[] conditions) => this;

    public Where Eql(object value)
    {
        _condition = string.IsNullOrWhiteSpace(_tableAlias) is true ? _columnName : $"{_tableAlias}.{_columnName}";
        _condition += $" == {value}";
        return this;
    }
    public Where More(object value) => this;
    public Where Less(object value)
    {
        _condition = string.IsNullOrWhiteSpace(_tableAlias) is true ? _columnName : $"{_tableAlias}.{_columnName}";
        _condition += $" < {value}";
        return this;
    }
    public Where MoreOrEql(object value) => this;
    public Where LessOrEql(object value) => this;
    public Where Like(string value)
    {
        _condition = string.IsNullOrWhiteSpace(_tableAlias) is true ? _columnName : $"{_tableAlias}.{_columnName}";
        _condition += $" like '%{value}%'";
        return this;
    }

    public override string ToString() => _condition;

}

public enum WhereCondition 
{ 
    AND,
    OR
}

public enum JoinType
{
    LEFT,
    RIGHT,
    INNER
}


internal static class Tokens
{
    public readonly static string OR = "OR";
    public readonly static string ON = "ON";
    public readonly static string AND = "AND";
    public readonly static string FROM = " FROM ";
    public readonly static string JOIN = "JOIN ";
    public readonly static string WHERE = "WHERE ";
    public readonly static string DISTINCT = " DISTINCT ";
}

