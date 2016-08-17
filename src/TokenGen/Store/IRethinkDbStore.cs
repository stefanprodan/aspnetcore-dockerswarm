using System.Collections.Generic;

namespace TokenGen
{
    public interface IRethinkDbStore
    {
        void InitializeDatabase();
        List<IssuerStatus> GetIssuerStatus();
        TokenStatus GetTokenStatus(string tokenId);
        void InserToken(Token token);
        string InsertOrUpdateIssuer(Issuer issuer);
        void Reconfigure(int shards, int replicas);
    }
}