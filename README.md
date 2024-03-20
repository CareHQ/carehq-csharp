# CareHQ C# API client

CareHQ API Client for C#.


## Installation

```
dotnet add package CareHQ.APIClient
```


## Requirements

- .NET 4.7.2+


# Usage

```Csharp


using CareHQ;
using System.Text.Json;

ApiClient apiClient = new ApiClient(
    "MY_ACCOUNT_ID",
    "MY_API_KEY",
    "MY_API_SECRET"
);

JsonDocument users = apiClient.Request(
  HttpMethod.Get,
  "users",
  new MultiValueDict()
    .Add(
      "attributes",
      "first_name",
      "last_name"
    )
    .Add("filters-q", "ant")
);
```
