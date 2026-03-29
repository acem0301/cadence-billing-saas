using Application.Invoices.Commands;
using Application.Invoices.Queries;
using Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[Authorize]
[ApiController]
[Route("invoices")]
public sealed class InvoicesController(
    GetInvoicesQuery getInvoices,
    CreateInvoiceCommand createInvoice,
    TransitionInvoiceCommand transitionInvoice) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var items = await getInvoices.ExecuteAsync(ct);
        return Ok(items);
    }

    public sealed record CreateInvoiceRequest(string Description, decimal Amount, Guid CustomerId);

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateInvoiceRequest req, CancellationToken ct)
    {
        var result = await createInvoice.ExecuteAsync(
            new CreateInvoiceCommand.Input(req.Description, req.Amount, req.CustomerId), ct);

        return CreatedAtAction(nameof(GetAll), new { id = result.Id }, result);
    }

    public sealed record TransitionRequest(InvoiceStatus NewStatus);

    [HttpPost("{id:guid}/transition")]
    public async Task<IActionResult> Transition(
        Guid id, [FromBody] TransitionRequest req, CancellationToken ct)
    {
        var result = await transitionInvoice.ExecuteAsync(
            new TransitionInvoiceCommand.Input(id, req.NewStatus), ct);

        return Ok(result);
    }
}