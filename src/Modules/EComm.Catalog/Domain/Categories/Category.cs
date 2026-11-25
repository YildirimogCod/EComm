using System.Text.RegularExpressions;

namespace EComm.Catalog.Domain.Categories;

internal sealed class Category
{
    public const int NameMaxLength = 100;
    public const int SlugMaxLength = 120;

    private static readonly Regex SlugRegex = new(
        "^[a-z0-9]+(?:-[a-z0-9]+)*$",
        RegexOptions.Compiled);

    public Guid Id { get; private set; }
    public Guid? ParentId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Slug { get; private set; } = string.Empty;
    public int SortOrder { get; private set; }
    public bool IsActive { get; private set; }

    private Category()
    {
    }

    public static Category Create(string name, string slug, int sortOrder = 0, Guid? parentId = null)
    {
        return new Category
        {
            Id = Guid.CreateVersion7(),
            ParentId = ValidateParentId(parentId),
            Name = ValidateName(name),
            Slug = ValidateSlug(slug),
            SortOrder = ValidateSortOrder(sortOrder),
            IsActive = true
        };
    }

    public void Rename(string name)
    {
        Name = ValidateName(name);
    }

    public void ChangeSlug(string slug)
    {
        Slug = ValidateSlug(slug);
    }

    public void ChangeSortOrder(int sortOrder)
    {
        SortOrder = ValidateSortOrder(sortOrder);
    }

    public void Activate()
    {
        IsActive = true;
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    private static Guid? ValidateParentId(Guid? parentId)
    {
        if (parentId == Guid.Empty)
            throw new ArgumentException("Üst kategori kimliği boş olamaz.", nameof(parentId));

        return parentId;
    }

    private static string ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Kategori adı boş olamaz.", nameof(name));

        var trimmed = name.Trim();

        if (trimmed.Length > NameMaxLength)
            throw new ArgumentException($"Kategori adı en fazla {NameMaxLength} karakter olabilir.", nameof(name));

        return trimmed;
    }

    private static string ValidateSlug(string slug)
    {
        if (string.IsNullOrWhiteSpace(slug))
            throw new ArgumentException("Slug boş olamaz.", nameof(slug));

        var normalized = slug.Trim().ToLowerInvariant();

        if (normalized.Length > SlugMaxLength)
            throw new ArgumentException($"Slug en fazla {SlugMaxLength} karakter olabilir.", nameof(slug));

        if (!SlugRegex.IsMatch(normalized))
            throw new ArgumentException("Slug sadece küçük harf, rakam ve tire (-) içerebilir.", nameof(slug));

        return normalized;
    }

    private static int ValidateSortOrder(int sortOrder)
    {
        if (sortOrder < 0)
            throw new ArgumentOutOfRangeException(nameof(sortOrder), "Sıralama değeri negatif olamaz.");

        return sortOrder;
    }
}