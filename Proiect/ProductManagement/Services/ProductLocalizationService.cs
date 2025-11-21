using System.Globalization;
using ProductManagement.Features.Products;

namespace ProductManagement.Services;

/// <summary>
/// Service for managing product-related translations across multiple languages.
/// </summary>
public class ProductLocalizationService
{
    private readonly ILogger<ProductLocalizationService> _logger;
    private readonly Dictionary<string, Dictionary<string, string>> _translations;

    public ProductLocalizationService(ILogger<ProductLocalizationService> logger)
    {
        _logger = logger;
        _translations = InitializeTranslations();
    }

    private Dictionary<string, Dictionary<string, string>> InitializeTranslations()
    {
        return new Dictionary<string, Dictionary<string, string>>
        {
            ["en"] = new Dictionary<string, string>
            {
                ["product.created"] = "Product created successfully",
                ["product.updated"] = "Product updated successfully",
                ["product.deleted"] = "Product deleted successfully",
                ["product.notfound"] = "Product not found",
                ["category.electronics"] = "Electronics & Technology",
                ["category.clothing"] = "Clothing & Apparel",
                ["category.home"] = "Home & Garden",
                ["category.books"] = "Books & Media",
                ["category.sports"] = "Sports & Outdoors",
                ["category.toys"] = "Toys & Games",
                ["stock.available"] = "In Stock",
                ["stock.low"] = "Low Stock",
                ["stock.out"] = "Out of Stock"
            },
            ["ro"] = new Dictionary<string, string>
            {
                ["product.created"] = "Produs creat cu succes",
                ["product.updated"] = "Produs actualizat cu succes",
                ["product.deleted"] = "Produs șters cu succes",
                ["product.notfound"] = "Produs negăsit",
                ["category.electronics"] = "Electronică și Tehnologie",
                ["category.clothing"] = "Îmbrăcăminte și Accesorii",
                ["category.home"] = "Casă și Grădină",
                ["category.books"] = "Cărți și Media",
                ["category.sports"] = "Sport și Activități",
                ["category.toys"] = "Jucării și Jocuri",
                ["stock.available"] = "În Stoc",
                ["stock.low"] = "Stoc Limitat",
                ["stock.out"] = "Stoc Epuizat"
            },
            ["fr"] = new Dictionary<string, string>
            {
                ["product.created"] = "Produit créé avec succès",
                ["product.updated"] = "Produit mis à jour avec succès",
                ["product.deleted"] = "Produit supprimé avec succès",
                ["product.notfound"] = "Produit introuvable",
                ["category.electronics"] = "Électronique et Technologie",
                ["category.clothing"] = "Vêtements et Accessoires",
                ["category.home"] = "Maison et Jardin",
                ["category.books"] = "Livres et Médias",
                ["category.sports"] = "Sports et Loisirs",
                ["category.toys"] = "Jouets et Jeux",
                ["stock.available"] = "En Stock",
                ["stock.low"] = "Stock Limité",
                ["stock.out"] = "Rupture de Stock"
            }
        };
    }

    /// <summary>
    /// Gets a translated string for the given key and culture.
    /// </summary>
    /// <param name="key">The translation key.</param>
    /// <param name="culture">The culture code (e.g., "en", "ro", "fr"). Defaults to current culture.</param>
    /// <returns>The translated string, or the key itself if not found.</returns>
    public string GetTranslation(string key, string? culture = null)
    {
        var cultureCode = culture ?? CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
        
        // Fallback to English if culture not found
        if (!_translations.ContainsKey(cultureCode))
        {
            _logger.LogWarning("Culture {Culture} not found, falling back to English", cultureCode);
            cultureCode = "en";
        }

        if (_translations[cultureCode].TryGetValue(key, out var translation))
        {
            return translation;
        }

        // Fallback to English for missing keys
        if (cultureCode != "en" && _translations["en"].TryGetValue(key, out var englishTranslation))
        {
            _logger.LogWarning("Translation key {Key} not found for {Culture}, using English", key, cultureCode);
            return englishTranslation;
        }

        _logger.LogWarning("Translation key {Key} not found", key);
        return key;
    }

    /// <summary>
    /// Gets the localized display name for a product category.
    /// </summary>
    /// <param name="category">The product category.</param>
    /// <param name="culture">The culture code. Defaults to current culture.</param>
    /// <returns>The localized category name.</returns>
    public string GetCategoryName(ProductCategory category, string? culture = null)
    {
        var key = $"category.{category.ToString().ToLower()}";
        return GetTranslation(key, culture);
    }

    /// <summary>
    /// Gets the stock status message based on the stock quantity.
    /// </summary>
    /// <param name="stockQuantity">The quantity of stock.</param>
    /// <param name="culture">The culture code. Defaults to current culture.</param>
    /// <returns>The stock status message.</returns>
    public string GetStockStatus(int stockQuantity, string? culture = null)
    {
        var key = stockQuantity switch
        {
            0 => "stock.out",
            <= 5 => "stock.low",
            _ => "stock.available"
        };
        return GetTranslation(key, culture);
    }
}
