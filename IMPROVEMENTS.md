# What I Would Improve

If I had more time I would prioritize:

## Database

Replace the in-memory `List<Order>` in `OrderService` with a real database. Data is lost on restart and won't work correctly if multiple instances run in parallel. Would use something like EF Core with PostgreSQL, running it in Docker with PgAdmin4 to manage DB.

## Partial refund validation

`RefundService` rejects a refund if `amount > order.TotalAmount`, but doesn't track previous refunds. A customer could call the endpoint multiple times and get approved every time as long as each individual request is within the original total. A fix could be to store refunds and sum them before approving a new one.

## Order status rules

Right now status can be set to anything at any time. I would add an explicit order flow, like: `Pending -> Confirmed -> Completed`, with `Cancelled` only allowed before `Completed`) this is to remove the chance for invalid state combinations.

## Input validation

- Validate `CustomerEmail` format, right now it's only checked for empty.
- Validate `Currency` against a known set like (`SEK`, `NOK`, `DKK`, `EUR`) instead of accepting a random string.

## Testing

The unit tests only cover service logic individually. I would add integration tests that test the full HTTP request/response cycle for each endpoint, including error responses.

## Architecture

For a bigger project with 100+ query and commands calls it would be cool to add some structural changes like splitting logic into QueryHandlers and CommandHandlers instead of having services handle both reads and writes, but it's not that critical.
