namespace BECOSOFT.Data.Services.Interfaces.QueryData {
    public interface IServerQueryService : IBaseService {
        bool LinkedServerExists(string linkedServer);
    }
}