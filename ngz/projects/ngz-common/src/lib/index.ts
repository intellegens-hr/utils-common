// Main library module
// ----------------------------------------------------------------------------

// Import and (re)export modules
import { NgModule } from '@angular/core';

// Import and (re)export services
import { HttpService, ApiEndpointFactory, ApiEndpointToGridAdapterFactory } from './services';
export * from './services';
const services = [
  HttpService, ApiEndpointFactory, ApiEndpointToGridAdapterFactory
];

// Import and (re)export data models
export * from './data';

/**
 * Common utils module
 */
@NgModule({
  imports: [],
  providers: [
    ...services
  ],
  exports: []
})
export class CommonUtilsModule { }
