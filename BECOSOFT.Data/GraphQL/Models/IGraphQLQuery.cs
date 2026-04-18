using GraphQL;

namespace BECOSOFT.Data.GraphQL.Models {
    public interface IGraphQLQuery {
        string GetQuery();
        GraphQLRequest ToGraphQLRequest(IGraphQLQueryParameters parameters);
    }
}