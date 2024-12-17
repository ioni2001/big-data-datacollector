using AutoMapper;
using Big.Data.DataCollection.Models.DbEntities;
using Big.Data.DataCollection.Models.Events;

namespace Big.Data.DataCollection.Profiles;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<SocialMediaCommentEvent, SocialMediaComment>();
    }
}
