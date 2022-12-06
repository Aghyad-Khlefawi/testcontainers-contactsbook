using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<IMongoDatabase>(sp =>
{
    var mongoClient = new MongoClient(builder.Configuration.GetConnectionString("MongoDb"));
    var mongoDatabase = mongoClient.GetDatabase("contactsBook");
    return mongoDatabase;
});

var app = builder.Build();

// Get all contacts
app.MapGet("/contacts", async ([FromServices] IMongoDatabase mongoDatabase) =>
 Results.Ok(await GetContactsCollection(mongoDatabase).Find(new BsonDocument()).ToListAsync())
);

// Create a new contact
app.MapPost("/contacts", async ([FromBody] Contact contact, [FromServices] IMongoDatabase mongoDatabase) =>
{
    contact.Id = ObjectId.GenerateNewId().ToString();
    await GetContactsCollection(mongoDatabase).InsertOneAsync(contact);
    return Results.Created($"/contacts/{contact.Id}", "contact created");
});

// Get a contact by id
app.MapGet("/contacts/{id}", async ([FromRoute] string id, [FromServices] IMongoDatabase mongoDatabase) =>
{
    var contact = await GetContactsCollection(mongoDatabase).Find(e => e.Id == id).FirstOrDefaultAsync();
    if (contact == null)
        return Results.NotFound("No contact was found with the provided id.");
    return Results.Ok(contact);
});

// Update a contact
app.MapPut("/contacts/{id}", async ([FromRoute] string id, [FromBody] Contact contact, [FromServices] IMongoDatabase mongoDatabase) =>
{
    var collection = GetContactsCollection(mongoDatabase);
    var dbContact = await collection.Find(e => e.Id == id).FirstOrDefaultAsync();
    if (dbContact == null)
        return Results.NotFound("No contact was found with the provided id.");

    dbContact.FirstName = contact.FirstName;
    dbContact.LastName = contact.LastName;
    dbContact.EmailAddress = contact.LastName;

    await collection.ReplaceOneAsync(e => e.Id == id, dbContact);
    return Results.Ok("Contact updated");
});

// Delete a contact
app.MapDelete("/contacts/{id}", async ([FromRoute] string id, [FromServices] IMongoDatabase mongoDatabase) =>
{
    var collection = GetContactsCollection(mongoDatabase);
    var dbContact = await collection.Find(e => e.Id == id).FirstOrDefaultAsync();
    if (dbContact == null)
        return Results.NotFound("No contact was found with the provided id.");

    await collection.DeleteOneAsync(e => e.Id == id);
    return Results.Ok("Contact deleted");
});

app.Run();

IMongoCollection<Contact> GetContactsCollection(IMongoDatabase mongoDatabase) => mongoDatabase.GetCollection<Contact>("contacts");