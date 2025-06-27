# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files and restore dependencies
COPY ["HttpObservable.Samples/HttpObservable Samples.csproj", "HttpObservable.Samples/"]
COPY ["HttpObservable/HttpObservable.csproj", "HttpObservable/"]
RUN dotnet restore "HttpObservable.Samples/HttpObservable Samples.csproj"

# Copy the rest of the source code
COPY . .

# Publish the application
RUN dotnet publish "HttpObservable.Samples/HttpObservable Samples.csproj" -c Release -o /app/publish

# Final stage
FROM nginx:alpine
WORKDIR /usr/share/nginx/html
COPY --from=build /app/publish/wwwroot .
COPY HttpObservable.Samples/nginx.conf /etc/nginx/nginx.conf

EXPOSE 80

CMD ["nginx", "-g", "daemon off;"]