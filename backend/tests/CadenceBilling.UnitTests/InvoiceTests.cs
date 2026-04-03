using Domain.Entities;
using Domain.Enums;
using Domain.Exceptions;
using FluentAssertions;

namespace CadenceBilling.UnitTests;

public class InvoiceTests
{
    private static Invoice CreateDraftInvoice() => new()
    {
        Id = Guid.NewGuid(),
        Description = "Test Invoice",
        Amount = 100,
        CustomerId = Guid.NewGuid(),
        TenantId = Guid.NewGuid()
    };

    [Fact]
    public void Transition_FromDraft_ToApproved_Succeeds()
    {
        var invoice = CreateDraftInvoice();
        invoice.Transition(InvoiceStatus.Approved);
        invoice.Status.Should().Be(InvoiceStatus.Approved);
    }

    [Fact]
    public void Transition_FromDraft_ToCancelled_Succeeds()
    {
        var invoice = CreateDraftInvoice();
        invoice.Transition(InvoiceStatus.Cancelled);
        invoice.Status.Should().Be(InvoiceStatus.Cancelled);
    }

    [Fact]
    public void Transition_FromApproved_ToSent_Succeeds()
    {
        var invoice = CreateDraftInvoice();
        invoice.Transition(InvoiceStatus.Approved);
        invoice.Transition(InvoiceStatus.Sent);
        invoice.Status.Should().Be(InvoiceStatus.Sent);
    }

    [Fact]
    public void Transition_FromSent_ToPaid_Succeeds()
    {
        var invoice = CreateDraftInvoice();
        invoice.Transition(InvoiceStatus.Approved);
        invoice.Transition(InvoiceStatus.Sent);
        invoice.Transition(InvoiceStatus.Paid);
        invoice.Status.Should().Be(InvoiceStatus.Paid);
    }

    [Fact]
    public void Transition_ToPaid_SetsPaidAt()
    {
        var invoice = CreateDraftInvoice();
        invoice.Transition(InvoiceStatus.Approved);
        invoice.Transition(InvoiceStatus.Sent);

        var before = DateTimeOffset.UtcNow;
        invoice.Transition(InvoiceStatus.Paid);
        var after = DateTimeOffset.UtcNow;

        invoice.PaidAt.Should().NotBeNull();
        invoice.PaidAt.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
    }

    [Fact]
    public void Transition_FromDraft_ToPaid_Throws()
    {
        var invoice = CreateDraftInvoice();
        var act = () => invoice.Transition(InvoiceStatus.Paid);
        act.Should().Throw<InvoiceDomainException>();
    }

    [Fact]
    public void Transition_FromDraft_ToSent_Throws()
    {
        var invoice = CreateDraftInvoice();
        var act = () => invoice.Transition(InvoiceStatus.Sent);
        act.Should().Throw<InvoiceDomainException>();
    }

    [Fact]
    public void Transition_FromPaid_ToAnyStatus_Throws()
    {
        var invoice = CreateDraftInvoice();
        invoice.Transition(InvoiceStatus.Approved);
        invoice.Transition(InvoiceStatus.Sent);
        invoice.Transition(InvoiceStatus.Paid);

        var act = () => invoice.Transition(InvoiceStatus.Cancelled);
        act.Should().Throw<InvoiceDomainException>();
    }

    [Fact]
    public void Transition_FromCancelled_ToAnyStatus_Throws()
    {
        var invoice = CreateDraftInvoice();
        invoice.Transition(InvoiceStatus.Cancelled);

        var act = () => invoice.Transition(InvoiceStatus.Approved);
        act.Should().Throw<InvoiceDomainException>();
    }
}