using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PaymentService.Data;
using PaymentService.Models;

namespace PaymentService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentsController : ControllerBase
    {
        private readonly PaymentDbContext _context;

        public PaymentsController(PaymentDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Payment>>> GetPayments()
        {
            return await _context.Payments.AsNoTracking().ToListAsync();
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<Payment>> GetPayment(int id)
        {
            var payment = await _context.Payments.FindAsync(id);
            return payment is null ? NotFound() : Ok(payment);
        }

        [HttpPost]
        public async Task<ActionResult<Payment>> CreatePayment(CreatePaymentRequest request)
        {
            if (request.OrderId <= 0 || request.Amount <= 0 || string.IsNullOrWhiteSpace(request.PaymentMethod))
            {
                return BadRequest("A valid order, positive amount, and payment method are required.");
            }

            var completedAtUtc = DateTime.UtcNow;
            var payment = new Payment
            {
                OrderId = request.OrderId,
                Amount = request.Amount,
                PaymentMethod = request.PaymentMethod.Trim(),
                Status = "Completed",
                CreatedAtUtc = completedAtUtc,
                UpdatedAtUtc = completedAtUtc
            };
            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetPayment), new { id = payment.Id }, payment);
        }

        [HttpPost("{id:int}/confirm")]
        public async Task<IActionResult> ConfirmPayment(int id)
        {
            var payment = await _context.Payments.FindAsync(id);
            if (payment is null)
            {
                return NotFound();
            }
            if (payment.Status == "Completed")
            {
                return Ok(payment);
            }

            return await ChangeStatus(id, "Pending", "Completed");
        }

        [HttpPost("{id:int}/fail")]
        public Task<IActionResult> FailPayment(int id) => ChangeStatus(id, "Pending", "Failed");

        [HttpPost("{id:int}/refund")]
        public async Task<IActionResult> RefundPayment(int id)
        {
            var payment = await _context.Payments.FindAsync(id);
            if (payment is null)
            {
                return NotFound();
            }
            if (payment.Status != "Completed")
            {
                return Conflict("Only completed payments can be refunded.");
            }

            payment.Status = "Refunded";
            payment.RefundedAtUtc = DateTime.UtcNow;
            payment.UpdatedAtUtc = payment.RefundedAtUtc;
            await _context.SaveChangesAsync();
            return Ok(payment);
        }

        private async Task<IActionResult> ChangeStatus(int id, string requiredStatus, string newStatus)
        {
            var payment = await _context.Payments.FindAsync(id);
            if (payment is null)
            {
                return NotFound();
            }
            if (payment.Status != requiredStatus)
            {
                return Conflict($"Only {requiredStatus.ToLowerInvariant()} payments can be marked {newStatus.ToLowerInvariant()}.");
            }

            payment.Status = newStatus;
            payment.UpdatedAtUtc = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return Ok(payment);
        }
    }

    public record CreatePaymentRequest(int OrderId, decimal Amount, string PaymentMethod);
}