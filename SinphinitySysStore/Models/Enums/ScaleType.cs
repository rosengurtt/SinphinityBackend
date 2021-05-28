using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SinphinitySysStore.Models
{
    public enum ScaleType
    {
        [BsonRepresentation(BsonType.String)]
        major = 0,
        [BsonRepresentation(BsonType.String)]
        minor = 1
    }
}
