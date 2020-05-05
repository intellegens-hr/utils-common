// Services module
// ----------------------------------------------------------------------------

// Import and (re)export modules
import { NgModule } from '@angular/core';
import { RouterModule } from '@angular/router';
const modules = [
  RouterModule
];

// Import and (re)export providers
import { HttpService, ApiEndpointFactory, ApiEndpointToGridAdapterFactory } from './api';
export * from './api';
const providers = [
  HttpService, ApiEndpointFactory, ApiEndpointToGridAdapterFactory
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
