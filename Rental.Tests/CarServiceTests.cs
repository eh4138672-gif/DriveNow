using AutoMapper;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using MyProject.Core.DTOs;
using MyProject.Core.Entities;
using MyProject.Core.Exceptions;
using MyProject.Core.Interfaces;
using MyProject.Core.Results;
using MyProject.Services;
using Xunit;

namespace Rental.Tests
{
    public class CarServiceTests
    {
        private readonly Mock<ICarRepository> _cars = new();
        private readonly Mock<IRentalRepository> _rentals = new();
        private readonly Mock<IFeatureRepository> _features = new();
        private readonly Mock<ICurrentUser> _currentUser = new();
        private readonly IMapper _mapper = new MapperConfiguration(c => c.AddProfile<MappingProfile>()).CreateMapper();
        private readonly Car _car = new() { Id = 1, Model = "Mazda 3", LicenseNumber = "123-45-678", PricePerHour = 50 };

        public CarServiceTests()
        {
            _cars.Setup(r => r.GetForUpdateAsync(_car.Id, It.IsAny<CancellationToken>())).ReturnsAsync(_car);
            _cars.Setup(r => r.GetWithFeaturesForUpdateAsync(_car.Id, It.IsAny<CancellationToken>())).ReturnsAsync(_car);
        }

        private CarService CreateService() =>
            new(_cars.Object, _rentals.Object, _features.Object, _currentUser.Object, _mapper, NullLogger<CarService>.Instance);

        [Fact]
        public async Task GetAll_NonAdminAsksForInactiveCars_GetsOnlyActive()
        {
            _cars.Setup(r => r.GetPagedAsync(It.IsAny<CarQueryParameters>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PagedResult<Car>(new List<Car>(), 1, 10, 0));

            await CreateService().GetAllAsync(new CarQueryParameters { IncludeInactive = true }, default);

            _cars.Verify(r => r.GetPagedAsync(It.Is<CarQueryParameters>(q => !q.IncludeInactive), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task SetAvailability_TakeOutOfRental_SavesInactiveCar()
        {
            var result = await CreateService().SetAvailabilityAsync(_car.Id, false, default);

            Assert.True(result.IsSuccess);
            Assert.False(_car.IsActive);
            _cars.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetById_InactiveCarForCustomer_ReturnsNotFound()
        {
            _cars.Setup(r => r.GetByIdAsync(_car.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Car { Id = _car.Id, Model = "Mazda 3", IsActive = false });

            var result = await CreateService().GetByIdAsync(_car.Id, default);

            Assert.Equal(ResultStatus.NotFound, result.Status);
        }

        [Fact]
        public async Task Create_UnknownFeature_ReturnsInvalid()
        {
            _features.Setup(r => r.GetByIdsForUpdateAsync(It.IsAny<IReadOnlyCollection<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Feature> { new() { Id = 1, Name = "GPS" } });

            var result = await CreateService().CreateAsync(
                new CarRequestDto { LicenseNumber = "111-22-333", Model = "Kia", PricePerHour = 40, FeatureIds = new() { 1, 99 } }, default);

            Assert.Equal(ResultStatus.Invalid, result.Status);
            _cars.Verify(r => r.Add(It.IsAny<Car>()), Times.Never);
        }

        [Fact]
        public async Task Create_DuplicateLicenseNumber_ReturnsConflict()
        {
            _cars.Setup(r => r.LicenseNumberExistsAsync("123-45-678", null, It.IsAny<CancellationToken>())).ReturnsAsync(true);

            var result = await CreateService().CreateAsync(
                new CarRequestDto { LicenseNumber = "123-45-678", Model = "Kia", PricePerHour = 40 }, default);

            Assert.Equal(ResultStatus.Conflict, result.Status);
            _cars.Verify(r => r.Add(It.IsAny<Car>()), Times.Never);
        }

        [Fact]
        public async Task Update_StaleRowVersion_ReturnsConflict()
        {
            _cars.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new ConcurrencyConflictException("conflict", new Exception()));
            var staleVersion = new byte[] { 1, 2, 3 };

            var result = await CreateService().UpdateAsync(_car.Id,
                new CarUpdateDto { LicenseNumber = "123-45-678", Model = "Mazda 3", PricePerHour = 60, RowVersion = staleVersion }, default);

            Assert.Equal(ResultStatus.Conflict, result.Status);
            _cars.Verify(r => r.SetOriginalRowVersion(_car, staleVersion), Times.Once);
        }

        [Fact]
        public async Task Delete_CarWithRentals_ReturnsConflict()
        {
            _rentals.Setup(r => r.AnyForCarAsync(_car.Id, It.IsAny<CancellationToken>())).ReturnsAsync(true);

            var result = await CreateService().DeleteAsync(_car.Id, default);

            Assert.Equal(ResultStatus.Conflict, result.Status);
            _cars.Verify(r => r.Remove(It.IsAny<Car>()), Times.Never);
        }
    }
}
