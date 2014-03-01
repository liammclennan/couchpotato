module CouchPotato.Urls
open System
open CouchPotato.Types
open CouchPotato.PublicTypes

let extractDatabaseIdentification = function 
    | AnonymousDatabaseClient di -> di
    | AuthenticatingDatabaseClient (di,_) -> di

let getServerUri client = 
    (extractDatabaseIdentification client).origin

let getDatabaseUri client =
    let di = extractDatabaseIdentification client
    new Uri(di.origin, new Uri(di.database, UriKind.Relative))

let getDatabaseUriString client = 
    (getDatabaseUri client).ToString()

let getDocumentUri client id = 
    let di = extractDatabaseIdentification client
    new Uri(getDatabaseUriString client + "/" + id)

let getViewUri client viewDoc viewName = 
    sprintf "%s/_design/%s/_view/%s" (getDatabaseUriString client) viewDoc viewName 