using Microsoft.AspNetCore.Mvc;
using Sales.Application.Sales;
using Sales.Application.Sales.Dtos;

namespace Sales.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class SalesController : ControllerBase
{
    private readonly ISaleService _saleService;
    private readonly ISaleRepository _repository;
    private readonly ILogger<SalesController> _logger;

    public SalesController(ISaleService saleService, ISaleRepository repository, ILogger<SalesController> logger)
    {
        _saleService = saleService;
        _repository = repository;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<SaleResponse>>> GetAll(CancellationToken ct)
    {
        var sales = await _repository.GetAllAsync(ct);

        var result = sales.Select(MapToResponse);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<SaleResponse>> GetById(Guid id, CancellationToken ct)
    {
        var sale = await _repository.GetByIdAsync(id, ct);
        if (sale is null)
            return NotFound();

        return Ok(MapToResponse(sale));
    }

    [HttpPost]
    public async Task<ActionResult<SaleResponse>> Create([FromBody] CreateSaleRequest request, CancellationToken ct)
    {
        var sale = await _saleService.CreateSaleAsync(request, ct);

        // Exemplo simples de “evento” logado:
        _logger.LogInformation("CompraEfetuada: {SaleId}", sale.Id);

        return CreatedAtAction(nameof(GetById), new { id = sale.Id }, MapToResponse(sale));
    }

    // PUT e DELETE/Cancel ficariam semelhantes, atualizando/Cancelando e logando
    // CompraAlterada / CompraCancelada / ItemCancelado

    private static SaleResponse MapToResponse(Domain.Sales.Sale sale)
    {
        return new SaleResponse(
            sale.Id,
            sale.Number,
            sale.Date,
            sale.CustomerId,
            sale.CustomerName,
            sale.BranchId,
            sale.BranchName,
            sale.Total,
            sale.Status.ToString(),
            sale.Items.Select(i => new SaleItemResponse(
                i.Id,
                i.ProductId,
                i.ProductName,
                i.Quantity,
                i.UnitPrice,
                i.DiscountPercent,
                i.Total,
                i.IsCanceled
            )).ToList()
        );
    }
}
