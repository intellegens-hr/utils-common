// Main showcase module
// ----------------------------------------------------------------------------

// Import dependencies
import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';

// Import library
import { CommonUtilsModule } from '../../../ngz-common/src/public-api';

// Import routing and main component
import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';

@NgModule({
  declarations: [
    AppComponent
  ],
  imports: [
    BrowserModule,
    CommonUtilsModule,
    AppRoutingModule
  ],
  providers: [],
  bootstrap: [AppComponent]
})
export class AppModule { }
