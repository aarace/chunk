using Chunk.Core;
using Chunk.Worker;

var builder = Host.CreateApplicationBuilder(args);
builder.Services
    .AddChunkCore()
    .AddChunkWorker(builder.Configuration);

var host = builder.Build();
host.Run();
