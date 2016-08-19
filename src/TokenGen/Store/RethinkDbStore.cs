using RethinkDb.Driver;
using RethinkDb.Driver.Ast;
using RethinkDb.Driver.Model;
using RethinkDb.Driver.Net;
using RethinkDb.Driver.Net.Clustering;
using RethinkDbLogProvider;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TokenGen
{
    public class RethinkDbStore : IRethinkDbStore
    {
        private static IRethinkDbConnectionFactory _connectionFactory;
        private static RethinkDB R = RethinkDB.R;
        private string _dbName;

        public RethinkDbStore(IRethinkDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
            _dbName = connectionFactory.GetOptions().Database;
        }

        public string InsertOrUpdateIssuer(Issuer issuer)
        {
            var conn = _connectionFactory.CreateConnection();
            Cursor<Issuer> all = R.Db(_dbName).Table(nameof(Issuer))
                .GetAll(issuer.Name)[new { index = nameof(Issuer.Name) }]
                .Run<Issuer>(conn);

            var issuers = all.ToList();

            if (issuers.Count > 0)
            {
                // update
                R.Db(_dbName).Table(nameof(Issuer)).Get(issuers.First().Id).Update(issuer).RunResult(conn);

                return issuers.First().Id;
            }
            else
            {
                // insert
                var result = R.Db(_dbName).Table(nameof(Issuer))
                    .Insert(issuer)
                    .RunResult(conn);

                return result.GeneratedKeys.First().ToString();
            }
        }

        public List<Token> SearchToken(string querystring)
        {
            var conn = _connectionFactory.CreateConnection();

            // case-insensitive search in all Token fields
            Cursor<Token> all = R.Db(_dbName).Table(nameof(Token)).Filter(t => t.CoerceTo("string").Match($"(?i){querystring}")).RunCursor<Token>(conn);
            return all.OrderByDescending(f => f.Expires).ToList();
        }

        public List<IssuerStatus> GetTokensCountByIssuer()
        {
            var conn = _connectionFactory.CreateConnection();

            var list = R.Db(_dbName).Table(nameof(Token))
                .Group()[new { index = nameof(Token.Issuer) }]
                .Count()
                .RunGrouping<string, long>(conn);

            var issuerReport = list.Select(f => new IssuerStatus
            {
                Name = f.Key,
                TotalTokensIssued = f.Items.First()
            }).ToList();

            return issuerReport;
        }

        public List<IssuerStatus> GetIssuerStatus()
        {
            var conn = _connectionFactory.CreateConnection();
            Cursor<Issuer> all = R.Db(_dbName).Table(nameof(Issuer)).RunCursor<Issuer>(conn);
            var list = all.OrderByDescending(f => f.Timestamp)
                .Select(f => new IssuerStatus
                {
                    Name = f.Name,
                    RegisterDate = f.Timestamp,
                    Version = f.Version,
                    TotalTokensIssued = R.Db(_dbName).Table(nameof(Token)).GetAll(f.Name)[new { index = nameof(Token.Issuer) }].Count().Run<long>(conn)
                }).ToList();

            return list;
        }

        public void InserToken(Token token)
        {
            var conn = _connectionFactory.CreateConnection();
            var result = R.Db(_dbName).Table(nameof(Token))
                .Insert(token)
                .RunResult(conn);
        }

        public TokenStatus GetTokenStatus(string tokenId)
        {
            var conn = _connectionFactory.CreateConnection();
            var tokenStatus = new TokenStatus();

            Token token = R.Db(_dbName).Table(nameof(Token)).Get(tokenId).Run<Token>(conn);

            if(token == null)
            {
                tokenStatus.Status = "Not found";
                return tokenStatus;
            }
            else
            {
                tokenStatus.Issuer = token.Issuer;
                tokenStatus.Expires = token.Expires;
                tokenStatus.Status = DateTime.UtcNow > token.Expires ? "Expired" : "Valid";
            }

            Cursor<Issuer> all = R.Db(_dbName).Table(nameof(Issuer))
                .GetAll(token.Issuer)[new { index = nameof(Issuer.Name) }]
                .Run<Issuer>(conn);

            var issuers = all.ToList();

            if (issuers.Count > 0)
            {
                tokenStatus.IssuerVersion = issuers.First().Version;
                tokenStatus.IssuerTimestamp = issuers.First().Timestamp;
            }

            return tokenStatus;
        }

        public void InitializeDatabase()
        {
            // database
            CreateDb(_dbName);

            // tables
            CreateTable(_dbName, nameof(Token));
            CreateTable(_dbName, nameof(Issuer));

            // indexes
            CreateIndex(_dbName, nameof(Token), nameof(Token.Issuer));
            CreateIndex(_dbName, nameof(Issuer), nameof(Issuer.Name));

            // configure shards and replicas for each table
            //R.Db(dbName).Table(nameof(Token)).Reconfigure().OptArg("shards", 1).OptArg("replicas", 2).Run(conn);
            //R.Db(dbName).Table(nameof(Issuer)).Reconfigure().OptArg("shards", 1).OptArg("replicas", 2).Run(conn);
        }

        protected void CreateDb(string dbName)
        {
            var conn = _connectionFactory.CreateConnection();
            var exists = R.DbList().Contains(db => db == dbName).Run(conn);

            if (!exists)
            {
                R.DbCreate(dbName).Run(conn);
                R.Db(dbName).Wait_().Run(conn);
            }
        }

        protected void DropDb(string dbName)
        {
            var conn = _connectionFactory.CreateConnection();
            var exists = R.DbList().Contains(db => db == dbName).Run(conn);

            if (exists)
            {
                R.DbDrop(dbName).Run(conn);
            }
        }

        protected void CreateTable(string dbName, string tableName)
        {
            var conn = _connectionFactory.CreateConnection();
            var exists = R.Db(dbName).TableList().Contains(t => t == tableName).Run(conn);
            if (!exists)
            {
                R.Db(dbName).TableCreate(tableName).Run(conn);
                R.Db(dbName).Table(tableName).Wait_().Run(conn);
            }
        }

        protected void CreateIndex(string dbName, string tableName, string indexName)
        {
            var conn = _connectionFactory.CreateConnection();
            var exists =  R.Db(dbName).Table(tableName).IndexList().Contains(t => t == indexName).Run(conn);
            if (!exists)
            {
                R.Db(dbName).Table(tableName).IndexCreate(indexName).Run(conn);
                R.Db(dbName).Table(tableName).IndexWait(indexName).Run(conn);
            }
        }

        protected void DropTable(string dbName, string tableName)
        {
            var conn = _connectionFactory.CreateConnection();
            var exists = R.Db(dbName).TableList().Contains(t => t == tableName).Run(conn);
            if (exists)
            {
                R.Db(dbName).TableDrop(tableName).Run(conn);
            }
        }

        public void Reconfigure(int shards, int replicas)
        {
            var conn = _connectionFactory.CreateConnection();
            var tables = R.Db(_dbName).TableList().Run(conn);
            foreach (string table in tables)
            {
                R.Db(_dbName).Table(table).Reconfigure().OptArg("shards", shards).OptArg("replicas", replicas).Run(conn);
                R.Db(_dbName).Table(table).Wait_().Run(conn);
            }
        }

    }
}
