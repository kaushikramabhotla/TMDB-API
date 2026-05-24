namespace TMDB_API
{
    public class MongoDbSettings
    {
        public string ConnectionString { get; set; }

        public string DatabaseName { get; set; }

        public string MessagesCollection { get; set; }
    }
}