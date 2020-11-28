import { OnInit } from '@angular/core';
import { Component } from '@angular/core';
import { BroadcastService, MsalService } from '@azure/msal-angular';
import { Subscription } from 'rxjs';
import { Logger, CryptoUtils } from 'msal';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent implements OnInit {
  title = 'Azure AD - Sample';
  subscriptions: Subscription[] = [];
  loggedIn = false;

  constructor(private broadcastService: BroadcastService, private authService: MsalService) { }

  ngOnInit() {
    this.subscriptions.push(
      this.broadcastService.subscribe('msal:loginSuccess', () => {
        this.checkAccount();
      })
    );

    this.subscriptions.push(
      this.broadcastService.subscribe('msal:loginFailure', (error) => {
        console.log('Login Fails:', error);
      })
    );

    this.authService.handleRedirectCallback((authError, response) => {
      if (authError) {
        console.error('Redirect Error: ', authError.errorMessage);
        return;
      }

      console.log('Redirect Success: ', response.accessToken);
    });

    this.authService.setLogger(new Logger((logLevel, message, piiEnabled) => {
      console.log('MSAL Logging: ', message);
    }, {
      correlationId: CryptoUtils.createNewGuid(),
      piiLoggingEnabled: false
    }));
  }

  ngOnDestroy(): void {
    this.subscriptions.forEach((subscription) => subscription.unsubscribe());
  }

  public login() {
    this.authService.loginPopup();
  }

  public logout() {
    this.authService.logout();
  }

  checkAccount() {
    this.loggedIn = !!this.authService.getAccount();
  }
}
