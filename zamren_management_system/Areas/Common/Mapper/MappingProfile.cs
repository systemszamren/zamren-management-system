using AutoMapper;
using zamren_management_system.Areas.Security.Dto;
using zamren_management_system.Areas.Security.Models;

namespace zamren_management_system.Areas.Common.Mapper;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Branch, BranchDto>();
        CreateMap<BranchDto, Branch>();
        
        CreateMap<Organization, OrganizationDto>();
        CreateMap<OrganizationDto, Organization>();
    }
}