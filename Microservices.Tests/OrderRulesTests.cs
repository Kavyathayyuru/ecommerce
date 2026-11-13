extern alias OrderServiceAlias;

using Order = OrderServiceAlias::OrderService.Models.Order;
using OrderRules = OrderServiceAlias::OrderService.Services.OrderRules;
using Xunit;

namespace Microservices.Tests;

public sealed class OrderRulesTests
{
    [Fact]
    public void IsValidRequestRequiresMatchingCustomerAndPositiveValues()
    {
        Assert.True(OrderRules.IsValidRequest(7, 7, 2, 1));
        Assert.False(OrderRules.IsValidRequest(7, 8, 2, 1));
        Assert.False(OrderRules.IsValidRequest(7, 7, 0, 1));
        Assert.False(OrderRules.IsValidRequest(7, 7, 2, 0));
    }

    [Fact]
    public void NormalizeStatusUsesCanonicalStatusAndCompletedCannotBeCancelled()
    {
        Assert.True(OrderRules.IsAllowedStatus("processing"));
        Assert.False(OrderRules.IsAllowedStatus("unknown"));
        Assert.Equal("Processing", OrderRules.NormalizeStatus("processing"));
        Assert.True(OrderRules.CanCancel(new Order { Status = "Pending" }));
        Assert.False(OrderRules.CanCancel(new Order { Status = "Completed" }));
    }
}
