using AutoMapper;
using SmartClass.Application.Contracts.Auth;
using SmartClass.Application.Contracts.User;
using SmartClass.Infrastructure.Identity.Entities;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SmartClass.Infrastructure.Mapping
{
    public class ApplicationProfile : Profile
    {
        public ApplicationProfile()
        {
            CreateMap<ApplicationUser, UserInfoDto>()
                .ForMember(d => d.Email, opt => opt.MapFrom(s => s.Email))
                .ForMember(d => d.Firstname, opt => opt.MapFrom(s => s.Firstname))
                .ForMember(d => d.Lastname, opt => opt.MapFrom(s => s.Lastname))
                .ForMember(d => d.BirthDate, opt => opt.MapFrom(s => s.BirthDate))
                .ForMember(d => d.Roles, opt => opt.Ignore());
            // Roles заповнимо окремо через UserManager.GetRolesAsync
        }
    }
}
