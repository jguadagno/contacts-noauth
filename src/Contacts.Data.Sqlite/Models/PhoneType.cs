using System.Diagnostics.CodeAnalysis;

namespace Contacts.Data.Sqlite.Models;

[ExcludeFromCodeCoverage]

public class PhoneType
{
    public int PhoneTypeId { get; set; }
    public string Type { get; set; }
    public string Description { get; set; }
}