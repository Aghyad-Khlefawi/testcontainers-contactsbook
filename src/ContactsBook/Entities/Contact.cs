using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

public class Contact
{
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = default!;
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; }= default!;
    public string EmailAddress { get; set; }= default!;
}