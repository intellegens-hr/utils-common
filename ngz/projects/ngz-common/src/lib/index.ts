// Main library module
// ----------------------------------------------------------------------------

// Import and (re)export modules
import { NgModule } from '@angular/core';

// Import and (re)export services
import { ServicesModule } from './services';
export * from './services';

const modules = [
  ServicesModule
];

// Import and (re)export data models
export * from './data';

/**
 * Common utils module
 */
@NgModule({
  imports: [ ...modules ],
  exports: [ ...modules ]
})
export class CommonUtilsModule {}
