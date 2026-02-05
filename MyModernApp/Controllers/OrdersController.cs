using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyModernApp.Data;
using MyModernApp.Models;

namespace MyModernApp.Controllers;

[ApiController]
[Route("v1/orders")]
public class OrdersController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(AppDbContext context, ILogger<OrdersController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Get orders filtered by status
    /// </summary>
    /// <param name="status">Order status (e.g., pending, completed, cancelled)</param>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 50, max: 100)</param>
    /// <returns>List of orders</returns>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Order>>> GetOrders(
        [FromQuery] string? status = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        var startTime = DateTime.UtcNow;
        
        try
        {
            // Validate pagination parameters
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 50;

            var query = _context.Orders.AsQueryable();

            // Apply status filter if provided
            if (!string.IsNullOrWhiteSpace(status))
            {
                // This query will use the IX_Orders_Status index for optimal performance
                query = query.Where(o => o.Status == status);
            }

            // Apply pagination to limit result set
            var orders = await query
                .OrderByDescending(o => o.OrderDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .AsNoTracking() // Improves performance for read-only queries
                .ToListAsync();

            var elapsed = (DateTime.UtcNow - startTime).TotalMilliseconds;
            _logger.LogInformation(
                "Query execution completed in {ElapsedMs}ms for status filter: {Status}, page: {Page}, pageSize: {PageSize}, results: {Count}",
                elapsed, status ?? "all", page, pageSize, orders.Count);

            // Log warning if query takes longer than expected
            if (elapsed > 5000)
            {
                _logger.LogWarning(
                    "Slow query detected: Query took {ElapsedMs}ms for status={Status}",
                    elapsed, status ?? "all");
            }

            return Ok(orders);
        }
        catch (Exception ex)
        {
            var elapsed = (DateTime.UtcNow - startTime).TotalMilliseconds;
            _logger.LogError(ex,
                "Error querying orders with status={Status}. Query execution took {ElapsedMs}ms",
                status ?? "all", elapsed);
            
            return StatusCode(500, new { error = "An error occurred while processing your request." });
        }
    }

    /// <summary>
    /// Get a specific order by ID
    /// </summary>
    /// <param name="id">Order ID</param>
    /// <returns>Order details</returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<Order>> GetOrder(int id)
    {
        var order = await _context.Orders
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null)
        {
            return NotFound();
        }

        return Ok(order);
    }
}
