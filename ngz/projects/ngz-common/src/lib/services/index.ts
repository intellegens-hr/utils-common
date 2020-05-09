// Services module
// ----------------------------------------------------------------------------

// Import and (re)export modules
import { NgModule } from '@angular/core';
import { RouterModule } from '@angular/router';
const modules = [
  RouterModule
];

// Import and (re)export providers
export * from './api';
import {
  HttpService,
  ApiEndpointFactory, ApiEndpointToGridAdapterFactory, ApiEndpointToAutocompleteAdapterFactory
} from './api';
const providers = [
  HttpService,
  ApiEndpointFactory, ApiEndpointToGridAdapterFactory, ApiEndpointToAutocompleteAdapterFactory
];

/**
 * Common utils module
 */
@NgModule({
  imports:   [ ...modules ],
  // providers: [ ...providers ],
  exports:   [ ...modules ]
})
export class ServicesModule {}
