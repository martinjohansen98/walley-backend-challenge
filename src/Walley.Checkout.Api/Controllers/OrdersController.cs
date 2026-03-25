using Microsoft.AspNetCore.Mvc;
using Walley.Checkout.Api.Models;
using Walley.Checkout.Api.Services;

namespace Walley.Checkout.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly IRefundService _refundService;

    public OrdersController(IOrderService orderService, IRefundService refundService)
    {
        _orderService = orderService;
        _refundService = refundService;
    }

    /// <summary>
    /// Get all orders.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Order>>> GetAll()
    {
        var orders = await _orderService.GetAllOrdersAsync();
        return Ok(orders);
    }

    /// <summary>
    /// Get a specific order by ID.
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<Order>> GetById(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            return BadRequest(new ApiErrorResponse { Message = "Order ID is required.", StatusCode = 400 });

        var order = await _orderService.GetOrderByIdAsync(id);

        if (order == null)
            return NotFound(new ApiErrorResponse { Message = $"Order '{id}' was not found.", StatusCode = 404 });

        return Ok(order);
    }

    /// <summary>
    /// Create a new order.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<Order>> Create([FromBody] CreateOrderRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.CustomerName))
            return BadRequest(new ApiErrorResponse { Message = "Customer name is required.", StatusCode = 400 });

        if (string.IsNullOrWhiteSpace(request.CustomerEmail))
            return BadRequest(new ApiErrorResponse { Message = "Customer email is required.", StatusCode = 400 });

        if (request.Lines.Count == 0)
            return BadRequest(new ApiErrorResponse { Message = "Order must contain at least one line.", StatusCode = 400 });

        if (request.Lines.Any(l => string.IsNullOrWhiteSpace(l.ProductName) || l.Quantity <= 0 || l.UnitPrice <= 0))
            return BadRequest(new ApiErrorResponse { Message = "Each order line must have a product name, positive quantity and positive unit price.", StatusCode = 400 });

        var order = new Order
        {
            CustomerName = request.CustomerName,
            CustomerEmail = request.CustomerEmail,
            Currency = request.Currency,
            Lines = request.Lines
        };

        var created = await _orderService.CreateOrderAsync(order);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>
    /// Request a refund for an order.
    /// </summary>
    [HttpPost("{orderId}/refunds")]
    public async Task<ActionResult<Refund>> RequestRefund(
        string orderId,
        [FromBody] RefundRequest request)
    {
        var refund = await _refundService.ProcessRefundAsync(
            orderId,
            request.Amount,
            request.Reason);

        if (refund.Status == RefundStatus.Rejected)
        {
            return UnprocessableEntity(refund);
        }

        return Ok(refund);
    }
}

public class RefundRequest
{
    public decimal Amount { get; set; }
    public string Reason { get; set; } = string.Empty;
}

public class CreateOrderRequest
{
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public string Currency { get; set; } = "SEK";
    public List<OrderLine> Lines { get; set; } = new();
}
