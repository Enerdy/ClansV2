﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Data;
using TShockAPI;
using TShockAPI.DB;
using Mono.Data.Sqlite;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;

namespace ClansV2.Managers
{
    public class MemberManager
    {
        private static IDbConnection db;
        internal static void ConnectDB()
        {
            switch (TShock.Config.StorageType.ToLower())
            {
                case "mysql":
                    string[] dbHost = TShock.Config.MySqlHost.Split(':');
                    db = new MySqlConnection()
                    {
                        ConnectionString = string.Format("Server={0}; Port={1}; Database={2}; Uid={3}; Pwd={4};",
                            dbHost[0],
                            dbHost.Length == 1 ? "3306" : dbHost[1],
                            TShock.Config.MySqlDbName,
                            TShock.Config.MySqlUsername,
                            TShock.Config.MySqlPassword)

                    };
                    break;

                case "sqlite":
                    string sql = Path.Combine(TShock.SavePath, "tshock.sqlite");
                    db = new SqliteConnection(string.Format("uri=file://{0},Version=3", sql));
                    break;
            }

            SqlTableCreator sqlcreator = new SqlTableCreator(db, db.GetSqlType() == SqlType.Sqlite ? (IQueryBuilder)new SqliteQueryCreator() : new MysqlQueryCreator());

            sqlcreator.EnsureTableStructure(new SqlTable("ClanMembers",
                new SqlColumn("UserID", MySqlDbType.Int32),
                new SqlColumn("Clan", MySqlDbType.VarChar),
                new SqlColumn("Rank", MySqlDbType.VarChar)));
        }

        internal static ClanMember LoadMemberFromResult(ClanMember member, QueryResult reader)
        {
            member.UserID = reader.Get<int>("UserID");
            member.Clan = ClanManager.GetClanByName(reader.Get<string>("Clan"));
            member.Rank = JsonConvert.DeserializeObject<Tuple<int, string>>(reader.Get<string>("Rank"));
            return member;
        }

        internal static ClanMember GetMemberByID(int ID)
        {
            using (QueryResult reader = db.QueryReader("SELECT * FROM ClanMembers WHERE ID=@0;", ID.ToString()))
            {
                if (reader.Read())
                {
                    return LoadMemberFromResult(new ClanMember(), reader);
                }
            }

            return null;
        }
    }
}