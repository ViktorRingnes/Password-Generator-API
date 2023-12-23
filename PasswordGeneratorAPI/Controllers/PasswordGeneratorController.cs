using Microsoft.AspNetCore.Mvc;
using System;
using System.Diagnostics;
using System.Security.Cryptography;
using MongoDB.Driver;
using MongoDB.Bson;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using System.Text.Json; 

namespace PasswordGeneratorAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PasswordGeneratorController : ControllerBase
    {
        private readonly IMongoCollection<BsonDocument> _accessLogsCollection;
        private readonly IMongoCollection<BsonDocument> _metricsCollection;
        private readonly HttpClient _httpClient;
        private const string ApiVersion = "1.01";

        public PasswordGeneratorController(HttpClient httpClient)
        {
            _httpClient = httpClient;
            var client = new MongoClient("");
            var database = client.GetDatabase("APILogs");
            _accessLogsCollection = database.GetCollection<BsonDocument>("AccessLogs");
            _metricsCollection = database.GetCollection<BsonDocument>("Metrics");
        }

        [HttpGet("generate")]
        public IActionResult GeneratePassword()
        {
            return GeneratePasswordAndLog();
        }

        [HttpGet("generate2")]
        public IActionResult GeneratePassword2()
        {
            return GeneratePasswordAndLog();
        }

        private IActionResult GeneratePasswordAndLog()
        {
            var requestId = Guid.NewGuid().ToString();
            var stopwatch = Stopwatch.StartNew();
            var password = GenerateRandomPassword();
            stopwatch.Stop();
            var payloadSize = System.Text.Encoding.UTF8.GetByteCount(password);

            LogRequest(stopwatch.ElapsedMilliseconds, requestId, payloadSize);
            LogMetrics(stopwatch.ElapsedMilliseconds, 200, requestId); 

            return Ok(password);
        }

        private string GenerateRandomPassword()
        {
            using var rng = RandomNumberGenerator.Create();
            var randomNumber = new byte[1];
            rng.GetBytes(randomNumber);
            var passwordLength = 35 + (randomNumber[0] % 6);
            return PasswordGenerator.Generate(passwordLength);
        }

        private async void LogRequest(long responseTime, string requestId, int payloadSize)
        {
            string ipAddress = GetIpAddress();
            var browser = Request.Headers["User-Agent"].ToString();
            var timestamp = DateTime.UtcNow;
            var endpoint = Request.Path.ToString();

            var logEntry = new BsonDocument
            {
                { "IPAddress", ipAddress },
                { "Browser", browser },
                { "Timestamp", timestamp },
                { "Endpoint", endpoint },
                { "ResponseTime", responseTime },
                { "RequestId", requestId },
                { "ApiVersion", ApiVersion },
                { "PayloadSize", payloadSize },
            };

            _accessLogsCollection.InsertOne(logEntry);
        }

        private void LogMetrics(long responseTime, int statusCode, string requestId)
        {
            var metricsDocument = new BsonDocument
            {
                { "Timestamp", DateTime.UtcNow },
                { "ResponseTime", responseTime },
                { "StatusCode", statusCode },
                { "RequestId", requestId },
                { "ApiVersion", ApiVersion }
            };

            _metricsCollection.InsertOne(metricsDocument);
        }

        private string GetIpAddress()
        {
            string ipAddress = Request.Headers["X-Forwarded-For"].FirstOrDefault()?.Split(',').FirstOrDefault();
            return string.IsNullOrEmpty(ipAddress) ? HttpContext.Connection.RemoteIpAddress?.ToString() : ipAddress;
        }
}
