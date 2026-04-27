using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace MissionLogGeneratorService
{
    public class MissionLog
    {
        [BsonId]
        [BsonRepresentation(BsonType.String)]
        public Guid Id { get; set; } = Guid.NewGuid();

        public int MissionId { get; set; }

        public string Message { get; set; } = "";

        public DateTime Timestamp { get; set; }
    }
}
