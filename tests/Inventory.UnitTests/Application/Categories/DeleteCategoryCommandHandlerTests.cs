using Inventory.Application.Abstractions;
using Inventory.Application.Categories.Commands.DeleteCategory;
using Inventory.Domain.Exceptions;
using NSubstitute;
using Shouldly;
namespace Inventory.UnitTests.Application.Categories;
public sealed class DeleteCategoryCommandHandlerTests
{
    private readonly ICategoryWriteRepository _categoryWriteRepository = Substitute.For<ICategoryWriteRepository>();
    private readonly IProductReadRepository _productReadRepository = Substitute.For<IProductReadRepository>();
    private readonly DeleteCategoryCommandHandler _handler;
    public DeleteCategoryCommandHandlerTests()
    {
        _handler = new DeleteCategoryCommandHandler(_categoryWriteRepository, _productReadRepository, TimeProvider.System);
    }
    [Fact]
    public async Task Handle_ShouldDeactivateCategory_WhenItHasNoActiveProducts()
    {
        var categoryId = Guid.NewGuid();
        _categoryWriteRepository.DeactivateAsync(categoryId, Arg.Any<DateTime>(), Arg.Any<CancellationToken>()).Returns(true);
        await _handler.Handle(new DeleteCategoryCommand(categoryId), CancellationToken.None);
        await _categoryWriteRepository.Received(1).DeactivateAsync(categoryId, Arg.Any<DateTime>(), Arg.Any<CancellationToken>());
    }
    [Fact]
    public async Task Handle_ShouldThrowConflict_WhenCategoryHasActiveProducts()
    {
        var categoryId = Guid.NewGuid();
        _productReadRepository.CountActiveByCategoryAsync(categoryId, Arg.Any<CancellationToken>()).Returns(2);
        await Should.ThrowAsync<ConflictException>(() => _handler.Handle(new DeleteCategoryCommand(categoryId), CancellationToken.None));
    }
    [Fact]
    public async Task Handle_ShouldThrowNotFound_WhenCategoryDoesNotExist()
    {
        var categoryId = Guid.NewGuid();
        _categoryWriteRepository.DeactivateAsync(categoryId, Arg.Any<DateTime>(), Arg.Any<CancellationToken>()).Returns(false);
        await Should.ThrowAsync<NotFoundException>(() => _handler.Handle(new DeleteCategoryCommand(categoryId), CancellationToken.None));
    }
}
