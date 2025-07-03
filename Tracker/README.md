# Tracker - Envelope Budgeting
Use this to do envelope budgeting.
## Project Conventions
 * Use [plurals for resources](https://learn.microsoft.com/en-us/aspnet/core/web-api/advanced/conventions?view=aspnetcore-9.0)
* Use [kebab-cased](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/routing?view=aspnetcore-9.0#parameter-transformers) (slugified) resources
* ASP.Net MVC
* Guided by [Locality of Behaviour](https://htmx.org/essays/locality-of-behaviour/) and HATEOAS

# Resource Routing Conventions <sup>1</sup>
| Method | Resource              | Used for                  | Read-only? (Safe)  | Can retry? (idempotent) |
|--------|-----------------------|---------------------------|--------------------|-------------------------|
| GET    | /accounts             | Get all accounts          | ✓                  | ✓                      |
| GET    | /accounts/:id         | Get account :id           | ✓                  | ✓                      |
| POST   | /accounts             | Create a new account      | ✗                  | ✗                      |
| PUT    | /accounts/:id         | Replace account           | ✗                  | ✓                      |
| PATCH  | /accounts/:id         | Partial update account    | ✗                  | ✗                      |
| DELETE | /accounts/:id         | Remove account            | ✗                  | ✓                      |

# Resource Routing Conventions — New/Create/Add/Edit
| Method | Resource              | Used for                  | Read-only? (Safe)  | Can retry? (idempotent) |
|--------|-----------------------|---------------------------|--------------------|-------------------------|
| GET    | /accounts/add         | Get form for new account  | ✓                  | ✓                      |
| GET    | /accounts/:id/edit    | Get form for edit account | ✓                  | ✓                      |

# Resource Routing Conventions — Custom Actions
| Method | Resource               | Used for                              | Read-only? (Safe)  | Can retry? (idempotent) |
|--------|------------------------|---------------------------------------|--------------------|-------------------------|
| POST   | /accounts/:id/actions  | Perform "action" on existing account  | ✗                  | ✗                      |

## Action Examples
| Payload                                | Used for                              | Read-only? (Safe)  | Can retry? (idempotent) |
|----------------------------------------|---------------------------------------|--------------------|-------------------------|
| { action: "archive" }                  | Archive an existing account           | ✗                  | ✗                      |
| { action: "activate" }                 | Activate an existing account          | ✗                  | ✗                      |
| { action: "close" }                    | Close an existing account             | ✗                  | ✗                      |
| { action: "merge_into", otherId: 123 } | Merge this account into other account | ✗                  | ✗                      |

<sup>1</sup> Table based on "The Life & Death of HTMX" by Alexander Petros at Big Sky Dev Con 2024, June 8th 2024
