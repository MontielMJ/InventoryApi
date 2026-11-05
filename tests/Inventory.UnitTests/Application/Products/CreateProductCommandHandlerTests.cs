using Inventory.Application.Abstractions;
using Inventory.Application.Categories.Dtos;
using Inventory.Application.Products.Commands.CreateProduct;
using Inventory.Domain.Entities;
using Inventory.Domain.Exceptions;
using NSubstitute;
using Shouldly;
namespace Inventory.UnitTests.Application.Products;
public sealed class CreateProductCommandHandlerTests
{
    private readonly IProductReadRepository _productReadRepository = Substitute.For<IProductReadRepository>();
    private readonly IProductWriteRepository _productWriteRepository = Substitute.For<IProductWriteRepository>();
    private readonly ICategoryReadRepository _categoryReadRepository = Substitute.For<ICategoryReadRepository>();
    private readonly CreateProductCommandHandler _handler;
    public CreateProductCommandHandlerTests()
    {
        _handler = new CreateProductCommandHandler(_productReadRepository, _productWriteRepository, _categoryReadRepository, TimeProvider.System);
    }
    [Fact]
    public async Task Handle_ShouldCreateProduct_WhenCommandIsValid()
    {
        var categoryId = Guid.NewGuid();
        _categoryReadRepository.GetByIdAsync(categoryId, Arg.Any<CancellationToken>())
            .Returns(new CategoryDto(categoryId, "Protein", null, true, DateTime.UtcNow, DateTime.UtcNow));
        var command = new CreateProductCommand("SKU-001", "Whey Protein", "Description", categoryId, 49.99m, 5);
        var id = await _handler.Handle(command, CancellationToken.None);
        id.ShouldNotBe(Guid.Empty);
        await _productWriteRepository.Received(1).CreateAsync(Arg.Is<Product>(product => product.Sku == "SKU-001" && product.Stock == 0), Arg.Any<CancellationToken>());
    }
    [Fact]
    public async Task Handle_ShouldThrowNotFound_WhenCategoryDoesNotExist()
    {
        var command = new CreateProductCommand("SKU-001", "Whey Protein", null, Guid.NewGuid(), 49.99m, 5);
        await Should.ThrowAsync<NotFoundException>(() => _handler.Handle(command, CancellationToken.None));
    }
    [Fact]
    public async Task Handle_ShouldThrowConflict_WhenSkuAlreadyExists()
    {
        var categoryId = Guid.NewGuid();
        _categoryReadRepository.GetByIdAsync(categoryId, Arg.Any<CancellationToken>())
            .Returns(new CategoryDto(categoryId, "Protein", null, true, DateTime.UtcNow, DateTime.UtcNow));
        _productReadRepository.SkuExistsAsync("SKU-001", null, Arg.Any<CancellationToken>()).Returns(true);
        var command = new CreateProductCommand("SKU-001", "Whey Protein", null, categoryId, 49.99m, 5);
        await Should.ThrowAsync<ConflictException>(() => _handler.Handle(command, CancellationToken.None));
    }
    [Fact]
    public async Task Handle_ShouldThrowNotFound_WhenCategoryIsInactive()
    {
        var categoryId = Guid.NewGuid();
        _categoryReadRepository.GetByIdAsync(categoryId, Arg.Any<CancellationToken>())
            .Returns(new CategoryDto(categoryId, "Protein", null, false, DateTime.UtcNow, DateTime.UtcNow));
        var command = new CreateProductCommand("SKU-001", "Whey Protein", null, categoryId, 49.99m, 5);
        await Should.ThrowAsync<NotFoundException>(() => _handler.Handle(command, CancellationToken.None));
    }
}
