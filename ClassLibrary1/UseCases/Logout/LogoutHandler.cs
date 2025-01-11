using MediatR;
using Project.Application.HadlerResponce;
using Project.Application.UseCases.Create;
using Project.Domain.Entities;
using Project.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.Application.UseCases.Logout
{
    public class LogoutHandler : IRequestHandler<LogoutRequest, Response>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRefreshRepository _refreshRepository;

        public LogoutHandler(IUnitOfWork unitOfWork, IRefreshRepository refreshRepository)
        {
            _unitOfWork = unitOfWork;
            _refreshRepository = refreshRepository;
        }

        public async Task<Response> Handle(LogoutRequest request, CancellationToken cancellationToken)
        {
            RefreshSession? refreshSession;
            try
            {
                refreshSession = await _refreshRepository.GetByUserIdAsync(request.UserId, cancellationToken);
                if (refreshSession is null)
                    return new Response("Refresh session not found", 404);

            }
            catch
            {
                return new Response("Internal Server Error", 500);
            }
            _refreshRepository.Delete(refreshSession);
            await _unitOfWork.Commit(cancellationToken);
            return new Response("Logout success", 200);

        }
    }
}
