// ProcessImage/Controllers/SubscriptionsController.cs

using Data.SDK.Repository;
using ProcessImage.Entities; 
using Microsoft.AspNetCore.Mvc;
using ProcessImage.repository;

[Route("api/[controller]")]
[ApiController]
public class SubscriptionsController : ControllerBase
{
    private readonly IAccesor _repository;
    public SubscriptionsController(IAccesor repository)
    {
        _repository = repository;
    }
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Subscription>>> GetSubscriptions()
    {
        // Folosește metoda moștenită GetAllAsync()
        var subscriptions = await _repository.GetAllAsync();
        return Ok(subscriptions);
    }
    [HttpGet("{id}")]
    public async Task<ActionResult<Subscription>> GetSubscription(int id)
    {
        // Folosește metoda moștenită GetByIdAsync()
        var subscription = await _repository.GetByIdAsync(id);

        if (subscription == null)
        {
            return NotFound();
        }

        return Ok(subscription);
    }
    [HttpGet("procesare")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<SubscripteProcesare>>> GetSubscripteProcesare()
    {
        try
        {
            var procesari = await _repository.GetAllSubscripteProcesareAsync();

            if (!procesari.Any())
            {
                return NotFound("Nu s-au găsit înregistrări în tabelul SubscripteProcesare.");
            }

            return Ok(procesari);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError,
                $"Eroare la extragerea Subscriptiilor de Procesare: {ex.Message}");
        }
    }
}