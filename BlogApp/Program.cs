using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BlogApp
{
    class Program
    {
        private static string _connectionString = @"Data Source=DEVSQL;Initial Catalog=blog-system;Pooling=true;Integrated Security=SSPI;MultiSubnetFailover=true";

        static void Main( string[] args )
        {
            string command = args[ 0 ];

            if ( command == "readpost" )
            {
                List<Post> posts = ReadPosts();
                foreach ( Post post in posts )
                {
                    Console.WriteLine( post.Title );
                }                
            }
            else if ( command == "insert" )
            {
                int createdPostId = InsertPost( 1, "TITLE", "BODY" );
                Console.WriteLine( "Created post: " + createdPostId );
            }
            else if ( command == "update" )
            {
                UpdatePost( 1, "UPDATED TITLE" );
            }
        }

        private static List<Post> ReadPosts()
        {
            List<Post> posts = new List<Post>();
            using ( SqlConnection connection = new SqlConnection( _connectionString ) )
            {
                connection.Open();
                using ( SqlCommand command = new SqlCommand() )
                {
                    command.Connection = connection;
                    command.CommandText =
                        @"SELECT
                            [PostId],
                            [Title],
                            [Body],
                            [AuthorId],
                            [CreationDateTime]
                        FROM Post";

                    using ( SqlDataReader reader = command.ExecuteReader() )
                    {
                        while ( reader.Read() )
                        {
                            var post = new Post
                            {
                                PostId = Convert.ToInt32( reader[ "PostId" ] ),
                                Title = Convert.ToString( reader[ "Title" ] ),
                                Body = Convert.ToString( reader[ "Body" ] ),
                                AuthorId = Convert.ToInt32( reader[ "AuthorId" ] ),
                                CreationDateTime = Convert.ToDateTime( reader[ "CreationDateTime" ] ),
                            };
                            posts.Add( post );
                        }
                    }
                }
            }
            return posts;
        }

        private static int InsertPost( int authorId, string title, string body )
        {
            using ( SqlConnection connection = new SqlConnection( _connectionString ) )
            {
                connection.Open();
                using ( SqlCommand cmd = connection.CreateCommand() )
                {
                    cmd.CommandText = @"
                    INSERT INTO [Post]
                       ([Title],
                        [Body],
                        [AuthorId],
                        [CreationDateTime]) 
                    VALUES 
                       (@title,
                        @body,
                        @authorId,
                        @creationDateTime)
                    SELECT SCOPE_IDENTITY()";

                    cmd.Parameters.Add( "@title", SqlDbType.NVarChar ).Value = title;
                    cmd.Parameters.Add( "@body", SqlDbType.NVarChar ).Value = body;
                    cmd.Parameters.Add( "@authorId", SqlDbType.Int ).Value = authorId;
                    cmd.Parameters.Add( "@creationDateTime", SqlDbType.DateTime ).Value = DateTime.Now;

                    return Convert.ToInt32( cmd.ExecuteScalar() );
                }
            }
        }

        private static void UpdatePost( int postId, string title )
        {
            using ( SqlConnection connection = new SqlConnection( _connectionString ) )
            {
                using ( SqlCommand command = connection.CreateCommand() )
                {
                    command.CommandText = @"
                        UPDATE [Post]
                        SET [Title] = @title
                        WHERE PostId = @postId";

                    command.Parameters.Add( "@postId", SqlDbType.BigInt ).Value = postId;
                    command.Parameters.Add( "@title", SqlDbType.NVarChar ).Value = title;

                    command.ExecuteNonQuery();
                }
            }
        }
    }
}
