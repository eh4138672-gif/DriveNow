using AutoMapper;
using MyProject.Core.DTOs;
using MyProject.Core.Entities;

namespace MyProject.Services
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Feature, FeatureDto>();

            CreateMap<Car, CarDto>();
            // התכונות משויכות ב-Service לפי FeatureIds
            CreateMap<CarRequestDto, Car>()
                .ForMember(c => c.Features, o => o.Ignore());
            // ה-Version מהלקוח נקבע כערך המקורי דרך ה-Repository, לא נדרס כערך חדש
            CreateMap<CarUpdateDto, Car>()
                .ForMember(c => c.Features, o => o.Ignore())
                .ForMember(c => c.Version, o => o.Ignore());

            CreateMap<Customer, CustomerDto>();
            CreateMap<CustomerRequestDto, Customer>();

            // CarModel, CarLicenseNumber ו-CustomerName ממופים אוטומטית (flattening)
            CreateMap<Rental, RentalDto>()
                .ForMember(d => d.CarFeatures, o => o.MapFrom(r => r.Car.Features.Select(f => f.Name)))
                .ForMember(d => d.TotalPrice, o => o.MapFrom(r => RentalPricing.TotalPrice(r.Car.PricePerHour, r.DateStart, r.DateEnd)));
            CreateMap<RentalRequestDto, Rental>();
        }
    }
}
