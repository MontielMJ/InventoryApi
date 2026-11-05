using FluentValidation.TestHelper;
using Inventory.Application.Products.Commands.CreateProduct;
namespace Inventory.UnitTests.Application.Products;
public sealed class CreateProductCommandValidatorTests
{
    private readonly CreateProductCommandValidator _validator = new();
    [Fact]
    public void Validate_ShouldPass_WhenCommandIsValid()
    {
        var command = new CreateProductCommand("SKU-001", "Whey Protein", "Description", Guid.NewGuid(), 49.99m, 5);
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }
    [Fact]
    public void Validate_ShouldFail_WhenPriceIsNotPositive()
    {
        var command = new CreateProductCommand("SKU-001", "Whey Protein", null, Guid.NewGuid(), 0m, 5);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(product => product.Price);
    }
    [Fact]
    public void Validate_ShouldFail_WhenSkuContainsInvalidCharacters()
    {
        var command = new CreateProductCommand("SKU 001", "Whey Protein", null, Guid.NewGuid(), 49.99m, 5);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(product => product.Sku);
    }
    [Fact]
    public void Validate_ShouldFail_WhenCategoryIsEmpty()
    {
        var command = new CreateProductCommand("SKU-001", "Whey Protein", null, Guid.Empty, 49.99m, 5);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(product => product.CategoryId);
    }
}
