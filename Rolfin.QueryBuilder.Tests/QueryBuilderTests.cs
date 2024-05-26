namespace Rolfin.QueryBuilder.Tests
{
    public class QueryBuilderTests
    {
        [Fact]
        public void Creation()
        {
            var queryBuilder = new QueryBuilder<object>(new { });
            queryBuilder.Select("*").Distinct().From("table").Join("table2", "tbl2").On("id").Eql(32);
            queryBuilder.WhereIf(x => x != null, "table.version").Less(22);

            queryBuilder.Where("id").More("2")
                .Or("name").Like("florin");

            queryBuilder.Join("Items", "itms")
                .On("name").Eql("florin");

            var result = queryBuilder.ToString();

            Assert.NotEmpty(result);
        }
    }
}