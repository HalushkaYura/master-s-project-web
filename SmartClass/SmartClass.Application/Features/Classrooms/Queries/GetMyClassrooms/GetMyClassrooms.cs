using MediatR;
using SmartClass.Application.Contracts.Classrooms;

namespace SmartClass.Application.Features.Classrooms.Queries.GetMyClassrooms
{
    public sealed class GetMyClassroomsQuery : IRequest<IReadOnlyList<ClassroomDto>>
    {
    }
}
