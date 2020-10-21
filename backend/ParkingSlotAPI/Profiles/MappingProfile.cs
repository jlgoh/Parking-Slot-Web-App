using AutoMapper;
using ParkingSlotAPI.Entities;
using ParkingSlotAPI.Models;
using ParkingSlotAPI.PublicAPIEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ParkingSlotAPI.Profiles
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Carpark, CarparkDto>();
            CreateMap<Carpark, CarparkMarkerDto>();
            CreateMap<User, UserDto>();
            CreateMap<UserForCreationDto, User>()
                .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.isAdmin ? Role.Admin : Role.User));
            CreateMap<UserForUpdateDto, User>();
            CreateMap<UserForUpdatePasswordDto, User>();
            CreateMap<FavoriteForCreationDto, Favorite>();
            CreateMap<Favorite, FavoriteDto>();
            CreateMap<FeedbackForCreationDto, Feedback>();
            CreateMap<Feedback, FeedbackDto>();
            CreateMap<FeedbackForUpdateDto, Feedback>();
            CreateMap<UserForUpdateForgetPasswordDto, User>();
        }
    }
}
