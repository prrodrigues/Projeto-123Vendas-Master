using System;
using Sales.Domain.Sales;
using Shouldly;
using System.Net.ServerSentEvents;
using Xunit;

namespace Sales.Tests;

public class SaleItemTests
{
    [Fact]
    public void Quantity_less_than_4_should_have_no_discount()
    {
        // Arrange
        var item = new SaleItem(
            productId: Guid.NewGuid(),
            productName: "Produto A",
            quantity: 3,
            unitPrice: 10m
        );

        // Assert
        item.DiscountPercent.ShouldBe(0m);
        item.Total.ShouldBe(30m);
    }

    [Fact]
    public void Quantity_between_4_and_9_should_have_10_percent_discount()
    {
        var item = new SaleItem(
            Guid.NewGuid(),
            "Produto X",
            5,
            10m
        );

        item.DiscountPercent.ShouldBe(0.10m);
        item.Total.ShouldBe(45m);
    }

    [Fact]
    public void Quantity_between_10_and_20_should_have_20_percent_discount()
    {
        var item = new SaleItem(
            Guid.NewGuid(),
            "Produto X",
            12,
            10m
        );

        item.DiscountPercent.ShouldBe(0.20m);
        item.Total.ShouldBe(96m);
    }

    [Fact]
    public void Quantity_above_20_should_throw_exception()
    {
        Should.Throw<ArgumentException>(() =>
            new SaleItem(
                Guid.NewGuid(),
                "Produto X",
                21,
                10m
            )
        );
    }

    [Fact]
    public void UpdateQuantity_should_recalculate_total_and_discount()
    {
        var item = new SaleItem(
            Guid.NewGuid(),
            "Produto A",
            4,
            10m
        );

        // Act
        item.SetQuantity(12);

        // Assert
        item.DiscountPercent.ShouldBe(0.20m);
        item.Total.ShouldBe(96m);
    }
}
