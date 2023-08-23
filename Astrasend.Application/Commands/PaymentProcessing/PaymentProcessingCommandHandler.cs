using Astrasend.Application.ApiClients;
using Astrasend.DataLayer.Repositories;
using Astrasend.Infrastructure.Np.Extensions.Result;
using Astrasend.Models;
using Astrasend.Models.ApiClient;
using Astrasend.Models.Entities;
using MediatR;
using Serilog;

namespace Astrasend.Application.Commands.PaymentProcessing;

/// <summary>
/// Handle for <see cref="PaymentProcessingCommand"/>
/// </summary>
public class PaymentProcessingCommandHandler : IRequestHandler<PaymentProcessingCommand, Result<Unit>>
{
    private readonly IOperationRepository _operationRepository;
    private readonly IApiClient _apiClient;
    private readonly ILogger _logger;

    /// ctor
    public PaymentProcessingCommandHandler(
        IOperationRepository operationRepository,
        IApiClient apiClient,
        ILogger logger)
    {
        _operationRepository = operationRepository;
        _apiClient = apiClient;
        _logger = logger;
    }
    
    /// <inheritdoc />
    public async Task<Result<Unit>> Handle(PaymentProcessingCommand request, CancellationToken cancellationToken)
    {
        var operation = await _operationRepository.AddAsync(new Operation(request), cancellationToken);
        await _operationRepository.SaveChangesAsync(cancellationToken);
        
        try
        {
            await _apiClient.SendInvoiceAsync(
                new InvoiceRequest
                {
                    Id = request.Request.Id,
                    CreditAccountNumber = request.CreditPart.AccountNumber,
                    DebitAccountNumber = request.DebitPart.AccountNumber,
                    DebitAmount = request.DebitPart.Amount,
                    Currency = request.DebitPart.Currency,
                    Details = request.Details,
                    Pack = new SerializableDictionary<string,string>(
                        request.Attributes.Attribute?.ToDictionary(x => x.Key, x => x.Value))
                }, cancellationToken);
            
            operation.TransferredToExternalSystem();
        }
        catch (HttpRequestException e)
        {
            operation.Error();
            _logger.Error(e, "Не успешный код ответа на передачу информации о платеже");
        }
        catch (Exception e)
        {
            operation.Error();
            _logger.Error(e, "Ошибка передачи данных");
        }
        finally
        {
            await _operationRepository.SaveChangesAsync(cancellationToken);
        }
        
        return Unit.Value;
    }
}