# Use the official .NET SDK image for building
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy the .csproj file and restore dependencies
COPY CSharpProxy.csproj ./
RUN dotnet restore CSharpProxy.csproj

# Copy all files and build the project
COPY . ./
RUN dotnet publish CSharpProxy.csproj -c Release -o /out

# Use a smaller runtime image for the final container
FROM mcr.microsoft.com/dotnet/runtime:8.0
WORKDIR /app
COPY --from=build /out .
CMD ["dotnet", "CSharpProxy.dll"]
