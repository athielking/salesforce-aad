@restResource(urlMapping='/contacts')
global class ContactsApi {
    
    @httpGet
    global static Contact[] onGet() {
        return [SELECT ID, Name FROM Contact];
    }
}