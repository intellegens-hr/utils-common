#### v1.0.6

- `ApiEndpoint` now exposes a global and instance level `.actions` even emitter triggering on any create, update or delete action

#### v1.0.5

- `HttpRequestPromise<T>` returned from `HttpService` now contains a new `.info` property describing the request being handled
- `HttpRequestPromise<T>` returned from `ApiEndpoint` now contains a new `.info` property describing the request being handled
- Updated `ApiSearchRequestModel` with separated filtering/search functionality and both `ApiEndpointToGridAdapter` and `ApiEndpointToAutocompleteAdapter` to match
- `ApiEndpointToGridAdapter` and `ApiEndpointToAutocompleteAdapter` now accept and expose a "toString" function for converting EnTT instances into presentable string representation
- `ApiEndpoint` now uses PUT instead of POST for updates

#### v1.0.3

- Making sure publishing to NPM captures correct README.md
- Fixed issues with `ApiEndpoint` service's `.get()` and `.list()` methods
- Fixes to the `ApiEndpointToAutocompleteAdapter` adapter
- `ApiEndpointToAutocompleteAdapter` adapter now takes explicit column key to order by and ordering direction as configuration

#### v1.0.2

- Added CHANGELOG.md
- Updated package description, repository, keywords, license and author for publishing
- Updated README.md
- Updated to Angular 9.1.6
- `HttpService` now returns `HttpRequestPromise<T>` instead of `Promise<T>` exposing a `.cancel()` method for canceling in-flight API requests
- `ApiEndpoint` now returns `HttpRequestPromise<T>` instead of `Promise<T>` exposing a `.cancel()` method for canceling in-flight API requests
- `ApiEndpointToGridAdapter`: Now cancels in-flight API requests upon sending newer ones and supports debouncing ng-grid's change events
- Added `ApiEndpointToAutocompleteAdapter`: Works identically to already existing `ApiEndpointToGridAdapter`

## v1.0.1, Stable version

  - Basic HTTP and API CRUD support for .NET core and Angular clients
    - List, search, create, update, delete
    - Adapter for use with @intellegens/ngz-material <ngz-grid /> Angular component
