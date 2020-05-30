# Intellegens Utils: Commons, Angular

Angular library containing reusable code shared between multiple projects ...


## Usage

```ts
import { CommonUtilsModule } from '@intellegens/ngz-common'
```

### HttpAuthTokenInjector

```ts
import { HTTP_INTERCEPTORS } from '@angular/common/http';
import { HttpAuthTokenInjector, HttpErrorInterceptor } from '@intellegens/ngz-common';

@NgModule({
  providers: [
    // Inject HTTP Error interceptor
    { provide: HTTP_INTERCEPTORS, useClass: HttpErrorInterceptor, multi: true },
    // Inject HTTP Authentication token injection interceptor
    { provide: HTTP_INTERCEPTORS, useClass: HttpAuthTokenInjector, multi: true }
  ]
})
class MyModule {}


```

### HttpService

```ts
import { HttpService } from '@intellegens/ngz-common';
import { HttpErrorResponse } from '@angular/common/http';

class MyApp {
  constructor () {
    // Set authentication token
    HttpService.setAuthToken('abcdef')
    // Initialize API base url
    HttpService.initialize('/api');
    // Set up global error handling
    HttpService.error.subscribe((err : HttpErrorResponse) => alert(err));
  }
}

class MyComponent {
  constructor (private http: HttpService) {

    // Use HTTP service
    try {
      const data = await this.http.request('POST', '/resources', {
        body:    {}
        query:   {},
        headers: {},
        options: {}
      });
    } catch (err) {}

    // Use HTTP service to get raw request
    try {
      const res = await this.http._request('POST', '/resources', {
        body:    {}
        query:   {},
        headers: {},
        options: {}
      });
    } catch (err) {}

    // Run request and then cancel it
    const req = this.http._request('GET', '/resources');
    setTimeout(() => { req.cancel(); }, 100)
    try {
      const res = await req;
    } catch (err) {}

  }
}
```

### ApiEndpoint

```ts
import { EnTT } from '@ofzza/entt-rxjs'
import { ApiEndpointFactory, ApiEndpoint, ApiEndpointAction } from '@intellegens/ngz-common';

class MyApp {
  constructor () {
    // Initialize API 
    ApiEndpoint.initialize();
    // Set up global error handling
    ApiEndpoint.error.subscribe((err: EnttValidationError) => alert(err));
    // Set up global action handling
    ApiEndpoint.action.subscribe((e: ApiEndpointAction) => alert(e));
  }
}

class ResourceModel extends EnTT { /* ... */}

class MyComponent {
  constructor (private endpointFactory: ApiEndpointFactory) {

    // Initialize endpoint(s)
    this.endpointRaw = this.endpointFactory('/resource');
    this.endpointEnTT = this.endpointFactory('/resource', ResourceModel);

    // Set up local action handling
    this.endpointEnTT.action.subscribe((e: ApiEndpointAction) => {
      if (e.action === ApiEndpointActions.CREATE) {
        alert(e);
        e.preventDefault();
      }
    });

    // Get list of all resources from endpoint
    const dataRaw: object[]             = await this.endpointRaw.list(),
          dataEnTT: ResourceModel[] = await this.endpointEnTT.list();

    // Search, order and paginate resources from endpoint
    const searchReq                     = new ApiSearchRequestModel(), // Edit properties to set search
          dataRaw: object[]             = await this.endpointRaw.search(searchReq),
          dataEnTT: ResourceModel[] = await this.endpointEnTT.search(searchReq);

    // Get single resource
    const dataRaw: object               = await this.endpointRaw.get(id),
          dataEnTT: ResourceModel   = await this.endpointEnTT.get(id);

    // Create single resource
    const dataRaw: object               = await this.endpointRaw.create(resource),
          dataEnTT: ResourceModel   = await this.endpointEnTT.create(resource);

    // Update single resource
    const dataRaw: object               = await this.endpointRaw.update(resource.id, resource),
          dataEnTT: ResourceModel   = await this.endpointEnTT.update(resource.id, resource);

    // Delete single resource
    await this.endpointRaw.delete(id),
    await this.endpointEnTT.delete(id);

  }
}
```

### ApiEndpointToGridAdapter

```ts
import { ApiEndpointToGridAdapterFactory } from '@intellegens/ngz-common';

class ResourceModel extends EnTT { /* ... */}

class MyClass {
  constructor (public _adapterFactory: ApiEndpointToGridAdapterFactory) {
    this._adapter = this._adapterFactory.create('/resources', ResourceModel);
    this._adapter.configure({
      preload: false,
      debounceInterval: 400,
      defaultPageLength: 20
    });
  }
}
```

```html
<ngz-grid [dataSource]="_adapter.dataSource" [dataLength]="_adapter.dataLength" (changed)="_adapter.changed($event)"></ngz-grid>
```

### ApiEndpointToAutocompleteAdapter

```ts
import { MatAutocompleteSelectedEvent } from "@angular/material/autocomplete";
import { ApiEndpointToAutocompleteAdapterFactory } from '@intellegens/ngz-common';

class ResourceModel extends EnTT { /* ... */}

class MyClass {
  public _selected: ResourceModel;

  constructor (public _adapterFactory: ApiEndpointToAutocompleteAdapterFactory) {
    this._adapter = this._adapterFactory.create(
      '/resources',
      ResourceModel, {
        searchBy: ['title', 'code'],
        orderBy: ['title', '!code']
      }
    );
    this._adapter.configure({
      preload: false,
      debounceInterval: 400,
      defaultPageLength: 20
    });
  }

  public _onSelected (e: MatAutocompleteSelectedEvent) {
    this._selected = event.option.value;
  }
}
```

```html
<mat-form-field class="w-1of3">
  <mat-label>Autocomplete</mat-label>
  <input type="text" matInput [matAutocomplete]="autocomplete"
    [value]="_adapter.toString(_selected)"
    (input)="_adapter.changed($event.target.value)" />
  <mat-autocomplete #autocomplete="matAutocomplete"
    (opened)="_adapter.opened($event)"
    (optionSelected)="_onSelected($event)"
    [displayWith]="_adapter.toString">
    <mat-option *ngFor="let option of _adapter.dataItems"
      [value]="option" [matTooltip]="_adapter.toString(option)" [matTooltipPosition]="'right'">
      {{ _adapter.toString(option) }}
    </mat-option>
  </mat-autocomplete>
</mat-form-field>
```


## Build

Run `ng build` to build the project. The build artifacts will be stored in the `dist/` directory. Use the `--prod` flag for a production build.


## Contributing

### Reporting issues

When reporting issues, please keep to provided templates.

Before reporting issues, please read: [GitHub Work-Flow](https://github.com/ofzza/onboarding/blob/master/CONTRIBUTING/github.md)

### Contributing

For work-flow and general etiquette when contributing, please see:
- [Git Source-Control Work-Flow](https://github.com/ofzza/onboarding/blob/master/CONTRIBUTING/git.md)
- [GitHub Work-Flow](https://github.com/ofzza/onboarding/blob/master/CONTRIBUTING/github.md)
- [Angular Work-Flow](https://github.com/ofzza/onboarding/blob/master/CONTRIBUTING/angular.md)

Please accompany any work, fix or feature with their own issue, in it's own branch (see [Git Source-Control Work-Flow](https://github.com/ofzza/onboarding/blob/master/CONTRIBUTING/git.md) for branch naming conventions), and once done, request merge via pull request.

When creating issues and PRs, please keep to provided templates.
