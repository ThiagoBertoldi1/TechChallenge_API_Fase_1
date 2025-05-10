using Queue.Insert;
using TechChallenge.Data.Base.Queue;
using TechChallenge.Domain.Entities;
using TechChallenge.Domain.Interface.BaseRepository.Queue;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();
builder.Services.AddSingleton<IQueueRepository<Contact>, QueueRepository<Contact>>();
builder.Services.AddSingleton(provider => new QueueRepository<Contact>(builder.Configuration));

var host = builder.Build();
host.Run();
