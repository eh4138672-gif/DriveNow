using AutoMapper;
using Microsoft.Extensions.Logging;
using MyProject.Core.DTOs;
using MyProject.Core.Entities;
using MyProject.Core.Exceptions;
using MyProject.Core.Interfaces;
using MyProject.Core.Results;

namespace MyProject.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly ICustomerRepository _customers;
        private readonly IRentalRepository _rentals;
        private readonly IMapper _mapper;
        private readonly ILogger<CustomerService> _logger;

        public CustomerService(ICustomerRepository customers, IRentalRepository rentals, IMapper mapper, ILogger<CustomerService> logger)
        {
            _customers = customers;
            _rentals = rentals;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<PagedResult<CustomerDto>> GetAllAsync(CustomerQueryParameters query, CancellationToken ct)
        {
            var page = await _customers.GetPagedAsync(query, ct);
            return new PagedResult<CustomerDto>(_mapper.Map<List<CustomerDto>>(page.Items), page.Page, page.PageSize, page.TotalCount);
        }

        public async Task<Result<CustomerDto>> GetByIdAsync(int id, CancellationToken ct)
        {
            var customer = await _customers.GetByIdAsync(id, ct);
            return customer == null
                ? Result<CustomerDto>.NotFound($"לקוח {id} לא נמצא")
                : Result<CustomerDto>.Success(_mapper.Map<CustomerDto>(customer));
        }

        public async Task<Result<CustomerDto>> CreateAsync(CustomerRequestDto dto, CancellationToken ct)
        {
            if (await _customers.DriverLicenseExistsAsync(dto.DriverLicenseId, null, ct))
            {
                return Result<CustomerDto>.Conflict($"כבר קיים לקוח עם רישיון נהיגה {dto.DriverLicenseId}");
            }

            var customer = _mapper.Map<Customer>(dto);
            _customers.Add(customer);
            var saved = await SaveAsync(ct);
            if (!saved.IsSuccess)
            {
                return Result<CustomerDto>.FromFailure(saved);
            }

            return Result<CustomerDto>.Success(_mapper.Map<CustomerDto>(customer));
        }

        public async Task<Result> UpdateAsync(int id, CustomerRequestDto dto, CancellationToken ct)
        {
            var customer = await _customers.GetForUpdateAsync(id, ct);
            if (customer == null)
            {
                return Result.NotFound($"לקוח {id} לא נמצא");
            }
            if (await _customers.DriverLicenseExistsAsync(dto.DriverLicenseId, id, ct))
            {
                return Result.Conflict($"כבר קיים לקוח עם רישיון נהיגה {dto.DriverLicenseId}");
            }

            _mapper.Map(dto, customer);
            return await SaveAsync(ct);
        }

        public async Task<Result> DeleteAsync(int id, CancellationToken ct)
        {
            var customer = await _customers.GetForUpdateAsync(id, ct);
            if (customer == null)
            {
                return Result.NotFound($"לקוח {id} לא נמצא");
            }
            if (await _rentals.AnyForCustomerAsync(id, ct))
            {
                return Result.Conflict("לא ניתן למחוק לקוח שיש לו השכרות");
            }

            _customers.Remove(customer);
            return await SaveAsync(ct);
        }

        // שמירה שמתרגמת התנגשות עם משתמש אחר ל-409 במקום 500
        private async Task<Result> SaveAsync(CancellationToken ct)
        {
            try
            {
                await _customers.SaveChangesAsync(ct);
                return Result.Success();
            }
            catch (ConcurrencyConflictException)
            {
                _logger.LogWarning("Customer save conflict: the customer was changed or removed by another user");
                return Result.Conflict("הלקוח שונה או נמחק על ידי משתמש אחר. רענן ונסה שוב.");
            }
            catch (DuplicateKeyException)
            {
                _logger.LogWarning("Customer save conflict: the same driver license was saved concurrently");
                return Result.Conflict("כבר קיים לקוח עם רישיון הנהיגה הזה");
            }
        }
    }
}
