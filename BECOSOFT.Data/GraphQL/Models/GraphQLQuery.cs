using GraphQL;

namespace BECOSOFT.Data.GraphQL.Models {
    public abstract class GraphQLQuery : IGraphQLQuery {
        public abstract string GetQuery();

        public GraphQLRequest ToGraphQLRequest(IGraphQLQueryParameters parameters) =>
            new GraphQLRequest {
                Query = GetQuery(),
                Variables = parameters.ToGraphQLParameters(),
            };
    }
}