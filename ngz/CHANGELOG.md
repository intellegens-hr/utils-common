#### v1.0.2

- Added CHANGELOG.md
- Updated package description, repository, keywords, license and author for publishing

- `HttpService` now returns `HttpRequestPromise<T>` instead of `Promise<T>` exposing a `.cancel()` method for canceling in-flight API requests
- `ApiEndpoint` now returns `HttpRequestPromise<T>` instead of `Promise<T>` exposing a `.cancel()` method for canceling in-flight API requests
- `ApiEndpointToGridAdapterInternal`: Now cancels in-flight API requests upon sending newer ones and supports debouncing ng-grid's change events

## v1.0.1, Stable version

  - Basic HTTP and API CRUD support for .NET core and Angular clients
    - List, search, create, update, delete
    - Adapter for use with @intellegens/ngz-material <ngz-grid /> Angular component
