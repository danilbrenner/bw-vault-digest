using System.Text.Json;
using Bw.VaultDigest.Infrastructure.BwClientProvider;

namespace Bw.VaultDigest.UnitTests.Infrastructure.BwClientTests;

public class SerializationTests
{
    [Fact]
    public void Deserialize_ValidJson_ReturnsObject()
    {
        const string str = """
                           [
                               {
                                   "passwordHistory": [
                                       {
                                           "lastUsedDate": "2023-04-12T10:00:00Z",
                                           "password": "randomPassword123"
                                       },
                                       {
                                           "lastUsedDate": "2022-12-25T08:45:00Z",
                                           "password": "examplePassword456"
                                       }
                                   ],
                                   "revisionDate": "2023-05-01T11:00:00Z",
                                   "creationDate": "2022-11-15T09:00:00Z",
                                   "deletedDate": null,
                                   "object": "item",
                                   "id": "123e4567-e89b-12d3-a456-426614174000",
                                   "organizationId": null,
                                   "folderId": null,
                                   "type": 1,
                                   "reprompt": 0,
                                   "name": "Test Laptop Pin",
                                   "notes": null,
                                   "favorite": false,
                                   "login": {
                                       "fido2Credentials": [],
                                       "uris": [],
                                       "username": "TestUser",
                                       "password": "testPassword789",
                                       "totp": null,
                                       "passwordRevisionDate": "2023-05-01T11:00:00Z"
                                   },
                                   "collectionIds": []
                               },
                               {
                                   "passwordHistory": [
                                       {
                                           "lastUsedDate": "2023-04-12T10:00:00Z",
                                           "password": "randomPassword123"
                                       },
                                       {
                                           "lastUsedDate": "2022-12-25T08:45:00Z",
                                           "password": "examplePassword456"
                                       }
                                   ],
                                   "revisionDate": "2023-05-01T11:00:00Z",
                                   "creationDate": "2022-11-15T09:00:00Z",
                                   "deletedDate": null,
                                   "object": "item",
                                   "id": "123e4567-e89b-12d3-a456-426614174011",
                                   "organizationId": null,
                                   "folderId": null,
                                   "type": 1,
                                   "reprompt": 0,
                                   "name": "Test ",
                                   "notes": null,
                                   "favorite": false,
                                   "login": {
                                       "fido2Credentials": [],
                                       "uris": [
                                           {
                                               "match": null,
                                               "uri": "https://www.example.com/directory/subdirectory/page?param1=value1&param2=value2&param3=value3&param4=value4&param5=value5&param6=value6&param7=value7&param8=value8&param9=value9&param10=value10&param11=value11&param12=value12&param13=value13&param14=value14&param15=value15&param16=value16&param17=value17&param18=value18&param19=value19&param20=value20"
                           
                                           }
                                       ],
                                       "username": "TestUser",
                                       "password": "testPassword789",
                                       "totp": null,
                                       "passwordRevisionDate": "2023-05-01T11:00:00Z"
                                   },
                                   "collectionIds": []
                               },
                               {
                                   "passwordHistory": [
                                       {
                                           "lastUsedDate": "2024-06-30T12:00:00Z",
                                           "password": "randomPassword321"
                                       }
                                   ],
                                   "revisionDate": "2024-07-10T14:00:00Z",
                                   "creationDate": "2023-01-05T07:30:00Z",
                                   "deletedDate": null,
                                   "object": "item",
                                   "id": "9b1c29d3-45d2-4f76-9e7e-abc123def456",
                                   "organizationId": null,
                                   "folderId": "abc12345-6789-0def-1234-567890abcdef",
                                   "type": 1,
                                   "reprompt": 0,
                                   "name": "example.com",
                                   "notes": null,
                                   "favorite": false,
                                   "login": {
                                       "fido2Credentials": [],
                                       "username": "testuser@example.com",
                                       "password": "examplePassword123",
                                       "totp": null,
                                       "passwordRevisionDate": "2024-06-30T12:00:00Z"
                                   },
                                   "collectionIds": []
                               },
                               {
                                   "passwordHistory": [
                                       {
                                           "lastUsedDate": "2023-02-14T10:00:00Z",
                                           "password": "randomPassword456"
                                       },
                                       {
                                           "lastUsedDate": "2024-03-20T16:00:00Z",
                                           "password": "testPassword321"
                                       }
                                   ],
                                   "revisionDate": "2024-03-20T16:30:00Z",
                                   "creationDate": "2022-04-22T14:15:00Z",
                                   "deletedDate": null,
                                   "object": "item",
                                   "id": "789e12d3-45d6-78a9-bcde-456789abcdef",
                                   "organizationId": null,
                                   "folderId": null,
                                   "type": 1,
                                   "reprompt": 0,
                                   "name": "example.net",
                                   "notes": null,
                                   "favorite": false,
                                   "login": {
                                       "fido2Credentials": [],
                                       "uris": [
                                           {
                                               "match": null,
                                               "uri": "http://"
                                           }
                                       ],
                                       "username": "user@example.net",
                                       "password": "testPassword654",
                                       "totp": null,
                                       "passwordRevisionDate": "2024-03-20T16:30:00Z"
                                   },
                                   "collectionIds": []
                               },
                               {
                                   "passwordHistory": null,
                                   "revisionDate": "2022-09-12T08:00:00Z",
                                   "creationDate": "2021-08-11T07:00:00Z",
                                   "deletedDate": null,
                                   "object": "item",
                                   "id": "456f789a-bcde-1234-5678-90abcdef1234",
                                   "organizationId": null,
                                   "folderId": null,
                                   "type": 2,
                                   "reprompt": 0,
                                   "name": "Sample Note",
                                   "notes": "This is a sample secure note.",
                                   "favorite": false,
                                   "secureNote": {
                                       "type": 0
                                   },
                                   "collectionIds": []
                               },
                               {
                                   "passwordHistory": null,
                                   "revisionDate": "2023-05-06T09:00:00Z",
                                   "creationDate": "2022-10-10T11:30:00Z",
                                   "deletedDate": null,
                                   "object": "item",
                                   "id": "1a2b3c4d-5e6f-7a8b-9c0d-ef1234567890",
                                   "organizationId": null,
                                   "folderId": null,
                                   "type": 3,
                                   "reprompt": 0,
                                   "name": "Sample Card",
                                   "notes": null,
                                   "favorite": false,
                                   "card": {
                                       "cardholderName": "Test User",
                                       "brand": "Visa",
                                       "number": "4111111111111111",
                                       "expMonth": "12",
                                       "expYear": "2025",
                                       "code": "123"
                                   },
                                   "collectionIds": []
                               }
                           ]
                           """;
        var obj = JsonSerializer.Deserialize<IEnumerable<Item>>(str, BwClient.SerializeOptions);
        obj.Should().NotBeNull();
    }
}