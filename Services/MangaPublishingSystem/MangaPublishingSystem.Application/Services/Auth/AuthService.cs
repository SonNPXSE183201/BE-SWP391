using System.Threading.Tasks;
using BuildingBlocks.Exceptions;
using MangaPublishingSystem.Application.Common.Security;
using MangaPublishingSystem.Application.DTOs.Auth;
using MangaPublishingSystem.Application.IRepositories;
using MangaPublishingSystem.Application.IServices.Auth;
using MangaPublishingSystem.Domain.Enums;

namespace MangaPublishingSystem.Application.Services.Auth
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;

        public AuthService(
            IUserRepository userRepository,
            IPasswordHasher passwordHasher,
            IJwtTokenGenerator jwtTokenGenerator)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _jwtTokenGenerator = jwtTokenGenerator;
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto loginDto)
        {
            var user = await _userRepository.GetUserWithRoleByUsernameOrEmailAsync(loginDto.Identifier);

            if (user == null)
            {
                throw new UnauthorizedException("Tên đăng nhập hoặc mật khẩu không chính xác.");
            }

            // Verify account status (must be Active as per SRS)
            if (user.Status != UserStatus.Active)
            {
                if (user.Status == UserStatus.Pending)
                {
                    throw new ForbiddenException("Tài khoản của bạn đang chờ duyệt. Vui lòng quay lại sau.");
                }
                if (user.Status == UserStatus.Rejected)
                {
                    throw new ForbiddenException("Tài khoản của bạn đã bị từ chối phê duyệt.");
                }
                if (user.Status == UserStatus.Locked)
                {
                    throw new ForbiddenException("Tài khoản của bạn đã bị khóa. Vui lòng liên hệ với quản trị viên.");
                }
                throw new ForbiddenException("Tài khoản chưa được kích hoạt hoặc đã bị khóa.");
            }

            // Verify password
            var isPasswordValid = _passwordHasher.VerifyPassword(loginDto.Password, user.PasswordHash);
            if (!isPasswordValid)
            {
                throw new UnauthorizedException("Tên đăng nhập hoặc mật khẩu không chính xác.");
            }

            // Generate JWT Token
            var token = _jwtTokenGenerator.GenerateToken(user);

            return new AuthResponseDto
            {
                UserId = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                FullName = user.FullName,
                RoleName = user.Role.RoleName,
                Token = token
            };
        }
    }
}
