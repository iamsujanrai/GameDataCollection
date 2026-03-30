using System.Net.Mail;
using System.Net;
using GameDataCollection.Models;
using System.Text;
using GameDataCollection.Services;

namespace GameDataCollection.Extension
{
    public class EmailScheduler : IHostedService, IDisposable
    {
        private static readonly TimeZoneInfo NepalTz =
            TimeZoneInfo.FindSystemTimeZoneById("Nepal Standard Time");

        private readonly IServiceScopeFactory _serviceProvider;
        private Timer _timer;
        private DateTime? _lastSentDate; // Nepal date of last successful send

        public EmailScheduler(IServiceScopeFactory serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            // Poll every 60 seconds
            _timer = new Timer(CheckAndSend, null, TimeSpan.Zero, TimeSpan.FromMinutes(1));
            return Task.CompletedTask;
        }

        private void CheckAndSend(object state)
        {
            try
            {
                DateTime nowNepal = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, NepalTz);
                DateTime todayNepal = nowNepal.Date;

                // Only fire at or after 14:00, and only once per calendar day
                if (nowNepal.Hour < 14)
                    return;

                if (_lastSentDate.HasValue && _lastSentDate.Value == todayNepal)
                    return;

                using var scope = _serviceProvider.CreateScope();
                var emailSetupService = scope.ServiceProvider.GetRequiredService<IEmailSetupService>();
                var gameRecordService = scope.ServiceProvider.GetRequiredService<IGameRecordService>();
                SendDailyEmail(emailSetupService, gameRecordService);

                _lastSentDate = todayNepal;
            }
            catch (Exception ex)
            {
                EmailSender.EmailSend("rojinbastola@gmail.com", "Error in EmailScheduler", ex.Message);
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
        private string GetEmailBody(IEnumerable<GameRecord> gameRecords)
        {
            var sb = new StringBuilder();
            sb.Append(@"
    <div class='card-body' style='padding: 20px; background-color: #f9f9f9;'>
        <table id='nonexpiredtbl' style='width: 100%; border-collapse: collapse; margin-bottom: 1rem; background-color: #fff;'>
            <thead>
                <tr>
                    <th style='background-color: #007bff; color: white; text-align: center; padding: 10px;'>Full Name</th>
                    <th style='background-color: #007bff; color: white; text-align: center; padding: 10px;'>Phone Number</th>
                    <th style='background-color: #007bff; color: white; text-align: center; padding: 10px;'>State</th>
                    <th style='background-color: #007bff; color: white; text-align: center; padding: 10px;'>Referred By</th>
                    <th style='background-color: #007bff; color: white; text-align: center; padding: 10px;'>Email</th>
                    <th style='background-color: #007bff; color: white; text-align: center; padding: 10px;'>Facebook Name</th>
                    <th style='background-color: #007bff; color: white; text-align: center; padding: 10px;'>Game</th>
                    <th style='background-color: #007bff; color: white; text-align: center; padding: 10px;'>Game Id</th>
                    <th style='background-color: #007bff; color: white; text-align: center; padding: 10px;'>Entry Date</th>
                    <th style='background-color: #007bff; color: white; text-align: center; padding: 10px;'>Expiry Date</th>
                </tr>
            </thead>
            <tbody>");

            foreach (var item in gameRecords)
            {
                sb.Append($@"
            <tr>
                <td style='padding: 10px; border: 1px solid #dee2e6; text-align: center;'>{item.FullName}</td>
                <td style='padding: 10px; border: 1px solid #dee2e6; text-align: center;'>{item.PhoneNumber}</td>
                <td style='padding: 10px; border: 1px solid #dee2e6; text-align: center;'>{item.State.Name}</td>
                <td style='padding: 10px; border: 1px solid #dee2e6; text-align: center;'>{item.RefferedBy}</td>
                <td style='padding: 10px; border: 1px solid #dee2e6; text-align: center;'>{item.Email}</td>
                <td style='padding: 10px; border: 1px solid #dee2e6; text-align: center;'>{item.FacebookName}</td>
                <td style='padding: 10px; border: 1px solid #dee2e6; text-align: center;'>{item.Game.Name}</td>
                <td style='padding: 10px; border: 1px solid #dee2e6; text-align: center;'>{item.GameUserId}</td>
                <td style='padding: 10px; border: 1px solid #dee2e6; text-align: center;'>{item.CreatedDateTime:yyyy-MM-dd}</td>
                <td style='padding: 10px; border: 1px solid #dee2e6; text-align: center;'>{item.ExpiryDateTime:yyyy-MM-dd}</td>
            </tr>");
            }

            sb.Append(@"
            </tbody>
            <tfoot>
                <tr>
                    <th style='background-color: #f1f1f1; color: #333; padding: 10px; text-align: center;'>Full Name</th>
                    <th style='background-color: #f1f1f1; color: #333; padding: 10px; text-align: center;'>Phone Number</th>
                    <th style='background-color: #f1f1f1; color: #333; padding: 10px; text-align: center;'>State</th>
                    <th style='background-color: #f1f1f1; color: #333; padding: 10px; text-align: center;'>Referred By</th>
                    <th style='background-color: #f1f1f1; color: #333; padding: 10px; text-align: center;'>Email</th>
                    <th style='background-color: #f1f1f1; color: #333; padding: 10px; text-align: center;'>Facebook Name</th>
                    <th style='background-color: #f1f1f1; color: #333; padding: 10px; text-align: center;'>Game</th>
                    <th style='background-color: #f1f1f1; color: #333; padding: 10px; text-align: center;'>Game Id</th>
                    <th style='background-color: #f1f1f1; color: #333; padding: 10px; text-align: center;'>Entry Date</th>
                    <th style='background-color: #f1f1f1; color: #333; padding: 10px; text-align: center;'>Expiry Date</th>
                </tr>
            </tfoot>
        </table>
    </div>");

            return sb.ToString();
        }


        public void SendDailyEmail(IEmailSetupService _emailSetupService, IGameRecordService _gameRecordService)
        {
            TimeZoneInfo nepalTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Nepal Standard Time");

            DateTime nowNepal = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, nepalTimeZone);

            var listOfEmail = _emailSetupService.GetAll().Result.Where(a => a.IsActive).ToList();
            var expiredGameList = _gameRecordService.GetExpiredGameRecordsAsync().Result.ToList();

            var body = GetEmailBody(expiredGameList);
            var subject = $"Monthly Bonus User list Date: {nowNepal.Year}:{nowNepal.Month}....................................................................:{nowNepal.Day}";
            foreach (var item in listOfEmail)
            {
                EmailSender.EmailSend(item.MemberEmail, subject, body);
            }
        }
    }
}
