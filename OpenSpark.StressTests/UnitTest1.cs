using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.SignalR.Client;
using Newtonsoft.Json;
using NUnit.Framework;
using OpenSpark.ApiGateway;
using OpenSpark.ApiGateway.InputModels;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;

namespace OpenSpark.StressTests
{
    public class Tests
    {
        private readonly HttpClient _client;
        private readonly TestServer _testServer;

        public Tests()
        {
            _testServer = new TestServer(new WebHostBuilder().UseStartup<Startup>());
            _client = _testServer.CreateClient();
        }

        [SetUp]
        public void Setup()
        {

        }

        [Test]
        public void Test1()
        {
            var connection = new HubConnectionBuilder()
                .WithUrl("http://localhost:5000/hub", o => 
                    o.AccessTokenProvider = async () => await Task.FromResult(""))
                .Build();

            connection.On<string>("TestCreateProject", (projectId) =>
            {
                Console.WriteLine("YAY!");
            });

            var model = new NewProjectInputModel
            {
                Name = "Test Project",
                Visibility = "Public",
                About = "Description",
                Callback = "TestCreateProject",
                ConnectionId = connection.ConnectionId,
                Tags = new List<string> { "Tag1", "Tag2", "Tag3" }
            };

            var json = JsonConvert.SerializeObject(model);
            var data = new StringContent(json, Encoding.UTF8, "application/json");

            _client.PostAsync("http://localhost:5000/api/projects/create", data);
        }

        private void CreateApiHubClient()
        {
        }
    }
}