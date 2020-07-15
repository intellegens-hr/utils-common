#### v1.0.12

- TODO: ...

#### v1.0.11

- `ApiEndpoint` no longer validating fetched data when casting into EnTT instance provides performance improvement
- Updated to Angular v10

#### v1.0.10

- Fixed API bug translating collections from DTO models to entity models

#### v1.0.9

- Updated API and `ApiSearchRequestModel` to support:
  - Deeply nested query criteria
  - Text search of predefined property paths
  - Ordering by number of matches
- `HttpService`'s methods now support an additional `circumvent` argument of type `HttpInterceptorsCircumventionDefinition` providing a way to bypass being processed by HTTP interceptors
- All `ApiEndpointAdapter`s now expose a `.beforeSearch` event emitter exposing a composed search request for last-minute changes before being executed

#### v1.0.8

- `ApiEndpoint` now exposes a `toString()` method, same as adapters, for converting EnTT instances into presentable strings representation
- `ApiEndpointToGridAdapter` no longer loads data as soon as bound to endpoint
- `ApiEndpointToAutocompleteAdapter` now exposes read-only properties `searchBy`, `orderBy` and `excludeIds`
- `ApiEndpointToAutocompleteAdapter` now splits search string and also searches by each split part
- `ApiEndpointFactory.create()` can now be run with no arguments, delaying binding for after construction of endpoint instance 
- Implemented `ApiEndpointToCrudComponentAdapter` adapter, for usage when building typical(ish) CRUD components

#### v1.0.7

- `ApiEndpoint`'s `.actions` event emitter will now include the action EnTT instance instance even for the`delete` action

#### v1.0.6

- `ApiEndpoint` now exposes a global and instance level `.actions` event emitter triggering on any create, update or delete action

#### v1.0.5

- `HttpRequestPromise<T>` returned from `HttpService` now contains a new `.info` property describing the request being handled
- `HttpRequestPromise<T>` returned from `ApiEndpoint` now contains a new `.info` property describing the request being handled
- Updated `ApiSearchRequestModel` with separated filtering/search functionality and both `ApiEndpointToGridAdapter` and `ApiEndpointToAutocompleteAdapter` to match
- `ApiEndpointToGridAdapter` and `ApiEndpointToAutocompleteAdapter` now accept and expose a "toString" method for converting EnTT instances into presentable string representation
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
