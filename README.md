# ASP.NET-Trip-Management-System

## Overview
This project is a REST API developed using **ASP.NET Core** for managing clients and their trip registrations. It uses **ADO.NET** with `SqlConnection` and `SqlCommand` to interact with a SQL Server database, without relying on any ORM like Entity Framework.

## Features
- Create new clients with input validation
- Retrieve trips associated with a specific client
- Register a client for a trip with validation (PESEL, max participants, duplicates)
- Remove a client from a trip
- Structured error handling with proper HTTP status codes

## Technologies
- ASP.NET Core Web API
- SQL Server
- ADO.NET
- C#

## Endpoints

| Method | Endpoint                            | Description                             |
|--------|-------------------------------------|-----------------------------------------|
| POST   | `/api/clients`                      | Create a new client                     |
| GET    | `/api/clients/{id}/trips`           | Get all trips for a client              |
| PUT    | `/api/clients/{id}/trips/{tripId}`  | Register a client to a trip             |
| DELETE | `/api/clients/{id}/trips/{tripId}`  | Remove a client from a trip             |

## HTTP Status Codes Used
- `200 OK`: Successful requests
- `201 Created`: Client successfully created
- `400 Bad Request`: Invalid input
- `404 Not Found`: Client or trip not found
- `409 Conflict`: Duplicate registration or PESEL conflict
- `500 Internal Server Error`: Unhandled exceptions

## How to Run
1. Clone the repository:
   ```bash
   git clone https://github.com/mertAltinbas/ASP.NET-Trip-Management-System.git
