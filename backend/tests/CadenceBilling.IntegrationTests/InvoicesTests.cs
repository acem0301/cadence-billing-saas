using CadenceBilling.IntegrationTests.Helpers;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace CadenceBilling.IntegrationTests;

public sealed class InvoicesTests(IntegrationTestFactory factory)
    : IClassFixture<IntegrationTestFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    private async Task<Guid> CreateCustomerAsync()
    {
        var response = await _client.PostAsJsonAsync("/customers", new
        {
            name = "Cliente Factura",
            email = "factura@test.com"
        });

        var content = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(content);
        return json.RootElement.GetProperty("id").GetGuid();
    }

    private async Task<Guid> CreateInvoiceAsync(Guid customerId)
    {
        var response = await _client.PostAsJsonAsync("/invoices", new
        {
            description = "Servicio mensual",
            amount = 1500.00,
            customerId
        });

        var content = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(content);
        return json.RootElement.GetProperty("id").GetGuid();
    }

    [Fact]
    public async Task CreateInvoice_WithValidData_ReturnsCreated()
    {
        var token = await AuthHelper.RegisterAndLoginAsync(_client);
        _client.SetBearerToken(token);

        var customerId = await CreateCustomerAsync();

        var response = await _client.PostAsJsonAsync("/invoices", new
        {
            description = "Servicio mensual",
            amount = 1500.00,
            customerId
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task CreateInvoice_StartsAsDraft()
    {
        var token = await AuthHelper.RegisterAndLoginAsync(_client);
        _client.SetBearerToken(token);

        var customerId = await CreateCustomerAsync();
        var invoiceId = await CreateInvoiceAsync(customerId);

        var response = await _client.GetAsync("/invoices");
        var invoices = await response.Content.ReadFromJsonAsync<List<InvoiceResponse>>();

        var invoice = invoices!.First(x => x.Id == invoiceId);
        invoice.Status.Should().Be("Draft");
    }

    [Fact]
    public async Task TransitionInvoice_FromDraftToApproved_Succeeds()
    {
        var token = await AuthHelper.RegisterAndLoginAsync(_client);
        _client.SetBearerToken(token);

        var customerId = await CreateCustomerAsync();
        var invoiceId = await CreateInvoiceAsync(customerId);

        var response = await _client.PostAsJsonAsync($"/invoices/{invoiceId}/transition", new
        {
            newStatus = "Approved"
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task TransitionInvoice_InvalidTransition_ReturnsBadRequest()
    {
        var token = await AuthHelper.RegisterAndLoginAsync(_client);
        _client.SetBearerToken(token);

        var customerId = await CreateCustomerAsync();
        var invoiceId = await CreateInvoiceAsync(customerId);

        var response = await _client.PostAsJsonAsync($"/invoices/{invoiceId}/transition", new
        {
            newStatus = "Paid"
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    private sealed record InvoiceResponse(Guid Id, string Status, decimal Amount);
}