# Skill: Feature Vertical Slice

## When to use
Use this skill when adding a new application feature such as localization/translation.

## Rules
- Start from the HTTP contract: define a Request DTO under the feature's `Requests` namespace.
- Define the use case as a Mediator Command or Query under the feature's `Commands` or `Queries` namespace.
- Define the result under the feature's `Responses` namespace when it is a feature-specific response.
- Implement a thin handler that invokes the corresponding feature service abstraction.
- Put the service abstraction in Application and the implementation in Infrastructure.
- If persistence is needed, inject the generic `IRepository<TEntity>` into the service and use Domain entities. Use `IUnitOfWork` according to the existing repository conventions.
- Keep controller actions thin: bind Request, create Command/Query, send through `IMediator`, and translate the result into HTTP status codes.
- Reuse the existing validation middleware/pipeline and response conventions.

## Completion
Add tests for request validation, handler delegation and service behavior. Verify the composition root registers every new abstraction and implementation.