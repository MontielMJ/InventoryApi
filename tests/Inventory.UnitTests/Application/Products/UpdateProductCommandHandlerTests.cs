using Inventory.Application.Abstractions;
using Inventory.Application.Categories.Dtos;
using Inventory.Application.Products.Commands.UpdateProduct;
using Inventory.Domain.Entities;
using Inventory.Domain.Exceptions;
using NSubstitute;
using Shouldly;
namespace Inventory.UnitTests.Application.Products;
public sealed class UpdateProductCommandHandlerTests
{
    private readonly IProductReadRepository _productReadRepository = Substitute.For<IProductReadRepository>();
    private readonly IProductWriteRepository _productWriteRepository = Substitute.For<IProductWriteRepository>();
    private readonly ICategoryReadRepository _categoryReadRepository = Substitute.For<ICategoryReadRepository>();
    private readonly UpdateProductCommandHandler _handler;
    public UpdateProductCommandHandlerTests()
    {
        _handler = new UpdateProductCommandHandler(_productReadRepository, _productWriteRepository, _categoryReadRepository, TimeProvider.System);
    }
    [Fact]
    public async Task Handle_ShouldUpdateProduct_WhenCommandIsValid()
    {
        var product = Product.Create("SKU-001", "Old name", null, Guid.NewGuid(), 10m, 1, DateTime.UtcNow);
        var categoryId = Guid.NewGuid();
        _productReadRepository.GetEntityAsync(product.Id, Arg.Any<CancellationToken>()).Returns(product);
        _categoryReadRepository.GetByIdAsync(categoryId, Arg.Any<CancellationToken>()).Returns(new CategoryDto(categoryId, "Protein", null, true, DateTime.UtcNow, DateTime.UtcNow));
        _productWriteRepository.UpdateAsync(product, Arg.Any<CancellationToken>()).Returns(true);
        var command = new UpdateProductCommand(product.Id, "SKU-001", "New name", null, categoryId, 20m, 2);
        await _handler.Handle(command, CancellationToken.None);
        product.Name.ShouldBe("New name");
        await _productWriteRepository.Received(1).UpdateAsync(product, Arg.Any<CancellationToken>());
    }
    [Fact]
    public async Task Handle_ShouldThrowNotFound_WhenProductDoesNotExist()
    {
        var command = new UpdateProductCommand(Guid.NewGuid(), "SKU-001", "Name", null, Guid.NewGuid(), 10m, 1);
        await Should.ThrowAsync<NotFoundException>(() => _handler.Handle(command, CancellationToken.None));
    }
    [Fact]
    public async Task Handle_ShouldThrowConflict_WhenSkuBelongsToAnotherProduct()
    {
        var product = Product.Create("SKU-001", "Name", null, Guid.NewGuid(), 10m, 1, DateTime.UtcNow);
        var categoryId = Guid.NewGuid();
        _productReadRepository.GetEntityAsync(product.Id, Arg.Any<CancellationToken>()).Returns(product);
        _categoryReadRepository.GetByIdAsync(categoryId, Arg.Any<CancellationToken>()).Returns(new CategoryDto(categoryId, "Protein", null, true, DateTime.UtcNow, DateTime.UtcNow));
        _productReadRepository.SkuExistsAsync("SKU-002", product.Id, Arg.Any<CancellationToken>()).Returns(true);
        var command = new UpdateProductCommand(product.Id, "SKU-002", "Name", null, categoryId, 10m, 1);
        await Should.ThrowAsync<ConflictException>(() => _handler.Handle(command, CancellationToken.None));
    }
    [Fact]
    public async Task Handle_ShouldThrowNotFound_WhenCategoryIsInactive()
    {
        var product = Product.Create("SKU-001", "Name", null, Guid.NewGuid(), 10m, 1, DateTime.UtcNow);
        var categoryId = Guid.NewGuid();
        _productReadRepository.GetEntityAsync(product.Id, Arg.Any<CancellationToken>()).Returns(product);
        _categoryReadRepository.GetByIdAsync(categoryId, Arg.Any<CancellationToken>()).Returns(new CategoryDto(categoryId, "Protein", null, false, DateTime.UtcNow, DateTime.UtcNow));
        var command = new UpdateProductCommand(product.Id, "SKU-001", "Name", null, categoryId, 10m, 1);
        await Should.ThrowAsync<NotFoundException>(() => _handler.Handle(command, CancellationToken.None));
    }
}
