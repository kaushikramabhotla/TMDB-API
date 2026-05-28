using Microsoft.Extensions.Options;
using MongoDB.Driver;
using TMDB_API.Models.Mongo;

namespace TMDB_API.Services
{
    public class MessageService
    {
        private readonly IMongoCollection<ChatMessage> _messages;

        public MessageService(IOptions<MongoDbSettings> settings)
        {
            var mongoClient = new MongoClient(settings.Value.ConnectionString);

            var mongoDatabase = mongoClient.GetDatabase(settings.Value.DatabaseName);

            _messages = mongoDatabase.GetCollection<ChatMessage>(settings.Value.MessagesCollection);
        }

        public async Task SaveMessage(ChatMessage message)
        {
            await _messages.InsertOneAsync(message);
        }

        public async Task<List<ChatMessage>> GetConversation(string user1,string user2)
        {
            var filter = Builders<ChatMessage>.Filter.Or(
                Builders<ChatMessage>.Filter.And(
                    Builders<ChatMessage>.Filter.Eq(x => x.SenderId, user1),
                    Builders<ChatMessage>.Filter.Eq(x => x.ReceiverId, user2)
                ),
                Builders<ChatMessage>.Filter.And(
                    Builders<ChatMessage>.Filter.Eq(x => x.SenderId, user2),
                    Builders<ChatMessage>.Filter.Eq(x => x.ReceiverId, user1)
                )
            );

            return await _messages
                .Find(filter)
                .SortBy(x => x.SentAt)
                .ToListAsync();
        }
    }
}