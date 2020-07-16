
using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace myLineRobot.Models
{
    public class UserMessage
    {
        
        [BsonId]
        public ObjectId Id { get; set; }

        public string UserName { get; set; }

        public string Content { get; set; }

        public string TextTime { get; set; }
    }
}