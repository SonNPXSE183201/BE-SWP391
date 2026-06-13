namespace MangaPublishingSystem.Application.Common.Templates
{
    public static class EmailTemplates
    {
        public static string GetRegisterSuccessBody(string fullName, string username, string email, string? portfolioUrl)
        {
            var portfolioDisplay = string.IsNullOrEmpty(portfolioUrl)
                ? "<span style=\"color: #94a3b8; font-style: italic;\">Chưa cung cấp</span>"
                : $@"<a href=""{portfolioUrl}"" style=""color: #3b82f6; text-decoration: none; font-weight: 500; word-break: break-all;"" target=""_blank"">{portfolioUrl}</a>";

            return $@"
                <div style=""font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; background-color: #f4f6f9; padding: 40px 20px; color: #333333; line-height: 1.6;"">
                    <div style=""max-width: 600px; margin: 0 auto; background: #ffffff; border-radius: 12px; box-shadow: 0 8px 24px rgba(0, 0, 0, 0.05); overflow: hidden;"">
                        <!-- Header -->
                        <div style=""background: linear-gradient(135deg, #4f46e5, #3b82f6); padding: 30px; text-align: center; color: #ffffff;"">
                            <h2 style=""margin: 0; font-size: 24px; font-weight: 600; letter-spacing: 0.5px;"">Manga Publishing System</h2>
                            <p style=""margin: 5px 0 0 0; font-size: 14px; opacity: 0.9;"">Hệ thống Quản lý Sáng tác & Xuất bản</p>
                        </div>
                        
                        <!-- Content -->
                        <div style=""padding: 40px 30px;"">
                            <h3 style=""margin-top: 0; color: #1e293b; font-size: 20px; font-weight: 600;"">Đăng ký tài khoản thành công!</h3>
                            <p style=""font-size: 15px; color: #475569;"">Chào <strong>{fullName}</strong>,</p>
                            <p style=""font-size: 15px; color: #475569;"">Hồ sơ ứng tuyển vị trí <strong>Trợ lý vẽ tranh (Assistant)</strong> của bạn đã được ghi nhận thành công trên hệ thống của chúng tôi.</p>
                            
                            <!-- Box Info -->
                            <div style=""background-color: #f8fafc; border-left: 4px solid #3b82f6; padding: 20px; border-radius: 0 8px 8px 0; margin: 25px 0;"">
                                <h4 style=""margin: 0 0 10px 0; color: #1e293b; font-size: 15px; font-weight: 600;"">Thông tin đăng ký của bạn:</h4>
                                <table style=""width: 100%; font-size: 14px; border-collapse: collapse;"">
                                    <tr>
                                        <td style=""padding: 4px 0; color: #64748b; width: 140px;"">Tên đăng nhập:</td>
                                        <td style=""padding: 4px 0; color: #1e293b; font-weight: 500;"">{username}</td>
                                    </tr>
                                    <tr>
                                        <td style=""padding: 4px 0; color: #64748b;"">Email:</td>
                                        <td style=""padding: 4px 0; color: #1e293b; font-weight: 500;"">{email}</td>
                                    </tr>
                                    <tr>
                                        <td style=""padding: 4px 0; color: #64748b;"">Portfolio:</td>
                                        <td style=""padding: 4px 0;"">{portfolioDisplay}</td>
                                    </tr>
                                </table>
                            </div>
                            
                            <p style=""font-size: 15px; color: #475569;""><strong>Trạng thái tài khoản:</strong> <span style=""background-color: #fef3c7; color: #d97706; padding: 4px 10px; border-radius: 6px; font-size: 13px; font-weight: 600;"">CHỜ DUYỆT (PENDING)</span></p>
                            <p style=""font-size: 15px; color: #475569; margin-top: 20px;"">Ban quản trị (Admin) đang xem xét và thẩm định portfolio của bạn. Bạn sẽ nhận được thông báo kết quả phê duyệt qua email ngay khi quá trình hoàn tất.</p>
                        </div>
                        
                        <!-- Footer -->
                        <div style=""background-color: #f8fafc; padding: 25px; text-align: center; font-size: 12px; color: #94a3b8; border-top: 1px solid #e2e8f0;"">
                            <p style=""margin: 0 0 5px 0;"">Đây là thư thông báo tự động từ hệ thống. Vui lòng không trả lời trực tiếp email này.</p>
                            <p style=""margin: 0;"">&copy; 2026 Manga Publishing System. All rights reserved.</p>
                        </div>
                    </div>
                </div>";
        }

        public static string GetOtpEmailBody(string otpCode)
        {
            return $@"
                <div style=""font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; background-color: #f4f6f9; padding: 40px 20px; color: #333333; line-height: 1.6;"">
                    <div style=""max-width: 600px; margin: 0 auto; background: #ffffff; border-radius: 12px; box-shadow: 0 8px 24px rgba(0, 0, 0, 0.05); overflow: hidden;"">
                        <!-- Header -->
                        <div style=""background: linear-gradient(135deg, #4f46e5, #3b82f6); padding: 30px; text-align: center; color: #ffffff;"">
                            <h2 style=""margin: 0; font-size: 24px; font-weight: 600; letter-spacing: 0.5px;"">Manga Publishing System</h2>
                            <p style=""margin: 5px 0 0 0; font-size: 14px; opacity: 0.9;"">Xác thực Email Đăng ký</p>
                        </div>
                        
                        <!-- Content -->
                        <div style=""padding: 40px 30px; text-align: center;"">
                            <h3 style=""margin-top: 0; color: #1e293b; font-size: 20px; font-weight: 600;"">Mã xác thực đăng ký tài khoản</h3>
                            <p style=""font-size: 15px; color: #475569; text-align: left;"">Chào bạn,</p>
                            <p style=""font-size: 15px; color: #475569; text-align: left;"">Chúng tôi nhận được yêu cầu đăng ký tài khoản Trợ lý vẽ tranh với email này. Vui lòng sử dụng mã OTP dưới đây để hoàn tất quá trình xác thực email:</p>
                            
                            <!-- OTP Display -->
                            <div style=""margin: 30px auto; background-color: #f8fafc; border: 1px dashed #3b82f6; padding: 15px; border-radius: 8px; width: 200px; font-size: 32px; font-weight: bold; letter-spacing: 5px; color: #3b82f6;"">
                                {otpCode}
                            </div>
                            
                            <p style=""font-size: 13px; color: #94a3b8; text-align: left;"">* Mã xác thực này có hiệu lực trong vòng <strong>5 phút</strong> và chỉ sử dụng được 1 lần.</p>
                            <p style=""font-size: 15px; color: #475569; text-align: left; margin-top: 20px;"">Nếu bạn không gửi yêu cầu này, vui lòng bỏ qua email này.</p>
                        </div>
                        
                        <!-- Footer -->
                        <div style=""background-color: #f8fafc; padding: 25px; text-align: center; font-size: 12px; color: #94a3b8; border-top: 1px solid #e2e8f0;"">
                            <p style=""margin: 0 0 5px 0;"">Đây là thư thông báo tự động từ hệ thống. Vui lòng không trả lời trực tiếp email này.</p>
                            <p style=""margin: 0;"">&copy; 2026 Manga Publishing System. All rights reserved.</p>
                        </div>
                    </div>
                </div>";
        }

        public static string GetForgotPasswordOtpEmailBody(string otpCode)
        {
            return $@"
                <div style=""font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; background-color: #f4f6f9; padding: 40px 20px; color: #333333; line-height: 1.6;"">
                    <div style=""max-width: 600px; margin: 0 auto; background: #ffffff; border-radius: 12px; box-shadow: 0 8px 24px rgba(0, 0, 0, 0.05); overflow: hidden;"">
                        <!-- Header -->
                        <div style=""background: linear-gradient(135deg, #4f46e5, #3b82f6); padding: 30px; text-align: center; color: #ffffff;"">
                            <h2 style=""margin: 0; font-size: 24px; font-weight: 600; letter-spacing: 0.5px;"">Manga Publishing System</h2>
                            <p style=""margin: 5px 0 0 0; font-size: 14px; opacity: 0.9;"">Yêu cầu Đặt lại Mật khẩu</p>
                        </div>
                        
                        <!-- Content -->
                        <div style=""padding: 40px 30px; text-align: center;"">
                            <h3 style=""margin-top: 0; color: #1e293b; font-size: 20px; font-weight: 600;"">Mã xác thực đặt lại mật khẩu</h3>
                            <p style=""font-size: 15px; color: #475569; text-align: left;"">Chào bạn,</p>
                            <p style=""font-size: 15px; color: #475569; text-align: left;"">Chúng tôi đã nhận được yêu cầu đặt lại mật khẩu cho tài khoản liên kết với email này. Vui lòng sử dụng mã OTP dưới đây để hoàn tất quá trình xác thực:</p>
                            
                            <!-- OTP Display -->
                            <div style=""margin: 30px auto; background-color: #f8fafc; border: 1px dashed #3b82f6; padding: 15px; border-radius: 8px; width: 200px; font-size: 32px; font-weight: bold; letter-spacing: 5px; color: #3b82f6;"">
                                {otpCode}
                            </div>
                            
                            <p style=""font-size: 13px; color: #94a3b8; text-align: left;"">* Mã xác thực này có hiệu lực trong vòng <strong>5 phút</strong> và chỉ sử dụng được 1 lần.</p>
                            <p style=""font-size: 15px; color: #475569; text-align: left; margin-top: 20px;"">Nếu bạn không gửi yêu cầu này, vui lòng bỏ qua email này để giữ an toàn cho tài khoản của bạn.</p>
                        </div>
                        
                        <!-- Footer -->
                        <div style=""background-color: #f8fafc; padding: 25px; text-align: center; font-size: 12px; color: #94a3b8; border-top: 1px solid #e2e8f0;"">
                            <p style=""margin: 0 0 5px 0;"">Đây là thư thông báo tự động từ hệ thống. Vui lòng không trả lời trực tiếp email này.</p>
                            <p style=""margin: 0;"">&copy; 2026 Manga Publishing System. All rights reserved.</p>
                        </div>
                    </div>
                </div>";
        }
    }
}
