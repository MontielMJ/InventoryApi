using Inventory.Application.Abstractions;
using Inventory.Application.Common;
using Inventory.Application.Products.Dtos;
using Inventory.Application.Products.Queries.GetProducts;
using NSubstitute;
using Shouldly;
namespace Inventory.UnitTests.Application.Products;
public sealed class GetProductsQueryHandlerTests
{
    private readonly IProductReadRepository _productReadRepository = Substitute.For<IProductReadRepository>();
    [Fact]
    public async Task Handle_ShouldNormalizePagination_WhenValuesAreOutOfRange()
    {
        var handler = new GetProductsQueryHandler(_productReadRepository);
        _productReadRepository.GetPagedAsync(Arg.Any<GetProductsQuery>(), Arg.Any<CancellationToken>())
            .Returns(new PagedResult<ProductDto>([], 1, 100, 0));
        var query = new GetProductsQuery(null, "  whey  ", null, 0, 500);
        await handler.Handle(query, CancellationToken.None);
        await _productReadRepository.Received(1).GetPagedAsync(
            Arg.Is<GetProductsQuery>(normalized => normalized.Page == 1 && normalized.PageSize == 100 && normalized.Search == "whey"),
            Arg.Any<CancellationToken>());
    }
    [Fact]
    public async Task Handle_ShouldReturnRepositoryResult()
    {
        var handler = new GetProductsQueryHandler(_productReadRepository);
        var expected = new PagedResult<ProductDto>([], 2, 10, 0);
        _productReadRepository.GetPagedAsync(Arg.Any<GetProductsQuery>(), Arg.Any<CancellationToken>()).Returns(expected);
        var result = await handler.Handle(new GetProductsQuery(null, null, true, 2, 10), CancellationToken.None);
        result.ShouldBe(expected);
    }
    [Fact]
    public async Task Handle_ShouldUseDefaultPageSize_WhenValueIsZero()
    {
        var handler = new GetProductsQueryHandler(_productReadRepository);
        _productReadRepository.GetPagedAsync(Arg.Any<GetProductsQuery>(), Arg.Any<CancellationToken>())
            .Returns(new PagedResult<ProductDto>([], 1, 10, 0));
        await handler.Handle(new GetProductsQuery(null, null, null, 0, 0), CancellationToken.None);
        await _productReadRepository.Received(1).GetPagedAsync(
            Arg.Is<GetProductsQuery>(normalized => normalized.Page == 1 && normalized.PageSize == 10),
            Arg.Any<CancellationToken>());
    }
}
