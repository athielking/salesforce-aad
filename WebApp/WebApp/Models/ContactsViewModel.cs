using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApp.Models
{
    public class ContactsViewModel
    {
        public List<Contact> Contacts { get; set; }
        public ContactsViewModel()
        {
            Contacts = new List<Contact>();
        }
    }
}