FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY Chunk.Core/Chunk.Core.csproj Chunk.Core/
COPY Chunk.Worker/Chunk.Worker.csproj Chunk.Worker/
RUN dotnet restore Chunk.Worker/Chunk.Worker.csproj
COPY Chunk.Core/ Chunk.Core/
COPY Chunk.Worker/ Chunk.Worker/
RUN dotnet publish Chunk.Worker/Chunk.Worker.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/runtime:10.0
WORKDIR /app
COPY --from=build /app .
USER $APP_UID
ENTRYPOINT ["dotnet", "Chunk.Worker.dll"]
