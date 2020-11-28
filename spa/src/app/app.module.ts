import { BrowserModule } from '@angular/platform-browser';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { NgModule } from '@angular/core';
import { MsalModule, MsalInterceptor } from '@azure/msal-angular';
import { HTTP_INTERCEPTORS, HttpClientModule } from '@angular/common/http';

import { MatButtonModule } from '@angular/material/button';
import { MatListModule } from '@angular/material/list';
import { MatToolbarModule } from '@angular/material/toolbar';

import { AppComponent } from './app.component';
import { AppRoutingModule } from './app-routing.module';
import { HomeComponent } from './home/home.component';
import { ContactsComponent } from './contacts/contacts.component';

@NgModule({
  declarations: [
    AppComponent,
    HomeComponent,
    ContactsComponent
  ],
  imports: [
    AppRoutingModule,
    BrowserModule,
    HttpClientModule,
    BrowserAnimationsModule,
    MatToolbarModule,
    MatButtonModule,
    MatListModule,
    MsalModule.forRoot({
      auth: {
        clientId: "eeece1ed-7821-415b-b20c-9b2c6972cd84",
        authority: "https://login.microsoftonline.com/culhamdagmail.onmicrosoft.com",
        redirectUri: "http://localhost:4200"
      },
      cache: {
        cacheLocation: "localStorage",
        storeAuthStateInCookie: false
      }
    },
    {
      popUp: true,
      consentScopes: [
        'user_delegation',
        'openid',
        'profile'
      ],
      protectedResourceMap: [
        ['https://localhost:44309/api/contact',['api://bc60f2f9-d6e0-4a06-8114-53c3c8bc1104/user_delegation']]
      ]
    })
  ],
  providers: [
    {
      provide: HTTP_INTERCEPTORS,
      useClass: MsalInterceptor,
      multi: true
    }
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }
