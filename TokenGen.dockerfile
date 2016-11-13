FROM microsoft/dotnet:latest

# Set environment variables
ENV ASPNETCORE_URLS="http://*:5000"
ENV ASPNETCORE_ENVIRONMENT="Staging"

# Copy files to app directory
COPY . /app

# RethinkDbLogProvider
WORKDIR /app/src/RethinkDbLogProvider
RUN ["dotnet", "restore"]

# Set working directory
WORKDIR /app/src/TokenGen

# Restore NuGet packages
RUN ["dotnet", "restore"]

# Build the app
RUN mkdir release && dotnet publish -c Release -o /app/src/TokenGen/release

# Open port
EXPOSE 5000/tcp

HEALTHCHECK CMD curl --fail http://localhost:5000/api/healthcheck || exit 1

# Set working directory to release
WORKDIR /app/src/TokenGen/release

# Run the app
ENTRYPOINT ["dotnet", "TokenGen.dll"]