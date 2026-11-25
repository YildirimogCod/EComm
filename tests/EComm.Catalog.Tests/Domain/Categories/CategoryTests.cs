using EComm.Catalog.Domain.Categories;

namespace EComm.Catalog.Tests.Domain.Categories;

public class CategoryTests
{
    public static TheoryData<string> InvalidNames => new()
    {
        "",
        "   ",
        new string('a', Category.NameMaxLength + 1)
    };

    public static TheoryData<string> InvalidSlugs => new()
    {
        "a b",
        "-a",
        "a-",
        "a--b",
        "çocuk",
        "a_b",
        "",
        "   ",
        new string('a', Category.SlugMaxLength + 1)
    };

    // ---------- Create ----------

    [Fact]
    public void Create_WithValidValues_SetsPropertiesAndIsActive()
    {
        var category = Category.Create("Elektronik", "elektronik", sortOrder: 5);

        Assert.NotEqual(Guid.Empty, category.Id);
        Assert.Equal("Elektronik", category.Name);
        Assert.Equal("elektronik", category.Slug);
        Assert.Equal(5, category.SortOrder);
        Assert.True(category.IsActive);
    }

    [Fact]
    public void Create_WithoutOptionalParameters_UsesDefaultValues()
    {
        var category = Category.Create("Elektronik", "elektronik");

        Assert.Equal(0, category.SortOrder);
        Assert.Null(category.ParentId);
    }

    [Fact]
    public void Create_WithParentId_SetsParentId()
    {
        var parentId = Guid.CreateVersion7();

        var category = Category.Create("Telefon", "telefon", parentId: parentId);

        Assert.Equal(parentId, category.ParentId);
    }

    [Fact]
    public void Create_WithEmptyParentId_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            Category.Create("Elektronik", "elektronik", parentId: Guid.Empty));
    }

    [Fact]
    public void Create_CalledTwice_GeneratesDistinctIds()
    {
        var first = Category.Create("Elektronik", "elektronik");
        var second = Category.Create("Giyim", "giyim");

        Assert.NotEqual(first.Id, second.Id);
    }

    // ---------- Name ----------

    [Fact]
    public void Create_WithUntrimmedName_TrimsName()
    {
        var category = Category.Create("  Kadın  ", "kadin");

        Assert.Equal("Kadın", category.Name);
    }

    [Theory]
    [MemberData(nameof(InvalidNames))]
    public void Create_WithInvalidName_ThrowsArgumentException(string invalidName)
    {
        Assert.Throws<ArgumentException>(() => Category.Create(invalidName, "slug"));
    }

    [Fact]
    public void Create_WithNullName_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => Category.Create(null!, "slug"));
    }

    [Fact]
    public void Create_WithMaxLengthName_Succeeds()
    {
        var maxLengthName = new string('a', Category.NameMaxLength);

        var category = Category.Create(maxLengthName, "slug");

        Assert.Equal(maxLengthName, category.Name);
    }

    [Fact]
    public void Create_WithPaddedMaxLengthName_Succeeds_BecauseLengthIsMeasuredAfterTrim()
    {
        var maxLengthName = new string('a', Category.NameMaxLength);

        var category = Category.Create($"  {maxLengthName}  ", "slug");

        Assert.Equal(maxLengthName, category.Name);
    }

    [Fact]
    public void Rename_WithValidName_UpdatesAndTrimsName()
    {
        var category = Category.Create("Eski Ad", "eski-ad");

        category.Rename("  Yeni Ad  ");

        Assert.Equal("Yeni Ad", category.Name);
    }

    [Theory]
    [MemberData(nameof(InvalidNames))]
    public void Rename_WithInvalidName_ThrowsArgumentException(string invalidName)
    {
        var category = Category.Create("Geçerli Ad", "gecerli-ad");

        Assert.Throws<ArgumentException>(() => category.Rename(invalidName));
    }

    [Fact]
    public void Rename_WithInvalidName_KeepsOldName()
    {
        var category = Category.Create("Eski Ad", "eski-ad");

        Assert.Throws<ArgumentException>(() => category.Rename(""));

        Assert.Equal("Eski Ad", category.Name);
    }

    // ---------- Slug ----------

    [Theory]
    [InlineData("cocuk-gomlek")]
    [InlineData("t-shirt")]
    [InlineData("123")]
    [InlineData("a")]
    public void Create_WithValidSlug_SetsSlug(string validSlug)
    {
        var category = Category.Create("Kategori", validSlug);

        Assert.Equal(validSlug, category.Slug);
    }

    [Fact]
    public void Create_WithUppercaseSlug_NormalizesSlugToLowercase()
    {
        var category = Category.Create("Elbise", "Elbise");

        Assert.Equal("elbise", category.Slug);
    }

    [Fact]
    public void Create_WithUntrimmedSlug_TrimsSlug()
    {
        var category = Category.Create("Elbise", "  elbise  ");

        Assert.Equal("elbise", category.Slug);
    }

    [Theory]
    [MemberData(nameof(InvalidSlugs))]
    public void Create_WithInvalidSlug_ThrowsArgumentException(string invalidSlug)
    {
        Assert.Throws<ArgumentException>(() => Category.Create("Kategori", invalidSlug));
    }

    [Fact]
    public void Create_WithNullSlug_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => Category.Create("Kategori", null!));
    }

    [Fact]
    public void Create_WithMaxLengthSlug_Succeeds()
    {
        var maxLengthSlug = new string('a', Category.SlugMaxLength);

        var category = Category.Create("Kategori", maxLengthSlug);

        Assert.Equal(maxLengthSlug, category.Slug);
    }

    [Fact]
    public void ChangeSlug_WithValidSlug_UpdatesSlug()
    {
        var category = Category.Create("Kategori", "eski-slug");

        category.ChangeSlug("yeni-slug");

        Assert.Equal("yeni-slug", category.Slug);
    }

    [Fact]
    public void ChangeSlug_WithUppercaseSlug_NormalizesSlugToLowercase()
    {
        var category = Category.Create("Kategori", "eski-slug");

        category.ChangeSlug("Yeni-Slug");

        Assert.Equal("yeni-slug", category.Slug);
    }

    [Theory]
    [MemberData(nameof(InvalidSlugs))]
    public void ChangeSlug_WithInvalidSlug_ThrowsArgumentException(string invalidSlug)
    {
        var category = Category.Create("Kategori", "gecerli-slug");

        Assert.Throws<ArgumentException>(() => category.ChangeSlug(invalidSlug));
    }

    [Fact]
    public void ChangeSlug_WithInvalidSlug_KeepsOldSlug()
    {
        var category = Category.Create("Kategori", "eski-slug");

        Assert.Throws<ArgumentException>(() => category.ChangeSlug("a b"));

        Assert.Equal("eski-slug", category.Slug);
    }

    // ---------- SortOrder ----------

    [Fact]
    public void Create_WithNegativeSortOrder_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            Category.Create("Kategori", "kategori", sortOrder: -1));
    }

    [Fact]
    public void Create_WithZeroSortOrder_Succeeds()
    {
        var category = Category.Create("Kategori", "kategori", sortOrder: 0);

        Assert.Equal(0, category.SortOrder);
    }

    [Fact]
    public void ChangeSortOrder_WithValidValue_UpdatesSortOrder()
    {
        var category = Category.Create("Kategori", "kategori", sortOrder: 1);

        category.ChangeSortOrder(10);

        Assert.Equal(10, category.SortOrder);
    }

    [Fact]
    public void ChangeSortOrder_WithNegativeValue_ThrowsArgumentOutOfRangeException()
    {
        var category = Category.Create("Kategori", "kategori");

        Assert.Throws<ArgumentOutOfRangeException>(() => category.ChangeSortOrder(-1));
    }

    [Fact]
    public void ChangeSortOrder_WithNegativeValue_KeepsOldSortOrder()
    {
        var category = Category.Create("Kategori", "kategori", sortOrder: 3);

        Assert.Throws<ArgumentOutOfRangeException>(() => category.ChangeSortOrder(-1));

        Assert.Equal(3, category.SortOrder);
    }

    // ---------- Activate / Deactivate ----------

    [Fact]
    public void Deactivate_ActiveCategory_SetsIsActiveFalse()
    {
        var category = Category.Create("Kategori", "kategori");

        category.Deactivate();

        Assert.False(category.IsActive);
    }

    [Fact]
    public void Activate_DeactivatedCategory_SetsIsActiveTrue()
    {
        var category = Category.Create("Kategori", "kategori");
        category.Deactivate();

        category.Activate();

        Assert.True(category.IsActive);
    }

    [Fact]
    public void Deactivate_AlreadyInactiveCategory_RemainsInactive()
    {
        var category = Category.Create("Kategori", "kategori");
        category.Deactivate();

        category.Deactivate();

        Assert.False(category.IsActive);
    }

    [Fact]
    public void Activate_AlreadyActiveCategory_RemainsActive()
    {
        var category = Category.Create("Kategori", "kategori");

        category.Activate();

        Assert.True(category.IsActive);
    }
}