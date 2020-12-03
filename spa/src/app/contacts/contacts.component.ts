import { HttpClient } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';
import { MsalService } from '@azure/msal-angular';
import { AuthError, InteractionRequiredAuthError } from 'msal';
import { Contact } from './contact.interface';

@Component({
  selector: 'app-contacts',
  templateUrl: './contacts.component.html',
  styleUrls: ['./contacts.component.css']
})
export class ContactsComponent implements OnInit {

  public contacts: Contact[] = [];
  constructor(private authService: MsalService, private http: HttpClient) { }

  ngOnInit() {
    this.getContacts()
  }

  getContacts() {
    this.http.get('https://localhost:44309/api/contact')
      .subscribe({
        next: (contacts: any) => {
          if(Array.isArray(contacts)){
            this.contacts = (<any[]>contacts).map( x => <Contact>{Id: x.hasOwnProperty("Id") ? x.Id : x.sfid, Name: x.hasOwnProperty("Name") ? x.Name: x.name} )
          } else {
            this.contacts = (<any[]>contacts.results).map( x => <Contact>{Id: x.Id, Name: x.Name});
          }
        },
        error: (err: AuthError) => {
          if(InteractionRequiredAuthError.isInteractionRequiredError(err.errorCode)) {
            this.authService.acquireTokenPopup({
              scopes: this.authService.getScopesForEndpoint('https://localhost/SpaApi/api/contact')
            }).then(() => {
              this.http.get('https://localhost/SpaApi/api/contact').toPromise()
                .then((contacts: any) => {
                  if(Array.isArray(contacts)){
                    this.contacts = (<any[]>contacts).map( x => <Contact>{Id: x.hasOwnProperty("Id") ? x.Id : x.sfid, Name: x.hasOwnProperty("Name") ? x.Name: x.name} )
                  } else {
                    this.contacts = (<any[]>contacts.results).map( x => <Contact>{Id: x.Id, Name: x.Name});
                  }
                });
            });
          }
        }
      });
  }
}
