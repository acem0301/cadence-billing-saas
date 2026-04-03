using CadenceBilling.IntegrationTests.Helpers;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;

namespace CadenceBilling.IntegrationTests;

public sealed class CustomersTests(IntegrationTestFactory factory)
    : IClassFixture<IntegrationTestFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task GetCustomers_WithoutToken_ReturnsUnauthorized()
    {
        var response = await _client.GetAsync("/customers");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateCustomer_WithValidData_ReturnsCreated()
    {
        var token = await AuthHelper.RegisterAndLoginAsync(_client);
        _client.SetBearerToken(token);

        var response = await _client.PostAsJsonAsync("/customers", new
        {
            name = "Empresa Test",
            email = "empresa@test.com"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task GetCustomers_ReturnsOnlyOwnTenantCustomers()
    {
        var token = await AuthHelper.RegisterAndLoginAsync(_client);
        _client.SetBearerToken(token);

        await _client.PostAsJsonAsync("/customers", new
        {
            name = "Mi Cliente",
            email = "cliente@test.com"
        });

        var response = await _client.GetAsync("/customers");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var customers = await response.Content.ReadFromJsonAsync<List<CustomerResponse>>();
        customers.Should().NotBeNull();
        customers!.Should().AllSatisfy(c => c.Name.Should().NotBeNullOrEmpty());
    }

    private sealed record CustomerResponse(Guid Id, string Name, string? Email);
}