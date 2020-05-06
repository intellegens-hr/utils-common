// Main library module
// ----------------------------------------------------------------------------

// Import and (re)export modules
import { NgModule } from '@angular/core';

// Import and (re)export services
export * from './services';
export { ServicesModule } from './services'; // Required IVY hinting
import { ServicesModule } from './services';

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
