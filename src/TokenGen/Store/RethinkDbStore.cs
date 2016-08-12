using RethinkDb.Driver;
using RethinkDb.Driver.Ast;
using RethinkDb.Driver.Model;
using RethinkDb.Driver.Net;
using RethinkDb.Driver.Net.Clustering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TokenGen
{
    public class RethinkDbStore
    {
        private static RethinkDB R = RethinkDB.R;
        private Connection conn;
        private string dbName;

        public void Connect(string cluster, string db)
        {
            dbName = db;
            EnsureConnection(cluster);
        }

        public string InsertOrUpdateIssuer(Issuer issuer)
        {
            Cursor<Issuer> all = R.Db(dbName).Table(nameof(Issuer))
                .GetAll(issuer.Name)[new { index = nameof(Issuer.Name) }]
                .Run<Issuer>(conn);

            var issuers = all.ToList();

            if (issuers.Count > 0)
            {
                // update
                R.Db(dbName).Table(nameof(Issuer)).Get(issuers.First().Id).Update(issuer).RunResult(conn);

                return issuers.First().Id;
            }
            else
            {
                // insert
                var result = R.Db(dbName).Table(nameof(Issuer))
                    .Insert(issuer)
                    .RunResult(conn);

                return result.GeneratedKeys.First().ToString();
            }
        }

        public List<IssuerStatus> GetIssuerStatus()
        {
            Cursor<Issuer> all = R.Db(dbName).Table(nameof(Issuer)).RunCursor<Issuer>(conn);
            var list = all.OrderByDescending(f => f.Timestamp)
                .Select(f => new IssuerStatus
                {
                    Name = f.Name,
                    RegisterDate = f.Timestamp,
                    Version = f.Version,
                    TotalTokensIssued = R.Db(dbName).Table(nameof(Token)).GetAll(f.Name)[new { index = nameof(Token.Issuer) }].Count().Run<long>(conn)
                }).ToList();

            return list;
        }

        public void InserToken(Token token)
        {
            var result = R.Db(dbName).Table(nameof(Token))
                .Insert(token)
                .RunResult(conn);
        }

        public TokenStatus GetTokenStatus(string tokenId)
        {
            var tokenStatus = new TokenStatus();

            Token token = R.Db(dbName).Table(nameof(Token)).Get(tokenId).Run<Token>(conn);

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

            Cursor<Issuer> all = R.Db(dbName).Table(nameof(Issuer))
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

        private void EnsureConnection(string cluster)
        {
            if (conn == null)
            {
                conn = R.Connection()
                    .Hostname(cluster.Split(':')[0])
                    .Port(Convert.ToInt32(cluster.Split(':')[1]))
                    .Timeout(10)
                    .Connect();
            }
        }

        public void ApplySchema()
        {
            // database
            CreateDb(dbName);

            // tables
            CreateTable(dbName, nameof(Token));
            CreateTable(dbName, nameof(Issuer));

            // indexes
            CreateIndex(dbName, nameof(Token), nameof(Token.Issuer));
            CreateIndex(dbName, nameof(Issuer), nameof(Issuer.Name));

            // configure shards and replicas for each table
            R.Db(dbName).Table(nameof(Token)).Reconfigure().OptArg("shards", 1).OptArg("replicas", 2).Run(conn);
            R.Db(dbName).Table(nameof(Issuer)).Reconfigure().OptArg("shards", 1).OptArg("replicas", 2).Run(conn);
        }

        protected void CreateDb(string dbName)
        {
            var exists = R.DbList().Contains(db => db == dbName).Run(conn);

            if (!exists)
            {
                R.DbCreate(dbName).Run(conn);
                R.Db(dbName).Wait_().Run(conn);
            }
        }

        protected void DropDb(string dbName)
        {
            var exists = R.DbList().Contains(db => db == dbName).Run(conn);

            if (exists)
            {
                R.DbDrop(dbName).Run(conn);
            }
        }

        protected void CreateTable(string dbName, string tableName)
        {
            var exists = R.Db(dbName).TableList().Contains(t => t == tableName).Run(conn);
            if (!exists)
            {
                R.Db(dbName).TableCreate(tableName).Run(conn);
                R.Db(dbName).Table(tableName).Wait_().Run(conn);
            }
        }

        protected void CreateIndex(string dbName, string tableName, string indexName)
        {
            var exists =  R.Db(dbName).Table(tableName).IndexList().Contains(t => t == indexName).Run(conn);
            if (!exists)
            {
                R.Db(dbName).Table(tableName).IndexCreate(indexName).Run(conn);
                R.Db(dbName).Table(tableName).IndexWait(indexName).Run(conn);
            }
        }

        protected void DropTable(string dbName, string tableName)
        {
            var exists = R.Db(dbName).TableList().Contains(t => t == tableName).Run(conn);
            if (exists)
            {
                R.Db(dbName).TableDrop(tableName).Run(conn);
            }
        }

    }
}
