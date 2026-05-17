namespace BasicCommerce.Domain.ValueObjects;

public sealed record Address(
    string Line1,
    string? Line2,
    string City,
    string District,
    string PostalCode,
    string Country = "BD")
{
    public static Address Create(string line1, string city, string district,
        string postalCode, string? line2 = null, string country = "BD") =>
        new(line1, line2, city, district, postalCode, country);

    public override string ToString() =>
        string.Join(", ", new[] { Line1, Line2, City, District, PostalCode, Country }
            .Where(s => !string.IsNullOrWhiteSpace(s)));
}
