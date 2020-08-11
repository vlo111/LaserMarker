namespace LaserMarker.DataAccess
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.SQLite;
    using System.Globalization;

    public static class UserDataRepository
    {
        private static string db = @"laserMark";

        private static SQLiteConnectionStringBuilder connectionStringBuilder;

        static UserDataRepository()
        {
            connectionStringBuilder = new SQLiteConnectionStringBuilder();

            connectionStringBuilder.DataSource = db;
        }

        public static void CreateIfNotUserTable()
        {
            using (var connection = new SQLiteConnection(connectionStringBuilder.ConnectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();

                command.CommandText = @"CREATE TABLE IF NOT EXISTS [UserData] (
  [Id] INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL
, [Login] text NULL
, [Token] text NULL
, [Password] text NULL
, [Url] text NULL
, [BgImageName] text NOT NULL
, [EzdImageName] text NOT NULL
, [FullImageName] text NOT NULL
, [FullImage] image NOT NULL
, [Sequence] bigint NOT NULL
, [BgImageScale] real NOT NULL
, [BgImagePosX] bigint NOT NULL
, [BgImagePosY] bigint NOT NULL
, [BgImagePosStartX] bigint NOT NULL
, [BgImagePosStartY] bigint NOT NULL
, [EzdImageScale] real NOT NULL
, [EzdImagePosX] bigint NOT NULL
, [EzdImagePosY] bigint NOT NULL
, [EzdImagePosStartX] bigint NOT NULL
, [EzdImagePosStartY] bigint NOT NULL
);
CREATE UNIQUE INDEX IF NOT EXISTS [UserData_UserData_UserData_UserData_UserData_sqlite_autoindex_UserData_1] ON [UserData] ([Id] ASC);";

                command.ExecuteNonQuery();
            }
        }

        public static void Insert(UserDataDto user)
        {
            CreateIfNotUserTable();

            using (var connection = new SQLiteConnection(connectionStringBuilder.ConnectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();

                command.CommandText = $@"DELETE FROM [UserData]
WHERE Sequence = {user.Sequence} and EXISTS (select * from [UserData] where Sequence = {user.Sequence})";

                command.ExecuteNonQuery();

                command.CommandText = $@"INSERT INTO [UserData]
([Token],
[BgImageScale],
[BgImagePosX],
[BgImagePosY],
[BgImagePosStartX],
[BgImagePosStartY],
[EzdImageScale],
[EzdImagePosX],
[EzdImagePosY],
[EzdImagePosStartX],
[EzdImagePosStartY], 
[Sequence],
[Login],
[Password],
[Url],
[FullImage], 
[FullImageName],
[BgImageName],
[EzdImageName])
VALUES ('{user.Token}'
,{user.BgImageScale.ToString(new CultureInfo("en-US"))}
,{user.BgImagePosX}
,{user.BgImagePosY}
,{user.BgImagePosStartX}
,{user.BgImagePosStartY}
,{user.EzdImageScale.ToString(new CultureInfo("en-US"))}
,{user.EzdImagePosX}
,{user.EzdImagePosY}
,{user.EzdImagePosStartX}
,{user.EzdImagePosStartY}
,{user.Sequence},
'{user.Login}',
'{user.Password}', 
'{user.UrlSport}', 
@fullImg, 
'{user.FullImageName}', 
'{user.BgImageName}', 
'{user.EzdImageName}');";

                command.Parameters.Add("@fullImg", DbType.Binary, 10000000).Value = user.FullImage;

                command.ExecuteNonQuery();
            }
        }

        public static List<UserDataDto> GetAllUser()
        {
            CreateIfNotUserTable();

            var user = new List<UserDataDto>();

            try
            {
                using (var connection = new SQLiteConnection(connectionStringBuilder.ConnectionString))
                {
                    connection.Open();

                    var command = connection.CreateCommand();

                    command.CommandText = "SELECT * FROM UserData";

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            user.Add(
                                new UserDataDto
                                {
                                    Id = (long) reader["Id"],
                                    Token = (string) reader["Token"],
                                    Login = (string) reader["Login"],
                                    Password = (string) reader["Password"],
                                    UrlSport = (string) reader["Url"],
                                    FullImage = (byte[]) reader["FullImage"],
                                    FullImageName = (string) reader["FullImageName"],
                                    BgImageName = (string) reader["BgImageName"],
                                    EzdImageName = (string) reader["EzdImageName"],
                                    Sequence = (long) reader["Sequence"],
                                    BgImagePosX = (long) reader["BgImagePosX"],
                                    BgImagePosY = (long) reader["BgImagePosY"],
                                    BgImagePosStartX = (long) reader["BgImagePosX"],
                                    BgImagePosStartY = (long) reader["BgImagePosY"],
                                    BgImageScale = (double) reader["BgImageScale"],
                                    EzdImageScale = (double) reader["EzdImageScale"],
                                    EzdImagePosX = (long) reader["EzdImagePosX"],
                                    EzdImagePosY = (long) reader["EzdImagePosY"],
                                    EzdImagePosStartX = (long) reader["EzdImagePosStartX"],
                                    EzdImagePosStartY = (long) reader["EzdImagePosStartY"],
                                });
                        }
                    }
                }
            }
            catch (Exception e)
            {
                throw e;
            }

            return user;
        }

        public static UserDataDto GetByTabIndex(long index)
        {
            UserDataDto user = new UserDataDto();
            try
            {
                using (var connection = new SQLiteConnection(connectionStringBuilder.ConnectionString))
                {
                    connection.Open();

                    var command = connection.CreateCommand();

                    command.CommandText = $@"SELECT * FROM UserData Where Sequence = {index}";

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            user = new UserDataDto
                            {
                                Id = (long) reader["Id"],
                                Token = (string) reader["Token"],
                                Login = (string) reader["Login"],
                                Password = (string) reader["Password"],
                                UrlSport = (string) reader["Url"],
                                FullImage = (byte[]) reader["FullImage"],
                                FullImageName = (string) reader["FullImageName"],
                                BgImageName = (string) reader["BgImageName"],
                                EzdImageName = (string) reader["EzdImageName"],
                                Sequence = (long) reader["Sequence"],
                                BgImagePosX = (long) reader["BgImagePosX"],
                                BgImagePosY = (long) reader["BgImagePosY"],
                                BgImagePosStartX = (long) reader["BgImagePosStartX"],
                                BgImagePosStartY = (long) reader["BgImagePosStartY"],
                                BgImageScale = (double) reader["BgImageScale"],
                                EzdImageScale = (double) reader["EzdImageScale"],
                                EzdImagePosX = (long) reader["EzdImagePosX"],
                                EzdImagePosY = (long) reader["EzdImagePosY"],
                                EzdImagePosStartX = (long) reader["EzdImagePosStartX"],
                                EzdImagePosStartY = (long) reader["EzdImagePosStartY"],
                            };
                        }
                    }
                }
            }
            catch (Exception e)
            {
                throw e;
            }

            return user;
        }

        public static byte CheckSquence(long index)
        {
            long ifExist = 0;
            try
            {
                using (var connection = new SQLiteConnection(connectionStringBuilder.ConnectionString))
                {
                    connection.Open();

                    var command = connection.CreateCommand();

                    command.CommandText = $@"select EXISTS (select * from [UserData] where Sequence = {index})";

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            ifExist = (long) reader[0];
                        }
                    }
                }
            }
            catch (Exception e)
            {
                throw e;
            }

            return (byte) ifExist;
        }

        public static void DeleteByTabIndex(long index)
        {
            using (var connection = new SQLiteConnection(connectionStringBuilder.ConnectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();

                command.CommandText = $@"DELETE FROM [UserData] WHERE Sequence = {index}";

                command.ExecuteNonQuery();

                var users = GetAllUser();

                long reset_sequence = 0;

                users.ForEach(p => UpdateSequencesById(p.Id, reset_sequence++));
            }
        }

        private static void UpdateSequencesById(long id, long sequence)
        {
            using (var connection = new SQLiteConnection(connectionStringBuilder.ConnectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();

                command.CommandText = $@"UPDATE [UserData]
SET Sequence = {sequence}
WHERE
    Id == {id}";

                command.ExecuteNonQuery();
            }
        }
    }
}