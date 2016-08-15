using System.Collections.Generic;

namespace TokenGen
{
    public interface IRethinkDbStore
    {
        void ApplySchema();
        List<IssuerStatus> GetIssuerStatus();
        TokenStatus GetTokenStatus(string tokenId);
        void InserToken(Token token);
        string InsertOrUpdateIssuer(Issuer issuer);
    }
}