using EComm.Catalog.Domain.Categories;

namespace EComm.Catalog.Tests.Domain.Categories;

public class CategoryTests
{
    [Fact]
    public void Create_WithParentId_SetsParentId()
    {
        Guid parentId = Guid.CreateVersion7();

        var category = Category.Create("Telefon", "telefon", parentId: parentId);

        Assert.Equal(parentId, category.ParentId);

    }
}
