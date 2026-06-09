using System;
using System.Linq;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;
using BuildingBlocks.Exceptions;
using MangaPublishingSystem.Application.Common.Security;
using MangaPublishingSystem.Application.Common.Templates;
using MangaPublishingSystem.Application.DTOs.Auth;
using MangaPublishingSystem.Application.IRepositories;
using MangaPublishingSystem.Application.IServices;
using MangaPublishingSystem.Application.IServices.Auth;
using MangaPublishingSystem.Domain.Entities;
using MangaPublishingSystem.Domain.Enums;

namespace MangaPublishingSystem.Application.Services.Auth
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;
        private readonly IEmailService _emailService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IOtpService _otpService;
        private readonly IRefreshTokenRepository _refreshTokenRepository;

        public AuthService(
            IUserRepository userRepository,
            IRoleRepository roleRepository,
            IPasswordHasher passwordHasher,
            IJwtTokenGenerator jwtTokenGenerator,
            IEmailService emailService,
            IUnitOfWork unitOfWork,
            IOtpService otpService,
            IRefreshTokenRepository refreshTokenRepository)
        {
            _userRepository = userRepository;
            _roleRepository = roleRepository;
            _passwordHasher = passwordHasher;
            _jwtTokenGenerator = jwtTokenGenerator;
            _emailService = emailService;
            _unitOfWork = unitOfWork;
            _otpService = otpService;
            _refreshTokenRepository = refreshTokenRepository;
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
            var refreshTokenString = _jwtTokenGenerator.GenerateRefreshToken();

            var refreshToken = new RefreshToken
            {
                UserId = user.Id,
                Token = refreshTokenString,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                IsRevoked = false
            };

            await _refreshTokenRepository.AddAsync(refreshToken);
            await _unitOfWork.SaveChangesAsync();

            return new AuthResponseDto
            {
                UserId = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                FullName = user.FullName,
                RoleName = user.Role.RoleName,
                Token = token,
                RefreshToken = refreshTokenString
            };
        }

        public async Task<RegisterResponseDto> RegisterAssistantAsync(RegisterDto registerDto)
        {
            // 1. Kiểm tra duy nhất (Username / Email)
            var existingUser = await _userRepository.GetUserWithRoleByUsernameOrEmailAsync(registerDto.UserName);
            if (existingUser != null)
            {
                throw new ConflictException("Tên đăng nhập đã tồn tại trên hệ thống.");
            }

            var existingEmail = await _userRepository.GetUserWithRoleByUsernameOrEmailAsync(registerDto.Email);
            if (existingEmail != null)
            {
                throw new ConflictException("Email đã tồn tại trên hệ thống.");
            }

            var roles = await _roleRepository.FindAsync(r => r.RoleName == "Assistant");
            var assistantRole = roles.FirstOrDefault();
            if (assistantRole == null)
            {
                throw new NotFoundException("Vai trò Trợ lý không tồn tại trên hệ thống.");
            }

            // Nhánh A: Chưa nhập mã OTP -> Tạo OTP và gửi vào email
            if (string.IsNullOrEmpty(registerDto.VerificationCode))
            {
                await _otpService.SendOtpAsync(registerDto.Email);
                return new RegisterResponseDto
                {
                    RequiresVerification = true,
                    Message = "Mã xác thực OTP đã được gửi tới email của bạn. Vui lòng nhập mã để hoàn tất đăng ký."
                };
            }

            // Nhánh B: Đã nhập mã OTP -> So khớp OTP và hoàn tất đăng ký
            var isOtpValid = _otpService.VerifyOtp(registerDto.Email, registerDto.VerificationCode);
            if (!isOtpValid)
            {
                throw new BadRequestException("Mã xác thực email không chính xác hoặc đã hết hạn.");
            }

            var user = new User
            {
                RoleId = assistantRole.Id,
                UserName = registerDto.UserName,
                PasswordHash = _passwordHasher.HashPassword(registerDto.Password),
                Email = registerDto.Email,
                FullName = registerDto.FullName,
                Status = UserStatus.Pending,
                PortfolioUrl = registerDto.PortfolioUrl,
                Skills = registerDto.Skills,
                Wallet = new Wallet
                {
                    SetupFundBalance = 0.00m,
                    WithdrawableBalance = 0.00m,
                    LockedFund = 0.00m,
                    LockedWithdrawable = 0.00m
                },
                AssistantProfile = new AssistantProfile
                {
                    SpecialtyTags = registerDto.Skills,
                    TotalCompletedTasks = 0,
                    OnTimeRate = 0.00m,
                    DisputeRate = 0.00m,
                    CurrentActiveTasks = 0,
                    AverageRating = 0.00m
                }
            };

            await _userRepository.AddAsync(user);
            await _unitOfWork.SaveChangesAsync();

            try
            {
                var emailSubject = "Đăng ký tài khoản Trợ lý thành công - Chờ phê duyệt";
                var emailBody = EmailTemplates.GetRegisterSuccessBody(
                    user.FullName,
                    user.UserName,
                    user.Email,
                    user.PortfolioUrl);

                await _emailService.SendEmailAsync(user.Email, emailSubject, emailBody);
            }
            catch (Exception)
            {
                // Chỉ ghi nhận cảnh báo gửi mail lỗi, không hủy giao dịch lưu DB vì tài khoản đã tạo hợp lệ
            }

            return new RegisterResponseDto
            {
                RequiresVerification = false,
                Message = "Đăng ký tài khoản trợ lý thành công. Vui lòng chờ phê duyệt từ quản trị viên."
            };
        }

        public async Task ChangePasswordAsync(int userId, ChangePasswordDto changePasswordDto)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                throw new NotFoundException("Không tìm thấy người dùng.");
            }

            var isPasswordValid = _passwordHasher.VerifyPassword(changePasswordDto.CurrentPassword, user.PasswordHash);
            if (!isPasswordValid)
            {
                throw new UnauthorizedException("Mật khẩu hiện tại không chính xác.");
            }

            user.PasswordHash = _passwordHasher.HashPassword(changePasswordDto.NewPassword);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task ForgotPasswordRequestAsync(ForgotPasswordRequestDto requestDto)
        {
            var users = await _userRepository.FindAsync(u => u.Email == requestDto.Email);
            var user = users.FirstOrDefault();
            if (user == null)
            {
                throw new NotFoundException("Không tìm thấy người dùng với email đã cung cấp.");
            }

            await _otpService.SendForgotPasswordOtpAsync(requestDto.Email);
        }

        public async Task ForgotPasswordResetAsync(ForgotPasswordResetDto resetDto)
        {
            var isOtpValid = _otpService.VerifyOtp(resetDto.Email, resetDto.VerificationCode);
            if (!isOtpValid)
            {
                throw new BadRequestException("Mã xác thực OTP không chính xác hoặc đã hết hạn.");
            }

            var users = await _userRepository.FindAsync(u => u.Email == resetDto.Email);
            var user = users.FirstOrDefault();
            if (user == null)
            {
                throw new NotFoundException("Không tìm thấy người dùng với email đã cung cấp.");
            }

            user.PasswordHash = _passwordHasher.HashPassword(resetDto.NewPassword);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenDto tokenDto)
        {
            var principal = _jwtTokenGenerator.GetPrincipalFromExpiredToken(tokenDto.AccessToken);
            if (principal == null)
            {
                throw new UnauthorizedException("Access token không hợp lệ hoặc không thể phân tích.");
            }

            var userName = principal.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value;
            if (string.IsNullOrEmpty(userName))
            {
                throw new UnauthorizedException("Access token không chứa thông tin người dùng hợp lệ.");
            }

            var user = await _userRepository.GetUserWithRoleByUsernameOrEmailAsync(userName);
            if (user == null)
            {
                throw new UnauthorizedException("Không tìm thấy người dùng tương ứng với token.");
            }

            if (user.Status != UserStatus.Active)
            {
                throw new UnauthorizedException("Tài khoản người dùng đã bị khóa hoặc chưa được kích hoạt.");
            }

            var savedRefreshTokens = await _refreshTokenRepository.FindAsync(t => 
                t.Token == tokenDto.RefreshToken && 
                t.UserId == user.Id);
            var savedRefreshToken = savedRefreshTokens.FirstOrDefault();

            if (savedRefreshToken == null)
            {
                throw new UnauthorizedException("Refresh token không tồn tại hoặc không khớp với người dùng.");
            }

            if (savedRefreshToken.IsRevoked)
            {
                throw new UnauthorizedException("Refresh token đã bị thu hồi.");
            }

            if (savedRefreshToken.ExpiresAt < DateTime.UtcNow)
            {
                throw new UnauthorizedException("Refresh token đã hết hạn.");
            }

            // Revoke the old refresh token
            savedRefreshToken.IsRevoked = true;
            _refreshTokenRepository.Update(savedRefreshToken);

            // Generate new pair of tokens (Token Rotation)
            var newAccessToken = _jwtTokenGenerator.GenerateToken(user);
            var newRefreshTokenString = _jwtTokenGenerator.GenerateRefreshToken();

            var newRefreshToken = new RefreshToken
            {
                UserId = user.Id,
                Token = newRefreshTokenString,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                IsRevoked = false
            };

            await _refreshTokenRepository.AddAsync(newRefreshToken);
            await _unitOfWork.SaveChangesAsync();

            return new AuthResponseDto
            {
                UserId = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                FullName = user.FullName,
                RoleName = user.Role.RoleName,
                Token = newAccessToken,
                RefreshToken = newRefreshTokenString
            };
        }

        public async Task LogoutAsync(LogoutDto logoutDto)
        {
            var savedRefreshTokens = await _refreshTokenRepository.FindAsync(t => t.Token == logoutDto.RefreshToken);
            var savedRefreshToken = savedRefreshTokens.FirstOrDefault();
            if (savedRefreshToken != null)
            {
                savedRefreshToken.IsRevoked = true;
                _refreshTokenRepository.Update(savedRefreshToken);
                await _unitOfWork.SaveChangesAsync();
            }
        }
    }
}
