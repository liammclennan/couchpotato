module CouchPotato.Api
open System
open HttpClient
open CouchPotato.PublicTypes
open CouchPotato.Types
open CouchPotato.Serialization

let private extractDatabaseIdentification = function 
    | AnonymousDatabaseClient di -> di
    | AuthenticatingDatabaseClient (di,_) -> di

let private getServerUri client = 
    (extractDatabaseIdentification client).origin

let getDatabaseUri client =
    let di = extractDatabaseIdentification client
    new Uri(di.origin, new Uri(di.database, UriKind.Relative))

let private getDatabaseUriString client = 
    (getDatabaseUri client).ToString()

let getDocumentUri client id = 
    let di = extractDatabaseIdentification client
    new Uri(getDatabaseUriString client + "/" + id)

let createDatabaseClient url database =
    AnonymousDatabaseClient {origin=(new Uri(url)); database=database}

let withBasicAuthentication username password (c:DatabaseClient) = 
    match c with
        | AnonymousDatabaseClient u -> AuthenticatingDatabaseClient (u, {username = username; password = password})
        | AuthenticatingDatabaseClient (u,c) -> AuthenticatingDatabaseClient (u, {username = username; password = password})

let ping client : string = 
    (getServerUri client).ToString() |> createRequest Get |> getResponseBody
                                  
let private isSuccessResponse r =
    r.StatusCode > 199 && r.StatusCode < 300

let insertDocument client (document:'d) : CouchDocument<'d> =
    let resp = getDatabaseUriString client 
                |> createRequest Post
                |> withHeader (ContentType "application/json")
                |> withBody (serializeRecord document)
                |> getResponse
   
    if isSuccessResponse resp then
        CouchDocument<_>.createFromMutationResponse (responseToMutationResponse(resp.EntityBody.Value)) document
    else
        sprintf "Response status code was %d" resp.StatusCode |> failwith
       
let updateDocument client (cd:CouchDocument<'d>) : CouchDocument<'d> =
    let resp = getDatabaseUriString client + "/" + cd._id
                |> createRequest Put
                |> withHeader (ContentType "application/json")
                |> withBody (serializeCouchDocument cd)
                |> getResponse
   
    if isSuccessResponse resp then
        CouchDocument<_>.createFromMutationResponse (responseToMutationResponse(resp.EntityBody.Value)) cd.data 
    else
        sprintf "Response status code was %d" resp.StatusCode |> failwith

let getDocument client (id:string) : CouchDocument<'t> =
    let resp = getDatabaseUriString client + "/" + id
                |> createRequest Get
                |> withHeader (ContentType "application/json")
                |> getResponse

    if isSuccessResponse resp then
        CouchDocument<_>.createFromGetResponse 
            (responseToGetResponse(resp.EntityBody.Value)) 
            (responseToRecord<'t>(resp.EntityBody.Value))
    else
        sprintf "Response status code was %d" resp.StatusCode |> failwith

let createDatabase client =
    let resp = getDatabaseUriString client 
                |> createRequest Put
                |> getResponse

    if isSuccessResponse resp then
        ()
    else
        sprintf "Unable to create database %s. Status code was %d. Response was %A" (extractDatabaseIdentification client).database resp.StatusCode resp |> failwith

let deleteDatabase client =
    let resp = getDatabaseUriString client 
                |> createRequest Delete
                |> getResponse
    ()

type MigrationStep = { name: string; action: DatabaseClient -> Unit }

let currentVersion client =
    let resp = getDatabaseUriString client + "/" + "migration_version"
                |> createRequest Get
                |> withHeader (ContentType "application/json")
                |> getResponseBody
    let o = responseToRecord<DatabaseVersion> resp
    o.version

let migrateTo client (v:string) (steps: List<MigrationStep>)=
    let presentVersion = currentVersion client
    let stepsToApply = List.filter (fun s -> s.name <= v && s.name > presentVersion) steps |> List.sortBy (fun s -> s.name)
    if presentVersion < v && stepsToApply.Length > 0 then 
        for { name = name; action = action } in stepsToApply do
            action client
        let resp = (getDocumentUri client "migration_version").ToString()
                    |> createRequest Put
                    |> withHeader (ContentType "application/json")
                    |> withBody ("{\"version\": \"" + stepsToApply.[stepsToApply.Length-1].name + "\"}")
                    |> getResponse
        ()
    ()

let putView client designDocName (view:View) =
    let mr = seq {
                yield view.name, view.mapReduce
             } |> Map.ofSeq
    let couchView = { language = "javascript"; views = mr }
    
    let req = (getDocumentUri client ("_design/" + designDocName)).ToString()
                    |> createRequest Put
                    |> withHeader (ContentType "application/json")
                    |> withBody (serializeRecord couchView)
    let resp = getResponse req
    ()

let getViewUri client viewDoc viewName = 
    sprintf "%s/_design/%s/_view/%s" (getDatabaseUriString client) viewDoc viewName 

