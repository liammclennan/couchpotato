module CouchPotato.Api
open System
open HttpClient
open Newtonsoft.Json

type Credentials = { username: string; password: string }
type CouchDbError = { error: string; reason: string}
type DatabaseIdentification = {origin: Uri; database: string}

type CouchMutationResponse = {ok: bool; id: string; rev:string}
type CouchGetResponse = { _id: string; _rev: string }
type CouchDbDocument<'t> = { _id:string; _rev: string; ``type``:string; data: 't }
                            with static member createFromMutationResponse r d =
                                  { _id = r.id; _rev = r.rev; ``type`` = d.GetType().FullName; data = d }
                                 static member createFromGetResponse (r:CouchGetResponse) d =
                                  { _id = r._id; _rev = r._rev; ``type`` = d.GetType().FullName; data = d }
type DatabaseClient =
    | AnonymousDatabaseClient of DatabaseIdentification
    | AuthenticatingDatabaseClient of DatabaseIdentification * Credentials

let private extractDatabaseIdentification = function 
    | AnonymousDatabaseClient di -> di
    | AuthenticatingDatabaseClient (di,_) -> di

let private getServerUri client = 
    (extractDatabaseIdentification client).origin

let getDatabaseUri client =
    let di = extractDatabaseIdentification client
    new Uri(di.origin, new Uri(di.database, UriKind.Relative))

let getDocumentUri client id = 
    let di = extractDatabaseIdentification client
    new Uri((getDatabaseUri client).ToString() + "/" + id)

let createDatabaseClient url database =
    AnonymousDatabaseClient {origin=(new Uri(url)); database=database}

let withBasicAuthentication username password (c:DatabaseClient) = 
    /// validate username and password
    match c with
        | AnonymousDatabaseClient u -> AuthenticatingDatabaseClient (u, {username = username; password = password})
        | AuthenticatingDatabaseClient (u,c) -> AuthenticatingDatabaseClient (u, {username = username; password = password})

let ping client : string = 
    (getServerUri client).ToString() |> createRequest Get |> getResponseBody
                                  
let private isSuccessResponse r =
    r.StatusCode > 199 && r.StatusCode < 300

let private serialize o (rev:Option<string>) = 
    let jo = JsonConvert.SerializeObject o
                |> JsonConvert.DeserializeObject :?> Newtonsoft.Json.Linq.JObject
    jo.Add(new Newtonsoft.Json.Linq.JProperty("type", o.GetType().FullName))
    if rev.IsSome then
        jo.Add(new Newtonsoft.Json.Linq.JProperty("_rev", rev.Value))
    else
        ()
    JsonConvert.SerializeObject(jo)

let insertDocument client (document:'d) : Choice<CouchDbDocument<'d>,string> = 
    let resp = (getDatabaseUri client).ToString() 
                |> createRequest Post
                |> withHeader (ContentType "application/json")
                |> withBody (serialize document None)
                |> getResponse
   
    if isSuccessResponse resp then
        CouchDbDocument<'d>.createFromMutationResponse (JsonConvert.DeserializeObject<CouchMutationResponse>(resp.EntityBody.Value)) document 
            |> Choice1Of2
    else
        sprintf "Response status code was %d" resp.StatusCode |> Choice2Of2
       
let updateDocument client (cd:CouchDbDocument<'d>) : Choice<CouchDbDocument<'d>,string> =
    let resp = (getDatabaseUri client).ToString() + "/" + cd._id
                |> createRequest Put
                |> withHeader (ContentType "application/json")
                |> withBody (serialize cd.data (Some cd._rev))
                |> getResponse
   
    if isSuccessResponse resp then
        CouchDbDocument<'d>.createFromMutationResponse (JsonConvert.DeserializeObject<CouchMutationResponse>(resp.EntityBody.Value)) cd.data 
            |> Choice1Of2
    else
        sprintf "Response status code was %d" resp.StatusCode |> Choice2Of2
 

let getDocument client (id:string) : Choice<CouchDbDocument<'t>,string>= 
    let resp = (getDatabaseUri client).ToString() + "/" + id
                |> createRequest Get
                |> withHeader (ContentType "application/json")
                |> getResponse

    if isSuccessResponse resp then
        CouchDbDocument<_>.createFromGetResponse 
            (JsonConvert.DeserializeObject<CouchGetResponse>(resp.EntityBody.Value)) 
            (JsonConvert.DeserializeObject<'t>(resp.EntityBody.Value))
            |> Choice1Of2
    else
        sprintf "Response status code was %d" resp.StatusCode |> Choice2Of2

let createDatabase client =
    let resp = (getDatabaseUri client).ToString() 
                |> createRequest Put
                |> getResponse

    if isSuccessResponse resp then
        ()
    else
        failwith "Unable to create database %s. Status code was %d" (extractDatabaseIdentification client).database resp.StatusCode

let deleteDatabase client =
    let resp = (getDatabaseUri client).ToString() 
                |> createRequest Delete
                |> getResponse
                
    if isSuccessResponse resp then
        ()
    else
        sprintf "Unable to create database %s. Status code was %d. Response was %A" (extractDatabaseIdentification client).database resp.StatusCode resp
            |> failwith

type MapReduce = { map: string; reduce: Option<string>}
type CouchView = { _id: Option<string>; _rev: Option<string>; language: string; views: System.Collections.Generic.Dictionary<string,MapReduce> } 
type View = { name: string; mapReduce: MapReduce }

let queryView client view startkey endkey = 
    let docId = "_design/" + view.name
    let checkResp = (getDatabaseUri client).ToString() + "/" + docId
                    |> createRequest Get
                    |> getResponse

    if isSuccessResponse checkResp then
        JsonConvert.DeserializeObject<CouchView>(checkResp.EntityBody.Value)
    else
        let viewColl = System.Collections.Generic.Dictionary()
        viewColl.Add(view.name, view.mapReduce)
        let insertResp = (getDatabaseUri client).ToString() 
                            |> createRequest Post
                            |> withHeader (ContentType "application/json")
                            |> withBody (JsonConvert.SerializeObject({_id = Some docId; _rev = None; language = "javascript"; views = viewColl }))
                            |> getResponse
        failwith "yolo"