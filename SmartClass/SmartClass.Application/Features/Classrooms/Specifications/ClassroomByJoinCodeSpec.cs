using Ardalis.Specification;
using SmartClass.Domain.Entities;

namespace SmartClass.Application.Features.Classrooms.Specifications;

public sealed class ClassroomByJoinCodeSpec : Specification<Classroom>
{
    public ClassroomByJoinCodeSpec(string joinCode)
    {
        Query.Where(c => c.JoinCode == joinCode);
    }
}
