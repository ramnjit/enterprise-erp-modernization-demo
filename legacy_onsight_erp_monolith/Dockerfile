# Step 1: Use the official .NET 9 SDK to build the app
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy all files and compile the project
COPY . .
RUN dotnet restore "erp_monolith_project.csproj"
RUN dotnet publish "erp_monolith_project.csproj" -c Release -o /app

# Step 2: Use the lighter .NET 9 ASP.NET runtime to run the app
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Copy the compiled app (and our legacyerp.db file!) from Step 1
COPY --from=build /app .

# Tell Render to listen on port 8080
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

# Start the Monolith
ENTRYPOINT ["dotnet", "erp_monolith_project.dll"]