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
            _timer = new Timer(ExecuteTask, null, TimeSpan.Zero, TimeSpan.FromDays(1));

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
            <div class='card-body'>
                <style>
                    /* Card body styling */
                    .card-body {
                        padding: 20px;
                        background-color: #f9f9f9;
                    }

                    /* Table styling */
                    .table {
                        width: 100%;
                        border-collapse: collapse;
                        margin-bottom: 1rem;
                        background-color: #fff;
                    }

                    /* Table header styling */
                    .table th {
                        background-color: #007bff; /* Change this color to fit your theme */
                        color: white;
                        text-align: center;
                        padding: 10px;
                    }

                    /* Table body row styling */
                    .table td {
                        padding: 10px;
                        border: 1px solid #dee2e6;
                        text-align: center;
                    }

                    /* Alternate row colors */
                    .table-striped tbody tr:nth-of-type(odd) {
                        background-color: #f2f2f2;
                    }

                    /* Add hover effect on rows */
                    .table-hover tbody tr:hover {
                        background-color: #d6e9f9;
                    }

                    /* Table footer styling */
                    .table tfoot th {
                        background-color: #f1f1f1;
                        color: #333;
                        padding: 10px;
                        text-align: center;
                    }

                    /* Responsive table */
                    @media (max-width: 768px) {
                        .table th, .table td {
                            font-size: 12px;
                            padding: 8px;
                        }
                    }
                </style>
                <table id='nonexpiredtbl' class='table table-bordered table-striped'>
                    <thead>
                        <tr>
                            <th>Full Name</th>
                            <th>Phone Number</th>
                            <th>State</th>
                            <th>Referred By</th>
                            <th>Email</th>
                            <th>Facebook Name</th>
                            <th>Game</th>
                            <th>Game Id</th>
                            <th>Entry Date</th>
                            <th>Expiry Date</th>
                        </tr>
                    </thead>
                    <tbody>");

                        foreach (var item in gameRecords)
                        {
                            sb.Append($@"
                <tr>
                    <td>{item.FullName}</td>
                    <td>{item.PhoneNumber}</td>
                    <td>{item.State.Name}</td>
                    <td>{item.RefferedBy}</td>
                    <td>{item.Email}</td>
                    <td>{item.FacebookName}</td>
                    <td>{item.Game.Name}</td>
                    <td>{item.GameUserId}</td>
                    <td>{item.CreatedDateTime:yyyy-MM-dd}</td>
                    <td>{item.ExpiryDateTime:yyyy-MM-dd}</td>
                </tr>");
                        }

                        sb.Append(@"
                    </tbody>
                    <tfoot>
                        <tr>
                            <th>Full Name</th>
                            <th>Phone Number</th>
                            <th>State</th>
                            <th>Referred By</th>
                            <th>Email</th>
                            <th>Facebook Name</th>
                            <th>Game</th>
                            <th>Game Id</th>
                            <th>Entry Date</th>
                            <th>Expiry Date</th>
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
