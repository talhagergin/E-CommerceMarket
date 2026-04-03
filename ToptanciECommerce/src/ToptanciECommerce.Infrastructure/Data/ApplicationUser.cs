using Microsoft.AspNetCore.Identity;

namespace ToptanciECommerce.Infrastructure.Data;

public class ApplicationUser : IdentityUser
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public bool IsApproved { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Address fields
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? District { get; set; }
    public string? PostalCode { get; set; }
    public string? TaxNumber { get; set; }
    public string? Notes { get; set; }

    // B2B account management
    /// <summary>Geçici şifre (admin tarafından belirlenen, gösterim amaçlı)</summary>
    public string? TempPassword { get; set; }

    /// <summary>Hesap bakiyesi: pozitif = borçlu, negatif = alacaklı</summary>
    public decimal Balance { get; set; } = 0;

    public string FullName => $"{FirstName} {LastName}".Trim();
}
