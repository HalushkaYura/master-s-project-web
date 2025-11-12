using MediatR;
using SmartClass.Application.Contracts.Submissions;

namespace SmartClass.Application.Features.Submissions.Queries.GetMine;

public sealed class GetMySubmissionsQuery : IRequest<IReadOnlyList<MySubmissionDto>>
{
}
