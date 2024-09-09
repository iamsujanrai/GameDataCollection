using System.Net.Mail;
using System.Net;
using System.Net;
using System.Net.Mail;
using GameDataCollection.Models;
using System.Text;
using GameDataCollection.Services;
using static System.Formats.Asn1.AsnWriter;
namespace GameDataCollection.Extension
{
    public class EmailScheduler : IHostedService, IDisposable
    {
        private readonly IServiceScopeFactory _serviceProvider;
        public EmailScheduler(IServiceScopeFactory serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }
        private Timer _timer;

        // This is the method you want to run on a schedule
        public Task MyScheduledMethod()
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var emailSetupService = scope.ServiceProvider.GetRequiredService<IEmailSetupService>();
                var gameRecordService = scope.ServiceProvider.GetRequiredService<IGameRecordService>();
                SendDailyEmail(emailSetupService, gameRecordService);
                // Your logic here
                return Task.CompletedTask;
            }

        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            // Schedule the task to run every X seconds or minutes
            _timer = new Timer(ExecuteTask, null, TimeSpan.Zero, TimeSpan.FromSeconds(10));

            return Task.CompletedTask;
        }

        private void ExecuteTask(object state)
        {

            MyScheduledMethod();
            // Call your specific method

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
            var listOfEmail = _emailSetupService.GetAll().Result.Where(a => a.IsActive).ToList();
            var expiredGameList = _gameRecordService.GetExpiredGameRecordsAsync().Result.ToList();

            var body = GetEmailBody(expiredGameList);
            var subject = "All Expired User";
            foreach (var item in listOfEmail)
            {
                EmailSender.EmailSend(item.MemberEmail, subject, body);
            }
        }
    }
}
