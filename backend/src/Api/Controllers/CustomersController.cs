using Application.Customers.Commands;
using Application.Customers.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[Authorize]
[ApiController]
[Route("customers")]
public sealed class CustomersController(
    GetCustomersQuery getCustomers,
    CreateCustomerCommand createCustomer) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var items = await getCustomers.ExecuteAsync(ct);
        return Ok(items);
    }

    public sealed record CreateCustomerRequest(string Name, string? Email);

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCustomerRequest req, CancellationToken ct)
    {
        var result = await createCustomer.ExecuteAsync(
            new CreateCustomerCommand.Input(req.Name, req.Email), ct);

        return CreatedAtAction(nameof(GetAll), new { id = result.Id }, result);
    }
}