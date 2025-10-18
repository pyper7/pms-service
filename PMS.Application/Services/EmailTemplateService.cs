using Microsoft.Extensions.Configuration;
using PMS.Application.Interfaces;
using PMS.Application.Models.Notifications;
using PMS.Domain.Entities;

namespace PMS.Application.Services;

public class EmailTemplateService : IEmailTemplateService
{
    private readonly IConfiguration _configuration;

    public EmailTemplateService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<EmailMessage> CreateOfficerWelcomeEmailAsync(User officer, string onboardingUrl)
    {
        var placeholders = new Dictionary<string, string>
        {
            { "OFFICER_NAME", $"{officer.FirstName} {officer.LastName}" },
            { "OFFICER_EMAIL", officer.Email },
            { "STAFF_ID", officer.StaffId ?? "" },
            { "DEPARTMENT", officer.Department ?? "" },
            { "POSITION", officer.Position ?? "" },
            { "ONBOARDING_URL", "http://localhost:3000/signup" },
            { "TETFUND_LOGO_URL", _configuration["EmailSettings:TetfundLogoUrl"] ?? "https://tetfund.gov.ng/images/logo.png" },
            { "SUPPORT_URL", _configuration["EmailSettings:SupportUrl"] ?? "https://tetfund.gov.ng/support" },
            { "HELP_URL", _configuration["EmailSettings:HelpUrl"] ?? "https://tetfund.gov.ng/help" },
            { "PRIVACY_URL", _configuration["EmailSettings:PrivacyUrl"] ?? "https://tetfund.gov.ng/privacy" }
        };

        var template = GetOfficerWelcomeTemplate();
        var processedBody = ProcessEmailTemplate(template, placeholders);

        return new EmailMessage
        {
            To = officer.Email,
            Subject = "Welcome to TETFund Performance Management System - Account Created",
            Body = processedBody,
            IsHtml = true
        };
    }

    public string ProcessEmailTemplate(string template, Dictionary<string, string> placeholders)
    {
        var processedTemplate = template;

        foreach (var placeholder in placeholders)
        {
            processedTemplate = processedTemplate.Replace($"{{{{{placeholder.Key}}}}}", placeholder.Value);
        }

        return processedTemplate;
    }

    private string GetOfficerWelcomeTemplate()
    {
        return @"<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Welcome to TETFund PMS</title>
    <style>
        body {
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            line-height: 1.6;
            color: #333;
            max-width: 600px;
            margin: 0 auto;
            padding: 20px;
            background-color: #f4f4f4;
        }
        .container {
            background-color: #ffffff;
            padding: 25px;
            border-radius: 8px;
            box-shadow: 0 0 15px rgba(0,0,0,0.1);
        }
        .header {
            text-align: center;
            border-bottom: 2px solid #059669;
            padding-bottom: 15px;
            margin-bottom: 20px;
        }
        .logo {
            max-width: 120px;
            height: auto;
            margin-bottom: 10px;
        }
        .welcome-title {
            color: #059669;
            font-size: 20px;
            font-weight: bold;
            margin: 0;
        }
        .subtitle {
            color: #6b7280;
            font-size: 12px;
            margin: 5px 0 0 0;
        }
        .content {
            margin-bottom: 15px;
        }
        .greeting {
            font-size: 14px;
            color: #059669;
            margin-bottom: 10px;
        }
        .account-info {
            background-color: #f0fdf4;
            padding: 12px;
            border-radius: 6px;
            border-left: 3px solid #059669;
            margin: 12px 0;
        }
        .info-row {
            display: flex;
            margin-bottom: 6px;
            align-items: center;
        }
        .info-label {
            font-weight: bold;
            color: #374151;
            min-width: 70px;
            margin-right: 8px;
            font-size: 12px;
        }
        .info-value {
            color: #6b7280;
            flex: 1;
            font-size: 12px;
        }
        .cta-section {
            text-align: center;
            margin: 20px 0;
            padding: 15px;
            background: linear-gradient(135deg, #059669 0%, #10b981 100%);
            border-radius: 8px;
            color: white;
        }
        .cta-title {
            font-size: 16px;
            font-weight: bold;
            margin-bottom: 8px;
        }
        .cta-description {
            font-size: 12px;
            margin-bottom: 12px;
            opacity: 0.9;
        }
        .cta-button {
            display: inline-block;
            background-color: #ffffff;
            color: #059669;
            padding: 10px 20px;
            text-decoration: none;
            border-radius: 20px;
            font-weight: bold;
            font-size: 12px;
            transition: all 0.3s ease;
            box-shadow: 0 2px 10px rgba(0,0,0,0.2);
        }
        .cta-button:hover {
            transform: translateY(-1px);
            box-shadow: 0 4px 15px rgba(0,0,0,0.3);
        }
        .support-section {
            background-color: #f0fdf4;
            padding: 12px;
            border-radius: 6px;
            margin: 15px 0;
            text-align: center;
        }
        .support-title {
            color: #1e3a8a;
            font-size: 14px;
            font-weight: bold;
            margin-bottom: 8px;
        }
        .support-links {
            display: flex;
            justify-content: center;
            gap: 10px;
            flex-wrap: wrap;
        }
        .support-link {
            color: #3b82f6;
            text-decoration: none;
            font-weight: 500;
            padding: 4px 8px;
            border: 1px solid #3b82f6;
            border-radius: 12px;
            font-size: 10px;
            transition: all 0.3s ease;
        }
        .support-link:hover {
            background-color: #3b82f6;
            color: white;
        }
        .footer {
            text-align: center;
            margin-top: 20px;
            padding-top: 12px;
            border-top: 1px solid #e5e7eb;
            color: #6b7280;
            font-size: 10px;
        }
        .footer-links {
            margin: 8px 0;
        }
        .footer-link {
            color: #3b82f6;
            text-decoration: none;
            margin: 0 6px;
            font-size: 10px;
        }
        .footer-link:hover {
            text-decoration: underline;
        }
        .copyright {
            margin-top: 8px;
            font-size: 9px;
        }
        @media (max-width: 600px) {
            body {
                padding: 8px;
            }
            .container {
                padding: 15px;
            }
            .welcome-title {
                font-size: 18px;
            }
            .cta-button {
                padding: 8px 16px;
                font-size: 11px;
            }
            .support-links {
                flex-direction: column;
                align-items: center;
            }
            .info-row {
                flex-direction: column;
                align-items: flex-start;
            }
            .info-label {
                margin-bottom: 2px;
            }
        }
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <img src=""{TETFUND_LOGO_URL}"" alt=""TETFund Logo"" class=""logo"">
            <h1 class=""welcome-title"">Welcome to TETFund PMS</h1>
            <p class=""subtitle"">Performance Management System</p>
        </div>

        <div class=""content"">
            <div class=""greeting"">
                Hello {OFFICER_NAME},
            </div>

            <p>Your account has been created successfully!</p>

            <div class=""account-info"">
                <h3 style=""color: #1e3a8a; margin-top: 0; margin-bottom: 8px; font-size: 14px;"">Account Details</h3>
                <div class=""info-row"">
                    <span class=""info-label"">Name:</span>
                    <span class=""info-value"">{OFFICER_NAME}</span>
                </div>
                <div class=""info-row"">
                    <span class=""info-label"">Email:</span>
                    <span class=""info-value"">{OFFICER_EMAIL}</span>
                </div>
                <div class=""info-row"">
                    <span class=""info-label"">Staff ID:</span>
                    <span class=""info-value"">{STAFF_ID}</span>
                </div>
            </div>

            <div class=""cta-section"">
                <div class=""cta-title"">Complete Setup</div>
                <div class=""cta-description"">
                    Click below to set up your password and start using the system.
                </div>
                <a href=""{ONBOARDING_URL}"" class=""cta-button"">Start Setup</a>
            </div>

            <div class=""support-section"">
                <div class=""support-title"">Need Help?</div>
                <p style=""margin-bottom: 8px; color: #6b7280; font-size: 12px;"">Contact us if you need assistance</p>
                <div class=""support-links"">
                    <a href=""{SUPPORT_URL}"" class=""support-link"">Support</a>
                    <a href=""{HELP_URL}"" class=""support-link"">Help</a>
                </div>
            </div>
        </div>

        <div class=""footer"">
            <div class=""footer-links"">
                <a href=""{SUPPORT_URL}"" class=""footer-link"">Support</a>
                <a href=""{HELP_URL}"" class=""footer-link"">Help</a>
                <a href=""{PRIVACY_URL}"" class=""footer-link"">Privacy</a>
            </div>
            <div class=""copyright"">
                © 2024 TETFund. All rights reserved.<br>
                This is an automated message. Please do not reply to this email.
            </div>
        </div>
    </div>
</body>
</html>";
    }
}
